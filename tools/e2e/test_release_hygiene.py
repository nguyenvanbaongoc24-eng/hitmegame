import sys
from playwright.sync_api import sync_playwright

if sys.stdout.encoding and sys.stdout.encoding.lower() != 'utf-8':
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')

CHROME_PATH = r"C:\Program Files\Google\Chrome\Application\chrome.exe"
WEB_URL = "http://127.0.0.1:8103/index.html"

def test_release_hygiene():
    print("Testing Release build hygiene (no test bridges in production)...")
    with sync_playwright() as p:
        browser = p.chromium.launch(
            executable_path=CHROME_PATH,
            headless=True,
            args=["--use-gl=angle", "--use-angle=swiftshader", "--no-sandbox"]
        )
        page = browser.new_page()

        # 1. Open normal URL WITHOUT ?e2e=1
        page.goto(WEB_URL, wait_until="domcontentloaded")
        page.wait_for_selector("#unity-loading", state="detached", timeout=35000)

        checks = page.evaluate("""() => {
            return {
                origWs: typeof window.__OrigWebSocket,
                hitmeTest: typeof window.__hitmeTest,
                hitmeTestState: typeof window.__hitmeTestState,
                hitmeActiveSocket: typeof window.__hitmeActiveSocket
            };
        }""")

        print("Normal page checks (without ?e2e=1):", checks)
        assert checks["origWs"] == "undefined", f"Leaked __OrigWebSocket: {checks['origWs']}"
        assert checks["hitmeTest"] == "undefined", f"Leaked __hitmeTest: {checks['hitmeTest']}"
        assert checks["hitmeTestState"] == "undefined", f"Leaked __hitmeTestState: {checks['hitmeTestState']}"
        assert checks["hitmeActiveSocket"] == "undefined", f"Leaked __hitmeActiveSocket: {checks['hitmeActiveSocket']}"

        # 2. Open WITH ?e2e=1
        page.goto(WEB_URL + "?e2e=1", wait_until="domcontentloaded")
        checks_e2e = page.evaluate("""() => {
            return {
                origWs: typeof window.__OrigWebSocket,
                hitmeTest: typeof window.__hitmeTest,
                hitmeTestState: typeof window.__hitmeTestState
            };
        }""")
        print("E2E page checks (with ?e2e=1):", checks_e2e)
        assert checks_e2e["origWs"] == "function", "Missing __OrigWebSocket in e2e"
        assert checks_e2e["hitmeTest"] == "object", "Missing __hitmeTest in e2e"
        assert checks_e2e["hitmeTestState"] == "object", "Missing __hitmeTestState in e2e"

        browser.close()
    print("HYGIENE TEST PASSED: No test variables leaked into Release mode!")

if __name__ == "__main__":
    test_release_hygiene()
