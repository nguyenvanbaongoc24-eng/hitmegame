# HIT ME – LỆNH CHO CODEX, ĐỢT 18

> Dán cho Codex, kèm `hit-me-codex-round17.md` (A3, P4, A2, H1, H2, Z1, Z2, S5, Z3, R3, P5, P6, P7). **Commit nhỏ; chỉ báo "pass" khi dán được `Passed! N/N`.** Hết token thì ghi "Đang dở: ..." vào `server/README.md`.

## Đánh giá trả lời vừa rồi
- **A1 đạt:** `tools/patches/gesture_v2_sync/`, 12 replay kiểm từng bước, NUnit ngoài Unity `Passed! 5/5`; AG đã đồng bộ ở `a7b2574` và báo 72/72 EditMode.
- **B1 đạt, đủ ca:** `ready` cũ trả lỗi rõ `legacy_ready_use_room_ready_or_lock_ready` (có `LegacyReadyReturnsExplicitMigrationError`); đủ các ca đếm ngược/hủy/host `start`/sai pha/vào muộn/6 người; `Passed! 43/43` (`8cb7e0b`).
- Hai chỗ chỉ ghi "test sai pha" mà chưa nêu tên test (`room_ready` trong trận, `lock_ready` ở phòng chờ): ghi tên test trong lần báo cáo sau.

## X4 – Kiểm độc lập đồng bộ của AG (chỉ đọc, ngắn, làm đầu tiên)
Số "72/72" là AG tự báo. Kiểm trên commit `a7b2574`:
1. `Test16` trong Unity có dùng bản kiểm **từng bước** của A1 (không phải kiểm trạng thái cuối): đọc mã, dán 3 dòng `Assert` tiêu biểu.
2. Đủ **12 replay** trong thư mục dữ liệu của Unity, trùng nội dung (sha256) với `tools/patches/gesture_v2_sync/replays`.
3. Số test tăng từ 59 → 72 (+13): liệt kê các test mới cho thấy phép cộng khớp (12 replay + 1 test khác?). Nếu không khớp, nêu chênh lệch.
4. `test-results*.xml` mới của AG trong `Evidence/` có dòng 72/72 và `commit` đúng `a7b2574`; `MANIFEST.csv` đã có dòng cho tệp đó.
5. Bản build WebGL `942ee08` **không** bị đụng (thư mục `Builds/WebGL_20261029_942ee08` còn nguyên sha256).
Báo ĐẠT / KHÔNG ĐẠT từng mục, chỉ đọc, không sửa.

## Rồi tiếp tục theo thứ tự
**A3 → P4 → A2 → H1 → H2 → Z1 → Z2 → S5 → Z3** → R3 → P5 → P6 → P7. (Chi tiết ở `round17`, `round15`, `round14`, `round12`.) Mỗi gói một commit có `Passed! N/N`.

## Báo cáo
```
VIỆC: X4 | A3 | P4 | A2 | H1 ...
ĐÃ LÀM: ...
BẰNG CHỨNG: (lệnh + "Passed! N/N" / bảng)
KHÔNG ĐẠT / PHÁT HIỆN QUAN TRỌNG: ...
CHỜ AG / CHỦ DỰ ÁN: ...
NHÁNH/COMMIT: prep/codex @ <hash>
```

## Điều cấm (nhắc lại)
Không mở/sửa Unity và `unity-client/Assets` trên `prep/codex`; không sửa thư mục `Builds`; không commit lên `main`; không viết lại lịch sử git; không đổi luật; không sửa Core; không triển khai Render/Supabase thật; không đưa bí mật/khóa thật vào mã; không tạo .md ngoài `server/README.md`; không báo "pass" khi thiếu kết quả test.
