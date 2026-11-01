# HIT ME — DANH SÁCH KIỂM TRA CHẤT LƯỢNG GIAO DIỆN (UI QA CHECKLIST)
*Được biên soạn và rà soát thực tế theo tiêu chuẩn Phase 2 Release & Round 34 Production UI/UX.*

---

## 1. Tiêu Chuẩn Kỹ Thuật Tổng Thể (Universal Quality Bar)

| Tiêu chí | Ngưỡng yêu cầu | Phương pháp kiểm tra | Trạng thái |
|---|---|---|:---:|
| **Safe Area (Vùng an toàn)** | Toàn bộ nút/thẻ/tiêu đề nằm trọn trong vùng an toàn của iPhone (Notch, Dynamic Island, Home Bar). | `SafeAreaFitter.cs` neo tự động `Screen.safeArea`, tỷ lệ neo co giãn chuẩn portrait 1080×2340. | **ĐẠT** |
| **Cỡ chữ tối thiểu** | Tiêu đề chính $\ge 32\text{ pt}$; Phụ đề/Nhãn nút $\ge 24\text{ pt}$; Nội dung nhỏ $\ge 18\text{ pt}$; Tuyệt đối không dùng cỡ $< 14\text{ pt}$. | TextMeshProUGUI cấu hình chuẩn trong `ProductionUISetup.cs`. | **ĐẠT** |
| **Vùng chạm cảm ứng** | Mọi nút bấm di động có vùng chạm tối thiểu $\ge 135\text{ px}$ chiều rộng (trên chuẩn 1080px). | Kích thước nút nhỏ nhất là $200\times 55\text{ px}$ (Bottom nav) đến $880\times 130\text{ px}$ (CTA). | **ĐẠT** |
| **Phản hồi xúc giác/hình ảnh** | Nút bấm co nhẹ 0.94x khi chạm, khôi phục khi nhả; Thẻ HP rung khi mất máu; Đồng hồ nhịp đập 2s cuối. | `JuiceFeedbackController.cs` + `ButtonPressHandler` (Lô 4). Tùy chọn "Giảm chuyển động" tắt hiệu ứng sạch sẽ. | **ĐẠT** |
| **Tiếng Việt có dấu đầy đủ** | Hiển thị trọn vẹn 100% nguyên âm tiếng Việt có dấu (`ă â đ ê ô ơ ư á à ả ã ạ ắ ằ ẳ ẵ ặ ấ ầ ổ ỗ ộ ớ ờ ở ỡ ợ ứ ừ ử ữ ự`). Không lỗi font/ô vuông. | Phông chữ Be Vietnam Pro SDF atlas 4096×4096 tích hợp sẵn toàn bộ 134 ký tự có dấu. | **ĐẠT** |
| **Song ngữ VI / EN** | Chuyển đổi ngôn ngữ tức thì trong Cài đặt; toàn bộ nhãn tiếng Anh dịch sát nghĩa, không tràn khung. | `UiStrings.cs` bảng ánh xạ Anh - Việt; Kiểm thử tự động `RuleL5_Localization_DualLanguage_VI_EN_NoMissingKeys`. | **ĐẠT** |
| **Không cấp phát mỗi khung hình** | `ManagedAllocPerFrame: 0` trong suốt trận đấu; không `Instantiate`/`Destroy`. | GenericPool + Image Pool + Text cache + Audio pool (10 nguồn). Audit tự động xác nhận 0 cấp phát. | **ĐẠT** |

---

## 2. Bảng Kiểm Tra Từng Màn Hình Chi Tiết (Screen-by-Screen QA)

