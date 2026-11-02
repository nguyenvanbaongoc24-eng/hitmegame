# HIT ME – PHASE 4, LƯỢT 3 (ANTIGRAVITY)

Đầu vào: commit `8265f9f`, 164/164 EditMode test. K2-C (3 người thật + 3 script, 3 trận) xong; cửa hàng, hồ sơ, đăng nhập (giao diện), rank, nhiệm vụ ngày, âm thanh xong ở mức cục bộ.

## Đường găng hiện tại (không nằm ở Antigravity)

Antigravity đang đi trước. Phần còn chặn game online đầy đủ là:

| Chặn | Ai gỡ |
|---|---|
| Codex chưa chạy lượt 5 (nhánh `prep/codex` vẫn ở `1c447e2`): thiếu protocol v2, thông tin hồ sơ/ví từ server, cửa hàng server | Bạn cho Codex chạy `hit-me-codex-luot-5.md` |
| Render `/healthz` vẫn timeout | Bạn kiểm tra Render (và Codex rà ở R1) |
| Mật khẩu database đã lộ trong chat | Bạn đổi trong Supabase |
| Ảnh vũ khí thiếu | Bạn gửi `MISSING_ASSETS.md` để mình viết bộ prompt tạo ảnh |

Vì vậy lượt này Antigravity làm những việc **không phụ thuộc Codex** nhưng bắt buộc phải có trước khi cho người thật chơi.

## Việc bạn làm tay

1. Cho Codex chạy `hit-me-codex-luot-5.md` ngay (ưu tiên R1, R4, R5).
2. Đổi mật khẩu database Supabase; cập nhật `.env` và Render.
3. Kiểm tra Render: mở `https://hitmegame.onrender.com/healthz`, đợi 1–2 phút, xem Logs.
4. Gửi mình nội dung `Evidence/round42/MISSING_ASSETS.md`.
5. Chuẩn bị tài khoản Vercel (đăng nhập bằng GitHub); chưa cần deploy ngay.

## PROMPT DÁN VÀO ANTIGRAVITY

