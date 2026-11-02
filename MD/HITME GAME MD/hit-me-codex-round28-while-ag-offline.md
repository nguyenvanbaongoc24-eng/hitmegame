# HIT ME – LỆNH CHO CODEX, ĐỢT 28: AG BỊ GIỚI HẠN ĐẾN 2/11 → CODEX LÀM TỐI ĐA PHẦN KHÔNG CẦN UNITY

> Dán cho Codex, kèm `hit-me-codex-round26.md`, `hit-me-codex-round27.md`, `hit-me-codex-ui-asset-qa.md`, `hit-me-room-size-rule-v1.3.md`, `hit-me-gameplay-rules-v1.2.md`. AG không làm việc đến **2/11**. Mục tiêu: khi AG quay lại, **mọi thứ phía server và mọi thứ có thể chuẩn bị ngoài Unity đã sẵn sàng**, AG chỉ chép vào và gắn giao diện. Quy tắc cũ giữ nguyên: không mở/sửa Unity và `unity-client/Assets`, không sửa Core, không đổi luật, không commit lên `main`, không triển khai thật, không đưa khóa vào mã, "pass" chỉ khi dán được `Passed! N/N` kèm tên test. **Commit nhỏ, đẩy `prep/codex` sau mỗi gói** để chủ dự án thấy tiến độ.

## Thứ tự (làm lần lượt; mỗi gói kết thúc bằng báo cáo ngắn theo mẫu)

### Gói A – Hoàn tất server (ưu tiên 1)
`K1` bảng trạng thái → `K2` (H2 `join.token`, P4 200 vòng vector vàng N=2..6, Z1 WebSocket thật hỗn hợp N=2..6, Z4 `ClientMatchSession`, Z3 hỗn hợp N=2..6 + một lượt WebSocket thật) → `M1` đóng băng giao thức (`protocolVersion`, bảng JSON, mã lỗi + câu tiếng Việt) → `M2` gói bàn giao cho AG → `K3`/Z6 (`.jslib` + thử trình duyệt thật bằng Playwright hai trang). Chi tiết ở `round26`/`round27`.

