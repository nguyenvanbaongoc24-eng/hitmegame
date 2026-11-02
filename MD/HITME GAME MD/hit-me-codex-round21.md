# HIT ME – LỆNH CHO CODEX, ĐỢT 21

> Dán cho Codex, kèm `hit-me-round20-audit-tool-and-ag-final-check.md`, `hit-me-codex-round17.md` (H2), `hit-me-round19-close-phase2-open-phase3.md`. **Commit nhỏ; chỉ báo "pass" khi dán được `Passed! N/N`.** Hết token thì ghi "Đang dở: ..." vào `server/README.md`.

## Đánh giá báo cáo (`60edae0`, `Passed! 59/59`)
- **Y3 (công cụ audit) có mặt, đạt bước đầu:** `tools/audit/phase2_audit.ps1` (~100 dòng) kiểm Build Settings, Core, chuỗi cấm, XML, manifest/sha, build, UI, GC/FPS, tài liệu, xóa tệp; chạy mẫu ra `Evidence/audit/phase2_audit_9ee68bf.txt` với nhiều KHÔNG ĐẠT (có chủ ý).
- **Điều tôi chưa tin:** 12 nhóm kiểm trong ~100 dòng, và lần chạy mẫu nằm trên **worktree của Codex** (nhánh server), không phải dự án Unity của AG. Một công cụ kiểm tra chỉ đáng tin khi **được chứng minh bắt được lỗi**.
- **H2 bắt đầu tốt:** `IAuthVerifier` + `FakeAuthVerifier` (guest token → ID ngẫu nhiên 128 bit do **server cấp**, tên được làm sạch, token thiếu/sai bị từ chối), `FakeAuthVerifierTests`. Đúng: gateway **chưa** bắt buộc `join.token` (commit nền tảng riêng để không làm vỡ giao thức hiện có).

## Y4 – Chứng minh công cụ audit bắt được lỗi (làm đầu tiên)
1. **Kiểm thử phủ định tự động** (`tools/audit/tests/`, chạy bằng `pwsh`/Pester hoặc một tập lệnh): dựng thư mục giả lập một dự án Unity tối thiểu **đạt**, rồi lần lượt tiêm từng lỗi và kiểm công cụ báo KHÔNG ĐẠT **đúng mục đó**:
   - thêm scene thứ hai vào `EditorBuildSettings.asset`, hoặc lặp `Graybox` hai lần;
   - thêm `using UnityEngine;` vào `Core/Phase1`;
   - thêm `OnGUI`, `Instantiate(`, `Shader.Find`, `Tint` vào `Scripts/Game`;
   - XML có `failures > 0`;
   - tệp trong `Evidence/` không có dòng trong `MANIFEST.csv`; hoặc sha256 sai; hoặc commit không tồn tại;
   - sửa 1 byte trong tệp build (sha256 lệch); thiếu `.wasm`; còn `UnityLockfile`;
   - thiếu một ảnh độ phân giải; `ManagedAllocPerFrame > 0`;
   - xóa một tệp trong `ASSET`/`SFX`/`Evidence` (so với commit gốc);
   - thiếu `docs/PHASE2_REPORT.md`.
   Mỗi ca: kỳ vọng mục nào đổi sang KHÔNG ĐẠT và mã thoát khác 0; ca **đạt** thì mã thoát 0. Dán kết quả (số ca đạt/tổng).
2. Với mỗi mục kiểm **không thể kiểm tự động** (phải đọc bằng mắt: ví dụ `Instantiate(` ngoài khởi tạo, số cứng), công cụ chỉ **liệt kê dòng** và ghi rõ "CẦN NGƯỜI XEM", không tự kết luận ĐẠT.
3. **Chạy bản thật, chỉ đọc**, trên **bản chép của dự án Unity của AG** ở commit hiện tại (nhánh `feature/ui-ux-production`, thư mục `D:\HITME GAME\unity-client`; chép sang thư mục tạm bằng `git worktree`/`git archive`, **không** sửa thư mục của AG, **không** mở Unity). Đẩy kết quả `Evidence/audit/phase2_audit_<commit>.txt` lên `prep/codex` và dán **bảng KHÔNG ĐẠT** trong khung chat: đó là **danh sách việc đầu tiên cho AG**.
4. Ghi cách chạy (3 dòng) vào chú thích đầu `phase2_audit.ps1`.

## H2 – Hoàn tất danh tính người chơi
Theo `round17` (H2): bắt buộc `join.token` ở gateway (token thiếu/sai → `error`; thông điệp `join` cũ không có token → lỗi di chuyển rõ ràng như `ready` cũ, có test); `playerId` do server quyết định (tự khai bị bỏ qua); `reconnectToken` ≥ 128 bit, gắn `playerId` + phòng, hết hạn đúng 30 giây; mạo danh bằng `reconnectToken` sai bị từ chối. **Cập nhật README + giao thức trước khi sửa** để Z2 (DTO) dùng bản cuối. Bài quét bí mật 6 kết nối vẫn chạy. `Passed! N/N`.

## Thứ tự tiếp theo (không chờ AG)
**Y4 → H2 → A3 → P4 → A2 → Z1 → Z2 → S5 → Z3** → C2 → R3 → P5 → P6 → P7. X5 chờ AG báo xong.

## Báo cáo
```
VIỆC: Y4 | H2 | A3 ...
ĐÃ LÀM: ...
BẰNG CHỨNG: (lệnh + "Passed! N/N" / bảng ca phủ định)
KHÔNG ĐẠT / PHÁT HIỆN QUAN TRỌNG: ...
CHỜ AG / CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```

## Điều cấm (nhắc lại)
Không mở/sửa Unity và `unity-client/Assets` trên `prep/codex`; không sửa thư mục của AG hay `Builds`; không commit lên `main`; không viết lại lịch sử git; không đổi luật; không sửa Core; không triển khai Render/Supabase thật; không đưa bí mật/khóa thật vào mã; không tạo .md ngoài `server/README.md`; không báo "pass" khi thiếu kết quả test.
