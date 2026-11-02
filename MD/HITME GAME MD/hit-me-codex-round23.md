# HIT ME – LỆNH CHO CODEX, ĐỢT 23: dọn gọn toàn bộ tồn đọng của Codex (gói việc có thứ tự)

> Dán cho Codex, kèm `hit-me-codex-round22.md`, `hit-me-codex-round21.md`, `hit-me-codex-round17.md` (H2), `hit-me-codex-round15.md` (A3, A2), `hit-me-codex-round14.md` (S5, Z3), `hit-me-codex-round12.md` (P4, Z1, Z2) và `hit-me-gameplay-rules-v1.2.md`. **Commit nhỏ; chỉ báo "pass" khi dán được `Passed! N/N`.** Hết token thì ghi "Đang dở: ..." vào `server/README.md`. File này **sắp thứ tự lại** và thêm vài việc để Phase 4 (kết nối mạng) chỉ còn là việc nối dây phía Unity.

## Đánh giá báo cáo Y5 + Y6 (`2612c77`)
- **Y5 đạt, trung thực:** audit cũ **sai**, không phải AG làm mất asmdef; `HitMe.Core.asmdef` vẫn có `"noEngineReferences": true` từ commit `6f1d270`; công cụ đã sửa để tìm đúng asmdef trong `Core/Phase1`, kiểm thử phủ định vẫn `Passed! 7/7`. (Bài học: công cụ kiểm tra cũng phải được kiểm tra; việc chạy trên dự án thật đã bắt được lỗi của chính công cụ.)
- **Y6 đạt:** `manifest_gen.ps1` sinh `MANIFEST.generated.csv`, không ghi đè bản hiện có nếu không có `-Overwrite`, `-Check` báo `missing`/`badHash`/`stale`, `NO_PROVENANCE` khi thiếu `.json`; `Passed! 5/5`. Đã dọn ba tệp rác (`new`, `one`, `png`) mà test tạo nhầm trong worktree Codex.

## Tình hình phase
**Chưa sang được Phase 3 chính thức:** Phase 2 còn chờ AG (S1–S12: xếp hạng về Core, bản WebGL mới, 460 B/khung hình, D0, MANIFEST, audit sạch), chủ dự án thử điện thoại, và Codex X5. Phase 3 Bước 0 (rà soát) AG làm song song. **Phần server (Phase 4) của Codex có thể hoàn tất trước**, rồi phía Unity chỉ còn nối dây.

## Gói việc của Codex (làm lần lượt; mỗi gói kết thúc bằng một báo cáo ngắn)

### Gói 1 – Hoàn thiện server (Phase 4 phía Codex)
1. **H2:** gateway **bắt buộc `join.token`** (thiếu/sai → `error`; `join` cũ không token → lỗi di chuyển rõ ràng như `ready` cũ, có test); `playerId` do server quyết định; `reconnectToken` ≥ 128 bit, gắn `playerId` + phòng, hết hạn đúng 30 giây; mạo danh bằng `reconnectToken` sai bị từ chối. Cập nhật README + giao thức **trước** khi sang gói 2 để DTO dùng bản cuối.
2. **P4:** 200 vòng vector vàng → `server/Tests/golden/rounds.json` (kèm `seed`, phiên bản `GameConfig`, các ca biên và **thứ hạng/đồng hạng cuối trận**), test server đọc nó.
3. **Z1:** kiểm thử đầu-cuối qua WebSocket thật: 6 bot × 20 trận, có chèn vào lại / `commit` lỗi / không `commit` / `join` không token; so kết quả từng vòng và thứ hạng với `Core/Phase1`; ghi độ trễ (trung bình/lớn nhất).
4. **S5:** AFK (2 vòng liên tiếp không `commit` khi vẫn kết nối → loại khỏi trận + cấm 120 giây qua `IBanStore`; mất kết nối ≠ AFK) và `emote` giữ bí mật (không kèm vị trí/hướng; giới hạn tần suất; danh sách `emoteId` cho phép; bài quét dấu vết trước khóa mở rộng cho `emote`).
5. **Z3:** thử tải 50 phòng × 6 bot đồng thời; ghi CPU, bộ nhớ đỉnh, thời gian tính một vòng, độ trễ broadcast p95, số kết nối rớt, 0 ngoại lệ, 0 treo; ước lượng số phòng một tiến trình gánh được vào README.

