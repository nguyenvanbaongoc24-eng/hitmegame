"""Hybrid Home button and startup checks in headless Chromium/WebKit.

README: python tools/blackbox/test_buttons_home.py
These are browser simulations; they do not establish real iPhone audio playback.
"""
from __future__ import annotations

import json
import re
from pathlib import Path
from playwright.sync_api import sync_playwright

REPO = Path(__file__).resolve().parents[2]
URL = "http://127.0.0.1:8765/"
OUT = REPO / "web-ui" / "evidence" / "button_home"


def run() -> dict:
    OUT.mkdir(parents=True, exist_ok=True)
    results = {}
    with sync_playwright() as playwright:
        for engine_name in ("webkit", "chromium"):
            browser = getattr(playwright, engine_name).launch(
                headless=True, channel="msedge" if engine_name == "chromium" else None)
            for lang, (width,height) in (("vi", (393,852)), ("en", (360,800))):
                name = f"{engine_name}/{lang}"
                page = browser.new_page(viewport={"width":width,"height":height},
                                        is_mobile=True, has_touch=True)
                failures = []
                errors = []
                page.on("pageerror", lambda error: errors.append(str(error)))
                page.goto(f"{URL}?mock=1&e2e=1&lang={lang}", wait_until="networkidle")
                interactive = page.evaluate("performance.getEntriesByName('hitme-home-interactive')[0]?.startTime")
                if interactive is None or interactive > 1000:
                    failures.append(f"Home interactive at {interactive} ms")
                expected = "CHƠI NHANH" if lang == "vi" else "QUICK PLAY"
                if expected not in page.locator('[data-action="quick"]').inner_text():
                    failures.append("wrong translation")
                tabs = page.locator('.tabs').inner_text()
                if ("Trang chủ" if lang == "vi" else "Home") not in tabs:
                    failures.append("wrong tab translation")
                button_audit = page.evaluate("""() => {
                  const buttons=[...document.querySelectorAll('.stage button')];
                  return {
                    clipped:buttons.filter(e=>e.matches('.cta,.selection-card,.tabs button') &&
                      (e.scrollWidth>e.clientWidth+2||e.scrollHeight>e.clientHeight+2)).map(e=>e.dataset.action),
                    transparent:buttons.filter(e=>{
                      const s=getComputedStyle(e);return s.backgroundImage==='none' &&
                        (s.backgroundColor==='rgba(0, 0, 0, 0)'||s.backgroundColor==='transparent');
                    }).map(e=>e.dataset.action),
                    squares:buttons.filter(e=>/[□▢�]/.test(e.textContent)).map(e=>e.dataset.action)
                  };
                }""")
                if button_audit["clipped"]: failures.append(f"clipped labels {button_audit['clipped']}")
                if button_audit["transparent"]: failures.append(f"transparent buttons {button_audit['transparent']}")
                if button_audit["squares"]: failures.append(f"square icons {button_audit['squares']}")
                quick = page.locator('[data-action="quick"]')
                box = quick.bounding_box()
                if box["height"] < 56: failures.append("primary button under 56px")
                page.mouse.move(box["x"] + box["width"]/2, box["y"] + box["height"]/2)
                page.mouse.down()
                page.wait_for_timeout(100)
                pressed = quick.evaluate("el=>getComputedStyle(el).transform")
                page.mouse.up()
                page.wait_for_timeout(160)
                # A real click navigates away from Home; the original button may be gone.
                released = quick.evaluate("el=>getComputedStyle(el).transform") if quick.count() else "navigated"
                match = re.search(r"matrix\(1, 0, 0, 1, 0, ([\d.]+)\)", pressed)
                if not match or not 2.8 <= float(match.group(1)) <= 3.2 or released not in ("none", "navigated"):
                    failures.append(f"pressed/released transform {pressed}/{released}")
                if released == "navigated": page.goto(f"{URL}?mock=1&e2e=1&lang={lang}", wait_until="networkidle")
                page.locator('[data-action="character"]').click()
                locked = page.locator('[data-id="quay_boy"]')
                locked_style = locked.evaluate("el=>({opacity:getComputedStyle(el).opacity,filter:getComputedStyle(el).filter})")
                if float(locked_style["opacity"]) > .71 or "grayscale" not in locked_style["filter"]:
                    failures.append("locked gray state missing")
                locked_box = locked.bounding_box()
                page.touchscreen.tap(locked_box["x"] + locked_box["width"]/2,
                                     locked_box["y"] + locked_box["height"]/2)
                if page.evaluate("window.__hitmeState.selectedCharacterId") != "nam_lang":
                    failures.append("locked button changed character")
                page.locator('[data-action="close"]').click()
                page.locator('[data-action="rank"]').click()
                if not page.locator('[data-action="close"].modal-back').is_visible():
                    failures.append("ranking lacks back button")
                page.locator('[data-action="close"].modal-back').click()
                page.locator('[data-action="quests"]').click()
                if not page.locator('[data-action="close"].modal-back').is_visible():
                    failures.append("quests lacks back button")
                page.locator('[data-action="close"].modal-back').click()
                if errors: failures.append(f"JS errors {errors}")
                results[name] = {"status":"PASS" if not failures else "FAIL", "failures":failures,
                                 "interactiveMs":interactive,"pressed":pressed,
                                 "audioAvailable":page.evaluate("Boolean(window.AudioContext||window.webkitAudioContext)"),
                                 "buttonAudit":button_audit}
                page.screenshot(path=str(OUT/f"{engine_name}_{lang}.png"),scale="device")
                page.close()
            # No Unity: the Home still paints and shows a loading state when Play is tapped.
            page = browser.new_page(viewport={"width":393,"height":852},is_mobile=True,has_touch=True)
            page.goto(f"{URL}?e2e=1",wait_until="domcontentloaded")
            page.wait_for_selector('[data-action="practice"]')
            interactive = page.evaluate("performance.getEntriesByName('hitme-home-interactive')[0]?.startTime")
            page.locator('[data-action="practice"]').click()
            page.locator('[data-action="start-practice"]').click()
            loading = page.locator('[role="dialog"]').inner_text()
            failures=[]
            if interactive is None or interactive > 1000: failures.append(f"Home interactive at {interactive} ms")
            if "Đang tải game" not in loading: failures.append("missing loading state")
            results[f"{engine_name}/no_unity"]={"status":"PASS" if not failures else "FAIL",
                                                  "failures":failures,"interactiveMs":interactive}
            page.close()
            browser.close()
    (OUT/"results.json").write_text(json.dumps(results,indent=2,ensure_ascii=False)+"\n",encoding="utf-8")
    print(f"Passed! {sum(v['status']=='PASS' for v in results.values())}/{len(results)}")
    for name,row in results.items():
        if row["failures"]: print(f"FAIL {name}: {'; '.join(row['failures'])}")
    return results


if __name__ == "__main__":
    raise SystemExit(0 if all(v["status"] == "PASS" for v in run().values()) else 1)
