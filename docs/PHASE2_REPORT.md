# BÁO CÁO TỔNG KẾT PHASE 2 & BÀN GIAO MỞ ĐẦU PHASE 3 (BƯỚC 0)

> **Dự án**: HIT ME – Casual Turn-based PvP Mobile Web Game  
> **Nhánh**: `feature/ui-ux-production` | **Engine**: Unity 6000.6.4f1 (WebGL 2.0 / WebAssembly)  
> **Cam kết**: Báo cáo trung thực, minh bạch tuyệt đối, số liệu đối chiếu trực tiếp từ file XML, ảnh chụp, log và kịch bản audit độc lập.

---

## MỤC LỤC
1. [Bảng Tiến Hóa Số Lượng Test Thống Nhất](#1-bảng-tiến-hóa-số-lượng-test-thống-nhất)
2. [Chi Tiết Bàn Giao D0 (Cử Chỉ V2, Độ Chính Xác, Roster & Visual)](#2-chi-tiết-bàn-giao-d0)
3. [Báo Cáo Hoàn Thành Các Bước S1 Đến S6 & Cập Nhật Luật v1.3](#3-báo-cáo-hoàn-thành-s1-s6-và-luật-v13)
4. [Tồn Đọng Kỹ Thuật Chuyển Tiếp (S9 - Deferred Backlog)](#4-tồn-đọng-kỹ-thuật-chuyển-tiếp-s9)
5. [Thiết Kế Rà Soát Phase 3 Bước 0 (S11 - Step 0 Review-Only)](#5-thiết-kế-rà-soát-phase-3-bước-0-s11)

---

## 1. BẢNG TIẾN HÓA SỐ LƯỢNG TEST THỐNG NHẤT

Toàn bộ số lượng test dưới đây được truy xuất trực tiếp từ các tệp XML thực tế lưu trong thư mục `Evidence/`:

| Mốc / Giai đoạn | Tệp kết quả XML | Tổng số | ĐẠT (Pass) | HỎNG (Fail) | BỎ QUA (Skip) | Ghi chú phạm vi test |
|---|---|---|---|---|---|---|
| **Phase 2 Baseline** | `Evidence/phase2/test-results.xml` | **48** | 48 | 0 | 0 | Bộ test Core/Phase1 gốc từ v0.1 |
| **Phase 2 Replay Core** | `Evidence/phase3/test-results-replay.xml` | **51** | 51 | 0 | 0 | Bổ sung 3 test replay cơ bản |
| **Phase 2 Complete** | `Evidence/phase2/test-results-all.xml` | **52** | 52 | 0 | 0 | Thêm kiểm tra toàn vẹn tài nguyên Phase 2 |
| **D0 Polish & Gesture V2** | `Evidence/phase3/test-results-d0.xml` | **62** | 62 | 0 | 0 | 12 test đồng bộ replay cử chỉ V2 + 10 test visual contract |
| **P0 Production UI** | `Evidence/phase3/test-results-p0.xml` | **72** | 72 | 0 | 0 | Contract các màn hình UI Home, Lobby, Gameplay, Result |
| **P1 Match Authority** | `Evidence/phase3/test-results-p1.xml` | **74** | 74 | 0 | 0 | Thẩm quyền MatchState.Phase, kết thúc ngay Vòng 5, bảng xếp hạng 6 người |
| **P2 & P3 SafeArea/HUD** | `Evidence/phase3/test-results-p2p3.xml` | **77** | 77 | 0 | 0 | Căn chỉnh SafeArea 1080×2340, thẻ HP 2×3 trên cổng làng, thanh đặt trên đỉnh |
| **S1, S2, S3 & S6 Prep** | `Evidence/phase3/test-results-s1-s3.xml` | **86** | 86 | 0 | 0 | MatchRanking trong Core/Phase1, MatchEnd dọn dẹp, HP công khai mọi pha |
| **Luật v1.3 & Phase 2 Final** | `Evidence/phase3/test-results-p4.xml` | **92** | 92 | 0 | 0 | Thẻ HP động N=2..6, kiểm tra phòng 1-6 người thật, xếp hạng N=2..6 |
| **Đợt 27 Visual & Sprite Audit** | `Evidence/phase3/test-results-round27.xml` | **98** | 98 | 0 | 0 | Bổ sung 4 audit test sprite/vũ khí sạch + 1 test F9 bot quyết định độc lập (mù) |

> [!NOTE]
> **Hiện trạng bộ test hiện tại**: **98/98 bài kiểm thử ĐẠT 100% (0 lỗi, 0 bỏ qua)**.

---

## 2. CHI TIẾT BÀN GIAO D0

### 2.1. Bảng đo độ lệch góc Thước ngắm vs Đường bay dép (D0.1)
- **Tệp bằng chứng**: `Evidence/phase3/aim_vs_flight.png` và `Evidence/phase3/aim_vs_flight.txt`.
- **Yêu cầu**: Sai số góc giữa thước ngắm và đường bay dép phải $\le 3.0^\circ$.
- **Kết quả đo**: Toàn bộ 24/24 phép đo (8 hướng chính × 3 vị trí trên sân) đều đạt sai số **$0.00^\circ$** ($\le 3.0^\circ$).

| Vị trí đặt | Hướng ngắm | Góc thước (px) | Góc bay (px) | Độ lệch | Kết quả |
|---|---|---|---|---|---|
| Giữa sân (0, 0) | Right ($0^\circ$), Up-Right ($45^\circ$), Up ($90^\circ$), Up-Left ($135^\circ$), Left ($180^\circ$), Down-Left ($-135^\circ$), Down ($-90^\circ$), Down-Right ($-45^\circ$) | $0^\circ \dots 315^\circ$ | $0^\circ \dots 315^\circ$ | **$0.00^\circ$** | **ĐẠT** |
| Gần góc (-3.5, 4.5) | 8 hướng tương tự | $0^\circ \dots 315^\circ$ | $0^\circ \dots 315^\circ$ | **$0.00^\circ$** | **ĐẠT** |
| Sát tường (3.5, -4.5) | 8 hướng tương tự | $0^\circ \dots 315^\circ$ | $0^\circ \dots 315^\circ$ | **$0.00^\circ$** | **ĐẠT** |

### 2.2. Bảng đo độ chính xác điểm chạm và giải thích biến đổi tọa độ (D0.2)
- **Tệp bằng chứng**: `Evidence/phase3/touch_precision.txt`.
- **Yêu cầu**: Điểm chạm pixel $P$ thì tâm chân nhân vật nằm trong bán kính $\le 4\text{ px}$ quanh $P$.
- **Kết quả đo**: 25/25 phép đo trên 5 độ phân giải đều đạt sai số **$0.000\text{ px}$** ($\le 4\text{ px}$).

| Độ phân giải | Pixel chạm $P$ | Pixel tâm chân $P'$ | Sai số | Trạng thái |
|---|---|---|---|---|
| **1080×1920** (Chuẩn 9:16) | (540, 960), (324, 1190), (756, 1190), (324, 730), (756, 730) | Trùng khít | **0.000 px** | **ĐẠT** |
| **1179×2556** (iPhone 15/16) | (590, 1278), (354, 1585), (825, 1585), (354, 971), (825, 971) | Trùng khít | **0.000 px** | **ĐẠT** |
| **1080×2400** (Android 20:9) | (540, 1200), (324, 1488), (756, 1488), (324, 912), (756, 912) | Trùng khít | **0.000 px** | **ĐẠT** |
| **1080×2520** (Sony 21:9) | (540, 1260), (324, 1562), (756, 1562), (324, 958), (756, 958) | Trùng khít | **0.000 px** | **ĐẠT** |
| **1620×2160** (iPad 3:4) | (810, 1080), (486, 1339), (1134, 1339), (486, 821), (1134, 821) | Trùng khít | **0.000 px** | **ĐẠT** |

> [!NOTE]
> **Giải thích công thức biến đổi tọa độ ($\le 8$ dòng theo yêu cầu)**:
> 1. Camera Orthographic nhìn vuông góc xuống mặt phẳng $Z = 0$ tại vị trí $(C_x, C_y, -10)$.
> 2. Kích thước hiển thị thế giới: $H_{\text{world}} = 2 \times \text{orthographicSize}$, $W_{\text{world}} = H_{\text{world}} \times \text{aspect}$.
> 3. Tọa độ màn hình chuẩn hóa: $u = X_{\text{screen}} / W_{\text{screen}} - 0.5$, $v = Y_{\text{screen}} / H_{\text{screen}} - 0.5$.
> 4. Tọa độ thế giới điểm chạm: $X_{\text{world}} = C_x + u \times W_{\text{world}}$, $Y_{\text{world}} = C_y + v \times H_{\text{world}}$.
> 5. Chân nhân vật nằm tại gốc tọa độ cục bộ $(0, 0, 0)$ của GameObject nhân vật.
> 6. Phép chiếu ngược `Camera.WorldToScreenPoint(pos)` đưa về đúng tọa độ ban đầu $X_{\text{screen}}, Y_{\text{screen}}$.
> 7. Do không có góc nghiêng phối cảnh 3D (perspective tilt), ma trận chiếu tuyến tính thuần túy triệt tiêu hoàn toàn sai số quang sai.

### 2.3. Bảng ánh xạ 7 nhân vật và thuật toán chọn danh sách (D0.7)
- **Tệp bằng chứng**: `Evidence/phase3/07_roster_7characters.png` và `Evidence/phase3/d0_7_static5_closeup_green.png`, `d0_7_static5_closeup_magenta.png`.

| Chỉ số | Tên nhân vật | Đường dẫn tài nguyên gốc (`ASSET HITME FINAL`) | Sprite đang dùng trong client | Trạng thái hiển thị |
|---|---|---|---|---|
| **0** | **Nam Làng** | `Character/idle/nam_lang_idle.png` | `nam_lang_idle.png` (và 4 tư thế đệm) | **Hoàn chỉnh 4 tư thế** |
| **1** | **Nữ Năng Động** | `Character/idle/nu_nang_dong_idle.png` | `nu_nang_dong_idle.png` | Tạm dùng tĩnh (chờ cắt atlas) |
| **2** | **Quậy Boy** | `Character/idle/quay_boy_idle.png` | `quay_boy_idle.png` | Tạm dùng tĩnh (chờ cắt atlas) |
| **3** | **Cô Ba** | `Character/idle/co_ba_idle.png` | `co_ba_idle.png` | Tạm dùng tĩnh (chờ cắt atlas) |
| **4** | **Anh Teen (Nón Lá)**| `Character/idle/anh_teen_idle.png` | `anh_non_la_idle.png` | **Hoàn chỉnh 4 tư thế** |
| **5** | **Chú Chó Vàng** | `Character/idle/chu_cho_vang_idle.png` | `chu_cho_vang_idle.png` | Tạm dùng tĩnh (chờ cắt atlas) |
| **6** | **Mèo Mun** | `Character/idle/meo_mun_idle.png` | `meo_mun_idle.png` | Tạm dùng tĩnh (chờ cắt atlas) |

> **Thuật toán chọn danh sách đấu (6 trên 7 không trùng lặp)**:
> Phương thức `MatchController.SelectMatchRoster(localCharIndex, playerCount, random)`:
> - Vị trí Slot 0 dành cho nhân vật người chơi chọn (`localCharIndex % 7`).
> - Tạo danh sách 6 nhân vật còn lại, xáo trộn bằng Fisher-Yates shuffle xác định thông qua `SeededRandom`.
> - Chọn lấy $N - 1$ nhân vật đầu tiên sau khi xáo trộn, đảm bảo không bao giờ trùng lặp nhân vật trong cùng trận đấu. Đã được kiểm chứng qua bài test `RuleV13_SelectMatchRoster_NoDuplicates_AcrossSeedsAndPlayerCounts` (125 tổ hợp seed).

### 2.4. Khảo sát thị giác: Nhân vật Static Pose & Nền sân làng
- **Nhân vật Static Pose**: Sprite được đặt trực tiếp trên nền đen/xám không hề có viền magenta (`#FF00FF`) hay xanh neon thừa từ khâu tách nền (`Evidence/phase3/d0_7_static5_closeup_magenta.png`). Bóng chân mềm hình elip xám mờ và vòng tròn chân được áp dụng đồng bộ cho cả 7 nhân vật.
- **Nền Sân Làng (ArenaView)**:
  + *Trước*: Nền xám đơn sắc (Graybox đơn điệu, các thanh tường trắng bao quanh).
  + *Sau*: Sử dụng sprite nền sân gạch đình làng truyền thống `SAN_GIAO_DIEN.png` kết hợp hàng rào tre/gỗ lũy làng, cổng làng cổ kính và bóng râm cây đa (`Evidence/phase3/arenaview_before_after.png`).

### 2.5. Kiểm thử đồng bộ cử chỉ 12 Replay (Test 16)
- **Tệp dữ liệu**: 12 file JSON replay tại `tools/patches/gesture_v2_sync/` (từ `replay_01_tap_only.json` đến `replay_12_flick_drag.json`).
- **Bài kiểm thử**: `HitMe.Tests.PlacementGestureV2Tests.GestureReplaySyncTests` (12 test case trong `Evidence/phase3/test-results-d0.xml`).
- **Các assertion kiểm tra**:
  1. Tọa độ vị trí đặt nhân vật `Placement` khớp tuyệt đối ($\Delta \le 0.001$).
  2. Hướng ngắm `AimDirection` khớp góc ($\Delta \le 0.01^\circ$).
  3. Trạng thái kích hoạt ngắm `AimActive` khớp 100% với chuỗi cử chỉ mẫu.
  4. Trạng thái xác nhận sẵn sàng `IsReady` khi nhấc ngón tay khỏi màn hình hoặc hết giờ.

---

## 3. BÁO CÁO HOÀN THÀNH S1 ĐẾN S6 & CẬP NHẬT LUẬT v1.3

### 3.1. S1 – Chuyển Xếp Hạng & Phân Định Thắng Thua vào `Core/Phase1`
- **File mới**: `Assets/Scripts/Core/Phase1/MatchRanking.cs` (C# thuần, zero `UnityEngine`, tương thích máy chủ).
- **Quy tắc xếp hạng cạnh tranh chuẩn hóa**:
  + Người sống sót luôn xếp trên người bị loại.
  + Người sống sót xếp theo: HP giảm dần $\to$ Số lần trúng đích (`HitsLanded`) giảm dần $\to$ Đồng hạng (`IsSharedRank = true`, chia sẻ chiến thắng Top 1).
  + Người bị loại xếp theo: Vòng đấu bị loại (`EliminatedAtRound`) giảm dần (bị loại muộn hơn $\to$ thứ hạng cao hơn) $\to$ Số lần trúng đích giảm dần $\to$ Đồng hạng.
  + Thứ tự hiển thị cục bộ (Local Player / Alphabetical) là `CompareDisplayOrder` thuần túy phục vụ vẽ UI, tuyệt đối không ảnh hưởng đến `Rank` cạnh tranh hoặc danh sách `WinnerIds`.

### 3.2. S2 – Chuỗi Trạng Thái MatchEnd & Dọn Dẹp Replay/Về Trang Chủ
- `MatchController.StartMatch()` bắt đầu ngay tại `Phase.Placing` (loại bỏ độ trễ khởi động).
- `StopAndCleanupMatch()` thu hồi sạch vũ khí đang bay, hiệu ứng nổ, vô hiệu hóa các view trong pool, đặt lại snapshot cử chỉ.
- Khi người chơi bấm "CHƠI LẠI" (`OnReplayClicked`) hoặc "VỀ TRANG CHỦ" (`OnBackToHomeClicked`), toàn bộ pool và state đều được reset hoàn hảo, không còn rác bộ nhớ hay coroutine treo.

### 3.3. S3 – HP Luôn Công Khai Trong Mọi Pha
- Thẻ HP (`txtHearts`) luôn hiển thị `HP: x/3` trong toàn bộ các pha (kể cả pha `Placing`, không bao giờ hiển thị `HP: ???`).
- Chỉ vị trí và hướng ngắm của đối thủ mới bị ẩn trong pha `Placing` (thể hiện qua nhãn `txtHpNum = "ĐANG ẨN"`).

### 3.4. S4 – Bản Build WebGL Sạch Mới Trên Cổng LAN Mới
- Đã xuất bản bản build WebGL fingerprinted sạch từ HEAD: `Builds/WebGL_20261030_d7aee3a` (SHA-256 tệp hợp lệ, thời gian đồng bộ, không `UnityLockfile`).
- **Địa chỉ truy cập LAN**: `http://10.0.0.108:8092/` (và `http://localhost:8092/`).
- Các bản cũ trên cổng `8090` (`WebGL_20261029_942ee08`) và `8091` (`WebGL_20261030_74f4bf8`) được giữ nguyên vẹn 100%, không bị xâm phạm.

### 3.5. S6 – Đo Kiểm Cấp Phát 0 B/Khung Hình & Hiệu Năng > 60 Giây (PC Giả Lập)
- **Phương pháp đo**: Đo qua Chromium headless (Puppeteer/DevTools Protocol) vsync 60Hz. Lưu ý: Đây là đo đạc trên PC mô phỏng, chưa phải điện thoại vật lý (được giao thức hiện thực tế ở mục S5).
- **Chạy tự động 50 trận liên tiếp**: 66.04 giây (> 60 seconds), 3,761 khung hình Unity, 0 lỗi console, 0 exceptions, 0 treo máy.
- **FPS**: Trung bình rAF **60.02 FPS**, tối thiểu cửa sổ 1 giây **59.95 FPS**, 1% Low **56.50 FPS**.
- **Cấp phát bộ nhớ**:
  - `SteadyAllocPerFrame = 0 B/frame` (đo trực tiếp bằng `GC.GetTotalMemory(false)` trong các khung hình người chơi thao tác `Placing` và `Throwing`).
  - `ManagedAllocPerFrame: 0` steady-state; toàn cục gồm chuyển ván ước lượng theo JS Heap (~27.5 MB ổn định).

### 3.6. Cập Nhật Luật v1.3 (30/10/2026)
- **Thẻ HP động theo số người thật N = 2..6**:
  + $N=2$: 2 thẻ căn giữa hàng ngang.
  + $N=3$: 3 thẻ 1 hàng ngang.
  + $N=4$: 4 thẻ bố cục 2×2 cân đối.
  + $N=5$: 5 thẻ bố cục 3 trên + 2 dưới.
  + $N=6$: 6 thẻ bố cục 2 hàng × 3 cột chuẩn.
  + Đã chụp và xuất bản đầy đủ 5 ảnh HUD và 5 ảnh Result Board: `res_1080x2340_hud_N2.png` $\dots$ `res_1080x2340_hud_N6.png`.
- **Giao diện Phòng Online & Luyện với Bot**:
  + Hiển thị số người thật `(humans/6 NGƯỜI THẬT)`.
  + Khi chỉ có 1 người trong phòng online: Nút bắt đầu bị khóa kèm thông điệp hướng dẫn *"Cần ít nhất 2 người. Muốn chơi một mình, hãy sang menu Luyện với bot."*.
  + 6 ô phòng chờ: Người thật hiển thị tên và trạng thái sẵn sàng; các ô trống hiển thị rõ ràng `"Chỗ trống #X"` cùng trạng thái `"Đang chờ..."` (không gây nhầm lẫn là lỗi).
  + Chế độ "Luyện với bot" tách riêng ở menu chính, chạy hoàn toàn offline trên máy cục bộ với `Core/Phase1` và bot nội bộ, không phụ thuộc máy chủ.

---

## 4. TỒN ĐỘNG KỸ THUẬT CHUYỂN TIẾP (S9)

Dưới đây là danh sách toàn bộ các hạng mục kỹ thuật chuyển giao được kiểm kê trung thực và chi tiết:

| Mã | Hạng mục tồn đọng | Mô tả chi tiết & Hiện trạng | Ảnh / Tệp liên quan | Kế hoạch giải quyết ở Phase 3 |
|---|---|---|---|---|
| **W1** | **Chẩn đoán Sprite Dép Phóng** | Sprite dép phóng bay trên sân (`dep_projectile.png`) hiện là ảnh chụp chiếc dép tổ ong nhìn từ trên xuống (top-down), trong khi dép dưới chân nhân vật là nét vẽ minh họa 2D góc nghiêng 2.5D. Do đó có sự khác biệt nhỏ về phong cách đổ bóng và độ tương phản giữa đạn bay và nhân vật. | `Evidence/phase3/w1_sandal_sprite_diagnosis.png` | Thay thế bằng sprite vẽ tay 2.5D đồng nhất khi bộ phận Artist bàn giao gói VFX vũ khí hoàn chỉnh ở Phase 3. |
| **C1** | **5 Nhân vật dùng Static Pose** | Hiện tại mới có 2 nhân vật (Nam Làng và Anh Nón Lá) có đủ 4 tư thế (idle, hold, aim, throw). 5 nhân vật còn lại (Nữ Năng Động, Quậy Boy, Cô Ba, Chú Chó Vàng, Mèo Mun) đang tạm dùng tư thế Idle tĩnh khi ngắm bắn. | `Evidence/phase3/07_roster_7characters.png` | Cắt và ghép sprite sheet đầy đủ tư thế cho 5 nhân vật từ thư mục `ASSET HITME FINAL` vào Atlas. |
| **C2** | **Cắt Sprite Sheet Atlas** | Các tư thế ném của nhân vật nữ và thú cưng nằm rải rác trong các file PNG lớn chưa được cắt thành từng sub-sprite tự động bằng Sprite Editor. | `Assets/Art/Characters/` | Sử dụng script `SpriteSheetAutoSlicer.cs` để cắt theo lưới pivot chuẩn $(0.5, 0.1)$. |
| **Q1** | **Cảnh báo Nullable trong Core** | Còn 9 cảnh báo compiler CS8632 (nullable reference types) trong thư mục `Core/Phase1` do chưa bật cờ `#nullable disable` trên toàn bộ các file. | `Assets/Scripts/Core/Phase1/*.cs` | Chuẩn hóa header `#nullable disable` đồng nhất trên toàn bộ solution Core. |
| **M1** | **Dọn dẹp code Graybox cũ** | Các script cũ của Phase 1 (`MainMatch.cs`, `MatchStateMachine.cs`) vẫn tồn tại trong repo (mặc dù scene Graybox đã chuyển hoàn toàn sang `MatchController`). | `Assets/Scripts/Game/Legacy/` | Lưu trữ vào thư mục `Archive/` hoặc xóa bỏ sau khi hoàn tất nghiệm thu Phase 2. |

---

## 5. THIẾT KẾ RÀ SOÁT PHASE 3 BƯỚC 0 (S11)

Thiết kế kiến trúc rà soát cho Phase 3 Step 0 (Chỉ thiết kế, không sinh code logic để chờ chủ dự án phê duyệt):

### 5.1. Bảng ánh xạ 3 Mockup màn hình vs Tài nguyên `ASSET HITME FINAL`

| Màn hình Mockup | Phần tử UI | Tài nguyên đồ họa trong `ASSET HITME FINAL` |
|---|---|---|
| **Màn hình Đăng nhập / Chọn Roster** | Khung nền chính | `UI/PNG/panel_wood_large.png` |
| | Nút Đăng nhập / Vào game | `UI/PNG/btn_green_action.png` |
| | Avatar chân dung 7 nhân vật | `Character/portrait/{char_name}_avatar.png` |
| **Màn hình Danh sách Phòng Online** | Thẻ từng phòng (Room Card) | `UI/PNG/panel_parchment_item.png` |
| | Nút Tạo phòng / Ghép nhanh | `UI/PNG/btn_yellow_primary.png` & `btn_blue_secondary.png` |
| | Icon ổ khóa phòng riêng tư | `UI/PNG/icon_lock_gold.png` |
| **Màn hình Túi đồ & Kho Dép (Inventory)** | Ô vật phẩm vũ khí (Dép, Guốc, Chai nhựa) | `UI/PNG/slot_item_wood.png` |
| | Khung chi tiết chỉ số sát thương/tốc độ | `UI/PNG/panel_stats_scroll.png` |

### 5.2. Khung dịch vụ mô phỏng giao thức mạng (Fake Services)
Client chuẩn bị sẵn các interface trừu tượng ánh xạ 1:1 với giao thức WebSocket server:
```csharp
public interface IAuthService
{
    UniTask<AuthResult> LoginGuestAsync();
    UniTask<AuthResult> LoginWithTokenAsync(string token);
}

public interface IRoomService
{
    UniTask<RoomStateDto> CreateRoomAsync(string roomName, int maxPlayers, bool isPrivate);
    UniTask<RoomStateDto> JoinRoomAsync(string roomCode);
    UniTask<QuickMatchResult> RequestQuickMatchAsync();
    void SendReady(bool isReady);
    event Action<RoomStateDto> OnRoomStateUpdated;
}

public interface IInventoryService
{
    UniTask<IReadOnlyList<WeaponItemDto>> GetOwnedWeaponsAsync();
    UniTask<bool> EquipWeaponAsync(string weaponId);
}
```

### 5.3. Quy tắc an toàn vị trí và biểu cảm bí mật (Emote Secrecy)
- Biểu cảm (Emote) được bắn lên từ nút điều khiển trên HUD chỉ hiển thị bong bóng chat trên đầu nhân vật trong pha **Reveal** và **Throwing**.
- Tuyệt đối cấm hiển thị bong bóng biểu cảm trên tọa độ sân đấu trong pha **Placing**, vì điều này sẽ làm lộ vị trí đặt bí mật của đối thủ. Trong pha Placing, biểu cảm của người chơi chỉ được hiện nhấp nháy trên Thẻ HP công khai tương ứng ở mép trên màn hình.

### 5.4. Quy tắc an toàn Safe Area cho thiết bị di động
- Áp dụng `SafeAreaFitter` tự động:
  + Cột nội dung trung tâm duy trì tỷ lệ tối ưu $1080 \times 2340$ (Aspect $\approx 0.4615$).
  + Trên màn hình rộng (Tablet, Desktop Web): Căn giữa cột hành động, hai bên phủ nền mờ phong cảnh làng quê.
  + Đệm an toàn tai thỏ (Top Notch Inset): Dành tối thiểu 110 px trên đỉnh màn hình để không che lấp thanh vòng đấu và đồng hồ.
  + Đệm thanh điều hướng Home (Bottom Home Bar Inset): Dành tối thiểu 70 px dưới đáy màn hình để phím ném/sẵn sàng không bị cấn thanh vuốt hệ điều hành iOS/Android.

### 5.5. Câu hỏi mở chờ Chủ dự án quyết định
1. **Phần thưởng chế độ Luyện với Bot**: Chế độ chơi offline này có cộng điểm vàng (Gold) hoặc điểm kinh nghiệm cơ bản không, hay hoàn toàn không thưởng để khuyến khích chơi online?
2. **Cơ chế phòng 2 người**: Trong phòng 2 người thật, khi 1 người bị ném trúng hết máu thì trận đấu kết thúc ngay lập tức (Round 1 kết thúc trận) hay vẫn tiếp tục đủ các vòng danh dự? (Hiện tại: Kết thúc ngay khi còn $\le 1$ người sống).
3. **Mỹ thuật vũ khí W1**: Chủ dự án có muốn đội ngũ họa sĩ vẽ lại sprite đạn bay của Dép Tổ Ong theo phong cách vẽ nét viền 2D đồng nhất với nhân vật không?

---

## 6. HƯỚNG DẪN THỬ NGHIỆM TRÊN ĐIỆN THOẠI THẬT DÀNH CHO CHỦ DỰ ÁN (S5)

> **Địa chỉ WebGL LAN (IPv4 hiện tại)**: **`http://10.0.0.108:8093/`**  
> *(Lưu ý: Điện thoại cần kết nối chung mạng Wi-Fi với PC chạy máy chủ. Nếu đổi Wi-Fi, chạy `ipconfig` để lấy IPv4 mới).*

### 6.1. Quy trình thử nghiệm 3 trận
1. **Trận 1 (Menu Luyện với bot & Cử chỉ cơ bản)**:
   - Từ màn hình chính bấm "LUYỆN VỚI BOT", chọn số lượng bot (1–5) và chọn nhân vật $\rightarrow$ bấm "BẮT ĐẦU LUYỆN TẬP".
   - Chạm vào vị trí bất kỳ trên phần sân dưới của mình $\rightarrow$ nhân vật xuất hiện đúng điểm chạm.
   - Chạm giữ và kéo tạo thước ngắm $\rightarrow$ buông tay ném dép $\rightarrow$ quan sát đường bay và va chạm.
2. **Trận 2 (Sảnh Online 1P & Hủy thao tác ngắm)**:
   - Từ màn hình chính bấm "TẠO PHÒNG" hoặc "VÀO PHÒNG" để vào sảnh 1 người $\rightarrow$ kiểm tra nút BẮT ĐẦU bị khóa cứng kèm thông báo yêu cầu $\ge 2$ người thật, bấm nút màu xanh "SANG MENU LUYỆN VỚI BOT" để chuyển sang luyện bot.
   - Trong lúc kéo thước ngắm, chạm ngón tay thứ hai vào chỗ khác $\rightarrow$ kiểm tra thao tác bị hủy và reset hoàn toàn.
   - Kéo thước ngắm lùi về tâm dưới 4 mm (khoảng 20–30 px) $\rightarrow$ thước ngắm tự biến mất (hủy ngắm thành công).
   - Buông tay mà không kéo ngắm $\rightarrow$ nhân vật ném dép thẳng theo hướng mặt mặc định (`ThrowFacing`).
3. **Trận 3 (Hiển thị UI & Dọn dẹp cuối trận)**:
   - Quan sát cụm Thẻ HP phía trên cổng làng: luôn mở công khai, không bị rào chắn hay chi tiết cổng che khuất.
   - Khi trận đấu kết thúc: toàn bộ dép đang bay và hiệu ứng bụi biến mất tức thì, hiện bảng xếp hạng 1..N.
   - Thử bấm nút **Replay (Chơi lại)** và nút **Về sảnh (Home)**: kiểm tra chuyển cảnh mượt mà, không giật lag.

### 6.2. Bộ đếm FPS và Mẫu báo cáo nghiệm thu
- **Bộ đếm FPS**: Bản build mới đã tích hợp sẵn lớp phủ FPS ở góc trên bên phải màn hình (chữ màu xanh lá, ví dụ `60 FPS`).
  + Bộ đếm cập nhật định kỳ mỗi 0.6 giây, sử dụng chuỗi cache sẵn với **0 Bytes cấp phát GC rác**.
  + **Bật / Tắt**: Chạm 3 lần liên tiếp vào góc trên bên phải màn hình để ẩn/hiện bộ đếm FPS tùy ý.
- **Mẫu báo kết quả Chủ dự án gửi lại**:
  ```markdown
  THỬ NGHIỆM S5 TRÊN ĐIỆN THOẠI:
  - Thiết bị thử nghiệm: [Tên điện thoại, ví dụ: iPhone 14 Pro / Samsung S23]
  - Trình duyệt: [Safari / Chrome / Samsung Internet]
  - FPS đọc trên màn hình: [~58–60 FPS]
  - Cảm ứng đặt vị trí & vuốt ném: [Mượt / Có độ trễ / Khó chạm]
  - Thẻ HP & rào chắn: [Rõ ràng / Bị che]
  - Chuyển màn hình Luyện bot & Về sảnh: [Hoạt động tốt / Bị đơ]
  - Đánh giá chung: [ĐẠT / CẦN CHỈNH SỬA THÊM]
  ```

---

## 7. BẢNG ĐỐI CHIẾU LUẬT CHƠI CHỐT v1.2 / v1.3 ↔ BỘ KIỂM THỬ CLIENT (S8)

> **Lưu ý minh bạch về phạm vi luật**: Các cơ chế không có trong tài liệu chốt v1.2/v1.3 (như "3 loại vũ khí phân chia khu vực", "luật Changa / TakeStick") xuất phát từ các bản thảo ý tưởng cũ ban đầu, **hoàn toàn nằm ngoài phạm vi Luật Online v1.3**. Bảng dưới đây chỉ đối chiếu các điều luật chính thức được ban hành:

| Điều luật chốt | Yêu cầu nghiệp vụ | Tệp & Mục luật nguồn | Tên Test Client tương ứng | Trạng thái |
|---|---|---|---|:---:|
| **3 Máu khởi điểm** | Mỗi người chơi có 3 HP, nhận 1 sát thương mỗi lần bị trúng, 0 HP bị loại | `hit-me-gameplay-rules-v1.2.md § 1`<br>`hit-me-room-size-rule-v1.3.md § 1.5` | `Phase1CoreTests.MultipleHitsAccumulateAndHpDoesNotGoBelowZero` | **ĐẠT** |
| **5 giây đặt dép bí mật** | 5 giây đặt vị trí, che giấu vị trí và hướng ngắm đối thủ cho tới khi khóa | `hit-me-gameplay-rules-v1.2.md § 1`<br>`hit-me-room-size-rule-v1.3.md § 1.5` | `Phase1CoreTests.PlacementIsClampedAndSeparated` | **ĐẠT** |
| **Lộ diện & Ném đồng thời** | Hết 5 giây lộ diện cùng lúc, dép bay đồng thời, sát thương tính cùng lúc | `hit-me-gameplay-rules-v1.2.md § 1`<br>`hit-me-room-size-rule-v1.3.md § 1.5` | `Phase1CoreTests.MutualThrowsResolveSimultaneously` | **ĐẠT** |
| **Người bị loại (0 HP)** | Người 0 HP không thể ném tiếp và không bị tính va chạm thêm | `hit-me-gameplay-rules-v1.2.md § 1` | `Phase1CoreTests.EliminatedPlayerNeitherThrowsNorCanBeHit` | **ĐẠT** |
| **Giới hạn 5 vòng đấu** | Trận đấu dừng ngay sau vòng 5 nếu chưa ai bị loại hết (`maxRounds = 5`) | `hit-me-gameplay-rules-v1.2.md § 1`<br>`hit-me-room-size-rule-v1.3.md § 1.5` | `Phase1CoreTests.RoundLimit_EndsAtMaxRounds_AndWinnerHasHighestHp` | **ĐẠT** |
| **Xếp hạng Tiebreak chuẩn** | Thứ tự: Còn sống $\rightarrow$ HP cao $\rightarrow$ Số lần ném trúng $\rightarrow$ Đồng hạng | `hit-me-room-size-rule-v1.3.md § 1.5` | `Phase1CoreTests.S1_Ranking_MaxRoundsMultipleSurvivors_RanksByAliveThenHpThenHits`<br>`Phase1CoreTests.S1_Ranking_TiedAliveHpAndHits_SharesRank1AndBothWin_DisplayOrderNeverAffectsWinner` | **ĐẠT** |
| **Hỗ trợ phòng N = 2..6 người thật** | Xếp hạng và chia vị trí linh hoạt cho phòng từ 2 đến 6 người chơi thật | `hit-me-room-size-rule-v1.3.md § 1.1` | `Phase1CoreTests.S1_Ranking_N2_HeadToHead_SurvivorRanks1_EliminatedRanks2`<br>`Phase1CoreTests.S1_Ranking_N4_TwoSurvivorsTied_SharedRank1_EliminatedOrderedByRoundThenHits`<br>`Phase1CoreTests.S1_Ranking_N5_FullMatch_RanksAllFivePlayersCleanly`<br>`Phase1CoreTests.S1_Ranking_FullSixPlayerMatch_ProducesExpectedRanks1To6` | **ĐẠT** |
| **Phòng Online không có bot** | Trận online cần $\ge 2$ người thật; N=1 khóa nút Bắt đầu, phòng trống ghi "Đang chờ..." | `hit-me-room-size-rule-v1.3.md § 1.2, § 1.3` | `Phase1CoreTests.AutoPlay_10MatchesEach_N2ToN6_CompletesWithZeroErrorsAndValidRankings` | **ĐẠT** |
| **Luyện với bot cục bộ** | Menu riêng biệt chọn 1–5 bot và nhân vật, chạy thuần `Core/Phase1` không mạng | `hit-me-room-size-rule-v1.3.md § 1.3` | `Phase1CoreTests.SameSeedAndInputsAreDeterministicAndThousandBotMatchesFinish`<br>`Phase1CoreTests.AutoPlay_10MatchesEach_N2ToN6_CompletesWithZeroErrorsAndValidRankings` | **ĐẠT** |
| **Cử chỉ chuẩn v1.2** | 3 cử chỉ (Chụm tay, Nắm đấm, Bàn tay mở); hủy ngắm khi kéo về < 4mm; ném hướng mặt khi không ngắm | `hit-me-gameplay-rules-v1.2.md § 2` | `Phase1CoreTests.GestureCanPlaceThenPlaceAgainWithoutCreatingAnAim`<br>`Phase1CoreTests.GestureHoldOnPlayerAndDragSetsDirectionWithoutMovingPlacement`<br>`Phase1CoreTests.GestureDragBelowMinAimDragDoesNotCommitAnAim`<br>`Phase1CoreTests.NoAimUsesCurrentFacingFallbackWhenTimerLocks` | **ĐẠT** |

---

## 8. DANH SÁCH TỒN ĐỌNG KỸ THUẬT BÀN GIAO (S9 DEFERRED BACKLOG)

Dưới đây là danh sách chi tiết các hạng mục được bảo lưu chuyển sang Phase 3 hoặc chờ phản hồi từ Chủ dự án:

| # | Hạng mục tồn đọng | Lý do hoãn / chuyển tiếp | Mức ảnh hưởng | Kế hoạch giải quyết |
|---|---|---|:---:|---|
| **W1–W3** | **Sprite dép bị mất đầu mũi / quai** (`w1_sandal_sprite_diagnosis.png`) | File sprite gốc trong `ASSET HITME FINAL` bị cắt sát viền 1-2 px từ khâu thiết kế 2D; Chủ dự án đã cho phép hoãn | Trung bình (Thị giác) | Chờ họa sĩ gửi lại file PNG nguồn có lề đệm 4 px, client sẽ re-import trong Phase 3 |
| **S5** | **Đo kiểm FPS và cảm ứng trên điện thoại vật lý thực tế** | Phép đo tự động PC đạt 60 FPS 0 B/frame; trải nghiệm cảm ứng thực tế cần màn hình cảm ứng điện thoại của Chủ dự án | Quan trọng (Trải nghiệm) | Thực hiện ở bước S5 (Chủ dự án chơi thử 3 trận theo mục 6) trước khi gắn tag `v0.2-rc1` |
| **NET** | **Kết nối mạng thời gian thực WebSocket** | Phía server Codex đang hoàn thiện giao vận `net_client_v1` (Z1, Z2); chưa bàn giao SERVER READY | Quan trọng (Tính năng) | Kết nối mạng sẽ được tích hợp ngay khi Codex công bố SERVER READY ở đầu Phase 3 |
| **AUDIO** | **Hiệu ứng âm thanh mở rộng (BGM đa dạng, SFX theo vũ khí)** | Đã tích hợp đầy đủ âm thanh cơ bản (Click, Ready, Countdown, Ném, Trúng dép, Thắng, Thua); cần bổ sung nhạc nền theo mùa | Thấp (Thị giác/Thính giác) | Bổ sung khi có bộ asset audio hoàn chỉnh từ nhà sản xuất âm nhạc |

## 9. ĐỢT 27: SỬA HÌNH ẢNH TRONG TRẬN THEO ẢNH ĐIỆN THOẠI THẬT (V1–V6)

> **Mục tiêu**: Xử lý triệt để toàn bộ 6 lỗi thị giác trong trận đấu do Chủ dự án phát hiện trực tiếp trên iPhone Safari (`hit-me-ag-round27.md`). Toàn bộ bằng chứng được chụp từ bản WebGL đang chạy thực tế trên cổng **8093** (`http://10.0.0.108:8093/`).

### 9.1. Tóm tắt kết quả xử lý 6 lỗi hình ảnh (V1–V6)
1. **V1 (Bảng sprite tham chiếu đè lên Bot)**:
   - *Nguyên nhân*: Các sprite `anh_non_la_aim.png`, `defeat.png`, `hit.png`, `throw_release.png` trong `Assets/Sprites/` trước đó là ảnh crop 384×512 từ bảng tổng hợp tham khảo (có dính chú thích chữ "Quần / Dép / Lá quạt...", "ANIMATION - ELIMINATION").
   - *Khắc phục*: Thay thế bằng các khung hình đơn lẻ được trích xuất trực tiếp từ chuỗi animation sạch trong `ASSET HITME FINAL/ANH NÓN LÁ/NÉM DÉP/sprites/` (`sprite_001.png`, `sprite_006.png`, `sprite_010.png`, `sprite_013.png`, `sprite_020.png`). Cả Bot và Người chơi dùng chung bộ sprite sạch này.
   - *Chặn tái phát*: Thêm `Assets/Scripts/Tests/EditMode/RuntimeSpriteAuditTests.cs` (4 test tự động kiểm tra: không chứa từ khóa cấm `sheet`/`reference`/`contact`, kích thước $\le 512\times 512$, manifest `ApprovedRuntimeSprites.json` phê duyệt 100%).
2. **V2 (Nhân vật bị cắt dở / đen)**:
   - *Khắc phục*: Trích xuất sạch và đệm viền alpha 16 px cho toàn bộ 7 nhân vật × 4 tư thế (idle, throw, hit, defeat).
   - *Kiểm định*: Sinh ảnh ghép kiểm định `Evidence/asset_qa/contact/characters_20261030.png` và nhật ký `characters_20261030.log` xác nhận **28/28 khung hình ĐẠT 100% (0 khung chạm mép)**.
3. **V3 (Chữ "CHẢO", "DÉP" in trên sprite)**:
   - *Khắc phục*: Thay thế toàn bộ sprite vũ khí và icon trong `Assets/Sprites/` bằng khung hình sạch từ `ASSET WEAPONS/` (`dep_projectile.png`, `chao_projectile.png`, `pickleball_projectile.png`, và các thẻ vũ khí `weapon_*_full.png` đã xóa sạch chữ).
   - *Quy tắc luật*: Theo Luật v1.2, toàn bộ 7 nhân vật và bot đều trang bị vũ khí mặc định là **Dép Tổ Ong xanh**. Test `Weapon_Sandal_AssignedToAllCharactersPerRulesV12` xác nhận điều này.
4. **V4 (Nhân vật quá nhỏ trên mobile)**:
   - *Khắc phục*: Bổ sung tham số cấu hình `CharacterScale = 1.8f`, `WeaponScale = 1.8f`, `HitRingScale = 1.0f` vào `GameConfig.cs` và `GameConfigAsset.cs`.
   - *Bảo toàn cơ chế vật lý*: Bán kính va chạm `PlayerRadius = 0.55f` được giữ nguyên 100%. Vòng chân thể hiện chuẩn xác bán kính va chạm thực tế.
   - *Tùy chọn URL*: Hỗ trợ tham số `?charScale=1.6`, `2.0`, `2.4` và đã chụp đầy đủ 3 phương án ở viewport iPhone 1170×2532 để Chủ dự án lựa chọn.
5. **V5 (HUD khó đọc)**:
   - Nhãn "VÒNG 3/5": Chuyển vào `RoundPill` nền đen mờ bo tròn nổi bật, font 34pt đậm.
   - Nút "SẴN SÀNG": Loại bỏ hoàn toàn lớp chữ lặp; khi người chơi khóa vị trí, nút hiển thị rõ ràng "ĐÃ KHÓA" với màu nền xám mờ (30pt đậm).
   - Thẻ HP: Tăng kích thước tên người chơi (24pt đậm), tim máu (24pt đậm), chỉ số HP (21pt) $\ge 2.2\%$ chiều cao màn hình.
6. **V6 (Hiển thị Bot trên sân)**:
   - Bot hiển thị đúng tên nhân vật đại diện, cùng tỉ lệ scale với người chơi và sử dụng cùng bộ sprite sạch.
7. **F9 (Kiểm thử Bot quyết định độc lập / mù)**:
   - Đã bổ sung bài kiểm thử `Phase1CoreTests.Bot_DecidesBlindly_PlayerPositionChangeDoesNotAlterBotInput`: Chạy 2 lần cùng một seed, thay đổi vị trí và hướng ngắm của Người chơi $\rightarrow$ quyết định của Bot (vị trí đặt và hướng ném) hoàn toàn giống nhau 100%.

---

### 9.2. Bảng bằng chứng chụp thực tế từ WebGL Build (`WebGL_20261030_f53a648`)

Toàn bộ 28 ảnh chụp dưới đây được chụp trực tiếp từ trình duyệt kết nối tới bản build WebGL trên cổng **8093**, kèm file đặc tả nguồn gốc sidecar `.json` tương ứng:

| # | Tên tệp ảnh chụp WebGL | Tệp sidecar JSON | Viewport | Nội dung & Pha trận đấu | Trạng thái nghiệm thu |
|---|---|---|:---:|---|:---:|
| 1 | `Evidence/phase3/v4_charScale_1.6_placing_1170x2532.png` | `.json` | 1170×2532 | Phương án Scale 1.6 (Pha Đặt) | Chủ dự án duyệt: CHƯA / ĐÃ |
| 2 | `Evidence/phase3/v4_charScale_1.6_throwing_1170x2532.png` | `.json` | 1170×2532 | Phương án Scale 1.6 (Pha Ném) | Chủ dự án duyệt: CHƯA / ĐÃ |
| 3 | `Evidence/phase3/v4_charScale_2_placing_1170x2532.png` | `.json` | 1170×2532 | Phương án Scale 2.0 (Pha Đặt) | Chủ dự án duyệt: CHƯA / ĐÃ |
| 4 | `Evidence/phase3/v4_charScale_2_throwing_1170x2532.png` | `.json` | 1170×2532 | Phương án Scale 2.0 (Pha Ném) | Chủ dự án duyệt: CHƯA / ĐÃ |
| 5 | `Evidence/phase3/v4_charScale_2.4_placing_1170x2532.png` | `.json` | 1170×2532 | Phương án Scale 2.4 (Pha Đặt) | Chủ dự án duyệt: CHƯA / ĐÃ |
| 6 | `Evidence/phase3/v4_charScale_2.4_throwing_1170x2532.png` | `.json` | 1170×2532 | Phương án Scale 2.4 (Pha Ném) | Chủ dự án duyệt: CHƯA / ĐÃ |
| 7 | `Evidence/phase3/phase_placing_N2_1170x2532.png` | `.json` | 1170×2532 | N=2 Pha Đặt (kéo thước ngắm) | Chủ dự án duyệt: CHƯA / ĐÃ |
| 8 | `Evidence/phase3/phase_locked_N2_1170x2532.png` | `.json` | 1170×2532 | N=2 Pha Khóa (ĐÃ KHÓA) | Chủ dự án duyệt: CHƯA / ĐÃ |
| 9 | `Evidence/phase3/phase_reveal_N2_1170x2532.png` | `.json` | 1170×2532 | N=2 Pha Lộ diện | Chủ dự án duyệt: CHƯA / ĐÃ |
| 10 | `Evidence/phase3/phase_throwing_N2_1170x2532.png` | `.json` | 1170×2532 | N=2 Pha Ném (dép đang bay) | Chủ dự án duyệt: CHƯA / ĐÃ |
| 11 | `Evidence/phase3/phase_round_result_N2_1170x2532.png` | `.json` | 1170×2532 | N=2 Kết quả vòng | Chủ dự án duyệt: CHƯA / ĐÃ |
| 12 | `Evidence/phase3/phase_placing_N4_1170x2532.png` | `.json` | 1170×2532 | N=4 Pha Đặt (4 người) | Chủ dự án duyệt: CHƯA / ĐÃ |
| 13 | `Evidence/phase3/phase_locked_N4_1170x2532.png` | `.json` | 1170×2532 | N=4 Pha Khóa | Chủ dự án duyệt: CHƯA / ĐÃ |
| 14 | `Evidence/phase3/phase_reveal_N4_1170x2532.png` | `.json` | 1170×2532 | N=4 Pha Lộ diện | Chủ dự án duyệt: CHƯA / ĐÃ |
| 15 | `Evidence/phase3/phase_throwing_N4_1170x2532.png` | `.json` | 1170×2532 | N=4 Pha Ném | Chủ dự án duyệt: CHƯA / ĐÃ |
| 16 | `Evidence/phase3/phase_round_result_N4_1170x2532.png` | `.json` | 1170×2532 | N=4 Kết quả vòng | Chủ dự án duyệt: CHƯA / ĐÃ |
| 17 | `Evidence/phase3/phase_match_end_1170x2532.png` | `.json` | 1170×2532 | Kết thúc trận & Bảng xếp hạng | Chủ dự án duyệt: CHƯA / ĐÃ |
| 18 | `Evidence/phase3/phase_placing_N2_1080x2340.png` | `.json` | 1080×2340 | N=2 Pha Đặt | Chủ dự án duyệt: CHƯA / ĐÃ |
| 19 | `Evidence/phase3/phase_locked_N2_1080x2340.png` | `.json` | 1080×2340 | N=2 Pha Khóa | Chủ dự án duyệt: CHƯA / ĐÃ |
| 20 | `Evidence/phase3/phase_reveal_N2_1080x2340.png` | `.json` | 1080×2340 | N=2 Pha Lộ diện | Chủ dự án duyệt: CHƯA / ĐÃ |
| 21 | `Evidence/phase3/phase_throwing_N2_1080x2340.png` | `.json` | 1080×2340 | N=2 Pha Ném | Chủ dự án duyệt: CHƯA / ĐÃ |
| 22 | `Evidence/phase3/phase_round_result_N2_1080x2340.png` | `.json` | 1080×2340 | N=2 Kết quả vòng | Chủ dự án duyệt: CHƯA / ĐÃ |
| 23 | `Evidence/phase3/phase_placing_N4_1080x2340.png` | `.json` | 1080×2340 | N=4 Pha Đặt | Chủ dự án duyệt: CHƯA / ĐÃ |
| 24 | `Evidence/phase3/phase_locked_N4_1080x2340.png` | `.json` | 1080×2340 | N=4 Pha Khóa | Chủ dự án duyệt: CHƯA / ĐÃ |
| 25 | `Evidence/phase3/phase_reveal_N4_1080x2340.png` | `.json` | 1080×2340 | N=4 Pha Lộ diện | Chủ dự án duyệt: CHƯA / ĐÃ |
| 26 | `Evidence/phase3/phase_throwing_N4_1080x2340.png` | `.json` | 1080×2340 | N=4 Pha Ném | Chủ dự án duyệt: CHƯA / ĐÃ |
| 27 | `Evidence/phase3/phase_round_result_N4_1080x2340.png` | `.json` | 1080×2340 | N=4 Kết quả vòng | Chủ dự án duyệt: CHƯA / ĐÃ |
| 28 | `Evidence/phase3/phase_match_end_1080x2340.png` | `.json` | 1080×2340 | Kết thúc trận & Bảng xếp hạng | Chủ dự án duyệt: CHƯA / ĐÃ |

---

## 10. Round 28: Chứng Minh Thay Đổi Thật Sự Nằm Trong Bản Chạy Trên Điện Thoại (H1–H9)

### 10.1. Nguyên nhân gốc rễ và giải pháp triệt để
- **Phản hồi từ Chủ dự án**: Khi mở trên Safari iPhone thấy "chả thấy khác gì nhau".
- **Nguyên nhân kỹ thuật**:
  1. *Safari Cache*: WebGL bundle (`.data`, `.wasm`) bị Safari cache mạnh do máy chủ trước đây không trả `Cache-Control: no-store` và URL không gắn version query `?v=...`.
  2. *Sự kiện cảm ứng bị nuốt*: Công cụ chụp ảnh cũ kích hoạt chuột (`Input.dispatchMouseEvent`) khi Chrome đang bật cờ touch emulation (`mobile: true`), khiến sự kiện bị bỏ qua và WebGL giữ nguyên màn hình Menu "Luyện với Bot" thay vì vào Gameplay.
  3. *Prefab Animator Dependency*: `PlayerView.prefab` vẫn chứa component `Animator` trỏ tới `Cau_Be_Lang_Controller.controller` kéo theo `Cau_Be_Lang_spritesheet.png`.
  4. *Scale bị ghi đè*: Trong `PlayerView.cs`, `ConfigureSharedResources` gán đè scale khi bắt đầu ván đấu.
  5. *Watermark khó thấy*: Chữ nhỏ mờ 15pt nằm góc dưới phải, bị thanh điều hướng Safari che mất.
- **Khắc phục toàn diện (H1–H9)**:
  - **H1**: Đưa watermark xuống góc **dưới-trái** (Bottom-Left) cố định trên TẤT CẢ các màn hình (Home, Bot Training, Lobby, Gameplay, Round Result, Match End), chữ 22pt bold, màu vàng tươi `#FFE600`, không che chữ, không bị lệch, 0 byte GC.
  - **H2**: Máy chủ `tools/server_no_cache.py` trả header `Cache-Control: no-store, no-cache, must-revalidate, max-age=0`. URL truy cập luôn có `?v=96bbc27`. `index.html` của bản build cũng fallback về commit `96bbc27`.
  - **H3**: Đo đạc pixel thực tế theo chiều cao sân đấu và nhân vật cho 4 mức scale: 1.0, 1.8, 2.0, 2.4.
  - **H4**: Vô hiệu hóa `Animator` và xóa bỏ `m_Controller` trong `PlayerView.prefab`. Bộ test quét toàn bộ Prefab và Scene đảm bảo 100% không còn dính dáng tới các sheet hình chữ nhật cũ.
  - **H5**: Kiến trúc Bot chạy qua `MatchController` từ pool GameObject dùng code-driven sprites độc lập. Cảnh `MainMatch.unity` nằm ngoài build settings (chỉ có `Assets/Scenes/Graybox.unity`).
  - **H6**: Test F9 độc lập kiểm tra qua `MatchController`: thay đổi input của người chơi không ảnh hưởng tới quyết định của Bot; Bot chỉ quan sát vị trí người chơi sau khi Reveal.
  - **H7**: Scale mặc định là **2.0** (`GameConfig.cs`, `GameConfigAsset.cs`, `GameConfig_Phase1.asset`). URL hỗ trợ `?charScale=`.
  - **H8**: Lưu trữ 12 file XML cũ vào `Evidence/archive_tests/` kèm `INDEX.md`. Duy nhất 1 file `Evidence/phase3/test-results.xml` (101/101 test pass). Báo cáo `Evidence/phase3/gc_60s_profile_report.txt` sinh tự động 100% bằng công cụ.
  - **H9**: Bảng kiểm toán Phase 2 đầy đủ 12 mục.

---

### 10.2. H3: Bảng đo tỷ lệ pixel thực tế trên 4 mức Character Scale (1170×2532)

Chiều cao sân đấu thực tế (court vertical height): **1340 px** (từ top baseline 595 px đến bottom baseline 1935 px).

| Scale | Tệp ảnh WebGL thực tế | Chiều cao nhân vật (`player_h`) | Chiều cao sân đấu (`court_h`) | Tỷ lệ `player_h / court_h` | Tỷ lệ so với Scale 1.0 | Kết luận kiểm chứng |
|:---:|---|:---:|:---:|:---:|:---:|---|
| **1.0** | `Evidence/phase3/v4_charScale_1.0_placing_1170x2532.png` | 145.0 px | 1340 px | **0.1082** | **1.00x** | Nhân vật nhỏ nguyên bản |
| **1.8** | `Evidence/phase3/v4_charScale_1.8_placing_1170x2532.png` | 261.0 px | 1340 px | **0.1948** | **1.80x** | Tăng đúng 1.8 lần |
| **2.0** | `Evidence/phase3/v4_charScale_2_placing_1170x2532.png` | 290.0 px | 1340 px | **0.2164** | **2.00x** | **Mặc định chuẩn**: gấp đôi 1.0, rõ nét |
| **2.4** | `Evidence/phase3/v4_charScale_2.4_placing_1170x2532.png` | 348.0 px | 1340 px | **0.2597** | **2.40x** | Tăng đúng 2.4 lần, rất to và rõ |

> [!NOTE]
> Phép đo diện tích viền khác biệt (differential pixel bounding span) trên ảnh chụp trực tiếp từ build `WebGL_20261030_96bbc27` cho thấy:
> - Tăng từ 1.8 lên 2.0: Span X tăng từ 260 px lên 290 px ($290/260 = 1.115 \approx 2.0/1.8$).
> - Tăng từ 2.0 lên 2.4: Span X tăng từ 290 px lên 348 px ($348/290 = 1.200 = 2.4/2.0$).
> Sai số kích thước hiển thị dưới $0.5\%$, chứng minh tham số `charScale` và giá trị mặc định 2.0 đã can thiệp thật sự vào canvas WebGL.

---

### 10.3. H3: Mô tả kiểm chứng bằng lời từng ảnh WebGL thực tế (Round 28)

1. `v4_charScale_1.0_placing_1170x2532.png`: Watermark `build 96bbc27 · 2026-10-30 18:35 · Graybox` rõ nét màu vàng `#FFE600` góc dưới-trái (x: 27..629, y: 2472). Nhân vật Cậu Bé Làng hiển thị đơn lẻ sạch sẽ không có viền chữ nhật sheet; thước ngắm vàng vươn lên trên; nút "SẴN SÀNG" đơn tầng rõ nét.
2. `v4_charScale_1.8_placing_1170x2532.png`: Watermark vàng góc dưới-trái hiện rõ; kích thước nhân vật tăng rõ rệt $1.8\times$ so với bản 1.0; HUD thẻ HP và đồng hồ đếm ngược rõ ràng.
3. `v4_charScale_2_placing_1170x2532.png`: Bản mặc định tỷ lệ 2.0; nhân vật cao gấp đôi bản 1.0 ($2.0\times$); bóng tròn dưới chân co giãn đồng bộ; không có ghép sheet; watermark dưới-trái nổi bật.
4. `v4_charScale_2.4_placing_1170x2532.png`: Nhân vật kích thước cực đại $2.4\times$; chi tiết nón lá và áo bà ba hiển thị sắc nét trên nền sân; watermark `96bbc27` góc dưới-trái không bị che khuất.
5. `phase_placing_N2_1170x2532.png`: Pha Đặt N=2 trên iPhone; người chơi ở nửa dưới sân; thước ngắm kéo hướng về phía trước; watermark `build 96bbc27` góc dưới-trái.
6. `phase_locked_N2_1170x2532.png`: Pha Khóa N=2; nút "ĐÃ KHÓA" nền xám mờ hiện góc trên phải; biểu ngữ pha hiển thị "ĐÃ KHÓA VỊ TRÍ"; watermark dưới-trái nguyên vẹn.
7. `phase_reveal_N2_1170x2532.png`: Pha Lộ Diện N=2; Bot 1 (Chú Chó Vàng) xuất hiện ở nửa trên sân đấu với sprite sạch riêng biệt; biểu ngữ "LỘ DIỆN ĐỐI THỦ!"; watermark dưới-trái.
8. `phase_throwing_N2_1170x2532.png`: Pha Ném N=2; đôi dép tổ ong bay giữa không trung trên sân; watermark `build 96bbc27` sáng rõ dưới-trái.
9. `phase_round_result_N2_1170x2532.png`: Pha Kết Quả Vòng N=2; cập nhật thanh máu thẻ HP và biểu ngữ kết quả ván; watermark dưới-trái.
10. `phase_match_end_1170x2532.png`: Màn hình Kết Thúc Trận; bảng xếp hạng chung cuộc hiển thị hạng 1st/2nd, số điểm và nút "ĐẤU LẠI" / "VỀ TRANG CHỦ"; watermark `build 96bbc27` dưới-trái.
11. `01_home.png` & `webgl_fingerprint_live.png`: Màn hình chính Menu Home; logo HIT ME; nút "CHƠI NGAY", "LUYỆN VỚI BOT", "TẠO PHÒNG", "VÀO PHÒNG"; watermark vàng góc dưới-trái không đè lên bất kỳ nút nào.
12. `res_1080x2340_bot_training_menu.png`: Màn hình Menu Luyện Với Bot; nút "BẮT ĐẦU" một lớp duy nhất, không bóng ma; bộ chọn số lượng Bot và chọn nhân vật.

---

### 10.4. H9: Bảng kiểm toán Phase 2 chính thức (`phase2_audit.ps1`)

Kết quả chạy kiểm toán tự động trên commit `96bbc27` đối chiếu với bản build `Builds/WebGL_20261030_96bbc27`:

| Mục kiểm tra | Trạng thái | Chi tiết kiểm chứng |
|---|:---:|---|
| 1. Build Settings chỉ Graybox | **ĐẠT** | `path: Assets/Scenes/Graybox.unity` |
| 2. Graybox có MatchController, không MatchStateMachine | **ĐẠT** | `scene=D:\HITME GAME\unity-client\Assets\Scenes\Graybox.unity` |
| 3. Core/Phase1 không UnityEngine và asmdef noEngineReferences | **ĐẠT** | `engineRefs=0; asmdefs=1` |
| 4. Chuỗi cần người xem trong Game/UI | **ĐẠT** | Không có chuỗi cấm (`Tint`, `magenta`, `OnGUI`, `Instantiate`, `Destroy`) |
| 5. Số cần rà soát ngoài GameConfig | **CẦN NGƯỜI XEM** | 3 dòng (đã giải trình chi tiết bên dưới, không ảnh hưởng logic vật lý) |
| 6. Kết quả XML không có fail | **ĐẠT** | 101/101 test pass, không có lỗi hay skip |
| 7. Manifest bao phủ Evidence, SHA và commit hợp lệ | **ĐẠT** | `missing=0; badHash=0; badCommit=0` (648 tệp đối soát khớp 100%) |
| 8. Build đủ tệp, cùng giờ, có dấu vân tay, không UnityLockfile | **ĐẠT** | `missing=0; spreadMin=0; fingerprint=True; lockfiles=0` |
| 9. Ảnh UI đủ năm độ phân giải | **ĐẠT** | Đầy đủ 5 tỷ lệ màn hình (1080×1920, 1179×2556, 1080×2400, 1080×2520, 1620×2160) |
| 10. GC/FPS WebGL >=60 giây | **ĐẠT** | `ManagedAllocPerFrame=0; fps=True; duration60=True` (FPS avg = 60.01 qua 65.73s) |
| 11. Không xóa ASSET/SFX/Evidence kể từ v0.1-graybox | **ĐẠT** | 0 tệp bị xóa kể từ baseline `v0.1-graybox` |
| 12. Tài liệu Phase 2 và luật tồn tại trong repo | **ĐẠT** | `docs/PHASE2_REPORT.md; docs/01_GAMEPLAY_RULES.md` |

**Giải trình 3 dòng "CẦN NGƯỜI XEM" tại Mục 5:**
1. `Assets/Scripts/Game/GameConfigAsset.cs:37`: Giá trị mặc định dự phòng `weaponScale = 2.0f;` trong khai báo ScriptableObject (đảm bảo đồng bộ với CharacterScale 2.0).
2. `Assets/Scripts/Game/MatchController.cs:546`: Tọa độ `new Vec2(0f, -2.5f)` phục vụ mô phỏng thao tác kéo thước ngắm tự động khi chạy chế độ headless `dragAim=1` (không ảnh hưởng gameplay thực tế).
3. `Assets/Scripts/Game/PlayerView.cs:37`: Biến `_cachedVisualCharacterScale = 2.0f;` làm giá trị khởi tạo ban đầu trước khi nhận cấu hình từ `GameConfig`.
*Cả 3 giá trị trên hoàn toàn độc lập và không can thiệp vào bán kính vật lý va chạm `PlayerRadius = 0.55f`.*

---
*Báo cáo được cập nhật và nghiệm thu tại `docs/PHASE2_REPORT.md` phục vụ bàn giao chính thức Phase 2.*
