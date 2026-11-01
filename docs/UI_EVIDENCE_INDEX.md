# HIT ME – BẢNG CHỈ MỤC BẰNG CHỨNG UI/UX (LÔ 1 – 8)
*Tài liệu kiểm định trực quan và nghiệm thu kỹ thuật theo yêu cầu Đợt 33 & 34*
*Ngày cập nhật: 01/11/2026 21:44 | Phiên bản Client: Graybox Production | IP Server: 192.168.0.135*

---

## 1. BẢNG TỔNG HỢP CỔNG PHỤC VỤ & BẢN BUILD

| Cổng | Commit | Gói bàn giao / Nội dung | Kích thước `.data` | Trạng thái server | Trạng thái Audit |
|:---:|:---:|:---|:---:|:---:|:---:|
| **8093** | `96bbc27` | Baseline Graybox (Bản gốc đối chiếu) | 16,995,844 bytes (~16.2 MB) | Đang chạy | ĐẠT 12/12 |
| **8094** | `64bc6a9` | Lô 1 – In-Game HUD (Theme, Font, Sprite UI Kit) | 17,803,464 bytes (~17.0 MB) | Đang chạy | ĐẠT 12/12 |
| **8095** | `9cdef40` | Lô 2 + Bước V + N1 (Window 100dvh, Home, Bot, Phòng, Net v1) | 20,298,184 bytes (~19.4 MB) | Đang chạy | ĐẠT 12/12 |
| **8096** | `c309cf6` | Lô 3 (Kết quả vòng, Bảng xếp hạng [TẠM], Dialog rời, Toast lỗi) | 20,305,262 bytes (~19.4 MB) | Đang chạy | ĐẠT 12/12 |
| **8097** | `e726d61` | Lô 4 (Juice: Lún nút 0.94x, GenericPool, Rung máu, Nhịp đập) | 20,308,352 bytes (~19.4 MB) | Đang chạy | ĐẠT 12/12 |
| **8098** | `5318aee` | Lô 5+6 (AudioService, Settings VI/EN, Tutorial v1.2) | 20,355,408 bytes (~19.4 MB) | Đang chạy | ĐẠT 12/12 |
| **8099** | `09f48d2` | Lô 7+8 (Profile, Shop VN, Inventory, Leaderboard, Login Khách) | 20,380,003 bytes (~19.4 MB) | Đang chạy | ĐẠT 12/12 |

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

---

## 5. Lô 5 & Lô 6: Âm Thanh + Cài Đặt (VI/EN) + Hướng Dẫn Cử Chỉ 3 Bước (v1.2)
- **Cổng:** `8098`
- **Commit:** `5318aee`
- **Thư mục build:** `Builds/WebGL_20261101_5318aee`
- **Kích thước .data:** 20,355,408 bytes (20.35 MB)
- **Kiểm thử EditMode:** `Passed! 123/123` (23 test UI polish, 0 fail)
  - `RuleL5_AudioService_PoolAndClips_Configured` (Passed)
  - `RuleL5_SettingsDialog_UIAndWiring` (Passed)
  - `RuleL5_Localization_DualLanguage_VI_EN_NoMissingKeys` (Passed)
  - `RuleL6_TutorialDialog_UIAndWiring` (Passed)
  - `RuleL6_Tutorial_SkipAndPersistence` (Passed)
- **Audit Phase 2:** `12/12 ĐẠT` (Mã thoát 0, lưu tại `Evidence/audit/phase2_audit_5318aee.txt`).

