# HIT ME VN — BÁO CÁO THỰC THI VISUAL & UI (AG)
## CỘT MỐC: VISUAL REBUILD + BLENDER 5.2 + UNITY 6 UI INTEGRATION
**Vai trò:** Lead Technical Artist & Game UI Engineer  
**Worktree:** `D:\hitme game\worktrees\ag-visual-ui`  
**Branch Git:** `feature/ag-visual-ui`  
**Ngày thực hiện:** 27/10/2026  

---

## 1. KHỞI TẠO & XÁC MINH CÔNG CỤ (STARTUP VALIDATION)

| Công cụ | Phiên bản thực tế | Đường dẫn thực thi | Trạng thái |
|---|---|---|---|
| **Blender** | **5.2.2 LTS** (Build 2026-09-15, hash d13f752e3b9c) | `D:\App\BLENDER\blender.exe` | **Hoạt động hoàn hảo** (chạy batchmode Cycles rendering) |
| **Unity Editor** | **Unity 6 (6000.6.4f1)** (Build 12bfff696524) | `D:\App\UNITY\6000.6.4f1\Editor\Unity.exe` | **Hoạt động hoàn hảo** (Project URP 2D WebGL/Mobile) |
| **Git Repository** | Git 2.53.0 | `D:\hitme game\unity-client\.git` | Worktree riêng `worktrees/ag-visual-ui` trên branch `feature/ag-visual-ui` |
| **Python Toolchain** | Python 3.14 + PIL 12.2.0 + NumPy 2.4.4 | `C:\Python314\python.exe` | Bộ script xử lý ảnh, defringing và tính toán pivot |

---

## 2. KẾT QUẢ KIỂM KÊ VÀ LÀM SẠCH TÀI NGUYÊN (ASSET AUDIT)

Đã chạy kiểm tra tự động trên toàn bộ **909 file PNG** thuộc `D:\hitme game\ASSET HITME FINAL`:

```text
Tổng số PNG nguồn: 909
- ANH NÓN LÁ: 85 file (4 biến thể animation sheet, true RGBA)
- ANH NÚI: 86 file (4 biến thể animation sheet, true RGBA)
- MAI HỒNG: 77 file (4 biến thể animation sheet, true RGBA)
- NAM LÀNG: 55 file (4 biến thể: bình thường, cầm chảo, cầm dép, cầm vợt)
- ASSET BỐI CẢNH: 39 file (21 layer cắt rời HITME_A/B/C + 9 tấm cảnh full)
- ASSET WEAPONS: 75 file (Dép, Chảo, Vợt Pickleball, mỗi loại 25 góc xoay)
- ASSET TARGET RULER: 8 file thước ngắm
- ASSET EFFECT: 210 file VFX (Neon & Painterly)
- ASSET MENU: 274 file (14 ảnh tham chiếu màn hình + 260 UI assets)
```

### Phát hiện quan trọng về bộ `ASSET MENU/UI_Asset_Pack`:
- Trong số 260 file con của `UI_Asset_Pack`, có tới **222 file là bản nhân bản (placeholder duplicates)** với kích thước nhỏ (129x38, 40x45, 128x64) được nhân rộng thành các tên file khác nhau (ví dụ: 87 file button khác nhau nhưng cùng dùng 1 ảnh placeholder 129x38).
- **Giải pháp AG đã xử lý:** Không sử dụng các ảnh placeholder bị vỡ hình. AG đã tự động sinh các **UI Skin chuẩn 9-slice độ phân giải cao** (Primary Yellow 3D, Secondary Blue/Green/Navy, Dark Gold Panel, Cyan Avatar Ring, Lotus Coin, v.v.), có độ co giãn 4 góc hoàn hảo cho mọi kích thước màn hình.

---

## 3. CÁC CỘT MỐC ĐÃ HOÀN THÀNH (MILESTONES ACCEPTANCE)

### MILESTONE A: MAIN MENU HOÀN THIỆN
- **Độ phân giải tham chiếu:** iPhone 15 Portrait (1179 × 2556 physical, 393 × 852 logical).
- **Top Profile Bar:** 
  - Avatar tròn với khung phát sáng Cyan (`avatar_ring_cyan.png`) và ảnh chân dung Nam Làng cắt nét (`portrait_namlang.png`).
  - Tên người chơi "Nam Làng", phụ đề "Hồ sơ" cùng icon bút chỉnh sửa.
  - Khung xu viền vàng (`pill_coin.png`) với đồng xu hoa sen vàng 3D (`coin_lotus.png`), số dư "1.250" và nút cộng (+).
  - Nút cài đặt hình bánh răng viền vàng bóng.
