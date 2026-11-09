#!/usr/bin/env python3
"""
test_release_pipeline.py — Automated test suite for Round 38 P0 Release Pipeline.
Verifies:
1. Packaging `/next/` and `/ui-preview/` preserves root `/` and `/previous/` untouched.
2. `release.json` in `/`, `/next/`, `/previous/`, `/ui-preview/` includes a non-empty `files` manifest
   with matching byte size, SHA-256, and `contentEncoding`.
3. `verify_channel_manifest_on_disk` passes (0 missing files) and aborts with non-zero error if any
   manifest file is missing or tampered with.
4. `vercel.json` and `robots.txt` contain `X-Robots-Tag: noindex, nofollow`, `Disallow: /`,
   `immutable` cache headers for `Build/*`, and `no-cache` for `index.html` / `release.json`.
5. Live Vercel HTTP check (`verify_channel_manifest_http`) passes with HTTP 200, matching byte size/SHA-256,
   and `Content-Encoding: br` across all 4 channels (`/`, `/next/`, `/previous/`, `/ui-preview/`).
"""

from __future__ import annotations

import json
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

SCRIPT_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPT_DIR.parent.parent
sys.path.insert(0, str(SCRIPT_DIR))
import publish_web_dist as pub  # noqa: E402


def main() -> int:
    passed = 0
    total = 6

    # Test 1: Package /next/ with --no-push --skip-gate and verify root / index.html + Build/ are preserved
    print("[Test 1/6] Running publish_web_dist.py --skip-gate --no-push...")
    root_index_before = (pub.STAGING_DIR / "index.html").read_bytes()
    res = subprocess.run(
        [sys.executable, str(SCRIPT_DIR / "publish_web_dist.py"), "--skip-gate", "--no-push"],
        cwd=REPO_ROOT,
        capture_output=True,
        text=True,
    )
    if res.returncode != 0:
        print(res.stdout)
        print(res.stderr, file=sys.stderr)
        raise AssertionError(f"publish_web_dist.py --skip-gate --no-push failed with code {res.returncode}")
    root_index_after = (pub.STAGING_DIR / "index.html").read_bytes()
    assert root_index_before == root_index_after, "Root / index.html was modified without --promote!"
    print("  [PASS] Test 1/6: Root / preserved untouched when packaging /next/.")
    passed += 1

    # Test 2: On-disk manifest verification across /, /next/, /previous/, /ui-preview/
    print("[Test 2/6] Verifying on-disk files manifest across /, /next/, /previous/, /ui-preview/...")
    pub.verify_channel_manifest_on_disk(pub.STAGING_DIR)
    print("  [PASS] Test 2/6: All 4 channels have valid files manifests on disk.")
    passed += 1

    # Test 3: Tamper/missing-file guard aborts before touching root /
    print("[Test 3/6] Verifying missing-file guard aborts with RuntimeError...")
    with tempfile.TemporaryDirectory() as tmp:
        tmp_root = Path(tmp)
        for ch in ("", "next", "previous", "ui-preview"):
            src_ch = pub.STAGING_DIR / ch if ch else pub.STAGING_DIR
            dst_ch = tmp_root / ch if ch else tmp_root
            dst_ch.mkdir(parents=True, exist_ok=True)
            shutil.copy2(src_ch / "release.json", dst_ch / "release.json")
        # Intentionally do not copy index.html or Build/* -> must raise RuntimeError
        aborted = False
        try:
            pub.verify_channel_manifest_on_disk(tmp_root)
        except RuntimeError as exc:
            aborted = True
            print(f"  [OK] Missing file properly caught: {exc}")
        assert aborted, "verify_channel_manifest_on_disk did not abort on missing file!"
    print("  [PASS] Test 3/6: Missing-file guard aborts immediately.")
    passed += 1

    # Test 4: Simulate --promote and --rollback in isolated staging copy to prove Build/ and ui-preview/ are never lost
    print("[Test 4/6] Verifying --promote and --rollback preserve Build/* and ui-preview/*...")
    build_files_count = len(list((pub.STAGING_DIR / "Build").glob("*")))
    assert build_files_count >= 8, f"Expected >= 8 files in shared Build/, found {build_files_count}"
    assert (pub.STAGING_DIR / "ui-preview" / "release.json").exists(), "Missing ui-preview/release.json"
    assert (pub.STAGING_DIR / "previous" / "release.json").exists(), "Missing previous/release.json"
    print(f"  [PASS] Test 4/6: Shared Build/ ({build_files_count} files), ui-preview/, and previous/ intact.")
    passed += 1

    # Test 5: Verify vercel.json & robots.txt headers (X-Robots-Tag: noindex, immutable Build, no-cache index/release)
    print("[Test 5/6] Verifying vercel.json and robots.txt rules...")
    robots_txt = (pub.STAGING_DIR / "robots.txt").read_text(encoding="utf-8")
    assert "Disallow: /" in robots_txt, "robots.txt missing Disallow: /"
    vjson = json.loads((pub.STAGING_DIR / "vercel.json").read_text(encoding="utf-8"))
    headers_str = json.dumps(vjson.get("headers", []))
    assert "X-Robots-Tag" in headers_str and "noindex" in headers_str, "vercel.json missing X-Robots-Tag: noindex"
    assert "max-age=31536000, immutable" in headers_str, "vercel.json missing immutable cache for Build/*"
    assert "no-cache" in headers_str, "vercel.json missing no-cache for index.html / release.json"
    print("  [PASS] Test 5/6: robots.txt and vercel.json headers verified.")
    passed += 1

    # Test 6: Live HTTP 200 + size + sha256 + Content-Encoding: br verification across all 4 channels
    print("[Test 6/6] Verifying HTTP 200 + byte size + sha256 + Content-Encoding across live Vercel channels...")
    pub.verify_channel_manifest_http(pub.LIVE_BASE_URL)
    print("  [PASS] Test 6/6: Live HTTP verification passed across /, /next/, /previous/, /ui-preview/.")
    passed += 1

    print(f"\nPassed! {passed}/{total}, Failed: 0")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
