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

## 5. Kết quả thử máy chủ Render thật trong bước S1 (Đợt 39 — `2026-11-09`)
Đã chạy kiểm tra trực tiếp tới `https://hitmegame.onrender.com` và từ trang `https://hitmegame-iota.vercel.app/next/` trong Playwright Chromium (`Evidence/round39/S1_render_probe.json`):

1. **`GET https://hitmegame.onrender.com/healthz` (có `Origin: https://hitmegame-iota.vercel.app`):**
   - Mã HTTP: **`200 OK`**, thời gian phản hồi: **`416.0 ms`**.
   - Body nguyên văn:
     ```json
     {"status":"ok","protocol":3}
     ```
   - Header trả về: `Content-Type: application/json; charset=utf-8`, `Server: cloudflare`, `x-render-origin-server: Kestrel`.
   - **Lưu ý CORS:** Phản hồi **không có header `Access-Control-Allow-Origin`**, nên khi gọi `fetch("https://hitmegame.onrender.com/healthz", { mode: "cors" })` trực tiếp từ trang `https://hitmegame-iota.vercel.app/next/`, trình duyệt ném `TypeError: Failed to fetch` (CORS blocked).

2. **`POST https://hitmegame.onrender.com/guest-token`:**
   - Mã HTTP: **`503 Service Unavailable`**, thời gian phản hồi: **`162.8 ms`**.
   - Ý nghĩa theo `server/README.md`: biến môi trường `GUEST_TOKEN_SIGNING_KEY` chưa được đặt trên Render, nghĩa là máy chủ đang cho phép định danh khách không chữ ký dạng `guest:<tên>` trong tin nhắn `join`.

3. **Kết nối WebSocket `wss://hitmegame.onrender.com/ws` từ nguồn `Origin: https://hitmegame-iota.vercel.app`:**
   - Trong trình duyệt mở tại `https://hitmegame-iota.vercel.app/next/`:
     ```json
     {
       "opened": false,
       "closeCode": 1006,
       "closeReason": "",
       "wasClean": false,
       "elapsedMs": 245.1,
       "messages": [],
       "errorEvent": true
     }
     ```
   - Kiểm tra trực tiếp bắt tay HTTP Upgrade (`Upgrade: websocket`, `Origin: https://hitmegame-iota.vercel.app`) cho thấy nguyên nhân chính xác:
     - Mã HTTP: **`403 Forbidden`**
     - Body nguyên văn từ Kestrel:
       ```json
       {"v":1,"type":"error","code":"origin_not_allowed"}
       ```
   - **Nguyên nhân gốc rễ (mục 11 Đợt 39):** WebSocket trên trình duyệt **không dùng CORS preflight**, nhưng máy chủ Kestrel (`server/Program.cs`) kiểm tra header `Origin` của yêu cầu HTTP Upgrade đối chiếu với danh sách `ALLOWED_ORIGINS`. Hiện tại trên Render, `ALLOWED_ORIGINS` vẫn đang ở giá trị mặc định (`http://localhost,http://127.0.0.1`) và **chưa có `https://hitmegame-iota.vercel.app`**.

4. **Đối chứng kết nối `wss://hitmegame.onrender.com/ws` với `Origin: http://localhost`:**
   - Bắt tay HTTP Upgrade trả **`101 Switching Protocols`** ngay lập tức.
   - Tin nhắn đầu tiên máy chủ gửi về ngay khi mở:
     ```json
     {"v":2,"type":"hello","serverVersion":"0.1.0-beta.1","minClientVersion":"0.0.0"}
     ```
   - Khi gửi tiếp `{"v":1,"type":"join","token":"guest:ProbeNam","clientVersion":"0.1.0-beta.6"}`, máy chủ tạo phòng thành công và trả về:
     ```json
     {"v":1,"type":"room_state","protocolVersion":1,"roomCode":"U44YKT","phase":"waiting_for_players","hostId":1,"maxPlayers":6,"minPlayersToStart":2,"round":0,"maxRounds":5,"players":[{"id":1,"name":"ProbeNam","hp":3,"alive":true,"connected":true,"ready":false,"isHost":true,"character":0}]}
     ```

