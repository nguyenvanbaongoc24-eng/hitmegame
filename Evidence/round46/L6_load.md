# L6 — Render load probe (2026-11-02)

Target: `wss://hitmegame.onrender.com/ws`. Command:

```text
node server/Tools/RenderLoad.mjs wss://hitmegame.onrender.com/ws --seconds 600 --interval 30
```

The script opens three concurrent rooms per cycle (2, 4 and 6 guest clients), sends placements and locks, and waits for five rounds plus `match_end`. It uses unsigned guest tokens because the deployed Render service currently accepts them; this is **not** proof of production authentication. It records no token values.

| Measurement | Result |
|---|---:|
| Duration | 601 s |
| Cycles / expected matches | 20 / 60 |
| Completed matches | 58 |
| Measured rounds | 290 (5 per completed match) |
| Lock-to-result mean / p95 / max | 170 / 528 / 1590 ms |
| Errors | 2: `wrong_round`, `wait_timeout_room_state` |
| Unexpected disconnects | 1 |
| Peak observed connections at sampled `/metrics` calls | 12 |
| CPU / peak RAM | Unavailable from public endpoint |

**Result: NOT PASS.** The 2 failed rooms and 1 dropped connection need a reproduction with server-side room/match correlation. Public `/metrics` gives cumulative counters (`wrong_round: 2`, `round_locked: 4` after the run) but does not identify the failing rooms. The two errors may originate in the client harness, network timing, or server; this run does not distinguish them. Do not infer a safe room capacity from a run with errors or missing CPU/RAM measurements.

`L6_validated_raw.txt` is the complete output of this run; its command exited 1. `L6_raw.txt` is an earlier **invalid, interrupted** attempt and must not be used as a passing result. A short corrected dry run completed 3/3 rooms and 15/15 rounds with no errors before the 10-minute run.

Next diagnostic run: add redacted per-room phase/round timestamps to the harness and correlate with server logs; rerun 10 minutes after the mismatch is explained. Verify `GUEST_TOKEN_SIGNING_KEY` and Supabase environment on Render before any public test with real users. The database migration is not involved in this load probe.
