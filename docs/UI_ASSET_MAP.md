# HIT ME — BẢNG ÁNH XẠ TÀI NGUYÊN GIAO DIỆN (UI ASSET MAP)
*Tài liệu kiểm kê và quy hoạch toàn bộ thành phần giao diện theo bộ tài nguyên `ASSET HITME FINAL` phục vụ triển khai Đợt 29 & 30 (Round 30 UI Skin U1 & U2-Lô 1).*

---

## 1. Nguyên tắc triển khai và bảo toàn tài nguyên
1. **Tuyệt đối không sửa file gốc**: Mọi hình ảnh được sao chép nguyên trạng từ `D:\HITME GAME\ASSET HITME FINAL\` vào `Assets/Art/UI/` trong Unity client.
2. **Không tự vẽ khung phẳng thay thế**: Dùng đúng kit. Thành phần thiếu bắt buộc gắn nhãn `[TẠM]` để Chủ dự án quyết định.
3. **Phạm vi luật chơi v1.2**: Vũ khí duy nhất trong trận đấu là **Dép Tổ Ong**. Các asset Chảo gang, Vợt Pickleball được xếp vào mục "Ngoài phạm vi".
4. **Ngôn ngữ thuần Việt**: Toàn bộ nhãn hiển thị bằng tiếng Việt chuẩn có dấu (dùng "HẠNG 1/2/3", không dùng "1ST/2ND/3RD").
5. **Cơ chế 9-Slice chuẩn**: Cấu hình biên co giãn (Border L/R/T/B) chính xác trong Sprite Editor để không làm méo hoa văn góc viền gỗ và kim loại.
6. **Hiệu năng & Bộ nhớ**:
   - Vùng chạm tối thiểu $\ge 135\text{ px}$ (chuẩn mobile chiều rộng 1080px).
   - Dung lượng bản build WebGL tăng không quá $\le 4\text{ MB}$ (so sánh file `.data`).
   - 0 cấp phát bộ nhớ mỗi frame trong trận đấu (`ManagedAllocPerFrame: 0`), dùng pool UI, không `Instantiate`/`Destroy`.

---

## 2. Bảng ánh xạ chi tiết từng thành phần giao diện (Lô 1 – HUD Trong Trận)

| # | Thành phần giao diện | Màn hình | Đường dẫn asset gốc trong `ASSET HITME FINAL` | Kích thước gốc | Cách dùng (9-slice / Cố định) | Trạng thái kỹ thuật | Chủ dự án duyệt |
|:---:|---|:---:|---|:---:|---|:---:|:---:|
| **1** | Nút "CHƠI NGAY" / "BẮT ĐẦU" | Home, Bot Training | `HITME_UI_KIT_CLEAN/ui_btn_cta_yellow.png` | 512×128 | 9-slice (L:35, R:35, T:25, B:25) | Đã triển khai | CHƯA |
| **2** | Nút "LUYỆN VỚI BOT" | Home | `HITME_UI_KIT_CLEAN/ui_btn_cta_blue.png` | 360×110 | 9-slice (L:35, R:35, T:25, B:25) | Đã triển khai | CHƯA |
| **3** | Nút "TẠO PHÒNG" / "VÀO PHÒNG" | Home | `HITME_UI_KIT_CLEAN/ui_btn_cta_green.png` | 360×110 | 9-slice (L:35, R:35, T:25, B:25) | Đã triển khai | CHƯA |
| **4** | Khung nhập mã phòng (HM4826) | Home, Lobby | `HITME_UI_KIT_CLEAN/ui_input_box_cyan.png` | 480×100 | 9-slice (L:25, R:25, T:25, B:25) | Đã triển khai | CHƯA |
| **5** | Nút "RỜI PHÒNG" / "HỦY" | Lobby, Bot Training | `HITME_UI_KIT_CLEAN/ui_btn_cta_red.png` | 512×128 | 9-slice (L:35, R:35, T:25, B:25) | Đã triển khai | CHƯA |
| **6** | Nút tròn quay lại (Back / Exit) | Bot Training, Lobby | `HITME_UI_KIT_CLEAN/ui_btn_circle_back.png` | 128×128 | Sprite cố định (1:1 tròn) | Đã triển khai | CHƯA |
| **7** | Khung Panel chính (Nền Modal) | Bot Training, Result | `HITME_UI_KIT_CLEAN/ui_panel_dark_gold.png` | 512×512 | 9-slice (L:40, R:40, T:40, B:40) | Đã triển khai | CHƯA |
| **8** | Khung Thẻ HP Nhân Vật (Bình thường) | HUD, Lobby | `HITME_UI_KIT_CLEAN/ui_card_char_normal.png` | 256×320 | 9-slice (L:30, R:30, T:30, B:30) | **Đã lên HUD Lô 1** | CHƯA |
| **9** | Khung Thẻ HP Bản Thân (Được chọn) | HUD, Lobby | `HITME_UI_KIT_CLEAN/ui_card_char_selected.png` | 256×320 | 9-slice (L:30, R:30, T:30, B:30) | **Đã lên HUD Lô 1** | CHƯA |
| **10** | Banner Gỗ Biểu Ngữ Pha ("ĐẶT VỊ TRÍ & HƯỚNG NÉM") | Gameplay HUD | `HITME_UI_KIT_CLEAN/ui_banner_wood.png` | 512×140 | 9-slice (L:40, R:40, T:20, B:20) | **Đã lên HUD Lô 1** | CHƯA |
| **11** | Nhãn "VÒNG n/5" | Gameplay HUD | `HITME_UI_KIT_CLEAN/ui_banner_wood.png` | 512×140 | 9-slice (L:40, R:40, T:20, B:20) | **Đã lên HUD Lô 1** | CHƯA |
| **12** | Đồng hồ đếm ngược (Coin Vàng + Pulse Ring 2s cuối) | Gameplay HUD | `ui_timer_circle_gold.png` + `ui_aim_pulse_gold.png` | 512×512 | Sprite cố định (PA1 mặc định) | **Đã lên HUD Lô 1** | CHƯA |
| **13** | Trái tim máu đầy (Heart Full) | Gameplay HUD | `Assets/Art/UI/Kit/ui_icon_heart_full.png` | 512×512 | Sprite Image (thay ký tự "♥") | **Đã lên HUD Lô 1** | CHƯA |
| **14** | Trái tim vỡ khi mất máu (Heart Break) | Gameplay HUD | `Assets/Art/UI/Kit/ui_icon_heart_break.png` | 512×512 | Sprite Image (thay ký tự "♥") | **Đã lên HUD Lô 1** | CHƯA |
| **15** | Nút "SẴN SÀNG" (Trạng thái 1: CTA Vàng) | Gameplay HUD | `HITME_UI_KIT_CLEAN/ui_btn_cta_yellow.png` | 512×128 | 9-slice (320×76 px, >135px chạm) | **Đã lên HUD Lô 1** | CHƯA |
| **16** | Nút "ĐÃ KHÓA" (Trạng thái 2: CTA Xanh Dương) | Gameplay HUD | `HITME_UI_KIT_CLEAN/ui_btn_cta_blue.png` | 360×110 | 9-slice (320×76 px, >135px chạm) | **Đã lên HUD Lô 1** | CHƯA |
| **17** | Ô chỉ báo vũ khí (Weapon Box Gỗ Ấm) | Gameplay HUD | `HITME_UI_KIT_CLEAN/ui_banner_wood.png` | 512×140 | 9-slice (460×72 px) | **Đã lên HUD Lô 1** | CHƯA |
| **18** | Biểu tượng vũ khí Dép tổ ong + Tên rõ | Gameplay HUD | `Assets/Art/Weapons/dep_to_ong_icon.png` | 256×256 | Sprite cố định + Font chữ kem rõ | **Đã lên HUD Lô 1** | CHƯA |
| **19** | Thước ngắm mềm (Aim Beam) | Gameplay (Sân) | `HITME_AIM_KIT_V1/.../Aim_Beam_Soft.png` | 1024×128 | Co giãn theo hướng & độ dài kéo | **Đã lên HUD Lô 1** | CHƯA |
| **20** | Mũi tên hướng ngắm (Direction Arrow)| Gameplay (Sân) | `HITME_AIM_KIT_V1/.../Aim_Direction_Arrow.png` | 256×256 | Xoay theo góc ngắm | **Đã lên HUD Lô 1** | CHƯA |
| **21** | Vòng chân người chơi (Foot Ring / Touch Ring) | Gameplay (Sân) | `HITME_AIM_KIT_V1/.../Aim_Origin_Ring.png` | 512×512 | Vòng tròn bám chân nhân vật | **Đã lên HUD Lô 1** | CHƯA |
| **22** | Tâm ngắm mục tiêu (Target Reticle) | Gameplay (Sân) | `HITME_AIM_KIT_V1/.../Aim_Target_Reticle.png` | 512×512 | Xoay nhẹ tại điểm nhắm | **Đã lên HUD Lô 1** | CHƯA |
| **23** | Dòng xếp hạng đối thủ | Result Screen | `HITME_UI_KIT_CLEAN/ui_row_leaderboard.png` | 600×96 | 9-slice (L:30, R:30, T:30, B:30) | Sẵn sàng Lô 3 | CHƯA |
| **24** | Dòng xếp hạng bản thân | Result Screen | `HITME_UI_KIT_CLEAN/ui_row_leaderboard_me.png` | 600×96 | 9-slice (L:30, R:30, T:30, B:30) | Sẵn sàng Lô 3 | CHƯA |
| **25** | Nút "ĐẤU LẠI" | Result Screen | `HITME_UI_KIT_CLEAN/ui_btn_cta_yellow.png` | 512×128 | 9-slice (L:35, R:35, T:25, B:25) | Sẵn sàng Lô 3 | CHƯA |
| **26** | Nút "VỀ TRANG CHỦ" | Result Screen | `HITME_UI_KIT_CLEAN/ui_btn_cta_blue.png` | 360×110 | 9-slice (L:35, R:35, T:25, B:25) | Sẵn sàng Lô 3 | CHƯA |
| **27** | Tranh nền màn hình chính (Bối cảnh làng quê) | Home Screen | `ASSET BỐI CẢNH/BỐI CẢNH/300b93e0-d734-4f09-93dd-4dc44de4e43a.png` | 941×1672 | Tranh nền dọc (Aspect Fit/Fill) | Sẵn sàng Lô 2 | CHƯA |
| **28** | Huy hiệu Hạng 1, Hạng 2, Hạng 3 (`[TẠM]`) | Result Screen | Nhãn số tiếng Việt tròn viền Vàng/Bạc/Đồng | N/A | Huy hiệu số vector/TMP `[TẠM]` | Đã định dạng tiếng Việt | CHƯA |

---

## 3. Mục ngoài phạm vi (Out of Scope per Rules v1.2)
*Theo luật chơi v1.2, vũ khí duy nhất được phép sử dụng là **Dép Tổ Ong**. Không đưa các vũ khí sau vào gameplay hay UI chọn vũ khí:*
- **Chảo gang** (`ASSET WEAPONS/CHẢO/`): Ngoài phạm vi gameplay v1.2.
- **Vợt Pickleball** (`ANH NÓN LÁ/NÉM VỢT PICKLEBALL/`): Ngoài phạm vi gameplay v1.2.

---

## 4. Quyết định cần Chủ dự án chọn

1. **Chọn Phông chữ tiếng Việt (2 phương án OFL)**:
   - **Phương án A (Mặc định hiện hành)**: `Be Vietnam Pro` (OFL 1.1) — Hình học hiện đại, hiển thị sắc nét trên màn hình retina mobile, hỗ trợ đầy đủ 100% dấu tiếng Việt ("Ế Ổ Ữ Ặ Đ Ằ Ẳ Ố Ậ Ứ Ớ Ệ Ồ Ẵ").
   - **Phương án B**: `Nunito` / `Montserrat` (OFL) — Bo tròn thân thiện, phong cách casual game.
   *(Xem ảnh thử so sánh trực quan trong thư mục bằng chứng).*

2. **Chọn Kiểu Đồng hồ đếm ngược (2 phương án)**:
   - **Phương án 1 (PA1 - Mặc định)**: Đồng xu vàng hiện có (`ui_timer_circle_gold.png`) + vòng sáng pulse sau lưng trong 2 giây cuối.
   - **Phương án 2 (PA2)**: Chỉ vòng sáng pulse (`ui_aim_pulse_gold.png`) và số đếm, không có đồng xu.
   *(Xem ảnh thử so sánh trực quan trong thư mục bằng chứng).*

---

## 5. Bảng màu chủ đạo (UiTheme Palette)

| Tên màu | Mã HEX | RGB (0..1) | Mục đích sử dụng trong UI |
|---|:---:|:---:|---|
| **Vàng Nắng Làng Quê (CTA Primary)** | `#F8B920` | `0.972, 0.725, 0.125` | Nút "SẴN SÀNG", "CHƠI NGAY", viền sáng |
| **Đỏ Cờ Truyền Thống (Alert / Cancel)** | `#CD2B2B` | `0.804, 0.169, 0.169` | Nút hủy, cảnh báo, máu thấp, đã loại |
| **Xanh Lá Tre Đồng Quê (Action Green)** | `#09A372` | `0.040, 0.640, 0.447` | Nút thành công, trạng thái "CÒN SỐNG" |
| **Xanh Dương Sông Nước (Action Blue)** | `#2861E3` | `0.157, 0.380, 0.890` | Nút "ĐÃ KHÓA", "LUYỆN VỚI BOT", "VỀ TRANG CHỦ" |
| **Nâu Gỗ Mái Đình (Wood Banner)** | `#6C3313` | `0.424, 0.200, 0.075` | Nền biểu ngữ thông báo pha, khung nhãn vòng |
| **Vàng Kim Cổ Điển (Gold Border)** | `#DFB244` | `0.875, 0.698, 0.267` | Viền khung thẻ nhân vật, viền modal panel |
| **Nền Tối Xanh Chàm (Dark Slate Background)**| `#0F172A` | `0.059, 0.090, 0.165` | Nền modal dialog (thay thế nền đen đặc) |
| **Kem Gạo Chữ Đọc (Cream Text)** | `#FFF8DC` | `1.000, 0.973, 0.863` | Màu chữ chính trên nền gỗ và nền tối (dịu mắt) |
| **Chữ Phụ Vàng Nắng (Subtitle Gold)** | `#FFE882` | `1.000, 0.910, 0.510` | Tên nhân vật, tiêu đề phụ |
| **Watermark Góc Dưới-Trái (Watermark Yellow)** | `#FFE600` | `1.000, 0.902, 0.000` | Dòng `build <commit>` góc dưới màn hình |

