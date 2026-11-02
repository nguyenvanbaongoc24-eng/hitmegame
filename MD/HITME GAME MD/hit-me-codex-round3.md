# HIT ME – LỆNH CHO CODEX, ĐỢT 3 (xếp theo ưu tiên, làm lần lượt)

> Dán toàn bộ file này cho Codex, kèm `hit-me-gameplay-rules-v1.2.md` (bản mới nhất, đã sửa: **chạm chỗ khác = làm lại hoàn toàn, hướng reset về mặc định**). File này **thay** thứ tự trong `hit-me-codex-next-tasks.md` và `hit-me-codex-gesture-v2.md`; nội dung chi tiết P3, P4, P5, P6 ở file cũ vẫn có hiệu lực.
> **Commit nhỏ, thường xuyên** (`WIP:` nếu dang dở). Nếu sắp hết token: làm R1 → R2, commit, ghi "Đang dở: ..." vào `server/README.md` rồi dừng.

## Đánh giá báo cáo vừa rồi (để bạn biết đã được ghi nhận gì)

| Việc | Kết quả | Nhận xét |
|---|---|---|
| P1 bản đồ hai lõi luật | **Đạt**, có `git grep` làm bằng chứng | Kết luận quan trọng: `MatchController` đang chạy `Phase1/MatchState` + `Phase1BotBrain`; 5 file `Core/*.cs` cũ (có `UnityEngine`) là bản trùng, không phải nguồn luật. Chấp nhận: **`Core/Phase1` là nguồn luật duy nhất.** |
| P2 vệ sinh repo | **Đạt** (đề xuất, chưa áp dụng) | Phát hiện thật: build WebGL/Windows đang bị commit trong `Evidence`, `.git` ≈ 371 MiB, chưa có LFS, `MD/PHASE2_REPORT.md` nằm ngoài repo. File lớn nhất 38,7 MB (chưa chạm giới hạn 100 MB). |
| P3 server | **Chưa đạt**, Codex tự báo đúng là WIP | Mới có skeleton `/ws` + `/health`, build 0 lỗi. Room Authority, commit bí mật, reconnect, golden vectors, test: **chưa có**. Không tính là Phase 4. |
| Q1 cử chỉ v1.2 | **WIP trung thực, chưa đạt** | Đã viết `PlacementGestureV2.cs` + `GestureReplay.cs`, nhưng `dotnet test` lỗi thiếu `xunit.abstractions.dll`, **chưa có pass nào**, chưa có 6 replay. Cảm ơn vì không báo "xong" khi chưa chạy được. |

## 0. Quy tắc (giữ nguyên)
Không mở Unity; không sửa `unity-client/Assets`, `ProjectSettings`, `Packages`, `MD/`; làm trên `prep/codex`; không commit lên `main`; không viết lại lịch sử git; không đổi luật; không sửa Core; không tạo .md mới ngoài `server/README.md`. **Chỉ báo "pass" khi dán được dòng kết quả `Passed! N/N` của `dotnet test`.**

---

## R1 – Sửa test chạy được, hoàn thành Q1 (ưu tiên 1)

**Triệu chứng:** `xunit.abstractions.dll` thiếu dù đã xóa cache và restore lại.

1. **Chẩn đoán trước, tối đa 2 lần thử** (ghi lại từng lần):
   - Xem `GestureV2.Tests.csproj`: có đủ ba gói cùng nhóm phiên bản `Microsoft.NET.Test.Sdk` (17.11.x), `xunit` (2.9.x, **gói gộp**, không chỉ `xunit.core`), `xunit.runner.visualstudio` (2.8.x) không? Có `PackageReference` bị thêm `ExcludeAssets`/`PrivateAssets=all` làm mất DLL không? Có `<Reference>` nào trỏ thẳng vào một DLL xunit không?
   - Xóa `bin/` và `obj/` của dự án test, `dotnet restore --force --no-cache`, `dotnet test --nologo -v n`. Thử thêm `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>`.
2. **Nếu sau 2 lần vẫn lỗi: chuyển sang NUnit** (`NUnit` 4.x + `NUnit3TestAdapter` 4.6.x + `Microsoft.NET.Test.Sdk`). Đây là lựa chọn **tốt hơn** vì EditMode của Unity cũng dùng NUnit, AG chỉ việc chép test sang không phải viết lại. Nếu phải đổi, đổi luôn cho `server.Tests` để cả repo dùng một khung test.
3. Thêm 6 tệp `replays/*.json` (đặt+ngắm; đặt rồi **chạm chỗ khác → hướng reset**; ngắm rồi hủy (kéo về dưới 4 mm → hướng mặc định); không chạm; không trượt; chạm bắt đầu ngoài sân) và để `GestureReplay` phát lại, so với kết quả mong đợi trong chính tệp JSON.
4. **Cập nhật theo luật v1.2 mới (đã đổi so với lần trước):**
   - Chạm mới ở Q: vị trí = Q, hướng = **mặc định**, thước cũ bị xóa, `gestureId` tăng (không còn "giữ hướng cũ").
   - Kéo ngược dưới ngưỡng trước khi nhả: hủy lần ngắm, hướng về mặc định (vì lần chạm này chưa có hướng nào khác).
   - Test 5, 6, 7 trong mục 6 của file luật là bản đã sửa; làm đúng theo đó.
