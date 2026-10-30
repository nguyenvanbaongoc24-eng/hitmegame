import os
from PIL import Image

base_dir = r"D:\HITME GAME\ASSET HITME FINAL"

def scan():
    categories = {}
    for root, dirs, files in os.walk(base_dir):
        rel_root = os.path.relpath(root, base_dir)
        top_cat = rel_root.split(os.sep)[0] if rel_root != "." else "root"
        for f in sorted(files):
            if f.lower().endswith(('.png', '.jpg', '.jpeg', '.tga', '.psd')):
                full_path = os.path.join(root, f)
                rel_path = os.path.relpath(full_path, base_dir).replace('\\', '/')
                try:
                    im = Image.open(full_path)
                    w, h = im.size
                    mode = im.mode
                    has_alpha = 'A' in mode or im.info.get('transparency') is not None
                except Exception as e:
                    w, h, mode, has_alpha = 0, 0, 'ERR', False
                
                categories.setdefault(top_cat, []).append({
                    "rel_path": rel_path,
                    "filename": f,
                    "width": w,
                    "height": h,
                    "mode": mode,
                    "has_alpha": has_alpha,
                    "full_path": full_path
                })
    return categories

if __name__ == "__main__":
    cats = scan()
    with open(r"D:\HITME GAME\unity-client\docs\asset_inventory_raw.txt", "w", encoding="utf-8") as out:
        for c, items in cats.items():
            out.write(f"\n=== CATEGORY: {c} ({len(items)} files) ===\n")
            for item in items:
                out.write(f"{item['rel_path']} | {item['width']}x{item['height']} | {item['mode']} | alpha={item['has_alpha']}\n")
    print("Done inventorying. Total categories:", len(cats))
