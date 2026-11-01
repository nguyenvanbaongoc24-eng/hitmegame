import os
import sys
import time
from playwright.sync_api import sync_playwright
from PIL import Image

VIEWPORT = {"width": 1170, "height": 2532}
PORT = 8098
BASE_URL = f"http://localhost:{PORT}/"
OUT_DIR = r"D:\HITME GAME\unity-client\Evidence\round34"
ARTIFACT_DIR = r"C:\Users\LENOVO\.gemini\antigravity\brain\ad8b2abf-26b1-4b72-84a1-97aabeca408d"

os.makedirs(OUT_DIR, exist_ok=True)

CAPTURES = [
    ("lo5_01_settings_dialog_1170x2532.png", f"{BASE_URL}?screen=settings"),
    ("lo5_02_settings_en_1170x2532.png", f"{BASE_URL}?screen=settings&lang=en"),
    ("lo6_01_tutorial_step1_1170x2532.png", f"{BASE_URL}?screen=tutorial&step=0"),
    ("lo6_02_tutorial_step2_1170x2532.png", f"{BASE_URL}?screen=tutorial&step=1"),
    ("lo6_03_tutorial_step3_1170x2532.png", f"{BASE_URL}?screen=tutorial&step=2"),
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
            time.sleep(4.0) # wait for WebGL engine and canvas render

            out_path = os.path.join(OUT_DIR, filename)
            page.screenshot(path=out_path)
            print(f"Saved: {out_path} ({os.path.getsize(out_path)} bytes)")

            # Also copy to artifact dir for viewing
            artifact_path = os.path.join(ARTIFACT_DIR, filename)
            with open(out_path, "rb") as fsrc, open(artifact_path, "wb") as fdst:
                fdst.write(fsrc.read())

            # Verify with PIL
            img = Image.open(out_path)
            print(f"Verified {filename}: format={img.format}, size={img.size}, mode={img.mode}")

        browser.close()

if __name__ == "__main__":
    capture_all()
