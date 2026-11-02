# HIT ME — BÁO CÁO KIỂM THỬ THIẾT BỊ DI ĐỘNG & BẢN DỰNG (ROUND 45)

**Thời gian:** 2026-11-02 18:15 UTC+7  
**Bản dựng WebGL:** `WebGL_Release_20261102_188344e`  
**Nhánh Git Vercel:** `web-dist` (Commit: `58895db`)  
**Kết quả Unit Tests:** 182 / 182 PASS (100%)  
**Môi trường kiểm thử:** WebKit Mobile Touch Emulation (Playwright) trên 2 cấu hình thiết bị thực tế:  
1. **iPhone 15 Pro** (393 × 852, DPR 3, Touchscreen)  
2. **iPhone SE** (375 × 667, DPR 2, Touchscreen 16:9)

---

## 1. TỔNG HỢP KẾT QUẢ ĐẠT ĐƯỢC (F0 — F6)

| Mục | Nội dung kiểm thử | Hiện trạng trước | Kết quả sau xử lý | Trạng thái |
| :--- | :--- | :--- | :--- | :---: |
| **F0** | Kiểm thử cảm ứng WebKit thực tế | Dùng giả lập synthetic `__hitmeTest` | Dùng `page.touchscreen.tap` trực tiếp trên canvas WebKit | **ĐẠT** |
| **F1** | Tràn viền 100vw × 100dvh, bỏ viền đen | Bị viền đen hai bên trên iPhone SE / iPad do ép tỉ lệ 9:19.5 | Canvas 100vw × 100dvh trọn màn hình, camera tự co giãn | **ĐẠT** |
| **F2** | Lỗi liệt cảm ứng trên WebKit/Mobile | Chạm vào sân bị nuốt, không di chuyển được nhân vật | Gỡ bỏ `raycastTarget` trên HUD panel, gỡ `preventDefault` | **ĐẠT** |
| **F3** | Tỉ lệ nhân vật & vũ khí, ô xám battle | Nhân vật & dép khổng lồ (2.0f); nút góc dưới có ô xám chưa gán sprite | Nhân vật 1.0f, dép 0.65f; nút cảm xúc tròn có texture | **ĐẠT** |
| **F4** | Nút kéo kép, nút Báo Lỗi đè chữ | Thanh trượt 2 núm; nút Báo Lỗi nằm đè nút Đóng; hiện Rời Trận ở Home | 1 núm chuẩn; Báo Lỗi dòng riêng; ẩn Rời Trận tại Home | **ĐẠT** |
| **F5** | Xếp thẳng hàng Top Bar, bỏ vệt đen sân | Banner HIT ME đè lên tiền; vệt đen cắt ngang nền; dính chữ "Graybox" | Top bar 1 hàng Y=0.965; Banner ở dưới; xóa vệt đen; fingerprint sạch | **ĐẠT** |
| **F6** | Báo cáo kiểm thử & triển khai web-dist | Chưa đẩy bundle mới lên Vercel | Đã đóng gói và đẩy commit `58895db` lên nhánh `web-dist` | **ĐẠT** |

---

## 2. NGUYÊN NHÂN GỐC RỄ LỖI CẢM ỨNG (ROOT CAUSE OF TOUCH FAILURE)

1. **HUD nuốt trọn sự kiện chạm (`GameplayHudPanel`):**
   - Trong `ProductionUISetup.cs`, hàm `CreatePanel` mặc định gán `Image.raycastTarget = true`.
   - Bảng điều khiển trận đấu `GameplayHudPanel` phủ kín 100% diện tích màn hình. Khi người chơi chạm vào bất cứ đâu trên sân cỏ, `EventSystem.current.IsPointerOverGameObject()` luôn trả về `true`, khiến đoạn mã xử lý đặt quân trong `PlacementInput.cs` lập tức hủy lệnh (`continue`/`return`).
   - **Khắc phục:** Chuyển `GameplayHudPanel` và các panel bao phủ toàn màn hình sang `raycastTarget = false`. Chỉ các nút bấm tương tác (`Button`) mới nhận raycast.

2. **Xung đột sự kiện trình duyệt trong `index.html`:**
   - Script khởi tạo WebGL gắn trình lắng nghe `touchstart` / `touchmove` trực tiếp vào `#unity-canvas` với `event.preventDefault()`, vô hiệu hóa pipeline dịch sự kiện của Emscripten WebKit.
   - **Khắc phục:** Loại bỏ chặn sự kiện trên canvas; chỉ chặn cuộn trang ngoài vùng canvas.

---

## 3. BẰNG CHỨNG HÌNH ẢNH (SCREENSHOTS ĐỐI CHIẾU)

### A. Màn hình chính (Home Screen)
- **iPhone 15 Pro:** `Evidence/round45/01_home_iphone_15_pro.png`
- **iPhone SE:** `Evidence/round45/01_home_iphone_se.png`
- *Ghi nhận:* Thanh Top bar (Khách, Vàng, Kim cương, Đổi ngôn ngữ, Nút Cài đặt) thẳng tắp 1 hàng. Banner HIT ME nằm thoáng phía dưới. Vết đen cắt ngang đã biến mất hoàn toàn. Fingerprint hiển thị tinh tế ở mép dưới cùng, không còn chữ "Graybox".

### B. Hộp thoại Cài đặt (Settings Dialog)
- **iPhone 15 Pro:** `Evidence/round45/02_settings_iphone_15_pro.png`
- **iPhone SE:** `Evidence/round45/02_settings_iphone_se.png`
- *Ghi nhận:* Hai thanh âm lượng chỉ có DUY NHẤT 1 núm kéo tròn. Nút BÁO LỖI tách biệt thành hàng riêng chuẩn 9-slice vàng. Nút ĐÓNG màu xanh riêng biệt. Không còn nút "RỜI TRẬN" khi đang ở Home. Số phiên bản hiển thị gọn bên dưới.

### C. Giai đoạn Đặt quân (Placing Phase) & Chiến đấu (Battle)
- **iPhone 15 Pro:** `Evidence/round45/03_gameplay_placing_iphone_15_pro.png` & `04_gameplay_battle_iphone_15_pro.png`
- **iPhone SE:** `Evidence/round45/03_gameplay_placing_iphone_se.png` & `04_gameplay_battle_iphone_se.png`
- *Ghi nhận:* 
  - Toàn bộ sân đình 100% hiển thị không bị viền đen cắt xén.
  - Thao tác cảm ứng `page.touchscreen.tap` đặt chính xác nhân vật vào vị trí chỉ định.
  - Tỉ lệ nhân vật và dép hài hòa 1.0f / 0.65f.
  - Nút cảm xúc góc dưới bên phải hiển thị icon tròn tối màu có viền (`ui_btn_circle_dark`), hoàn toàn không còn ô xám placeholder lỗi texture.

---

## 4. KẾT LUẬN & TRẠNG THÁI TRIỂN KHAI

Toàn bộ các mục từ F0 đến F6 đã hoàn tất 100%, vượt qua tất cả 182 bài unit tests. Mã nguồn và gói triển khai tĩnh đã được đẩy lên nhánh `web-dist` (Commit `58895db`) để Vercel tự động cập nhật bản chơi trực tiếp tại https://hitmegame-iota.vercel.app/.
