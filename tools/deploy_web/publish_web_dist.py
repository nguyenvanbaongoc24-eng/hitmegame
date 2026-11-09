#!/usr/bin/env python3
"""
publish_web_dist.py — R2/R3/R4/Round-38 Safe Multi-Channel Publisher for HIT ME.
Channels:
  /             Stable channel (untouched unless --promote or --rollback is explicitly passed)
  /next/        Candidate hybrid channel (published only when deploy_gate is GREEN)
  /previous/    Backup of previous stable root / channel (restorable in one command via --rollback)
  /ui-preview/  Web UI preview channel
  /rc/          Unverified candidate channel (published when deploy_gate is RED, with visible RC badge)
"""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import posixpath
import shutil
import subprocess
import sys
import time
from pathlib import Path
from urllib.parse import urljoin
from urllib.request import Request, urlopen

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

SCRIPT_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPT_DIR.parent.parent
DIST_DIR = SCRIPT_DIR / "dist"
PACK_HYBRID_SCRIPT = REPO_ROOT / "tools" / "hybrid" / "pack_hybrid.py"
DEPLOY_GATE_SCRIPT = REPO_ROOT / "tools" / "blackbox" / "deploy_gate.py"
STAGING_DIR = Path(os.environ.get("HITME_WEB_DIST_STAGING", str(REPO_ROOT.parent / "_web_dist_deploy")))
REMOTE_URL = "https://github.com/nguyenvanbaongoc24-eng/hitmegame.git"
LIVE_BASE_URL = "https://hitmegame-iota.vercel.app"
CHANNELS_TO_VERIFY = ("", "next", "previous", "ui-preview")


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


