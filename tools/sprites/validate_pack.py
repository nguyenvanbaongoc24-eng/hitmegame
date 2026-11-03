"""Validate a HIT ME gameplay sprite set without changing source files.

README: python tools/sprites/validate_pack.py PATH_TO_SPRITES
Optional: --json PATH_TO_REPORT.json. Requires Pillow and numpy. Exit 0 only
when every character has all 18 frames and every PNG meets the visual contract.
Coordinates use image pixels with origin at the top-left. The foot anchor is
the median x of the lowest opaque pixels, at their bottommost y. The alpha
heuristic for a baked ring/shadow is a warning that needs visual confirmation;
it cannot prove an arbitrary illustration has no such effect.
"""

from __future__ import annotations

import argparse
import json
import re
from dataclasses import asdict, dataclass
from pathlib import Path

import numpy as np
from PIL import Image

POSES = ("idle", "aim", "throw", "hit", "ko", "win")
DIRECTIONS = ("down", "up", "side")
FRAME_NAMES = {(pose, direction) for pose in POSES for direction in DIRECTIONS}
NAME = re.compile(
    r"^char_([a-z][a-z0-9_]*)_(idle|aim|throw|hit|ko|win)_(down|up|side)\.png$"
)
SIZE = 256


@dataclass
class SpriteResult:
    path: str
    character_id: str | None
    pose: str | None
    direction: str | None
    width: int | None
    height: int | None
    mode: str | None
    opaque_bbox: list[int] | None
    foot_anchor: list[int] | None
    issues: list[str]


def inspect_sprite(path: Path, display_path: str | None = None) -> SpriteResult:
    issues: list[str] = []
    match = NAME.fullmatch(path.name)
    if not match:
        issues.append("invalid_name: expected char_[id]_[pose]_[dir].png")
    character_id, pose, direction = match.groups() if match else (None, None, None)
    result = SpriteResult(display_path or str(path), character_id, pose, direction,
                          None, None, None, None, None, issues)
    if path.suffix.lower() != ".png":
        issues.append("non_png")
        return result
    try:
        with Image.open(path) as image:
            result.width, result.height = image.size
            result.mode = image.mode
            if image.format != "PNG" or image.mode != "RGBA":
                issues.append("missing_rgba_alpha")
            if image.size != (SIZE, SIZE):
                issues.append("wrong_size: expected 256x256")
            if image.mode != "RGBA" or image.size != (SIZE, SIZE):
                return result
            pixels = np.asarray(image)
    except (OSError, ValueError) as exc:
        issues.append(f"unreadable_image: {type(exc).__name__}")
        return result

    alpha = pixels[:, :, 3]
    if not np.any(alpha == 0):
        issues.append("no_fully_transparent_pixels")
    if np.any(alpha[0, :] > 8) or np.any(alpha[:, 0] > 8) or np.any(alpha[:, -1] > 8):
        issues.append("opaque_top_or_side_border")
    if np.any(alpha[-1, :96] > 8) or np.any(alpha[-1, 160:] > 8):
        issues.append("opaque_bottom_outside_foot_zone")

    body_y, body_x = np.nonzero(alpha > 32)
    if len(body_x) == 0:
        issues.append("empty_frame")
        return result
    x0, x1 = int(body_x.min()), int(body_x.max())
    y0, y1 = int(body_y.min()), int(body_y.max())
    result.opaque_bbox = [x0, y0, x1, y1]
    body_height = y1 - y0 + 1
    if not (round(SIZE * .70) <= body_height <= round(SIZE * .80)):
        issues.append(f"character_height_outside_70_80_percent: {body_height}px")
    bottom_x = body_x[body_y >= y1 - 2]
    foot_x = int(round(float(np.median(bottom_x))))
    result.foot_anchor = [foot_x, y1]
    if SIZE - 1 - y1 > 4:
        issues.append(f"feet_too_far_from_bottom: {SIZE - 1 - y1}px")
    if abs(foot_x - SIZE // 2) > 4:
        issues.append(f"foot_anchor_off_center: {foot_x}px")

    # A broad, translucent cyan or near-black patch low in the frame is a
    # likely ring/shadow baked into the sprite. The renderer should own these.
    lower = pixels[SIZE * 3 // 4:, :, :]
    a = lower[:, :, 3]
    r, g, b = (lower[:, :, channel].astype(np.int16) for channel in range(3))
    translucent = (a >= 16) & (a <= 190)
    cyan = (b >= 110) & (g >= 105) & (b > r + 35) & (g > r + 35)
    dark = (r < 70) & (g < 70) & (b < 70)
    for label, mask in (("cyan_ring", translucent & cyan),
                        ("dark_shadow", translucent & dark)):
        yy, xx = np.nonzero(mask)
        if len(xx) >= 30 and int(xx.max() - xx.min()) >= 60:
            issues.append(f"possible_baked_{label}: {len(xx)} pixels")
    return result


def validate_directory(root: Path) -> tuple[list[SpriteResult], list[str]]:
    if not root.is_dir():
        raise ValueError(f"Input directory does not exist: {root}")
    paths = sorted(path for path in root.rglob("*") if path.is_file()
                   and path.name not in {"Thumbs.db", ".DS_Store"})
    results = [inspect_sprite(path, path.relative_to(root).as_posix()) for path in paths]
    set_issues: list[str] = []
    if not paths:
        set_issues.append("empty_directory")
    seen: dict[tuple[str, str, str], str] = {}
    by_character: dict[str, set[tuple[str, str]]] = {}
    for item in results:
        if item.character_id is None:
            continue
        key = (item.character_id, item.pose, item.direction)
        if key in seen:
            set_issues.append(f"duplicate_frame: {item.path} and {seen[key]}")
        seen[key] = item.path
        by_character.setdefault(item.character_id, set()).add((item.pose, item.direction))
    for character_id, frames in sorted(by_character.items()):
        for pose, direction in sorted(FRAME_NAMES - frames):
            set_issues.append(f"missing_frame: char_{character_id}_{pose}_{direction}.png")
    return results, set_issues


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("input", type=Path)
    parser.add_argument("--json", type=Path, help="optional machine-readable report")
    args = parser.parse_args()
    try:
        results, set_issues = validate_directory(args.input)
    except ValueError as exc:
        parser.error(str(exc))
    print("FILE | SIZE | MODE | FOOT | ISSUES")
    for item in results:
        size = f"{item.width}x{item.height}" if item.width else "?"
        print(f"{item.path} | {size} | {item.mode or '?'} | "
              f"{item.foot_anchor or '?'} | {', '.join(item.issues) or 'OK'}")
    for issue in set_issues:
        print(issue)
    failed = sum(bool(item.issues) for item in results) + len(set_issues)
    print(f"Checked {len(results)} files; {failed} file/set failures")
    if args.json:
        args.json.parent.mkdir(parents=True, exist_ok=True)
        args.json.write_text(json.dumps({"files": [asdict(item) for item in results],
                                         "setIssues": set_issues}, ensure_ascii=False,
                                        indent=2) + "\n", encoding="utf-8")
    return 1 if failed else 0


if __name__ == "__main__":
    raise SystemExit(main())
