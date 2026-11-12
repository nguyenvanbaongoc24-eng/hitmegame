"""Test beta WebGL reopens and a mobile orientation change in WebKit emulation.

Run: python tools/blackbox/test_lifecycle_round34.py --output Evidence/codex_selftest/lifecycle
This does not simulate an iOS process kill or a physical phone.
"""
from __future__ import annotations

import argparse
import json
from datetime import datetime, timezone
from pathlib import Path

from playwright.sync_api import sync_playwright


def run(url: str, output: Path) -> int:
    output.mkdir(parents=True, exist_ok=True)
    report = {"utc": datetime.now(timezone.utc).isoformat(), "url": url, "cases": []}
    with sync_playwright() as playwright:
        browser = playwright.webkit.launch(headless=True)
        context = browser.new_context(viewport={"width": 393, "height": 852},
                                      device_scale_factor=3, is_mobile=True, has_touch=True)
        page = context.new_page()
        errors: list[str] = []
        page.on("pageerror", lambda error: errors.append(str(error)[:400]))

        def open_home(name: str, navigate: bool) -> None:
            row = {"name": name}
            try:
                if navigate:
                    page.goto(url.rstrip("/") + "/?e2e=1", wait_until="domcontentloaded", timeout=45000)
                else:
                    page.reload(wait_until="domcontentloaded", timeout=45000)
                page.wait_for_function("() => window.__hitmeState?.screen === 'home'", timeout=90000)
                row["loaded"] = True
                row["screen"] = page.evaluate("window.__hitmeState.screen")
            except Exception as error:
                row["loaded"] = False
                row["error"] = f"{type(error).__name__}: {str(error)[-350:]}"
            report["cases"].append(row)

        open_home("initial", True)
        if report["cases"][-1]["loaded"] and page.locator('[data-action="ftue-step-2"]').count():
            page.locator('[data-action="set-lang-vi"]').click(timeout=5000)
            for action in ("ftue-step-2", "ftue-save-name", "ftue-step-4", "ftue-step-5", "ftue-finish"):
                page.locator(f'[data-action="{action}"]').click(timeout=10000)
        for index in range(5):
            open_home(f"reload_{index + 1}", False)
        for index in range(5):
            page.close()
            page = context.new_page()
            page.on("pageerror", lambda error: errors.append(str(error)[:400]))
            open_home(f"reopen_tab_{index + 1}", True)
        if report["cases"][-1]["loaded"]:
            page.screenshot(path=str(output / "portrait_after_reopens.png"))
            page.set_viewport_size({"width": 852, "height": 393})
            page.wait_for_timeout(1000)
            row = {"name": "landscape", "loaded": page.evaluate("window.__hitmeState?.screen === 'home'"),
                   "overflowPx": page.evaluate("Math.max(0, document.documentElement.scrollWidth - innerWidth)")}
            report["cases"].append(row)
            page.screenshot(path=str(output / "landscape.png"))
            page.set_viewport_size({"width": 393, "height": 852})
            page.wait_for_timeout(1000)
            row = {"name": "portrait_restored", "loaded": page.evaluate("window.__hitmeState?.screen === 'home'"),
                   "overflowPx": page.evaluate("Math.max(0, document.documentElement.scrollWidth - innerWidth)")}
            report["cases"].append(row)
            page.screenshot(path=str(output / "portrait_restored.png"))
        report["pageErrors"] = errors
        context.close()
        browser.close()
    report["passed"] = all(row.get("loaded") for row in report["cases"]) and not errors
    (output / "results.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    count = sum(bool(row.get("loaded")) for row in report["cases"])
    print(f"Passed! {count}/{len(report['cases'])} lifecycle cases" if report["passed"]
          else f"Failed: {count}/{len(report['cases'])} lifecycle cases; pageErrors={len(errors)}")
    return 0 if report["passed"] else 1


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--url", default="https://hitmegame-iota.vercel.app/next/")
    parser.add_argument("--output", type=Path, default=Path("Evidence/codex_selftest/lifecycle"))
    args = parser.parse_args()
    raise SystemExit(run(args.url, args.output))
