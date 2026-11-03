"""Compare named image regions using global SSIM, changed pixels and color centroid."""

import json
from pathlib import Path

import numpy as np
from PIL import Image


def ssim(a, b):
    """Global luminance SSIM; for tiny UI regions use changed_fraction too."""
    a, b = np.asarray(a, dtype=np.float64), np.asarray(b, dtype=np.float64)
    if a.shape != b.shape:
        raise ValueError("image_shape_mismatch")
    mean_a, mean_b = a.mean(), b.mean()
    var_a, var_b = a.var(), b.var()
    covariance = ((a - mean_a) * (b - mean_b)).mean()
    c1, c2 = (0.01 * 255) ** 2, (0.03 * 255) ** 2
    return float(((2 * mean_a * mean_b + c1) * (2 * covariance + c2)) /
                 ((mean_a ** 2 + mean_b ** 2 + c1) * (var_a + var_b + c2)))


def centroid(rgb, background):
    rgb = np.asarray(rgb, dtype=np.float64)
    weights = np.linalg.norm(rgb - np.asarray(background), axis=2)
    total = weights.sum()
    if total < 1e-8:
        return (rgb.shape[1] / 2, rgb.shape[0] / 2)
    yy, xx = np.indices(weights.shape)
    return (float((xx * weights).sum() / total), float((yy * weights).sum() / total))


def compare(actual_path, reference_path, specification, overlay_path=None):
    actual = Image.open(actual_path).convert("RGB")
    reference = Image.open(reference_path).convert("RGB")
    if actual.size != reference.size:
        raise ValueError("image_shape_mismatch")
    if isinstance(specification, (str, Path)):
        specification = json.loads(Path(specification).read_text(encoding="utf-8"))
    result = []
    for region in specification["regions"]:
        x, y, width, height = region["rect"]
        box = (x, y, x + width, y + height)
        a, b = np.asarray(actual.crop(box)), np.asarray(reference.crop(box))
        if a.size == 0 or a.shape != (height, width, 3):
            raise ValueError(f"invalid_region:{region['name']}")
        gray_a, gray_b = a.mean(axis=2), b.mean(axis=2)
        score = ssim(gray_a, gray_b)
        changed = float(np.mean(np.max(np.abs(a.astype(int) - b.astype(int)), axis=2) >
                                region.get("pixelThreshold", 20)))
        background = region.get("background", [0, 0, 0])
        ca, cb = centroid(a, background), centroid(b, background)
        shift = float(np.hypot(ca[0] - cb[0], ca[1] - cb[1]))
        passed = score >= region.get("minSsim", 0.8) and \
            changed <= region.get("maxChangedFraction", 0.2) and \
            shift <= region.get("maxCentroidShift", 10)
        result.append({"name": region["name"], "passed": passed,
                       "ssim": round(score, 5), "changed_fraction": round(changed, 5),
                       "centroid_shift_px": round(shift, 3)})
    if overlay_path:
        Path(overlay_path).parent.mkdir(parents=True, exist_ok=True)
        Image.blend(reference, actual, 0.5).save(overlay_path)
    return {"passed": all(r["passed"] for r in result), "regions": result}
