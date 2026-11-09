#!/usr/bin/env python3
"""
Captures Round 38 Step U2 evidence:
1) Daily Quests modal before & after completing a Bot Practice match (with experimental claim)
2) Sample Leaderboard modal with local stats and 'Dữ liệu mẫu • Bảng xếp hạng thật mở khi có máy chủ' banner
"""
from __future__ import annotations

import functools
import http.server
import socketserver
import threading
from pathlib import Path
from playwright.sync_api import sync_playwright

STAGING_DIR = Path(r"D:\HITME GAME\_web_dist_deploy")
OUT_DIR = Path(r"D:\HITME GAME\Evidence\round38\U2_screens")


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

            # Simulate completing 1 bot practice match (rank 1, 4 hits, 4 rounds)
            page.evaluate("""() => {
              window.__hitmeRecordBotResult({
                type: "result",
                round: 4,
                rankings: [
                  {rank: 1, id: "player", name: "BẠN", characterId: "nam_lang", hits: 4, roundSurvived: 4}
                ]
              });
            }""")

            # Open Daily Quests
            page.click('#ui-root [data-action="quests"]')
            page.wait_for_selector('#ui-root .r38-u2-content', timeout=5000)
            page.wait_for_timeout(150)
            page.screenshot(path=str(OUT_DIR / "u2_daily_quests_ready_to_claim.png"))

            # Click experimental claim on play_1_match
            page.click('#ui-root [data-quest-claim="play_1_match"]')
            page.wait_for_timeout(150)
            page.screenshot(path=str(OUT_DIR / "u2_daily_quests_claimed.png"))

            # Close and open Leaderboard
            page.click('#ui-root .modal [data-action="close"]')
            page.wait_for_timeout(150)
            page.click('#ui-root [data-action="rank"]')
            page.wait_for_selector('#ui-root .r38-u2-content', timeout=5000)
            page.wait_for_timeout(150)
            page.screenshot(path=str(OUT_DIR / "u2_sample_leaderboard.png"))

            browser.close()
        print("U2_CAPTURE: PASS")
        return 0
    finally:
        httpd.shutdown()
        httpd.server_close()


if __name__ == "__main__":
    raise SystemExit(main())
