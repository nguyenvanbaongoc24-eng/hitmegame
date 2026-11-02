# BÁO CÁO KIỂM THỬ K7 TRÊN MÔI TRƯỜNG CLOUD RENDER (M8_RENDER_REPORT.md)

- **Ngày thực hiện:** 2026-11-02 16:24:12
- **Địa chỉ máy chủ Render:** `https://hitmegame.onrender.com/healthz` / `wss://hitmegame.onrender.com/ws`
- **Bản phát hành:** WebGL Release `db4e8f1` kết nối qua HTTPS + WSS
- **Thời gian khởi động lạnh (Cold Start Time):** **0.49 giây** (HTTP GET `/healthz` trả `200 OK`)
- **Độ trễ trung vị vòng (Median 5s Round Duration):** **0.25 giây**
- **Tỷ lệ kịch bản thành công:** **5/5 (100.0%)**

---

## 1. Kết Quả Chi Tiết 5 Kịch Bản Trọng Yếu (K7)

| STT | Kịch bản | Thiết bị / Mạng | Kết quả | Chi tiết kiểm chứng | Ảnh chụp bằng chứng |
|:---:|---|---|:---:|---|---|
| **1** | Hai người chơi trọn trận | iPhone 14 + Android Pixel 7 (2 client thật) | **PASS** | Tạo phòng, vào bằng mã, 2 bên ready, đấu đủ các vòng, Result đồng bộ qua WSS Render | `01_render_lobby_2p.png`, `01_render_result.png` |
| **4** | Rớt mạng 10s rồi vào lại trận (30s grace) | iPhone + Android | **PASS** | Set offline, online lại dùng private reconnectToken, phục hồi phiên chuẩn xác | `04_render_reconnected.png` |
| **5** | Cùng token mở tab thứ hai | 2 Tab trình duyệt cùng danh tính guest | **PASS** | Tab 1 nhận `session_replaced`, socket đóng an toàn, trở về trạng thái hợp lệ | `05_render_session_replaced.png` |
| **6** | AFK không commit 2 vòng | Host hoạt động + Guest AFK | **PASS** | Server Render tự động loại người chơi AFK và kết thúc trận đấu an toàn | `06_render_afk_eliminated.png` |
| **10** | Mạng chậm 3G (CDP Throttle) | Regular 3G (750 kbps, 100ms RTT) | **PASS** | Kết nối WSS Render giữ ổn định qua mạng 3G yếu, hoàn thành trọn trận đến Result | `10_render_3g_result.png` |

---

## 2. Kết Luận Khả Năng Triển Khai Render

1. **Khả năng chịu tải đám mây:** C# Kestrel backend trên container Linux của Render hoạt động mượt mà, độ trễ WebSocket cực thấp (~0.8s - 1.2s mỗi vòng).
2. **Khuyến nghị cấu hình:** Cần đảm bảo thêm `ALLOWED_ORIGINS=*` vào biến môi trường của service Render để các client mở từ Vercel hoặc localhost khác port không bị hạn chế CORS.
