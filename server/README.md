# HIT ME authoritative server

Nguồn luật duy nhất là các tệp được liên kết từ `Assets/Scripts/Core/Phase1` (nhánh Core hiện tại của checkout). Chạy: `dotnet run --project server/HitMe.Server.csproj`; dùng `PORT=8080` để chọn cổng. Kiểm tra: `GET /healthz`. Test: `dotnet test server/Tests/HitMe.Server.Tests.csproj --nologo`.

WebSocket dùng JSON `{ "v": 1, "type": "..." }`. Client gửi `join`, `room_ready`, `start`, `lock_ready`, `commit`, `emote`, `leave`; server gửi `room_state`, `round_start`, `reveal`, `round_result`, `match_end`, `emote`, `error`. Phòng online có 1–6 người thật; `room_state.config` trả `maxPlayers: 6` và `minPlayersToStart: 2`. Một người không thể `start` (`not_enough_players`) và tự ready cũng không mở đếm ngược; UI cần dẫn người đó tới **Luyện với bot** cục bộ, không qua server. `join` bắt buộc có `token`; bản tạm chỉ nhận `guest:<tên>` (server cấp ID khách), còn `playerId`/`name` do client gửi luôn bị bỏ qua. Thiếu token trả `auth_required_use_join_token`, token sai trả `auth_invalid`. `room_ready` dùng riêng ở phòng chờ: khi ít nhất hai người đều ready, server đếm 3 giây rồi phát `round_start`; bỏ ready/rời phòng hủy đếm. Host có thể gửi `start` để bắt đầu ngay. `lock_ready` chỉ dùng trong pha đặt để khóa sớm. Ví dụ commit: `{"v":1,"type":"commit","roundId":1,"positionX":1,"positionY":2,"aimX":0,"aimY":1}`. Commit chỉ trả xác nhận riêng trong lúc đặt; sau khóa mới broadcast `reveal` và kết quả. Tin tối đa 1 KiB, tối đa 20 tin/giây/kết nối.

`quick_match` cũng cần `token`: người đầu nhận `quick_match_waiting`; người thứ hai tạo một phòng thường gồm đúng hai người thật và cả hai được ready để đếm 3 giây. Sau 30 giây chưa có đối thủ, server chỉ gửi `quick_match_hint` (`no_opponent_yet`) — không thêm bot và không tự bắt đầu. Thời gian gợi ý hiện là hằng `ProtocolGateway.QuickMatchHintSeconds = 30`; client có thể rời hàng bằng `leave`, nhận `quick_match_left`, và sẽ không còn nhận gợi ý hay bị ghép lại.

Ví dụ được sinh bởi `ProtocolGateway` và đối chiếu trong test `ReadmeWireExamplesAreProducedByTheGateway`: `{"v":1,"type":"error","code":"not_enough_players"}`; `{"v":1,"type":"quick_match_waiting","quickMatchHintSeconds":30}`.

Server dùng đồng hồ riêng cho 5 giây đặt, kẹp vị trí vào sân, coi aim 0/NaN/vô cực là không ngắm để Core áp `ThrowFacing`; người mất kết nối được giữ 30 giây bằng reconnect token ngẫu nhiên 256-bit. Vào lại phải đồng thời có token xác thực của đúng người chơi; token vào lại được xoay sau khi dùng và không được ghi log. AFK là người **còn kết nối** không gửi commit hợp lệ trong 2 vòng liên tiếp: bị loại và cấm 120 giây qua `IBanStore` trong bộ nhớ; ngắt kết nối/qua hạn reconnect không bị cấm. `emote` chỉ hợp lệ trong pha đặt, giới hạn một lần/giây và ba lần/pha; payload phát chỉ có `playerId`, `emoteId`, `phase`, không có vị trí/hướng. Socket chậm có tối đa bốn lượt gửi chờ và bị ngắt khi timeout. WebSocket keep-alive là 30 giây; `IDLE_TIMEOUT_SECONDS` (mặc định 90) đóng socket im lặng và vẫn giữ chỗ 30 giây. `ALLOWED_ORIGINS` là danh sách Origin ngăn cách bằng dấu phẩy; mặc định chỉ localhost. Khi SIGTERM, server đóng các socket với lý do `server_shutdown`. Nhật ký chỉ ghi ID kết nối rút gọn, loại tin và lỗi — không ghi payload, vị trí, hướng hay token. `Dockerfile` và `render.yaml` chỉ là bản nháp, không tự triển khai.

