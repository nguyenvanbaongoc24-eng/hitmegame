# HIT ME network protocol v1, v2 and v3

## v3, gameplay rules v1.3

Use `v:3` for a v1.3 match. A dedicated v1.3 server should set `MIN_PROTOCOL_VERSION=3`; older clients then receive `error.code=client_outdated`. Do not set this on a backend shared with stable clients. The older examples below document legacy messages.

`round_start` adds `roundLabel` (`VÒNG 1` or `HIỆP PHỤ 1`), `tiebreak`, `tiebreakIndex` and `aliveCount`. `round_result` adds `roundLabel`, `tiebreak` (`active`, `index`, `pending`) and `eliminatedIds`. `match_end` adds `winnerIds`, `tiebreakCount`; each ranking adds `rank`, `hits`, `throws`, `accuracy`, `eliminatedRound`. Existing HP and hit fields remain. The server does not reveal opponent placement or aim before round lock.

Example: `{"v":3,"type":"match_end","winnerIds":["p1","p2"],"tiebreakCount":3,"rankings":[{"playerId":"p1","rank":1,"hits":2,"throws":4,"accuracy":0.5,"eliminatedRound":0}]}`. See server tests for full payloads.

Beta 0.1 handshake: the first WebSocket packet is `{"v":2,"type":"hello","serverVersion":"0.1.0-beta.1","minClientVersion":"0.0.0"}`. Send `clientVersion` with `join` or `quick_match`. A version below `MIN_CLIENT_VERSION` receives `error.code=client_outdated`, then a graceful socket close. `SERVER_VERSION` and `MIN_CLIENT_VERSION` are configurable server environment variables. The default minimum `0.0.0` accepts legacy clients until the updated Unity client is verified.

Beta room cleanup: an untouched lobby expires after ten minutes and each connected participant receives `error.code=room_expired` before socket close. Finished rooms may also expire after ten minutes. Return to Home and create or join another room. `/healthz` does not depend on Supabase. Default beta caps are 50 rooms and 120 sockets, overridable by `MAX_ROOMS` and `MAX_CONNECTIONS`; saturation returns `server_busy`.

Guest names: the signed token issuer accepts 2–24 Unicode letters/digits/spaces (including Vietnamese combining marks); rename requires 3–24. The server rejects configured `BANNED_NAME_TERMS` (comma separated; default admin, moderator, fuck), invalid symbols and duplicate names within one room (`name_taken_in_room`). The database still applies its own rename guard.

Protocol v2 is additive. A socket selects its version in `join` or `quick_match`; room broadcasts use each recipient's version. v1 clients remain accepted. A v2 client sends `v:2` on every message and verifies `v:2` in every response. Friend requests use a shared IFriendStore. In-memory mode resets on restart; Supabase mode requires migrations 009-010 and SUPABASE_FRIENDS_ENABLED=true.

## v2 additions

```json
{"v":2,"type":"chat","chatId":"hello"}
{"v":2,"type":"friend_request","targetPlayerId":"g_example"}
{"v":2,"type":"friend_reply","targetPlayerId":"g_example","accepted":true}
```

Allowed `chatId` values: `hello`, `good_luck`, `nice_throw`, `well_played`, `oops`, `thanks`. The server broadcasts `{v:2,type:"chat",playerId,chatId}` to v2 sockets in the same room, at most once per second per player. The client maps IDs to localized text; free-form text is rejected. Chat contains no position or aim, including during placement. Friend requests require both players to be connected in the same room on v2. Only the recipient may reply; both receive `{v:2,type:"friend_reply",playerId,targetPlayerId,accepted}`. Duplicate/pending requests return errors. Friend IDs are opaque; never expose tokens.

`match_end` on v2 includes `matchId` and a full `rankings` array: `{playerId,rank,hp,hitsLanded,goldReward}`. `goldReward` follows the approved 2/3–4/5–6 player table and is zero for AFK or disconnected players marked ineligible. It is the reward recorded by the server match result store; clients must read their current balance from the server, not add this field locally. The result store writes the match before broadcasting `match_end`.

