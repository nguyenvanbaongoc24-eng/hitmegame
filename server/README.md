# HIT ME authoritative server

## Trạng thái tích hợp Supabase (2026-11-02)

Project URL do chủ dự án cung cấp: `https://lpylrbtpruyzdehkcaiu.supabase.co`. URL công khai này **không** phải khóa truy cập. Chưa có `SUPABASE_DB_URL` hay server-only secret trong biến môi trường và chưa xác nhận các migration SQL đã chạy trên project; vì vậy C5b chưa thể kiểm thử trên Supabase thật. Bản SQL `003_functions.sql` còn công thức thưởng cũ và không được chạy để phát thưởng. Bảng thưởng được duyệt ngày 2026-11-02 đã nằm trong `ApprovedGoldByRoomSize` (test 2–6 người); luồng trận vẫn chỉ lưu kết quả trong RAM, chưa cộng vào ví. C8 có `FileBanStore` bật bằng `BAN_STORE_PATH` để giữ cấm AFK sau khi khởi động lại trên một máy; khi triển khai Render hoặc nhiều replica cần kho bền dùng chung. Không đưa khóa vào Git, client hay chat.

Xác thực khách có chữ ký: đặt `GUEST_TOKEN_SIGNING_KEY` (base64 của ít nhất 32 byte ngẫu nhiên) trên server; gọi `POST /guest-token` với `{"name":"Nam"}`, rồi dùng token trả về cho `join` hoặc `quick_match`. Lưu token phía client để giữ danh tính sau khởi động lại. Khi không đặt biến này, `guest:<name>` cũ chỉ phục vụ test cục bộ. Token này không phải Supabase Auth và chưa sở hữu ví vàng.

`MD/NET_PROTOCOL.md` mô tả wire v1 cho AG. `MD/DEPLOY_RENDER.md` là hướng dẫn nháp; `server/render.yaml` đã qua phân tích cú pháp YAML, chưa Docker build hoặc triển khai.

Migration Supabase: tệp nguồn theo thứ tự `backend/supabase/001_schema.sql`, `002_rls.sql`, `003_functions.sql`, `004_match_result_store.sql`, `005_reward_and_bans.sql`. `seed.sql` chưa chạy vì giá vật phẩm chỉ là giả định. Đặt `SUPABASE_DB_URL` trong `.env` bị Git bỏ qua hoặc biến môi trường, dùng kết nối trực tiếp hay session pooler cổng 5432; chạy `python Tools/supabase/apply_migrations.py --check` trước, sau đó `--apply`. Script từ chối project khác, schema đã có nhưng thiếu ledger, hoặc checksum thay đổi; 5 file áp trong một transaction. Kiểm nhanh sau khi áp: `--check` phải báo `profiles`, `wallets`, `matches`, `match_players`, `server_bans` đều hiện diện. Test SQL cục bộ: `npm ci --prefix Tools/supabase && npm test --prefix Tools/supabase` (PGlite, không thay cho Supabase thật). Chưa có `SupabaseProfileStore`, chưa gọi RPC thưởng từ trận thật.

## Test cục bộ (Hub + Online Lab)

1. Từ `D:\HITME GAME\worktrees\codex-prep`, chạy `powershell -ExecutionPolicy Bypass -File tools\dev\run_local.ps1 -Open` (cần .NET 8 và Node 22+). Nếu 8100/8080 đang dùng, thêm `-HubPort 8110 -ServerPort 8081`.
2. Máy tính mở `http://localhost:8100/` (hoặc cổng đã chọn). Lệnh in địa chỉ LAN; điện thoại phải cùng Wi-Fi. Hub A là build Unity của AG **Luyện với bot**; Hub B là **Online Lab**, trang thử server thật 2–6 người, **Unity chưa nối mạng**.
3. Mở B trên hai tab hoặc hai thiết bị, dùng tên khách khác nhau. Một người tạo phòng, người kia nhập mã, rồi Sẵn sàng cả hai hoặc host bấm Bắt đầu. Một người không mở được trận; ghép nhanh chỉ ghép người thật.
4. Quan sát: (1) mã phòng và số người; (2) phòng 1 người bị chặn; (3) 3 giây đếm đầu; (4) 5 giây đặt/ngắm; (5) trước lộ diện không thấy vị trí/hướng đối thủ; (6) emote chỉ nằm trên thẻ khi đặt; (7) HP và bảng hạng đủ người; (8) ngắt rồi vào lại trong 30 giây.
5. Mẫu báo lỗi: `thiết bị / trình duyệt / N người / mã phòng / bước / lỗi / cảm nhận`; không gửi `token` hoặc `reconnectToken`.
6. Dừng bằng Ctrl+C ở cửa sổ lệnh. Nếu cổng chiếm, đổi cổng; nếu điện thoại không vào được, kiểm IP mới, cùng Wi-Fi và tường lửa. Lệnh cho tường lửa và lệnh xóa quy tắc được in bởi launcher, không tự thực thi.
7. Kiểm tự động khi launcher đang chạy: `node tools/dev/smoke_local.mjs http://127.0.0.1:8100`. Bằng chứng cục bộ: `Evidence/dev_lab/smoke_local.txt` và ảnh cùng thư mục. Chưa xác nhận trên điện thoại thật.

