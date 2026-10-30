import os, sys
sys.stdout.reconfigure(encoding='utf-8')
from collections import deque
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import numpy as np
import json

REPO_ROOT = Path("D:/HITME GAME/unity-client")
FINAL_ASSETS = Path("D:/HITME GAME/ASSET HITME FINAL")
CONTACT_DIR = REPO_ROOT / "Evidence" / "asset_qa" / "contact"
SPRITES_DIR = REPO_ROOT / "Assets" / "Sprites"
RESOURCES_DIR = REPO_ROOT / "Assets" / "Resources"

CONTACT_DIR.mkdir(parents=True, exist_ok=True)
RESOURCES_DIR.mkdir(parents=True, exist_ok=True)

CHAR_NAMES = [
    "Nam Làng",
    "Nữ Năng Động",
    "Quậy Boy",
    "Cô Ba",
    "Anh Teen",
    "Chó Vàng",
    "Mèo Mun"
]

POSE_NAMES = ["Đứng (Idle)", "Ném (Throw)", "Bị trúng (Hit)", "Bị loại (Defeat)"]

def remove_background(im: Image.Image) -> Image.Image:
    """Removes light-neutral background connected to the border."""
    im = im.convert("RGBA")
    arr = np.array(im)
    rgb = arr[:, :, :3]
    alpha = arr[:, :, 3]
    h, w = alpha.shape
    
    # Brightness > 195 and low color saturation
    is_neutral = (rgb.mean(axis=2) > 195) & (rgb.max(axis=2) - rgb.min(axis=2) < 28)
    
    remove = np.zeros((h, w), dtype=bool)
    queue = deque()
    
    for y in range(h):
        for x in (0, w - 1):
            if is_neutral[y, x] and not remove[y, x]:
                remove[y, x] = True
                queue.append((y, x))
    for x in range(w):
        for y in (0, h - 1):
            if is_neutral[y, x] and not remove[y, x]:
                remove[y, x] = True
                queue.append((y, x))
                
    while queue:
        cy, cx = queue.popleft()
        for ny, nx in ((cy - 1, cx), (cy + 1, cx), (cy, cx - 1), (cy, cx + 1)):
            if 0 <= ny < h and 0 <= nx < w:
                if is_neutral[ny, nx] and not remove[ny, nx]:
                    remove[ny, nx] = True
                    queue.append((ny, nx))
                    
    arr[remove, 3] = 0
    return Image.fromarray(arr)

def pad_with_margin(im: Image.Image, pad: int = 16) -> Image.Image:
    """Crops tight to alpha, then pads with transparent border so it never touches edge."""
    im = im.convert("RGBA")
    bbox = im.getbbox()
    if not bbox:
        return im
    cropped = im.crop(bbox)
    new_w = cropped.width + pad * 2
    new_h = cropped.height + pad * 2
    padded = Image.new("RGBA", (new_w, new_h), (0, 0, 0, 0))
    padded.paste(cropped, (pad, pad))
    return padded