5. **Yêu cầu cấu hình trên Render (Codex / Chủ dự án):**
   - Đặt biến môi trường trên dịch vụ Render `hitmegame`:
     - `ALLOWED_ORIGINS=https://hitmegame-iota.vercel.app,http://localhost,http://127.0.0.1`
   - Thêm header `Access-Control-Allow-Origin` (khớp `ALLOWED_ORIGINS`) cho phản hồi `GET /healthz` và `POST /guest-token` để trình duyệt trên Vercel đọc được JSON `/healthz` ở chế độ CORS chuẩn.
   - Trong khi chờ Render cập nhật `ALLOWED_ORIGINS`, mặt định `/next/` giữ nhãn **"Máy chủ chưa sẵn sàng"**, và toàn bộ màn hình Online + giao thức thật được mở qua cờ `?online=1` (hoặc `?lab=1` cho Loopback 2 người).

## 6. Kết quả kiểm tra máy chủ Render thật trong bước L10 (Đợt 40 — `2026-11-10T00:55:05Z`)
- **Thời gian kiểm thử:** `2026-11-10T00:55:05Z`.
- **Mục tiêu:** Kiểm thử toàn trình 2–6 người chơi thật từ nguồn Vercel `https://hitmegame-iota.vercel.app/next/` tới máy chủ Render `wss://hitmegame.onrender.com/ws`.

### Nhật ký đo đạc nguyên văn (Verbatim Probe Log):
1. `GET https://hitmegame.onrender.com/healthz`:
   - **Mã HTTP:** `200 OK` (thời gian: 320 ms)
   - **Header:** `Server: cloudflare`, `x-render-origin-server: Kestrel`, `Content-Type: application/json; charset=utf-8`
   - **Body:** `{"status":"ok","protocol":3}`
   - **Lưu ý CORS:** Vẫn thiếu header `Access-Control-Allow-Origin: https://hitmegame-iota.vercel.app`.

2. `POST https://hitmegame.onrender.com/guest-token`:
   - **Mã HTTP:** `503 Service Unavailable`
   - **Header:** `Server: cloudflare`, `x-render-origin-server: Kestrel`, `allow: POST`
   - **Ý nghĩa:** Biến môi trường ký token khách `GUEST_TOKEN_SIGNING_KEY` chưa được bật trên Render.

3. `POST https://hitmegame.onrender.com/client-log`:
   - **Mã HTTP:** `404 Not Found` (Codex chưa triển khai endpoint nhận telemetry log khách; client L8 giữ ở trạng thái fallback an toàn, không làm gián đoạn trải nghiệm).

4. Bắt tay WebSocket `wss://hitmegame.onrender.com/ws` với `Origin: https://hitmegame-iota.vercel.app`:
   - **Playwright log:**
     ```
     WS Created: wss://hitmegame.onrender.com/ws
     WS SocketError: Error during WebSocket handshake: Unexpected response code: 403
     WS Closed: {'errTriggered': True, 'code': 1006, 'reason': '', 'wasClean': False}
     ```
   - **Mã lỗi handshake:** HTTP `403 Forbidden` (`origin_not_allowed`).

### Kết luận bước L10:
- Theo đúng quy tắc an toàn và chỉ thị Đợt 40: *"Nếu Render chưa cho phép Origin Vercel hoặc guest-token chưa mở: ghi vào docs/NET_FEEDBACK.md nguyên văn và dừng L10 ở CHỜ CODEX; không dùng Kestrel cục bộ để tuyên bố 'online chạy'."*
- Antigravity **không** dùng kết quả Kestrel cục bộ để thay thế bằng chứng Render thật; xác nhận dừng L10 ở trạng thái: **`BỊ CHẶN / CHỜ CODEX (Render ALLOWED_ORIGINS & GUEST_TOKEN_SIGNING_KEY)`**.
- Toàn bộ mã client (L1–L9) đã hoàn tất 100%, sẵn sàng chạy ngay khi Codex cập nhật Render `ALLOWED_ORIGINS` cho phép `https://hitmegame-iota.vercel.app`.
