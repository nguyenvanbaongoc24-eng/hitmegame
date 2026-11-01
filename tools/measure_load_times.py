"""Measure first-page load time for each WebGL build port (8093-8099)."""
import time, json
from playwright.sync_api import sync_playwright

ports = [8093, 8094, 8095, 8096, 8097, 8098, 8099]
results = {}

with sync_playwright() as p:
    browser = p.chromium.launch(channel="msedge", headless=True)
    for port in ports:
        page = browser.new_page(viewport={"width": 414, "height": 896})
        url = f"http://127.0.0.1:{port}/"
        t0 = time.time()
        try:
            page.goto(url, timeout=60000, wait_until="networkidle")
            t_networkidle = (time.time() - t0) * 1000

            nav_js = r'JSON.parse(JSON.stringify(performance.getEntriesByType("navigation")[0]))'
            nav = page.evaluate(nav_js)

            res_js = r'JSON.parse(JSON.stringify(performance.getEntriesByType("resource")))'
            resources = page.evaluate(res_js)

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

            results[port] = {
                "domContentLoaded_ms": round(nav.get("domContentLoadedEventEnd", 0), 1),
                "loadEventEnd_ms": round(nav.get("loadEventEnd", 0), 1),
                "networkidle_ms": round(t_networkidle, 1),
                "resources": res_summary,
            }
            print(
                f"Port {port}: DCL={results[port]['domContentLoaded_ms']}ms, "
                f"Load={results[port]['loadEventEnd_ms']}ms, "
                f"Idle={results[port]['networkidle_ms']}ms"
            )
        except Exception as e:
            print(f"Port {port} error: {e}")
            results[port] = {"error": str(e)}
        finally:
            page.close()
    browser.close()

with open(r"D:\HITME GAME\unity-client\Evidence\load_time_measurements.json", "w") as f:
    json.dump(results, f, indent=2)
print("Saved to Evidence/load_time_measurements.json")
