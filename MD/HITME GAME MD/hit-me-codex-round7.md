# HIT ME – LỆNH CHO CODEX, ĐỢT 7

> Dán cho Codex, kèm `hit-me-gameplay-rules-v1.2.md`, `hit-me-codex-round3.md` (S2–S4, P4, R3, P5, P6), `hit-me-codex-round5.md` (T1, S2a–c). **Commit nhỏ; chỉ báo "pass" khi dán được `Passed! N/N` hoặc `PATCH_APPLY_CHECK=PASS`.** Hết token thì ghi "Đang dở: ..." vào `server/README.md`.

## Đánh giá V1
**Đạt.** `entry_scene.patch` qua `git apply --check` trên worktree sạch; một scene vào game (`Graybox.unity`), một hằng số danh sách scene, `MainMatch` hết khỏi Build Settings, scene test chỉ bỏ khỏi build, có 3 test chốt chặn. Đúng phạm vi: chưa chạy Unity, chờ AG xác nhận.

Hai điều cần làm rõ (làm ngay, ngắn):
1. Báo cáo ghi "Edited 9 files +206 −26". **Xác nhận các sửa đó nằm trong worktree tạm / bản vá, không phải trong `unity-client/Assets` của nhánh `prep/codex`.** Dán kết quả `git status --short unity-client` (phải rỗng) và `git diff --stat origin/prep/codex~..HEAD -- unity-client/Assets` (phải rỗng, trừ bản vá nằm ở `tools/patches`).
2. Hai bản vá `core_cleanup.patch` và `entry_scene.patch` đều có thể đụng `SceneSetupEditor.cs`. Trên worktree sạch, chạy `git apply --check core_cleanup.patch` rồi áp thật vào worktree tạm, sau đó `git apply --check entry_scene.patch`. Báo kết quả theo thứ tự nào áp được; nếu xung đột, **sinh lại một bản vá gộp** `tools/patches/unity_cleanup_all.patch` (core + entry scene) để AG chỉ phải áp một lần. Xóa worktree tạm.

## Việc tiếp theo (làm lần lượt, không thêm báo cáo phân tích)
1. **T1** – replay tự đối chiếu (khối `expected` sau từng sự kiện và sau `Lock()`, test chạy mọi tệp trong `replays/`, replay luật v1.2: chạm chỗ khác → hướng reset, hủy ngắm dưới 4 mm, ngón thứ hai, chạm ngoài sân). Dán `Passed! N/N`.
2. **S2a** – `Round` + `RoundClock` (qua `TimeProvider`), `commit` ghi đè tới khi khóa, khóa khi hết giờ/tất cả `ready`, `commit` trễ bị bỏ. Test đồng hồ giả.
3. **S2b** – tính bằng `RoundResolver`/`MatchState` của `Core/Phase1`; người không commit → vị trí vòng trước (vòng 1 ngẫu nhiên theo `seed`) + `ThrowFacing` bằng `DefaultFacing` chung. Test tương đương Core theo `seed`.
4. **S2c** – phát `reveal`, `round_result`, `match_end`; **test bí mật** quét toàn bộ byte gửi cho từng kết nối trước khi khóa.
5. Sau đó S3 → S4 → P4 → R3 → P5 → P6 theo `round3`.

Mỗi gói một commit, mỗi commit có kết quả test.

## Mẫu báo cáo
```
VIỆC: ...
ĐÃ LÀM: ...
BẰNG CHỨNG: (lệnh + kết quả)
KHÔNG ĐẠT / PHÁT HIỆN QUAN TRỌNG: ...
CHỜ AG / CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```

## Điều cấm (nhắc lại)
Không mở/sửa Unity và `unity-client/Assets` trên nhánh `prep/codex`; không commit lên `main`; không viết lại lịch sử git; không đổi luật; không sửa Core; không triển khai thật; không tạo .md ngoài `server/README.md`; không báo "pass" khi thiếu kết quả.
