#!/usr/bin/env python3
"""
pack_hybrid.py — R2 Channel Packager for HIT ME Hybrid Web UI + Unity WebGL Release.
Channels:
  /             Stable channel (preserved unchanged unless --promote is explicitly passed)
  /next/        Candidate hybrid channel (Web UI from prep/codex + latest Unity WebGL Release)
  /ui-preview/  Web UI preview channel (web-ui/dist from prep/codex)
  /rc/          Unverified candidate channel (used only if deploy gate fails, with visible RC warning badge)
"""

from __future__ import annotations

import argparse
import hashlib
import io
import json
import os
import shutil
import subprocess
import sys
import tarfile
from datetime import datetime, timezone
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
DEPLOY_WEB_DIR = REPO_ROOT / "tools" / "deploy_web"
CATALOG_SRC_DIR = DEPLOY_WEB_DIR / "catalog"
HOME_BG_WEB_DIR = DEPLOY_WEB_DIR / "web_assets" / "bg_home"
DIST_DIR = DEPLOY_WEB_DIR / "dist"
BUILDS_DIR = REPO_ROOT / "Builds"
HOME_BG_DIR = REPO_ROOT / "Assets" / "Art" / "UI" / "HomeBg"
STAGING_DEPLOY_DIR = REPO_ROOT.parent / "_web_dist_deploy"
TEMP_EXTRACT_DIR = REPO_ROOT.parent / "_codex_extract"


