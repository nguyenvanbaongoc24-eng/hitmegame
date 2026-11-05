# HIT ME V7 — QA và giới hạn bằng chứng (2026-11-05)

## Đã chạy

| Kiểm tra | Kết quả | Bằng chứng |
|---|---|---|
| Unity EditMode sau sửa Settings và điều hướng Home | 202/202 đạt, 0 lỗi | `Evidence/reform_v7/editmode_results_nav.xml` |
| WebGL Release | Build thành công, 12.817.441 byte; có `index.html`, `.data.br`, `.framework.js.br`, `.loader.js`, `.wasm.br` | `Evidence/reform_v7/webgl_build_nav.log`, dòng `Release WebGL Build succeeded` |
| Local WebGL trong trình duyệt | Home hiển thị; tab Xếp hạng mở bảng và ghi “Chưa có dữ liệu”; nút Đóng trở về Home. Console không có lỗi nghiêm trọng trong lượt thử. | Build `Builds/WebGL_Release_20261105_4ce207e`, `BuildFingerprint` ghi `4ce207e` |
| Render | `GET /healthz` trả 200; `POST /guest-token` trả 503 khi kiểm lại ngày 2026-11-05. | Lệnh `Invoke-WebRequest` với hai endpoint; phản hồi HTTP 200/503 |

## Chưa đạt / chưa xác nhận

- **Phòng online thật:** endpoint guest token trả 503. Chưa thể xác nhận tạo phòng, vào phòng, ghép nhanh hoặc trận nhiều người trên bản này. Cần cấu hình `GUEST_TOKEN_SIGNING_KEY` trên Render rồi redeploy và thử lại. Không đưa giá trị khóa vào repo hoặc báo cáo.
- **Nhiệm vụ ở footer Home:** listener runtime đã có và EditMode kiểm ánh xạ, nhưng thao tác trong trình duyệt chưa cho bằng chứng UI ổn định; một lượt thử mở hộp Ghép nhanh khi định chạm Nhiệm vụ. Cần thử lại bằng cảm ứng trên thiết bị hoặc PlayMode có đồ họa trước khi đánh dấu đạt.
- **Âm thanh:** đã sửa đường bật âm WebGL độc lập và đồng bộ slider với `AudioManager`; chưa có bằng chứng nghe nhạc/SFX trên iPhone hoặc Android.
- **Mobile:** chưa có kết quả thử bản build `4ce207e` trên Safari/Chrome điện thoại; chưa xác nhận safe area, cảm ứng, bàn phím ảo và xoay màn hình.
- **Deployment:** build ở worktree riêng, chưa triển khai Vercel; không coi bản production là cùng revision. Những tính năng hồ sơ, số dư, cửa hàng, nhiệm vụ và xếp hạng thật chưa được kiểm E2E trên production.

## Phạm vi thực hiện

Sửa tối thiểu lỗi chức năng trong Home, loader, phòng nhập mã, xếp hạng mẫu, âm thanh và Settings. Không chỉnh Core combat hoặc luật trận. Thư mục Unity của Antigravity không bị sửa từ worktree này. Khi tích hợp vào nhánh Unity chính, chạy lại `ProductionUISetup.BuildProductionUI`, toàn bộ EditMode và build WebGL rồi thử tay trên hai điện thoại.