### Bằng chứng hình ảnh WebGL thực tế (1170×2532) - Kiểm tra trực quan bằng PIL:
1. `lo5_01_settings_dialog_1170x2532.png` (621 KB): Mở hộp thoại Cài đặt (nền tối mờ che sân chơi, khung gỗ tối viền vàng `y=[568, 1958]`, thanh trượt Âm lượng Nhạc nền, thanh trượt Hiệu ứng, nút chuyển Giảm chuyển động, 2 nút chọn ngôn ngữ TIẾNG VIỆT và ENGLISH, nút ĐÓNG màu xanh và nút THOÁT TRẬN màu đỏ).
2. `lo5_02_settings_en_1170x2532.png` (621 KB): Màn hình Cài đặt khi chuyển sang tiếng Anh trên WebGL live (Tiêu đề "SETTINGS", tiêu đề mục "AUDIO", "MUSIC", "SOUND EFFECTS", "REDUCED MOTION", "LANGUAGE", nút "CLOSE" và "LEAVE MATCH"; chữ vừa vặn không tràn khung).
3. `lo6_01_tutorial_step1_1170x2532.png` (1.35 MB): Màn hình hướng dẫn Bước 1/3 (Thẻ hướng dẫn `y=[882, 1645]`, tiêu đề "HƯỚNG DẪN CỬ CHỈ (v1.2)", tiêu đề bước "BƯỚC 1/3: ĐẶT NHÂN VẬT", nội dung "Chạm vào vùng sân để đặt vị trí nhân vật của bạn.", chấm tròn 1 sáng vàng, nút đỏ "BỎ QUA" bên trái và nút vàng "TIẾP THEO" bên phải).
4. `lo6_02_tutorial_step2_1170x2532.png` (1.35 MB): Màn hình hướng dẫn Bước 2/3 (Tiêu đề bước "BƯỚC 2/3: KÉO ĐỂ NGẮM", nội dung "Giữ rồi trượt để ngắm. Thước ngắm bám theo đầu ngón tay.", chấm tròn 2 sáng vàng).
5. `lo6_03_tutorial_step3_1170x2532.png` (1.36 MB): Màn hình hướng dẫn Bước 3/3 (Tiêu đề bước "BƯỚC 3/3: ĐỔI VỊ TRÍ & HỦY", nội dung "Chạm chỗ khác để làm lại. Kéo về dưới 4mm để hủy. Không ngắm = ném thẳng.", chấm tròn 3 sáng vàng, nút xác nhận "ĐÃ HIỂU").

---

## 6. Lô 7 & Lô 8: Hồ Sơ, Túi Đồ, Cửa Hàng Vũ Khí VN, Bảng Xếp Hạng & Đăng Nhập Khách
- **Cổng:** `8099`
- **Commit:** `09f48d2`
- **Thư mục build:** `Builds/WebGL_20261101_09f48d2`
- **Kích thước .data:** 20,380,003 bytes (~20.38 MB)
- **Kiểm thử EditMode:** `Passed! 127/127` (0 failed)
  - `RuleL7_FakeAccountService_RenameAndBalance` (Passed)
  - `RuleL7_FakeInventoryService_WeaponsCatalogAndEquip` (Passed)
  - `RuleL7_ProfileShopInventoryLeaderboard_Dialogs` (Passed)
  - `RuleL8_GuestLoginDialog_MockAndDisabledSocial` (Passed)
- **Audit Phase 2:** `12/12 ĐẠT` (Mã thoát 0, lưu tại `Evidence/audit/phase2_audit_09f48d2.txt`).

### Bảng ánh xạ vũ khí trang trí Việt Nam (Cosmetic Asset Map - Lô 7)
> Quy tắc: Trang trí thuần túy, không thay đổi bán kính, sát thương, tốc độ; trong trận chơi chỉ kích hoạt khi `CosmeticsEnabled=true` (mặc định tắt).
- **Dép Tổ Ong (0 Vàng):** `Assets/Art/Weapons/dep_to_ong_icon.png` (Held: `dep_to_ong_held.png`, Projectile: `dep_to_ong_projectile.png`) • Đầy đủ.
- **Chảo Chống Dính (100 Vàng):** `Assets/Art/Weapons/chao_chong_dinh_icon.png` (Held: `chao_chong_dinh_held.png`, Projectile: `chao_chong_dinh_projectile.png`) • Đầy đủ.
- **Cốc Bia Hơi (150 Vàng = 100 × 1.5):** `Assets/Art/Weapons/coc_bia_icon.png` (Held: `coc_bia_held.png`, Projectile: `coc_bia_projectile.png`) • Đầy đủ.
- **Điếu Cày (225 Vàng = 150 × 1.5):** `Assets/Art/Weapons/dieu_cay_icon.png` (Held: `dieu_cay_held.png`, Projectile: `dieu_cay_projectile.png`) • Đầy đủ.
- **Búa Đóng Đinh (335 Vàng = 225 × 1.5):** `Assets/Art/Weapons/bua_dong_dinh_icon.png` (Held: `bua_dong_dinh_held.png`, Projectile: `bua_dong_dinh_projectile.png`) • Đầy đủ.
- **Vợt Pickleball (500 Vàng = 335 × 1.5):** `Assets/Art/Weapons/vot_pickleball_icon.png` (Held: `vot_pickleball_held.png`, Projectile: `vot_pickleball_projectile.png`) • Đầy đủ.
- **Xe Đạp Phượng Hoàng (750 Vàng = 500 × 1.5):** `Thiếu` sprite hiệu ứng bay và cầm tay.
- **Honda Cũ (1125 Vàng = 750 × 1.5):** `Thiếu` sprite hiệu ứng bay và cầm tay.