```
Tiếp tục từ commit 8265f9f. Luật ưu tiên: luật trong repo (maxRounds=5, ném dừng ở người đầu tiên, phòng 1–6, không bot trong phòng online). Không sửa code server. Không xóa asset/ và SFX/. Mọi "đạt" kèm bằng chứng; fail thì ghi fail.

L1. Dọn nội dung không được lộ ra người chơi
- Toast đăng nhập đang ghi "Sắp ra mắt trong Phase 5!": sửa thành "Sắp ra mắt" (VI/EN), không lộ tên giai đoạn dự án. Quét toàn bộ chuỗi giao diện tìm "Phase", "TODO", "mock", "debug", "test" và sửa.
- Bảng xếp hạng top 5 giả, ví vàng cục bộ, rank cục bộ: chỉ dùng ở chế độ Luyện với bot/ngoại tuyến. Ở chế độ online, nếu chưa có dữ liệu server thì hiển thị trạng thái "Đang tải"/"Chưa có dữ liệu", KHÔNG hiện số giả như thật. Thêm test.

L2. Ngân sách hiệu năng cho máy yếu (người dùng không có thiết bị thật)
- Trong Playwright dùng CDP: CPU throttle 4x và 6x, mạng 4G chậm. Đo FPS trung vị và 1% thấp nhất trong trận 4 người ở Placing, Reveal/Throw và Result; đo thời gian đến HOME_SHOWN; đo bộ nhớ Wasm.
- Mục tiêu đề xuất: ≥ 30 FPS ở CPU 4x, ≥ 20 FPS ở 6x, HOME_SHOWN Release ≤ 8 giây trên 4G chậm. Nếu không đạt, tìm 3 điểm nghẽn lớn nhất (draw call, kích thước texture, hiệu ứng hạt, cập nhật UI mỗi khung), sửa, đo lại; ghi bảng trước/sau vào Evidence/round43/L2_perf.md. Không giảm chất lượng đến mức lệch mockup; có tùy chọn "Chất lượng thấp" trong Cài đặt nếu cần.

L3. Gói triển khai Vercel
- Tạo Tools/deploy_web/ với script đóng gói thư mục tĩnh từ build Release Brotli và vercel.json: header Content-Encoding: br cho *.br, Content-Type đúng cho .wasm/.data/.js, cache dài cho tệp có hash, không cache index.html; kiểm tra Unity WebGL tải được với header đó (dùng máy chủ tĩnh cục bộ giả lập đúng header và chạy lại kịch bản E2E 1 trên đó).
- Trang index: meta viewport, viewport-fit=cover, vùng an toàn tai thỏ, chặn zoom/kéo-làm-mới trên mobile, màn hình tải có thanh tiến độ và chữ Việt, xử lý khi WebGL không được hỗ trợ.
- Địa chỉ máy chủ đọc từ cấu hình lúc chạy (config.json cạnh index.html hoặc tham số URL), không hard-code; mặc định wss://hitmegame.onrender.com/ws.
- Viết MD/DEPLOY_VERCEL.md: từng bước bấm cho người dùng (kết nối repo hoặc dùng vercel CLI), biến cấu hình, cách kiểm. Chưa deploy thật.

L4. Hướng dẫn người chơi mới và bot
- Lần đầu mở game: hướng dẫn tương tác ngắn (đặt nhân vật, giữ và trượt để ngắm, thấy kết quả) trong một trận luyện tập 1 vòng với bot, có thể bỏ qua, lưu cờ đã xem. Chữ ngắn, nói bằng hình, VI/EN.
- Bot ở chế độ Luyện: 3 mức (Dễ/Thường/Khó) đọc từ tệp cấu hình; Dễ nhắm lệch, Khó đoán vị trí tốt hơn nhưng không biết vị trí thật của người chơi. Test tất định bằng seed.

L5. Cảm giác chơi (game feel)
- Rung nhẹ camera khi trúng đòn, hit-stop ngắn, số sát thương/trái tim bay, vệt vũ khí khi ném, dừng khựng hợp lý ở mép sân, phản ứng ngã/loại có trọng lượng. Mọi hiệu ứng có thể tắt (Cài đặt) và không làm tụt FPS dưới ngân sách L2. Chụp ảnh/video ngắn trước và sau vào Evidence/round43/L5.

L6. Sẵn sàng nối dữ liệu server
- Tạo IAccountService (hồ sơ, ví, vật phẩm, rank, nhiệm vụ) với FakeAccountService (cục bộ) và ServerAccountService (stub theo dạng message hồ sơ đề xuất trong MD/PROFILE_CONTRACT.md do bạn viết ngay bây giờ, ghi rõ trường nào chờ Codex xác nhận). Khi Codex bàn giao MD/NET_PROTOCOL.md mới (R5), khớp lại và ghi chênh lệch vào MD/PROTOCOL_MISMATCH.md. Mua/trang bị ở chế độ online phải chờ server xác nhận, hiện trạng thái đang xử lý, không trừ vàng cục bộ.

QUY TẮC: chạy toàn bộ test 1 lần mỗi nhiệm vụ; build WebGL không quá 1 lần cho mỗi 2 nhiệm vụ; commit riêng; cập nhật MD/STATUS.md; hết quota thì commit phần chạy được và ghi phần dở. Làm theo thứ tự L1 → L3 → L2 → L6 → L4 → L5 nếu quota ít.

BÁO CÁO CUỐI (tối đa 12 dòng): L1–L6 xong/dở/chặn; bảng FPS/HOME_SHOWN trước-sau; số test; commit hash; có phát hiện nào về dữ liệu giả hiện như thật không.
```

## Sau lượt này

Khi Codex xong R1, R4, R5 và Render phản hồi, Antigravity làm: nối protocol v2, đọc số dư và vật phẩm từ server, chạy lại E2E online với Supabase, rồi chạy K7 trên Render. Đạt hết thì mở Phase 5: mời 5–10 người quen thử trên điện thoại.
