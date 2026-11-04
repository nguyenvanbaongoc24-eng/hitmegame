import re
from pathlib import Path

dist_dir = Path(r"d:\HITME GAME\unity-client\tools\deploy_web\dist")
build_dir = dist_dir / "Build"

# Find build files
loader_file = [f.name for f in build_dir.iterdir() if f.name.endswith(".loader.js")][0]
data_file = [f.name for f in build_dir.iterdir() if f.name.endswith(".data.br")][0]
framework_file = [f.name for f in build_dir.iterdir() if f.name.endswith(".framework.js.br")][0]
wasm_file = [f.name for f in build_dir.iterdir() if f.name.endswith(".wasm.br")][0]

html = f"""<!DOCTYPE html>
<html lang="vi">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, height=device-height, initial-scale=1.0, maximum-scale=1.0, user-scalable=no, viewport-fit=cover">
  <meta name="theme-color" content="#101b28">
  <title>HIT ME — Web UI + Unity Hybrid</title>
  <link rel="stylesheet" href="./style.css">
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
      z-index: 10;
      pointer-events: none;
    }}
    #ui-root > * {{
      pointer-events: auto;
    }}
    #ui-root.in-gameplay {{
      pointer-events: none;
    }}
    #ui-root.in-gameplay .hud-top,
    #ui-root.in-gameplay .hud-bottom,
    #ui-root.in-gameplay header,
    #ui-root.in-gameplay nav,
    #ui-root.in-gameplay .modal,
    #ui-root.in-gameplay button {{
      pointer-events: auto;
    }}
    #unity-loading {{
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
<body>
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
    // Hybrid Mode Configuration: Web UI handles interface, Unity handles 2.5D gameplay
    window.__hitmeConfig = {{ ui: "web" }};

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

    const buildUrl = "../Build";
    const urlParams = new URLSearchParams(window.location.search);
    const buildVersion = urlParams.get("v") || "1f1cdfa";
    const versioned = path => path + "?v=" + encodeURIComponent(buildVersion);

    const config = {{
      arguments: [],
      dataUrl: versioned(buildUrl + "/{data_file}"),
      frameworkUrl: versioned(buildUrl + "/{framework_file}"),
      codeUrl: versioned(buildUrl + "/{wasm_file}"),
      streamingAssetsUrl: "../StreamingAssets",
      companyName: "HitMeStudio",
      productName: "HIT ME",
      productVersion: "1.0.0",
      matchWebGLToCanvasSize: false,
      devicePixelRatio: Math.min(window.devicePixelRatio || 1, 2.5)
    }};

    const script = document.createElement("script");
    script.src = versioned(buildUrl + "/{loader_file}");
    script.onload = () => {{
      createUnityInstance(canvas, config, (progress) => {{
        const pct = Math.round(progress * 100);
        if (progressFill) progressFill.style.width = pct + "%";
        if (loadingStatus) loadingStatus.textContent = `Đang tải dữ liệu trò chơi… ${{pct}}%`;
      }}).then(instance => {{
        window.unityInstance = instance;
        if (loading) loading.style.display = "none";
        canvas.focus();
        console.log("[Next] Unity WebGL loaded successfully in Web UI mode.");
      }}).catch(error => {{
        console.error("[Next] Unity WebGL load error:", error);
        if (loadingStatus) loadingStatus.textContent = "Lỗi nạp trò chơi: " + error;
      }});
    }};
    document.body.appendChild(script);
  </script>
  <script type="module" src="./app.js"></script>
</body>
</html>
"""

next_index_path = dist_dir / "next" / "index.html"
with open(next_index_path, "w", encoding="utf-8") as f:
    f.write(html)
print(f"Wrote {next_index_path}")
