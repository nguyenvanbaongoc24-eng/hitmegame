# UI kiểm tra ảnh và phông

AG lấy tệp qua `git show prep/codex:tools/ui/TÊN_TỆP`, hoặc lấy cả thư mục từ nhánh `prep/codex`. Cần Python, Pillow, numpy và fontTools. Các công cụ không sửa ảnh nguồn.

- `python tools/ui/glyph_coverage.py FONT.ttf Assets/Scripts/Game Assets/Data --vietnamese --json-out glyphs.json`: kiểm mã Unicode; exit 1 nếu thiếu, in tệp/dòng. `--vietnamese` kiểm đủ chữ tiếng Việt.
- `python tools/ui/slice_ui_kit.py kit.png output --names "Nút Chơi,Khung Phòng" --expected 2`: xuất crop PNG alpha, `items.json`, contact sheet. Nếu số món không khớp, chỉ xuất `rejected_annotated.png` và dừng.
- `python tools/ui/nineslice_suggest.py output/01_nut_choi.png`: gợi ý viền 9-slice; kiểm bằng mắt trước khi dùng.
- `python tools/ui/regression_from_screenshot.py screenshot.png --output report.json`: báo ô trắng lớn và dải đen phủ ngang cạnh trên/dưới. Exit 1 nếu có cờ. OCR chưa hỗ trợ, nên công cụ này không xác nhận dấu `?` hoặc ô vuông thay chữ.

Kiểm thử: `python -m unittest discover -s tools/ui -p 'test_*.py' -v`. Các ngưỡng ảnh chỉ giúp khoanh vùng lỗi; kết luận cần xem ảnh thật trên thiết bị.