### Màn 1: Trang Chủ (Home Screen) — Cổng :8095, :8098, :8099
- [x] **Safe Area:** Hàng tiêu đề và nút Cài đặt/Hồ sơ cách mép trên 120px; Thanh điều hướng dưới cách mép dưới 40px (tránh vướng Home indicator).
- [x] **Trật tự CTA:** 4 nút xếp theo đúng ưu tiên: (1) Luyện với Bot (Vàng lớn), (2) Chơi ngay/Ghép nhanh (Xanh dương), (3) Tạo phòng (Xanh lá), (4) Vào phòng (Xanh dương viền).
- [x] **Tài nguyên người chơi:** Thanh trên hiển thị Tên khách ("Khách_4826"), Vàng ("500 G"), Ngọc ("20 💎").
- [x] **Thẻ nhân vật:** Khung viền gỗ hiển thị chân dung nhân vật đang chọn, tên nhân vật, mô tả, và biểu tượng vũ khí Dép Tổ Ong.
- [x] **Thanh dưới:** 4 nút CỬA HÀNG, TÚI ĐỒ, XẾP HẠNG, KHÁCH mở đúng các dialog tương ứng.

### Màn 2: Luyện Với Bot (Bot Training Screen) — Cổng :8095
- [x] **Điều khiển số Bot:** Tăng/giảm số bot từ 1 đến 5; tự động vô hiệu hóa nút giảm khi bot=1 và nút tăng khi bot=5.
- [x] **Chọn nhân vật:** Cuộn qua lại danh sách 7 nhân vật làng quê Việt Nam; cập nhật hình ảnh chân dung và mô tả đặc tính theo thời gian thực.
- [x] **Bắt đầu ván đấu:** Nhấn bắt đầu chuyển mượt sang Gameplay HUD và khởi động trận đấu ngoại tuyến không cần mạng.

### Màn 3: Tạo Phòng & Vào Phòng (Create & Join Room) — Cổng :8095
- [x] **Tạo phòng:** Khung nhập tên phòng, thanh chọn số người (2-6), nút xác nhận tạo phòng.
- [x] **Vào phòng:** Ô nhập mã phòng 6 ký tự, hỗ trợ dán mã từ bộ nhớ tạm, tìm kiếm phòng tự động khi đủ 6 ký tự.

### Màn 4: Phòng Chờ Chiến Hữu (Lobby) — Cổng :8095
- [x] **Cảnh báo 1 người:** Khi chỉ có 1 người trong phòng, nút "BẮT ĐẦU" bị làm mờ (vô hiệu) và hiển thị thông báo "Cần ít nhất 2 người. Muốn chơi một mình, hãy sang menu Luyện với bot" kèm nút chuyển thẳng sang Luyện với bot.
- [x] **Đếm ngược tự động:** Khi đủ người và tất cả bấm SẴN SÀNG, kích hoạt đếm ngược 5 giây vào trận đấu.
- [x] **Sao chép mã:** Nút sao chép mã phòng chuyển trạng thái "ĐÃ SAO CHÉP!" trong 1.5 giây.

### Màn 5: Giao Diện Trận Đấu (Gameplay HUD) — Cổng :8094
- [x] **Hàng trên (HUD Top):** Thước ngắm, Biểu ngữ pha trận đấu, Nhãn "VÒNG n/5", Đồng hồ xu vàng đếm ngược 5s.
- [x] **Thẻ máu (HP Cards):** Hiển thị danh sách người chơi/bot, thẻ bản thân viền vàng nổi bật; tim sprite đầy và tim vỡ rõ nét.
- [x] **Hàng dưới (HUD Bottom):** Ô vũ khí Dép Tổ Ong hiển thị góc trái; nút "SẴN SÀNG" to, dễ chạm, chuyển sang "ĐÃ KHÓA" khi khóa vị trí.
- [x] **Bộ đếm FPS:** Chạm 3 lần liên tiếp vào góc trên bên phải để bật/tắt bộ đếm FPS góc trên.

