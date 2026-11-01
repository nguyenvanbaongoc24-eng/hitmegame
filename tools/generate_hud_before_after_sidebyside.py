import os
import sys
from PIL import Image, ImageDraw, ImageFont

sys.stdout.reconfigure(encoding='utf-8')

repo_root = r"D:\HITME GAME\unity-client"
r30_dir = os.path.join(repo_root, "Evidence", "round30")
art_dir = r"C:\Users\LENOVO\.gemini\antigravity\brain\15ef4497-c923-4d70-a855-259c984d1ed2"
kit_dir = os.path.join(repo_root, "Assets", "Art", "UI", "Kit")

# 1. Compare Placing Phase N=2: Before vs After (HUD top crop: 1170 x 800)
after_path = os.path.join(r30_dir, "phase_placing_N2_1170x2532.png")
before_path = os.path.join(repo_root, "Evidence", "round28", "phase_placing_N2_1170x2532.png")
if not os.path.exists(before_path):
    before_path = os.path.join(repo_root, "Evidence", "phase3", "v4_charScale_2_placing_1170x2532.png")

if os.path.exists(after_path):
    im_after = Image.open(after_path)
    crop_after = im_after.crop((0, 0, 1170, 750))
    
    crop_before = None
    if os.path.exists(before_path):
        im_before = Image.open(before_path)
        crop_before = im_before.crop((0, 0, 1170, 750))
    
    # Create Side-by-Side Canvas: 2360 x 850
    comparison = Image.new("RGBA", (2380, 860), (15, 23, 42, 255))
    draw = ImageDraw.Draw(comparison)
    
    # Title Header
    draw.rectangle([(0, 0), (2380, 60)], fill=(108, 51, 19, 255))
    draw.text((40, 18), "SO SÁNH GIAO DIỆN TRẬN ĐẤU (HUD): TRƯỚC (ROUND 28) VÀ SAU KHI LÊN ASSET (ROUND 30 U2 - LÔ 1)", fill=(255, 248, 220, 255))
    
    # Paste Before
    if crop_before:
        comparison.paste(crop_before, (10, 90))
        draw.rectangle([(10, 65), (1180, 90)], fill=(40, 40, 40, 255))
        draw.text((20, 68), "TRƯỚC (ROUND 28): Khung đen viền phẳng, chữ xanh neon, nút chữ phẳng, HP bằng text 'HP: 3/3'", fill=(255, 120, 120, 255))
    
    # Paste After
    comparison.paste(crop_after, (1200, 90))
    draw.rectangle([(1200, 65), (2370, 90)], fill=(20, 60, 40, 255))
    draw.text((1210, 68), "SAU (ROUND 30 - LÔ 1): Nền gỗ ấm, đồng xu vàng 5.0s, nút SẴN SÀNG CTA vàng, 3 tim đỏ sprite, thước ngắm Aim Kit", fill=(100, 255, 160, 255))

    out_comp_path = os.path.join(r30_dir, "hud_before_after_comparison.png")
    comparison.save(out_comp_path)
    comparison.save(os.path.join(art_dir, "hud_before_after_comparison.png"))
    print(f"Saved HUD Before/After comparison: {out_comp_path}")

print("HUD comparison generation finished.")
