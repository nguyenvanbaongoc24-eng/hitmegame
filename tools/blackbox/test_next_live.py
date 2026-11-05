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
    fingerprints = {}
    for name in ('app.js', 'style.css', 'catalog.json', 'live.js', 'audio.js', 'bridge.js'):
        local_hash = sha((ROOT / 'web-ui' / 'dist' / name).read_bytes())
        try:
            with urlopen(base + name, timeout=20) as response:
                remote_hash = sha(response.read()) if response.status == 200 else None
            fetch_error = None
        except Exception as exc:
            remote_hash = None
            fetch_error = type(exc).__name__
        fingerprints[name] = {'localSha256': local_hash, 'remoteSha256': remote_hash,
                              'synced': local_hash == remote_hash, 'fetchError': fetch_error}
    synced = all(item['synced'] for item in fingerprints.values())
    results: dict = {'url': base, 'utc': datetime.now(timezone.utc).isoformat(),
                     'fingerprints': fingerprints, 'deploymentSynced': synced, 'profiles': {}}
    with sync_playwright() as playwright:
        for name, (engine, width, height, dpr) in PROFILES.items():
            browser = getattr(playwright, engine).launch(headless=True, **({'channel': 'msedge'} if engine == 'chromium' else {}))
            context = browser.new_context(viewport={'width': width, 'height': height},
                                          device_scale_factor=dpr, is_mobile=True, has_touch=True)
            page = context.new_page()
            errors: list[str] = []
            page.on('pageerror', lambda error: errors.append(str(error)))
            row: dict = {'status': 'UNKNOWN', 'errors': errors, 'matches': [], 'checks': {}}
            try:
                page.goto(base + '?e2e=1', wait_until='domcontentloaded', timeout=45000)
                page.wait_for_function("() => window.__hitmeState?.build && document.querySelector('[data-screen=home]')", timeout=90000)
                page.wait_for_function("() => document.querySelector('.hero')?.naturalWidth > 0", timeout=20000)
                row['build'] = page.evaluate('window.__hitmeState.build')
                row['onlineLocked'] = page.locator('[data-action="online-locked"]').count()
                row['canvasRect'] = page.locator('#unity-canvas').bounding_box()
                row['homeVisible'] = page.locator('[data-screen="home"]').is_visible()
                row['heroLoaded'] = page.locator('.hero').evaluate('e => e.complete && e.naturalWidth > 0')
                row['buttonOverflow'] = {kind: page.locator(f'.home .{kind}').evaluate(
                    'e => Math.max(0, e.scrollWidth - e.clientWidth)') for kind in ('blue', 'green')}
                row['checks']['HOME-01'] = row['homeVisible']
                row['checks']['HOME-HERO'] = row['heroLoaded']
                row['checks']['BTN-LABEL'] = all(value == 0 for value in row['buttonOverflow'].values())
                row['checks']['BTN-09'] = row['onlineLocked'] == 3
                row['checks']['VIEW-01'] = bool(row['canvasRect'] and row['canvasRect']['width'] >= width - 2)
                row['status'] = 'PASS' if synced and all(row['checks'].values()) and not errors else 'FAIL'
                page.screenshot(path=str(output / f'{name}_home.png'))
                if gameplay and synced and row['status'] == 'PASS' and name == 'iphone15_safari':
                    page.locator('[data-action="character"]').click()
                    row['checks']['SEL-LOCK'] = page.locator('[data-action="locked"]').count() == 3
                    page.locator('[data-action="close"]').click()
                    for run_no in range(matches):
                        if run_no == 0:
                            page.locator('[data-action="practice"]').click()
                            page.locator('[data-action="start-practice"]').click()
                            page.wait_for_function("() => window.__hitmeState?.screen === 'match'", timeout=15000)
                            row['checks']['HUD-01'] = page.locator('.live-card').count() >= 2
                            row['checks']['F7-TUTORIAL'] = page.locator('.live-tutorial').count() == 1
                            if row['checks']['F7-TUTORIAL']:
                                page.locator('[data-action="tutorial-skip"]').click()
                            page.locator('.live-summary [data-action="settings"]').click()
                            row['checks']['SET-PAUSE'] = page.locator('[data-action="pause"]').count() == 1
                            page.locator('[data-action="pause"]').click()
                            row['checks']['SET-RESUME'] = page.locator('[data-action="resume"]').count() == 1
                            page.locator('[data-action="resume"]').click()
                            page.locator('.live-summary [data-action="settings"]').click()
                            page.locator('[data-setting="sfx"]').evaluate("el=>{el.value='35';el.dispatchEvent(new Event('input',{bubbles:true}))}")
                            row['checks']['SET-VOLUME'] = page.evaluate("localStorage.getItem('hitme.sfxVolume')") == '0.35'
                            page.locator('[data-action="tutorial-reopen"]').click()
                            row['checks']['F7-REOPEN'] = page.locator('.live-tutorial').count() == 1
                            page.locator('[data-action="tutorial-skip"]').click()
                            page.locator('.live-bottom [data-action="emote"]').click()
                            row['checks']['EMO-OPEN'] = page.locator('[data-action="send-live-emote"]').count() >= 3
                            page.locator('[data-action="send-live-emote"]').first.click()
                            row['checks']['EMO-SEND'] = page.locator('[data-action="send-live-emote"]').count() == 0
                            row['checks']['ARENA-TOUCH'] = page.evaluate("""() => {
                                const c=document.querySelector('#unity-canvas');const r=c.getBoundingClientRect();
                                return document.elementFromPoint(r.x+r.width/2,r.y+r.height/2)===c;
                            }""")
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
                    row['checks']['F1-MATCHES'] = len(row['matches']) == matches and all(m['rankings'] >= 2 for m in row['matches'])
                    character_checks = []
                    for character_id in ('nam_lang', 'nu_nang_dong', 'co_ba', 'anh_teen'):
                        page.locator('[data-action="character"]').click()
                        page.locator(f'[data-action="select-character"][data-id="{character_id}"]').click()
                        page.locator('[data-action="practice"]').click()
                        page.locator('[data-action="start-practice"]').click()
                        page.wait_for_function("() => window.__hitmeState?.screen === 'match'", timeout=15000)
                        card_src = page.locator('.live-card.local img').get_attribute('src') or ''
                        character_checks.append(character_id in card_src)
                        page.locator('.live-summary [data-action="settings"]').click()
                        page.locator('[data-action="exit-confirm"]').click()
                        page.locator('[data-action="exit-match"]').click()
                        page.wait_for_function("() => window.__hitmeState?.screen === 'home'", timeout=15000)
                    row['checks']['F2-ROSTER'] = all(character_checks)
                    if not all(row['checks'].values()): row['status'] = 'FAIL'
            except Exception as exc:
                row['status'] = 'FAIL'
                row['failure'] = f'{type(exc).__name__}: {str(exc)[:240]}'
                page.screenshot(path=str(output / f'{name}_failure.png'))
            results['profiles'][name] = row
            context.close(); browser.close()
    required = ('HOME-01','HOME-HERO','BTN-LABEL','BTN-09','VIEW-01','SEL-LOCK','HUD-01','F1-MATCHES',
                'F2-ROSTER','F7-TUTORIAL','F7-REOPEN','SET-PAUSE','SET-RESUME',
                'SET-VOLUME','EMO-OPEN','EMO-SEND','ARENA-TOUCH')
    observed = results['profiles'].get('iphone15_safari', {}).get('checks', {})
    results['coverage'] = {key: ('BLOCKED_DEPLOY' if not synced else
                                 'NOT_RUN' if key not in observed else
                                 'PASS' if observed[key] else 'FAIL') for key in required}
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
