"""Measure Home spacing in a built hybrid folder, with a mock bridge.

Run: python tools/blackbox/test_home_spacing.py --root "D:/HITME GAME/worktrees/web-dist-mobile"
The test checks DOM geometry; it does not certify the appearance of Unity sprites.
"""
import argparse
import json
import threading
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

from playwright.sync_api import sync_playwright

PROFILES = [("iphone15pro_safari", 393, 695, 3), ("iphone15pro_standalone", 393, 852, 3),
            ("iphone15pro_collapsed", 393, 780, 3), ("iphonese_safari", 375, 553, 2),
            ("android_chrome", 360, 640, 3), ("android_tall", 412, 780, 2.6)]


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--output", type=Path, default=Path("Evidence/mobile/20261106/home_spacing"))
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=True)
    class Handler(SimpleHTTPRequestHandler):
        def __init__(self, *a, **k): super().__init__(*a, directory=str(args.root), **k)
        def log_message(self, *_): pass
    server = ThreadingHTTPServer(("127.0.0.1", 0), Handler)
    threading.Thread(target=server.serve_forever, daemon=True).start()
    results = {}
    try:
        with sync_playwright() as playwright:
            for name, width, height, dpr in PROFILES:
                engine = playwright.webkit if name.startswith("iphone") else playwright.chromium
                browser = engine.launch(headless=True, **({"channel":"msedge"} if engine == playwright.chromium else {}))
                context = browser.new_context(viewport={"width":width,"height":height}, device_scale_factor=dpr,
                                              is_mobile=True, has_touch=True)
                page = context.new_page()
                page.route("**/Build/**", lambda route: route.abort())
                page.goto(f"http://127.0.0.1:{server.server_port}/next/?mock=1&e2e=1", wait_until="networkidle")
                page.wait_for_selector(".home .hero")
                # Unity is intentionally absent in mock mode; remove its loading veil for visual QA.
                page.evaluate("document.querySelector('#unity-loading')?.remove()")
                page.screenshot(path=str(args.output / f"{name}.png"))
                boxes = page.evaluate("""() => Object.fromEntries(['logo','hero','character-card','weapon-card','quick','blue','green','practice','tabs'].map(k=>{const r=document.querySelector('.home .'+k).getBoundingClientRect();return [k,{x:r.x,y:r.y,right:r.right,bottom:r.bottom,w:r.width,h:r.height}]}))""")
                gaps = {"logoHero":boxes["hero"]["y"]-boxes["logo"]["bottom"],
                        "cardsSideBySide":boxes["weapon-card"]["x"]-boxes["character-card"]["right"],
                        "cardsQuick":boxes["quick"]["y"]-max(boxes[k]["bottom"] for k in ("character-card","weapon-card")),
                        "quickRooms":min(boxes[k]["y"] for k in ("blue","green"))-boxes["quick"]["bottom"],
                        "roomsPractice":boxes["practice"]["y"]-max(boxes[k]["bottom"] for k in ("blue","green")),
                        "practiceTabs":boxes["tabs"]["y"]-boxes["practice"]["bottom"]}
                limits={"logoHero":8,"cardsSideBySide":12,"cardsQuick":16,"quickRooms":12,"roomsPractice":16,"practiceTabs":12}
                failures=[f"{k} {gaps[k]:.1f} < {minimum}" for k,minimum in limits.items() if gaps[k]<minimum-0.5]
                results[name]={"gaps":gaps,"boxes":boxes,"failures":failures}
                context.close(); browser.close()
    finally: server.shutdown()
    (args.output / "results.json").write_text(json.dumps(results,ensure_ascii=False,indent=2),encoding="utf-8")
    passed=sum(not row["failures"] for row in results.values())
    print(f"Passed! {passed}/{len(results)}")
    for name,row in results.items():
        if row["failures"]: print(name, "; ".join(row["failures"]))
    return 0 if passed==len(results) else 1

if __name__ == "__main__": raise SystemExit(main())
