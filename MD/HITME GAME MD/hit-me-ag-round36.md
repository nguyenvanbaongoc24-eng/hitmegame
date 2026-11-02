# HIT ME – LỆNH CHO AG, ĐỢT 36: KIỂM CHỨNG KHÁCH QUAN + ĐỘ BỀN + CHUẨN BỊ NỐI MẠNG

> Dán cho AG, kèm `hit-me-ag-round35.md`, `hit-me-ag-round34.md`, `hit-me-gameplay-rules-v1.2.md`, `hit-me-room-size-rule-v1.3.md`. Quy tắc bằng chứng round 28 vẫn bắt buộc. **Không tag (checklist `v0.2-rc1` còn CHỜ chủ dự án và Codex X6), không viết mã mạng thật, không sửa `tools/audit`, không sửa asset gốc, không đổi luật, không thêm bot vào phòng online.** Làm liên tục theo thứ tự.

## Đánh giá báo cáo Lô 9–10 (cổng 8100, 8101; 129/129 test; audit 12/12 mã thoát 0)
**Ghi nhận:** hoạt họa thở/ngắm/ném/trúng/gục, dép xoay + bóng + hiệu ứng, 25 khung dép khôi phục (ảnh ghép + ảnh trước/sau), video `.webm` cho N=2/N=4, hồ sơ phát hành Brotli (`.br`), tổng tải 46,8 → 16,28 MB, checklist `v0.2-rc1` soạn sẵn mà chưa chạy lệnh tag (đúng).

**Điểm đáng ngờ phải làm rõ:**
1. **Số đo 5,68 giây ở 20 Mbps là không thể.** 16,28 MB × 8 ÷ 20 Mbps = **6,5 giây** chỉ riêng truyền dữ liệu. Kết quả nhanh hơn giới hạn vật lý ⇒ giới hạn băng thông chưa áp dụng đúng, hoặc có dữ liệu từ bộ nhớ đệm/IndexedDB. Đo lại: tắt cache, xóa IndexedDB, xác nhận CDP `Network.emulateNetworkConditions` đang áp dụng (đọc lại trạng thái), dán **số byte đã truyền** và thời gian từng tệp (HAR hoặc `PerformanceResourceTiming`). Mục tiêu "≤ 10 s ở 20 Mbps" chỉ tính ĐẠT khi số đo hợp lý.
2. **"Audio synth SFX" ở Lô 5+6:** round 33 yêu cầu dùng SFX **có sẵn** trong thư mục SFX của dự án, thiếu thì ghi "Thiếu", **không tự tạo âm thanh**. Lập bảng `sự kiện → tệp nguồn trong SFX (đường dẫn)`; chỗ nào đang dùng âm tổng hợp bằng mã thì gắn nhãn `TẠM` và đưa vào danh sách Thiếu; chỗ nào có tệp thật trong SFX mà bạn không dùng thì **thay bằng tệp thật**. Chuẩn hóa âm lượng, dán danh sách.
3. **"Khử số ngoài GameConfig (0.50f, 0.54f) để tránh bẫy regex"** — số phải thật sự nằm trong `GameConfig`/`UiTheme`, không chỉ viết lại. Dán `git diff` các dòng này.
4. **"Mở và kiểm tra ảnh qua PIL"** vẫn xuất hiện (`inspect Lo 9 screenshots via PIL`). Mở ảnh bằng công cụ xem ảnh và ghi điều thấy; hoặc nói thẳng bạn không xem được ảnh, chủ dự án sẽ duyệt hình.
5. **Băm lệch nhau lần nữa:** HEAD `a202ee3`, build `31810c4`, audit `336d982`. Bảng cổng phải ghi `commit mã nguồn cuối` của từng cổng; dán `git diff --stat <commit cổng 8100/8101>..HEAD -- Assets ProjectSettings` (kỳ vọng rỗng).
6. **Bảng câu hỏi hỏi chủ dự án có đồng ý gắn tag chưa** là quá sớm (còn Codex X6 và đo FPS điện thoại). Giữ nguyên checklist, không hỏi tag cho đến khi các dòng (6)(7)(8) đã ĐÃ.
7. **N2 "bị chặn do cần backend WebSocket"** là chưa chính xác: N2 chỉ cần `ClientMatchSession` + `unity_online_glue` của Codex (chạy bằng `LoopbackTransport`, không cần backend). Kiểm `git fetch origin prep/codex` và ghi tên commit/tệp đã có; chưa có thì ghi "CHỜ CODEX: <tên tệp>".

## Việc làm (theo thứ tự)

**Q1. Mỗi cổng là bản cộng dồn?** Xác nhận 8100/8101 chứa toàn bộ Lô 1–10 (chủ dự án chỉ cần thử 8100 và 8101). Tạo `Builds/<thư mục>/BUILD_INFO.json` cho **mọi** bản build (cũ và mới): `commit`, `ngày`, `lô`, `mô tả`, `cộng dồn: true/false`, `nén: none|br`, `cổng gợi ý`. Trình khởi chạy của Codex (`run_local.ps1`) sẽ đọc tệp này để liệt kê.

