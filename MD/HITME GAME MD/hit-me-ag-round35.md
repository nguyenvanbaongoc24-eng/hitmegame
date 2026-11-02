# HIT ME – LỆNH CHO AG, ĐỢT 35: CHỐT PHASE 2 SẠCH + "CẢM GIÁC CHƠI" + BẢN PHÁT HÀNH WEBGL

> Dán cho AG, kèm `hit-me-ag-round34.md`, `hit-me-ag-round33.md`, `hit-me-gameplay-rules-v1.2.md`, `hit-me-room-size-rule-v1.3.md`, `hit-me-weapon-sprite-fix.md`. Quy tắc bằng chứng round 28 vẫn bắt buộc. **Không tag, không viết mã mạng thật, không sửa `tools/audit`, không sửa asset gốc, không đổi luật, không thêm bot vào phòng online.** Làm liên tục theo thứ tự, không dừng chờ.

## Đánh giá báo cáo (Lô 1–8, 7 cổng 8093–8099, 127/127 test, audit 12/12 mã thoát 0, `.data` 16,21 → 19,44 MB)
**Ghi nhận:** đủ 8 lô có commit và cổng riêng; `docs/UI_EVIDENCE_INDEX.md`, `UI_QA_CHECKLIST.md`, mục `Thiếu/TẠM` trong `UI_ASSET_MAP.md`; `.data` trong ngưỡng cộng dồn (+3,23 MB ≤ 4 MB); bản kê đo thời gian tải; ý tưởng Brotli đúng hướng.

**Chưa chấp nhận / phải làm rõ:**
1. **"25 ảnh đã mở và kiểm tra trực quan qua PIL"** — lần thứ tư dùng cụm này. PIL là đếm điểm ảnh, không phải xem ảnh. Mở từng ảnh bằng công cụ xem ảnh, ghi điều thấy. Nếu bạn không có công cụ xem ảnh trong phiên này, **nói thẳng** và chủ dự án sẽ là người duyệt hình ảnh (không gắn nhãn "đạt" cho phần hình ảnh).
2. **Báo "hoàn thành nghiệm thu Phase 2 (8a,b,c)" là sai trình tự:** chưa có chủ dự án thử trên iPhone, chưa có Codex X6. Phase 2 chỉ đóng khi có cả hai. Đổi nhãn thành "SẴN SÀNG CHO NGHIỆM THU".
3. **Hash lệch nhau:** HEAD `a713ee8`, build `09f48d2`, audit `a41b0c6`. Dán `git diff --stat 09f48d2..HEAD -- Assets ProjectSettings`; nếu khác rỗng thì build lại; nếu rỗng, ghi "chỉ docs/Evidence". Bảng cổng phải ghi thêm cột `HEAD của cổng = commit mã nguồn cuối`.
4. **Thời gian tải 748 → 904 ms trên localhost vô nghĩa** cho điện thoại (không có mạng thật). Quy đổi bằng số: `.data` + `.wasm` + `.js` (byte, gzip/Brotli ước tính) ÷ băng thông giả định 20 Mbps và 5 Mbps = giây tải ở 4G/Wi-Fi yếu; ghi rõ là **ước tính**. Mục tiêu mục dưới (Lô 10): lần tải đầu ≤ 10 giây ở 20 Mbps.
5. Mục `1.` của báo cáo "tuân thủ nghiêm ngặt" không có bằng chứng; thay bằng: bảng luật v1.2/v1.3 → test (đã có ở `PHASE2_REPORT.md`) cập nhật cho 127 test; luật chưa có test thì liệt kê.

## Việc làm (theo thứ tự)

### P. `docs/RELEASE_CHECKLIST_v0.2-rc1.md` (làm đầu tiên, ngắn)
Bảng điều kiện đóng Phase 2, mỗi dòng `trạng thái` (ĐÃ/CHƯA/CHỜ AI) và bằng chứng: (1) audit 12/12 mã thoát 0 trên HEAD sạch; (2) manifest `missing=badHash=stale=0` trên bản clone sạch; (3) build ↔ HEAD khớp; (4) một XML chuẩn; (5) ảnh WebGL từng pha có sidecar; (6) **chủ dự án thử iPhone: Lô 1–8 duyệt** (CHỜ chủ dự án); (7) **Codex X6** (CHỜ Codex); (8) đo FPS điện thoại (CHỜ chủ dự án); (9) danh sách tồn đọng được duyệt; (10) lệnh gắn tag chuẩn bị sẵn nhưng **chưa chạy**.

