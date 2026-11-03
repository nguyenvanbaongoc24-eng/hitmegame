"""Extract UI kit pieces from transparent or #00FF00 artwork.

README: python tools/ui/slice_ui_kit.py KIT.png OUT --names "Nút Chơi,Khung phòng,..."
--expected N. Components within --merge-gap pixels are joined. On a count
mismatch, only rejected_annotated.png is written; no guessed names/assets.
The source is read-only. A Vietnamese TTF is bundled in fonts/ for labels.
Requires Pillow and numpy.
"""

from __future__ import annotations

import argparse
import json
import re
from collections import deque
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont


FONT = Path(__file__).parent / "fonts" / "BeVietnamPro-Medium.ttf"


def _foreground(image: Image.Image) -> np.ndarray:
    rgba = np.asarray(image.convert("RGBA"))
    if image.mode in {"RGBA", "LA", "PA"} and np.any(rgba[:, :, 3] < 240):
        return rgba[:, :, 3] > 32
    rgb = rgba[:, :, :3].astype(np.int16)
    return np.max(np.abs(rgb - [0, 255, 0]), axis=2) > 20


def _components(mask: np.ndarray, min_area: int) -> list[tuple[int, int, int, int]]:
    # Connected components on a two-pixel grid keep large UI kits inexpensive.
    reduced = mask[::2, ::2]
    h, w = reduced.shape
    seen = np.zeros((h, w), dtype=bool)
    boxes = []
    for sy, sx in zip(*np.nonzero(reduced)):
        if seen[sy, sx]:
            continue
        queue = deque([(int(sy), int(sx))])
        seen[sy, sx] = True
        area = 0
        x0 = x1 = int(sx)
        y0 = y1 = int(sy)
        while queue:
            y, x = queue.popleft()
            area += 1
            x0, x1 = min(x0, x), max(x1, x)
            y0, y1 = min(y0, y), max(y1, y)
            for yy in range(max(0, y - 1), min(h, y + 2)):
                for xx in range(max(0, x - 1), min(w, x + 2)):
                    if reduced[yy, xx] and not seen[yy, xx]:
                        seen[yy, xx] = True
                        queue.append((yy, xx))
        if area * 4 >= min_area:
            boxes.append((2 * x0, 2 * y0, min(mask.shape[1], 2 * x1 + 2), min(mask.shape[0], 2 * y1 + 2)))
    return boxes


def _slug(label: str) -> str:
    import unicodedata
    value = unicodedata.normalize("NFKD", label).encode("ascii", "ignore").decode().lower()
    return re.sub(r"[^a-z0-9]+", "_", value).strip("_") or "item"


def _annotated(image: Image.Image, boxes: list[tuple[int, int, int, int]], path: Path, message: str) -> None:
    preview = image.convert("RGB").copy()
    draw = ImageDraw.Draw(preview)
    font = ImageFont.truetype(FONT, 18)
    for index, box in enumerate(boxes, 1):
        draw.rectangle(box, outline="#ff3030", width=3)
        draw.text((box[0] + 2, box[1] + 2), str(index), font=font, fill="#ff3030", stroke_width=2, stroke_fill="white")
    draw.text((8, 8), message, font=font, fill="#ff3030", stroke_width=2, stroke_fill="white")
    path.parent.mkdir(parents=True, exist_ok=True)
    preview.save(path)


def slice_kit(source: Path, output: Path, names: list[str], expected: int, merge_gap: int = 4,
              min_area: int = 24) -> list[dict]:
    if expected < 1 or len(names) != expected or len({_slug(name) for name in names}) != expected:
        raise ValueError("Expected count, names and unique slugs must match")
    with Image.open(source) as opened:
        image = opened.copy()
    mask = _foreground(image)
    if merge_gap < 0 or merge_gap > 30:
        raise ValueError("merge_gap must be between 0 and 30")
    radius = merge_gap if merge_gap % 2 == 0 else merge_gap + 1
    joined = np.asarray(Image.fromarray((mask * 255).astype(np.uint8)).filter(
        ImageFilter.MaxFilter(radius + 1))) > 0 if radius else mask
    candidates = _components(joined, min_area)
    boxes = []
    for x0, y0, x1, y1 in candidates:
        yy, xx = np.nonzero(mask[y0:y1, x0:x1])
        if len(xx):
            boxes.append((x0 + int(xx.min()), y0 + int(yy.min()),
                          x0 + int(xx.max()) + 1, y0 + int(yy.max()) + 1))
    boxes.sort(key=lambda box: (box[1] // max(1, image.height // 8), box[0], box[1]))
    if len(boxes) != expected:
        _annotated(image, boxes, output / "rejected_annotated.png",
                   f"Phát hiện {len(boxes)}/{expected} món; kiểm tra tay")
        raise ValueError(f"Detected {len(boxes)}/{expected} items")
    if output.exists() and any(output.iterdir()):
        raise FileExistsError("Output contains files; choose a fresh directory")
    output.mkdir(parents=True, exist_ok=True)
    rgba = np.asarray(image.convert("RGBA")).copy()
    rgba[:, :, 3] = np.where(mask, rgba[:, :, 3] if image.mode in {"RGBA", "LA", "PA"} else 255, 0)
    records = []
    previews = []
    font = ImageFont.truetype(FONT, 18)
    for index, (label, box) in enumerate(zip(names, boxes), 1):
        x0, y0, x1, y1 = box
        crop = Image.fromarray(rgba[y0:y1, x0:x1], "RGBA")
        filename = f"{index:02d}_{_slug(label)}.png"
        crop.save(output / filename)
        records.append({"name": label, "file": filename,
                        "rect": {"x": x0, "y": y0, "width": x1 - x0, "height": y1 - y0},
                        "size": [x1 - x0, y1 - y0]})
        tile = Image.new("RGB", (260, 250), "#30353c")
        thumb = crop.copy()
        thumb.thumbnail((236, 205))
        tile.paste(thumb, ((260 - thumb.width) // 2, 6), thumb)
        ImageDraw.Draw(tile).text((8, 218), label, font=font, fill="white")
        previews.append(tile)
    sheet = Image.new("RGB", (260 * min(4, len(previews)), 250 * ((len(previews) + 3) // 4)), "#30353c")
    for index, tile in enumerate(previews):
        sheet.paste(tile, ((index % 4) * 260, (index // 4) * 250))
    sheet.save(output / "contact_sheet.png")
    (output / "items.json").write_text(json.dumps({"source": str(source), "items": records}, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return records


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--names", required=True, help="comma-separated names in reading order")
    parser.add_argument("--expected", type=int, required=True)
    parser.add_argument("--merge-gap", type=int, default=4)
    args = parser.parse_args()
    try:
        items = slice_kit(args.source, args.output, [name.strip() for name in args.names.split(",")],
                          args.expected, args.merge_gap)
    except (ValueError, FileExistsError) as exc:
        parser.error(str(exc))
    print(f"Wrote {len(items)} UI items to {args.output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
