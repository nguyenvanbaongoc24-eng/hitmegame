# HIT ME – ĐỢT 20: Codex không chờ AG; AG rà soát toàn bộ rồi chốt Phase 2

## Quyết định
**Codex không cần chờ AG.** Hai bên làm hai mảng độc lập (Codex: server ngoài Unity; AG: client trong Unity). Chỉ **X5** (kiểm độc lập bước cuối) phải chờ AG xong. Để việc "AG check toàn bộ" có **thước đo khách quan** (không còn AG tự chấm), Codex viết sẵn **một công cụ kiểm tra tự động** mà AG chạy và Codex chạy lại ở X5.

---

## PHẦN A – CODEX (không chờ AG)

### Ghi nhận báo cáo H1 (`44d6a42`, `35870fb`, `9ee68bf`, `Passed! 53/53`)
Đạt: khung WebSocket 1 KB, giới hạn IP/origin/phòng, keep-alive 30 giây, idle timeout cấu hình được, giữ chỗ vào lại 30 giây, `server_shutdown`, nhật ký không ghi commit/tọa độ/token. **Chưa chốt:** thiếu bằng chứng mạng cho ping/timeout/`SIGTERM` vì môi trường chặn chạy tiến trình nền.

### Y2 – Chốt H1 mà không cần tiến trình nền
Kiểm **trong tiến trình test**, không khởi chạy server riêng:
1. Chạy server bằng host trong tiến trình (`WebApplicationFactory`/`TestServer` hoặc Kestrel trên cổng tạm ngay trong test; client là `ClientWebSocket` hoặc `WebSocketClient` của test host).
2. Idle timeout: `TimeProvider` giả, không gửi gì quá `idleTimeoutSeconds` → kết nối bị đóng; người trong phòng vẫn được giữ chỗ 30 giây.
3. Ping/pong: client không trả pong đến hạn → đóng; client trả pong → còn sống.
4. `SIGTERM`: gọi `IHostApplicationLifetime.StopApplication()` (hoặc cơ chế tương đương) → mọi client nhận `server_shutdown` rồi bị đóng sạch, không treo.
5. Nhật ký: bắt đầu ra nhật ký trong test, chạy một vòng có `commit` mang giá trị dấu vết (`3.14159`, `-2.71828`…), **xác nhận nhật ký không chứa** chúng (kể cả dạng làm tròn) và không chứa token.
Dán `Passed! N/N`; nếu môi trường vẫn cản, ghi rõ chỗ nào KHÔNG XÁC ĐỊNH thay vì báo đạt.

### Y3 – Công cụ kiểm tra Phase 2 tự động (mới, quan trọng)
Viết `tools/audit/phase2_audit.ps1` (PowerShell; chạy được trên Windows của chủ dự án; **chỉ đọc**, không sửa gì; tham số: `-RepoRoot`, `-BuildDir`; xuất bảng ĐẠT/KHÔNG ĐẠT và mã thoát khác 0 nếu có KHÔNG ĐẠT; ghi kết quả ra `Evidence/audit/phase2_audit_<commit>.txt`). Kiểm:
1. **Build Settings** (`ProjectSettings/EditorBuildSettings.asset`) chỉ có `Graybox.unity`, không trùng.
2. Scene `Graybox` có `MatchController`, **không** có component `MatchStateMachine`.
3. `Core/Phase1`: không `using UnityEngine`; asmdef `noEngineReferences`.
4. **Chuỗi cấm** trong `Assets/Scripts/Game` và `Assets/Scripts/UI`: `Tint`/`magenta`, `OnGUI`, `Instantiate(`/`Destroy(` (ngoài khởi tạo — liệt kê dòng, người đọc kiểm), `Shader.Find`, `new Material`, `pet`/`thú cưng`.
5. **Số cứng** ngoài `GameConfig`: tìm `0.45`, `0.55`, `5f`, `9.0`, `11.4` (liệt kê, không kết luận cứng).
6. **Kết quả test:** đọc mọi `test-results*.xml` trong `Evidence`, in bảng `tệp | tổng | pass | fail | skip`, báo KHÔNG ĐẠT nếu có fail hoặc nếu tệp mới nhất không phải bản có số test lớn nhất.
7. **MANIFEST:** mọi tệp (trừ chính `MANIFEST.csv`) dưới `Evidence/` có dòng trong `Evidence/MANIFEST.csv`; sha256 trong dòng khớp tệp thật; cột `commit` là commit tồn tại (`git cat-file -e`); tệp có giờ ghi **trước** thời điểm commit ghi trong dòng thì đánh dấu cảnh báo.
8. **Thư mục build** (`-BuildDir`): đủ 5 tệp, giờ ghi trong khoảng 10 phút, sha256 khớp MANIFEST, `index.html` chứa dòng dấu vân tay/biến build; không có `UnityLockfile` thừa; log build có "Build completed ... Succeeded".
9. **Bằng chứng UI/độ phân giải:** có đủ ảnh `res_1080x1920`, `res_1179x2556`, `res_1080x2400`, `res_1080x2520`, `res_1620x2160` cho **mỗi** nội dung UI trong danh sách (kèm tệp provenance .json cùng commit hiện tại).
10. **GC/FPS:** tệp đo hiệu năng WebGL có dòng `ManagedAllocPerFrame` (cảnh báo nếu > 0) và FPS trung bình/thấp nhất, thời lượng đo ≥ 60 giây.
11. Không tệp nào trong `ASSET`, `SFX`, `Evidence` bị xóa so với thẻ `v0.1-graybox`/commit gốc (`git diff --diff-filter=D`).
12. Tài liệu: `docs/PHASE2_REPORT.md`, `docs/01_GAMEPLAY_RULES.md` tồn tại **trong repo**, mỗi tài liệu có đúng một bản.
Kèm ví dụ chạy và kết quả mẫu trên commit hiện tại (nhiều mục sẽ KHÔNG ĐẠT — đó là điều mong đợi, dùng để AG sửa). Đẩy lên `prep/codex`; **AG lấy về dùng**.

