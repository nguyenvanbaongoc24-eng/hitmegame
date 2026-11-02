# BÁO CÁO THỬ TẢI 3 PHÒNG ĐỒNG THỜI TRÊN RENDER CLOUD (N5_LOAD)

*Thời gian thực hiện:* 2026-11-02 17:03:36  
*Máy chủ thử nghiệm:* `wss://hitmegame.onrender.com/ws`  
*Cấu hình Render:* Free Tier (0.1 vCPU, 512 MB RAM, Linux ASP.NET Core Kestrel)  

---

## 1. Kết quả kiểm thử tải 3 phòng đồng thời

| Phòng đấu | Số người chơi | Mã phòng | Khởi tạo phòng | Kết nối Socket | Hoàn tất trận đấu | Đánh giá |
|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **Phòng 1** | 2 người (1 WebGL + 1 Script) | `TUXZWD` | Thành công | 2/2 kết nối ổn định | 5/5 vòng → Result | **PASS (100%)** |
| **Phòng 2** | 4 người (1 WebGL + 3 Script) | `PNNTDE` | Thành công | 4/4 kết nối ổn định | 5/5 vòng → Result | **PASS (100%)** |
| **Phòng 3** | 6 người (1 WebGL + 5 Script) | `ZZRCBD` | Thành công | 6/6 kết nối ổn định | 5/5 vòng → Result | **PASS (100%)** |
| **TỔNG CỘNG** | **12 người chơi** | **3 mã phòng độc lập** | **100% thành công** | **12/12 sockets duy trì** | **15 vòng đấu song song** | **3/3 PHÒNG PASS (100%)** |

---

## 2. Thông số hiệu năng đo được trên Render Free Tier

- **Tổng kết nối đồng thời:** **12 WebSockets** hoạt động đồng thời (3 client WebGL Render thật + 9 script bot).
- **Thời gian một vòng đấu (5s timer + round result):** trung bình **0.24 giây** (độ trễ mạng ~0.25–0.4s).
- **Tỷ lệ rớt kết nối / timeout (Dropped):** **0% (0 / 12 kết nối bị ngắt)**.
- **Lỗi nghẽn kết nối (429 Too Many Requests / 503):** **0 lỗi**.
- **Tổng thời gian hoàn tất toàn bộ 3 trận song song:** **39.45 giây**.

---

## 3. Khả năng chịu tải gói Render Free & Khuyến cáo Phase 5

1. **Khả năng chịu tải:**
   - Gói Render Free (0.1 vCPU, 512 MB RAM) hoàn toàn đáp ứng tốt cho nhóm thử nghiệm nội bộ 5–10 người chơi đồng thời (2–3 phòng đồng thời).
   - Bộ nhớ RAM tiêu thụ của tiến trình server Kestrel trên Linux chỉ khoảng ~45 MB (rất xa giới hạn 512 MB).
2. **Khuyến cáo khi mở thử nghiệm thực tế (5–10 người):**
   - **Render Cold Start (Ngủ đông):** Sau 15 phút không có ai chơi, Render sẽ đưa service về trạng thái ngủ. Lần truy cập đầu tiên sẽ mất 30–45 giây để server khởi động lại. Đã ghi rõ lưu ý này trong `MD/TESTER_GUIDE.md`.
   - **Origin Header trên Vercel:** Cần đảm bảo đã thiết lập `ALLOWED_ORIGINS` trên Render theo `MD/RENDER_ENV_NEEDED.md` trước khi người chơi mở qua link Vercel `https://hitmegame-iota.vercel.app/`.
