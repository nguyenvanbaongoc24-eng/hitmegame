# Codex preparation status (2026-11-02)

Branch `prep/codex`; Unity and Core are not edited here.

| Task | Status | Evidence |
|---|---|---|
| R1 Render health | Local server verified; live Render unresolved | `MD/DEPLOY_RENDER.md`: local HTTP 200 in 329 ms, public URL timed out at 35 s; Docker unavailable |
| R4 protocol v2 | Implemented in server; Unity integration pending | `MD/PROTOCOL_V2_CHANGES.md`, server tests 143/143, WebSocket FakeClient v2 3/3 (no Supabase env) |
| R5 authoritative profile/balance | Partial | `profile_state` sends server gold, match count and best rank after join/match; rank points and durable inventory pending R6/R7. Server tests 145/145; FakeClient v2 with Supabase verified 3/3. |
| R6 shop and rename | Migration applied and database RPC verified live; Render integration pending | `Evidence/round46/R6_migration_live.txt`; first rename free, later renames 200 gold; 008 committed on Supabase 2026-11-03, ledger 8/8 and rolled-back shop checks passed. Server 152/152. Set `SUPABASE_SHOP_ENABLED=true` on Render only after Supabase URL, server-only key and guest signing key are configured there. |
| R3 live AFK ban adapter contract | Partial | `Evidence/round43/R3_ban_supabase.txt`: 5/5 live checks; fresh adapter proves database persistence, but full server process restart was not exercised. Test-prefixed row removed. |
| R2 test data cleanup/security scan | Pending | Live test data remains; do not delete real users |
| R7 rank/daily quests | Pending | No server handlers yet |

L0–L7 handoff: L0 verified directly on 2026-11-02. L1 partially underway (R6 database verified live 2026-11-03; R7 and durable friends pending). L2–L7 not completed. Do not present database shop tests as proof of live Render or WebGL behavior.

L4 local work: shutdown now sends `server_restarting` then closes the socket; total capacity returns `server_busy`; `/metrics` includes median round processing time; manual backup script and `MD/OPS.md` added. Server tests 149/149. SIGTERM, capacity and backup have not been exercised on Render; `pg_dump` is absent locally.

L3 local work: repeated malformed/invalid commit packets (5 in one placement round) eliminate the sender without an AFK ban. Valid placement revisions remain allowed. Reconnect cannot reset the count. Audit records room ID, match ID, code and count, never raw payloads or tokens. Server tests 152/152. This has not been exercised on Render.

L2 preparation only: `MD/AUTH_SETUP.md` lists Google/Supabase dashboard setup and required acceptance tests. OAuth provider configuration, server token verification, guest-account merge and Unity callback are not implemented or tested yet.

L5 preparation only: `backend/supabase/cleanup_test_data.sql` is a read-only, empty-allowlist preview; PGlite syntax/no-write check passed 21/21. No live preview or deletion was run; candidate IDs and the replacement DB URL are still unavailable.

L7 partial handoff: `MD/PROTOCOL_V2_CHANGES.md` now includes current v2 payload shapes and explicit placeholder/pending fields. Rank, quests, durable friends and Google login have no live wire contract yet, so this is not a complete L7 handoff.

L6 live Render probe: **not passed**. A 601-second run of 3 concurrent rooms (2/4/6 clients) completed 58/60 matches and measured 290 rounds; mean/p95/max lock-to-result = 170/528/1590 ms, 2 errors and 1 unexpected disconnect. CPU/RAM unavailable. Evidence: `Evidence/round46/L6_load.md` and `L6_validated_raw.txt`. Local server suite rerun: Passed! 152/152. Do not claim live capacity or production authentication from this run.

Owner action: rotate the database password previously exposed in chat before opening the service to external users; keep replacement secrets in local/deployment environment only.

2026-11-03 migration 008 attempt: `SUPABASE_DB_URL` is now present in ignored `.env`. The direct database host does not resolve on this machine; a read-only probe through the previously supplied session pooler reaches the service but password authentication fails. No migration SQL was executed and `guest_catalog` remains absent (REST returned 404). Correct the session-pooler connection credentials privately; then rerun `python tools/supabase/apply_migrations.py --check` before `--apply`. Raw reserved password characters are now escaped in memory; URL parser tests 4/4 passed.

2026-11-03 follow-up: the owner corrected the ignored `.env`; read-only preflight passed, migration 008 committed, `verify_live.py` passed in a rolled-back transaction, and REST returned HTTP 200 for `guest_catalog`. Full evidence: `Evidence/round46/R6_migration_live.txt`. Supabase is ready for the server shop adapter; Render is not yet configured or verified for shop traffic.

Gameplay visual spec handoff (2026-11-03): `tools/sprites/validate_pack.py` and `pack_sprites.py` validate and losslessly pack gameplay sprite frames outside Unity. Synthetic image tests passed 13/13 (`python -m unittest discover -s tools/sprites -p test_sprites.py -v`); no production 18-frame sprite set has been supplied or packed. The available `asset/GAMEPLAY_AIM_REFERENCE.png` is only 316×304, a cropped aim detail, so AG cannot use it to measure a full 1179×2556 gameplay composition. V1–V7 rendering/harness work remains with AG; no Unity/Core/server files changed for this tool.

