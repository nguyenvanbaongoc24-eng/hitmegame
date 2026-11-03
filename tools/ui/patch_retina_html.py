import sys
from pathlib import Path

# 1. deploy_web/index.html
path1 = Path('unity-client/tools/deploy_web/index.html')
if path1.exists():
    text1 = path1.read_text(encoding='utf-8')
    text1 = text1.replace('matchWebGLToCanvasSize: true,', 'matchWebGLToCanvasSize: false,')
    path1.write_text(text1, encoding='utf-8')
    print('Updated deploy_web/index.html matchWebGLToCanvasSize to false')

# 2. Phase1Mobile/index.html
path2 = Path('unity-client/Assets/WebGLTemplates/Phase1Mobile/index.html')
if path2.exists():
    text2 = path2.read_text(encoding='utf-8')
    text2 = text2.replace('matchWebGLToCanvasSize: true', 'matchWebGLToCanvasSize: false, devicePixelRatio: Math.min(window.devicePixelRatio || 1, 2.5)')
    
    # Also add canvas resolution updater if missing
    if 'updateCanvasResolution' not in text2:
        snippet = """      function updateCanvasResolution() {
        const dpr = Math.min(window.devicePixelRatio || 1, 2.5);
        canvas.width = Math.round(window.innerWidth * dpr);
        canvas.height = Math.round(window.innerHeight * dpr);
      }
      updateCanvasResolution();
      window.addEventListener("resize", updateCanvasResolution);
"""
        text2 = text2.replace('const script = document.createElement("script");', snippet + '      const script = document.createElement("script");')
    path2.write_text(text2, encoding='utf-8')
    print('Updated Phase1Mobile/index.html for Retina sharpness')
