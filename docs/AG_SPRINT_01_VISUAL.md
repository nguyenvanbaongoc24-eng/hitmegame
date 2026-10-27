# HIT ME VN — ANTIGRAVITY VISUAL SPRINT 01 REPORT
## CLEAN ASSET AUDIT + CORRECT ARENA + UI FOUNDATION

**Dự án:** HIT ME VN (Unity 6 WebGL mobile portrait)  
**Root chính thức:** `D:\HITME GAME`  
**Nguồn Art:** `D:\HITME GAME\ASSET HITME FINAL`  
**Working Trees:** `worktrees/ag-visual-ui` & `worktrees/codex-unity-core`  
**Engine:** Unity 6000.6.4f1  
**Blender:** 5.2.2 LTS (`D:\App\BLENDER\blender.exe`)  
**Ngày hoàn thành:** 27/10/2026  
**Chịu trách nhiệm:** Senior Unity Technical Artist & Visual QA Specialist (AG)

---

## 1. PHÂN TÍCH HAI ẢNH LỖI (IMAGE A & IMAGE B)

### A. IMAGE A — Main Menu Thực Tế
* **Landscape background bị phóng theo màn hình dọc:** Hình nền cổng làng `bg_village_gate_menu.png` (nguyên bản phối cảnh ngang rộng) khi ép vào khung dọc 9:19.5 khiến hình ảnh bị ngập tràn chi tiết vụn (mái nhà, đồi núi, cờ quạt), làm mất đi độ thoáng của sảnh chờ.
* **Cổng làng không còn là focal point gần nhân vật:** Thay vì làm cổng chào uy nghiêm nằm ngay sau lưng nhân vật, cổng làng bị đẩy lùi sâu vào hậu cảnh xa tít tắp.
* **Logo và character preview thiếu cân đối:** Logo HIT ME 3D và nhân vật Nam Làng bị lẫn vào nền bối cảnh phức tạp phía sau, thiếu bục đứng tiếp đất (pedestal) rõ ràng.
* **Các nút quá nhỏ:** Nút bấm hành động và CTA chưa đạt kích thước touch target tiêu chuẩn mobile (tối thiểu 48-60 CSS px).
* **Selector nhân vật & vũ khí khó đọc:** Màu chữ và nền panel chưa đủ độ tương phản, icon bé và thiếu khung viền vàng sang trọng.
* **Bottom Navigation sát đáy:** Chưa tính toán Safe Area (Dynamic Island / Home Indicator) của iPhone 15, khiến các icon bị dính vào mép dưới.
* **Typography & tương phản:** Font chữ mảnh, đổ bóng chưa chuẩn, thiếu lớp phủ vignette/dimming làm nổi bật các thẻ tương tác.

### B. IMAGE B — Gameplay Thực Tế
* **Hoa văn sen quá lớn:** Hoa sen trung tâm (`57496e78...`) chiếm tới hơn 60% toàn bộ màn hình, nuốt chửng toàn bộ diện tích sàn đấu.
* **Mặt sân không giống khu vực gameplay:** Mặt sân là một bức tranh vẽ tổng hợp liền khối, không có sự tách bạch giữa sàn gạch thi đấu và môi trường xung quanh.
* **Đạo cụ lấn vào vùng điều khiển:** Cặp trống hội, vò gốm, rặng chuối và ao sen được đặt chèn vào ngay giữa sân — đúng vào không gian di chuyển và ngắm ném của người chơi và Bot!
* **Không có ranh giới sân rõ ràng:** Thiếu đường viền bo (border/curb) phân định rõ ràng giữa "Combat Floor" (vùng di chuyển hợp lệ) và "Environment Margins" (vùng trang trí).
* **Nhân vật bị lẫn trong đồ trang trí:** Màu sắc nhân vật bị chìm vào hoa văn sen khổng lồ và các đạo cụ xung quanh.
* **Bottom controls đặt sai bố cục:** Cụm điều khiển dưới đáy màn hình bị lệch, thiếu slot vũ khí chuẩn Clash Royale và chiếm không gian của sàn đấu.
* **HUD quá nhỏ và thiếu thông tin:** Thiếu thẻ người chơi compact với số tim máu rõ ràng, đồng hồ vòng đấu và nhãn trạng thái.

---

## 2. KIỂM KÊ ASSET THẬT (ASSET AUDIT & CLASSIFICATION)

