# HIT ME – LỆNH CHO AG, ĐỢT 34: GOM BẰNG CHỨNG + ĐẨY NHANH LÔ 5–8 THEO THIẾT KẾ GỐC

> Dán cho AG, kèm `hit-me-ag-round33.md`, `hit-me-ag-round31.md`, `hit-me-ag-round29-ui-skin.md`, `hit-me-gameplay-rules-v1.2.md`, `hit-me-room-size-rule-v1.3.md`. Tiến độ code tốt (Lô 2→4 đã có commit, 118/118 test, audit 12/12 mã thoát 0, 5 cổng chạy). Nhưng **báo cáo gần nhất thiếu bằng chứng** nên chủ dự án chưa thể duyệt lô nào. Làm mục E trước (ngắn), rồi chạy tiếp hàng đợi.

## E. Gom bằng chứng cho Lô 1–4 (bắt buộc, làm trước khi qua Lô 5)
Tạo `docs/UI_EVIDENCE_INDEX.md`. Mỗi lô một mục với: cổng + commit; danh sách ảnh WebGL (1170×2532) — **mỗi ảnh đã mở bằng công cụ xem ảnh, một dòng mô tả điều nhìn thấy** (không viết "mượt", "0 cấp phát", "bám ngón" vì ảnh tĩnh không chứng minh); ảnh ghép **WebGL ↔ asset gốc** từng thành phần chính; `.data` trước/sau; tên test mới + `Passed! N/N`; `git diff` số cứng/chuỗi bị sửa để qua audit; câu hỏi cho chủ dự án (không tích sẵn). Thiếu mục nào → ghi "CHƯA CÓ BẰNG CHỨNG", **không** ghi "xong". Riêng:
- **Lô 2 + N1** (`9cdef40`): ảnh Home, Luyện với bot (3 bot và 5 bot), Phòng 1 người (nút khóa + câu "Cần ít nhất 2 người…"), Phòng nhiều người, Ghép nhanh; test N1 (tên + kết quả), WebGL vẫn chạy 10 trận.
- **Lô 3** (`c309cf6`): ảnh kết quả vòng N=2 và N=4, bảng xếp hạng cuối trận, hộp thoại rời, toast lỗi.
- **Lô 4** (`e726d61`): vì ảnh tĩnh không chứng minh hiệu ứng, bổ sung **đoạn quay màn hình ngắn** (Playwright `recordVideo`, 8–10 giây mỗi đoạn) cho nhấn nút, mất máu, đếm giây cuối; và log đo (không đoán): số đối tượng pool/khung, byte cấp phát trong 60 giây.
- **Cách tránh lỗi nhận diện sai của audit:** bạn đổi `SetInt` → `SetString` vì mẫu `Tint` khớp nhầm `SetInt`. Ghi rõ trong báo cáo đây là **dương tính giả của mẫu quét**, không sửa `tools/audit`; Codex sẽ chỉnh mẫu (đã giao). Không đổi tên/viết lại số chỉ để né mẫu ở chỗ nào khác.

## Quyết định tạm cho câu hỏi của bạn (chủ dự án đổi được)
Cài đặt mở được **cả trong trận** nhưng là lớp phủ **không tạm dừng** đồng hồ (trận online không có tạm dừng); chỉ chứa âm thanh, giảm chuyển động, FPS, thoát trận. Ghi vào `UiStrings`/tài liệu.

## Hàng đợi tiếp theo (làm liên tục, không dừng chờ; mỗi 2 lô một build mới và một bảng cổng)

**Lô 5 – Âm thanh + Cài đặt** (đang làm): đúng `round33`; thêm **chuyển ngôn ngữ VI/EN** ở Cài đặt (theo thiết kế gốc có nút VI/EN): `UiStrings` hai bảng, mặc định VI, EN dịch sát nghĩa; test không thiếu khóa; ảnh thử chữ có dấu cho VI và ảnh EN không bị tràn khung.

**Lô 6 – Hướng dẫn lần đầu 3 bước** theo cử chỉ v1.2 (đúng `round33`). **Cổng 8098 cho Lô 5+6.**