5. **15 test** của mục 6 + biên (NaN/∞, nhiều `pointerId`, sự kiện sau `Lock()`), **tất cả phải pass**. Dán kết quả `dotnet test` (số test, thời gian).
6. **`DefaultFacing`:** hiện là `Vec2.Up` trong tệp patch. Không được sửa Core, nên viết đề xuất tối thiểu trong chú thích đầu `PlacementGestureV2.cs` (một hằng số dùng chung, ví dụ `Phase1Defaults.DefaultFacing`, ở `Core/Phase1`) và đảm bảo **cả cử chỉ lẫn `ThrowFacing` của server đều dùng cùng một giá trị**. Thêm một test xác nhận hai nơi bằng nhau (test server khi S2 xong).

Commit `prep/codex`, báo cáo theo mẫu.

## R2 – Bản vá dọn Core trùng lặp (không áp dụng, chỉ tạo bản vá)

Mục tiêu: **một** lõi luật. Bạn đã kết luận 5 tệp cũ trùng/không dùng. Chuẩn bị bản vá cho AG áp dụng, **không đụng `unity-client/Assets` trên nhánh `prep/codex`**:

1. **Kiểm tra an toàn trước (bắt buộc):** với từng tệp `Core/BotBrain.cs`, `CharacterData.cs`, `WeaponData.cs`, `GameModels.cs`, `MatchStateMachine.cs`: đọc GUID trong `.meta`, rồi `git grep` GUID đó trong `*.unity`, `*.prefab`, `*.asset`, `*.meta`. Liệt kê **tệp nào đang được scene/prefab/asset tham chiếu** (đặc biệt `CharacterData`/`WeaponData` là ScriptableObject, xóa nhầm sẽ làm mất liên kết dữ liệu).
2. Trong **một worktree tạm** (thư mục khác, nhánh tạm, xóa sau khi xong), thử: `git mv` `CharacterData`, `WeaponData` (kèm `.meta`, **giữ nguyên GUID**) sang `Scripts/Game/Data/`, đổi namespace nếu cần; tệp trùng không ai dùng (`BotBrain`, `MatchStateMachine`, `GameModels` nếu xác nhận không ai tham chiếu) chuyển vào `Assets/_Archive/` (kèm `.meta`, **không xóa hẳn**).
3. Kiểm tra bằng `git grep`/phân tích: không còn tham chiếu tới tên lớp đã chuyển trong `Scripts/Game`, `Scripts/Editor`, `Tests`; asmdef (`noEngineReferences`) của `Core` vẫn đúng.
4. Xuất `tools/patches/core_cleanup.patch` bằng `git diff` (hoặc `format-patch`) và danh sách ở đầu bản vá: tệp nào chuyển đi đâu, tệp nào được scene tham chiếu và cách giữ liên kết. AG áp dụng bằng `git apply --3way` rồi mở Unity xác nhận không có lỗi biên dịch/`Missing Script`. Xóa worktree tạm. **Không áp dụng bản vá lên `prep/codex`.**
5. Nếu bước 1 cho thấy một tệp đang được scene tham chiếu mà bạn không chắc cách giữ liên kết: **không đoán**, ghi vào báo cáo để chủ dự án quyết.

## S1–S4 – Server Phase 4 (P3 chia nhỏ, mỗi bước một commit có test)

Làm sau R1–R2. Chi tiết yêu cầu vẫn như mục P3 của `hit-me-codex-next-tasks.md`. **Chỉ lấy luật từ `Core/Phase1`.** Mỗi bước phải có `dotnet test` xanh trước khi sang bước sau; không gộp.

