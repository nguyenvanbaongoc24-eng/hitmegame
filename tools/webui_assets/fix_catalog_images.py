#!/usr/bin/env python3
"""
fix_catalog_images.py — Clean and rebuild all 17 character and weapon menu assets:
  - 7 character portraits (<id>_portrait.png): clean alpha fringe, crop to tight bbox, export native + 720p optimized PNG.
  - 7 character head avatars (<id>_head.png): crop upper head+collar region from clean full-body sprites (no black circle background, no color-key holes, no clipped hats/ears), export 256x256 and 128x128 transparent PNGs.
  - 3 weapon icons (<id>_icon.png + dep_icon.png): clean transparent icons from Assets/Sprites/dep_icon.png, chao_icon.png, pickleball_icon.png (no baked 'DÉP' text, correct Frying Pan and Pickleball icons).
"""

from __future__ import annotations

import os
from pathlib import Path
import numpy as np
from PIL import Image

REPO_ROOT = Path(__file__).resolve().parents[2]
SPRITES_DIR = REPO_ROOT / "Assets" / "Sprites"
CHARS_DIR = REPO_ROOT / "Assets" / "Art" / "Characters"
CATALOG_DIR = REPO_ROOT / "tools" / "deploy_web" / "catalog"
CODEX_EXTRACT = REPO_ROOT.parent / "_codex_extract"
WEBUI_ASSETS = CODEX_EXTRACT / "web-ui" / "assets"
WEBUI_DIST_ASSETS = CODEX_EXTRACT / "web-ui" / "dist" / "assets"

CHAR_SOURCES = {
    "nam_lang": (SPRITES_DIR / "cau_be_lang_idle.png", 0.62),
    "nu_nang_dong": (SPRITES_DIR / "mai_hong_idle.png", 0.62),
    "co_ba": (SPRITES_DIR / "anh_non_la_idle.png", 0.62),
    "anh_teen": (SPRITES_DIR / "anh_nui_idle.png", 0.62),
    "quay_boy": (SPRITES_DIR / "co_nang_the_thao_idle.png", 0.56),
    "cho_vang": (SPRITES_DIR / "chu_cho_vang_idle.png", 0.68),
    "meo_mun": (SPRITES_DIR / "meo_mun_idle.png", 0.68),
}

WEAPON_SOURCES = {
    "dep_to_ong": SPRITES_DIR / "dep_icon.png",
    "chao_chong_dinh": SPRITES_DIR / "chao_icon.png",
    "vot_pickleball": SPRITES_DIR / "pickleball_icon.png",
}


def clean_alpha_fringe(im: Image.Image) -> Image.Image:
    im = im.convert("RGBA")
    arr = np.array(im, dtype=np.float32)
    r, g, b, a = arr[:, :, 0], arr[:, :, 1], arr[:, :, 2], arr[:, :, 3]
    a[a < 12] = 0
    edge = (a > 0) & (a < 235) & (r > 220) & (g > 220) & (b > 220)
    a[edge] *= 0.35
    a[a < 16] = 0
    arr[:, :, 3] = np.clip(a, 0, 255)
    zero_mask = arr[:, :, 3] == 0
    arr[zero_mask, 0] = 0
    arr[zero_mask, 1] = 0
    arr[zero_mask, 2] = 0
    return Image.fromarray(arr.astype(np.uint8), "RGBA")


def crop_non_empty(im: Image.Image) -> Image.Image:
    bbox = im.getbbox()
    return im.crop(bbox) if bbox else im


