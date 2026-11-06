"""Run: python tools/blackbox/test_web_safety.py. Web-only ERR-01..03 fixture."""
from __future__ import annotations

import json
import threading
from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

from playwright.sync_api import sync_playwright

DIST = Path(__file__).resolve().parents[2] / 'web-ui' / 'dist'


class Handler(SimpleHTTPRequestHandler):
    def log_message(self, *_args):
        pass


def run() -> int:
    server = ThreadingHTTPServer(('127.0.0.1', 0), partial(Handler, directory=str(DIST)))
    threading.Thread(target=server.serve_forever, daemon=True).start()
    results = {}
    try:
        with sync_playwright() as playwright:
            for ready in (False, True):
                browser = playwright.webkit.launch(headless=True)
                context = browser.new_context(viewport={'width': 393, 'height': 695},
                                              user_agent='Mozilla/5.0 iPhone Zalo Mobile',
                                              is_mobile=True, has_touch=True)
                page = context.new_page()
                if ready:
                    page.add_init_script("window.unityInstance={SendMessage:()=>{throw Error('private_token_123')}}")
                page.goto(f'http://127.0.0.1:{server.server_port}/?mock=1', wait_until='networkidle')
                page.wait_for_selector('.hitme-browser-tip')
                for name in ('pagehide', 'pageshow', 'visibilitychange', 'blur', 'focus',
                             'resize', 'orientationchange', 'beforeinstallprompt', 'appinstalled'):
                    page.evaluate('(name) => window.dispatchEvent(new Event(name))', name)
                page.evaluate("window.dispatchEvent(new ErrorEvent('error',{message:'Script error.'}))")
                opaque_ok = page.locator('.hitme-interruption').count() == 0
                page.locator('[data-action=character]').click()
                usable = page.locator('.choice-grid').count() == 1
                page.evaluate("window.unityInstance={SendMessage:()=>{throw Error('private_token_123')}}")
                page.locator('[data-action=select-character]').first.click()
                bridge_ok = page.evaluate("window.__hitmeDiagnostics.records.some(x=>x.kind==='bridge_error')")
                page.evaluate("window.__hitmeFatal()")
                fatal_ok = page.locator('.hitme-interruption').count() == 1 and page.get_by_text('Game bị gián đoạn').count() == 1
                logs = page.evaluate('window.__hitmeDiagnostics.records')
                redacted = 'private_token_123' not in json.dumps(logs)
                results[f'ready_{ready}'] = {
                    'ERR-01': opaque_ok and usable and bridge_ok,
                    'ERR-02': fatal_ok,
                    'ERR-03': redacted,
                    'inAppTip': page.locator('.hitme-browser-tip').count() == 1,
                }
                context.close()
                browser.close()
            for marker in ('Zalo', 'FBAN', 'FBAV', 'FB_IAB', 'Messenger', 'Instagram', 'Line', 'TikTok'):
                browser = playwright.webkit.launch(headless=True)
                context = browser.new_context(user_agent=f'Mozilla/5.0 iPhone {marker} Mobile')
                page = context.new_page()
                page.goto(f'http://127.0.0.1:{server.server_port}/?mock=1', wait_until='domcontentloaded')
                page.wait_for_selector('.hitme-browser-tip')
                results[f'ua_{marker}'] = {'tip_visible': page.locator('.hitme-browser-tip').count() == 1}
                context.close(); browser.close()
            browser = playwright.webkit.launch(headless=True)
            context = browser.new_context(user_agent='Mozilla/5.0 iPhone Safari Mobile')
            page = context.new_page()
            page.goto(f'http://127.0.0.1:{server.server_port}/?mock=1', wait_until='domcontentloaded')
            page.wait_for_selector('.home')
            results['ua_safari'] = {'tip_hidden': page.locator('.hitme-browser-tip').count() == 0}
            context.close(); browser.close()
    finally:
        server.shutdown()
    passed = sum(all(row.values()) for row in results.values())
    print(json.dumps(results, ensure_ascii=False, indent=2))
    print(f'Passed! {passed}/{len(results)}')
    return 0 if passed == len(results) else 1


if __name__ == '__main__':
    raise SystemExit(run())
