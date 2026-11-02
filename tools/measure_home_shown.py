"""
Measure WebGL Release load time until [HITME] HOME_SHOWN / window.__hitmeHomeShown
on simulated 20 Mbps and 5 Mbps network, plus cached reload.
"""
import sys
import os
import json
import time

sys.stdout.reconfigure(encoding="utf-8")
from playwright.sync_api import sync_playwright

OUT_DIR = r"D:\HITME GAME\unity-client\Evidence\round38"
os.makedirs(OUT_DIR, exist_ok=True)
TARGET_URL = "http://127.0.0.1:8103/"

results = {
    "target_url": TARGET_URL,
    "build_profile": "Release Brotli (Port 8103)",
    "timestamp": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
    "measurement_marker": "window.__hitmeHomeShown / console.log('[HITME] HOME_SHOWN')",
    "scenarios": {}
}

def measure_scenario(browser, name, bandwidth_mbps, latency_ms, is_cached=False):
    print(f"\n--- Measuring: {name} (Bandwidth: {bandwidth_mbps} Mbps, Latency: {latency_ms}ms, Cached: {is_cached}) ---")
    context = browser.new_context(viewport={"width": 414, "height": 896})
    page = context.new_page()

    home_shown_ts = {"val": None}
    console_logs = []

    def handle_console(msg):
        text = msg.text
        console_logs.append(text)
        if "[HITME] HOME_SHOWN" in text:
            print(f"  [Console Event] {text}")
            home_shown_ts["val"] = time.time()

    page.on("console", handle_console)

    cdp = context.new_cdp_session(page)
    cdp.send("Network.setCacheDisabled", {"cacheDisabled": not is_cached})

    if bandwidth_mbps > 0:
        download_bps = int(bandwidth_mbps * 1024 * 1024 / 8)
        upload_bps = int(max(1, bandwidth_mbps / 4) * 1024 * 1024 / 8)
        cdp.send("Network.emulateNetworkConditions", {
            "offline": False,
            "latency": latency_ms,
            "downloadThroughput": download_bps,
            "uploadThroughput": upload_bps
        })

    if not is_cached:
        # Clear IndexedDB and cache before cold run
        cdp.send("Storage.clearDataForOrigin", {
            "origin": "http://127.0.0.1:8103",
            "storageTypes": "all"
        })

    t_start = time.time()
    page.goto(TARGET_URL, timeout=90000)

    # Wait until window.__hitmeHomeShown is populated or home_shown_ts or canvas is ready
    max_wait = 90
    waited = 0
    poll_interval = 0.5
    while waited < max_wait:
        res = page.evaluate("() => typeof window.__hitmeHomeShown !== 'undefined' ? window.__hitmeHomeShown : null")
        if res is not None:
            print(f"  Detected window.__hitmeHomeShown = {res:.1f}ms")
            break
        if home_shown_ts["val"] is not None:
            break
        time.sleep(poll_interval)
        waited += poll_interval

    t_home_shown = time.time() - t_start

    # Extract resource timings
    resources = page.evaluate("""() => {
        return performance.getEntriesByType('resource').map(r => ({
            name: r.name.split('/').pop(),
            transferSize: r.transferSize || 0,
            encodedBodySize: r.encodedBodySize || 0,
            decodedBodySize: r.decodedBodySize || 0,
            duration_ms: Math.round(r.duration)
        }));
    }""")

    # Filter to unity core files
    core_files = [r for r in resources if any(ext in r["name"] for ext in [".data", ".wasm", ".framework", ".loader", "index.html"])]
    total_transfer_bytes = sum(r["transferSize"] for r in core_files)

    scenario_res = {
        "bandwidth_mbps": bandwidth_mbps,
        "latency_ms": latency_ms,
        "is_cached": is_cached,
        "time_to_home_shown_seconds": round(t_home_shown, 2),
        "total_bundle_bytes": total_transfer_bytes,
        "core_files": core_files
    }

    # Take screenshot of the loaded Home screen
    screenshot_name = f"home_shown_{name.lower().replace(' ', '_')}.png"
    screenshot_path = os.path.join(OUT_DIR, screenshot_name)
    page.screenshot(path=screenshot_path)
    scenario_res["screenshot"] = screenshot_path

    print(f"  Total Bundle: {total_transfer_bytes / (1024*1024):.2f} MB")
    print(f"  Time to HOME_SHOWN: {t_home_shown:.2f} s")
    print(f"  Core files transferred:")
    for f in core_files:
        print(f"    - {f['name']}: {f['transferSize']} bytes ({f['duration_ms']} ms)")

    context.close()
    return scenario_res

with sync_playwright() as p:
    browser = p.chromium.launch(channel="msedge", headless=True)

    # 1. 20 Mbps (4G) Cold
    results["scenarios"]["20mbps_cold"] = measure_scenario(browser, "20Mbps_Cold", bandwidth_mbps=20, latency_ms=40, is_cached=False)

    # 2. 5 Mbps (3G / Weak Wi-Fi) Cold
    results["scenarios"]["5mbps_cold"] = measure_scenario(browser, "5Mbps_Cold", bandwidth_mbps=5, latency_ms=100, is_cached=False)

    # 3. Cached reload (Warm cache)
    results["scenarios"]["cached_reload"] = measure_scenario(browser, "Cached_Reload", bandwidth_mbps=20, latency_ms=40, is_cached=True)

    browser.close()

out_json = os.path.join(OUT_DIR, "load_measurement_home_shown.json")
with open(out_json, "w", encoding="utf-8") as f:
    json.dump(results, f, indent=2, ensure_ascii=False)

print(f"\nAll measurements saved to: {out_json}")
