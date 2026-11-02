# HIT ME – LỆNH CHO AG, ĐỢT 25: chốt Phase 2 bằng bằng chứng kiểm lại được, rồi Phase 3 Bước 0

> Dán cho AG, kèm `hit-me-room-size-rule-v1.3.md` (bản sửa 30/10, **phòng online không có bot**), `hit-me-ag-remaining-work-v2.md`, `hit-me-gameplay-rules-v1.2.md`. **Chưa gắn tag `v0.2-rc1`.** Chỉ báo "đạt" khi dán được lệnh + kết quả; mục nào không làm được thì ghi "CHƯA LÀM" kèm lý do.

## Đánh giá báo cáo S1–S11
**Ghi nhận:** có build mới `WebGL_20261030_6e0da22` trên cổng 8091 (giữ 8090), MANIFEST sinh bằng công cụ (446 tệp, missing/badHash/stale = 0), HP card công khai mọi pha, lobby N=1 khóa Start, ảnh HUD/kết quả N=2..6, tài liệu `docs/`.

**Chưa tin được, cần xử lý trước khi chốt (ngắn, theo thứ tự):**
1. **Số test mâu thuẫn:** commit nói `86/86`, báo cáo nói `92/92`, audit nói `11/11 file XML`. Lập **bảng đếm test**: mỗi file XML → số test, pass, fail, skip, lệnh chạy, commit. Một con số tổng duy nhất, khớp với lần chạy mới nhất (chạy lại EditMode ở HEAD, dán `Passed! N/N`).
2. **Bạn đã sửa công cụ audit của Codex** (`phase2_audit.ps1`, `manifest_gen.ps1`: "BOM, đường dẫn, lọc danh mục audit/") rồi tự báo 12/12 ĐẠT. Điều này **không độc lập**. Làm: (a) đưa bản sửa thành **một commit riêng chỉ chứa thay đổi công cụ** kèm `git diff` so với bản Codex (`prep/codex @ 2612c77`); (b) không được nới tiêu chí; (c) Codex sẽ chạy lại bộ kiểm thử phủ định trên bản đã sửa (X5). Các mục "Số cần rà soát = 0" và "Chuỗi cần người xem = không vi phạm" trước đây công cụ ghi **CẦN NGƯỜI XEM**; giải thích vì sao nay thành ĐẠT (đã sửa mã hay đã đổi luật kiểm?), dán danh sách dòng cũ đã xử lý.
3. **S6 (0 B/frame, 60 FPS):** nói rõ **cách đo**: bản Release WebGL không có Profiler; con số `ManagedAllocPerFrame` lấy từ đâu (bản Development? bộ đếm riêng trong mã? `performance.memory`?). 60.01 FPS trung bình trong trình duyệt tự động hóa thường chỉ là khóa vsync, không chứng minh gì về điện thoại. Dán: lệnh/mã đo, thiết bị/trình duyệt/độ phân giải, thông số khóa FPS, và tệp thô. **Nếu chỉ đo trên PC: ghi rõ "chưa đo trên điện thoại"**; phép đo điện thoại thuộc bước S5 của chủ dự án (bổ sung hướng dẫn đọc FPS trên điện thoại, xem mục 6).
4. **Build ↔ HEAD:** build gắn `6e0da22`, nhưng HEAD là `f84ed43`. Chạy `git diff --stat 6e0da22..HEAD -- unity-client/Assets unity-client/ProjectSettings` và dán: nếu có thay đổi mã/asset thì **phải build lại**; nếu chỉ docs/Evidence/tools thì ghi rõ để Codex đối chiếu.
5. **Ảnh bằng chứng:** mỗi ảnh HUD N=2..6 và 5 độ phân giải: chụp từ **Editor hay từ build WebGL**? Provenance `.json` phải ghi đúng (`scene`, `bản build`, `lệnh tạo`). Ảnh Editor không được ghi là ảnh build.
6. **S2 (10 trận Playwright):** dán đường dẫn log/`json` thô và tệp `verify_webgl.mjs`; ghi rõ "không rớt khung" đo bằng gì.

## Việc tiếp theo cho AG

