# HIT ME VN — BÁO CÁO KIỂM TOÁN NGUYÊN NHÂN GỐC RỄ THỊ GIÁC
## AUDIT BÁO CÁO LỖI GIAO DIỆN, BỐI CẢNH VÀ HIỂN THỊ NHÂN VẬT (AG)
**Vai trò:** Lead Unity UI Engineer, Mobile Game Technical Artist & Visual QA Specialist  
**Ngày thực hiện:** 27/10/2026  
**Môi trường:** Unity 6000.6.4f1 • WebGL Development Build • Viewport 390 × 844 • URL: `http://localhost:8015/`  
**Nhánh kiểm tra:** `feature/codex-unity-core` (Commit `c9eb17f`) đối chiếu với `feature/ag-visual-ui` (Commit `1247a8c`)

---

## 1. TỔNG QUAN HIỆN TRẠNG (RUNTIME SCREENSHOT AUDIT)

Qua kiểm tra trực tiếp trên trình duyệt tại `http://localhost:8015/`, ghi nhận các hiện tượng bất thường:
1. **Nam Làng xuất hiện ở góc trên màn hình:** Một nhân vật Nam Làng kèm vòng phát sáng Cyan chân đứng xuất hiện tại đỉnh trên cùng ($Y \approx +3.0$).
2. **Khối đen trống khổng lồ chiếm 80% màn hình:** Khu vực từ giữa màn hình xuống đáy bị che phủ bởi một mảng màu tối đồng nhất (`#0E1A21`).
3. **Mất toàn bộ Main Menu chuẩn:** Chỉ có dòng chữ text vàng "HIT ME VN", chữ phụ "NAM LÀNG • DÉP TỔ ONG", một nút bấm phẳng màu cam "LUYỆN TẬP", và dòng chữ hướng dẫn. Hoàn toàn không có TopProfileBar, Logo 3D gỗ, thẻ chọn nhân vật/vũ khí, các nút chế độ chơi và Bottom Dock điều hướng.
4. **Mất HUD / Navigation trong trận đấu:** Khi vào trận đấu qua nút "LUYỆN TẬP", giao diện chiến đấu (In-Match HUD) thiếu thước ngắm chuẩn, tim máu đỏ bóng, timer cảnh báo.
5. **Bối cảnh Arena bị đứt đoạn:** Sàn đấu chỉ hiển thị một mảng gạch đá nhỏ ở trung tâm, nửa dưới hoàn toàn là màu nền camera rỗng (`#14262E`).

---

## 2. BẢNG KIỂM TRA CHI TIẾT TỪNG THÀNH PHẦN KỸ THUẬT (SYSTEMATIC INSPECTION)

| Thành phần kiểm tra | Hiện trạng thực tế trong `MainMatch.unity` | Đánh giá & Sai lệch kỹ thuật |
|---|---|---|
| **Canvas Root** | `GameObject: 812333180`, `Name: Canvas` | Tồn tại, kích hoạt (`m_IsActive: 1`), có `GraphicRaycaster` |
| **Canvas Render Mode** | `m_RenderMode: 0` (`ScreenSpaceOverlay`) | Render đè lên toàn bộ camera world. Tuy nhiên camera world bên dưới bị che phủ bởi Image màu tối. |
| **Canvas Enabled State** | `m_Enabled: 1` | Bật bình thường. |
| **Canvas Scaler** | `m_ReferenceResolution: {x: 1080, y: 1920}`<br>`m_MatchWidthOrHeight: 0.5` | **SAI LỆCH TỶ LỆ:** Tỷ lệ tham chiếu 1080×1920 (16:9) không tương thích với tỷ lệ mobile dọc hiện đại (iPhone 15 là 19.5:9, tức 1080×2340 hoặc 1179×2556). Khi chạy trên canvas WebGL 390×844 (~19.5:9), UI bị co hẹp hoặc kéo dài không chuẩn. |
| **Safe Area Inset** | `GameObject: 143965299`, `Name: SafeAreaRoot`<br>Script: `SafeAreaFitter.cs` | Có script `SafeAreaFitter`, nhưng trên WebGL desktop `Screen.safeArea` trả về toàn màn hình `(0, 0, width, height)`, không bù trừ cho notch/dynamic island. |
| **UI Sorting Order** | `Canvas.sortingOrder: 0` | Không xung đột sorting order. |
| **UI Camera References** | Không gán (`m_Camera: {fileID: 0}`) | Bình thường đối với `ScreenSpaceOverlay`. |
| **Overlay Canvases** | Không có Canvas con độc lập. | Tất cả dồn vào 1 Canvas duy nhất. |
| **CanvasGroup Alpha** | Không sử dụng CanvasGroup làm mờ. | |
| **EventSystem** | Tồn tại `GameObject: EventSystem` | Tương tác click chuột bình thường. |
| **UI Prefab References** | **HOÀN TOÀN THIẾU:** Các prefab chuẩn của AG (`MainMenuPanel.prefab`, `InMatchHUDPanel.prefab`) **chưa từng được đưa vào `MainMatch.unity`**. | Thay vì nạp prefab chuẩn, Codex viết một hàm script tạm thời `FinalFoundationBuilder.BuildModularUi` tạo UI thủ công bằng code nguyên thủy (`TextElement`, `ButtonElement`). |
| **Runtime-created UI** | Do `FinalFoundationBuilder.cs` tạo ra trong Editor time | Tạo UI dạng placeholder thô sơ. |
| **Build Scene Reference** | `FinalWebBuildPipeline.cs` chỉ build duy nhất `Assets/Scenes/MainMatch.unity`. | Đúng scene mục tiêu, nhưng nội dung scene chưa được tích hợp asset chuẩn từ AG. |

