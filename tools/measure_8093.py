"""Measure port 8093 load time."""
import time, json
from playwright.sync_api import sync_playwright

with sync_playwright() as p:
    browser = p.chromium.launch(channel="msedge", headless=True)
    page = browser.new_page(viewport={"width": 414, "height": 896})
    url = "http://127.0.0.1:8093/"
    t0 = time.time()
    page.goto(url, timeout=60000, wait_until="networkidle")
    t_networkidle = (time.time() - t0) * 1000
    nav = page.evaluate(r'JSON.parse(JSON.stringify(performance.getEntriesByType("navigation")[0]))')
    resources = page.evaluate(r'JSON.parse(JSON.stringify(performance.getEntriesByType("resource")))')

    res_summary = {}
    for r in resources:
        name = r.get("name", "").split("/")[-1]
        for ext in [".loader.js", ".framework.js", ".wasm", ".data"]:
            if ext in name:
                res_summary[ext] = {
                    "name": name,
                    "duration_ms": round(r.get("duration", 0), 1),
                    "transferSize": r.get("transferSize", 0),
                }

    result = {
        "domContentLoaded_ms": round(nav.get("domContentLoadedEventEnd", 0), 1),
        "loadEventEnd_ms": round(nav.get("loadEventEnd", 0), 1),
        "networkidle_ms": round(t_networkidle, 1),
        "resources": res_summary,
    }
    print(f"Port 8093: DCL={result['domContentLoaded_ms']}ms, Load={result['loadEventEnd_ms']}ms, Idle={result['networkidle_ms']}ms")

    # Merge into existing file
    try:
        with open(r"D:\HITME GAME\unity-client\Evidence\load_time_measurements.json", "r") as f:
            all_results = json.load(f)
    except FileNotFoundError:
        all_results = {}
    all_results["8093"] = result
    with open(r"D:\HITME GAME\unity-client\Evidence\load_time_measurements.json", "w") as f:
        json.dump(all_results, f, indent=2)
    print("Updated Evidence/load_time_measurements.json with port 8093")

    page.close()
    browser.close()