On v2 `join`/`quick_match`, and after `match_end`, the server sends the owner-only `profile_state`: `{playerId,name,gold,matchesPlayed,bestRank,rank,points,purchasedItemIds,ownedItemIds,equippedWeaponId,equippedCharacterId,inventoryStatus}`. `gold`, `matchesPlayed`, and `bestRank` come from the server's profile store (Supabase when configured). Rank/points and the catalog-backed inventory are still pending: `rank:"unranked"`, `points:0`, `purchasedItemIds:[]`, `ownedItemIds:["dep_to_ong"]`, `equippedWeaponId:"dep_to_ong"`, `equippedCharacterId:null`, `inventoryStatus:"starter_only"` signal that state. Never treat these placeholder fields as durable ownership. A profile read error yields `error.code:"profile_unavailable"` without blocking the match result.

New v2 errors: `invalid_chat_id`, `chat_rate_limited`, `invalid_friend_target`, `friend_target_unavailable`, `friend_request_pending`, `friend_request_not_found`, `already_friends`. Friend errors also include friend_request_closed and friend_unavailable. Durable storage is gated by SUPABASE_FRIENDS_ENABLED=true after migrations 009-010. Existing match flow, privacy, room limits, and combat rules remain the same.

The server is authoritative. Use WebSocket `wss://<host>/ws` in production. Every JSON message has `v:1` and `type`. Payloads are limited to 1024 UTF-8 bytes and 20 messages per second per connection. Without `GUEST_TOKEN_SIGNING_KEY`, the server accepts `guest:<name>` for local testing. With that key, obtain a signed guest token from `POST /guest-token` (`{"name":"Nam"}`), then send it in `join.token`; the unsigned form is rejected. This is not yet Supabase Auth. Keep `token` and `reconnectToken` out of logs.

## Client messages

```json
{"v":1,"type":"join","token":"guest:Nam","roomCode":"HM4826","characterId":"nam_lang","weaponId":"dep_to_ong"}
{"v":1,"type":"room_ready","ready":true,"characterId":"co_ba","weaponId":"chao_ran"}
{"v":1,"type":"start"}
{"v":1,"type":"commit","roundId":1,"positionX":1.2,"positionY":-0.8,"hasAim":true,"aimX":0,"aimY":1}
{"v":1,"type":"lock_ready","ready":true}
{"v":1,"type":"emote","emoteId":"happy"}
{"v":1,"type":"leave"}
{"v":1,"type":"quick_match","token":"guest:Nam"}
```

For a new room, omit `roomCode`; for re-entry, include `roomCode` and the private `reconnectToken` received earlier. `commit` with `hasAim:false` and no aim coordinates invokes the server's `ThrowFacing` behavior. The old `aimDir:null` form is accepted by the gateway for compatibility. `room_ready` is for the lobby, `lock_ready` for the 5-second placement phase. Legacy `ready` returns `legacy_ready_use_room_ready_or_lock_ready`.

`characterId` and `weaponId` are optional on first `join` (defaults: `nam_lang`, `dep_to_ong`) and on `room_ready` (keeps the current selection). Selection is frozen after the room starts. The trial roster is `nam_lang`, `nu_nang_dong`, `co_ba`, `anh_teen`; operators may set `ENABLED_CHARACTER_IDS` to a comma-separated subset containing `nam_lang`. Trial characters are currently free to select; weapon ownership comes from the server profile/catalog. Different players may select the same character. Errors: `character_not_found`, `character_locked`, `weapon_not_found`, `weapon_not_owned`, `profile_unavailable`. A rejected selection does not change readiness or loadout.

## Server messages

