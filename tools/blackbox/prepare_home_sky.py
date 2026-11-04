"""Extract a sky/foliage strip from the approved Home mockup, omitting phone chrome.

Usage: python tools/blackbox/prepare_home_sky.py SOURCE OUTPUT
The mockup is read-only; the derivative is a temporary web asset for Home.
"""
from pathlib import Path
import argparse
import numpy as np
from PIL import Image


def create(source: Path, destination: Path) -> None:
    original = Image.open(source).convert("RGB")
    # Side foliage in the reference does not contain the Dynamic Island or HUD.
    left = np.asarray(original.crop((95, 75, 275, 145)).resize((180, 300)), dtype=np.float32)
    right = np.asarray(original.crop((578, 75, 758, 145)).resize((180, 300)), dtype=np.float32)
    out = np.empty((300, 757, 3), dtype=np.float32)
    sky_top = np.array(original.getpixel((440, 49)), dtype=float)
    sky_bottom = np.array(original.getpixel((440, 149)), dtype=float)
    for y in range(300):
        out[y, :, :] = sky_top * (1-y/299) + sky_bottom * y/299
    for x in range(180):
        alpha = 1 if x < 143 else max(0, (180-x)/37)
        out[:, x, :] = left[:, x, :] * alpha + out[:, x, :] * (1-alpha)
        right_alpha = min(1, x / 37)
        out[:, 577+x, :] = right[:, x, :] * right_alpha + out[:, 577+x, :] * (1-right_alpha)
    destination.parent.mkdir(parents=True, exist_ok=True)
    Image.fromarray(np.uint8(np.clip(out, 0, 255))).save(destination)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('source', type=Path)
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    create(args.source, args.output)
