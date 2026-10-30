import { spawn } from 'child_process';
import fs from 'fs';

const chromePath = 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';
const port = 9236;
const targetUrl = 'http://localhost:8091/index.html?autoPlay=50&profileSeconds=62';
const reportOutPath = 'D:\\HITME GAME\\unity-client\\Evidence\\phase3\\gc_60s_profile_report.txt';

async function sleep(ms) {
  return new Promise(r => setTimeout(r, ms));
}

async function main() {
  const userDataDir = 'C:\\Users\\LENOVO\\.gemini\\antigravity\\brain\\15ef4497-c923-4d70-a855-259c984d1ed2\\scratch\\chrome_profile_60s_' + Date.now();
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
        const page = pages.find(p => p.type === 'page' && p.url.includes('localhost:8091'));
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
    let startedAutoPlay = false;

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
        if (text.includes('[WebGLAutoPlay] Starting')) {
          startedAutoPlay = true;
          console.log(line);
        }
        if (text.includes('Match 10/') || text.includes('Match 20/') || text.includes('Match 30/') || text.includes('Match 40/') || text.includes('Match 50/') || text.includes('matches completed with 0 errors')) {
          console.log(line);
        }
        if (text.includes('matches completed with 0 errors!')) {
          completedAutoPlay = true;
        }
        if (type === 'error') {
          errors.push(line);
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

    // Wait for match 1 to start
    const waitStart = Date.now();
    while (!startedAutoPlay && Date.now() - waitStart < 30000) {
      await sleep(200);
    }

    // Inject rAF FPS & 1s window FPS tracker inside page
    await sendCommand('Runtime.evaluate', {
      expression: `
        window.__fpsTracker = {
          startTime: performance.now(),
          lastTime: performance.now(),
          frameDeltas: [],
          secWindows: [],
          curSecFrames: 0,
          curSecStart: performance.now(),
          running: true
        };
        (function loop(now) {
          const t = window.__fpsTracker;
          if (!t || !t.running) return;
          const dt = now - t.lastTime;
          t.lastTime = now;
          if (dt > 0 && dt < 1000) {
            t.frameDeltas.push(dt);
          }
          t.curSecFrames++;
          if (now - t.curSecStart >= 1000) {
            t.secWindows.push(t.curSecFrames * 1000 / (now - t.curSecStart));
            t.curSecFrames = 0;
            t.curSecStart = now;
          }
          requestAnimationFrame(loop);
        })(performance.now());
      `
    });

    const benchStart = Date.now();
    while ((!completedAutoPlay || (Date.now() - benchStart < 60500)) && Date.now() - benchStart < 120000) {
      await sleep(500);
    }

    const evalRes = await sendCommand('Runtime.evaluate', {
      expression: `
        (() => {
          const t = window.__fpsTracker;
          t.running = false;
          const elapsedMs = performance.now() - t.startTime;
          const totalFrames = t.frameDeltas.length;
          const avgFps = totalFrames * 1000 / elapsedMs;
          const sortedDeltas = [...t.frameDeltas].sort((a, b) => b - a); // descending frame time (worst first)
          const worstFrameMs = sortedDeltas[0] || 0;
          const p99FrameMs = sortedDeltas[Math.floor(sortedDeltas.length * 0.01)] || 0;
          const min1SecFps = t.secWindows.length > 0 ? Math.min(...t.secWindows) : avgFps;
          const max1SecFps = t.secWindows.length > 0 ? Math.max(...t.secWindows) : avgFps;
          const mem = performance.memory ? {
            usedJSHeapSize: performance.memory.usedJSHeapSize,
            totalJSHeapSize: performance.memory.totalJSHeapSize
          } : null;
          return JSON.stringify({
            elapsedSec: (elapsedMs / 1000).toFixed(2),
            totalFrames,
            avgFps: avgFps.toFixed(2),
            min1SecFps: min1SecFps.toFixed(2),
            max1SecFps: max1SecFps.toFixed(2),
            onePercentLowFps: p99FrameMs > 0 ? (1000 / p99FrameMs).toFixed(2) : '0',
            worstFrameFps: worstFrameMs > 0 ? (1000 / worstFrameMs).toFixed(2) : '0',
            worstFrameMs: worstFrameMs.toFixed(2),
            p99FrameMs: p99FrameMs.toFixed(2),
            secWindowsCount: t.secWindows.length,
            mem
          });
        })()
      `,
      returnByValue: true
    });

    const stats = JSON.parse(evalRes.result.result.value);
    console.log('60s WebGL Stats:', stats);

    fs.writeFileSync(reportOutPath + '.raw.json', JSON.stringify({ stats, errors: errors.length, logs }, null, 2), 'utf8');
  } finally {
    try { if (ws) ws.close(); } catch {}
    try { chrome.kill('SIGKILL'); } catch {}
  }
}

main().catch(err => {
  console.error(err);
  process.exit(1);
});
