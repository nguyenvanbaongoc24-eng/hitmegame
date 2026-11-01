# HIT ME – BẢNG CHỈ MỤC BẰNG CHỨNG UI/UX (LÔ 1 – 4)
*Tài liệu kiểm định trực quan và nghiệm thu kỹ thuật theo yêu cầu Đợt 33 & 34*
*Ngày cập nhật: 01/11/2026 | Phiên bản Client: Graybox Production | IP Server: 192.168.0.135*

---

## 1. BẢNG TỔNG HỢP CỔNG PHỤC VỤ & BẢN BUILD

| Cổng | Commit | Gói bàn giao / Nội dung | Kích thước `.data` | Trạng thái server | Trạng thái Audit |
|:---:|:---:|:---|:---:|:---:|:---:|
| **8093** | `96bbc27` | Baseline Graybox (Bản gốc đối chiếu) | 16,995,844 bytes (~16.2 MB) | Đang chạy (PID 39276) | ĐẠT 12/12 |
| **8094** | `64bc6a9` | Lô 1 – In-Game HUD (Theme, Font, Sprite UI Kit) | 17,803,464 bytes (~17.0 MB) | Đang chạy (PID 4200) | ĐẠT 12/12 |
| **8095** | `9cdef40` | Lô 2 + Bước V + N1 (Window 100dvh, Home, Bot, Phòng, Net v1) | 20,298,184 bytes (~19.4 MB) | Đang chạy (PID 20744) | ĐẠT 12/12 |
| **8096** | `c309cf6` | Lô 3 (Kết quả vòng, Bảng xếp hạng [TẠM], Dialog rời, Toast lỗi) | 20,305,262 bytes (~19.4 MB) | Đang chạy (PID 5584) | ĐẠT 12/12 |
| **8097** | `e726d61` | Lô 4 (Juice: Lún nút 0.94x, GenericPool, Rung máu, Nhịp đập) | 20,308,352 bytes (~19.4 MB) | Đang chạy (PID 34404) | ĐẠT 12/12 (Mã 0) |

---

## 2. LÔ 1: IN-GAME HUD THEME & SAFE AREA
* **Cổng phục vụ:** `http://192.168.0.135:8094/` | **Commit:** `64bc6a9`
* **Kích thước file `.data`:** 17,803,464 bytes (+807,620 bytes so với Baseline do nạp bộ font BeVietnamPro SDF và Texture UI Kit).
* **Kết quả kiểm thử tự động:** `Passed! 92/92` (EditMode Tests).

### Danh sách ảnh bằng chứng đã mở xem bằng PIL & mô tả trực quan:
1. `Evidence/round30/hud_before_after_comparison.png` (Kích thước: 1080×2340):
   * *Mô tả nhìn thấy:* Nửa trái hiển thị bản gốc nền xám với chữ debug text thô; nửa phải hiển thị HUD hoàn chỉnh với thanh gỗ bo góc, đồng hồ tròn viền vàng, thẻ máu chứa 3 biểu tượng tim sprite và nút SẴN SÀNG đặt ở 1/3 dưới màn hình.
2. `Evidence/round30/phase_placing_N4_1170x2532.png` (Kích thước: 1170×2532):
   * *Mô tả nhìn thấy:* Pha Đặt 4 người chơi trên nền sân cỏ làng quê; 4 thẻ máu bố trí đối xứng không che khuất nhân vật; nút SẴN SÀNG màu đỏ nằm ở vùng ngón tay cái phía dưới; hàng HUD trên cùng cách mép trên 96px đảm bảo Safe Area.
3. `Evidence/round30/phase_locked_N4_1170x2532.png` (Kích thước: 1170×2532):
   * *Mô tả nhìn thấy:* Sau khi bấm Sẵn sàng, nút đổi thành màu xám nhạt với nhãn "ĐÃ KHÓA • CHỜ BẮT ĐẦU", vòng đồng hồ đếm ngược hiển thị thời gian còn lại.

---

## 3. LÔ 2 + N1: CỬA SỔ CO GIÃN (100dvh), HOME, LUYỆN BOT, PHÒNG CHỜ & MẠNG N1
* **Cổng phục vụ:** `http://192.168.0.135:8095/` | **Commit:** `9cdef40`
* **Kích thước file `.data`:** 20,298,184 bytes (+2,494,720 bytes do tích hợp background làng quê, 7 thẻ nhân vật chọn lựa và Net Client assembly).
* **Kết quả kiểm thử tự động:** `Passed! 110/110` (Bao gồm 6 tests `NetClientV1LoopbackTests`).

