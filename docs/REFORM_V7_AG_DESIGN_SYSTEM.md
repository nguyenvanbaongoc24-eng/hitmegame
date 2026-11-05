# HIT ME VN — ANTIGRAVITY DESIGN SYSTEM V7.0
## Vietnamese Chibi Arena / Premium Casual Battle UI

**Ngày cập nhật:** 2026-11-05  
**Phiên bản:** V7.0 (Master UI/UX Reconstruction)  
**Áp dụng cho:** Toàn bộ runtime UI trong `Assets/Editor/ProductionUISetup.cs`, `Assets/Scripts/UI/ProductionUIController.cs`, `Assets/Scripts/UI/UiTheme.cs`, và WebGL Loading Wrapper (`tools/deploy_web/index.html`).

---

## 1. ĐỊNH HƯỚNG THẨM MỸ (ART DIRECTION)

**HIT ME — Vietnamese Chibi Arena / Premium Casual Battle UI** kết hợp bối cảnh sân đình làng quê Việt Nam ấm áp, rực rỡ với hệ khung giao diện (HUD & Card System) sắc nét, hiện đại theo tiêu chuẩn game casual đối kháng di động thương mại:
- **Nền (Panels & Modals):** Tông **Dark Navy (`#111B2C`)** và **Deep Navy (`#0B1421`)** có độ tương phản cao, viền sáng tinh tế giúp nổi bật nhân vật chibi, icon vũ khí và chữ tiếng Việt có dấu.
- **Điểm nhấn (Accents):** Chỉ sử dụng tối đa 2 màu nhấn trong một panel (Golden Yellow `#FFCD38` cho CTA chính/vàng/chủ phòng, Cyan `#19D5D8` cho trạng thái đang chọn/thông tin phụ, Emerald Green `#17B987` cho trạng thái Sẵn sàng/Vào phòng, Coral Red `#F66A5D` cho hành động Hủy/Rời phòng/Cảnh báo).
- **Không pha tạp chất liệu:** Loại bỏ các thanh tiêu đề giả gỗ tối màu đè lên nền Navy hoặc các đường gạch ngang `---` trang trí thô.

---

## 2. BẢNG MÀU CHUẨN (COLOR PALETTE TOKENS)

| Token Name | Hex | RGB (0..1) | Mục đích sử dụng |
| :--- | :--- | :--- | :--- |
| **Dark Navy (Panel Surface)** | `#111B2C` | `(0.067, 0.106, 0.173, 0.96)` | Nền chính của Modal, Card lớn, Bottom Dock, Header Bar |
| **Deep Navy (Card / Slot Surface)** | `#0B1421` | `(0.043, 0.078, 0.129, 0.94)` | Nền ô Slot, Input field, Stat Card, Row nền phụ |
| **Elevated Navy (Sub-card)** | `#18263E` | `(0.094, 0.149, 0.243, 0.96)` | Nền thẻ nhân vật/vũ khí, nút phụ (Secondary Action) |
| **Golden Yellow (Primary Accent)** | `#FFCD38` | `(1.000, 0.804, 0.220, 1.00)` | CTA chính (`CHƠI NHANH`, `BẮT ĐẦU`), Huy hiệu Chủ phòng, Coin/Vàng |
| **Cyan (Active / Selection)** | `#19D5D8` | `(0.098, 0.835, 0.847, 1.00)` | Viền thẻ đang chọn, Tab active, Thanh tiến trình, Nút Tạo Phòng |
| **Emerald Green (Positive / Ready)** | `#17B987` | `(0.090, 0.725, 0.529, 1.00)` | Nút `VÀO PHÒNG`, `SẴN SÀNG`, trạng thái Online/Ready |
| **Coral Red (Danger / Cancel)** | `#F66A5D` | `(0.965, 0.416, 0.365, 1.00)` | Nút `RỜI PHÒNG`, `HỦY TÌM TRẬN`, `ĐÓNG`, cảnh báo lỗi/hết giờ |
| **Cream White (Primary Text)** | `#FFF8E8` | `(1.000, 0.973, 0.910, 1.00)` | Chữ tiêu đề, tên nhân vật, thông số chính trên nền tối |
| **Soft Slate (Secondary Text)** | `#B8C7E0` | `(0.722, 0.780, 0.878, 1.00)` | Nhãn phụ (Label), mô tả ngắn, trạng thái chờ |
| **Muted Slate (Disabled Text)** | `#6E809E` | `(0.431, 0.502, 0.620, 0.85)` | Chữ trên nút Disabled hoặc Slot trống |
| **Dark Ink (Text on Gold/Yellow)** | `#23180B` | `(0.137, 0.094, 0.043, 1.00)` | Chữ đậm trên nền nút Vàng (`#FFCD38`) đảm bảo tương phản WCAG AAA |