Nguồn luật duy nhất là các tệp được liên kết từ `Assets/Scripts/Core/Phase1` (nhánh Core hiện tại của checkout). Chạy: `dotnet run --project server/HitMe.Server.csproj`; dùng `PORT=8080` để chọn cổng. Kiểm tra: `GET /healthz`. Test: `dotnet test server/Tests/HitMe.Server.Tests.csproj --nologo`.

WebSocket dùng JSON `{ "v": 1, "type": "..." }`. Client gửi `join`, `room_ready`, `start`, `lock_ready`, `commit`, `emote`, `leave`; server gửi `room_state`, `round_start`, `reveal`, `round_result`, `match_end`, `emote`, `error`. Phòng online có 1–6 người thật; `room_state.config` trả `maxPlayers: 6` và `minPlayersToStart: 2`. Một người không thể `start` (`not_enough_players`) và tự ready cũng không mở đếm ngược; UI cần dẫn người đó tới **Luyện với bot** cục bộ, không qua server. `join` bắt buộc có `token`; bản tạm chỉ nhận `guest:<tên>` (server cấp ID khách), còn `playerId`/`name` do client gửi luôn bị bỏ qua. Thiếu token trả `auth_required_use_join_token`, token sai trả `auth_invalid`. `room_ready` dùng riêng ở phòng chờ: khi ít nhất hai người đều ready, server đếm 3 giây rồi phát `round_start`; bỏ ready/rời phòng hủy đếm. Host có thể gửi `start` để bắt đầu ngay. `lock_ready` chỉ dùng trong pha đặt để khóa sớm. Ví dụ commit: `{"v":1,"type":"commit","roundId":1,"positionX":1,"positionY":2,"aimX":0,"aimY":1}`. Commit chỉ trả xác nhận riêng trong lúc đặt; sau khóa mới broadcast `reveal` và kết quả. Tin tối đa 1 KiB, tối đa 20 tin/giây/kết nối.

`quick_match` cũng cần `token`: người đầu nhận `quick_match_waiting`; người thứ hai tạo một phòng thường gồm đúng hai người thật và cả hai được ready để đếm 3 giây. Sau 30 giây chưa có đối thủ, server chỉ gửi `quick_match_hint` (`no_opponent_yet`) — không thêm bot và không tự bắt đầu. Thời gian gợi ý hiện là hằng `ProtocolGateway.QuickMatchHintSeconds = 30`; client có thể rời hàng bằng `leave`, nhận `quick_match_left`, và sẽ không còn nhận gợi ý hay bị ghép lại.

Ví dụ được sinh bởi `ProtocolGateway` và đối chiếu trong test `ReadmeWireExamplesAreProducedByTheGateway`: `{"v":1,"type":"error","code":"not_enough_players"}`; `{"v":1,"type":"quick_match_waiting","quickMatchHintSeconds":30}`.

