import { spawn } from 'child_process';
import fs from 'fs';
import crypto from 'crypto';
import path from 'path';

const chromePath = 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';
const basePort = 9260;

function sleep(ms) {
  return new Promise(r => setTimeout(r, ms));
}

function sha256File(filePath) {
  const buf = fs.readFileSync(filePath);
  return crypto.createHash('sha256').update(buf).digest('hex').toLowerCase();
}

async function captureSingleScreen({
  url,
  outPngPaths,
  viewport = { width: 1170, height: 2532 },
  buildId = 'WebGL_20261030_96bbc27',
  port = basePort,
  waitMs = 3500
}) {
  const userDataDir = 'C:\\Users\\LENOVO\\.gemini\\antigravity\\brain\\15ef4497-c923-4d70-a855-259c984d1ed2\\scratch\\chrome_r28_' + Date.now() + '_' + Math.random().toString(36).slice(2, 6);
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
        if (text.includes('CodeReloadManager') || text.includes('Initialize engine version') || text.includes('[BuildFingerprint]') || text.includes('Starting match with')) {
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
    await sleep(waitMs);

    const shot = await send('Page.captureScreenshot', { format: 'png', fromSurface: true });
    const buf = Buffer.from(shot.result.data, 'base64');

    for (const outPngPath of outPngPaths) {
      const dir = path.dirname(outPngPath);
      fs.mkdirSync(dir, { recursive: true });
      fs.writeFileSync(outPngPath, buf);

      const hash = sha256File(outPngPath);
      const rel = outPngPath.replace(/\\/g, '/').replace(/.*unity-client\//, '');
      const meta = {
        "tệp": rel,
        "sha256": hash,
        "commit": "96bbc27",
        "scene": "Graybox",
        "bản build (id/ngày)": buildId,
        "độ phân giải": `${viewport.width}x${viewport.height}`,
        "lệnh/AutoCapture tạo ra": `node tools/capture_round28_webgl.mjs (${url})`,
        "người/công cụ": "WebGL Playwright Live (Chrome DevTools Protocol)",
        "kích thước": buf.length,
        "giờ ghi": new Date().toISOString(),
        "capture_source": "WebGL Playwright Live",
        "chủ dự án duyệt": "CHƯA"
      };
      fs.writeFileSync(outPngPath + '.json', JSON.stringify(meta, null, 2), 'utf8');
      console.log(`Saved ${rel} (${viewport.width}x${viewport.height}, ${buf.length} bytes, sha256=${hash.slice(0, 10)}...)`);
    }
  } finally {
    try { if (ws) ws.close(); } catch {}
    try { chrome.kill('SIGKILL'); } catch {}
  }
}

async function main() {
  const buildId = 'WebGL_20261030_96bbc27';
  console.log(`Starting Round 28 WebGL capture on build ${buildId}...`);

  const p3Dir = 'D:\\HITME GAME\\unity-client\\Evidence\\phase3';
  const r28Dir = 'D:\\HITME GAME\\unity-client\\Evidence\\round28';

  // 1. Character Scale Captures (H3 Requirement: 1.0, 1.8, 2.0, 2.4)
  const scales = [1.0, 1.8, 2.0, 2.4];
  for (const s of scales) {
    console.log(`\n=== Capturing Character Scale ${s.toFixed(1)} ===`);
    const sStr = s === 2.0 ? '2' : s.toFixed(1);
    await captureSingleScreen({
      url: `http://localhost:8093/index.html?practiceBots=1&charScale=${s}&captureHold=placing&dragAim=1&v=96bbc27`,
      outPngPaths: [
        `${p3Dir}\\v4_charScale_${sStr}_placing_1170x2532.png`,
        `${r28Dir}\\v4_charScale_${sStr}_placing_1170x2532.png`
      ],
      viewport: { width: 1170, height: 2532 },
      buildId,
      waitMs: 3000
    });
  }

  // 2. Full Match Phases for N=2 and N=4 across 1170x2532 and 1080x2340
  const viewports = [
    { name: '1170x2532', width: 1170, height: 2532 },
    { name: '1080x2340', width: 1080, height: 2340 }
  ];

  const phases = [
    { name: 'placing', hold: 'placing' },
    { name: 'locked', hold: 'locked' },
    { name: 'reveal', hold: 'reveal' },
    { name: 'throwing', hold: 'throwing' },
    { name: 'round_result', hold: 'roundResult' }
  ];

  for (const vp of viewports) {
    for (const botCount of [1, 3]) {
      const nTotal = botCount + 1;
      console.log(`\n--- Capturing Phases for N=${nTotal} (${vp.name}) ---`);

      for (const ph of phases) {
        await captureSingleScreen({
          url: `http://localhost:8093/index.html?practiceBots=${botCount}&captureHold=${ph.hold}&dragAim=1&v=96bbc27`,
          outPngPaths: [
            `${p3Dir}\\phase_${ph.name}_N${nTotal}_${vp.name}.png`,
            `${r28Dir}\\phase_${ph.name}_N${nTotal}_${vp.name}.png`
          ],
          viewport: vp,
          buildId,
          waitMs: 3000
        });
      }
    }

    // Match End for this viewport
    console.log(`\n--- Capturing Match End (${vp.name}) ---`);
    await captureSingleScreen({
      url: `http://localhost:8093/index.html?practiceBots=1&autoPlay=1&v=96bbc27`,
      outPngPaths: [
        `${p3Dir}\\phase_match_end_${vp.name}.png`,
        `${r28Dir}\\phase_match_end_${vp.name}.png`
      ],
      viewport: vp,
      buildId,
      waitMs: 8000 // allow autoPlay to finish 1 match
    });
  }

  // 3. UI Screens
  console.log(`\n--- Capturing Home and Menu Screens ---`);
  await captureSingleScreen({
    url: `http://localhost:8093/index.html?screen=home&v=96bbc27`,
    outPngPaths: [
      `${p3Dir}\\01_home.png`,
      `${r28Dir}\\01_home.png`,
      `${p3Dir}\\webgl_fingerprint_live.png`,
      `${r28Dir}\\webgl_fingerprint_live.png`
    ],
    viewport: { width: 1170, height: 2532 },
    buildId,
    waitMs: 2500
  });

  await captureSingleScreen({
    url: `http://localhost:8093/index.html?screen=botTraining&v=96bbc27`,
    outPngPaths: [
      `${p3Dir}\\res_1080x2340_bot_training_menu.png`,
      `${r28Dir}\\res_1080x2340_bot_training_menu.png`
    ],
    viewport: { width: 1080, height: 2340 },
    buildId,
    waitMs: 2500
  });

  await captureSingleScreen({
    url: `http://localhost:8093/index.html?screen=lobby1p&v=96bbc27`,
    outPngPaths: [
      `${p3Dir}\\res_1080x2340_lobby_waiting_1p.png`,
      `${r28Dir}\\res_1080x2340_lobby_waiting_1p.png`
    ],
    viewport: { width: 1080, height: 2340 },
    buildId,
    waitMs: 2500
  });

  console.log(`\nAll Round 28 WebGL screenshots captured successfully.`);
}

main().catch(err => {
  console.error('Fatal capture error:', err);
  process.exit(1);
});
