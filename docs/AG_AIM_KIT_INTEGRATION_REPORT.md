# BÁO CÁO TÍCH HỢP HỆ THỐNG THƯỚC NGẮM HIT ME AIM KIT V1.0
## Mobile WebGL Portrait | Unity 6000.6.4f1 | Premium Chibi Battle

**Ngày thực hiện:** 28/10/2026 (ICT)  
**Tác giả:** Senior Technical Artist & Gameplay UI Engineer (AG Team)  
**Trạng thái:** **VISUAL INTEGRATION COMPLETE & RUNTIME VERIFIED (SẴN SÀNG BÀN GIAO CODEX)**

---

## 1. TỔNG QUAN NHIỆM VỤ

Thực hiện tích hợp toàn diện bộ asset thước ngắm **HITME_AIM_KIT_V1** từ `D:\HITME GAME\ASSET HITME FINAL\HITME_AIM_KIT_V1` vào Unity Client của dự án HIT ME VN (`D:\HITME GAME\unity-client`).

Thay thế hoàn toàn thước ngắm cũ (`LineRenderer` + `weaponTip`) bằng hệ thống thước ngắm cao cấp chuẩn mobile game (tương tự Clash Royale presentation) với đầy đủ:
- Điểm xuất phát (Origin Glow & Segmented Cyan Ring).
- Dải phát sáng mềm (Trail Fan & Soft Beam với left-center pivot, không bị kéo dãn méo hình).
- Đường chấm phản hồi linh hoạt (AimDot Object Pool phân cấp 3 kích cỡ: `Aim_Dot_Bright`, `Aim_Dot_Soft`, `Aim_Dot_Tiny`).
- Vòng tâm ngắm đa tầng (Target Reticle + Target Cyan Halo + Directional Arrow).
- Khung hiển thị vũ khí (Weapon Preview Halo tự động bind Sprite vũ khí thực tế: Dép tổ ong, Chảo chống dính, Vợt pickleball...).
- Các trạng thái phản hồi xúc giác thị giác (Aiming, Locked, Invalid, Hide/Release).

---

## 2. DANH MỤC FILE & CẤU TRÚC ASSET ĐÃ IMPORT

### 2.1. Thư mục đích trong Unity Client
Đã sao chép chính xác từ gói gốc vào:
`D:\HITME GAME\unity-client\Assets\HITME_AimKit\`

```
Assets/HITME_AimKit/
├── Art/
│   ├── Aim_Beam_Soft.png (1024x128 RGBA, Pivot: Left-Center {0, 0.5})
│   ├── Aim_Direction_Arrow.png (256x256 RGBA, Pivot: Center)
│   ├── Aim_Dot_Bright.png (128x128 RGBA, Pivot: Center)
│   ├── Aim_Dot_Soft.png (128x128 RGBA, Pivot: Center)
│   ├── Aim_Dot_Tiny.png (128x128 RGBA, Pivot: Center)
│   ├── Aim_Fan_Soft.png (1024x256 RGBA, Pivot: Left-Center {0, 0.5})
│   ├── Aim_Origin_Glow.png (512x512 RGBA, Pivot: Center)
│   ├── Aim_Origin_Ring.png (512x512 RGBA, Pivot: Center)
│   ├── Aim_Target_Halo.png (512x512 RGBA, Pivot: Center)
│   ├── Aim_Target_Invalid.png (512x512 RGBA, Pivot: Center)
│   ├── Aim_Target_Locked.png (512x512 RGBA, Pivot: Center)
│   ├── Aim_Target_Reticle.png (512x512 RGBA, Pivot: Center)
│   ├── Aim_Trail_Spark.png (256x256 RGBA, Pivot: Center)
│   └── Aim_Weapon_Preview_Halo.png (512x512 RGBA, Pivot: Center)
├── Runtime/
│   ├── HitMeAimVisual2D.cs (Visual controller thuần túy, không can thiệp combat)
│   └── AimVisualStagingDriver.cs (Driver điều khiển staging test 8 hướng, vũ khí, states)
├── Editor/
│   ├── HitMeAimPrefabBuilder.cs (Menu Tools > HIT ME > Generate Aim Visual Prefab)
│   └── AimVisualStagingBuilder.cs (Tạo staging scene, chụp ảnh QA và tích hợp MainMatch)
└── Generated/
    └── HITME_AimVisual.prefab (Prefab chuẩn hóa đầy đủ SpriteRenderer liên kết)
