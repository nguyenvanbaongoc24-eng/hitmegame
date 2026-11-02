# HƯỚNG DẪN TRIỂN KHAI VERCEL CHO HIT ME (WEB CLIENT)

Tài liệu này hướng dẫn chi tiết cấu hình và triển khai WebGL Client lên nền tảng đám mây Vercel bằng nhánh mồ côi `web-dist` (chỉ chứa gói web tĩnh ~10.1 MB, không chứa mã nguồn Unity hay lịch sử git nặng).

---

## 1. Cơ cấu gói web và Nhánh `web-dist`

Nhánh `web-dist` là nhánh độc lập (orphan branch) được tự động tạo và đẩy lên GitHub repo `nguyenvanbaongoc24-eng/hitmegame`.
Nội dung ở gốc của nhánh `web-dist`:
- `index.html`: Giao diện tối ưu di động (viewport-fit=cover, an toàn tai thỏ iPhone, chặn pull-to-refresh), thanh tiến độ % + số MB đã tải (`x / 10.1 MB`), toast cảnh báo mạng chậm > 15s.
- `vercel.json`: Cấu hình routing, security headers và giải nén Brotli (`Content-Encoding: br`) cho `.wasm.br`, `.data.br`, `.framework.js.br`.
- `config.json`: Cấu hình WebSocket server runtime (`wss://hitmegame.onrender.com/ws`).
- `Build/`: Chứa 4 file build WebGL Release Brotli:
  - `WebGL_Release_*.data.br` (~5.96 MB)
  - `WebGL_Release_*.wasm.br` (~4.56 MB)
  - `WebGL_Release_*.framework.js.br` (~65 KB)
  - `WebGL_Release_*.loader.js` (~28 KB)

---

## 2. Cài đặt trên Vercel Dashboard (Dự án `hitmegame`)

Truy cập [https://vercel.com](https://vercel.com) → Chọn dự án **hitmegame** (hoặc `hitmegame-iota`):

### Bước 1: Đổi Production Branch sang `web-dist`
1. Vào **Settings** → **Git** (hoặc **Environments** → **Production**).
2. Tại mục **Production Branch**, đổi giá trị thành: `web-dist`.
3. Bấm **Save**.

### Bước 2: Cấu hình Build and Output Settings
1. Vào **Settings** → **General** → Cuộn xuống mục **Build & Development Settings**.
2. **Framework Preset**: Chọn **Other**.
3. **Build Command**: Tắt Override (để trống).
4. **Output Directory**: Tắt Override hoặc để `.` (thư mục gốc).
5. **Install Command**: Tắt Override (để trống).
6. Bấm **Save**.

### Bước 3: Triển khai (Redeploy)
1. Vào tab **Deployments**.
2. Bấm vào dấu `...` ở bản deployment gần nhất → Chọn **Redeploy**.
3. Chờ 10–20 giây, Vercel sẽ triển khai thành công từ nhánh `web-dist`.
4. Truy cập URL production: `https://hitmegame-iota.vercel.app/` (hoặc URL dự án của bạn).

---

## 3. Lệnh cập nhật bản web sau này (Chỉ 1 lệnh duy nhất)

Khi có bản build mới hoặc thay đổi giao diện web, chỉ cần chạy script tự động:

**PowerShell:**
```powershell
.\tools\deploy_web\publish_web_dist.ps1
```
hoặc **Python:**
```bash
python tools/deploy_web/publish_web_dist.py
```

Script sẽ tự động:
1. Đóng gói bản Release mới nhất từ `Builds/` vào `dist/`.
2. Kiểm tra tính toàn vẹn 4 tệp Brotli + `index.html` + `vercel.json` + `config.json`.
3. Đồng bộ vào thư mục staging `_web_dist_deploy`.
4. Commit và push nhánh `web-dist` lên GitHub. Vercel sẽ tự động phát hiện và build lại ngay lập tức.

---

## 4. Kiểm tra chất lượng triển khai Vercel

1. **HTTP Status & Headers:**
   - URL gốc `https://hitmegame-iota.vercel.app/` trả về `200 OK`.
   - File `.br` có header `Content-Encoding: br` và `Content-Type` chuẩn (`application/wasm` cho `.wasm.br`).
2. **Màn hình tải:**
   - Thanh tiến độ hiển thị phần trăm và số MB đã tải (`Đang tải dữ liệu... x% (x.x / 10.1 MB)`).
   - Tự động hiển thị banner thông báo nếu mạng chậm quá 15 giây.
3. **Kết nối Server:**
   - Trỏ mặc định tới `wss://hitmegame.onrender.com/ws`.
   - Nếu đổi máy chủ test, có thể truyền tham số: `https://hitmegame-iota.vercel.app/?server=wss://...`
