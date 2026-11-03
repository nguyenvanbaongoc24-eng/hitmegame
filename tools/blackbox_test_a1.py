import os
import sys
import time
from pathlib import Path
from playwright.sync_api import sync_playwright

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8")

OUT_DIR = Path("D:/HITME GAME/unity-client/Evidence/blackbox")
OUT_DIR.mkdir(parents=True, exist_ok=True)

def verify_a1():
    with sync_playwright() as p:
        browser = p.webkit.launch(headless=True)
        # iPhone 15 Pro viewport 393x852 with DPR 3 -> 1179 x 2556 render
        context = browser.new_context(
            viewport={"width": 393, "height": 852},
            device_scale_factor=3,
            has_touch=True,
            is_mobile=True,
            user_agent="Mozilla/5.0 (iPhone; CPU iPhone OS 17_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4 Mobile/15E148 Safari/604.1"
        )
        page = context.new_page()

        print("[*] Navigating to http://127.0.0.1:8104/?e2e=1 ...")
        page.goto("http://127.0.0.1:8104/?e2e=1", wait_until="commit")

        print("[*] Waiting for #unity-canvas ...")
        page.wait_for_selector("#unity-canvas", timeout=30000)

        loaded = False
        for _ in range(60):
            state = page.evaluate("() => window.__hitmeState")
            if state and isinstance(state, dict) and state.get("build"):
                loaded = True
                print(f"[+] Loaded successfully! Build: {state.get('build')}, Screen: {state.get('screen')}")
                break
            time.sleep(0.5)

        if not loaded:
            print("[!] Timed out waiting for __hitmeState")
            browser.close()
            return False

        # 1. Capture Home
        time.sleep(1.0)
        home_path = OUT_DIR / "A1_Home_1179x2556.png"
        page.screenshot(path=str(home_path))
        print(f"[+] Saved Home screenshot: {home_path}")

        # 2. Tap Bot Training at (196.6, 750.1)
        print("[*] Tapping Bot Training at (196.6, 750.1)...")
        page.touchscreen.tap(196.6, 750.1)
        time.sleep(1.5)

        select_path = OUT_DIR / "A1_Select_1179x2556.png"
        page.screenshot(path=str(select_path))
        print(f"[+] Saved Select screenshot: {select_path}")

        state = page.evaluate("() => window.__hitmeState")
        print(f"[*] State Screen after tap: {state.get('screen')}")

        # 3. Tap Start Match ("BẮT ĐẦU" button at 196.5, 716.8)
        print("[*] Tapping Start Match at (196.5, 716.8)...")
        page.touchscreen.tap(196.5, 716.8)

        print("[*] Waiting for match placing phase...")
        time.sleep(3.0)

        # 4. In-game Placement Phase screenshot
        match_placing_path = OUT_DIR / "A1_Match_Placing_1179x2556.png"
        page.screenshot(path=str(match_placing_path))
        print(f"[+] Saved Match Placing screenshot: {match_placing_path}")

        state = page.evaluate("() => window.__hitmeState")
        match_info = state.get("match", {})
        print(f"[*] Match Round: {match_info.get('round')}, Phase: {match_info.get('phase')}, Time: {match_info.get('timeLeft')}")
        print(f"[*] PlayableRect: {match_info.get('playableRect')}")
        players = match_info.get("players", [])
        print(f"[*] Match Players ({len(players)}):")
        for pl in players:
            print(f"    - {pl.get('name')} (char: {pl.get('characterId')}, hp: {pl.get('hp')}, rect: {pl.get('rect')})")

        # 5. Tap SẴN SÀNG button
        # Let's find ready button from UiDump
        uidump = page.evaluate("() => window.__hitmeUiDump ? window.__hitmeUiDump() : []")
        ready_btn = next((item for item in uidump if "ready" in item.get("name", "").lower() or "sẵn sàng" in item.get("text", "").lower()), None)
        if ready_btn and ready_btn.get("rect"):
            r = ready_btn["rect"]
            sx = 393.0 / 983.0
            sy = 852.0 / 2130.0
            cx = (r.get("x", 0) + r.get("w", 0) / 2) * sx
            cy = (r.get("y", 0) + r.get("h", 0) / 2) * sy
            print(f"[*] Tapping Ready at ({cx:.1f}, {cy:.1f})...")
            page.touchscreen.tap(cx, cy)
        else:
            print("[*] Ready button fallback tap (cx=300, cy=65)...")
            page.touchscreen.tap(300, 65)

        # Monitor round resolution, throw, hit, KO events
        print("[*] Monitoring round resolution and events...")
        for sec in range(12):
            time.sleep(1.0)
            state = page.evaluate("() => window.__hitmeState")
            m = state.get("match", {})
            events = m.get("events", [])
            phase = m.get("phase")
            if events:
                print(f"    [t={sec}s] Phase: {phase}, Events recorded: {len(events)} -> {[e.get('type') for e in events]}")
            if phase in ["resolve", "roundEnd"]:
                res_path = OUT_DIR / f"A1_Match_Resolve_{sec}s_1179x2556.png"
                page.screenshot(path=str(res_path))
                print(f"[+] Captured resolve screenshot: {res_path}")
                break

        final_path = OUT_DIR / "A1_Match_Action_1179x2556.png"
        page.screenshot(path=str(final_path))
        print(f"[+] Saved Match Action screenshot: {final_path}")

        browser.close()
        return True

if __name__ == "__main__":
    verify_a1()
