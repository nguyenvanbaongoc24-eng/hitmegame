import os
import sys
import time
from pathlib import Path
from playwright.sync_api import sync_playwright

ROOT = Path("D:/HITME GAME")
OUT_DIR = ROOT / "unity-client" / "Evidence" / "round43" / "L5"
OUT_DIR.mkdir(parents=True, exist_ok=True)
WEB_URL = "http://127.0.0.1:8103/index.html"
CHROME_PATH = r"C:\Program Files\Google\Chrome\Application\chrome.exe"

with sync_playwright() as p:
    browser = p.chromium.launch(
        executable_path=CHROME_PATH,
        headless=True,
        args=["--use-gl=angle", "--use-angle=swiftshader", "--no-sandbox"]
    )
    context = browser.new_context(
        viewport={"width": 390, "height": 844},
        is_mobile=True,
        has_touch=True,
        user_agent="Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15"
    )
    page = context.new_page()
    page.goto(f"{WEB_URL}?screen=botpractice", wait_until="domcontentloaded")
    page.wait_for_selector("#unity-loading", state="detached", timeout=20000)
    time.sleep(1.0)
    page.screenshot(path=str(OUT_DIR / "01_bot_practice_setup.png"))
    print("[+] Captured 01_bot_practice_setup.png")

    page.goto(f"{WEB_URL}?screen=settings", wait_until="domcontentloaded")
    page.wait_for_selector("#unity-loading", state="detached", timeout=20000)
    time.sleep(1.0)
    page.screenshot(path=str(OUT_DIR / "02_settings_game_feel_toggle.png"))
    print("[+] Captured 02_settings_game_feel_toggle.png")

    context.close()
    browser.close()