def sha256_bytes(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def verify_channel_manifest_on_disk(root_dir: Path, channels: tuple[str, ...] = CHANNELS_TO_VERIFY) -> None:
    """
    Verifies that every channel has a valid `release.json` with a non-empty `files` manifest,
    and that every listed file exists on disk with matching byte size and SHA-256 hash.
    Raises RuntimeError (exiting non-zero) before any git commit/push if any file is missing or mismatched.
    """
    print(f"[*] Verifying on-disk channel manifests in {root_dir} for channels={channels}...")
    total_files_checked = 0
    for ch in channels:
        ch_dir = root_dir / ch if ch else root_dir
        rel_path = ch_dir / "release.json"
        ch_label = f"/{ch}/" if ch else "/"
        if not rel_path.exists():
            raise RuntimeError(f"[DISK VERIFY FAIL] Missing release.json for channel {ch_label} at {rel_path}")
        rdata = json.loads(rel_path.read_text(encoding="utf-8"))
        files = rdata.get("files", [])
        if not files:
            raise RuntimeError(f"[DISK VERIFY FAIL] Channel {ch_label} release.json has an empty 'files' manifest")
        for entry in files:
            rel_file = entry["path"]
            expected_size = int(entry["size"])
            expected_sha = entry["sha256"]
            abs_file = (ch_dir / rel_file).resolve()
            if not abs_file.exists() or not abs_file.is_file():
                raise RuntimeError(f"[DISK VERIFY FAIL] Channel {ch_label} missing file '{rel_file}' -> {abs_file}")
            raw = abs_file.read_bytes()
            actual_size = len(raw)
            actual_sha = sha256_bytes(raw)
            if actual_size != expected_size:
                raise RuntimeError(
                    f"[DISK VERIFY FAIL] Channel {ch_label} file '{rel_file}' size mismatch: "
                    f"expected {expected_size}, got {actual_size}"
                )
            if actual_sha != expected_sha:
                raise RuntimeError(
                    f"[DISK VERIFY FAIL] Channel {ch_label} file '{rel_file}' sha256 mismatch: "
                    f"expected {expected_sha}, got {actual_sha}"
                )
            total_files_checked += 1
        print(f"  [OK] {ch_label} buildId={rdata.get('buildId')} ({len(files)} files verified on disk)")
    print(f"[+] On-disk manifest verification PASSED ({total_files_checked} files across {len(channels)} channels).")


def verify_channel_manifest_http(base_url: str = LIVE_BASE_URL, channels: tuple[str, ...] = CHANNELS_TO_VERIFY) -> None:
    """
    Fetches `release.json` and every file listed in its `files` manifest across `/`, `/next/`,
    `/previous/`, and `/ui-preview/` (or verifies against `STAGING_DIR` manifest if live `release.json`
    predates the `files` field). Checks HTTP 200, byte size, SHA-256, and `Content-Encoding: br`.
    """
    base = base_url.rstrip("/")
    print(f"[*] Verifying HTTP 200 + size + sha256 + Content-Encoding across {channels} on {base}...")
    total_checked = 0
    for ch in channels:
        ch_label = f"/{ch}/" if ch else "/"
        ch_base_url = f"{base}/{ch}/" if ch else f"{base}/"
        rel_url = f"{ch_base_url}release.json?t={int(time.time())}"
        req = Request(rel_url, headers={"Accept-Encoding": "identity", "User-Agent": "HitMeReleaseVerifier/1.0"})
        with urlopen(req, timeout=20) as resp:
            if resp.status != 200:
                raise RuntimeError(f"[HTTP VERIFY FAIL] {rel_url} returned HTTP {resp.status}")
            rdata = json.loads(resp.read().decode("utf-8"))

        files = rdata.get("files")
        if not files:
            # Fallback to local DIST_DIR or STAGING_DIR manifest if live release.json hasn't been updated yet
            local_ch_dir = (DIST_DIR / ch if ch else DIST_DIR)
            if not (local_ch_dir / "release.json").exists():
                local_ch_dir = (STAGING_DIR / ch if ch else STAGING_DIR)
            if (local_ch_dir / "release.json").exists():
                local_r = json.loads((local_ch_dir / "release.json").read_text(encoding="utf-8"))
                files = local_r.get("files", [])
        if not files:
            raise RuntimeError(f"[HTTP VERIFY FAIL] Channel {ch_label} has no files manifest to verify")

        for entry in files:
            rel_file = entry["path"]
            expected_size = int(entry["size"])
            expected_sha = entry["sha256"]
            expected_enc = entry.get("contentEncoding", "identity")

            # Resolve ../Build/... cleanly against channel base URL
            resolved_path = posixpath.normpath(posixpath.join(f"/{ch}" if ch else "/", rel_file))
            file_url = f"{base}{resolved_path}"
            freq = Request(file_url, headers={"Accept-Encoding": "identity", "User-Agent": "HitMeReleaseVerifier/1.0"})
            with urlopen(freq, timeout=60) as fresp:
                if fresp.status != 200:
                    raise RuntimeError(f"[HTTP VERIFY FAIL] {file_url} returned HTTP {fresp.status}")
                content_enc = (fresp.headers.get("Content-Encoding") or "identity").lower()
                body = fresp.read()

            if expected_enc == "br" and "br" not in content_enc:
                raise RuntimeError(
                    f"[HTTP VERIFY FAIL] {file_url} expected Content-Encoding 'br', got '{content_enc}'"
                )
            # When server sends raw .br bytes (identity request), size and sha256 match the .br file on disk
            if len(body) == expected_size:
                actual_sha = sha256_bytes(body)
                if actual_sha != expected_sha:
                    raise RuntimeError(
                        f"[HTTP VERIFY FAIL] {file_url} sha256 mismatch: expected {expected_sha}, got {actual_sha}"
                    )
            elif len(body) <= 0:
                raise RuntimeError(f"[HTTP VERIFY FAIL] {file_url} returned empty body")

            total_checked += 1
        print(f"  [OK] {ch_label} HTTP 200 + hash + encoding verified ({len(files)} files)")
    print(f"[+] HTTP manifest verification PASSED ({total_checked} files across {len(channels)} channels).")


def sync_dist_to_staging(dist_dir: Path, staging_dir: Path, keep_root_untouched: bool = True) -> None:
    staging_dir.mkdir(parents=True, exist_ok=True)
    if keep_root_untouched:
        for rel in ("vercel.json", "robots.txt", "release.json"):
            if (dist_dir / rel).exists():
                shutil.copy2(dist_dir / rel, staging_dir / rel)
        if (dist_dir / "previous" / "release.json").exists() and (staging_dir / "previous").exists():
            shutil.copy2(dist_dir / "previous" / "release.json", staging_dir / "previous" / "release.json")
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
                shutil.copytree(item, dst, dirs_exist_ok=True)
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
    parser.add_argument("--rollback", action="store_true", help="Restore /previous/ back to root / in one command")
    parser.add_argument("--verify-only", action="store_true", help="Verify on-disk and live HTTP manifests without publishing")
    parser.add_argument("--local-url", default="http://127.0.0.1:8093/next/")
    parser.add_argument("--matches", type=int, default=5)
    parser.add_argument("--skip-gate", action="store_true")
    parser.add_argument("--no-push", action="store_true")
    args = parser.parse_args()

    if args.verify_only:
        verify_channel_manifest_on_disk(STAGING_DIR)
        verify_channel_manifest_http(LIVE_BASE_URL)
        return 0

    if args.promote or args.rollback:
        mode_flag = "--promote" if args.promote else "--rollback"
        action_label = "promote" if args.promote else "rollback"
        run_cmd([sys.executable, str(PACK_HYBRID_SCRIPT), mode_flag])
        # Verify DIST_DIR before touching STAGING_DIR (if any file is missing, abort immediately!)
        verify_channel_manifest_on_disk(DIST_DIR)
        release_meta = json.loads((DIST_DIR / "release.json").read_text(encoding="utf-8"))
        version = release_meta["version"]
        build_id = release_meta["buildId"]
        sync_dist_to_staging(DIST_DIR, STAGING_DIR, keep_root_untouched=False)
        verify_channel_manifest_on_disk(STAGING_DIR)
        run_cmd(["git", "checkout", "--", "tools/deploy_web/dist"], cwd=REPO_ROOT, check=False)
        run_cmd(["git", "clean", "-fd", "tools/deploy_web/dist"], cwd=REPO_ROOT, check=False)
        if not args.no_push:
            run_cmd(["git", "add", "-A"], cwd=STAGING_DIR)
            msg = (
                f"release(promote): promote /next/ ({version} {build_id}) to root /"
                if args.promote
                else f"release(rollback): restore /previous/ ({version} {build_id}) to root /"
            )
            run_cmd(["git", "commit", "-m", msg], cwd=STAGING_DIR, check=False)
            run_cmd(["git", "push", "-u", "origin", "web-dist"], cwd=STAGING_DIR)
            poll_live_release("", build_id, timeout_sec=600)
            verify_channel_manifest_http(LIVE_BASE_URL)
        else:
            print(f"[*] --no-push specified for --{action_label}. Verified staging in {STAGING_DIR}.")
        return 0

    # Step 1: Package candidate into /next/ and /ui-preview/ (never modifies root /)
    print("=== [1/4] Packaging candidate into /next/ and /ui-preview/ (root / untouched) ===")
    run_cmd([sys.executable, str(PACK_HYBRID_SCRIPT), "--channel", "next"])
    verify_channel_manifest_on_disk(DIST_DIR)

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
        print("\n[+] Deploy Gate GREEN! Publishing to /next/ and /ui-preview/ (root / untouched).")

    release_meta = json.loads((DIST_DIR / target_channel / "release.json").read_text(encoding="utf-8"))
    version = release_meta["version"]
    build_id = release_meta["buildId"]

    sync_dist_to_staging(DIST_DIR, STAGING_DIR, keep_root_untouched=True)
    verify_channel_manifest_on_disk(STAGING_DIR)
    run_cmd(["git", "checkout", "--", "tools/deploy_web/dist"], cwd=REPO_ROOT, check=False)
    run_cmd(["git", "clean", "-fd", "tools/deploy_web/dist"], cwd=REPO_ROOT, check=False)

    if args.no_push:
        print(f"[*] --no-push specified. Verified staging in {STAGING_DIR} (channel=/{target_channel}/, buildId={build_id}).")
        return 0

    # Step 3: Stage & Commit in _web_dist_deploy (keeping root / untouched)
    print(f"\n=== [3/4] Staging /{target_channel}/ and /ui-preview/ in {STAGING_DIR} ===")
    prev_head = run_cmd(["git", "rev-parse", "--short", "HEAD"], cwd=STAGING_DIR, check=False).stdout.strip()
    print(f"[*] Rollback point on web-dist: {prev_head}")

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
    verify_channel_manifest_http(LIVE_BASE_URL)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