HOME_VISUAL_PATCH_HTML = """
  <link rel="preload" as="image" href="./assets/bg_home_1170.webp">
  <link rel="preload" as="image" href="./assets/nam_lang_portrait.png">
  <link rel="preload" as="image" href="./assets/nu_nang_dong_portrait.png">
  <link rel="preload" as="image" href="./assets/co_ba_portrait.png">
  <link rel="preload" as="image" href="./assets/anh_teen_portrait.png">
  <style id="hitme-home-bg-patch">
    /* B-2..B-5: Village Courtyard Home Background (u4_background.png) + Single Baked Lotus Pedestal */
    .home .backdrop {
      background-color: #d89658 !important;
      background-image: url('./assets/bg_home_1170.webp'), url('./assets/bg_home_lqip.webp') !important;
      background-image: -webkit-image-set(
        url('./assets/bg_home_750.webp') 1x,
        url('./assets/bg_home_1170.webp') 2x,
        url('./assets/bg_home_1536.webp') 3x
      ), url('./assets/bg_home_lqip.webp') !important;
      background-image: image-set(
        url('./assets/bg_home_750.webp') 1x,
        url('./assets/bg_home_1170.webp') 2x,
        url('./assets/bg_home_1536.webp') 3x
      ), url('./assets/bg_home_lqip.webp') !important;
      background-position: 50% 48% !important;
      background-size: cover !important;
      background-repeat: no-repeat !important;
    }
    .home .backdrop:before {
      content: "" !important;
      position: absolute !important;
      inset: 0 !important;
      height: 100% !important;
      background: linear-gradient(180deg, rgba(8, 15, 26, 0.45) 0%, rgba(8, 15, 26, 0.14) 14%, rgba(0, 0, 0, 0) 26%) !important;
      -webkit-mask-image: none !important;
      mask-image: none !important;
      pointer-events: none !important;
    }
    .home .backdrop:after {
      content: "" !important;
      position: absolute !important;
      inset: 0 !important;
      height: 100% !important;
      background: linear-gradient(180deg, rgba(0, 0, 0, 0) 53%, rgba(10, 18, 30, 0.25) 78%, rgba(8, 14, 24, 0.60) 100%) !important;
      -webkit-mask-image: none !important;
      mask-image: none !important;
      pointer-events: none !important;
    }
    /* Remove duplicate CSS-drawn lotus pedestal and bottom arena strip */
    .home .pedestal,
    .home .pedestal:before,
    .home .home-bottom-art {
      display: none !important;
    }
    /* Ground character feet onto the baked stone lotus pedestal at (fx=0.5, fy=0.4844) */
    .home .hero {
      top: calc(var(--pedestal-feet-y, 48.44%) - var(--hero-stand-h, 28%)) !important;
      height: var(--hero-stand-h, 28%) !important;
      left: 28% !important;
      width: 44% !important;
      object-fit: contain !important;
      object-position: center bottom !important;
      filter: drop-shadow(0 7px 6px rgba(28, 15, 6, 0.58)) !important;
    }
    /* Clean framing for character head avatars and weapon icons */
    .home .avatar {
      background: radial-gradient(circle at 50% 42%, #1e3a5f 0%, #0d1b2a 100%) !important;
      display: grid !important;
      place-items: center !important;
      padding: 2px !important;
    }
    .home .avatar img {
      width: 100% !important;
      height: 100% !important;
      object-fit: contain !important;
      object-position: center center !important;
    }
    .selection-card img,
    .choice-grid img,
    .live-card img,
    .live-rank img,
    .preview-character img {
      object-fit: contain !important;
      filter: drop-shadow(0 2px 3px rgba(0, 0, 0, 0.45));
    }
    @media (max-height: 660px) {
      .home .logo { top: 9.5% !important; height: 11.5% !important; }
      .home .selection-card { top: 53.5% !important; height: 8.2% !important; min-height: 44px !important; }
      .home .quick { top: 62.8% !important; height: 9.2% !important; min-height: 48px !important; }
      .home .blue, .home .green { top: 73.2% !important; height: 8.0% !important; min-height: 44px !important; }
      .home .practice { top: 82.2% !important; height: 7.4% !important; min-height: 44px !important; }
    }
  </style>
  <script>
    (function() {
      // Preload character portraits, heads, and weapon icons so switching is instant
      const preloadAssets = [
        "./assets/bg_home_750.webp",
        "./assets/bg_home_1170.webp",
        "./assets/nam_lang_portrait.png",
        "./assets/nu_nang_dong_portrait.png",
        "./assets/co_ba_portrait.png",
        "./assets/anh_teen_portrait.png",
        "./assets/nam_lang_head.png",
        "./assets/nu_nang_dong_head.png",
        "./assets/co_ba_head.png",
        "./assets/anh_teen_head.png",
        "./assets/quay_boy_head.png",
        "./assets/cho_vang_head.png",
        "./assets/meo_mun_head.png",
        "./assets/dep_to_ong_icon.png",
        "./assets/chao_chong_dinh_icon.png",
        "./assets/vot_pickleball_icon.png"
      ];
      preloadAssets.forEach(src => { const img = new Image(); img.src = src; });

      // Dynamic pedestal anchor alignment (B-4 formula from bg_home_meta.json)
      function syncPedestalAnchor() {
        const vv = window.visualViewport;
        const vw = vv ? vv.width : window.innerWidth;
        const vh = vv ? vv.height : window.innerHeight;
        const stageW = Math.min(vw, 430);
        const stageH = vh;
        const origW = 941, origH = 1672;
        const fy = 0.4844, posY = 0.48;
        const scale = Math.max(stageW / origW, stageH / origH);
        const renderH = origH * scale;
        const dy = (stageH - renderH) * posY;
        const feetY = Math.round(dy + fy * renderH);
        const heroH = Math.round(Math.max(110, Math.min(220, stageH * 0.278)));
        document.documentElement.style.setProperty("--pedestal-feet-y", feetY + "px");
        document.documentElement.style.setProperty("--hero-stand-h", heroH + "px");
      }
      window.addEventListener("resize", syncPedestalAnchor);
      if (window.visualViewport) {
        window.visualViewport.addEventListener("resize", syncPedestalAnchor);
        window.visualViewport.addEventListener("scroll", syncPedestalAnchor);
      }
      syncPedestalAnchor();
    })();
  </script>
"""


