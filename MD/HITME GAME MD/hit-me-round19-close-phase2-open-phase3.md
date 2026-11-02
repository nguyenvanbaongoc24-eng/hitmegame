# HIT ME – ĐỢT 19: đóng Phase 2 nhanh, mở Phase 3 (Codex + AG + chủ dự án)

> Mỗi bên dán phần của mình. **Commit nhỏ; chỉ báo "đạt/pass" khi có bằng chứng (số đo, ảnh, log, `Passed! N/N`).** Hết token thì ghi "Đang dở" (Codex vào `server/README.md`, AG vào báo cáo).

## Nhận xét báo cáo Codex (X4 + H1 một phần, `438ad5b`)
- **X4:** `Test16` dùng kiểm từng bước (đạt); 12 replay trong Unity khớp SHA-256 với bản vá (đạt); build `942ee08` còn nguyên 5 tệp, sha256 khớp 5/5 (đạt).
- **Hai điểm KHÔNG ĐẠT, đều thuộc về AG:**
  1. **Số test không khớp.** AG từng báo 51 → 52 → 59 → 72; Codex đếm XML thật: **62 → 72 (+10)**, không phải 59 → 72 (+13). Chưa có một bảng số test duy nhất đối chiếu được.
  2. **`MANIFEST.csv` thiếu dòng** cho `test-results-p0.xml` và commit `a7b2574`, trái quy tắc F4 (mọi tệp bằng chứng phải có dòng).
- **H1 (một phần):** giới hạn 1 KB bằng bộ đệm cố định, giới hạn kết nối/IP, allow-list `Origin`, giới hạn số phòng; `Passed! 50/50`. Còn: ping/pong + timeout kết nối chết, `SIGTERM` có thông báo, nhật ký cấu trúc không lộ `commit`.

## Cổng đóng Phase 2 (để sớm sang Phase 3)
Phase 2 đóng và gắn `v0.2-rc1` khi **đủ cả 4 điều kiện**:
1. Chủ dự án thử bản mới trên điện thoại: cử chỉ dùng được, dép bay đúng theo thước (phần C).
2. AG xong danh sách đóng Phase 2 (phần B), có bằng chứng.
3. Codex xác nhận độc lập X5 (phần A mục 1).
4. Không còn mục KHÔNG ĐẠT nào mở **ngoài** danh sách "tồn đọng đưa sang phase sau" đã được chủ dự án đồng ý (xem phần B mục 9).
Trong lúc chờ điều kiện 1, **AG được bắt đầu Phase 3 Bước 0 (chỉ rà soát, không code)** để không phí thời gian.

---

## PHẦN A – CODEX
1. **X5 (làm khi AG báo xong phần B):** chạy lại 4 mục KHÔNG ĐẠT/lưu ý: số test (một bảng duy nhất đối chiếu từ XML trên đĩa, có commit), `MANIFEST.csv` đủ dòng mọi tệp trong `Evidence` (so với `git ls-files`), UI cuối trận/HUD/canvas có ảnh ở 5 độ phân giải trong MANIFEST, 460 B/khung hình đã đo lại. ĐẠT/KHÔNG ĐẠT từng mục, chỉ đọc.
2. **Hoàn tất H1:** ping/pong + `idleTimeoutSeconds` đóng kết nối chết (vẫn giữ chỗ 30 giây cho người trong phòng); `SIGTERM` → `server_shutdown` + đóng sạch; nhật ký cấu trúc (`roomId`, `playerId` rút gọn, `type`, lỗi) **không** chứa vị trí/hướng/`commit` trước khóa và không chứa token; test mạng tương ứng (kể cả bài quét dấu vết trong nhật ký). `Passed! N/N`.
3. **Rồi tiếp tục:** A3 → P4 → A2 → H2 → Z1 → Z2 → S5 → Z3 (chi tiết `round17`, `round15`, `round14`, `round12`).
4. **C2 (mới, nếu còn token): kiểm các bản nháp SQL Supabase** (Việc C cũ: tài khoản, vàng, túi đồ, xếp hạng, cấm AFK) bằng PostgreSQL cục bộ không cần Docker (ví dụ PGlite qua npm, hoặc Postgres cài sẵn): chạy được từ đầu đến cuối, có **RLS** từng bảng (người chơi chỉ đọc/ghi dữ liệu của mình, bảng vàng/xếp hạng chỉ ghi qua hàm máy chủ), không có khóa/bí mật trong tệp; ghi kết quả vài dòng vào `server/README.md` (chưa triển khai thật).

## PHẦN B – ANTIGRAVITY: danh sách đóng Phase 2 (mỗi mục có bằng chứng, ghi vào MANIFEST)
1. **Cuối trận:** `MatchEnd` chuyển chắc chắn sang bảng kết quả trận (xếp hạng, HP, số lần trúng), ẩn nút SẴN SÀNG; test điều khiển trạng thái (vòng cuối → MatchEnd → bảng kết quả, không quay lại pha đặt).
2. **HUD 6 người** không chồng lên cổng sân, tên + HP đọc được trên điện thoại thật.
3. **Canvas** neo trong vùng portrait 1080×2340 (Safe Area), nền ngoài đặt màu tối, kiểm cả khi cửa sổ rộng.
4. **Nhãn vũ khí** gom sát sân, trong Safe Area, không bị thanh gạch dưới iPhone che.
   (Mục 1–4: ảnh trước/sau ở 1080×1920, 1179×2556, 1080×2400, 1080×2520, 1620×2160.)