Server dùng đồng hồ riêng cho 5 giây đặt, kẹp vị trí vào sân, coi aim 0/NaN/vô cực là không ngắm để Core áp `ThrowFacing`; người mất kết nối được giữ 30 giây bằng reconnect token ngẫu nhiên 256-bit. Vào lại phải đồng thời có token xác thực của đúng người chơi; token vào lại được xoay sau khi dùng và không được ghi log. AFK là người **còn kết nối** không gửi commit hợp lệ trong 2 vòng liên tiếp: bị loại và cấm 120 giây qua `IBanStore` trong bộ nhớ; ngắt kết nối/qua hạn reconnect không bị cấm. `emote` chỉ hợp lệ trong pha đặt, giới hạn một lần/giây và ba lần/pha; payload phát chỉ có `playerId`, `emoteId`, `phase`, không có vị trí/hướng. Socket chậm có tối đa bốn lượt gửi chờ và bị ngắt khi timeout. WebSocket keep-alive là 30 giây; `IDLE_TIMEOUT_SECONDS` (mặc định 90) đóng socket im lặng và vẫn giữ chỗ 30 giây. `ALLOWED_ORIGINS` là danh sách Origin ngăn cách bằng dấu phẩy; mặc định chỉ localhost. Khi SIGTERM, server đóng các socket với lý do `server_shutdown`. Nhật ký chỉ ghi ID kết nối rút gọn, loại tin và lỗi — không ghi payload, vị trí, hướng hay token. `Dockerfile` và `render.yaml` chỉ là bản nháp, không tự triển khai.

Vector vàng 200 vòng nằm tại `server/Tests/golden/rounds.json`; chúng phân bố N=2..6. Cùng tệp còn có bảy vector trận, gồm người bị loại qua nhiều vòng và hai hòa cuối `maxRounds` (HP rồi số lần trúng, còn hòa thì đồng thắng). Test server và bản vá NUnit Unity cùng đọc định dạng trường công khai để so Core theo từng hit, người chơi và kết quả trận.

Z1 chạy Kestrel và `ClientWebSocket` thật với 20 trận luân phiên N=2..6 người, cùng một phòng N=1 bị từ chối `start`; có commit lỗi, một lượt không commit, vào lại và join thiếu token. Mỗi `round_result` được so với Core trực tiếp. Lần đo cục bộ được in trong output test (không phải số liệu Internet/Render).

Z3 (cục bộ, trong tiến trình, 2026-10-30) chạy đồng thời 50 phòng x 6 người x 5 vòng: `Passed! 1/1`, 0 ngoại lệ, 0 treo, 0 kết nối rớt; 250 vòng có trung bình 2.294 ms, p95 31.428 ms, lớn nhất 31.553 ms, 4,800 broadcast nội bộ, CPU 343.8 ms và peak working set 67.95 MiB. Đây là đo Core/session, **không** thay cho tải WebSocket/Internet; mục tiêu khởi đầu bảo thủ là 50 phòng mỗi process trước khi có benchmark mạng thực.

Z3 bổ sung (2026-10-31): 50 phòng hỗn hợp N=2..6 (10 phòng mỗi N), 200 người, 250 vòng, `Passed! 1/1` (`FiftyMixedSizeRoomsFromTwoToSixCompleteWithoutDropsOrExceptions`), 0 ngoại lệ/rớt/treo; 90.6 ms tổng, mỗi vòng trung bình 2.433 ms, p95 31.087 ms, tối đa 31.146 ms. Đây vẫn chỉ là trong tiến trình; Z1 đã chạy WebSocket thật với 20 trận hỗn hợp.

Render miễn phí có thể ngủ sau khoảng 15 phút không có truy cập, làm WebSocket bị ngắt. Chủ dự án cần chọn: gói trả phí, ping giữ ấm, hoặc chấp nhận hành vi này; chưa triển khai lựa chọn nào.

Sau khi chủ dự án tự triển khai, thử `node tools/deploy/smoke_test.mjs wss://TEN-MAY-CHU/ws` (Node 22+): công cụ tạo hai tài khoản khách tạm, chơi hết trận và in `Passed! 1/1` hoặc lỗi. Kiểm cục bộ bằng `node tools/deploy/smoke_test.mjs --local` (đã chạy 5 vòng). Chưa thử URL công khai, TLS, Render hoặc Vercel; không coi smoke test cục bộ là bằng chứng triển khai.

Z2 có sẵn tại `tools/patches/net_client_v1`: DTO thuần tương thích cấu trúc `JsonUtility`, loopback adapter và WebSocket Desktop cho .NET 8. Bản biên dịch `netstandard2.1` chứa WebSocket stub, vì vậy **chưa có WebGL transport**. Z3 đã hoàn thành; chưa deploy server WIP này.

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

