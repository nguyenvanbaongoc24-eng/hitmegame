"""Run one real WebGL training match at each supported total player count.

Run: python tools/blackbox/test_v13_player_matrix.py --url https://hitmegame-iota.vercel.app/next/ --output Evidence/v13/stage1_player_matrix.json
Playwright WebKit emulates mobile Safari; this is not a physical iPhone test.
"""
from __future__ import annotations

import argparse
import json
from datetime import datetime, timezone
from pathlib import Path

from playwright.sync_api import sync_playwright


def run(url: str, output: Path) -> int:
    results = {"url": url, "utc": datetime.now(timezone.utc).isoformat(), "engine": "WebKit mobile emulation", "matches": []}
    with sync_playwright() as playwright:
        browser = playwright.webkit.launch(headless=True)
        context = browser.new_context(viewport={"width": 393, "height": 695}, device_scale_factor=3, is_mobile=True, has_touch=True)
        page = context.new_page()
        errors: list[str] = []
        page.on("pageerror", lambda error: errors.append(str(error)))
        try:
            page.goto(url.rstrip("/") + "/?e2e=1", wait_until="domcontentloaded", timeout=45000)
            page.wait_for_function("() => window.__hitmeState?.build === 'unity-prod' && document.querySelector('[data-screen=home]')", timeout=90000)
            for total_players in range(2, 7):
                page.locator('[data-action="practice"]').click()
                page.wait_for_function("() => window.__hitmeState?.screen === 'bot'", timeout=15000)
                desired_bots = total_players - 1
                current_bots = page.evaluate("window.__hitmeState.botCount")
                button = 'bot-plus' if desired_bots > current_bots else 'bot-minus'
                for _ in range(abs(desired_bots - current_bots)):
                    page.locator(f'[data-action="{button}"]').click()
                selected_bots = page.evaluate("window.__hitmeState.botCount")
                row = {"totalPlayers": total_players, "selectedBots": selected_bots}
                page.locator('[data-action="start-practice"]').click()
                page.wait_for_function("() => window.__hitmeState?.screen === 'match'", timeout=15000)
                if page.locator('.live-tutorial').count():
                    page.locator('[data-action="tutorial-skip"]').evaluate('el => el.click()')
                page.wait_for_function("() => window.__hitmeState?.screen === 'result'", timeout=180000)
                row["rankings"] = page.locator('.live-rank').count()
                row["resultText"] = page.locator('[data-screen="result"]').inner_text()[:160]
                row["passed"] = selected_bots == desired_bots and row["rankings"] == total_players
                results["matches"].append(row)
                page.locator('[data-action="exit-match"]').click()
                page.wait_for_function("() => window.__hitmeState?.screen === 'home'", timeout=15000)
        except Exception as exc:
            results["failure"] = f"{type(exc).__name__}: {str(exc)[:400]}"
        results["pageErrors"] = errors
        context.close()
        browser.close()
    results["passed"] = len(results["matches"]) == 5 and all(row["passed"] for row in results["matches"]) and not errors and "failure" not in results
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(results, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Passed! {sum(row['passed'] for row in results['matches'])}/5" if results["passed"] else f"FAILED: {len(results['matches'])}/5 completed; {results.get('failure', errors)}")
    return 0 if results["passed"] else 1


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--url", default="https://hitmegame-iota.vercel.app/next/")
    parser.add_argument("--output", type=Path, default=Path("Evidence/v13/stage1_player_matrix.json"))
    args = parser.parse_args()
    raise SystemExit(run(args.url, args.output))
