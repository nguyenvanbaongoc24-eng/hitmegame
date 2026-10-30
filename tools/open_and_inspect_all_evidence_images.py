from PIL import Image
import os
import glob
import numpy as np

def inspect_all():
    target_dirs = [
        r"D:\HITME GAME\unity-client\Evidence\phase3",
        r"D:\HITME GAME\unity-client\Evidence\round28"
    ]
    
    inspected_count = 0
    print("=== INSPECTING AND OPENING ALL CAPTURED EVIDENCE IMAGES ===")
    
    for d in target_dirs:
        print(f"\n--- Scanning directory: {d} ---")
        png_files = sorted(glob.glob(os.path.join(d, "*.png")))
        for p in png_files:
            fn = os.path.basename(p)
            # Open file with PIL to verify integrity
            try:
                im = Image.open(p)
                w, h = im.size
                arr = np.array(im)
                
                # Check bottom-left watermark (y: h-80 to h-10, x: 20 to 650)
                sub_bl = arr[max(0, h-80):h-10, 20:min(w, 650), :3]
                # Yellow pixels (#FFE600: R>200, G>180, B<70)
                yellow_pixels = np.sum((sub_bl[:, :, 0] > 200) & (sub_bl[:, :, 1] > 175) & (sub_bl[:, :, 2] < 75))
                
                # Mean color of center screen
                center_crop = arr[h//4:3*h//4, w//4:3*w//4, :3]
                mean_rgb = np.mean(center_crop, axis=(0,1)).astype(int)
                
                inspected_count += 1
                has_watermark = "CO WATERMARK DUOI-TRAI" if yellow_pixels > 500 else f"YELLOW_PX={yellow_pixels}"
                print(f"[OK] {fn:<38} | {w}x{h:<5} | {has_watermark} | center_rgb=({mean_rgb[0]},{mean_rgb[1]},{mean_rgb[2]})")
            except Exception as e:
                print(f"[ERR] Failed to open {fn}: {e}")
                
    print(f"\nTotal images verified and opened: {inspected_count}")

if __name__ == "__main__":
    inspect_all()
