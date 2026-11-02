# HIT ME – CODEX: KIỂM ĐỊNH ASSET GIAO DIỆN (chỉ đọc, song song với AG round 29)

> Dán cho Codex, xen kẽ với `hit-me-codex-round24.md`/`round25.md`. Không mở Unity, không sửa `D:\HITME GAME\ASSET HITME FINAL` và `unity-client`. Kết quả đặt trong `tools/asset_qa/` và `Evidence/asset_qa/` của nhánh `prep/codex`, và một mục ngắn trong `server/README.md` **không** cần; dùng tệp `Evidence/asset_qa/UI_KIT_REPORT.md` (ngoại lệ cho việc này).

1. Quét `HITME_UI_KIT_CLEAN`, `HITME_AIM_KIT_V1`, `ASSET TARGET RULER`, `UI_KIT_EXTRACTED`, `ASSET EFFECT`: mỗi tệp một dòng `đường dẫn | kích thước | có kênh alpha | tỉ lệ pixel trong suốt | còn dấu ô vuông (checkerboard) | chạm mép (hộp bao alpha chạm biên) | lũy thừa của 2 | ghi chú`.
2. Gợi ý **viền 9-slice** (L/R/T/B) cho từng khung/nút bằng cách dò vùng góc không đổi; ghi mức tin cậy (cao/thấp).
3. Tìm tệp **trùng** (cùng băm) và tệp **chứa chữ in sẵn** (ví dụ "CHẢO", "DÉP", chú thích bảng); đánh dấu "không dùng làm sprite trong game".
4. Bảng màu chủ đạo của kit (5–8 màu, mã hex) để AG dùng cho `UiTheme`.
5. Ảnh ghép xem trước theo nhóm (`Evidence/asset_qa/contact/ui_kit_*.png`) + sidecar `.json`.
6. Chạy lại hai lần với kết quả giống nhau (tất định) và dán `Passed! N/N` cho phần kiểm thử của công cụ (ca mẫu có checkerboard, có chữ, trùng băm).

Báo cáo: VIỆC / ĐÃ LÀM / BẰNG CHỨNG / PHÁT HIỆN QUAN TRỌNG / COMMIT. Chỉ đọc; không đổi file gốc.
