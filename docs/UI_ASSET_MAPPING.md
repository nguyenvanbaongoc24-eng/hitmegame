# HIT ME VN — BẢNG ÁNH XẠ TÀI NGUYÊN GIAO DIỆN (UI ASSET MAPPING)
## Tài liệu Kỹ thuật Tích hợp UI/UX cho Lead Technical Artist & Game Engineer
**Worktree:** `D:\hitme game\worktrees\ag-visual-ui`  
**Nhánh Git:** `feature/ag-visual-ui`  
**Tiêu chuẩn thiết bị:** iPhone 15 Portrait (1179 × 2556 physical, 393 × 852 logical @ 3x)  
**Hệ thống phân cấp tham chiếu:** Clash Royale Mobile Portrait UI Structure

---

## 1. THIẾT LẬP CANVAS SCALER & SAFE AREA

| Thuộc tính Unity Canvas | Giá trị cấu hình chuẩn | Giải thích kỹ thuật |
|---|---|---|
| **UI Scale Mode** | `Scale With Screen Size` | Đảm bảo kích thước UI nhất quán trên mọi tỉ lệ màn hình mobile dọc |
| **Reference Resolution** | `X = 1179, Y = 2556` | Khung hình chuẩn iPhone 15 Super Retina XDR (19.5:9) |
| **Screen Match Mode** | `Match Width Or Height` | |
| **Match** | `0.5` (Cân bằng rộng/cao) | Giữ bố cục cân đối giữa màn hình 16:9, 19.5:9 và 20:9 |
| **Reference Pixels Per Unit** | `100` | Tỷ lệ chuẩn cho Sprite Renderer và UI Image |
| **Safe Area Inset (Top)** | `132 px` (44pt) | Chừa khoảng trống cho Dynamic Island / Notch |
| **Safe Area Inset (Bottom)** | `102 px` (34pt) | Chừa khoảng trống cho thanh điều hướng vuốt Home Indicator |

---

## 2. BẢNG ÁNH XẠ UI SKIN 9-SLICE (`Assets/HITME/Art/UI/Skins/`)

Tất cả các skin 9-slice đã được tạo lập với độ phân giải cao, hỗ trợ kéo giãn không vỡ góc:

| Tên File Sprite | Kích thước (px) | 9-Slice Border (L, B, R, T) | Mục đích sử dụng | Component liên kết |
|---|---|---|---|---|
| `btn_primary_yellow_normal.png` | 320 × 110 | `36, 36, 36, 36` | Nút bấm chính "CHƠI NHANH" (vàng 3D viền nổi) | `MainMenuPresenter._quickPlayButton` |
| `btn_primary_yellow_pressed.png` | 320 × 110 | `36, 36, 36, 36` | Trạng thái nhấn của nút chính | SpriteState Pressed |
| `btn_primary_yellow_disabled.png` | 320 × 110 | `36, 36, 36, 36` | Trạng thái vô hiệu hóa của nút chính | SpriteState Disabled |
| `btn_blue_normal.png` | 260 × 90 | `28, 28, 28, 28` | Nút phụ "TẠO PHÒNG" (xanh dương sáng) | `MainMenuPresenter._createRoomButton` |
| `btn_blue_pressed.png` | 260 × 90 | `28, 28, 28, 28` | Trạng thái nhấn nút tạo phòng | SpriteState Pressed |
| `btn_green_normal.png` | 260 × 90 | `28, 28, 28, 28` | Nút phụ "VÀO PHÒNG" (xanh lục ngọc) | `MainMenuPresenter._joinRoomButton` |
| `btn_green_pressed.png` | 260 × 90 | `28, 28, 28, 28` | Trạng thái nhấn nút vào phòng | SpriteState Pressed |
| `btn_navy_normal.png` | 520 × 85 | `24, 24, 24, 24` | Thanh nút "LUYỆN VỚI BOT" (xanh than sẫm) | `MainMenuPresenter._trainWithBotButton` |
| `btn_navy_pressed.png` | 520 × 85 | `24, 24, 24, 24` | Trạng thái nhấn nút luyện bot | SpriteState Pressed |
| `btn_circle_gold.png` | 100 × 100 | `48, 48, 48, 48` | Nút tròn viền vàng (chuyển nhân vật, cài đặt) | `_prevCharacterButton`, `_nextCharacterButton`, `_settingsButton` |
| `btn_circle_gold_pressed.png` | 100 × 100 | `48, 48, 48, 48` | Trạng thái nhấn nút tròn vàng | SpriteState Pressed |
| `btn_circle_navy.png` | 100 × 100 | `48, 48, 48, 48` | Nút tròn xanh than (chat nhanh, emote trong trận) | `InMatchHUDPresenter._quickChatButton` |
| `btn_circle_navy_pressed.png` | 100 × 100 | `48, 48, 48, 48` | Trạng thái nhấn nút tròn navy | SpriteState Pressed |
| `btn_circle_plus.png` | 52 × 52 | `24, 24, 24, 24` | Nút tròn cộng xu (+) màu xanh ngọc | `MainMenuPresenter._addCoinButton` |
| `panel_card_dark_gold.png` | 240 × 140 | `32, 32, 32, 32` | Khung thẻ chọn Nhân vật & Vũ khí | `_characterCardButton`, `_weaponCardButton` |
| `panel_card_dark_selected.png` | 240 × 140 | `32, 32, 32, 32` | Khung thẻ trạng thái đã chọn (viền Cyan sáng) | Highlight State |
| `panel_profile_strip.png` | 360 × 80 | `28, 28, 28, 28` | Khung nền hiển thị Tên & Danh hiệu người chơi | `MainMenuPresenter.TopBar.ProfileStrip` |
| `pill_coin.png` | 220 × 64 | `26, 26, 26, 26` | Khung chứa số dư xu vàng | `MainMenuPresenter.TopBar.CoinPill` |
| `dock_bg.png` | 600 × 130 | `40, 40, 40, 40` | Khung thanh điều hướng chân trang (Bottom Dock) | `MainMenuPresenter.BottomDock` |
| `panel_toast.png` | 480 × 80 | `24, 24, 24, 24` | Khung thông báo popup / toast nổi giữa màn hình | `MainMenuPresenter._toastPanel` |
| `avatar_ring_cyan.png` | 120 × 120 | `58, 58, 58, 58` | Khung phát sáng Cyan bọc quanh avatar người chơi | `MainMenuPresenter.TopBar.AvatarRing` |

