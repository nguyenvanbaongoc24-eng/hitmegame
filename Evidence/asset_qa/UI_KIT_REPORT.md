# HIT ME – UI asset QA

Nguồn: `ASSET HITME FINAL` (chỉ đọc). Lệnh tái chạy: `python tools/asset_qa/prepare_ui_assets.py`.

Đã quét **301 ảnh**, 294 có alpha, 39 chạm mép, 37 bản trùng byte, 0 nghi ô vuông theo phép dò biên bảo thủ. Đã tạo 18 bản sao UI clean.

Bảng chi tiết: `ui_kit_qa.csv`; manifest import: `ui_import_manifest.json`; ảnh ghép: `contact/`; bản sao và ảnh trước/sau: `staging/ui/`.

## Rủi ro trước khi nhập Unity

- 9-slice lấy từ `UI_ASSET_MAP.md`, chỉ là **đề xuất độ tin cậy thấp**, chưa đo góc bằng mắt từng ảnh. Bản staging đã cắt alpha nên phải dùng viền điều chỉnh trong sidecar `.json`, không lấy nguyên viền của file gốc.
- Phép dò ô vuông chỉ xét hai màu trung tính trên biên. Giá trị 0 không chứng minh mọi ảnh sạch. Không tự xóa ô vuông hoặc defringe trong bộ này.
- Cờ chữ in sẵn dựa trên **tên tệp**, không phải OCR. 18 ảnh `HITME_UI_KIT_CLEAN` được xem nhanh trên contact sheet: không thấy chữ. Các ảnh khác vẫn cần AG duyệt bằng mắt.
- `touches_edge` có thể là sprite cố ý tràn canvas; cần xem trước khi tự cắt. Bản staging chỉ áp dụng UI kit clean và giữ nguyên gốc.

## Màu kit (lấy mẫu pixel, lượng tử 32 mức)

#101030, #103030, #101010, #703010, #D03030, #F0B010, #30D0F0, #501010

## Thiếu hoặc chưa xác minh cho Lô 2–4

| Mục | Tình trạng |
|---|---|
| Home: logo HIT ME tách nền | Chưa thấy tệp đặt tên logo; cần AG/chủ dự án chọn nguồn |
| Luyện với bot: bộ chọn số người, nút và nền | Nút có trong kit; bố cục và icon chưa xác minh |
| Phòng: biểu tượng chia sẻ/mã phòng/khóa | Chưa thấy tệp đặt tên riêng; có thể dùng icon vector/TMP tạm |
| Kết quả: huy chương hạng 1–3 | `UI_ASSET_MAP.md` ghi `[TẠM]`, chưa thấy sprite riêng |
| Tài nguyên chữ in sẵn | Không dùng thay cho nhãn TMP tiếng Việt |

Ưu tiên: (1) duyệt 18 sprite clean trên contact sheet, (2) duyệt border 9-slice và staging, (3) chọn logo/huy chương, (4) kiểm riêng 39 ảnh chạm mép.
