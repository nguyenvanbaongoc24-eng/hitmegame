# HIT ME – LƯỢT 5 CHO CODEX

Đầu vào: nhánh `prep/codex`, commit `1c447e2`. Migration 001–007 áp lên Supabase (ledger 7/7), RLS chặn truy cập ẩn danh vào `leaderboard`, thưởng 150/80/40 đúng và không cộng trùng, 3/3 trận WebSocket thật (2, 4, 6 người), server 140/140, SQL 14/14.

## Làm ngay: bảo mật (bạn tự làm, vài phút)

1. **Đổi mật khẩu database trong Supabase** (Project Settings → Database → Reset database password), vì mật khẩu cũ đã nằm trong lịch sử chat.
2. Cập nhật mật khẩu mới vào `.env` cục bộ và vào biến môi trường của Render. Không dán mật khẩu mới vào chat.
3. Kiểm tra Supabase → Settings → API: nếu bạn từng dán **service key** ở đâu ngoài `.env`, hãy tạo lại khóa. Khóa này bỏ qua toàn bộ RLS, nên coi như chìa khóa vạn năng.
4. Chưa deploy Render xong thì chưa cần làm bước 2 phần Render, nhưng làm trước khi mở cho người ngoài.

## Điều còn thiếu so với báo cáo

| Mục | Tình trạng |
|---|---|
| Protocol v2 (chat câu mẫu, rankings trong match_end, kết bạn) | Báo cáo không nhắc; Antigravity đang chờ `MD/PROTOCOL_V2_CHANGES.md` |
| Adapter cấm AFK trên Supabase thật | Chưa thử |
| 12 ví khách thử trong database | Cần dọn trước khi mở cho người thật |
| Render `/healthz` không phản hồi | Cần Codex rà phía server (xem R1) |
| Unity chưa đọc số dư | Việc của Antigravity (xem cuối file) |

## PROMPT DÁN VÀO CODEX

```
Tiếp tục nhánh prep/codex từ commit 1c447e2. Luật ưu tiên: luật trong repo (maxRounds=5, ném dừng ở người đầu tiên, phòng 1–6, không bot trong phòng online). Không sửa Core/Unity. Tuyệt đối không in, ghi log hay commit mật khẩu/khóa; đọc từ biến môi trường.

BÁO CÁO NGAY ĐẦU: V2-1 (protocol v2) đã xong/dở/chặn; S3 (cửa hàng server), S4 (đổi tên) đã làm chưa.

R1. Rà server cho Render (làm trước)
- URL https://hitmegame.onrender.com/healthz không phản hồi trong 90 giây. Kiểm tra: server có lắng nghe 0.0.0.0 và đọc cổng từ biến PORT (không cố định 8080)? Đường /healthz có tồn tại và trả 200 nhanh không phụ thuộc DB? Dockerfile build/chạy đúng lệnh khởi động? Nếu cần phải sửa, sửa và thêm test. Ghi kết luận + lệnh người dùng chạy để xác nhận vào MD/DEPLOY_RENDER.md.

R2. Dọn dữ liệu thử + bảo mật dữ liệu
- Các ví khách thử cần đánh dấu được: thêm cột/cờ is_test hoặc tiền tố id, script Tools/supabase/cleanup_test_data.sql chỉ xóa bản ghi test (chạy ở chế độ xem trước, in số dòng sẽ xóa, rồi mới xóa khi có tham số xác nhận). KHÔNG tự xóa dữ liệu thật; chỉ báo hướng dẫn cho người dùng chạy.
- Quét repo và lịch sử nhánh prep/codex tìm chuỗi giống mật khẩu/khóa (kể cả trong Evidence/ và log); nếu tìm thấy, liệt kê tệp và commit, KHÔNG in giá trị, và hướng dẫn xóa/viết lại. Thêm .gitignore + hook kiểm tra trước commit chặn tệp .env và chuỗi khóa.

R3. Cấm AFK trên Supabase thật
- Chạy contract test cho adapter cấm AFK với Supabase thật (dữ liệu test có cờ), kiểm: lưu lệnh cấm, hết hạn đúng, server khởi động lại vẫn nhớ, đồng hồ lệch nhẹ vẫn đúng. Evidence/round43/R3_ban_supabase.txt.

R4. Protocol v2 (nếu chưa xong): chat câu mẫu theo id có giới hạn tốc độ, rankings + vàng trong match_end, kết bạn; cập nhật MD/NET_PROTOCOL.md (v2) và viết MD/PROTOCOL_V2_CHANGES.md liệt kê từng thay đổi để Antigravity nối. FakeClient hỗ trợ v2.

R5. Số dư và hồ sơ cho client
- Thêm message hồ sơ: server gửi {tên, vàng, rank, điểm, vật phẩm đã mua, vật phẩm đang dùng} khi kết nối và mỗi khi thay đổi (sau trận, sau mua). Ghi rõ trong NET_PROTOCOL.md. Không tin vàng do client báo.

R6. Cửa hàng + đổi tên (nếu chưa làm): mua/trang bị với giao dịch nguyên tử, idempotent theo requestId, giá tính ở server từ catalog (tệp Items.json do Antigravity tạo trong Assets/Data/; lấy bản mới nhất bằng git show từ nhánh Unity chính, nếu chưa có thì định nghĩa schema trong MD/ITEMS_SCHEMA.md). Đổi tên: miễn phí lần đầu, từ lần hai tốn vàng (cấu hình, ghi "chờ chốt"), kiểm độ dài/ký tự/từ cấm cơ bản.

R7. Rank + nhiệm vụ ngày (T1, T2): điểm rank theo hạng và cỡ trận (bảng đề xuất trong MD/PHASE4_ECONOMY.md, ghi "chờ chốt"), bậc rank đọc cấu hình, bảng xếp hạng top N + hạng người hỏi; 3 nhiệm vụ ngày, tiến độ từ sự kiện trận trên server, nhận thưởng một lần, đặt lại 00:00 giờ Việt Nam (test đồng hồ giả qua ranh giới ngày).

QUY TẮC: chạy toàn bộ suite 1 lần mỗi nhiệm vụ; commit riêng; cập nhật MD/STATUS.md; không ghi "xong" khi chưa có test thật; hết quota thì commit phần xanh và ghi phần dở. Làm theo thứ tự R1 → R4 → R5 → R6 → R3 → R2 → R7 nếu quota ít.

BÁO CÁO CUỐI (tối đa 12 dòng): R1–R7 xong/dở/chặn, số test, commit hash, tệp MD Antigravity cần lấy.
```

## Việc cho Antigravity khi Codex xong R4 và R5 (để sẵn, chưa dán)

- Lấy `MD/PROTOCOL_V2_CHANGES.md` và `MD/NET_PROTOCOL.md` mới bằng `git show prep/codex:...`, nối chat câu mẫu, rankings + vàng trong `match_end`, kết bạn.
- Màn Hồ sơ/Cửa hàng đọc số dư, vật phẩm, rank từ message hồ sơ của server thay vì ví cục bộ; mua/trang bị gửi qua server và chờ xác nhận.
- Chạy lại E2E ở chế độ nối server thật + Supabase; kiểm: vàng hai máy khớp, mua hai lần chỉ trừ một lần, mất mạng giữa lúc mua không trừ trùng.

Khi bạn gửi báo cáo của Codex, mình sẽ viết sẵn prompt này cho Antigravity.