```json
{"v":1,"type":"room_state","protocolVersion":1,"roomCode":"HM4826","hostId":"p1","started":false,"players":[{"id":"p1","name":"Nam","slot":0,"ready":false,"characterId":"nam_lang","weaponId":"dep_to_ong"}],"reconnectToken":"private","config":{"maxPlayers":6,"minPlayersToStart":2,"startHp":3,"placeSeconds":5,"maxRounds":5,"arenaWidth":8,"arenaHeight":12,"playerRadius":0.4}}
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

Examples show shapes, not exact identifiers or final arena numbers. Always read the live `room_state.config`. `reconnectToken` is private to its owner. The original v1 contract promised winners only; the current server also sends additive `matchId` and `rankings` fields, which v1 clients may ignore.

`room_state.players[].slot` is a server-assigned integer from 0 to 5. It stays with that player through reconnect and roster updates. When a player leaves before start, a new player may reuse the lowest free slot; existing players keep theirs. Every client should map the same slot to the same ring/HUD color from one shared client palette. A cyan local-player highlight, if desired, is a visual overlay only and must not replace the common slot identity. The server sends slot IDs, not presentation colors.

## Match order and privacy

`join` creates or enters a room (1–6 real humans; no online bots). At least two players must be present. All `room_ready:true` starts a 3-second countdown; the host may send `start` early. The server sends `round_start` with a Unix-millisecond deadline. Clients show a countdown using that server deadline. During placement, `commit` receives a private acknowledgment; opponents never receive coordinates or aim. All `lock_ready` or the deadline locks the round. Only then does the server broadcast `reveal`, `round_result`, and either the next `round_start` or `match_end`. A disconnected player can rejoin within 30 seconds with both the same identity and reconnect token. Rejoining does not disclose other players' prelock commits. `emote` in placement contains no location; show it on the player card only.

Privacy decision: `room_state.players[].ready` describes **lobby** readiness only. The server does not broadcast a player's placement commit, lock status, commit count, aim, coordinates, or submit timestamp to opponents before `reveal`; the submitting player alone receives a fixed-shape `room_state` acknowledgment. Reconnect during placement receives the same public roster and server deadline, never another player's locked input. The first placement detail visible to everyone is the identical `reveal` payload broadcast after the round locks.

## Errors and close reasons

`room_full`, `room_limit`, `room_not_found`, `room_started`, `not_enough_players`, `host_only`, `wrong_phase`, `wrong_round`, `round_locked`, `invalid_commit`, `reconnect_invalid`, `session_replaced`, `banned` (`remainingSeconds`), `auth_required_use_join_token`, `auth_invalid`, `protocol_mismatch`, `bad_message`, `invalid_json`, `rate_limited`, and `message_too_large` are JSON `error.code` values. `idle_timeout` is a WebSocket close reason; `server_restarting` is both a JSON error code and a close reason. On `protocol_mismatch`, stop play and request an update. Error codes are stable for v1; structural wire changes require a version bump.

Repeated malformed or invalid `commit` packets in the same placement round lead to `protocol_violation_eliminated` on the fifth attempt; the player is removed from match eligibility without an AFK ban. Valid replacement commits within the 5-second window remain allowed. An old round ID or a commit after lock returns the existing error and does not count toward this threshold, since delayed packets can be legitimate. The server audits room ID, match ID and error code without logging payloads or credentials.

Current gaps: Unity WebGL network integration, Supabase Auth, persistent friend graph, and an authoritative profile/balance message. Signed guest gold persistence is tested locally against Supabase; Render deployment is unverified. The browser Online Lab and server WebSocket tests exercise the protocol; they do not prove the Unity client is connected.

The CLI client `node server/Tools/FakeClient.mjs --local` also exercised the actual v1 sequence for three rooms of 2, 4 and 6 players on 2026-11-02 (`Evidence/round40/C10_fakeclient.txt`). It observed `match_end.winners` only. Its printed reward is an approved-table preview, not wallet credit.
