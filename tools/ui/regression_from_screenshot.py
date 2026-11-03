"""Flag large blank-white regions and solid dark edge bands in screenshots.

README: python tools/ui/regression_from_screenshot.py shot.png --output report.json
Thresholds are heuristics for triage, not proof of a visual regression. OCR is
not bundled; question marks and tofu glyphs are explicitly not assessed.
"""

from __future__ import annotations

import argparse
import json
from collections import deque
from pathlib import Path

import numpy as np
from PIL import Image


def _white_regions(mask: np.ndarray, min_fraction: float) -> list[dict]:
    height, width = mask.shape
    seen = np.zeros(mask.shape, dtype=bool)
    regions = []
    for start_y, start_x in zip(*np.nonzero(mask)):
        if seen[start_y, start_x]:
            continue
        queue = deque([(int(start_y), int(start_x))])
        seen[start_y, start_x] = True
        area = 0
        x0 = x1 = int(start_x)
        y0 = y1 = int(start_y)
        while queue:
            y, x = queue.popleft()
            area += 1
            x0, x1 = min(x0, x), max(x1, x)
            y0, y1 = min(y0, y), max(y1, y)
            for yy, xx in ((y - 1, x), (y + 1, x), (y, x - 1), (y, x + 1)):
                if 0 <= yy < height and 0 <= xx < width and mask[yy, xx] and not seen[yy, xx]:
                    seen[yy, xx] = True
                    queue.append((yy, xx))
        if area >= min_fraction * height * width:
            regions.append({"rect": {"x": x0, "y": y0, "width": x1 - x0 + 1,
                                      "height": y1 - y0 + 1},
                            "pixel_fraction": round(area / (height * width), 5)})
    return sorted(regions, key=lambda r: r["pixel_fraction"], reverse=True)


def _edge_band(dark: np.ndarray, from_top: bool, coverage: float) -> dict:
    rows = dark if from_top else dark[::-1]
    # A complete band must cover almost the entire screen width on every row.
    width_fraction = np.mean(rows, axis=1)
    count = 0
    for fraction in width_fraction:
        if fraction < coverage:
            break
        count += 1
    return {"rows": count, "width_coverage_min": round(float(np.min(width_fraction[:count])), 4) if count else 0.0}


def analyze(path: Path, white_fraction: float = 0.05, dark_coverage: float = 0.98,
            band_fraction: float = 0.08) -> dict:
    with Image.open(path) as opened:
        rgb = np.asarray(opened.convert("RGB"), dtype=np.uint8)
    height, width = rgb.shape[:2]
    # Downsample for bounded cost on full-resolution phone screenshots.
    factor = max(1, max(width, height) // 600)
    sample = rgb[::factor, ::factor]
    white = np.all(sample >= 250, axis=2)
    dark = np.all(sample <= 12, axis=2)
    top = _edge_band(dark, True, dark_coverage)
    bottom = _edge_band(dark, False, dark_coverage)
    regions = _white_regions(white, white_fraction)
    for region in regions:
        for key in ("x", "y", "width", "height"):
            region["rect"][key] *= factor
    top["rows"] *= factor
    bottom["rows"] *= factor
    return {"file": str(path), "size": [width, height], "white_regions": regions,
            "edge_bands": {"top": top, "bottom": bottom},
            "flags": {"large_blank_white": bool(regions),
                      "top_black_band": top["rows"] >= height * band_fraction,
                      "bottom_black_band": bottom["rows"] >= height * band_fraction},
            "ocr": {"supported": False,
                    "note": "Question marks and missing-glyph boxes not assessed; OCR is not installed."}}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("image", type=Path)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    report = analyze(args.image)
    result = json.dumps(report, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(result, encoding="utf-8")
    print(result, end="")
    return 1 if any(report["flags"].values()) else 0


if __name__ == "__main__":
    raise SystemExit(main())
