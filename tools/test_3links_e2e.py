import sys
import time
from pathlib import Path
from playwright.sync_api import sync_playwright

sys.stdout.reconfigure(encoding='utf-8')

out_dir = Path(r"d:\HITME GAME\unity-client\Evidence\e2e_3links")
out_dir.mkdir(parents=True, exist_ok=True)

def test_endpoints():
    print("[*] Starting E2E verification on localhost:8093 with WebKit...")
    with sync_playwright() as p:
        browser = p.webkit.launch(headless=True)
        context = browser.new_context(
            viewport={"width": 393, "height": 695},
            device_scale_factor=3,
            is_mobile=True,
            has_touch=True
        )

        # 1. Test /ui-preview/
        print("\n--- Testing /ui-preview/ ---")
        page = context.new_page()
        page.goto("http://localhost:8093/ui-preview/?mock=1", wait_until="networkidle")
        time.sleep(1)
        page.screenshot(path=str(out_dir / "preview_home_393x695.png"))
        print(f"[+] Saved screenshot: {out_dir / 'preview_home_393x695.png'}")

        # Switch character
        arrow_right = page.locator("[data-name='arrow-right']")
        arrow_right.click()
        time.sleep(0.5)
        page.screenshot(path=str(out_dir / "preview_switch_char_393x695.png"))
        print(f"[+] Saved screenshot: {out_dir / 'preview_switch_char_393x695.png'}")

        # Practice click
        practice_btn = page.locator("[data-name='practice']")
        practice_btn.click()
        time.sleep(1)
        page.screenshot(path=str(out_dir / "preview_match_393x695.png"))
        print(f"[+] Saved screenshot: {out_dir / 'preview_match_393x695.png'}")

        # 2. Test root /
        print("\n--- Testing root / ---")
        page_root = context.new_page()
        page_root.goto("http://localhost:8093/", wait_until="domcontentloaded")
        time.sleep(2)
        page_root.screenshot(path=str(out_dir / "root_unity_393x695.png"))
        print(f"[+] Saved screenshot: {out_dir / 'root_unity_393x695.png'}")

        # 3. Test /next/
        print("\n--- Testing /next/ ---")
        page_next = context.new_page()
        page_next.goto("http://localhost:8093/next/?v=1f1cdfa", wait_until="domcontentloaded")
        time.sleep(2)
        page_next.screenshot(path=str(out_dir / "next_hybrid_393x695.png"))
        print(f"[+] Saved screenshot: {out_dir / 'next_hybrid_393x695.png'}")

        browser.close()
        print("\n[ALL SCREENSHOTS CAPTURED SUCCESSFULLY!]")

if __name__ == "__main__":
    test_endpoints()
