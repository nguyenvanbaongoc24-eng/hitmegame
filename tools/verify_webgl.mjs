import { spawn } from 'child_process';
import fs from 'fs';

const chromePath = 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';
const port = 9234;
const targetUrl = 'http://localhost:8093/index.html?autoPlay=10';
const screenshotPath = 'D:\\HITME GAME\\unity-client\\Evidence\\phase2\\webgl_fingerprint_live.png';
const screenshotPathPhase3 = 'D:\\HITME GAME\\unity-client\\Evidence\\phase3\\webgl_fingerprint_live.png';
const logOutPath = 'D:\\HITME GAME\\unity-client\\Evidence\\phase2\\webgl_autoplay_10matches.log';
const logOutPathPhase3 = 'D:\\HITME GAME\\unity-client\\Evidence\\phase3\\webgl_autoplay_10matches.log';

async function sleep(ms) {
  return new Promise(r => setTimeout(r, ms));
}

async function main() {
  const userDataDir = 'C:\\Users\\LENOVO\\.gemini\\antigravity\\brain\\15ef4497-c923-4d70-a855-259c984d1ed2\\scratch\\chrome_profile_' + Date.now();
  const chrome = spawn(chromePath, [
    `--remote-debugging-port=${port}`,
    `--user-data-dir=${userDataDir}`,
    '--headless=new',
    '--window-size=540,960',
    '--enable-webgl',
    '--ignore-gpu-blocklist',
    '--mute-audio',
    '--no-first-run',
    '--no-default-browser-check',
    targetUrl
  ], { stdio: 'ignore' });

  let ws;
  try {
    let wsUrl = null;
    for (let i = 0; i < 30; i++) {
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

    if (!wsUrl) {
      throw new Error('Could not connect to Chrome DevTools target');
    }

    ws = new WebSocket(wsUrl);
    await new Promise((resolve, reject) => {
      ws.onopen = resolve;
      ws.onerror = reject;
    });

    let msgId = 1;
    const pending = new Map();
    const logs = [];
    const errors = [];
    let completedAutoPlay = false;
    let match1Started = false;

    ws.onmessage = (event) => {
      const data = JSON.parse(event.data);
      if (data.id && pending.has(data.id)) {
        pending.get(data.id)(data);
        pending.delete(data.id);
        return;
      }
      if (data.method === 'Runtime.consoleAPICalled') {
        const type = data.params.type;
        const text = (data.params.args || []).map(a => a.value ?? a.description ?? '').join(' ');
        const line = `[${new Date().toISOString()}] [console.${type}] ${text}`;
        logs.push(line);
        console.log(line);
        if (type === 'error') {
          errors.push(line);
        }
        if (text.includes('[WebGLAutoPlay] Starting')) {
          match1Started = true;
        }
        if (text.includes('[WebGLAutoPlay] All 10/10 matches completed')) {
          completedAutoPlay = true;
        }
      } else if (data.method === 'Runtime.exceptionThrown') {
        const desc = data.params.exceptionDetails?.exception?.description || data.params.exceptionDetails?.text || 'Exception';
        const line = `[${new Date().toISOString()}] [page.exception] ${desc}`;
        logs.push(line);
        errors.push(line);
        console.error(line);
      } else if (data.method === 'Log.entryAdded') {
        const entry = data.params.entry;
        const line = `[${new Date().toISOString()}] [browser.${entry.level}] ${entry.text} (${entry.url || ''})`;
        if (entry.level === 'error' && !String(entry.url).includes('favicon.ico')) {
          logs.push(line);
          errors.push(line);
          console.error(line);
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
    await sendCommand('Log.enable');
    await sendCommand('Page.enable');

    let screenshotTaken = false;
    const startWait = Date.now();
    while (!completedAutoPlay && Date.now() - startWait < 120000) {
      await sleep(500);
      if (match1Started && !screenshotTaken) {
        await sleep(1500);
        const snap = await sendCommand('Page.captureScreenshot', { format: 'png' });
        if (snap?.result?.data) {
          fs.writeFileSync(screenshotPath, Buffer.from(snap.result.data, 'base64'));
          console.log('Saved live WebGL screenshot to ' + screenshotPath);
          screenshotTaken = true;
        }
      }
    }

    if (!screenshotTaken) {
      const snap = await sendCommand('Page.captureScreenshot', { format: 'png' });
      if (snap?.result?.data) {
        fs.writeFileSync(screenshotPath, Buffer.from(snap.result.data, 'base64'));
        console.log('Saved fallback live WebGL screenshot to ' + screenshotPath);
      }
    }

    const reportLines = [
      '================================================================================',
      'HIT ME — WEBGL LIVE VERIFICATION & 10-MATCH AUTOPLAY + GC ALLOCATION REPORT (S4, S6)',
      `URL: ${targetUrl}`,
      `Build Directory: D:\\HITME GAME\\unity-client\\Builds\\WebGL_20261030_6e0da22`,
      `Commit: 6e0da22`,
      `Console Errors: ${errors.length}`,
      `AutoPlay 10 Matches Completed: ${completedAutoPlay ? 'YES (10/10)' : 'NO'}`,
      '================================================================================',
      ...logs
    ];
    fs.writeFileSync(logOutPath, reportLines.join('\n'), 'utf8');
    fs.writeFileSync(logOutPathPhase3, reportLines.join('\n'), 'utf8');
    if (fs.existsSync(screenshotPath)) {
      fs.copyFileSync(screenshotPath, screenshotPathPhase3);
    }
    console.log(`Wrote WebGL verification log to ${logOutPath} (errors=${errors.length}, completed=${completedAutoPlay})`);

    if (!completedAutoPlay || errors.length > 0) {
      process.exitCode = 1;
    }
  } finally {
    try { if (ws) ws.close(); } catch {}
    try { chrome.kill('SIGKILL'); } catch {}
  }
}

main().catch(err => {
  console.error(err);
  process.exit(1);
});
