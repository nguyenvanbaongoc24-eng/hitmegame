# HIT ME – LỆNH CHO AG, ĐỢT 37: SỬA LỆCH BẢN BUILD (R0) → PHASE 3: NỐI MẠNG (N2 → N3 → N4)

> Dán cho AG, kèm `hit-me-ag-round36.md`, `hit-me-gameplay-rules-v1.2.md`, `hit-me-room-size-rule-v1.3.md`, `server/README.md` của Codex (nhánh `origin/prep/codex`). Quy tắc bằng chứng round 28 vẫn bắt buộc. **Không tag (checklist `v0.2-rc1` còn CHỜ chủ dự án + Codex X6 + FPS điện thoại), không sửa `tools/audit`, không sửa asset gốc, không đổi luật, không thêm bot vào phòng online.** Làm liên tục theo thứ tự R0 → N2 → N3 → N4, không dừng hỏi chủ dự án các câu kỹ thuật.

## Đánh giá báo cáo round 36
**Ghi nhận:** bảng SFX có nguồn, 12 sự kiện dùng `.wav` thật, dép bay để im lặng thay vì làm giả (đúng); số `0.50f/0.54f` đã vào `UiTheme`; đã nêu thẳng "không xem ảnh bằng mắt, chủ dự án duyệt" (đúng, giữ cách nói này); khỉ chạm 465 thao tác, 0 lỗi console, RAM phẳng 27 MB; `BUILD_INFO.json`; đã kiểm tra nhánh Codex có glue.

**Lỗi / chưa chấp nhận:**
1. **Cổng 8100/8101 KHÔNG chứa mã mới.** Build là `31810c4`, nhưng `git diff --stat 31810c4..HEAD` cho thấy `Assets/` đã đổi: `DebugOverlay.cs` (+433), `UILintTests.cs`, `PlayerView.cs`, `WeaponFlightView.cs`, `UiTheme.cs`, **và `ProjectSettings.asset` (7 dòng)**. Vậy `http://…:8100?debug=1` **không có bảng debug**, và câu hỏi số 1 gửi chủ dự án là vô nghĩa. Thay đổi `PlayerView/WeaponFlightView` đọc từ `UiTheme` cũng **chưa từng chạy trong bản build**. Kỳ vọng của round 36 là `git diff --stat <commit cổng>..HEAD -- Assets ProjectSettings` **rỗng**; bạn đã dán diff khác rỗng mà vẫn báo xong.
2. **Cách giải thích 5,68 s vẫn sai điểm kết thúc.** Bạn đo tới `networkidle` với 11,55 MB (`data.br` + `framework` + `loader`), còn `wasm.br` (≈ 16,28 − 11,55 ≈ 4,7 MB) "tải sau". Nhưng Unity **bắt buộc có wasm trước khi hiện Home**; `networkidle` không phải "Home đã hiện". Số cần là **thời gian tới khung hình Home đầu tiên với đủ 16,28 MB** (tối thiểu vật lý: 6,5 s ở 20 Mbps; 26 s ở 5 Mbps). Số 20,12 s ở 5 Mbps của bạn chỉ là cho 11,55 MB.
3. **UI Lint tự mâu thuẫn:** tóm tắt ghi mọi mục ĐẠT, nhưng JSON ghi `Passed! 4/5`. Lỗi iPad là **có thật**: ở khung rộng 1620, ngưỡng 135 px (tại 1080) tương ứng 202,5 px, nút chỉ 153–157 px ⇒ **vi phạm**, không phải "cảnh báo". Báo cáo cũng không nêu (i) **cách đo** (Playwright không đọc được `RectTransform` trong canvas; số liệu lấy từ đâu?), (ii) **ma trận màn hình × khung** đã phủ (liệt kê các ô N/A), (iii) `UILintTests.cs` đã chạy chưa, **tổng test hiện là bao nhiêu** (round 35 là 129/129).
4. **Audit chạy trên `90e288f`, HEAD là `deefc0d`**, thêm nhiều commit "intermediate audit log". Audit phải chạy trên **HEAD cuối**, sau mọi commit mã.
5. **`BUILD_INFO.json` "mtime đồng bộ với index.html"**: không được chỉnh dấu thời gian tệp để khớp. Chỉ ghi nội dung đúng; giải thích dòng này.
6. **Khỉ chạm thiếu 3 thứ round 36 yêu cầu:** thời lượng thực tế (có đủ 10 phút không?), hành vi sau khi ẩn tab trong Luyện với bot (tự tạm dừng + nút Tiếp tục, kết quả không đổi), nhập tên rất dài/ký tự lạ. Bạn cũng đã sửa `server_no_cache.py` (favicon 204) và sửa bài thử hai lần: liệt kê bộ lọc lỗi console đang bỏ qua điều gì.
7. **5 tệp SFX còn nằm không** (`sfx_wall_thud`, `sfx_error`, `sfx_coin`, `sfx_purchase`, `sfx_chat_pop`). Gắn: dép chạm tường, hộp lỗi/toast lỗi, cửa hàng (nhận/mua), emote. Sự kiện nào thực sự không có chỗ dùng thì ghi lý do.
8. **N2 "chờ chủ dự án kích hoạt" là sai:** lộ trình đã quyết, glue của Codex đã có, **làm ngay**. Câu hỏi 3 (có tích hợp `LoopbackTransport` không) rút lại. Chủ dự án chỉ được hỏi về **hình ảnh/cảm giác**, không hỏi kỹ thuật.

