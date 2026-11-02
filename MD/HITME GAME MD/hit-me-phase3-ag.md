# HIT ME – PHASE 3: NỐI CLIENT VỚI SERVER THẬT

Đầu vào Antigravity: A7–A10 xong, 148/148 test, `IGameClient` + `WebSocketGameClient` đã có, Release Brotli HOME_SHOWN 5,98 giây (mới vừa dưới mốc 6 giây, đo trên mạng giả lập cục bộ, chưa đo trên điện thoại).

## Kiểm cổng trước khi dán prompt

| Điều kiện | Trạng thái |
|---|---|
| Antigravity A7–A10 | Xong |
| Codex lượt 4 (C10b client giả, C12 token khách, C5c) | **Chưa thấy báo cáo** |

Nếu Codex chưa xong lượt 4, vẫn chạy được Phase 3 song song: Antigravity nối vào server hiện có (132 test) và ghi mọi chỗ lệch giao thức vào `MD/PROTOCOL_MISMATCH.md`; Codex sửa theo danh sách đó. Hai bên không sửa file của nhau. Nếu Codex đã xong lượt 4, gửi mình báo cáo để cập nhật.

## Việc bạn làm tay

1. Chạy server từ thư mục `worktrees/codex-prep` (lệnh có trong `server/README.md`) và để cửa sổ đó mở khi Antigravity thử.
2. Máy tính và điện thoại cùng mạng Wi-Fi; ghi lại địa chỉ IP LAN của máy tính cho bước thử trên điện thoại.
3. Khi tới bước thử 2 điện thoại + 1 máy tính, làm theo danh sách kiểm ở cuối file và gửi mình kết quả.

## PROMPT DÁN VÀO ANTIGRAVITY

```
Tiếp tục từ commit 007e947. Đọc: MD/01_GAMEPLAY_RULES.md (v1.2), MD/DECISIONS.md, MD/NET_PROTOCOL.md (cập nhật mới nhất từ nhánh prep/codex bằng git show prep/codex:MD/NET_PROTOCOL.md, chỉ lấy tệp tài liệu, không merge nhánh). Không sửa code server, không xóa asset/ và SFX/. Luật ưu tiên: luật trong repo (maxRounds=5, ném dừng ở người đầu tiên, phòng 1–6, không bot trong phòng online).

P3-1. Kết nối thật cục bộ
- Chạy server từ worktrees/codex-prep, bật cờ WebSocket, trỏ client tới ws://localhost cổng đúng. Làm từng bước và kiểm: kết nối + token khách, tạo phòng (hiện mã), vào phòng bằng mã, lobby cập nhật đủ 1–6 người, bắt đầu trận khi >= 2 người sẵn sàng, từ chối khi chỉ 1 người.
- Dùng FakeClient của Codex (Tools/FakeClient, nếu đã có; nếu chưa thì mở tab trình duyệt thứ hai làm người chơi thứ hai) để có đủ người.

P3-2. Chơi trọn một trận qua mạng
- Cửa sổ 5s: gửi vị trí + hướng ném theo luật v1.2 (chạm đầu tiên đặt, giữ và trượt để ngắm, chạm chỗ khác thì đổi, mặc định Up, không chạm thì vị trí vòng trước). Nhận công bố đồng thời, phát animation ném/trúng/loại theo SỰ KIỆN SERVER (không tự tính kết quả phía client), kết thúc trận, Result đúng hạng và vàng.
- Ba trận liên tiếp với 2, 3 và 6 người: ghi lại sự kiện và so với kết quả server.

P3-3. Tình huống lỗi
- Rớt mạng và vào lại (trong và ngoài thời gian ân hạn), session_replaced (mở tab thứ hai cùng tài khoản), phòng đầy, bị cấm AFK 120 giây, token sai, server tắt giữa trận. Mỗi tình huống có thông báo tiếng Việt/Anh rõ ràng và người dùng quay về được Home, không treo màn hình.

P3-4. Chat, emote, kết bạn qua mạng
- Nối khay emote/câu mẫu và nút KẾT BẠN theo NET_PROTOCOL.md. Nếu server chưa có message tương ứng thì ghi vào PROTOCOL_MISMATCH.md và giữ chế độ cục bộ, đừng bịa message.

P3-5. Thử trên điện thoại + build
- Build WebGL Release Brotli, chạy trên iPhone Safari qua Wi-Fi LAN (ws:// tới IP máy tính). Lưu ý: trang tải qua https không gọi được ws:// (mixed content); kiểm và ghi cách xử lý (wss:// khi lên Render/Vercel, ws:// khi thử LAN từ http).
- Chụp màn hình lobby + trận + result trên điện thoại vào Evidence/round40/screens/.

P3-6. Dọn các điểm lệch mockup còn mở
- Đầu mũi tên dép trên thước ngắm; hào quang xoay sau Cúp chiến thắng (theo UI_DIFF.md).

TỆP GHI LỖI: mọi chỗ client và server lệch nhau (tên message, thứ tự, trường thiếu, mã lỗi) ghi vào MD/PROTOCOL_MISMATCH.md theo bảng: message | server gửi/nhận thực tế | NET_PROTOCOL.md nói | đề xuất sửa phía nào.

QUY TẮC: chạy toàn bộ test 1 lần mỗi nhiệm vụ; build WebGL không quá 1 lần cho mỗi 2 nhiệm vụ trừ P3-5; commit riêng từng nhiệm vụ; cập nhật MD/STATUS.md; không ghi "xong" khi chưa chạy thật và có bằng chứng; hết quota thì commit phần chạy được và ghi phần dở.

BÁO CÁO CUỐI (tối đa 12 dòng): P3-1…P3-6 xong/dở/chặn, số test, commit hash, số mục trong PROTOCOL_MISMATCH.md, lỗi nặng nhất còn mở.
```

## PROMPT NGẮN CHO CODEX (dán sau khi Antigravity báo PROTOCOL_MISMATCH.md)

```
Đọc MD/PROTOCOL_MISMATCH.md do Antigravity lập (lấy bằng git show từ nhánh Unity chính, chỉ tệp đó). Với từng mục: quyết định sửa phía server hay sửa tài liệu NET_PROTOCOL.md, sửa, thêm test, commit riêng. Không đổi luật chơi. Nếu mục nào cần client đổi, ghi lại thành danh sách ngắn cho Antigravity. Chạy toàn bộ suite 1 lần ở cuối. Báo cáo tối đa 10 dòng.
```

## Danh sách kiểm tay: 2 điện thoại + 1 máy tính cùng một phòng

- [ ] Cả ba thiết bị vào được phòng bằng mã
- [ ] Lobby hiện đủ 3 người, tên đúng
- [ ] Bắt đầu trận khi đủ người sẵn sàng; người ra khỏi phòng thì lobby cập nhật
- [ ] Trận chạy hết, hạng và vàng ba máy giống nhau
- [ ] Tắt Wi-Fi một điện thoại 10 giây rồi bật lại: vào lại trận được
- [ ] Mở tab thứ hai cùng tài khoản: tab cũ nhận session_replaced
- [ ] Emote và chat hiện trên cả ba máy
- [ ] Một mình bấm Tạo phòng rồi Bắt đầu: bị chặn và gợi ý Luyện với bot
