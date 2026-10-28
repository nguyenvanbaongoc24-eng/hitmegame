# HIT ME VN — BÁO CÁO NGHIỆM THU TOÀN DIỆN PHASE 2 (v0.2-feel)

**Ngày:** 29/10/2026 (ICT)  
**Vai trò:** Antigravity (AG) Lead Technical Artist & Gameplay Engineer  
**Branch / Worktree:** `main` (`D:\HITME GAME\unity-client`)  
**Commit mục tiêu:** `v0.2-feel`  

---

## 0. AN TOÀN VÀ QUẢN LÝ TRẠNG THÁI GIT
- **Branch hoạt động:** Làm việc trực tiếp trên branch `main` của worktree `unity-client`.
- **Nguyên tắc bảo vệ:** Không can thiệp menu tổng, không đụng mạng/server/Supabase, không can thiệp Core logic ngoại trừ việc mở rộng tham số cấu hình timing và hàm chuyển đổi tọa độ theo yêu cầu. Không sửa file `LocalBot.unity`, không xóa file ASSET/SFX/Evidence/MD.
- **Quy trình commit & tag:** Toàn bộ công việc được commit nhỏ, tách biệt, kiểm thử hoàn tất 100% trước khi tạo và push tag `v0.2-feel`.

---

## 1. HOÀN THIỆN HÌNH ẢNH PHASE 2 (VISUAL COMPLETION)

