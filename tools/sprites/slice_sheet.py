"""Slice a flat-green HIT ME sprite sheet into transparent 256x256 frames.

README: python tools/sprites/slice_sheet.py SHEET OUTPUT_DIR --id nam_lang
  --poses hit,ko --direction down --rows 2
Use --rows 3 with three comma-separated poses. Input must be four columns,
pure #00FF00 background, no merged or missing cells. On rejection writes only
OUTPUT_DIR/rejected_annotated.png. On success writes gp_<id>_<pose>_<dir>_<frame>.png
and anchors.json. Source is read-only; every frame shares scale and foot line.
These are partial animation frames, not the complete 18-pose package expected
by validate_pack.py; do not pass them to pack_sprites.py until the set is complete.
"""

from __future__ import annotations

import argparse
import json
import re
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw


def _annotate(image: Image.Image, rows: int, reasons: list[str], path: Path) -> None:
    canvas = image.convert("RGB").copy()
    pen = ImageDraw.Draw(canvas)
    w, h = canvas.size
    for col in range(1, 4):
        pen.line((col * w // 4, 0, col * w // 4, h), fill="red", width=3)
    for row in range(1, rows):
        pen.line((0, row * h // rows, w, row * h // rows), fill="red", width=3)
    for i, reason in enumerate(reasons[:12]):
        pen.text((8, 8 + i * 18), reason, fill="red", stroke_width=2, stroke_fill="white")
    path.parent.mkdir(parents=True, exist_ok=True)
    canvas.save(path)


def slice_sheet(sheet: Path, output: Path, character_id: str, poses: list[str],
                direction: str = "down") -> tuple[list[Path], Path]:
    if not re.fullmatch(r"[a-z][a-z0-9_]*", character_id):
        raise ValueError("Invalid character id")
    if direction not in {"down", "up", "side"} or not poses or len(poses) not in (2, 3):
        raise ValueError("Expected 2 or 3 poses and a known direction")
    if any(pose not in {"idle", "aim", "throw", "hit", "ko", "win"} for pose in poses):
        raise ValueError("Unknown pose")
    rows = len(poses)
    with Image.open(sheet) as source:
        image = source.convert("RGB")
    width, height = image.size
    reasons: list[str] = []
    if width % 4 or height % rows:
        reasons.append("Sheet dimensions do not divide into 4 columns and selected rows")
    if width < 4 * 64 or height < rows * 64:
        reasons.append("Cells are too small")
    if reasons:
        _annotate(image, rows, reasons, output / "rejected_annotated.png")
        raise ValueError("; ".join(reasons))
    pixels = np.asarray(image).astype(np.int16)
    cell_w, cell_h = width // 4, height // rows
    boxes: list[tuple[int, int, int, int, int, int]] = []
    for row in range(rows):
        for col in range(4):
            x0, y0 = col * cell_w, row * cell_h
            cell = pixels[y0:y0 + cell_h, x0:x0 + cell_w]
            green_distance = np.max(np.abs(cell - np.array([0, 255, 0])), axis=2)
            foreground = green_distance > 16
            yy, xx = np.nonzero(foreground)
            number = row * 4 + col + 1
            if len(xx) < 50:
                reasons.append(f"Cell {number}: missing frame")
                continue
            if xx.min() < 3 or yy.min() < 3 or xx.max() >= cell_w - 3 or yy.max() >= cell_h - 3:
                reasons.append(f"Cell {number}: figure touches cell edge or merged frames")
            # Pure green is required in the outer margin. This catches uneven
            # backgrounds independently of character colors.
            margin = np.concatenate((cell[:2].reshape(-1, 3), cell[-2:].reshape(-1, 3),
                                     cell[:, :2].reshape(-1, 3), cell[:, -2:].reshape(-1, 3)))
            if np.any(np.max(np.abs(margin - [0, 255, 0]), axis=1) > 8):
                reasons.append(f"Cell {number}: uneven green background")
            # Bright green leftovers touching the silhouette cannot be keyed
            # reliably, so reject instead of silently leaving a colored halo.
            fringe = (cell[:, :, 1] > 140) & (cell[:, :, 1] > cell[:, :, 0] * 1.5) & (cell[:, :, 1] > cell[:, :, 2] * 1.5) & foreground
            if int(fringe.sum()) > 10:
                reasons.append(f"Cell {number}: green fringe")
            boxes.append((x0 + int(xx.min()), y0 + int(yy.min()),
                          x0 + int(xx.max()) + 1, y0 + int(yy.max()) + 1, row, col))
    if len(boxes) != rows * 4:
        reasons.append(f"Detected {len(boxes)}/{rows * 4} frames")
    if reasons:
        _annotate(image, rows, reasons, output / "rejected_annotated.png")
        raise ValueError("; ".join(reasons))
    if output.exists() and any(output.iterdir()):
        raise FileExistsError("Output contains files; choose a fresh directory")
    max_height = max(y1 - y0 for _, y0, _, y1, _, _ in boxes)
    factor = 190 / max_height
    if any(round((x1 - x0) * factor) > 248 or round((y1 - y0) * factor) > 248
           for x0, y0, x1, y1, _, _ in boxes):
        _annotate(image, rows, ["Scaled frame exceeds 256x256"], output / "rejected_annotated.png")
        raise ValueError("Scaled frame exceeds 256x256")
    output.mkdir(parents=True, exist_ok=True)
    paths: list[Path] = []
    metadata: list[dict] = []
    for x0, y0, x1, y1, row, col in boxes:
        crop = image.crop((x0, y0, x1, y1))
        raw = np.asarray(crop).astype(np.int16)
        distance = np.max(np.abs(raw - [0, 255, 0]), axis=2)
        alpha = np.where(distance > 16, 255, 0).astype(np.uint8)
        rgba = np.dstack((raw.astype(np.uint8), alpha))
        sprite = Image.fromarray(rgba, "RGBA")
        size = (round(sprite.width * factor), round(sprite.height * factor))
        sprite = sprite.resize(size, Image.Resampling.NEAREST)
        frame = Image.new("RGBA", (256, 256))
        # Align the lowest opaque pixels to one common ground line.
        sx = (256 - sprite.width) // 2
        sy = 253 - sprite.height + 1
        frame.alpha_composite(sprite, (sx, sy))
        name = f"gp_{character_id}_{poses[row]}_{direction}_{col + 1}.png"
        path = output / name
        frame.save(path)
        paths.append(path)
        metadata.append({"name": name, "pose": poses[row], "direction": direction,
                         "frame": col + 1, "pivotPixelsTopLeft": {"x": 128, "y": 253},
                         "sourceRect": [x0, y0, x1, y1]})
    manifest = output / "anchors.json"
    manifest.write_text(json.dumps({"version": 1, "frameSize": 256,
                                    "scale": factor, "frames": metadata}, indent=2) + "\n", encoding="utf-8")
    return paths, manifest


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("sheet", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--id", required=True)
    parser.add_argument("--poses", required=True, help="comma-separated row pose names")
    parser.add_argument("--direction", default="down")
    parser.add_argument("--rows", type=int, choices=(2, 3), default=2)
    args = parser.parse_args()
    poses = args.poses.split(",")
    if len(poses) != args.rows:
        parser.error("--rows must equal the number of --poses")
    try:
        paths, manifest = slice_sheet(args.sheet, args.output, args.id, poses, args.direction)
    except (ValueError, FileExistsError) as exc:
        parser.error(str(exc))
    print(f"Wrote {len(paths)} frames and {manifest}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