**Q2. "UI Lint" khách quan (thay mô tả bằng số):** kiểm thử tự động (EditMode/PlayMode, hoặc công cụ xuất cây `RectTransform` ra JSON rồi tập lệnh kiểm) cho **mọi màn hình** (Home, Luyện với bot, Phòng, Đặt, Khóa, Lộ diện, Ném, Kết quả, Kết thúc, Cài đặt, Hồ sơ, Cửa hàng, Túi đồ, Xếp hạng, Đăng nhập, hộp thoại, toast) ở **5 khung** (1080×1920, 1170×2532, 1080×2400, 1080×2520, 1620×2160) và N=2/N=4: (a) mọi `Text` cao ≥ 2,2 % chiều cao màn hình; (b) mọi nút có vùng chạm ≥ 135 px (ở 1080 rộng); (c) không phần tử tương tác nào chồng lên nhau hoặc nằm ngoài Safe Area; (d) không văn bản bị cắt/tràn khung (VI **và** EN); (e) nút chính nằm trong 1/3 dưới ở màn trận. Bảng kết quả `Evidence/ui_lint/ui_lint_<commit>.json` + tóm tắt `Passed! N/N`; vi phạm → sửa hoặc ghi vào danh sách có lý do.

**Q3. Độ bền (khỉ chạm + vòng đời):** `tools/robustness_webgl.mjs` (Playwright): chạy 10 phút trên bản 8100: chạm ngẫu nhiên khắp màn, nhấn hai lần nhanh vào mọi nút, bấm Back/mở-đóng hộp thoại liên tục, đổi kích thước/xoay ngang-dọc, `visibilitychange` (ẩn tab 5/30 giây khi đang Đặt/Ném), nhập tên rất dài/ký tự lạ. Điều kiện: **0 lỗi console, 0 treo, 0 màn hình kẹt, bộ nhớ JS không tăng đơn điệu**; trạng thái sau khi ẩn tab/quay lại rõ ràng (Luyện với bot: tự tạm dừng và có nút Tiếp tục; không đổi kết quả). Dán log + số liệu; sửa lỗi tìm được (commit riêng).

**Q4. Bảng gỡ lỗi cho chủ dự án (`?debug=1`):** hiện `build <commit>`, FPS, kích thước khung, 50 dòng nhật ký cuối, trạng thái cờ (`NetConfig.Enabled`, `CosmeticsEnabled`, `ReducedMotion`), nút "Sao chép nhật ký". Bắt mọi ngoại lệ chưa xử lý và hiện thông báo tiếng Việt thân thiện kèm mã lỗi + commit (thay vì treo im). Không cấp phát mỗi khung khi ẩn; không có trong Release trừ khi có `?debug=1`.

**Q5. Âm thanh:** làm mục 2 ở trên (bảng sự kiện → tệp SFX). Dán bảng và test "mọi sự kiện có nguồn hoặc nhãn TẠM".

**Q6. Đo lại tải trang** theo mục 1; thêm kịch bản "lần tải thứ hai (có cache)". Nếu bản Release (`.br`) vượt 10 s ở 20 Mbps khi đo hợp lệ, đề xuất giảm (cắt texture, atlas, bỏ nhạc dài khỏi `.data` và tải sau) — chỉ đề xuất, chưa đổi hình.

**Q7. N2** nếu glue của Codex đã có (theo mục 7): `UnityOnlineUi : IOnlineUi`; hai "người chơi" trong một tiến trình bằng `LoopbackTransport`; ảnh + video các trạng thái Lobby → Đếm ngược → Đặt → Khóa → Lộ diện → Kết quả → Kết thúc, `not_enough_players`, vào lại, `server_shutdown`. Chưa có → ghi CHỜ và làm Q2–Q6 trước.

## Phiếu cho chủ dự án
Đã rút gọn: chủ dự án **chỉ cần thử `:8100` (đầy đủ nhất) và `:8101` (Release)** nếu Q1 xác nhận cộng dồn; các cổng cũ để so sánh. AG **không** tích hộ.

## Mẫu báo cáo
```
BẢNG CỔNG → COMMIT MÃ NGUỒN CUỐI → NỘI DUNG
BƯỚC: Q1 | Q2 | Q3 | Q4 | Q5 | Q6 | Q7 (và các điểm 1–7 ở phần đánh giá)
ĐÃ LÀM: ...
BẰNG CHỨNG: (UI lint JSON + Passed! N/N; log khỉ chạm; bảng SFX; HAR/số byte; git diff; ảnh đã mở xem + 1 dòng nhìn thấy)
CHƯA CÓ BẰNG CHỨNG / BỊ CHẶN (chờ ai): ...
CÂU HỎI CHO CHỦ DỰ ÁN (không tích sẵn): ...
URL (ipconfig): ...
```