## R0. Sửa lệch bản build (làm trước, ngắn)
1. Giải thích 7 dòng đổi trong `ProjectSettings.asset` (dán diff); nếu chỉ là Unity tự đổi (ví dụ serialization) thì hoàn nguyên bằng `git checkout`, trừ khi cần thật.
2. **Build lại từ HEAD sạch**, cổng **8102 = bản dev cộng dồn có debug**, **8103 = Release Brotli**. Mỗi `Builds/<thư mục>/BUILD_INFO.json` ghi `commit` = HEAD thật lúc build. Dán `git diff --stat <commit build>..HEAD -- Assets ProjectSettings` = **rỗng**. Thủy ấn `build <commit>` khớp.
3. Chạy lại toàn bộ test (Unity EditMode/PlayMode + `GoldenRoundsParityTests` nếu đã nhập) ⇒ dán **một** `Passed! N/N` + tên test mới (UiLint, DebugOverlay).
4. Gắn 5 tệp SFX còn thừa (mục 7); cập nhật `sfx_mapping_table.md`; test "mọi sự kiện có nguồn hoặc nhãn TẠM".
5. **UI Lint v2:** nêu cách đo; ma trận màn hình × khung; sửa nút ≥ ngưỡng ở khung 1620×2160 (hoặc ghi rõ `iPad: ngoài phạm vi v1`, và để chủ dự án duyệt dòng này, không tự gạt). Tóm tắt phải khớp JSON (`Passed! N/N`).
6. **Đo lại tải trang tới "Home hiện":** thêm mốc `window.__hitmeHomeShown = performance.now()` (hoặc log `HOME_SHOWN`) khi khung Home đầu tiên vẽ xong; đo ở 20 Mbps và 5 Mbps, cache tắt, IndexedDB xóa; dán **tổng byte** (`data + wasm + framework + loader`), thời gian từng tệp, thời gian tới `HOME_SHOWN`, và lần tải thứ hai. Nếu > 10 s ở 20 Mbps: đề xuất giảm (chưa đổi hình). Ghi rõ "giả lập mạng".
7. **Khỉ chạm bổ sung trên cổng 8102**: đủ 10 phút (ghi giờ bắt đầu/kết thúc), ẩn tab 5/30 s khi Đặt/Ném (Luyện với bot: tạm dừng + Tiếp tục; kết quả không đổi so với chạy không ẩn cùng hạt giống), tên 64 ký tự + ký tự lạ/emoji, xoay ngang/dọc. Dán log + bộ lọc console.
8. Audit chạy trên **HEAD cuối** (một lần, sau mọi commit mã); không commit "intermediate". Cập nhật bảng cổng: `cổng → commit mã nguồn cuối → khớp HEAD (có/không)`.

## Phase 3 – Nối mạng (làm theo thứ tự; mỗi bước một commit + cổng riêng)

### N2. Glue + hai người trong một tiến trình (Loopback)
- Lấy từ `origin/prep/codex`: `tools/patches/net_client_v1/` (DTO, `ClientMatchSession`, `MatchTransports` có `LoopbackTransport`, `.jslib` nếu có) và `tools/patches/unity_online_glue/` (`IOnlineUi`, `OnlineFlowController`, test). Ghi commit Codex đã lấy. Không sửa nội dung giao thức; thấy bất hợp lý thì ghi vào `docs/NET_FEEDBACK.md` để chuyển Codex.
- Nhập vào `Assets` bằng asmdef riêng (`HitMe.Net`, `noEngineReferences` nếu đúng thiết kế của Codex; phần dính Unity ở asmdef khác). `Core/Phase1` **không đổi**.
- Cài `UnityOnlineUi : IOnlineUi` dùng đúng UI đã có (Lobby/Đặt/Khóa/Lộ diện/Ném/Kết quả/Kết thúc). Cờ `NetConfig.Enabled` **tắt mặc định**; bản Release không có đường vào mạng trừ khi bật cờ.
- Màn nội bộ `?lab=1`: một "người chơi" thật (UI đầy đủ) + 1–5 "người chơi" ảo chạy bằng `LoopbackTransport` (không bot AI trong luật; chỉ là client mô phỏng của test). Chạy N=2 và N=6.
- Test: chạy test của Codex (`OnlineFlowTests`) trong Unity ⇒ `Passed! N/N`; thêm test: kết quả trận qua Loopback **bằng** `GoldenRoundsParityTests`/Core cùng hạt giống.
- Bằng chứng: ảnh + video các trạng thái Lobby → Đếm ngược → Đặt → Khóa → Lộ diện → Ném → Kết quả → Kết thúc; ca `not_enough_players` (1 người bấm Bắt đầu), vào lại trong 30 s, `server_shutdown`, `protocol_mismatch`.

