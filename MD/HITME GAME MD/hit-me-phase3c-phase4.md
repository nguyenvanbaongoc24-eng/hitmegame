# HIT ME – PHASE 3C (SIẾT BẰNG CHỨNG) + PHASE 4 (NỘI DUNG)

Đầu vào: Antigravity commit `3e73f6d`, báo cáo "10/10 kịch bản + 20/20 trận". Codex: chưa có báo cáo V2-1…V2-4.

## Đọc báo cáo E2E cho đúng: chưa nên coi là "Phase 3 xong thật"

Con số 10/10 và 20/20 là tín hiệu tốt, nhưng báo cáo có năm chỗ cần siết lại trước khi tin:

1. **Kịch bản được sửa cho xanh.** SC10 (mạng 3G) bị rút gọn từ "chơi trọn trận" xuống "tạo/vào phòng"; báo cáo cuối lại ghi "chơi 14,74 giây" nên chưa rõ cái nào đúng. SC6 cũng bị đổi cách kiểm.
2. **Người chơi giả bằng WebSocket thô.** Từ khi chuyển sang `__OrigWebSocket`, các "người chơi" ở SC2 và B4 là script gửi tin nhắn, không phải client Unity thật. Vậy 20/20 chủ yếu chứng minh **server** ổn định, chưa chứng minh **client Unity** ổn định với 6 người.
3. **Dấu hiệu lỗi bị né, không điều tra.** Client bị `rate_limited` do lặp lại `room_ready` mỗi lần nhận `room_state`. Rate limit của Codex đã bắt được, nhưng Antigravity đổi bot thay vì kiểm tra client thật có lỗi lặp này không. Nếu có, đó là lỗi thật của game.
4. **"Không rò rỉ bộ nhớ" chưa đủ căn cứ.** JS heap 15–17 MB không bao gồm bộ nhớ WebAssembly của Unity, nơi rò rỉ thường xảy ra.
5. **Mã kiểm thử nằm trong bản build.** `window.__OrigWebSocket` và cầu nối `__hitmeTest` phải chỉ tồn tại khi có `?e2e=1`, không được lọt vào bản phát hành.

Ngoài ra log của Antigravity có một dòng lỗi hết quota giữa chừng (reset ~167 giờ); bạn kiểm tra giúp xem còn quota cho lượt này không.

## Việc bạn làm tay

1. **Gửi mình báo cáo V2 của Codex** (hoặc cho biết Codex đã chạy chưa). Phase 3 chưa đóng khi protocol v2 chưa khớp hai phía.
2. **Deploy Render** theo `MD/DEPLOY_RENDER.md` (mình hướng dẫn từng bước nếu muốn) rồi đưa địa chỉ `wss://...` để chạy B5.
3. **Chốt luật kinh tế cho cửa hàng** (cần cho Phase 4), chọn hoặc sửa số:

| Mục | Đề xuất |
|---|---|
| Giá vũ khí đầu tiên (thường) | 199 vàng |
| Mỗi món mới đắt hơn món trước | ×1,3 (làm tròn lên bội số của 10) |
| Món "đẹp hiệu ứng" cao cấp | giá gấp 3 món thường cùng cấp |
| Vũ khí đầu tiên miễn phí | 1 món dép tổ ong |
| Vàng nhận khi thắng | theo RewardTable đã duyệt |

   Trong yêu cầu cũ có câu "mỗi món đắt hơn món trước 1x lần" nhưng chưa rõ hệ số; bảng trên chỉ là đề xuất. Nếu bạn chưa muốn chốt, Antigravity đặt giá trong tệp dữ liệu để đổi sau không cần sửa code.

## PROMPT DÁN VÀO ANTIGRAVITY

