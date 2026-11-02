# HIT ME — Phase 1 Report

**Ngày:** 28/10/2026 (ICT)  
**Phạm vi:** gameplay graybox offline; không làm Phase 2.

## Kết quả

Core thuần C# (`HitMe.Core`) có asmdef riêng, game dùng 1 người chơi + 5 bot, luật đồng thời được kiểm thử bằng EditMode. `Graybox.unity` được tạo riêng; `MainMatch.unity` và asset cũ được giữ nguyên.

### Luật đã chốt

- Vũ khí dừng ở người đầu tiên trúng (`maxHitsPerThrow = 1`).
- Trận thường kết thúc khi còn một người sống.
- Hai người cuối cùng cùng chết: cả hai đồng thắng.
- Hết pha đặt: dùng vị trí đang đứng và hướng mặt/hướng đã khóa, vẫn ném.
- Không tự trúng, không có đồng đội.

### Luật và cấu hình đang áp dụng trong code

`HitCalculator` sắp mục tiêu theo khoảng cách trên tia ném và `maxHitsPerThrow = 1`, nên vũ khí dừng ở người đầu tiên. `RoundResolver` dùng ảnh chụp vị trí, cộng dồn damage rồi mới giảm HP; không có tự trúng. `MatchState` kết thúc khi còn đúng một người sống; nếu không ai sống sau lượt đồng thời, mọi người còn sống trước lượt đó đồng thắng (bao gồm hai người cuối cùng).

`GameConfig_Phase1.asset`: `maxPlayers=6`, `startHp=3`, `placeSeconds=5`, `maxRounds=5` (hiện chưa override luật last-survivor), `roundLimitTiebreak=ContinueUntilElimination`, sân `10×16`, `playerRadius=0.45`, `weaponSpeed=14`, `damagePerHit=1`, `maxHitsPerThrow=1`, `minSpawnDistance=0.9`, `minAimDrag=0.55`, `aimTouchRadiusMultiplier=1.75` (vùng chạm 0.7875 world), `aimLineExtraLength=0.35`, `noAimRule=ThrowFacing`, `seed=12345`. Khi hết giờ chưa đặt, `LockRound` giữ vị trí hiện tại và ném theo `NoAimPolicy` mặc định là hướng mặt/hướng đã có.

### Rà soát số vòng và giới hạn trận

Trận iPhone do chủ dự án chơi đã tới **Vòng 19** rồi `MatchEnd` (BẠN thắng còn 1 HP). Đây xác nhận `maxRounds=5` đang chỉ là tham số lưu trong `GameConfig`, **không được `MatchState` dùng để kết thúc trận**; hành vi hiện tại vẫn là tiếp tục cho tới khi còn một người sống hoặc xảy ra đồng thắng sau lượt đồng thời.

Đã chạy lại 1.000 trận bot deterministic: trung bình **25,801** vòng, thấp nhất **4**, cao nhất **87**. Phân bố chi tiết từng số vòng và từng seed nằm trong `unity-client/Evidence/phase1/sim.txt`; Vòng 19 nằm trong miền phân bố này. Không thay luật kết thúc.

Để đổi nhanh khi chủ dự án duyệt một giới hạn vòng, `GameConfig`/`GameConfigAsset` đã có sẵn `maxRounds` và `RoundLimitTiebreakPolicy { ContinueUntilElimination, HighestHp, SharedWin }`. Giá trị asset là `ContinueUntilElimination`; lựa chọn tiebreak chưa được nối vào luật kết thúc. Test tham số cả ba lựa chọn xác nhận hiện chưa làm đổi hành vi Phase 1.

## Luật nhập liệu Graybox (xác nhận 28/10/2026)

