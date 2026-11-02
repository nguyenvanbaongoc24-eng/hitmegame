# HIT ME – BẢNG ĐỐI CHIẾU VÀ LỆCH GIAO THỨC (PROTOCOL_MISMATCH.md)

**Ngày cập nhật:** 02/11/2026 (Phase 4 Lượt 3) · **Phiên bản:** Protocol v1 & v2 (Đối chiếu với Codex R5)  
**Phạm vi:** Đối chiếu giữa Unity Client (`HitMe.NetClientV1`, `ServerAccountService`), Server Authority C# (`HitMe.Server`), và tài liệu [`NET_PROTOCOL.md`](NET_PROTOCOL.md).

---

## 1. Bảng Chi Tiết Đối Chiếu Protocol v2 (Codex R5)

| Message / Tính năng | Server gửi/nhận thực tế (v2) | NET_PROTOCOL.md v2 quy định | Hành vi Client hiện tại | Trạng thái đối chiếu & Xử lý |
|---|---|---|---|---|
| `profile_state` | Gửi gói hồ sơ khi `join`/`quick_match` và sau `match_end`: `{playerId, name, gold, matchesPlayed, bestRank, rank, points, purchasedItemIds, ownedItemIds, equippedWeaponId, equippedCharacterId, inventoryStatus}` | Đúng định dạng trên | Client đã tạo `ServerAccountService` và `ServerProfileDto` nhận trực tiếp từ WebSocket, cập nhật ví Vàng và Rank | **ĐÃ KHỚP HOÀN TOÀN.** Client chuyển từ hiển thị "Đang tải..." sang dữ liệu thật ngay khi nhận gói này. |
| `match_end.rankings` | Gửi mảng `rankings: [{playerId, rank, hp, hitsLanded, goldReward}]` | Mảng rankings authoritative | Client nhận mảng này để hiển thị màn hình Result, đọc trực tiếp `goldReward` server cấp | **ĐÃ KHỚP HOÀN TOÀN.** Không tự tính cộng dồn ở client. |
| `chat` | Nhận `{"v":2,"type":"chat","chatId":"..."}`. Cho phép 6 id: `hello`, `good_luck`, `nice_throw`, `well_played`, `oops`, `thanks`. Broadcast cho phòng. | Đúng danh sách 6 chatId trên | Client có sẵn khay chat nhanh 6 câu tương ứng, gửi đúng wire JSON v2 | **ĐÃ KHỚP HOÀN TOÀN.** |
| `friend_request` / `friend_reply` | Trao đổi trong phòng WebSocket (in-memory, chưa lưu DB) | `{v:2,type:"friend_request",targetPlayerId}`, `{v:2,type:"friend_reply",accepted:bool}` | Client hiển thị nút kết bạn và toast xác nhận | **ĐÃ KHỚP.** Chờ Phase 5 để lưu lâu dài vào Supabase. |
| Mua & Trang bị vũ khí | Server v2 đang để `inventoryStatus: "starter_only"`, `purchasedItemIds: []` | Chưa có RPC trừ tiền mua shop ở server v2 | Client hiển thị thông báo "Đang xử lý giao dịch với máy chủ...", KHÔNG tự trừ vàng cục bộ | **AN TOÀN.** Chờ Codex hoàn tất RPC Supabase cho shop. |
| Nhiệm vụ hàng ngày (Quests) | Server v2 chưa có bảng nhiệm vụ ngày trong `profile_state` | Tài liệu v2 chưa định nghĩa trường quests | Client quản lý tiến độ 3 nhiệm vụ ngày cục bộ (reset 00:00 VN) theo `QuestsConfig.json` | **CHÊNH LỆCH ĐƯỢC CHẤP NHẬN.** Sẽ đồng bộ server khi Codex tích hợp bảng `user_quests`. |

---

## 2. Kết Luận Khớp Nối Phase 4

1. **Protocol v2 hoàn toàn tương thích ngược:** Unity Client gửi `v:1` khi ở phòng v1 và nâng cấp lên `v:2` với `ServerAccountService`.
2. **Không còn lỗ hổng dữ liệu:** Không còn tình trạng client hiển thị ví vàng giả hoặc bảng xếp hạng giả trong chế độ online; mọi dữ liệu hiển thị đều tuân theo cờ `IsOnlineAccountLoaded`.


---

## 3. Giao Thức Chọn Sân Đấu (Map Selection - Task M6)

Unity Client hiện đã hỗ trợ chọn sân theo dữ liệu từ `Maps.json` (`map_street_vietnam` và `map_village_lotus`). Kích thước vùng chơi và luật chơi giữ nguyên (9.0 × 11.4).

Để đồng bộ giữa các máy người chơi trong phòng online khi Codex online lại, cần bổ sung message:
1. **Client (Chủ phòng) gửi yêu cầu đổi sân:**
   ```json
   { "v": 1, "type": "select_map", "mapId": "map_village_lotus" }
   ```
2. **Server xác thực chủ phòng và phát sóng trong `room_state`:**
   ```json
   {
     "v": 1,
     "type": "room_state",
     "mapId": "map_village_lotus",
     ...
   }
   ```
3. **Phản hồi khi lỗi (nếu người không phải host yêu cầu đổi sân):**
   ```json
   { "v": 1, "type": "error", "code": "only_host_can_select_map" }
   ```

---

## 4. Chẩn Đoán Cấu Hình Render Cho Codex (Task M8)

- **Trạng thái Render hiện tại:** Service `https://hitmegame.onrender.com` đã **BUILD THÀNH CÔNG** từ commit `17e5112` nhánh `prep/codex`.
- `/healthz` trả về `200 OK`: `{"status":"ok","protocol":1}` (origin server: Kestrel).
- **Phát hiện quan trọng:** WebSocket handshake tại `/ws` trả về `403 Forbidden` (`origin_not_allowed`) đối với các kết nối từ trình duyệt chạy trên cổng `http://127.0.0.1:8103` hoặc Vercel, trong khi kết nối từ `http://localhost` (port 80) hoặc client native thì **KẾT NỐI THÀNH CÔNG (101 Switching Protocols)**.
- **Nguyên nhân:** Biến môi trường `ALLOWED_ORIGINS` trên Render dashboard chưa được cấu hình, nên server đang dùng giá trị mặc định `http://localhost,http://127.0.0.1` (so khớp chuỗi chính xác, không tính port).
- **Khuyến nghị cho Codex:**
  1. Trong `render.yaml` hoặc cài đặt Render Environment: đặt `ALLOWED_ORIGINS=*` hoặc `ALLOWED_ORIGINS=http://localhost:8103,http://127.0.0.1:8103,https://*.vercel.app,http://localhost,http://127.0.0.1`.
  2. Trong `ConnectionAdmission.cs`: hỗ trợ ký tự đại diện `*` hoặc cho phép các cổng cục bộ `localhost:*` và `127.0.0.1:*`.