Vector vàng 200 vòng nằm tại `server/Tests/golden/rounds.json`; chúng phân bố N=2..6. Cùng tệp còn có bảy vector trận, gồm người bị loại qua nhiều vòng và hai hòa cuối `maxRounds` (HP rồi số lần trúng, còn hòa thì đồng thắng). Test server và bản vá NUnit Unity cùng đọc định dạng trường công khai để so Core theo từng hit, người chơi và kết quả trận.

Z1 chạy Kestrel và `ClientWebSocket` thật với 20 trận luân phiên N=2..6 người, cùng một phòng N=1 bị từ chối `start`; có commit lỗi, một lượt không commit, vào lại và join thiếu token. Mỗi `round_result` được so với Core trực tiếp. Lần đo cục bộ được in trong output test (không phải số liệu Internet/Render).

Z3 (cục bộ, trong tiến trình, 2026-10-30) chạy đồng thời 50 phòng x 6 người x 5 vòng: `Passed! 1/1`, 0 ngoại lệ, 0 treo, 0 kết nối rớt; 250 vòng có trung bình 2.294 ms, p95 31.428 ms, lớn nhất 31.553 ms, 4,800 broadcast nội bộ, CPU 343.8 ms và peak working set 67.95 MiB. Đây là đo Core/session, **không** thay cho tải WebSocket/Internet; mục tiêu khởi đầu bảo thủ là 50 phòng mỗi process trước khi có benchmark mạng thực.

Render miễn phí có thể ngủ sau khoảng 15 phút không có truy cập, làm WebSocket bị ngắt. Chủ dự án cần chọn: gói trả phí, ping giữ ấm, hoặc chấp nhận hành vi này; chưa triển khai lựa chọn nào.

Z2 có sẵn tại `tools/patches/net_client_v1`: DTO thuần tương thích `JsonUtility`, loopback adapter và WebSocket Desktop; AG còn phải chép patch vào Unity và làm bridge `.jslib` cho WebGL. Z3 đã hoàn thành; chưa deploy server WIP này.

## Giao thức v1 đã đóng băng

Mỗi tin JSON có `v:int=1` và `type:string`. `v` khác 1 nhận `error.code=protocol_mismatch`. `join` trả `room_state` với `protocolVersion:int=1`; mọi `room_state` sau đó cũng có trường này. Client phải dừng và báo cập nhật game nếu không khớp. Giới hạn mỗi tin UTF-8 là 1024 byte và mỗi kết nối là 20 tin/giây. Dấu `?` nghĩa là tùy chọn; các số tọa độ là số hữu hạn, server kẹp vị trí vào sân. `token`/`reconnectToken` chỉ truyền qua kết nối an toàn khi triển khai thật.

| Client → Server | Trường ngoài `v,type` | Phản hồi/ý nghĩa |
|---|---|---|
| `join` | `token:string` bắt buộc; `roomCode?:string`, `reconnectToken?:string` | Tạo/vào phòng hoặc vào lại; trả riêng `room_state`. |
| `quick_match` | `token:string` bắt buộc | Xếp hàng người thật; `quick_match_waiting` hoặc `room_state`. |
| `room_ready` | `ready:bool` | Trong sảnh; broadcast `room_state`; tất cả sẵn sàng thì đếm 3 giây. |
| `start` | không thêm | Chủ phòng bắt đầu ngay nếu đủ 2 người. |
| `commit` | `roundId:int`, `positionX:number`, `positionY:number`, `hasAim?:bool`, `aimX?:number`, `aimY?:number` | Trong pha đặt; xác nhận riêng bằng `room_state`; không gửi dữ liệu đối thủ. `hasAim=false` phải đi cùng hướng 0 hoặc bỏ hướng; Core dùng `ThrowFacing`. |
| `lock_ready` | `ready:bool=true` | Khóa sớm ở pha đặt; đủ người thì phát kết quả. |
| `emote` | `emoteId:string` | Một trong `happy,sad,angry,surprised,thumbs_up`; giới hạn 1/giây và 3/pha. |
| `leave` | không thêm | Rời phòng hoặc hàng ghép nhanh. |