| Luật | Trạng thái | Cách áp dụng |
|---|---|---|
| Đặt ở bất kỳ đâu trong sân, kẹp biên và giữ khoảng cách | ĐẠT | Chạm vùng trống chuyển `PlacementGestureState` sang `Moving`; `MatchController.SyncGesturePreview` gọi `MatchState.ConstrainPlacement`, nên dùng biên và `minSpawnDistance` của `GameConfig`. Có thể đặt/kéo-thả lại nhiều lần. |
| Giữ nhân vật rồi lướt để chọn hướng, không lực | ĐẠT | Chạm trong vòng cảm ứng `playerRadius × 1.75` chuyển sang `Aiming`; hướng là vector từ vị trí nhân vật tới điểm kéo. `minAimDrag=0.55`; kéo về gần nhân vật hủy aim. Một gesture không thể vừa `Moving` vừa `Aiming`. |
| Hết 5 giây tự khóa và ném; SẴN SÀNG khóa sớm | ĐẠT | Vòng `Placing` đếm `placeSeconds`; khi hết thời gian gọi `LockRound` rồi tự sang Reveal/Throwing. Nút **SẴN SÀNG** chỉ là lựa chọn khóa sớm, đúng với ngoại lệ chủ dự án nêu nên không xung đột luật timeout. |
| Chưa chọn hướng | ĐẠT — fallback đã chốt | Nhân vật **vẫn ném** theo hướng mặt tại đầu pha (mặc định `Vec2.Up`). Nếu có aim hợp lệ thì dùng aim cuối; nếu kéo về dưới `minAimDrag`, aim bị hủy và quay lại fallback. |
| Pha đặt chỉ thấy người chơi | ĐẠT | `MatchController.Present` ẩn mọi `PlayerView` bot trong Placing và `HudView` cũng không liệt kê bot. |

### Rà soát input và cảm ứng

Trước cập nhật: click ngoài nhân vật đã dời vị trí có kẹp biên/khoảng cách, và kéo từ nhân vật đã đổi hướng; nhưng chưa có `minAimDrag`, không hủy aim khi kéo ngược, chưa tách state thuần C#, luôn gửi `HasAim=true`, và Placing hiển thị bot. Bản hiện tại thay các phần thiếu/sai này bằng `PlacementGestureState` thuần C#; Core combat (HP, hit calculation, resolve đồng thời) không thay đổi.

Project hiện dùng **Input Manager cũ** (`UnityEngine.Input`); package Input System và Device Simulator chưa được cài/bật. Cả mouse lẫn touch đều được `PlacementInput` chuyển thành `PointerSample`, rồi gọi cùng `MatchController.HandlePointer → PlacementGestureState`. `ActivePointerGate` giữ finger đầu tiên nên finger thứ hai không đổi/hủy aim đang kéo. Unity 6 hỗ trợ Input System/Device Simulator, nhưng chưa thể tuyên bố giả lập Device Simulator vì dependency chưa có trong project; phương án kiểm chứng là bản WebGL qua LAN và thiết bị thật.

### Chẩn đoán WebGL Safari (28/10/2026)

Lỗi iPhone ban đầu **đã tái hiện trên Chrome desktop và Device Toolbar**, nên không phải mobile-only: Console ghi `Can't add component 'LineRenderer'` và `NullReferenceException`; `PlayerView` thêm hai `LineRenderer` trên cùng object rồi làm ngắt khởi tạo trận (HUD/state không lên). Đã sửa lớp presentation: mỗi line ở child object riêng, dùng material đã có từ `SpriteRenderer` thay `Shader.Find/new Material`, camera Graybox dùng Solid Color. Khi kiểm tra tiếp, sân còn quá nhỏ vì sprite trắng 1 px dùng PPU mặc định 100; đã đổi PPU=1 để scale `10×16` đúng. Bản rebuild cuối hiển thị HUD, sân có viền, nhân vật và vòng ngắm trong Chrome Device Toolbar; Console không còn exception này. Bằng chứng: `unity-client/Evidence/phase1/webgl_debug.md`, `webgl_chrome_device_fixed.png`.

