# HIT ME VN — REFORM V7.0 IMPLEMENTATION REPORT
## Toàn bộ thay đổi Runtime UI, Design System, Responsive & WebGL Loading

**Ngày thực hiện:** 2026-11-05  
**Nhánh làm việc:** `feature/ui-ux-production`  
**Phạm vi sở hữu (AG):** Visual Presentation & UI Interaction Layer (không can thiệp server logic, audio manager core hay multiplayer protocol của Codex).

---

## 1. TỔNG QUAN CÁC FILE ĐÃ CẬP NHẬT

| File | Mục đích & Thay đổi chính |
| :--- | :--- |
| `Assets/Scripts/UI/UiTheme.cs` | Chuẩn hóa bảng màu V7.0 (`Dark Navy #111B2C`, `Deep Navy #0B1421`, `Golden Yellow #FFCD38`, `Cyan #19D5D8`, `Emerald Green #17B987`, `Coral Red #F66A5D`, `Cream White #FFF8E8`) và hệ thống Typography tiếng Việt (`BeVietnamPro-Bold SDF`). |
| `Assets/Art/UI/ui_bg_home_village.png` | Cập nhật nền sân đình chất lượng cao (`u4_background.png` / `MAIN MENU.png`) có sẵn bệ đá hoa sen ở trung tâm sân gạch. |
| `Assets/Editor/ProductionUISetup.cs` | Tái cấu trúc toàn bộ 10 màn hình/modal trong scene `Graybox.unity` theo chuẩn Mobile-First Portrait (`1080×2340`, `SafeAreaFitter`). |
| `Assets/Scripts/UI/ProductionUIController.cs` | Đồng bộ logic hiển thị runtime cho Settings (`%` âm lượng trực tiếp, nút `X`), Profile (Avatar nhân vật thật + thẻ chỉ số), Join Room (tắt bàn phím giả 32 nút gây chật màn hình), Room Lobby (tách vị trí chọn bản đồ không đè nút), và Main Menu. |
| `tools/deploy_web/index.html` | Thiết kế lại hoàn toàn màn hình Loading WebGL: nền sân đình `bg_home_750.webp`, logo 3D `logo_hit_me.webp`, nhân vật Nam Làng nhún nhảy, thanh tiến trình Vàng-Cyan, câu gợi ý vui nhộn, nút `CHẠM ĐỂ BẮT ĐẦU` và `THỬ LẠI`, xóa hoàn toàn nhãn `beta`. |
| `tools/deploy_web/web_assets/loading/*` | Tạo bộ ảnh nhẹ `logo_hit_me.webp` và `nam_lang_idle.webp` phục vụ màn hình Loading tức thì. |

---

## 2. CHI TIẾT TRIỂN KHAI THEO TỪNG PHASE (D → O)

### Phase D — Redesign Loading Screen (`tools/deploy_web/index.html`)
- **Xóa bỏ:** Nhãn `.beta-badge` (`HIT ME v1.0.0-beta · Bản thử nghiệm`), nền đen đơn điệu, và chuỗi thông số MB kỹ thuật thô.
- **Triển khai mới:**
  - Nền sân đình làng quê (`web_assets/bg_home/bg_home_750.webp`) phủ lớp vignette Dark Navy giúp chữ và nhân vật nổi bật.
  - Logo 3D chính thức (`web_assets/loading/logo_hit_me.webp`) và hình Nam Làng (`web_assets/loading/nam_lang_idle.webp`) với hiệu ứng idle mượt mà.
  - Thanh tiến trình bo tròn màu Golden Yellow → Cyan (`linear-gradient(90deg, #FFCD38, #19D5D8)`).
  - Các câu gợi ý tiếng Việt thay đổi theo tiến độ tải thật:
    1. `"Đang chuẩn bị sân đấu..."` (`0% – 44%`)
    2. `"Đang xếp dép vào giỏ..."` (`45% – 84%`)
    3. `"Chuẩn bị vào trận!"` (`85% – 100%`)
  - Hỗ trợ nút `"CHẠM ĐỂ BẮT ĐẦU"` (kích hoạt mở khóa âm thanh WebAudio khi cần) và nút `"THỬ LẠI"` khi mất kết nối mạng.

