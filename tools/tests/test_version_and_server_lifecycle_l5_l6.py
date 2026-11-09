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
    print("--- RUNNING L5/L6 VERSION & SERVER LIFECYCLE PLAYWRIGHT SUITE ---")
    PORT = 8117
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

        # 1. Open game
        page.goto(f"http://127.0.0.1:{PORT}/?lab=1&open=1&mode=create&name=Tester56")
        page.wait_for_selector('[data-screen="online-lobby"]', timeout=8000)
        print("✓ T1 Passed: Page loaded in lobby")

        # 2. Test Protocol Mismatch Overlay (L5)
        page.evaluate("window.__hitmeOnlineUi.handleServerMessage({ v: 1, type: 'error', code: 'protocol_mismatch' })")
        page.wait_for_selector('#r39-net-alert-shade.active', timeout=5000)
        mismatch_title = page.text_content('#r39-net-alert-shade h3').strip()
        assert "CÓ BẢN MỚI" in mismatch_title, f"Unexpected title: {mismatch_title}"
        assert page.is_visible('#r39-alert-reload-btn'), "Reload button missing on protocol mismatch"
        print(f"✓ T2 Passed: Protocol mismatch triggered 'Có bản mới — Tải lại' modal: '{mismatch_title}'")

        # 3. Test Home Release Polling Toast (L5)
        page.evaluate("window.__hitmeOnlineUi.showUpdateToast('new_build_xyz')")
        page.wait_for_selector('#r40-update-toast', timeout=5000)
        toast_text = page.text_content('#r40-update-toast').strip()
        assert "Có bản cập nhật mới" in toast_text, f"Unexpected toast text: {toast_text}"
        assert page.is_visible('#r40-toast-reload-btn'), "Toast reload button missing"
        print(f"✓ T3 Passed: New release detected triggered floating update toast: '{toast_text}'")

        # 4. Test Sleeping Server 60s Waking Progress (L6)
        page.evaluate("window.__hitmeOpenOnlineSheet(true)")
        page.wait_for_selector('#r38-online-sheet-shade.active', timeout=5000)
        wake_title = page.text_content('#r38-online-title').strip()
        assert "ĐÁNH THỨC" in wake_title, f"Unexpected waking title: {wake_title}"
        assert page.is_visible('#r38-waking-bar'), "Waking progress bar missing"
        assert page.is_visible('#r38-btn-go-bot'), "Chuyển sang bot practice button missing"
        print(f"✓ T4 Passed: Sleeping server 60s waking bar and retry button visible: '{wake_title}'")

        # 5. Test Server Shutdown / Restarting (L6)
        page.evaluate("window.__hitmeOnlineUi.handleServerMessage({ v: 1, type: 'error', code: 'server_shutdown' })")
        page.wait_for_selector('#r39-net-alert-shade.active', timeout=5000)
        shutdown_title = page.text_content('#r39-net-alert-shade h3').strip()
        assert "BẢO TRÌ" in shutdown_title or "MÁY CHỦ" in shutdown_title, f"Unexpected shutdown title: {shutdown_title}"

        # Close shutdown alert and verify it returns to Home cleanly without loop
        page.click('#r39-alert-close-btn')
        page.wait_for_timeout(300)
        is_online_active = page.evaluate("document.getElementById('r39-online-root')?.classList.contains('active') || false")
        assert not is_online_active, "Online root still active after server_shutdown close"
        print(f"✓ T5 Passed: Server shutdown closed cleanly back to Home without infinite loop: '{shutdown_title}'")

        browser.close()

    server.shutdown()
    print("--- ALL L5/L6 TESTS PASSED! ---")

if __name__ == "__main__":
    run_tests()
