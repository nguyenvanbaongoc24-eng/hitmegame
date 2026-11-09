import http.server
import socketserver
import threading
import sys
import json
import time
from pathlib import Path
from playwright.sync_api import sync_playwright

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", line_buffering=True)

DIST_DIR = Path("tools/deploy_web/dist").resolve()

class BrotliHTTPHandler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(DIST_DIR), **kwargs)

    def end_headers(self):
        clean_path = self.path.split("?")[0]
        if clean_path.endswith(".br"):
            self.send_header("Content-Encoding", "br")
            if clean_path.endswith(".wasm.br"):
                self.send_header("Content-Type", "application/wasm")
            elif clean_path.endswith(".data.br"):
                self.send_header("Content-Type", "application/octet-stream")
            elif clean_path.endswith(".js.br"):
                self.send_header("Content-Type", "application/javascript")
        elif clean_path.endswith(".wasm"):
            self.send_header("Content-Type", "application/wasm")
        elif clean_path.endswith(".js"):
            self.send_header("Content-Type", "application/javascript")
        super().end_headers()

    def log_message(self, format, *args):
        pass

PORT = 8201
server = socketserver.TCPServer(("127.0.0.1", PORT), BrotliHTTPHandler)
server_thread = threading.Thread(target=server.serve_forever, daemon=True)
server_thread.start()

try:
    with sync_playwright() as pw:
        browser = pw.chromium.launch(channel="msedge", headless=True)
        context = browser.new_context(viewport={"width": 393, "height": 852}, has_touch=True)
        page = context.new_page()

        page.goto(f"http://127.0.0.1:{PORT}/next/?e2e=1")
        page.wait_for_function("() => typeof window.unityInstance !== 'undefined'", timeout=40000)
        page.wait_for_timeout(1500)

        # Test Vợt Pickleball + Chó Vàng
        page.tap(".selection-card.weapon-card")
        page.wait_for_timeout(400)
        page.tap('[data-id="vot_pickleball"]')
        page.wait_for_timeout(400)

        page.tap(".selection-card.character-card")
        page.wait_for_timeout(400)
        page.tap('[data-id="cho_vang"]')
        page.wait_for_timeout(400)

        page.tap(".home .practice")
        page.wait_for_timeout(800)
        page.tap('[data-action="start-practice"]')
        page.wait_for_timeout(1000)

        snap = page.evaluate("""() => ({
            screen: document.querySelector('.stage')?.dataset?.screen,
            summary: document.querySelector('.live-summary')?.innerText,
            weaponChip: document.querySelector('.live-weapon-chip')?.innerText?.trim(),
            cards: document.querySelectorAll('.live-card')?.length,
            phase: window.__hitmeLastMatchEvt?.phase,
            char: window.__hitmeLastMatchEvt?.players?.[0]?.characterId
        })""")
        print("[*] Vợt Pickleball + Chó Vàng test:", json.dumps(snap, ensure_ascii=True))
        assert "Vợt" in snap["weaponChip"], f"Expected Vợt in chip, got {snap['weaponChip']}"
        assert snap["cards"] == 4, f"Expected 4 cards, got {snap['cards']}"
        assert snap["char"] == "cho_vang", f"Expected cho_vang, got {snap['char']}"
        print("[SUCCESS] All weapon and character unlock assertions passed!")
        browser.close()
finally:
    server.shutdown()
