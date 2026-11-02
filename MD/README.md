# HIT ME — Rebuild Documentation

Đây là bộ tài liệu đã chuyển sang project rebuild HIT ME.

- Root project duy nhất: `D:\HITME GAME`.
- Master điều phối duy nhất: [HITME_MASTER_PROJECT_V1.md](HITME_MASTER_PROJECT_V1.md).
- Báo cáo chuyển đổi và các mục cần xác minh: [MD_MIGRATION_REPORT.md](MD_MIGRATION_REPORT.md).

## Thứ tự đọc

1. Đọc master trước để lấy luật gameplay, phạm vi, ownership và trạng thái đã xác minh.
2. Đọc `coordination/WORK_OWNERSHIP.md` và `coordination/GAMEPLAY_VISUAL_CONTRACT.md` trong đúng worktree đang làm.
3. Dùng các file trong `docs` của repository/worktree làm tài liệu chuyên đề: bootstrap, asset manifest, handoff, WebGL hoặc báo cáo visual.
4. File prompt hoặc báo cáo cũ chỉ được sử dụng sau khi đối chiếu với master, HEAD hiện tại và runtime thực tế. Một PASS trong báo cáo cũ không tự động là PASS của build hiện tại.

## Quy tắc ưu tiên

Khi tài liệu mâu thuẫn, yêu cầu hiện tại của người dùng được ưu tiên. Master này đứng trên tài liệu chuyên đề; tài liệu chuyên đề đứng trên prompt cũ đã rà soát. Mâu thuẫn chưa giải quyết phải được đánh dấu **CẦN XÁC MINH** và ghi vào báo cáo, không tự chọn một phương án.

Không có file nào khác trong thư mục này được tuyên bố là master thứ hai.