`server_shutdown` và `idle_timeout` hiện là **lý do đóng WebSocket**, không phải tin JSON. `ready` cũ nhận `legacy_ready_use_room_ready_or_lock_ready`; không dùng nữa. Trường JSON thừa, thiếu, sai kiểu, không hữu hạn hoặc chuỗi quá dài nhận `bad_message`; cú pháp JSON hỏng nhận `invalid_json`. Mã phòng mới gồm sáu ký tự ngẫu nhiên mạnh từ `A–Z/2–9`, bỏ `I/L/O/0/1`.

**Mã lỗi và gợi ý UI:** `not_enough_players` → Cần ít nhất 2 người; `protocol_mismatch` → Cập nhật game; `auth_required_use_join_token`, `auth_invalid`, `token_invalid` (nếu bộ xác thực trả), `reconnect_invalid` → Đăng nhập/vào lại; `session_replaced` → Tài khoản vừa mở ở kết nối khác, socket cũ đóng sau thông báo; `room_full`, `room_limit`, `room_code_exhausted` → Thử phòng khác; `room_not_found`, `room_started` → Kiểm mã/chờ trận khác; `banned` (`remainingSeconds`) → Đợi hết cấm; `rate_limited`, `message_too_large` → Gửi chậm hơn; `legacy_ready_use_room_ready_or_lock_ready` → Cập nhật game. Các mã kỹ thuật còn lại (`bad_message`, `invalid_json`, `invalid_envelope`, `unknown_type`, `already_joined`, `not_joined`, `not_in_room`, `not_in_round`, `wrong_phase`, `wrong_round`, `round_locked`, `ready_required`, `match_not_started`, `invalid_commit`, `invalid_emote`, `emote_rate_limited`, `emote_phase_limit`, `host_only`, `duplicate_player`, `invalid_player`, `no_response`) → Hiển thị thông báo thao tác không hợp lệ và ghi mã lỗi khi báo lỗi. HTTP handshake còn có `origin_not_allowed`/giới hạn kết nối.

**Nhật ký thay đổi giao thức:** v1 (2026-10-31): đóng băng tên tin hiện có, thêm `protocolVersion` vào `room_state`, trả `protocol_mismatch` cho phiên bản sai. Thay đổi cấu trúc/tên tin từ đây phải tăng `ProtocolGateway.Version` và cập nhật cả DTO, test, bảng này.

Đợt 30 (2026-11-02): giữ v1; thêm lỗi `bad_message`, mã phòng khó nhầm và giới hạn tài nguyên cấu hình được. Mẫu SQL `backend/supabase/004_match_result_store.sql` đã qua PGlite `Passed! 4/4` với bảng mô phỏng, ghi một lần và ID khách; **chưa** thử trên PostgreSQL/Supabase thật và chưa nối adapter vào server.

**Cấu hình tiến trình** (số không hợp lệ dùng mặc định):

| Biến | Mặc định | Công dụng |
|---|---:|---|
| `PORT` | 8080 | Cổng Kestrel, nghe trên mọi interface. |
| `ALLOWED_ORIGINS` | `http://localhost,http://127.0.0.1` | Origin WebSocket được phép, phân tách bằng dấu phẩy. |
| `IDLE_TIMEOUT_SECONDS` | 90 | Đóng socket không hoạt động. |
| `MAX_ROOMS` | 200 | Tối đa phòng cùng lúc. |
| `MAX_CONNECTIONS` | 500 | Tối đa socket cùng lúc. |
| `MAX_CONNECTIONS_PER_IP` | 12 | Tối đa socket từ một địa chỉ IP; cần xem lại khi nhiều người dùng chung NAT. |
| `JOIN_TIMEOUT_SECONDS` | 10 | Đóng socket chưa vào phòng. |
| `SHUTDOWN_ROUND_WAIT_SECONDS` | 6 | Khi tắt server, đợi vòng đặt đang mở kết thúc tối đa từng này giây rồi đóng socket. |