def git_rev_short(ref: str, cwd: Path = REPO_ROOT) -> str:
    res = subprocess.run(["git", "rev-parse", "--short=7", ref], cwd=cwd, capture_output=True, text=True, check=True)
    return res.stdout.strip()


def find_latest_release_build() -> Path:
    candidates = []
    if BUILDS_DIR.exists():
        for d in BUILDS_DIR.iterdir():
            if d.is_dir() and d.name.startswith("WebGL_Release_"):
                candidates.append(d)
    if not candidates:
        raise FileNotFoundError(f"No WebGL_Release_* builds found in {BUILDS_DIR}")
    candidates.sort(key=lambda p: p.stat().st_mtime, reverse=True)
    return candidates[0]


def extract_from_codex(ref: str, paths: list[str], dest_root: Path) -> None:
    if dest_root.exists():
        shutil.rmtree(dest_root)
    dest_root.mkdir(parents=True, exist_ok=True)
    cmd = ["git", "archive", "--format=tar", ref] + paths
    res = subprocess.run(cmd, cwd=REPO_ROOT, capture_output=True, check=True)
    with tarfile.open(fileobj=io.BytesIO(res.stdout)) as tf:
        tf.extractall(path=dest_root)


def preserve_stable_root() -> None:
    """Ensures / (root index.html, config.json, version.json, and Build/) matches the deployed stable root."""
    DIST_DIR.mkdir(parents=True, exist_ok=True)
    if STAGING_DEPLOY_DIR.exists() and (STAGING_DEPLOY_DIR / "index.html").exists():
        for name in ("index.html", "config.json", "version.json"):
            src = STAGING_DEPLOY_DIR / name
            if src.exists():
                shutil.copy2(src, DIST_DIR / name)
        src_build = STAGING_DEPLOY_DIR / "Build"
        dst_build = DIST_DIR / "Build"
        if src_build.exists():
            if dst_build.exists():
                shutil.rmtree(dst_build)
            shutil.copytree(src_build, dst_build)

    # Write release.json for stable root / without changing root index.html or Build/
    root_ver = "0.1.0-beta.1"
    root_build = "1f1cdfa"
    if (DIST_DIR / "version.json").exists():
        try:
            vdata = json.loads((DIST_DIR / "version.json").read_text(encoding="utf-8"))
            root_ver = vdata.get("version", root_ver)
            root_build = vdata.get("build", root_build)
        except Exception:
            pass
    root_release = {
        "version": root_ver,
        "buildId": root_build,
        "createdAt": "2026-11-05T02:10:37Z",
        "unityCommit": root_build,
        "webUiCommit": "none",
        "catalogVersion": 1,
        "gate": {"passed": True, "failedIds": []},
        "notes": "Stable root channel (preserved unchanged)"
    }
    (DIST_DIR / "release.json").write_text(json.dumps(root_release, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def sync_visual_assets(assets_dir: Path) -> None:
    """Copies cleaned catalog character/weapon PNGs and bg_home_* WebP/JSON files into assets_dir."""
    assets_dir.mkdir(parents=True, exist_ok=True)
    for bg_dir in (HOME_BG_WEB_DIR, HOME_BG_DIR):
        if bg_dir.exists():
            for f in bg_dir.iterdir():
                if f.is_file() and not f.name.endswith(".meta") and (f.name.startswith("bg_home_") or f.name == "bg_home_meta.json"):
                    shutil.copy2(f, assets_dir / f.name)
    if CATALOG_SRC_DIR.exists():
        for f in CATALOG_SRC_DIR.iterdir():
            if f.is_file() and f.suffix.lower() == ".png":
                shutil.copy2(f, assets_dir / f.name)


def write_release_metadata(channel_dir: Path, version: str, build_id: str, unity_commit: str, webui_commit: str,
                           catalog_version: int, gate_passed: bool, failed_ids: list[str], notes: str) -> None:
    now_iso = datetime.now(timezone.utc).isoformat()
    release_data = {
        "version": version,
        "buildId": build_id,
        "createdAt": now_iso,
        "unityCommit": unity_commit,
        "webUiCommit": webui_commit,
        "catalogVersion": catalog_version,
        "gate": {
            "passed": gate_passed,
            "failedIds": failed_ids
        },
        "notes": notes
    }
    (channel_dir / "release.json").write_text(json.dumps(release_data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    version_data = {
        "version": version,
        "build": build_id,
        "unityCommit": unity_commit,
        "webUiCommit": webui_commit,
        "timestamp": now_iso
    }
    (channel_dir / "version.json").write_text(json.dumps(version_data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def generate_hybrid_index_html(channel_dir: Path, build_rel_url: str, build_files: dict[str, str],
                               version: str, build_id: str, is_rc: bool, failed_ids: list[str]) -> None:
    rc_banner = ""
    if is_rc:
        failed_str = ", ".join(failed_ids) if failed_ids else "chưa qua cổng"
        rc_banner = f"""
  <div id="rc-warning-badge" data-name="rc-badge" style="position:fixed;top:0;left:0;right:0;z-index:99999;background:#b91c1c;color:#fff8e8;font-size:12px;font-weight:800;text-align:center;padding:4px 8px;pointer-events:none;">
    RC - chưa qua kiểm thử ({failed_str})
  </div>"""

    html = f"""<!DOCTYPE html>
<html lang="vi">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, height=device-height, initial-scale=1.0, maximum-scale=1.0, user-scalable=no, viewport-fit=cover">
  <meta name="theme-color" content="#101b28">
  <title>HIT ME — Web UI + Unity Hybrid ({version})</title>
  <link rel="stylesheet" href="./style.css">{HOME_VISUAL_PATCH_HTML}
  <style>
    * {{ box-sizing: border-box; }}
    html, body {{
      width: 100%;
      height: 100%;
      height: 100dvh;
      margin: 0;
      padding: 0;
      overflow: hidden;
      background: #101b28;
      display: flex;
      justify-content: center;
      align-items: center;
      touch-action: none;
      user-select: none;
      -webkit-user-select: none;
      -webkit-touch-callout: none;
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
    }}
    #unity-container {{
      position: fixed;
      inset: 0;
      width: 100vw;
      height: 100vh;
      height: 100dvh;
      margin: 0;
      padding: 0;
      overflow: hidden;
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 1;
    }}
    #unity-canvas {{
      width: 100% !important;
      height: 100% !important;
      outline: none;
      display: block;
      touch-action: none;
    }}
    #ui-root {{
      position: fixed;
      inset: 0;
      width: 100%;
      height: 100%;
      z-index: 20;
      pointer-events: none;
    }}
    #ui-root > * {{
      pointer-events: auto;
    }}
    #ui-root [data-screen="match"] {{
      pointer-events: none;
    }}
    #ui-root [data-screen="match"] .live-top,
    #ui-root [data-screen="match"] .live-bottom,
    #ui-root [data-screen="match"] .live-tutorial,
    #ui-root [data-screen="match"] .modal-shade,
    #ui-root [data-screen="match"] .modal,
    #ui-root [data-screen="match"] button {{
      pointer-events: auto;
    }}
    #unity-loading {{
      pointer-events: none;
      position: fixed;
      inset: 0;
      display: flex;
      flex-direction: column;
      justify-content: center;
      align-items: center;
      background: radial-gradient(circle at center, #141b2d 0%, #060709 100%);
      color: #ffe882;
      z-index: 100;
      padding: 24px;
      text-align: center;
    }}
    .loading-brand {{
      font-size: 32px;
      font-weight: 900;
      letter-spacing: 2px;
      color: #f5c518;
      margin-bottom: 6px;
      text-shadow: 0 2px 10px rgba(245,197,24,0.3);
    }}
    .loading-tagline {{
      font-size: 14px;
      color: #b0c4de;
      margin-bottom: 28px;
      letter-spacing: 1px;
    }}
    .progress-container {{
      width: 240px;
      max-width: 80vw;
      height: 10px;
      background: #1a2234;
      border-radius: 6px;
      overflow: hidden;
      border: 1px solid rgba(255,255,255,0.1);
      position: relative;
    }}
    .progress-fill {{
      width: 0%;
      height: 100%;
      background: linear-gradient(90deg, #ff9800, #ffeb3b);
      transition: width 0.15s ease-out;
    }}
    .loading-status {{
      margin-top: 14px;
      font-size: 13px;
      color: #90caf9;
      letter-spacing: 0.5px;
    }}
  </style>
</head>
<body>{rc_banner}
  <div id="unity-container">
    <canvas id="unity-canvas" aria-label="Sân chơi HIT ME"></canvas>
  </div>
  <div id="ui-root"></div>

  <div id="unity-loading">
    <div class="loading-brand">HIT ME</div>
    <div class="loading-tagline">Đang nạp sân chơi &amp; Web UI...</div>
    <div class="progress-container">
      <div class="progress-fill" id="progress-fill"></div>
    </div>
    <div class="loading-status" id="loading-status">Đang tải dữ liệu trò chơi… 0%</div>
  </div>

  <script>
    window.__hitmeConfig = {{ ui: "web" }};
    window.__hitmeReleaseInfo = {{ version: "{version}", buildId: "{build_id}" }};

    // Deduplicate 10 Hz Unity match ticks so app.js innerHTML does not detach DOM buttons mid-click
    (function() {{
      let _realHitmeUi = null;
      let _lastMatchSig = "";
      let _tutorialHoldUntil = 0;
      document.addEventListener("click", function(e) {{
        const btn = e.target && e.target.closest ? e.target.closest("[data-action]") : null;
        const act = btn ? btn.dataset.action : "";
        if (act === "tutorial-reopen") _tutorialHoldUntil = Date.now() + 8000;
        else if (act === "tutorial-skip") _tutorialHoldUntil = 0;
      }}, true);
      Object.defineProperty(window, "hitmeUi", {{
        configurable: true,
        enumerable: true,
        get() {{ return _realHitmeUi; }},
        set(val) {{
          if (val && typeof val.onUnity === "function") {{
            const origOnUnity = val.onUnity.bind(val);
            val.onUnity = function(event) {{
              let evt = event;
              try {{ if (typeof event === "string") evt = JSON.parse(event); }} catch (e) {{}}
              if (evt && typeof evt === "object") {{
                if (evt.type === "screen" || evt.type === "result") {{
                  _lastMatchSig = "";
                  _tutorialHoldUntil = 0;
                }} else if (evt.type === "match") {{
                  if (Date.now() < _tutorialHoldUntil && evt.phase !== "placing") {{
                    evt = Object.assign({{}}, evt, {{ phase: "placing", timeLeft: Math.max(2, Number(evt.timeLeft) || 2) }});
                    event = evt;
                  }}
                  const playersSig = Array.isArray(evt.players)
                    ? evt.players.map(p => `${{p.id}}:${{p.characterId}}:${{p.hp}}:${{p.alive}}`).join(",")
                    : "";
                  const sig = `${{evt.round}}|${{evt.phase}}|${{Math.ceil(Number(evt.timeLeft) || 0)}}|${{evt.toast || ""}}|${{playersSig}}`;
                  if (sig === _lastMatchSig) return;
                  _lastMatchSig = sig;
                }}
              }}
              return origOnUnity(event);
            }};
          }}
          _realHitmeUi = val;
        }}
      }});
    }})();

    // Inject version + short buildId into Web Settings sheet (R2) without altering fingerprinted app.js
    const uiRootEl = document.getElementById("ui-root");
    if (uiRootEl && typeof MutationObserver !== "undefined") {{
      const obs = new MutationObserver(() => {{
        const sheet = uiRootEl.querySelector(".settings-sheet");
        if (sheet && !sheet.querySelector(".release-version-badge")) {{
          const badge = document.createElement("small");
          badge.className = "release-version-badge";
          badge.dataset.name = "release-version";
          badge.style.cssText = "display:block;margin-top:10px;opacity:0.8;font-size:12px;text-align:center;color:#cbd5e1;";
          badge.textContent = "Phiên bản: {version} ({build_id})";
          sheet.appendChild(badge);
        }}
      }});
      obs.observe(uiRootEl, {{ childList: true, subtree: true }});
    }}

    const canvas = document.querySelector("#unity-canvas");
    const loading = document.querySelector("#unity-loading");
    const progressFill = document.querySelector("#progress-fill");
    const loadingStatus = document.querySelector("#loading-status");

    function updateCanvasResolution() {{
      const vv = window.visualViewport;
      const cssW = vv ? Math.round(vv.width) : window.innerWidth;
      const cssH = vv ? Math.round(vv.height) : window.innerHeight;
      const dpr = Math.min(window.devicePixelRatio || 1, 2.5);

      canvas.style.width = cssW + "px";
      canvas.style.height = cssH + "px";
      canvas.width = Math.round(cssW * dpr);
      canvas.height = Math.round(cssH * dpr);

      const container = document.querySelector("#unity-container");
      if (container && vv) {{
        container.style.width = cssW + "px";
        container.style.height = cssH + "px";
        container.style.top = (vv.offsetTop || 0) + "px";
        container.style.left = (vv.offsetLeft || 0) + "px";
      }}
    }}

    if (window.visualViewport) {{
      window.visualViewport.addEventListener("resize", updateCanvasResolution);
      window.visualViewport.addEventListener("scroll", updateCanvasResolution);
    }}
    window.addEventListener("resize", updateCanvasResolution);
    window.addEventListener("orientationchange", updateCanvasResolution);
    updateCanvasResolution();

    const buildUrl = "{build_rel_url}";
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get("e2e") === "1" && typeof WebAssembly !== "undefined" && WebAssembly.Memory) {{
      const OrigMem = WebAssembly.Memory;
      WebAssembly.Memory = function(desc) {{
        const mem = new OrigMem(desc);
        window.__hitmeWasmMemory = mem;
        return mem;
      }};
      WebAssembly.Memory.prototype = OrigMem.prototype;
    }}
    const buildVersion = urlParams.get("v") || "{build_id}";
    const versioned = path => path + "?v=" + encodeURIComponent(buildVersion);

    const config = {{
      arguments: [],
      dataUrl: versioned(buildUrl + "/{build_files['data']}"),
      frameworkUrl: versioned(buildUrl + "/{build_files['framework']}"),
      codeUrl: versioned(buildUrl + "/{build_files['wasm']}"),
      streamingAssetsUrl: "StreamingAssets",
      companyName: "HitMeStudio",
      productName: "HIT ME",
      productVersion: "{version}",
      matchWebGLToCanvasSize: false,
      devicePixelRatio: Math.min(window.devicePixelRatio || 1, 2.5)
    }};

    const script = document.createElement("script");
    script.src = versioned(buildUrl + "/{build_files['loader']}");
    script.onload = () => {{
      createUnityInstance(canvas, config, (progress) => {{
        const pct = Math.round(progress * 100);
        if (progressFill) progressFill.style.width = pct + "%";
        if (loadingStatus) loadingStatus.textContent = `Đang tải dữ liệu trò chơi… ${{pct}}%`;
      }}).then(instance => {{
        window.unityInstance = instance;
        if (loading) loading.style.display = "none";
        canvas.focus();
        if (urlParams.get("autostart") === "practice") {{
          const autoChar = urlParams.get("char") || "nam_lang";
          const autoWeapon = urlParams.get("weapon") || "dep_to_ong";
          const autoBots = parseInt(urlParams.get("bots") || "3", 10) || 3;
          setTimeout(() => {{
            instance.SendMessage("WebBridge", "StartPractice", JSON.stringify({{
              characterId: autoChar,
              weaponId: autoWeapon === "dep" ? "dep_to_ong" : autoWeapon,
              botCount: autoBots
            }}));
          }}, 120);
        }}
      }}).catch(error => {{
        console.error("[Hybrid] Unity WebGL load error:", error);
        if (loadingStatus) loadingStatus.textContent = "Lỗi nạp trò chơi: " + error;
      }});
    }};
    document.body.appendChild(script);
  </script>
  <script type="module" src="./app.js"></script>
</body>
</html>
"""
    (channel_dir / "index.html").write_text(html, encoding="utf-8")


def promote_next_to_root() -> None:
    next_dir = DIST_DIR / "next"
    if not next_dir.exists() or not (next_dir / "index.html").exists():
        raise FileNotFoundError("Cannot promote: dist/next/ does not exist")
    prev_dir = DIST_DIR / "previous"
    if prev_dir.exists():
        shutil.rmtree(prev_dir)
    prev_dir.mkdir(parents=True, exist_ok=True)
    for name in ("index.html", "version.json", "release.json", "config.json"):
        if (DIST_DIR / name).exists():
            shutil.copy2(DIST_DIR / name, prev_dir / name)
    if (DIST_DIR / "Build").exists():
        shutil.copytree(DIST_DIR / "Build", prev_dir / "Build")
    print(f"[+] Backed up current root / to {prev_dir}")

    # Copy next/Build to root Build/ and next files to root
    if (next_dir / "Build").exists():
        if (DIST_DIR / "Build").exists():
            shutil.rmtree(DIST_DIR / "Build")
        shutil.copytree(next_dir / "Build", DIST_DIR / "Build")
    for item in next_dir.iterdir():
        if item.name == "Build":
            continue
        dst = DIST_DIR / item.name
        if item.is_dir():
            if dst.exists():
                shutil.rmtree(dst)
            shutil.copytree(item, dst)
        else:
            shutil.copy2(item, dst)
    print("[+] Promoted /next/ to root /")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--channel", choices=["next", "rc"], default="next")
    parser.add_argument("--codex-ref", default="origin/prep/codex")
    parser.add_argument("--gate-passed", action="store_true", default=True)
    parser.add_argument("--gate-failed-ids", default="")
    parser.add_argument("--promote", action="store_true")
    parser.add_argument("--notes", default="Hybrid Web UI (prep/codex) + Unity WebGL Release (feature/ui-ux-production)")
    args = parser.parse_args()

    if args.promote:
        promote_next_to_root()
        return 0

    failed_ids = [x.strip() for x in args.gate_failed_ids.split(",") if x.strip()]
    gate_passed = (len(failed_ids) == 0) and (args.channel == "next")

    version_file = DEPLOY_WEB_DIR / "VERSION"
    version = version_file.read_text(encoding="utf-8").strip() if version_file.exists() else "0.1.0-beta.3"
    unity_commit = git_rev_short("HEAD")
    webui_commit = git_rev_short(args.codex_ref)
    build_id = f"{unity_commit}-{webui_commit}"

    print(f"[*] Packaging version={version}, buildId={build_id}, channel=/{args.channel}/")

    # 1. Preserve root / unchanged
    preserve_stable_root()

    # 2. Extract web-ui/dist and tools from prep/codex into temporary folder _codex_extract
    extract_from_codex(
        args.codex_ref,
        ["web-ui", "tools/blackbox", "tools/ui", "tools/webui_assets", "tools/supabase"],
        TEMP_EXTRACT_DIR
    )
    codex_webui_dist = TEMP_EXTRACT_DIR / "web-ui" / "dist"
    if not (codex_webui_dist / "app.js").exists():
        raise FileNotFoundError(f"Missing app.js in extracted {codex_webui_dist}")

    # Sync cleaned catalog avatars/portraits/icons into extracted web-ui/dist/assets
    sync_visual_assets(codex_webui_dist / "assets")
    sync_visual_assets(TEMP_EXTRACT_DIR / "web-ui" / "assets")

    catalog_data = json.loads((codex_webui_dist / "catalog.json").read_text(encoding="utf-8"))
    catalog_version = int(catalog_data.get("version", 1))

    # 3. Populate /ui-preview/
    ui_preview_dir = DIST_DIR / "ui-preview"
    if ui_preview_dir.exists():
        shutil.rmtree(ui_preview_dir)
    shutil.copytree(codex_webui_dist, ui_preview_dir)
    ui_prev_index = ui_preview_dir / "index.html"
    if ui_prev_index.exists():
        uhtml = ui_prev_index.read_text(encoding="utf-8")
        uhtml = uhtml.replace('<link rel="stylesheet" href="./style.css">', '<link rel="stylesheet" href="./style.css">' + HOME_VISUAL_PATCH_HTML)
        mock_boot = '<script>if(!new URLSearchParams(location.search).has("mock")){const u=new URL(location.href);u.searchParams.set("mock","1");history.replaceState(null,"",u);}</script>\n  <script type="module" src="./app.js"></script>'
        uhtml = uhtml.replace('<script type="module" src="./app.js"></script>', mock_boot)
        ui_prev_index.write_text(uhtml, encoding="utf-8")
    sync_visual_assets(ui_preview_dir / "assets")
    write_release_metadata(
        ui_preview_dir, version, build_id, unity_commit, webui_commit,
        catalog_version, gate_passed, failed_ids, args.notes
    )
    print(f"[+] Packaged /ui-preview/ ({ui_preview_dir})")

    # 4. Populate target channel (/next/ or /rc/)
    channel_dir = DIST_DIR / args.channel
    if channel_dir.exists():
        shutil.rmtree(channel_dir)
    shutil.copytree(codex_webui_dist, channel_dir)
    sync_visual_assets(channel_dir / "assets")

    # Copy latest Unity WebGL Release build into shared DIST_DIR/Build/ (preserving root / files alongside it)
    latest_build = find_latest_release_build()
    src_build = latest_build / "Build"
    shared_build = DIST_DIR / "Build"
    shared_build.mkdir(parents=True, exist_ok=True)
    shutil.copytree(src_build, shared_build, dirs_exist_ok=True)

    build_files = {}
    for f in src_build.iterdir():
        if f.name.endswith(".data.br"):
            build_files["data"] = f.name
        elif f.name.endswith(".framework.js.br"):
            build_files["framework"] = f.name
        elif f.name.endswith(".wasm.br"):
            build_files["wasm"] = f.name
        elif f.name.endswith(".loader.js"):
            build_files["loader"] = f.name

    if len(build_files) != 4:
        raise RuntimeError(f"Expected 4 WebGL build files in {src_build}, found: {build_files}")

    generate_hybrid_index_html(
        channel_dir, "../Build", build_files, version, build_id,
        is_rc=(args.channel == "rc"), failed_ids=failed_ids
    )
    write_release_metadata(
        channel_dir, version, build_id, unity_commit, webui_commit,
        catalog_version, gate_passed, failed_ids, args.notes
    )

    # 5. Copy vercel.json to DIST_DIR
    shutil.copy2(DEPLOY_WEB_DIR / "vercel.json", DIST_DIR / "vercel.json")

    print(f"[SUCCESS] Packaged /{args.channel}/ and /ui-preview/ with buildId={build_id} (root / preserved unchanged)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