### Màn 6: Kết Quả & Kết Thúc Trận (Round & Match Result) — Cổng :8096
- [x] **Kết quả vòng:** Lớp phủ tự động hiển thị số máu mất, người trúng dép sau mỗi vòng ném và tự ẩn khi qua vòng mới.
- [x] **Bảng xếp hạng chung cuộc:** Hiển thị thứ hạng 1 đến N người chơi kèm số điểm và số mạng hạ gục; dòng bản thân nổi bật màu vàng.
- [x] **Hộp thoại xác nhận rời:** Chặn bấm nhầm khi đang chơi, hỏi xác nhận "Bạn có chắc chắn muốn rời trận?".

### Màn 7: Phản Hồi Xúc Giác & Đồ Họa (Juice Feedback) — Cổng :8097
- [x] **Co nút bấm:** Nút bấm co tỷ lệ $0.94\times$ khi ngón tay chạm xuống và nảy về $1.0\times$ khi nhấc lên.
- [x] **Rung mất máu:** Thẻ HP rung lắc nhẹ khi bị ném trúng.
- [x] **Đồng hồ đập nhịp:** Vòng hào quang phát sáng đập nhịp tuần hoàn trong 2 giây cuối của pha đặt.
- [x] **Giảm chuyển động:** Tùy chọn trong Cài đặt tắt toàn bộ hiệu ứng chuyển động phục vụ người dùng nhạy cảm.

### Màn 8: Âm Thanh & Cài Đặt (Audio & Settings) — Cổng :8098
- [x] **Mở khóa âm thanh Safari iOS:** Tự động kích hoạt AudioContext ngay ở lần chạm đầu tiên của người dùng.
- [x] **Âm lượng độc lập:** Hai thanh trượt điều chỉnh âm lượng riêng biệt cho Nhạc nền và Hiệu ứng âm thanh; lưu vào `PlayerPrefs`.
- [x] **Chuyển ngữ VI/EN:** Nút chọn TIẾNG VIỆT và ENGLISH cập nhật toàn bộ nhãn giao diện tức thì không gián đoạn trận đấu.
- [x] **Overlay không dừng trận:** Hộp thoại Cài đặt là lớp phủ không tạm dừng đồng hồ trận online.

### Màn 9: Hướng Dẫn Cử Chỉ 3 Bước (Tutorial v1.2) — Cổng :8098
- [x] **Bước 1/3:** "ĐẶT NHÂN VẬT" - Chạm vào sân đấu để chọn vị trí đứng.
- [x] **Bước 2/3:** "KÉO ĐỂ NGẮM" - Giữ và kéo trượt thước ngắm bám theo đầu ngón tay.
- [x] **Bước 3/3:** "ĐỔI VỊ TRÍ & HỦY" - Chạm chỗ khác để đổi vị trí; kéo về dưới 4mm để hủy; không ngắm = ném thẳng.
- [x] **Điều hướng & Bỏ qua:** Nút "BỎ QUA" và "TIẾP THEO"/"ĐÃ HIỂU"; lưu trạng thái đã xem vào `PlayerPrefs`.

### Màn 10: Hồ Sơ, Túi Đồ, Cửa Hàng, Bảng Xếp Hạng & Đăng Nhập (Lô 7 & 8) — Cổng :8099
- [x] **Hồ sơ:** Xem ID, tên, số dư; đổi tên lần đầu miễn phí, từ lần hai hiển thị phí 100 vàng.
- [x] **Cửa hàng vũ khí VN:** Danh mục vũ khí Việt Nam giá tăng lũy tiến $1.5\times$ (Chảo, Cốc bia, Điếu cày, Búa, Vợt Pickleball). Ghi rõ "Trang trí thuần túy • Không đổi chỉ số chiến đấu".
- [x] **Túi đồ:** Danh sách vũ khí đã sở hữu, hiển thị rõ vũ khí đang trang bị mặc định là Dép Tổ Ong.
- [x] **Bảng xếp hạng sân làng:** Top người chơi điểm cao, làm nổi bật hàng của người dùng.
- [x] **Đăng nhập khách:** Mặc định tên "Khách_4826"; các nút đăng nhập mạng xã hội (Google, Facebook, Apple) bị vô hiệu hóa kèm nhãn "(SẮP CÓ)".
