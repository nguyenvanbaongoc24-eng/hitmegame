# HIT ME — NET FEEDBACK & MIXED CONTENT / TLS REPORT (`docs/NET_FEEDBACK.md`)

## 1. Mixed Content (`ws://` trên `https://`) và yêu cầu `wss://`
- **Vấn đề thực tế:** Trang WebGL Hybrid xuất bản trên Vercel chạy tại nguồn `https://hitmegame-iota.vercel.app`. Theo chính sách bảo mật trình duyệt (W3C Mixed Content / Safari iOS WebKit / Chromium), mọi kết nối `new WebSocket("ws://...")` khởi tạo từ trang `https://` đều bị trình duyệt **chặn cứng ngay lập tức** (`SecurityError: The operation is insecure` / Mixed Content blocked) trước khi gói tin TCP/SYN rời máy khách.
- **Yêu cầu cho Codex (Server / Hạ tầng):**
  1. Điểm cuối WebSocket phục vụ bản Vercel **bắt buộc dùng `wss://`** (ví dụ `wss://<service>.onrender.com/ws`), có chứng chỉ TLS hợp lệ do Render hoặc Cloudflare/Reverse Proxy cấp.
  2. Biến môi trường `ALLOWED_ORIGINS` trên máy chủ phải bao gồm chính xác:
     - `https://hitmegame-iota.vercel.app`
     - (Nếu dùng preview domain của Vercel thì thêm origin tương ứng hoặc cấu hình theo môi trường).
  3. Đối với **thử nghiệm mạng LAN nội bộ** (`ws://<LAN-IP>:8080/ws` không có TLS): người thử phải mở bản client qua giao thức **`http://<LAN-IP>:<port>/`** cục bộ (không mở từ `https://hitmegame-iota.vercel.app` trỏ về `ws://192.168.x.x`).

## 2. Cold Start (Đánh thức máy chủ miễn phí trên Render)
- Gói miễn phí của Render có thể ngủ sau 15 phút không có lưu lượng và mất **30–60 giây** để khởi động lạnh (cold start).
- Phía client (`U3`) thực hiện:
  - Ping trước `GET https://<server-host>/healthz` (hoặc mở kết nối với thời gian chờ 60 giây) kèm trạng thái hiển thị rõ: **"Đang đánh thức máy chủ… (tới 60 giây)"** + bộ đếm thời gian và nút **Thử lại** / **Chuyển sang Luyện với bot**.
- **Đề xuất cho Codex:** Endpoint `/healthz` cần trả lời HTTP `200 OK` kèm header `Access-Control-Allow-Origin: https://hitmegame-iota.vercel.app` để trình duyệt trên Vercel có thể gọi `fetch("https://<server>/healthz")` kiểm tra trạng thái thức dậy trước khi mở `wss://<server>/ws`.

## 3. Đồng bộ hợp đồng Wire v1.3 (`room_state`, `quick_match_hint`, `not_enough_players`)
- Client (`Assets/Scripts/Net/ClientMatchSession.cs` + `OnlineFlowController.cs`) đã khớp với wire v1.3 của `origin/prep/codex`:
  - Không có bot trong phòng online (`maxPlayers = 6`, `minPlayersToStart = 2`).
  - Khi `room_state.players.Count < 2`, nút Bắt đầu bị khóa và hiển thị thông báo kèm nút chuyển sang chế độ **Luyện với bot** cục bộ.
  - Khi nhận `quick_match_hint`, chỉ hiện gợi ý chuyển sang **Luyện với bot**, không tự động thêm bot vào phòng.

## 4. Kết quả kiểm tra máy chủ trực tiếp trong bước U3 (Đợt 38)
- **Kiểm tra `GET https://hitmegame.onrender.com/healthz` với header `Origin: https://hitmegame-iota.vercel.app`:**
  - HTTP Status: `200 OK`, body: `{"status":"ok","protocol":3}`.
  - Response headers: `Content-Type: application/json; charset=utf-8`, `x-render-origin-server: Kestrel` — **chưa có header `Access-Control-Allow-Origin: https://hitmegame-iota.vercel.app`**, khiến trình duyệt khi gọi `fetch` từ Vercel chỉ nhận `response.type === "opaque"` (`state.serverHealth = "waking"` trong `app.js`).
  - Theo `origin/prep/codex:server/README.md`, cầu nối WebGL `.jslib` (`HitMeWebSocket.jslib`) mới qua kiểm thử JS bridge độc lập, chưa hoàn tất ghép nối toàn trình vào bản build WebGL Unity + thử iPhone, nên `NetConfig.Enabled` mặc định giữ `false`.
- **Trạng thái giao diện trên `/next/` (U3):**
  - Ba nút **CHƠI NHANH / TẠO PHÒNG / VÀO PHÒNG** hiển thị rõ nhãn **"Máy chủ chưa sẵn sàng"** (thay cho *"Sắp ra mắt"*).
  - Khi người chơi chạm vào bất kỳ nút online nào, hộp thoại **"MÁY CHỦ CHƯA SẴN SÀNG"** hiện ra giải thích rõ lý do, có nút chính **"CHUYỂN SANG LUYỆN VỚI BOT"** (chuyển thẳng vào màn chọn bot 1 chạm) và nút **"ĐÁNH THỨC MÁY CHỦ (TỚI 60 GIÂY)"** (hiển thị thanh tiến trình đếm 0–60 giây + nút **THỬ LẠI KẾT NỐI**).
