# HIT ME – ĐỢT 6: bản build Phase 2 đang chạy NHẦM luật cũ (phần Codex + phần AG)

> Codex dán **Phần A** (kèm `hit-me-codex-round5.md` cho T1, S2). AG dán **Phần B**. **Commit nhỏ; chỉ báo "pass" khi có dòng `Passed! N/N` hoặc ảnh/log làm bằng chứng.**

## Phát hiện U1 (rất quan trọng)
Từ `Evidence/phase2/webgl-build.log`: bản WebGL Phase 2 có **Level 0 = `MainMatch.unity`**, Level 4 = `Graybox.unity`; scene đầu là `MainMatch`, gắn `MatchStateMachine` **cũ**, **không** dùng `Core/Phase1`. Nguyên nhân: `SceneSetupEditor.BuildSampleScene()` tạo `MainMatch`, ghi Build Settings chỉ với `MainMatch`; `WebBuildPipeline` cũng build riêng `MainMatch`. Ngoài ra `Graybox.unity` bị lặp 2 lần trong Build Settings và WebGL đóng gói cả 5 scene test.

**Hệ quả:** những gì chủ dự án thử trên điện thoại, và những gì AG báo "đạt Phase 2", có thể là **luật cũ, không phải luật đã kiểm thử 11/11 + dùng chung với server**. Các bằng chứng Phase 2 chưa đáng tin cho tới khi sửa xong. Luật cử chỉ v1.2 cũng phải nối vào scene dùng `Core/Phase1`.

---

## PHẦN A – CODEX

### V1 – Bản vá "một scene vào game" (làm đầu tiên, nhỏ, không áp dụng)
Không sửa `unity-client/Assets` trên `prep/codex`. Dùng worktree tạm như R2, xuất `tools/patches/entry_scene.patch` (kiểm bằng `git apply --check`, xóa worktree sau đó):
1. Đọc `ProjectSettings/EditorBuildSettings.asset` (và nếu Unity 6 có Build Profile: `Assets/Settings/Build Profiles/*.asset`) để biết danh sách scene **thật sự** dùng khi build, và nó nằm ở đâu.
2. Danh sách scene build chỉ còn **một hằng số dùng chung** (ví dụ `BuildScenes.cs`): `["Assets/Scenes/Graybox.unity"]`, không trùng lặp. Scene đầu = scene dùng `MatchController` + `Core/Phase1`.
3. `SceneSetupEditor.BuildSampleScene()`: **không còn ghi Build Settings** và không nằm trong đường build (bỏ khỏi menu build, hoặc gắn `[System.Obsolete]` + chuyển sang `Assets/Editor/_Archive`). `WebBuildPipeline`: đọc danh sách từ hằng số ở trên, không build `MainMatch`.
4. Scene test (`Phase2_Step1_*`, v.v.): giữ trong repo, **chỉ bỏ khỏi build** (không xóa).
5. Thêm **test chốt chặn** (EditMode, kèm trong bản vá, chưa chạy được ngoài Unity): (a) `EditorBuildSettings.scenes` bật đủ đúng hằng số, không trùng; (b) scene đầu có `MatchController`, **không** có component `MatchStateMachine`; (c) `MatchStateMachine` không còn được tham chiếu bởi mọi scene trong danh sách build.
6. Ghi đầu bản vá: tệp nào đổi, vì sao, thứ tự áp dụng cùng `core_cleanup.patch` (xung đột nếu có ở `SceneSetupEditor.cs`).

### Sau V1: T1 → S2a → S2b → S2c
Theo `hit-me-codex-round5.md` (T1: replay tự đối chiếu; S2: vòng chơi + giữ bí mật, chia 3 gói). Hết token thì ghi "Đang dở" vào `server/README.md`.

### Mẫu báo cáo
```
VIỆC: V1 | T1 | S2a ...
ĐÃ LÀM: ...
BẰNG CHỨNG: (lệnh + "PATCH_APPLY_CHECK=PASS" hoặc "Passed! N/N")
KHÔNG ĐẠT / PHÁT HIỆN QUAN TRỌNG: ...
CHỜ AG / CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```

---

## PHẦN B – ANTIGRAVITY (làm **trước** khi gắn `v0.2-rc1`; đặt trước cả luật cử chỉ v1.2)

Dán cùng `hit-me-gameplay-rules-v1.2.md`. Không xóa `ASSET`, `SFX`, `Evidence`, `MD`. Commit sau mỗi bước.

**B1 – Chốt scene vào game.** Bản chơi chính = `Graybox.unity` (hoặc scene nào đang dùng `MatchController` + `Core/Phase1`; nếu tên khác, ghi rõ). Nếu trên `prep/codex` đã có `tools/patches/entry_scene.patch`, áp dụng bằng `git apply --3way`. Nếu chưa có, tự làm: danh sách scene build chỉ còn scene đó, không trùng lặp; `SceneSetupEditor`/`WebBuildPipeline` không được tạo lại `MainMatch` hay ghi Build Settings; scene test chỉ bỏ khỏi build, không xóa.

**B2 – Xác nhận.** Build WebGL lại, đưa log vào `Evidence/phase2/webgl-build.log` (phải liệt kê đúng **một** scene, Level 0 = scene dùng `Core/Phase1`). Chụp ảnh trận chạy, chuỗi AutoPlay 10 trận không lỗi, và `grep` cho thấy không còn `MatchStateMachine` trong scene nào của danh sách build.

**B3 – Kiểm tra lại các bằng chứng Phase 2 đã báo.** Những ảnh/clip/mục DoD trước đây chụp từ `MainMatch`: ghi rõ cái nào phải chụp lại. Chủ dự án phải thử lại trên iPhone/Android bằng bản build mới.

**B4 – Rồi mới làm** luật cử chỉ v1.2 (`hit-me-ag-gesture-v2.md`), nối vào `MatchController` của scene này; sau đó đóng DoD và gắn tag như `hit-me-ag-close-phase2-open-phase3.md`.

Báo cáo theo mẫu: BƯỚC / ĐÃ LÀM / BẰNG CHỨNG (đường dẫn log, ảnh) / KHÔNG ĐẠT / CHỜ CHỦ DỰ ÁN / SẴN SÀNG SANG BƯỚC.

## Điều cấm
Không sửa luật combat ở `Core/Phase1`; không xóa `ASSET`/`SFX`/`Evidence`/`MD`; không tạo .md mới; không báo "đạt" khi thiếu log/ảnh làm bằng chứng; không để scene legacy quay lại danh sách build.
