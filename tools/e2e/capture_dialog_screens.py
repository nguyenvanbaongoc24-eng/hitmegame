#!/usr/bin/env python3
"""
Capture crisp screenshots of all new dialog screens on iPhone viewport:
- Shop Dialog
- Inventory Dialog
- Profile Dialog
- Settings Dialog
- Leaderboard Tab
- Quests Tab
- Login Dialog
"""

import sys
import time
from pathlib import Path
from playwright.sync_api import sync_playwright

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8")

ROOT = Path("D:/HITME GAME")
WEB_URL = "http://127.0.0.1:8103/index.html"
CHROME_PATH = r"C:\Program Files\Google\Chrome\Application\chrome.exe"
SCREENS_DIR = ROOT / "unity-client" / "Evidence" / "round41" / "e2e" / "screens"
BRAIN_DIR = Path(r"C:\Users\LENOVO\.gemini\antigravity\brain\ad8b2abf-26b1-4b72-84a1-97aabeca408d")
SCREENS_DIR.mkdir(parents=True, exist_ok=True)

IPHONE_CONFIG = {
    "viewport": {"width": 390, "height": 844},
    "user_agent": "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1",
    "device_scale_factor": 3,
    "is_mobile": True,
    "has_touch": True,
}

def save_both(page, filename):
    p1 = SCREENS_DIR / filename
    p2 = BRAIN_DIR / filename
    page.screenshot(path=str(p1))
    page.screenshot(path=str(p2))
    print(f"  Đã lưu ảnh chụp: {filename}")

def run():
    with sync_playwright() as p:
        browser = p.chromium.launch(
            executable_path=CHROME_PATH,
            headless=True,
            args=[
                "--use-gl=angle",
                "--use-angle=swiftshader",
                "--enable-webgl",
                "--no-sandbox",
                "--disable-dev-shm-usage"
            ]
        )
        context = browser.new_context(**IPHONE_CONFIG)
        page = context.new_page()

        print("Tải trang WebGL Release trên cổng 8103...")
        page.goto(f"{WEB_URL}?e2e=1", wait_until="domcontentloaded")
        page.wait_for_selector("#unity-loading", state="detached", timeout=35000)
        time.sleep(2.0)
        print("Trang Home đã sẵn sàng!")

        # 1. Shop Dialog
        print("Mở Shop Dialog...")
        page.mouse.click(78, 827)
        time.sleep(1.0)
        save_both(page, "13_dialog_shop.png")

        # Close Shop
        page.mouse.click(195, 742)
        time.sleep(0.5)

        # 2. Inventory Dialog
        print("Mở Inventory Dialog...")
        page.mouse.click(156, 827)
        time.sleep(1.0)
        save_both(page, "14_dialog_inventory.png")

        # Close Inventory
        page.mouse.click(195, 742)
        time.sleep(0.5)

        # 3. Profile Dialog
        print("Mở Profile Dialog...")
        page.mouse.click(70, 59)
        time.sleep(1.0)
        save_both(page, "15_dialog_profile.png")

        # Close Profile
        page.mouse.click(195, 742)
        time.sleep(0.5)

        # 4. Settings Dialog
        print("Mở Settings Dialog...")
        page.mouse.click(351, 59)
        time.sleep(1.0)
        save_both(page, "16_dialog_settings.png")

        # Close Settings
        page.mouse.click(195, 715)
        time.sleep(0.5)

        # 5. Leaderboard Dialog (Tab 0)
        print("Mở Leaderboard Dialog...")
        page.mouse.click(234, 827)
        time.sleep(1.0)
        save_both(page, "17_dialog_leaderboard.png")

        # 6. Switch to Quests Tab (Tab 1)
        print("Chuyển sang Tab Quests...")
        # Tab 2 (Quests) is at x = 0.76 of card (roughly x = 275, y = 175)
        page.mouse.click(275, 175)
        time.sleep(1.0)
        save_both(page, "18_dialog_quests.png")

        # Close Leaderboard/Quests
        page.mouse.click(195, 742)
        time.sleep(0.5)

        # 7. Login Dialog
        print("Mở Login Dialog...")
        page.mouse.click(312, 827)
        time.sleep(1.0)
        save_both(page, "19_dialog_login.png")

        # Close Login
        page.mouse.click(195, 742)
        time.sleep(0.5)

        print("\nHOÀN TẤT CHỤP ẢNH TẤT CẢ CÁC MÀN MỚI!")
        context.close()
        browser.close()

if __name__ == "__main__":
    run()
