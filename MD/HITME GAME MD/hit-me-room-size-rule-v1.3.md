# HIT ME – LUẬT v1.3 (bản sửa 30/10): phòng online 1–6 người, KHÔNG có bot; chơi với bot là chế độ riêng ở menu

> **Nguồn sự thật cho số người chơi**, bổ sung `hit-me-gameplay-rules-v1.2.md` (luật cử chỉ không đổi). Dán cho **Codex** và **AG**. **File này thay bản v1.3 gửi lúc 10:10 (bản có "bot trong phòng"): bỏ hoàn toàn các mục `set_bots`, bot bổ sung khi ghép nhanh, `bots` trong `room_state`.**

## 1. Luật (chủ dự án chốt 30/10)
1. **Phòng online** chứa **tối đa 6 người**. Người chơi có thể **tạo hoặc vào phòng khi trong phòng có 1–6 người**; một trận **không mặc định đủ 6**.
2. **Trận online cần ít nhất 2 người thật.** Nếu chỉ có 1 người trong phòng: **không bắt đầu được** (host `start` và tự bắt đầu đều không chạy). Giao diện báo rõ: *"Cần ít nhất 2 người. Muốn chơi một mình, hãy sang menu **Luyện với bot**."*
3. **Không có bot trong phòng online.** Chơi với bot là **chế độ riêng ở menu** ("Luyện với bot"): chạy **cục bộ trên máy** (`Core/Phase1` + `LocalBot`), **không qua server**, 1 người thật + 1–5 bot (mặc định 3), không cần mạng.
4. **Ghép nhanh** chỉ ghép **người thật**: bắt đầu khi có ≥ 2 người thật sẵn sàng (đếm ngược 3 giây). Nếu chờ quá `quickMatchHintSeconds` (mặc định 30 giây) mà chưa ghép được: **chỉ gợi ý** "Chưa tìm thấy đối thủ. Chơi với bot?" (nút sang Luyện với bot), **không tự thêm bot**.
5. Mọi luật khác giữ nguyên (3 HP, 5 giây đặt, lộ diện đồng thời, ném đồng thời, `maxRounds = 5`, xếp hạng HP → số lần trúng → đồng hạng; giữ chỗ 30 giây khi mất kết nối; không nhận người vào giữa trận).
6. Phần thưởng/xếp hạng/vàng của chế độ Luyện với bot: **chưa quyết** (mặc định: không tính xếp hạng, không vàng). Ghi vào câu hỏi mở cho chủ dự án.

## 2. Việc cho CODEX (server, mỗi mục có test)
1. **Phòng chờ/B1:** điều kiện bắt đầu = **≥ 2 người thật** và ≤ 6. Dưới 2 người: `start`/tự bắt đầu → `error: not_enough_players` (người thứ hai rời trong lúc đếm ngược → hủy đếm ngược). `room_state` có `players` (danh sách + `ready`), `maxPlayers = 6`, `minPlayersToStart = 2`. Phòng vẫn tồn tại khi chỉ có 1 người (đang chờ người khác).
2. **Test theo N người thật** (`N = 2, 3, 4, 5, 6`): bắt đầu đúng, bí mật vị trí (bài quét dấu vết cho **mọi N**, không chỉ 6 kết nối), khóa vòng, kết thúc khi còn ≤ 1 người sống hoặc hết `maxRounds`, thứ hạng/đồng hạng đúng. Ca **N = 1**: không bắt đầu được (cả hai đường).
3. **P4 (vector vàng):** các vòng có **N từ 2 đến 6**, kể cả người bị loại làm N giảm giữa trận; thứ hạng cuối trận cho từng N.
4. **Z1/Z3:** mô phỏng đầu-cuối và thử tải với **hỗn hợp** phòng 2/3/4/5/6 người thật, có phòng 1 người (không bắt đầu); ghép nhanh chỉ ghép người thật; hết `quickMatchHintSeconds` server phát thông điệp gợi ý (`quick_match_hint`), không thêm bot.
5. **Z2/Z4:** DTO `room_state` (players, ready, maxPlayers, minPlayersToStart); `ClientMatchSession` xử lý roster **N người thật** và các trạng thái "đang chờ người chơi" / "không đủ người". **Không** làm `LocalBotSession` (việc của AG).
6. **S5:** AFK/cấm áp cho mọi người chơi online (không còn bot trong phòng). Biểu cảm theo luật bí mật.
7. **A3/A2:** bảng luật ↔ test thêm dòng "N người thật, không bot"; `GoldenRoundsParityTests` chạy mọi N.
8. `server/README.md`: giao thức + ví dụ JSON (`not_enough_players`, `quick_match_hint`), cấu hình `quickMatchHintSeconds`.

## 3. Việc cho AG (client; thêm vào `hit-me-ag-remaining-work-v2.md`)
1. **HUD/thẻ người chơi theo N (2–6):** chỉ vẽ số thẻ có thật, căn gọn; bảng kết quả N dòng; ảnh ở 1080×2340 cho **mỗi N = 2..6** (vào MANIFEST). Chế độ Luyện với bot dùng cùng bố cục (1 người + 1–5 bot).
2. **Roster/MatchController:** `SelectMatchRoster` nhận N (2–6), không trùng nhân vật; test N = 2..6 × nhiều `seed`.
3. **Luyện với bot (chế độ riêng ở menu):** chọn số bot 1–5 (mặc định 3), chọn nhân vật; chạy cục bộ bằng `Core/Phase1` + `LocalBot`, **không** cần server; vẫn dùng đúng luật v1.2 (cử chỉ, 5 giây, bí mật vị trí với bot). AutoPlay N = 2..6 (10 trận mỗi N), 0 lỗi, 0 treo.
4. **Phòng online (UI):** hiển thị `humans/maxPlayers`; khi chỉ có 1 người: nút **Bắt đầu** bị khóa kèm câu giải thích và nút **Sang Luyện với bot**; mockup "Vào phòng" mặc định 6 chỗ, chỗ trống ghi "Đang chờ…" (không phải lỗi).
5. **Phase 3 Bước 0 (rà soát):** thêm hai luồng riêng: *Phòng online* (tạo/vào, 1–6 người, bắt đầu khi ≥ 2) và *Luyện với bot* (một mình); câu hỏi mở: có tính phần thưởng/xếp hạng cho Luyện với bot không.
6. **Xếp hạng trong `Core/Phase1` (S1):** test cho N = 2..6, người bị loại theo thứ tự, đồng hạng.

## 4. Báo cáo
Dùng mẫu hiện có của từng bên; ghi rõ **N** trong tên test và ảnh (`..._N4.png`). Không báo "đạt" khi chỉ thử N = 6.

## 5. Điều cấm (nhắc lại)
Không giả định 6 người trong code mới (hằng số `6` chỉ là `maxPlayers` lấy từ cấu hình); **không thêm bot vào phòng online, không tự thêm bot khi ghép nhanh**; không để server nhận `commit` của "bot"; không hiện vị trí/hướng người khác trong pha đặt dù N nhỏ; không sửa logic combat ngoài danh sách cho phép; không báo "pass" khi thiếu kết quả test.
