"""Check UI dump geometry; all coordinates are pixels in the same canvas."""

import json
from itertools import combinations
from pathlib import Path


def area(rect):
    return max(0, rect[2]) * max(0, rect[3])


def overlap(a, b):
    w = max(0, min(a[0] + a[2], b[0] + b[2]) - max(a[0], b[0]))
    h = max(0, min(a[1] + a[3], b[1] + b[3]) - max(a[1], b[1]))
    return w * h


def contains(outer, inner):
    return inner[0] >= outer[0] and inner[1] >= outer[1] and \
        inner[0] + inner[2] <= outer[0] + outer[2] and \
        inner[1] + inner[3] <= outer[1] + outer[3]


def check(dump):
    safe = dump["safeArea"]
    elements = dump["elements"]
    failures = []
    minimum_touch = float(dump.get("minTouchCssPx", 44)) * float(dump.get("pixelsPerCssPixel", 1))
    for element in elements:
        name, rect = element["name"], element["rect"]
        if area(rect) <= 0:
            failures.append(f"{name}: empty_rect")
        if not contains(safe, rect):
            failures.append(f"{name}: outside_safe_area")
        if element.get("clickable") and (rect[2] < minimum_touch or rect[3] < minimum_touch):
            failures.append(f"{name}: touch_target_below_{minimum_touch:g}px")
        if element.get("text") and not contains(rect, element.get("textRect", rect)):
            failures.append(f"{name}: text_outside_rect")
    clickable = [e for e in elements if e.get("clickable")]
    threshold = float(dump.get("maxClickableOverlapFraction", 0.05))
    for a, b in combinations(clickable, 2):
        divisor = min(area(a["rect"]), area(b["rect"]))
        if divisor and overlap(a["rect"], b["rect"]) / divisor > threshold:
            failures.append(f"{a['name']}+{b['name']}: clickable_overlap")
    return {"passed": not failures, "failures": failures, "checked": len(elements)}


def check_file(path):
    return check(json.loads(Path(path).read_text(encoding="utf-8")))
