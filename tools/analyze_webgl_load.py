"""Read-only one-run browser timing audit for the two round-37 WebGL builds.

Run: python tools/analyze_webgl_load.py
Writes Evidence/round39/C11_resources.json; does not open Unity.
"""

import json
from pathlib import Path
from playwright.sync_api import sync_playwright
from measure_home_shown import DEFAULTS, ROOT, serve


def main():
    results = []
    with sync_playwright() as playwright:
        browser = playwright.chromium.launch(channel="msedge", headless=True)
        try:
            for label, directory in zip(("dev", "release"), DEFAULTS):
                server = serve(directory)
                context = browser.new_context(viewport={"width": 414, "height": 896})
                try:
                    page = context.new_page()
                    cdp = context.new_cdp_session(page)
                    cdp.send("Network.enable")
                    cdp.send("Network.emulateNetworkConditions", {
                        "offline": False, "latency": 40,
                        "downloadThroughput": 20 * 1024 * 1024 / 8,
                        "uploadThroughput": 5 * 1024 * 1024 / 8,
                    })
                    page.goto(f"http://127.0.0.1:{server.server_port}/", wait_until="domcontentloaded", timeout=90000)
                    page.wait_for_function("typeof window.__hitmeHomeShown !== 'undefined'", timeout=90000)
                    browser_data = page.evaluate("""() => ({
                        homeMs: window.__hitmeHomeShown,
                        resources: performance.getEntriesByType('resource').map(r => ({
                            name: r.name.split('/').pop().split('?')[0],
                            durationMs: Math.round(r.duration), responseEndMs: Math.round(r.responseEnd),
                            transferBytes: r.transferSize, decodedBytes: r.decodedBodySize
                        }))
                    })""")
                    files = {file.name: file.stat().st_size for file in (directory / "Build").iterdir() if file.is_file()}
                    core = [r for r in browser_data["resources"] if r["name"] in files]
                    last_transfer = max((r["responseEndMs"] for r in core), default=0)
                    results.append({"build": label, "commit": "8c6b328", "home_ms": browser_data["homeMs"],
                                    "last_core_response_ms": last_transfer,
                                    "post_transfer_to_home_ms": round(browser_data["homeMs"] - last_transfer),
                                    "files_bytes": files, "core_resources": core})
                    print(label, round(browser_data["homeMs"]), last_transfer, flush=True)
                finally:
                    context.close()
                    server.shutdown()
                    server.server_close()
        finally:
            browser.close()
    output = ROOT / "Evidence" / "round39" / "C11_resources.json"
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(results, indent=2), encoding="utf-8")
    print("Passed! 2/2 resource traces", flush=True)


if __name__ == "__main__":
    main()
