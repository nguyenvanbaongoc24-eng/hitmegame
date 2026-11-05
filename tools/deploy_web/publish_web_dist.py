#!/usr/bin/env python3
"""
publish_web_dist.py — R2/R3/R4 Safe Multi-Channel Publisher for HIT ME.
Channels:
  /             Stable channel (untouched unless --promote is passed)
  /next/        Candidate hybrid channel (published only when deploy_gate is GREEN)
  /ui-preview/  Web UI preview channel
  /rc/          Unverified candidate channel (published when deploy_gate is RED, with visible RC badge)
"""

from __future__ import annotations

import argparse
import json
import shutil
import subprocess
import sys
import time
from datetime import datetime, timezone
from pathlib import Path
from urllib.request import urlopen

SCRIPT_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPT_DIR.parent.parent
DIST_DIR = SCRIPT_DIR / "dist"
PACK_HYBRID_SCRIPT = REPO_ROOT / "tools" / "hybrid" / "pack_hybrid.py"
DEPLOY_GATE_SCRIPT = REPO_ROOT / "tools" / "blackbox" / "deploy_gate.py"
STAGING_DIR = REPO_ROOT.parent / "_web_dist_deploy"
REMOTE_URL = "https://github.com/nguyenvanbaongoc24-eng/hitmegame.git"
LIVE_BASE_URL = "https://hitmegame-iota.vercel.app"


def run_cmd(cmd: list[str], cwd: Path | None = None, check: bool = True) -> subprocess.CompletedProcess:
    print(f"[CMD] {' '.join(str(c) for c in cmd)} (cwd={cwd or '.'})")
    res = subprocess.run(cmd, cwd=cwd, text=True, capture_output=True)
    if res.stdout:
        print(res.stdout.strip())
    if res.stderr:
        print(res.stderr.strip(), file=sys.stderr)
    if check and res.returncode != 0:
        raise RuntimeError(f"Command failed ({res.returncode}): {' '.join(str(c) for c in cmd)}")
    return res


def sync_dist_to_staging(dist_dir: Path, staging_dir: Path, keep_root_untouched: bool = True) -> None:
    staging_dir.mkdir(parents=True, exist_ok=True)
    # Only update vercel.json, release.json, ui-preview/, next/, rc/ when keep_root_untouched is True
    if keep_root_untouched:
        for rel in ("vercel.json", "release.json"):
            if (dist_dir / rel).exists():
                shutil.copy2(dist_dir / rel, staging_dir / rel)
        if (dist_dir / "Build").exists():
            (staging_dir / "Build").mkdir(parents=True, exist_ok=True)
            for bf in (dist_dir / "Build").iterdir():
                if bf.is_file():
                    shutil.copy2(bf, staging_dir / "Build" / bf.name)
        for folder in ("ui-preview", "next", "rc"):
            src = dist_dir / folder
            dst = staging_dir / folder
            if src.exists():
                if dst.exists():
                    shutil.rmtree(dst)
                shutil.copytree(src, dst)
    else:
        for item in staging_dir.iterdir():
            if item.name == ".git":
                continue
            if item.is_dir():
                shutil.rmtree(item)
            else:
                item.unlink()
        for item in dist_dir.iterdir():
            dst = staging_dir / item.name
            if item.is_dir():
                shutil.copytree(item, dst)
            else:
                shutil.copy2(item, dst)


