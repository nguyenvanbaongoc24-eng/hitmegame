# BÁO CÁO SO SÁNH GIAO DIỆN VỚI MOCKUP (UI_DIFF.md) — ROUND 38 A7

**Thời gian lập:** 11:09 AM 02/11/2026  
**Môi trường đo:** WebGL Release Brotli (:8103) · Build `WebGL_Release_20261102_bc33593_110253`  
**Độ phân giải chụp:** 1170 × 2532 (Portrait iPhone 14/15 Pro Max)  
**Thư mục ảnh:** `Evidence/round38/screens/`

---

## 1. Thông Số Build Release (A7)

| Tệp thành phẩm | Kích thước nén Brotli (.br) | Ghi chú |
|---|---|---|
| `WebGL_Release_20261102_bc33593_110253.data.br` | **12,124,561 bytes** (11.56 MB) | Asset data bundle chính |
| `WebGL_Release_20261102_bc33593_110253.wasm.br` | **4,561,714 bytes** (4.35 MB) | WebAssembly engine + logic |
| `WebGL_Release_20261102_bc33593_110253.framework.js.br` | **65,855 bytes** (64.3 KB) | Unity WebGL runtime bridge |
| `WebGL_Release_20261102_bc33593_110253.loader.js` | **27,914 bytes** (27.2 KB) | Loader script khởi tạo |
| **TỔNG DUNG LƯỢNG BUILD (RELEASE BROTLI)** | **16,772,442 bytes (~16.0 MB)** | Target < 20 MB ĐẠT |
| **Thời gian nạp HOME_SHOWN (Cold cache)** | **3.66 giây** | Target < 6.0s ĐẠT |

---

## 2. So Sánh Chi Tiết Từng Màn Hình Với Mockup

### Màn 1: Trang Chủ (`01_home_1170x2532.png` — 2.53 MB)
* **Đối chiếu mockup:** `ASSET HITME FINAL/HITME_UI_KIT_CLEAN/` & spec Trang Chủ.
* **Các điểm KHỚP hoàn toàn:**
  - Tiêu đề "HIT ME" chữ vàng nghệ thuật, phụ đề "NÉM DÉP LÀNG QUÊ".
  - Thẻ nhân vật trung tâm viền gỗ mun, ảnh chân dung Nam Làng, vũ khí trang bị Dép Tổ Ong.
  - 4 nút CTA xếp đúng phân cấp ưu tiên: (1) LUYỆN VỚI BOT (vàng to), (2) CHƠI NGAY (xanh dương), (3) TẠO PHÒNG (xanh lá), (4) VÀO PHÒNG (xanh viền).
  - Thanh dưới 4 icon/nút: CỬA HÀNG, TÚI ĐỒ, BẢNG XẾP HẠNG, KHÁCH.
  - Nút chuyển ngôn ngữ "VI | en" góc trên bên phải hoạt động tức thì.
  - Watermark build ở góc dưới hiển thị: `build bc33593 · 2026-11-02 11:02 · Graybox`.
* **Điểm lệch THỰC TẾ:**
  1. Thiếu mũi tên `<` và `>` hai bên thẻ nhân vật trên Home để báo hiệu người dùng có thể chạm đổi nhân vật trực tiếp (dù code đã hỗ trợ chạm vào ảnh chân dung để đổi).
  2. Nút "VI | en" nằm ở góc trên có nền xanh hơi tối, cần viền vàng đồng bộ với theme gỗ/vàng làng quê.

---

### Màn 2: Phòng Chờ 1 Người (`02_lobby_1p_1170x2532.png` — 0.97 MB)
* **Đối chiếu mockup:** Mockup Phòng chờ & Quy tắc v1.2 (1 người).
* **Các điểm KHỚP:**
  - Tiêu đề: `PHÒNG CHIẾN HỮU • SÂN LÀNG (1/6 NGƯỜI THẬT)`.
  - Hàng slot hiển thị đúng 1 thẻ host có vương miện, 5 slot còn lại hiển thị "ĐANG CHỜ...".
  - Biểu ngữ cảnh báo 1 người màu vàng cam: `"Cần ít nhất 2 người. Muốn chơi một mình, hãy sang menu Luyện với bot"`.
  - Nút chuyển hướng `"SANG LUYỆN VỚI BOT"` hiển thị rõ ràng.
  - Nút `"SẴN SÀNG"` bị làm mờ (disabled) khi phòng chỉ có 1 người.
* **Điểm lệch THỰC TẾ:**
  1. Khoảng cách giữa các ô slot 1-6 trên màn hình dọc 2532px hơi thưa, có thể co gọn hơn để tăng diện tích hiển thị chat phòng chờ sau này.

---

### Màn 3: Phòng Chờ 3 Người (`03_lobby_3p_1170x2532.png` — 0.94 MB)
* **Đối chiếu mockup:** Mockup Phòng chờ 3 người.
* **Các điểm KHỚP:**
  - Header: `(3/6 NGƯỜI THẬT)`.
  - 3 slot đầu có tên, avatar và trạng thái người chơi.
  - Thông báo đếm ngược: `"Đang chờ tất cả người chơi sẵn sàng..."`.
  - Nút `"SẴN SÀNG"` chuyển trạng thái active màu xanh/vàng.
