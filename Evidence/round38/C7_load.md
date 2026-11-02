# C7 – HOME_SHOWN trên WebGL build 8c6b328

Lệnh: `python tools/measure_home_shown.py` → **Passed! 6/6 HOME_SHOWN measurements**. Dữ liệu thô: `C7_load.json`.

| Build | Lần 1 | Lần 2 | Lần 3 | Trung vị |
|---|---:|---:|---:|---:|
| Dev 8102 (`WebGL_20261102_8c6b328`) | 17,743 s | 17,318 s | 17,672 s | **17,672 s** |
| Release Brotli 8103 (`WebGL_Release_20261102_8c6b328`) | 10,026 s | 9,996 s | 10,006 s | **10,006 s** |

Giảm 7,666 giây (43,4%) theo trung vị. Mỗi lượt là browser context riêng trong Edge headless, viewport 414×896, CDP giả lập tải xuống 20 Mbps và trễ 40 ms. Server chỉ đọc hai thư mục build có sẵn và phục vụ Brotli với `Content-Encoding: br`; không mở Unity. Mốc đo là `window.__hitmeHomeShown`, không phải tải xong HTML hay hiện canvas.

Đây là số đo cục bộ trên máy tính, chưa phải iPhone/LAN/Render/Vercel. Chủ dự án chưa chốt ngưỡng chấp nhận.
