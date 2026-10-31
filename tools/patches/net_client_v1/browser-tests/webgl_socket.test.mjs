// Run with `npm test` from this directory. Uses installed Chrome and the real
// ASP.NET WebSocket server; Unity Editor and Assets are never opened or edited.
import { chromium } from 'playwright';
import { createServer } from 'node:http';
import { spawn } from 'node:child_process';
import { resolve } from 'node:path';
import assert from 'node:assert/strict';

const root = resolve(import.meta.dirname, '../../../..');
const bridge = resolve(import.meta.dirname, '../HitMeWebSocket.jslib');
const staticServer = createServer((_request, response) => {
  response.writeHead(200, { 'content-type': 'text/html; charset=utf-8' });
  response.end('<!doctype html><title>HIT ME WebGL socket bridge test</title>');
});
await new Promise(resolveListen => staticServer.listen(0, '127.0.0.1', resolveListen));
const staticPort = staticServer.address().port;
const portProbe = createServer();
await new Promise(resolveListen => portProbe.listen(0, '127.0.0.1', resolveListen));
const serverPort = portProbe.address().port;
await new Promise(resolveClose => portProbe.close(resolveClose));

const server = spawn('dotnet', [resolve(root, 'server/bin/Debug/net8.0/HitMe.Server.dll')], {
  cwd: root,
  env: { ...process.env, PORT: String(serverPort), ALLOWED_ORIGINS: `http://127.0.0.1:${staticPort}` },
  windowsHide: true,
  stdio: ['ignore', 'pipe', 'pipe']
});
let serverLog = '';
server.stdout.on('data', chunk => { serverLog += chunk; });
server.stderr.on('data', chunk => { serverLog += chunk; });
let browser;
try {
  const health = `http://127.0.0.1:${serverPort}/healthz`;
  let healthy = false;
  for (let attempt = 0; attempt < 100; attempt++) {
    try { healthy = (await fetch(health)).ok; } catch { /* starting */ }
    if (healthy) break;
    await new Promise(delay => setTimeout(delay, 100));
  }
  assert.ok(healthy, `Server did not become healthy: ${serverLog}`);

  browser = await chromium.launch({ channel: 'chrome', headless: true });
  const pages = await Promise.all([browser.newPage(), browser.newPage()]);
  for (const page of pages) {
    await page.goto(`http://127.0.0.1:${staticPort}/`);
    await page.evaluate(() => {
      window.LibraryManager = { library: {} };
      window.mergeInto = (target, source) => Object.assign(target, source);
      window.UTF8ToString = value => value;
      window.hitMeEvents = [];
      window.SendMessage = (receiver, method, payload) => window.hitMeEvents.push({ receiver, method, payload });
    });
    await page.addScriptTag({ path: bridge });
    await page.evaluate(url => { window.hitMeHandle = window.LibraryManager.library.HitMeWsOpen(url, 'HitMeBridge'); }, `ws://127.0.0.1:${serverPort}/ws`);
    await page.waitForFunction(() => window.hitMeEvents.some(event => event.method === 'OnHitMeWsOpen'));
    assert.equal(await page.evaluate(() => window.LibraryManager.library.HitMeWsIsOpen(window.hitMeHandle)), 1);
  }

  const send = async (page, message) => {
    const sent = await page.evaluate(json => window.LibraryManager.library.HitMeWsSend(window.hitMeHandle, json), JSON.stringify({ v: 1, ...message }));
    assert.equal(sent, 1);
  };
  const received = async (page, type, after = 0) => {
    await page.waitForFunction(({ wanted, from }) => window.hitMeEvents.slice(from).some(event => {
      if (event.method !== 'OnHitMeWsMessage') return false;
      try { return JSON.parse(event.payload.slice(event.payload.indexOf('|') + 1)).type === wanted; } catch { return false; }
    }), { wanted: type, from: after });
    return page.evaluate(({ wanted, from }) => {
      const event = window.hitMeEvents.slice(from).find(entry => entry.method === 'OnHitMeWsMessage' && JSON.parse(entry.payload.slice(entry.payload.indexOf('|') + 1)).type === wanted);
      return JSON.parse(event.payload.slice(event.payload.indexOf('|') + 1));
    }, { wanted: type, from: after });
  };
  const [host, guest] = pages;
  await send(host, { type: 'join', token: 'guest:WebGL Host' });
  const room = await received(host, 'room_state');
  assert.equal(room.protocolVersion, 1);
  await send(guest, { type: 'join', token: 'guest:WebGL Guest', roomCode: room.roomCode });
  assert.equal((await received(guest, 'room_state')).players.length, 2);
  await send(host, { type: 'start' });
  assert.equal((await received(host, 'round_start')).roundId, 1);
  assert.equal((await received(guest, 'round_start')).roundId, 1);

  const beforeGuest = await guest.evaluate(() => window.hitMeEvents.length);
  const beforeHostAck = await host.evaluate(() => window.hitMeEvents.length);
  await send(host, { type: 'commit', roundId: 1, positionX: 3.14159, positionY: -2.71828, hasAim: true, aimX: 1, aimY: 0 });
  await received(host, 'room_state', beforeHostAck);
  const preLock = await guest.evaluate(from => window.hitMeEvents.slice(from).map(event => event.payload).join('\n'), beforeGuest);
  assert.ok(!preLock.includes('3.14159') && !preLock.includes('position'), 'Private commit leaked before lock');
  const beforeGuestAck = await guest.evaluate(() => window.hitMeEvents.length);
  await send(guest, { type: 'commit', roundId: 1, positionX: -1, positionY: 1, hasAim: false, aimX: 0, aimY: 0 });
  await received(guest, 'room_state', beforeGuestAck);
  await send(host, { type: 'lock_ready', ready: true });
  await send(guest, { type: 'lock_ready', ready: true });
  for (const page of pages) {
    assert.equal((await received(page, 'reveal')).roundId, 1);
    assert.equal((await received(page, 'round_result')).roundId, 1);
  }
  console.log('Passed! 1/1 Chrome two-tab WebGL .jslib bridge: join, start, private commit, reveal, result');
  for (const page of pages) await page.evaluate(() => window.LibraryManager.library.HitMeWsClose(window.hitMeHandle));
  await Promise.all(pages.map(page => page.waitForFunction(() => window.hitMeEvents.some(event => event.method === 'OnHitMeWsClose'))));
} finally {
  if (browser) await browser.close();
  server.kill();
  staticServer.close();
}
