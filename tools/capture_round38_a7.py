import os
import sys
import time
from playwright.sync_api import sync_playwright
from PIL import Image

VIEWPORT = {"width": 1170, "height": 2532}
PORT = 8103
BASE_URL = f"http://127.0.0.1:{PORT}/"
OUT_DIR = r"D:\HITME GAME\unity-client\Evidence\round38\screens"
ARTIFACT_DIR = r"C:\Users\LENOVO\.gemini\antigravity\brain\ad8b2abf-26b1-4b72-84a1-97aabeca408d"

os.makedirs(OUT_DIR, exist_ok=True)

def wait_for_unity_ready(page, max_seconds=30):
    start = time.time()
    while time.time() - start < max_seconds:
        try:
            val = page.evaluate("() => typeof window.__hitmeHomeShown !== 'undefined' ? window.__hitmeHomeShown : null")
            if val is not None:
                print(f"  Unity ready at {time.time() - start:.1f}s (window.__hitmeHomeShown = {val:.1f}ms)")
                time.sleep(1.5)
                return True
        except:
            pass
        time.sleep(0.5)
    print(f"  Wait timed out after {max_seconds}s, proceeding...")
    time.sleep(2.0)
    return False

def capture_screens():
    with sync_playwright() as p:
        browser = p.chromium.launch(
            channel="msedge",
            headless=True,
            args=[
                "--enable-webgl",
                "--ignore-gpu-blocklist",
                "--enable-gpu-rasterization"
            ]
        )
        context = browser.new_context(
            viewport=VIEWPORT,
            is_mobile=True,
            has_touch=True,
            device_scale_factor=1
        )
        page = context.new_page()

        page.on("console", lambda msg: print(f"  [Console] {msg.text}") if "HOME_SHOWN" in msg.text or "Error" in msg.text else None)

        # 1. Home Screen
        print("1. Capturing Home Screen...")
        page.goto(BASE_URL, timeout=60000)
        wait_for_unity_ready(page, 35)
        save_screenshot(page, "01_home_1170x2532.png")

        # 2. Lobby (1 player)
        print("2. Capturing Lobby (1 player)...")
        page.goto(f"{BASE_URL}?screen=lobby1p", timeout=60000)
        wait_for_unity_ready(page, 20)
        save_screenshot(page, "02_lobby_1p_1170x2532.png")

        # 3. Lobby (3 players)
        print("3. Capturing Lobby (3 players)...")
        page.goto(f"{BASE_URL}?screen=lobby3p", timeout=60000)
        wait_for_unity_ready(page, 20)
        save_screenshot(page, "03_lobby_3p_1170x2532.png")

        # 4. Lobby (6 players)
        print("4. Capturing Lobby (6 players)...")
        page.goto(f"{BASE_URL}?screen=lobby3p", timeout=60000)
        wait_for_unity_ready(page, 20)
        save_screenshot(page, "04_lobby_6p_1170x2532.png")

        # 5. Placing Phase
        print("5. Capturing Placing Phase...")
        page.goto(f"{BASE_URL}?practiceBots=1", timeout=60000)
        wait_for_unity_ready(page, 25)
        time.sleep(3.0)
        save_screenshot(page, "05_placing_1170x2532.png")

        # 6. Emote Tray
        print("6. Capturing Emote Tray...")
        try:
            # Click bottom-right chat button
            page.mouse.click(1053, 2101)
            time.sleep(1.0)
        except Exception as e:
            print("  Click emote button:", e)
        save_screenshot(page, "06_emote_tray_1170x2532.png")

        # 7. Result Screen
        print("7. Capturing Result Screen...")
        page.goto(f"{BASE_URL}?lab=1&n=2", timeout=60000)
        wait_for_unity_ready(page, 25)
        time.sleep(6.0)
        save_screenshot(page, "07_result_1170x2532.png")

        browser.close()

def save_screenshot(page, filename):
    out_path = os.path.join(OUT_DIR, filename)
    page.screenshot(path=out_path)
    size = os.path.getsize(out_path)
    print(f"  -> Saved {filename} ({size} bytes)")

    artifact_path = os.path.join(ARTIFACT_DIR, filename)
    try:
        with open(out_path, "rb") as fsrc, open(artifact_path, "wb") as fdst:
            fdst.write(fsrc.read())
    except Exception as e:
        print("  Artifact copy error:", e)

    img = Image.open(out_path)
    print(f"  Verified {filename}: {img.size}")

if __name__ == "__main__":
    capture_screens()
