#!/usr/bin/env python3
"""
package_deploy.py — Packages WebGL Release build for Vercel deployment.
Produces a clean static bundle in Tools/deploy_web/dist/ with vercel.json and runtime config.
"""

import os
import sys
import shutil
import re
import hashlib
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent.parent
BUILDS_DIR = REPO_ROOT / "Builds"
DEPLOY_DIR = REPO_ROOT / "Tools" / "deploy_web"
DIST_DIR = DEPLOY_DIR / "dist"

def find_latest_release_build():
    candidates = []
    if BUILDS_DIR.exists():
        for d in BUILDS_DIR.iterdir():
            if d.is_dir() and d.name.startswith("WebGL_Release_"):
                candidates.append(d)
    if not candidates:
        raise FileNotFoundError(f"No WebGL_Release_* builds found in {BUILDS_DIR}")
    candidates.sort(key=lambda p: p.stat().st_mtime, reverse=True)
    return candidates[0]

def file_sha256(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        while chunk := f.read(65536):
            h.update(chunk)
    return h.hexdigest()

def package():
    build_src = find_latest_release_build()
    print(f"[*] Packaging from source build: {build_src.name}")

    if DIST_DIR.exists():
        dst_build_dir = DIST_DIR / "Build"
        if dst_build_dir.exists():
            for f in dst_build_dir.iterdir():
                try:
                    f.unlink()
                except Exception:
                    pass
    DIST_DIR.mkdir(parents=True, exist_ok=True)

    # 1. Copy Build directory
    src_build_dir = build_src / "Build"
    dst_build_dir = DIST_DIR / "Build"
    if not src_build_dir.exists():
        raise FileNotFoundError(f"Build folder not found in {build_src}")
    
    shutil.copytree(src_build_dir, dst_build_dir, dirs_exist_ok=True)
    print(f"[+] Copied Build/ directory ({len(list(dst_build_dir.iterdir()))} files)")

    # 2. Identify build files
    data_file = None
    framework_file = None
    code_file = None
    loader_file = None

    for f in dst_build_dir.iterdir():
        if f.name.endswith(".data.br"):
            data_file = f.name
        elif f.name.endswith(".framework.js.br"):
            framework_file = f.name
        elif f.name.endswith(".wasm.br"):
            code_file = f.name
        elif f.name.endswith(".loader.js"):
            loader_file = f.name

    print(f"    - Data:      {data_file}")
    print(f"    - Framework: {framework_file}")
    print(f"    - Wasm:      {code_file}")
    print(f"    - Loader:    {loader_file}")

    if not all([data_file, framework_file, code_file, loader_file]):
        raise RuntimeError("Missing one or more required Brotli build assets!")

    # 3. Copy & Adapt index.html
    template_index = DEPLOY_DIR / "index.html"
    dst_index = DIST_DIR / "index.html"
    with open(template_index, "r", encoding="utf-8") as f:
        html = f.read()

    # Replace build file references if needed
    html = re.sub(r'WebGL_Release_[^"]+\.data\.br', data_file, html)
    html = re.sub(r'WebGL_Release_[^"]+\.framework\.js\.br', framework_file, html)
    html = re.sub(r'WebGL_Release_[^"]+\.wasm\.br', code_file, html)
    html = re.sub(r'WebGL_Release_[^"]+\.loader\.js', loader_file, html)

    with open(dst_index, "w", encoding="utf-8") as f:
        f.write(html)
    print(f"[+] Wrote customized index.html")

    # 4. Copy vercel.json & config.json
    shutil.copy(DEPLOY_DIR / "vercel.json", DIST_DIR / "vercel.json")
    shutil.copy(DEPLOY_DIR / "config.json", DIST_DIR / "config.json")
    print(f"[+] Copied vercel.json and config.json")

    # 5. Integrity verification & manifest
    print("\n--- DIST MANIFEST ---")
    total_size = 0
    for root, _, files in os.walk(DIST_DIR):
        for f in sorted(files):
            fp = Path(root) / f
            rel = fp.relative_to(DIST_DIR)
            sz = fp.stat().st_size
            total_size += sz
            h = file_sha256(fp)[:12]
            print(f"  {str(rel):<40} {sz:>10,} bytes  sha256:{h}")
    print(f"Total bundle size: {total_size / (1024*1024):.2f} MB")
    print(f"[SUCCESS] Packaged static site ready for Vercel in: {DIST_DIR}")

if __name__ == "__main__":
    package()
