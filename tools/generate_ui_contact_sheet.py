import os
from PIL import Image, ImageDraw, ImageFont

base_final = r"D:\HITME GAME\ASSET HITME FINAL"
out_dir = r"D:\HITME GAME\unity-client\docs"
os.makedirs(out_dir, exist_ok=True)

def create_contact_sheet():
    # We will arrange the UI elements onto a grid
    kit_dir = os.path.join(base_final, "HITME_UI_KIT_CLEAN")
    aim_dir = os.path.join(base_final, "HITME_AIM_KIT_V1", "HITME_AIM_KIT_V1", "Art", "PNG")
    vfx_heart = os.path.join(base_final, "ASSET EFFECT", "HITME_VFX_02_Elements_Impacts", "HIT_ME_VN_VFX_PACK", "01_PRODUCTION_NEON", "12_Heart_Heal", "12_Heart_Heal_F01.png")
    weapon_chao = os.path.join(base_final, "ASSET WEAPONS", "CHẢO", "sprites", "sprite_001.png")
    
    # Target elements to showcase
    items = [
        ("Nút Vàng CTA (Bắt đầu / Sẵn sàng)", os.path.join(kit_dir, "ui_btn_cta_yellow.png")),
        ("Nút Đỏ (Rời phòng / Hủy)", os.path.join(kit_dir, "ui_btn_cta_red.png")),
        ("Nút Xanh Lá (Tạo phòng / Vào)", os.path.join(kit_dir, "ui_btn_cta_green.png")),
        ("Nút Xanh Dương (Luyện với Bot)", os.path.join(kit_dir, "ui_btn_cta_blue.png")),
        ("Banner Gỗ (Biểu ngữ pha / Vòng)", os.path.join(kit_dir, "ui_banner_wood.png")),
        ("Khung Panel Viền Vàng", os.path.join(kit_dir, "ui_panel_dark_gold.png")),
        ("Thẻ Nhân Vật (Bình thường)", os.path.join(kit_dir, "ui_card_char_normal.png")),
        ("Thẻ Nhân Vật (Được chọn)", os.path.join(kit_dir, "ui_card_char_selected.png")),
        ("Khung Nhập Mã Phòng", os.path.join(kit_dir, "ui_input_box_cyan.png")),
        ("Nút Tròn Quay Lại (Back)", os.path.join(kit_dir, "ui_btn_circle_back.png")),
        ("Dòng Xếp Hạng (Leaderboard)", os.path.join(kit_dir, "ui_row_leaderboard.png")),
        ("Dòng Xếp Hạng Của Tôi", os.path.join(kit_dir, "ui_row_leaderboard_me.png")),
        ("Trái Tim Máu (HP Icon)", vfx_heart),
        ("Vũ Khí Chảo (Weapon Icon)", weapon_chao),
        ("Thước Ngắm Mềm (Aim Beam)", os.path.join(aim_dir, "Aim_Beam_Soft.png")),
        ("Mũi Tên Hướng Ngắm", os.path.join(aim_dir, "Aim_Direction_Arrow.png")),
        ("Vòng Khóa Mục Tiêu", os.path.join(aim_dir, "Aim_Target_Locked.png")),
        ("Tâm Ngắm Reticle", os.path.join(aim_dir, "Aim_Target_Reticle.png")),
    ]
    
    canvas_w = 1600
    canvas_h = 1800
    canvas = Image.new("RGBA", (canvas_w, canvas_h), (20, 24, 32, 255))
    draw = ImageDraw.Draw(canvas)
    
    # Title
    draw.text((40, 30), "HIT ME - BANG ANH XA TAI NGUYEN GIAO DIEN (ROUND 29 UI ASSET PREVIEW)", fill=(255, 230, 0, 255))
    draw.text((40, 65), "Toan bo asset trich xuat truc tiep tu thu muc goc 'ASSET HITME FINAL' (khong sua file goc)", fill=(180, 200, 220, 255))
    
    col_w = 760
    row_h = 180
    start_x = 40
    start_y = 110
    
    for idx, (title, path) in enumerate(items):
        r = idx // 2
        c = idx % 2
        x = start_x + c * col_w
        y = start_y + r * row_h
        
        # Draw box
        draw.rectangle([x, y, x + col_w - 20, y + row_h - 15], outline=(60, 75, 100, 255), width=2, fill=(28, 34, 48, 255))
        draw.text((x + 15, y + 10), f"#{idx+1}. {title}", fill=(240, 240, 240, 255))
        
        if os.path.exists(path):
            try:
                sprite = Image.open(path).convert("RGBA")
                sw, sh = sprite.size
                draw.text((x + 15, y + 32), f"Tep: {os.path.basename(path)} ({sw}x{sh})", fill=(140, 160, 180, 255))
                
                # Fit thumbnail into (col_w - 50, row_h - 65)
                max_tw = col_w - 60
                max_th = row_h - 65
                ratio = min(max_tw / sw, max_th / sh, 1.0)
                tw = max(1, int(sw * ratio))
                th = max(1, int(sh * ratio))
                thumb = sprite.resize((tw, th), Image.Resampling.LANCZOS)
                
                # Center sprite in bottom area of box
                pos_x = x + 20 + (max_tw - tw) // 2
                pos_y = y + 55 + (max_th - th) // 2
                canvas.paste(thumb, (pos_x, pos_y), thumb)
            except Exception as e:
                draw.text((x + 15, y + 60), f"Loi doc anh: {e}", fill=(255, 80, 80, 255))
        else:
            draw.text((x + 15, y + 60), "THIEU ASSET (Can ve bo sung)", fill=(255, 120, 120, 255))
            
    out_path = os.path.join(out_dir, "ui_preview_contact_sheet.png")
    canvas.save(out_path)
    print("Created contact sheet:", out_path)

if __name__ == "__main__":
    create_contact_sheet()