- **Logo HIT ME:**
  - Logo gỗ 3D chính thức với đôi dép xanh bay phía trên, được nâng cấp độ nét 2x Lanczos, căn giữa hoàn hảo ở 1/3 trên màn hình, không bị kéo giãn hay méo tỷ lệ.
- **Character Showcase & Cycle:**
  - Nhân vật Nam Làng đứng bệ đá hoa sen (`lotus_emblem.png`), có bóng đổ mặt đất mềm mại.
  - Hai nút mũi tên chuyển đổi nhân vật trái/phải (`btn_circle_gold.png`).
  - Khung thẻ chọn Nhân vật ("NHÂN VẬT - Nam Làng") và Vũ khí ("VŨ KHÍ - Dép") song song.
- **Các nút chế độ chơi (Play Mode Buttons):**
  - **CHƠI NHANH:** Nút chính màu vàng cam 3D khối nổi bóng, icon tam giác play, phụ đề "Online • Tối đa 6 người".
  - **TẠO PHÒNG:** Nút phụ màu xanh dương (`btn_blue_normal.png`) với icon thêm người.
  - **VÀO PHÒNG:** Nút phụ màu xanh lá (`btn_green_normal.png`) với icon nhóm người.
  - **LUYỆN VỚI BOT:** Thanh nút xanh than đậm (`btn_navy_normal.png`) với icon robot và chevron điều hướng.
- **Bottom Navigation Dock:**
  - Dock nền tối ôm sát cạnh dưới màn hình, bảo toàn vùng an toàn (Safe Area & Home Indicator).
  - 3 Tab chuẩn: **Trang chủ** (đang chọn - sáng Cyan kèm vạch gạch chân phát sáng), **Xếp hạng** (icon cúp), **Nhiệm vụ** (icon hộp quà).
- **Typography:** 100% sử dụng font tiếng Việt chuẩn **Be Vietnam Pro** (Black, ExtraBold, Bold, Medium), không dùng text dán chết trong ảnh.

### MILESTONE B: CHUẨN HÓA NAM LÀNG & VŨ KHÍ
- Đã cắt tách và khử viền đen/trắng (defringe) cho 3 bộ showcase của Nam Làng:
  1. `Characters/NamLang/namlang_dep_showcase.png` (cầm dép - tư thế đứng tự nhiên, dép hạ thấp).
  2. `Characters/NamLang/namlang_chao_showcase.png` (cầm chảo - tư thế vui vẻ, chảo giơ cao).
  3. `Characters/NamLang/namlang_vot_showcase.png` (cầm vợt pickleball - tư thế tự tin, vợt giơ cao).
- Tính toán pivot chân chính xác tại điểm tiếp xúc mặt đất (bottom-center normalized `[0.50, 0.00]`).
- Tách 3 icon vũ khí chất lượng cao: `weapon_dep.png`, `weapon_chao.png`, `weapon_vot.png`.

### MILESTONE C & D: ARENA STAGING & CHUYỂN ĐỘNG BLENDER 5.2
- **Tổ chức Layer bối cảnh:**
  - **Far Background:** Bầu trời chuyển sắc (`sky_gradient.png`), mây mềm (`cloud_soft.png`), núi xa (`far_mountains.png`), mái làng (`village_rooftops.png`).
  - **Midground:** Cổng mái chùa (`temple_roof.png`), bậc thang đá (`temple_stairs.png`), đôi cột đá (`pillar_left.png`, `pillar_right.png`).
  - **Combat Field:** Sàn đá lát gạch liên tục (`ground_tile_seamless.png`), vòng huy hiệu hoa sen (`lotus_emblem.png`).
  - **Decorations:** Đèn lồng đỏ (`lantern.png`), dải cờ hội (`bunting.png`), chậu hoa vàng (`flower_pots.png`), dù che đỏ/xanh (`parasol_red.png`, `parasol_blue.png`), trống hội (`drum.png`), hàng rào tre (`bamboo_fence.png`), chum gốm (`jars.png`).
