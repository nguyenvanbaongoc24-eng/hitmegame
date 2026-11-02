# HIT ME – LỆNH CHO CODEX, ĐỢT 16 (kèm việc UI phát hiện cho AG)

> Dán cho Codex, kèm `hit-me-codex-round15.md` (A1, A3, A2), `hit-me-codex-round14.md` (S5, Z3), `hit-me-codex-round12.md` (P4, Z1, Z2) và `hit-me-gameplay-rules-v1.2.md`. **Commit nhỏ; chỉ báo "pass" khi dán được `Passed! N/N`.** Hết token thì ghi "Đang dở: ..." vào `server/README.md`.

## Ghi nhận báo cáo vừa rồi
Codex nhìn ảnh chụp trận và nêu đúng các lỗi UI (xem phần AG bên dưới), đồng thời nêu một khác biệt quan trọng cho Phase 4: **server hiện chỉ bắt đầu phòng khi host gửi `start`, trong khi quyết định UX là "tất cả bấm SẴN SÀNG thì tự bắt đầu"**. Đúng. Lộ trình Codex tự đề ra (Z1 → Z2 → S5 → Z3 → dọn repo/asset) **thiếu** ba việc đã giao ở đợt 15 là **A1, A3, A2**. Thứ tự đúng bên dưới.

## Thứ tự việc (cập nhật)
**A1 → B1 (mới) → A3 → P4 → A2 → Z1 → Z2 → S5 → Z3** → R3 → P5 → P6 → P7.
(A1: bản vá đồng bộ Test16 cho AG, ngắn, mở khóa AG — làm trước. Chi tiết A1/A3/A2 ở `round15`.)

## B1 – Bắt đầu phòng: tách hai loại "sẵn sàng" (mới, làm ngay sau A1)
**Vấn đề:** hai việc cùng tên "SẴN SÀNG" dễ nhầm trong giao thức: (1) **sẵn sàng ở phòng chờ** (mọi người bấm → trận tự bắt đầu); (2) **khóa sớm trong pha đặt** (bấm SẴN SÀNG trong trận = chốt vị trí/hướng, không cần chờ hết 5 giây).

**Quy tắc (mặc định đề xuất, cấu hình được; chủ dự án sẽ xác nhận):**
- Thông điệp riêng: `room_ready` (`{ready: true|false}`) cho phòng chờ; thông điệp `lock_ready` (hoặc giữ tên `ready` cũ nhưng **chỉ hợp lệ khi đang trong pha đặt**) cho khóa sớm. Gửi sai pha → `error`, không đổi trạng thái.
- **Tự bắt đầu:** khi phòng có ≥ 2 người và **tất cả** `room_ready = true`, bắt đầu đếm ngược `startCountdownSeconds = 3` (ai bỏ ready hoặc rời phòng thì hủy đếm ngược); hết đếm ngược phát `round_start` vòng 1.
- **Host `start`:** vẫn dùng được khi ≥ 2 người và chưa đủ ready (bắt đầu ngay, không đếm ngược). Trong phòng bí mật/bạn bè: host mới có quyền `start`; **ghép nhanh** chỉ dùng cơ chế tự bắt đầu (không host).
- Ai chưa ready khi host `start`: vẫn vào trận. Người mới vào khi trận đã bắt đầu: bị từ chối (`error`).
- Đủ 6 người: không chờ thêm, bắt đầu đếm ngược ngay khi tất cả ready.
- Test (đồng hồ giả): đủ ready → đếm 3 giây → `round_start`; bỏ ready giữa đếm → hủy; host `start` khi chưa đủ ready; `room_ready` trong pha đặt bị `error`; `lock_ready` ở phòng chờ bị `error`; người rời phòng làm đếm ngược hủy; không có thông điệp phòng chờ nào lộ vị trí/hướng (giữ nguyên bài quét bí mật 6 kết nối).
- Cập nhật `server/README.md` (danh sách thông điệp + ví dụ JSON) và, nếu đã làm Z2, DTO tương ứng trong `tools/patches/net_client_v1`.

## Dành cho AG – UI phát hiện (dán cho AG, làm sau khi hoàn tất việc đang dở)
Codex xem ảnh chụp trận và thấy các lỗi UI cần sửa **trước khi đóng Phase 2/3**. Mỗi lỗi cần ảnh trước/sau ở **5 độ phân giải chuẩn** (1080×1920, 1179×2556, 1080×2400, 1080×2520, 1620×2160) và ghi vào MANIFEST:
1. **Cuối trận:** vòng `5/5`, đồng hồ `0.0s` vẫn hiện nút **SẴN SÀNG** và màn "KẾT QUẢ VÒNG". Khi `MatchEnd` phải **chuyển chắc chắn** sang bảng kết quả trận (xếp hạng, HP, số lần trúng), ẩn nút SẴN SÀNG; thêm test điều khiển trạng thái (vòng cuối → MatchEnd → bảng kết quả, không quay lại pha đặt).
2. **HUD 6 người chồng/đè lên cổng sân, chữ khó đọc:** sắp lại 6 thẻ người chơi theo mockup (hàng thẻ gọn, không che cổng sân), tên + HP đọc được ở cỡ tối thiểu (kiểm bằng ảnh thật trên điện thoại, không chỉ Editor).
3. **Canvas chỉ chiếm một dải dọc giữa màn hình desktop:** mọi HUD/nút phải neo trong vùng portrait 1080×2340 (Safe Area), nền ngoài vùng đó đặt màu tối; kiểm cả khi cửa sổ trình duyệt rộng (letterbox).
4. **Nhãn vũ khí nằm quá xa sân:** gom vào Safe Area cùng HUD, sát khu vực dưới sân, không bị che bởi thanh gạch dưới của iPhone.
5. Vẫn còn việc cũ: truy 460 B/khung hình về 0, bảng nhân vật ↔ thư mục `ASSET HITME FINAL`, bổ sung D0 (thước-đường bay, độ chính xác chạm), sửa `Test16` theo bản vá A1 của Codex.
Chờ phản hồi thử điện thoại của chủ dự án trước khi đóng Phase 2.

## Báo cáo
```
VIỆC: A1 | B1 | A3 | P4 | A2 | Z1 ...
ĐÃ LÀM: ...
BẰNG CHỨNG: (lệnh + "Passed! N/N" / bảng)
KHÔNG ĐẠT / PHÁT HIỆN QUAN TRỌNG: ...
CHỜ AG / CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```

## Điều cấm (nhắc lại)
Không mở/sửa Unity và `unity-client/Assets` trên `prep/codex`; không sửa thư mục `Builds`; không commit lên `main`; không viết lại lịch sử git; không đổi luật; không sửa Core; không triển khai Render/Supabase thật; không tạo .md ngoài `server/README.md`; không báo "pass" khi thiếu kết quả test.
