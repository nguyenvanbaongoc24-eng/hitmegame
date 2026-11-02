# HIT ME — BÁO CÁO TOÀN DIỆN THIẾT BỊ & RỦI RO CODEX (ROUND 45)

**Thời gian:** 2026-11-02 19:45 UTC+7  
**Bản dựng WebGL:** `WebGL_Release_20261102_188344e`  
**Nhánh Git Vercel:** `web-dist` (Commit: `58895db`)  
**URL Trực tiếp Vercel:** [hitmegame-iota.vercel.app](https://hitmegame-iota.vercel.app/)  
**Độ phủ kiểm thử:** Playwright WebKit + Chromium trên 4 dòng thiết bị (cả Dọc và Ngang), tổng cộng 32 ảnh chụp màn hình kiểm chứng.

---

## 1. RÀ SOÁT & XỬ LÝ DANH SÁCH RỦI RO CODEX (CAO / TRUNG BÌNH)

| Hạng mục rủi ro | Mức độ | Hiện trạng & Xử lý | Trạng thái |
| :--- | :---: | :--- | :---: |
| **R1. Render Build & Health Check** | **CAO** | Server Render bị lỗi `open Dockerfile: no such file or directory`. Codex đã bổ sung `Dockerfile` và `render.yaml` ở thư mục gốc (commit `17e5112`). Kiểm tra cục bộ `/health` và `/healthz` đều trả về HTTP 200 (329ms). Biến môi trường then chốt: **`ALLOWED_ORIGINS`** cần chứa `https://hitmegame-iota.vercel.app` (không có dấu gạch chéo cuối). | **ĐÃ CẤP SỬA** |
| **R2. Rò rỉ mật khẩu / Dọn dữ liệu** | **CAO** | Đã chạy công cụ quét an toàn `tools/supabase/scan_secrets.py`: **0 vi phạm** (blobs=2676, findings=0). Đã thêm hook `pre-commit` chặn commit `.env`. **Hành động người dùng:** Reset database password trên Supabase Dashboard và nhập vào Render Environment. | **ĐÃ KIỂM TRA** |
| **R3. Adapter cấm AFK trên Supabase** | **TRUNG BÌNH** | Đã chạy contract test trực tiếp với Supabase: **5/5 PASS** (`Evidence/round43/R3_ban_supabase.txt`). Dữ liệu test tự động dọn sạch. | **ĐẠT** |
| **R4 & R5. Protocol v2 & Số dư server** | **TRUNG BÌNH** | Codex đã hoàn thành protocol v2 trong `ProtocolGateway.cs` và `PlayerProfileReader.cs` (145/145 server tests pass). Client Unity WebGL tương thích cả envelope v1 và v2. | **ĐẠT** |

---

## 2. KIỂM TRA BẢN BUILD TRÊN VERCEL (hitmegame-iota.vercel.app)

- **Trạng thái HTTP:** HTTP 200 OK.
- **Tài sản WebGL:** File `WebGL_Release_20261102_188344e.wasm.br` (4,613,397 bytes) trả về đúng `Content-Encoding: br`, `Content-Type: application/wasm`, `Etag: "1988ed57ce7beaf20208b4a47643da46"`.
- **Tránh Cache bản cũ:** File tải trang index.html gọi trực tiếp gói `WebGL_Release_20261102_188344e`. Khởi tạo thành công vào màn hình Home trong 20.63s, không còn cảnh báo lỗi WebGL.
- **Ảnh chụp trực tiếp từ Vercel:** `Evidence/round45/vercel_live_home.png`.

---

## 3. MA TRẬN KIỂM THỬ THIẾT BỊ: PLAYWRIGHT WEBKIT + CHROMIUM (8 CẤU HÌNH)

Toàn bộ 32 ảnh chụp màn hình kiểm chứng lưu tại thư mục: `Evidence/round45/matrix/`.

| Thiết bị | Động cơ Browser | Hướng màn hình | Kích thước (CSS) | Màn Home | Màn Cài đặt | Màn Đặt quân | Màn Trận đấu |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **iPhone SE** | WebKit | Dọc (Portrait) | 375 × 667 | PASS | PASS | PASS | PASS |
| **iPhone SE** | WebKit | Ngang (Landscape) | 667 × 375 | Lock banner | Lock banner | Lock banner | Lock banner |
| **iPhone 15 Pro** | WebKit | Dọc (Portrait) | 393 × 852 | PASS | PASS | PASS | PASS |
| **iPhone 15 Pro** | WebKit | Ngang (Landscape) | 852 × 393 | Lock banner | Lock banner | Lock banner | Lock banner |
| **Pixel 7** | Chromium | Dọc (Portrait) | 412 × 915 | PASS | PASS | PASS | PASS |
| **Pixel 7** | Chromium | Ngang (Landscape) | 915 × 412 | Lock banner | Lock banner | Lock banner | Lock banner |
| **iPad** | WebKit | Dọc (Portrait) | 810 × 1080 | PASS | PASS | PASS | PASS |
| **iPad** | WebKit | Ngang (Landscape) | 1080 × 810 | PASS (Pillar) | PASS (Pillar) | PASS (Pillar) | PASS (Pillar) |

*Ghi chú về chế độ Ngang (Landscape):*
- Trên điện thoại di động (iPhone SE, iPhone 15 Pro, Pixel 7), màn hình hiển thị thông báo xoay màn hình chuẩn: **"VUI LÒNG XOAY DỌC MÀN HÌNH - Trò chơi được thiết kế tối ưu cho trải nghiệm màn hình dọc điện thoại"**.
- Trên máy tính bảng (iPad), hệ thống tự động căn giữa cột dọc (pillar-box) để đảm bảo tỉ lệ sân đình hiển thị chuẩn xác không bị bóp méo.

---

## 4. BẢNG PHÂN ĐỊNH RÕ RÀNG: GIẢ LẬP VS MÁY THẬT (PHỤC VỤ ĐÁNH GIÁ KHÁCH QUAN)

> [!IMPORTANT]
> Để đảm bảo tính trung thực tuyệt đối theo tiêu chuẩn dự án, dưới đây là phân định chi tiết giữa kết quả giả lập và máy vật lý:

### A. Đã xác nhận trên MÁY THẬT VẬT LÝ (Người dùng kiểm chứng)
1. **Lỗi hiển thị trước đây (Bản cũ 52f9100):** Đã được người dùng chụp lại từ iPhone thật mở qua Safari/Zalo:
   - Viền đen bó khung hai bên.
   - Nút Cài đặt bị đè chữ; hai núm kéo trùng lặp; nút Báo Lỗi phẳng khối cam đè nút Đóng.
   - Nhân vật/vũ khí to quá khổ (2.0x).
   - Ô vuông xám không texture ở góc dưới bên phải trận đấu.
   - Vết đen cắt ngang nền sân đình.

### B. Đã xác nhận trên GIẢ LẬP TỰ ĐỘNG HÓA (Playwright WebKit & Chromium)
1. **Cảm ứng WebKit:** Mô phỏng chạm ngón tay thật qua `page.touchscreen.tap` trên WebKit iOS engine (không dùng synthetic hook). Nhân vật nhận vị trí chạm và di chuyển chính xác.
2. **Kích thước & Tràn viền:** 100vw × 100dvh trên cả 4 kích thước iPhone SE, iPhone 15 Pro, Pixel 7, iPad (0% viền đen).
3. **Căn chỉnh UI:** 100% các nút và banner đã thẳng hàng, nút Báo lỗi nằm riêng hàng, nút Rời trận ẩn tại Home, thanh trượt chỉ 1 núm tròn.
4. **Vercel Live:** Kiểm thử headless tải thành công từ CDN Vercel đạt 20.63s.

### C. CẦN KIỂM TRA TRÊN MÁY THẬT VẬT LÝ (Bước người dùng thực hiện tiếp)
1. Mở Safari trên điện thoại iPhone thật (khuyến nghị mở Tab Ẩn danh / Private Tab hoặc xóa cache để đảm bảo không lưu bản cũ).
2. Truy cập: `https://hitmegame-iota.vercel.app/`
3. Quan sát góc dưới màn hình hiển thị: `build 188344e ... Bản thử nghiệm`.
4. Dùng ngón tay chạm thử vào sân trong chế độ "Luyện với Bot" để cảm nhận độ nhạy cảm ứng vuốt thật trên mặt kính điện thoại.
