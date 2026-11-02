# HIT ME – LỆNH CHO AG, ĐỢT 31: LÔ 2 (HOME, LUYỆN VỚI BOT, PHÒNG) + SỬA CÁCH BÁO CÁO

> Dán cho AG, kèm `hit-me-ag-round29-ui-skin.md` và `hit-me-ag-round30.md`. **Bắt đầu Lô 2 ngay** (không chờ chủ dự án duyệt Lô 1; chủ dự án đang thử Lô 1 trên iPhone, phản hồi của họ sẽ áp vào Lô 1 bằng một commit sửa riêng sau). Giữ nguyên các ràng buộc cũ: không đổi luật, không thêm vũ khí ngoài dép, không sửa `tools/audit`, không sửa asset gốc, không tag, không viết mã mạng.

## Đánh giá báo cáo Lô 1 (`64bc6a9`)
**Ghi nhận:** có `UiTheme` + `UiStrings`, thẻ HP dùng khung kit + 3 sprite tim, nhãn VÒNG trên băng gỗ, biểu ngữ gỗ thay hộp neon, nút 3 trạng thái, ô vũ khí, vòng sáng pulse, 104/104 test, audit 12/12 mã thoát 0, build từ HEAD, `.data` +0.77 MB (đạt), huy hiệu "HẠNG 1/2/3 [TẠM]", chảo/vợt đưa ra khỏi phạm vi.

**Không chấp nhận:**
1. **Lần thứ ba bạn viết "đã mở và kiểm tra từng ảnh bằng PIL".** Nhật ký lần này **không có một dòng "Analyzed …png" nào**; `inspect_round30_captures.py` chỉ đếm điểm ảnh. Mô tả như "thước ngắm bám ngón", "hiệu ứng mượt, không giật, 0 cấp phát bộ nhớ", "đối thủ ẩn vị trí" **không thể thấy được trên ảnh tĩnh** — bỏ khẳng định đó khỏi mô tả ảnh. Quy tắc: **mở từng ảnh bằng công cụ xem ảnh**, ghi **chỉ điều nhìn thấy** (cái gì ở đâu, chữ đọc được không, có gì lạ). Lô này mở lại 10 ảnh Lô 1 và sửa mô tả.
2. **Bạn tích sẵn lựa chọn của chủ dự án** (`[x] Phương án A`, `[x] Phương án 1`). Để trống, chỉ ghi "đang dùng tạm: A / 1".
3. **Thiếu "ảnh asset gốc đặt cạnh" để chứng minh bám sát** (quy định ở round 29, U4). Với mỗi thành phần Lô 1: ảnh cắt từ màn hình WebGL ↔ ảnh asset gốc trong `ASSET HITME FINAL`, trong một ảnh ghép; thành phần nào **khác asset gốc** (màu, tỉ lệ, viền) thì ghi rõ vì sao.
4. **Số cứng:** bạn nói đã loại cả `0.447f` (xanh tre) khỏi mã UI để qua mẫu quét. Màu phải nằm trong `UiTheme` (đúng), nhưng **không** viết lại số cho hết khớp mẫu (ví dụ `0.4470f`, chia nhỏ phép tính). Dán `git diff` các dòng này để Codex kiểm.
5. Bạn **tắt máy chủ cổng 8093** khi mở 8094. Giữ **ít nhất bản trước** chạy để chủ dự án so sánh (8093 = `96bbc27`, 8094 = `64bc6a9`); ghi bảng "cổng → commit".
6. Chưa có số FPS **trên điện thoại** (chỉ PC): vẫn "CHƯA ĐO". Chủ dự án sẽ bật bộ đếm bằng 3 lần chạm góc trên bên phải; ghi lại cách bật đúng trong hướng dẫn.

## Lô 2 – Home, Luyện với bot, Phòng (Lobby)

