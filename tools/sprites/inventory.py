"""Read Unity sprite metadata and write independent frame QA outside Assets.

Run: python tools/sprites/inventory.py --source "D:/HITME GAME/unity-client/Assets/Art/Characters"
Only source PNGs and their .meta files are read. All output is under
tools/sprites/out. Flags are screening hints; inspect contact sheets manually.
"""

import argparse
import json
from collections import deque
from pathlib import Path

import numpy as np
import yaml
from PIL import Image, ImageDraw, ImageFont


ROSTER = {
    "Nam Làng": "Cau_Be_Lang",
    "Nữ Năng Động": "CÔ_NÀNG_THỂ_THAO",
    "Quậy Boy": "ANH_NÚI",
    "Cô Ba": None,
    "Anh Teen": "ANH_NÓN_LÁ",
    "Chó Vàng": "CHÚ_CHÓ_VÀNG",
    "Mèo Mun": "MÈO_MUN",
}


def components(mask, step=4):
    """8-neighbor areas/boxes; default is a quarter-sized opaque mask."""
    reduced = mask[::step, ::step]
    height, width = reduced.shape
    seen = np.zeros(reduced.shape, dtype=bool)
    found = []
    for sy, sx in zip(*np.nonzero(reduced)):
        if seen[sy, sx]:
            continue
        seen[sy, sx] = True
        queue = deque([(int(sy), int(sx))])
        area = 0
        min_x = max_x = int(sx)
        min_y = max_y = int(sy)
        while queue:
            y, x = queue.popleft()
            area += 1
            min_x, max_x = min(min_x, x), max(max_x, x)
            min_y, max_y = min(min_y, y), max(max_y, y)
            for ny in range(max(0, y - 1), min(height, y + 2)):
                for nx in range(max(0, x - 1), min(width, x + 2)):
                    if reduced[ny, nx] and not seen[ny, nx]:
                        seen[ny, nx] = True
                        queue.append((ny, nx))
        found.append((area, (min_x * step, min_y * step, (max_x + 1) * step, (max_y + 1) * step)))
    return sorted(found, reverse=True)