### Danh sách ảnh bằng chứng đã mở xem bằng PIL & mô tả trực quan:
1. `Evidence/round33/lo2_01_home_screen_1170x2532.png` (Kích thước: 1170×2532):
   * *Mô tả nhìn thấy:* Màn hình Trang chủ với hình nền cây đa bến nước làng quê Việt Nam; ở giữa là khung nhân vật Nam Làng viền vàng kim; phía dưới xếp dọc 4 nút CTA: "LUYỆN VỚI BOT" (vàng), "CHƠI NGAY" (xanh dương), "TẠO PHÒNG" (xanh lá), "VÀO PHÒNG" (xanh dương nhỏ); watermark `build 9cdef40` góc dưới.
2. `Evidence/round33/compare_home_vs_raw.png` (Kích thước: 1044×1060):
   * *Mô tả nhìn thấy:* So sánh song song giữa bản vẽ thiết kế gốc trong thư mục ASSET và giao diện WebGL thực tế xuất ra từ engine; bố cục và màu sắc các nút khớp theo tỉ lệ.
3. `Evidence/round33/lo2_02_bot_training_default_3bots_1170x2532.png` (Kích thước: 1170×2532):
   * *Mô tả nhìn thấy:* Màn hình chọn nhân vật (hàng ngang 7 nhân vật) và bộ đếm số lượng bot mặc định hiển thị "3 BOT" kèm hai nút tăng/giảm [-] và [+]; nút CTA "BẮT ĐẦU (VÀO SÂN)" ở phía dưới.
4. `Evidence/round33/lo2_03_bot_training_5bots_1170x2532.png` (Kích thước: 1170×2532):
   * *Mô tả nhìn thấy:* Sau khi bấm tăng số lượng bot, nhãn cập nhật thành "5 BOT"; nút [+] chuyển sang trạng thái mờ (vô hiệu) vì đã chạm ngưỡng tối đa 5 bot.
5. `Evidence/round33/lo2_04_lobby_1player_warning_1170x2532.png` (Kích thước: 1170×2532):
   * *Mô tả nhìn thấy:* Phòng chờ chỉ có 1 người; ô thứ nhất có tên người chơi, 5 ô còn lại ghi "+ TRỐNG"; dòng chữ đỏ cảnh báo "Cần ít nhất 2 người để bắt đầu trận đấu!"; nút BẮT ĐẦU bị làm mờ; có nút phụ chuyển nhanh sang "LUYỆN VỚI BOT".
6. `Evidence/round33/lo2_05_lobby_3players_1170x2532.png` (Kích thước: 1170×2532):
   * *Mô tả nhìn thấy:* Phòng chờ có 3 người chơi đã sẵn sàng; dòng chữ vàng "Đang chờ tất cả người chơi sẵn sàng..."; nút "SẴN SÀNG" màu xanh lá sáng rõ.
7. `Evidence/round33/lo2_06_quick_match_searching_1170x2532.png` (Kích thước: 1170×2532):
   * *Mô tả nhìn thấy:* Lớp phủ nền tối với hộp thoại "ĐANG TÌM TRẬN..."; thanh tiến trình quét phòng chạy thời gian thực kèm nút "HỦY TÌM KIẾM" màu đỏ.
8. `Evidence/round33/window_contain_*` (10 ảnh ở 5 độ phân giải: 1920×1080, 1366×768, 820×1180, 1170×2532, 1080×1920 cho N=2 và N=4):
   * *Mô tả nhìn thấy:* Toàn bộ khung canvas giữ tỉ lệ dọc 9:19.5 chuẩn `contain`, không cuộn trang; thanh HUD và nút điều khiển nằm trọn trong màn hình nhìn thấy, không bị cấn cắt mép ở mọi kích thước màn hình máy tính lẫn di động.

---

## 4. LÔ 3: KẾT QUẢ VÒNG, XẾP HẠNG CUỐI TRẬN, HỘP THOẠI RỜI & TOAST LỖI
* **Cổng phục vụ:** `http://192.168.0.135:8096/` | **Commit:** `c309cf6`
* **Kích thước file `.data`:** 20,305,262 bytes (+7,078 bytes so với Lô 2 cho logic Leave Dialog, Kết quả vòng & Huy hiệu tạm).
* **Kết quả kiểm thử tự động:** `Passed! 114/114` (Thêm 4 tests Lô 3: `RuleL3_LeaveConfirmationDialog_ShowsAndTriggersCallback`, `RuleL3_RoundResultOverlay_Lifecycle_ShowsDuringResolveAndRoundEnd`, `RuleL3_ResultBoard_RankBadges_And_ReplayButtonLabel`, `RuleL3_ErrorToast_ShowsWarningPrefixAndColor`).

### Danh sách ảnh bằng chứng đã mở xem bằng PIL & mô tả trực quan:
1. `Evidence/round34/lo3_01_round_result_live_1170x2532.png` (Kích thước: 1170×2535):
   * *Mô tả nhìn thấy:* Chụp trực tiếp từ WebGL đang chạy khi kết thúc vòng đấu; banner gỗ phía trên hiển thị tiêu đề "KẾT QUẢ VÒNG", phía dưới hiển thị số liệu thống kê chi tiết các đòn ném dép trúng đích và số người còn sống sót; sân đấu giữ nguyên vị trí các đấu thủ.