---

## 3. PHÂN TÍCH NGUYÊN NHÂN GỐC RỄ CHO TỪNG HIỆN TƯỢNG (ROOT CAUSE AUDIT)

### Hiện tượng 1: "Khối đen trống khổng lồ chiếm 80% màn hình"
- **Vị trí code chịu trách nhiệm:** `Assets/HITME/Scripts/Editor/FinalFoundationBuilder.cs` (Dòng 138-139).
```csharp
var menuImage = ui.mainMenuPanel != null ? ui.mainMenuPanel.GetComponent<Image>() : null;
if (menuImage != null) { 
    menuImage.sprite = null; 
    menuImage.color = new Color(.055f, .10f, .13f, .96f); 
    menuImage.type = Image.Type.Simple; 
}
```
- **Nguyên nhân chính xác:** `mainMenuPanel` (`GameObject: 590569084`) bị gán một `Image` không có sprite (sprite = null) với màu đặc `RGBA(14, 26, 33, 245)` (gần như mờ đục 96%). Tấm panel này có anchor toàn màn hình (`anchorMin (0,0)`, `anchorMax (1,1)`), biến thành một tấm phông đen phủ kín toàn bộ nửa dưới và giữa màn hình, che khuất hoàn toàn nhân vật và sàn đấu 2D bên dưới.
- **Phân loại lỗi:** **G & H** (UI placeholder tạm thời do script editor sinh ra đè lên màn hình).

---

### Hiện tượng 2: "Nam Làng xuất hiện ở góc trên màn hình ($Y \approx +3.0$)"
- **Vị trí đối tượng chịu trách nhiệm:** `BotActor` (`GameObject: 1065780919`), `Transform: 1065780921` tại tọa độ thế giới `(x: 0, y: 3.0, z: 0)`.
- **Nguyên nhân chính xác:**
  - Trong `MainMatch.unity`, có 2 nhân vật tồn tại sẵn trong thế giới gameplay: `PlayerActor` (tại $Y = -3.5$) và `BotActor` (tại $Y = 3.0$).
  - Trong `FinalFoundationBuilder.cs` (Dòng 96-109), hàm `BindNamLangAndDep()` gán sprite Nam Làng cho **tất cả** `ActorView` trong scene:
    `actor.spriteIdle = idle; // NamLang/BÌNH THƯỜNG/sprites/sprite_001.png`
    Vì vậy, cả `PlayerActor` và `BotActor` đều mang hình dạng Nam Làng!
  - `PlayerActor` ($Y = -3.5$) bị tấm phông đen của `mainMenuPanel` che mất ở phía dưới.
  - `BotActor` ($Y = 3.0$) nằm ở góc trên màn hình, phía trên tiêu đề "HIT ME VN", lọt ra khỏi vùng che hoặc được nhìn thấy qua mép trên của camera.
  - **Bản chất:** Đây KHÔNG PHẢI là UI Character Showcase của Main Menu. Người dùng đang nhìn xuyên qua mép UI để thấy trực tiếp con `BotActor` đang đứng trong sàn đấu gameplay!
- **Phân loại lỗi:** **A, D & F** (UI Character Showcase của Main Menu chưa từng được tạo; nhân vật nhìn thấy là thực thể Bot trong gameplay world).

---

### Hiện tượng 3: "Mất Main Menu hoàn chỉnh và mất HUD / Navigation"
- **Vị trí code chịu trách nhiệm:** `Assets/HITME/Scripts/Editor/FinalFoundationBuilder.cs` (Dòng 141-149).
```csharp
if (ui.mainMenuPanel != null)
{
    foreach (Transform child in ui.mainMenuPanel.transform) child.gameObject.SetActive(false);
    var title = TextElement("Title", ui.mainMenuPanel.transform, "HIT ME VN", 54, new Vector2(0f, 420f), new Vector2(900f, 100f));
    title.color = new Color(1f, .77f, .18f);
    TextElement("Subtitle", ui.mainMenuPanel.transform, "NAM LÀNG • DÉP TỔ ONG", 25, new Vector2(0f, 335f), new Vector2(900f, 60f));
    TextElement("Instruction", ui.mainMenuPanel.transform, "Chạm để đặt chân • Giữ và kéo để ngắm", 23, new Vector2(0f, -415f), new Vector2(900f, 60f));
    var play = ButtonElement("BtnPlayTraining", ui.mainMenuPanel.transform, "LUYỆN TẬP", new Vector2(0f, -280f));
    ui.btnTrainWithBot = play; ui.btnQuickMatch = null;
}
```
- **Nguyên nhân chính xác:**
  - Lệnh `foreach (Transform child in ui.mainMenuPanel.transform) child.gameObject.SetActive(false);` đã cố ý ẩn toàn bộ các thành phần con hiện hữu của `mainMenuPanel`.
  - Thay vào đó, script chỉ tạo vỏn vẹn 3 chuỗi text thô và 1 nút bấm cam duy nhất ("LUYỆN TẬP").
  - Toàn bộ hệ thống UI Clash Royale chuẩn gồm: Profile người chơi, Logo 3D chính thức, Thẻ chọn nhân vật, Thẻ chọn vũ khí, Nút Chơi Nhanh, Tạo Phòng, Vào Phòng, Luyện Với Bot và Bottom Dock (Trang chủ / Xếp hạng / Nhiệm vụ) **chưa được gắn kết vào scene**.
