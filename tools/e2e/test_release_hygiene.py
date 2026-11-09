#!/usr/bin/env python3
"""
test_release_hygiene.py — Verifies R1 release hygiene:
1. Test bridges (window.__hitmeState, __hitmeUiDump, __hitmeAudioLog, __OrigWebSocket)
   must NOT be exposed when ?e2e=1 is absent.
2. When ?e2e=1 is present, test bridges are exposed as expected.
3. Static inspection of index.html, next/index.html, next/app.js, next/audio.js, and HitMeWebBridge.jslib
   confirms all test bridges are gated behind e2e=1.
"""

import argparse
import json
import sys
from pathlib import Path
from playwright.sync_api import sync_playwright

REPO_ROOT = Path(__file__).resolve().parents[2]
DIST_DIR = REPO_ROOT / "tools" / "deploy_web" / "dist"


def verify_static_files(dist_dir: Path) -> list[str]:
    errors = []
    root_index = dist_dir / "index.html"
    next_app = dist_dir / "next" / "app.js"
    next_audio = dist_dir / "next" / "audio.js"
    jslib = REPO_ROOT / "Assets" / "Plugins" / "WebGL" / "HitMeWebBridge.jslib"

    if root_index.exists():
        txt = root_index.read_text(encoding="utf-8")
        if 'urlParams.get("e2e") === "1"' not in txt and 'urlParams.has("e2e")' not in txt:
            errors.append("dist/index.html missing e2e query parameter guard")
    else:
        errors.append(f"Missing {root_index}")

    if next_app.exists():
        txt = next_app.read_text(encoding="utf-8")
        if "params.get('e2e') === '1'" not in txt:
            errors.append("dist/next/app.js missing params.get('e2e') === '1' guard for __hitmeState/__hitmeUiDump")
    else:
        errors.append(f"Missing {next_app}")

    if next_audio.exists():
        txt = next_audio.read_text(encoding="utf-8")
        if "e2e" not in txt or "__hitmeAudioLog" not in txt:
            errors.append("dist/next/audio.js missing e2e guard for __hitmeAudioLog")

    if jslib.exists():
        txt = jslib.read_text(encoding="utf-8")
        if 'indexOf("e2e=1") !== -1' not in txt:
            errors.append("HitMeWebBridge.jslib missing e2e=1 guard")

    return errors


def verify_runtime(base_url: str) -> dict:
    url_clean = base_url.rstrip("/") + "/"
    url_e2e = url_clean + "?e2e=1"
    report = {"url": url_clean, "clean_check": {}, "e2e_check": {}, "passed": False}

    with sync_playwright() as p:
        browser = p.webkit.launch(headless=True)
        context = browser.new_context(viewport={"width": 393, "height": 695}, device_scale_factor=3)
        page = context.new_page()

        # 1. Load WITHOUT ?e2e=1
        page.goto(url_clean, wait_until="domcontentloaded", timeout=45000)
        # The remote WebGL bundle can take much longer than two seconds to
        # initialize. Check after Unity is ready so late test-bridge leaks
        # cannot escape this assertion.
        page.wait_for_function("() => !!window.unityInstance", timeout=90000)
        page.wait_for_timeout(1000)
        clean_globals = page.evaluate("""() => ({
            hasHitmeState: typeof window.__hitmeState !== 'undefined',
            hasHitmeUiDump: typeof window.__hitmeUiDump !== 'undefined',
            hasHitmeAudioLog: typeof window.__hitmeAudioLog !== 'undefined',
            hasOrigWebSocket: typeof window.__OrigWebSocket !== 'undefined'
        })""")
        report["clean_check"] = clean_globals

        # 2. Load WITH ?e2e=1
        page.goto(url_e2e, wait_until="domcontentloaded", timeout=45000)
        page.wait_for_function(
            "() => !!window.unityInstance && !!window.__hitmeState && typeof window.__hitmeUiDump === 'function'",
            timeout=90000,
        )
        e2e_globals = page.evaluate("""() => ({
            hasHitmeState: typeof window.__hitmeState !== 'undefined',
            hasHitmeUiDump: typeof window.__hitmeUiDump === 'function',
            hasHitmeAudioLog: Array.isArray(window.__hitmeAudioLog)
        })""")
        report["e2e_check"] = e2e_globals
        context.close()
        browser.close()

    clean_ok = not any(clean_globals.values())
    e2e_ok = e2e_globals.get("hasHitmeState", False) and e2e_globals.get("hasHitmeUiDump", False)
    report["passed"] = clean_ok and e2e_ok
    return report


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--dist", type=Path, default=DIST_DIR)
    parser.add_argument("--url", default="")
    args = parser.parse_args()

    static_errors = verify_static_files(args.dist)
    result = {"static_errors": static_errors, "runtime": None}
    if static_errors:
        print(json.dumps(result, indent=2, ensure_ascii=False))
        return 1

    if args.url:
        rt = verify_runtime(args.url)
        result["runtime"] = rt
        if not rt["passed"]:
            print(json.dumps(result, indent=2, ensure_ascii=False))
            return 1

    print(json.dumps({"status": "PASS", "static_errors": [], "runtime": result["runtime"]}, indent=2, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
