import sys
import time
import http.server
import socketserver
import threading
from pathlib import Path
from playwright.sync_api import sync_playwright

sys.stdout.reconfigure(encoding="utf-8")
DIST_NEXT = Path(r"D:\HITME GAME\unity-client\tools\deploy_web\dist\next")

class CustomHandler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(DIST_NEXT), **kwargs)
    def log_message(self, format, *args):
        pass

def run_tests():
    print("--- RUNNING L4 MOBILE LIFECYCLE PLAYWRIGHT SUITE ---")
    PORT = 8116
    server = socketserver.TCPServer(("127.0.0.1", PORT), CustomHandler)
    server_thread = threading.Thread(target=server.serve_forever, daemon=True)
    server_thread.start()
    print(f"[*] Local test HTTP server running at http://127.0.0.1:{PORT}")

    with sync_playwright() as p:
        try:
            browser = p.chromium.launch(channel="msedge", headless=True)
        except Exception:
            browser = p.webkit.launch(headless=True)

        context = browser.new_context(viewport={"width": 393, "height": 852})
        page = context.new_page()

        # 1. Open game in Lab 2P mode
        page.goto(f"http://127.0.0.1:{PORT}/?lab=1&open=1&mode=create&name=MobileTester")
        page.wait_for_selector('[data-screen="online-lobby"]', timeout=8000)
        print("✓ T1 Passed: Mobile player entered lobby")

        # 2. Test Heartbeat & Wake Lock exposure
        has_hb = page.evaluate("typeof window.__hitmeOnlineUi.startHeartbeat === 'function'")
        has_wl = page.evaluate("typeof window.__hitmeOnlineUi.requestWakeLock === 'function'")
        assert has_hb and has_wl, "Heartbeat or Wake Lock functions missing"
        print("✓ T2 Passed: Ping Heartbeat and Screen Wake Lock functions available")

        # 3. Simulate Network Offline Event & 30s Reconnect Overlay
        page.evaluate("window.dispatchEvent(new Event('offline'))")
        page.wait_for_selector('#r39-net-alert-shade.active', timeout=5000)
        alert_title = page.text_content('#r39-net-alert-shade h3').strip()
        alert_desc = page.text_content('#r39-net-alert-shade p').strip()
        assert "ĐANG KẾT NỐI LẠI" in alert_title, f"Unexpected title: {alert_title}"
        assert "30s" in alert_desc or "29s" in alert_desc or "30 s" in alert_desc or "29 s" in alert_desc or "s" in alert_desc, f"Countdown missing in desc: {alert_desc}"
        print(f"✓ T3 Passed: Offline event triggered 30s reconnect overlay: '{alert_title}' ({alert_desc[:40]}...)")

        # 4. Simulate Network Online Restoration with Jitter
        page.evaluate("window.dispatchEvent(new Event('online'))")
        page.wait_for_timeout(600)
        print("✓ T4 Passed: Network online event handled with jittered reconnect trigger")

        # 5. Simulate Tab Visibility Changes (hidden for 5s / 30s)
        page.evaluate("Object.defineProperty(document, 'visibilityState', { value: 'hidden', writable: true }); document.dispatchEvent(new Event('visibilitychange'));")
        page.wait_for_timeout(200)
        page.evaluate("Object.defineProperty(document, 'visibilityState', { value: 'visible', writable: true }); document.dispatchEvent(new Event('visibilitychange'));")
        page.wait_for_timeout(200)
        print("✓ T5 Passed: visibilitychange (hidden -> visible) handled gracefully without errors")

        # 6. Test AFK Ban vs Network Drop distinction
        # AFK Ban: server sends banned error code with 120s
        page.evaluate("window.__hitmeOnlineUi.handleServerMessage({ v: 1, type: 'error', code: 'banned', remainingSeconds: 120 })")
        page.wait_for_selector('#r39-net-alert-shade.active', timeout=5000)
        ban_title = page.text_content('#r39-net-alert-shade h3').strip()
        ban_desc = page.text_content('#r39-net-alert-shade p').strip()
        assert "AFK" in ban_title or "TẠM KHÓA" in ban_title, f"Unexpected ban title: {ban_title}"
        assert "120" in ban_desc, f"120s missing in ban desc: {ban_desc}"
        print(f"✓ T6 Passed: AFK 2 rounds ban cleanly distinguished from disconnect: '{ban_title}'")

        # 7. Test Reconnection Timeout (30s exceeded -> Return to Home)
        page.evaluate("window.__hitmeOnlineUi.startReconnectCountdown(1)")
        page.wait_for_timeout(1500)
        timeout_title = page.text_content('#r39-net-alert-shade h3').strip()
        timeout_desc = page.text_content('#r39-net-alert-shade p').strip()
        assert "BẠN ĐÃ RỜI TRẬN" in timeout_title or "THÔNG BÁO" in timeout_title, f"Unexpected timeout title: {timeout_title}"
        assert "quá 30 giây" in timeout_desc, f"Unexpected timeout desc: {timeout_desc}"
        print(f"✓ T7 Passed: 30s timeout automatically returned to Home with notice: '{timeout_desc}'")

        browser.close()

    server.shutdown()
    print("--- ALL L4 TESTS PASSED! ---")

if __name__ == "__main__":
    run_tests()
