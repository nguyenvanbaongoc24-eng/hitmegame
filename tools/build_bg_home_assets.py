import os
import shutil
from PIL import Image, ImageFilter

src_path = r"D:\HITME GAME\asset\UI_KIT_RAW\u4_background.png"
out_dir = r"D:\HITME GAME\unity-client\tools\deploy_web\web_assets\bg_home"
os.makedirs(out_dir, exist_ok=True)

img = Image.open(src_path)
orig_w, orig_h = img.size
aspect = orig_h / orig_w

configs = [
    (750, "bg_home_750.webp", 76),    # tuned to be <= 180 KB
    (1170, "bg_home_1170.webp", 80),  # tuned to be <= 350 KB
    (1536, "bg_home_1536.webp", 80)
]

generated = []
for w, fname, q in configs:
    h = int(w * aspect)
    res = img.resize((w, h), Image.Resampling.LANCZOS)
    dst = os.path.join(out_dir, fname)
    res.save(dst, "WEBP", quality=q, method=6)
    sz_kb = os.path.getsize(dst) / 1024
    print(f"{fname}: {w}x{h} px, quality={q} -> {sz_kb:.1f} KB")
    generated.append((fname, sz_kb))

# LQIP: 32px width
lqip_w = 32
lqip_h = int(lqip_w * aspect)
lqip = img.resize((lqip_w, lqip_h), Image.Resampling.BOX)
lqip = lqip.filter(ImageFilter.GaussianBlur(radius=1.5))
lqip_dst = os.path.join(out_dir, "bg_home_lqip.webp")
lqip.save(lqip_dst, "WEBP", quality=35)
lqip_sz = os.path.getsize(lqip_dst)
print(f"bg_home_lqip.webp: {lqip_w}x{lqip_h} px -> {lqip_sz} bytes")

# Sync to dist/ui-preview/assets and dist/next/assets
sync_dirs = [
    r"D:\HITME GAME\unity-client\tools\deploy_web\dist\ui-preview\assets",
    r"D:\HITME GAME\unity-client\tools\deploy_web\dist\next\assets"
]

for d in sync_dirs:
    os.makedirs(d, exist_ok=True)
    for f in ["bg_home_750.webp", "bg_home_1170.webp", "bg_home_1536.webp", "bg_home_lqip.webp"]:
        src = os.path.join(out_dir, f)
        dst = os.path.join(d, f)
        shutil.copy2(src, dst)
    print(f"[+] Synced WebP assets to {d}")