**Lô 7 – Hồ sơ, Túi đồ, Cửa hàng, Bảng xếp hạng (khung giao diện, dữ liệu giả):** theo thiết kế gốc (Home có vàng, ngọc, sự kiện, quà ngày, bản đồ, bộ sưu tập, bảng xếp hạng; cửa hàng bán **vũ khí trang trí** chủ đề Việt Nam: dép tổ ong, chảo, xe đạp, Honda cũ, cốc bia, chó vàng…). Quy tắc:
- Dùng `IAccountService`/`IInventoryService` với bản **Fake** (dữ liệu cố định trong bộ nhớ); **không** gọi mạng/Supabase, không thanh toán thật.
- **Trang trí thuần túy:** không đổi luật, bán kính trúng, tốc độ, sát thương. Vũ khí mặc định vẫn là Dép Tổ Ong; món khác chỉ đổi hình/hiệu ứng, và trong trận chơi **chỉ bật khi cờ `CosmeticsEnabled=true`** (mặc định tắt). Ghi rõ ở bảng ánh xạ: mỗi món lấy từ asset nào, món nào **thiếu hiệu ứng**.
- Giá mỗi món cao hơn món trước theo hệ số cấu hình (`priceStepMultiplier`, mặc định 1.5, chủ dự án chốt sau); phần thưởng vàng cấu hình, mặc định tắt trong chế độ bot.
- Nút nhân vật/trang bị/túi đồ (thanh dưới Home) theo mockup.
- Mỗi màn dùng asset kit và `UiTheme`; thành phần thiếu asset ghi `Thiếu`.

**Lô 8 – Đăng nhập (khung):** màn "Khách" (nhập tên, mặc định "Khách_####"), nút Google/Facebook/Apple hiển thị nhưng **vô hiệu** có nhãn "Sắp có"; đổi tên: lần đầu miễn phí, từ lần hai tính phí (hiển thị, không trừ tiền thật) — dữ liệu giả. **Cổng 8099 cho Lô 7+8.**

**N2** khi Codex báo Z4 xong (nhớ lấy `unity_online_glue` của Codex): `UnityOnlineUi : IOnlineUi`, hai người trong một tiến trình bằng `LoopbackTransport`.

**Khi hết việc:** `docs/UI_QA_CHECKLIST.md`, danh sách `Thiếu`/`TẠM`, đo tải trang đầu và kích thước build (đúng `round33` mục 8).

## Chưa làm trong đợt này (cần quyết định của chủ dự án, AG chỉ ghi vào danh sách câu hỏi)
- **Trò chuyện tự do trong trận:** thiết kế gốc có ô nhập tin nhắn; giao thức server hiện chỉ có `emote` (danh sách cố định). Chat văn bản cần kiểm duyệt (từ cấm, báo cáo) và thay đổi giao thức. Mặc định: **chỉ `emote`** ở v1; chat để sau.
- Kết bạn sau trận; sự kiện/quà ngày/bản đồ: chỉ tạo ô giữ chỗ trong Home nếu mockup có, không làm chức năng.

## Điều cấm (nhắc lại)
Không tag; không viết mã mạng thật (`.jslib`, URL máy chủ — Codex); không đổi giao thức/luật; không thêm bot vào phòng online; không sửa `tools/audit`; không sửa asset gốc; không báo "xong" khi chưa có bằng chứng; không dừng chờ khi còn bước.

## Mẫu báo cáo
```
BẢNG CỔNG → COMMIT → NỘI DUNG: 8093 … 8099
BƯỚC: E | Lô 5 | Lô 6 | Lô 7 | Lô 8 | N2
ĐÃ LÀM: ...
BẰNG CHỨNG: (đường dẫn mục trong docs/UI_EVIDENCE_INDEX.md; Passed! N/N + tên test; .data trước/sau; audit mã thoát)
CHƯA CÓ BẰNG CHỨNG / BỊ CHẶN: ...
CÂU HỎI CHO CHỦ DỰ ÁN (không tích sẵn): ...
HEAD = commit build: ...  URL (ipconfig): ...
```