def poll_live_release(channel: str, expected_build_id: str, timeout_sec: int = 600) -> dict | None:
    url = f"{LIVE_BASE_URL}/{channel.strip('/')}/release.json" if channel.strip("/") else f"{LIVE_BASE_URL}/release.json"
    print(f"[*] Polling {url} for buildId={expected_build_id} (timeout={timeout_sec}s)...")
    deadline = time.time() + timeout_sec
    while time.time() < deadline:
        try:
            with urlopen(url + f"?t={int(time.time())}", timeout=15) as resp:
                if resp.status == 200:
                    data = json.loads(resp.read().decode("utf-8"))
                    if data.get("buildId") == expected_build_id:
                        print(f"[+] Live {url} matched buildId={expected_build_id}!")
                        return data
                    else:
                        print(f"    current live buildId={data.get('buildId')} (waiting for {expected_build_id})...")
        except Exception as exc:
            print(f"    waiting for {url} ({type(exc).__name__})...")
        time.sleep(8)
    return None


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--promote", action="store_true", help="Promote /next/ to root / with backup in /previous/")
    parser.add_argument("--local-url", default="http://127.0.0.1:8093/next/")
    parser.add_argument("--matches", type=int, default=5)
    parser.add_argument("--skip-gate", action="store_true")
    parser.add_argument("--no-push", action="store_true")
    args = parser.parse_args()

    if args.promote:
        run_cmd([sys.executable, str(PACK_HYBRID_SCRIPT), "--promote"])
        sync_dist_to_staging(DIST_DIR, STAGING_DIR, keep_root_untouched=False)
        if not args.no_push:
            run_cmd(["git", "add", "-A"], cwd=STAGING_DIR)
            run_cmd(["git", "commit", "-m", "release(promote): promote /next/ to root /"], cwd=STAGING_DIR, check=False)
            run_cmd(["git", "push", "-u", "origin", "web-dist"], cwd=STAGING_DIR)
        return 0

    # Step 1: Package candidate into /next/ and /ui-preview/
    print("=== [1/4] Packaging candidate into /next/ and /ui-preview/ ===")
    run_cmd([sys.executable, str(PACK_HYBRID_SCRIPT), "--channel", "next"])

    # Step 2: Run deploy_gate against local server
    gate_passed = True
    failed_ids: list[str] = []
    if not args.skip_gate:
        print("\n=== [2/4] Running Pre-Push Deploy Gate (tools/blackbox/deploy_gate.py) ===")
        gate_res = run_cmd(
            [sys.executable, str(DEPLOY_GATE_SCRIPT), "--url", args.local_url, "--matches", str(args.matches)],
            cwd=REPO_ROOT,
            check=False,
        )
        gate_json = REPO_ROOT.parent / "Evidence" / "deploy_gate" / "gate_results.json"
        if gate_json.exists():
            gdata = json.loads(gate_json.read_text(encoding="utf-8"))
            gate_passed = bool(gdata.get("passed", False))
            failed_ids = gdata.get("failedIds", [])
        else:
            gate_passed = gate_res.returncode == 0
            if not gate_passed:
                failed_ids = ["GATE_NO_JSON"]

    target_channel = "next" if gate_passed else "rc"
    if not gate_passed:
        print(f"\n[!] Deploy Gate RED (failedIds={failed_ids}). Routing candidate to /rc/ instead of /next/!")
        # Restore /next/ from STAGING_DIR so /next/ is not overwritten with an unverified build, and package /rc/
        if (DIST_DIR / "next").exists():
            shutil.rmtree(DIST_DIR / "next")
        if (STAGING_DIR / "next").exists():
            shutil.copytree(STAGING_DIR / "next", DIST_DIR / "next")
        run_cmd([
            sys.executable,
            str(PACK_HYBRID_SCRIPT),
            "--channel",
            "rc",
            "--gate-failed-ids",
            ",".join(failed_ids),
        ])
    else:
        print("\n[+] Deploy Gate GREEN! Publishing to /next/ and /ui-preview/.")

    release_meta = json.loads((DIST_DIR / target_channel / "release.json").read_text(encoding="utf-8"))
    version = release_meta["version"]
    build_id = release_meta["buildId"]

    if args.no_push:
        print(f"[*] --no-push specified. Package ready in {DIST_DIR} (channel=/{target_channel}/, buildId={build_id}).")
        return 0

    # Step 3: Stage & Commit in _web_dist_deploy (keeping root / untouched)
    print(f"\n=== [3/4] Staging /{target_channel}/ and /ui-preview/ in {STAGING_DIR} ===")
    prev_head = run_cmd(["git", "rev-parse", "--short", "HEAD"], cwd=STAGING_DIR, check=False).stdout.strip()
    print(f"[*] Rollback point on web-dist: {prev_head}")

    sync_dist_to_staging(DIST_DIR, STAGING_DIR, keep_root_untouched=True)
    # Restore tracked tools/deploy_web/dist so Unity working tree remains 100% clean
    run_cmd(["git", "checkout", "--", "tools/deploy_web/dist"], cwd=REPO_ROOT, check=False)
    run_cmd(["git", "clean", "-fd", "tools/deploy_web/dist"], cwd=REPO_ROOT, check=False)
    run_cmd(["git", "add", "-A"], cwd=STAGING_DIR)
    status_res = run_cmd(["git", "status", "--porcelain"], cwd=STAGING_DIR)
    if status_res.stdout.strip():
        commit_msg = f"release({version}): publish /{target_channel}/ and /ui-preview/ (build {build_id})"
        run_cmd(["git", "commit", "-m", commit_msg], cwd=STAGING_DIR)

    # Step 4: Push to origin/web-dist and poll live release.json
    print("\n=== [4/4] Pushing web-dist to GitHub & Polling Vercel ===")
    run_cmd(["git", "config", "http.postBuffer", "524288000"], cwd=STAGING_DIR)
    run_cmd(["git", "push", "-u", "origin", "web-dist"], cwd=STAGING_DIR)
    poll_live_release(target_channel, build_id, timeout_sec=600)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