WebGL dùng template `PROJECT:Phase1Mobile`: viewport khóa zoom, `touch-action:none`, `overscroll-behavior:none`, cấm selection/callout và `preventDefault` cho touch canvas để Safari không cuộn trang. URL `?debugPointer=1` bật telemetry (scene/FPS/touch/raw/world/gesture) và chỉ giữ pha Placing cho chẩn đoán. Chrome automation chỉ tạo mouse (`touchCount=0`) nhưng click đã dời được nhân vật. Chủ dự án đã xác nhận trên **iPhone/Safari/WebGL qua LAN ngày 28/10/2026**: nhận cảm ứng, đặt/ngắm được, chơi trọn một trận tới Vòng 19; `MatchEnd`, BẠN thắng 1 HP, bảng kết quả và nút **CHƠI LẠI** hoạt động. Ảnh gốc: `unity-client/Evidence/phase1/iphone_safari_round8.png`, `iphone_safari_round14.png`, `iphone_safari_matchend_round19.png`.

Safari từng cache bộ file WebGL tên cố định. Template truyền `v` vào URL của loader/data/framework/wasm; artifact đóng Phase 1 dùng mặc định `phase1-closed-20261028` và có thể mở bằng `?v=phase1-closed-20261028` để buộc tải mới.

Ngưỡng 0.25 world tương đương khoảng 27 px trên chiều rộng 1080 của sân 10 world, quá dễ bị rung ngón cái. Đã đổi sang **0.55 world** (khoảng 59 px), và thêm vòng chạm 0.7875 world cùng thước vàng bắt đầu ngoài vòng đó, kéo dài hơn vị trí pointer 0.35 world. Giá trị là trường `GameConfig`, không hard-code. Chủ dự án đã xác nhận thao tác đặt/ngắm trên thiết bị thật.

## Definition of Done

| Mục | Trạng thái | Bằng chứng / ghi chú |
|---|---|---|
| Audit Phase 1 được duyệt | ĐẠT | `MD/AUDIT_REPORT.md` đã cập nhật luật chủ dự án duyệt. |
| Core C# thuần + asmdef | ĐẠT | `Assets/Scripts/Core/Phase1/HitMe.Core.asmdef`; không tham chiếu `UnityEngine`. |
| Test luật + gesture | ĐẠT | `unity-client/Evidence/phase1/tests.xml`: **24/24 pass** (28/10/2026), gồm đặt lại, aim, dưới ngưỡng, kéo ngược hủy, timeout, chặn finger thứ hai, ba `NoAimPolicy` và ba cấu hình round-limit tiebreak. |
| 1.000 trận bot | ĐẠT | `Evidence/phase1/sim.txt`: 1.000 trận hoàn tất; trung bình 25,801, thấp nhất 4, lớn nhất 87 vòng và có phân bố theo seed. |
| AutoPlay 10 trận | THAY THẾ | `Evidence/phase1/playtest_auto.txt`: batchmode dùng `Phase1BotBrain` cho cả người chơi và bot; 10/10 trận, 0 exception, 0 treo. Không thay thế trực quan hoàn toàn cho test tay. |
| Screenshot Placing/Reveal/Throwing/Result | ĐẠT | Player Windows có đồ họa tự chụp runtime ở 1080×2340: `unity-client/Evidence/phase1/shot_1_placing.png`, `shot_2_reveal.png`, `shot_3_throwing.png`, `shot_4_result.png`. WebGL Device Toolbar sau sửa: `unity-client/Evidence/phase1/webgl_chrome_device_fixed.png`. |
| Kiểm tra input mouse/touch | ĐẠT (code + test) | Chuột/cảm ứng đi chung `PointerSample → PlacementGestureState`; `ActivePointerGate` có test chặn finger thứ hai. Input Manager cũ đang được dùng; `unity-client/Evidence/phase1/tests.xml`. |
| Thử cảm ứng trên thiết bị thật | ĐẠT | iPhone, Safari, WebGL qua LAN; chủ dự án xác nhận 28/10/2026. Nhận touch, đặt/ngắm, chơi hết 1 trận. Ảnh: `unity-client/Evidence/phase1/iphone_safari_round8.png`, `iphone_safari_round14.png`, `iphone_safari_matchend_round19.png`. |
| Chơi tay | ĐẠT | Chủ dự án chơi trên iPhone/Safari tới `MatchEnd` ở Vòng 19, BẠN thắng 1 HP; bảng kết quả và nút **CHƠI LẠI** hiện đúng. `Evidence/phase1/iphone_safari_matchend_round19.png`. |
| Build thử | ĐẠT | `unity-client/Evidence/phase1/build.txt`: Windows 64-bit succeeded. `unity-client/Evidence/phase1/build_webgl.txt`: WebGL succeeded; output đủ `index.html`, `.data`, `.wasm`, `.framework.js`, `.loader.js`; custom mobile template đã được đóng gói. |
| Không xóa asset/scene cũ | ĐẠT | Không xóa thư mục/scene/prefab; có scene mới `Graybox.unity`. |
| Commit/tag `v0.1-graybox` | ĐẠT | Tag được dời về commit đóng Phase 1 sau build/test/evidence và push GitHub. |