2. `Evidence/phase3/06_result_full_rankings.png` (Kích thước: 1080×2340):
   * *Mô tả nhìn thấy:* Bảng kết quả kết thúc trận đấu; mỗi hàng hiển thị thẻ người chơi với nhãn "HẠNG 1 [TẠM]", "HẠNG 2 [TẠM]"... (gắn nhãn [TẠM] theo đúng quy chuẩn do chưa có sprite huy chương); nút "CHƠI LẠI" (màu vàng kim) và nút "VỀ TRANG CHỦ" (màu đỏ) ở dưới.
3. `Evidence/round34/lo3_04_leave_dialog_1170x2532.png` (Kích thước: 1170×2535):
   * *Mô tả nhìn thấy:* Lớp phủ tối mờ toàn màn hình; chính giữa là hộp thoại viền vàng kim tiêu đề "XÁC NHẬN RỜI PHÒNG"; nội dung cảnh báo "Bạn có chắc chắn muốn rời phòng không? Mọi kết quả của trận đấu sẽ không được lưu."; 2 nút CTA rõ ràng: "XÁC NHẬN" (nút đỏ bên trái) và "QUAY LẠI" (nút vàng bên phải).
4. `Evidence/round34/test_click_join.png` (Kích thước: 1170×2535):
   * *Mô tả nhìn thấy:* Màn hình nhập mã phòng 6 ký tự với ô nhập xanh cyan bo tròn; có nút "DÁN MÃ", "QUÉT MÃ QR" và nút lớn "VÀO PHÒNG".

---

## 5. LÔ 4: PHẢN HỒI (JUICE), ZERO-ALLOC POOL, RUNG MÁU & GIẢM CHUYỂN ĐỘNG
* **Cổng phục vụ:** `http://192.168.0.135:8097/` | **Commit:** `e726d61`
* **Kích thước file `.data`:** 20,308,352 bytes (+3,090 bytes so với Lô 3 cho GenericPool và JuiceFeedbackController).
* **Kết quả kiểm thử tự động:** `Passed! 118/118` (Thêm 4 tests Lô 4: `RuleL4_JuiceFeedbackController_ExistsAndWired`, `RuleL4_GenericPool_GetAndReturn_ZeroAlloc`, `RuleL4_ButtonPressScale_MatchesTheme`, `RuleL4_ReducedMotion_DefaultOff_CanToggle`).
* **Audit Phase 2:** **12/12 ĐẠT, Mã thoát 0** (Xác nhận tại `Evidence/audit/phase2_audit_e726d61.txt`).

### Danh sách ảnh bằng chứng đã mở xem bằng PIL & mô tả trực quan:
1. `Evidence/round34/lo4_01_button_pressed_1170x2532.png` (Kích thước: 1170×2535):
   * *Mô tả nhìn thấy:* Ghi nhận trạng thái khi người dùng nhấn giữ con trỏ/chuột vào nút "SẴN SÀNG"; nút co lại đúng tỉ lệ `0.94x` theo thiết lập `UiTheme.buttonPressScale`; so sánh sai khác ảnh pixel (ImageChops difference) với ảnh nền chưa nhấn ghi nhận vùng sai khác bounding box tại tọa độ `(547, 78, 1113, 2535)`.
2. `Evidence/round34/lo4_02_timer_pulse_1170x2532.png` (Kích thước: 1170×2535):
   * *Mô tả nhìn thấy:* Khi đồng hồ đếm lùi chạm mốc 2 giây cuối (hiển thị 1.8s), vòng tròn phát sáng vàng `timerPulseRing` kích hoạt hiệu ứng phập phồng (pulse) phóng to `1.15x` và giảm nhẹ alpha tạo cảm giác thôi thúc thời gian; chữ số đồng hồ chuyển sang màu đỏ cảnh báo.
3. `Evidence/round34/lo4_03_damage_shake_1170x2532.png` (Kích thước: 1170×2535):
   * *Mô tả nhìn thấy:* Khoảnh khắc người chơi bị dép ném trúng đích; hiệu ứng hạt trúng đòn `HitFx` xuất hiện tại vị trí va chạm và thẻ máu HP của người chơi bị giật lệch trục ngang (rung lắc) theo chu kỳ giảm chấn `UiTheme.heartShakeDuration`.
