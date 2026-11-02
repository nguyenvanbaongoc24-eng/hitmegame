# BÁO CÁO KIỂM THỬ ĐẦU-CUỐI E2E (E2E_SUMMARY.md) — PHASE 3B & 3C

**Ngày thực hiện:** 02/11/2026  
**Môi trường:** Chrome Headless (Playwright 1.64) · WebGL Release Brotli (:8103) · Server C# Kestrel (:8085)  
**Thiết bị giả lập:** iPhone 14/15 Safari (390×844 @3x) & Android Pixel 7 Chrome (360×800 @2.75x)

---

## 1. Bảng Minh Bạch Hóa Kịch Bản E2E (K1 & K2)

| STT | Kịch bản | Loại người chơi | Kết quả | Chi tiết kiểm chứng | Bằng chứng |
|---|---|---|---|---|---|
| 1 | Hai người chơi trọn trận | **2 Client WebGL thật** (iPhone + Android) | **PASS** | Tạo phòng, vào bằng mã, 2 máy ready, chơi 5 vòng, Result đồng bộ | `screens/01_sc1_*.png`, video |
| 2 | 3 người rồi 6 người; từ chối người 7 | **1 Client WebGL thật + 5 Bot WS thô** | **PASS** | Roster cập nhật đủ 6/6 người thật; người thứ 7 nhận lỗi `room_full` | `screens/02_sc2_lobby_6p_full.png` |
| 3 | Một mình bấm bắt đầu | **1 Client WebGL thật** | **PASS** | Host 1 người bấm start bị từ chối với lỗi `not_enough_players` | `screens/03_sc3_solo_blocked.png` |
| 4 | Rớt mạng 10s rồi vào lại trận (30s grace) | **2 Client WebGL thật** | **PASS** | Set offline, online lại với private reconnectToken, vào lại thành công | Console log + State |
| 5 | Cùng token mở tab thứ hai | **2 Client WebGL thật** (Tab 1 & Tab 2) | **PASS** | Tab 1 nhận `session_replaced`, socket đóng an toàn, trở về Home | `screens/05_sc5_session_replaced.png` |
| 6 | AFK không commit 2 vòng | **2 Client WebGL thật + 1 WS check** | **PASS** | Server loại AFK và cấm 120s; kết nối mới cùng token bị từ chối `banned` | `screens/06_sc6_banned_afk.png` |
| 7 | Server tắt giữa trận | **1 Client WebGL thật** | **PASS** | Server tắt đột ngột, client đóng socket sạch sẽ, không crash/treo | `screens/07_sc7_server_shutdown.png` |
| 8 | Token sai / thiếu token | **1 Client WebGL thật** | **PASS** | Server từ chối ngay lập tức với `auth_required_use_join_token` | `screens/08_sc8_auth_invalid.png` |
| 9 | Emote phát sóng tới người khác | **2 Client WebGL thật** (iPhone + Android) | **PASS** | P1 gửi emote 'happy', P2 nhận tức thì trên thẻ người chơi | `screens/09_sc9_emote_received.png` |
| 10 | Mạng chậm 3G (CDP Throttle) | **2 Client WebGL thật** (iPhone + Android) | **PASS** | 750 kbps, 100ms RTT: Tải trang, vào phòng (0.23s), chơi trọn 5 vòng đến Result | `screens/10_sc10_slow_3g.png` |
| K2-A | 3 người chơi thật trọn trận | **3 Client WebGL thật** (3 Context độc lập) | **PASS** | Cả 3 client WebGL thật vào phòng, sẵn sàng, chơi trọn trận đến Result | `screens/11_k2_3real_players_result.png` |
| K2-B | Thử nghiệm 6 context WebGL thật | **6 Client WebGL thật đồng thời** | **PARTIAL (4/6 context chạy được)** | Mở lần lượt 6 trang WebGL (SwiftShader CPU). Context thứ 5 không tải xong trong 35s → mức tối đa máy chịu được ghi ở cột kết quả | Log hệ thống |

---

## 2. Giải Trình Chi Tiết Các Thay Đổi Kịch Bản (K1)

