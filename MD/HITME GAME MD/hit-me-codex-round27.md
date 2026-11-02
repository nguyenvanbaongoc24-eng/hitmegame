# HIT ME – LỆNH CHO CODEX, NGÀY 31/10 (ĐỢT 27): đóng băng giao thức + gói bàn giao cho AG

> Dán cho Codex, kèm `hit-me-codex-round26.md` (K1–K5 vẫn là danh sách việc chính; đợt này **thêm** 3 việc và đặt thứ tự). Commit nhỏ; "pass" chỉ khi dán được `Passed! N/N` kèm tên test.

## Thứ tự trong ngày
1. **K1** bảng trạng thái (làm đầu tiên, 10 phút).
2. **K2** H2 → P4 → Z1 → Z4 → Z3 (hỗn hợp N=2..6).
3. **M1, M2, M3** bên dưới (AG đang chờ Z4/Z6 để nối dây; ưu tiên cao hơn K3 và K4).
4. **K3** (Z6: `.jslib` + thử trình duyệt thật) → **K4** (X6/X7/X8 trên bản AG mới) → **K5**.

## M1. Đóng băng giao thức v1 (trước khi AG nối UI)
- Thêm `protocolVersion` (số nguyên) vào phản hồi `join`/`room_state`; client cũ/mới khác phiên bản → `error: protocol_mismatch` có test.
- Bảng giao thức cuối trong `server/README.md`: mọi thông điệp client→server và server→client, **trường bắt buộc/tùy chọn/kiểu dữ liệu**, ví dụ JSON thật (đã được test sinh ra, không viết tay), mã `error` đầy đủ (`not_enough_players`, `protocol_mismatch`, `token_invalid`, `room_full`, `match_in_progress`, `banned`, `rate_limited`, `legacy_ready_use_room_ready_or_lock_ready`, …) và câu thông báo gợi ý cho người chơi (tiếng Việt) để AG dùng nguyên văn.
- Quy ước: từ lúc "đóng băng", mọi thay đổi giao thức phải tăng `protocolVersion` và ghi vào mục "Nhật ký thay đổi giao thức"; không đổi tên thông điệp.

## M2. Gói bàn giao cho AG (mục "Tích hợp Unity" trong `server/README.md`)
Danh sách: commit cần lấy (`prep/codex @ <hash>`), thư mục `tools/patches/net_client_v1/`, tệp nào chép vào đâu trong `Assets/Scripts/Net/`, asmdef cần có, cờ `NetConfig.Enabled`, cách chạy test với `LoopbackTransport`, các điểm `#if UNITY_WEBGL`, trình tự sự kiện của `ClientMatchSession` (bảng trạng thái → sự kiện → việc giao diện cần làm), và **cách biên dịch kiểm tra ngoài Unity** (lệnh). Đảm bảo bản vá **không** dùng Newtonsoft/`Dictionary`/`null` trong DTO (chỉ `JsonUtility`), C# phù hợp Unity 6.

## M3. Kiểm chéo bảng giao thức do AG viết
AG nói đã sửa `docs/PHASE3_STEP0.md` theo `server/README.md` với số dòng nguồn: client gửi `join`, `room_ready`, `start`, `lock_ready`, `commit`, `leave`, `quick_match`, `emote`; server gửi `room_state`, `round_start`, `reveal`, `round_result`, `match_end`, `quick_match_waiting`, `quick_match_hint`, `quick_match_left`, `error`, `server_shutdown`. Đọc `docs/PHASE3_STEP0.md` (ở `feature/ui-ux-production`, chỉ đọc) và báo: tên nào **không tồn tại** ở server (ví dụ `leave`, `round_start`, `quick_match_waiting`, `quick_match_left`), tên nào server có mà AG thiếu, số dòng nguồn có đúng không. Danh sách chênh lệch → AG sửa.

## Không đổi
Không mở/sửa Unity và `unity-client/Assets`; không sửa Core; không commit lên `main`; không đổi luật; không thêm bot vào phòng online; không triển khai thật; không đưa khóa/bí mật vào mã; không tạo `.md` ngoài `server/README.md` và `Evidence/asset_qa/UI_KIT_REPORT.md`.

## Báo cáo
```
VIỆC: K1 | K2 | M1 | M2 | M3 | K3 | K4 | K5
ĐÃ LÀM: ...
BẰNG CHỨNG: (lệnh + "Passed! N/N" + tên test / số đo)
CHƯA LÀM / KHÔNG ĐẠT: ...
CHỜ AG / CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```
