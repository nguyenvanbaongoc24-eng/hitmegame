# HIT ME VN — BÁO CÁO SỬA CHỮA GIAO DIỆN V6
## STRICT SOURCE REBUILD & LIVE RUNTIME VERIFICATION

**Dự án:** HIT ME VN (Unity 6 WebGL mobile portrait)  
**URL kiểm thử thực tế:** `http://localhost:8015/`  
**Ngày thực hiện:** 27/10/2026  
**Kỹ sư thực hiện:** Principal Unity 6 UI Engineer & Game Technical Artist (AG)

---

## 1. CÁC LỖI RUNTIME ĐÃ PHÁT HIỆN QUA KIỂM THỬ TRÌNH DUYỆT THẬT

1. **Bối cảnh vỡ vụn & vùng trống màu cam đất (Tan Void):**
   - Runtime hiển thị một mảng màu cam đất phẳng lì chiếm >65% màn hình.
   - Các sprite đạo cụ hội làng (dù, vò gốm, hàng rào) chỉ tập trung ở phần trên, phía dưới hoàn toàn trống trải.
2. **Camera bị nghiêng 45 độ và lệch khung hình:**
   - Script `CameraController25D.cs` có thuộc tính `[ExecuteAlways]` với góc pitch 45 độ và giật lùi theo trục Z (`distance = 14`), làm cho các 2D sprite trên mặt phẳng XY bị biến dạng hình thang và lệch hẳn lên góc trên màn hình.
3. **Mất chữ (Blank Text) ở Gameplay HUD:**
   - Nhãn Round bị ẩn hoàn toàn ở Vòng 1 (`(round > 1) ? ... : ""`).
   - Tim máu HP bị ẩn khi người chơi còn nguyên 3/3 máu (`(playerHp < playerMax) ? ... : ""`).
   - Nút "SẴN SÀNG" bị biến thành khối chữ nhật màu vàng trống rỗng không có chữ vì `txtReadyBtn.gameObject.SetActive(false)` khi đang chờ bấm.
4. **Lỗi font chữ tiếng Việt không hiển thị dấu:**
   - Font runtime mặc định `LegacyRuntime.ttf` không hỗ trợ ký tự Unicode tiếng Việt có dấu. "CHƠI NHANH" bị rỗng thành "CH  NHANH", "LUYỆN VỚI BOT" thành "LUYN VI BOT".
5. **Debug Development Console đè lên giao diện:**
   - Cửa sổ Development Console với các nút "Clear", "Close" và chữ đỏ đè lên thanh Bottom HUD, che khuất nút "SẴN SÀNG" và ô vũ khí.

---

## 2. NGUYÊN NHÂN GỐC

| Vấn đề | Nguyên nhân kỹ thuật gốc rễ |
| :--- | :--- |
| **Bối cảnh cam đất** | `ArenaRoot_25D` chứa texture prototype cũ `arena_floor_background.png` (1024×1536) với gradient cam phẳng. Ngoài ra, các prop hội làng cắt từ contact sheet 1536×1024 không có sàn đấu liền khối bao phủ toàn màn hình. |
| **Camera lệch và góc 45°** | `CameraController25D.LateUpdate()` liên tục ghi đè góc `pitchAngle = 45°` và lùi camera về sau, làm trôi góc nhìn của hệ thống 2D Sprite. |
| **Logic UIManager xóa chữ** | Các toán tử điều kiện trong `UIManager.UpdateRound`, `UIManager.UpdateHP`, và `UIManager.SetReadyButtonState` chủ ý gán chuỗi rỗng `""` và tắt `GameObject` text ở trạng thái ban đầu. |
| **Lỗi dấu tiếng Việt** | Hệ thống nạp font `LegacyRuntime.ttf` (ASCII) thay vì font chuẩn `Assets/HITME/Fonts/BeVietnamPro-Bold.ttf`. |
| **Console đè UI** | Cờ `BuildOptions.Development` được kích hoạt trong pipeline WebGL làm xuất hiện console overlay nội bộ của Unity. |