### N3. Màn hình Online (bám asset, VI/EN, đủ trạng thái lỗi)
Thiết kế bằng `UiTheme`/`UiStrings`, chỉ asset có sẵn (thiếu → nhãn `TẠM`). Giá trị chủ dự án chưa chốt dùng **mặc định của Codex** (đổi được bằng cờ): 
- **Vào phòng bằng mã 6 ký tự** (`A–Z` + `2–9`, bỏ `0 O 1 I L`). Dùng **bàn phím ký tự riêng trong game** (nút lớn, đúng ngưỡng 135 px), **không** phụ thuộc bàn phím hệ thống của Safari. Danh sách phòng mở: có cờ, **tắt mặc định**.
- **Tạo phòng**, **Phòng chờ**: danh sách N người (1–6) + trạng thái Sẵn sàng, huy hiệu host, chọn nhân vật **trước khi Sẵn sàng** (khóa khi bắt đầu; không trùng nhân vật), nút Bắt đầu mờ khi < 2 người + câu "Cần ít nhất 2 người…" + nút sang "Luyện với bot" (**không** thêm bot vào phòng).
- **Ghép nhanh:** màn chờ, sau 30 s hiện gợi ý `quick_match_hint` (**chỉ gợi ý**, không tự vào bot).
- **Mất kết nối / vào lại:** lớp phủ "Đang kết nối lại… 30 s", hết hạn → về Home kèm thông báo; AFK bị loại (2 vòng không commit) → thông báo + thời gian cấm 120 s; `server_shutdown`; `rate_limited`; `error` mọi mã trong `server/README.md` → bảng **mã → câu tiếng Việt/EN** trong `UiStrings` + test "không có mã nào hiện thô".
- **Emote** (không chat chữ).
- Cổng 8104 (dev). Test UI Lint v2 phủ thêm các màn này ở 5 khung.

### N4. Nối WebSocket thật tới máy chủ local của Codex (kiểm tra song song)
- Điều kiện: nhánh Codex có `HitMeWebSocket.jslib`/`WebGLWebSocketTransport` và server khởi động được bằng `run_local.ps1`/lệnh trong README. Nếu **chưa**, ghi `CHỜ CODEX: <tên tệp>` và dừng N4 ở đây (không tự viết giao thức khác).
- URL máy chủ lấy từ `?server=ws://<IP>:<cổng>` (mặc định rỗng = tắt mạng); **không** ghi URL/token cứng trong mã.
- Hai trình duyệt Playwright (hai context) vào cùng phòng, chơi hết trận N=2 và N=3; so sánh `match_end` của server với kết quả hiển thị phía client (xếp hạng, HP, số lần trúng). Chạy cùng lúc **Online Lab của Codex** như đối chứng độc lập.
- Ca lỗi: ngắt mạng giữa Đặt và Ném → vào lại < 30 s đúng trạng thái; > 30 s → từ chối; hai tab cùng token; `commit` trễ (hết 5 s) → không đổi trạng thái; tắt server giữa trận → màn lỗi thân thiện.
- Cổng 8105. Chủ dự án sẽ thử bằng điện thoại + máy tính cùng Wi-Fi sau khi Codex báo **SERVER READY v1**; AG không tự nói "online đã chạy tốt" nếu chưa có bằng chứng 2 trình duyệt.

## Phiếu cho chủ dự án
Chủ dự án **vẫn có thể thử ngay `:8100` và `:8101`** (chỉ để xem hình/cảm giác; chưa có bảng debug). Sau R0 sẽ chuyển sang `:8102` (dev) và `:8103` (Release). Ở N3 sẽ có thêm một phiếu riêng. AG **không** tích hộ, **không** hỏi tag.

## Mẫu báo cáo
```
BẢNG CỔNG → COMMIT MÃ NGUỒN CUỐI → KHỚP HEAD? → NỘI DUNG (8093 … 8105)
BƯỚC: R0 | N2 | N3 | N4
ĐÃ LÀM: ...
BẰNG CHỨNG: (git diff --stat build..HEAD rỗng; Passed! N/N + tên test; UI lint JSON + cách đo + ma trận; tổng byte + mốc HOME_SHOWN; log khỉ chạm có giờ; bảng SFX; ảnh/video và nhãn "chưa xem bằng mắt" nếu không xem được; audit trên HEAD cuối)
CHƯA CÓ BẰNG CHỨNG / BỊ CHẶN (chờ ai): ...
PHẢN HỒI GIAO THỨC CHO CODEX (docs/NET_FEEDBACK.md): ...
CÂU HỎI CHO CHỦ DỰ ÁN (chỉ hình ảnh/cảm giác; không tích sẵn): ...
URL (ipconfig): ...
```