### A. Chế độ "Luyện với bot" ở menu (luật v1.3 bản sửa, **không** thuộc phòng online)
Báo cáo chỉ nói lobby "mở nút Luyện tập với Bot". Xác nhận/hoàn tất:
1. Màn riêng ở menu: chọn **số bot 1–5 (mặc định 3)** và nhân vật; chạy cục bộ (`Core/Phase1` + `LocalBot`), **không** gọi server/mạng.
2. Đúng luật v1.2 (cử chỉ, 5 giây, bot không thấy vị trí người chơi trước khóa và ngược lại, `ThrowFacing` mặc định lên).
3. `AutoPlay` 10 trận cho **mỗi** số người N = 2..6: 0 lỗi, 0 treo, nhật ký có kết quả từng trận.
4. Lobby/phòng online: hiển thị `người/6`, chỗ trống "Đang chờ…", khi 1 người: Start khóa + câu "Cần ít nhất 2 người. Muốn chơi một mình, hãy sang menu Luyện với bot." + nút sang Luyện với bot. Ảnh 1080×2340 cho trạng thái này.
5. Xóa mọi chỗ còn ngụ ý "bot trong phòng online" ở UI/tài liệu. **Sửa `docs/01_GAMEPLAY_RULES.md`** nếu còn chép bản v1.3 cũ: phòng online chỉ người thật, ≥ 2 mới bắt đầu, ghép nhanh chỉ gợi ý bot sau 30 giây (không tự thêm), Luyện với bot là chế độ riêng. Dán `git diff` của tệp.
6. Câu "N=2..6 người chơi **thật**" trong báo cáo dễ gây hiểu lầm: ghi rõ ảnh/test N=2..6 của Graybox mô phỏng bằng cái gì (bot cục bộ hay danh sách giả).

### B. S8 – bảng đối chiếu luật ↔ test và S9 – danh sách tồn đọng
- Bảng ngắn (cột: luật v1.2/v1.3, test client, kết quả); luật chưa có test thì liệt kê, không tự nhận đạt.
- Danh sách tồn đọng đưa cho chủ dự án duyệt, mỗi dòng: việc, lý do hoãn, mức ảnh hưởng. Phải có: dép bị mất đầu (W1–W3, `hit-me-weapon-sprite-fix.md`, chủ dự án đã cho hoãn), mọi thứ chưa đo trên điện thoại, phần chưa nối mạng.

### C. Phase 3 Bước 0 (chỉ rà soát, chưa code UI mới)
Tạo `docs/PHASE3_STEP0.md` (một tài liệu): danh sách màn hình theo luồng (Đăng nhập → Sảnh → **Phòng online** (tạo/vào bằng mã, 1–6 người, bắt đầu khi ≥ 2, ghép nhanh) → Trận; **Luyện với bot** một luồng riêng; Cửa hàng/Túi đồ/Hồ sơ), mỗi màn: mục đích, dữ liệu cần từ server (đối chiếu giao thức của Codex trong `server/README.md`: `join.token`, `room_state`, `room_ready`, `start`, `lock_ready`, `emote`, `quick_match_hint`, `not_enough_players`), asset có sẵn trong `ASSET HITME FINAL\HITME_UI_KIT_CLEAN` và asset thiếu. Cuối tài liệu: **câu hỏi mở cho chủ dự án** (ví dụ Luyện với bot có tính vàng/xếp hạng không, mặc định "không"; luật chọn nhân vật trong phòng; ai là host khi host rời).

### D. Hướng dẫn thử điện thoại cho chủ dự án (S5)
Viết 10 dòng trong `docs/PHASE2_REPORT.md`: URL `http://172.20.10.3:8091/` (ghi chú: cùng Wi-Fi/điểm phát sóng, IP có thể đổi), 3 trận cần chơi, 8 điều cần quan sát (chạm đặt vị trí; giữ-trượt ngắm; chạm chỗ khác = làm lại hoàn toàn; kéo về dưới 4 mm = hủy ngắm; không ngắm = ném thẳng; thẻ HP không bị rào che; dọn vũ khí cuối trận; Replay/Về sảnh), cách đọc FPS (nếu có bộ đếm trong build, bật bằng cách nào) và mẫu báo kết quả để chủ dự án điền.

## Không làm trong đợt này
Không gắn `v0.2-rc1`; không sửa asset dép; không viết mã mạng (Codex đang làm `net_client_v1`; khi Codex báo SERVER READY mới nối); không đổi luật; không xóa tệp trong `ASSET`/`SFX`/`Evidence`.

## Mẫu báo cáo
```
VIỆC: mục (ví dụ 1 bảng đếm test | A.3 AutoPlay | C Phase 3 Bước 0)
ĐÃ LÀM: ...
BẰNG CHỨNG: (lệnh + kết quả "Passed! N/N" / đường dẫn tệp thô)
CHƯA LÀM / KHÔNG ĐẠT: ...
CHỜ CHỦ DỰ ÁN / CODEX: ...
COMMIT: <hash>
```
