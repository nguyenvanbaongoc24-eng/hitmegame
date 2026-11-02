#!/usr/bin/env python3
"""
test_multi_device_matrix.py — Comprehensive Multi-Device Matrix Test
Tests HIT ME across WebKit and Chromium on 4 device form factors in both Portrait and Landscape:
- iPhone SE (375x667 / 667x375) [WebKit]
- iPhone 15 Pro (393x852 / 852x393) [WebKit]
- Pixel 7 (412x915 / 915x412) [Chromium]
- iPad (810x1080 / 1080x810) [WebKit]

Captures for each:
- Home
- Settings
- Placing
- Battle
"""

import os
import sys
import time
import json
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8")

from playwright.sync_api import sync_playwright

ROOT = Path("D:/HITME GAME")
UNITY_CLIENT = ROOT / "unity-client"
MATRIX_DIR = UNITY_CLIENT / "Evidence" / "round45" / "matrix"
MATRIX_DIR.mkdir(parents=True, exist_ok=True)

TARGET_DEVICES = [
    {
        "id": "iphone_se",
        "name": "iPhone SE",
        "engine": "webkit",
        "portrait": (375, 667),
        "landscape": (667, 375),
        "dpr": 2.0,
        "is_mobile": True,
    },
    {
        "id": "iphone_15_pro",
        "name": "iPhone 15 Pro",
        "engine": "webkit",
        "portrait": (393, 852),
        "landscape": (852, 393),
        "dpr": 3.0,
        "is_mobile": True,
    },
    {
        "id": "pixel_7",
        "name": "Pixel 7 (Android)",
        "engine": "chromium",
        "portrait": (412, 915),
        "landscape": (915, 412),
        "dpr": 2.625,
        "is_mobile": True,
    },
    {
        "id": "ipad",
        "name": "iPad (Tablet)",
        "engine": "webkit",
        "portrait": (810, 1080),
        "landscape": (1080, 810),
        "dpr": 2.0,
        "is_mobile": True,
    },
]

def run_device_orientation(p, dev, orientation, base_url):
    w, h = dev["portrait"] if orientation == "portrait" else dev["landscape"]
    engine_name = dev["engine"]
    dev_id = dev["id"]
    test_key = f"{dev_id}_{orientation}"
    print(f"\n=======================================================")
    print(f"RUNNING: {dev['name']} [{orientation.upper()}] ({w}x{h}) via {engine_name.upper()}")
    print(f"=======================================================")

    browser = None
    if engine_name == "webkit":
        browser = p.webkit.launch(headless=True)
    else:
        browser = p.chromium.launch(channel="chrome", headless=True)

    context = browser.new_context(
        viewport={"width": w, "height": h},
        device_scale_factor=dev["dpr"],
        is_mobile=dev["is_mobile"],
        has_touch=True,
        ignore_https_errors=True
    )
    page = context.new_page()

    results = {"device": dev["name"], "orientation": orientation, "viewport": f"{w}x{h}", "engine": engine_name}

    try:
        url = f"{base_url}/index.html?e2e=1"
        page.goto(url, wait_until="domcontentloaded")
        page.wait_for_selector("#unity-loading", state="detached", timeout=45000)
        time.sleep(2.0)

        # 1. SCREEN: HOME
        home_path = MATRIX_DIR / f"{test_key}_01_home.png"
        page.screenshot(path=str(home_path))
        print(f"  [+] Captured Home: {home_path.name}")
        results["home"] = home_path.name

        # 2. SCREEN: SETTINGS
        # In portrait: settings is top right (90% w, 6% h). In landscape: 95% w, 8% h.
        settings_x = int(w * (0.90 if orientation == "portrait" else 0.94))
        settings_y = int(h * (0.06 if orientation == "portrait" else 0.08))
        page.touchscreen.tap(settings_x, settings_y)
        time.sleep(1.0)
        settings_path = MATRIX_DIR / f"{test_key}_02_settings.png"
        page.screenshot(path=str(settings_path))
        print(f"  [+] Captured Settings: {settings_path.name}")
        results["settings"] = settings_path.name

        # Close settings: tap DONG button
        close_x = int(w * 0.50)
        close_y = int(h * (0.83 if orientation == "portrait" else 0.88))
        page.touchscreen.tap(close_x, close_y)
        time.sleep(0.8)

        # 3. SCREEN: PLACING
        # Tap LUYEN VOI BOT
        luyen_x = int(w * 0.50)
        luyen_y = int(h * (0.64 if orientation == "portrait" else 0.68))
        page.touchscreen.tap(luyen_x, luyen_y)
        time.sleep(1.0)

        # Tap BAT DAU
        start_x = int(w * 0.50)
        start_y = int(h * (0.85 if orientation == "portrait" else 0.88))
        page.touchscreen.tap(start_x, start_y)
        time.sleep(2.0)

        # Dismiss Tutorial if present: tap BO QUA
        boqua_x = int(w * (0.30 if orientation == "portrait" else 0.38))
        boqua_y = int(h * (0.61 if orientation == "portrait" else 0.65))
        page.touchscreen.tap(boqua_x, boqua_y)
        time.sleep(1.0)

        placing_path = MATRIX_DIR / f"{test_key}_03_placing.png"
        page.screenshot(path=str(placing_path))
        print(f"  [+] Captured Placing: {placing_path.name}")
        results["placing"] = placing_path.name

        # 4. SCREEN: BATTLE
        # Tap arena placement
        tap_arena_x = int(w * 0.50)
        tap_arena_y = int(h * (0.75 if orientation == "portrait" else 0.70))
        page.touchscreen.tap(tap_arena_x, tap_arena_y)
        time.sleep(0.5)

        # Tap SAN SANG (top right)
        ready_x = int(w * (0.80 if orientation == "portrait" else 0.75))
        ready_y = int(h * (0.07 if orientation == "portrait" else 0.09))
        page.touchscreen.tap(ready_x, ready_y)
        time.sleep(2.5)

        battle_path = MATRIX_DIR / f"{test_key}_04_battle.png"
        page.screenshot(path=str(battle_path))
        print(f"  [+] Captured Battle: {battle_path.name}")
        results["battle"] = battle_path.name

        results["status"] = "PASS"

    except Exception as ex:
        print(f"  [-] Error on {test_key}: {ex}")
        results["status"] = "FAIL"
        results["error"] = str(ex)
    finally:
        context.close()
        browser.close()

    return results

def main():
    base_url = "http://127.0.0.1:8103"
    print(f"Starting Multi-Device Matrix Test against: {base_url}")
    all_results = []

    with sync_playwright() as p:
        for dev in TARGET_DEVICES:
            for orient in ["portrait", "landscape"]:
                res = run_device_orientation(p, dev, orient, base_url)
                all_results.append(res)

    out_json = MATRIX_DIR / "matrix_results.json"
    with open(out_json, "w", encoding="utf-8") as f:
        json.dump(all_results, f, indent=2)
    print(f"\n[SUCCESS] Matrix execution complete. Saved results to {out_json}")

if __name__ == "__main__":
    main()
