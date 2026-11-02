# HIT ME protocol v2 handoff to Antigravity

Server branch: `prep/codex`. v1 remains accepted. Unity should send `v:2` with all messages after switching, and expect `v:2` in responses. The server selects the response version for each connection, including mixed v1/v2 rooms. Combat, placement, deadlines and room size are unchanged. See `MD/NET_PROTOCOL.md` for the full wire contract.

1. `join`/`quick_match` with `v:2` returns `room_state.protocolVersion:2`. Keep the private `reconnectToken` out of UI/logs. Reconnect with the same signed identity and token.
2. New canned chat: send `{v:2,type:"chat",chatId:"hello"}`. Supported IDs: `hello`, `good_luck`, `nice_throw`, `well_played`, `oops`, `thanks`. Render localized text from IDs. Incoming `{v:2,type:"chat",playerId,chatId}` has no world position; during placement show it in player cards, never above hidden characters. One message per second per player; handle `chat_rate_limited` and `invalid_chat_id`.
3. Final `{v:2,type:"match_end",matchId,winners,rankings}` includes every participant. Each ranking has `playerId`, `rank`, `hp`, `hitsLanded`, `goldReward`. Ties share rank and reward. Show the full table. Do not add `goldReward` to a local wallet: await the owner-only `profile_state` sent after `match_end`.
4. Friend request: send `{v:2,type:"friend_request",targetPlayerId}` to a player connected in the same room. The recipient receives `{v:2,type:"friend_request",playerId}` and replies `{v:2,type:"friend_reply",targetPlayerId:senderId,accepted:true|false}`. Both receive `friend_reply`. Current storage is memory only and disappears at server restart; show it as a temporary session feature until durable accounts/friends are added.
5. v2 clients should continue to handle all v1 match messages (`round_start`, `reveal`, `round_result`, `room_state`, `error`, `emote`) with the envelope version changed to 2. `commit` still uses `hasAim`, `aimX`, `aimY`; no position or aim is broadcast before lock.
6. `profile_state` is sent after join and after each finished match. `gold`, `matchesPlayed`, `bestRank` are authoritative. `rank` and `points` remain placeholders. In local mode, purchased items and equipment are authoritative; on Supabase they become authoritative only after migration 008 and `SUPABASE_SHOP_ENABLED=true`. Otherwise `inventoryStatus:"starter_only"` means do not offer purchases. Handle `profile_unavailable` as a retryable read failure.

7. R6 local contract: send `{ "v":2, "type":"buy_item", "requestId":"<UUID>", "itemId":"chao_ran" }`, or `equip_item` with the same fields, or `{ "v":2, "type":"rename_player", "requestId":"<UUID>", "newName":"Tên mới" }`. Reuse the exact `requestId` on retry; reusing it with different content returns `request_id_reused`. Success sends `shop_result` with `gold`, `displayName`, `renameCount`, `equippedWeaponId`, `equippedCharacterId`, followed by `profile_state`. Errors: `not_joined`, `invalid_shop_request`, `shop_unavailable`, `item_not_found`, `already_owned`, `not_owned`, `insufficient_gold`, `invalid_name`, `same_name`, `request_id_reused`. First rename is free; subsequent renames cost 200 gold (approved). Item prices come from server catalog, never client input. This is not yet verified against live Supabase.

8. On graceful restart, server sends `{ "v":1, "type":"error", "code":"server_restarting" }` before closing the socket with reason `server_restarting`. Show a short notice and return Home; do not award an unfinished match. A new server process does not restore in-memory rooms. `server_busy` means total room or connection capacity is full; `connection_limit` means the per-IP limit is reached.

9. `protocol_violation_eliminated` means the server rejected five malformed/invalid commits in one placement round and removed the player from the match. Valid commit revisions are still permitted until the deadline. Show a match removal notice; do not retry the same invalid packet.

### Mẫu payload v2 hiện có

Các giá trị dưới đây là ví dụ theo đúng trường mà server hiện phát; ID, số dư và mã phòng chỉ minh họa. `profile_state` chỉ đến chủ tài khoản:

```json
{"v":2,"type":"profile_state","playerId":"g_example","name":"Nam","gold":301,"matchesPlayed":2,"bestRank":1,"rank":"unranked","points":0,"purchasedItemIds":["chao_ran"],"ownedItemIds":["dep_to_ong","chao_ran"],"equippedWeaponId":"chao_ran","equippedCharacterId":null,"renameCount":1,"inventoryStatus":"authoritative"}
{"v":2,"type":"shop_result","requestId":"30000000-0000-4000-8000-000000000002","action":"equip","gold":301,"displayName":"Nam","renameCount":1,"equippedWeaponId":"chao_ran","equippedCharacterId":null}
{"v":2,"type":"chat","playerId":"g_example","chatId":"hello"}
{"v":2,"type":"friend_request","playerId":"g_example"}
{"v":2,"type":"friend_reply","playerId":"g_friend","targetPlayerId":"g_example","accepted":true}
```

`rank:"unranked"` và `points:0` là placeholder, **không** phải thứ hạng đã lưu. Server chưa phát thông điệp nhiệm vụ ngày hay đăng nhập Google; tính năng kết bạn hiện chỉ ở RAM và mất khi khởi động lại. AG có thể nối màn hồ sơ/số dư, mua/trang bị/đổi tên khi `inventoryStatus:"authoritative"`, nhưng chưa hiển thị rank/nhiệm vụ/kết bạn như dữ liệu bền vững. Không gửi mật khẩu, service key hay refresh token trong WebSocket.

Verification: `server/Tests/ProtocolV2Tests.cs` tests chat privacy, limits, friend authorization and versioned room state. `node server/Tools/FakeClient.mjs --local --v2` plays real WebSocket matches with 2, 4 and 6 players. This does not prove Unity WebGL integration or Render deployment.
