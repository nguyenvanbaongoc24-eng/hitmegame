# HIT ME authoritative server

Nguồn luật duy nhất là các tệp được liên kết từ `Assets/Scripts/Core/Phase1` (nhánh Core hiện tại của checkout). Chạy: `dotnet run --project server/HitMe.Server.csproj`; dùng `PORT=8080` để chọn cổng. Kiểm tra: `GET /healthz`. Test: `dotnet test server/Tests/HitMe.Server.Tests.csproj --nologo`.

WebSocket dùng JSON `{ "v": 1, "type": "..." }`. Client gửi `join`, `ready`, `commit`, `leave`; server gửi `room_state`, `round_start`, `reveal`, `round_result`, `match_end`, `error`. Ví dụ commit: `{"v":1,"type":"commit","roundId":1,"positionX":1,"positionY":2,"aimX":0,"aimY":1}`. Commit chỉ trả xác nhận riêng trong lúc đặt; sau khóa mới broadcast `reveal` và kết quả. Tin tối đa 1 KiB, tối đa 20 tin/giây/kết nối.

Server dùng đồng hồ riêng cho 5 giây đặt, kẹp vị trí vào sân, coi aim 0/NaN/vô cực là không ngắm để Core áp `ThrowFacing`; người mất kết nối được giữ 30 giây bằng reconnect token. Socket chậm có tối đa bốn lượt gửi chờ và bị ngắt khi timeout. `Dockerfile` và `render.yaml` chỉ là bản nháp, không tự triển khai.

Đang dở: hook AFK, vector vàng, kiểm thử WebSocket đầu-cuối 20 trận, transport Unity/WebGL và review triển khai thật. Không deploy server WIP này.