K1 placement secrecy (2026-11-03): two-client data-independence test checks revised commits, fixed-shape acknowledgments, reconnect before lock and byte-identical reveal after lock. Full local server suite Passed! 153/153. Evidence: `Evidence/round47/K1_secrecy.txt`. This is in-process protocol verification, not a live packet capture or Unity visual test.

K2 stable player slots (2026-11-03): `room_state.players[].slot` is assigned 0–5 by the server, unique per room, unchanged for survivors and reconnects; new lobby players reuse the lowest free slot. Six targeted cases passed; full suite and handoff evidence in `Evidence/round47/K2_slots.txt`. Client color palette and rendering remain AG work.

K3 independent sprite inventory (2026-11-03): read-only scan of Unity Characters PNG/.meta produced 840 frame records and 7-roster contact sheet at `tools/sprites/out/`. 128 metadata rectangles are outside their actual PNG; many candidate frames include opaque logo/background. Cô Ba source is not identified in this folder. Synthetic/tool suite 18/18. Evidence: `Evidence/round47/K3_inventory.txt`. These are provisional screening findings, not approved replacement sprites.

K4 independent harness library (2026-11-03): `tools/harness_lib/` checks UI safe-area/overlap/target/text geometry, regional global SSIM/pixel difference/color centroid with overlay, head/body ratio, and Markdown summaries. Six synthetic tests passed; full server suite Passed! 159/159 and sprite suite 18/18. Evidence: `Evidence/round47/K4_harness.txt`. No real Unity UI dump or full-match target screenshot was supplied to calibrate thresholds.

K5 remains open: L1 rank/quests/durable friends; L2 Google token verification and guest merge; live Render shop/auth/operation validation; L6 live load retry. Existing local code/evidence are described above. Do not treat K1–K4 as completion of K5.

K5 L6 diagnostic (2026-11-03): Render load harness now emits redacted room/cycle/stage/last-packet context on failure and fails pending reads immediately on unexpected socket close. Short live Render dry run completed 3/3 matches and 15/15 rounds; 0 errors/disconnects. Evidence: `Evidence/round47/K5_load_dryrun.txt`. Ten-minute retry remains pending.

K5 L7 partial handoff (2026-11-03): `MD/PROTOCOL_V2_CHANGES.md` now carries the stable slot field, placement privacy, independent sprite/harness paths, and correct migration 008 live status. Rank, daily quests, durable friends and Google login still have no complete wire contract.

K4 follow-up: layout touch-size threshold now scales by `pixelsPerCssPixel` for high-DPI screenshots; harness tests 7/7. Evidence updated in `Evidence/round47/K4_harness.txt`.

K5/L6 live Render retry (2026-11-03): 600-second, 3-concurrent-room probe completed 60/60 matches and 300/300 rounds with 0 errors/disconnects; lock-to-result mean/p95/max 72/91/285 ms. Evidence: `Evidence/round47/K5_load_10min.txt` and raw log. Prior 58/60 failure did not recur, but root cause is undetermined. CPU/RAM and production-auth capacity remain unverified. Other K5 items above remain open.

M1 (2026-11-03): Server lobby accepts selected character/weapon on join and room_ready, validates trial roster and weapon ownership, and broadcasts both with stable slot. Trial character entitlement is free; durable character inventory is pending. Evidence: `Evidence/round48/M1_loadouts.txt`. Unity integration and live Render deployment remain unverified.

M2 verified (2026-11-03): K1 secrecy and K2 slot evidence already exists in `Evidence/round47/K1_secrecy.txt` and `K2_slots.txt`; no new Unity/live-render claim.

M4 (2026-11-03): `tools/sprites/slice_sheet.py` slices 4×2 or 4×3 flat-green sheets into shared-scale 256×256 `gp_` frames with common foot anchor; rejects missing/merged/uneven/fringed cells with an annotated image. `validate_pack.py` and `pack_sprites.py` accept this partial GP format via `anchors.json`; complete `char_` format remains unchanged. Synthetic tests 24/24; no ChatGPT sheet or Unity import tested. Evidence: `Evidence/round48/M4_slice_sheet.txt`.

N1 (2026-11-03): `tools/ui/glyph_coverage.py` audits C#/JSON/Markdown UI text against a TTF/OTF cmap with file:line diagnostics and full Vietnamese character mode. Three synthetic tests passed; actual BeVietnamPro-Medium and HitMeFont scans of four UI/data files find only missing check mark and crown glyphs (not Vietnamese letters). Evidence: `Evidence/round48/N1_glyphs.txt` and JSON reports. Rendering in Unity remains unverified.

N2 (2026-11-03): tools/ui/slice_ui_kit.py extracts named UI elements from alpha or green key, rejects count mismatch with annotated image; nineslice_suggest.py flags patterned centers. Synthetic UI suite 7/7, sprite 24/24, server Passed! 170/170. Evidence: Evidence/round48/N2_ui_kit.txt. Real UI kit and Unity import remain unverified.

N3 (2026-11-03): tools/ui/regression_from_screenshot.py flags large white placeholders and full-width dark top/bottom bands; OCR explicitly unsupported. Synthetic UI suite 10/10, sprite 24/24, server Passed! 170/170. Evidence: Evidence/round48/N3_screenshot.txt. No real iPhone screenshot audited in this task.
