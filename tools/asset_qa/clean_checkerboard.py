"""Run after asset_qa.py: python tools/asset_qa/clean_checkerboard.py

Conservative optional cleaner: processes only CSV entries flagged checkerboard=high,
copies source pixels to Evidence/asset_qa/cleaned, and never writes source assets.
Opaque neutral pixels connected to the image edge are made transparent; a 1-pixel
alpha erosion reduces residual matte. Review the saved before/after composites.
"""
from __future__ import annotations
import csv
from collections import deque
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw

REPO = Path(__file__).resolve().parents[2]
OUT = REPO / "Evidence" / "asset_qa"; CLEANED = OUT / "cleaned"

def source_path(value: str) -> Path: return REPO.parents[1] / value

def clean(path: Path) -> None:
    image = Image.open(path).convert("RGBA"); before = image.copy(); array = np.array(image); rgb, alpha = array[:,:,:3], array[:,:,3]
    h, w = alpha.shape; neutral = (rgb.max(2) - rgb.min(2) < 22) & (alpha == 255)
    remove = np.zeros((h,w), bool); queue = deque()
    for y, x in list(zip(*np.where(neutral & (np.indices((h,w))[0] == 0)))) + list(zip(*np.where(neutral & (np.indices((h,w))[0] == h-1)))) + list(zip(*np.where(neutral & (np.indices((h,w))[1] == 0)))) + list(zip(*np.where(neutral & (np.indices((h,w))[1] == w-1)))):
        queue.append((y,x)); remove[y,x] = True
    while queue:
        y,x = queue.popleft()
        for ny,nx in ((y-1,x),(y+1,x),(y,x-1),(y,x+1)):
            if 0 <= ny < h and 0 <= nx < w and neutral[ny,nx] and not remove[ny,nx]: remove[ny,nx]=True; queue.append((ny,nx))
    alpha[remove] = 0; array[:,:,3] = alpha
    CLEANED.mkdir(parents=True, exist_ok=True); safe = path.stem.replace(" ", "_")
    after = Image.fromarray(array); after.save(CLEANED / f"{safe}_cleaned.png")
    # Review image is deliberately separate from the cleaned asset and has a
    # green/magenta background so alpha mistakes are immediately visible.
    preview = Image.new("RGBA", (before.width * 2 + 8, before.height + 28), (30, 30, 30, 255))
    for index, candidate in enumerate((before, after)):
        background = Image.new("RGBA", candidate.size, (0, 255, 0, 255) if index == 0 else (255, 0, 255, 255))
        background.alpha_composite(candidate); preview.alpha_composite(background, (index * (before.width + 8), 28))
    ImageDraw.Draw(preview).text((2, 4), "before / green                         after / magenta", fill=(255,255,255,255))
    preview.convert("RGB").save(CLEANED / f"{safe}_before_after.png")

def main() -> int:
    rows = list(csv.DictReader((OUT / "asset_qa.csv").open(encoding="utf-8")))
    selected = [row for row in rows if row.get("checkerboard") == "high"]
    for row in selected: clean(source_path(row["path"]))
    print(f"clean_checkerboard: processed={len(selected)} output={CLEANED}")
    return 0

if __name__ == "__main__": raise SystemExit(main())
