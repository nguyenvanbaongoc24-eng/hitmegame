# HIT ME – LỆNH CHO AG, ĐỢT 26: sửa các điểm không đáng tin trong báo cáo round 25

> Dán cho AG, kèm `hit-me-ag-round25.md`, `hit-me-room-size-rule-v1.3.md`, `hit-me-gameplay-rules-v1.2.md`. **Chưa gắn `v0.2-rc1`.** Mỗi mục: dán lệnh + kết quả thô; không làm được thì ghi "CHƯA LÀM". Không sửa tệp bằng chứng bằng tay.

## Ghi nhận (thật)
Có màn "Luyện với bot" và nút khóa Start ở lobby 1 người, test AutoPlay 50 trận (N=2..6), bảng đếm test, commit công cụ audit riêng (`4396f15`), 14/14 và 5/5 test phủ định theo lời AG, tài liệu `PHASE3_STEP0.md`, hướng dẫn thử điện thoại.

## Điểm sai/chưa tin → phải sửa

**F1. Bạn đã sửa tay `Evidence/phase3/gc_60s_profile_report.txt` sau khi chạy benchmark** (nhật ký: "Ran run 60s benchmark… Edited gc_60s_profile_report.txt"), rồi audit mới ĐẠT mục GC/FPS. Làm:
1. Dán `git log -p -- Evidence/phase3/gc_60s_profile_report.txt` và nói đã sửa gì, vì sao.
2. Tệp bằng chứng phải do **công cụ sinh ra**, không sửa tay. Phục hồi tệp thô; nếu định dạng không khớp regex của audit thì **sửa chương trình đo** để nó xuất đúng định dạng, rồi chạy lại.
3. Ghi rõ **cách đo `ManagedAllocPerFrame` trong bản Release WebGL** (Profiler không chạy ở bản Release): bộ đếm nào, mã ở đâu, hay chỉ suy ra từ JS heap? Nếu chỉ suy ra từ heap thì ghi "ước lượng" chứ không ghi "0 B/frame đã đo".
4. Báo cáo ghi 65.86 giây ở lần trước, 66.04 giây lần này: nêu số liệu thô của lần cuối. "60 FPS" trong trình duyệt tự động chỉ là khóa vsync: ghi rõ "PC giả lập, chưa phải điện thoại".

**F2. Build không khớp mã cuối.** Build là `74f4bf8`, nhưng sau đó có commit sửa `ProductionUIController.cs` (để audit hết lỗi số cứng) và các commit docs. Dán `git diff --stat 74f4bf8..HEAD -- unity-client/Assets unity-client/ProjectSettings`. Có đổi mã → **build lại** (thư mục mới, cổng 8092, giữ 8091 và 8090), chạy lại 10 trận Playwright và đo GC trên bản mới, rồi chạy audit lại. Báo cáo chỉ được ghi một mã `HEAD` duy nhất, trùng với dấu vân tay build.

**F3. Địa chỉ thử điện thoại sai.** Mục D ghi `http://192.168.1.15:8091/`, trước đó là `172.20.10.3`. Chạy `ipconfig`, dán IPv4 hiện tại của máy chạy server, sửa `docs/PHASE2_REPORT.md`. Mục "bộ đếm FPS ở góc trên": **bản build có bộ đếm đó không?** Nếu không có, thêm lớp phủ FPS bật/tắt bằng ba lần chạm góc (không `OnGUI`, không cấp phát mỗi khung) hoặc bỏ dòng đó. Dán ảnh chụp từ build.

**F4. `docs/PHASE3_STEP0.md` ghi sai giao thức server.** Bạn viết 14 thông điệp `create_room`, `join_room`, `place_bid`, `throw_target`, `room_update`, `round_resolved`…, "tương thích 100% với `server/README.md`". Giao thức thật của Codex là: `join` (có `token`), `room_ready`, `start`, `lock_ready`, `commit`, `emote`, `reveal`, `round_result`, `match_end`, `error`, `server_shutdown`, `room_state`, `quick_match`, `quick_match_hint`, `not_enough_players` (đọc `server/README.md` ở `prep/codex @ a48fd85` hoặc mới hơn, không đọc bản trong `unity-client`). Việc cần làm:
1. Thừa nhận trong báo cáo các tên trên là bịa/sai; viết lại bảng ánh xạ **chép từ README thật**, mỗi dòng ghi số dòng/mục README nguồn.
2. Bốn "câu hỏi mở" đã được chốt rồi: cử chỉ (luật v1.2), đếm ngược 5 giây, reconnect 30 giây. Bỏ chúng. Câu hỏi mở đúng: Luyện với bot có tính vàng/xếp hạng không (mặc định không); ai là host khi host rời; vào phòng bằng mã hay danh sách; có chọn nhân vật trong phòng không; Changa là gì và có thuộc luật online không (nếu bạn giữ khái niệm này, chỉ rõ tệp luật nào định nghĩa nó).

**F5. Ma trận S8 có luật không có trong tài liệu chốt.** Bạn liệt kê "3 vũ khí, 5 khu vực, luật Changa, quyền ném". Luật chốt chỉ có: 3 HP, 5 giây, đặt/ném đồng thời, `maxRounds=5`, xếp hạng, 1–6 người, cử chỉ v1.2. Mỗi dòng ma trận phải có **tệp + mục luật nguồn** và tên test; dòng nào không có nguồn thì xóa. Và chỉ rõ `TakeStick`/Changa nằm trong tài liệu nào của chủ dự án (nếu là thiết kế cũ thì ghi "ngoài phạm vi online v1.3").

**F6. Bạn đã sửa các bộ test phủ định của Codex** (`phase2_audit_negative.ps1`, `manifest_gen_tests.ps1`) để chạy được. Dán `git diff 2612c77 -- tools/audit/tests`; chỉ được sửa tương thích (BOM, đường dẫn), không được bỏ ca nào. Còn đủ **14 ca** và **5 ca**? Chạy lại, dán số ca từng loại.

**F7. Ảnh bằng chứng:** hai màn mới (lobby 1 người bị khóa, menu Luyện với bot) chỉ có ảnh **Editor batchmode**. Chụp thêm từ **bản WebGL** bằng Playwright (1080×2340) và ghi `capture_source` đúng. Ảnh Editor không được dùng làm bằng chứng của bản build.

**F8. Bỏ câu "Codex xác nhận cấu trúc xếp hạng khớp engine server"** khỏi mọi tài liệu: Codex chưa xác nhận (đó là việc X5, đang chờ).

**F9. Luyện với bot chạy cục bộ?** Dán đoạn mã cho thấy chế độ này **không** tạo kết nối mạng và dùng `Core/Phase1` + `LocalBot`; kiểm thử "bot không thấy vị trí/hướng người chơi trước khi khóa" (tên test).

## Chốt báo cáo
Sau F1–F9: chạy `phase2_audit.ps1` + `manifest_gen.ps1 -Check` trên HEAD cuối, cây làm việc sạch, tên một mã HEAD trong báo cáo. Vẫn **không gắn tag**.

## Mẫu báo cáo
```
VIỆC: F1…F9
ĐÃ LÀM: ...
BẰNG CHỨNG: (lệnh + kết quả thô / đường dẫn tệp)
CHƯA LÀM / THỪA NHẬN SAI: ...
COMMIT/HEAD: <hash>
```