### Gói B – Keo nối giao diện mạng, thuần C# (thay phần N1/N2 của AG)
Trong `tools/patches/unity_online_glue/` (biên dịch và test **ngoài Unity** bằng `dotnet`, C# phù hợp Unity 6, DTO chỉ `JsonUtility`-tương thích):
1. `IOnlineUi`: hợp đồng giao diện (không phụ thuộc UnityEngine): `ShowLobby(RoomStateDto)`, `ShowWaitingForPlayers(humans, maxPlayers)`, `ShowNotEnoughPlayers(minPlayers)` (kèm gợi ý sang Luyện với bot), `ShowQuickMatchSearching()`, `ShowQuickMatchHint()`, `ShowCountdown(seconds)`, `ShowPlacing(round, secondsLeft)`, `ShowLocked()`, `ShowReveal(RevealDto)`, `ShowRoundResult(RoundResultDto)`, `ShowMatchEnd(MatchEndDto)`, `ShowError(code, textVi)`, `ShowReconnecting()`, `ShowServerShutdown()`, `ShowEmote(playerId, emoteId, visibleOnCharacter)`.
2. `OnlineFlowController` (thuần C#): nhận sự kiện của `ClientMatchSession` và gọi `IOnlineUi`; xử lý thứ tự đúng (không hiện vị trí/hướng người khác trước Reveal; `emote` chỉ hiện trên thẻ người chơi khi đặt, hiện trên nhân vật sau Reveal); hủy đếm ngược khi có người rời; vào lại bằng `reconnectToken`.
3. `FakeOnlineUi` ghi lại chuỗi lời gọi; test với `LoopbackTransport` trên **logic phòng thật**: trận trọn vẹn N=2..6, N=1 (`not_enough_players`), ghép nhanh quá 30 giây (`quick_match_hint`, không thêm bot), mất kết nối vào lại, `server_shutdown`, `protocol_mismatch`, token sai, bị cấm do AFK. Dán `Passed! N/N` + tên test. Một bài kiểm "quét bí mật": trong pha Đặt, chuỗi lời gọi **không bao giờ** chứa vị trí/hướng của người khác.
4. Hướng dẫn cho AG (README, 10 dòng): chép vào đâu, chỉ cần viết lớp `UnityOnlineUi : IOnlineUi` gắn các `Show*` vào giao diện đã làm.

### Gói C – Chuẩn bị asset giao diện cho AG (chỉ đọc asset gốc; sản phẩm đặt trong worktree Codex)
Theo `hit-me-codex-ui-asset-qa.md`, cộng thêm:
1. `Evidence/asset_qa/ui_import_manifest.json`: mỗi asset giao diện cần dùng → `đường dẫn gốc`, `kích thước`, `viền 9-slice (L/R/T/B)`, `pixelsPerUnit` đề xuất, nhóm atlas, `có chữ in sẵn (không dùng)`, `độ tin cậy`. Dựa trên `docs/UI_ASSET_MAP.md` của AG (đọc ở `feature/ui-ux-production`).
2. `tools/asset_qa/prepare_ui_assets.py`: sinh bản **đã dọn** (cắt khoảng trong suốt thừa, loại ô vuông nếu còn, đệm alpha 2–4 px) vào thư mục tạm `Evidence/asset_qa/staging/ui/` — **không** ghi đè file gốc, **không** đưa vào `Assets`. Kèm ảnh ghép trước/sau và `.json` nguồn gốc cho mọi ảnh. Test tất định (`Passed! N/N`).
3. Bảng "asset còn thiếu cho Lô 2–4" (Home, Luyện với bot, Phòng, Kết quả, huy chương, logo): để chủ dự án quyết.

### Gói D – Kiểm độc lập bản AG hiện có (chỉ đọc)
`K4` = X6/X7/X8 trên HEAD mới nhất của AG (đọc ở `D:\HITME GAME\unity-client`, chỉ đọc) + `M3` (kiểm bảng giao thức trong `docs/PHASE3_STEP0.md`) + so `docs/01_GAMEPLAY_RULES.md` với `hit-me-gameplay-rules-v1.2.md` + `hit-me-room-size-rule-v1.3.md` (luật nào AG chép sai/thiếu, ví dụ phòng online không bot). Bổ sung: kiểm "né mẫu quét" (số cứng bị viết lại cho hết khớp), và bản build 8094 (`64bc6a9`) có dấu vân tay đúng commit không. Kết quả vào một bảng ĐẠT/KHÔNG ĐẠT; AG đọc khi quay lại.

### Gói E – Chuẩn bị triển khai (bản nháp, không triển khai thật)
`K5`: C2 (SQL + RLS), Z5 (`vercel.json`, `render.yaml` mẫu, hướng dẫn từng bước cho chủ dự án, `/healthz`, `ALLOWED_ORIGINS`), A2/A3 (bảng luật ↔ test có dòng "N người thật, không bot"), R3, P6. Thêm `tools/deploy/smoke_test.ps1` (hoặc `.mjs`): nhận URL `wss://…`, tự chạy hai người chơi mô phỏng một trận trọn vẹn và in ĐẠT/KHÔNG ĐẠT — chủ dự án chạy sau khi tự triển khai. Khi đủ: báo **SERVER READY** kèm commit.

## Báo cáo cuối giai đoạn (trước 2/11)
Một tin duy nhất: bảng trạng thái mọi gói (XONG @ commit + test / CHƯA / BỊ CHẶN), đường dẫn các sản phẩm bàn giao, và **danh sách việc AG làm đầu tiên khi quay lại** (3–6 dòng). Giữa chừng báo ngắn sau mỗi gói.

## Mẫu báo cáo mỗi gói
```
GÓI: A | B | C | D | E
ĐÃ LÀM: ...
BẰNG CHỨNG: (lệnh + "Passed! N/N" + tên test / số đo / đường dẫn tệp)
CHƯA LÀM / KHÔNG ĐẠT / BỊ CHẶN: ...
CHỜ AG / CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```
