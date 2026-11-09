#!/usr/bin/env python3
"""
deploy_gate.py — R3 Pre-Push Black-Box Deploy Gate for HIT ME Hybrid Release.
Runs:
  1. Codex's test_next_live.py (with --matches 5 --gameplay) across all 6 Codex device profiles
     and 17 mandatory coverage IDs (HOME-01, HOME-HERO, BTN-LABEL, BTN-09, VIEW-01, SEL-LOCK,
     HUD-01, F1-MATCHES, F2-ROSTER, F7-TUTORIAL, F7-REOPEN, SET-PAUSE, SET-RESUME, SET-VOLUME,
     EMO-OPEN, EMO-SEND, ARENA-TOUCH).
  2. Additional R3 viewport matrix checks:
     393x695 (DPR 3), 393x780 (DPR 3), 393x852 (DPR 3), 375x553 (DPR 2), 360x640 (DPR 3), 412x780 (DPR 2.625).
  3. Release hygiene check (tools/e2e/test_release_hygiene.py).

Exits 0 if and only if all mandatory checks PASS (zero FAIL).
Outputs gate_results.json with { passed: bool, failedIds: [...] }.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import subprocess
import sys
from datetime import datetime, timezone
from pathlib import Path
from urllib.request import urlopen

from playwright.sync_api import sync_playwright

REPO_ROOT = Path(__file__).resolve().parents[2]
TEMP_EXTRACT_DIR = REPO_ROOT.parent / "_codex_extract"
EVIDENCE_DIR = REPO_ROOT.parent / "Evidence" / "deploy_gate"

R3_VIEWPORTS = {
    "vp_393x695_dpr3": ("webkit", 393, 695, 3),
    "vp_393x780_dpr3": ("webkit", 393, 780, 3),
    "vp_393x852_dpr3": ("webkit", 393, 852, 3),
    "vp_375x553_dpr2": ("webkit", 375, 553, 2),
    "vp_360x640_dpr3": ("chromium", 360, 640, 3),
    "vp_412x780_dpr26": ("chromium", 412, 780, 2.625),
}


def sha(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def launch_browser(p, engine: str):
    if engine == "chromium":
        for ch in ("msedge", "chrome", None):
            try:
                return p.chromium.launch(headless=True, channel=ch) if ch else p.chromium.launch(headless=True)
            except Exception:
                continue
        return p.webkit.launch(headless=True)
    return getattr(p, engine).launch(headless=True)


def run_r3_viewports(base_url: str, output_dir: Path) -> tuple[dict, list[str]]:
    output_dir.mkdir(parents=True, exist_ok=True)
    base = base_url.rstrip("/") + "/"
    vp_results = {}
    failed_ids = []

    with sync_playwright() as p:
        for vp_name, (engine, w, h, dpr) in R3_VIEWPORTS.items():
            browser = launch_browser(p, engine)
            context = browser.new_context(
                viewport={"width": w, "height": h},
                device_scale_factor=dpr,
                is_mobile=True,
                has_touch=True,
            )
            page = context.new_page()
            errors = []
            page.on("pageerror", lambda err: errors.append(str(err)))
            checks = {}
            try:
                page.goto(base + "?e2e=1", wait_until="domcontentloaded", timeout=45000)
                page.wait_for_function("() => window.__hitmeState?.build && document.querySelector('[data-screen=home]')", timeout=90000)
                page.wait_for_function("() => document.querySelector('.hero')?.naturalWidth > 0", timeout=20000)

                checks["HOME-01"] = page.locator('[data-screen="home"]').is_visible()
                checks["HOME-HERO"] = page.locator(".hero").evaluate("e => e.complete && e.naturalWidth > 0")
                overflow = {
                    k: page.locator(f".home .{k}").evaluate("e => Math.max(0, e.scrollWidth - e.clientWidth)")
                    for k in ("blue", "green")
                }
                checks["BTN-LABEL"] = all(v == 0 for v in overflow.values())
                checks["BTN-09"] = page.locator('[data-action="online-locked"]').count() == 3
                canvas_box = page.locator("#unity-canvas").bounding_box()
                checks["VIEW-01"] = bool(canvas_box and canvas_box["width"] >= w - 2 and canvas_box["height"] >= h - 2)

                # Check Settings version badge on primary viewport
                if vp_name == "vp_393x695_dpr3":
                    page.locator('.home [data-action="settings"]').click()
                    page.wait_for_selector(".settings-sheet", timeout=5000)
                    page.wait_for_timeout(200)
                    has_ver = page.locator(".settings-sheet .release-version-badge").count() == 1
                    checks["SET-VERSION"] = has_ver
                    page.locator('[data-action="close"]').click()

                    # Check weapon lock (F3)
                    page.locator('[data-action="weapon"]').click()
                    checks["F3-WEAPON-LOCK"] = page.locator('[data-action="locked"]').count() == 2
                    page.locator('[data-action="close"]').click()

                page.screenshot(path=str(output_dir / f"{vp_name}.png"))
                status = "PASS" if all(checks.values()) and not errors else "FAIL"
            except Exception as exc:
                status = "FAIL"
                errors.append(f"{type(exc).__name__}: {str(exc)[:200]}")
            finally:
                context.close()
                browser.close()

            vp_results[vp_name] = {"status": status, "checks": checks, "errors": errors}
            if status != "PASS":
                for cid, ok in checks.items():
                    if not ok and cid not in failed_ids:
                        failed_ids.append(cid)
                if errors and f"ERR_{vp_name}" not in failed_ids:
                    failed_ids.append(f"ERR_{vp_name}")

    return vp_results, failed_ids


def run_codex_next_live(base_url: str, output_dir: Path, matches: int) -> tuple[dict, list[str]]:
    codex_script = TEMP_EXTRACT_DIR / "tools" / "blackbox" / "test_next_live.py"
    if not codex_script.exists():
        return {"error": f"Missing {codex_script}"}, ["MISSING_CODEX_GATE"]

    script_txt = codex_script.read_text(encoding="utf-8")
    if "channel" not in script_txt and "getattr(playwright, engine).launch(headless=True)" in script_txt:
        script_txt = script_txt.replace(
            "getattr(playwright, engine).launch(headless=True)",
            "getattr(playwright, engine).launch(headless=True, **({'channel': 'msedge'} if engine == 'chromium' else {}))",
        )
        codex_script.write_text(script_txt, encoding="utf-8")

    cmd = [
        sys.executable,
        str(codex_script),
        "--url",
        base_url,
        "--output",
        str(output_dir),
        "--matches",
        str(matches),
    ]
    if matches > 0:
        cmd.append("--gameplay")

    print(f"[*] Running Codex test_next_live.py: {' '.join(cmd)}")
    env = os.environ.copy()
    env["HITME_SOURCE_REPO"] = str(REPO_ROOT)
    res = subprocess.run(cmd, cwd=TEMP_EXTRACT_DIR, env=env, capture_output=True, text=True)
    if res.stdout:
        print(res.stdout.strip())
    if res.stderr:
        print(res.stderr.strip(), file=sys.stderr)

    results_file = output_dir / "results.json"
    if not results_file.exists():
        return {"error": "results.json not written by test_next_live.py"}, ["GATE_EXEC_FAIL"]

    data = json.loads(results_file.read_text(encoding="utf-8"))
    failed_ids = []
    if not data.get("deploymentSynced", False):
        failed_ids.append("DEPLOYMENT_SYNC")
    for cid, status in data.get("coverage", {}).items():
        if status != "PASS":
            failed_ids.append(cid)
    for prof_name, prof_row in data.get("profiles", {}).items():
        if prof_row.get("status") != "PASS":
            failed_ids.append(f"PROFILE_{prof_name}")

    return data, failed_ids


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--url", default="http://127.0.0.1:8093/next/")
    parser.add_argument("--matches", type=int, default=5)
    parser.add_argument("--output", type=Path, default=EVIDENCE_DIR)
    args = parser.parse_args()

    args.output.mkdir(parents=True, exist_ok=True)
    failed_ids: list[str] = []

    # 1. Release hygiene check
    hygiene_script = REPO_ROOT / "tools" / "e2e" / "test_release_hygiene.py"
    hygiene_res = subprocess.run(
        [sys.executable, str(hygiene_script), "--url", args.url],
        cwd=REPO_ROOT,
        capture_output=True,
        text=True,
    )
    hygiene_ok = hygiene_res.returncode == 0
    if not hygiene_ok:
        failed_ids.append("RELEASE_HYGIENE")

    # 2. R3 Multi-Viewport Matrix + F3/SET-VERSION checks
    vp_results, vp_failed = run_r3_viewports(args.url, args.output / "viewports")
    for fid in vp_failed:
        if fid not in failed_ids:
            failed_ids.append(fid)

    # 3. Codex test_next_live.py (F1 x5 matches, F2 4 chars, F5 Pause/Resume/Vol, F6 Emote, F7 Tutorial, F8 Arena Touch)
    codex_results, codex_failed = run_codex_next_live(args.url, args.output / "codex_next_live", args.matches)
    for fid in codex_failed:
        if fid not in failed_ids:
            failed_ids.append(fid)

    passed = len(failed_ids) == 0
    summary = {
        "utc": datetime.now(timezone.utc).isoformat(),
        "url": args.url,
        "passed": passed,
        "failedIds": failed_ids,
        "hygienePassed": hygiene_ok,
        "viewports": vp_results,
        "codexCoverage": codex_results.get("coverage", {}),
        "deploymentSynced": codex_results.get("deploymentSynced", False),
    }

    out_json = args.output / "gate_results.json"
    out_json.write_text(json.dumps(summary, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"[GATE] passed={passed}, failedIds={failed_ids}")
    return 0 if passed else 1


if __name__ == "__main__":
    raise SystemExit(main())
