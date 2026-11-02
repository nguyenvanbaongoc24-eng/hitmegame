from playwright.sync_api import sync_playwright
import time

def test_load():
    with sync_playwright() as p:
        browser = p.chromium.launch(
            executable_path=r"C:\Program Files\Google\Chrome\Application\chrome.exe",
            headless=True,
            args=["--use-gl=angle", "--use-angle=swiftshader", "--no-sandbox"]
        )
        context = browser.new_context(
            viewport={"width": 390, "height": 844},
            is_mobile=True,
            has_touch=True
        )
        page = context.new_page()
        console_msgs = []
        page.on("console", lambda msg: console_msgs.append(f"[{msg.type}] {msg.text}"))
        
        t0 = time.time()
        print("Navigating to WebGL page...")
        page.goto("http://127.0.0.1:8103/index.html?e2e=1", wait_until="domcontentloaded")
        
        # Wait up to 15s for loading screen to disappear
        print("Waiting for unity-loading to disappear...")
        try:
            page.wait_for_selector("#unity-loading", state="detached", timeout=15000)
            elapsed = time.time() - t0
            print(f"HOME_SHOWN achieved in {elapsed:.2f}s!")
        except Exception as e:
            print(f"Wait failed: {e}")
            
        screenshot_path = "D:/HITME GAME/unity-client/Evidence/round41/e2e/screens/00_test_load_390x844.png"
        page.screenshot(path=screenshot_path)
        print(f"Screenshot saved to {screenshot_path}")
        
        test_state = page.evaluate("window.__hitmeTest ? window.__hitmeTest.getState() : null")
        print(f"Test state: {test_state}")
        
        print(f"Console messages ({len(console_msgs)}):")
        for m in console_msgs[:10]:
            print("  ", m)
            
        browser.close()

if __name__ == "__main__":
    test_load()