- **S1 – Phòng + giao thức:** tạo/vào/rời phòng (mã `HM`+4 số, kiểm tra trùng), 2–6 người, chủ phòng bắt đầu; giao thức JSON có `v` + `type` (`join`, `ready`, `commit`, `leave` / `room_state`, `round_start`, `reveal`, `round_result`, `match_end`, `error`); `room_state` mang cấu hình trận từ `GameConfig`; thông điệp ≤ 1 KB, ≤ 20 tin/giây/kết nối. Test: vòng đời phòng, `type` lạ, JSON hỏng, thông điệp quá lớn.
- **S2 – Vòng chơi + bí mật:** `commit` giữ kín; khóa khi hết `placeSeconds` hoặc tất cả `ready`; tính bằng `RoundResolver` của Core; phát `reveal` + `round_result`; `match_end` khi hết `maxRounds` hoặc còn ≤ 1 người; thời gian qua `TimeProvider` (đồng hồ giả trong test). `commit` nhận `aimDir = null` → áp `ThrowFacing` bằng **cùng `DefaultFacing`** như R1. Test bắt buộc: **giữ bí mật** (quét toàn bộ tin gửi cho A trước khi khóa không chứa vị trí/hướng của B), tương đương Core theo `seed`, hết giờ → `ThrowFacing`, `commit` trễ → bỏ qua, hai `commit` cùng vòng (lần sau đè lần trước cho đến khi khóa).
- **S3 – Vào lại + cô lập lỗi:** `reconnectToken`, giữ chỗ 30 giây, quá hạn thì loại; hook AFK để `TODO` rỗng; một client chậm/đứt không treo năm client còn lại. Test: mất kết nối 10 giây vào lại đúng chỗ, quá 30 giây bị loại, client không đọc tin không chặn vòng.
- **S4 – Xác thực đầu vào + mô phỏng:** `NaN`/∞, vị trí ngoài sân (kẹp vào sân), hướng độ dài 0/không hữu hạn (coi như không ngắm); `/healthz`, đọc `PORT`; `Dockerfile` + `render.yaml` nháp (không triển khai); mô phỏng 20 trận × 6 bot: 0 ngoại lệ, 0 treo. Hoàn thiện `server/README.md` (≤ 1,5 trang, ghi rõ commit `Core/Phase1` đang dùng).

## P4 – Vector vàng (sau S2)
Như mục P4 cũ: 200 vòng mẫu từ `Core/Phase1` thành `server/Tests/golden/rounds.json` (kèm `seed`, phiên bản `GameConfig`, các trường hợp biên). Test server dùng nó; về sau test EditMode của Unity đọc cùng tệp để chứng minh **client và server không lệch luật**.

## R3 – Kế hoạch dọn repo dạng kịch bản (chỉ viết, chưa áp dụng)
Từ P2 đã có đề xuất `.gitignore`/`.gitattributes`. Bổ sung `tools/repo_hygiene/apply_plan.ps1` (kèm chú thích từng dòng, **mặc định chạy chế độ `-WhatIf`/dry-run, không tự chạy**):
1. `git rm -r --cached` các thư mục build đang bị theo dõi (`Evidence/**/WebGLBuild`, `Evidence/**/WindowsBuild`, …) nhưng **giữ nguyên trên đĩa**; thêm vào `.gitignore`.
2. Bật Git LFS cho `*.png *.jpg *.psd *.wav *.mp3 *.ogg` **chỉ cho commit mới** (không `git lfs migrate`, không viết lại lịch sử; việc làm nhỏ `.git` 371 MiB để chủ dự án quyết định riêng).
3. Đưa `MD/` vào repo: đề xuất đặt vào `docs/` (đã có `docs/`), chỉ liệt kê hash các bản `PHASE2_REPORT.md` khác nhau và **bản nào mới nhất**, không tự chọn.
Ghi rõ chỗ nào có rủi ro mất dữ liệu nếu chạy sai. Chủ dự án/AG duyệt rồi mới chạy.

## P5 – Danh sách thay asset (nếu còn token)
Như mục P5 cũ: thêm cột `uu_tien` (P0 = Nam Làng, Anh Nón Lá, Chó Vàng, Mèo Mun, dép, bóng/vòng chân, hiệu ứng trúng đòn; P1 = 3 nhân vật còn lại + UI Home/Vào phòng/Biểu cảm; P2 = vũ khí khác và phần còn lại), sắp theo `uu_tien` rồi mức độ lỗi, xuất `Evidence/asset_qa/contact/P0_green.png` và `P0_magenta.png`, chỉ điền `ban_da_lam_sach` khi bản đó thật sự đạt.

## P6 – Bản vá `ProjectileView` (nếu còn token)
Như mục P6 cũ: `tools/patches/projectileview_material.patch` (vật liệu gán sẵn trong prefab hoặc một `Material` dùng chung, không `Shader.Find` lúc chạy). Bỏ qua nếu AG đã sửa.

---

## Báo cáo cuối mỗi việc (bắt buộc dùng mẫu)
```
VIỆC: R1 | R2 | S1 | S2 | S3 | S4 | P4 | R3 | P5 | P6
ĐÃ LÀM: ...
FILE: ...
BẰNG CHỨNG: (lệnh + dòng kết quả, ví dụ "Passed! 21/21")
KHÔNG ĐẠT / PHÁT HIỆN QUAN TRỌNG: ...
CHỜ AG / CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```

## Điều cấm (nhắc lại)
Không mở/sửa Unity và `unity-client/Assets`; không commit lên `main`; không viết lại lịch sử git; không sửa/xóa `ASSET`, `SFX`, `SpriteSources`; không đổi luật; không sửa Core; không triển khai Render/Supabase thật; không tạo .md ngoài `server/README.md`; không báo "pass" khi không có kết quả `dotnet test`.
