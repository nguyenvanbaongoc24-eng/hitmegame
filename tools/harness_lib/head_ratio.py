"""Measure head/height from four anatomy landmarks in image coordinates."""

import json
from pathlib import Path


def check(anatomy):
    if isinstance(anatomy, (str, Path)):
        anatomy = json.loads(Path(anatomy).read_text(encoding="utf-8"))
    points = anatomy["points"]
    top, chin, hip, heel = (float(points[k][1]) for k in ("headTop", "chin", "hip", "heel"))
    if not top < chin < hip < heel:
        return {"passed": False, "error": "landmark_order_invalid"}
    ratio = (chin - top) / (heel - top)
    expected, tolerance = anatomy["expectedHeadFraction"], anatomy["tolerance"]
    return {"passed": abs(ratio - expected) <= tolerance,
            "head_fraction": round(ratio, 5), "expected": expected, "tolerance": tolerance}
