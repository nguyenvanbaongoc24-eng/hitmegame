"""Test the gated scenario controls against the built web UI (not Unity runtime)."""
from __future__ import annotations

import threading
from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

from playwright.sync_api import sync_playwright


ROOT = Path(__file__).resolve().parents[2]
DIST = ROOT / "web-ui" / "dist"


class QuietHandler(SimpleHTTPRequestHandler):
    def log_message(self, *_args: object) -> None:
        pass


def main() -> int:
    server = ThreadingHTTPServer(("127.0.0.1", 0), partial(QuietHandler, directory=str(DIST)))
    threading.Thread(target=server.serve_forever, daemon=True).start()
    checks: list[tuple[str, bool]] = []
    try:
        with sync_playwright() as playwright:
            browser = playwright.webkit.launch(headless=True)
            for query, expected in (("", 0), ("?e2e=1", 0), ("?dev=1", 6)):
                page = browser.new_page(viewport={"width": 393, "height": 695}, device_scale_factor=3,
                                        is_mobile=True, has_touch=True)
                page.add_init_script("window.__sent=[];window.unityInstance={SendMessage:(o,m,p)=>window.__sent.push({m,p:JSON.parse(p)})}")
                page.goto(f"http://127.0.0.1:{server.server_port}/?mock=0&{query.lstrip('?')}", wait_until="networkidle")
                page.evaluate("hitmeUi.onUnity({type:'ready',version:'1.3',roster:[],weapons:[]})")
                page.locator('[data-action="settings"]').first.click()
                count = page.locator('[data-action="start-scenario"]').count()
                checks.append((f"{query or 'normal'} visible={expected}", count == expected))
                if expected:
                    page.locator('[data-scenario="tiebreak"]').click()
                    checks.append(("dev bridge request", any(x["m"] == "StartScenario" and x["p"].get("name") == "tiebreak"
                                      for x in page.evaluate("window.__sent"))))
                else:
                    checks.append(("normal bridge inert", not any(x["m"] == "StartScenario" for x in page.evaluate("window.__sent"))))
                page.close()
            browser.close()
    finally:
        server.shutdown()
    passed = sum(ok for _, ok in checks)
    for label, ok in checks:
        if not ok: print(f"FAILED: {label}")
    print(f"Passed! {passed}/{len(checks)}" if passed == len(checks) else f"FAILED {passed}/{len(checks)}")
    return 0 if passed == len(checks) else 1


if __name__ == "__main__":
    raise SystemExit(main())
