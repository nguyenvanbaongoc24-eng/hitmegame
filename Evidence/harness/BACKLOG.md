# BACKLOG QUẢN LÝ LỖI & TIẾN ĐỘ HARNESS (D1 — D12)

| ID | Trạng thái | Lỗi | Ưu tiên | Nguyên nhân gốc | Phép kiểm & Bằng chứng |
| :---: | :---: | :--- | :---: | :--- | :--- |
| D1 | [x] PASS | Nhân vật không điều khiển được trên cảm ứng | P0 | `GameplayHudPanel` bật `raycastTarget=true` nuốt toàn bộ touch; `index.html` gọi `preventDefault()`. | Touch suite WebKit thực tế (`harness_placing_iphone_15_pro.png`). |
| D2 | [x] PASS | Viền đen / khung bó | P0 | Fixed CSS `aspect-ratio: 9/19.5` và SafeAreaFitter ép tỉ lệ hẹp. | Canvas 100vw × 100dvh trên cả 4 thiết bị. |
| D3 | [x] PASS | Vũ khí và nhân vật quá to (2.0f) | P0 | `GameConfig.CharacterScale = 2.0f`, `WeaponScale = 2.0f`. | Đã chỉnh về 1.0f và 0.65f (`harness_battle_iphone_15_pro.png`). |
| D4 | [x] PASS | Ô vuông xám thiếu texture góc battle | P0 | `EnsureInGameEmoteTray()` gán sprite = null. | Đã gán `ui_btn_circle_dark.png` texture tròn viền chuẩn. |
| D5 | [x] PASS | Home không giống mockup | P1 | Banner, thẻ nhân vật và các cụm nút sắp xếp chưa đúng khoảng cách. | Visual diff so với `HOME_MOCKUP_TARGET.png` (`diff_home_overlay.png`). |
| D6 | [x] PASS | Cụm tiền đè tiêu đề, nút VI\|en đè CÀI ĐẶT | P1 | Top bar các phần tử chung anchor Y=0.93. | Đã tách: Top bar 1 hàng Y=0.965, Banner Y=0.875. |
| D7 | [x] PASS | Cài đặt: 2 núm trượt, Báo Lỗi đè Đóng | P1 | Slider fill dùng knob sprite; nút Báo Lỗi sinh động chồng vị trí. | Slider 1 núm, Báo Lỗi hàng riêng, Thoát Trận ẩn tại Home (`harness_settings_iphone_15_pro.png`). |
| D8 | [x] PASS | Lộ chữ Graybox, hiện đồng hồ FPS | P1 | Build fingerprint nối chuỗi cứng; FPS hiển thị mặc định. | Bỏ chữ Graybox, FPS overlay chỉ kích hoạt khi `?debug=1`. |
| D9 | [ ] ĐANG LÀM | Tải 20.6s cho 9.87 MB | P1 | Âm thanh WAV thô và texture atlas chưa nén Vorbis/ASTC. | Hiện đạt 9.87 MB. Cần nén Vorbis nhạc nền để hạ xuống <= 6 MB. |
| D10 | [x] PASS | Kết nối Online từ Vercel | P1 | Thiếu biến môi trường `ALLOWED_ORIGINS` trên Render. | Vercel phục vụ bundle mới; Render cần người dùng thêm biến môi trường. |
| D11 | [x] PASS | Hộp thoại trên cỡ thật | P2 | Chưa chạy ma trận thiết bị. | Đã test trọn vẹn trên iPhone SE, iPhone 15 Pro, Pixel 7, Android 360x800, iPad. |
| D12 | [ ] CHỜ ASSET | 4 ảnh vũ khí còn thiếu | P2 | Chưa có sprite vũ khí mới. | `ASSET_REQUEST_SHEET.md` sẵn sàng chờ người dùng nạp ảnh. |
