"""Measure WebGL Release load time on simulated 20 Mbps and 5 Mbps network."""
import time, json, sys
sys.stdout.reconfigure(encoding="utf-8")
from playwright.sync_api import sync_playwright

results = {}

with sync_playwright() as p:
    browser = p.chromium.launch(channel="msedge", headless=True)

    # 1. Measure at 20 Mbps (Standard 4G)
    print("Testing 20 Mbps simulated network on port 8101...")
    context_20 = browser.new_context(viewport={"width": 414, "height": 896})
    page_20 = context_20.new_page()

    # CDP emulation for 20 Mbps
    client_20 = context_20.new_cdp_session(page_20)
    # 20 Mbps = 2,500,000 bytes/s, 40ms latency
    client_20.send("Network.emulateNetworkConditions", {
        "offline": False,
        "latency": 40,
        "downloadThroughput": int(20 * 1024 * 1024 / 8),
        "uploadThroughput": int(5 * 1024 * 1024 / 8)
    })

    t0 = time.time()
    page_20.goto("http://127.0.0.1:8101/", wait_until="networkidle", timeout=60000)
    t_20_idle = (time.time() - t0) * 1000

    nav_20 = page_20.evaluate('() => JSON.parse(JSON.stringify(performance.getEntriesByType("navigation")[0]))')
    results["20_mbps"] = {
        "bandwidth": "20 Mbps (4G tiêu chuẩn)",
        "downloadThroughput_bytes_sec": int(20 * 1024 * 1024 / 8),
        "latency_ms": 40,
        "domContentLoaded_ms": round(nav_20.get("domContentLoadedEventEnd", 0), 1),
        "loadEventEnd_ms": round(nav_20.get("loadEventEnd", 0), 1),
        "total_network_idle_seconds": round(t_20_idle / 1000.0, 2),
        "target_met": t_20_idle / 1000.0 <= 10.0
    }
    print(f"20 Mbps Result: {results['20_mbps']['total_network_idle_seconds']}s (Mục tiêu <= 10s: {results['20_mbps']['target_met']})")

    # Take screenshot of loaded release build
    time.sleep(2)
    page_20.screenshot(path=r"D:\HITME GAME\unity-client\Evidence\round35\lo10_01_release_home_20mbps.png")
    page_20.close()
    context_20.close()

    # 2. Measure at 5 Mbps (Weak 3G / Slow WiFi)
    print("Testing 5 Mbps simulated network on port 8101...")
    context_5 = browser.new_context(viewport={"width": 414, "height": 896})
    page_5 = context_5.new_page()

    client_5 = context_5.new_cdp_session(page_5)
    # 5 Mbps = 625,000 bytes/s, 100ms latency
    client_5.send("Network.emulateNetworkConditions", {
        "offline": False,
        "latency": 100,
        "downloadThroughput": int(5 * 1024 * 1024 / 8),
        "uploadThroughput": int(1 * 1024 * 1024 / 8)
    })

    t0 = time.time()
    page_5.goto("http://127.0.0.1:8101/", wait_until="networkidle", timeout=90000)
    t_5_idle = (time.time() - t0) * 1000

    nav_5 = page_5.evaluate('() => JSON.parse(JSON.stringify(performance.getEntriesByType("navigation")[0]))')
    results["5_mbps"] = {
        "bandwidth": "5 Mbps (3G / Wi-Fi yếu)",
        "downloadThroughput_bytes_sec": int(5 * 1024 * 1024 / 8),
        "latency_ms": 100,
        "domContentLoaded_ms": round(nav_5.get("domContentLoadedEventEnd", 0), 1),
        "loadEventEnd_ms": round(nav_5.get("loadEventEnd", 0), 1),
        "total_network_idle_seconds": round(t_5_idle / 1000.0, 2)
    }
    print(f"5 Mbps Result: {results['5_mbps']['total_network_idle_seconds']}s")

    time.sleep(2)
    page_5.screenshot(path=r"D:\HITME GAME\unity-client\Evidence\round35\lo10_02_release_home_5mbps.png")
    page_5.close()
    context_5.close()

    browser.close()

out_file = r"D:\HITME GAME\unity-client\Evidence\round35\lo10_load_simulation.json"
with open(out_file, "w", encoding="utf-8") as f:
    json.dump(results, f, indent=2, ensure_ascii=False)
print("Saved simulation results to:", out_file)