### Lô 9 – Cảm giác chơi: hoạt họa + hiệu ứng trong trận (chỉ bằng asset có sẵn)
Chủ dự án từng chê "động tác/hiệu ứng giả". Làm cho chuỗi một vòng **đọc được** trên màn hình nhỏ, dùng khung có sẵn của 7 nhân vật (đứng, ngắm, ném – gồng/thả, trúng đòn, bị loại) và `ASSET EFFECT`:
- Nhân vật: nhịp thở/nhún nhẹ khi đứng, tư thế ngắm khi có hướng, ném (gồng → thả) đồng bộ với lúc dép rời tay, phản ứng trúng đòn (nhấp nháy/lùi nhẹ), bị loại (khung `defeat`); không dịch chuyển vị trí logic.
- Dép: xoay khi bay, bóng đổ, vệt bay, hiệu ứng va chạm khi trúng/chạm tường; **phục hồi sprite dép đầy đủ (W1–W3, `hit-me-weapon-sprite-fix.md`)**: dùng đúng khung 25 frame trong `ASSET WEAPONS\DÉP\sprites`; ảnh ghép khung dép trước/sau (hộp bao, màu xanh còn lại) để chứng minh không còn bị cụt.
- Số liệu thời gian (độ dài hoạt họa, vệt, rung màn) đều nằm trong `GameConfig`/`UiTheme`, **không đổi** thời lượng pha của luật; chạy lại toàn bộ test + `GoldenRoundsParityTests` (nếu đã nhập) để chứng minh **kết quả trận không đổi**. 0 cấp phát trong trận (pool), có "giảm chuyển động".
- Bằng chứng: **đoạn quay màn hình** (Playwright `recordVideo`) một vòng đủ pha ở N=2 và N=4, ảnh ghép từng khung hoạt họa (contact sheet) cho mỗi nhân vật, bảng thời gian mỗi hoạt họa. Không viết "mượt" nếu không có số.
- Cổng 8100.

### Lô 10 – Bản phát hành WebGL (tải nhanh, chơi ổn trên iPhone Safari)
1. **Hồ sơ build Release** (hàm riêng trong `WebBuildPipeline`, không đổi Development): nén **Brotli**, bỏ mã không dùng (Managed Stripping cao nhất an toàn), không bắt ngoại lệ đầy đủ (nếu test vẫn đạt), nén texture phù hợp (ASTC cho thiết bị di động nếu Unity 6 WebGL hỗ trợ; ghi rõ lựa chọn và kết quả), tên tệp chứa mã commit. Máy chủ thử cần header `Content-Encoding: br` đúng: dùng máy chủ tĩnh của Codex (`run_local.ps1`, round29) hoặc `server_no_cache.py` bổ sung `.br` — **không** sửa `tools/audit`. Dán kích thước trước/sau từng tệp và bảng ước tính thời gian tải (xem mục 4).
2. **Màn hình chờ tải** trong mẫu WebGL: logo/nền từ asset, thanh tiến trình thật, câu "Đang tải…", thông báo lỗi tiếng Việt khi trình duyệt không hỗ trợ WebGL/thiếu bộ nhớ, nhắc xoay dọc khi ở chế độ ngang.
3. **Tinh chỉnh trình duyệt di động:** `viewport-fit=cover`, chặn cuộn/kéo làm mới/phóng to/chọn chữ/menu nhấn giữ (`touch-action`, `user-select`, `-webkit-touch-callout`), `100dvh`, mở khóa âm thanh ở lần chạm đầu, giữ màn hình sáng (nếu API có), không tải lại khi đổi kích thước.
4. **Bộ đo:** Playwright với hạn chế mạng (CDP `Network.emulateNetworkConditions` 20 Mbps/5 Mbps) đo thời gian tới màn Home; báo số **đo được trong giả lập**, ghi rõ là giả lập. Cổng 8101.

### N2 (khi Codex báo Z4 xong) và `.jslib`
Lấy `unity_online_glue` + `HitMeWebSocket.jslib`/`WebGLWebSocketTransport` của Codex khi có (nhánh `prep/codex`, xem `server/README.md` mục "Tích hợp Unity"): viết `UnityOnlineUi : IOnlineUi`; chạy hai người bằng `LoopbackTransport`; **chưa** kết nối URL thật. Chưa có bản của Codex → ghi "CHỜ CODEX" và làm tiếp mục khác.

## Phiếu cho chủ dự án
`hit-me-owner-test-sheet-lo1-4.md` đã mở rộng sang Lô 5–8 (cổng 8098/8099). Chủ dự án trả lời bằng mẫu trong phiếu; AG **không** tích hộ.

## Mẫu báo cáo
```
BẢNG CỔNG → HEAD CỔNG → NỘI DUNG: 8093 … 8101
BƯỚC: P | Lô 9 | Lô 10 | N2
ĐÃ LÀM: ...
BẰNG CHỨNG: (docs/UI_EVIDENCE_INDEX.md mục mới; ảnh đã mở xem + 1 dòng nhìn thấy; video; ảnh ghép khung; bảng thời gian; Passed! N/N + tên test; kích thước trước/sau; audit mã thoát)
CHƯA CÓ BẰNG CHỨNG / BỊ CHẶN (chờ ai): ...
CÂU HỎI CHO CHỦ DỰ ÁN (không tích sẵn): ...
URL (ipconfig): ...
```
