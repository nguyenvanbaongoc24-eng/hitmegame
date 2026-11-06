"""W1 loader error check against a local hybrid tree.

Run: python tools/blackbox/test_hybrid_loader.py --root "D:/HITME GAME/worktrees/web-dist-mobile"
"""
import argparse
import threading
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

from playwright.sync_api import sync_playwright


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--root', type=Path, required=True)
    args = parser.parse_args()

    class Handler(SimpleHTTPRequestHandler):
        def __init__(self, *a, **kw):
            super().__init__(*a, directory=str(args.root), **kw)

        def log_message(self, *_):
            pass

    server = ThreadingHTTPServer(('127.0.0.1', 0), Handler)
    threading.Thread(target=server.serve_forever, daemon=True).start()
    checks = {}
    try:
        with sync_playwright() as playwright:
            browser = playwright.webkit.launch(headless=True)
            context = browser.new_context(viewport={'width': 393, 'height': 695}, is_mobile=True, has_touch=True)
            page = context.new_page()
            page.route('**/*.loader.js*', lambda route: route.abort())
            page.goto(f'http://127.0.0.1:{server.server_port}/next/?e2e=1', wait_until='networkidle')
            page.wait_for_selector('.hitme-interruption')
            checks['ERR-02_loader_failure_shows_friendly_panel'] = 'Game bị gián đoạn' in page.locator('.hitme-interruption').inner_text()
            checks['ERR-03_no_raw_loader_error'] = 'loader.js' not in page.locator('.hitme-interruption').inner_text()
            page.locator('.hitme-interruption [data-later]').click()
            checks['ERR-02_later_closes_panel'] = page.locator('.hitme-interruption').count() == 0
            context.close(); browser.close()
    finally:
        server.shutdown()
    passed = sum(checks.values())
    print(f'Passed! {passed}/{len(checks)}')
    for name, ok in checks.items():
        if not ok:
            print('FAIL', name)
    return 0 if passed == len(checks) else 1


if __name__ == '__main__':
    raise SystemExit(main())