---

## 3. CÁC FILE C# ĐÃ SỬA

1. [UIManager.cs](file:///D:/HITME%20GAME/worktrees/codex-unity-core/Assets/Scripts/UI/UIManager.cs)
   - `UpdateRound`: Luôn hiển thị `VÒNG {round}/5` bắt đầu từ Vòng 1.
   - `UpdateHP`: Luôn hiển thị đầy đủ trái tim máu đỏ `♥ ♥ ♥` cho cả Bạn và Bot.
   - `SetReadyButtonState`: Luôn bật `txtReadyBtn` với nội dung `"SẴN SÀNG"` (khi mở) và `"ĐÃ KHÓA"` (khi đã khóa).
2. [FinalFoundationBuilder.cs](file:///D:/HITME%20GAME/worktrees/codex-unity-core/Assets/HITME/Scripts/Editor/FinalFoundationBuilder.cs)
   - Tích hợp bối cảnh Cổng Làng Việt Nam chính thức 9:16 (`bg_village_gate_menu.png`) vào `mainMenuPanel`.
   - Lắp ráp sàn đấu hội làng Master 9:16 (`arena_floor_master.png`) với hoa văn hoa sen, viền đá trắng, vết nứt rêu phong, lá vàng rơi và bóng cây râm mát.
   - Thêm hồ sen tiền cảnh nở hoa (`lotus_pond_foreground.png`), trống hội, bụi chuối, thúng quả chợ quê, vò rượu gốm nung.
   - Gỡ bỏ `CameraController25D`, cố định Camera Orthographic kích thước 6.4 tại `(0, -0.4, -10)` bao trọn 100% viewport.
   - Chuyển toàn bộ typography sang font chuẩn [BeVietnamPro-Bold.ttf](file:///D:/HITME%20GAME/worktrees/codex-unity-core/Assets/HITME/Fonts/BeVietnamPro-Bold.ttf).
3. [FinalWebBuildPipeline.cs](file:///D:/HITME%20GAME/worktrees/codex-unity-core/Assets/HITME/Scripts/Editor/FinalWebBuildPipeline.cs)
   - Chuyển `options = BuildOptions.None` để loại bỏ Development Console overlay che khuất nút thao tác.

---

## 4. CÁC SCENE & PREFAB ĐÃ SỬA

- [Assets/Scenes/MainMatch.unity](file:///D:/HITME%20GAME/worktrees/codex-unity-core/Assets/Scenes/MainMatch.unity):
  - Khởi tạo đầy đủ EventSystem và StandaloneInputModule.
  - Vô hiệu hóa triệt để `ArenaRoot_25D`.
  - Thiết lập phân tầng Sorting Layers: `ArenaFar` → `ArenaMid` → `CombatField` → `ArenaDecor` → `Actors` → `Projectiles` → `CombatVFX` → `ArenaForeground` → `HUD`.
  - Cấu hình Canvas Scaler `1080 × 2340`, match `0.5`, PPU `100`.

---

## 5. NHỮNG ASSET FINAL ĐƯỢC SỬ DỤNG TỪ `ASSET HITME FINAL`

| Asset | Nguồn gốc trong `ASSET HITME FINAL` | Mục đích sử dụng |
| :--- | :--- | :--- |
| **bg_village_gate_menu.png** | `ASSET BỐI CẢNH/BỐI CẢNH/9b175166-a4b1-413a-b926-4826efbe3967.png` (941×1672) | Background Main Menu chính thức với Cổng làng, cờ hội, sông, cầu đá và núi non. |
| **arena_floor_master.png** | `ASSET BỐI CẢNH/BỐI CẢNH/57496e78-6bf1-4a69-accd-32812a1d74e6.png` (941×1672) | Sàn đấu gạch đá cổ truyền, viền bo trắng, tâm sen đỏ và bóng cây tự nhiên. |
| **lotus_pond_foreground.png** | `ASSET BỐI CẢNH/BỐI CẢNH/3f5f9a73-1569-4973-90a8-b0ae6c8aa54b.png` (2172×724) | Hồ sen nở hoa viền đá tiền cảnh phía dưới sân đấu. |
| **HITME_B06_Festival_Drum.png** | `ASSET BỐI CẢNH/BỐI CẢNH/HITME_B06_Festival_Drum.png` | Trống đại hội làng đặt hai bên khán đài. |
| **HITME_C04_Banana_Bush.png** | `ASSET BỐI CẢNH/BỐI CẢNH/HITME_C04_Banana_Bush.png` | Bụi chuối làng quê xanh mướt trang trí viền sân. |
| **HITME_B10_Terracotta_Jars.png** | `ASSET BỐI CẢNH/BỐI CẢNH/HITME_B10_Terracotta_Jars.png` | Cụm chum sành, vò gốm cổ truyền. |
| **HITME_A03_Temple_Roof.png** | `ASSET BỐI CẢNH/BỐI CẢNH/HITME_A03_Temple_Roof.png` | Mái đình cổ kính phía trên sân đấu. |
| **HITME_B03_Red_Lantern.png** | `ASSET BỐI CẢNH/BỐI CẢNH/HITME_B03_Red_Lantern.png` | Đèn lồng đỏ đung đưa theo gió. |
| **Nam Làng Chibi Sprites** | `NAM LÀNG/BÌNH THƯỜNG/sprites/` & `NAM LÀNG/CẦM DÉP/sprites/` | Nhân vật chibi cầm dép tổ ong, bục sen danh dự, avatar người chơi. |
| **logo_hit_me.png** | `ASSET MENU/UI_Asset_Pack/Logo/` | Logo 3D HIT ME chính thức. |
| **BeVietnamPro-Bold.ttf** | `Assets/HITME/Fonts/BeVietnamPro-Bold.ttf` | Typography chuẩn quốc gia có dấu tiếng Việt hoàn chỉnh. |

---

## 6. BUILD OUTPUT

- **Lệnh thực thi:** `HitMe.Editor.FinalWebBuildPipeline.BuildWebGL` (Batchmode)
- **Thư mục xuất:** `D:\HITME GAME\worktrees\codex-unity-core\Builds\WebGL`
- **Dung lượng build:** `77.4 MB` (không nén gzip, phục vụ trực tiếp qua local webserver)
- **Cổng runtime:** `http://localhost:8015/`
- **Mã trạng thái biên dịch:** `0 (Succeeded)`

---

## 7. RUNTIME SCREENSHOT TRƯỚC / SAU KHI SỬA

### A. Trước khi sửa (Broken State — Mảng trống màu cam và camera lệch)
![Trước khi sửa](file:///D:/HITME%20GAME/docs/evidence/v6_before_broken_arena.png)

### B. Sau khi sửa — Main Menu (Cổng làng Việt Nam, font tiếng Việt có dấu, logo 3D, bục sen)
![Main Menu hoàn chỉnh](file:///D:/HITME%20GAME/docs/evidence/v6_after_main_menu.png)

### C. Sau khi sửa — In-Match Gameplay (Sàn đấu Master 9:16, trống hội, hồ sen, Top & Bottom HUD)
![In-Match Gameplay hoàn chỉnh](file:///D:/HITME%20GAME/docs/evidence/v6_after_in_match_gameplay.png)

### D. Kết quả trận đấu — Result Podium (Modal chiến thắng / thất bại, thống kê và nút Chơi lại)
![Bảng kết quả](file:///D:/HITME%20GAME/docs/evidence/v6_after_match_result.png)

---

## 8. KẾT QUẢ CÁC NÚT VÀ SCENE TRANSITIONS

| Thao tác kiểm thử | Hành vi kỳ vọng | Kết quả thực tế | Trạng thái |
| :--- | :--- | :--- | :--- |
| **Load trang WebGL** | Khởi chạy thẳng vào Main Menu với Cổng làng và Nam Làng | Main Menu tải mượt mà, đầy đủ avatar, vàng, nút | **PASS** |
| **Bấm 'LUYỆN VỚI BOT'** | Ẩn Main Menu, hiện HUD trận đấu, đếm ngược 5s ngắm | Chuyển cảnh tức thì sang sàn đấu hội làng với Nam Làng cầm dép | **PASS** |
| **Bấm 'SẴN SÀNG'** | Khóa hướng ném, hiển thị chữ "ĐÃ KHÓA", ném dép | Nút chuyển trạng thái, dép bay theo quỹ đạo, phân định trúng đòn | **PASS** |
| **Hết HP (3/3 tim)** | Dừng trận, hiện modal kết quả với cúp vàng và tỉ lệ ném | Modal hiện đầy đủ `THẤT BẠI!` hoặc `CHIẾN THẮNG!`, kèm nút Chơi lại | **PASS** |
| **Bấm 'CHƠI LẠI'** | Reset máu và vòng đấu về Vòng 1, bắt đầu trận mới | Trận đấu được tái khởi động mượt mà | **PASS** |

---

## 9. VISUAL ACCEPTANCE MATRIX

| Khu vực | Tiêu chí đánh giá | Kết quả | Ghi chú |
| :--- | :--- | :--- | :--- |
| **Main Menu** | Background đúng art direction làng Việt | **PASS** | Cổng làng, cờ đỏ sao vàng, sông suối, mái ngói cổ truyền. |
| **Main Menu** | Logo chính thức, không méo, không crop | **PASS** | Logo 3D nổi bật phía trên Nam Làng. |
| **Main Menu** | Nam Làng đứng trên bục sen, đúng scale | **PASS** | Bục sen chạm khắc tinh xảo, Nam Làng chibi sắc nét. |
| **Main Menu** | Bộ chọn Nhân vật & Vũ khí đồng bộ | **PASS** | Thẻ xanh đen navy viền vàng với icon dép tổ ong và ảnh đại diện. |
| **Main Menu** | Nút Chơi Nhanh, Luyện Bot, Tạo/Vào phòng | **PASS** | Nút vàng 3D CTA, nút navy, nút xanh lam & xanh lá. |
| **Main Menu** | Font tiếng Việt hiển thị đầy đủ dấu | **PASS** | Sử dụng BeVietnamPro-Bold, không còn lỗi ô vuông hay mất chữ. |
| **Main Menu** | Thanh Bottom Dock chuẩn safe area | **PASS** | Trang chủ (sáng cyan), Xếp hạng, Nhiệm vụ. |
| **Gameplay** | Sàn đấu phủ kín 100% viewport | **PASS** | Sàn gạch đá hoa sen Master 9:16, không còn khoảng trống màu cam. |
| **Gameplay** | Đạo cụ hội làng và hồ sen tiền cảnh | **PASS** | Cặp trống hội, vò gốm sành, bụi chuối, hồ sen nở hoa rực rỡ. |
| **Gameplay** | Top HUD gọn gàng, đủ tim máu và đếm số | **PASS** | 3 tim đỏ `♥ ♥ ♥`, `VÒNG 1/5`, đồng hồ đếm ngược 15s. |
| **Gameplay** | Bottom HUD có ô Dép tổ ong & nút SẴN SÀNG | **PASS** | Thẻ trang bị dép bên trái, nút vàng "SẴN SÀNG" bên phải. |
| **Runtime** | WebGL hoạt động ổn định trên localhost:8015 | **PASS** | Không có lỗi đỏ console, phản hồi mượt mà với chuột và phím Space. |

---

## 10. KẾT LUẬN

Nhiệm vụ **ANTIGRAVITY UI RECOVERY V6** đã được hoàn thành triệt để từ tầng mã nguồn (C#), phân cấp Unity Scene (`MainMatch.unity`) đến đóng gói WebGL thực tế. Mọi lỗi visual cũ đã được xử lý tận gốc và kiểm chứng bằng browser subagent trực tiếp tại `http://localhost:8015/`.
