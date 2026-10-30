# HIT ME — BẢNG ÁNH XẠ TÀI NGUYÊN GIAO DIỆN (UI ASSET MAP)
*Tài liệu kiểm kê và quy hoạch toàn bộ thành phần giao diện theo bộ tài nguyên `ASSET HITME FINAL` phục vụ triển khai Đợt 29 (Round 29 UI Skin).*

---

## 1. Nguyên tắc triển khai và bảo toàn tài nguyên
1. **Tuyệt đối không sửa file gốc**: Mọi hình ảnh được sao chép nguyên trạng từ `D:\HITME GAME\ASSET HITME FINAL\` vào `Assets/Art/UI/` trong Unity client.
2. **Không tự vẽ khung phẳng thay thế**: Nếu asset có sẵn $\rightarrow$ dùng đúng kit. Nếu thiếu $\rightarrow$ đánh dấu trạng thái `Thiếu` / `Cần vẽ lại` để xin ý kiến Chủ dự án, các khung tạm thời bắt buộc gắn nhãn `TẠM`.
3. **Cơ chế 9-Slice chuẩn**: Cấu hình biên co giãn (Border L/R/T/B) chính xác trong Sprite Editor để không làm méo hoa văn góc viền gỗ và kim loại.
4. **Hiệu năng & Bộ nhớ**:
   - Đóng gói Sprite Atlas theo màn hình.
   - Dung lượng bản build WebGL tăng không quá $\le 4\text{ MB}$ (so sánh file `.data`).
   - 0 cấp phát bộ nhớ mỗi frame trong trận đấu (`ManagedAllocPerFrame: 0`), dùng pool UI, không `Instantiate`/`Destroy`.

---

## 2. Bảng ánh xạ chi tiết từng thành phần giao diện

| # | Thành phần giao diện | Màn hình | Đường dẫn asset gốc trong `ASSET HITME FINAL` | Kích thước gốc | Cách dùng (9-slice / Cố định) | Trạng thái | Chủ dự án duyệt |
|:---:|---|:---:|---|:---:|---|:---:|:---:|
| **1** | Nút "CHƠI NGAY" / "BẮT ĐẦU" | Home, Bot Training | `HITME_UI_KIT_CLEAN/ui_btn_cta_yellow.png` | 512×128 | 9-slice (L:36, R:36, T:28, B:28) | **Có** | CHƯA |
| **2** | Nút "LUYỆN VỚI BOT" | Home | `HITME_UI_KIT_CLEAN/ui_btn_cta_blue.png` | 360×110 | 9-slice (L:36, R:36, T:28, B:28) | **Có** | CHƯA |
| **3** | Nút "TẠO PHÒNG" / "VÀO PHÒNG" | Home | `HITME_UI_KIT_CLEAN/ui_btn_cta_green.png` | 360×110 | 9-slice (L:36, R:36, T:28, B:28) | **Có** | CHƯA |
| **4** | Khung nhập mã phòng (HM4826) | Home, Lobby | `HITME_UI_KIT_CLEAN/ui_input_box_cyan.png` | 480×100 | 9-slice (L:24, R:24, T:20, B:20) | **Có** | CHƯA |
| **5** | Nút "RỜI PHÒNG" / "HỦY" | Lobby, Bot Training | `HITME_UI_KIT_CLEAN/ui_btn_cta_red.png` | 512×128 | 9-slice (L:36, R:36, T:28, B:28) | **Có** | CHƯA |
| **6** | Nút tròn quay lại (Back / Exit) | Bot Training, Lobby | `HITME_UI_KIT_CLEAN/ui_btn_circle_back.png` | 128×128 | Sprite cố định (1:1 tròn) | **Có** | CHƯA |
| **7** | Khung Panel chính (Nền Modal) | Bot Training, Result | `HITME_UI_KIT_CLEAN/ui_panel_dark_gold.png` | 512×512 | 9-slice (L:48, R:48, T:48, B:48) | **Có** | CHƯA |
| **8** | Khung Thẻ Nhân Vật (Bình thường) | Bot Training, Lobby, HUD | `HITME_UI_KIT_CLEAN/ui_card_char_normal.png` | 256×320 | 9-slice (L:24, R:24, T:24, B:24) | **Có** | CHƯA |
| **9** | Khung Thẻ Nhân Vật (Được chọn) | Bot Training, Lobby | `HITME_UI_KIT_CLEAN/ui_card_char_selected.png` | 256×320 | 9-slice (L:24, R:24, T:24, B:24) | **Có** | CHƯA |
| **10** | Banner Gỗ Biểu Ngữ Pha ("ĐẶT", "KHÓA", "LỘ DIỆN", "NÉM") | Gameplay HUD | `HITME_UI_KIT_CLEAN/ui_banner_wood.png` | 512×140 | 9-slice (L:64, R:64, T:20, B:20) | **Có** | CHƯA |
| **11** | Nhãn "VÒNG 1/5" | Gameplay HUD | `HITME_UI_KIT_CLEAN/ui_banner_wood.png` (thu nhỏ) | 512×140 | 9-slice (L:40, R:40, T:20, B:20) | **Có** | CHƯA |
| **12** | Đồng hồ đếm ngược (Icon + Viền) | Gameplay HUD | `HITME_AIM_KIT_V1/.../Art/PNG/Aim_Pulse_00.png` | 512×512 | Sprite cố định (co giãn nhịp pulse) | **Có** | CHƯA |
| **13** | Trái tim máu đầy (Heart Full) | Gameplay HUD | `ASSET EFFECT/.../12_Heart_Heal/12_Heart_Heal_F01.png` | 512×512 | Sprite cố định (32×32 hiển thị) | **Có** | CHƯA |
| **14** | Trái tim vỡ khi mất máu (Heart Break) | Gameplay HUD | `ASSET EFFECT/.../12_Heart_Heal/12_Heart_Heal_F04.png` | 512×512 | Sprite animation (chuỗi 6 frames) | **Có** | CHƯA |
| **15** | Nút "SẴN SÀNG" (Chưa khóa) | Gameplay HUD | `HITME_UI_KIT_CLEAN/ui_btn_cta_yellow.png` | 512×128 | 9-slice (L:36, R:36, T:28, B:28) | **Có** | CHƯA |
| **16** | Nút "ĐÃ KHÓA" (Sau khi khóa) | Gameplay HUD | `HITME_UI_KIT_CLEAN/ui_btn_cta_blue.png` | 360×110 | 9-slice (L:36, R:36, T:28, B:28) | **Có** | CHƯA |
| **17** | Ô chỉ báo vũ khí (Weapon Box) | Gameplay HUD | `HITME_UI_KIT_CLEAN/ui_card_char_normal.png` (vuông) | 256×320 | 9-slice (L:24, R:24, T:24, B:24) | **Có** | CHƯA |
| **18** | Biểu tượng vũ khí Chảo | Gameplay HUD | `ASSET WEAPONS/CHẢO/sprites/sprite_001.png` | 164×240 | Sprite cố định | **Có** | CHƯA |
| **19** | Biểu tượng vũ khí Dép tổ ong | Gameplay HUD | `Assets/Art/Sprites/SandalProjectile.png` | 256×256 | Sprite cố định | **Có** | CHƯA |
| **20** | Thước ngắm mềm (Aim Beam) | Gameplay (Sân) | `HITME_AIM_KIT_V1/.../Art/PNG/Aim_Beam_Soft.png` | 1024×128 | Sprite co giãn theo độ dài kéo | **Có** | CHƯA |
| **21** | Mũi tên hướng ngắm (Direction Arrow)| Gameplay (Sân) | `HITME_AIM_KIT_V1/.../Art/PNG/Aim_Direction_Arrow.png` | 256×256 | Sprite cố định xoay theo góc ngắm | **Có** | CHƯA |
| **22** | Vòng chân người chơi (Foot Ring) | Gameplay (Sân) | `HITME_AIM_KIT_V1/.../Art/PNG/Aim_Origin_Ring.png` | 512×512 | Sprite cố định chân nhân vật | **Có** | CHƯA |
| **23** | Tâm ngắm mục tiêu (Target Reticle) | Gameplay (Sân) | `HITME_AIM_KIT_V1/.../Art/PNG/Aim_Target_Reticle.png` | 512×512 | Sprite xoay nhẹ tại điểm nhắm | **Có** | CHƯA |
| **24** | Dòng xếp hạng đối thủ | Result Screen | `HITME_UI_KIT_CLEAN/ui_row_leaderboard.png` | 600×96 | 9-slice (L:32, R:32, T:20, B:20) | **Có** | CHƯA |
| **25** | Dòng xếp hạng bản thân | Result Screen | `HITME_UI_KIT_CLEAN/ui_row_leaderboard_me.png` | 600×96 | 9-slice (L:32, R:32, T:20, B:20) | **Có** | CHƯA |
| **26** | Nút "ĐẤU LẠI" | Result Screen | `HITME_UI_KIT_CLEAN/ui_btn_cta_yellow.png` | 512×128 | 9-slice (L:36, R:36, T:28, B:28) | **Có** | CHƯA |
| **27** | Nút "VỀ TRANG CHỦ" | Result Screen | `HITME_UI_KIT_CLEAN/ui_btn_cta_blue.png` | 360×110 | 9-slice (L:36, R:36, T:28, B:28) | **Có** | CHƯA |
| **28** | Tranh nền màn hình chính (Bối cảnh làng quê) | Home Screen | `ASSET BỐI CẢNH/BỐI CẢNH/300b93e0-d734-4f09-93dd-4dc44de4e43a.png` | 941×1672 | Tranh nền dọc (Aspect Fit/Fill) | **Có** | CHƯA |
| **29** | Huy chương Hạng 1st, 2nd, 3rd (Medals) | Result Screen | Chưa có sprite huy chương riêng biệt trong kit | N/A | Sprite cố định | **Thiếu** | CHƯA |
| **30** | Icon Coin vàng đếm ngược | Gameplay HUD | Chưa có sprite đồng tiền vàng phẳng riêng lẻ | N/A | Sprite cố định | **Thiếu** | CHƯA |
| **31** | Biểu tượng Vợt Pickleball (Vũ khí 3) | Gameplay HUD | Có trong pose ném của nhân vật, chưa có sprite độc lập | N/A | Sprite cố định | **Cần cắt** | CHƯA |

---

## 3. Danh sách thành phần thiếu asset (Cần Chủ dự án quyết định)

1. **Huy chương Hạng 1, 2, 3 (Medals)**:
   - *Hiện trạng*: Trong `HITME_UI_KIT_CLEAN` chưa có bộ sprite huy chương vàng/bạc/đồng riêng lẻ.
   - *Đề xuất tạm thời*: Dùng nhãn số nổi bật kết hợp màu sắc vàng (#FFD700) / bạc (#C0C0C0) / đồng (#CD7F32) đặt bên trong dòng `ui_row_leaderboard.png` gắn mác `[TẠM]`.
   - *Quyết định của Chủ dự án*: [ ] Duyệt dùng nhãn số tạm / [ ] Cung cấp thêm bộ sprite huy chương.

2. **Biểu tượng Đồng xu vàng đếm ngược (Coin Timer)**:
   - *Hiện trạng*: Bộ `HITME_AIM_KIT_V1` có icon vòng pulse vàng sáng `Aim_Pulse_00.png` có thể làm nền đồng hồ rất đẹp và khớp chủ đề, nhưng chưa có sprite đồng xu đúc nét làng quê.
   - *Đề xuất tạm thời*: Dùng vòng tròn vàng pulse của Aim Kit kết hợp chữ số đếm đậm gắn mác `[TẠM]`.
   - *Quyết định của Chủ dự án*: [ ] Duyệt dùng vòng Aim Kit / [ ] Cung cấp sprite đồng xu.

3. **Sprite vũ khí Vợt Pickleball đơn lẻ**:
   - *Hiện trạng*: Đã có sprite Chảo gang và Dép tổ ong. Vợt Pickleball hiện nằm trong chuỗi animation `ANH NÓN LÁ/NÉM VỢT PICKLEBALL/`.
   - *Đề xuất*: Trích xuất frame `sprite_011.png` (frame rơi riêng biệt của vợt) làm sprite biểu tượng vũ khí.

---

## 4. Bảng màu chủ đạo (Theme Palette trích xuất từ Asset)

Bảng màu được trích xuất trực tiếp từ các file texture của bộ kit bằng thuật toán phân tích màu trung vị (median color clustering):

| Tên màu | Mã HEX | RGB | Mục đích sử dụng trong UI |
|---|:---:|:---:|---|
| **Vàng Nắng Làng Quê (CTA Primary)** | `#F8B920` | `248, 185, 32` | Nút hành động chính ("CHƠI NGAY", "SẴN SÀNG", "BẮT ĐẦU"), điểm nhấn quan trọng |
| **Đỏ Cờ Truyền Thống (Alert / Cancel)** | `#CD2B2B` | `205, 43, 43` | Nút hủy, rời phòng, cảnh báo nguy hiểm, tim máu |
| **Xanh Lá Tre Đồng Quê (Action Secondary)** | `#09A372` | `9, 163, 114` | Nút tạo phòng, vào phòng, xác nhận thành công |
| **Xanh Dương Sông Nước (Info / Action)** | `#2861E3` | `40, 97, 227` | Nút luyện với bot, nút đã khóa, nút về sảnh |
| **Nâu Gỗ Mái Đình (Wood Banner)** | `#6C3313` | `108, 51, 19` | Nền biểu ngữ thông báo pha, khung nhãn vòng đấu |
| **Vàng Kim Cổ Điển (Gold Border)** | `#DFB244` | `223, 178, 68` | Viền khung thẻ nhân vật, viền modal panel |
| **Nền Tối Xanh Chàm (Dark Slate Background)**| `#0F172A` | `15, 23, 42` | Nền modal dialog, nền bảng xếp hạng (thay thế nền đen đặc) |
| **Kem Gạo Chữ Đọc (Cream Text)** | `#FFF8DC` | `255, 248, 220` | Màu chữ trên nền gỗ và nền tối (dịu mắt, không chói như neon) |

---

## 5. Ảnh ghép xem trước tài nguyên (Contact Sheet Preview)
Toàn bộ asset đã được tập hợp và kết xuất thành ảnh xem trước độ phân giải cao tại:
- Đường dẫn repository: `docs/ui_preview_contact_sheet.png`
- Đường dẫn kiểm định artifact: `C:\Users\LENOVO\.gemini\antigravity\brain\15ef4497-c923-4d70-a855-259c984d1ed2\ui_preview_contact_sheet.png`
