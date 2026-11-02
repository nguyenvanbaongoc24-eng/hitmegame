# SO SÁNH GIAO DIỆN E2E TRÊN MOBILE (UI_DIFF_E2E.md) — PHASE 3B

**Ngày lập:** 02/11/2026  
**Độ phân giải thử nghiệm:** iPhone 14/15 (390×844 @3x) và Android Pixel 7 (360×800 @2.75x)

---

## 1. Kết Quả Rà Soát Bố Cục Responsive

| Màn hình | Viewport 390×844 (iOS) | Viewport 360×800 (Android) | Đánh giá |
|---|---|---|---|
| **Home** | Căn giữa hoàn hảo, thanh tài nguyên nằm trọn dưới tai thỏ, 4 nút CTA hiển thị rõ nét | Bố cục co giãn tự động theo tỷ lệ 9:19.5, không bị tràn viền | **ĐẠT** |
| **Lobby 1-6 người** | 6 ô slot xếp 2 cột ngay ngắn, nút Sẵn sàng/Bắt đầu to rõ, biểu ngữ 1 người nổi bật | Font chữ co tỷ lệ vừa vặn, không đè nút rời phòng | **ĐẠT** |
| **Placing (5s đặt)** | Sân đấu đất nẻ hiển thị trọn vẹn, HUD trên/dưới nằm trong vùng an toàn | Touch ring và vạch thước ngắm mũi tên sắc nét | **ĐẠT** |
| **Result** | Bảng xếp hạng 1-6 cuộn mượt, nút Kết bạn 16pt bấm chính xác, cúp có hào quang xoay | Hiển thị đầy đủ số mạng hạ gục và HP còn lại | **ĐẠT** |

## 2. Quét Console Trình Duyệt

- **Số lỗi JS / WebGL unhandled exception:** **0 lỗi**.
- **Cảnh báo (Warnings):** Không có cảnh báo nghiêm trọng.
