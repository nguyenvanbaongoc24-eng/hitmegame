# Codex preparation status (2026-11-02)

Branch `prep/codex`; Unity and Core are not edited here.

| Task | Status | Evidence |
|---|---|---|
| R1 Render health | Local server verified; live Render unresolved | `MD/DEPLOY_RENDER.md`: local HTTP 200 in 329 ms, public URL timed out at 35 s; Docker unavailable |
| R4 protocol v2 | Implemented in server; Unity integration pending | `MD/PROTOCOL_V2_CHANGES.md`, server tests 143/143, WebSocket FakeClient v2 3/3 (no Supabase env) |
| R5 authoritative profile/balance | Partial | `profile_state` sends server gold, match count and best rank after join/match; rank points and durable inventory pending R6/R7. Server tests 145/145; FakeClient v2 with Supabase verified 3/3. |
| R6 shop and rename | Local implementation; live migration pending | `backend/supabase/008_guest_shop.sql`, `server/Tests/GuestShopTests.cs`; first rename free, later renames 200 gold; PGlite 20/20, server 148/148. Supabase migration 008 not applied because `SUPABASE_DB_URL` is not yet available. Set `SUPABASE_SHOP_ENABLED=true` only after applying 008. |
| R3 live AFK ban adapter contract | Partial | `Evidence/round43/R3_ban_supabase.txt`: 5/5 live checks; fresh adapter proves database persistence, but full server process restart was not exercised. Test-prefixed row removed. |
| R2 test data cleanup/security scan | Pending | Live test data remains; do not delete real users |
| R7 rank/daily quests | Pending | No server handlers yet |

L0–L7 handoff: L0 verified directly on 2026-11-02. L1 partially underway (R6 local); R7 and durable friends pending. L2–L7 not completed. Do not present local shop tests as proof of live Supabase or Render behavior.

L4 local work: shutdown now sends `server_restarting` then closes the socket; total capacity returns `server_busy`; `/metrics` includes median round processing time; manual backup script and `MD/OPS.md` added. Server tests 149/149. SIGTERM, capacity and backup have not been exercised on Render; `pg_dump` is absent locally.

Owner action: rotate the database password previously exposed in chat before opening the service to external users; keep replacement secrets in local/deployment environment only.
