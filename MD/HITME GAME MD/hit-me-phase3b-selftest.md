# HIT ME – PHASE 3B: TỰ KIỂM TRA TỰ ĐỘNG (KHÔNG CẦN ĐIỆN THOẠI) + PROTOCOL V2

Đầu vào: Antigravity commit `428d174` (151/151 test), Codex server 139/139, FakeClient chơi đúng 3/3 trận.

## Nhìn thẳng vào mức "xong" hiện tại

Báo cáo Phase 3 ghi "Xong" cho P3-1 → P3-6, nhưng bằng chứng là test EditMode và FakeClient (người chơi giả ở phía server). **Chưa có lần nào bản WebGL thật chạy trong trình duyệt, nối vào server thật, với nhiều người chơi.** P3-5 mới là cấu hình cờ, chưa thử trên thiết bị. Phase 3B bù đúng chỗ đó bằng cách để Antigravity tự điều khiển nhiều trình duyệt giả lập điện thoại.

## Việc bạn làm tay (không cần thiết bị)

1. Làm theo `MD/DEPLOY_RENDER.md` để tạo service server trên Render (hoặc nhờ mình hướng dẫn từng bước). Khi có địa chỉ `wss://...onrender.com/ws`, gửi cho mình hoặc dán cho Antigravity ở bước B5.
2. Tạo project trên Vercel từ repo HIT-ME để có địa chỉ https cho bản WebGL (làm sau khi B1–B4 xong).
3. Chạy migration Supabase và đặt khóa vào biến môi trường Render (vẫn chưa làm; chưa cần cho B1–B4).
4. Khi tới B6, xem video/ảnh Antigravity ghi lại thay cho việc cầm điện thoại.

## PROMPT DÁN VÀO ANTIGRAVITY

```
Tiếp tục từ commit 428d174. Người dùng KHÔNG có thiết bị thật, vì vậy bạn tự kiểm thử bằng trình duyệt giả lập; mọi kết luận "đạt" phải kèm bằng chứng (video, ảnh, log, trace). Không sửa code server. Không xóa asset/ và SFX/. Luật ưu tiên: luật trong repo (maxRounds=5, ném dừng ở người đầu tiên, phòng 1–6, không bot trong phòng online).

B1. Bộ kiểm thử đầu-cuối (E2E) tự động
- Dựng bằng Playwright (npm hoặc pip) trong Tools/e2e/. Chạy bản WebGL Release Brotli thật qua http cục bộ, nhiều BrowserContext song song, mỗi context là một người chơi (khác localStorage → token khách khác nhau), giả lập thiết bị mobile (viewport 390x844, deviceScaleFactor 3, hasTouch, userAgent iPhone Safari; thêm một cấu hình Android).
- Máy chủ lấy từ worktrees/codex-prep (lệnh trong server/README.md). Viết script khởi động/tắt server tự động.
- Điều khiển bằng sự kiện cảm ứng thật (touchstart/move/end) lên canvas: chạm đặt nhân vật, giữ và trượt ngắm, chạm chỗ khác đổi vị trí. Nếu Unity WebGL khó nhận sự kiện tổng hợp, thêm một cầu nối kiểm thử (query ?e2e=1) cho phép script gửi lệnh ý định ("đặt tại x,y; ngắm hướng θ") và đọc trạng thái (màn hình hiện tại, mã phòng, số người, hạng, vàng) qua window.__hitmeTest. Cầu nối chỉ bật khi ?e2e=1.

B2. Kịch bản phải tự động chạy xanh (mỗi kịch bản lưu video + ảnh + log vào Evidence/round41/e2e/)
1. Hai người: tạo phòng, vào bằng mã, sẵn sàng, chơi hết một trận; hạng + vàng hai máy khớp nhau và khớp server.
2. Ba người rồi sáu người (6 context): lobby đủ người, trận trọn vẹn, người thứ 7 bị từ chối.
3. Một mình bấm bắt đầu: bị chặn + gợi ý Luyện với bot.
4. Rớt mạng 10 giây (context.setOffline) rồi vào lại: trở lại trận. Rớt quá thời gian ân hạn: bị coi là rời.
5. Cùng token mở tab thứ hai: tab cũ nhận session_replaced và về Home.
6. AFK: một người không làm gì: bị loại + cấm 120 giây; vào lại phòng trong thời gian cấm bị từ chối với thông báo có số giây; không phải bị cấm khi chỉ rớt mạng.
7. Server tắt giữa trận: mọi client hiện thông báo rõ và về Home, không treo.
8. Token sai/hết hạn: bị từ chối, tự xin token mới hoặc báo lỗi đúng.
9. Emote hiện trên máy người khác.
10. Mạng chậm (throttle 3G giả lập qua CDP): tải đến HOME_SHOWN, vẫn vào phòng và chơi được; ghi thời gian.

B3. Soi lỗi giao diện trong E2E
- Chụp ảnh ở từng màn hình (Home, Lobby 1/3/6, Placing, Reveal, Result) ở viewport 390x844 và 360x800; kiểm không tràn chữ, không đè nút, vùng an toàn (notch) đúng. Dùng chụp ảnh so sánh với ảnh chuẩn trong asset/ nếu làm được; ghi lệch vào Evidence/round41/UI_DIFF_E2E.md.
- Quét console trình duyệt: không có lỗi JS/WebGL/exception chưa xử lý; ghi ngoại lệ vào bảng.

B4. Độ ổn định
- Chạy vòng lặp 20 trận liên tiếp (2–6 người ngẫu nhiên) không treo, không rò rỉ bộ nhớ đáng kể (đo performance.memory nếu có, ghi xu hướng), tỷ lệ pass báo cáo rõ.

B5. Sau khi có địa chỉ Render (chỉ làm khi người dùng đưa wss://...)
- Chạy lại B2 (1, 2, 4, 6, 10) với server trên Render: ?server=wss://... qua trang https (Vercel hoặc server tĩnh có https). Ghi độ trễ trung vị của vòng 5s và thời gian tạo phòng.

B6. Hồ sơ cho người dùng
- Viết Evidence/round41/E2E_SUMMARY.md: bảng 10 kịch bản + kết quả + đường dẫn video; liệt kê rõ phần NÀO CHƯA THỂ kiểm bằng giả lập (cảm ứng đa điểm thật, hiệu năng GPU thật, âm thanh Safari, thanh địa chỉ iOS...) để sau này nhờ người thử máy thật.

QUY TẮC: chạy toàn bộ test 1 lần mỗi nhiệm vụ; build WebGL không quá 1 lần cho mỗi 2 nhiệm vụ; commit riêng từng nhiệm vụ; cập nhật MD/STATUS.md; kịch bản nào fail thì ghi fail và nguyên nhân, KHÔNG nới điều kiện cho xanh; lỗi phía server ghi vào MD/PROTOCOL_MISMATCH.md thay vì tự sửa; hết quota thì commit phần chạy được và ghi phần dở.

BÁO CÁO CUỐI (tối đa 12 dòng): B1–B6 xong/dở/chặn, kịch bản pass/fail (x/10), tỷ lệ pass 20 trận, số lỗi console, commit hash, đường dẫn E2E_SUMMARY.md.
```

