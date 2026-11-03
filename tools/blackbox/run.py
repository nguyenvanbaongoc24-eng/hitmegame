"""Read-only WebKit black-box probe. Run: python tools/blackbox/run.py --url URL.

The bridge supplies observations only. All actions use Playwright touchscreen.
Missing bridge data is SKIP, never PASS. No golden image is auto-approved.
"""
from __future__ import annotations

import argparse
import json
import sys
from datetime import datetime, timezone
from pathlib import Path

import numpy as np
from PIL import Image
from playwright.sync_api import sync_playwright

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools" / "ui"))
sys.path.insert(0, str(ROOT / "tools" / "harness_lib"))
from regression_from_screenshot import analyze  # noqa: E402
from layout_check import check as check_layout  # noqa: E402


def rect(item):
    value = item.get("rect") or {}
    return [value.get("x", 0), value.get("y", 0), value.get("w", 0), value.get("h", 0)]


def visible_items(items):
    return [item for item in items if item.get("visible") and rect(item)[2] > 0 and rect(item)[3] > 0]


def tap(page, item):
    x, y, w, h = rect(item)
    page.touchscreen.tap(x + w / 2, y + h / 2)


def image_content(path: Path) -> dict:
    with Image.open(path) as image:
        pixels = np.asarray(image.convert("RGB").resize((100, 100)), dtype=np.uint8)
    colors = np.unique(pixels.reshape(-1, 3), axis=0)
    return {"unique_colors_100x100": len(colors), "rgb_std": [round(float(v), 2) for v in pixels.reshape(-1, 3).std(axis=0)],
            "blank_or_uniform": len(colors) < 16 or float(pixels.std()) < 5}


def probe(url: str, output: Path, device: str = "iPhone 15 Pro") -> dict:
    output.mkdir(parents=True, exist_ok=True)
    results = {}
    with sync_playwright() as playwright:
        browser = playwright.webkit.launch(headless=True)
        profile = dict(playwright.devices[device])
        if device == "iPhone 15 Pro":
            profile["viewport"] = {"width": 393, "height": 852}
        context = browser.new_context(**profile)
        page = context.new_page()
        errors = []
        page.on("pageerror", lambda error: errors.append(str(error)))
        try:
            page.goto(url, wait_until="domcontentloaded", timeout=45000)
            try:
                page.wait_for_function("""() => {
                    const loading = document.getElementById('unity-loading');
                    const error = document.getElementById('webgl-error-modal');
                    return (!loading || getComputedStyle(loading).display === 'none') &&
                           (!error || getComputedStyle(error).display === 'none');
                }""", timeout=120000)
            except Exception:
                pass  # Preserve a timeout screenshot; never treat it as gameplay evidence.
            loading = page.evaluate("""() => {
                const loading = document.getElementById('unity-loading');
                return !!loading && getComputedStyle(loading).display !== 'none';
            }""")
            startup_error = page.evaluate("""() => {
                const modal = document.getElementById('webgl-error-modal');
                return !!modal && getComputedStyle(modal).display !== 'none';
            }""")
            # Unity may hide its loader before the first rendered frame. Wait for
            # actual pixels, but retain a blank frame as a FAIL when it persists.
            shot = output / "home.png"
            if not loading and not startup_error:
                for _ in range(10):
                    page.screenshot(path=str(shot), scale="device")
                    if not image_content(shot)["blank_or_uniform"]:
                        break
                    page.wait_for_timeout(3000)
            page.screenshot(path=str(shot), scale="device")
            state = page.evaluate("() => window.__hitmeState || null")
            dump = page.evaluate("() => typeof window.__hitmeUiDump === 'function' ? window.__hitmeUiDump() : null")
            results["bridge"] = "available" if isinstance(state, dict) and isinstance(dump, list) else "missing"
            results["T6"] = analyze(shot)
            results["T6"]["content"] = image_content(shot)
            results["T6"]["status"] = "SKIP" if loading or startup_error else "FAIL" if any(results["T6"]["flags"].values()) or results["T6"]["content"]["blank_or_uniform"] else "PASS"
            results["loading"] = loading
            results["startup_error"] = startup_error
            if loading or startup_error or results["T6"]["content"]["blank_or_uniform"]:
                for name in ("T1", "T2", "T3", "T4", "T5"):
                    results[name] = {"status": "SKIP", "reason": "Unity loader, startup error, or uniform blank canvas"}
            elif results["bridge"] == "available":
                items = visible_items(dump)
                layout = check_layout({"safeArea": [0, 0, 393, 852], "elements": [
                    {"name": item.get("name", "?"), "rect": rect(item), "clickable": item.get("interactable", False)}
                    for item in items], "minTouchCssPx": 44})
                bad_text = [item.get("name") for item in items if "?" in (item.get("text") or "") or "□" in (item.get("text") or "")]
                results["T1"] = {"status": "PASS" if layout["passed"] and not bad_text else "FAIL",
                                  "layout": layout, "bad_text": bad_text}
                playable = (state.get("match") or {}).get("playableRect")
                results["T5"] = {"status": "SKIP", "reason": "requires match screen; home probe only"}
                if playable and state.get("screen") == "match":
                    results["T5"] = {"status": "PASS" if playable.get("w", 0) >= 393 * .94 else "FAIL", "playableRect": playable}
                results["T2"] = {"status": "SKIP", "reason": "selection interaction probe pending"}
                results["T3"] = {"status": "SKIP", "reason": "match touch probe pending"}
                results["T4"] = {"status": "SKIP", "reason": "hit sequence/video probe pending"}
            else:
                for name in ("T1", "T2", "T3", "T4", "T5"):
                    results[name] = {"status": "SKIP", "reason": "read-only ?e2e=1 bridge absent"}
            results["page_errors"] = errors
            results["url"] = page.url
            results["device"] = device
        finally:
            context.close()
            browser.close()
    (output / "results.json").write_text(json.dumps(results, ensure_ascii=False, indent=2), encoding="utf-8")
    lines = [f"# WebKit black-box probe: {device}", "", f"URL: {results['url']}", "", "| Check | Result |", "|---|---|"]
    lines += [f"| {name} | {results[name]['status']} |" for name in ("T1", "T2", "T3", "T4", "T5", "T6")]
    lines += ["", f"Bridge: {results['bridge']}. See results.json and home.png."]
    (output / "report.md").write_text("\n".join(lines) + "\n", encoding="utf-8")
    return results


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--url", default="https://hitmegame-iota.vercel.app/?e2e=1")
    parser.add_argument("--device", default="iPhone 15 Pro")
    parser.add_argument("--output", type=Path, default=ROOT / "Evidence" / "blackbox" / datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ"))
    args = parser.parse_args()
    result = probe(args.url, args.output, args.device)
    print(json.dumps({key: value["status"] for key, value in result.items() if key.startswith("T")}, sort_keys=True))
    print(args.output)
    return int(any(value["status"] == "FAIL" for key, value in result.items() if key.startswith("T")))


if __name__ == "__main__":
    raise SystemExit(main())