### Phase E — Main Menu Total Redesign (`HomePanel`)
- **Loại bỏ bệ đá trùng lặp:** Thay thế tấm decal `HeroPedestal` đè lên nền bằng bóng đổ mềm (`procedural_shadow.png`, alpha `0.42`) đặt đúng tâm bệ đá hoa sen có sẵn trong ảnh nền sân đình mới.
- **Top Bar (`Y = 0.958`):** Khung hồ sơ người chơi (`280×78`) kèm viền Cyan + Avatar Nam Làng, Pill Vàng (`210×64`), và nút Cài đặt bánh răng (`84×84`, đạt chuẩn vùng chạm `> 44px`).
- **2 Thẻ Nhân Vật & Vũ Khí đối xứng (`Y = 0.435`, `435×126`):** Căn lề trái chuẩn (`pivot = (0, 0.5)`), icon `78×78` sắc nét, nhãn phụ `#19D5D8` / `#FFCD38` và tên chính `27pt` không tràn chữ.
- **Hệ nút hành động phân cấp rõ ràng:**
  - Primary CTA: `CHƠI NHANH` (`890×155`, nền vàng 3D `#FFCD38`, chữ đậm `#23180B` `40pt` kèm phụ đề `"Ghép trận Online • 2–6 người"`).
  - Secondary CTA đối xứng: `TẠO PHÒNG` (`435×132`, Blue 3D) và `VÀO PHÒNG` (`435×132`, Green 3D).
  - Training CTA: `LUYỆN VỚI BOT` (`890×124`, Navy 3D viền sáng, chữ vàng `#FFCD38`).
- **Bottom Dock (`Y = 0.052`, `960×114`):** Một thanh dock thống nhất gồm 3 tab (`Trang chủ`, `Xếp hạng`, `Nhiệm vụ`), tab đang chọn sáng màu Cyan `#19D5D8` kèm gạch chân phát sáng.

### Phase F — Settings Redesign (`SettingsDialog`)
- **Thu gọn Modal:** Giảm kích thước từ `940×1300` xuống `900×1020`, nằm gọn trong vùng nhìn an toàn trên mọi màn hình điện thoại.
- **Xóa bỏ gạch ngang trang trí `---`:** Các tiêu đề nhóm (`ÂM THANH`, `HIỂN THỊ & CHUYỂN ĐỘNG`, `NGÔN NGỮ`) dùng màu Cyan `#19D5D8` căn trái rõ ràng.
- **Thanh trượt lớn kèm phần trăm trực tiếp:** Slider Nhạc nền và Hiệu ứng âm thanh (`390×46`) có tay kéo `48×48` dễ chạm, hiển thị trực tiếp `50%` / `80%` bên phải và cập nhật tức thì khi kéo.
- **Đồng nhất nút đóng:** Nút `✕` góc phải trên (`78×78`) và nút `ĐÓNG` phía dưới gọi chung `HideSettingsDialog()`.

### Phase G — Create Room Fix & Redesign (`CreateRoomPanel`)
- **Khắc phục triệt để lỗi P0 tràn chữ mép trái:** Thay thế 4 dòng chữ đơn có `pivot = (0.5, 0.5)` đặt tại `anchorX = 0.08` (khiến chữ tràn `-274px` ra ngoài màn hình) bằng 4 hàng thẻ cấu hình (`RowPlayers`, `RowMode`, `RowMap`, `RowTimer`, kích thước `830×88`):
  - Cột trái (`anchorX = 0.05`, `pivot = (0, 0.5)`): Nhãn cấu hình (`Số người tối đa`, `Chế độ phòng`, `Bản đồ thi đấu`, `Thời gian đặt & ngắm`).
  - Cột phải (`anchorX = 0.95`, `pivot = (1, 0.5)`): Giá trị nổi bật (`6 người (Chuẩn)`, `Công khai • Mã mời`, `Sân Làng Đình Cổ`, `5.0 giây / vòng`).