def make_square_icon(im: Image.Image, size: int, margin_ratio: float = 0.06) -> Image.Image:
    im = crop_non_empty(im)
    w, h = im.size
    target_inner = int(round(size * (1.0 - 2.0 * margin_ratio)))
    scale = min(target_inner / max(1, w), target_inner / max(1, h))
    nw = max(1, int(round(w * scale)))
    nh = max(1, int(round(h * scale)))
    resized = im.resize((nw, nh), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    canvas.paste(resized, ((size - nw) // 2, (size - nh) // 2), resized)
    return canvas


def make_portrait_720(im: Image.Image) -> Image.Image:
    im = crop_non_empty(im)
    w, h = im.size
    target_h = 720
    scale = target_h / max(1, h)
    nw = max(1, int(round(w * scale)))
    return im.resize((nw, target_h), Image.Resampling.LANCZOS)


def save_optimized_rgba(im: Image.Image, path: Path, quantize: bool = True) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    if quantize:
        q = im.quantize(colors=256, method=Image.Quantize.FASTOCTREE, dither=Image.Dither.NONE)
        q.save(path, optimize=True)
    else:
        im.save(path, optimize=True)


def main() -> None:
    CATALOG_DIR.mkdir(parents=True, exist_ok=True)
    for cid, (src_path, head_ratio) in CHAR_SOURCES.items():
        raw = Image.open(src_path)
        cleaned = crop_non_empty(clean_alpha_fringe(raw))
        w, h = cleaned.size

        menu_dir = CHARS_DIR / cid / "Menu"
        menu_dir.mkdir(parents=True, exist_ok=True)
        save_optimized_rgba(cleaned, menu_dir / f"menu_{cid}_portrait.png", quantize=False)

        p720 = make_portrait_720(cleaned)
        save_optimized_rgba(p720, CATALOG_DIR / f"{cid}_portrait.png", quantize=True)
        if WEBUI_ASSETS.exists():
            save_optimized_rgba(p720, WEBUI_ASSETS / f"{cid}_portrait.png", quantize=True)
        if WEBUI_DIST_ASSETS.exists():
            save_optimized_rgba(p720, WEBUI_DIST_ASSETS / f"{cid}_portrait.png", quantize=True)

        head_crop_h = max(1, int(round(h * head_ratio)))
        head_crop = cleaned.crop((0, 0, w, head_crop_h))
        head_256 = make_square_icon(head_crop, 256, margin_ratio=0.06)
        head_128 = make_square_icon(head_crop, 128, margin_ratio=0.06)

        save_optimized_rgba(head_256, menu_dir / f"menu_{cid}_head.png", quantize=False)
        save_optimized_rgba(head_128, CATALOG_DIR / f"{cid}_head.png", quantize=True)
        if WEBUI_ASSETS.exists():
            save_optimized_rgba(head_128, WEBUI_ASSETS / f"{cid}_head.png", quantize=True)
        if WEBUI_DIST_ASSETS.exists():
            save_optimized_rgba(head_128, WEBUI_DIST_ASSETS / f"{cid}_head.png", quantize=True)

        p_size = (CATALOG_DIR / f"{cid}_portrait.png").stat().st_size
        h_size = (CATALOG_DIR / f"{cid}_head.png").stat().st_size
        print(f"[CHAR] {cid:14s} portrait={p_size:6d} B, head={h_size:5d} B")

    for wid, src_path in WEAPON_SOURCES.items():
        raw = Image.open(src_path)
        cleaned = crop_non_empty(clean_alpha_fringe(raw))
        icon_256 = make_square_icon(cleaned, 256, margin_ratio=0.08)
        icon_128 = make_square_icon(cleaned, 128, margin_ratio=0.08)

        save_optimized_rgba(icon_128, CATALOG_DIR / f"{wid}_icon.png", quantize=True)
        if WEBUI_ASSETS.exists():
            save_optimized_rgba(icon_128, WEBUI_ASSETS / f"{wid}_icon.png", quantize=True)
        if WEBUI_DIST_ASSETS.exists():
            save_optimized_rgba(icon_128, WEBUI_DIST_ASSETS / f"{wid}_icon.png", quantize=True)

        if wid == "dep_to_ong":
            save_optimized_rgba(icon_128, CATALOG_DIR / "dep_icon.png", quantize=True)
            if WEBUI_ASSETS.exists():
                save_optimized_rgba(icon_128, WEBUI_ASSETS / "dep_icon.png", quantize=True)
            if WEBUI_DIST_ASSETS.exists():
                save_optimized_rgba(icon_128, WEBUI_DIST_ASSETS / "dep_icon.png", quantize=True)

        i_size = (CATALOG_DIR / f"{wid}_icon.png").stat().st_size
        print(f"[WEAP] {wid:16s} icon={i_size:5d} B")


if __name__ == "__main__":
    main()