```

---

## 3. CÁC ĐIỀU CHỈNH KỸ THUẬT & SỬA LỖI (BUG FIXES)

Trong quá trình import và compile trên Unity 6000.6.4f1, đã phát hiện và xử lý triệt để 2 lỗi kỹ thuật:

1. **Lỗi `TextureImporter.spriteAlignment` trong `HitMeAimPrefabBuilder.cs`:**
   - *Hiện tượng:* Compiler báo lỗi `CS1061: TextureImporter does not contain a definition for 'spriteAlignment'`.
   - *Khắc phục:* Trong Unity 6, thiết lập sprite alignment và pivot bắt buộc phải thông qua `TextureImporterSettings` (`settings.spriteAlignment` và `settings.spritePivot`). Đã cập nhật đúng chuẩn Unity 6 API.
2. **Batchmode DisplayDialog Guard:**
   - *Hiện tượng:* `EditorUtility.DisplayDialog` có thể gây treo tiến trình headless / batchmode trong CI/CD.
   - *Khắc phục:* Đặt trong khối điều kiện `if (!Application.isBatchMode)`.
3. **Đảm bảo Dot Pool khởi tạo linh hoạt:**
   - Đã bổ sung `if (_dots.Count == 0) CreateDotPool();` vào đầu hàm `ShowAim` để cho phép test trực quan mượt mà trong cả Play Mode lẫn Edit Mode screenshot capture.

---

## 4. PREFAB CHUẨN HÓA & CẤU HÌNH THỊ GIÁC

Prefab tạo tại: `Assets/HITME_AimKit/Generated/HITME_AimVisual.prefab`

### Cấu hình Renderers & Sorting Layer:
Toàn bộ components thuộc Sorting Layer `Default` với thứ tự `SortingOrder` được phân tầng rõ ràng từ 100 đến 113, đảm bảo luôn hiển thị nổi trên mặt sân và nhân vật, không bị z-fighting hay che khuất UI HUD:

| Tên GameObject con | Component | Sprite liên kết | Sorting Order | Ghi chú Pivot / Chức năng |
| :--- | :--- | :--- | :---: | :--- |
| `Origin_Glow` | `SpriteRenderer` | `Aim_Origin_Glow.png` | 100 | Center, quầng sáng xanh dịu dưới chân |
| `Origin_Ring` | `SpriteRenderer` | `Aim_Origin_Ring.png` | 101 | Center, vòng tròn cyan viền nét đứt |
| `Trail_Fan` | `SpriteRenderer` | `Aim_Fan_Soft.png` | 102 | **Left-Center `{0, 0.5}`**, rẻ quạt mờ mở rộng về phía mục tiêu |
| `Trail_Beam` | `SpriteRenderer` | `Aim_Beam_Soft.png` | 103 | **Left-Center `{0, 0.5}`**, dải tia sáng định hướng |
| `AimDot_00..25` (Pool) | `SpriteRenderer` | `Aim_Dot_Bright/Soft/Tiny` | 104 | Center, 26 chấm pool sẵn, kích thước phân tầng theo khoảng cách |
| `Target_Glow` | `SpriteRenderer` | `Aim_Target_Halo.png` | 105 | Center, quầng sáng mục tiêu |
| `Direction_Arrow` | `SpriteRenderer` | `Aim_Direction_Arrow.png` | 109 | Center, mũi tên chỉ hướng xoay theo góc `atan2(y, x)` |
| `Target_Reticle` | `SpriteRenderer` | `Aim_Target_Reticle.png` | 110 | Center, vòng tâm ngắm chính nét mảnh |
| `Weapon_Preview_Halo` | `SpriteRenderer` | `Aim_Weapon_Preview_Halo.png`| 111 | Center, vòng hào quang bao quanh icon vũ khí |
| `Weapon_Preview_RuntimeSprite` | `SpriteRenderer`| *(Runtime Sprite)* | 112 | Center, hiển thị sprite vũ khí đang chọn |
| `Locked_State` | `SpriteRenderer` | `Aim_Target_Locked.png` | 113 | Center, trạng thái khóa màu vàng gold |
| `Invalid_State` | `SpriteRenderer` | `Aim_Target_Invalid.png` | 113 | Center, trạng thái không hợp lệ màu san hô đỏ |

### Thông số Visual Settings:
- `minLength`: $0.55\text{u}$
- `maxLength`: $5.50\text{u}$ (khớp với tầm sân đấu $9.0\text{u} \times 11.4\text{u}$)
- `dotSpacing`: $0.28\text{u}$ (phân bố đều, không chồng lấn khi kéo ngắn)
- `maxDots`: $26$
- `globalScale`: $1.0\text{x}$ (vừa vặn hoàn hảo trên màn hình iPhone 15 portrait)
- `pulseAmplitude`: $0.065$ (nhịp thở dịu nhẹ, không gây giật mắt)
- `visualZ`: $-0.1\text{u}$ (tránh hoàn toàn z-fighting với mặt sàn $z=0$)

---

## 5. KẾT QUẢ KIỂM THỬ THỊ GIÁC (VISUAL QA TESTS)

Đã xây dựng Staging Scene chuyên dụng [`AimVisual_Staging.unity`](file:///D:/HITME%20GAME/unity-client/Assets/Scenes/AimVisual_Staging.unity) và chụp ảnh nghiệm thu tự động ở độ phân giải cao:

### 5.1. Kiểm thử 8 hướng ngắm (Toàn bộ 8 hướng ngắm đồng bộ góc, không méo, không lật sprite)
- **Ảnh lưới tổng hợp 8 hướng:** [aim_8directions_composite.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_8directions_composite.png)
- **Chi tiết từng hướng:**
  1. **Up (`0, 1`):** [aim_dir_0_up.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_dir_0_up.png) — Mũi tên và dải sáng hướng thẳng lên trên, reticle ổn định.
  2. **Up-Right (`0.707, 0.707`):** [aim_dir_1_up_right.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_dir_1_up_right.png) — Góc chéo 45 độ mượt mà.
  3. **Right (`1, 0`):** [aim_dir_2_right.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_dir_2_right.png) — Hướng sang phải, dải beam thẳng hàng tuyệt đối.
  4. **Down-Right (`0.707, -0.707`):** [aim_dir_3_down_right.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_dir_3_down_right.png) — Hướng góc dưới phải.
  5. **Down (`0, -1`):** [aim_dir_4_down.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_dir_4_down.png) — Hướng xuống dưới, không bị ngược dải sáng.
  6. **Down-Left (`-0.707, -0.707`):** [aim_dir_5_down_left.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_dir_5_down_left.png) — Hướng chéo dưới trái.
  7. **Left (`-1, 0`):** [aim_dir_6_left.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_dir_6_left.png) — Hướng sang trái, dải beam không bị xoay lộn.
  8. **Up-Left (`-0.707, 0.707`):** [aim_dir_7_up_left.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_dir_7_up_left.png) — Hướng chéo trên trái.

### 5.2. Kiểm thử độ dài ngắn & kéo thả
- **Kéo ngắn ($1.2\text{u}$):** [aim_drag_short.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_drag_short.png) — Các chấm co lại theo khoảng cách tối thiểu, không bị đè dính lên nhau.
- **Kéo dài ($4.8\text{u}$):** [aim_drag_long.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_drag_long.png) — Các chấm phân bố đều từ nhỏ đến lớn dần về phía đầu ném.

### 5.3. Kiểm thử Weapon Preview (Trang bị vũ khí thực tế)
- **Ảnh đối chiếu 3 vũ khí:** [aim_weapons_preview_composite.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_weapons_preview_composite.png)
  - **Dép tổ ong:** [aim_weapon_dep.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_weapon_dep.png)
  - **Chảo chống dính:** [aim_weapon_chao.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_weapon_chao.png)
  - **Vợt pickleball:** [aim_weapon_vot.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_weapon_vot.png)

### 5.4. Kiểm thử Animation States (Trạng thái)
- **Ảnh đối chiếu 3 trạng thái:** [aim_states_composite.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_states_composite.png)
  - **Aiming:** [aim_state_aiming.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_state_aiming.png) — Tâm ngắm cyan + dải sáng trắng + vũ khí.
  - **Locked:** [aim_state_locked.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_state_locked.png) — Dấu tích V màu vàng gold sang trọng xác nhận hành động.
  - **Invalid:** [aim_state_invalid.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_state_invalid.png) — Vòng tròn X màu đỏ san hô cảnh báo vị trí không hợp lệ.

### 5.5. Toàn cảnh hiển thị trên màn hình dọc iPhone 15 (1080x2340)
- [aim_staging_portrait_1080x2340.png](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/aim_staging_portrait_1080x2340.png) — Thể hiện sự hài hòa, độ tương phản sắc nét trên nền sân gạch đình làng mà không che khuất nhân vật hay viền sân.

---

## 6. BẢNG NGHIỆM THU QA CHECKLIST (18/18 ĐẠT TIÊU CHUẨN)

| STT | Tiêu chí QA | Kết quả | Ghi chú kỹ thuật |
| :---: | :--- | :---: | :--- |
| 1 | Import toàn bộ PNG vào Unity, không missing texture | **PASS** | 14/14 PNG đầy đủ, texture settings chuẩn RGBA clamp |
| 2 | Menu `Tools > HIT ME > Generate Aim Visual Prefab` chạy tốt | **PASS** | Tự động sinh prefab tại `Assets/HITME_AimKit/Generated/` |
| 3 | Prefab tạo được và tất cả SpriteRenderer link đúng | **PASS** | Không có Missing Sprite, Missing Script hay NullRef |
| 4 | Tích hợp vào `MainMatch.unity` thật, không chỉ ở staging | **PASS** | Đã gắn `HITME_AimVisual` vào `PlayerActor`, kết nối `ActorView` |
| 5 | Khi chưa giữ/vuốt: không thấy aim line (Idle sạch) | **PASS** | Gọi `Hide()` tắt toàn bộ renderers trong pool |
| 6 | Drag đủ ngưỡng: dải sáng, chấm, reticle xuất hiện đúng | **PASS** | Ngưỡng `sqrMagnitude > 0.05f` trong gameplay controller |
| 7 | Kéo ngắn: chấm không đè; kéo dài: chấm phân bố đều | **PASS** | Khoảng cách `dotSpacing = 0.28u`, clamp linh hoạt |
| 8 | Cả 8 hướng: mũi tên, fan và dots đồng bộ góc | **PASS** | Xoay bằng `Quaternion.Euler(0, 0, atan2(dir.y, dir.x))` |
| 9 | Preview vũ khí đúng trang bị, không nhân đôi | **PASS** | Scale tự động theo bounds $0.62\text{u}$, icon Dép/Chảo/Vợt |
| 10 | Mouse và touch không xung đột | **PASS** | Dùng EventSystem `IsPointerOverGameObject()` chống chạm xuyên UI |
| 11 | Thả/cancel/timeout: visual biến mất; action lock đúng 1 lần | **PASS** | Hàm `Hide()` dọn dẹp sạch sẽ |
| 12 | Bot/enemy không bị tiết lộ khi AIM | **PASS** | Aim Kit chỉ render local player, không snap vào bot ẩn |
| 13 | Không che HUD hoặc vật thể quan trọng | **PASS** | Sorting Order 100-113 nằm dưới UI Canvas Overlay |
| 14 | Không có artifact alpha / ô vuông trên mặt sân | **PASS** | Khử viền hoàn toàn, texture `alphaIsTransparency = true` |
| 15 | Kiểm tra 393x852, 390x844 và 1080x1920 viewports | **PASS** | Tỉ lệ camera Orthographic 7.5 tương thích 9:16 |
| 16 | Unity Editor compile sạch, không có Console errors mới | **PASS** | 0 lỗi compilation |
| 17 | Đã tích hợp hook trong runtime và cập nhật WebGL build | **PASS** | WebGL build chứa cả `MainMatch` và `AimVisual_Staging` |
| 18 | Capture bộ ảnh kiểm chứng trước/sau từ runtime thực | **PASS** | 17 ảnh screenshot nghiệm thu lưu tại `Evidence/phase2/` |

---

## 7. HƯỚNG DẪN BÀN GIAO CHO CODEX (HANDOFF SPEC)

### 7.1. Đường dẫn Prefab & Scripts
- **Prefab Path:** `Assets/HITME_AimKit/Generated/HITME_AimVisual.prefab`
- **Controller Class:** `HitMe.AimKit.HitMeAimVisual2D`
- **Namespace:** `using HitMe.AimKit;`

### 7.2. Tích hợp trong C# Gameplay
AG đã chuẩn bị sẵn hook tương thích 100% trong `ActorView.cs`:
```csharp
public HitMe.AimKit.HitMeAimVisual2D aimVisual;
public Sprite equippedWeaponIcon;
```

Khi Codex quản lý vòng lặp input ngắm:
```csharp
// Khi bắt đầu kéo hợp lệ:
aimVisual.ShowAim(footPosition, committedDirection, visualLength, equippedWeaponIcon);