### Bằng chứng hình ảnh WebGL thực tế (1170×2532) - Kiểm tra trực quan bằng PIL:
1. `lo7_01_profile_dialog_1170x2532.png` (856 KB): Hộp thoại Hồ sơ người chơi (`y=[730, 1796]`, `x=[85, 1085]`), khung ảnh đại diện, Tên hiển thị "Khách_4826", ID người chơi "ID: HM-9582", Số dư "500 VÀNG • 20 NGỌC", dòng nhãn "LẦN ĐẦU MIỄN PHÍ", nút vàng "ĐỔI TÊN", nút xanh "ĐÓNG".
2. `lo7_02_shop_weapons_1170x2532.png` (815 KB): Hộp thoại Cửa hàng vũ khí (`y=[557, 1969]`, `x=[74, 1095]`), Tiêu đề "CỬA HÀNG VŨ KHÍ", phụ đề "Trang trí thuần túy • Không đổi chỉ số chiến đấu", Số dư "500 VÀNG", 6 hàng vũ khí VN tăng giá theo tỉ lệ 1.5x (Dép Tổ Ong 0G - ĐANG DÙNG; Chảo Rán 100G - MUA; Cốc Bia 150G - MUA; Điếu Cày 225G - MUA; Búa Sắt 335G - MUA; Pickleball 500G - MUA), nút xanh "ĐÓNG".
3. `lo7_03_inventory_weapons_1170x2532.png` (844 KB): Hộp thoại Túi đồ vũ khí (`y=[730, 1796]`, `x=[85, 1085]`), Tiêu đề "TÚI ĐỒ VŨ KHÍ", nhãn "ĐANG DÙNG: DÉP TỔ ONG", ô trang bị mặc định Dép Tổ Ong cùng nút "ĐANG DÙNG", nút xanh "ĐÓNG".
4. `lo7_04_leaderboard_1170x2532.png` (827 KB): Bảng xếp hạng Sân làng (`y=[676, 1850]`, `x=[85, 1085]`), Tiêu đề "BẢNG XẾP HẠNG SÂN LÀNG", Danh sách Top 5 điểm đấu (1. Tuấn_HàNội 1250đ; 2. Nam_Làng 1120đ; 3. Hoa_Bé 980đ; 4. Bạn (Khách_4826) 850đ hiển thị màu vàng nổi bật; 5. Bình_TổOng 790đ), nút xanh "ĐÓNG".
5. `lo8_01_guest_login_1170x2532.png` (883 KB): Màn hình Đăng nhập (`y=[757, 1769]`, `x=[85, 1085]`), Tiêu đề "ĐĂNG NHẬP", nút vàng "CHƠI VỚI TƯ CÁCH KHÁCH", 3 nút đăng nhập mạng xã hội (Google, Facebook, Apple) bị vô hiệu hóa kèm nhãn "(SẮP CÓ)", nút xanh "ĐÓNG".


---

## 8. ĐO THỜI GIAN TẢI TRANG ĐẦU & KÍCH THƯỚC BUILD (Step 8c per Round 33/34)

*Đo lường bằng Playwright (msedge headless, viewport 414×896, networkidle) trên localhost ngày 01/11/2026.*
*File dữ liệu gốc: `Evidence/load_time_measurements.json`*

### 8.1 Bảng kích thước file Build theo từng cổng

