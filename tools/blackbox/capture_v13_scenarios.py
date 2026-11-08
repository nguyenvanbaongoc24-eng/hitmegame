"""Capture real WebGL v1.3 scenario phases on mobile WebKit (not a physical iPhone).

Example: python tools/blackbox/capture_v13_scenarios.py --url http://127.0.0.1:8093/next/ --scenario tiebreak --height 695 --output Evidence/v13/stage1_closeout/scenarios
"""
import argparse
import json
import shutil
import time
from pathlib import Path
from urllib.parse import urlsplit, urlunsplit, parse_qsl, urlencode

from playwright.sync_api import sync_playwright


def with_flags(url, scenario):
    parts = urlsplit(url)
    query = dict(parse_qsl(parts.query))
    query.update({"dev": "1", "e2e": "1", "autostart": "practice", "scenario": scenario})
    return urlunsplit((parts.scheme, parts.netloc, parts.path, urlencode(query), parts.fragment))


def run(args):
    args.output.mkdir(parents=True, exist_ok=True)
    stem = f"{args.scenario}_{args.height}"
    row = {"url": with_flags(args.url, args.scenario), "scenario": args.scenario,
           "viewport": {"width": 393, "height": args.height, "dpr": 3},
           "engine": "Playwright WebKit mobile emulation", "events": [], "screenshots": [], "errors": []}
    with sync_playwright() as playwright:
        browser = playwright.webkit.launch(headless=True)
        context = browser.new_context(viewport={"width": 393, "height": args.height},
                                      device_scale_factor=3, is_mobile=True, has_touch=True,
                                      record_video_dir=str(args.output),
                                      record_video_size={"width": 393, "height": args.height})
        page = context.new_page()
        page.on("pageerror", lambda error: row["errors"].append(str(error)))
        start = time.monotonic()
        last_count = 0
        seen = set()
        try:
            page.goto(row["url"], wait_until="domcontentloaded", timeout=45000)
            page.wait_for_function("() => window.__hitmeState?.build === 'unity-prod' && !!window.unityInstance?.SendMessage", timeout=90000)
            page.evaluate("""() => { window.__capturedUnityEvents=[]; const original=window.hitmeUi.onUnity;
                window.hitmeUi.onUnity=function(event){let value=event;try{if(typeof event==='string')value=JSON.parse(event)}catch{}
                  window.__capturedUnityEvents.push(value);return original.call(this,event)}; }""")
            # Autostart may already have entered the match before the capture hook.
            page.wait_for_function("() => window.__hitmeState?.screen === 'match' || window.__hitmeState?.screen === 'result'", timeout=25000)
            if page.locator('[data-action="tutorial-skip"]').count():
                page.locator('[data-action="tutorial-skip"]').evaluate("el => el.click()")
            while time.monotonic() - start < args.timeout:
                events = page.evaluate("() => window.__capturedUnityEvents || []")
                for event in events[last_count:]:
                    if not isinstance(event, dict):
                        continue
                    if event.get("type") in ("match", "result"):
                        item = {key: event.get(key) for key in ("type", "round", "roundLabel", "tiebreak", "tiebreakCount", "phase", "aliveCount", "winnerIds", "rankings") if key in event}
                        item["elapsedSeconds"] = round(time.monotonic() - start, 2)
                        row["events"].append(item)
                        if event["type"] == "match":
                            phase = str(event.get("phase", "")).lower()
                            key = (bool(event.get("tiebreak")), phase)
                            want = (not key[0] and phase in ("placing", "reveal", "roundend")) or (key[0] and phase in ("placing", "reveal", "roundend"))
                            if want and key not in seen:
                                seen.add(key)
                                page.wait_for_timeout(120)
                                shot = args.output / f"{stem}_{'tiebreak' if key[0] else 'normal'}_{phase}.png"
                                page.screenshot(path=str(shot))
                                row["screenshots"].append(str(shot))
                        elif event["type"] == "result":
                            page.wait_for_timeout(120)
                            shot = args.output / f"{stem}_result.png"
                            page.screenshot(path=str(shot))
                            row["screenshots"].append(str(shot))
                last_count = len(events)
                if page.evaluate("() => window.__hitmeState?.screen === 'result'"):
                    break
                page.wait_for_timeout(100)
            row["finalScreen"] = page.evaluate("() => window.__hitmeState?.screen || null")
            row["finalText"] = page.locator("body").inner_text()[-700:]
        except Exception as exc:
            row["failure"] = f"{type(exc).__name__}: {str(exc)[:500]}"
        finally:
            row["durationSeconds"] = round(time.monotonic() - start, 2)
            video = page.video.path()
            context.close()
            browser.close()
            destination = args.output / f"{stem}.webm"
            shutil.move(video, destination)
            row["video"] = str(destination)
    output = args.output / f"{stem}.json"
    output.write_text(json.dumps(row, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    passed = row.get("finalScreen") == "result" and not row["errors"] and "failure" not in row
    print(("Passed! 1/1" if passed else "FAILED") + f" {args.scenario} {args.height} {row['durationSeconds']}s -> {output}")
    return 0 if passed else 1


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--url", required=True)
    parser.add_argument("--scenario", required=True,
                        choices=("tiebreak", "tiebreak3", "tiebreak_draw", "suddendeath", "hardcap", "ranking_ties"))
    parser.add_argument("--height", type=int, choices=(695, 852), required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--timeout", type=int, default=150)
    raise SystemExit(run(parser.parse_args()))
