# HIT ME WebKit black-box checks

Run from the repository root on Windows after `pip install playwright pillow numpy` and `python -m playwright install webkit`:

```powershell
python tools/blackbox/run.py --url 'https://hitmegame-iota.vercel.app/?e2e=1' --device 'iPhone 15 Pro'
python tools/blackbox/run.py --url 'https://hitmegame-iota.vercel.app/?e2e=1' --device 'iPhone SE'
python -m unittest discover tools/blackbox -v
```

`--output` chooses the evidence folder. The runner saves `home.png` at the device pixel ratio, `results.json`, and `report.md`. A FAIL exits nonzero. SKIP means the observation or interaction was unavailable, never a pass. The current runner checks loader completion, nonblank screenshot, blank white regions and dark edge bands (T6), and UI geometry/text from the read-only `?e2e=1` bridge (T1). T2–T4 and match-only T5 remain SKIP until the bridge and full touch/animation probes are implemented. `page.touchscreen.tap` is the only interaction; the bridge must not set state.

Playwright's built-in iPhone 15 Pro profile uses a 393×659 viewport; this tool overrides it to 393×852 with DPR 3 for 1179×2556 screenshots. iPhone SE uses Playwright's 320×568 profile. Emulation is not proof of behavior on a physical iPhone.

Image comparison: `python tools/blackbox/golden.py compare Evidence/blackbox/RUN/home.png --name home`. A missing reference reports `NO_GOLDEN` and exits nonzero. Only after the project owner accepts an image, run `python tools/blackbox/golden.py approve PATH.png --name home --approve`. Supported names: `home`, `select`, `match_placing`, `match_result`, `settings`. Comparison writes regional SSIM, changed pixel fraction, an overlay and a difference image. Thresholds are configurable via `--min-ssim` and `--max-changed`; do not loosen them solely to pass.

Antigravity can inspect this tool with `git show prep/codex:tools/blackbox/run.py`. The Unity build must expose `window.__hitmeState` and `window.__hitmeUiDump()` only when `?e2e=1`; otherwise geometry and interaction checks report SKIP.
