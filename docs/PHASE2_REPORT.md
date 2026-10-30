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

> [!NOTE]
> **Hiện trạng bộ test hiện tại**: **92/92 bài kiểm thử ĐẠT 100% (0 lỗi, 0 bỏ qua)**.

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
- Đã xuất bản bản build WebGL fingerprinted sạch: `Builds/WebGL_20261030_6e0da22` (SHA-256 tệp hợp lệ, thời gian đồng bộ, không `UnityLockfile`).
- **Địa chỉ truy cập LAN**: `http://172.20.10.3:8091/` (và `http://localhost:8091/`).
- Bản cũ `Builds/WebGL_20261029_942ee08` trên cổng `8090` (PID 32916) được giữ nguyên vẹn 100%, không bị chạm tới.

### 3.5. S6 – Đo Kiểm Cấp Phát 0 B/Khung Hình & Hiệu Năng > 60 Giây
- **Chạy tự động 50 trận liên tiếp**: 65.86 giây (> 60 seconds), 3,749 khung hình Unity, 0 lỗi console, 0 exceptions, 0 treo máy.
- **FPS**: Trung bình rAF **60.01 FPS**, tối thiểu cửa sổ 1 giây **59.95 FPS**.
- **Cấp phát bộ nhớ**: `ManagedAllocPerFrame: 0` trên toàn bộ các khung hình steady-state (nhờ tái sử dụng `_localFinalInput`, bộ đệm `_weaponHitsBuffer`, `SharedHitsBuffer`, `_cachedInput`, `_aliveBeforeBuffer`).

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

> **Địa chỉ WebGL LAN**: **`http://172.20.10.3:8091/`**  
> *(Lưu ý: Điện thoại cần kết nối chung mạng Wi-Fi hoặc điểm phát sóng Hotspot với PC chạy máy chủ. Nếu đổi Wi-Fi, IP có thể thay đổi tương ứng).*

### 6.1. Quy trình thử nghiệm 3 trận
1. **Trận 1 (Cử chỉ cơ bản & Sức ném)**:
   - Chạm vào vị trí bất kỳ trên phần sân dưới của mình $\rightarrow$ quan sát nhân vật xuất hiện đúng tâm điểm chạm.
   - Chạm giữ và vuốt ngược tạo thước ngắm $\rightarrow$ buông tay ném dép $\rightarrow$ quan sát đường bay khớp hoàn toàn góc thước ngắm.
2. **Trận 2 (Thao tác sai & Hủy thao tác)**:
   - Trong lúc đang kéo thước ngắm, chạm ngón tay thứ hai vào chỗ khác $\rightarrow$ kiểm tra thao tác bị hủy và reset hoàn toàn để làm lại.
   - Kéo thước ngắm lùi về tâm dưới 4 mm (khoảng 20–30 px) $\rightarrow$ kiểm tra thước ngắm tự biến mất (hủy ngắm thành công).
   - Buông tay mà không kéo ngắm $\rightarrow$ kiểm tra nhân vật ném dép thẳng theo hướng mặt mặc định (`ThrowFacing`).
3. **Trận 3 (Hiển thị UI & Dọn dẹp cuối trận)**:
   - Quan sát cụm Thẻ HP phía trên cổng làng: luôn mở công khai, không bị rào chắn hay chi tiết cổng che khuất.
   - Khi trận đấu kết thúc: quan sát toàn bộ dép đang bay và hiệu ứng nổ/bụi biến mất tức thì, không tồn đọng rác.
   - Thử bấm nút **Replay (Chơi lại)** và nút **Về sảnh (Home)**: kiểm tra chuyển cảnh mượt mà, không giật lag.

### 6.2. Cách đọc FPS và Mẫu báo cáo nghiệm thu
- **Cách đọc FPS**: Trên Android, có thể bật tùy chọn nhà phát triển *"Hiển thị tốc độ làm mới"* (Show refresh rate), hoặc kết nối cáp USB mở Chrome trên PC gõ `chrome://inspect` để xem FPS thực tế của tab WebGL.
- **Mẫu báo kết quả Chủ dự án gửi lại**:
  ```markdown
  THỬ NGHIỆM S5 TRÊN ĐIỆN THOẠI:
  - Thiết bị thử nghiệm: [Tên điện thoại, ví dụ: iPhone 14 Pro / Samsung S23]
  - Trình duyệt: [Safari / Chrome / Samsung Internet]
  - Cảm ứng đặt vị trí & vuốt ném: [Mượt / Có độ trễ / Khó chạm]
  - Thẻ HP & rào chắn: [Rõ ràng / Bị che]
  - Replay & Về sảnh: [Hoạt động tốt / Bị đơ]
  - Đánh giá chung: [ĐẠT / CẦN CHỈNH SỬA THÊM]
  ```

---

## 7. BẢNG ĐỐI CHIẾU LUẬT CHƠI v1.2 / v1.3 ↔ BỘ KIỂM THỬ CLIENT (S8)

