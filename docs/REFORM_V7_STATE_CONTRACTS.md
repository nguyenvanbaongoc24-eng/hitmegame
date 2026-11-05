# HIT ME V7 — hợp đồng trạng thái

## Màn và dữ liệu

| Trạng thái | Nguồn xác nhận | Chuyển tiếp hợp lệ | Lỗi / rời |
|---|---|---|---|
| Loading | Unity loader tải đủ `.data`, `.wasm`, mã khởi tạo và canvas | Home sau khi Unity khởi chạy | Hiện lỗi và nút thử lại; không tự ghi “E2E đã chạy”. |
| Home | `ProductionUIController` | CreateRoom, JoinRoom, BotTraining; hộp Profile/Settings | Nút chưa phát hành phải thông báo rõ. |
| Create/Join | Token khách do `POST /guest-token` cấp; `room_state` từ server | Lobby chỉ sau `room_state` | Timeout và lỗi giữ người chơi ở màn hiện tại, không tự dựng phòng giả. |
| Quick Match | Phản hồi `quick_match_waiting` | Lobby khi nhận `room_state` | Cancel gửi yêu cầu rời hàng chờ; gợi ý bot không tự ghép bot online. |
| Lobby | `room_state.players`, `hostId`, ready từ server | Vào trận khi server gửi `round_start` | Rời phòng và mất kết nối theo protocol. |
| Placing | `round_start.roundId` và `deadline` theo đồng hồ server | Gửi `commit`, tùy chọn `lock_ready`; server khóa sau hạn hoặc tất cả khóa | Trước khóa không hiển thị vị trí/hướng của đối thủ. |
| Reveal/Throwing/Result | `reveal` rồi `round_result` | Vòng tiếp hoặc `match_end` | Chỉ Core/server tính trúng, HP và xếp hạng. |
| MatchEnd | `match_end` | Chơi lại hoặc Home | Không cộng vàng trong client từ kết quả tự dự đoán. |

Gameplay offline dùng `Assets/Scripts/Core/Phase1` qua `MatchController`. Online dùng `Assets/Scripts/Net/OnlineGlue`, `WebSocketGameClient` và server có thẩm quyền. Mọi thông điệp online có `{v,type}`; xem `MD/NET_PROTOCOL.md` để biết các trường và mã lỗi hiện ghi trong repo. Tài liệu giao thức đó vẫn mang nhãn v1; cần đối chiếu với server đang triển khai trước khi khẳng định v2 đã nối xong.

Các cổng kiểm nhận: server `/healthz` hoạt động; `/guest-token` phải trả token hợp lệ; hai client nhận cùng `room_state`; không client nào nhận tọa độ đối thủ trước `reveal`; trận kết thúc đúng Core; số dư thật chỉ đổi sau phản hồi server. Hiện cổng `/guest-token` chưa đạt ở Render.
