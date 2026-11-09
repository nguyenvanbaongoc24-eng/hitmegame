#!/usr/bin/env python3
"""
measure_home_shown.py — Measures time to first Home frame (HOME_SHOWN) and total network transfer bytes
(data + wasm + framework + loader + web-ui assets) on a fresh browser context (cache disabled, IndexedDB cleared)
at simulated 20 Mbps and 5 Mbps, plus a second load (cached).
"""

from __future__ import annotations

import json
import subprocess
import sys
import time
from pathlib import Path
from playwright.sync_api import sync_playwright

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

REPO_ROOT = Path(__file__).resolve().parents[2]
STAGING_DIR = REPO_ROOT.parent / "_web_dist_deploy"
EVIDENCE_OUT = REPO_ROOT.parent / "Evidence" / "round38_p0_home_shown.json"


def measure_profile(pw, url: str, label: str, throughput_mbps: float | None, disable_cache: bool) -> dict:
    browser = pw.chromium.launch(headless=True, channel="msedge")
    context = browser.new_context(
        viewport={"width": 393, "height": 695},
        device_scale_factor=3,
        is_mobile=True,
        has_touch=True,
    )
    page = context.new_page()
    cdp = context.new_cdp_session(page)
    cdp.send("Network.enable")
    if disable_cache:
        cdp.send("Network.setCacheDisabled", {"cacheDisabled": True})
        cdp.send("Network.clearBrowserCache")

    if throughput_mbps is not None:
        bytes_per_sec = int((throughput_mbps * 1_000_000) / 8)
        cdp.send("Network.emulateNetworkConditions", {
            "offline": False,
            "latency": 28 if throughput_mbps >= 20 else 45,
            "downloadThroughput": bytes_per_sec,
            "uploadThroughput": bytes_per_sec // 2,
        })

    transfer_by_url: dict[str, int] = {}

    def on_loading_finished(params):
        req_id = params.get("requestId", "")
        encoded_len = int(params.get("encodedDataLength", 0))
        transfer_by_url[req_id] = encoded_len

    req_urls: dict[str, str] = {}

    def on_request_will_be_sent(params):
        req_id = params.get("requestId", "")
        req_urls[req_id] = params.get("request", {}).get("url", "")

    cdp.on("Network.loadingFinished", on_loading_finished)
    cdp.on("Network.requestWillBeSent", on_request_will_be_sent)

    t0 = time.perf_counter()
    page.goto(url, wait_until="domcontentloaded", timeout=120000)
    page.wait_for_function(
        "() => Boolean(document.querySelector('#ui-root [data-screen=\"home\"]') && document.querySelector('#ui-root .home .hero') && window.unityInstance)",
        timeout=120000,
    )
    elapsed_ms = round((time.perf_counter() - t0) * 1000)
    in_page_ms = page.evaluate("() => window.__hitmeHomeShown || Math.round(performance.now())")

    # Second load measurement if requested
    second_load_ms = None
    if label == "20Mbps_cold":
        cdp.send("Network.setCacheDisabled", {"cacheDisabled": False})
        t1 = time.perf_counter()
        page.reload(wait_until="domcontentloaded", timeout=120000)
        page.wait_for_function(
            "() => Boolean(document.querySelector('#ui-root [data-screen=\"home\"]') && document.querySelector('#ui-root .home .hero') && window.unityInstance)",
            timeout=120000,
        )
        second_load_ms = round((time.perf_counter() - t1) * 1000)

    buckets = {"data": 0, "wasm": 0, "framework": 0, "loader": 0, "webui_and_assets": 0, "total": 0}
    for req_id, nbytes in transfer_by_url.items():
        u = req_urls.get(req_id, "")
        buckets["total"] += nbytes
        if ".data" in u:
            buckets["data"] += nbytes
        elif ".wasm" in u:
            buckets["wasm"] += nbytes
        elif ".framework.js" in u:
            buckets["framework"] += nbytes
        elif ".loader.js" in u:
            buckets["loader"] += nbytes
        else:
            buckets["webui_and_assets"] += nbytes

    browser.close()
    return {
        "profile": label,
        "throughputMbps": throughput_mbps,
        "homeShownWallMs": elapsed_ms,
        "homeShownInPageMs": int(in_page_ms),
        "secondLoadWarmMs": second_load_ms,
        "transferBytes": buckets,
    }


def main() -> int:
    url = sys.argv[1] if len(sys.argv) > 1 else "https://hitmegame-iota.vercel.app/next/"
    print(f"[*] Measuring HOME_SHOWN against {url} ...")
    with sync_playwright() as pw:
        res_20 = measure_profile(pw, url, "20Mbps_cold", 20.0, disable_cache=True)
        print(f"  [20 Mbps Cold] HOME_SHOWN={res_20['homeShownWallMs']} ms (warm 2nd load={res_20['secondLoadWarmMs']} ms), total={res_20['transferBytes']['total']:,} bytes")
        res_5 = measure_profile(pw, url, "5Mbps_cold", 5.0, disable_cache=True)
        print(f"  [5 Mbps Cold]  HOME_SHOWN={res_5['homeShownWallMs']} ms, total={res_5['transferBytes']['total']:,} bytes")

    report = {"url": url, "measuredAt": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()), "results": [res_20, res_5]}
    EVIDENCE_OUT.parent.mkdir(parents=True, exist_ok=True)
    EVIDENCE_OUT.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(f"[+] Saved HOME_SHOWN metrics to {EVIDENCE_OUT}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