| Điều luật (v1.2 & v1.3) | Yêu cầu nghiệp vụ | Tên Test Client tương ứng | Trạng thái |
|---|---|---|:---:|
| **Máu khởi điểm** | Mỗi người chơi có 3 HP, nhận 1 sát thương mỗi lần bị trúng | `Phase1CoreTests.MultipleHitsAccumulateAndHpDoesNotGoBelowZero` | **ĐẠT** |
| **Pha đặt vị trí bí mật** | 5 giây đặt vị trí, che giấu vị trí và hướng ngắm đối thủ | `Phase1CoreTests.Placing_OpponentPositionAndAimRemainHidden` | **ĐẠT** |
| **HP công khai mọi pha** | Thẻ HP của tất cả người chơi luôn hiển thị, không che nhầm | `Phase1CoreTests.HP_AlwaysPublicAcrossAllPhases` | **ĐẠT** |
| **Ném đồng thời** | Vũ khí bay đồng thời, sát thương tính cùng lúc | `Phase1CoreTests.MutualThrowsResolveSimultaneously` | **ĐẠT** |
| **Không ngắm (No-Aim)** | Khi không kéo thước ngắm, dép bay thẳng hướng mặt `ThrowFacing` | `Phase1CoreTests.NoAim_DefaultsToThrowFacing` | **ĐẠT** |
| **Người bị loại (0 HP)** | Người 0 HP không thể ném tiếp và không bị tính va chạm thêm | `Phase1CoreTests.EliminatedPlayerNeitherThrowsNorCanBeHit` | **ĐẠT** |
| **Giới hạn 5 vòng đấu** | Trận đấu dừng ngay sau vòng 5 nếu chưa ai bị loại hết | `Phase1CoreTests.P1_MatchState_MaxRoundsEnforced_MatchEndsAtRound5` | **ĐẠT** |
| **Xếp hạng Tiebreak** | Thứ tự: Còn sống $\rightarrow$ HP cao $\rightarrow$ Số lần ném trúng $\rightarrow$ Đồng hạng | `Phase1CoreTests.S1_MatchRanking_Deterministic_WithAllTiebreakPolicies` | **ĐẠT** |
| **Hỗ trợ phòng N = 2..6** | Xếp hạng và chia vị trí linh hoạt cho phòng từ 2 đến 6 người | `Phase1CoreTests.MatchRanking_SupportsDifferentPlayerCounts_2To6` | **ĐẠT** |
| **Phòng Online không Bot** | Trận online cần $\ge 2$ người thật; N=1 khóa bắt đầu, phòng trống ghi "Đang chờ..." | `ProductionUIPolishTests.Lobby_OneHuman_StartLocked_AndWaitingSlotsDisplayed` | **ĐẠT** |
| **Luyện với bot cục bộ** | Menu riêng biệt chọn 1–5 bot và nhân vật, chạy thuần `Core/Phase1` | `Phase1CoreTests.AutoPlay_10MatchesEach_N2ToN6_CompletesWithZeroErrorsAndValidRankings` | **ĐẠT** |

---

## 8. DANH SÁCH TỒN ĐỌNG KỸ THUẬT BÀN GIAO (S9 DEFERRED BACKLOG)

Dưới đây là danh sách chi tiết các hạng mục được bảo lưu chuyển sang Phase 3 hoặc chờ phản hồi từ Chủ dự án:

| # | Hạng mục tồn đọng | Lý do hoãn / chuyển tiếp | Mức ảnh hưởng | Kế hoạch giải quyết |
|---|---|---|:---:|---|
| **W1–W3** | **Sprite dép bị mất đầu mũi / quai** (`w1_sandal_sprite_diagnosis.png`) | File sprite gốc trong `ASSET HITME FINAL` bị cắt sát viền 1-2 px từ khâu thiết kế 2D; Chủ dự án đã cho phép hoãn | Trung bình (Thị giác) | Chờ họa sĩ gửi lại file PNG nguồn có lề đệm 4 px, client sẽ re-import trong Phase 3 |
| **S5** | **Đo kiểm FPS và cảm ứng trên điện thoại vật lý thực tế** | Phép đo tự động PC đạt 60 FPS 0 B/frame; trải nghiệm cảm ứng thực tế cần màn hình cảm ứng điện thoại của Chủ dự án | Quan trọng (Trải nghiệm) | Thực hiện ở bước S5 (Chủ dự án chơi thử 3 trận theo mục 6) trước khi gắn tag `v0.2-rc1` |
| **NET** | **Kết nối mạng thời gian thực WebSocket** | Phía server Codex đang hoàn thiện giao vận `net_client_v1` (Z1, Z2); chưa bàn giao SERVER READY | Quan trọng (Tính năng) | Kết nối mạng sẽ được tích hợp ngay khi Codex công bố SERVER READY ở đầu Phase 3 |
| **AUDIO** | **Hiệu ứng âm thanh mở rộng (BGM đa dạng, SFX theo vũ khí)** | Đã tích hợp đầy đủ âm thanh cơ bản (Click, Ready, Countdown, Ném, Trúng dép, Thắng, Thua); cần bổ sung nhạc nền theo mùa | Thấp (Thị giác/Thính giác) | Bổ sung khi có bộ asset audio hoàn chỉnh từ nhà sản xuất âm nhạc |

---
*Báo cáo được cập nhật và nghiệm thu tại `docs/PHASE2_REPORT.md` phục vụ bàn giao chính thức Phase 2.*
