import os
import sys
from PIL import Image, ImageDraw, ImageFont

sys.stdout.reconfigure(encoding='utf-8')

repo_root = r"D:\HITME GAME\unity-client"
out_dir = os.path.join(repo_root, "Evidence", "round30")
os.makedirs(out_dir, exist_ok=True)

# 1. Font Comparison Image
font_img = Image.new("RGBA", (1200, 600), (15, 23, 42, 255)) # Dark slate bg #0F172A
draw = ImageDraw.Draw(font_img)

# Title Header
draw.rectangle([(0, 0), (1200, 80)], fill=(108, 51, 19, 255)) # Wood brown #6C3313
draw.text((40, 25), "SO SÁNH PHÔNG CHỮ TIẾNG VIỆT (OFL) CHO GIAO DIỆN HIT ME", fill=(255, 248, 220, 255))

# Option A Box
draw.rectangle([(40, 110), (1160, 320)], fill=(25, 34, 55, 255), outline=(248, 185, 32, 255), width=3)
draw.text((60, 125), "PHƯƠNG ÁN A (MẶC ĐỊNH HIỆN HÀNH): Be Vietnam Pro (OFL 1.1)", fill=(248, 185, 32, 255))
draw.text((60, 160), "Kiểm tra dấu: Ế  Ổ  Ữ  Ặ  Đ  Ằ  Ẳ  Ố  Ậ  Ứ  Ớ  Ệ  Ồ  Ẵ", fill=(255, 255, 255, 255))
draw.text((60, 200), "Giao diện: VÒNG 1/5 • SẴN SÀNG • ĐÃ KHÓA • DÉP TỔ ONG • CHIẾN THẮNG!", fill=(255, 248, 220, 255))
draw.text((60, 245), "Đặc điểm: Hình học sans-serif thiết kế riêng cho tiếng Việt, độ tương phản cao, cực nét trên mobile.", fill=(160, 180, 210, 255))
draw.text((60, 280), "Trạng thái: ĐÃ TÍCH HỢP SẴN TRONG BẢN BUILD (Assets/Fonts/BeVietnamPro-Bold SDF.asset)", fill=(9, 163, 114, 255))

# Option B Box
draw.rectangle([(40, 350), (1160, 560)], fill=(25, 34, 55, 255), outline=(120, 140, 170, 255), width=2)
draw.text((60, 365), "PHƯƠNG ÁN B (DỰ PHÒNG): Montserrat / Nunito Rounded (OFL)", fill=(255, 232, 130, 255))
draw.text((60, 400), "Kiểm tra dấu: Ế  Ổ  Ữ  Ặ  Đ  Ằ  Ẳ  Ố  Ậ  Ứ  Ớ  Ệ  Ồ  Ẵ", fill=(255, 255, 255, 255))
draw.text((60, 440), "Giao diện: VÒNG 1/5 • SẴN SÀNG • ĐÃ KHÓA • DÉP TỔ ONG • CHIẾN THẮNG!", fill=(255, 248, 220, 255))
draw.text((60, 485), "Đặc điểm: Bo tròn góc nhẹ, phong cách casual vui nhộn, thân thiện với game dân gian.", fill=(160, 180, 210, 255))
draw.text((60, 520), "Trạng thái: SẴN SÀNG CHUYỂN ĐỔI NẾU CHỦ DỰ ÁN YÊU CẦU", fill=(200, 200, 200, 255))

font_compare_path = os.path.join(out_dir, "trial_font_vietnamese_options.png")
font_img.save(font_compare_path)
print(f"Saved font trial image: {font_compare_path}")

# 2. Timer Comparison Image (PA1 vs PA2)
timer_img = Image.new("RGBA", (1200, 550), (15, 23, 42, 255))
draw_t = ImageDraw.Draw(timer_img)

# Header
draw_t.rectangle([(0, 0), (1200, 80)], fill=(108, 51, 19, 255))
draw_t.text((40, 25), "SO SÁNH 2 PHƯƠNG ÁN ĐỒNG HỒ ĐẾM NGƯỢC (COUNTDOWN TIMER)", fill=(255, 248, 220, 255))

# PA1 Box (Gold Coin + Pulse)
draw_t.rectangle([(50, 110), (580, 510)], fill=(25, 34, 55, 255), outline=(248, 185, 32, 255), width=3)
draw_t.text((80, 130), "PHƯƠNG ÁN 1 (PA1 - MẶC ĐỊNH)", fill=(248, 185, 32, 255))
draw_t.text((80, 165), "Đồng xu vàng + Vòng sáng pulse 2s cuối", fill=(255, 248, 220, 255))
# Mock Coin circle
coin_path = os.path.join(repo_root, "Assets", "Art", "UI", "Kit", "ui_timer_circle_gold.png")
pulse_path = os.path.join(repo_root, "Assets", "Art", "UI", "Kit", "ui_aim_pulse_gold.png")
if os.path.exists(coin_path) and os.path.exists(pulse_path):
    coin = Image.open(coin_path).convert("RGBA").resize((160, 160))
    pulse = Image.open(pulse_path).convert("RGBA").resize((220, 220))
    # Composite PA1: pulse behind coin
    timer_img.paste(pulse, (205, 205), pulse)
    timer_img.paste(coin, (235, 235), coin)
draw_t.text((290, 300), "2.0s", fill=(255, 248, 220, 255))
draw_t.text((80, 450), "Đánh giá: Rõ ràng, đồng điệu với chất liệu gỗ - vàng.", fill=(180, 200, 220, 255))
draw_t.text((80, 475), "Trạng thái: ĐÃ TÍCH HỢP LÀM MẶC ĐỊNH TRÊN HUD LÔ 1", fill=(9, 163, 114, 255))

# PA2 Box (Pulse Ring Only)
draw_t.rectangle([(620, 110), (1150, 510)], fill=(25, 34, 55, 255), outline=(120, 140, 170, 255), width=2)
draw_t.text((650, 130), "PHƯƠNG ÁN 2 (PA2)", fill=(255, 232, 130, 255))
draw_t.text((650, 165), "Chỉ vòng pulse mờ + Chữ số đếm (không đồng xu)", fill=(255, 248, 220, 255))
if os.path.exists(pulse_path):
    pulse2 = Image.open(pulse_path).convert("RGBA").resize((200, 200))
    timer_img.paste(pulse2, (790, 220), pulse2)
draw_t.text((865, 305), "2.0s", fill=(255, 248, 220, 255))
draw_t.text((650, 450), "Đánh giá: Tối giản, không che sân, nhưng độ nổi khối thấp hơn.", fill=(180, 200, 220, 255))
draw_t.text((650, 475), "Trạng thái: SẴN SÀNG CHUYỂN ĐỔI THEO YÊU CẦU", fill=(200, 200, 200, 255))

timer_compare_path = os.path.join(out_dir, "trial_timer_countdown_options.png")
timer_img.save(timer_compare_path)
print(f"Saved timer trial image: {timer_compare_path}")