| Server → Client | Trường ngoài `v,type` | Ý nghĩa |
|---|---|---|
| `room_state` | `protocolVersion:int`, `roomCode:string`, `hostId:string`, `started:bool`, `players:[{id,name,ready}]`, `reconnectToken?:string`, `config:{maxPlayers,minPlayersToStart,startHp,placeSeconds,maxRounds,arenaWidth,arenaHeight,playerRadius}` | Trạng thái sảnh/xác nhận riêng; `reconnectToken` chỉ gửi cho chủ token. |
| `round_start` | `roundId:int`, `deadline:long` (Unix ms) | Đồng hồ đặt do server quyết định. |
| `reveal` | `roundId:int`, `players:[{id,position:{x,y},aim:{x,y}?}]` | Chỉ phát sau khóa. |
| `round_result` | `roundId:int`, `hits:[{shooterId,targetId,point:{x,y},distance}]`, `players:[{id,hp,eliminated}]` | Kết quả Core; có thể đi ngay sau `reveal`. |
| `match_end` | `winners:[string]` | Kết thúc trận; **chưa có `rankings` trên wire**. |
| `emote` | `playerId:string`, `emoteId:string`, `phase:string` | Trong pha đặt chỉ hiện trên thẻ người chơi, không có tọa độ. |
| `quick_match_waiting` / `quick_match_hint` / `quick_match_left` | `quickMatchHintSeconds?:int`, `code?:string` | Chờ, gợi ý sau 30 giây, hủy chờ. |
| `error` | `code:string`, `remainingSeconds?:int` | Xem mã dưới đây. |

`server_shutdown` và `idle_timeout` hiện là **lý do đóng WebSocket**, không phải tin JSON. `ready` cũ nhận `legacy_ready_use_room_ready_or_lock_ready`; không dùng nữa.

**Mã lỗi và gợi ý UI:** `not_enough_players` → Cần ít nhất 2 người; `protocol_mismatch` → Cập nhật game; `auth_required_use_join_token`, `auth_invalid`, `token_invalid` (nếu bộ xác thực trả), `reconnect_invalid` → Đăng nhập/vào lại; `room_full`, `room_limit`, `room_code_exhausted` → Thử phòng khác; `room_not_found`, `room_started` → Kiểm mã/chờ trận khác; `banned` (`remainingSeconds`) → Đợi hết cấm; `rate_limited`, `message_too_large` → Gửi chậm hơn; `legacy_ready_use_room_ready_or_lock_ready` → Cập nhật game. Các mã kỹ thuật còn lại (`invalid_json`, `invalid_envelope`, `unknown_type`, `already_joined`, `not_joined`, `not_in_room`, `not_in_round`, `wrong_phase`, `wrong_round`, `round_locked`, `ready_required`, `match_not_started`, `invalid_commit`, `invalid_emote`, `emote_rate_limited`, `emote_phase_limit`, `host_only`, `duplicate_player`, `invalid_player`, `no_response`) → Hiển thị thông báo thao tác không hợp lệ và ghi mã lỗi khi báo lỗi. HTTP handshake còn có `origin_not_allowed`/giới hạn kết nối.

**Nhật ký thay đổi giao thức:** v1 (2026-10-31): đóng băng tên tin hiện có, thêm `protocolVersion` vào `room_state`, trả `protocol_mismatch` cho phiên bản sai. Thay đổi cấu trúc/tên tin từ đây phải tăng `ProtocolGateway.Version` và cập nhật cả DTO, test, bảng này.