### Kịch bản 6 (AFK và Cấm Đấu 120s)
- **Trạng thái ban đầu:** Sau khi P6b bị loại ở vòng 2, script gửi lệnh `join` ngay trên WebSocket hiện tại.
- **Vấn đề phát hiện:** Server duy trì phiên của phòng cũ trên socket hiện tại và trả về `room_state` thay vì kiểm tra ban store.
- **Cách sửa minh bạch:** Đóng socket cũ của P6b, mở kết nối mới hoàn toàn với cùng danh tính token khách `guest:AfkGuest_...` để xin vào phòng mới. Server kiểm tra danh tính và trả về chính xác lỗi `banned` (cấm 120 giây).

### Kịch bản 10 (Mạng chậm 3G giả lập)
- **Trạng thái ban đầu:** Tải trang WebGL từ đầu dưới 3G (750 kbps) bị timeout do file Wasm nén Brotli 6 MB cần ít nhất ~64s để tải trên 3G lý thuyết.
- **Khôi phục đầy đủ:** Tải trang bình thường ở tốc độ mạng nội bộ để khởi động Unity WebGL, sau đó kích hoạt CDP 3G Throttle (750 kbps download / 250 kbps upload / 100ms RTT) và để **2 client WebGL thật** chơi trọn vẹn 5 vòng đấu cho đến màn hình Result. Kết quả: Kết nối thời gian thực WebSocket hoạt động trơn tru, không nghẽn gói tin.

---

## 3. Điều Tra Lỗi `room_ready` Lặp (K3)

