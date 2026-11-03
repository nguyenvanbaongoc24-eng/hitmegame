"""Pack validated HIT ME gameplay frames into a lossless PNG atlas and JSON.

README: python tools/sprites/pack_sprites.py INPUT_DIR OUTPUT_DIR [--columns 6]
Requires Pillow, numpy, and validate_pack.py in the same directory. Refuses
incomplete/invalid sprite sets, existing outputs, and an output inside input.
The source images are opened read-only and are never resized or rewritten.
Atlas rects and pivots use pixel coordinates from the top-left. AG can map
the pixel pivot to Unity normalized coordinates as (x/256, 1-y/256).
"""

from __future__ import annotations

import argparse
import json
import math
from pathlib import Path

from PIL import Image

from validate_pack import SIZE, validate_directory


def pack(input_dir: Path, output_dir: Path, columns: int = 6) -> tuple[Path, Path]:
    source = input_dir.resolve()
    target = output_dir.resolve()
    if target == source or source in target.parents:
        raise ValueError("Output must be outside the source directory")
    if columns < 1 or columns > 64:
        raise ValueError("columns must be between 1 and 64")
    results, set_issues = validate_directory(source)
    if set_issues or any(item.issues for item in results):
        raise ValueError("Sprite set did not pass validation; run validate_pack.py first")
    atlas_path, manifest_path = target / "gameplay_atlas.png", target / "gameplay_atlas.json"
    if atlas_path.exists() or manifest_path.exists():
        raise FileExistsError("Atlas or manifest already exists; choose a new output directory")
    rows = math.ceil(len(results) / columns)
    atlas = Image.new("RGBA", (columns * SIZE, rows * SIZE), (0, 0, 0, 0))
    frames = []
    for index, item in enumerate(results):
        x, y = index % columns * SIZE, index // columns * SIZE
        with Image.open(source / item.path) as frame:
            atlas.alpha_composite(frame, (x, y))
        frames.append({"name": Path(item.path).name, "source": item.path,
                       "characterId": item.character_id, "pose": item.pose,
                       "direction": item.direction,
                       "rect": {"x": x, "y": y, "width": SIZE, "height": SIZE},
                       "pivotPixelsTopLeft": {"x": item.foot_anchor[0],
                                              "y": item.foot_anchor[1]}})
    manifest = {"version": 1, "atlas": atlas_path.name,
                "coordinateSystem": "top-left pixels",
                "width": atlas.width, "height": atlas.height,
                "frameSize": SIZE, "frames": frames}
    target.mkdir(parents=True, exist_ok=True)
    atlas.save(atlas_path, format="PNG", optimize=True)
    manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n",
                             encoding="utf-8")
    return atlas_path, manifest_path


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("input", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--columns", type=int, default=6)
    args = parser.parse_args()
    try:
        atlas, manifest = pack(args.input, args.output, args.columns)
    except (ValueError, FileExistsError) as exc:
        parser.error(str(exc))
    print(f"Atlas: {atlas}")
    print(f"Manifest: {manifest}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
