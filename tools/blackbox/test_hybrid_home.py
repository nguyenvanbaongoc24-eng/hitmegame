"""WebKit/Chromium smoke tests for the Hybrid Home, with real touch hit testing.

Usage: python tools/blackbox/test_hybrid_home.py --url http://127.0.0.1:8765/?mock=1
The mock Unity is only for interface tests. A failure exits nonzero and is never
silently turned into an approved visual golden.
"""
from __future__ import annotations

import argparse
import json
from pathlib import Path
from playwright.sync_api import sync_playwright

ROOT = Path(__file__).resolve().parents[2]
VIEWPORTS = {"iphone15": (393, 852), "iphone_se": (375, 667),
             "android360": (360, 800), "android412": (412, 915),
             "ipad": (768, 1024)}
KEYS = ["profile", "settings", "logo", "hero", "character-card", "weapon-card",
        "quick-play", "create-room", "join-room", "practice", "tabs"]


def run(url: str, output: Path) -> dict:
    output.mkdir(parents=True, exist_ok=True)
    results = {}
    with sync_playwright() as playwright:
        for engine_name in ("webkit", "chromium"):
            engine = getattr(playwright, engine_name)
            browser = engine.launch(headless=True, channel="msedge" if engine_name == "chromium" else None)
            for device_name, (width, height) in VIEWPORTS.items():
                context = browser.new_context(viewport={"width": width, "height": height},
                                              device_scale_factor=3 if engine_name == "webkit" else 2,
                                              is_mobile=True, has_touch=True)
                page = context.new_page()
                errors = []
                page.on("pageerror", lambda error: errors.append(str(error)))
                page.goto(url, wait_until="networkidle", timeout=30000)
                page.wait_for_selector('[data-name="quick-play"]')
                page.evaluate("document.fonts.ready")
                page.screenshot(path=str(output / f"{engine_name}_{device_name}.png"), scale="device")
                geometry = page.evaluate("""(keys) => {
                    const boxes = Object.fromEntries(keys.map(k => {
                      const e=document.querySelector(`[data-name="${k}"]`);
                      const r=e?.getBoundingClientRect();
                      return [k,r?{x:r.x,y:r.y,w:r.width,h:r.height}:null];
                    }));
                    const controls=[...document.querySelectorAll('.stage button')].filter(e=>e.offsetParent!==null);
                    const undersized=controls.filter(e=>{
                      const r=e.getBoundingClientRect();
                      return r.width<44 || r.height<44;
                    }).map(e=>({action:e.dataset.action,w:e.getBoundingClientRect().width,h:e.getBoundingClientRect().height}));
                    const clipped=controls.filter(e=>e.textContent.trim() && (e.scrollWidth>e.clientWidth+2 || e.scrollHeight>e.clientHeight+2)).map(e=>e.dataset.action);
                    const point=document.elementFromPoint(innerWidth/2,innerHeight*.42);
                    return {boxes,undersized,clipped,canvasTarget:point?.id==='unity-canvas',
                      wrongTab:document.querySelector('.tabs')?.textContent.includes('ĐĂNG NHẬP'),
                      glyphSquare:document.querySelector('.stage')?.textContent.includes('□')};
                }""", KEYS)
                failures = []
                if errors: failures.append(f"JS errors: {errors}")
                if geometry["undersized"]: failures.append(f"tap targets <44: {geometry['undersized']}")
                if geometry["clipped"]: failures.append(f"clipped text: {geometry['clipped']}")
                if not geometry["canvasTarget"]: failures.append("middle arena touch blocked")
                if geometry["wrongTab"] or geometry["glyphSquare"]: failures.append("bad Home label or square glyph")
                boxes=geometry["boxes"]
                order=("character-card", "quick-play", "create-room", "practice", "tabs")
                for upper, lower in zip(order, order[1:]):
                    if boxes[upper]["y"]+boxes[upper]["h"] > boxes[lower]["y"]+1:
                        failures.append(f"vertical overlap {upper}/{lower}")
                page.locator('[data-action="next"]').click()
                selected = page.evaluate("window.__hitmeState?.selectedCharacterId")
                if selected != "nu_nang_dong": failures.append(f"next character selected {selected}")
                page.locator('[data-action="character"]').click()
                if not page.locator('[role="dialog"]').is_visible(): failures.append("character dialog hidden")
                if not page.locator('[data-id="quay_boy"]').is_disabled(): failures.append("locked character enabled")
                page.locator('[data-action="close"]').click()
                results[f"{engine_name}/{device_name}"] = {"status":"PASS" if not failures else "FAIL",
                    "failures":failures,"geometry":geometry,"selectedAfterNext":selected}
                context.close()
            browser.close()
    (output/"results.json").write_text(json.dumps(results,ensure_ascii=False,indent=2),encoding="utf-8")
    print(f"Passed! {sum(v['status']=='PASS' for v in results.values())}/{len(results)}")
    for name,value in results.items():
        if value["failures"]: print(f"FAIL {name}: {'; '.join(value['failures'])}")
    return results


if __name__ == '__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--url',default='http://127.0.0.1:8765/?mock=1&e2e=1')
    parser.add_argument('--output',type=Path,default=ROOT/'web-ui'/'evidence'/'home_smoke')
    args=parser.parse_args()
    raise SystemExit(0 if all(x['status']=='PASS' for x in run(args.url,args.output).values()) else 1)