### Rồi tiếp tục (không chờ AG)
**H2 → A3 → P4 → A2 → Z1 → Z2 → S5 → Z3** → C2 (SQL) → R3 → P5 → P6 → P7 (chi tiết `round19`, `round17`, `round15`, `round14`, `round12`). **X5** làm khi AG báo xong: chạy `phase2_audit.ps1` trên commit/bản build cuối của AG + đọc lại các mục thủ công.

---

## PHẦN B – ANTIGRAVITY: rà soát toàn bộ rồi chốt Phase 2

> Dán cùng `hit-me-round19-close-phase2-open-phase3.md` (phần B, mục 1–11 và Phase 3 Bước 0). **Quy tắc:** commit nhỏ; mục KHÔNG ĐẠT/CÒN THIẾU phải điền thật; không báo "đạt" khi thiếu bằng chứng; không build đè thư mục đang thử; không sửa logic combat `Core/Phase1`.

### Cách làm (theo thứ tự)
1. **Lấy công cụ kiểm tra** `tools/audit/phase2_audit.ps1` từ `prep/codex` (Codex đang viết; nếu chưa có thì làm các bước 2–6 trước rồi chạy khi có). Chạy trên commit hiện tại, **lưu kết quả đầu tiên** làm "danh sách việc" (đừng sửa tệp báo cáo của công cụ).
2. **Rà soát toàn bộ** theo `hit-me-round19-close-phase2-open-phase3.md` phần B mục 1–11 và ghi lại từng mục: đã đạt / chưa đạt / cần chủ dự án quyết.
3. **Sửa lần lượt:** (a) 4 lỗi UI (cuối trận → bảng kết quả, HUD 6 người, canvas portrait, nhãn vũ khí) có ảnh trước/sau ở 5 độ phân giải; (b) 460 B/khung hình → 0 (nêu nguồn, đo 60 giây trên bản WebGL, ghi FPS); (c) D0 còn thiếu (thước-đường bay, độ chính xác chạm, bảng nhân vật ↔ `ASSET HITME FINAL`, ảnh cận cảnh chân, ảnh trước/sau `ArenaView`); (d) **bảng số test duy nhất** từ XML thật giải thích 51/52/59/62/72; (e) **MANIFEST đủ dòng** cho mọi tệp bằng chứng.
4. **Build sạch mới** (thư mục mới, dấu vân tay, MANIFEST), AutoPlay 10 trận trên bản build, **không** ghi đè bản `942ee08` cho tới khi chủ dự án thử xong.
5. **Chạy lại `phase2_audit.ps1`** trên commit cuối và bản build mới: **mọi dòng phải ĐẠT**, trừ những dòng AG đã liệt kê vào "tồn đọng chuyển sang phase sau" (dép cụt, 5 nhân vật pose tĩnh, sprite trích từ ảnh bảng, cảnh báo nullable Core, `MainMatch` legacy chưa dọn) và đã được chủ dự án đồng ý. Dán toàn bộ bảng kết quả trong báo cáo.
6. **Cập nhật tài liệu** `docs/PHASE2_REPORT.md` và `docs/01_GAMEPLAY_RULES.md` (v1.2); khi chủ dự án xác nhận đã thử điện thoại **và** Codex X5 đạt thì gắn tag **`v0.2-rc1`**, báo commit.
7. **Song song, ngay từ đầu:** Phase 3 Bước 0 (chỉ rà soát, không code), dừng chờ duyệt.

### Báo cáo cuối (một lần, điền thật)
```
CHẶNG: Phase 2 – chốt
KẾT QUẢ phase2_audit.ps1: (dán bảng; đường dẫn tệp kết quả)
ĐÃ SỬA: (danh sách + commit)
TỒN ĐỌNG CHUYỂN SANG PHASE SAU: (mức ảnh hưởng, phase đề xuất)
KHÔNG ĐẠT / CÒN THIẾU: (thật)
BẢN BUILD: (thư mục, dấu vân tay, MANIFEST)
CHỜ CHỦ DỰ ÁN: thử điện thoại, duyệt tồn đọng
COMMIT: <hash>
```

## PHẦN C – CHỦ DỰ ÁN
- Dán Phần A cho Codex, Phần B cho AG.
- Khi AG đưa bản build mới: thử 3 trận (8 điều ở `round19` phần C), báo tôi.
- Duyệt danh sách tồn đọng khi AG nộp.

## Điều cấm (nhắc lại)
Codex: không mở/sửa Unity và `unity-client/Assets` trên `prep/codex`, không sửa `Builds`, không triển khai Render/Supabase thật, không tạo .md ngoài `server/README.md`. AG: không sửa logic combat `Core/Phase1`, không xóa `ASSET`/`SFX`/`Evidence`/`MD`/`ASSET HITME FINAL`, không viết lại lịch sử git, không tạo .md ngoài `docs/PHASE2_REPORT.md` và `docs/01_GAMEPLAY_RULES.md`. Cả hai: không báo "đạt/pass" khi thiếu bằng chứng.
