#!/usr/bin/env python3
import time
from pathlib import Path
from playwright.sync_api import sync_playwright

OUT_DIR = Path("D:/HITME GAME/unity-client/Evidence/round45")
OUT_DIR.mkdir(parents=True, exist_ok=True)

def test_vercel():
    with sync_playwright() as p:
        browser = p.chromium.launch(channel="chrome", headless=True)
        page = browser.new_page(viewport={"width": 393, "height": 852})

        logs = []
        page.on("console", lambda msg: logs.append(f"[{msg.type}] {msg.text}"))

        print("Navigating to https://hitmegame-iota.vercel.app ...")
        t0 = time.time()
        page.goto("https://hitmegame-iota.vercel.app", wait_until="domcontentloaded")

        try:
            page.wait_for_selector("#unity-loading", state="detached", timeout=45000)
            load_time = time.time() - t0
            print(f"Unity WebGL loaded successfully on Vercel in {load_time:.2f}s!")
        except Exception as e:
            print(f"Timeout waiting for unity-loading: {e}")

        time.sleep(2)
        ss_path = OUT_DIR / "vercel_live_home.png"
        page.screenshot(path=str(ss_path))
        print(f"Saved screenshot: {ss_path}")

        # Check fingerprint
        info = page.evaluate("""() => {
            const meta = document.querySelector('meta[name="build-fingerprint"]');
            return {
                title: document.title,
                metaFingerprint: meta ? meta.getAttribute('content') : null,
                innerW: window.innerWidth,
                innerH: window.innerHeight
            };
        }""")
        print("DOM Info:", info)

        print("\nRelevant Console logs:")
        for l in logs:
            if "build" in l.lower() or "unity" in l.lower() or "error" in l.lower():
                print(" ", l)

        browser.close()

if __name__ == "__main__":
    test_vercel()
