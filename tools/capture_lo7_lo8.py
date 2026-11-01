import os
import sys
import time
from playwright.sync_api import sync_playwright
from PIL import Image

VIEWPORT = {"width": 1170, "height": 2532}
PORT = 8099
BASE_URL = f"http://localhost:{PORT}/"
OUT_DIR = r"D:\HITME GAME\unity-client\Evidence\round34"
ARTIFACT_DIR = r"C:\Users\LENOVO\.gemini\antigravity\brain\ad8b2abf-26b1-4b72-84a1-97aabeca408d"

os.makedirs(OUT_DIR, exist_ok=True)

CAPTURES = [
    ("lo7_01_profile_dialog_1170x2532.png", f"{BASE_URL}?screen=profile"),
    ("lo7_02_shop_weapons_1170x2532.png", f"{BASE_URL}?screen=shop"),
    ("lo7_03_inventory_weapons_1170x2532.png", f"{BASE_URL}?screen=inventory"),
    ("lo7_04_leaderboard_1170x2532.png", f"{BASE_URL}?screen=leaderboard"),
    ("lo8_01_guest_login_1170x2532.png", f"{BASE_URL}?screen=login"),
]

def capture_all():
    with sync_playwright() as p:
        browser = p.chromium.launch(channel="msedge", headless=True)
        context = browser.new_context(
            viewport=VIEWPORT,
            is_mobile=True,
            has_touch=True,
            device_scale_factor=1
        )
        page = context.new_page()

        for filename, url in CAPTURES:
            print(f"Navigating to {url}...")
            page.goto(url, wait_until="networkidle", timeout=45000)
            time.sleep(4.0)

            out_path = os.path.join(OUT_DIR, filename)
            page.screenshot(path=out_path)
            print(f"Saved: {out_path} ({os.path.getsize(out_path)} bytes)")

            artifact_path = os.path.join(ARTIFACT_DIR, filename)
            with open(out_path, "rb") as fsrc, open(artifact_path, "wb") as fdst:
                fdst.write(fsrc.read())

            img = Image.open(out_path)
            print(f"Verified {filename}: format={img.format}, size={img.size}, mode={img.mode}")

        browser.close()

if __name__ == "__main__":
    capture_all()
