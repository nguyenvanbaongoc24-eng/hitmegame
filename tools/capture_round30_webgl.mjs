import { spawn } from 'child_process';
import fs from 'fs';
import crypto from 'crypto';
import path from 'path';

const chromePath = 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';
const basePort = 9260;
const targetWebPort = 8094;

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
  commit = '64bc6a9',
  buildId = 'WebGL_20261030_64bc6a9',
  port = basePort,
  waitMs = 3500
}) {
  const userDataDir = 'C:\\Users\\LENOVO\\.gemini\\antigravity\\brain\\15ef4497-c923-4d70-a855-259c984d1ed2\\scratch\\chrome_r30_' + Date.now() + '_' + Math.random().toString(36).slice(2, 6);
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
        "commit": commit,
        "scene": "Graybox",
        "bản build (id/ngày)": buildId,
        "độ phân giải": `${viewport.width}x${viewport.height}`,
        "lệnh/AutoCapture tạo ra": `node tools/capture_round30_webgl.mjs (${url})`,
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
  const commit = process.argv[2] || '64bc6a9';
  const buildId = `WebGL_20261030_${commit}`;
  console.log(`Starting Round 30 WebGL capture on port ${targetWebPort}, build ${buildId} (commit=${commit})...`);

  const r30Dir = 'D:\\HITME GAME\\unity-client\\Evidence\\round30';
  const artifactDir = 'C:\\Users\\LENOVO\\.gemini\\antigravity\\brain\\15ef4497-c923-4d70-a855-259c984d1ed2';

  const viewports = [
    { name: '1170x2532', width: 1170, height: 2532 }
  ];

  const phases = [
    { name: 'placing', hold: 'placing', drag: 1 },
    { name: 'locked', hold: 'locked', drag: 0 },
    { name: 'reveal', hold: 'reveal', drag: 0 },
    { name: 'throwing', hold: 'throwing', drag: 0 },
    { name: 'round_result', hold: 'roundResult', drag: 0 }
  ];

  for (const vp of viewports) {
    for (const botCount of [1, 3]) {
      const nTotal = botCount + 1;
      console.log(`\n--- Capturing Phases for N=${nTotal} (${vp.name}) ---`);

      for (const ph of phases) {
        const fileName = `phase_${ph.name}_N${nTotal}_${vp.name}.png`;
        const dragParam = ph.drag ? '&dragAim=1' : '';
        const url = `http://localhost:${targetWebPort}/index.html?practiceBots=${botCount}&captureHold=${ph.hold}${dragParam}&v=${commit}`;

        console.log(`Capturing N=${nTotal} Phase=${ph.name}...`);
        await captureSingleScreen({
          url,
          outPngPaths: [
            `${r30Dir}\\${fileName}`,
            `${artifactDir}\\${fileName}`
          ],
          viewport: { width: vp.width, height: vp.height },
          commit,
          buildId,
          waitMs: 3500
        });
      }
    }
  }

  console.log('\nAll Round 30 live WebGL screenshots captured successfully!');
}

main().catch(err => {
  console.error('Fatal capture error:', err);
  process.exit(1);
});
