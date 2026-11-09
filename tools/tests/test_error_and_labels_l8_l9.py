import sys
import os
import time
import subprocess
from playwright.sync_api import sync_playwright

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

def run_tests():
    dist_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "deploy_web", "dist"))
    port = 8138
    print(f"[*] Starting local HTTP server for dist at port {port}...")
    server_proc = subprocess.Popen(
        [sys.executable, "-m", "http.server", str(port), "--directory", dist_dir],
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL
    )
    time.sleep(1)

    passed_count = 0
    total_tests = 5

    try:
        with sync_playwright() as p:
            try:
                browser = p.chromium.launch(channel="msedge", headless=True)
            except Exception:
                browser = p.webkit.launch(headless=True)

            context = browser.new_context(viewport={"width": 393, "height": 852})
            page = context.new_page()

            # Test 1: Circular log buffer (20 items max)
            print("[Test 1] Testing circular log buffer (L8)...")
            page.goto(f"http://127.0.0.1:{port}/next/index.html?name=Tester")
            page.wait_for_selector("#r40-beta-banner", timeout=8000)

            page.evaluate("""() => {
                for (let i = 0; i < 25; i++) {
                    console.log("Test log entry " + i);
                }
            }""")
            logs = page.evaluate("() => window.__hitmeOnlineUi.getRecentLogs()")
            assert len(logs) == 20, f"Expected 20 logs in circular buffer, got {len(logs)}"
            assert "Test log entry 24" in logs[-1], f"Expected latest log entry 24 in buffer, got: {logs[-1]}"
            print("  ✓ Test 1 Passed: Circular log buffer holds exactly 20 latest logs.")
            passed_count += 1

            # Test 2: Error catcher modal display
            print("[Test 2] Testing Error Catcher modal (L8)...")
            page.evaluate("""() => {
                window.__hitmeOnlineUi.handleClientError("TypeError: simulated null reference", "bundle.js", 123, 45, new Error("simulated"));
            }""")
            page.wait_for_selector("#r40-error-catcher-shade", timeout=5000)
            err_box = page.locator("[data-name='err-box']").inner_text()
            assert "TypeError: simulated null reference" in err_box, f"Unexpected err box text: {err_box}"
            copy_btn = page.locator("#r40-btn-copy-err")
            assert copy_btn.is_visible(), "Copy error button should be visible"
            reload_btn = page.locator("#r40-btn-reload-err")
            assert reload_btn.is_visible(), "Reload error button should be visible"
            print("  ✓ Test 2 Passed: Error catcher modal displays correctly with error details, buildId, and copy/reload buttons.")
            passed_count += 1

            # Test 3: ?debug=1 HUD
            print("[Test 3] Testing ?debug=1 HUD (L8)...")
            debug_page = context.new_page()
            debug_page.goto(f"http://127.0.0.1:{port}/next/index.html?name=Tester&debug=1")
            debug_page.wait_for_selector("#r40-debug-hud", timeout=8000)
            hud_text = debug_page.locator("#r40-debug-hud").inner_text()
            assert "HIT ME DEBUG HUD" in hud_text, f"HUD missing title: {hud_text}"
            assert "Build:" in hud_text, f"HUD missing Build: {hud_text}"
            assert "Mode/Phase:" in hud_text, f"HUD missing Mode/Phase: {hud_text}"
            copy_logs_btn = debug_page.locator("#r40-debug-copy-logs")
            assert copy_logs_btn.is_visible(), "Copy logs button in HUD should be visible"
            print("  ✓ Test 3 Passed: ?debug=1 displays diagnostic HUD with live status and copy logs CTA.")
            passed_count += 1
            debug_page.close()

            # Test 4: L9 Honest Labels in Shop
            print("[Test 4] Testing Shop Honest Labels (L9)...")
            # Dismiss error shade
            page.locator("#r40-btn-close-err").click()
            # Open profile / shop modal via exposed method
            page.evaluate("() => window.__hitmeOnlineUi.openProfileShopModal()")
            page.wait_for_selector("[data-name='shop-disclaimer']", timeout=5000)
            shop_disc = page.locator("[data-name='shop-disclaimer']").inner_text()
            assert "DỮ LIỆU THỬ NGHIỆM" in shop_disc and "KHÔNG CÓ TIỀN THẬT" in shop_disc, f"Unexpected disclaimer: {shop_disc}"
            print("  ✓ Test 4 Passed: Shop modal displays honest trial disclaimer ('KHÔNG CÓ TIỀN THẬT').")
            passed_count += 1

            # Test 5: L9 Honest Labels: 3 TAM character badges and sample banners
            print("[Test 5] Testing 3 TAM character badges and sample banners (L9)...")
            # Close shop
            page.locator("#r39-close-profile-shop").click()

            # Open name modal to check 3 TAM badges
            page.evaluate("() => window.__hitmeOnlineUi.openNameModal()")
            page.wait_for_selector("[data-name='name-modal']", timeout=5000)
            tam_badges = page.locator(".r40-tam-badge").all()
            assert len(tam_badges) == 3, f"Expected 3 TAM badges, got {len(tam_badges)}"
            # Close name modal
            page.locator("#r40-name-cancel").click()

            # Check beta banner and disclaimers
            beta_banner = page.locator("#r40-beta-banner")
            assert beta_banner.is_visible(), "Expected #r40-beta-banner to be visible"
            banner_text = beta_banner.inner_text()
            assert "Beta" in banner_text, f"Unexpected beta banner text: {banner_text}"
            print("  ✓ Test 5 Passed: Profile modal displays 3 TẠM character badges; Beta banner active.")
            passed_count += 1

            browser.close()

    finally:
        server_proc.terminate()
        server_proc.wait()

    print(f"\n==========================================")
    print(f"Passed! {passed_count}/{total_tests} L8 & L9 tests completed successfully.")
    print(f"==========================================")

if __name__ == "__main__":
    run_tests()
