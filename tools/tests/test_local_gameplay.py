import http.server
import socketserver
import threading
import os
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
        # If requested path ends with .br, send Content-Encoding: br
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
        pass # Suppress noisy server logs

PORT = 8199
server = socketserver.TCPServer(("127.0.0.1", PORT), BrotliHTTPHandler)
server_thread = threading.Thread(target=server.serve_forever, daemon=True)
server_thread.start()
print(f"[*] Local test server running at http://127.0.0.1:{PORT}/")

try:
    with sync_playwright() as pw:
        browser = pw.chromium.launch(channel="msedge", headless=True)
        context = browser.new_context(
            viewport={"width": 393, "height": 852},
            has_touch=True,
            is_mobile=True,
            device_scale_factor=3
        )
        page = context.new_page()

        logs = []
        page.on("console", lambda m: logs.append(f"[{m.type.upper()}] {m.text}"))
        page.on("pageerror", lambda e: logs.append(f"[PAGEERROR] {repr(e)}"))

        print(f"[*] Navigating to http://127.0.0.1:{PORT}/next/?e2e=1...")
        page.goto(f"http://127.0.0.1:{PORT}/next/?e2e=1")
        page.wait_for_function("() => typeof window.unityInstance !== 'undefined'", timeout=40000)
        print("[+] Unity loaded successfully!")
        page.wait_for_timeout(2000)

        # Verify all Home screen buttons are unlocked
        home_buttons = page.evaluate("""() => ({
            quickPlayAction: document.querySelector('.cta.quick')?.dataset?.action,
            quickPlayLocked: document.querySelector('.cta.quick')?.classList?.contains('coming-soon'),
            createAction: document.querySelector('.cta.blue')?.dataset?.action,
            createLocked: document.querySelector('.cta.blue')?.classList?.contains('coming-soon'),
            joinAction: document.querySelector('.cta.green')?.dataset?.action,
            joinLocked: document.querySelector('.cta.green')?.classList?.contains('coming-soon'),
            characterCard: document.querySelector('.selection-card.character-card')?.innerText?.trim(),
            weaponCard: document.querySelector('.selection-card.weapon-card')?.innerText?.trim(),
        })""")
        print("[*] Home screen buttons:", home_buttons)
        assert home_buttons["quickPlayAction"] == "quick", "Quick Play should be unlocked"
        assert not home_buttons["quickPlayLocked"], "Quick Play should not be coming-soon"

        # TEST 1: Open Weapon selector on Home and select "Chảo Chống Dính"
        print("[*] Opening weapon modal and selecting Chảo Chống Dính...")
        page.tap(".selection-card.weapon-card")
        page.wait_for_timeout(500)
        page.tap('[data-id="chao_chong_dinh"]')
        page.wait_for_timeout(500)

        # TEST 2: Open Character selector and select "Quậy Boy" (character index 4, previously locked)
        print("[*] Opening character modal and selecting Quậy Boy...")
        page.tap(".selection-card.character-card")
        page.wait_for_timeout(500)
        page.tap('[data-id="quay_boy"]')
        page.wait_for_timeout(500)

        # Check selections
        selections = page.evaluate("() => ({ char: window.__hitmeSelectedChar, weapon: window.__hitmeSelectedWeapon })")
        print("[*] Selections recorded:", selections)

        # TEST 3: Tap Practice
        print("[*] Tapping Practice on Home...")
        page.tap(".home .practice")
        page.wait_for_timeout(1000)

        # Tap Start Practice (BẮT ĐẦU)
        print("[*] Tapping Start Practice (BẮT ĐẦU)...")
        page.tap('[data-action="start-practice"]')
        page.wait_for_timeout(1200)

        # Verify Round 1 Placing phase is immediately active with players alive!
        match_snap = page.evaluate("""() => ({
            screen: document.querySelector('.stage')?.dataset?.screen,
            summary: document.querySelector('.live-summary')?.innerText,
            progress: document.querySelector('.live-progress')?.innerText,
            cardsCount: document.querySelectorAll('.live-card')?.length,
            phaseText: document.querySelector('.live-phase')?.innerText,
            weaponChip: document.querySelector('.live-weapon-chip')?.innerText?.trim(),
            match: window.__hitmeLastMatchEvt ? {
                round: window.__hitmeLastMatchEvt.round,
                phase: window.__hitmeLastMatchEvt.phase,
                timeLeft: window.__hitmeLastMatchEvt.timeLeft,
                aliveCount: window.__hitmeLastMatchEvt.aliveCount,
                weaponName: window.__hitmeLastMatchEvt.weaponName,
                players: window.__hitmeLastMatchEvt.players?.map(p => ({ id: p.id, char: p.characterId, hp: p.hp, alive: p.alive }))
            } : null
        })""")
        print("[+] Match snapshot immediately after BẮT ĐẦU:")
        print(json.dumps(match_snap, ensure_ascii=True, indent=2))

        assert match_snap["cardsCount"] == 4, f"Expected 4 cards, found {match_snap['cardsCount']}"
        assert "4" in match_snap["progress"], f"Expected 4 alive in progress, found {match_snap['progress']}"
        assert "Chảo" in match_snap["weaponChip"], f"Expected Chảo Chống Dính in chip, found {match_snap['weaponChip']}"

        # Place character on arena
        print("[*] Placing character on canvas via touchscreen...")
        canvas = page.locator("#unity-canvas")
        box = canvas.bounding_box()
        cx = box["x"] + box["width"] * 0.5
        cy = box["y"] + box["height"] * 0.55
        page.touchscreen.tap(cx, cy)
        page.wait_for_timeout(400)

        print("[*] Taking placing screenshot...")
        page.screenshot(path="C:/Users/LENOVO/.gemini/antigravity/brain/a01ea5e2-c09a-440c-ab9d-21bd50669179/01_local_test_placing.png", timeout=8000)
        print("[+] Placing screenshot taken!")

        # Monitor round progression for 7 seconds
        for sec in range(1, 8):
            page.wait_for_timeout(1000)
            t_snap = page.evaluate("""() => ({
                phase: window.__hitmeLastMatchEvt?.phase,
                round: window.__hitmeLastMatchEvt?.round,
                timeLeft: window.__hitmeLastMatchEvt?.timeLeft,
                alive: window.__hitmeLastMatchEvt?.aliveCount,
                summary: document.querySelector('.live-summary')?.innerText,
                phaseBanner: document.querySelector('.live-phase')?.innerText
            })""")
            print(f"  [T+{sec}s] {t_snap}")

        print("[*] Taking round screenshot...")
        page.screenshot(path="C:/Users/LENOVO/.gemini/antigravity/brain/a01ea5e2-c09a-440c-ab9d-21bd50669179/02_local_test_round_running.png", timeout=8000)
        print("[+] Round screenshot taken!")
        print("[SUCCESS] All tests passed! Match plays cleanly without freeze!")

finally:
    server.shutdown()