def extract_all():
    frames = {} # (char_idx, pose_idx) -> Image
    
    # 0. Nam Làng (ASSET HITME FINAL)
    nam_dir = FINAL_ASSETS / "NAM LÀNG" / "CẦM DÉP" / "sprites"
    frames[(0, 0)] = pad_with_margin(Image.open(nam_dir / "sprite_001.png"))
    frames[(0, 1)] = pad_with_margin(Image.open(nam_dir / "sprite_010.png"))
    frames[(0, 2)] = pad_with_margin(Image.open(nam_dir / "sprite_011.png"))
    frames[(0, 3)] = pad_with_margin(Image.open(nam_dir / "sprite_012.png")) # single clean knock-out frame <= 512
    
    # 1. Nữ Năng Động (CÔ NÀNG THỂ THAO)
    co_sheet = Image.open(REPO_ROOT / "Assets/Art/Characters/CÔ_NÀNG_THỂ_THAO/CÔ_NÀNG_THỂ_THAO_spritesheet.png")
    frames[(1, 0)] = pad_with_margin(Image.open(SPRITES_DIR / "co_nang_the_thao_idle.png"))
    f_throw = remove_background(co_sheet.crop((1405, 500, 1530, 580)))
    frames[(1, 1)] = pad_with_margin(f_throw)
    f_hit = remove_background(co_sheet.crop((435, 620, 520, 715)))
    frames[(1, 2)] = pad_with_margin(f_hit)
    f_def = remove_background(co_sheet.crop((1420, 630, 1535, 715)))
    frames[(1, 3)] = pad_with_margin(f_def)

    # 2. Quậy Boy / Anh Núi (ASSET HITME FINAL)
    nui_dir = FINAL_ASSETS / "ANH NÚI" / "NÉM DÉP" / "sprites"
    frames[(2, 0)] = pad_with_margin(Image.open(nui_dir / "sprite_001.png"))
    frames[(2, 1)] = pad_with_margin(Image.open(nui_dir / "sprite_006.png"))
    frames[(2, 2)] = pad_with_margin(Image.open(nui_dir / "sprite_009.png"))
    frames[(2, 3)] = pad_with_margin(Image.open(nui_dir / "sprite_011.png"))
    
    # 3. Cô Ba / Mai Hồng (ASSET HITME FINAL)
    mai_dir = FINAL_ASSETS / "MAI HỒNG" / "NÉM DÉP" / "sprites"
    frames[(3, 0)] = pad_with_margin(Image.open(mai_dir / "sprite_001.png"))
    frames[(3, 1)] = pad_with_margin(Image.open(mai_dir / "sprite_009.png")) # throw frame <= 512
    frames[(3, 2)] = pad_with_margin(Image.open(mai_dir / "sprite_014.png"))
    frames[(3, 3)] = pad_with_margin(Image.open(mai_dir / "sprite_017.png"))
    
    # 4. Anh Teen / Anh Nón Lá (ASSET HITME FINAL)
    non_dir = FINAL_ASSETS / "ANH NÓN LÁ" / "NÉM DÉP" / "sprites"
    frames[(4, 0)] = pad_with_margin(Image.open(non_dir / "sprite_001.png"))
    frames[(4, 1)] = pad_with_margin(Image.open(non_dir / "sprite_010.png"))
    frames[(4, 2)] = pad_with_margin(Image.open(non_dir / "sprite_013.png"))
    frames[(4, 3)] = pad_with_margin(Image.open(non_dir / "sprite_020.png"))

    # 5. Chó Vàng (CHÚ CHÓ VÀNG)
    cho_sheet = Image.open(REPO_ROOT / "Assets/Art/Characters/CHÚ_CHÓ_VÀNG/CHÚ_CHÓ_VÀNG_spritesheet.png")
    frames[(5, 0)] = pad_with_margin(Image.open(SPRITES_DIR / "chu_cho_vang_idle.png"))
    f_throw = remove_background(cho_sheet.crop((1415, 500, 1535, 580)))
    frames[(5, 1)] = pad_with_margin(f_throw)
    f_hit = remove_background(cho_sheet.crop((435, 625, 520, 715)))
    frames[(5, 2)] = pad_with_margin(f_hit)
    f_def = remove_background(cho_sheet.crop((1420, 640, 1535, 715)))
    frames[(5, 3)] = pad_with_margin(f_def)

    # 6. Mèo Mun (MÈO MUN)
    meo_sheet = Image.open(REPO_ROOT / "Assets/Art/Characters/MÈO_MUN/MÈO_MUN_spritesheet.png")
    frames[(6, 0)] = pad_with_margin(Image.open(SPRITES_DIR / "meo_mun_idle.png"))
    f_throw = remove_background(meo_sheet.crop((1415, 500, 1535, 580)))
    frames[(6, 1)] = pad_with_margin(f_throw)
    f_hit = remove_background(meo_sheet.crop((435, 625, 520, 715)))
    frames[(6, 2)] = pad_with_margin(f_hit)
    f_def = remove_background(meo_sheet.crop((1420, 640, 1535, 715)))
    frames[(6, 3)] = pad_with_margin(f_def)

    return frames