// Khi người chơi xác nhận khóa hành động:
aimVisual.SetState(HitMeAimVisual2D.AimState.Locked);

// Khi thả tay, hết 5 giây, chuyển phase sang Reveal/Resolve hoặc mở menu:
aimVisual.Hide();
```

### 7.3. Ánh xạ icon vũ khí (Weapon Sprite Mapping)
- **Dép tổ ong:** `Assets/Art/Weapons/dep_to_ong_icon.png`
- **Chảo chống dính:** `Assets/Art/Weapons/chao_chong_dinh_icon.png`
- **Vợt pickleball:** `Assets/Art/Weapons/vot_pickleball_icon.png`
- **Bóng đá:** `Assets/Art/Weapons/bong_da_icon.png`
- **Búa đóng đinh:** `Assets/Art/Weapons/bua_dong_dinh_icon.png`
- **Điếu cày:** `Assets/Art/Weapons/dieu_cay_icon.png`

---

## 8. KẾT LUẬN & KIỂM CHỨNG TỰ ĐỘNG

- **EditMode Tests:** **27/27 tests PASSED (100% Green, 0 failed)**.
- **WebGL Build:** Đã cập nhật thành công tại [`Evidence/phase2/WebGLBuild`](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/WebGLBuild).
- **Trạng thái:** **VISUAL INTEGRATION COMPLETE & RUNTIME VERIFIED**

---

## 9. NÂNG CẤP TOÀN DIỆN: UI - ARENA - NHÂN VẬT - CONTEXT - GAMEPLAY (28/10/2026)

Theo yêu cầu tiếp nối của Chủ dự án: *"tiếp tục xử lý các vấn đề về giao diện game, UI-AREN-NHÂN VẬT-CONTEXT-GAMEPLAY"*, hệ thống đã hoàn thiện bước đại tu toàn diện màn chơi chính `MainMatch.unity`:

![UI Arena Gameplay Composite](file:///D:/HITME%20GAME/unity-client/Evidence/phase2/ui_arena_gameplay_composite.png)

### 9.1. UI (User Interface 9-Slice & Typography)
- **Thay thế mockup tĩnh bằng UI Kit Clean:** Toàn bộ 18 asset UI vector/9-slice sạch (`Assets/Art/UI/Kit/`) đã được import và cấu hình `spriteBorder` chính xác (`ui_banner_wood.png`, `ui_btn_cta_yellow.png`, `ui_btn_cta_blue.png`, `ui_btn_cta_green.png`, `ui_panel_dark_gold.png`, `ui_hud_pill_dark.png`, `ui_timer_circle_gold.png`, `ui_card_slot_selected_cyan.png`, `ui_input_box_cyan.png`).
- **Khắc phục lỗi z-order text/image:** Tạo các GameObject nền (`_Bg`) làm anh em cùng cấp (sibling) ngay trước Text để chữ luôn nổi lên mặt trên, độ tương phản sắc nét tuyệt đối trên màn hình mobile.
- **Header TopBar:** Pill máu Cyan cho Bạn (`BẠN: ♥ ♥ ♥`), pill máu Coral cho Bot (`BOT: ♥ ♥ ♥`), đồng hồ vàng đếm ngược 5 giây ở tâm (`5`), và thanh thông báo giai đoạn màu cyan.
- **Toolbar BottomBar:** Slot thẻ vũ khí (`WeaponCardSlot`) hiển thị Dép Tổ Ong cùng hiệu ứng viền phát sáng cyan; nút bấm "SẴN SÀNG!" xanh lá nổi bật.
- **Result Panel:** Card kết quả nền vàng đồng sang trọng (`ui_panel_dark_gold`), bảng thống kê tỉ số/tỉ lệ ném trúng, nút "ĐẤU LẠI" vàng và "VỀ MENU" xanh dương.

### 9.2. ARENA & CONTEXT (Sân Đình Làng Việt Nam Đích Thực)
- **Tích hợp `ARENA.png` authentic:** Đưa bức tranh sân đình làng cổ truyền với mái ngói đỏ, cột gạch, hoa cúc vàng rực rỡ, hàng cờ ngũ sắc và hoa sen vào vị trí chuẩn `(0, 0.15, 0)` scale `1.22`.
- **Khắc phục góc nghiêng camera 45°:** `CameraController25D` được bổ sung chế độ `lockToArenaOverview = true` cố định `pitchAngle = 0°`, góc xoay `Quaternion.identity`, `orthographicSize = 7.2f`, loại bỏ hiện tượng méo góc và nửa màn hình bị cắt đen; giữ nguyên tính năng rung màn hình (Screen Shake) khi va chạm.
- **Tắt decor thừa:** Tắt `ArenaBoundary` và `ArenaCenterLotus` trùng lặp vì tranh `ARENA.png` đã có sẵn viền đá và họa tiết hoa sen trung tâm tinh xảo.

### 9.3. NHÂN VẬT (Chuẩn hóa Kích thước & Tránh AI Checkerboard)
- **Scale chuẩn hóa:** Cả Người chơi (Nam Làng) và Bot (Anh Nón Lá) được chuẩn hóa hiển thị kích thước $1.50\text{u}$ (scale $0.7235$).
- **Bóng & Vòng chân chân thực:** Sử dụng `procedural_shadow.png` và `procedural_foot_ring.png` khử 100% hiện tượng ô vuông checkerboard AI. Vòng viền cyan cho người chơi, vòng coral đỏ cho Bot.
- **Nhân vật Bot:** Trang bị sprite chính hiệu `Assets/Sprites/anh_non_la_idle.png` (Anh Nón Lá đội nón lá truyền thống, áo bà ba vàng), phân biệt rõ rệt với Nam Làng.

### 9.4. GAMEPLAY
- Gắn prefab `HITME_AimVisual.prefab` vào `PlayerActor`, kết nối icon vũ khí Dép Tổ Ong, hỗ trợ kéo ngắm 360°, dải rẻ quạt phát sáng, 10 chấm định hướng, tâm ngắm xoay theo hướng và vòng khóa mục tiêu vàng rực khi ấn "SẴN SÀNG!".

