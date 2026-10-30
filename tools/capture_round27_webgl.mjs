import { spawn } from 'child_process';
import fs from 'fs';
import crypto from 'crypto';
import path from 'path';

const chromePath = 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';
const basePort = 9250;

function sleep(ms) {
  return new Promise(r => setTimeout(r, ms));
}

function sha256File(filePath) {
  const buf = fs.readFileSync(filePath);
  return crypto.createHash('sha256').update(buf).digest('hex').toLowerCase();
}

async function captureSingleScreen({
  url,
  outPngPath,
  viewport = { width: 1170, height: 2532 },
  buildId = 'WebGL_20261030_f53a648',
  port = basePort,
  setupAction = null
}) {
  const dir = path.dirname(outPngPath);
  fs.mkdirSync(dir, { recursive: true });

  const userDataDir = 'C:\\Users\\LENOVO\\.gemini\\antigravity\\brain\\15ef4497-c923-4d70-a855-259c984d1ed2\\scratch\\chrome_r27_' + Date.now() + '_' + Math.random().toString(36).slice(2, 6);
  const chrome = spawn(chromePath, [
    `--remote-debugging-port=${port}`,
    `--user-data-dir=${userDataDir}`,
    '--headless=new',
    `--window-size=${viewport.width},${viewport.height}`,
    '--enable-webgl',
    '--ignore-gpu-blocklist',
    '--mute-audio',
    '--no-first-run',
    '--no-default-browser-check',
    url
  ], { stdio: 'ignore' });

  let ws;
  try {
    let wsUrl = null;
    for (let i = 0; i < 40; i++) {
      await sleep(500);
      try {
        const res = await fetch(`http://127.0.0.1:${port}/json/list`);
        const pages = await res.json();
        const page = pages.find(p => p.type === 'page' && p.url.includes('localhost:8093'));
        if (page && page.webSocketDebuggerUrl) {
          wsUrl = page.webSocketDebuggerUrl;
          break;
        }
      } catch {}
    }

    if (!wsUrl) throw new Error('Could not connect to Chrome DevTools target');

    ws = new WebSocket(wsUrl);
    await new Promise((resolve, reject) => {
      ws.onopen = resolve;
      ws.onerror = reject;
    });

    let msgId = 1;
    const pending = new Map();
    let engineReady = false;

    ws.onmessage = (event) => {
      const data = JSON.parse(event.data);
      if (data.id && pending.has(data.id)) {
        pending.get(data.id)(data);
        pending.delete(data.id);
        return;
      }
      if (data.method === 'Runtime.consoleAPICalled') {
        const text = (data.params.args || []).map(a => a.value ?? a.description ?? '').join(' ');
        if (text.includes('CodeReloadManager') || text.includes('Initialize engine version') || text.includes('[BuildFingerprint]')) {
          engineReady = true;
        }
      }
    };

    const send = (method, params = {}) => {
      const id = msgId++;
      return new Promise((resolve) => {
        pending.set(id, resolve);
        ws.send(JSON.stringify({ id, method, params }));
      });
    };

    await send('Runtime.enable');
    await send('Page.enable');
    await send('Input.enable');
    await send('Emulation.setDeviceMetricsOverride', {
      width: viewport.width,
      height: viewport.height,
      deviceScaleFactor: 1,
      mobile: true
    });

    for (let i = 0; i < 40; i++) {
      await sleep(500);
      if (engineReady) break;
    }
    await sleep(2200);

    if (setupAction) {
      await setupAction(send, viewport);
    }

    const shot = await send('Page.captureScreenshot', { format: 'png', fromSurface: true });
    const buf = Buffer.from(shot.result.data, 'base64');
    fs.writeFileSync(outPngPath, buf);

    const hash = sha256File(outPngPath);
    const rel = outPngPath.replace(/\\/g, '/').replace(/.*unity-client\//, '');
    const meta = {
      "tệp": rel,
      "sha256": hash,
      "commit": buildId.replace('WebGL_20261030_', ''),
      "scene": "Graybox",
      "bản build (id/ngày)": buildId,
      "độ phân giải": `${viewport.width}x${viewport.height}`,
      "lệnh/AutoCapture tạo ra": `node tools/capture_round27_webgl.mjs (${url})`,
      "người/công cụ": "WebGL Playwright Live (Chrome DevTools Protocol)",
      "kích thước": buf.length,
      "giờ ghi": new Date().toISOString(),
      "capture_source": "WebGL Playwright Live",
      "chủ dự án duyệt": "CHƯA"
    };
    fs.writeFileSync(outPngPath + '.json', JSON.stringify(meta, null, 2), 'utf8');
    console.log(`Captured ${rel} (${viewport.width}x${viewport.height}, ${buf.length} bytes, sha256=${hash.slice(0, 10)}...)`);
  } finally {
    try { if (ws) ws.close(); } catch {}
    try { chrome.kill('SIGKILL'); } catch {}
  }
}

async function main() {
  const buildId = 'WebGL_20261030_f53a648';
  console.log(`Starting Round 27 WebGL screenshot capture on build ${buildId}...`);

  const outDir = 'D:\\HITME GAME\\unity-client\\Evidence\\phase3';
  const r27Dir = 'D:\\HITME GAME\\unity-client\\Evidence\\round27';

  // Helper to click canvas normalized coords (0..1)
  const clickNorm = async (send, vp, nx, ny) => {
    const x = Math.round(vp.width * nx);
    const y = Math.round(vp.height * ny);
    await send('Input.dispatchMouseEvent', { type: 'mousePressed', x, y, button: 'left', clickCount: 1 });
    await sleep(60);
    await send('Input.dispatchMouseEvent', { type: 'mouseReleased', x, y, button: 'left', clickCount: 1 });
  };

  // Helper to drag aim on canvas normalized coords
  const dragAim = async (send, vp, startNx, startNy, endNx, endNy) => {
    const sx = Math.round(vp.width * startNx);
    const sy = Math.round(vp.height * startNy);
    const ex = Math.round(vp.width * endNx);
    const ey = Math.round(vp.height * endNy);

    await send('Input.dispatchMouseEvent', { type: 'mousePressed', x: sx, y: sy, button: 'left', clickCount: 1 });
    await sleep(100);
    const steps = 6;
    for (let s = 1; s <= steps; s++) {
      const cx = Math.round(sx + (ex - sx) * (s / steps));
      const cy = Math.round(sy + (ey - sy) * (s / steps));
      await send('Input.dispatchMouseEvent', { type: 'mouseMoved', x: cx, y: cy, button: 'left' });
      await sleep(40);
    }
  };

  // 1. V4: 3 Variants of charScale at 1170x2532 (Placing with aim line & Throwing with bot)
  const scales = [1.6, 2.0, 2.4];
  for (const scale of scales) {
    console.log(`\n--- Capturing charScale=${scale} variant ---`);
    // Placing phase
    await captureSingleScreen({
      url: `http://localhost:8093/index.html?screen=botTraining&charScale=${scale}`,
      outPngPath: `${r27Dir}\\v4_charScale_${scale}_placing_1170x2532.png`,
      viewport: { width: 1170, height: 2532 },
      buildId,
      setupAction: async (send, vp) => {
        // Click "BẮT ĐẦU LUYỆN TẬP" at bottom (approx nx=0.5, ny=0.76)
        await clickNorm(send, vp, 0.50, 0.76);
        await sleep(1000);
        // Drag aim to draw aim line
        await dragAim(send, vp, 0.50, 0.56, 0.68, 0.44);
        await sleep(400);
      }
    });

    // Throwing phase
    await captureSingleScreen({
      url: `http://localhost:8093/index.html?screen=botTraining&charScale=${scale}`,
      outPngPath: `${r27Dir}\\v4_charScale_${scale}_throwing_1170x2532.png`,
      viewport: { width: 1170, height: 2532 },
      buildId,
      setupAction: async (send, vp) => {
        // Start match
        await clickNorm(send, vp, 0.50, 0.76);
        await sleep(600);
        // Drag aim and lock
        await dragAim(send, vp, 0.50, 0.56, 0.65, 0.45);
        await sleep(200);
        // Click "SẴN SÀNG" button (top right: nx=0.82, ny=0.032)
        await clickNorm(send, vp, 0.82, 0.032);
        // Wait for Reveal (0.5s) then Throwing (0.8s) -> mid-flight at ~1.7s
        await sleep(1700);
      }
    });
  }

  // 2. Full Match Phases for N=2 and N=4 across 1170x2532 and 1080x2340
  const viewports = [
    { name: '1170x2532', width: 1170, height: 2532 },
    { name: '1080x2340', width: 1080, height: 2340 }
  ];

  for (const vp of viewports) {
    for (const botCount of [1, 3]) {
      const nTotal = botCount + 1;
      console.log(`\n--- Capturing Phases for N=${nTotal} (${vp.name}) ---`);

      // Pha 1: ĐẶT (Placing with aim drag)
      await captureSingleScreen({
        url: `http://localhost:8093/index.html?screen=botTraining`,
        outPngPath: `${r27Dir}\\phase_placing_N${nTotal}_${vp.name}.png`,
        viewport: vp,
        buildId,
        setupAction: async (send, v) => {
          // Adjust bot count if needed
          if (botCount === 1) {
            // Click "-" twice (nx=0.26, ny=0.62)
            await clickNorm(send, v, 0.26, 0.62);
            await sleep(150);
            await clickNorm(send, v, 0.26, 0.62);
            await sleep(200);
          }
          // Click start
          await clickNorm(send, v, 0.50, 0.76);
          await sleep(1000);
          // Drag aim
          await dragAim(send, v, 0.50, 0.56, 0.68, 0.42);
          await sleep(300);
        }
      });

      // Pha 2: KHÓA (Locked - ĐÃ KHÓA button & banner)
      await captureSingleScreen({
        url: `http://localhost:8093/index.html?screen=botTraining`,
        outPngPath: `${r27Dir}\\phase_locked_N${nTotal}_${vp.name}.png`,
        viewport: vp,
        buildId,
        setupAction: async (send, v) => {
          if (botCount === 1) {
            await clickNorm(send, v, 0.26, 0.62);
            await sleep(150);
            await clickNorm(send, v, 0.26, 0.62);
            await sleep(200);
          }
          await clickNorm(send, v, 0.50, 0.76);
          await sleep(600);
          await dragAim(send, v, 0.50, 0.56, 0.68, 0.42);
          await sleep(200);
          // Click SẴN SÀNG
          await clickNorm(send, v, 0.82, 0.032);
          await sleep(200);
        }
      });

      // Pha 3: LỘ DIỆN (Reveal - LỘ DIỆN ĐỐI THỦ!)
      await captureSingleScreen({
        url: `http://localhost:8093/index.html?screen=botTraining`,
        outPngPath: `${r27Dir}\\phase_reveal_N${nTotal}_${vp.name}.png`,
        viewport: vp,
        buildId,
        setupAction: async (send, v) => {
          if (botCount === 1) {
            await clickNorm(send, v, 0.26, 0.62);
            await sleep(150);
            await clickNorm(send, v, 0.26, 0.62);
            await sleep(200);
          }
          await clickNorm(send, v, 0.50, 0.76);
          await sleep(600);
          await dragAim(send, v, 0.50, 0.56, 0.68, 0.42);
          await sleep(200);
          await clickNorm(send, v, 0.82, 0.032);
          // Wait for Locked (0.5s) -> Reveal phase begins at ~0.7s
          await sleep(850);
        }
      });

      // Pha 4: NÉM (Throwing - mid flight)
      await captureSingleScreen({
        url: `http://localhost:8093/index.html?screen=botTraining`,
        outPngPath: `${r27Dir}\\phase_throwing_N${nTotal}_${vp.name}.png`,
        viewport: vp,
        buildId,
        setupAction: async (send, v) => {
          if (botCount === 1) {
            await clickNorm(send, v, 0.26, 0.62);
            await sleep(150);
            await clickNorm(send, v, 0.26, 0.62);
            await sleep(200);
          }
          await clickNorm(send, v, 0.50, 0.76);
          await sleep(600);
          await dragAim(send, v, 0.50, 0.56, 0.68, 0.42);
          await sleep(200);
          await clickNorm(send, v, 0.82, 0.032);
          // Wait for Locked (0.5s) + Reveal (0.8s) + Throw start -> ~1.7s
          await sleep(1700);
        }
      });

      // Pha 5: KẾT QUẢ VÒNG (Round End banner)
      await captureSingleScreen({
        url: `http://localhost:8093/index.html?screen=botTraining`,
        outPngPath: `${r27Dir}\\phase_round_result_N${nTotal}_${vp.name}.png`,
        viewport: vp,
        buildId,
        setupAction: async (send, v) => {
          if (botCount === 1) {
            await clickNorm(send, v, 0.26, 0.62);
            await sleep(150);
            await clickNorm(send, v, 0.26, 0.62);
            await sleep(200);
          }
          await clickNorm(send, v, 0.50, 0.76);
          await sleep(600);
          await dragAim(send, v, 0.50, 0.56, 0.68, 0.42);
          await sleep(200);
          await clickNorm(send, v, 0.82, 0.032);
          // Wait for Round End -> ~3.2s
          await sleep(3200);
        }
      });
    }
  }

  // Pha 6: KẾT THÚC TRẬN (Result Panel with ranking board)
  console.log(`\n--- Capturing Match End (Result Panel) ---`);
  for (const vp of viewports) {
    await captureSingleScreen({
      url: `http://localhost:8093/index.html?autoPlay=1`,
      outPngPath: `${r27Dir}\\phase_match_end_${vp.name}.png`,
      viewport: vp,
      buildId,
      setupAction: async (send, v) => {
        // AutoPlay 1 match finishes in ~2.5s, then shows Result Panel
        await sleep(3500);
      }
    });
  }

  // Also copy key captures to Evidence/phase3/ for audit verification
  const copyTargets = fs.readdirSync(r27Dir).filter(f => f.endsWith('.png') || f.endsWith('.json'));
  for (const f of copyTargets) {
    fs.copyFileSync(path.join(r27Dir, f), path.join(outDir, f));
  }
  console.log(`Successfully mirrored ${copyTargets.length} files to Evidence/phase3/!`);

  console.log('\nAll Round 27 WebGL screenshot captures completed successfully!');
}

main().catch(err => {
  console.error(err);
  process.exit(1);
});
