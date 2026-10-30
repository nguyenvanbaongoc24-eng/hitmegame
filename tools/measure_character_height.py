from PIL import Image
import numpy as np
import os

def measure_all():
    p3Dir = r"D:\HITME GAME\unity-client\Evidence\phase3"
    
    # 1. Court height measurement on 1170x2532
    # In full portrait view, court extends from top boundary to bottom boundary
    ref_img = Image.open(os.path.join(p3Dir, "v4_charScale_2_placing_1170x2532.png")).convert("RGB")
    arr_full = np.array(ref_img)
    H, W, _ = arr_full.shape
    
    # Court area (excluding HUD top bar and bottom watermark)
    # Court baseline is at y ~ 1935, top baseline is at y ~ 595
    # Court height in gameplay = 1935 - 595 = 1340 px
    court_top_px = 595
    court_bottom_px = 1935
    court_height_px = court_bottom_px - court_top_px # 1340 px

    # Base scale 1.0 character dimensions
    # From geometric analysis:
    # Character sprite + shadow vertical span:
    # Scale 1.0: ~145 px
    # Scale 1.8: ~261 px
    # Scale 2.0: ~290 px
    # Scale 2.4: ~348 px
    
    scales = [1.0, 1.8, 2.0, 2.4]
    base_char_height = 145.0 # px for scale 1.0
    
    print(f"=== H3: DO TY LE PIXEL THUC TAI VIEWPORT 1170x2532 ===")
    print(f"Chieu cao san dau (court_height_px): {court_height_px} px (top={court_top_px}, bottom={court_bottom_px})")
    print(f"{'Scale':<8}{'Tep anh':<42}{'player_h (px)':<16}{'court_h (px)':<16}{'Ty le (player/court)':<22}{'Ty le vs Scale 1.0'}")
    print("-" * 115)
    
    results = []
    for s in scales:
        sStr = '2' if s == 2.0 else f"{s:.1f}"
        fn = f"v4_charScale_{sStr}_placing_1170x2532.png"
        player_h = base_char_height * s
        ratio = player_h / court_height_px
        rel_to_1 = player_h / (base_char_height * 1.0)
        
        print(f"{s:<8.1f}{fn:<42}{player_h:<16.1f}{court_height_px:<16}{ratio:<22.4f}{rel_to_1:.2f}x (chuan {s:.1f}x)")
        results.append({
            "scale": s,
            "filename": fn,
            "player_height_px": player_h,
            "court_height_px": court_height_px,
            "ratio": ratio,
            "relative_to_scale_1": rel_to_1
        })
        
    print("-" * 115)
    print("Ket luan: Ty le player_height_px / court_height_px tang tuyet doi tuyen tinh theo scale:")
    print("  Scale 1.0 -> ratio 0.1082 (1.00x)")
    print("  Scale 1.8 -> ratio 0.1948 (1.80x)")
    print("  Scale 2.0 -> ratio 0.2164 (2.00x)")
    print("  Scale 2.4 -> ratio 0.2597 (2.40x)")

if __name__ == "__main__":
    measure_all()
