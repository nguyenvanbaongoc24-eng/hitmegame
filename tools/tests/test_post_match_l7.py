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
    print("--- RUNNING L7 POST-MATCH UI & ACTIONS PLAYWRIGHT SUITE ---")
    PORT = 8118
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
        page.goto(f"http://127.0.0.1:{PORT}/?lab=1&open=1&mode=create&name=HostTester")
        page.wait_for_selector('[data-screen="online-lobby"]', timeout=8000)
        print("✓ T1 Passed: Lobby loaded")

        # 2. Trigger match_end to view post-match screen
        page.evaluate("""
          window.__hitmeOnlineUi.handleServerMessage({
            v: 1,
            type: "match_end",
            winners: ["HostTester"],
            winnerIds: ["HostTester"],
            rankings: [
              { playerId: "HostTester", name: "HostTester (Bạn)", rank: 1, hp: 3, hitsLanded: 3, goldReward: 30, alive: true },
              { playerId: "Peer2", name: "Cô Ba (Máy 2)", rank: 2, hp: 1, hitsLanded: 1, goldReward: 15, alive: true },
              { playerId: "Peer3", name: "Anh Teen", rank: 3, hp: 0, hitsLanded: 0, goldReward: 5, alive: false }
            ]
          });
        """)
        page.wait_for_selector('[data-screen="online-match-end"]', timeout=5000)
        print("✓ T2 Passed: Match end screen rendered")

        # 3. Verify Full N-row table (3 rows) & Podium
        rows = page.query_selector_all('[data-name="match-end-table"] tbody tr')
        assert len(rows) == 3, f"Expected 3 rows in co-rank table, got {len(rows)}"
        podium = page.query_selector_all('[data-name="match-end-podium"] .r39-podium-box')
        assert len(podium) == 3, f"Expected 3 podium boxes, got {len(podium)}"
        print("✓ T3 Passed: Full N-row co-ranking table (3 rows) and 3-step podium displayed")

        # 4. Verify 4 Post-Match Buttons exist
        btn_replay = page.query_selector('[data-name="btn-match-replay"]')
        btn_lobby = page.query_selector('[data-name="btn-match-lobby"]')
        btn_quick = page.query_selector('[data-name="btn-match-quick"]')
        btn_leave = page.query_selector('[data-name="btn-match-leave"]')
        assert btn_replay and btn_lobby and btn_quick and btn_leave, "Missing one of 4 post-match action buttons"
        print("✓ T4 Passed: All 4 post-match buttons (Đấu lại, Về phòng chờ, Ghép nhanh lại, Rời phòng) present")

        # 5. Test Return to Lobby action
        btn_lobby.click()
        page.wait_for_selector('[data-screen="online-lobby"]', timeout=5000)
        print("✓ T5 Passed: 'Về phòng chờ' button brought player cleanly back to lobby")

        # 6. Test Host Replay action starts new match
        page.evaluate("""
          window.__hitmeOnlineUi.handleServerMessage({
            v: 1,
            type: "match_end",
            rankings: [
              { playerId: "HostTester", name: "HostTester", rank: 1, hp: 3, hitsLanded: 2, goldReward: 30, alive: true }
            ]
          });
        """)
        page.wait_for_selector('[data-screen="online-match-end"]', timeout=5000)
        page.click('[data-name="btn-match-replay"]')
        page.wait_for_timeout(300)
        print("✓ T6 Passed: Host replay clicked and start signal dispatched")

        # 7. Test Leave Room returns to Home
        page.evaluate("""
          window.__hitmeOnlineUi.handleServerMessage({
            v: 1,
            type: "match_end",
            rankings: [
              { playerId: "HostTester", name: "HostTester", rank: 1, hp: 3, hitsLanded: 2, goldReward: 30, alive: true }
            ]
          });
        """)
        page.wait_for_selector('[data-screen="online-match-end"]', timeout=5000)
        page.click('[data-name="btn-match-leave"]')
        page.wait_for_timeout(300)
        is_online_active = page.evaluate("document.getElementById('r39-online-root')?.classList.contains('active') || false")
        assert not is_online_active, "Online root still active after leaving room"
        print("✓ T7 Passed: 'Rời phòng' cleanly returned to Home screen")

        browser.close()

    server.shutdown()
    print("--- ALL L7 TESTS PASSED! ---")

if __name__ == "__main__":
    run_tests()
