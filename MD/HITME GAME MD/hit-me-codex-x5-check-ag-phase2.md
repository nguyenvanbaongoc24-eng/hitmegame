# HIT ME – CODEX X5: kiểm độc lập báo cáo Phase 2 của AG (chỉ đọc)

> Chạy xen kẽ với round24, **không sửa** thư mục của AG. Chép bản AG bằng `git worktree`/`git archive` ở `feature/ui-ux-production @ f84ed43` (hoặc HEAD mới nhất AG báo).

1. **Công cụ audit bị AG sửa:** so `tools/audit/phase2_audit.ps1` và `manifest_gen.ps1` bản AG với bản của bạn (`prep/codex @ 2612c77`). Liệt kê từng thay đổi: nới tiêu chí hay chỉ sửa tương thích? Chạy lại bộ kiểm thử phủ định (14 ca) trên **bản AG**, dán `Passed! N/N`. Hai mục trước đây "CẦN NGƯỜI XEM" (số cứng ngoài `GameConfig`, chuỗi cần người xem) nay ĐẠT: tự liệt kê các dòng khớp mẫu trong `Scripts/Game` và `Scripts/UI` và kết luận.
2. **Chạy audit và `manifest_gen.ps1 -Check`** trên bản chép: ĐẠT/KHÔNG ĐẠT từng mục, không dùng kết quả AG.
3. **Build `WebGL_20261030_6e0da22`:** sha256 từng tệp so với MANIFEST; fingerprint trong `index.html` ↔ commit; `git diff --stat 6e0da22..HEAD -- unity-client/Assets unity-client/ProjectSettings` (nếu có đổi mã thì báo "build cũ hơn mã").
4. **Số test:** đếm `<test-case>` trong từng XML AG nộp; so với `86/86`, `92/92` AG nêu; báo tổng, số trùng lặp giữa file.
5. **Xếp hạng:** đối chiếu `Core/Phase1/MatchRanking.cs` với `ServerMatch` trên `rounds.json` (nếu P4 xong) hoặc vector tay N=2..6 (đồng hạng, người bị loại, tiebreak). Lưu ý AG có thêm 3 cơ chế tiebreak (`HighestHp`, `SharedWin`, `ContinueUntilElimination`): luật chốt là HP → số lần trúng → đồng hạng; cái nào không thuộc luật thì báo.
6. **Đo GC/FPS:** đọc `Evidence/phase3/gc_60s_profile_report.txt` và mã đo; kết luận phép đo có hợp lệ cho bản Release WebGL không.
7. **Ảnh/provenance:** lấy mẫu 10 ảnh, đối chiếu `.json` (nguồn Editor hay build).

Báo cáo theo mẫu: VIỆC / ĐÃ LÀM / BẰNG CHỨNG / KHÔNG ĐẠT / COMMIT. Chỉ đọc, không sửa.