`GET /metrics` chỉ trả số kết nối, phòng, trận hoạt động, p95 thời gian gửi WebSocket gần đây và lỗi theo mã; không chứa người chơi, token hay tọa độ. Bản nháp Render nên bắt đầu với giới hạn trên rồi đo tải trên gói thực tế; số đo 50 phòng trong tiến trình không bảo đảm 50 phòng WebSocket trên Render. `IMatchResultStore` mặc định là bộ nhớ, ghi một lần theo `matchId` và không bền qua khởi động lại; `NoReward` là chính sách mặc định, chưa cộng vàng. Chưa có adapter Supabase, danh sách phòng mở, hạn sống 10 phút cho phòng trống hay giới hạn toàn cục riêng cho socket chưa join. Bộ chaos hiện gồm 5 hạt giống × 200 phòng trong tiến trình; chưa thay cho kiểm tải mạng thật. Không tuyên bố SERVER READY v1 khi các mục này chưa hoàn tất.

Đang dở: H3 kiểm seed nội bộ; H6 chaos qua WebSocket; R3 adapter bền vững và xác nhận SQL trên Supabase; T3 bộ smoke triển khai bao phủ đủ Online Lab. H4 đã có bài takeover WebSocket thật; H5 đợi vòng khi tắt đã kiểm bằng đồng hồ cục bộ, chưa thử SIGTERM thật trong môi trường triển khai. Giới hạn hiện tại không áp quy tắc "một commit mỗi vòng" vì người chơi được đặt/ngắm lại trong 5 giây; tọa độ ngoài sân vẫn được Core kẹp theo luật hiện hành. Cần quyết định luật riêng trước khi đổi hai hành vi này.

**AG tích hợp online (đề xuất tuần tự):**

1. Lấy commit mới nhất `prep/codex`; giữ Unity/Core của AG, không chép lại luật.
2. Chép DTO, transport, session từ `tools/patches/net_client_v1` và `tools/patches/unity_online_glue` vào assembly riêng.
3. Gắn `IOnlineUi` vào màn sảnh, trận, kết quả; báo rõ `protocol_mismatch`.
4. Thêm adapter `JsonUtility`, kiểm mọi DTO round-trip trong EditMode.
5. Chép `HitMeWebSocket.jslib`, gắn callback Unity trên main thread.
6. Tạo `NetConfig.Enabled=false`; thử `LoopbackTransport` trước.
7. Mở Online Lab song song với Unity, đối chiếu `join` → `room_ready`/`start` → `round_start` → `commit` → `reveal` → `round_result` → `match_end`.
8. Xác nhận trước `reveal` Unity chỉ thấy nhân vật mình và emote trên thẻ.
9. Chạy EditMode, PlayMode, WebGL có đồ họa; sau đó thử iPhone thật và vào lại sau mất mạng.
10. Chỉ bật cờ online khi có bằng chứng build, bảo mật và trận 2–6 người.

## Tích hợp Unity — gói cho AG

Lấy `prep/codex` tối thiểu tại `16b6c86` (sau đó lấy HEAD mới nhất của nhánh). Chép **ba** tệp `tools/patches/net_client_v1/{NetClientDtos,MatchTransports,ClientMatchSession}.cs` vào `Assets/Scripts/Net/`. Tạo `HitMe.NetClient.asmdef` tham chiếu được từ assembly giao diện; tệp vá thuần C# không cần `UnityEngine`, còn adapter `IProtocolJson` của AG dùng `UnityEngine.JsonUtility` nên để trong assembly Unity riêng hoặc bỏ `noEngineReferences` cho assembly đó. AG tạo `NetConfig.Enabled` mặc định `false` để nhánh online chỉ mở khi có server/transport hoạt động; không đưa token thật vào repo.

K3/Z6: chép `tools/patches/net_client_v1/HitMeWebSocket.jslib` vào `Assets/Plugins/WebGL/`; AG viết lớp Unity P/Invoke cho `HitMeWsOpen(url, receiverName)`, `HitMeWsIsOpen(handle)`, `HitMeWsSend(handle, json)`, `HitMeWsClose(handle)` và bốn callback `OnHitMeWsOpen/Message/Error/Close(string)` trên GameObject nhận. Test `npm ci && npm test` trong `tools/patches/net_client_v1/browser-tests` đã chạy **hai trang Chrome thật** với server WebSocket, qua join/start/commit/reveal/result và quét bí mật trước khóa. Đây là test JS bridge, **chưa phải WebGL build Unity hoặc thử iPhone**.

