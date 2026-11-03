# WebKit black-box probe: iPhone 15 Pro

URL: https://hitmegame-iota.vercel.app/?e2e=1

| Check | Result |
|---|---|
| T1 | SKIP |
| T2 | SKIP |
| T3 | SKIP |
| T4 | SKIP |
| T5 | SKIP |
| T6 | PASS |

Bridge: missing. See results.json and home.png.

## Scope and visual inspection

- `home.png` is a real Playwright WebKit capture of the deployed Vercel URL at 1179×2556. `results.json` records zero JavaScript page exceptions. This is desktop WebKit emulation of an iPhone profile, not a physical Safari test.
- T6 PASS means only that this Home capture has no large white blank region, full-width near-black edge band, or uniform blank canvas. It is not approval of layout or artwork.
- T1–T5 are SKIP because neither `window.__hitmeState` nor `window.__hitmeUiDump()` is present on the deployed `?e2e=1` page. No hit, selection, placement, arena-width or touch-target assertion is claimed.
- Manual inspection of `home.png`: the backdrop is a landscape rather than the target village courtyard; the logo is pale/silver; the primary quick-play button is white, the create/join and training buttons have nearly transparent fills; the bottom left label says “ĐĂNG NHẬP” where the Home tab belongs. These observations agree with H1/H2/H4/H5 in the project document. They are visual findings, not automated T1 assertions.
- No golden image was approved: `golden.py compare ... --name home` returned `NO_GOLDEN` (exit 1). The current Home screenshot must not become the reference.
- The companion iPhone SE run is in `../20261104_live_iphone_se/` and returned T6 PASS, T1–T5 SKIP for the same bridge reason.

Next handoff to Antigravity: publish the read-only A0 bridge on `?e2e=1`; then rerun T1–T5. The current test code does not yet implement the complete gesture/animation probes, so these remain incomplete even after A0 until extended and self-tested.
