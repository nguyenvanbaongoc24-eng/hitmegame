# HIT ME authoritative server

Nguồn luật duy nhất là các tệp được liên kết từ `Assets/Scripts/Core/Phase1` (nhánh Core hiện tại của checkout). Chạy: `dotnet run --project server/HitMe.Server.csproj`; dùng `PORT=8080` để chọn cổng. Kiểm tra: `GET /healthz`. Test: `dotnet test server/Tests/HitMe.Server.Tests.csproj --nologo`.

WebSocket dùng JSON `{ "v": 1, "type": "..." }`. Client gửi `join`, `room_ready`, `start`, `lock_ready`, `commit`, `leave`; server gửi `room_state`, `round_start`, `reveal`, `round_result`, `match_end`, `error`. `join` bắt buộc có `token`; bản tạm chỉ nhận `guest:<tên>` (server cấp ID khách), còn `playerId`/`name` do client gửi luôn bị bỏ qua. Thiếu token trả `auth_required_use_join_token`, token sai trả `auth_invalid`. `room_ready` dùng riêng ở phòng chờ: khi ít nhất hai người đều ready, server đếm 3 giây rồi phát `round_start`; bỏ ready/rời phòng hủy đếm. Host có thể gửi `start` để bắt đầu ngay. `lock_ready` chỉ dùng trong pha đặt để khóa sớm. Ví dụ commit: `{"v":1,"type":"commit","roundId":1,"positionX":1,"positionY":2,"aimX":0,"aimY":1}`. Commit chỉ trả xác nhận riêng trong lúc đặt; sau khóa mới broadcast `reveal` và kết quả. Tin tối đa 1 KiB, tối đa 20 tin/giây/kết nối.

Server dùng đồng hồ riêng cho 5 giây đặt, kẹp vị trí vào sân, coi aim 0/NaN/vô cực là không ngắm để Core áp `ThrowFacing`; người mất kết nối được giữ 30 giây bằng reconnect token ngẫu nhiên 256-bit. Vào lại phải đồng thời có token xác thực của đúng người chơi; token vào lại được xoay sau khi dùng và không được ghi log. Socket chậm có tối đa bốn lượt gửi chờ và bị ngắt khi timeout. WebSocket keep-alive là 30 giây; `IDLE_TIMEOUT_SECONDS` (mặc định 90) đóng socket im lặng và vẫn giữ chỗ 30 giây. `ALLOWED_ORIGINS` là danh sách Origin ngăn cách bằng dấu phẩy; mặc định chỉ localhost. Khi SIGTERM, server đóng các socket với lý do `server_shutdown`. Nhật ký chỉ ghi ID kết nối rút gọn, loại tin và lỗi — không ghi payload, vị trí, hướng hay token. `Dockerfile` và `render.yaml` chỉ là bản nháp, không tự triển khai.

Vector vàng 200 vòng nằm tại `server/Tests/golden/rounds.json`; test server và bản vá NUnit Unity cùng đọc định dạng trường công khai để so Core theo từng hit và trạng thái người chơi.

Render miễn phí có thể ngủ sau khoảng 15 phút không có truy cập, làm WebSocket bị ngắt. Chủ dự án cần chọn: gói trả phí, ping giữ ấm, hoặc chấp nhận hành vi này; chưa triển khai lựa chọn nào.

Đang dở: kiểm thử WebSocket đầu-cuối 20 trận (Z1), transport Unity/WebGL (Z2), hook AFK và thử tải. Không deploy server WIP này.