---

## 6. Tổng hợp các tài nguyên THIẾU và TẠM (Gom cho Chủ dự án duyệt)
*Theo yêu cầu rà soát của Đợt 33 & 34, toàn bộ thành phần chưa có asset hoàn chỉnh hoặc đang dùng giải pháp tạm được gom tại đây:*

| # | Tên tài nguyên | Màn hình sử dụng | Trạng thái hiện tại | Giải pháp đang dùng tạm | Yêu cầu cung cấp bổ sung |
|:---:|---|:---:|:---:|---|---|
| **1** | **Xe Đạp Phượng Hoàng** | Cửa hàng, Túi đồ | `Thiếu` | Ghi nhãn "(Thiếu hiệu ứng)", chưa kích hoạt trong trận | Cung cấp file sprite icon, sprite cầm tay (held), sprite bay (projectile) |
| **2** | **Honda Cũ** | Cửa hàng, Túi đồ | `Thiếu` | Ghi nhãn "(Thiếu hiệu ứng)", chưa kích hoạt trong trận | Cung cấp file sprite icon, sprite cầm tay (held), sprite bay (projectile) |
| **3** | **Huy hiệu Hạng 1, 2, 3** | Bảng xếp hạng cuối trận | `TẠM` | Nhãn số TextMeshPro viền Vàng/Bạc/Đồng | Bộ 3 sprite huy chương/cúp (Gold, Silver, Bronze) phong cách làng quê |
| **4** | **Ảnh đại diện người chơi** | Trang chủ, Hồ sơ | `TẠM` | Khung thẻ nhân vật `ui_card_char_selected` | Bộ sprite avatar người chơi (chân dung nhân vật, khung viền cấp độ) |
| **5** | **Icon Đăng nhập MXH** | Hộp thoại Đăng nhập | `TẠM` | Nút CTA xanh có chữ "Google (SẮP CÓ)", "Facebook", "Apple" | Icon SVG/PNG chính thức của Google, Facebook, Apple |
| **6** | **SFX Dép bay trong gió** | Trong trận (pha ném) | `Thiếu` | Để trống (null an toàn), không tự chế âm thanh | Tệp âm thanh tiếng dép xé gió ngắn (0.3s - 0.5s) |
