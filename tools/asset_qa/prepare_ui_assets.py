"""README: python tools/asset_qa/prepare_ui_assets.py --source "D:/HITME GAME/ASSET HITME FINAL".
Reads five source groups without modifying them; writes deterministic QA, import
manifest, contacts and alpha-safe staging copies to Evidence/asset_qa.
Run self-tests: python tools/asset_qa/prepare_ui_assets.py --test.
"""
from __future__ import annotations

import argparse
import csv
import hashlib
import json
import re
import unittest
from collections import Counter
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw

GROUPS = ("HITME_UI_KIT_CLEAN", "HITME_AIM_KIT_V1", "ASSET TARGET RULER", "UI_KIT_EXTRACTED", "ASSET EFFECT")
EXTS = {".png", ".jpg", ".jpeg", ".webp"}
KNOWN_BORDERS = {
    "ui_btn_cta": (35, 35, 25, 25), "ui_input_box": (25, 25, 25, 25),
    "ui_panel_dark_gold": (40, 40, 40, 40), "ui_card_char": (30, 30, 30, 30),
    "ui_banner_wood": (40, 40, 20, 20), "ui_row_leaderboard": (30, 30, 30, 30),
}


def digest(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def checkerboard_suspect(rgba: np.ndarray) -> bool:
    """Conservative border check: two alternating opaque neutral colours on grid scale."""
    h, w, _ = rgba.shape
    if min(h, w) < 24:
        return False
    sample = np.concatenate((rgba[: min(32, h)].reshape(-1, 4), rgba[-min(32, h) :].reshape(-1, 4), rgba[:, : min(32, w)].reshape(-1, 4), rgba[:, -min(32, w) :].reshape(-1, 4)))
    opaque = sample[sample[:, 3] == 255, :3]
    if len(opaque) < len(sample) * .8:
        return False
    neutral = opaque[np.max(opaque, axis=1) - np.min(opaque, axis=1) < 10]
    if len(neutral) < len(opaque) * .85:
        return False
    # A mere grey border is not evidence of checkerboard: require two common tones.
    counts = Counter(tuple(int(v // 8) for v in rgb) for rgb in neutral)
    top = counts.most_common(2)
    if len(top) < 2 or sum(v for _, v in top) < len(neutral) * .75:
        return False
    a, b = top[0][0][0] * 8, top[1][0][0] * 8
    return 10 <= abs(a - b) <= 100 and min(v for _, v in top) > len(neutral) * .1


def scan(path: Path, source: Path) -> dict:
    with Image.open(path) as raw:
        mode = raw.mode
        has_alpha = mode in ("RGBA", "LA", "PA") or "transparency" in raw.info
        rgba = np.array(raw.convert("RGBA"))
    h, w = rgba.shape[:2]
    alpha = rgba[:, :, 3]
    mask = alpha > 0
    ys, xs = np.where(mask)
    bbox = [int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1] if len(xs) else [0, 0, 0, 0]
    name = path.name.lower()
    border = next((value for key, value in KNOWN_BORDERS.items() if key in name), None)
    baked = bool(re.search(r"(chảo|chao|dép|dep|label|text|caption|title)", name, re.I))
    return {
        "source": path.relative_to(source).as_posix(), "width": w, "height": h,
        "mode": mode, "has_alpha": has_alpha,
        "transparent_pct": round(float(np.mean(alpha == 0) * 100), 3),
        "semialpha_pct": round(float(np.mean((alpha > 0) & (alpha < 255)) * 100), 3),
        "checkerboard_suspect": checkerboard_suspect(rgba),
        "alpha_bbox": bbox,
        "touches_edge": bool(mask[0].any() or mask[-1].any() or mask[:, 0].any() or mask[:, -1].any()),
        "power_of_two": w & (w - 1) == 0 and h & (h - 1) == 0,
        "sha256": digest(path), "baked_text_suspect": baked,
        "nine_slice": list(border) if border else None,
        "nine_slice_confidence": "low_map_only" if border else "none",
        "pixels_per_unit_suggested": 100,
        "atlas_group": path.relative_to(source).parts[0],
    }


def contact_sheet(paths: list[Path], target: Path, source: Path) -> None:
    if not paths:
        return
    cols, cell_w, cell_h = 5, 220, 195
    rows = (len(paths) + cols - 1) // cols
    sheet = Image.new("RGB", (cols * cell_w, rows * cell_h), "#202b36")
    draw = ImageDraw.Draw(sheet)
    entries = []
    for index, path in enumerate(paths):
        with Image.open(path) as im:
            tile = im.convert("RGBA")
            tile.thumbnail((200, 158))
        x, y = index % cols * cell_w, index // cols * cell_h
        sheet.paste(tile, (x + (cell_w - tile.width) // 2, y), tile)
        label = path.name[:30]
        draw.text((x + 4, y + 163), label, fill="white")
        entries.append({"source": path.relative_to(source).as_posix(), "cell": [x, y, cell_w, cell_h]})
    target.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(target)
    target.with_suffix(".json").write_text(json.dumps(entries, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def stage_asset(path: Path, target: Path, source: Path, border: list[int] | None = None) -> dict:
    with Image.open(path) as im:
        rgba = im.convert("RGBA")
    bbox = rgba.getbbox()
    if bbox is None:
        raise ValueError("empty alpha image")
    cropped = rgba.crop(bbox)
    padded = Image.new("RGBA", (cropped.width + 8, cropped.height + 8), (0, 0, 0, 0))
    padded.alpha_composite(cropped, (4, 4))
    target.parent.mkdir(parents=True, exist_ok=True)
    padded.save(target)
    before_after = Image.new("RGB", (rgba.width + padded.width, max(rgba.height, padded.height)), "#00ff00")
    before_after.paste(rgba, (0, 0), rgba)
    before_after.paste(padded, (rgba.width, 0), padded)
    preview = target.with_name(target.stem + "_before_after.png")
    before_after.save(preview)
    provenance = {"source": path.relative_to(source).as_posix(), "source_sha256": digest(path),
                  "output": target.name, "output_sha256": digest(target), "method": "alpha crop and 4px transparent padding; no checkerboard removal"}
    if border:
        left, right, top, bottom = border
        provenance["staged_nine_slice_suggested"] = [max(0, left - bbox[0] + 4), max(0, right - (rgba.width - bbox[2]) + 4),
                                                   max(0, top - bbox[1] + 4), max(0, bottom - (rgba.height - bbox[3]) + 4)]
    target.with_suffix(".json").write_text(json.dumps(provenance, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return provenance


def run(source: Path, output: Path) -> dict:
    paths = sorted((p for group in GROUPS for p in (source / group).rglob("*") if p.is_file() and p.suffix.lower() in EXTS), key=lambda p: p.relative_to(source).as_posix().casefold())
    rows = [scan(p, source) for p in paths]
    hashes = Counter(row["sha256"] for row in rows)
    for row in rows:
        row["duplicate_count"] = hashes[row["sha256"]]
        row["notes"] = ", ".join(label for flag, label in ((row["checkerboard_suspect"], "possible baked checkerboard"), (row["touches_edge"], "alpha touches edge"), (row["baked_text_suspect"], "possible baked text; review manually"), (row["duplicate_count"] > 1, "duplicate bytes")) if flag)
    output.mkdir(parents=True, exist_ok=True)
    fields = [k for k in rows[0] if k != "sha256"] if rows else []
    with (output / "ui_kit_qa.csv").open("w", newline="", encoding="utf-8-sig") as file:
        writer = csv.DictWriter(file, fieldnames=fields)
        writer.writeheader()
        for row in rows: writer.writerow({k: json.dumps(v, ensure_ascii=False) if isinstance(v, list) else v for k, v in row.items() if k != "sha256"})
    (output / "ui_import_manifest.json").write_text(json.dumps(rows, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    for group in GROUPS:
        contact_sheet([p for p in paths if p.relative_to(source).parts[0] == group], output / "contact" / ("ui_kit_" + group.lower().replace(" ", "_") + ".png"), source)
    staged = []
    for path, row in zip(paths, rows):
        if path.relative_to(source).parts[0] != "HITME_UI_KIT_CLEAN" or row["checkerboard_suspect"] or row["baked_text_suspect"]:
            continue
        staged.append(stage_asset(path, output / "staging" / "ui" / path.name, source, row["nine_slice"]))
    palette_counts: Counter[tuple[int, int, int]] = Counter()
    for path in paths:
        if path.relative_to(source).parts[0] != "HITME_UI_KIT_CLEAN":
            continue
        with Image.open(path) as image:
            pixels = np.asarray(image.convert("RGBA"))[::8, ::8].reshape(-1, 4)
        for pixel in pixels[pixels[:, 3] > 200]:
            palette_counts[tuple(int(value // 32 * 32 + 16) for value in pixel[:3])] += 1
    palette = ["#%02X%02X%02X" % rgb for rgb, _ in palette_counts.most_common(8)]
    report = ["# HIT ME – UI asset QA", "", "Nguồn: `ASSET HITME FINAL` (chỉ đọc). Lệnh tái chạy: `python tools/asset_qa/prepare_ui_assets.py`.", "",
              f"Đã quét **{len(rows)} ảnh**, {sum(r['has_alpha'] for r in rows)} có alpha, {sum(r['touches_edge'] for r in rows)} chạm mép, {sum(v - 1 for v in hashes.values() if v > 1)} bản trùng byte, {sum(r['checkerboard_suspect'] for r in rows)} nghi ô vuông theo phép dò biên bảo thủ. Đã tạo {len(staged)} bản sao UI clean.", "",
              "Bảng chi tiết: `ui_kit_qa.csv`; manifest import: `ui_import_manifest.json`; ảnh ghép: `contact/`; bản sao và ảnh trước/sau: `staging/ui/`.", "",
              "## Rủi ro trước khi nhập Unity", "",
              "- 9-slice lấy từ `UI_ASSET_MAP.md`, chỉ là **đề xuất độ tin cậy thấp**, chưa đo góc bằng mắt từng ảnh. Bản staging đã cắt alpha nên phải dùng viền điều chỉnh trong sidecar `.json`, không lấy nguyên viền của file gốc.",
              "- Phép dò ô vuông chỉ xét hai màu trung tính trên biên. Giá trị 0 không chứng minh mọi ảnh sạch. Không tự xóa ô vuông hoặc defringe trong bộ này.",
              "- Cờ chữ in sẵn dựa trên **tên tệp**, không phải OCR. 18 ảnh `HITME_UI_KIT_CLEAN` được xem nhanh trên contact sheet: không thấy chữ. Các ảnh khác vẫn cần AG duyệt bằng mắt.",
              "- `touches_edge` có thể là sprite cố ý tràn canvas; cần xem trước khi tự cắt. Bản staging chỉ áp dụng UI kit clean và giữ nguyên gốc.", "",
              "## Màu kit (lấy mẫu pixel, lượng tử 32 mức)", "", ", ".join(palette), "",
              "## Thiếu hoặc chưa xác minh cho Lô 2–4", "", "| Mục | Tình trạng |", "|---|---|",
              "| Home: logo HIT ME tách nền | Chưa thấy tệp đặt tên logo; cần AG/chủ dự án chọn nguồn |",
              "| Luyện với bot: bộ chọn số người, nút và nền | Nút có trong kit; bố cục và icon chưa xác minh |",
              "| Phòng: biểu tượng chia sẻ/mã phòng/khóa | Chưa thấy tệp đặt tên riêng; có thể dùng icon vector/TMP tạm |",
              "| Kết quả: huy chương hạng 1–3 | `UI_ASSET_MAP.md` ghi `[TẠM]`, chưa thấy sprite riêng |",
              "| Tài nguyên chữ in sẵn | Không dùng thay cho nhãn TMP tiếng Việt |", "",
              "Ưu tiên: (1) duyệt 18 sprite clean trên contact sheet, (2) duyệt border 9-slice và staging, (3) chọn logo/huy chương, (4) kiểm riêng 39 ảnh chạm mép.", ""]
    (output / "UI_KIT_REPORT.md").write_text("\n".join(report), encoding="utf-8")
    return {"images": len(rows), "duplicates": sum(v - 1 for v in hashes.values() if v > 1),
            "checkerboard_suspects": sum(r["checkerboard_suspect"] for r in rows),
            "baked_text_suspects": sum(r["baked_text_suspect"] for r in rows),
            "staged": len(staged), "rows": rows}


class SelfTests(unittest.TestCase):
    def test_duplicate_hash(self):
        import tempfile
        with tempfile.TemporaryDirectory() as folder:
            a, b = Path(folder) / "a.png", Path(folder) / "b.png"
            Image.new("RGBA", (16, 16), (1, 2, 3, 4)).save(a)
            b.write_bytes(a.read_bytes())
            self.assertEqual(digest(a), digest(b))

    def test_baked_text_filename_flag(self):
        import tempfile
        with tempfile.TemporaryDirectory() as folder:
            p = Path(folder) / "DEP_label.png"
            Image.new("RGBA", (16, 16), "red").save(p)
            self.assertTrue(scan(p, Path(folder))["baked_text_suspect"])

    def test_checkerboard_sample(self):
        a = np.zeros((64, 64, 4), dtype=np.uint8)
        for y in range(64):
            for x in range(64):
                value = 192 if ((x // 16 + y // 16) % 2) else 232
                a[y, x] = [value, value, value, 255]
        self.assertTrue(checkerboard_suspect(a))


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", type=Path, default=Path("D:/HITME GAME/ASSET HITME FINAL"))
    parser.add_argument("--output", type=Path, default=Path(__file__).resolve().parents[2] / "Evidence" / "asset_qa")
    parser.add_argument("--test", action="store_true")
    args = parser.parse_args()
    if args.test:
        result = unittest.TestResult()
        SelfTests().defaultTestResult()
        suite = unittest.defaultTestLoader.loadTestsFromTestCase(SelfTests)
        suite.run(result)
        print(f"{'Passed!' if result.wasSuccessful() else 'Failed!'} {result.testsRun - len(result.failures) - len(result.errors)}/{result.testsRun}")
        for _, error in result.failures + result.errors: print(error)
        raise SystemExit(0 if result.wasSuccessful() else 1)
    summary = run(args.source, args.output)
    print(json.dumps({k: v for k, v in summary.items() if k != "rows"}, ensure_ascii=False))