---

## 3. BẢNG ÁNH XẠ ICON VECTOR ĐỘ NÉT CAO (`Assets/HITME/Art/UI/Icons/`)

| Tên File Icon | Kích thước | Mô tả trực quan | Vị trí sử dụng |
|---|---|---|---|
| `ic_home.png` | 48 × 48 | Ngôi nhà mái chữ A | Tab "Trang chủ" trên Bottom Dock |
| `ic_trophy.png` | 48 × 48 | Cúp chiến thắng có quai | Tab "Xếp hạng" trên Bottom Dock |
| `ic_gift.png` | 48 × 48 | Hộp quà buộc nơ nổi | Tab "Nhiệm vụ" trên Bottom Dock |
| `ic_bot.png` | 48 × 48 | Đầu robot mắt kính tròn | Icon nút "Luyện với Bot" |
| `ic_person_add.png` | 48 × 48 | Người kèm dấu cộng bên phải | Icon nút "Tạo phòng" |
| `ic_people.png` | 48 × 48 | Nhóm 2 người kề vai | Icon nút "Vào phòng" |
| `ic_gear.png` | 48 × 48 | Bánh răng cơ khí 6 răng | Icon nút "Cài đặt" |
| `ic_pencil.png` | 40 × 40 | Bút chì nghiêng | Nút chỉnh sửa hồ sơ người chơi |
| `ic_play_outline.png` | 48 × 48 | Tam giác Play bo góc | Icon trên nút chính "Chơi Nhanh" |
| `ic_plus.png` | 36 × 36 | Dấu cộng cân đối | Icon trên nút nạp thêm xu |
| `ic_check.png` | 40 × 40 | Dấu tích chuẩn V | Badge "Đã sẵn sàng" trong In-Match HUD |
| `ic_chevron_left.png` | 40 × 40 | Mũi tên trái nét dày | Nút xoay xem nhân vật trước |
| `ic_chevron_right.png` | 40 × 40 | Mũi tên phải nét dày | Nút xoay xem nhân vật kế tiếp |
| `heart_full.png` | 48 × 48 | Trái tim đỏ khối bóng 3D | Điểm sinh lực (HP) còn lại |
| `heart_empty.png` | 48 × 48 | Trái tim xám mờ bán trong suốt | Điểm sinh lực (HP) đã mất |
| `coin_lotus.png` | 64 × 64 | Đồng xu vàng hoa sen dập nổi | Biểu tượng tiền tệ HIT ME |

