# HIT ME – PROMPT TỔNG CHO ANTIGRAVITY: dọn hết việc dang dở, đưa game chạy được sớm nhất

> Dán toàn bộ file này cho AG, kèm `hit-me-gameplay-rules-v1.2.md` (luật điều khiển, nguồn sự thật). File này **thay thế thứ tự** trong các prompt AG trước (`hit-me-ag-close-phase2-open-phase3.md`, `hit-me-ag-gesture-v2.md`, `hit-me-round6-entry-scene.md` phần B); nội dung chi tiết trong các file đó vẫn dùng làm tham khảo.
> **Mục tiêu mốc M1 (làm trước tiên):** trên điện thoại (iPhone Safari + Android Chrome) chơi được **một trận trọn vẹn 6 người (1 người + 5 bot)** bằng **đúng luật v1.2 và `Core/Phase1`**, nhân vật đúng cỡ, chữ có dấu, không lỗi. Sau M1 mới đóng Phase 2 và mở Phase 3.

## 0. Quy tắc cứng
1. **Dừng sau mỗi chặng** (A, B, C, D, E, F) và báo cáo theo mẫu có bằng chứng. Không tự chấm "đạt" khi thiếu log/ảnh/test. Chủ dự án duyệt rồi mới sang chặng sau; riêng chặng A→B→C làm liền nếu mỗi chặng đều có bằng chứng.
2. **Commit nhỏ sau mỗi bước** (`WIP:` nếu dang dở), làm trên nhánh riêng của AG. Không xóa `ASSET`, `SFX`, `Evidence`, `MD`.
3. **Không sửa logic combat ở `Core/Phase1`** (tính trúng, HP, kết thúc, bot). Được phép thay máy trạng thái cử chỉ (nhập liệu), `GameConfig` theo bảng quyết định, và lớp chuyển đổi tọa độ.
4. Không tạo .md mới ngoài các tài liệu đã có (`MD/01_GAMEPLAY_RULES.md`, `MD/PHASE2_REPORT.md`). Không asset giả/placeholder lẫn vào build chính.
5. Codex đã chuẩn bị sẵn trên nhánh `prep/codex` (đọc, không sửa nhánh đó): `tools/patches/core_cleanup.patch`, `tools/patches/entry_scene.patch`, `tools/patches/gesture_v2/` (máy trạng thái cử chỉ C# thuần + test + replay). **Hãy dùng các tệp đó làm gốc** thay vì viết lại. Nếu `git apply --check` hay biên dịch lỗi, báo chỗ lỗi.

## CHẶNG A – Chốt đường vào game đúng bộ luật (làm đầu tiên, blocker)
**Vấn đề đã xác nhận:** bản WebGL Phase 2 hiện vào `MainMatch.unity` (Level 0), chạy `MatchStateMachine` **cũ**, không dùng `Core/Phase1`. Build Settings có `Graybox.unity` lặp 2 lần và đóng gói 5 scene test. Các bằng chứng Phase 2 đã báo có thể thuộc luật cũ.
- **A1.** Áp dụng theo thứ tự: `git apply --3way tools/patches/core_cleanup.patch` rồi `entry_scene.patch` (Codex đã kiểm không xung đột). Mở Unity, không còn `Missing Script`/lỗi biên dịch; chuyển `CharacterData`/`WeaponData` giữ nguyên GUID.
- **A2.** Build Settings chỉ còn **một** scene vào game (dùng `MatchController` + `Core/Phase1`), không trùng lặp; `SceneSetupEditor`/`WebBuildPipeline` không được tạo lại `MainMatch` hay ghi Build Settings. Giữ `BotBrain`, `GameModels`, `MatchStateMachine` cũ ở nguyên chỗ (còn scene tham chiếu) nhưng **không còn scene build nào dùng chúng**; ghi chú để dọn ở Phase 3.
- **A3.** Chạy 3 test chốt chặn trong bản vá (build list đúng, scene đầu có `MatchController`, không có `MatchStateMachine`) và toàn bộ test hiện có.
- **A4.** Build WebGL lại. `Evidence/phase2/webgl-build.log` phải liệt kê **đúng một** scene ở Level 0. Chạy AutoPlay 10 trận không lỗi.
- **A5.** Lập bảng 12 mục DoD Phase 2: mục nào bằng chứng cũ chụp từ `MainMatch` thì đánh dấu **phải chụp lại**.
Báo cáo A, kèm đường dẫn log/ảnh, rồi sang B.

## CHẶNG B – Luật điều khiển v1.2 (cử chỉ)
Đọc kỹ `hit-me-gameplay-rules-v1.2.md` (mục 2–7). Tóm tắt: chạm xuống = đặt nhân vật; giữ rồi trượt = thước ngắm theo hướng; nhân vật không chạy theo ngón; **chạm chỗ khác = làm lại hoàn toàn (dời tức thì, hướng reset về mặc định, xóa thước cũ)**; kéo ngược dưới 4 mm = hủy lần ngắm; không trượt = ném thẳng từ mặt theo hướng mặc định (lên); hết 5 giây tự khóa và ném; chỉ nhận ngón đầu; chạm bắt đầu ngoài sân (HUD/nút) không đặt nhân vật.
- **B1.** Báo hiện trạng cử chỉ hiện nay trong 8–10 dòng (file nào xử lý, chỗ nào mâu thuẫn v1.2, hướng ngắm có đi qua `ScreenToGame` chưa).
- **B2.** Chép `tools/patches/gesture_v2/PlacementGestureV2.cs` (+ `GestureReplay.cs`) vào `Scripts/Core/Phase1`, thay máy trạng thái cũ (mã cũ chuyển `Assets/_Archive/`), chuyển test sang NUnit EditMode, **15 test của mục 6 file luật + các replay phải pass**. `DefaultFacing` dùng **một hằng số** trong Core, `ThrowFacing` dùng cùng giá trị. `minAimDrag` = 4 mm quy từ `Screen.dpi` (dự phòng 160), hướng tính qua `ScreenToGame`.
- **B3.** Nối vào màn trận (xem mục 4 file luật): đầu vòng sân trống + chấm mờ vị trí cũ + chuỗi "Chạm để đặt nhân vật"; vòng sáng ≥ 9 mm quanh điểm chạm; thước ngắm (vệt chấm + nón + mũi tên) theo hướng ném thật, vẽ trong sân; 3 chuỗi hướng dẫn VI/EN theo trạng thái; nhân vật khác không bao giờ hiện trong pha đặt; `commit` gửi `aimDir = null` khi chưa ngắm.
- **B4.** Bằng chứng: phát lại 6 tình huống (đặt+ngắm; đặt rồi chạm chỗ khác; ngắm rồi hủy; không chạm; không trượt; chạm ngoài sân), mỗi tình huống một ảnh `Evidence/phase2/gesture_*.png`, và `gesture_demo.mp4` ≤ 20 giây; ảnh 3 chuỗi hướng dẫn trên WebGL với chữ đủ dấu. Build WebGL lại (giữ template `Phase1Mobile` và cache-busting, `touch-action: none` cho Safari). **Chủ dự án sẽ thử trên điện thoại.**

## CHẶNG C – Đúng cỡ, đúng hình, đúng chuẩn mobile (vì chủ dự án đã chê nhiều lần)
Theo `hit-me-ag-prompt-responsive-arena.md` (bảng quyết định + bảng responsive). Kiểm tra từng mục, chỉ làm lại phần nào **chưa đạt có bằng chứng**:
- **C1. Cỡ nhân vật:** chiều cao ≈ 13% chiều cao sân, ≈ 11–12% chiều rộng, đúng như mockup. Xuất `Evidence/phase2/size_compare.png`: mockup bên cạnh ảnh chụp thật, cùng tỉ lệ.
- **C2. Dưới chân:** **hết ô vuông caro** (nguyên nhân: alpha giả trong PNG). Chẩn đoán alpha thật của từng sprite; bóng/vòng chân **tạo bằng code** (elip, tỉ lệ ≈ 0,6, ≈ 12,5% chiều rộng sân), không dùng phần "bóng" nằm trong PNG. Chụp cận cảnh chân của mọi nhân vật dùng trong trận (xem `Evidence/asset_qa` của Codex để biết file nào lỗi). **Không tự báo 100%**: đưa ảnh cận cảnh làm bằng chứng. Sprite nào alpha hỏng mà không sửa được bằng code → đưa vào danh sách "cần chủ dự án xuất lại" (nguồn: `Evidence/asset_qa/REPLACE_LIST.csv` và `P0_green/magenta.png` của Codex nếu đã có).
- **C3. Sân & phối cảnh:** `GameConfig` sân 9,0 × 11,4, `playerRadius` đúng giá trị cấu hình, `maxRounds = 5`, hòa HP → theo số lần trúng; homography `GameToScreen/ScreenToGame` khứ hồi < 0,01 (4 góc), có test.
- **C4. Responsive:** thử 1080×1920, 1179×2556, 1080×2400, 1080×2520, 1620×2160; Canvas Scaler 1080×2340 match 0,5; Safe Area; vùng chạm tính theo mm (`Screen.dpi`); nút ≥ 48 dp. Ảnh chụp mỗi độ phân giải.
- **C5. Đội hình:** 6 người lấy từ **7 nhân vật** (Nam Làng, Nữ Năng Động, Quậy Boy, Cô Ba, Anh Teen, Chó Vàng, Mèo Mun), **hết bản "tint"** thay chỗ nhân vật thiếu (dùng nhân vật thật; nhân vật chưa có animation đủ thì dùng pose tĩnh thật, không giả). Không có "pet/thú cưng" trong tên asset/prefab/code; Chó Vàng và Mèo Mun là nhân vật chơi được, ném bằng **chân**.
- **C6. Hiệu năng:** bản vá `ProjectileView` (không `Shader.Find` lúc chạy; dùng vật liệu gán sẵn/dùng chung, có thể dùng `tools/patches/projectileview_material.patch` nếu Codex đã làm); 0 cấp phát GC trong trận (không `new`/`GetComponent`/`Find`/LINQ trong `Update`), pool vũ khí/hiệu ứng; ASTC, atlas ≤ 2048; TMP static atlas đủ 134 ký tự tiếng Việt.
Báo cáo C với ảnh/số liệu (FPS trung bình, GC alloc từ Profiler hoặc log).

## CHẶNG D – Mốc M1: chơi được trên điện thoại
- **D1.** Build WebGL cuối (một scene, luật `Core/Phase1`, cử chỉ v1.2). Chạy AutoPlay 100 trận: không lỗi, thống kê số vòng trung bình (kỳ vọng ≤ 5 vì `maxRounds = 5`), có xếp hạng đúng.
- **D2.** Hướng dẫn chủ dự án thử: URL LAN/Render tạm, danh sách 8–10 điều cần kiểm tay trên iPhone và Android (đặt, dời, ngắm, hủy, không ngắm, hết giờ, kết thúc trận, chữ có dấu, cỡ nhân vật, chân không ô vuông).
- **D3.** Sửa lỗi chủ dự án báo trong lần thử (nếu có), rồi mới sang E.
**Cổng:** chủ dự án xác nhận M1 trên máy thật.

## CHẶNG E – Đóng Phase 2
- Bảng DoD 12 mục: mỗi mục ĐẠT/KHÔNG ĐẠT kèm đường dẫn bằng chứng (ảnh/log/test), không mục nào dựa vào bằng chứng chụp từ scene cũ.
- Cập nhật `MD/01_GAMEPLAY_RULES.md` (v1.2: thay mục điều khiển/pha đặt, giữ nguyên phần còn lại) và `MD/PHASE2_REPORT.md`; hai tệp phải **nằm trong repo** (đưa vào `docs/` nếu `MD/` đang ở ngoài repo; ghi rõ bản nào mới nhất).
- Kiểm tra an toàn: không file nào bị xóa trong `ASSET`/`SFX`/`Evidence`; diff `Scripts/Core/Phase1` so với tag `v0.1-graybox` chỉ gồm thay đổi được phép (`GameConfig`, cử chỉ, chuyển tọa độ, xếp hạng khi hết `maxRounds`).
- Gắn tag **`v0.2-rc1`** (không phải `v0.2-feel` cho tới khi chủ dự án duyệt). Báo commit, để Codex rebase server lên đúng commit này.

## CHẶNG F – Mở Phase 3 (chỉ làm Bước 0: rà soát, **dừng chờ duyệt**)
Theo `hit-me-ag-close-phase2-open-phase3.md` (Phase 3 Bước 0): rà soát, **không code**. Nội dung: 7 nhân vật chơi được; 3 mockup (Menu chính, Vào phòng, Biểu cảm) đối chiếu từng thành phần với asset hiện có; thiết kế màn hình theo wireframe chữ; dịch vụ giả sau interface (Auth, Profile, Room, Inventory) để sau thay bằng server/Supabase thật; **quy tắc bí mật biểu cảm** (người khác chỉ thấy biểu cảm trên thẻ danh sách người chơi trong pha đặt; bong bóng trên đầu nhân vật chỉ hiện sau khi lộ diện); quy tắc responsive/Safe Area như chặng C. Kết thúc bằng danh sách câu hỏi cho chủ dự án và kế hoạch bước 1. **Dừng, chờ duyệt.**

## Mẫu báo cáo cuối mỗi chặng/bước
```
CHẶNG/BƯỚC: x
ĐÃ LÀM: ...
FILE THAY ĐỔI: ...
BẰNG CHỨNG: (đường dẫn log/ảnh/video/test + "Passed! N/N")
KHÔNG ĐẠT / CÒN THIẾU: ...
CHỜ CHỦ DỰ ÁN: ...
SẴN SÀNG SANG: x+1? (có/không)
COMMIT: <hash>
```

## Điều cấm (nhắc lại)
Không sửa logic combat ở `Core/Phase1`; không xóa `ASSET`/`SFX`/`Evidence`/`MD`; không để scene legacy (`MainMatch`) quay lại danh sách build; không tạo .md mới; không dùng asset giả; không để nhân vật chạy theo ngón khi ngắm; không hiện nhân vật/hướng ngắm của người khác trong pha đặt; không báo "đạt" khi thiếu bằng chứng; không dùng bằng chứng chụp từ scene cũ.
