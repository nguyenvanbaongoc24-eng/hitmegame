# Codex preparation status (2026-11-02)

Branch `prep/codex`; Unity and Core are not edited here.

| Task | Status | Evidence |
|---|---|---|
| R1 Render health | Local server verified; live Render unresolved | `MD/DEPLOY_RENDER.md`: local HTTP 200 in 329 ms, public URL timed out at 35 s; Docker unavailable |
| R4 protocol v2 | Implemented in server; Unity integration pending | `MD/PROTOCOL_V2_CHANGES.md`, server tests 143/143, WebSocket FakeClient v2 3/3 (no Supabase env) |
| R5 authoritative profile/balance | Pending | No wire message or test yet |
| R6 shop and rename | Pending | No server handlers yet |
| R3 live AFK ban adapter contract | Pending | Existing adapter unit tests only |
| R2 test data cleanup/security scan | Pending | Live test data remains; do not delete real users |
| R7 rank/daily quests | Pending | No server handlers yet |

Owner action: rotate the database password previously exposed in chat before opening the service to external users; keep replacement secrets in local/deployment environment only.
