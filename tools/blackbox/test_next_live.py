"""Inspect the deployed /next/ hybrid without injecting mock match data.

Run: python tools/blackbox/test_next_live.py --url https://hitmegame-iota.vercel.app/next/
     python tools/blackbox/test_next_live.py --url URL --matches 5 --gameplay
The deployment must contain the current app.js before gameplay can be marked PASS.
Headless browsers do not replace a physical iPhone touch/audio test.
"""
from __future__ import annotations

import argparse
import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path
from urllib.request import urlopen

from playwright.sync_api import sync_playwright

ROOT = Path(__file__).resolve().parents[2]
PROFILES = {
    'iphone15_safari': ('webkit', 393, 695, 3),
    'iphonese_safari': ('webkit', 375, 667, 2),
    'android_chrome': ('chromium', 393, 873, 3),
    'android_tall': ('chromium', 360, 800, 3),
    'ipad_safari': ('webkit', 768, 1024, 2),
    'landscape_chrome': ('chromium', 852, 393, 2),
}


def sha(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def run(url: str, output: Path, matches: int, gameplay: bool) -> int:
    output.mkdir(parents=True, exist_ok=True)
    base = url.rstrip('/') + '/'
    local_hash = sha((ROOT / 'web-ui' / 'dist' / 'app.js').read_bytes())
    try:
        with urlopen(base + 'app.js', timeout=20) as response:
            remote_hash = sha(response.read()) if response.status == 200 else None
    except Exception as exc:
        remote_hash = None
        fetch_error = type(exc).__name__
    else:
        fetch_error = None
    synced = local_hash == remote_hash
    results: dict = {'url': base, 'utc': datetime.now(timezone.utc).isoformat(),
                     'localAppSha256': local_hash, 'remoteAppSha256': remote_hash,
                     'deploymentSynced': synced, 'fetchError': fetch_error, 'profiles': {}}
    with sync_playwright() as playwright:
        for name, (engine, width, height, dpr) in PROFILES.items():
            browser = getattr(playwright, engine).launch(headless=True, **({'channel': 'msedge'} if engine == 'chromium' else {}))
            context = browser.new_context(viewport={'width': width, 'height': height},
                                          device_scale_factor=dpr, is_mobile=True, has_touch=True)
            page = context.new_page()
            errors: list[str] = []
            page.on('pageerror', lambda error: errors.append(str(error)))
            row: dict = {'status': 'UNKNOWN', 'errors': errors, 'matches': []}
            try:
                page.goto(base + '?e2e=1', wait_until='domcontentloaded', timeout=45000)
                page.wait_for_function("() => window.__hitmeState?.build && document.querySelector('[data-screen=home]')", timeout=90000)
                row['build'] = page.evaluate('window.__hitmeState.build')
                row['onlineLocked'] = page.locator('[data-action="online-locked"]').count()
                row['canvasRect'] = page.locator('#unity-canvas').bounding_box()
                row['homeVisible'] = page.locator('[data-screen="home"]').is_visible()
                row['status'] = 'PASS' if synced and row['homeVisible'] and row['onlineLocked'] == 3 and not errors else 'FAIL'
                page.screenshot(path=str(output / f'{name}_home.png'))
                if gameplay and synced and row['status'] == 'PASS' and name == 'iphone15_safari':
                    for run_no in range(matches):
                        page.locator('[data-action="practice"]').click()
                        page.locator('[data-action="start-practice"]').click()
                        page.wait_for_function("() => window.__hitmeState?.screen === 'match'", timeout=15000)
                        page.wait_for_function("() => window.__hitmeState?.screen === 'result'", timeout=180000)
                        ranking_count = page.locator('.live-rank').count()
                        row['matches'].append({'run': run_no + 1, 'rankings': ranking_count})
                        page.screenshot(path=str(output / f'{name}_result_{run_no+1}.png'))
                        if ranking_count < 2:
                            row['status'] = 'FAIL'
                            break
                        if run_no + 1 < matches:
                            page.locator('[data-action="rematch"]').click()
                            page.wait_for_function("() => window.__hitmeState?.screen === 'match'", timeout=15000)
                        else:
                            page.locator('[data-action="exit-match"]').click()
                            page.wait_for_function("() => window.__hitmeState?.screen === 'home'", timeout=15000)
            except Exception as exc:
                row['status'] = 'FAIL'
                row['failure'] = f'{type(exc).__name__}: {str(exc)[:240]}'
                page.screenshot(path=str(output / f'{name}_failure.png'))
            results['profiles'][name] = row
            context.close(); browser.close()
    (output / 'results.json').write_text(json.dumps(results, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    passed = sum(row['status'] == 'PASS' for row in results['profiles'].values())
    print(f"deploymentSynced={synced}; Passed! {passed}/{len(PROFILES)}")
    print(output)
    return 0 if passed == len(PROFILES) else 1


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--url', default='https://hitmegame-iota.vercel.app/next/')
    parser.add_argument('--output', type=Path, default=ROOT / 'web-ui' / 'evidence' / 'next_live')
    parser.add_argument('--matches', type=int, default=5)
    parser.add_argument('--gameplay', action='store_true')
    args = parser.parse_args()
    raise SystemExit(run(args.url, args.output, args.matches, args.gameplay))
