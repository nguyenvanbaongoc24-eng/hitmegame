"""Collect read-only browser evidence for the deployed HIT ME beta.

Run: python tools/blackbox/selftest_round34.py --output Evidence/codex_selftest/browser
Playwright emulates mobile browsers; it does not prove behavior on a real phone.
"""
from __future__ import annotations

import argparse
import json
from datetime import datetime, timezone
from pathlib import Path
from playwright.sync_api import sync_playwright


PROFILES = (
    ("webkit_393x852", "webkit", 393, 852, 3),
    ("webkit_375x667", "webkit", 375, 667, 2),
    ("webkit_430x932", "webkit", 430, 932, 3),
    ("chromium_412x915", "chromium", 412, 915, 2.6),
)


def run(base: str, output: Path) -> int:
    output.mkdir(parents=True, exist_ok=True)
    report = {"utc": datetime.now(timezone.utc).isoformat(), "url": base, "cases": []}
    with sync_playwright() as pw:
        for name, engine, width, height, dpr in PROFILES:
            browser = getattr(pw, engine).launch(headless=True, **({"channel": "msedge"} if engine == "chromium" else {}))
            for language in ("vi", "en"):
                context = browser.new_context(viewport={"width": width, "height": height}, device_scale_factor=dpr, is_mobile=True, has_touch=True, locale="vi-VN" if language == "vi" else "en-US", record_video_dir=str(output / "video"))
                page = context.new_page()
                row = {"profile": name, "language": language, "pageErrors": [], "consoleErrors": [], "failedRequests": []}
                page.on("pageerror", lambda error, r=row: r["pageErrors"].append(str(error)[:300]))
                page.on("console", lambda msg, r=row: r["consoleErrors"].append(msg.text[:300]) if msg.type == "error" else None)
                page.on("requestfailed", lambda req, r=row: r["failedRequests"].append({"url": req.url.split("?")[0], "failure": req.failure}))
                try:
                    page.goto(base.rstrip("/") + "/?e2e=1", wait_until="domcontentloaded", timeout=45000)
                    page.wait_for_function("() => window.__hitmeState?.screen === 'home'", timeout=90000)
                    row["htmlLang"] = page.evaluate("document.documentElement.lang")
                    row["home"] = True
                    page.screenshot(path=str(output / f"{name}_{language}_home.png"))
                    row["firstRunOverlay"] = page.locator(".modal-shade").count() > 0
                    if row["firstRunOverlay"] and page.locator('[data-action="ftue-step-2"]').count():
                        page.locator(f'[data-action="set-lang-{language}"]').click()
                        for action in ("ftue-step-2", "ftue-save-name", "ftue-step-4", "ftue-step-5", "ftue-finish"):
                            page.locator(f'[data-action="{action}"]').click(timeout=5000)
                        row["firstRunFinished"] = page.locator(".modal-shade").count() == 0
                    row["htmlLangAfterSetup"] = page.evaluate("document.documentElement.lang")
                    button = page.locator('[data-action="practice"]')
                    row["practiceButton"] = button.evaluate("""e=>{let r=e.getBoundingClientRect(),x=r.x+r.width/2,y=r.y+r.height/2,t=document.elementFromPoint(x,y);return {rect:{x:r.x,y:r.y,w:r.width,h:r.height},topTag:t?.tagName,topClass:t?.className,topAction:t?.getAttribute('data-action'),buttonHtml:e.outerHTML.slice(0,350)}}""")
                    try:
                        button.click(timeout=5000)
                        row["practiceClick"] = "native"
                    except Exception as error:
                        row["practiceClick"] = type(error).__name__
                        row["clickError"] = str(error)[-450:]
                    row["screenAfterClick"] = page.evaluate("window.__hitmeState?.screen")
                    page.screenshot(path=str(output / f"{name}_{language}_after_click.png"))
                except Exception as error:
                    row["failure"] = f"{type(error).__name__}: {str(error)[-500:]}"
                finally:
                    report["cases"].append(row)
                    context.close()
                    (output / "results.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
            browser.close()
    (output / "results.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    passed = sum(row.get("screenAfterClick") == "bot" and not row["pageErrors"] for row in report["cases"])
    print(f"Passed! {passed}/{len(report['cases'])} practice-navigation cases")
    return 0 if passed == len(report["cases"]) else 1


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--url", default="https://hitmegame-iota.vercel.app/next/")
    parser.add_argument("--output", type=Path, default=Path("Evidence/codex_selftest/browser"))
    args = parser.parse_args()
    raise SystemExit(run(args.url, args.output))
