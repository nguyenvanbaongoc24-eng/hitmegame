// Local HTTP/WebSocket Origin regression test. Node 22+; no secrets are read.
import assert from 'node:assert/strict';
import { spawn } from 'node:child_process';
import { createServer, connect } from 'node:net';
import { request as httpRequest } from 'node:http';
import { resolve } from 'node:path';

const root = resolve(import.meta.dirname, '../..');
const port = await reservePort();
const base = `http://127.0.0.1:${port}`;
const allowed = 'https://hitmegame-iota.vercel.app';
const signingKey = Buffer.alloc(32, 7).toString('base64');
let serverOutput = '';
const dotnet = process.platform === 'win32' ? resolve(process.env.ProgramFiles ?? 'C:/Program Files', 'dotnet/dotnet.exe') : 'dotnet';
const server = spawn(dotnet, [resolve(root, 'server/bin/Debug/net8.0/HitMe.Server.dll')], {
  cwd: root, windowsHide: true, stdio: ['ignore', 'pipe', 'pipe'],
  env: { ...process.env, PORT: String(port), ALLOWED_ORIGINS: allowed, GUEST_TOKEN_SIGNING_KEY: signingKey,
    ALLOW_MISSING_ORIGIN: 'false', ALLOW_VERCEL_PREVIEW_ORIGINS: 'false', RENDER_GIT_COMMIT: 'self-test' }
});
server.stdout.on('data', chunk => { serverOutput += chunk; });
server.stderr.on('data', chunk => { serverOutput += chunk; });
const checks = [];
try {
  await waitForHealth();
  const health = await http(`${base}/healthz`, { headers: { Origin: 'https://foreign.example' } });
  check('healthz is public CORS liveness only', health.status === 200 && health.headers['access-control-allow-origin'] === '*' && !health.body.includes(allowed));

  const good = await guest(allowed);
  check('guest-token accepts configured Origin and echoes ACAO', good.status === 200 && good.acao === allowed);
  const foreign = await guest('https://foreign.example');
  check('guest-token rejects foreign Origin without ACAO', foreign.status === 403 && foreign.acao === null && foreign.body.includes('origin_not_allowed'));
  const missing = await http(`${base}/guest-token`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: '{"name":"NoOrigin"}' });
  check('guest-token rejects missing Origin by default', missing.status === 403 && missing.headers['access-control-allow-origin'] === undefined);
  const preflight = await http(`${base}/guest-token`, { method: 'OPTIONS', headers: { Origin: allowed, 'Access-Control-Request-Method': 'POST' } });
  check('guest-token allows configured preflight', preflight.status === 204 && preflight.headers['access-control-allow-origin'] === allowed);
  const badPreflight = await http(`${base}/guest-token`, { method: 'OPTIONS', headers: { Origin: 'https://foreign.example', 'Access-Control-Request-Method': 'POST' } });
  check('guest-token rejects foreign preflight without ACAO', badPreflight.status === 403 && badPreflight.headers['access-control-allow-origin'] === undefined);
  check('WebSocket rejects foreign Origin with HTTP 403', (await wsProbe('https://foreign.example')).startsWith('HTTP/1.1 403'));
  check('WebSocket accepts configured Origin', (await wsProbe(allowed)).startsWith('HTTP/1.1 101'));
  console.log(`Passed! ${checks.length}/${checks.length} local Origin HTTP/WS tests: ${checks.join('; ')}`);
} catch (error) {
  console.error(`FAILED local Origin HTTP/WS tests after ${checks.length}:`, error);
  process.exitCode = 1;
} finally {
  server.kill();
}

function check(name, value) { assert.ok(value, name); checks.push(name); }
async function guest(origin) {
  const response = await http(`${base}/guest-token`, { method: 'POST', headers: { Origin: origin, 'Content-Type': 'application/json' }, body: '{"name":"OriginTest"}' });
  return { status: response.status, acao: response.headers['access-control-allow-origin'] ?? null, body: response.body };
}
async function waitForHealth() {
  let observation = '';
  for (let attempt = 0; attempt < 80; attempt++) {
    try { const response = await http(`${base}/healthz`); if (response.status === 200) return; observation = `HTTP ${response.status}: ${response.body}`; }
    catch (error) { observation = error instanceof Error ? error.message : String(error); }
    await new Promise(resolve => setTimeout(resolve, 50));
  }
  throw new Error(`local Kestrel health check timed out (${observation}): ${serverOutput || 'no server output'}`);
}
function http(url, options = {}) {
  return new Promise((resolve, reject) => {
    const request = httpRequest(url, options, response => {
      let body = ''; response.setEncoding('utf8'); response.on('data', chunk => { body += chunk; });
      response.on('end', () => resolve({ status: response.statusCode, headers: response.headers, body }));
    });
    request.on('error', reject); if (options.body) request.write(options.body); request.end();
  });
}
async function reservePort() {
  const probe = createServer();
  await new Promise(resolve => probe.listen(0, '127.0.0.1', resolve));
  const { port } = probe.address(); await new Promise(resolve => probe.close(resolve)); return port;
}
async function wsProbe(origin) {
  return await new Promise((resolve, reject) => {
    const socket = connect(port, '127.0.0.1'); let text = '';
    socket.setTimeout(5000, () => reject(new Error('WebSocket probe timeout')));
    socket.on('connect', () => socket.write(`GET /ws HTTP/1.1\r\nHost: 127.0.0.1:${port}\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==\r\nSec-WebSocket-Version: 13\r\nOrigin: ${origin}\r\n\r\n`));
    socket.on('data', chunk => { text += chunk; if (text.includes('\r\n\r\n')) { socket.destroy(); resolve(text); } });
    socket.on('error', reject);
  });
}