def classify(image, expected_size=None):
    rgba = np.asarray(image.convert("RGBA"))
    height, width = rgba.shape[:2]
    opaque = rgba[:, :, 3] > 32
    area = int(opaque.sum())
    reasons = []
    if image.mode not in ("RGBA", "LA", "PA") or np.all(rgba[:, :, 3] == 255):
        reasons.append("no_real_alpha")
    if area == 0:
        reasons.append("empty")
        return reasons, {"opaque_fraction": 0, "components": 0}
    if opaque[0].any() or opaque[-1].any() or opaque[:, 0].any() or opaque[:, -1].any():
        reasons.append("touches_frame_edge")
    if width / height > 1.8 or height / width > 2.5:
        reasons.append("unusual_aspect")
    if expected_size and (abs(width - expected_size[0]) > expected_size[0] * 0.15 or
                          abs(height - expected_size[1]) > expected_size[1] * 0.15):
        reasons.append("size_outlier")
    groups = components(opaque)
    big = [g for g in groups if g[0] * 16 >= area * 0.08]
    if len(big) > 1:
        reasons.append("multiple_large_components")
    top_groups = components(opaque[:max(1, height // 3)], step=1)
    small = [g for g in top_groups if 2 <= g[0] <= max(30, area * 0.01)]
    if len(small) >= 5:
        ys = [g[1][1] for g in small]
        if max(ys) - min(ys) <= height * 0.22:
            reasons.append("possible_text_or_ui")
    return reasons, {"opaque_fraction": round(area / (width * height), 4),
                     "components": len(groups), "large_components": len(big)}


def read_slices(meta_path):
    doc = yaml.safe_load(meta_path.read_text(encoding="utf-8"))
    return doc.get("TextureImporter", {}).get("spriteSheet", {}).get("sprites", [])


def crop_unity(image, rect):
    x, y, width, height = (int(rect[key]) for key in ("x", "y", "width", "height"))
    if min(x, y, width, height) < 0 or width == 0 or height == 0 or x + width > image.width or y + height > image.height:
        raise ValueError("rect_outside_texture")
    return image.crop((x, image.height - y - height, x + width, image.height - y))


def font(size):
    for path in (Path("C:/Windows/Fonts/arial.ttf"), Path("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf")):
        if path.exists():
            return ImageFont.truetype(str(path), size)
    raise RuntimeError("Vietnamese-capable font required for contact sheet")


def contact_sheet(rows, output):
    cell_w, cell_h = 246, 300
    sheet = Image.new("RGB", (cell_w * 4, cell_h * ((len(rows) + 3) // 4)), "#20242a")
    draw = ImageDraw.Draw(sheet)
    label_font = font(17)
    detail_font = font(13)
    for index, (label, image, reasons) in enumerate(rows):
        x, y = index % 4 * cell_w, index // 4 * cell_h
        thumb = image.copy().convert("RGBA")
        thumb.thumbnail((224, 220))
        checker = Image.new("RGBA", thumb.size, "#4a784c")
        checker.alpha_composite(thumb)
        sheet.paste(checker.convert("RGB"), (x + (cell_w - thumb.width) // 2, y + 7))
        draw.text((x + 5, y + 234), label, font=label_font, fill="white")
        status = "CẦN SOI" if reasons else "TẠM ỔN"
        draw.text((x + 5, y + 258), status + ": " + ",".join(reasons)[:26],
                  font=detail_font, fill="#ff8585" if reasons else "#97e6ac")
    output.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(output)


def inventory(source, out):
    source, out = Path(source), Path(out)
    if out.resolve().is_relative_to(source.resolve()):
        raise ValueError("output_must_be_outside_Unity_source")
    if not source.is_dir():
        raise FileNotFoundError(source)
    results = []
    representative = {}
    out.mkdir(parents=True, exist_ok=True)
    for png in sorted(source.rglob("*.png"), key=lambda p: str(p).casefold()):
        meta = Path(str(png) + ".meta")
        if not meta.exists():
            results.append({"source": str(png.relative_to(source)), "error": "missing_meta"})
            continue
        with Image.open(png) as opened:
            texture = opened.convert("RGBA")
        slices = read_slices(meta)
        dimensions = [(int(s["rect"]["width"]), int(s["rect"]["height"])) for s in slices]
        expected = max(set(dimensions), key=dimensions.count) if dimensions else None
        stem = png.relative_to(source).with_suffix("")
        for index, sprite in enumerate(slices):
            name = sprite["name"]
            record = {"source": str(png.relative_to(source)), "sprite": name,
                      "rect_unity": sprite["rect"], "index": index}
            try:
                frame = crop_unity(texture, sprite["rect"])
                reasons, metrics = classify(frame, expected)
                record.update({"flags": reasons, "metrics": metrics, "verdict": "inspect" if reasons else "provisionally_valid"})
                destination = out / "frames" / stem / f"{index:03d}.png"
                destination.parent.mkdir(parents=True, exist_ok=True)
                frame.save(destination)
                record["frame_file"] = str(destination.relative_to(out))
                if index == 0:
                    representative[str(png.parent.name).casefold()] = (frame, reasons, record)
            except ValueError as error:
                record.update({"flags": [str(error)], "verdict": "invalid_rect"})
            results.append(record)
    cards = []
    roster_status = {}
    for name, folder in ROSTER.items():
        match = representative.get(folder.casefold()) if folder else None
        if match is None:
            blank = Image.new("RGBA", (160, 200), "#34383e")
            cards.append((name, blank, ["source_not_identified"]))
            roster_status[name] = "source_not_identified"
        else:
            image, flags, record = match
            cards.append((name, image, flags))
            roster_status[name] = record["source"]
    contact_sheet(cards, out / "contact_sheet.png")
    (out / "inventory.json").write_text(json.dumps({"source": str(source), "frames": results,
        "roster_representatives": roster_status}, ensure_ascii=False, indent=2), encoding="utf-8")
    return results


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", default=r"D:\HITME GAME\unity-client\Assets\Art\Characters")
    parser.add_argument("--out", default="tools/sprites/out")
    args = parser.parse_args()
    frames = inventory(args.source, args.out)
    print(f"frames={len(frames)}, flagged={sum(bool(f.get('flags')) for f in frames)}; output={args.out}")