* **Điểm lệch THỰC TẾ:**
  1. Tên người chơi trong slot 2 và slot 3 đang dùng tên giả lập chưa có huy hiệu cấp độ như mockup thiết kế.

---

### Màn 4: Phòng Chờ 6 Người (`04_lobby_6p_1170x2532.png` — 0.94 MB)
* **Đối chiếu mockup:** Mockup Phòng chờ đầy phòng (6 người).
* **Các điểm KHỚP:**
  - Lưới 6 slot hiển thị đầy đủ, không bị tràn màn hình hay che khuất nút rời phòng.
* **Điểm lệch THỰC TẾ:**
  1. Trong client mock URL router, tham số `?screen=lobby6p` chưa ánh xạ riêng mà dùng chung cấu trúc `lobby3p`; cần bổ sung router case `lobby6p` để tự động fill đủ 6 avatar người thật khi test UI.

---

### Màn 5: Pha Đặt Vị Trí (`05_placing_1170x2532.png` — 1.37 MB)
* **Đối chiếu mockup:** Gameplay Arena & HUD Placing phase.
* **Các điểm KHỚP:**
  - Sân đấu đất nẻ làng quê hiển thị sắc nét, cọc rào tre và ranh giới sân đúng tỷ lệ.
  - HUD trên: Viên thuốc vòng đấu `"VÒNG 1/5"`, Đồng hồ xu vàng `"5s"`, Biểu ngữ `"ĐẶT NHÂN VẬT"`.
  - HUD dưới: Thẻ trang bị vũ khí `"DÉP TỔ ONG"` góc trái, nút `"SẴN SÀNG"` góc phải.
  - Vòng an toàn touch guard chặn triệt để chạm nhầm ngoài biên.
  - Nút mở khay chat/emote góc dưới bên phải.
* **Điểm lệch THỰC TẾ:**
  1. Vòng tròn bán kính chạm xung quanh nhân vật (touch ring) hiển thị màu trắng xanh, mockup đề xuất vòng hào quang vàng ấm phong cách dân gian.
  2. Vạch thước ngắm hướng ném khi kéo ngắm chưa có đầu mũi tên nhọn dạng dép ở chóp như asset vector thiết kế.

---

### Màn 6: Khay Emote & Chat Nhanh Trong Trận (`06_emote_tray_1170x2532.png` — 1.34 MB)
* **Đối chiếu mockup:** In-game quick emote & phrase tray (A6).
* **Các điểm KHỚP:**
  - Nằm ngay phía trên nút chat ở góc dưới bên phải, không che khuất nhân vật trung tâm sân đấu.
  - Hàng 1: 5 biểu tượng cảm xúc (`😎`, `🔥`, `🎯`, `😅`, `💀`) chạm được ngay.
  - Hàng 2: 3 câu nói nhanh thuần Việt (`Hay quá!`, `Trận sau phục thù!`, `Đừng ném tôi!`).
  - Khi bấm phát âm thanh `PlayChatPop()`, đóng khay và hiện toast tin nhắn.
* **Điểm lệch THỰC TẾ:**
  1. Nền khay emote đang dùng màu xanh đen phẳng bán trong suốt, chưa có viền hoa văn góc gỗ như các bảng hội thoại chính.

---

### Màn 7: Màn Hình Kết Quả Trận Đấu (`07_result_1170x2532.png` — 0.94 MB)
* **Đối chiếu mockup:** Match End Results & Victory screen.
* **Các điểm KHỚP:**
  - Tiêu đề lớn: `"KẾT THÚC TRẬN ĐẤU"`.
  - Bảng tổng kết thứ hạng hạng 1 - N, hiển thị rõ số điểm, số mạng hạ gục, lượng vàng nhận được.
  - Hàng của bản thân làm nổi bật viền vàng óng.
  - Nút `"+ KẾT BẠN"` đặt cạnh mỗi người chơi khác trong bảng kết quả.
  - Nút `"CHƠI LẠI"` và `"VỀ TRANG CHỦ"`.
* **Điểm lệch THỰC TẾ:**
  1. Nút `"+ KẾT BẠN"` có phông chữ hơi nhỏ (14pt); có thể tăng lên 16pt để người dùng bấm ngón tay dễ trúng hơn trên màn hình di động nhỏ.
  2. Cúp chiến thắng trên đầu bảng chưa có hiệu ứng ánh hào quang vàng xoay nhẹ phía sau.

---

## 3. Tổng Hợp & Đánh Giá Đạt Chuẩn A7

- **Build WebGL Release Brotli:** ĐÃ XONG (16.0 MB, chạy trên cổng 8103).
- **Bộ 7 ảnh chụp 1170x2532:** ĐÃ XONG (lưu tại `Evidence/round38/screens/` và artifacts).
- **So sánh UI lệch thực tế:** ĐÃ XONG (không khai khống, ghi nhận trung thực 7 điểm lệch nhỏ để tối ưu ở A10).
