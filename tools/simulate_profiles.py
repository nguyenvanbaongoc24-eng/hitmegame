import os
import json
from PIL import Image, ImageDraw

bg_path = r"D:\HITME GAME\asset\UI_KIT_RAW\u4_background.png"
bg = Image.open(bg_path)
bw, bh = bg.size

profiles = [
    (393, 695, "iphone15pro_safari_393x695"),
    (393, 780, "iphone15pro_safari_collapsed_393x780"),
    (393, 852, "iphone15pro_standalone_393x852"),
    (375, 553, "iphonese_safari_375x553"),
    (360, 640, "android_chrome_360x640"),
    (412, 780, "android_chrome_tall_412x780")
]

out_dir = r"D:\HITME GAME\unity-client\Evidence\home_bg_audit\simulated_profiles"
os.makedirs(out_dir, exist_ok=True)

# Focal anchor on original image (fx, fy)
fx_norm = 0.500
fy_norm = 0.485

# Pedestal ellipse in original image
cx_norm = 0.500
cy_norm = 0.474
rx_norm = 0.361
ry_norm = 0.073

# object-position: 50% 48%
pos_x = 0.50
pos_y = 0.48

results = []

for vw, vh, name in profiles:
    # Scale calculation for 'cover'
    scale = max(vw / bw, vh / bh)
    rw = int(round(bw * scale))
    rh = int(round(bh * scale))
    
    # Offsets based on object-position
    dx = int(round((vw - rw) * pos_x))
    dy = int(round((vh - rh) * pos_y))
    
    # Resize background with high quality
    resized_bg = bg.resize((rw, rh), Image.Resampling.LANCZOS)
    
    # Crop to viewport
    viewport_img = Image.new("RGB", (vw, vh))
    sub = resized_bg.crop((max(0, -dx), max(0, -dy), min(rw, -dx + vw), min(rh, -dy + vh)))
    viewport_img.paste(sub, (max(0, dx), max(0, dy)))
    
    # Calculate screen coordinates of anchor and pedestal
    screen_fx = dx + fx_norm * rw
    screen_fy = dy + fy_norm * rh
    screen_cx = dx + cx_norm * rw
    screen_cy = dy + cy_norm * rh
    screen_rx = rx_norm * rw
    screen_ry = ry_norm * rh
    
    draw = ImageDraw.Draw(viewport_img)
    
    # Draw pedestal ellipse
    draw.ellipse([screen_cx - screen_rx, screen_cy - screen_ry, screen_cx + screen_rx, screen_cy + screen_ry], outline="#38bdf8", width=2)
    # Draw feet anchor
    draw.ellipse([screen_fx - 5, screen_fy - 5, screen_fx + 5, screen_fy + 5], fill="#ef4444", outline="white", width=1)
    
    # Draw character box: standing on feet anchor
    char_h = int(min(vh * 0.35, 230))
    char_w = int(char_h * 0.55)
    char_x1 = int(screen_fx - char_w / 2)
    char_y1 = int(screen_fy - char_h)
    char_x2 = char_x1 + char_w
    char_y2 = int(screen_fy)
    draw.rectangle([char_x1, char_y1, char_x2, char_y2], outline="#fbbf24", width=2)
    draw.text((char_x1 + 4, char_y1 + 4), "Hero", fill="#fbbf24")
    
    # Overlay UI Safe Areas:
    top_bar_h = 50
    draw.rectangle([8, 8, vw - 8, top_bar_h], outline="#ec4899", width=1)
    draw.text((12, 12), f"Top Bar ({vw}x{top_bar_h})", fill="#ec4899")
    
    logo_y1 = top_bar_h + 8
    logo_y2 = max(logo_y1 + 40, char_y1 - 6)
    draw.rectangle([int(vw * 0.2), logo_y1, int(vw * 0.8), logo_y2], outline="#f97316", width=1)
    draw.text((int(vw * 0.2) + 4, logo_y1 + 4), "HIT ME Logo", fill="#f97316")
    
    card_y1 = int(screen_fy + 10)
    card_y2 = min(card_y1 + 50, vh - 170)
    draw.rectangle([int(vw * 0.05), card_y1, int(vw * 0.47), card_y2], outline="#3b82f6", width=1)
    draw.rectangle([int(vw * 0.53), card_y1, int(vw * 0.95), card_y2], outline="#3b82f6", width=1)
    draw.text((int(vw * 0.06), card_y1 + 4), "Char Card", fill="#3b82f6")
    draw.text((int(vw * 0.54), card_y1 + 4), "Weapon Card", fill="#3b82f6")
    
    btn_y1 = card_y2 + 8
    btn_y2 = vh - 60
    draw.rectangle([int(vw * 0.08), btn_y1, int(vw * 0.92), btn_y2], outline="#22c55e", width=1)
    draw.text((int(vw * 0.10), btn_y1 + 4), "CTA & Action Buttons", fill="#22c55e")
    
    draw.rectangle([0, vh - 52, vw, vh], outline="#a855f7", width=1)
    draw.text((8, vh - 45), "Bottom Tab Bar (52px)", fill="#a855f7")
    
    hero_no_overlap_top = (char_y1 >= top_bar_h)
    hero_no_overlap_tab = (char_y2 <= (vh - 52))
    
    out_file = os.path.join(out_dir, f"{name}.png")
    viewport_img.save(out_file)
    
    results.append({
        "profile": name,
        "size": f"{vw}x{vh}",
        "scale": round(scale, 4),
        "pedestal_y": f"{screen_fy:.1f}px ({screen_fy/vh*100:.1f}%)",
        "hero_y_range": f"{char_y1}..{char_y2}px",
        "hero_safe": hero_no_overlap_top and hero_no_overlap_tab,
        "saved": out_file
    })

print("=== SIMULATION RESULTS ACROSS 6 PROFILES ===")
for r in results:
    print(f"{r['profile']:<38} | Pedestal: {r['pedestal_y']:<15} | Hero safe: {r['hero_safe']}")

with open(os.path.join(out_dir, "simulation_results.json"), "w", encoding="utf-8") as f:
    json.dump(results, f, indent=2)
