import { spawn } from 'child_process';
import fs from 'fs';
import crypto from 'crypto';
import path from 'path';

const chromePath = 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';
const basePort = 9275;
const targetWebPort = 8095;

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
  commit = '9cdef40',
  buildId = 'WebGL_20261101_9cdef40',
  port = basePort,
  waitMs = 3800,
  isMobile = true
}) {
  const userDataDir = 'C:\\Users\\LENOVO\\.gemini\\antigravity\\brain\\ad8b2abf-26b1-4b72-84a1-97aabeca408d\\scratch\\chrome_r33_' + Date.now() + '_' + Math.random().toString(36).slice(2, 6);
  fs.mkdirSync(userDataDir, { recursive: true });

  const chrome = spawn(chromePath, [
    `--remote-debugging-port=${port}`,
    `--user-data-dir=${userDataDir}`,
    '--headless=new',
    `--window-size=${viewport.width},${viewport.height}`,
    '--enable-webgl',
    '--ignore-gpu-blocklist',
    '--mute-audio',
    '--no-first-run',
    '--no-default-browser-change',
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
        const page = pages.find(p => p.type === 'page' && p.url.includes(`localhost:${targetWebPort}`));
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
        if (text.includes('CodeReloadManager') || text.includes('Initialize engine version') || text.includes('[BuildFingerprint]') || text.includes('Starting match with') || text.includes('Entering screen')) {
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
      mobile: isMobile
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
        "commit": commit,
        "scene": "Graybox",
        "bản build (id/ngày)": buildId,
        "độ phân giải": `${viewport.width}x${viewport.height}`,
        "lệnh/AutoCapture tạo ra": `node tools/capture_round33_webgl.mjs (${url})`,
        "người/công cụ": "WebGL Chrome DevTools Protocol",
        "kích thước": buf.length,
        "giờ ghi": new Date().toISOString(),
        "capture_source": "WebGL Playwright/CDP Live",
        "chủ dự án duyệt": "CHƯA"
      };
      fs.writeFileSync(outPngPath + '.json', JSON.stringify(meta, null, 2), 'utf8');
      console.log(`Saved ${rel} (${viewport.width}x${viewport.height}, ${buf.length} bytes, sha256=${hash.slice(0, 10)}...)`);
    }
  } finally {
    try { if (ws) ws.close(); } catch {}
    try { chrome.kill('SIGKILL'); } catch {}
    try { fs.rmSync(userDataDir, { recursive: true, force: true }); } catch {}
  }
}

async function main() {
  const commit = process.argv[2] || '9cdef40';
  const buildId = `WebGL_20261101_${commit}`;
  console.log(`Starting Round 33 WebGL capture on port ${targetWebPort}, build ${buildId} (commit=${commit})...`);

  const r33Dir = 'D:\\HITME GAME\\unity-client\\Evidence\\round33';
  const artifactDir = 'C:\\Users\\LENOVO\\.gemini\\antigravity\\brain\\ad8b2abf-26b1-4b72-84a1-97aabeca408d';

  // ==================== SECTION V: WINDOW CONTAINMENT AT 5 RESOLUTIONS ====================
  console.log('\n==================== 1. SECTION V: WINDOW CONTAINMENT ====================');
  const windowViewports = [
    { name: '1920x1080', width: 1920, height: 1080, isMobile: false },
    { name: '1366x768',  width: 1366, height: 768,  isMobile: false },
    { name: '820x1180',  width: 820,  height: 1180, isMobile: false },
    { name: '1170x2532', width: 1170, height: 2532, isMobile: true },
    { name: '1080x1920', width: 1080, height: 1920, isMobile: true }
  ];

  for (const vp of windowViewports) {
    for (const botCount of [1, 3]) { // N=2 and N=4
      const nTotal = botCount + 1;
      const fileName = `window_contain_${vp.name}_N${nTotal}.png`;
      const url = `http://localhost:${targetWebPort}/index.html?practiceBots=${botCount}&captureHold=placing&v=${commit}`;
      console.log(`Capturing Window Containment: ${vp.name} (N=${nTotal})...`);

      await captureSingleScreen({
        url,
        outPngPaths: [
          `${r33Dir}\\${fileName}`,
          `${artifactDir}\\${fileName}`
        ],
        viewport: { width: vp.width, height: vp.height },
        commit,
        buildId,
        waitMs: 3800,
        isMobile: vp.isMobile
      });
    }
  }

  // ==================== SECTION 1: LÔ 2 SCREENS (1170x2532) ====================
  console.log('\n==================== 2. SECTION 1: LÔ 2 SCREENS ====================');
  const lo2Screens = [
    { name: '01_home_screen', url: `http://localhost:${targetWebPort}/index.html?screen=home&v=${commit}` },
    { name: '02_bot_training_default_3bots', url: `http://localhost:${targetWebPort}/index.html?screen=botTraining&bots=3&v=${commit}` },
    { name: '03_bot_training_5bots', url: `http://localhost:${targetWebPort}/index.html?screen=botTraining&bots=5&v=${commit}` },
    { name: '04_lobby_1player_warning', url: `http://localhost:${targetWebPort}/index.html?screen=lobby1p&v=${commit}` },
    { name: '05_lobby_3players', url: `http://localhost:${targetWebPort}/index.html?screen=lobby3p&v=${commit}` },
    { name: '06_quick_match_searching', url: `http://localhost:${targetWebPort}/index.html?screen=quickmatch&v=${commit}` }
  ];

  for (const sc of lo2Screens) {
    const fileName = `lo2_${sc.name}_1170x2532.png`;
    console.log(`Capturing Lô 2 Screen: ${sc.name}...`);
    await captureSingleScreen({
      url: sc.url,
      outPngPaths: [
        `${r33Dir}\\${fileName}`,
        `${artifactDir}\\${fileName}`
      ],
      viewport: { width: 1170, height: 2532 },
      commit,
      buildId,
      waitMs: 3800,
      isMobile: true
    });
  }

  console.log('\nAll Section V and Lô 2 WebGL captures completed successfully!');
}

main().catch(err => {
  console.error('Fatal capture error:', err);
  process.exit(1);
});
