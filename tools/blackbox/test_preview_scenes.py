"""README: python tools/blackbox/test_preview_scenes.py

Verify the static mock routes fit their visible viewport. This is not a Unity
or real-device test; results are screenshots plus measured DOM geometry.
"""
from __future__ import annotations

import json
import argparse
from pathlib import Path

from playwright.sync_api import sync_playwright

ROOT = Path(__file__).resolve().parents[2]
PROFILES = json.loads(Path(__file__).with_name("profiles.json").read_text(encoding="utf-8"))
SCENES = ("bot", "lobby", "match", "result", "settings", "error")
OUT = ROOT / "Evidence" / "blackbox" / "20261104_webui_preview"


def run(base_url: str, output: Path) -> dict:
    output.mkdir(parents=True, exist_ok=True)
    results = {}
    with sync_playwright() as playwright:
        browser = playwright.webkit.launch(headless=True)
        for name, profile in PROFILES.items():
            context = browser.new_context(viewport={"width": profile["width"], "height": profile["height"]},
                                          device_scale_factor=profile["dpr"], is_mobile=True, has_touch=True)
            page = context.new_page()
            errors = []
            page.on("pageerror", lambda error: errors.append(str(error)))
            for scene in SCENES:
                page.goto(f"{base_url}?mock=1&scene={scene}", wait_until="networkidle")
                geometry = page.evaluate("""() => {
                  const box = selector => {const r=document.querySelector(selector)?.getBoundingClientRect();
                    return r?{x:r.x,y:r.y,w:r.width,h:r.height}:null};
                  return {viewport:visualViewport?.height||innerHeight, stage:box('.stage'),
                    header:box('.preview-header'), footer:box('.preview-footer'),
                    arena:box('.match-arena'), scrollWidth:document.documentElement.scrollWidth};
                }""")
                failures = []
                if errors:
                    failures.append(f"JS errors {errors}")
                if not geometry["stage"] or abs(geometry["stage"]["h"] - geometry["viewport"]) > 1:
                    failures.append("stage height differs from visualViewport")
                if geometry["scrollWidth"] > profile["width"] + 1:
                    failures.append("horizontal overflow")
                if scene != "match":
                    h = geometry["header"]
                    if not h or h["y"] < -1 or h["y"] + h["h"] > geometry["viewport"]:
                        failures.append("header clipped")
                if scene in ("bot", "lobby", "result", "error"):
                    f = geometry["footer"]
                    if not f or f["y"] < 0 or f["y"] + f["h"] > geometry["viewport"] - 12:
                        failures.append("primary action clipped or too close to bottom")
                if scene == "match":
                    arena = geometry["arena"]
                    if not arena or arena["w"] < profile["width"] - 1:
                        failures.append("arena not full width")
                if name == "iphone15pro_safari" or (name == "iphonese_safari" and scene == "bot"):
                    page.screenshot(path=str(output / f"{name}_{scene}.png"), scale="device")
                results[f"{name}/{scene}"] = {"status": "PASS" if not failures else "FAIL",
                                              "failures": failures, "geometry": geometry}
                errors.clear()
            if name == "iphone15pro_safari":
                page.goto(f"{base_url}?mock=1&scene=home&debug=viewport", wait_until="networkidle")
                debug = page.locator("#viewport-debug").inner_text()
                failures = [] if "393×695" in debug and "DPR 3" in debug and "canvas" in debug else [f"incomplete debug overlay: {debug}"]
                page.screenshot(path=str(output / "iphone15pro_safari_debug.png"), scale="device")
                results[f"{name}/debug"] = {"status": "PASS" if not failures else "FAIL", "failures": failures, "text": debug}
            context.close()
        browser.close()
    (output / "results.json").write_text(json.dumps(results, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Passed! {sum(row['status']=='PASS' for row in results.values())}/{len(results)}")
    for name, row in results.items():
        if row["failures"]:
            print(f"FAIL {name}: {'; '.join(row['failures'])}")
    return results


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--url", default="http://127.0.0.1:8765/")
    parser.add_argument("--output", type=Path, default=OUT)
    args = parser.parse_args()
    raise SystemExit(0 if all(row["status"] == "PASS" for row in run(args.url, args.output).values()) else 1)