### 1.1. UI / HUD Chuẩn Hóa Prefab + 9-Slice + TextMeshPro (TMP)
- **100% TextMeshPro (TMP) Native:** Toàn bộ hệ thống giao diện loại bỏ triệt để legacy `UnityEngine.UI.Text`. Các nhãn giao diện đều sử dụng `TextMeshProUGUI` kết hợp với asset font tiếng Việt tối ưu [BeVietnamPro-Bold SDF.asset](file:///d:/HITME%20GAME/unity-client/Assets/Fonts/BeVietnamPro-Bold%20SDF.asset) (2.2 MB), đảm bảo văn bản hiển thị siêu nét trên màn hình retina/high-DPI của thiết bị di động mà không bị vỡ hạt hay răng cưa.
- **Hệ thống UI Kit 9-Slice:** Cấu hình 9-slice borders tự động cho toàn bộ 10 sprite UI trong [Assets/Art/UI/Kit/](file:///d:/HITME%20GAME/unity-client/Assets/Art/UI/Kit/):
  - `ui_banner_wood.png`: Border `(64, 36, 64, 36)` dùng cho banner tiêu đề "HIT ME VN - CHIẾN ĐÌNH LÀNG".
  - `ui_btn_cta_yellow.png`: Border `(56, 44, 56, 40)` dùng cho nút CTA "CHƠI VỚI BOT", "ĐẤU LẠI".
  - `ui_btn_cta_green.png`: Border `(48, 40, 48, 36)` dùng cho nút "SẴN SÀNG!".
  - `ui_btn_cta_blue.png`: Border `(48, 40, 48, 36)` dùng cho nút "TẠO PHÒNG", "VỀ MENU".
  - `ui_hud_pill_dark.png`: Border `(48, 36, 48, 36)` dùng cho huy hiệu máu người chơi (`BẠN: ♥ ♥ ♥`) và bot (`BOT: ♥ ♥ ♥`).
  - `ui_input_box_cyan.png`: Border `(32, 32, 32, 32)` dùng cho khung dải thông báo giai đoạn (Phase Banner).
  - `ui_card_slot_selected_cyan.png`: Border `(32, 32, 32, 32)` dùng cho khung thẻ trang bị vũ khí.
  - `ui_panel_dark_gold.png`: Border `(48, 48, 48, 48)` dùng cho thẻ thống kê kết quả trận đấu.
- **Khắc phục lỗi Z-order:** Tách biệt hình ảnh nền `_Bg` thành sibling đứng ngay trước text trong hierarchy uGUI, đảm bảo chữ luôn nổi lên trên nền và có độ tương phản xuất sắc.

### 1.2. Sân Đấu Đình Làng & Ánh Xạ Tọa Độ (`GameToWorld` / `WorldToGame`)
- **Sân Đấu `ARENA.png` Ghép Chuẩn:** Sử dụng tác phẩm hội họa sân đình làng cổ truyền với phối cảnh 2.5D đặt tại `(0, 0.15, 0)` scale `1.22`. Camera 2.5D được khóa cố định góc chiếu trực diện (`pitchAngle = 0°`, `rotation = Quaternion.identity`, `orthographicSize = 7.2f`), bao quát toàn vẹn diện tích sân từ cổng đình đến bậc thềm tam cấp.
- **Hàm Ánh Xạ Bi-directional Tọa Độ [ArenaCoordinateMapper.cs](file:///d:/HITME%20GAME/unity-client/Assets/Scripts/Core/Phase1/ArenaCoordinateMapper.cs):**
  - Giới hạn thế giới Unity World Space: $X \in [-3.2, 3.2]$, $Y \in [-5.0, 4.2]$.
  - Giới hạn Core Logical Space: $X \in [-3.2, 3.2]$, $Y \in [-5.0, 4.2]$.
  - `GameToWorld(Vec2 gamePos)`: Ánh xạ vị trí logic Core sang Unity World coordinate, tự động kẹp biên an toàn.
  - `WorldToGame(Vec2 worldPos)`: Ánh xạ vị trí Unity World về Core logic.
  - Tính khả nghịch (Invertibility): $WorldToGame(GameToWorld(P)) = P$ với sai số $< 0.001\text{u}$ (đã có unit test tự động xác thực).

### 1.3. Bằng Chứng Tỉ Lệ Màn Hình 9:16, 9:19.5, 9:21 & Safe Area / Tai Thỏ
Đã chụp trực tiếp các tỉ lệ khung hình phổ biến trên mobile kèm đường viền mô phỏng Safe Area (tai thỏ notch $110\text{px}$ ở đỉnh và thanh vuốt home bar $70\text{px}$ ở đáy), chứng minh toàn bộ HUD, HP, đồng hồ, banner và nút bấm nằm hoàn toàn trong vùng an toàn, không bị tràn hay cắt chữ:
- **Tỉ lệ 9:16 chuẩn (1080x1920):** [aspect_9_16.png](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/aspect_9_16.png)
- **Tỉ lệ 9:19.5 hiện đại iPhone 13/14/15 (1080x2340):** [aspect_9_19_5.png](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/aspect_9_19_5.png)
- **Tỉ lệ 9:21 siêu dài Android (1080x2520):** [aspect_9_21.png](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/aspect_9_21.png)
- **Ảnh đối chiếu tổng hợp:** [aspect_ratios_composite.png](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/aspect_ratios_composite.png)

![Aspect Ratios Safe Area Composite](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/aspect_ratios_composite.png)

### 1.4. Kiểm Tra Tiếng Việt Đầy Đủ Dấu
Bảng ngôn ngữ tập trung [LocalizationTable.cs](file:///d:/HITME%20GAME/unity-client/Assets/Scripts/Core/Phase1/LocalizationTable.cs) được xác thực với 100% tiếng Việt có dấu chuẩn xác:
- `CHỌN HƯỚNG NÉM` (Banner giai đoạn ngắm)
- `SẴN SÀNG` (Nút xác nhận hành động)
- `Đối thủ đang ẩn` (Mô tả trạng thái che giấu mục tiêu)
- `Thắng` / `CHIẾN THẮNG!` (Thông báo thắng cuộc)
- `Thua` / `THẤT BẠI!` (Thông báo thua cuộc)
- `Hòa nhau` / `HÒA NHAU!` (Thông báo kết quả hòa)
- `Chơi lại` / `ĐẤU LẠI` (Nút tái đấu)
- `ĐÃ KHÓA` (Nhãn nút sau khi người chơi đã nhấn Sẵn sàng)

---

## 2. HOÀN THIỆN GAME FEEL (GAME FEEL & PACING)

### 2.1. Đầy Đủ Chu Trình Vòng Đấu
1. **Đặt vị trí (Placement):** Nhấp/chạm vào bất kỳ điểm nào trên mặt sân cát để nhân vật Nam Làng bước đến vị trí đó với hiệu ứng đánh dấu điểm đến và âm thanh đặt chân `sfx_tap_place.wav`.
2. **Ngắm 360° (Aiming):** Kéo/rê chuột hoặc vuốt cảm ứng để điều khiển thước ngắm AimKit:
   - Dải beam rẻ quạt mềm mại định hướng (Aim Cone).
   - Vệt chấm hạt quỹ đạo bay (Trajectory Dots).
   - Tâm ngắm xoay theo góc hướng (Reticle Ring).
   - Biểu tượng vũ khí Dép Tổ Ong xoay tại đầu mũi ngắm.
3. **Đếm ngược & Khóa:** Đồng hồ đếm ngược 5 giây. Trong 3 giây cuối cùng, phát âm thanh tick nhịp nhàng `sfx_countdown_tick.wav`, giây cuối cùng phát `sfx_countdown_last.wav`. Khi bấm "SẴN SÀNG", nút chuyển sang "ĐÃ KHÓA" và vòng chân chuyển sang màu vàng khóa mục tiêu.
4. **Reveal:** Đối thủ Bot (Anh Nón Lá) lộ diện đồng thời tại vị trí đã khóa kèm âm thanh `sfx_reveal.wav` và banner "REVEAL ĐỐI THỦ!".
5. **Ném Dép Xoay & Vệt Bay:** Cả hai nhân vật chuyển sang thế vung tay (`ThrowWindup`) và ném (`ThrowRelease`). Dép Tổ Ong được phóng đi đồng thời với tốc độ xoay $1080^\circ/\text{s}$ (3 vòng/giây) cùng dải đuôi sáng hạt vàng fading (`TrailRenderer`), phát âm thanh `sfx_throw.wav`.
6. **Hit Reaction & Eliminate:** Va chạm dép trúng đích kích hoạt âm thanh bop `sfx_hit_bop.wav`, mục tiêu chuyển sang thế trúng đòn (`CharacterPose.Hit`), trừ máu trái tim. Khi hết máu, phát âm thanh loại bỏ `sfx_eliminate.wav`, chuyển sang thế gục ngã (`CharacterPose.Defeat`).
7. **Kết Quả & Đấu Lại:** Bảng kết quả hiện lên sang trọng kèm âm thanh chiến thắng `sfx_win.wav` hoặc thất bại `sfx_lose.wav`, thống kê chi tiết lượt ném, trúng đích và nút "ĐẤU LẠI".

### 2.2. Kiểm Soát Pacing Theo `GameConfig` ($\le 6$ Giây)
Các tham số thời gian được đưa vào [GameConfig.cs](file:///d:/HITME%20GAME/unity-client/Assets/Scripts/Core/Phase1/GameConfig.cs) và [GameConfigAsset.cs](file:///d:/HITME%20GAME/unity-client/Assets/Scripts/Game/GameConfigAsset.cs):
- `PlaceSeconds = 5.0f` (Thời gian chọn vị trí và ngắm)
- `RevealSeconds = 0.8f` (Thời gian mở màn lộ diện đối thủ)
- `ThrowSeconds = 1.35f` (Thời gian vung tay ném và dép bay tới đích)
- `ResolveSeconds = 1.2f` (Thời gian hiển thị phản ứng trúng đòn, trừ máu và cập nhật trạng thái)
- **Tổng thời gian từ Reveal đến Kết quả vòng:** $0.8 + 1.35 + 1.2 = \mathbf{3.35\text{ giây}} \le 6.0\text{ giây}$ (ĐẠT CHỈ TIÊU).

### 2.3. Video Bằng Chứng Vòng Đấu Hoàn Chỉnh ($\ge 30$ Giây)
- **Video Browser Subagent Session WebP:** [round_gameplay_30s.webp](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/round_gameplay_30s.webp) — Video ghi nhận tự động toàn bộ quá trình tương tác thực tế trên WebGL build (Menu $\rightarrow$ Chơi với bot $\rightarrow$ Đặt vị trí $\rightarrow$ Ngắm 360° $\rightarrow$ Khóa sẵn sàng $\rightarrow$ Reveal $\rightarrow$ Ném dép xoay $\rightarrow$ Kết quả).
- **Video Gameplay Cycle Đầy Đủ 32s:** [round_gameplay_30s_cycle.webp](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/round_gameplay_30s_cycle.webp) — Trình tự chuyển động 8 pha của chu trình trận đấu hoàn chỉnh kéo dài 32 giây.

---

## 3. HỆ THỐNG ÂM THANH (AUDIO SYSTEM AUDIT)

### 3.1. Danh Mục SFX & Background Music
Toàn bộ danh mục âm thanh của Phase 2 được tích hợp đầy đủ vào [AudioManager.cs](file:///d:/HITME%20GAME/unity-client/Assets/Scripts/Audio/AudioManager.cs):
1. **Nhạc nền Menu (Loop):** `Assets/Audio/bgm_menu_loop.wav` (`PlayMenuBgm()`).
2. **Nhạc nền Trận đấu (Loop):** `Assets/Audio/bgm_match_loop.wav` (`PlayMatchBgm()`).
3. **Click nút giao diện:** `Assets/Audio/sfx_ui_click.wav` (`PlayClick()`).
4. **Chạm đặt nhân vật / di chuyển:** `Assets/Audio/sfx_tap_place.wav` (`PlayPlace()`).
5. **Đếm ngược 3 giây cuối:** `Assets/Audio/sfx_countdown_tick.wav` (`PlayTick()`).
6. **Tiếng đếm ngược giây cuối cùng:** `Assets/Audio/sfx_countdown_last.wav` (`PlayCountdownLast()`).
7. **Khóa mục tiêu / Sẵn sàng:** `Assets/Audio/sfx_ready.wav` (`PlayReady()`).
8. **Lộ diện đối thủ (Reveal):** `Assets/Audio/sfx_reveal.wav` (`PlayReveal()`).
9. **Vung tay ném (Throw):** `Assets/Audio/sfx_throw.wav` (`PlayThrow()`).
10. **SFX "Dép bay" (Slipper Flight):** Asset QA xác nhận dự án hiện chưa có audio file cho dép bay. Đã triển khai phương thức placeholder im lặng `PlayFly()` (no-op an toàn, không phát ra tiếng ồn giả mạo, sẵn sàng kết nối audio asset chính thức ở Phase 3).
11. **Dép trúng đích (Hit):** `Assets/Audio/sfx_hit_bop.wav` (`PlayHit()`).
12. **Bị loại / Gục ngã (Death):** `Assets/Audio/sfx_eliminate.wav` (`PlayDeath()`).
13. **Chiến thắng (Win):** `Assets/Audio/sfx_win.wav` (`PlayWin()`).
14. **Thất bại (Lose):** `Assets/Audio/sfx_lose.wav` (`PlayLose()`).

### 3.2. Cấu Trúc Kênh Âm Lượng & Lưu Trữ PlayerPrefs
- `AudioManager` chia thành 2 kênh độc lập: `bgmSource` (Music) và `sfxSource` (SFX).
- Hai thuộc tính điều chỉnh âm lượng độc lập: `MusicVolume` và `SfxVolume` (từ $0.0$ đến $1.0$).
- Tự động lưu và nạp trạng thái âm lượng từ `PlayerPrefs`:
  - `HitMe_MusicVolume` (Mặc định: 0.5)
  - `HitMe_SFXVolume` (Mặc định: 0.8)

---

## 4. KIỂM THỬ, HIỆU NĂNG VÀ BUILD (TESTS, PERFORMANCE & BUILD)

### 4.1. Kết Quả Chạy Toàn Bộ Unit & Integration Tests
Đã chạy toàn bộ bộ test bằng Unity Test Runner qua command line (`-runTests -testPlatform EditMode`):
- **File kết quả chi tiết:** [test-results.xml](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/test-results.xml)
- **Tổng số test cases:** 32 tests.
- **Passed:** 32 / 32 tests (**100% Green, 0 failed, 0 skipped**).
- **Các nhóm test bao gồm:**
  1. 27 test cases Phase 1 Core (Luật ném dép, tính khoảng cách, va chạm tia quét, giải quyết đồng thời, trừ máu, tỉ lệ hòa).
  2. `GameToWorld_And_WorldToGame_AreInvertible_WithinArenaBounds`: Xác thực tính khả nghịch tọa độ.
  3. `GameToWorld_ClampsOutOfBoundsCoordinates`: Xác thực kẹp biên sân đấu.
  4. `LocalizationTable_ContainsAllRequiredPhase2Keys_WithAccents`: Xác thực đầy đủ key tiếng Việt có dấu.
  5. `GameConfig_PhaseTiming_RevealToResult_UnderSixSeconds`: Xác thực pacing reveal $\rightarrow$ kết quả $\le 6.0\text{s}$.
  6. `PrefabIntegrity_NoMissingScriptsOrSpritesInPrefabs`: Quét đệ quy toàn bộ prefab đảm bảo không có script bị `Missing` hay sprite bị mất.

### 4.2. Chơi Tự Động 10 Trận Liên Tiếp (`autoplay_10matches.txt`)
- **File nhật ký bằng chứng:** [autoplay_10matches.txt](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/autoplay_10matches.txt)
- Chạy tự động 10 trận liên tiếp với 10 seed ngẫu nhiên khác nhau thông qua engine mô phỏng chi tiết:
  - 10/10 trận hoàn thành trọn vẹn từ vòng 1 đến vòng 5.
  - Tỉ lệ kết quả: Player thắng 4 trận, Bot thắng 6 trận, Hòa 0 trận.
  - Không xuất hiện bất kỳ ngoại lệ, crash hoặc deadlock nào.

### 4.3. Đo Đạc Hiệu Năng Chi Tiết (`performance_metrics.txt`)
- **File số liệu hiệu năng:** [performance_metrics.txt](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/performance_metrics.txt)
- **Thời gian nạp scene (`MainMatch.unity`):** $57\text{ ms}$ (Mục tiêu di động $< 1200\text{ ms}$: **ĐẠT XUẤT SẮC**).
- **Draw Calls & Batching:**
  - Tổng số batches UI & Sân: $12 - 18\text{ batches}$ (Mục tiêu $\le 35$: **ĐẠT**).
  - SetPass calls: $6 - 9\text{ calls}$ (Mục tiêu $\le 15$: **ĐẠT**).
  - Dynamic font SDF atlas batching hoạt động hoàn hảo.
- **Bộ nhớ đồ họa (Texture Memory):** $34.0\text{ MB}$ (Mục tiêu $\le 64\text{ MB}$: **ĐẠT**).
- **Tốc độ khung hình (Framerate):**
  - Average FPS: $60.0\text{ FPS}$ (V-Sync Capped $60\text{ FPS}$).
  - 1% Low FPS: $58.4\text{ FPS}$.
  - Frame time tối đa: $17.1\text{ ms}$, không có hiện tượng giật lag / GC spike khi điều khiển AimKit.

### 4.4. Bản Build WebGL
- **File nhật ký build:** [webgl-build.txt](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/webgl-build.txt)
- **Kết quả build:** `Target: WebGL | Result: Succeeded | Errors: 0 | Warnings: 7`
- Thư mục build phân phối: [Evidence/phase2/WebGLBuild/](file:///d:/HITME%20GAME/unity-client/Evidence/phase2/WebGLBuild/)
  - `WebGLBuild.data` (25.9 MB)
  - `WebGLBuild.wasm` (27.6 MB)
  - `WebGLBuild.framework.js` (330 KB)
  - `WebGLBuild.loader.js` (27.9 KB)

---

## 5. DANH MỤC ASSET / SFX THIẾU & ĐỀ XUẤT PHASE 3

### 5.1. Danh mục Asset / SFX Còn Thiếu
1. **SFX "Dép bay" (Slipper Flight Whoosh):** Chưa có asset chính thức từ sound designer. Hiện đang sử dụng phương thức placeholder im lặng `PlayFly()` theo đúng yêu cầu Asset QA, không dùng âm thanh giả.
2. **Spritesheet hoạt ảnh chuyển động đầy đủ của 4 nhân vật phụ:** Chú Chó Vàng, Mai Hồng, Anh Núi, Nam Làng Tint hiện mới có sprite tĩnh (idle/pose). Cần bổ sung đầy đủ bộ frames chạy/ném/gục ngã cho cả 6 nhân vật ở Phase 3.

### 5.2. Đề Xuất Mở Rộng Cho Phase 3
1. **Âm thanh:** Tích hợp bộ SFX "Dép bay" (whoosh nhẹ xoay theo vận tốc) khi nhận được bàn giao từ đội ngũ âm thanh.
2. **Animation Controller / Tweening:** Thêm độ nảy mềm (squash & stretch) khi nhân vật bị ném trúng dép và hiệu ứng bụi đất khi dép rơi xuống sân cát.
3. **Mạng & Đa người chơi (Multiplayer):** Mở rộng từ mô hình Local MatchStateMachine hiện tại sang WebSockets / Supabase Realtime cho phòng đấu 6 người chơi online.

---

## 6. KẾT LUẬN & ĐIỀU KIỆN ĐÓNG PHASE 2 (DoD SIGN-OFF)
- Toàn bộ Definition of Done (DoD) của Phase 2 đã hoàn thành 100% với đầy đủ bằng chứng kiểm thử (ảnh 3 tỉ lệ aspect ratio có Safe Area, video $\ge 30$s, 32/32 tests passed, log 10 trận tự động, số đo hiệu năng, bản build WebGL 0 lỗi).
- Sẵn sàng gắn tag `v0.2-feel` và push lên GitHub remote `origin main`.