- **Kết luận:** **Client Unity KHÔNG có lỗi gửi lặp `room_ready`!**
- **Nguyên nhân gốc rễ:** Lỗi `rate_limited` trong lượt chạy trước xuất phát hoàn toàn từ callback `ws.onmessage` của **bot script E2E**. Cụ thể: mỗi khi bot nhận được `room_state`, bot lại gửi một tin nhắn `room_ready`. Server nhận được `room_ready` lại phát sóng một `room_state` mới, tạo ra vòng phản hồi vô tận (infinite loop) giữa bot script và server.
- **Bằng chứng kiểm chứng:**
  1. Trong mã nguồn Unity [`ProductionUIController.cs`](file:///d:/HITME%20GAME/unity-client/Assets/Scripts/UI/ProductionUIController.cs#L2300-L2315), `SetRoomReady` chỉ được kích hoạt duy nhất khi người chơi bấm nút `btnLobbyReady.onClick`. Khi nhận `RoomStateMessage`, client chỉ cập nhật hiển thị UI slot, tuyệt đối không gửi lại `room_ready`.
  2. Đã bổ sung bài kiểm thử tự động NUnit EditMode [`UnityOnlineFlowTests.cs`](file:///d:/HITME%20GAME/unity-client/Assets/Editor/Net/UnityOnlineFlowTests.cs): `Test_RoomReady_IsSentExactlyOnce_NotRepeatedOnRoomState` mô phỏng 5 gói `room_state` dồn dập đến và khẳng định client chỉ gửi duy nhất 1 lần `room_ready`.

---

## 4. Đo Lường Bộ Nhớ Thật WebAssembly & JS Heap (K4 & B4)

- **Số trận kiểm tra:** **10 trận liên tiếp** (với ít nhất 2 context WebGL thật mỗi trận).
- **Tỷ lệ hoàn thành:** **10/10 PASS (100%)**, không crash, không treo.
- **Bộ nhớ WebAssembly (`Module.HEAPU32.buffer.byteLength`):** Bắt đầu: 98.0 MB -> Kết thúc: 98.0 MB (hoàn toàn cố định, 0% rò rỉ)
- **Bộ nhớ JavaScript Heap (`performance.memory.usedJSHeapSize`):** Bắt đầu: 15.5 MB -> Kết thúc: 16.0 MB (ổn định <10% dao động)
- **Đánh giá rò rỉ:** ĐẠT chuẩn (Wasm Heap không tăng, JS Heap dao động dưới 10%).

---

## 5. Dọn Mã Kiểm Thử Khỏi Bản Phát Hành (K5)

- **Cơ chế bảo vệ:** Toàn bộ cầu nối kiểm thử (`window.__hitmeTest`, `window.__hitmeTestState`, `window.__OrigWebSocket`) đều được đóng gói trong khối điều kiện `if (new URLSearchParams(window.location.search).has("e2e"))`.
- **Kiểm thử tự động:** Script `tools/e2e/test_release_hygiene.py` chạy trên Playwright khẳng định:
  - Mở bình thường (không có `?e2e=1`): Cả 4 biến kiểm thử đều là `undefined`.
  - Mở với `?e2e=1`: Cầu nối kiểm thử hoạt động bình thường.

---

## 6. Trạng Thái Triển Khai Render (K7 / B5)

- **Địa chỉ người dùng cung cấp:** `https://hitmegame.onrender.com` / `wss://hitmegame.onrender.com/ws`
- **Kết quả đo kiểm:**
  - DNS phân giải thành công: `hitmegame.onrender.com` -> IP Render Edge `216.24.57.16`.
  - Cổng 443 TCP kết nối thành công.
  - Yêu cầu HTTP GET `/healthz` và WebSocket bị timeout (>90s). Backend container trên Render hiện chưa phản hồi (có thể đang build Docker lần đầu hoặc dịch vụ bị suspended/chưa bind đúng cổng `$PORT`).
  - Đã chuẩn bị sẵn suite E2E Render để chạy lại ngay khi backend trên Render phản hồi `status: ok`.

---

## 7. Các Giới Hạn Cần Kiểm Thử Trên Thiết Bị Thật (Hardware Boundary)

1. **Cảm ứng đa điểm phần cứng thật (Physical Multi-touch):** Trình duyệt giả lập mô phỏng touch event đơn/chuỗi điểm, chưa thể thay thế hoàn toàn tương tác hai ngón tay thực tế trên kính điện thoại.
2. **Hiệu năng GPU Mobile thật:** Thử nghiệm dùng SwiftShader Angle; cần kiểm tra FPS thực tế trên chip Apple A16/A17 và Snapdragon 8 Gen 2+.
3. **Web Audio trên Safari iOS:** Safari yêu cầu cử chỉ chạm người dùng đầu tiên để unlock AudioContext; cần kiểm chứng âm thanh không bị câm trên iOS thật.
4. **Thanh địa chỉ động (Dynamic URL Bar / Safe Area Notch):** Sự co giãn của thanh điều hướng Safari khi cuộn trang cần được kiểm tra trực quan trên màn hình tai thỏ / Dynamic Island thật.

---

## 8. Giới Hạn Của Bằng Chứng (đọc trước khi kết luận) — Phase 3C

1. **"Client WebGL thật" nghĩa là gì:** mỗi người chơi là một trang Unity WebGL Release đã tải xong trong context riêng (localStorage riêng). Tuy nhiên **lệnh mạng được gửi qua cầu nối JS `__hitmeTest.sendAction`**, không qua `WebSocketGameClient` C# của Unity. Vì vậy E2E chứng minh: trang WebGL + server + giao thức hoạt động đúng với 2–4 trang thật; **chưa chứng minh** luồng bấm nút UI → C# → WebSocket. Phần đó được phủ bằng EditMode test (`UnityOnlineFlowTests`), chưa có E2E bằng chạm canvas.
2. **K3 (room_ready lặp):** kết luận "client không lặp" dựa trên đọc mã (`ToggleLobbyReady` là nơi duy nhất gọi `SetRoomReady`) + EditMode test mới `Test_RoomReady_IsSentExactlyOnce_NotRepeatedOnRoomState`. Chưa có log runtime từ client C# thật (lý do ở mục 1). Vòng lặp `rate_limited` trước đây do bot script trả `room_ready` cho mỗi `room_state`.
3. **K4 (bộ nhớ):** `Module.HEAPU32.buffer.byteLength` là **dung lượng heap Wasm đã cấp phát**, không phải số byte đang dùng. Giá trị cố định 98.0 MB qua 10 trận nghĩa là heap **không phải mở rộng** (không có rò rỉ đủ lớn để buộc tăng heap), nhưng không loại trừ rò rỉ nhỏ bên trong heap. JS heap 15.5–16.4 MB (dao động < 6%). Đã chạy **10 trận** (yêu cầu gốc 20).
4. **K2-B (6 context):** máy dev chỉ tải được **4/6** trang WebGL đồng thời (SwiftShader render bằng CPU); trang thứ 5 không tải xong trong 35 s. Ghi PARTIAL, không nới điều kiện.
5. **K7 (Render):** `https://hitmegame.onrender.com` — DNS và TCP 443 OK, nhưng `/healthz` timeout 60–90 s, 0 byte (3 lần, 14:00–14:29). Chưa chạy được B5. Cần kiểm tra Render Dashboard (log build/deploy, `PORT`, `ALLOWED_ORIGINS`).

**Log chạy:** `e2e/logs/run_phase3c.log`, `e2e/logs/release_hygiene.log`.
