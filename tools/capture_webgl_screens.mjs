import { spawn } from 'child_process';
import fs from 'fs';
import crypto from 'crypto';

const chromePath = 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';
const port = 9240;

async function sleep(ms) {
  return new Promise(r => setTimeout(r, ms));
}

function sha256File(path) {
  const buf = fs.readFileSync(path);
  return crypto.createHash('sha256').update(buf).digest('hex').toLowerCase();
}

async function captureScreen(url, outPngPath, buildId) {
  const userDataDir = 'C:\\Users\\LENOVO\\.gemini\\antigravity\\brain\\15ef4497-c923-4d70-a855-259c984d1ed2\\scratch\\chrome_snap_' + Date.now();
  const chrome = spawn(chromePath, [
    `--remote-debugging-port=${port}`,
    `--user-data-dir=${userDataDir}`,
    '--headless=new',
    '--window-size=1080,2340',
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
        const page = pages.find(p => p.type === 'page' && p.url.includes('localhost:8092'));
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
    let unityReady = false;

    ws.onmessage = (event) => {
      const data = JSON.parse(event.data);
      if (data.id && pending.has(data.id)) {
        pending.get(data.id)(data);
        pending.delete(data.id);
        return;
      }
      if (data.method === 'Runtime.consoleAPICalled') {
        const text = (data.params.args || []).map(a => a.value ?? a.description ?? '').join(' ');
        if (text.includes('[BuildFingerprint]') || text.includes('CodeReloadManager') || text.includes('Initialize engine version')) {
          unityReady = true;
        }
      }
    };

    const sendCommand = (method, params = {}) => {
      const id = msgId++;
      return new Promise((resolve) => {
        pending.set(id, resolve);
        ws.send(JSON.stringify({ id, method, params }));
      });
    };

    await sendCommand('Runtime.enable');
    await sendCommand('Page.enable');
    await sendCommand('Emulation.setDeviceMetricsOverride', {
      width: 1080,
      height: 2340,
      deviceScaleFactor: 1,
      mobile: true
    });

    // Wait for unity engine to finish loading and rendering scene
    for (let i = 0; i < 30; i++) {
      await sleep(500);
      if (unityReady) break;
    }
    // Extra grace period for UI and canvas to settle
    await sleep(2500);

    const shot = await sendCommand('Page.captureScreenshot', { format: 'png', fromSurface: true });
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
      "độ phân giải": "1080x2340",
      "lệnh/AutoCapture tạo ra": `node tools/capture_webgl_screens.mjs (${url})`,
      "người/công cụ": "WebGL Playwright Live (Chrome DevTools Protocol)",
      "kích thước": buf.length,
      "giờ ghi": new Date().toISOString(),
      "capture_source": "WebGL Playwright Live"
    };
    fs.writeFileSync(outPngPath + '.json', JSON.stringify(meta, null, 2), 'utf8');
    console.log(`Captured ${rel} (${buf.length} bytes, sha256=${hash.slice(0, 12)}...)`);
  } finally {
    try { if (ws) ws.close(); } catch {}
    try { chrome.kill('SIGKILL'); } catch {}
  }
}

async function main() {
  const buildId = process.argv[2] || 'WebGL_20261030_f14c587';
  console.log(`Starting WebGL screenshot capture for ${buildId}...`);

  await captureScreen(
    'http://localhost:8092/index.html?screen=lobby1p',
    'D:\\HITME GAME\\unity-client\\Evidence\\phase3\\res_1080x2340_lobby_waiting_1p.png',
    buildId
  );

  await captureScreen(
    'http://localhost:8092/index.html?screen=botTraining',
    'D:\\HITME GAME\\unity-client\\Evidence\\phase3\\res_1080x2340_bot_training_menu.png',
    buildId
  );

  console.log('Capture completed successfully!');
}

main().catch(err => {
  console.error(err);
  process.exit(1);
});
