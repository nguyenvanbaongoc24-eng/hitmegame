# HIT ME — kiểm tra độc lập bản mobile beta.3

Ngày 2026-11-06. URL: `https://hitmegame-iota.vercel.app/next/`. `release.json` ghi `0.1.0-beta.3`, Unity `5e27d8f`, web `3d96cd7`. Bản ổn định `/` vẫn là `0.1.0-beta.1`.

## Bằng chứng đã chạy

`python tools/blackbox/test_next_live.py --url https://hitmegame-iota.vercel.app/next/ --output Evidence/mobile/20261106/next_live_fixed --matches 5 --gameplay` → `deploymentSynced=True; Passed! 6/6`. Bộ kiểm dùng WebKit giả lập các khung 393×695, 393×780, 393×852, 375×553 và Chromium 360×640, 412×780. Ở khung 393×695, 5 trận có bảng xếp hạng 4 người và không có `pageerror`. Tệp kết quả: `Evidence/mobile/20261106/next_live_fixed/results.json`; ảnh Home và Result trong cùng thư mục. Đây là kiểm trên trình duyệt giả lập, chưa phải thử bằng tay trên iPhone.

| ID | Kết quả | Giới hạn / phát hiện |
|---|---|---|
| F1 | ĐẠT trong bộ kiểm | 5 trận và bảng kết quả; chưa quay video đường ném và hiệu ứng trúng. |
| F2 | ĐẠT trong bộ kiểm | 4 nhân vật đang mở có thẻ HUD khớp lựa chọn. |
| F2b | CHƯA ĐẠT | `catalog.json` còn đặt Chảo và Vợt `enabled:false`; chưa thể chứng minh clip và vật bay của hai vũ khí. |
| F3 | ĐẠT | Ba nút online bị khóa đúng trạng thái chưa phát hành. |
| F5 | ĐẠT ở mức thao tác | Pause, Resume, âm lượng phản hồi; chưa kiểm âm thanh phát thực trên iPhone. |
| F6 | ĐẠT một phần | Mở và gửi biểu cảm; chưa kiểm bong bóng trên thẻ và hồi 3 giây. |
| F7 | ĐẠT | Hướng dẫn đầu trận và mở lại. |
| F8 | ĐẠT trong bộ kiểm | Điểm chạm giữa sân đến canvas; chưa kiểm bằng ngón tay thật. |
| HOME-12/13 | CHƯA ĐẠT | Ở ảnh WebKit, tóc nhân vật chạm/đè logo; nhóm nút chật. CSS hiện chồng nhiều quy tắc theo chiều cao. |
| HUD-12..16 | CHƯA ĐẠT | `web-ui/live.js` vẽ hai nút Cài đặt, trạng thái pha lặp, dùng emoji, vùng dưới trống và chưa có câu chat nhanh. |
| ERR-01..03 | CHƯA KIỂM | Ảnh người dùng báo hộp `Script error.` trong trình duyệt Zalo; cần tái hiện có kiểm soát và sửa bộ xử lý ở HTML hybrid. |

## Phân công theo nguồn lỗi

- **Web:** Home logo/nhân vật và khoảng cách, HUD trùng, emote/quick chat, thông báo lỗi trình duyệt trong ứng dụng.
- **Unity:** chữ tên/HP nổi trên đầu nhân vật trong chế độ `ui=web`, clip/hiệu ứng Chảo và Vợt, cầu nối online. Không sửa các mục này trong lượt web.
- **Chưa xác định:** hiệu ứng trúng đòn, nhật ký âm thanh thực, khoảng cách spawn, độ bền PWA. Không xem các mục này là đạt khi chưa có phép đo.

Ghi chú kiểm bản xuất bản: `catalog.json` trong Windows checkout đổi xuống dòng sang CRLF. Bộ kiểm nay so byte Vercel với `git show <webUiCommit>:web-ui/dist/...`; cả sáu tệp khớp, không thay ngưỡng đạt.
