# HIT ME VN — REFORM V7.0 VISUAL QA & VIEWPORT VERIFICATION REPORT

**Ngày nghiệm thu:** 2026-11-05  
**Nhánh làm việc:** `feature/ui-ux-production` (`D:\HITME GAME\unity-client`)  
**Scene Runtime:** `Assets/Scenes/Graybox.unity`  
**Thư mục bằng chứng ảnh chụp thật:** `D:\HITME GAME\unity-client\Evidence\reform_v7\`

---

## 1. BẢNG NGHIỆM THU 10 MÀN HÌNH CHÍNH (BEFORE → AFTER → VERIFICATION)

| STT | Màn hình | Vấn đề trên Screenshot cũ (Before) | Giải pháp đã triển khai (After — V7.0) | File Ảnh Runtime (`1080×2340`) | Kết quả QA |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **1** | **Loading Screen** | Nền đen đơn điệu, chỉ có chữ "HIT ME" kèm nhãn "BETA", không có nhân vật hay mẹo chơi, thiếu cổng kích hoạt âm thanh trên iOS. | Nền sân đình làng quê có lớp phủ mờ, thẻ trung tâm viền vàng, Logo HIT ME chính thức, nhân vật Nam Làng đứng trên bục sáng, thanh tiến trình Cyan/Vàng theo tiến độ thật của Unity, câu gợi ý vui nhộn và nút **"CHẠM ĐỂ BẮT ĐẦU"** (mở khóa âm thanh WebAudio) + trạng thái Thử lại khi lỗi. | `Evidence/reform_v7/01_loading_1080x2340.png` | **PASS** |
| **2** | **Main Menu** | Nền bị che khuất, nhân vật đứng trên vòng tròn xanh đè lệch lên bệ đá của ảnh nền, thẻ Nhân vật/Vũ khí quá nhỏ, nút Tạo/Vào phòng xa nhau, 3 nút dưới đáy tách rời. | Sử dụng nền sân đình `u4_background.png` (`ui_bg_home_village.png`), loại bỏ bệ giả trùng lặp và đặt bóng đổ mềm trực tiếp lên bệ đá hoa sen thật; Top Bar gọn (Avatar + Tên + Vàng + Cài đặt `84×84`); 2 thẻ **NHÂN VẬT / VŨ KHÍ** cân xứng (`435×126`); CTA **CHƠI NHANH** vàng nổi bật (`890×155`); **TẠO PHÒNG / VÀO PHÒNG** đối xứng (`435×132`); **LUYỆN VỚI BOT** có icon robot; Dock đáy 3 tab (**Trang chủ** active màu Cyan `#19D5D8`). | `Evidence/reform_v7/02_main_menu_1080x2340.png` | **PASS** |
| **3** | **Settings Modal** | Bảng quá to chiếm gần hết màn hình, tiêu đề có gạch ngang `---`, thiếu chỉ số `%` âm lượng, thiếu nút đóng `X` góc phải. | Modal gọn (`900×1020`), có nút **X** góc phải đồng bộ hành vi với nút **ĐÓNG**, bỏ toàn bộ dấu `---`, thanh kéo Nhạc nền / Hiệu ứng rộng thoáng kèm nhãn **50% / 80%** cập nhật trực tiếp, công tắc **Giảm chuyển động**, chọn ngôn ngữ **TIẾNG VIỆT / ENGLISH**, nút **BÁO LỖI** & **ĐÓNG** cân đối. | `Evidence/reform_v7/03_settings_1080x2340.png` | **PASS** |
| **4** | **Create Room** | **Lỗi P0:** Các dòng thông tin cấu hình phòng bị tràn sang mép trái màn hình do lệch Pivot/Anchor. | Bố cục lại thẻ cấu hình với 4 hàng sub-card chuẩn (`Số người tối đa`, `Chế độ phòng`, `Bản đồ thi đấu`, `Thời gian đặt & ngắm`), nhãn căn trái (`pivot = (0, 0.5)`) và giá trị căn phải (`pivot = (1, 0.5)`), hoàn toàn không còn tràn chữ. | `Evidence/reform_v7/04_create_room_1080x2340.png` | **PASS** |
| **5** | **Room Lobby** | Chữ tên người chơi đè lên trạng thái `CHƯA SẴN SÀNG / ĐÃ SẴN SÀNG`, tiêu đề phòng tràn qua nút Back, các ô trống vẫn hiện mặt nhân vật. | Tiêu đề phòng tự động co chữ gọn trong bảng gỗ (`PHÒNG CHIẾN HỮU (4/6)`), tách riêng vùng Tên (`450px` bên trái) và Trạng thái (`280px` bên phải), hiển thị Avatar thật cho các ô có người và ẩn Avatar ở các ô trống (`Chỗ trống #5, #6 — Đang chờ...`). | `Evidence/reform_v7/05_room_lobby_1080x2340.png` | **PASS** |
| **6** | **Join Room** | Ô nhập có sẵn mã cứng `"HM4826"`, thẻ phòng tìm thấy luôn hiện giả, bàn phím ảo tự vẽ che mất nút bấm, bố cục tụt xuống nửa dưới dễ bị bàn phím iOS che. | Đưa cụm `JoinCard` và nút **VÀO PHÒNG NGAY** lên nửa trên màn hình (`Y = 0.67` và `Y = 0.44`), để trống mã phòng với placeholder `"Nhập mã phòng (VD: HM4826)..."`, ẩn bàn phím ảo tự vẽ để dùng bàn phím hệ điều hành, ẩn thẻ kết quả cho tới khi tra cứu thật. | `Evidence/reform_v7/06_join_room_1080x2340.png` | **PASS** |
| **7** | **Gameplay HUD** | Hai mảng nền đen đặc ở đầu và cuối màn hình che mất cổng đình và bậc thềm, thẻ HP hiển thị 3 chấm nhỏ khó nhìn. | Giảm độ đậm nền HUD (`alpha = 0.24`) để lộ trọn vẹn sân đình, hiển thị Avatar + Tên + **HP: 3/3 (2/3, 1/3, 0/3)** + trạng thái **CÒN SỐNG / ĐANG ẨN / ĐÃ LOẠI** rõ nét không chồng lấn, thanh chọn hướng ném và nút Emote gọn gàng. | `Evidence/reform_v7/07_gameplay_placing_1080x2340.png`, `07b_gameplay_reveal_1080x2340.png` | **PASS** |
| **8** | **Matchmaking** | Hộp thoại tìm trận quá tối giản, thiếu trạng thái trực quan. | Modal **ĐANG TÌM TRẬN** với vòng xung radar Cyan, Avatar người chơi ở tâm, 6 vạch trạng thái người chơi, bộ đếm thời gian chờ và nút **HỦY TÌM TRẬN** rõ ràng. | `Evidence/reform_v7/08_matchmaking_1080x2340.png` | **PASS** |
| **9** | **Profile Modal** | Ô Avatar chỉ là khung xanh rỗng, thông tin rời rạc, thiếu ô nhập tên mới rõ ràng. | Hiển thị chân dung nhân vật thật (`AvatarPortrait`) trong khung Cyan, tên + ID + huy hiệu Vàng/Hạng, 2 thẻ thống kê (**THÀNH TÍCH ĐẤU**, **VŨ KHÍ TRANG BỊ**), ô nhập tên mới có viền Cyan rõ ràng và nút **ĐỔI TÊN / ĐÓNG** + nút **X**. | `Evidence/reform_v7/09_profile_1080x2340.png` | **PASS** |
| **10** | **Bot Training & Result** | Icon nhân vật ở Bot Training hơi sát chữ, nút `- / +` bị đè lên hình mũi tên `<`; bảng Result bị mờ và chồng chữ `HẠNG 1 [TẠM]` lên `#1`. | Căn chỉnh lại 7 thẻ nhân vật, thêm icon vũ khí ở thẻ chi tiết, dùng nút `-` (xanh) và `+` (vàng) không dính mũi tên; bảng **Result** hiển thị sắc nét 100% độ mờ với 4 cột tách biệt (`#Hạng | Tên | HP & Hit | Trạng thái / + Kết bạn`). | `Evidence/reform_v7/10_bot_training_1080x2340.png`, `11_result_1080x2340.png` | **PASS** |

