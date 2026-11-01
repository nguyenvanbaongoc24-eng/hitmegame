"""
Q3 Round 36: Robustness / Monkey Test for HIT ME WebGL (Port 8100)
Runs comprehensive monkey testing on port 8100 via Playwright Python.
"""
import sys
import os
import time
import json
import random

sys.stdout.reconfigure(encoding="utf-8")
from playwright.sync_api import sync_playwright

PORT = 8100
URL = f"http://localhost:{PORT}"
DURATION_S = 60  # 1 minute monkey verification test (~450 actions)
LOG_FILE = r"D:\HITME GAME\unity-client\Evidence\round36\robustness_log.json"
os.makedirs(os.path.dirname(LOG_FILE), exist_ok=True)


def main():
    with sync_playwright() as p:
        browser = p.chromium.launch(channel="msedge", headless=True)
        context = browser.new_context(
            viewport={"width": 390, "height": 844},
            device_scale_factor=3
        )
        page = context.new_page()

        errors = []
        warnings = []
        memory_snapshots = []
        actions = []
        action_count = 0
        hang_count = 0
        error_count = 0

        def on_console(msg):
            nonlocal error_count
            if msg.type == "error":
                if "favicon.ico" in msg.text or "404" in msg.text:
                    return
                error_count += 1
                errors.append({"time": time.time(), "text": msg.text[:300]})
            elif msg.type == "warning":
                warnings.append({"time": time.time(), "text": msg.text[:200]})

        def on_page_error(err):
            nonlocal error_count
            error_count += 1
            errors.append({"time": time.time(), "text": f"PAGE_ERROR: {str(err)[:300]}"})

        page.on("console", on_console)
        page.on("pageerror", on_page_error)

        print(f"[Robustness] Navigating to {URL}...")
        page.goto(URL, timeout=120000, wait_until="networkidle")
        print("[Robustness] Page loaded. Waiting 10s for Unity to initialize...")
        page.wait_for_timeout(10000)

        start_time = time.time()

        viewports = [
            {"width": 1080, "height": 1920},
            {"width": 1170, "height": 2532},
            {"width": 1080, "height": 2400},
            {"width": 1620, "height": 2160},
            {"width": 390, "height": 844},
            {"width": 844, "height": 390},
        ]

        iteration = 0
        while time.time() - start_time < DURATION_S:
            iteration += 1

            # Periodic memory check
            if iteration % 30 == 0:
                try:
                    mem = page.evaluate("""() => {
                        if (performance && performance.memory) {
                            return {
                                used: Math.round(performance.memory.usedJSHeapSize / (1024 * 1024)),
                                total: Math.round(performance.memory.totalJSHeapSize / (1024 * 1024))
                            };
                        }
                        return null;
                    }""")
                    if mem:
                        memory_snapshots.append({"time": round(time.time() - start_time, 1), "usedMB": mem["used"]})
                except Exception:
                    pass

            # Periodic resize / orientation
            if iteration % 80 == 0:
                vp = random.choice(viewports)
                page.set_viewport_size(vp)
                action_count += 1
                actions.append("resize")
                page.wait_for_timeout(200)

            # Periodic tab hide / visibilitychange
            if iteration % 150 == 0:
                hide_ms = 10000 if iteration % 300 == 0 else 3000
                try:
                    page.evaluate("""(ms) => {
                        Object.defineProperty(document, 'hidden', { value: true, writable: true, configurable: true });
                        document.dispatchEvent(new Event('visibilitychange'));
                        setTimeout(() => {
                            Object.defineProperty(document, 'hidden', { value: false, writable: true, configurable: true });
                            document.dispatchEvent(new Event('visibilitychange'));
                        }, ms);
                    }""", hide_ms)
                    page.wait_for_timeout(hide_ms + 200)
                    action_count += 1
                    actions.append(f"visibilitychange_{hide_ms}ms")
                except Exception:
                    pass

            # Responsive check
            if iteration % 40 == 0:
                try:
                    t_check = time.time()
                    page.evaluate("() => 1 + 1")
                    if time.time() - t_check > 5.0:
                        hang_count += 1
                except Exception:
                    hang_count += 1

            # Random touch actions
            roll = random.random()
            w = page.viewport_size["width"]
            h = page.viewport_size["height"]
            rx = random.randint(10, max(20, w - 10))
            ry = random.randint(10, max(20, h - 10))

            if roll < 0.50:
                page.mouse.click(rx, ry)
                action_count += 1
            elif roll < 0.70:
                page.mouse.dblclick(rx, ry)
                action_count += 1
            elif roll < 0.85:
                # Swipe gesture
                ex = rx + (50 if random.random() > 0.5 else -50)
                ey = ry + (80 if random.random() > 0.5 else -80)
                page.mouse.move(rx, ry)
                page.mouse.down()
                page.mouse.move(ex, ey, steps=5)
                page.mouse.up()
                action_count += 1
            else:
                # Rapid double tap
                page.mouse.click(rx, ry)
                page.wait_for_timeout(50)
                page.mouse.click(rx, ry)
                action_count += 2

            page.wait_for_timeout(random.randint(40, 120))

            if iteration % 100 == 0:
                elapsed = int(time.time() - start_time)
                print(f"[Robustness] {elapsed}s: {action_count} actions, {error_count} errors, {hang_count} hangs")

        # Screenshot final state
        screenshot_path = r"D:\HITME GAME\unity-client\Evidence\round36\robustness_final_state.png"
        page.screenshot(path=screenshot_path)

        # Monotonicity check
        memory_monotonic = False
        if len(memory_snapshots) >= 4:
            first_q = memory_snapshots[:len(memory_snapshots)//4]
            last_q = memory_snapshots[3*len(memory_snapshots)//4:]
            avg_first = sum(m["usedMB"] for m in first_q) / len(first_q)
            avg_last = sum(m["usedMB"] for m in last_q) / len(last_q)
            memory_monotonic = avg_last > (avg_first * 1.5)

        result = {
            "duration_seconds": int(time.time() - start_time),
            "total_actions": action_count,
            "console_errors": error_count,
            "hangs": hang_count,
            "memory_monotonically_increasing": memory_monotonic,
            "memory_snapshots": memory_snapshots,
            "errors_sample": errors[:20],
            "warnings_count": len(warnings),
            "pass": (error_count == 0 and hang_count == 0 and not memory_monotonic)
        }

        with open(LOG_FILE, "w", encoding="utf-8") as f:
            json.dump(result, f, indent=2, ensure_ascii=False)

        print("\n=== ROBUSTNESS TEST RESULT ===")
        print(f"Duration: {result['duration_seconds']}s")
        print(f"Actions: {result['total_actions']}")
        print(f"Errors: {result['console_errors']}")
        print(f"Hangs: {result['hangs']}")
        print(f"Memory monotonic: {result['memory_monotonically_increasing']}")
        print(f"PASS: {result['pass']}")
        print(f"Saved: {LOG_FILE}")

        browser.close()


if __name__ == "__main__":
    main()
