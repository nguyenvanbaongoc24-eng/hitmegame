# HIT ME – LƯỢT 4 CHO CODEX (chốt server để sang Phase 3)

Đầu vào: nhánh `prep/codex`, commit `679b9f0`, server suite 132/132.

## Còn thiếu sau lượt 3

| Mục | Tình trạng |
|---|---|
| C5b adapter Supabase | Báo cáo lượt trước không nhắc tới → phải báo rõ xong/dở |
| C9 cộng vàng vào ví thật | Chưa; chờ định danh người chơi + Supabase |
| C10 client giả + 3 trận kèm vàng | Chưa xong; đây là cửa ải để Unity nối vào |
| C8 cấm AFK dùng chung | Mới lưu tệp, chưa dùng được khi chạy trên Render |
| Xác thực | Server chỉ nhận khách; chưa có token bền vững |

## Việc bạn làm tay (không cần Codex)

1. Mở Supabase → SQL Editor, chạy file migration trong `Tools/supabase/` (Codex sẽ chỉ rõ tên file ở C12), xác nhận các bảng xuất hiện.
2. Lấy **service key** (Settings → API). Đặt vào biến môi trường của Render (SUPABASE_SERVICE_KEY) và vào `.env` cục bộ. Không dán vào chat, không commit.
3. Thêm `__pycache__/` vào `.gitignore` (hoặc xóa thủ công thư mục cache Python còn sót trong worktree).
4. Sửa `maxRounds=20` → `5` trong `MD/DECISIONS.md` khi Antigravity dừng làm việc.
5. Gửi mình báo cáo của Antigravity (A1–A7 đến đâu) để giao việc nối client.

## PROMPT DÁN VÀO CODEX

```
Tiếp tục nhánh prep/codex từ commit 679b9f0. Luật ưu tiên: luật trong repo (maxRounds=5, ném dừng ở người đầu tiên, phòng 1–6, không bot trong phòng online). Không sửa Core, không sửa Unity, không sửa MD/DECISIONS.md chính. Không in hoặc ghi khóa vào log/commit.

BÁO CÁO NGAY ĐẦU: trạng thái C5b (SupabaseProfileStore) xong/dở/chặn và lý do.

C10b. Hoàn tất client giả + 3 trận
- Tools/FakeClient: dòng lệnh kết nối WebSocket thật, tạo N người (2–6), mỗi người vào phòng, sẵn sàng, đặt vị trí + hướng ném trong 5s, chơi trọn trận. Chạy 3 trận liên tiếp với 2, 4, 6 người; kiểm kết quả, hạng, thưởng vàng (đọc từ InMemory store) đúng RewardTable. Lưu Evidence/round40/C10_fakeclient.txt.
- Cập nhật MD/NET_PROTOCOL.md cho khớp thực tế (ví dụ message thật lấy từ log của FakeClient).

C12. Định danh người chơi
- Thiết kế và cài đặt: khách nhận token bền vững (guestId + token ký, lưu client), server xác thực token khi kết nối, token sai bị từ chối với mã lỗi rõ. Chuẩn bị chỗ cắm đăng nhập Google/Facebook/Apple sau này (interface IAuthProvider; chưa cài nhà cung cấp thật, ghi vào MD/AUTH_PLAN.md các bước cần làm).
- Rà migration Supabase: liệt kê tên tệp cần chạy, thứ tự, lệnh kiểm tra bảng/chính sách RLS; viết vào server/README.md. Chưa chạy trên project thật.

C5c. Nối Supabase thật (chỉ khi có SUPABASE_URL + SUPABASE_SERVICE_KEY trong môi trường)
- Contract test chạy trên cả InMemory và Supabase; thiếu biến thì Skip có thông báo, không làm hỏng suite.
- Lệnh cấm AFK lưu trong store dùng chung (Supabase) thay vì tệp cục bộ khi có khóa; có chế độ fallback tệp khi không có.
- C9: cộng vàng thật vào ví qua store khi trận kết thúc, idempotent theo matchId, chỉ server được cộng.

Tách rác: thêm __pycache__/ vào .gitignore nếu chưa có; không cố xóa thư mục.

QUY TẮC: chạy toàn bộ suite tối đa 1 lần mỗi nhiệm vụ; không đọc log dài; commit riêng từng nhiệm vụ; cập nhật MD/STATUS.md; không ghi "xong" khi chưa có test chạy thật; hết quota thì commit phần xanh và ghi phần dở.

BÁO CÁO CUỐI (tối đa 12 dòng): C5b, C10b, C12, C5c, C9 xong/dở/chặn, số test, commit hash.
```

## Cổng vào Phase 3 (kiểm khi Codex báo xong)

- [ ] FakeClient chơi trọn 3 trận 2/4/6 người, thưởng đúng
- [ ] NET_PROTOCOL.md khớp thực tế
- [ ] Token khách hoạt động, token sai bị từ chối
- [ ] Antigravity xong A1–A5 (font Việt, Home, luồng phòng với mock, Placing, Result)

Khi bốn mục trên đạt, giao Phase 3: Antigravity nối client với server thật theo NET_PROTOCOL.md, build Release Brotli, thử trên 2 điện thoại + 1 máy tính cùng một phòng.
