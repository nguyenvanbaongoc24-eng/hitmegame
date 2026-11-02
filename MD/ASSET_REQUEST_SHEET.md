# BẢNG ĐẶC TẢ YÊU CẦU TẠO ẢNH SPRITE CÒN THIẾU (ASSET_REQUEST_SHEET.md)

Tài liệu này được biên soạn từ `Evidence/round42/MISSING_ASSETS.md` và `Assets/Resources/Items.json` dành riêng cho artist hoặc công cụ sinh ảnh AI (Midjourney, DALL-E, Stable Diffusion, Recraft). Mọi ảnh xuất ra cần tuân thủ tuyệt đối quy chuẩn kỹ thuật:
- **Định dạng:** PNG-32 với kênh Alpha (RGB + Alpha), nền trong suốt 100% (zero background, no white fringes).
- **Phong cách mỹ thuật (Art Style):** Chibi 2.5D dân gian Việt Nam, đường nét rõ ràng (bold clean outlines 2-3px dark tint, không viền đen thô), đổ bóng dịu (soft ambient occlusion ở đáy), màu sắc tươi sáng nhưng giữ nét hoài cổ.
- **Quy chuẩn kích thước:** Icon trang bị đóng khung vuông 256×256 px; Projectile (vũ khí ném) nằm vừa khung 256×256 px hoặc 128×128 px tùy tỷ lệ dài rộng.
- **Góc nhìn (Perspective):** Top-down nghiêng 45 độ (2.5D isometric view), đầu ngắm hoặc mũi vũ khí hướng lên trên hoặc chéo góc 45 độ theo chuẩn bay.

---

## 1. Bảng Đặc Tả Chi Tiết Từng Vật Phẩm

| STT | Tên vật phẩm | Item ID | Loại ảnh | Kích thước xuất | Số pose/khung | Hướng nhìn | Viền & Đổ bóng | Tên tệp đích | Thư mục lưu trữ | Ghi chú mỹ thuật & Mô tả chi tiết |
|:---:|:---|:---|:---:|:---:|:---:|:---|:---|:---|:---|:---|
| 1 | Cốc bia hơi thủy tinh xanh | `coc_bia_hoi` | Icon cửa hàng | 256×256 | 1 tĩnh | Thẳng đứng 2.5D | Viền xanh thẫm 2px, bóng tròn mờ đáy | `coc_bia_icon.png` | `Assets/Sprites/Icons/` | Cốc thủy tinh vại xanh bọt trắng mịn sủi trên miệng, có quai cầm |
| 2 | Cốc bia hơi thủy tinh xanh | `coc_bia_hoi` | Projectile bay | 256×256 | 4 góc xoay (0°, 90°, 180°, 270°) | Góc nghiêng 45° | Viền mỏng 2px, vệt bọt bia văng | `coc_bia_projectile.png` | `Assets/Sprites/Projectiles/` | Cốc bia đang xoay lộn vòng khi ném, vệt bia sánh nhẹ |
| 3 | Xe đạp Phượng Hoàng cổ | `xe_dap_phuong_hoang` | Icon cửa hàng | 256×256 | 1 tĩnh | Nghiêng 30° trái | Viền xám chì 2px, bóng ngả sau | `xe_dap_icon.png` | `Assets/Sprites/Icons/` | Xe đạp đòn giông màu xanh rêu thời bao cấp, có chuông kính coong |
| 4 | Xe đạp Phượng Hoàng cổ | `xe_dap_phuong_hoang` | Projectile bay | 256×256 | 1 tĩnh | Trục ngang xoay 15° | Viền kim loại 2px, vệt gió lốc | `xe_dap_projectile.png` | `Assets/Sprites/Projectiles/` | Khung xe đạp xoay tròn theo trục trọng tâm khi phóng |
| 5 | Xe máy Honda Cub 50cc | `xe_may_honda_cub` | Icon cửa hàng | 256×256 | 1 tĩnh | Nghiêng 30° trái | Viền xanh tím 2px, bóng đổ xe | `xe_may_icon.png` | `Assets/Sprites/Icons/` | Chiếc Cub 50 yếm trắng thân xanh ngọc/xanh cánh chả, gương tròn |
| 6 | Xe máy Honda Cub 50cc | `xe_may_honda_cub` | Projectile bay | 256×256 | 1 tĩnh | Xoay 20° đầu dốc | Viền kim loại, khói pô sau | `xe_may_projectile.png` | `Assets/Sprites/Projectiles/` | Chiếc Cub bay vèo theo đường parabol, khói phì nhẹ đuôi |
| 7 | Xương vàng Cún may mắn | `chu_cho_vang` | Icon cửa hàng | 256×256 | 1 tĩnh | Nằm ngang nghiêng 15° | Hào quang vàng kim lung linh, viền cam | `cho_vang_icon.png` | `Assets/Sprites/Icons/` | Khúc xương bằng vàng ròng 9999 sáng lấp lánh, có nơ đỏ thắt giữa |
| 8 | Xương vàng Cún may mắn | `chu_cho_vang` | Projectile bay | 256×256 | 4 góc xoay | Xoay tròn đều | Viền phát sáng vàng gold, tia sao lấp lánh | `cho_vang_projectile.png` | `Assets/Sprites/Projectiles/` | Khúc xương vàng xoay tít như boomerang phát quang vàng rực |

---

## 2. Gợi Ý Prompt Chuẩn Dành Cho Công Cụ Sinh Ảnh (Midjourney / Stable Diffusion)

### `coc_bia_hoi` (Cốc bia hơi Hà Nội)
> **Prompt:** `2D game asset sprite, iconic Vietnamese green bubble-glass beer mug filled with golden beer and frothy white foam on top, chibi stylized, clean dark outline, soft drop shadow, 2.5D orthographic view, isolated on pure white background, PNG ready, no background --no text, realistic photos, blur`

### `xe_dap_phuong_hoang` (Xe đạp Phượng Hoàng)
> **Prompt:** `2D game asset sprite, vintage Phoenix vintage bicycle dark moss green frame, classic chrome handlebars with bell, cute chibi stylized proportions, clean outline, 2.5D game view, isolated on solid white background --no background, photographic, noisy details`

### `xe_may_honda_cub` (Xe máy Super Cub 50)
> **Prompt:** `2D game asset sprite, vintage Honda Super Cub 50 scooter, teal blue body with white leg shields, cute round headlight, chibi stylized vector game asset, bold clean line art, isolated on pure background --no background, complex gradients`

### `cho_vang_icon` (Xương vàng may mắn)
> **Prompt:** `2D mobile game icon, mythical glowing solid gold dog bone weapon with a small lucky red ribbon tied around it, shiny metallic sparkles, chibi stylized, clean bold outline, vibrant vibrant game UI icon on pure transparent background`
