"""Suggest 9-slice borders for a UI PNG; report when its center has detail.

README: python tools/ui/nineslice_suggest.py item.png. The suggestion is a
review aid; inspect the rendered sprite in Unity before accepting it.
"""

from __future__ import annotations

import argparse
import json
from pathlib import Path

import numpy as np
from PIL import Image


def suggest(path: Path) -> dict:
    with Image.open(path) as opened:
        pixels = np.asarray(opened.convert("RGBA"), dtype=np.int16)
    height, width = pixels.shape[:2]
    if min(width, height) < 12:
        raise ValueError("UI element is too small for a reliable 9-slice suggestion")
    rgb = pixels[:, :, :3]
    alpha = pixels[:, :, 3]
    # Find the last transition near the perimeter; the following flat strip
    # is the repeatable edge. A quarter-size cap preserves the center.
    cx0, cx1 = width // 4, max(width // 4 + 1, 3 * width // 4)
    cy0, cy1 = height // 4, max(height // 4 + 1, 3 * height // 4)
    row_delta = np.mean(np.abs(rgb[1:, cx0:cx1] - rgb[:-1, cx0:cx1]), axis=(1, 2))
    col_delta = np.mean(np.abs(rgb[cy0:cy1, 1:] - rgb[cy0:cy1, :-1]), axis=(0, 2))
    def border(deltas: np.ndarray, limit: int) -> int:
        transitions = np.flatnonzero(deltas[:limit] > 8)
        return max(2, min(limit, int(transitions[-1]) + 2)) if len(transitions) else 2
    left = border(col_delta, max(3, width // 3))
    right = left
    top = border(row_delta, max(3, height // 3))
    bottom = top
    # A textured center changes under stretch; compare both spatial axes.
    center = rgb[top:height - bottom, left:width - right]
    center_alpha = alpha[top:height - bottom, left:width - right]
    variation = float(np.mean(np.std(center.astype(np.float32), axis=(0, 1)))) if center.size else 0.0
    local_change = float((np.mean(np.abs(np.diff(center, axis=0))) +
                          np.mean(np.abs(np.diff(center, axis=1)))) / 2) if min(center.shape[:2]) > 1 else 0.0
    patterned = local_change > 9 or (variation > 30 and local_change > 3)
    return {"file": str(path), "width": width, "height": height,
            "border": {"left": left, "right": right, "top": top, "bottom": bottom},
            "center_patterned": bool(patterned), "center_local_change": round(local_change, 2),
            "center_has_alpha": bool(np.any(center_alpha < 255)),
            "warning": "Center has artwork; stretching may distort it" if patterned else None}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("image", type=Path)
    args = parser.parse_args()
    print(json.dumps(suggest(args.image), ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
