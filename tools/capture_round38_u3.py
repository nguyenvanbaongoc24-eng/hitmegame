#!/usr/bin/env python3
"""
Captures Round 38 Step U3 evidence:
1) Home screen showing 'Máy chủ chưa sẵn sàng' on CHƠI NHANH / TẠO PHÒNG / VÀO PHÒNG
2) Clicking CHƠI NHANH -> 'MÁY CHỦ CHƯA SẴN SÀNG' sheet with 1-tap 'CHUYỂN SANG LUYỆN VỚI BOT'
3) Clicking 'ĐÁNH THỨC MÁY CHỦ (TỚI 60 GIÂY)' -> 'ĐANG ĐÁNH THỨC MÁY CHỦ…' 60s waking sheet
"""
from __future__ import annotations

import functools
import http.server
import socketserver
import threading
from pathlib import Path
from playwright.sync_api import sync_playwright

STAGING_DIR = Path(r"D:\HITME GAME\_web_dist_deploy")
OUT_DIR = Path(r"D:\HITME GAME\Evidence\round38\U3_screens")


class BrotliHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        if self.path.split("?")[0].endswith(".br"):
            self.send_header("Content-Encoding", "br")
        self.send_header("Cache-Control", "no-store")
        super().end_headers()

    def guess_type(self, path):
        clean = path.split("?")[0]
        if clean.endswith(".wasm.br") or clean.endswith(".wasm"):
            return "application/wasm"
        if clean.endswith(".js.br") or clean.endswith(".js"):
            return "application/javascript"
        if clean.endswith(".data.br") or clean.endswith(".data"):
            return "application/octet-stream"
        return super().guess_type(path)

    def log_message(self, format, *args):
        pass


def main() -> int:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    handler = functools.partial(BrotliHandler, directory=str(STAGING_DIR))
    httpd = socketserver.TCPServer(("127.0.0.1", 0), handler)
    port = httpd.server_address[1]
    threading.Thread(target=httpd.serve_forever, daemon=True).start()
    try:
        with sync_playwright() as pw:
            browser = pw.chromium.launch(headless=True, channel="msedge")
            ctx = browser.new_context(viewport={"width": 390, "height": 844}, device_scale_factor=3, is_mobile=True, has_touch=True)
            page = ctx.new_page()
            page.goto(f"http://127.0.0.1:{port}/next/", wait_until="domcontentloaded", timeout=60000)
            page.wait_for_function("() => window.__hitmeHomeShown > 0", timeout=60000)
            page.wait_for_timeout(300)

            page.screenshot(path=str(OUT_DIR / "u3_home_server_not_ready.png"))

            # Click CHƠI NHANH
            page.click('#ui-root [data-name="quick-play"]')
            page.wait_for_selector('#r38-online-sheet-shade.active', timeout=5000)
            page.wait_for_timeout(150)
            page.screenshot(path=str(OUT_DIR / "u3_sheet_server_not_ready.png"))

            # Click ĐÁNH THỨC MÁY CHỦ (TỚI 60 GIÂY)
            page.click('#r38-btn-wake-retry')
            page.wait_for_timeout(2100)
            page.screenshot(path=str(OUT_DIR / "u3_sheet_waking_60s.png"))

            # Click CHUYỂN SANG LUYỆN VỚI BOT
            page.click('#r38-btn-go-bot')
            page.wait_for_selector('#ui-root [data-screen="bot"]', timeout=5000)
            page.wait_for_timeout(150)
            page.screenshot(path=str(OUT_DIR / "u3_fallback_to_bot_setup.png"))

            browser.close()
        print("U3_CAPTURE: PASS")
        return 0
    finally:
        httpd.shutdown()
        httpd.server_close()


if __name__ == "__main__":
    raise SystemExit(main())