---

## 4. BẢNG ÁNH XẠ NHÂN VẬT & VŨ KHÍ (`Assets/HITME/Art/Characters/` & `Weapons/`)

| Tên File Sprite | Kích thước (px) | Pivot chuẩn hóa | Trạng thái xử lý |
|---|---|---|---|
| `namlang_dep_showcase.png` | 512 × 564 | `[0.50, 0.00]` (chân tiếp đất) | Khử viền đen, khử răng cưa 2x Lanczos |
| `namlang_chao_showcase.png` | 512 × 564 | `[0.50, 0.00]` (chân tiếp đất) | Khử viền đen, tư thế giơ chảo |
| `namlang_vot_showcase.png` | 512 × 564 | `[0.50, 0.00]` (chân tiếp đất) | Khử viền đen, tư thế cầm vợt pickleball |
| `portrait_namlang.png` | 180 × 180 | `[0.50, 0.50]` (tâm khuôn mặt) | Cắt khung tròn avatar góc nhìn chính diện |
| `weapon_dep.png` | 140 × 140 | `[0.50, 0.50]` | Dép tổ ong màu xanh truyền thống |
| `weapon_chao.png` | 140 × 140 | `[0.50, 0.50]` | Chảo gang đen đáy bóng |
| `weapon_vot.png` | 140 × 140 | `[0.50, 0.50]` | Vợt pickleball thể thao năng động |

---

## 5. PHÂN BỔ TẦNG BỐI CẢNH ARENA (21 LAYERS)

```
[Layer 0 - Far Sky]         sky_gradient.png (1179x600)
[Layer 1 - Far Clouds]      cloud_soft.png
[Layer 2 - Distant Mtns]    far_mountains.png
[Layer 3 - Village Roofs]   village_rooftops.png
[Layer 4 - Temple Gate]     temple_roof.png + temple_stairs.png
[Layer 5 - Stone Pillars]   pillar_left.png + pillar_right.png
[Layer 6 - Village Decor]   bunting.png + lantern.png + parasol_red.png + parasol_blue.png
[Layer 7 - Arena Ground]    ground_tile_seamless.png (Sàn gạch đá chiến đấu liên tục)
[Layer 8 - Lotus Podium]    lotus_emblem.png (Vòng hoa sen bệ đấu tâm sân)
[Layer 9 - Dynamic Foliage] TreeLeft (16f Blender) + TreeRight (16f Blender) + FlagWave (16f Blender)
[Layer 10 - Actors & VFX]   Characters, Aim Indicator, Projectiles, Hit VFX
[Layer 11 - Foreground]     flower_pots.png + bamboo_fence.png + jars.png + drum.png
[Layer 12 - Screen HUD]     Canvas Scaler (Top HUD + Bottom Action Strip)
```

---

## 6. QUY TẮC LIÊN KẾT PRESENTER (PRESENTER INTEGRATION CONTRACT)

1. **`MainMenuPresenter.cs`:**
   - Hoàn toàn độc lập, có thể chạy trong scene `VisualPreview.unity` mà không cần server mạng.
   - Khi có `HitMe.UI.UIManager`, các sự kiện click nút sẽ tự động kích hoạt `_uiManager.btnQuickMatch.onClick.Invoke()` hoặc `_uiManager.btnTrainWithBot.onClick.Invoke()`.
   - Các tính năng chưa mở ở bản offline sẽ hiển thị banner thông báo Toast mềm mại (tự ẩn sau 2.5s) thay vì treo hoặc văng ứng dụng.

2. **`InMatchHUDPresenter.cs`:**
   - Quản lý đồng bộ số lượng tim người chơi và đối thủ (hỗ trợ hàm `UpdateHearts(playerHp, botHp)`).
   - Bộ đếm thời gian `UpdateTimer(secondsRemaining)` tự động chuyển sang màu đỏ cảnh báo (`#FF4040`) khi thời gian dưới 2 giây.
   - Thước ngắm `_aimRulerTransform` tự động xoay theo `AimController.CurrentDirection`.
   - **Tỷ lệ viewport chiến đấu:** Dành hơn 75% chiều cao màn hình cho việc quan sát sàn đấu và căn góc ném vũ khí.
