# HIT ME – PROMPT TỔNG CÒN LẠI CHO ANTIGRAVITY (bản v2, thay thế mọi danh sách việc AG trước đó)

> Dán toàn bộ file này cho AG, kèm `hit-me-gameplay-rules-v1.2.md`. File này **gộp** các việc còn dang dở từ `hit-me-ag-master-prompt.md`, `hit-me-round8…`, `hit-me-round9…`, `hit-me-round13…`, `hit-me-round19…` (phần B), `hit-me-round20…` (phần B) và `hit-me-weapon-sprite-fix.md`. Nếu có chỗ mâu thuẫn, **file này thắng**. Các file cũ chỉ còn làm tài liệu tham khảo chi tiết.
> Mục tiêu: **đóng Phase 2 (tag `v0.2-rc1`)** và **mở Phase 3 bằng Bước 0 (chỉ rà soát)** càng sớm càng tốt, bằng chứng đủ để chủ dự án và Codex kiểm độc lập.

## 0. Quy tắc cứng
1. Commit nhỏ sau mỗi bước (`WIP:` nếu dang dở), nhánh `feature/ui-ux-production`. Không viết lại lịch sử git.
2. **Không xóa** `ASSET`, `SFX`, `Evidence`, `MD`, `ASSET HITME FINAL`. **Không build đè** thư mục `Builds/WebGL_20261029_942ee08` và **không tắt cổng 8090** cho tới khi chủ dự án báo thử xong; build mới ra thư mục mới (`Builds/WebGL_<ngày>_<commit>`), cổng khác.
3. Không báo "đạt" khi thiếu bằng chứng (số đo, ảnh, log, `test-results*.xml`). **Mục "KHÔNG ĐẠT / CÒN THIẾU" phải điền thật**, không ghi "Không có" nếu còn tồn đọng.
4. Mỗi tệp bằng chứng mới vào `Evidence/` phải có dòng trong `Evidence/MANIFEST.csv` (sha256, commit, scene, bản build, độ phân giải, lệnh tạo) và tệp provenance `.json`.
5. Chỉ **một** lõi luật: mọi luật tính toán (trúng, HP, kết thúc, **xếp hạng, tie-break**) nằm ở `Core/Phase1` (C# thuần). Lớp `Game/` chỉ hiển thị và điều khiển.
6. Không tạo .md mới ngoài `docs/PHASE2_REPORT.md` và `docs/01_GAMEPLAY_RULES.md`.

## 1. Bảng trạng thái (theo báo cáo P1–P4, commit `fe2bb22`, `test-results-p4.xml` 77/77)

| # | Hạng mục | Trạng thái | Còn thiếu gì |
|---|---|---|---|
| 1 | Cuối trận → bảng kết quả 6 dòng, ẩn SẴN SÀNG/Banner/WeaponPill, Chơi lại dọn đạn | **Đã làm (AG báo)**, có ảnh `p4_05_matchend`, `p1_matchend_after` | Xem **S1**, **S2** |
| 2 | Tia ngắm/lộ diện theo pha (chỉ người chơi chính khi Placing) | **Đã làm (AG báo)** | Ảnh trên bản WebGL thật; xem **S3** |
| 3 | HUD 3 hàng + thẻ HP 2×3 + nhãn vũ khí + cột portrait | **Đã làm (AG báo)**, ảnh Editor 1080×2340 và desktop 1920×1080 | Xem **S3**, **S5** (5 độ phân giải, điện thoại thật) |
| 4 | Bằng chứng trực quan có watermark `build fe2bb22` | **Có** (ảnh Camera.Render trong Editor) | **Chưa có bản WebGL của `fe2bb22`** để chủ dự án thử |
| 5 | 460 B/khung hình → 0 | **Chưa làm** | **S6** |
| 6 | D0: thước ngắm trùng đường bay, độ chính xác chạm, biến đổi tọa độ, bảng nhân vật ↔ `ASSET HITME FINAL`, ảnh cận cảnh chân, ảnh trước/sau `ArenaView` | **Chưa có** | **S7** |
| 7 | Bảng số test duy nhất (51/52/59/62/72/77) | **Chưa có** | **S8** |
| 8 | `MANIFEST.csv` đủ dòng (kể cả `test-results-p0/p4.xml`, `a7b2574`, `fe2bb22`) | **Chưa xác nhận** | **S8** |
| 9 | Danh sách tồn đọng chuyển phase sau | **Chưa có** | **S9** |
| 10 | Chạy công cụ audit của Codex, mọi dòng ĐẠT (trừ tồn đọng được duyệt) | **Chưa chạy** | **S10** |
| 11 | Build sạch mới + AutoPlay 10 trận trên bản build | **Chưa** | **S4** |
| 12 | Phase 3 Bước 0 (rà soát, không code) | **Chưa làm** | **S11** |
| 13 | Tài liệu `docs/` + tag `v0.2-rc1` | **Chưa** (chờ chủ dự án + Codex X5) | **S12** |

## 2. Việc cần làm (theo thứ tự)

### S1 – Chuyển xếp hạng/tie-break về `Core/Phase1` (quan trọng nhất, sửa kiến trúc)
`BuildRankingsAndWinners` và `ComparePlayersForRanking` đang nằm trong `MatchController` (lớp Unity). **Server và client phải dùng cùng một hàm xếp hạng** (Codex đã nối server vào `Core/Phase1`).
1. Tạo trong `Core/Phase1` (C# thuần, không `UnityEngine`): `MatchRanking` (ví dụ `Rank(IReadOnlyList<PlayerResult>) → RankedPlayer[]`, `Winners`) với **luật chốt**:
   - Xếp hạng người **còn sống** trước, rồi theo `Hp` giảm dần, rồi `HitsLanded` giảm dần.
   - Bằng cả ba (cùng sống, cùng HP, cùng số lần trúng) → **đồng hạng**; thắng chung nếu hạng 1 có nhiều người. (Việc ưu tiên "người chơi chính"/`Name` chỉ được dùng để **sắp thứ tự hiển thị** giữa những người đồng hạng, **không** được quyết định ai thắng.)
   - Người **đã bị loại**: xếp theo **thứ tự bị loại** (bị loại muộn hơn → hạng cao hơn); cùng vòng bị loại thì theo `HitsLanded`, rồi đồng hạng.
2. Test C# thuần (NUnit EditMode + chạy được ngoài Unity): hết `maxRounds` còn nhiều người sống; đồng HP khác số lần trúng; đồng cả ba; người bị loại cùng vòng; 6 người; sau trận chỉ còn 1 người sống; không còn ai sống (cùng loại ở vòng cuối) — mỗi ca nêu hạng mong đợi.
3. `MatchController` chỉ **gọi** `MatchRanking` và hiển thị. Xóa bản trùng. `test-results` mới ghi số test.
4. Báo commit để Codex đối chiếu `server/ServerMatch` (cùng kết quả xếp hạng trên vector vàng).

### S2 – Chuỗi trạng thái cuối trận (kiểm cho chắc)
Test điều khiển trạng thái: vòng 5 kết thúc (hoặc còn ≤ 1 người) → `MatchEnd` → bảng kết quả; **không quay lại pha đặt**; `CHƠI LẠI` bắt đầu từ vòng 1 với trạng thái sạch (HP, đạn, tia, lựa chọn); `VỀ TRANG CHỦ` dọn sạch. Có ảnh `MatchEnd` trên **bản WebGL thật** (S4).

### S3 – Sửa hai điểm lệch so với luật/mockup
1. **HP luôn công khai:** thẻ HP đang hiện "HP: ???" ở pha Placing. Theo mockup và luật, **HP của mọi người luôn thấy** (chỉ **vị trí và hướng ngắm** bị ẩn). Hiển thị lại HP (tim hoặc `x/3`) ở mọi pha; "ĐANG ẨN" chỉ cho vị trí/hướng nếu cần.
2. **Placing:** xác nhận bằng ảnh WebGL: chỉ nhân vật + vòng + thước của **chính mình**; 5 người kia không thấy; thẻ danh sách vẫn hiện tên/HP (và biểu cảm trên thẻ nếu có).

### S4 – Build WebGL sạch mới (thư mục mới, không đụng bản đang thử)
`Builds/WebGL_<ngày>_<commit>`; log `Build completed … Succeeded`; mọi tệp cùng một khoảng thời gian; dấu vân tay `build <commit> · <ngày giờ> · Graybox` hiện ở góc; sha256 vào MANIFEST; phục vụ ở **cổng khác** (ví dụ 8091), đưa lại địa chỉ LAN. AutoPlay 10 trận trên bản build (0 lỗi, 0 treo); đo GC/FPS 60 giây trên bản build (S6). Chụp ảnh **trên bản WebGL thật** cho: Placing, Reveal, Throwing, RoundEnd, MatchEnd ở **5 độ phân giải** (1080×1920, 1179×2556, 1080×2400, 1080×2520, 1620×2160); mỗi ảnh có `.json` provenance và dòng MANIFEST.

### S5 – Chủ dự án thử điện thoại
Đưa địa chỉ LAN bản mới; chủ dự án thử (8 điều như `round19` phần C). AG **không** build đè bản đang thử. Phản hồi của chủ dự án quyết định có sửa gì thêm.

### S6 – 460 B/khung hình → 0
Dùng Profiler/Memory Profiler hoặc bộ đếm tự đo: liệt kê nguồn cấp phát (nghi: `string`/`ToString`/`$""`, `new`, LINQ, `List` tạm, boxing, `Debug.Log`, `GetComponent`, `SetText`, `RaycastAll`, `Vector` mảng). Sửa; đo lại **60 giây liên tục** trên bản WebGL, ghi FPS trung bình/thấp nhất, môi trường (máy, trình duyệt). Nếu không về 0, ghi rõ nguồn còn lại, số byte, lý do — mục này nằm ở KHÔNG ĐẠT, không được bỏ qua.

### S7 – D0 còn thiếu (mỗi mục có bằng chứng)
1. **Thước ngắm trùng đường bay:** 8 hướng (mỗi 45°) × 3 vị trí (gần góc, giữa, gần tường): ảnh có thước và vệt bay thật chồng nhau, đo góc lệch trên màn hình **≤ 3°**; `Evidence/phase3/aim_vs_flight.png` + bảng số `aim_vs_flight.txt`. Lệch thì sửa quy đổi tọa độ.
2. **Độ chính xác chạm:** chạm pixel P → tâm chân nhân vật trong ≤ 4 px của P, ở 5 độ phân giải; bảng số đo.
3. **Mô tả biến đổi tọa độ** (≤ 8 dòng): sân hình chữ nhật hay hình thang trong tranh nền; camera; có homography không; dẫn chứng bằng số đo 1 và 2.
4. **Bảng nhân vật:** `nhân vật trong game ↔ thư mục nguồn trong D:\HITME GAME\ASSET HITME FINAL (NAM LÀNG, ANH NÓN LÁ, MAI HỒNG, ANH NÚI…) ↔ tệp đang dùng ↔ trạng thái (animation đầy đủ/pose tĩnh/trích từ ảnh bảng)`; chỉ rõ 6 người trong trận chọn từ 7 nhân vật thế nào.
5. **Ảnh cận cảnh chân** của 5 nhân vật pose tĩnh trên nền xanh lá và magenta (không ô caro, không răng cưa); nhân vật nào lỗi thì đưa vào tồn đọng (S9).
6. **Ảnh trước/sau** cho thay đổi `ArenaView.cs`.
7. **`Test16`** (cử chỉ): liệt kê `Assert`; đủ 12 replay (từ `tools/patches/gesture_v2_sync/`), kiểm từng bước, có ca: chạm chỗ khác → hướng reset mặc định, hủy ngắm → mặc định, chạm ngoài sân, khóa lúc đang giữ.

### S8 – Số test và MANIFEST
1. **Bảng số test duy nhất** (trong `docs/PHASE2_REPORT.md`): `lần | commit | tệp XML | tổng | pass | fail`, lấy từ XML thật; giải thích 51 / 52 / 59 / 62 / 72 / 77.
2. **MANIFEST đủ dòng** cho **mọi** tệp trong `Evidence/` (kể cả `test-results-p0.xml`, `test-results-p4.xml`, ảnh `p1_*`, `p2_*`, `p4_*`, commit `a7b2574`, `fe2bb22`); chạy kiểm tự động: "tệp trong `Evidence` không có dòng MANIFEST" phải rỗng.

### S9 – Danh sách tồn đọng chuyển sang phase sau (chủ dự án duyệt)
Mỗi dòng: mô tả, mức ảnh hưởng, phase đề xuất. Tối thiểu: dép bay bị dẹt/cụt phần đầu (so khung gốc `ASSET WEAPONS\DÉP\sprites`, nguyên nhân a–e ở `hit-me-weapon-sprite-fix.md` W1: **chỉ chẩn đoán + ảnh đối chiếu, chưa sửa**), 5 nhân vật chỉ pose tĩnh, sprite trích từ ảnh bảng cần thay bằng tệp gốc, 9 cảnh báo nullable của Core, `MainMatch`/`MatchStateMachine` legacy chưa dọn, sàn `ArenaView` (nếu còn).

### S10 – Chạy công cụ audit của Codex
Lấy `tools/audit/phase2_audit.ps1` từ nhánh `prep/codex`. Chạy trên commit cuối và bản build mới: **mọi dòng ĐẠT**, trừ tồn đọng đã nêu ở S9 và được duyệt. Dán toàn bộ bảng kết quả; lưu `Evidence/audit/phase2_audit_<commit>.txt` (có dòng MANIFEST).

### S11 – Phase 3 Bước 0 (chỉ rà soát, không code; làm song song, dừng chờ duyệt)
Theo `hit-me-ag-close-phase2-open-phase3.md` (Bước 0) cập nhật như `round19`: bảng đối chiếu 3 mockup (Menu chính, Vào phòng, Biểu cảm) với asset trong `ASSET HITME FINAL` (`HITME_UI_KIT_CLEAN`, `UI_KIT_EXTRACTED`, `HITME_AIM_KIT_V1`, …); wireframe chữ từng màn hình; **dịch vụ giả sau interface** (`IAuthService`, `IProfileService`, `IRoomService`, `IInventoryService`) khớp giao thức server (`join` có `token`, `room_ready`, `start`, `lock_ready`, `emote`; phòng tự bắt đầu sau 3 giây khi tất cả sẵn sàng; AFK = 2 vòng liên tiếp không `commit`, bị loại + cấm 120 giây); **quy tắc bí mật biểu cảm** (người khác chỉ thấy biểu cảm trên thẻ người chơi trong pha đặt; bong bóng trên đầu nhân vật chỉ sau lộ diện); quy tắc responsive/Safe Area; danh sách câu hỏi cho chủ dự án; kế hoạch bước 1. **Dừng, chờ duyệt.**

### S12 – Đóng Phase 2
Khi **đủ 4 điều kiện**: (1) chủ dự án thử điện thoại đạt, (2) S1–S10 có bằng chứng, (3) Codex X5 đạt, (4) tồn đọng được duyệt: cập nhật `docs/PHASE2_REPORT.md` và `docs/01_GAMEPLAY_RULES.md` (v1.2, **nằm trong repo**), gắn tag **`v0.2-rc1`**, báo commit để Codex rebase server.

## 3. Mẫu báo cáo (điền thật, một báo cáo cho mỗi cụm S)
```
BƯỚC: S1 | S2 | ...
ĐÃ LÀM: ...
FILE THAY ĐỔI: ...
BẰNG CHỨNG: (đường dẫn + số đo + dòng MANIFEST + test-results XML)
KHÔNG ĐẠT / CÒN THIẾU: (liệt kê thật)
CHỜ CHỦ DỰ ÁN: ...
COMMIT: <hash>
```

## 4. Điều cấm (nhắc lại)
Không để luật xếp hạng/tie-break nằm ngoài `Core/Phase1`; không ẩn HP của người chơi khác; không hiện nhân vật/hướng ngắm của người khác trong pha đặt; không sửa logic combat ngoài danh sách cho phép (`GameConfig`, cử chỉ, chuyển tọa độ, xếp hạng khi hết `maxRounds`); không build đè bản đang thử; không xóa `ASSET`/`SFX`/`Evidence`/`MD`/`ASSET HITME FINAL`; không dùng ảnh Editor thay bằng chứng bản WebGL ở S4; không báo "đạt" khi thiếu bằng chứng.
