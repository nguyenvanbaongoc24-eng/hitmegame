# HIT ME – LỆNH CHO AG, ĐỢT 29: LÀM GIAO DIỆN ĐẸP HƠN, BÁM SÁT ASSET CỦA CHỦ DỰ ÁN

> Dán cho AG, kèm `hit-me-ag-round28.md`, `hit-me-gameplay-rules-v1.2.md`. **Không đổi luật, logic, bán kính trúng.** Mục tiêu: giao diện trông như **cùng một bộ tranh** với nền sân làng quê (đỏ–vàng–gỗ ấm), thay các khung/nút/chữ phẳng hiện nay bằng **asset có sẵn** trong `D:\HITME GAME\ASSET HITME FINAL` (`HITME_UI_KIT_CLEAN`, `HITME_AIM_KIT_V1`, `ASSET TARGET RULER`, `UI_KIT_EXTRACTED`, `ASSET EFFECT`, `ASSET BỐI CẢNH`, thư mục nhân vật/vũ khí).

## Đánh giá báo cáo round 28 (ngắn)
**Ghi nhận:** dòng `build <commit>` góc dưới trái, server `no-store`, đo tỉ lệ nhân vật bằng công cụ (1.0→2.4 tăng đúng tỉ lệ), gỡ Animator/sheet khỏi `PlayerView.prefab` kèm test quét mọi prefab/cảnh, 101/101 test, một XML chuẩn + `archive_tests/INDEX.md`, test F9 qua `MatchState`.

**Còn phải sửa trước/cùng đợt này:**
1. **Audit chưa sạch:** mục "Số cần rà soát ngoài GameConfig" = CẦN NGƯỜI XEM tại `GameConfigAsset.cs:37`, `MatchController.cs:546`, `PlayerView.cs:37` (số cứng mới do bạn thêm khi chỉnh scale). Đưa vào `GameConfig`; audit kết thúc mã thoát 0. Không sửa `tools/audit`.
2. **Chỉ một ảnh được mở xem** (`phase_placing_N2`), nhưng bạn mô tả lời cho cả chục ảnh. Từ nay mô tả chỉ những ảnh đã mở; mở nốt số còn lại.
3. **Địa chỉ LAN:** bạn báo `192.168.0.135`, chủ dự án đang thử ở `10.0.0.108`, trước đó `172.20.10.3`. Ghi trong hướng dẫn: IP đổi theo mạng; chạy `ipconfig` mỗi lần giao bản; điện thoại phải **cùng mạng** với máy chạy server.
4. Chủ dự án **chưa xác nhận** bản mới sửa được lỗi hình ảnh (bảng sprite, nhân vật nhỏ). Làm phần đợt này **trên nền bản đó**, nhưng không báo "xong" nếu chủ dự án chưa duyệt từng lô.

## Việc làm

### U0. Kiểm kê asset và bảng ánh xạ (làm đầu tiên, chưa đụng mã)
Tạo `docs/UI_ASSET_MAP.md`: mỗi dòng = một thành phần giao diện (nút Chơi/Sẵn sàng/Bắt đầu, khung thẻ HP, trái tim, đồng hồ đếm, nhãn vòng, biểu ngữ hướng dẫn, ô vũ khí, bảng kết quả, huy chương hạng, hộp thoại, thanh ngắm, nền màn hình…) | màn hình | **đường dẫn asset gốc** trong `ASSET HITME FINAL` | kích thước gốc | cách dùng (9-slice: viền L/R/T/B; hay sprite cố định) | trạng thái (`Có` / `Thiếu` / `Cần vẽ lại`). Dán danh sách **thành phần thiếu asset** để chủ dự án quyết định; **không tự vẽ khung phẳng thay thế** trừ khi gắn nhãn `TẠM`.
Kèm ảnh ghép xem trước các asset (chép từ bản đã dọn, không sửa file gốc).

### U1. Chủ đề (theme) lấy từ chính asset
- Trích bảng màu từ asset (đỏ, vàng, gỗ, kem, xanh lá, xanh dép...) vào `UiTheme` (ScriptableObject) — màu/viền/cỡ chữ dùng chung, **không** đặt rải rác trong mã.
- Hiện HUD dùng nền đen + chữ xanh neon, **lệch** với nền sân ấm; chuyển sang chữ/khung theo kit. Ghi nhận xét "trước → sau" từng màn.
- Một font hiển thị tiếng Việt đủ dấu (kiểm tra bằng ảnh có chữ "Ế Ổ Ữ Ặ Đ"). Dùng font có trong asset; nếu không có, nêu 2 lựa chọn OFL kèm ảnh thử để chủ dự án chọn. Tất cả chuỗi vào `UiStrings` một chỗ.