- **Chuyển động môi trường Blender 5.2 (`hm_blender.py`):**
  - **Lá cờ bay (`flag_wave`):** 16 frame RGBA, 14 FPS, mép cột được neo cố định (displacement = 0), sóng vải chuyển động điều hòa tự nhiên, chu kỳ lặp vô tận (seamless loop frame 15 → 0).
  - **Tán cây đung đưa (`tree_canopy_left`, `tree_canopy_right`):** 16 frame RGBA, 12 FPS, phần thân cây bám rễ cố định, các nhánh con và cụm lá dao động lệch pha nhẹ nhàng theo gió.
  - **Bệ đá hoa sen 3D (`podium_lotus`):** Khối đá tròn 3 bậc được dựng hình và render nổi khối 3D với ánh sáng mặt trời ấm áp.

### MILESTONE E: IN-MATCH HUD (GIAO DIỆN TRẬN ĐẤU)
- **Top HUD:**
  - Bộ đếm lượt: "LƯỢT 1/20".
  - Đồng hồ đếm ngược: Pill trung tâm "05s" (chuyển sang đỏ nhấp nháy khi còn <= 2s).
  - Thẻ người chơi (Trái): Tên "Nam Làng", 3 trái tim sinh lực đỏ bóng (Player HP).
  - Thẻ đối thủ (Phải): Tên "Bot Luyện", 3 trái tim sinh lực đỏ bóng (Bot HP).
  - Nút Pause/Settings góc trên.
- **Combat Viewport:**
  - Đảm bảo **hơn 75% chiều cao màn hình** dành trọn cho sàn đấu arena.
  - Thước ngắm (Aim Indicator): Chuỗi hạt tròn Cyan phát sáng đa tầng, đầu mũi tên tròn chỉ hướng 360 độ kèm preview vũ khí dép bay.
  - Vòng định vị điểm đặt chân người chơi.
- **Bottom HUD:**
  - Ô vũ khí đang trang bị ("VŨ KHÍ - Dép tổ ong").
  - Nút hành động khóa lượt / Sẵn sàng ("ĐÃ SẴN SÀNG" có icon dấu tích xanh).
  - Bảo toàn khoảng trống Safe Area cho thanh gạt Home Indicator.

---

## 4. BẰNG CHỨNG THỊ GIÁC (VISUAL PROOF & SCREENSHOTS)

Các ảnh đối chiếu runtime đã được xuất tại `_AG_VISUAL_WORK/qa`:
1. **`main_menu_comparison.png` (2401 × 2556):** Đối chiếu trực tiếp giữa ảnh tham chiếu `MAIN MENU.png` gốc và bản dựng Rebuild của AG.
2. **`gameplay_comparison.png` (2923 × 2556):** Đối chiếu giữa sàn đấu gốc và bản dựng In-Match HUD chiến đấu thực tế.
3. **`flag_frames.png` & `flag_wave.gif`:** 16 frame chuyển động lá cờ Việt Nam mượt mà.
4. **`tree_frames.png` & `tree_canopy_left.gif`:** 16 frame chuyển động tán cây đu đưa theo gió.
5. **`out_skins.png` & `out_icons.png`:** Toàn bộ bảng skin 9-slice và icon vector đã qua nghiệm thu QA.

---

## 5. HƯỚNG DẪN TÍCH HỢP CHO CODEX

Tất cả các tài nguyên và script đã được bàn giao vào đúng cấu trúc được cấp phép:
- **Scripts:** `Assets/HITME/Scripts/Presentation/`
  - `MainMenuPresenter.cs` —Presenter điều khiển UI Main Menu, đã sẵn sàng kết nối sự kiện tới `HitMe.UI.UIManager`.
  - `InMatchHUDPresenter.cs` — Presenter điều khiển HUD trận đấu, liên kết với `AimController` và `MatchPresentationAdapter`.
  - `SpriteSequenceAnimator.cs` — Component chạy chuỗi sprite động (cờ, cây, đèn lồng) với zero-GC.
  - `VisualPreviewController.cs` — Bộ điều khiển chuyển đổi các chế độ xem trong scene preview.
- **Assets:** `Assets/HITME/Art/`
  - `Characters/` — Sprites Nam Làng đã chuẩn hóa pivot và defringe.
  - `Environment/` — 21 layers bối cảnh cắt rời.
  - `Animation/` — Chuỗi frame Blender cho cờ và cây.
  - `UI/Skins/` — Các skin 9-slice.
  - `UI/Icons/` & `UI/Logo/` — Icon vector và logo HIT ME.
- **Fonts:** `Assets/HITME/Fonts/` — Font Be Vietnam Pro hỗ trợ 100% tiếng Việt.