5. **460 B/khung hình → 0:** nêu nguồn cấp phát, sửa, đo lại 60 giây trên bản WebGL (ghi FPS trung bình/thấp nhất, môi trường).
6. **D0 còn thiếu:** thước ngắm trùng đường bay (góc lệch ≤ 3°, 8 hướng × 3 vị trí), độ chính xác chạm ≤ 4 px, mô tả biến đổi tọa độ, bảng nhân vật ↔ thư mục `ASSET HITME FINAL` ↔ trạng thái, ảnh cận cảnh chân 5 nhân vật pose tĩnh (nền xanh lá và magenta), ảnh trước/sau của `ArenaView.cs`.
7. **Bảng số test duy nhất:** `lần | commit | tệp XML | tổng | pass`, từ XML thật; giải thích 51/52/59/62/72.
8. **`MANIFEST.csv` đủ dòng** cho mọi tệp trong `Evidence/phase2`, `phase3` (kể cả `test-results-p0.xml`, `a7b2574`); chạy tập lệnh tự kiểm "tệp trong `Evidence` mà không có dòng MANIFEST" (kết quả phải rỗng).
9. **Danh sách tồn đọng chuyển sang phase sau** (chủ dự án duyệt): dép bay bị dẹt/cụt phần đầu (so khung gốc `ASSET WEAPONS\DÉP\sprites`), 5 nhân vật chỉ pose tĩnh, sprite trích từ ảnh bảng cần thay bằng tệp gốc trong `ASSET HITME FINAL`, cảnh báo nullable của Core, `MainMatch`/`MatchStateMachine` legacy chưa dọn. Mỗi dòng: mức ảnh hưởng, phase đề xuất.
10. Build WebGL sạch mới (thư mục mới, dấu vân tay, MANIFEST, quy tắc đợt 10), AutoPlay 10 trận trên bản build, rồi báo.
11. **Sau khi chủ dự án thử và Codex X5 đạt:** cập nhật `MD/PHASE2_REPORT.md` và `MD/01_GAMEPLAY_RULES.md` (v1.2) **nằm trong repo** (`docs/`), gắn tag **`v0.2-rc1`**, báo commit để Codex rebase server.

### Phase 3 Bước 0 (làm ngay, **chỉ rà soát, không code**, dừng chờ duyệt)
Theo `hit-me-ag-close-phase2-open-phase3.md` (Bước 0): bảng đối chiếu 3 mockup (Menu chính, Vào phòng, Biểu cảm) với asset hiện có trong `ASSET HITME FINAL` (`HITME_UI_KIT_CLEAN`, `UI_KIT_EXTRACTED`, `HITME_AIM_KIT_V1`…); wireframe chữ từng màn hình; **dịch vụ giả sau interface** (`IAuthService`, `IProfileService`, `IRoomService`, `IInventoryService`; bản giả giữ dữ liệu trong bộ nhớ, **khớp thông điệp** của `server/README.md`: `join` có `token`, `room_ready`, `start`, `lock_ready`, `emote`); quy tắc bí mật biểu cảm; quy tắc responsive/Safe Area; danh sách câu hỏi cho chủ dự án; kế hoạch bước 1. Chờ duyệt.

### Mẫu báo cáo (điền thật, **không** để "Không có" khi còn tồn đọng)
```
BƯỚC: B1..B11 | P3-B0
ĐÃ LÀM: ...
BẰNG CHỨNG: (đường dẫn + số đo + dòng MANIFEST)
KHÔNG ĐẠT / CÒN THIẾU: (thật)
CHỜ CHỦ DỰ ÁN: ...
COMMIT: <hash>
```

## PHẦN C – CHỦ DỰ ÁN
1. Thử bản **mới nhất AG đưa** (kiểm dòng dấu vân tay ở góc màn hình) 3 trận, 7 điều như `hit-me-round13-owner-test-and-ag.md` phần 1; thêm điều 8: cuối trận có sang bảng kết quả không.
2. Gửi tôi: số bản, thiết bị + trình duyệt, đạt/không đạt từng điều, ảnh/quay màn hình nếu có.
3. Duyệt danh sách tồn đọng ở mục B9 (đồng ý chuyển sang phase sau, hoặc yêu cầu làm ngay mục nào).
4. Xác nhận hai quy tắc mặc định: **tự bắt đầu phòng sau 3 giây khi tất cả sẵn sàng** (host vẫn `start` sớm được), và **AFK = 2 vòng liên tiếp không `commit`, bị loại và cấm 120 giây**.

## Điều cấm (nhắc lại)
Không mở/sửa Unity và `unity-client/Assets` trên `prep/codex` (Codex); không sửa thư mục `Builds` đang thử (cả hai); không sửa logic combat `Core/Phase1`; không xóa `ASSET`/`SFX`/`Evidence`/`MD`/`ASSET HITME FINAL`; không viết lại lịch sử git; không triển khai Render/Supabase thật; không đưa bí mật/khóa thật vào mã; không tạo .md ngoài các tài liệu sẵn có; không báo "đạt/pass" khi thiếu bằng chứng.