## Kiểm tra screenshot runtime

- `shot_1_placing.png`: pha Placing, chỉ có **BẠN** (bot bị ẩn đúng luật mới), sân graybox, HUD 5 giây, nút **SẴN SÀNG** và thước ngắm vàng.
- `shot_2_reveal.png`: pha Reveal, đủ 6 nhân vật, HUD và các hướng ném đã khóa.
- `shot_3_throwing.png`: pha Throwing, đủ 6 nhân vật, HUD và sáu projectile vàng đang bay.
- `shot_4_result.png`: pha MatchEnd, sân/nhân vật còn hiển thị, bảng **KẾT QUẢ** có người thắng, xếp hạng sáu người và nút **CHƠI LẠI**.

Máy Windows đã clamp cửa sổ player gốc ở 1080×1800. Đây không phải ảnh dựng: script gọi `ScreenCapture` ở cuối frame từ player có đồ họa, khóa projection camera theo aspect 1080/2340, rồi xuất canvas evidence 1080×2340 từ chính capture đó. Bốn PNG trên đều đã được kiểm tra kích thước 1080×2340 và mở xem trực tiếp.

## Quyết định luật chờ duyệt (Bước C)

Codex/AG đang áp dụng cấu hình mặc định: hết giờ mà chưa ngắm hoặc đã chủ động hủy ngắm thì ném theo hướng mặt hiện tại (mặc định hướng lên: `NoAimPolicy.ThrowFacing`).
Đã chuẩn bị sẵn enum `NoAimPolicy { ThrowFacing, NoThrow, RepeatLastAim }` trong `GameConfig` và `GameConfigAsset` kèm unit test. Giá trị asset hiện là `ThrowFacing`, nên hành vi mặc định Phase 1 không đổi; `NoThrow` và `RepeatLastAim` chỉ là lựa chọn cấu hình **chờ chủ dự án duyệt**.
Ba câu hỏi luật chờ chủ dự án quyết:
1. **Hết 5 giây chưa ngắm:** Ném theo hướng mặt hiện tại (`ThrowFacing`), không ném (`NoThrow`), hay lặp lại hướng ngắm vòng trước (`RepeatLastAim`)?
2. **Chủ động kéo ngược về gần nhân vật để hủy ngắm:** Xử lý như câu 1 hay tính là người chơi chủ động bỏ ném?
3. **Vòng 1 khi chưa có hướng cũ:** Nếu chọn `RepeatLastAim` thì vòng 1 sẽ fallback về hướng mặt (`Vec2.Up`).

## Vấn đề đã biết chuyển sang Phase 2

- **Chữ tiếng Việt mất dấu trên WebGL:** cần chuyển sang TMP và font có glyph tiếng Việt.
- **HUD:** chữ nhỏ và bị khung sân che.
- **Bảng kết quả:** đang đè lên nhãn bot.
- **Thẻ trang:** từng hiện `{{{ PRODUCT_NAME }}}`; source/template và WebGL artifact đóng Phase 1 đã đổi thành **HIT ME — Graybox**. Cần mở lại Safari với cache mới để xác nhận UI browser đã nhận artifact này.
- **Che ngón tay:** chủ dự án đã thao tác được; Phase 2 nên tiếp tục tinh chỉnh sau khi có asset/UI thật.