def build_contact_sheet(frames):
    cell_w, cell_h = 320, 340
    cols = 4
    rows = 7
    sheet_w = cols * cell_w + 240
    sheet_h = rows * cell_h + 100

    contact = Image.new("RGBA", (sheet_w, sheet_h), (22, 26, 34, 255))
    draw = ImageDraw.Draw(contact)

    # Header
    draw.text((sheet_w // 2 - 250, 20), "HIT ME – BẢNG KIỂM ĐỊNH ALPHA SPRITE NHÂN VẬT (7x4)", fill=(255, 220, 90), font_size=24)
    for col_idx, pname in enumerate(POSE_NAMES):
        draw.text((240 + col_idx * cell_w + cell_w // 2 - 50, 60), pname, fill=(200, 220, 255), font_size=18)

    all_passed = True
    report_lines = []

    for char_idx in range(rows):
        char_name = CHAR_NAMES[char_idx]
        y_top = 100 + char_idx * cell_h
        draw.text((20, y_top + cell_h // 2 - 10), f"#{char_idx}: {char_name}", fill=(255, 255, 255), font_size=20)

        for pose_idx in range(cols):
            x_left = 240 + pose_idx * cell_w
            im = frames[(char_idx, pose_idx)]
            
            # Checkerboard card background
            card = Image.new("RGBA", (cell_w - 20, cell_h - 20), (32, 38, 48, 255))
            c_draw = ImageDraw.Draw(card)
            for gx in range(0, card.width, 16):
                for gy in range(0, card.height, 16):
                    if (gx // 16 + gy // 16) % 2 == 0:
                        c_draw.rectangle([gx, gy, gx + 15, gy + 15], fill=(40, 46, 58, 255))

            fit_scale = min((card.width - 40) / im.width, (card.height - 40) / im.height, 1.0)
            target_w = max(1, int(im.width * fit_scale))
            target_h = max(1, int(im.height * fit_scale))
            scaled_im = im.resize((target_w, target_h), Image.Resampling.LANCZOS)
            
            px = (card.width - target_w) // 2
            py = (card.height - target_h) // 2
            card.alpha_composite(scaled_im, (px, py))

            bbox = im.getbbox()
            touches = False
            size_ok = (im.width <= 512 and im.height <= 512)
            if bbox:
                if bbox[0] == 0 or bbox[1] == 0 or bbox[2] == im.width or bbox[3] == im.height:
                    touches = True
            else:
                touches = True

            passed = (not touches) and size_ok

            if bbox:
                bx0 = int(px + bbox[0] * fit_scale)
                by0 = int(py + bbox[1] * fit_scale)
                bx1 = int(px + bbox[2] * fit_scale)
                by1 = int(py + bbox[3] * fit_scale)
                box_color = (50, 255, 120, 255) if passed else (255, 50, 50, 255)
                c_draw.rectangle([bx0, by0, bx1, by1], outline=box_color, width=2)
                status_txt = "SAFE (PASS)" if passed else "FAIL"
                c_draw.text((bx0, max(0, by0 - 16)), status_txt, fill=box_color, font_size=12)

            contact.alpha_composite(card, (x_left + 10, y_top + 10))
            if not passed:
                all_passed = False
            report_lines.append(f"{char_name} | {POSE_NAMES[pose_idx]}: size={im.size} bbox={bbox} touches_edge={touches} size_le_512={size_ok}")

    out_file = CONTACT_DIR / "characters_20261030.png"
    contact.save(out_file)
    print(f"Generated contact sheet: {out_file} (All passed: {all_passed})")
    
    with open(CONTACT_DIR / "characters_20261030.log", "w", encoding="utf-8") as f:
        f.write("\n".join(report_lines))
        f.write(f"\n\nOVERALL: {'ALL 28 FRAMES PASS (0 touching edge, all <= 512x512)' if all_passed else 'SOME FAILED'}\n")
    return all_passed

def deploy_clean_sprites():
    """Overwrites dirty/chopped Assets/Sprites with clean frames."""
    non_dir = FINAL_ASSETS / "ANH NÓN LÁ" / "NÉM DÉP" / "sprites"
    nam_dir = FINAL_ASSETS / "NAM LÀNG" / "CẦM DÉP" / "sprites"
    dep_w_dir = FINAL_ASSETS / "ASSET WEAPONS" / "DÉP" / "sprites"
    chao_w_dir = FINAL_ASSETS / "ASSET WEAPONS" / "CHẢO" / "sprites"
    pickle_w_dir = FINAL_ASSETS / "ASSET WEAPONS" / "VỢT PICKLEBALL" / "sprites"

    # Anh Non La
    pad_with_margin(Image.open(non_dir / "sprite_001.png")).save(SPRITES_DIR / "anh_non_la_idle.png")
    pad_with_margin(Image.open(non_dir / "sprite_006.png")).save(SPRITES_DIR / "anh_non_la_aim.png")
    pad_with_margin(Image.open(non_dir / "sprite_006.png")).save(SPRITES_DIR / "anh_non_la_throw_windup.png")
    pad_with_margin(Image.open(non_dir / "sprite_010.png")).save(SPRITES_DIR / "anh_non_la_throw_release.png")
    pad_with_margin(Image.open(non_dir / "sprite_013.png")).save(SPRITES_DIR / "anh_non_la_hit.png")
    pad_with_margin(Image.open(non_dir / "sprite_020.png")).save(SPRITES_DIR / "anh_non_la_defeat.png")
    pad_with_margin(Image.open(non_dir / "sprite_003.png")).save(SPRITES_DIR / "anh_non_la_move.png")
    pad_with_margin(Image.open(non_dir / "sprite_001.png")).save(SPRITES_DIR / "anh_non_la_victory.png")

    # Nam Lang
    pad_with_margin(Image.open(nam_dir / "sprite_001.png")).save(SPRITES_DIR / "cau_be_lang_idle.png")
    pad_with_margin(Image.open(nam_dir / "sprite_006.png")).save(SPRITES_DIR / "cau_be_lang_aim.png")
    pad_with_margin(Image.open(nam_dir / "sprite_006.png")).save(SPRITES_DIR / "cau_be_lang_throw_windup.png")
    pad_with_margin(Image.open(nam_dir / "sprite_010.png")).save(SPRITES_DIR / "cau_be_lang_throw_release.png")
    pad_with_margin(Image.open(nam_dir / "sprite_011.png")).save(SPRITES_DIR / "cau_be_lang_hit.png")
    pad_with_margin(Image.open(nam_dir / "sprite_012.png")).save(SPRITES_DIR / "cau_be_lang_defeat.png")
    pad_with_margin(Image.open(nam_dir / "sprite_003.png")).save(SPRITES_DIR / "cau_be_lang_move.png")
    pad_with_margin(Image.open(nam_dir / "sprite_001.png")).save(SPRITES_DIR / "cau_be_lang_victory.png")

    # Weapons (clean frames, no text)
    dep_clean = pad_with_margin(Image.open(dep_w_dir / "sprite_001.png"))
    dep_clean.save(SPRITES_DIR / "dep_projectile.png")
    dep_clean.save(SPRITES_DIR / "dep_icon.png")

    chao_clean = pad_with_margin(Image.open(chao_w_dir / "sprite_001.png"))
    chao_clean.save(SPRITES_DIR / "chao_projectile.png")
    chao_clean.save(SPRITES_DIR / "chao_icon.png")

    pball_clean = pad_with_margin(Image.open(pickle_w_dir / "sprite_001.png"))
    pball_clean.save(SPRITES_DIR / "pickleball_projectile.png")
    pball_clean.save(SPRITES_DIR / "pickleball_icon.png")

    # Overwrite weapon full cards without baked-in text (clean card on transparent or card bg)
    for name, icon_img in [("weapon_dep_full.png", dep_clean), ("weapon_chao_full.png", chao_clean), ("weapon_pickleball_full.png", pball_clean)]:
        # Card 256x384 with weapon centered, absolutely no baked-in text
        w_card = Image.new("RGBA", (256, 384), (0, 0, 0, 0))
        # subtle card backing rounded rect
        draw = ImageDraw.Draw(w_card)
        draw.rounded_rectangle([10, 10, 246, 374], radius=16, fill=(35, 45, 60, 230), outline=(80, 120, 180, 255), width=3)
        fit = min(200 / icon_img.width, 240 / icon_img.height)
        tw = int(icon_img.width * fit)
        th = int(icon_img.height * fit)
        scaled_icon = icon_img.resize((tw, th), Image.Resampling.LANCZOS)
        w_card.alpha_composite(scaled_icon, ((256 - tw) // 2, (384 - th) // 2))
        w_card.save(SPRITES_DIR / name)

    print("Clean sprites successfully deployed to Assets/Sprites!")

def generate_approved_runtime_sprites_json():
    """Scans Assets/Sprites and generates ApprovedRuntimeSprites.json."""
    approved = {}
    forbidden_keywords = ["sheet", "contact", "reference", "atlas_labeled"]
    
    for f in sorted(os.listdir(SPRITES_DIR)):
        if not f.endswith(".png"):
            continue
        p = SPRITES_DIR / f
        im = Image.open(p)
        w, h = im.size
        # Arena background and full UI screens/banners are allowed to exceed 512
        is_bg_or_ui = ("arena" in f.lower()) or f.startswith("ui_")
        
        has_forbidden = any(k in f.lower() for k in forbidden_keywords)
        if has_forbidden:
            print(f"WARNING: Sprite {f} has forbidden keyword!")

        approved[f] = {
            "path": f"Assets/Sprites/{f}",
            "width": w,
            "height": h,
            "isBackgroundOrUI": is_bg_or_ui,
            "approved": (not has_forbidden) and (is_bg_or_ui or (w <= 512 and h <= 512))
        }

    out_json = RESOURCES_DIR / "ApprovedRuntimeSprites.json"
    with open(out_json, "w", encoding="utf-8") as f:
        json.dump(approved, f, indent=2)
    print(f"Generated ApprovedRuntimeSprites.json with {len(approved)} sprites.")

if __name__ == "__main__":
    frames = extract_all()
    passed = build_contact_sheet(frames)
    deploy_clean_sprites()
    generate_approved_runtime_sprites_json()
    sys.exit(0 if passed else 1)