- **Phân loại lỗi:** **A & H** (UI chưa được khởi tạo từ Prefab; bị script ghi đè bằng phiên bản tối giản).

---

### Hiện tượng 4: "Bối cảnh Arena đứt đoạn, nửa dưới sàn đấu là khoảng trống đen"
- **Vị trí đối tượng chịu trách nhiệm:** `FinalArena_2_5D` (`GameObject: 761873241`), Sprite `CombatField`.
- **Nguyên nhân chính xác:**
  - File ảnh được dùng cho `CombatField` là `HITME_A07_Stone_Tile_Floor.png`.
  - Kiểm tra kích thước file ảnh thực tế: **chỉ có 337 × 262 pixels** (chỉ là một miếng gạch cắt rời ở tâm sân khấu, không phải toàn bộ nền sân đấu).
  - Với PPU = 50 và scale 1.85, chiều cao của sprite này trên màn hình chỉ đạt:
    $$\text{Height} = \frac{262}{50} \times 1.85 = 9.69 \text{ world units}$$
  - Trong khi đó, Camera Orthographic Size = 6.15 (tổng chiều cao khung nhìn = $6.15 \times 2 = 12.3$ units).
  - Vị trí `CombatField` đặt tại $Y = -0.25$, chỉ phủ từ $Y \approx +4.5$ xuống $Y \approx -5.0$. Phía dưới chân người chơi ($Y = -3.5$ đến $Y = -6.15$) và hai bên cánh hoàn toàn không có sàn gạch, làm lộ màu nền Camera Clear Color (`#14262E`).
  - Trong báo cáo `CODEX_INTEGRATION_REPORT.md` (Dòng 40), Codex cũng đã ghi nhận lỗi này nhưng chưa có texture sàn gạch liền mạch (`ground_tile_seamless.png`) để thay thế.
- **Phân loại lỗi:** **C, E & G** (Sử dụng mảnh ảnh nhỏ 337×262 không lặp tile, không phủ kín khung nhìn camera).

---

## 4. KẾT LUẬN & HƯỚNG KHẮC PHỤC TRIỆT ĐỂ

Tất cả các lỗi thị giác trên WebGL xuất phát từ việc **bản build WebGL hiện tại được tạo từ một script dựng tạm (`FinalFoundationBuilder.cs`) với các asset chưa hoàn chỉnh và UI placeholder dạng text thô**.

**Phương án khắc phục (Giải pháp AG):**
1. **Thay thế `mainMenuPanel`:** Loại bỏ tấm phông đen đặc `rgba(14, 26, 33, 0.96)`. Tích hợp trực tiếp Prefab chuẩn [`MainMenuPanel.prefab`](file:///d:/hitme%20game/worktrees/ag-visual-ui/Assets/HITME/Prefabs/Presentation/MainMenuPanel.prefab) của AG với đầy đủ TopProfileBar, Logo HIT ME, CharacterShowcase Nam Làng đứng bệ sen, bộ nút Chơi Nhanh / Luyện Bot / Tạo Phòng và Bottom Dock.
2. **Thay thế `inGameHUDPanel`:** Tích hợp trực tiếp Prefab chuẩn [`InMatchHUDPanel.prefab`](file:///d:/hitme%20game/worktrees/ag-visual-ui/Assets/HITME/Prefabs/Presentation/InMatchHUDPanel.prefab) với tim HP 3D, thanh đếm ngược động (chuyển đỏ khi <= 2s), và nút Khóa lượt.
3. **Cấu hình Canvas Scaler:** Cập nhật `referenceResolution` thành `1080 × 2340` (hoặc `1179 × 2556`), `matchWidthOrHeight = 0.5`.
4. **Sửa sàn đấu Arena:** Thay thế mảnh ảnh `HITME_A07_Stone_Tile_Floor.png` bằng kết cấu sàn đá liền mạch rộng [`ground_tile_seamless.png`](file:///d:/hitme%20game/worktrees/ag-visual-ui/Assets/HITME/Art/Environment/ground_tile_seamless.png) phủ kín từ mép trên tới đáy màn hình, triệt tiêu hoàn toàn mảng trống đen.