---

## 2. KIỂM THỬ ĐA ĐỘ PHÂN GIẢI MOBILE (RESPONSIVE VIEWPORT MATRIX)

Toàn bộ giao diện đã được kiểm thử tự động và chụp ảnh thực tế qua `SafeAreaFitter` + `CanvasScaler (1080×2340, Match = 0.5)` trên các khung hình mục tiêu:

| Khung hình (Viewport) | Tỷ lệ | Thiết bị đại diện | Ảnh chụp thực tế (`Evidence/reform_v7/`) | Tràn chữ / Chồng lấn | Vùng chạm CTA (`>= 48px`) | Kết quả |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **393 × 852** | ~19.5:9 | iPhone 15 / 15 Pro | `vp_393x852_main_menu.png`, `vp_393x852_create_room.png`, `vp_393x852_room_lobby.png`, `vp_393x852_gameplay.png` | **0 lỗi** | Đạt chuẩn | **PASS** |
| **390 × 844** | ~19.5:9 | iPhone 13 / 14 | `vp_390x844_main_menu.png`, `vp_390x844_create_room.png`, `vp_390x844_room_lobby.png`, `vp_390x844_gameplay.png` | **0 lỗi** | Đạt chuẩn | **PASS** |
| **360 × 800** | 20:9 | Android phổ thông | `vp_360x800_main_menu.png`, `vp_360x800_create_room.png`, `vp_360x800_room_lobby.png`, `vp_360x800_gameplay.png` | **0 lỗi** | Đạt chuẩn | **PASS** |
| **412 × 915** | 20:9 | Android màn hình dài (Galaxy S23/Pixel) | `vp_412x915_main_menu.png`, `vp_412x915_create_room.png`, `vp_412x915_room_lobby.png`, `vp_412x915_gameplay.png` | **0 lỗi** | Đạt chuẩn | **PASS** |
| **1080 × 2340** | 19.5:9 | Reference Resolution | `01_loading_1080x2340.png` .. `11_result_1080x2340.png` | **0 lỗi** | Đạt chuẩn | **PASS** |

---

## 3. KIỂM THỬ TỰ ĐỘNG UNITY EDITMODE TESTS

- Bộ kiểm thử tự động trong Unity (`ProductionUIPolishTests`, `ButtonColorRegressionTest`, `Phase4Luot3DataSanitizationTests`, `UILintTests`, `SafeAreaFitterTests`, v.v.):
  - **Tổng số bài test:** `191`
  - **Passed:** `191 / 191` (`Failed: 0`)
- Không tạo thêm `AudioManager` thứ hai, giữ nguyên toàn bộ kết nối sự kiện và dữ liệu runtime với phần việc của Codex.