### Phase H — Room Lobby Redesign (`LobbyPanel`)
- **6 Slot người chơi có Avatar & không chồng chữ:** Mỗi slot (`880×80`) có `SlotAvatar` (`58×58`), tên người chơi căn trái (`anchorX = 0.12`, `pivot = (0, 0.5)`, rộng `450px`) và trạng thái `CHƯA SẴN SÀNG / ĐÃ SẴN SÀNG` căn phải (`anchorX = 0.96`, `pivot = (1, 0.5)`, rộng `280px`), đảm bảo khoảng cách an toàn không bao giờ đè lên nhau.
- **Tách biệt vị trí chọn bản đồ:** Nút `BtnLobbyMapSelector` được đặt tại `Y = 0.785..0.822` (ngay dưới thẻ Mã phòng), không còn đè lên nút `SANG LUYỆN VỚI BOT` hay dòng đếm ngược.

### Phase I — Join Room Redesign (`JoinRoomPanel`)
- **Loại bỏ bàn phím ảo 32 nút gây rối:** Vô hiệu hóa việc tự sinh `KeypadContainer` trong `EnsureRoomCodeKeypad()` vốn chiếm nửa dưới màn hình và đẩy nút `VÀO PHÒNG` sát đáy.
- **Bố cục nửa trên thân thiện bàn phím hệ thống (`Y = 0.67`):** Ô nhập mã phòng lớn (`820×112`) có placeholder `"Nhập mã phòng (VD: HM4826)..."` (không điền sẵn mã giả), đi kèm 2 nút `DÁN MÃ` và `XÓA MÃ`, thẻ `FoundCard` ẩn mặc định cho tới khi tìm thấy phòng thật, và nút CTA `VÀO PHÒNG NGAY` (`880×140`) nằm cao tại `Y = 0.27`.

### Phase J — Matchmaking Redesign (`QuickMatchOverlay`)
- Tiêu đề `"ĐANG TÌM TRẬN"`, trung tâm hiển thị vòng Radar phát sáng (`160×160`) bao quanh Avatar nhân vật của người chơi (`88×88`), dãy 6 chấm trạng thái slot (`1/6`), bộ đếm thời gian tìm trận, nút chuyển sang đấu Bot khi chờ lâu, và nút đỏ `"HỦY TÌM TRẬN"`.

### Phase K — Profile Redesign (`ProfileDialog`)
- **Khắc phục triệt để ô vuông xanh rỗng:** Khung `Avatar` (`180×180`) luôn chứa `AvatarPortrait` (`152×152`) hiển thị chân dung nhân vật đang chọn.
- **Bố cục Thẻ Chỉ Số (Stat Cards):** Tách rõ khối định danh (Tên, ID, Pill Vàng/Ngọc) và 2 thẻ thông tin (`THÀNH TÍCH ĐẤU` và `VŨ KHÍ TRANG BỊ`), giữ nguyên cơ chế hiển thị `"Đang tải..."` khi ở chế độ Online chưa tải xong dữ liệu.

### Phase L — Bot Training Redesign (`BotTrainingPanel`)
- Làm nổi bật 4 nhân vật khả dụng ở hàng trên kèm chân dung chibi lớn (`82×82`), 3 nhân vật sắp mở ở hàng dưới có nhãn `"SẮP MỞ"` gọn gàng.
- Thẻ chi tiết nhân vật (`CharDetailCard`) bổ sung hình xem trước vũ khí `WeaponPreview` (`84×84`) bên trái, tên và mô tả căn trái chuẩn (`pivot = (0, 0.5)`).
- Nút giảm/tăng số lượng Bot (`−` / `+`) mở rộng lên `92×92` giúp bấm dễ dàng trên điện thoại.

### Phase M & N — In-Match Gameplay HUD & Results (`GameplayHudPanel`, `ResultPanel`)
- **Loại bỏ mảng đen che sân đấu:** Giảm độ đặc của `TopHudBoard` và `BottomHudBoard` từ `0.98` (nâu đen đặc) xuống `0.24` (lớp phủ mờ tinh tế), giúp toàn bộ cổng đình và nền gạch sân làng hiển thị trọn vẹn.
- **6 Thẻ HP (`HpCard_0..5`):** Bổ sung đầu nhân vật `CharHead` (`36×36`), giữ khoảng cách tuyệt đối không chồng lấn giữa `Name`, `Hearts`, và `HpNum`.