Thứ tự: (1) thêm DTO và codec JsonUtility, đọc phong bì `{v,type}` trước; (2) dùng `LoopbackTransport(codec, callback)` để thử chuỗi tin trong EditMode, callback trả đúng các payload của **client đó**; nó không tự tạo phòng hoặc thay thế server; (3) dùng `ClientMatchSession` để cập nhật màn; (4) thêm transport WebGL bằng `.jslib` và chuyển callback về Unity main thread; (5) mới bật `NetConfig.Enabled`. `WebSocketTransport` trong patch chỉ hoạt động ở .NET 8 Desktop; Unity `netstandard2.1` sẽ ném `PlatformNotSupportedException`. Chưa có tự vào lại khi rớt mạng, xếp hạng trên wire hay codec Unity đã chạy trong Editor.

| `ClientMatchSession` nhận | Pha/sự kiện UI cần xử lý |
|---|---|
| `quick_match_waiting`, `quick_match_hint`, `quick_match_left` | Chờ ghép / hiện gợi ý Luyện bot / trở về sảnh. |
| `room_state` | Phòng chờ: mã, danh sách, ready, host, config; khi nhận sau `commit` trong trận thì chỉ xác nhận, không quay về sảnh. Kiểm `protocolVersion`. |
| `round_start` | `Placing`, lấy `roundId` và `deadline` Unix ms từ server. |
| `reveal` | `Revealed`, lúc này mới vẽ vị trí/hướng đối thủ. |
| `round_result` | `RoundResult`, cập nhật HP/hit; server có thể gửi `round_start` kế tiếp ngay sau đó. |
| `match_end` | `MatchEnd`, hiện người thắng; bảng xếp hạng phải tính từ Core hoặc bổ sung giao thức v2 sau khi thống nhất. |
| `emote`, `error` | Cập nhật thẻ biểu cảm; hiển thị mã lỗi phù hợp. |

Kiểm tra ngoài Unity: `dotnet build tools/patches/net_client_v1/HitMe.NetClientV1.csproj -f netstandard2.1` và `dotnet test tools/patches/net_client_v1/tests/HitMe.NetClientV1.Tests.csproj --nologo`. Các test dùng codec tương đương qua phản xạ, **chưa phải test `UnityEngine.JsonUtility` thực tế**. AG cần chạy EditMode/PlayMode và WebGL có đồ họa trước khi báo online đạt.

Gói nối UI `tools/patches/unity_online_glue/OnlineFlowController.cs` dùng cùng `ClientMatchSession`, có `IOnlineUi` để AG gắn giao diện. `dotnet test tools/patches/unity_online_glue/tests/HitMe.OnlineGlue.Tests.csproj` kiểm gateway thật và luồng N=2..6. Vào lại qua `Reconnect()` dùng token riêng sau khi socket kết nối lại; **chưa có vòng thử kết nối tự động**. Server nay phát lại `room_state` khi vào/rời phòng và `round_start` kèm deadline khi vào lại giữa pha đặt.

Triển khai nháp: Dockerfile chạy UID không phải root và kiểm `/healthz`; `render.yaml` chưa triển khai. Trên Render, tạo Web Service từ repo và chọn Dockerfile `server/Dockerfile` với build context ở gốc repo; đặt `ALLOWED_ORIGINS=https://<domain-WebGL>` (nhiều origin ngăn bằng dấu phẩy). `PORT` do host cấp hoặc đặt 10000; server lắng nghe `0.0.0.0:$PORT`. Các biến tùy chọn: `MAX_ROOMS=200`, `MAX_CONNECTIONS=500`, `MAX_CONNECTIONS_PER_IP=12`, `JOIN_TIMEOUT_SECONDS=10`, `IDLE_TIMEOUT_SECONDS=90`, `SHUTDOWN_ROUND_WAIT_SECONDS=6`. Sau khi chạy, kiểm `https://<server>/healthz`, rồi `node tools/deploy/smoke_test.mjs wss://<server>/ws` với origin được phép. Trước khi dùng thật cần thay guest auth, kết nối lưu trữ bền vững và kiểm thử restart; không đặt service role key vào client. Với Vercel, sao chép `tools/deploy/vercel.json` vào **gốc thư mục build WebGL chưa nén** trước khi tự triển khai static site; URL `wss://` phải là endpoint server riêng. Chưa chạy `docker build` vì máy này không có Docker, chưa triển khai Render/Vercel, chưa kiểm iPhone trên bản ghép online.
