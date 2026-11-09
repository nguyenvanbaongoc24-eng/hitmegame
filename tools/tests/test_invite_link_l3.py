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
    print("--- RUNNING L3 INVITE LINK E2E PLAYWRIGHT SUITE ---")
    PORT = 8115
    server = socketserver.TCPServer(("127.0.0.1", PORT), CustomHandler)
    server_thread = threading.Thread(target=server.serve_forever, daemon=True)
    server_thread.start()
    print(f"[*] Local test HTTP server running at http://127.0.0.1:{PORT}")

    with sync_playwright() as p:
        try:
            browser = p.chromium.launch(channel="msedge", headless=True)
        except Exception:
            browser = p.webkit.launch(headless=True)

        # Context A: Host creates room
        context_a = browser.new_context(viewport={"width": 393, "height": 852})
        page_a = context_a.new_page()
        page_a.goto(f"http://127.0.0.1:{PORT}/?lab=1&open=1&mode=create&name=HostPlayer")
        page_a.wait_for_selector('[data-screen="online-lobby"]', timeout=8000)
        print("✓ Context A: Lobby opened, room created")

        room_code = page_a.text_content('[data-name="lobby-room-code"]').strip()
        print(f"✓ Context A: Room code is '{room_code}'")
        assert len(room_code) == 6, f"Invalid room code length: {room_code}"

        # Context A clicks copy invite link
        copy_btn = page_a.wait_for_selector('[data-on-act="copy-invite-link"]')
        copy_btn.click()
        page_a.wait_for_timeout(300)
        btn_text = copy_btn.text_content()
        assert "ĐÃ CHÉP" in btn_text, f"Copy button text not updated: {btn_text}"
        print(f"✓ Context A: Copy button updated feedback: '{btn_text}'")

        # Context B: Invitee opens invite link ?join=HM4826
        context_b = browser.new_context(viewport={"width": 393, "height": 852})
        page_b = context_b.new_page()
        page_b.goto(f"http://127.0.0.1:{PORT}/?lab=1&join={room_code}&name=InviteeUser")
        page_b.wait_for_selector('[data-screen="online-lobby"]', timeout=8000)
        b_code = page_b.text_content('[data-name="lobby-room-code"]').strip()
        assert b_code == room_code, f"Context B room code mismatch: {b_code} vs {room_code}"
        print(f"✓ Context B: Auto-joined lobby via invite link ?join={room_code} successfully")

        # Context B Error Case 1: Room Not Found (BADRM9)
        page_err1 = context_b.new_page()
        page_err1.goto(f"http://127.0.0.1:{PORT}/?lab=1&join=BADRM9&name=ErrUser1")
        page_err1.wait_for_selector('#r39-net-alert-shade.active', timeout=8000)
        alert_title1 = page_err1.text_content('#r39-net-alert-shade h3').strip()
        alert_desc1 = page_err1.text_content('#r39-net-alert-shade p').strip()
        assert alert_title1 == "KHÔNG THỂ VÀO PHÒNG", f"Unexpected title: {alert_title1}"
        assert "Không tìm thấy" in alert_desc1, f"Unexpected message: {alert_desc1}"
        assert page_err1.is_visible('#r39-alert-newroom-btn'), "Tạo phòng mới button missing"
        assert page_err1.is_visible('#r39-alert-bot-btn'), "Luyện với bot button missing"
        print("✓ Error Case 1 Passed: BADRM9 handled with friendly alert and action buttons")

        # Context B Error Case 2: Room Full (MAX999)
        page_err2 = context_b.new_page()
        page_err2.goto(f"http://127.0.0.1:{PORT}/?lab=1&join=MAX999&name=ErrUser2")
        page_err2.wait_for_selector('#r39-net-alert-shade.active', timeout=8000)
        alert_desc2 = page_err2.text_content('#r39-net-alert-shade p').strip()
        assert "đủ 6 người" in alert_desc2, f"Unexpected message: {alert_desc2}"
        print("✓ Error Case 2 Passed: MAX999 handled with friendly alert: " + alert_desc2)

        # Context B Error Case 3: Room Started (STRT99)
        page_err3 = context_b.new_page()
        page_err3.goto(f"http://127.0.0.1:{PORT}/?lab=1&join=STRT99&name=ErrUser3")
        page_err3.wait_for_selector('#r39-net-alert-shade.active', timeout=8000)
        alert_desc3 = page_err3.text_content('#r39-net-alert-shade p').strip()
        assert "đã bắt đầu" in alert_desc3, f"Unexpected message: {alert_desc3}"
        print("✓ Error Case 3 Passed: STRT99 handled with friendly alert: " + alert_desc3)

        # Context B Error Case 4: Room Expired (EXP999)
        page_err4 = context_b.new_page()
        page_err4.goto(f"http://127.0.0.1:{PORT}/?lab=1&join=EXP999&name=ErrUser4")
        page_err4.wait_for_selector('#r39-net-alert-shade.active', timeout=8000)
        alert_desc4 = page_err4.text_content('#r39-net-alert-shade p').strip()
        assert "hết hạn" in alert_desc4, f"Unexpected message: {alert_desc4}"
        print("✓ Error Case 4 Passed: EXP999 handled with friendly alert: " + alert_desc4)

        browser.close()

    server.shutdown()
    print("--- ALL L3 TESTS PASSED! ---")

if __name__ == "__main__":
    run_tests()
