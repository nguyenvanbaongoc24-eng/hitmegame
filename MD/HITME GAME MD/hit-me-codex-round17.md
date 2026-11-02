# HIT ME – LỆNH CHO CODEX, ĐỢT 17

> Dán cho Codex, kèm `hit-me-codex-round16.md`, `hit-me-codex-round15.md` (A1, A3, A2), `hit-me-codex-round14.md` (S5, Z3), `hit-me-codex-round12.md` (P4, Z1, Z2), `hit-me-gameplay-rules-v1.2.md`. **Commit nhỏ; chỉ báo "pass" khi dán được `Passed! N/N`.** Hết token thì ghi "Đang dở: ..." vào `server/README.md`.

## Đánh giá B1 (`e3c489c`, `Passed! 40/40`)
**Đạt:** tách `room_ready` (phòng chờ) và `lock_ready` (khóa sớm trong pha đặt); tất cả ready → server tự đếm 3 giây → `round_start`; bỏ ready hoặc rời phòng → hủy đếm ngược; host `start` được khi ≥ 2 người; gửi sai pha → `error: wrong_phase`; `room_state` ở phòng chờ không có vị trí/hướng; bài quét 6 kết nối vẫn chạy; README cập nhật; tự nêu rõ việc `ready` cũ **không còn** là thông điệp phòng chờ (client Phase 4 phải dùng `room_ready`, `start`, `lock_ready`). Trung thực: Z1, Z2, S5, Z3 chưa làm.

**Cần xác nhận ngắn (trả lời một dòng mỗi mục):**
1. **A1 (bản vá đồng bộ Test16 cho AG) đã làm chưa?** Báo cáo chỉ có B1. Nếu chưa: làm **ngay bây giờ**, vì AG đang chờ.
2. Thông điệp `ready` **cũ** nay trả `error` rõ ràng (ví dụ `unknown_type`/`deprecated`) hay bị bỏ qua im lặng? Có test không?
3. Test B1 có đủ các ca: đếm ngược bị hủy khi bỏ ready; hủy khi có người rời; host `start` khi chưa đủ ready; `room_ready` trong pha đặt bị `error`; `lock_ready` ở phòng chờ bị `error`; người mới vào **sau khi trận bắt đầu** bị từ chối; đủ 6 người bắt đầu đếm ngược ngay? Ca nào chưa có thì thêm.

## Thứ tự việc
**A1 (nếu chưa) → A3 → P4 → A2 → H1 → H2 → Z1 → Z2 → S5 → Z3** → R3 → P5 → P6 → P7. (H1, H2 là việc mới bên dưới, làm trước Z1 để Z1 chạy trên server đã cứng hơn.)

## H1 – Làm cứng server (mới)
1. **Kích thước khung:** giới hạn **ở mức khung WebSocket/bộ đệm nhận** (không chỉ sau khi parse JSON): khung lớn hơn 1 KB bị đóng kết nối ngay, không cấp phát bộ nhớ theo kích thước khung do client khai báo.
2. **Giới hạn tổng:** số kết nối tối đa mỗi IP; số phòng tối đa; số người tối đa mỗi phòng (6); hết giới hạn → `error` rõ ràng.
3. **Kết nối chết:** ping/pong định kỳ, kết nối im lặng quá `idleTimeoutSeconds` bị đóng và dọn dẹp (giữ chỗ 30 giây cho người chơi trong phòng như S3 chứ không giải tán trận).
4. **Origin/CORS:** WebSocket chỉ nhận `Origin` trong danh sách cấu hình (Vercel/ localhost); thông điệp từ chối rõ ràng.
5. **Tắt máy nhẹ nhàng:** `SIGTERM` (Render) → thông báo `server_shutdown`, đóng kết nối, không treo.
6. **Nhật ký:** có cấu trúc (`roomId`, `playerId` rút gọn, `type`, lỗi), **không** ghi vị trí/hướng/`commit` trước khóa, không ghi token.
7. Test: khung quá lớn; quá giới hạn IP/phòng; kết nối im lặng bị đóng; `Origin` lạ bị từ chối; `SIGTERM` đóng sạch; nhật ký không chứa giá trị dấu vết (3.14159…) của `commit` trước khóa.

## H2 – Danh tính người chơi (mới, bản nháp, chưa dùng Supabase thật)
Hiện `join` có thể tự khai `playerId` → có thể **mạo danh**. Làm:
1. Giao diện `IAuthVerifier` (`Verify(token) → PlayerIdentity { playerId, displayName }` hoặc lỗi); bản giả `FakeAuthVerifier` (token khách `guest:<tên>` → `playerId` do **server cấp** và gắn vào kết nối); sau này Phase 5 thay bằng bản xác thực JWT của Supabase.
2. `join` bắt buộc có `token`; `playerId` **do server quyết định**, client không được tự đặt; hai kết nối cùng một `playerId` → kết nối cũ bị thay thế (chỉ khi dùng `reconnectToken`, còn không thì từ chối).
3. `reconnectToken` ngẫu nhiên mạnh (≥ 128 bit), gắn với `playerId` + phòng, dùng một lần/xoay vòng, hết hạn đúng 30 giây.
4. Test: không token → `error`; token giả/hết hạn → `error`; tự khai `playerId` bị bỏ qua; mạo danh người khác trong phòng bằng `reconnectToken` sai → `error`; `reconnectToken` hết hạn không dùng được; tên hiển thị được làm sạch (độ dài, ký tự điều khiển).
5. Ghi vào README: luồng danh tính hiện tại và chỗ Phase 5 sẽ thay.

## Dành cho AG (không chặn việc đang làm)
Khi Codex đẩy A1: sửa `Test16` theo bản vá. Giao thức server đã đổi: **client Phase 4 dùng `room_ready`, `start`, `lock_ready`, `join` có `token`** (chưa cần làm ngay, chỉ để biết; DTO chính thức sẽ có ở Z2).

## Báo cáo
```
VIỆC: A1 | A3 | P4 | A2 | H1 | H2 ...
ĐÃ LÀM: ...
BẰNG CHỨNG: (lệnh + "Passed! N/N" / bảng)
KHÔNG ĐẠT / PHÁT HIỆN QUAN TRỌNG: ...
CHỜ AG / CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```

## Điều cấm (nhắc lại)
Không mở/sửa Unity và `unity-client/Assets` trên `prep/codex`; không sửa thư mục `Builds`; không commit lên `main`; không viết lại lịch sử git; không đổi luật; không sửa Core; không triển khai Render/Supabase thật; không đưa bí mật/khóa thật vào mã; không tạo .md ngoài `server/README.md`; không báo "pass" khi thiếu kết quả test.
