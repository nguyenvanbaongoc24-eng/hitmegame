"""Compare phone screenshots with approved goldens; approval is always explicit.

Run: python tools/blackbox/golden.py compare ACTUAL.png --name home
     python tools/blackbox/golden.py approve ACTUAL.png --name home --approve
"""
from __future__ import annotations

import argparse
import json
from pathlib import Path

import numpy as np
from PIL import Image

SCREENS = ("home", "select", "match_placing", "match_result", "settings")
DEFAULT_ROOT = Path(__file__).resolve().parent / "goldens"


def _ssim(a: np.ndarray, b: np.ndarray) -> float:
    # SSIM over a small region, using the standard luminance/contrast constants.
    x, y = a.astype(np.float64), b.astype(np.float64)
    mx, my = x.mean(), y.mean()
    vx, vy = ((x - mx) ** 2).mean(), ((y - my) ** 2).mean()
    covariance = ((x - mx) * (y - my)).mean()
    c1, c2 = (0.01 * 255) ** 2, (0.03 * 255) ** 2
    return float(((2 * mx * my + c1) * (2 * covariance + c2)) /
                 ((mx * mx + my * my + c1) * (vx + vy + c2)))


def compare(actual: Path, golden: Path, output: Path, min_ssim: float = 0.94,
            max_changed: float = 0.08, pixel_delta: int = 32) -> dict:
    if not golden.exists():
        return {"status": "NO_GOLDEN", "name": golden.stem, "golden": str(golden)}
    with Image.open(actual) as image:
        a = np.asarray(image.convert("RGB"), dtype=np.uint8)
    with Image.open(golden) as image:
        b = np.asarray(image.convert("RGB"), dtype=np.uint8)
    if a.shape != b.shape:
        return {"status": "FAIL", "reason": "different_dimensions", "actual_size": list(a.shape), "golden_size": list(b.shape)}
    height, width = a.shape[:2]
    regions = []
    for row in range(4):
        for col in range(3):
            y0, y1 = row * height // 4, (row + 1) * height // 4
            x0, x1 = col * width // 3, (col + 1) * width // 3
            regions.append(_ssim(a[y0:y1, x0:x1], b[y0:y1, x0:x1]))
    changed = float(np.mean(np.max(np.abs(a.astype(np.int16) - b.astype(np.int16)), axis=2) > pixel_delta))
    structural = float(np.mean(regions))
    output.mkdir(parents=True, exist_ok=True)
    Image.blend(Image.fromarray(a), Image.fromarray(b), 0.5).save(output / "overlay.png")
    Image.fromarray(np.clip(np.abs(a.astype(np.int16) - b.astype(np.int16)) * 4, 0, 255).astype(np.uint8)).save(output / "difference.png")
    result = {"status": "PASS" if structural >= min_ssim and changed <= max_changed else "FAIL",
              "ssim_mean": round(structural, 5), "ssim_regions": [round(v, 5) for v in regions],
              "changed_fraction": round(changed, 5), "min_ssim": min_ssim, "max_changed": max_changed,
              "overlay": str(output / "overlay.png"), "difference": str(output / "difference.png")}
    (output / "comparison.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
    return result


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("mode", choices=("compare", "approve"))
    parser.add_argument("actual", type=Path)
    parser.add_argument("--name", required=True, choices=SCREENS)
    parser.add_argument("--golden-dir", type=Path, default=DEFAULT_ROOT)
    parser.add_argument("--output", type=Path, default=Path("Evidence/blackbox/golden_compare"))
    parser.add_argument("--min-ssim", type=float, default=0.94)
    parser.add_argument("--max-changed", type=float, default=0.08)
    parser.add_argument("--approve", action="store_true")
    args = parser.parse_args()
    golden = args.golden_dir / f"{args.name}.png"
    if args.mode == "approve":
        if not args.approve:
            parser.error("Approving a golden requires --approve")
        with Image.open(args.actual) as image:
            size = image.size
        if size != (1179, 2556):
            parser.error(f"Expected iPhone 15 Pro 1179x2556, got {size}")
        golden.parent.mkdir(parents=True, exist_ok=True)
        golden.write_bytes(args.actual.read_bytes())
        print(f"APPROVED {golden}")
        return 0
    result = compare(args.actual, golden, args.output, args.min_ssim, args.max_changed)
    print(json.dumps(result, ensure_ascii=False))
    return 0 if result["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
