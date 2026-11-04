# HIT ME – xem trước giao diện web

Đây là bản **mock tĩnh** để duyệt giao diện trên điện thoại. Nó chưa nối Unity, phòng online, tài khoản hay server thật. Bản WebGL đang chạy tại `hitmegame-iota.vercel.app` vẫn là giao diện Unity cũ.

## Chạy tại máy

Trong worktree `prep/codex`, chạy `python tools/blackbox/build_webui_preview.py`, rồi `python -m http.server 8765 --directory web-ui/dist`. Mở `http://localhost:8765/?mock=1&scene=home`.

## Tạo trang xem trước riêng trên Vercel

Tạo **project Vercel thứ hai**, import repository `hitmegame`, chọn branch `prep/codex`, Root Directory `web-ui/dist`, Framework Preset `Other`; không cần build command. Giữ nguyên project WebGL hiện có. Sau khi deploy, thêm `?mock=1&scene=home` vào URL mới. Chưa có URL công khai cho tới khi project thứ hai được tạo.

Các scene mock: `home`, `bot`, `lobby`, `match`, `result`, `settings`, `error`. Ví dụ `?mock=1&scene=bot`; đổi `scene=` để xem màn khác. Thêm `&debug=viewport` để hiện vùng nhìn thấy, DPR, canvas và safe area. Bản xem trước dùng phông trong `web-ui/dist/assets/fonts`, SVG ở mã giao diện và âm thanh trong `web-ui/dist/assets/audio`; âm thanh cần một lần chạm để trình duyệt cho phát. Không dùng bản mock để đánh giá luật, input trận hoặc đồng bộ server.

## Kiểm chứng và giới hạn

- Bản Unity hiện hành được đo tại sáu khung, chính là `iphone15pro_safari` **393×695 CSS px, DPR 3**. Bộ kiểm trả lỗi có chủ đích: nút chính trắng, nút phụ thiếu màu, logo xám trắng, avatar sai, tiêu đề và nút của Chọn bot bị cắt. Dữ liệu: `Evidence/blackbox/20261104_round_viewport_live/audit.json` và ảnh cùng thư mục. Sau khi bỏ qua hướng dẫn, phép quét trận phát hiện dải tối hai bên (RGB trung bình 25,3), vệt kéo giãn ở đáy, ô vuông trắng biểu cảm, hai sprite bot chồng nhau 26,5% diện tích nhỏ hơn. Ảnh ở `Evidence/blackbox/20261104_round_viewport_match/`. Toast đè nhân vật thấy được trên ảnh Reveal, chưa có bài kiểm định lượng. Độ tương phản chữ chưa được xác nhận vì chưa có OCR/vị trí từng glyph.
- Home web và sáu scene mock được đo tại sáu khung bằng WebKit; Home còn được kiểm bằng Chromium. Ảnh và số đo: `Evidence/blackbox/20261104_webui_dist_home/` và `Evidence/blackbox/20261104_webui_dist_preview/`. Đây là kiểm tra trình duyệt trên máy tính, **chưa** là ảnh chụp Safari trên iPhone thật.
- Logo, nền Home và avatar hiện là bản tạm từ asset có sẵn. Cần chủ dự án duyệt U1/U3/U4 và AG tích hợp cầu nối Unity trước khi thay giao diện thật. PWA, kiểm tra trận thật, VFX và tích hợp Unity đang chờ bước sau.

Lưu ý: nếu sau này cài bản PWA vào Màn hình chính, dữ liệu lưu trong trình duyệt của PWA có thể khác Safari thường; tài khoản khách có thể không tự chuyển qua.
