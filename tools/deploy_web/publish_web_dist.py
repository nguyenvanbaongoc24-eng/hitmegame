#!/usr/bin/env python3
"""
publish_web_dist.py — Packages WebGL Release build and pushes the orphan web-dist branch to GitHub for Vercel.
"""

import os
import sys
import shutil
import subprocess
from datetime import datetime
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPT_DIR.parent.parent
DEPLOY_WEB_DIR = SCRIPT_DIR
PACKAGE_SCRIPT = DEPLOY_WEB_DIR / "package_deploy.py"
DIST_DIR = DEPLOY_WEB_DIR / "dist"
STAGING_DIR = REPO_ROOT.parent / "_web_dist_deploy" if REPO_ROOT.name == "unity-client" else REPO_ROOT / "_web_dist_deploy"
REMOTE_URL = "https://github.com/nguyenvanbaongoc24-eng/hitmegame.git"

def run_cmd(cmd, cwd=None, check=True):
    print(f"[CMD] {' '.join(str(c) for c in cmd)} (cwd={cwd or '.'})")
    res = subprocess.run(cmd, cwd=cwd, text=True, capture_output=True)
    if res.stdout:
        print(res.stdout.strip())
    if res.stderr:
        print(res.stderr.strip(), file=sys.stderr)
    if check and res.returncode != 0:
        raise RuntimeError(f"Command failed with exit code {res.returncode}: {' '.join(str(c) for c in cmd)}")
    return res

def main():
    print("=== [1/4] Running package_deploy.py ===")
    run_cmd([sys.executable, str(PACKAGE_SCRIPT)])

    if not (DIST_DIR / "index.html").exists():
        raise FileNotFoundError(f"Expected index.html in {DIST_DIR}")
    if not (DIST_DIR / "vercel.json").exists():
        raise FileNotFoundError(f"Expected vercel.json in {DIST_DIR}")
    if not (DIST_DIR / "Build").exists():
        raise FileNotFoundError(f"Expected Build folder in {DIST_DIR}")

    print("\n=== [2/4] Preparing orphan staging repo ===")
    STAGING_DIR.mkdir(parents=True, exist_ok=True)
    git_dir = STAGING_DIR / ".git"

    if not git_dir.exists():
        print(f"[*] Initializing new orphan git repo in {STAGING_DIR}")
        run_cmd(["git", "init", "-b", "web-dist"], cwd=STAGING_DIR)
        run_cmd(["git", "remote", "add", "origin", REMOTE_URL], cwd=STAGING_DIR)
    else:
        # Check current branch
        res = run_cmd(["git", "branch", "--show-current"], cwd=STAGING_DIR, check=False)
        if res.stdout.strip() != "web-dist":
            run_cmd(["git", "checkout", "-B", "web-dist"], cwd=STAGING_DIR)

    # Sync files from DIST_DIR to STAGING_DIR
    print(f"[*] Syncing files from {DIST_DIR} to {STAGING_DIR}...")
    for item in STAGING_DIR.iterdir():
        if item.name == ".git":
            continue
        if item.is_dir():
            shutil.rmtree(item)
        else:
            item.unlink()

    for item in DIST_DIR.iterdir():
        dst = STAGING_DIR / item.name
        if item.is_dir():
            shutil.copytree(item, dst)
        else:
            shutil.copy2(item, dst)

    print("\n=== [3/4] Staging and Committing ===")
    run_cmd(["git", "add", "-A"], cwd=STAGING_DIR)

    status_res = run_cmd(["git", "status", "--porcelain"], cwd=STAGING_DIR)
    if not status_res.stdout.strip():
        print("[!] No changes to commit in web-dist.")
    else:
        commit_msg = f"deploy(web-dist): WebGL Release bundle for Vercel ({datetime.now().strftime('%Y-%m-%d %H:%M:%S')})"
        run_cmd(["git", "commit", "-m", commit_msg], cwd=STAGING_DIR)

    print("\n=== [4/4] Pushing web-dist to GitHub origin ===")
    # Push buffer optimization
    run_cmd(["git", "config", "http.postBuffer", "524288000"], cwd=STAGING_DIR)
    push_res = run_cmd(["git", "push", "-u", "origin", "web-dist"], cwd=STAGING_DIR)
    print(f"[SUCCESS] Branch web-dist successfully pushed to {REMOTE_URL}!")

if __name__ == "__main__":
    main()