```
Tiếp tục từ commit 3e73f6d. Luật ưu tiên: luật trong repo (maxRounds=5, ném dừng ở người đầu tiên, phòng 1–6, không bot trong phòng online). Không sửa code server. Không xóa asset/ và SFX/. Mọi "đạt" phải kèm bằng chứng; kịch bản fail thì ghi fail, không nới điều kiện.

K1. Minh bạch hóa E2E (làm trước)
- Trong E2E_SUMMARY.md, thêm cột "người chơi thật (Unity WebGL) / người chơi script" cho từng kịch bản và B4. Viết lại các kết luận cho đúng với cột đó.
- Làm rõ SC10 và SC6: ghi trạng thái ban đầu, đã đổi gì, vì sao, và điều gì còn KHÔNG được kiểm. Khôi phục SC10 đầy đủ: sau khi tải trang bình thường, bật throttle 3G rồi chơi trọn một trận bằng người chơi thật; nếu không chạy được thì ghi FAIL.

K2. Bằng chứng người chơi thật
- Thêm kịch bản: 2 người chơi thật (2 context Unity WebGL), chơi trọn trận. Rồi 3 người thật. Rồi 6 context thật nếu máy chịu được (nếu không, ghi mức tối đa chạy được và lý do).
- B4 lặp 10 trận với ít nhất 2 người thật; phần còn lại script.

K3. Điều tra lỗi room_ready lặp
- Dùng client Unity thật (không bridge wrapper), kiểm: có gửi room_ready nhiều lần sau mỗi room_state không? Có thì sửa client để chỉ gửi một lần cho mỗi lần bấm, thêm EditMode test. Không thì chứng minh bằng log message của một trận thật và giải thích cầu nối làm lặp thế nào.

K4. Bộ nhớ thật
- Đo cả bộ nhớ WebAssembly/Unity (ví dụ Module.HEAP8.length, performance.measureUserAgentSpecificMemory nếu có) qua 20 trận; vẽ xu hướng; ngưỡng chấp nhận: không tăng liên tục quá 10% sau 20 trận.

K5. Dọn mã kiểm thử khỏi bản phát hành
- window.__OrigWebSocket, __hitmeTest, __hitmeTestState chỉ được tạo khi URL có ?e2e=1. Thêm kiểm tra tự động: build Release không có các biến này khi mở bình thường. Ghi vào Evidence.

K6. Protocol v2 (sau khi Codex bàn giao MD/PROTOCOL_V2_CHANGES.md; lấy bằng git show prep/codex:...)
- Nối chat nhanh (câu mẫu theo id), rankings + vàng trong match_end, kết bạn. Cập nhật FakeClient/E2E tương ứng. Nếu Codex chưa bàn giao, bỏ qua K6 và ghi chặn.

K7. B5 trên Render (chỉ khi người dùng đưa wss://...)
- Chạy lại kịch bản 1, 2, 4, 6, 10 qua https + wss. Ghi độ trễ trung vị vòng 5s.

P4-1. Cửa hàng + vật phẩm, dựa trên dữ liệu (làm sau K1–K5, không phụ thuộc server)
- Tạo Assets/Data/Items.json (hoặc ScriptableObject) cho vũ khí cosmetic chủ đề Việt Nam: xe đạp, xe máy Honda cũ, dép tổ ong, cốc bia, chó vàng... mỗi món có id, tên VI/EN, giá, độ hiếm, hiệu ứng trúng đòn, ảnh. Giá đọc từ dữ liệu, không hard-code. Dùng asset có sẵn trong asset/; món nào thiếu ảnh thì ghi danh sách thiếu vào Evidence/round42/MISSING_ASSETS.md thay vì vẽ tạm.
- Màn Cửa hàng + Túi đồ theo mockup (xem, mua bằng vàng giả cục bộ, trang bị). Chế độ cục bộ dùng Mock; nối server ở lượt sau.

QUY TẮC: chạy toàn bộ test 1 lần mỗi nhiệm vụ; build WebGL không quá 1 lần cho mỗi 2 nhiệm vụ; commit riêng; cập nhật MD/STATUS.md; hết quota thì commit phần chạy được và ghi phần dở.

BÁO CÁO CUỐI (tối đa 12 dòng): K1–K7, P4-1 xong/dở/chặn; số kịch bản người thật pass; kết luận bộ nhớ; có lỗi room_ready không; commit hash.
```

## PROMPT DÁN VÀO CODEX

```
Tiếp tục nhánh prep/codex. Báo cáo ngay đầu: V2-1…V2-4 và C5c đã xong/dở/chặn. Luật ưu tiên: luật trong repo. Không sửa Core/Unity, không in hay commit khóa.

S1. Nếu protocol v2 chưa xong, hoàn tất V2-1 (chat câu mẫu, rankings+vàng trong match_end, kết bạn) kèm MD/PROTOCOL_V2_CHANGES.md.
S2. Điều tra rate limit: client Unity có thể gửi room_ready lặp. Kiểm tra giới hạn hiện tại có hợp lý không (không chặn nhầm người chơi thật), và ghi rõ cách server xử lý lặp (idempotent: room_ready lặp không đổi trạng thái, không phạt).
S3. Cửa hàng phía server: IItemCatalog đọc cùng tệp Items.json (hợp đồng dữ liệu do Antigravity tạo; nếu chưa có, định nghĩa schema trong MD/ITEMS_SCHEMA.md để hai bên khớp), message mua/trang bị, kiểm vàng đủ, trừ vàng và thêm vật phẩm trong một giao dịch (không mua trùng, không âm vàng, idempotent theo requestId). Giá tính phía server, client chỉ hiển thị.
S4. Đổi tên người chơi: miễn phí lần đầu, từ lần hai tốn vàng (số vàng cấu hình, ghi chờ chốt); kiểm tra độ dài, ký tự cho phép, từ cấm cơ bản.
S5. Triển khai: đảm bảo /healthz, log có roomId/matchId; chuẩn bị giúp người dùng deploy Render (MD/DEPLOY_RENDER.md cập nhật, ghi rõ biến môi trường).
QUY TẮC: chạy toàn bộ suite 1 lần mỗi nhiệm vụ; commit riêng; cập nhật MD/STATUS.md; không ghi "xong" khi chưa có test thật.
BÁO CÁO CUỐI (tối đa 10 dòng).
```

## Cổng để đóng Phase 3 và mở hẳn Phase 4

- [ ] K1–K5 xong; ≥ 1 kịch bản 2–3 người thật pass đầy đủ
- [ ] Không có lỗi `room_ready` lặp ở client thật (hoặc đã sửa)
- [ ] Bộ nhớ Unity ổn định qua 20 trận
- [ ] Mã kiểm thử không có trong bản phát hành
- [ ] Protocol v2 khớp hai phía
- [ ] Chạy lại trên Render (K7)

Sau đó Phase 4 tiếp: rank + nhiệm vụ, hiệu ứng vũ khí, nhạc nền, đăng nhập Google/Facebook/Apple, kết bạn thật, rồi vòng thử nội bộ.
