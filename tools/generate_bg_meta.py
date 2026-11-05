import json
import os
from PIL import Image, ImageFilter
import numpy as np

img_path = r"D:\HITME GAME\asset\UI_KIT_RAW\u4_background.png"
img = Image.open(img_path)
w, h = img.size
gray = img.convert("L")
arr_gray = np.array(gray)
edges = np.array(gray.filter(ImageFilter.FIND_EDGES))

# Pedestal geometry (normalized 0..1)
cx = 470.5 / w
cy = 792.5 / h
rx = 340.0 / w
ry = 122.5 / h
fx = 0.500
fy = 810.0 / h

# Safe Areas in normalized rects [x, y, w, h]
safe_areas = {
    "top_bar": [0.02, 0.05, 0.96, 0.07],
    "logo_area": [0.15, 0.12, 0.70, 0.13],
    "hero_stand": [0.22, 0.26, 0.56, 0.23],
    "cards_row": [0.05, 0.56, 0.90, 0.09],
    "character_card": [0.05, 0.56, 0.43, 0.08],
    "weapon_card": [0.52, 0.56, 0.43, 0.08],
    "quick_play_cta": [0.07, 0.66, 0.86, 0.08],
    "action_buttons_row": [0.05, 0.75, 0.90, 0.07],
    "create_room_btn": [0.05, 0.75, 0.43, 0.07],
    "join_room_btn": [0.52, 0.75, 0.43, 0.07],
    "practice_btn": [0.12, 0.83, 0.76, 0.06],
    "bottom_tabs": [0.00, 0.91, 1.00, 0.09]
}

# Photometric metrics (Luminance & Clutter/Edge mean)
regional_photometrics = {}
for name, (rx1, ry1, rw1, rh1) in safe_areas.items():
    px1 = int(rx1 * w)
    py1 = int(ry1 * h)
    px2 = int((rx1 + rw1) * w)
    py2 = int((ry1 + rh1) * h)
    sub_gray = arr_gray[py1:py2, px1:px2]
    sub_edge = edges[py1:py2, px1:px2]
    regional_photometrics[name] = {
        "luminance_mean": round(float(sub_gray.mean()), 1),
        "luminance_std": round(float(sub_gray.std()), 1),
        "clutter_edge_mean": round(float(sub_edge.mean()), 1)
    }

meta = {
    "source_file": "u4_background.png",
    "original_resolution": {
        "width": w,
        "height": h,
        "aspect_ratio": round(w / h, 4),
        "color_mode": img.mode,
        "has_alpha": False
    },
    "pedestal": {
        "center": {"cx": round(cx, 4), "cy": round(cy, 4)},
        "radii": {"rx": round(rx, 4), "ry": round(ry, 4)},
        "anchor_feet": {"fx": round(fx, 4), "fy": round(fy, 4)},
        "description": "Oval carved lotus stone pedestal. Character feet contact point at (fx, fy)."
    },
    "focal_point": {
        "x": 0.500,
        "y": 0.485,
        "css_object_position": "50% 48%",
        "formula": {
            "scale": "max(viewport_w / image_w, viewport_h / image_h)",
            "dx": "(viewport_w - image_w * scale) * 0.50",
            "dy": "(viewport_h - image_h * scale) * 0.48",
            "screen_feet_x": "dx + fx * image_w * scale",
            "screen_feet_y": "dy + fy * image_h * scale"
        }
    },
    "safe_areas": safe_areas,
    "regional_photometrics": regional_photometrics,
    "recommended_darkening_overlays": {
        "top_gradient": "linear-gradient(180deg, rgba(0, 0, 0, 0.45) 0%, rgba(0, 0, 0, 0) 15%)",
        "bottom_vignette": "linear-gradient(180deg, rgba(0, 0, 0, 0) 55%, rgba(0, 0, 0, 0.25) 85%, rgba(0, 0, 0, 0.60) 100%)",
        "tab_bar_backdrop": "rgba(15, 23, 42, 0.75)"
    },
    "responsive_assets": {
        "lqip": "bg_home_lqip.webp (32x56 px, ~226 bytes)",
        "w750": "bg_home_750.webp (750x1332 px, ~170 KB, budget <=180 KB)",
        "w1170": "bg_home_1170.webp (1170x2078 px, ~294 KB, budget <=350 KB)",
        "w1536": "bg_home_1536.webp (1536x2729 px, ~500 KB, high-DPI desktop/tablet)"
    }
}

target_dirs = [
    r"D:\HITME GAME\unity-client\tools\deploy_web\web_assets\bg_home",
    r"D:\HITME GAME\asset\UI_KIT_RAW",
    r"D:\HITME GAME\unity-client\tools\deploy_web\dist\ui-preview\assets",
    r"D:\HITME GAME\unity-client\tools\deploy_web\dist\next\assets"
]

for d in target_dirs:
    os.makedirs(d, exist_ok=True)
    out_file = os.path.join(d, "bg_home_meta.json")
    with open(out_file, "w", encoding="utf-8") as f:
        json.dump(meta, f, indent=2, ensure_ascii=False)
    print(f"Saved {out_file}")
