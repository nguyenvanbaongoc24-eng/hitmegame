# HIT ME – LỆNH CHO CODEX, ĐỢT 5

> Dán cho Codex, kèm `hit-me-gameplay-rules-v1.2.md`, `hit-me-codex-round3.md` (mô tả S2–S4, P4, R3, P5, P6) và `hit-me-codex-round4.md` (mô tả T1, S2). **Commit nhỏ; chỉ báo "pass" khi dán được `Passed! N/N`.** Hết token thì ghi "Đang dở: ..." vào `server/README.md`.
> **Lần này không viết thêm báo cáo phân tích nào ngoài U1. Phần còn lại là code + test.**

## Đánh giá
- **T2 đạt**, bảng rõ, có số dòng. Kết luận **giữ tạm** 3 tệp legacy (`BotBrain`, `GameModels`, `MatchStateMachine`) là đúng; không archive, không xóa. `core_cleanup.patch` (chỉ chuyển `CharacterData`/`WeaponData`) giữ nguyên.
- **Phát hiện cần làm rõ ngay (U1):** `MatchStateMachine` đang được gắn trong `Assets/Scenes/MainMatch.unity:4011` và `SceneSetupEditor.cs:195`. Trong khi P1 báo `MatchController` (scene graybox) chạy `Core/Phase1`. Nghĩa là có thể có **hai scene chạy hai bộ luật khác nhau**. Chưa biết scene nào là bản AG build WebGL và chủ dự án đang thử trên điện thoại.
- **T1 chưa làm, S2 chưa làm.** Codex chỉ ghi "đang dở" vào README. Chưa tính.

## U1 – Bản đồ scene (chỉ đọc, ngắn, làm đầu tiên)
Một bảng trong khung chat cho **mọi `.unity`** trong `Assets/Scenes` (và nơi khác nếu có):
| Scene | Có trong Build Settings? (đọc `ProjectSettings/EditorBuildSettings.asset`) | Component điều khiển trận (`MatchController` / `MatchStateMachine` / khác) | Dùng bộ luật nào (`Core/Phase1` hay legacy) | Bản build WebGL trong `Evidence/phase2` được build từ scene nào (nếu suy ra được) |
Kèm 3 câu trả lời: (1) Scene đang là bản chơi chính ở Phase 2? (2) Nếu `MainMatch.unity` là scene legacy, có đường chạy nào từ menu/`Build` tới nó không? (3) `Phase2PrefabBuilder`/`SceneSetupEditor` tạo scene nào? **Không sửa gì.**

## T1 – Replay tự đối chiếu (làm ngay sau U1, như `round4`)
Mỗi `replays/*.json` có khối `expected` sau từng sự kiện quan trọng và sau `Lock()`; một test chạy mọi tệp trong thư mục và so sánh (sai số 1e-4); thêm replay ngón thứ hai, chạm mới ở chỗ khác (hướng reset), hủy ngắm dưới 4 mm, chạm bắt đầu ngoài sân. Dán `Passed! N/N` mới.

## S2 – Vòng chơi + giữ bí mật (việc chính, làm ngay sau T1)
Làm đúng theo mục S2 trong `hit-me-codex-round3.md` và các điểm nhắc trong `round4`. Gói nhỏ để commit được từng phần:
1. **S2a:** lớp `Round` + `RoundClock` (qua `TimeProvider`): nhận `commit`, ghi đè cho tới khi khóa, khóa khi hết `placeSeconds` hoặc tất cả `ready`, bỏ `commit` trễ. Test với đồng hồ giả.
2. **S2b:** tính kết quả bằng `RoundResolver`/`MatchState` của `Core/Phase1`; áp `ThrowFacing` + vị trí vòng trước cho người không commit. Test **tương đương Core theo `seed`**.
3. **S2c:** phát `reveal` + `round_result` + `match_end`; **test bí mật** quét toàn bộ byte gửi cho từng kết nối trước khi khóa.
Mỗi gói một commit có `Passed! N/N`.

## Sau S2: S3 → S4 → P4 → R3 → P5 → P6 (theo `round3`)

## Mẫu báo cáo
```
VIỆC: U1 | T1 | S2a | S2b | S2c ...
ĐÃ LÀM: ...
BẰNG CHỨNG: (lệnh + "Passed! N/N" hoặc bảng U1)
KHÔNG ĐẠT / PHÁT HIỆN QUAN TRỌNG: ...
CHỜ AG / CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```

## Điều cấm (nhắc lại)
Không mở/sửa Unity và `unity-client/Assets`; không commit lên `main`; không viết lại lịch sử git; không đổi luật; không sửa Core; không triển khai thật; không tạo .md ngoài `server/README.md`; không báo "pass" khi thiếu kết quả `dotnet test`.
