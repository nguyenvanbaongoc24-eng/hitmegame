// Browser smoke for the experimental Online Lab. Run while run_local.ps1 is active.
// Usage: node tools/dev/smoke_local.mjs [http://127.0.0.1:8100]
import assert from 'node:assert/strict';
import { mkdir, writeFile } from 'node:fs/promises';
import { resolve } from 'node:path';
import { chromium } from '../patches/net_client_v1/browser-tests/node_modules/playwright/index.mjs';

const base = (process.argv[2] || 'http://127.0.0.1:8100').replace(/\/$/, '');
const evidence = resolve(import.meta.dirname, '../../Evidence/dev_lab');
await mkdir(evidence, { recursive: true });
const checks = [];
const latencies = [];
const screenshots = [];
const browser = await chromium.launch({ headless: true, channel: 'chrome' });
let counter = 0;
const uid = Date.now().toString(36);
function pass(name) { checks.push(name); process.stdout.write(`PASS ${name}\n`); }
async function open(name) {
  const context = await browser.newContext({ viewport: { width: 390, height: 844 } });
  const page = await context.newPage();
  const transcript = [];
  page.on('websocket', socket => {
    socket.on('framereceived', event => { try { transcript.push({ at: Date.now(), direction: 'in', message: JSON.parse(event.payload) }); } catch {} });
    socket.on('framesent', event => { try { transcript.push({ at: Date.now(), direction: 'out', message: JSON.parse(event.payload) }); } catch {} });
  });
  page.on('pageerror', error => { throw error; });
  await page.goto(`${base}/lab/?name=${encodeURIComponent(name)}`);
  return { context, page, transcript };
}
async function shot(page, name) {
  const file = resolve(evidence, `${name}.png`);
  await page.screenshot({ path: file, fullPage: true });
  screenshots.push(name + '.png');
}
async function closeAll(clients) { await Promise.all(clients.map(client => client.context.close())); }
async function placeAndAim(client, side) {
  const box = await client.page.locator('#arena').boundingBox();
  const x = box.x + box.width * (side === 'left' ? .3 : .7);
  const y = box.y + box.height * .55;
  await client.page.mouse.click(x, y);
  await client.page.mouse.move(x, y);
  await client.page.mouse.down();
  await client.page.mouse.move(box.x + box.width * (side === 'left' ? .7 : .3), y, { steps: 5 });
  await client.page.mouse.up();
}
async function testBuild() {
  const config = await (await fetch(base + '/config.json')).json();
  const html = await (await fetch(base + '/game/index.html')).text();
  assert.match(html, new RegExp(`build\\s+${config.buildCommit}`));
  const files = await import('node:fs/promises');
  const buildsRoot = resolve(import.meta.dirname, '../../../../unity-client/Builds');
  const buildDir = (await files.readdir(buildsRoot)).find(name => name === config.buildName);
  assert.ok(buildDir, 'Build folder matching served fingerprint not found');
  const build = resolve(buildsRoot, buildDir);
  const names = await files.readdir(resolve(build, 'Build'));
  for (const suffix of ['.data', '.wasm', '.loader.js']) {
    const name = names.find(n => n.endsWith(suffix) || n.endsWith(`${suffix}.br`) || n.endsWith(`${suffix}.gz`));
    assert.ok(name, suffix);
    const response = await fetch(`${base}/game/Build/${name}`);
    assert.equal(response.status, 200);
    assert.equal(response.headers.get('content-type'), suffix === '.wasm' ? 'application/wasm' : suffix === '.loader.js' ? 'text/javascript; charset=utf-8' : 'application/octet-stream');
    await response.arrayBuffer();
  }
  const status = await (await fetch(base + '/api/status')).json();
  assert.equal(status.status, 'ok');
  pass('Hub and AG WebGL assets: fingerprint, data, wasm MIME, server status');
  return config;
}
async function room(size) {
  const clients = [];
  try {
    for (let i = 0; i < size; i++) clients.push(await open(`lab${size}_${uid}_${i}`));
    if (size === 2) await shot(clients[0].page, '01_entry');
    await clients[0].page.locator('#create').click();
    await clients[0].page.waitForFunction(() => /^HM\d{4}$/.test(document.querySelector('#lobbyCode')?.textContent || ''));
    const code = await clients[0].page.locator('#lobbyCode').innerText();
    assert.match(code, /^HM\d{4}$/);
    for (let i = 1; i < size; i++) {
      await clients[i].page.locator('#roomCode').fill(code);
      await clients[i].page.locator('#join').click();
    }
    await clients[0].page.waitForFunction(n => document.querySelector('#rosterCount')?.textContent.startsWith(`${n}/`), size);
    if (size === 2) await shot(clients[0].page, '02_lobby');
    assert.equal(await clients[0].page.locator('#lobbyPlayers .card').count(), 6);
    if (size === 1) {
      assert.equal(await clients[0].page.locator('#hostStart').isDisabled(), true);
      pass('N=1 remains in lobby with start disabled');
      return;
    }
    await clients[0].page.locator('#hostStart').click();
    for (let round = 1; round <= 5; round++) {
      for (const client of clients) {
        await client.page.waitForFunction(r => document.querySelector('#phase')?.textContent.startsWith(`Vòng ${r} ·`) || !document.querySelector('#result').classList.contains('hidden'), round, { timeout: 12000 });
      }
      if (await clients[0].page.locator('#result').isVisible()) break;
      if (size === 2 && round === 1) {
        await shot(clients[0].page, '03_placing');
        await clients[1].page.locator('#arena').click({ position: { x: 140, y: 270 } });
        const trace = clients[1].transcript.findLast(entry => entry.direction === 'out' && entry.message.type === 'commit');
        assert.ok(trace, 'Client B must commit');
        const inbound = clients[0].transcript.filter(entry => entry.direction === 'in' && entry.message.type !== 'reveal' && entry.message.type !== 'round_result');
        assert.equal(inbound.some(entry => JSON.stringify(entry.message).includes(String(trace.message.positionX))), false, 'Prelock position leaked');
        await clients[0].page.locator('#emote').selectOption('happy');
        await clients[1].page.waitForFunction(() => document.querySelector('#hpCards')?.textContent.includes('happy'));
        pass('Prelock opponent coordinates hidden; emote visible on card');
      }
      if (size === 2) {
        await placeAndAim(clients[0], 'left');
        await placeAndAim(clients[1], 'right');
        if (round === 1) await shot(clients[0].page, '03b_aiming');
      }
      const started = Date.now();
      for (const client of clients) await client.page.locator('#lockReady').click();
      await clients[0].page.waitForFunction(r => document.querySelector('#result').classList.contains('hidden') === false || document.querySelector('#phase')?.textContent.startsWith(`Vòng ${r+1} ·`), round, { timeout: 12000 });
      latencies.push(Date.now() - started);
    }
    await clients[0].page.locator('#result').waitFor({ state: 'visible', timeout: 15000 });
    assert.equal(await clients[0].page.locator('#ranking tr').count(), size);
    const received = clients[0].transcript.filter(entry => entry.direction === 'in').map(entry => entry.message);
    const rounds = received.filter(message => message.type === 'round_result');
    const end = received.find(message => message.type === 'match_end');
    assert.ok(rounds.length >= 1 && rounds.length <= 5);
    assert.ok(end && end.winners.length >= 1);
    const finalHp = new Map(rounds.at(-1).players.map(p => [p.id, p.hp]));
    const maxHp = Math.max(...finalHp.values());
    assert.ok(end.winners.every(id => finalHp.get(id) === maxHp));
    assert.ok(await clients[0].page.locator('#hpCards .card').count() === size);
    if (size === 2) await shot(clients[0].page, '04_result');
    pass(`N=${size} full real-WebSocket match, ${rounds.length} rounds, ${size} ranking rows`);
  } finally { await closeAll(clients); counter++; }
}
async function quickMatch() {
  const clients = [await open(`quick_${uid}_a`), await open(`quick_${uid}_b`)];
  try {
    await clients[0].page.locator('#quick').click();
    await clients[0].page.waitForFunction(() => document.querySelector('#notice')?.textContent.includes('Đang ghép'));
    await clients[1].page.locator('#quick').click();
    await clients[0].page.locator('#lobbyCode').waitFor({ state: 'visible', timeout: 10000 });
    assert.equal(await clients[0].page.locator('#rosterCount').innerText(), '2/6 người thật');
    pass('Two real clients quick-match into the same room');
  } finally { await closeAll(clients); }
}
async function reconnect() {
  const client = await open(`reconnect_${uid}`);
  try {
    await client.page.locator('#create').click();
    await client.page.waitForFunction(() => /^HM\d{4}$/.test(document.querySelector('#lobbyCode')?.textContent || ''));
    const room = await client.page.locator('#lobbyCode').innerText();
    await client.page.locator('#disconnect').click();
    await client.page.waitForFunction(() => (document.querySelector('#log')?.textContent.match(/"type":"room_state"/g) || []).length >= 2, null, { timeout: 10000 });
    const states = client.transcript.filter(e => e.direction === 'in' && e.message.type === 'room_state');
    assert.ok(states.length >= 2);
    assert.equal(states.at(-1).message.roomCode, room);
    pass('Reconnect token rejoins the same room within 30 seconds');
  } finally { await client.context.close(); }
}
try {
  const config = await testBuild();
  await room(1);
  await room(2);
  await room(3);
  await room(6);
  await quickMatch();
  await reconnect();
  const avg = Math.round(latencies.reduce((a,b)=>a+b,0)/latencies.length);
  const max = Math.max(...latencies);
  const source = { at: new Date().toISOString(), hub: base, buildCommit: config.buildCommit, serverCommit: config.serverCommit,
    protocol: config.protocol, browser: 'Playwright Chromium', checks, roundLatencyMs: { average: avg, maximum: max, samples: latencies.length }, screenshots };
  await writeFile(resolve(evidence, 'smoke_local.json'), JSON.stringify(source, null, 2));
  await writeFile(resolve(evidence, 'smoke_local.txt'), `${checks.map(s => 'PASS '+s).join('\n')}\nPassed! ${checks.length}/${checks.length}\nRound broadcast latency measured from last lock request: avg ${avg} ms, max ${max} ms\n`);
  process.stdout.write(`Passed! ${checks.length}/${checks.length}\nLatency average ${avg} ms, maximum ${max} ms\n`);
} catch (error) {
  process.stderr.write(`FAILED after ${checks.length} checks: ${error.stack || error}\n`);
  process.exitCode = 1;
} finally { await browser.close(); }