## PROMPT DÁN VÀO CODEX (song song, không sửa Unity)

```
Tiếp tục nhánh prep/codex. Đọc MD/PROTOCOL_MISMATCH.md (lấy bằng git show từ nhánh Unity chính, chỉ tệp đó). Luật ưu tiên: luật trong repo (maxRounds=5, ném dừng ở người đầu tiên, phòng 1–6, không bot trong phòng online). Không sửa Core, không sửa Unity, không in khóa/commit khóa.

V2-1. Protocol v2 (tương thích ngược v1): thêm message chat nhanh toàn phòng (câu mẫu theo id, KHÔNG chuỗi tự do, có giới hạn tốc độ), mảng rankings + thưởng vàng trong match_end, yêu cầu/chấp nhận kết bạn (lưu qua IProfileStore). Cập nhật MD/NET_PROTOCOL.md (đánh dấu v2), thêm test, FakeClient hỗ trợ, rồi báo Antigravity bằng danh sách thay đổi tại MD/PROTOCOL_V2_CHANGES.md.
V2-2. Giới hạn an toàn: giới hạn kích thước/tốc độ mọi message, kiểm tra Origin của WebSocket (cấu hình danh sách tên miền cho phép, cho phép localhost khi phát triển), chống tạo phòng hàng loạt từ một token.
V2-3. Nối Supabase thật khi có SUPABASE_URL + SUPABASE_SERVICE_KEY trong môi trường (contract test chạy cả InMemory và Supabase; thiếu biến thì Skip có thông báo): cấm AFK và vàng dùng store chung; cộng vàng idempotent theo matchId.
V2-4. Triển khai: kiểm tra Dockerfile/render.yaml, tạo /healthz trả phiên bản + commit, log có matchId/roomId để truy vết, endpoint chỉ-đọc /metrics đơn giản (số phòng, số kết nối).
QUY TẮC: chạy toàn bộ suite 1 lần mỗi nhiệm vụ; commit riêng; cập nhật MD/STATUS.md; không ghi "xong" khi chưa có test thật.
BÁO CÁO CUỐI (tối đa 10 dòng).
```

## Cổng để coi Phase 3 là xong thật

- [ ] E2E ≥ 9/10 kịch bản pass, mỗi cái có video
- [ ] 20 trận liên tiếp không treo
- [ ] Không còn lỗi console chưa xử lý
- [ ] Protocol v2 khớp ở cả hai phía
- [ ] Đã chạy lại trên server Render (B5)

Khi đủ năm mục, giai đoạn tiếp theo là nội dung game: cửa hàng + vũ khí Việt Nam, rank, nhiệm vụ, âm nhạc. Mình sẽ giao ngay sau đó.