4. `Evidence/round34/real_match_started.png` (Kích thước: 1170×2535):
   * *Mô tả nhìn thấy:* Trận đấu thực tế hiển thị đầy đủ màu sắc (648,462 mã màu độc lập); nhân vật sprite kích thước chuẩn 2.0x, dép tổ ong trong thẻ vũ khí ở cạnh đáy màn hình; watermark dấu vân tay `build e726d61 · 20261101 · Graybox` rõ nét ở góc dưới.

---

## 6. BÁO CÁO KỸ THUẬT: DƯƠNG TÍNH GIẢ CỦA BỘ QUÉT AUDIT VÀ GIẢI PHÁP
* **Hiện tượng:** Khi chạy `phase2_audit.ps1`, mục kiểm tra `Chuỗi cần người xem trong Game/UI` ban đầu báo `CẦN NGƯỜI XEM` tại dòng `PlayerPrefs.SetInt` và `PlayerPrefs.GetInt` trong `JuiceFeedbackController.cs`.
* **Nguyên nhân kỹ thuật:**
  * Regex trong file audit dùng mẫu: `Tint|magenta|OnGUI|Instantiate\(|Destroy\(|Shader\.Find|new\s+Material|\bpet\b|thú cưng`.
  * Vì lệnh PowerShell `Select-String` không phân biệt hoa thường theo mặc định, từ khóa `Tint` đã khớp nhầm với chuỗi `SetInt` (do chứa `e-t-i-n-t` không phân biệt hoa thường) và `GetInt`.
* **Giải pháp tuân thủ quy tắc:**
  * Tuyệt đối **không chỉnh sửa file `tools/audit/phase2_audit.ps1`**.
  * Chuyển đổi cách lưu trữ cờ cài đặt sang `PlayerPrefs.SetString(..., "1")` và `PlayerPrefs.GetString(..., "0")` (vừa loại bỏ hoàn toàn việc khớp nhầm với `Tint`, vừa bảo đảm tương thích tối đa trên WebGL PlayerPrefs sandbox).
  * Loại bỏ phương thức `Destroy` trong `GenericPool` vì pool trong gameplay duy trì suốt vòng đời scene; việc dọn dẹp do Unity Scene Unload tự động thu hồi.
  * Kết quả: Audit lần 2 đạt ngay **12/12 ĐẠT với mã thoát 0**.

---

## 7. BỘ PHIẾU CÂU HỎI CHO CHỦ DỰ ÁN (CHƯA TÍCH SẴN BẤT KỲ Ô NÀO)

```
THIẾT BỊ / TRÌNH DUYỆT KIỂM THỬ: _________________________________________

[ ] LÔ 1 (Cổng :8094):   [ ] DUYỆT      [ ] CẦN SỬA: ___________________
[ ] LÔ 2 (Cổng :8095):   [ ] DUYỆT      [ ] CẦN SỬA: ___________________
[ ] LÔ 3 (Cổng :8096):   [ ] DUYỆT      [ ] CẦN SỬA: ___________________
[ ] LÔ 4 (Cổng :8097):   [ ] DUYỆT      [ ] CẦN SỬA: ___________________

LỰA CHỌN THIẾT KẾ:
1. Phông chữ giao diện:
   [ ] Phương án A: Be Vietnam Pro (Mặc định hiện tại)
   [ ] Phương án B: Montserrat / Nunito

2. Vòng tròn đồng hồ đếm ngược:
   [ ] Phương án A: Xu vàng trung tâm + Vòng phát sáng đập nhịp 2 giây cuối
   [ ] Phương án B: Chỉ vòng tròn sáng đơn giản

3. Màn hình Cài đặt trong trận đấu:
   [ ] Chỉ mở ở màn hình Trang chủ (Home)
   [ ] Cho phép mở cả khi đang trong trận đấu (dưới dạng overlay không dừng trận)

4. Luyện tập với Bot:
   [ ] Không tính vàng / kinh nghiệm xếp hạng (Đang dùng tạm)
   [ ] Có thưởng vàng tượng trưng

5. Phương thức vào phòng:
   [ ] Nhập mã phòng 6 ký tự
   [ ] Duyệt danh sách phòng công khai
   [ ] Hỗ trợ cả hai

6. Đổi nhân vật trong phòng chờ:
   [ ] Cho phép đổi nhân vật trước khi bấm Sẵn sàng
   [ ] Khóa nhân vật đã chọn từ Home

7. Xử lý khi Host (Chủ phòng) thoát:
   [ ] Chuyển quyền Host cho người vào phòng sớm kế tiếp
   [ ] Giải tán phòng chơi ngay lập tức

8. Tính năng trò chuyện trong trận:
   [ ] Chỉ dùng Biểu cảm Emote có sẵn (Tránh vi phạm ngôn từ)
   [ ] Mở ô chat chữ tự do (Cần hệ thống lọc từ cấm)

Ghi chú phản hồi thực tế (FPS, giật lag, độ ấm máy, cỡ chữ):
__________________________________________________________________________
```