### U2. Thay khung/nút/biểu tượng bằng asset (theo lô, mỗi lô gửi chủ dự án duyệt)
- **Lô 1 – HUD trong trận (hay thấy nhất):** thẻ HP (khung + trái tim bằng biểu tượng của kit, không dùng ký tự "♥"), đồng hồ đếm (đồng xu vàng hiện tại: dùng asset, số rõ, nhịp đập 2 giây cuối), nhãn "VÒNG n/5", biểu ngữ hướng dẫn, ô vũ khí (không có chữ "DÉP" nhỏ xíu — chỉ biểu tượng + tên rõ), nút SẴN SÀNG/ĐÃ KHÓA (3 trạng thái từ asset), thước ngắm/vòng đặt vị trí dùng `HITME_AIM_KIT_V1` và `ASSET TARGET RULER` (thay đường kẻ sinh bằng mã).
- **Lô 2 – Home, Luyện với bot, Phòng (Lobby):** nền, nút, ô chọn nhân vật (7 chân dung từ asset nhân vật), chọn số bot, chỗ trống "Đang chờ…", câu "Cần ít nhất 2 người…".
- **Lô 3 – Kết quả vòng, Kết thúc trận (bảng xếp hạng N dòng), hộp thoại thoát, thông báo lỗi:** khung bảng, hạng 1–N bằng huy chương/cờ từ kit, nút Đấu lại/Về sảnh.
- **Lô 4 – Phản hồi (juice):** nút nhấn xuống/nhả; HP mất (trái tim vỡ bằng khung hình sẵn có, rung nhẹ thẻ); trúng đòn bằng hiệu ứng trong `ASSET EFFECT`; số đếm; chuyển màn mờ. Tất cả **dùng pool, 0 cấp phát mỗi khung, không `Instantiate`/`Destroy` trong trận**, có tùy chọn giảm chuyển động.

### U3. Quy tắc kỹ thuật
- Nhập asset: bản **chép** vào `Assets/Art/UI/…` (không sửa file gốc, không tô màu lên sprite để "chữa" sai màu); 9-slice đúng cho khung; Sprite Atlas theo màn; nén phù hợp WebGL; **dung lượng build tăng ≤ 4 MB** (báo trước/sau của `.data`).
- Vùng chạm tối thiểu ≈ 9 mm trên điện thoại (khoảng 135 px ở 1080 rộng); nút chính trong 1/3 dưới màn hình; không đè lên Safe Area/notch; HUD không che nhân vật/sân (đã có ảnh chứng).
- Không `OnGUI`, không `Shader.Find`, không Tween cấp phát; giữ `Passed! N/N` và audit mã thoát 0.

### U4. Bằng chứng và duyệt (theo quy tắc round 28)
Mỗi lô: ảnh WebGL **trước/sau** 1170×2532 (và 1080×2340), có dòng `build <commit>`; bên cạnh là **ảnh asset gốc** để so "bám sát"; mô tả 1 dòng mỗi ảnh **đã mở xem**; bảng duyệt trong `docs/UI_ASSET_MAP.md` cột `Chủ dự án duyệt: CHƯA/ĐÃ`. Mỗi lô build mới (cổng mới, `?v=`), chủ dự án thử trên iPhone rồi mới sang lô tiếp theo.

## Không làm
Không tag; không viết mã mạng; không đổi luật; không sửa `tools/audit`; không vẽ lại/sửa file asset gốc; không thay asset bằng hình tự tạo nếu chưa có dòng `Thiếu` đã được chủ dự án đồng ý.

## Mẫu báo cáo
```
LÔ: U0 | U1 | U2-Lô n
ĐÃ LÀM: ...
BẰNG CHỨNG: (bảng ánh xạ; ảnh trước/sau + asset gốc; mô tả từng ảnh đã mở; dung lượng build; Passed! N/N; audit mã thoát)
THIẾU ASSET / CẦN CHỦ DỰ ÁN QUYẾT: ...
HEAD + build + URL (IP lấy từ ipconfig): ...
```