---

## 3. HỆ THỐNG TYPOGRAPHY (VIETNAMESE-FIRST SCALE)

- **Font duy nhất:** `BeVietnamPro-Bold SDF` (`Assets/Fonts/BeVietnamPro-Bold SDF.asset`), hỗ trợ đầy đủ 100% dấu tiếng Việt (`Ấ, Ầ, Ẩ, Ẫ, Ậ, Ắ, Ằ, Ẳ, Ẵ, Ặ, Ế, Ề, Ể, Ễ, Ệ, Ố, Ồ, Ổ, Ỗ, Ộ, Ớ, Ờ, Ở, Ỡ, Ợ, Ứ, Ừ, Ử, Ữ, Ự, Đ`).
- **Quy tắc chống tràn & chống co chữ:**
  - Không dùng `enableAutoSizing` với `fontSizeMin < 16` khiến chữ biến thành chấm nhỏ.
  - Không dùng `UPPERCASE` cho câu hướng dẫn dài hoặc thông báo nhiều hơn 5 từ.
  - Mọi nhãn (Label) căn trái phải đặt `pivot = (0, 0.5)` và `alignment = MidlineLeft`; mọi giá trị (Value) căn phải đặt `pivot = (1, 0.5)` và `alignment = MidlineRight`.

| Cấp chữ (Type Scale) | Kích thước Canvas (`1080×1920`) | Tương đương Mobile (`393×852`) | Quy tắc sử dụng |
| :--- | :--- | :--- | :--- |
| **Screen Title** | `40–46 pt` | `~18–20 px` | Tiêu đề màn hình / Modal (`CÀI ĐẶT`, `TẠO PHÒNG`, `PHÒNG CHỜ`) |
| **Section Title** | `28–32 pt` | `~13–15 px` | Tiêu đề nhóm (`ÂM THANH`, `CHỌN NHÂN VẬT`, `THÔNG TIN TRẬN`) |
| **Primary CTA Label** | `34–40 pt` | `~15–18 px` | Nhãn nút chính (`CHƠI NHANH`, `BẮT ĐẦU LUYỆN`) |
| **Secondary Button Label** | `26–30 pt` | `~12–14 px` | Nhãn nút phụ (`TẠO PHÒNG`, `VÀO PHÒNG`, `SAO CHÉP`, `SẴN SÀNG`) |
| **Body / Form Value** | `24–28 pt` | `~11.5–13 px` | Nội dung bảng, tên người chơi trong slot, thông số cấu hình |
| **Caption / Status Badge** | `20–23 pt` | `~10–11 px` | Nhãn tab dưới, trạng thái slot (`CHỦ PHÒNG`, `Đang chờ...`), HP số |

---

## 4. HỆ THỐNG 20 COMPONENTS TÁI SỬ DỤNG