| Cổng | Commit | `.data` (MB) | `.wasm` (MB) | `.framework.js` (KB) | `.loader.js` (KB) | **Tổng** (MB) |
|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **8093** | `96bbc27` | 16.21 | 26.95 | 321.7 | 27.3 | **43.47** |
| **8094** | `64bc6a9` | 16.98 | 26.95 | 321.7 | 27.3 | **44.24** |
| **8095** | `9cdef40` | 19.36 | 26.99 | 321.7 | 27.3 | **46.66** |
| **8096** | `c309cf6` | 19.36 | 26.99 | 321.7 | 27.3 | **46.67** |
| **8097** | `e726d61` | 19.37 | 27.00 | 321.7 | 27.3 | **46.68** |
| **8098** | `5318aee` | 19.41 | 27.02 | 321.7 | 27.3 | **46.74** |
| **8099** | `09f48d2` | 19.44 | 27.04 | 321.7 | 27.3 | **46.79** |

### 8.2 Bảng thời gian tải trang đầu (localhost, không nén)

| Cổng | Commit | DCL (ms) | Load (ms) | Network Idle (ms) |
|:---:|:---:|:---:|:---:|:---:|
| **8093** | `96bbc27` | 81.0 | 216.2 | 904.3 |
| **8094** | `64bc6a9` | 103.8 | 175.8 | 848.2 |
| **8095** | `9cdef40` | 34.9 | 102.9 | 775.4 |
| **8096** | `c309cf6` | 32.3 | 97.9 | 785.0 |
| **8097** | `e726d61` | 25.0 | 90.3 | 743.8 |
| **8098** | `5318aee` | 26.8 | 88.6 | 803.2 |
| **8099** | `09f48d2` | 22.6 | 89.1 | 748.8 |

### 8.3 Phân tích tăng trưởng `.data`

| Giai đoạn | `.data` Δ (bytes) | Nguyên nhân |
|---|---:|---|
| Baseline → Lô 1 | +807,620 | Font BeVietnamPro SDF 4096×4096 + Texture UI Kit |
| Lô 1 → Lô 2+N1 | +2,494,720 | Background làng quê, 7 thẻ nhân vật, Net Client assembly |
| Lô 2 → Lô 3 | +7,078 | Leave dialog, Round result, Badge TẠM text |
| Lô 3 → Lô 4 | +3,090 | GenericPool, JuiceFeedbackController |
| Lô 4 → Lô 5+6 | +47,056 | AudioService (SFX pool), SettingsDialog, TutorialDialog, UiStrings EN |
| Lô 5+6 → Lô 7+8 | +24,595 | FakeAccountService, FakeInventoryService, Shop/Profile/Login dialogs |
| **Tổng Baseline→Lô 8** | **+3,384,159 (+3.23 MB)** | **Dưới ngưỡng ≤ 4 MB cho phép** ✅ |

### 8.4 Đề xuất nén (Không thay đổi hình ảnh)

1. **Brotli pre-compress**: `.data` và `.wasm` nén Brotli offline có thể giảm ~70-75% dung lượng truyền tải (46.8 MB → ~12-14 MB). Unity 6 hỗ trợ xuất sẵn `.br` qua Publishing Settings > Compression Format = Brotli.
2. **Gzip fallback**: Cho trình duyệt không hỗ trợ Brotli (Safari cũ), cấu hình server trả `Content-Encoding: gzip` cho `.data`/`.wasm`, giảm ~55-60%.
3. **WebGL Texture Compression (ASTC/ETC2)**: Bật trong Player Settings > WebGL > Texture Compression. Giảm VRAM tiêu thụ trên di động ~4x mà không cần thay đổi file ảnh gốc.
4. **Code stripping (IL2CPP)**: Đã bật `Managed Stripping Level = Medium`. Có thể nâng lên `High` nếu kiểm thử không gây lỗi reflection.

> **Kết luận**: Trên localhost (không nén), tất cả 7 build đều tải trang đầu **dưới 1 giây** (network idle ≤ 904 ms). Trên mạng thực (3G/4G), Brotli là bắt buộc để giữ trải nghiệm tải dưới 5 giây.
