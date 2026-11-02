# HIT ME – LỆNH CHO AG, NGÀY 31/10 (ĐỢT 32): thứ tự làm việc + nối dây mạng bước đầu

> Dán cho AG, kèm `hit-me-ag-round31.md`, `hit-me-ag-round30.md`, `hit-me-ag-round29-ui-skin.md`, `hit-me-gameplay-rules-v1.2.md`, `hit-me-room-size-rule-v1.3.md`. Quy tắc bằng chứng từ round 28 **vẫn bắt buộc**: ảnh WebGL phải **mở bằng công cụ xem ảnh**, chỉ ghi điều nhìn thấy; có dòng `build <commit>`; `Passed! N/N`; audit mã thoát 0; không tích sẵn lựa chọn của chủ dự án; không tag; không sửa `tools/audit`.

## Thứ tự làm (không bỏ bước, không đảo)
1. **`hit-me-ag-round31.md`** (sửa báo cáo Lô 1 + Lô 2: Home, Luyện với bot, Phòng). Đây là việc chính.
2. **Phản hồi của chủ dự án về Lô 1** (font, đồng hồ, duyệt/cần sửa) → một commit sửa riêng, build lại, ghi rõ khác gì. **Nếu chủ dự án chưa trả lời, không đoán**: ghi "CHỜ" và đi tiếp bước 3.
3. **N1 – Nhập bản vá mạng vào Unity (không cần máy chủ thật, không đổi giao diện):**
   - Lấy `tools/patches/net_client_v1/` từ nhánh `prep/codex` (commit mới nhất Codex báo; hiện ≥ `3869a73`) vào `Assets/Scripts/Net/` với asmdef riêng (tham chiếu `HitMe.Core.Phase1`; **không** đưa `UnityEngine` vào DTO).
   - Cờ `NetConfig.Enabled = false` mặc định: bản chơi hiện tại (Luyện với bot) **không đổi hành vi**; `WebSocketTransport` dành cho Desktop **loại khỏi bản WebGL** bằng `#if` (WebGL dùng `.jslib` ở bước sau).
   - Test EditMode với `LoopbackTransport` (gọi thẳng logic phòng của server trong tiến trình): vào phòng 2 người, bắt đầu, `lock_ready`, `commit` có/không `aimDir` (kiểm `hasAim, aimX, aimY` qua `JsonUtility`), kết thúc trận, N=1 → `not_enough_players`. Dán `Passed! N/N` kèm tên test. Nếu bản vá không biên dịch được trong Unity 6, **báo lỗi và dòng nguyên văn**, không tự viết lại giao thức.
   - Build WebGL vẫn chạy như trước (10 trận Playwright, dung lượng `.data` trước/sau); không cấp phát mới trong trận.
4. **Lô 3 (Kết quả vòng, Kết thúc trận N dòng, hộp thoại thoát, thông báo lỗi)** theo round 29, quy trình giống Lô 1/2: ảnh trước/sau + asset gốc đặt cạnh, mô tả đã xem, cổng mới, chờ chủ dự án duyệt.
5. **Lô 4 (phản hồi/juice)**: nhấn nút, mất máu, trúng đòn bằng `ASSET EFFECT`, đếm số; pool, 0 cấp phát, có "giảm chuyển động".
6. **N2 – "Online giả lập trong máy" (chỉ làm khi Codex báo Z4 `ClientMatchSession` xong):** gắn giao diện Phòng/Ghép nhanh/Trận vào các sự kiện của `ClientMatchSession` (`OnLobbyChanged`, `OnCountdown`, `OnRoundStart`, `OnReveal`, `OnRoundResult`, `OnMatchEnd`, `OnError`) **bằng `LoopbackTransport` trong cùng một tiến trình** (hai "người chơi" trên một màn hình gỡ lỗi): chứng minh Lobby → Countdown → Placing → Locked → Reveal → RoundResult → MatchEnd, `not_enough_players`, mất kết nối vào lại, `server_shutdown`, `emote` (chỉ hiện trên thẻ người chơi khi đặt, hiện trên nhân vật sau Reveal). Chưa chạm mạng thật; `.jslib` và URL máy chủ chờ Codex K3.

## Việc không được làm
Không tự viết lại giao thức hay đổi tên thông điệp (bản đúng là `server/README.md` của Codex); không để vị trí/hướng người khác hiện trước khóa; không thêm bot vào phòng online; không thêm vũ khí ngoài dép; không viết chức năng đăng nhập thật.

## Câu hỏi cần chủ dự án (AG chuyển, không tự quyết)
Font (A/B), đồng hồ (xu vàng + pulse hay chỉ pulse), duyệt Lô 1/Lô 2, Luyện với bot có tính vàng/xếp hạng không (mặc định không), vào phòng bằng mã hay danh sách, đổi nhân vật trong phòng chờ, ai làm host khi host rời.

## Mẫu báo cáo
```
BƯỚC: 1 (round31) | 2 | 3 (N1) | 4 | 5 | 6 (N2)
ĐÃ LÀM: ...
BẰNG CHỨNG: (ảnh đã xem + 1 dòng/ ảnh; Passed! N/N + tên test; .data trước/sau; audit mã thoát)
CHƯA LÀM / BỊ CHẶN: ... (nói rõ chờ ai)
HEAD = commit build: ...  Bảng cổng → commit: ...  URL (ipconfig): ...
```