1. **PrimaryButton:** Nền vàng `#FFCD38` (`ui_btn_primary_yellow`), chữ `#23180B` (`32–40pt`), chiều cao `104–128px` trên Canvas (`~46–56 CSS px`).
2. **SecondaryButton:** Nền xanh Cyan/Blue (`ui_btn_secondary_blue`) hoặc Green (`ui_btn_secondary_green`), chữ `#FFF8E8`, chiều cao `92–110px`.
3. **DangerButton:** Nền đỏ Coral `#F66A5D` (`ui_btn_danger_red`), chữ `#FFF8E8`, dùng cho `RỜI PHÒNG`, `HỦY TÌM TRẬN`, `ĐÓNG`.
4. **DisabledButton:** Alpha `0.55`, nền xám navy `#2A364B`, chữ `#94A3B8`, khóa tương tác rõ ràng kèm badge trạng thái nếu tính năng đang bảo trì.
5. **IconButton:** Nút vuông bo góc `88×88px` trở lên (`Settings` bánh răng, nút đóng `X` góc phải trên của Modal, nút `Prev/Next` chọn nhân vật).
6. **GameCard:** Thẻ nhân vật / vũ khí có khung viền `#19D5D8` khi chọn, nền `#111B2C`, ảnh đại diện ở trên/trái, tên và trạng thái ở dưới/phải, không tràn chữ.
7. **PlayerCard (Lobby Slot):** Thẻ ngang trong Phòng chờ (2 cột × 3 hàng), gồm Avatar nhân vật `64×64`, Badge `[CHỦ PHÒNG]` tách riêng, Tên người chơi (`MidlineLeft`), và Pill trạng thái `SẴN SÀNG / Đang chờ...` (`MidlineRight`).
8. **TextInput:** Ô nhập mã phòng (`JoinRoom`) cao `110px`, nền `#0B1421`, viền `#19D5D8`, chữ căn giữa `40pt` rõ nét, kích hoạt bàn phím hệ thống iOS/Android (không vẽ bàn phím giả đè màn hình).
9. **Slider:** Thanh kéo `Music` / `SFX` cao `44px`, vùng chạm rộng, tay kéo (Handle) `48×48px` nổi bật, đi kèm nhãn phần trăm (`80%`) bên phải.
10. **Toggle:** Nút bật/tắt (`Giảm chuyển động`, `Chế độ riêng tư`) dạng pill rõ trạng thái `BẬT` (Emerald `#17B987`) / `TẮT` (Slate `#334155`).
11. **Modal:** Khung hộp thoại căn giữa màn hình (`Settings`, `Profile`), chiều rộng tối đa `900px` (`83%` chiều ngang), chiều cao gọn `920–980px`, có backdrop tối `rgba(5,10,18,0.78)` và nút `X` góc phải trên.
12. **Toast:** Thông báo nổi phía trên (`Y = 0.86`), nền `#111B2C` viền `#FFCD38`, tự ẩn sau 2.5s, không che khuất nút hành động.
13. **LoadingIndicator:** Vòng radar phát sáng đồng tâm + thanh tiến trình vàng/cyan mượt mà trên màn Loading và Matchmaking.
14. **StatusBadge:** Huy hiệu bo tròn hiển thị trạng thái (`CHỦ PHÒNG`, `SẴN SÀNG`, `ĐANG TẢI...`, `4/6 NGƯỜI THẬT`).
15. **BottomNavigation:** Dock 3 tab (`Trang chủ`, `Xếp hạng`, `Nhiệm vụ`) gắn liền thành một khối ở đáy màn hình (`Y = 0.015..0.095`), tab đang chọn sáng màu Cyan `#19D5D8`.
16. **HeaderBar:** Thanh tiêu đề trên cùng của các màn phụ (`Tạo phòng`, `Vào phòng`, `Phòng chờ`, `Luyện với Bot`) có nút `← QUAY LẠI` (`≥ 48px` touch target) và tiêu đề căn giữa.
17. **EmptyState:** Hiển thị rõ khi chưa tìm thấy phòng hoặc slot đang trống (`Chỗ trống #2 — Đang chờ người chơi...`).
18. **ErrorState:** Hộp thông báo lỗi mạng/mã phòng không hợp lệ kèm nút `THỬ LẠI` rõ ràng.
19. **CharacterPortrait:** Khung ảnh đại diện nhân vật (`Nam Làng`, `Cô Ba`, `Chú Tư`, `Bà Năm`) luôn có hình thật bên trong, tuyệt đối không để khung xanh rỗng.
20. **WeaponSlot:** Ô hiển thị vũ khí trang bị (`Dép Tổ Ong`, `Ghế Nhựa Đỏ`, `Bình Xịt Muỗi`, `Nón Lá`) kèm tên và nhãn hiệu ứng ngắn gọn.

---

## 5. QUY TẮC VÙNG CHẠM & RESPONSIVE (TOUCH TARGET & SAFE AREA)

- **Vùng chạm tối thiểu (Minimum Touch Target):** Mọi nút bấm tương tác phải đạt tối thiểu `96×96 px` trên Canvas `1080×1920` (tương đương `~43×43 CSS px` trên màn `393×852` và `~48×48 CSS px` khi `matchWidthOrHeight = 0.5`).
- **Nút CTA chính:** Cao `116–132 px` trên Canvas (`~52–60 CSS px` trên điện thoại).
- **Safe Horizontal Margin:** Toàn bộ nội dung nằm trong dải `anchorX = 0.04 .. 0.96` (`43px` lề mỗi bên trên Canvas `1080`), đảm bảo không bao giờ chạm mép màn hình trên cả `360×800` lẫn `412×915`.
- **In-Match Arena Visibility:** Khu vực sân đình từ `Y = 0.20` đến `Y = 0.80` hoàn toàn trong suốt, tuyệt đối không dùng bảng đen đặc che khuất cổng đình hay sàn gạch.