Toàn bộ **914 files hình ảnh** trong thư mục nguồn `D:\HITME GAME\ASSET HITME FINAL` đã được quét, đo đạc kích thước, kiểm tra kênh Alpha và phân loại chuẩn xác theo 12 nhóm chức năng.

Dữ liệu JSON chi tiết: [docs/FINAL_ASSET_AUDIT_SPRINT_01.json](file:///D:/HITME%20GAME/docs/FINAL_ASSET_AUDIT_SPRINT_01.json)

| Nhóm phân loại | Số lượng | Mô tả & Tài nguyên tiêu biểu | Khuyến nghị sử dụng |
| :--- | :---: | :--- | :--- |
| **1. Full background** | 5 | Các bối cảnh toàn màn hình: Cổng làng 9:16 (`9b175166...`), bối cảnh hội hè (`300b93e0...`, `417d3626...`) | Làm phông nền sảnh chờ Main Menu |
| **2. Combat field** | 2 | Bản vẽ sàn đấu tổng hợp (`57496e78...`), viền sân bo đá (`HITME_A08_Arena_Border.png`) | Dựng khung ranh giới thi đấu chuẩn |
| **3. Ground tiles** | 2 | Gạch đá cổ truyền liền mạch (`HITME_A07_Stone_Tile_Floor.png`), vân đá rạn nứt (`HITME_A10_Weathering_Detail.png`) | Lát nền sàn đấu sạch không lỗi lặp |
| **4. Center decal** | 1 | Hoa sen hội làng tâm sân đỏ & vàng kim (`HITME_A09_Lotus_Emblem.png`, 285×254) | Decal trung tâm tỷ lệ cân đối (1.8m) |
| **5. Foreground decorations** | 78 | Hồ sen nở hoa (`3f5f9a73...`), trống hội (`HITME_B06`), vò gốm (`HITME_B10`), chuối (`HITME_C04`), cờ (`HITME_B02`), đèn lồng (`HITME_B03`), mái đình (`HITME_A03`) | Đặt nghiêm ngặt ngoài biên sàn đấu |
| **6. Character sprites** | 55 | Nam Làng, Mai Hồng, Anh Nón Lá, Anh Núi (tư thế đứng Idle, avatar thẻ tròn) | Showcase sảnh chờ & chân dung HUD |
| **7. Character animation frames** | 265 | Chuỗi sprite ném Dép tổ ong, ném Chảo, ném Vợt Pickleball (20-25 frames/động tác) | Sprite sheet animation động tác ném |
| **8. UI buttons** | 111 | Nút vàng 3D CTA, nút navy, xanh lá, xanh lam, các nút tròn icon, nút phân nhánh | Tương tác chạm chuẩn 9-slice |
| **9. Panels** | 86 | Panel thẻ xanh đen navy viền vàng, thanh dock đáy, thanh profile, khung nhập phòng | Khung chứa nội dung thông tin |
| **10. Weapon icons** | 79 | Sprite dép tổ ong vàng/xanh, chảo gang đen, vợt pickleball đỏ cam, icon vũ khí | Ô trang bị & bộ chọn vũ khí |
| **11. VFX** | 215 | Hiệu ứng va chạm Neon & Painterly (Hit impact, stun, whoosh, dust, trail, shockwave) | Tích hợp hiệu ứng va chạm vũ khí |
| **12. Concept/reference boards** | 15 | Các bảng layout đã duyệt trong `ASSET MENU/MENU/` (`MAIN MENU.png`, `LUYỆN VỚI BOT.png`, v.v.) | Chỉ dùng làm kim chỉ nam thiết kế (Reference) |

### Contact Sheets (Bảng Tổng Hợp Trực Quan)
Bốn contact sheet phân giải cao đã được tạo và lưu tại:
1. **Environment & Arena:** [docs/evidence/contact_sheet_environment.png](file:///D:/HITME%20GAME/docs/evidence/contact_sheet_environment.png)
2. **Characters & Weapons:** [docs/evidence/contact_sheet_characters_weapons.png](file:///D:/HITME%20GAME/docs/evidence/contact_sheet_characters_weapons.png)
3. **UI Controls & Panels:** [docs/evidence/contact_sheet_ui_controls.png](file:///D:/HITME%20GAME/docs/evidence/contact_sheet_ui_controls.png)
4. **Visual Effects (VFX):** [docs/evidence/contact_sheet_vfx.png](file:///D:/HITME%20GAME/docs/evidence/contact_sheet_vfx.png)

---

## 3. TÁCH BẠCH BA VÙNG HÌNH HỌC SÂN ĐẤU (THE THREE-TIER SEPARATION)

Để giải quyết triệt để lỗi của IMAGE B, chúng tôi đã tách rời hoàn toàn 3 lớp cấu trúc:

```
+-------------------------------------------------------------+
|               FAR BACKGROUND (Y > 3.6)                      |
|   Núi mờ (A01) -> Mái ngói (A02) -> Mái đình (A03) -> Cờ    |
+-------------------------------------------------------------+
| LEFT MARGIN      |       COMBAT FLOOR (VÙNG CHƠI)     | RIGHT MARGIN |
| Cây trái (C01)   |   Width = 4.5m, Height = 8.2m      | Cây phải (C02)|
| Rào tre (B08)    |   Gạch đá liền mạch (A07)          | Vò gốm (B10) |
| Trống hội (B06)  |   Viền đá bo ranh giới (A08)       | Rặng chuối   |
| (KHÔNG va chạm)  |   Hoa sen tâm cân đối (A09, 1.8m)  | Cờ Việt Nam  |
|                  |   KHÔNG CÓ VẬT CẢN NỘI BỘ          |              |
|                  |   Bot Zone (Y = 1.0 -> 3.4)        |              |
|                  |   Player Zone (Y = -4.2 -> -1.8)   |              |
+-------------------------------------------------------------+
|             FOREGROUND MARGIN (Y < -4.8)                    |
|   Hồ sen nở hoa viền đá (C05 / lotus_pond_foreground)       |
|   Nằm trọn dưới đáy màn hình, ôm trọn Safe Area             |
+-------------------------------------------------------------+
```

1. **Combat Floor (Vùng chơi thật):**
   - Hình chữ nhật đứng $4.5 \times 8.2$ đơn vị (tỷ lệ chuẩn portrait mobile).
   - Lát bằng gạch đá cổ truyền liền mạch `HITME_A07_Stone_Tile_Floor.png`, phủ vân rạn nứt `HITME_A10_Weathering_Detail.png`.
   - Viền bo đá trắng xám tinh tế `HITME_A08_Arena_Border.png` xác định rõ ranh giới ném và di chuyển.
   - Vạch kẻ phân cách tâm sân tại $Y = -0.4$ với Decal hoa sen `HITME_A09_Lotus_Emblem.png` rộng $1.8$ đơn vị (chiếm ~20% chiều rộng sân, hoàn toàn không che khuất nhân vật).
   - **Tuyệt đối không có đạo cụ nào nằm trong hình chữ nhật này.**
2. **Environment (Bối cảnh ngoài vùng chơi):**
   - Phía trên: Núi xa, mái đình cổ kính, cổng tam quan, cờ ngũ sắc và đèn lồng đỏ.
   - Hai bên lề ($X < -2.25$ và $X > +2.25$): Cây cổ thụ râm mát, rào tre, cụm vò gốm đất nung, trống hội và rặng chuối làng quê.
   - Phía dưới ($Y < -4.8$): Hồ hoa sen nở rực rỡ ôm trọn cạnh dưới, tạo chiều sâu thị giác tiền cảnh.
3. **Decorations (Đạo cụ):**
   - Toàn bộ là visual non-blocking objects, nằm riêng ở sorting layer `SideDecor` và `EnvironmentBack`, không can thiệp vào gameplay raycast hay projectile physics.

---

## 4. HỆ THỐNG PREFABS & SCENE VISUAL STAGING ĐÃ TẠO

Chúng tôi đã thiết lập scene staging riêng biệt **hoàn toàn không sửa `Assets/Scenes/MainMatch.unity` của Codex**:

* **Scene Staging:** [Assets/Scenes/ArenaVisual_Staging.unity](file:///D:/HITME%20GAME/worktrees/codex-unity-core/Assets/Scenes/ArenaVisual_Staging.unity)
* **Editor Tool Generator:** [Assets/HITME/Scripts/Editor/ArenaStagingBuilder.cs](file:///D:/HITME%20GAME/worktrees/codex-unity-core/Assets/HITME/Scripts/Editor/ArenaStagingBuilder.cs)

### Các Prefab độc lập sẵn sàng bàn giao cho Codex:
1. **[Arena_CombatFloor.prefab](file:///D:/HITME%20GAME/worktrees/codex-unity-core/Assets/HITME/Prefabs/Arena/Arena_CombatFloor.prefab):**
   - Chứa toàn bộ sàn gạch, viền bo đá, vạch tâm và decal hoa sen tỉ lệ chuẩn.
2. **[Arena_EnvironmentDecor.prefab](file:///D:/HITME%20GAME/worktrees/codex-unity-core/Assets/HITME/Prefabs/Arena/Arena_EnvironmentDecor.prefab):**
   - Chứa bối cảnh núi xa, đình chùa, cờ hội, đèn lồng, cây xanh hai bên và hồ sen tiền cảnh.
3. **[MainMenu_VisualStaging.prefab](file:///D:/HITME%20GAME/worktrees/codex-unity-core/Assets/HITME/Prefabs/Presentation/MainMenu_VisualStaging.prefab):**
   - Giao diện sảnh chờ Main Menu hoàn chỉnh theo chuẩn Clash Royale: Profile bar trên cùng, logo 3D, bục sen danh dự cho Nam Làng, 2 thẻ chọn nhân vật/vũ khí, nút Primary Yellow CTA "CHƠI NHANH", "LUYỆN VỚI BOT", "TẠO PHÒNG", "VÀO PHÒNG" và Dock điều hướng đáy.
4. **[InMatchHUD_Compact.prefab](file:///D:/HITME%20GAME/worktrees/codex-unity-core/Assets/HITME/Prefabs/Presentation/InMatchHUD_Compact.prefab):**
   - Giao diện In-Match HUD thu gọn: Thẻ Bạn (3 tim đỏ), Thẻ Bot (3 tim đỏ), Pill Vòng 1/5 kèm đồng hồ 0:15, ô vũ khí Dép tổ ong và nút vàng "SẴN SÀNG".

---

## 5. HÌNH ẢNH SCREENSHOT THỰC TẾ XUẤT TỪ UNITY CAMERA

Bốn bức ảnh chụp thực tế từ Unity Orthographic Camera tại độ phân giải **1080 × 2340** (iPhone 15 Portrait):

### 1. Sàn Đấu Riêng Biệt (Arena Floor Only)
> Mặt sân hình chữ nhật đứng $4.5 \times 8.2\text{m}$, gạch lát liền mạch, viền bo đá trắng rõ nét, hoa sen tâm $1.8\text{m}$ cân đối, hoàn toàn sạch bóng không có vật cản.

![Arena Floor Only](file:///D:/HITME%20GAME/docs/evidence/unity_01_arena_floor_only.png)

---

### 2. Sân Đấu Kèm Lớp Môi Trường & Nhân Vật (Arena with Environment Layers)
> Bổ sung núi xa, mái đình, cờ hội, hàng cây lề bên ngoài, trống đại, chum vò gốm và hồ sen nở hoa dưới chân. Nam Làng và Bot đứng ngay ngắn trên sàn đấu có bóng tiếp đất.

![Arena with Environment](file:///D:/HITME%20GAME/docs/evidence/unity_02_arena_with_environment.png)

---

### 3. Sảnh Chờ Main Menu Chuẩn Hóa (Main Menu Staging)
> Cổng làng làm nền sâu lắng kèm lớp phủ vignette, Nam Làng đứng trên bục sen trang trọng, 2 thẻ chọn Navy viền vàng, nút vàng 3D "CHƠI NHANH", typography tiếng Việt đầy đủ dấu và thanh điều hướng Safe Area.

![Main Menu Staging](file:///D:/HITME%20GAME/docs/evidence/unity_03_main_menu_staging.png)

---

### 4. Giao Diện In-Match HUD Gọn Gàng (Compact HUD Preview)
> Top HUD hiển thị tim máu `♥ ♥ ♥`, thời gian đếm ngược 15s, vòng đấu `VÒNG 1/5`. Bottom HUD có ô Dép tổ ong và nút vàng 3D "SẴN SÀNG". Đường ngắm chấm cyan xuất phát từ nhân vật.

![Compact HUD Preview](file:///D:/HITME%20GAME/docs/evidence/unity_04_compact_hud_preview.png)

---

## 6. KIỂM TRA TƯƠNG THÍCH BLENDER 5.2

* **Trạng thái:** Verified executable tại `D:\App\BLENDER\blender.exe` (Blender 5.2.2 LTS).
* **Pipeline Headless:** Đã xác nhận cờ thực thi dòng lệnh `-b -P` hoạt động trơn tru trên Windows.
* **Quyết định kỹ thuật Sprint 01:** Tuân thủ chỉ thị hoãn render hàng loạt hiệu ứng đung đưa (cây, cờ, đèn lồng) để tập trung cố định mặt sân và tỷ lệ camera trước. Các assets đã được chuẩn hóa pivot để sẵn sàng nhận motion frame từ Blender ở Sprint tiếp theo.

---

## 7. VISUAL QA & ACCEPTANCE CHECKLIST (DEFINITION OF DONE)

| Tiêu chí | Trạng thái | Ghi chú kiểm thử thực tế |
| :--- | :---: | :--- |
| **Phân loại Asset đúng** | **PASS** | 914 file phân thành 12 nhóm, xuất manifest JSON và 4 contact sheets. |
| **Mặt sân không lỗi** | **PASS** | Gạch đá cổ truyền liền mạch, không còn mảng màu cam đất hay hoa văn vỡ vụn. |
| **Hoa sen đúng tỷ lệ** | **PASS** | Rộng 1.8m, nằm ngay ngắn ở tâm sân, không nuốt chửng màn hình. |
| **Environment không che combat field** | **PASS** | Đạo cụ (trống, chum, chuối, hồ sen) nằm tuyệt đối ngoài vùng di chuyển. |
| **Main Menu prefab đọc được** | **PASS** | Typography BeVietnamPro-Bold sắc nét, có bục sen, thẻ chọn và nút CTA nổi bật. |
| **HUD preview compact** | **PASS** | Thẻ máu 3 tim gọn gàng, pill đếm ngược, ô vũ khí và nút SẴN SÀNG chuẩn Clash Royale. |
| **Không sprite sheet dán nguyên tấm** | **PASS** | Mọi prop và UI widget đều là các sprite đã trích xuất độc lập. |
| **Không nền/crop artifact** | **PASS** | Alpha transparency mượt mà, PPU 100, bilinear filtering. |
| **File sẵn sàng bàn giao cho Codex** | **PASS** | 4 prefab độc lập trong `Assets/HITME/Prefabs/`, không đè scene `MainMatch.unity`. |
| **Screenshot lấy từ Unity thật** | **PASS** | Render trực tiếp từ Camera qua RenderTexture 1080×2340 bằng Unity batchmode. |

---

## 8. HƯỚNG DẪN BÀN GIAO CHO CODEX

Codex có thể tích hợp các thành phần đồ họa của AG vào gameplay bất kỳ lúc nào thông qua 4 prefab hợp đồng:

1. **Thay thế hình ảnh sàn đấu:** Kéo [Assets/HITME/Prefabs/Arena/Arena_CombatFloor.prefab](file:///D:/HITME%20GAME/worktrees/codex-unity-core/Assets/HITME/Prefabs/Arena/Arena_CombatFloor.prefab) vào Scene tại tọa độ $(0, 0, 0)$.
2. **Kích hoạt đạo cụ bối cảnh:** Kéo [Assets/HITME/Prefabs/Arena/Arena_EnvironmentDecor.prefab](file:///D:/HITME%20GAME/worktrees/codex-unity-core/Assets/HITME/Prefabs/Arena/Arena_EnvironmentDecor.prefab) vào Scene.
3. **Hiển thị HUD trận đấu:** Kết nối dữ liệu máu (HP), timer và trạng thái nút vào [Assets/HITME/Prefabs/Presentation/InMatchHUD_Compact.prefab](file:///D:/HITME%20GAME/worktrees/codex-unity-core/Assets/HITME/Prefabs/Presentation/InMatchHUD_Compact.prefab).
4. **Sảnh chờ Main Menu:** Sử dụng [Assets/HITME/Prefabs/Presentation/MainMenu_VisualStaging.prefab](file:///D:/HITME%20GAME/worktrees/codex-unity-core/Assets/HITME/Prefabs/Presentation/MainMenu_VisualStaging.prefab) làm sảnh chờ chính thức.

Toàn bộ mã nguồn, asset và tài liệu đã được kiểm chứng và đồng bộ trên cả hai worktree.
