# HIT ME — BẢN THỬ NGHIỆM BETA ĐÓNG v1 (`0.1.0-beta.7`)

- **Cách vào chơi:** Mở liên kết `https://hitmegame-iota.vercel.app/next/` trên trình duyệt điện thoại (Safari/Chrome). Nhập tên người chơi và chọn nhân vật bạn yêu thích.
- **Cách mời bạn cùng chơi:** Bấm **TẠO PHÒNG** → chạm **[📋 SAO CHÉP LIÊN KẾT]** hoặc **[📤 CHIA SẺ]** gửi qua Zalo/Messenger; bạn bè chỉ cần bấm vào liên kết dạng `https://hitmegame-iota.vercel.app/next/?join=<MÃ>` để vào thẳng phòng.
- **Tính năng nổi bật:**
  1. Hỗ trợ đầy đủ vòng đời di động: chống tắt màn hình (Screen Wake Lock), tự động giữ nhịp tim WebSocket (`ping/pong` 5s), lớp phủ đếm ngược 30 giây khi rớt mạng kèm tự động kết nối lại.
  2. Màn hình Kết thúc trận đấu hiển thị bảng xếp hạng đồng hạng chi tiết 2–6 người chơi (Sống sót → Tim còn → Đòn trúng), có 4 nút thao tác: Đấu lại (chủ phòng), Về phòng chờ, Ghép nhanh lại, Rời phòng.
  3. Tích hợp bộ bắt lỗi thời gian chạy (Error Catcher modal) và bảng chẩn đoán trực quan qua tham số `?debug=1`.
  4. Hiển thị nhãn minh bạch: Bảng xếp hạng, Nhiệm vụ, Cửa hàng gắn nhãn "Dữ liệu mẫu / Thử nghiệm", 3 nhân vật chưa xong động tác gắn nhãn "TẠM".
- **Lưu ý / Tình trạng máy chủ:** Máy chủ Render hiện tại chưa mở `ALLOWED_ORIGINS` cho tên miền Vercel (bắt tay WebSocket trả HTTP 403 `origin_not_allowed`), nên các nút Online trên `/next/` hiển thị thông báo "Máy chủ chưa sẵn sàng" kèm tùy chọn chuyển nhanh sang **Luyện với bot** (hoặc mở chế độ Lab 2 người qua `?lab=1`).