Theo `docs/UI_ASSET_MAP.md`; mọi thành phần dùng asset kit (không dùng hình chữ nhật phẳng, trừ khi gắn `[TẠM]` và đã nằm trong danh sách "Thiếu").

**Home:**
- Nền: ảnh bối cảnh từ `ASSET BỐI CẢNH` (sân làng/cổng), tối ưu dung lượng cho WebGL; logo "HIT ME" bằng asset (nếu chưa có logo sạch: ghi vào "Thiếu", đề xuất cách tạm bằng chữ theo `UiTheme`).
- Bốn nút theo cấp ưu tiên: **LUYỆN VỚI BOT** (nút vàng chính, vì chơi một mình), **CHƠI NGAY / GHÉP NHANH** (online, người thật), **TẠO PHÒNG**, **VÀO PHÒNG**. Nút chính nằm 1/3 dưới màn hình; vùng chạm ≥ 135 px; trạng thái nhấn/mờ.
- Hiển thị nhân vật đang chọn (chân dung từ asset), không ghép sheet.

**Luyện với bot (một mình, chạy cục bộ):**
- Chọn nhân vật: lưới 7 chân dung (khung kit, ô được chọn có viền vàng); tên + một dòng mô tả.
- Chọn số bot 1–5 (mặc định 3): hai nút −/+ bằng asset và số lớn; tổng số người = 1 + bot (hiển thị "N người").
- Nút BẮT ĐẦU (CTA vàng); nút Quay lại.

**Phòng (Lobby, online):**
- Khung phòng; ô người chơi theo `người/6`; chỗ trống ghi "Đang chờ…" (không phải lỗi); mã phòng + nút sao chép; trạng thái "Sẵn sàng" mỗi người.
- Khi chỉ 1 người: nút BẮT ĐẦU mờ + câu "Cần ít nhất 2 người. Muốn chơi một mình, hãy sang menu Luyện với bot." + nút **SANG LUYỆN VỚI BOT**.
- Ghép nhanh: trạng thái "Đang tìm đối thủ…"; sau 30 giây gợi ý "Chưa tìm thấy đối thủ. Chơi với bot?" (chỉ gợi ý, không tự thêm bot).

**Kỹ thuật:** dùng `UiTheme`/`UiStrings`; không số cứng trong mã UI; không `OnGUI`/`Shader.Find`; `.data` tăng ≤ 4 MB so với `64bc6a9` (dán trước/sau); Safe Area; không cấp phát mỗi khung; giữ `Passed! N/N` và audit mã thoát 0. Build mới từ HEAD đã commit, **cổng 8095**, giữ 8093/8094.

## Bằng chứng (đúng quy tắc)
Ảnh WebGL 1170×2532 cho: Home, Luyện với bot (mặc định + 5 bot), Phòng 1 người, Phòng 3 người, Ghép nhanh đang chờ — mỗi ảnh **đã mở bằng công cụ xem ảnh** và có **một dòng mô tả điều nhìn thấy**; ảnh ghép "WebGL ↔ asset gốc" cho từng thành phần; bảng trạng thái cập nhật trong `docs/UI_ASSET_MAP.md` (cột duyệt để trống). Danh sách 8 điều chủ dự án nhìn trên iPhone cho Lô 2 + mẫu trả lời (không tích sẵn).

## Mẫu báo cáo
```
LÔ: sửa Lô 1 (mô tả ảnh, cạnh asset gốc, số cứng) | Lô 2
ĐÃ LÀM: ...
BẰNG CHỨNG: (ảnh + 1 dòng nhìn thấy; ảnh ghép asset gốc; git diff số cứng; .data trước/sau; Passed! N/N; audit mã thoát)
CHƯA LÀM / THỪA NHẬN SAI: ...
CHỜ CHỦ DỰ ÁN: phản hồi Lô 1 (font, đồng hồ, duyệt); duyệt Lô 2
HEAD = commit build: ...   Bảng cổng → commit: 8093 / 8094 / 8095   URL (ipconfig): ...
```
