"""Run: python tools/measure_home_shown.py [dev-build-dir] [release-build-dir].

Read-only WebGL load comparison: three isolated Chromium runs per build on a
simulated 20 Mbps / 40 ms link. Writes JSON under Evidence/round38 only.
Requires playwright and a locally installed Microsoft Edge.
"""

import json
import mimetypes
import statistics
import sys
import threading
import time
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from playwright.sync_api import sync_playwright

ROOT = Path(__file__).resolve().parents[1]
BUILDS = Path(r"D:\HITME GAME\unity-client\Builds")
DEFAULTS = [BUILDS / "WebGL_20261102_8c6b328", BUILDS / "WebGL_Release_20261102_8c6b328"]
OUTPUT = ROOT / "Evidence" / "round38" / "C7_load.json"


def serve(directory):
    class Handler(SimpleHTTPRequestHandler):
        def __init__(self, *args, **kwargs):
            super().__init__(*args, directory=str(directory), **kwargs)

        def guess_type(self, path):
            return mimetypes.guess_type(path.removesuffix(".br"))[0] or "application/octet-stream"

        def end_headers(self):
            if self.path.split("?", 1)[0].endswith(".br"):
                self.send_header("Content-Encoding", "br")
            super().end_headers()

        def log_message(self, format, *args):
            pass

    server = ThreadingHTTPServer(("127.0.0.1", 0), Handler)
    thread = threading.Thread(target=server.serve_forever, daemon=True)
    thread.start()
    return server


def main():
    paths = [Path(item) for item in sys.argv[1:]] or DEFAULTS
    if len(paths) != 2 or any(not (item / "index.html").is_file() for item in paths):
        raise SystemExit("Provide existing Dev and Release build directories containing index.html")
    measurements = []
    with sync_playwright() as playwright:
        browser = playwright.chromium.launch(channel="msedge", headless=True)
        try:
            for label, path in zip(("dev", "release"), paths):
                server = serve(path)
                url = f"http://127.0.0.1:{server.server_port}/"
                try:
                    for run in range(1, 4):
                        context = browser.new_context(viewport={"width": 414, "height": 896})
                        page = context.new_page()
                        cdp = context.new_cdp_session(page)
                        cdp.send("Network.enable")
                        cdp.send("Network.emulateNetworkConditions", {
                            "offline": False, "latency": 40,
                            "downloadThroughput": 20 * 1024 * 1024 / 8,
                            "uploadThroughput": 5 * 1024 * 1024 / 8,
                        })
                        started = time.monotonic()
                        error = None
                        try:
                            page.goto(url, wait_until="domcontentloaded", timeout=90000)
                            page.wait_for_function("typeof window.__hitmeHomeShown !== 'undefined'", timeout=90000)
                            elapsed = round(time.monotonic() - started, 3)
                        except Exception as exc:
                            elapsed = None
                            error = str(exc).splitlines()[0]
                        measurements.append({"build": label, "run": run, "seconds": elapsed, "error": error})
                        print(label, run, elapsed, error, flush=True)
                        context.close()
                finally:
                    server.shutdown()
                    server.server_close()
        finally:
            browser.close()
    medians = {}
    for label in ("dev", "release"):
        values = [item["seconds"] for item in measurements if item["build"] == label and item["seconds"] is not None]
        medians[label] = statistics.median(values) if len(values) == 3 else None
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT.write_text(json.dumps({"builds": [str(path) for path in paths],
                                  "network": "20 Mbps download, 40 ms latency",
                                  "runs": measurements, "median_seconds": medians}, indent=2), encoding="utf-8")
    if any(value is None for value in medians.values()):
        raise SystemExit("INCOMPLETE: HOME_SHOWN was not observed in all six runs")
    print("Passed! 6/6 HOME_SHOWN measurements", flush=True)


if __name__ == "__main__":
    main()
