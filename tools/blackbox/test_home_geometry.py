"""Check the real dist Home assets and button geometry at six screen sizes.

Run: python tools/blackbox/test_home_geometry.py
This uses a local static server and does not certify the deployed Unity build.
"""
from __future__ import annotations

import json
import threading
from functools import partial
from http.server import ThreadingHTTPServer
from pathlib import Path

from playwright.sync_api import sync_playwright

from test_live_bridge_fixture import DIST, Handler
from test_next_live import PROFILES

OUT = DIST.parent / 'evidence' / 'home_geometry'


def run() -> int:
    OUT.mkdir(parents=True, exist_ok=True)
    server = ThreadingHTTPServer(('127.0.0.1', 0), partial(Handler, directory=str(DIST)))
    threading.Thread(target=server.serve_forever, daemon=True).start()
    results = {}
    try:
        with sync_playwright() as playwright:
            for name, (engine, width, height, dpr) in PROFILES.items():
                browser = getattr(playwright, engine).launch(
                    headless=True, **({'channel': 'msedge'} if engine == 'chromium' else {}))
                context = browser.new_context(
                    viewport={'width': width, 'height': height},
                    device_scale_factor=dpr, is_mobile=True, has_touch=True)
                page = context.new_page()
                errors = []
                page.on('pageerror', lambda error: errors.append(str(error)))
                try:
                    page.goto(f'http://127.0.0.1:{server.server_port}/next/?e2e=1',
                              wait_until='networkidle', timeout=30000)
                    page.wait_for_function("() => document.querySelector('.hero')?.naturalWidth > 0")
                    buttons = {key: page.locator(f'.home .{key}').evaluate(
                        'e => Math.max(0, e.scrollWidth - e.clientWidth)')
                        for key in ('blue', 'green', 'quick')}
                    weapon_name = page.locator('.weapon-card b').evaluate(
                        '''e => {const range=document.createRange();range.selectNodeContents(e);
                          const text=range.getBoundingClientRect();
                          const card=e.closest('.weapon-card').getBoundingClientRect();
                          return {fits:text.left>=card.left && text.right<=card.right &&
                            text.top>=card.top && text.bottom<=card.bottom,
                            text:e.textContent,
                            lines:Math.round(e.getBoundingClientRect().height /
                              parseFloat(getComputedStyle(e).lineHeight))};}''')
                    hero = page.locator('.hero').is_visible()
                    page.screenshot(path=str(OUT / f'{name}.png'))
                    results[name] = {'status': 'PASS' if hero and weapon_name['fits'] and not errors and
                                     all(value == 0 for value in buttons.values()) else 'FAIL',
                                     'heroLoaded': hero, 'weaponName': weapon_name,
                                     'overflowPx': buttons, 'errors': errors}
                except Exception as exc:
                    results[name] = {'status': 'FAIL', 'error': f'{type(exc).__name__}: {exc}'[:240]}
                context.close()
                browser.close()
    finally:
        server.shutdown()
    (OUT / 'results.json').write_text(json.dumps(results, ensure_ascii=False, indent=2) + '\n',
                                      encoding='utf-8')
    passed = sum(row['status'] == 'PASS' for row in results.values())
    print(f'Passed! {passed}/{len(results)}')
    for name, row in results.items():
        if row['status'] != 'PASS':
            print(name, json.dumps(row, ensure_ascii=True))
    return 0 if passed == len(results) else 1


if __name__ == '__main__':
    raise SystemExit(run())
