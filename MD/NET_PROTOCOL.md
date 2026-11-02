# HIT ME network protocol v1

The server is authoritative. Use WebSocket `wss://<host>/ws` in production. Every JSON message has `v:1` and `type`. Payloads are limited to 1024 UTF-8 bytes and 20 messages per second per connection. Without `GUEST_TOKEN_SIGNING_KEY`, the server accepts `guest:<name>` for local testing. With that key, obtain a signed guest token from `POST /guest-token` (`{"name":"Nam"}`), then send it in `join.token`; the unsigned form is rejected. This is not yet Supabase Auth. Keep `token` and `reconnectToken` out of logs.

## Client messages

```json
{"v":1,"type":"join","token":"guest:Nam","roomCode":"HM4826"}
{"v":1,"type":"room_ready","ready":true}
{"v":1,"type":"start"}
{"v":1,"type":"commit","roundId":1,"positionX":1.2,"positionY":-0.8,"hasAim":true,"aimX":0,"aimY":1}
{"v":1,"type":"lock_ready","ready":true}
{"v":1,"type":"emote","emoteId":"happy"}
{"v":1,"type":"leave"}
{"v":1,"type":"quick_match","token":"guest:Nam"}
```

For a new room, omit `roomCode`; for re-entry, include `roomCode` and the private `reconnectToken` received earlier. `commit` with `hasAim:false` and no aim coordinates invokes the server's `ThrowFacing` behavior. The old `aimDir:null` form is accepted by the gateway for compatibility. `room_ready` is for the lobby, `lock_ready` for the 5-second placement phase. Legacy `ready` returns `legacy_ready_use_room_ready_or_lock_ready`.

## Server messages

```json
{"v":1,"type":"room_state","protocolVersion":1,"roomCode":"HM4826","hostId":"p1","started":false,"players":[{"id":"p1","name":"Nam","ready":false}],"reconnectToken":"private","config":{"maxPlayers":6,"minPlayersToStart":2,"startHp":3,"placeSeconds":5,"maxRounds":5,"arenaWidth":8,"arenaHeight":12,"playerRadius":0.4}}
{"v":1,"type":"round_start","roundId":1,"deadline":1790000000000}
{"v":1,"type":"reveal","roundId":1,"players":[{"id":"p1","position":{"x":1.2,"y":-0.8},"aim":{"x":0,"y":1}}]}
{"v":1,"type":"round_result","roundId":1,"hits":[{"shooterId":"p1","targetId":"p2","point":{"x":0,"y":0},"distance":2}],"players":[{"id":"p1","hp":3,"eliminated":false}]}
{"v":1,"type":"match_end","winners":["p1"]}
{"v":1,"type":"emote","playerId":"p1","emoteId":"happy","phase":"Placing"}
{"v":1,"type":"error","code":"not_enough_players"}
{"v":1,"type":"quick_match_waiting","quickMatchHintSeconds":30}
{"v":1,"type":"quick_match_hint","code":"no_opponent_yet"}
{"v":1,"type":"quick_match_left"}
```

Examples show shapes, not exact identifiers or final arena numbers. Always read the live `room_state.config`. `reconnectToken` is private to its owner. `match_end` has winners but **no rankings field** in v1.

## Match order and privacy

`join` creates or enters a room (1–6 real humans; no online bots). At least two players must be present. All `room_ready:true` starts a 3-second countdown; the host may send `start` early. The server sends `round_start` with a Unix-millisecond deadline. Clients show a countdown using that server deadline. During placement, `commit` receives a private acknowledgment; opponents never receive coordinates or aim. All `lock_ready` or the deadline locks the round. Only then does the server broadcast `reveal`, `round_result`, and either the next `round_start` or `match_end`. A disconnected player can rejoin within 30 seconds with both the same identity and reconnect token. Rejoining does not disclose other players' prelock commits. `emote` in placement contains no location; show it on the player card only.

## Errors and close reasons

`room_full`, `room_limit`, `room_not_found`, `room_started`, `not_enough_players`, `host_only`, `wrong_phase`, `wrong_round`, `round_locked`, `invalid_commit`, `reconnect_invalid`, `session_replaced`, `banned` (`remainingSeconds`), `auth_required_use_join_token`, `auth_invalid`, `protocol_mismatch`, `bad_message`, `invalid_json`, `rate_limited`, and `message_too_large` are JSON `error.code` values. `idle_timeout` and `server_shutdown` are WebSocket close reasons. On `protocol_mismatch`, stop play and request an update. Error codes are stable for v1; structural wire changes require a version bump.

Current gaps: Unity WebGL network integration, Supabase Auth, persistent profiles and gold, and ranked `match_end` payload. The browser Online Lab and server WebSocket tests exercise the protocol; they do not prove the Unity client is connected.

The CLI client `node server/Tools/FakeClient.mjs --local` also exercised the actual v1 sequence for three rooms of 2, 4 and 6 players on 2026-11-02 (`Evidence/round40/C10_fakeclient.txt`). It observed `match_end.winners` only. Its printed reward is an approved-table preview, not wallet credit.
