"""README: python tools/blackbox/test_webui_flow.py

Serve dist at /ui-preview/ and verify the complete mock navigation in WebKit
and Chromium. Screenshots are design evidence, not gameplay evidence.
"""
from __future__ import annotations

import json
import threading
from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import unquote, urlsplit

from playwright.sync_api import sync_playwright

ROOT = Path(__file__).resolve().parents[2]
DIST = ROOT / 'web-ui' / 'dist'
OUT = ROOT / 'web-ui' / 'evidence' / 'preview_flow'


class Handler(SimpleHTTPRequestHandler):
    def translate_path(self, path: str) -> str:
        requested = unquote(urlsplit(path).path)
        if not requested.startswith('/ui-preview/'):
            return str(DIST / '404')
        relative = requested[len('/ui-preview/'):]
        candidate = (DIST / relative).resolve()
        return str(candidate if candidate.is_relative_to(DIST) else DIST / '404')

    def log_message(self, *_args: object) -> None:
        pass


def run() -> int:
    OUT.mkdir(parents=True, exist_ok=True)
    server = ThreadingHTTPServer(('127.0.0.1', 0), partial(Handler, directory=str(DIST)))
    thread = threading.Thread(target=server.serve_forever, daemon=True)
    thread.start()
    url = f'http://127.0.0.1:{server.server_port}/ui-preview/?mock=1&e2e=1'
    results: dict[str, dict] = {}
    with sync_playwright() as playwright:
        for engine_name in ('webkit', 'chromium'):
            browser = getattr(playwright, engine_name).launch(headless=True, **({'channel': 'msedge'} if engine_name == 'chromium' else {}))
            context = browser.new_context(viewport={'width': 393, 'height': 695}, device_scale_factor=3,
                                          is_mobile=True, has_touch=True)
            page = context.new_page()
            errors: list[str] = []
            requests: list[str] = []
            page.on('pageerror', lambda error: errors.append(str(error)))
            page.on('request', lambda request: requests.append(request.url))
            page.goto(url, wait_until='networkidle')
            page.locator('[data-name="quick-play"]').wait_for()
            page.screenshot(path=str(OUT / f'{engine_name}_home.png'))
            checks: list[str] = []
            def check(condition: bool, name: str) -> None:
                if not condition: checks.append(name)
            check(page.locator('.gold').is_visible(), 'mock gold chip')
            page.locator('[data-name="settings"]').click()
            check(page.locator('.settings-sheet [data-setting="music"]').is_visible(), 'Home settings sheet')
            page.locator('.settings-sheet [data-setting="music"]').fill('42')
            check(page.evaluate("localStorage.getItem('hitme.musicVolume')") == '0.42', 'settings persisted')
            page.locator('.settings-sheet [data-action="close"]').click()
            page.locator('[data-action="practice"]').click()
            check(page.locator('[data-screen="bot"]').is_visible(), 'Home → bot')
            page.screenshot(path=str(OUT / f'{engine_name}_bot.png'))
            check(page.locator('[data-id="quay_boy"]').get_attribute('aria-disabled') == 'true', 'locked card')
            page.evaluate("document.querySelector('[data-id=quay_boy]').click()")
            check(page.evaluate('window.__hitmeState.selectedCharacterId') == 'nam_lang', 'locked denied')
            page.locator('[data-id="co_ba"]').click()
            page.locator('[data-id="chao_chong_dinh"]').click()
            page.locator('[data-action="bot-plus"]').click()
            check(page.evaluate('window.__hitmeState.selectedCharacterId') == 'co_ba', 'character select')
            check(page.evaluate('window.__hitmeState.selectedWeaponId') == 'chao_chong_dinh', 'weapon select')
            check(page.evaluate('window.__hitmeState.botCount') == 4, 'bot count')
            page.reload(wait_until='networkidle')
            check(page.evaluate('window.__hitmeState.selectedCharacterId') == 'co_ba', 'character persistence')
            check(page.evaluate('window.__hitmeState.selectedWeaponId') == 'chao_chong_dinh', 'weapon persistence')
            page.evaluate('''() => {window.__mockEvents=[];const old=window.hitmeUi.onUnity;
              window.hitmeUi.onUnity=e=>{window.__mockEvents.push(e);old(e)}}''')
            page.locator('[data-action="practice"]').click()
            page.locator('[data-action="start-practice"]').click()
            check(page.locator('[data-screen="match"]').is_visible(), 'bot → match')
            check(page.locator('[data-name="mock-timer"]').is_visible(), 'match HUD timer')
            page.screenshot(path=str(OUT / f'{engine_name}_match.png'))
            page.locator('[data-action="settings"]').click()
            check(page.locator('.settings-sheet [data-setting="sfx"]').is_visible(), 'match settings sheet')
            page.locator('.settings-sheet [data-action="close"]').click()
            page.locator('[data-action="emote"]').click()
            check(page.locator('[data-screen="emote"]').is_visible(), 'emote panel')
            page.locator('[data-action="send-emote"]').first.click()
            check(page.locator('[data-screen="match"]').is_visible(), 'emote → match')
            page.wait_for_selector('[data-screen="result"]', timeout=12000)
            page.screenshot(path=str(OUT / f'{engine_name}_result.png'))
            events = page.evaluate('window.__mockEvents')
            phases = [e.get('phase') for e in events if e.get('type') == 'match']
            kinds = [e.get('kind') for e in events if e.get('type') == 'event']
            check(phases == ['placing', 'reveal', 'throwing', 'round_result'], f'phases: {phases}')
            check(kinds == ['hit', 'ko', 'win'], f'events: {kinds}')
            check(page.locator('.preview-list>div').count() == page.evaluate('window.__hitmeState.botCount') + 1, 'complete sample rankings')
            page.locator('[data-action="go-home"]').last.click()
            check(page.locator('[data-screen="home"]').is_visible(), 'result → home')
            page.locator('[data-action="create"]').click()
            check(page.locator('[data-screen="lobby"]').is_visible(), 'create → lobby')
            page.locator('[data-action="lobby-ready"]').click()
            check('HỦY SẴN SÀNG' in page.locator('[data-action="lobby-ready"]').inner_text(), 'lobby ready toggle')
            page.locator('[data-action="go-home"]').first.click()
            page.locator('[data-action="join"]').click()
            page.locator('#room-code').fill('HM4826')
            page.locator('[data-action="join-submit"]').click()
            check(page.locator('[data-screen="lobby"]').is_visible(), 'join → lobby')
            page.locator('[data-action="go-home"]').first.click()
            page.locator('[data-action="quick"]').click()
            page.wait_for_selector('[data-screen="lobby"]', timeout=3000)
            check(page.locator('[data-screen="lobby"]').is_visible(), 'quick → search → lobby')
            local_requests = [u for u in requests if urlsplit(u).scheme in ('http', 'https') and urlsplit(u).hostname == '127.0.0.1']
            check(all(urlsplit(u).path.startswith('/ui-preview/') for u in local_requests),
                  f'all local assets within subpath: {[u for u in local_requests if not urlsplit(u).path.startswith("/ui-preview/")]}')
            check(not errors, f'JavaScript errors: {errors}')
            results[engine_name] = {'status': 'PASS' if not checks else 'FAIL', 'failures': checks,
                                    'phases': phases, 'events': kinds, 'requests': len(requests)}
            context.close()
            browser.close()
    server.shutdown()
    (OUT / 'results.json').write_text(json.dumps(results, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    passed = sum(x['status'] == 'PASS' for x in results.values())
    print(f'Passed! {passed}/{len(results)}')
    for name, result in results.items():
        if result['failures']: print(f'FAIL {name}: {result["failures"]}')
    return 0 if passed == len(results) else 1


if __name__ == '__main__':
    raise SystemExit(run())
