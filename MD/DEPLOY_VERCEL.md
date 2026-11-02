# HƯỚNG DẪN TRIỂN KHAI VERCEL CHO HIT ME (WEB CLIENT)

Tài liệu này hướng dẫn chi tiết từng bước đóng gói và đưa WebGL Client lên nền tảng đám mây Vercel (miễn phí, phân phối toàn cầu qua CDN Edge).

---

## 1. Cơ cấu gói triển khai (Tools/deploy_web/)

Thư mục tĩnh đã được cấu hình tối ưu sẵn sàng deploy tại `Tools/deploy_web/`:
- `vercel.json`: Cấu hình routing, security headers và đặc biệt là giải nén Brotli (`Content-Encoding: br`) cho `.wasm.br`, `.data.br`, `.framework.js.br`.
- `config.json`: File cấu hình runtime để thay đổi URL server WebSocket mà không cần build lại Unity. Mặc định: `wss://hitmegame.onrender.com/ws`.
- `index.html`: Giao diện tối ưu di động (viewport-fit=cover, an toàn tai thỏ iPhone, chặn kéo làm mới pull-to-refresh), thanh tiến độ tải 0-100% tiếng Việt, và kiểm tra tính tương thích WebGL.
- `package_deploy.py`: Script tự động copy bản build Release mới nhất vào thư mục `Tools/deploy_web/dist/`.

---

## 2. Bước 1: Đóng gói bản build mới nhất

Mở terminal tại thư mục gốc hoặc thư mục `unity-client`, chạy:

```bash
python Tools/deploy_web/package_deploy.py
```

Kết quả sẽ xuất ra thư mục tĩnh hoàn chỉnh tại `Tools/deploy_web/dist/` (dung lượng ~10.14 MB đã nén Brotli).

---

## 3. Bước 2: Triển khai lên Vercel

Có 2 cách thực hiện:

### Cách A: Sử dụng Vercel CLI (Khuyên dùng - Nhanh nhất)

1. Cài đặt Vercel CLI (nếu chưa có):
   ```bash
   npm i -g vercel
   ```
2. Đăng nhập tài khoản Vercel:
   ```bash
   vercel login
   ```
3. Chuyển vào thư mục `dist` và triển khai:
   ```bash
   cd Tools/deploy_web/dist
   vercel --prod
   ```
4. Khi CLI hỏi:
   - `Set up and deploy?` -> Chọn `Y`
   - `Which scope do you want to deploy to?` -> Chọn tài khoản cá nhân của bạn
   - `Link to existing project?` -> Chọn `N` (lần đầu)
   - `What's your project's name?` -> Điền `hitme-game` (hoặc tên tùy thích)
   - `In which directory is your code located?` -> Nhấn Enter (`./`)
5. Sau 15-30 giây, Vercel sẽ trả về đường link production dạng `https://hitme-game.vercel.app`.

---

### Cách B: Kết nối qua GitHub Dashboard

1. Đăng nhập vào [https://vercel.com](https://vercel.com) bằng tài khoản GitHub.
2. Bấm nút **Add New...** -> **Project**.
3. Chọn kho lưu trữ `nguyenvanbaongoc24-eng/hitmegame`.
4. Cấu hình Project:
   - **Framework Preset**: Chọn `Other`.
   - **Root Directory**: Nhấn Edit và chọn `unity-client/Tools/deploy_web/dist` (hoặc thư mục static web).
   - **Build and Output Settings**: Giữ nguyên mặc định (không cần Build command).
5. Bấm **Deploy**.

---

## 4. Biến môi trường & Tham số cấu hình

Không cần cấu hình bí mật trên Vercel vì đây là client tĩnh. Mọi cấu hình kết nối server được điều khiển linh hoạt qua:

1. **File `config.json`**:
   ```json
   {
     "serverUrl": "wss://hitmegame.onrender.com/ws",
     "version": "1.0.0",
     "environment": "production"
   }
   ```
2. **Tham số URL (Query parameter)**:
   Bạn có thể trỏ game đến bất kỳ máy chủ nào để test mà không cần sửa file:
   - Chạy với máy chủ Render: `https://hitme-game.vercel.app`
   - Chạy với máy chủ Local qua ngrok: `https://hitme-game.vercel.app?server=wss://xxxx.ngrok-free.app/ws`
   - Chạy chế độ test tự động: `https://hitme-game.vercel.app?e2e=1`

---

## 5. Cách kiểm tra sau khi Deploy

1. Mở link Vercel trên điện thoại (Safari trên iOS hoặc Chrome trên Android).
2. Kiểm tra thanh tiến độ tải chạy từ 0% đến 100% mượt mà, màn hình HOME_SHOWN xuất hiện dưới 3 giây.
3. Thử tạo phòng: nếu Render server đã chạy, mã phòng 6 ký tự sẽ xuất hiện.
4. Mở thêm 1 tab hoặc thiết bị khác, nhập mã phòng để vào chung phòng.