### Gói 2 – Cầu nối Unity (để AG chỉ nối dây)
6. **Z2:** `tools/patches/net_client_v1/` — DTO thuần C# tương thích `JsonUtility` (`hasAim` + `aimX`, `aimY`, không `null`/`Dictionary`; server chấp nhận cả `aimDir: null` cũ), `IMatchTransport`, `LoopbackTransport` (gọi thẳng logic phòng trong tiến trình), khung `WebSocketTransport` (ghi chú WebGL cần `.jslib`/thư viện WebSocket khác). Dùng đúng giao thức cuối của gói 1 (`join.token`, `room_ready`, `start`, `lock_ready`, `emote`).
7. **Z4 (mới):** `ClientMatchSession` thuần C# trong cùng bản vá: máy trạng thái **phía client** điều khiển bởi `IMatchTransport`: `Disconnected → Connecting → Lobby → Countdown → Placing → Locked → Reveal → RoundResult → MatchEnd`, xử lý `reconnect`, `error`, `server_shutdown`, `emote`; phát sự kiện (`OnLobbyChanged`, `OnCountdown`, `OnRoundStart`, `OnReveal`, `OnRoundResult`, `OnMatchEnd`, `OnError`). Test với `LoopbackTransport` chạy trên **logic phòng thật của server**: trận trọn vẹn 6 người, mất kết nối vào lại, người vào muộn bị từ chối, `emote`, `server_shutdown`. Nhờ đó AG chỉ phải gắn UI vào các sự kiện, không tự viết lại luồng.
8. **A2:** `tools/patches/golden_parity/GoldenRoundsParityTests.cs` (NUnit EditMode đọc cùng `rounds.json`, so HP/bị trúng/bị loại/thứ hạng), kiểm biên dịch ngoài Unity; ghi chú nơi đặt `rounds.json` trong `Assets`.

### Gói 3 – Kiểm kê và tài liệu
9. **A3:** bảng luật ↔ test (luật chưa có test thì viết test cho server).
10. **C2:** chạy các bản nháp SQL Supabase (tài khoản, vàng, túi đồ, xếp hạng, cấm AFK) trên PostgreSQL cục bộ không cần Docker (ví dụ PGlite qua npm); có **RLS** từng bảng; bảng vàng/xếp hạng chỉ ghi qua hàm phía server; không khóa/bí mật trong tệp. Kết quả vài dòng vào `server/README.md`.
11. **Z5 (mới) – bản nháp triển khai, không triển khai thật:** `tools/deploy/vercel.json` mẫu cho bản WebGL của Unity (đặt đúng `Content-Encoding`/`Content-Type` cho `.br`/`.gz`/`.wasm`/`.data`, cache dài cho tệp có băm, **không cache** `index.html`) và `server/README.md` mục "Triển khai Render (cho chủ dự án)": từng bước, biến môi trường (`PORT`, `ALLOWED_ORIGINS`, khóa Supabase **không** nằm trong repo), đường kiểm tra sức khỏe `/healthz`, lưu ý gói miễn phí ngủ sau ~15 phút và ngắt WebSocket (phương án: gói trả phí hoặc ping giữ thức), cách thử kết nối bằng một bot.

### Gói 4 – Dọn dẹp và asset (cuối cùng, nếu còn token)
12. **R3:** `tools/repo_hygiene/apply_plan.ps1` (mặc định `-WhatIf`): bỏ theo dõi thư mục build trong `Evidence`, Git LFS cho ảnh/âm thanh chỉ cho commit mới, đưa `MD/` vào `docs/` (chỉ liệt kê bản mới nhất, không tự chọn).
13. **P5 + P7:** danh sách thay asset theo ưu tiên P0/P1/P2 và kiểm định bộ asset gốc `D:\HITME GAME\ASSET HITME FINAL` so với bản trong dự án (đặc biệt `ASSET WEAPONS\DÉP\sprites` 25 khung: diện tích pixel, hộp bao, màu xanh còn lại), ảnh ghép `Evidence/asset_qa/contact/`.
14. **P6:** `tools/patches/projectileview_material.patch` (nếu AG chưa sửa).

## Cổng "server xong" (Codex tự kiểm cuối Gói 1–2, ghi vào README)
H2 + Z1 + S5 `Passed`, `rounds.json` đã dùng, Z3 có số đo, Z2/Z4 biên dịch được và chạy trong `dotnet test`, README đủ (cách chạy, cách test, mọi thông điệp có ví dụ JSON, giao thức cuối, điều chưa làm). Khi đủ: báo "SERVER READY" kèm commit.

## Khi AG báo xong (xen kẽ, không chờ)
- AG báo **S1** (xếp hạng về `Core/Phase1`): đối chiếu `ServerMatch` với hàm xếp hạng mới trên vector vàng.
- AG báo **Phase 2 xong**: làm **X5** (chạy `phase2_audit.ps1` + `manifest_gen.ps1 -Check` trên commit/bản build cuối của AG, ĐẠT/KHÔNG ĐẠT từng mục, chỉ đọc).

## Báo cáo
```
VIỆC: gói/mục (ví dụ G1.1 H2)
ĐÃ LÀM: ...
BẰNG CHỨNG: (lệnh + "Passed! N/N" / số đo / bảng)
KHÔNG ĐẠT / PHÁT HIỆN QUAN TRỌNG: ...
CHỜ AG / CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```

## Điều cấm (nhắc lại)
Không mở/sửa Unity và `unity-client/Assets` trên `prep/codex`; không sửa thư mục của AG hay `Builds`; không commit lên `main`; không viết lại lịch sử git; không đổi luật; không sửa Core; không triển khai Render/Vercel/Supabase thật; không đưa bí mật/khóa thật vào mã; không tạo .md ngoài `server/README.md`; không báo "pass" khi thiếu kết quả test.
