"""README: python tools/blackbox/build_webui_preview.py

Copy the web UI into a standalone static preview without touching Unity or
the deployed WebGL build. Only the files used by index.html are packaged.
"""
from __future__ import annotations

import shutil
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "web-ui"
DEST = SOURCE / "dist"
FILES = ("index.html", "style.css", "app.js", "prototype.js", "bridge.js",
         "mock-unity.js", "audio.js", "i18n.json", "home_layout.json", "catalog.json")


def main() -> None:
    DEST.mkdir(exist_ok=True)
    for name in FILES:
        shutil.copy2(SOURCE / name, DEST / name)
    shutil.copytree(SOURCE / "assets", DEST / "assets", dirs_exist_ok=True)
    total = sum(1 for path in DEST.rglob("*") if path.is_file())
    size = sum(path.stat().st_size for path in DEST.rglob("*") if path.is_file())
    print(f"web-ui/dist: {total} files, {size} bytes")


if __name__ == "__main__":
    main()
