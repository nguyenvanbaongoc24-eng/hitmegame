// Deployment verification without secrets. Node 22+.
// node tools/deploy/smoke_deployed.mjs https://hitmegame.onrender.com --origin https://hitmegame-iota.vercel.app
import assert from 'node:assert/strict';
import { randomBytes } from 'node:crypto';
import { connect as connectTcp } from 'node:net';
import { connect as connectTls } from 'node:tls';

const [base, flag, origin] = process.argv.slice(2);
if (!base || flag !== '--origin' || !origin) {
  console.error('Usage: node tools/deploy/smoke_deployed.mjs https://SERVER --origin https://CLIENT');
  process.exit(2);
}
const httpBase = new URL(base);
const wsUrl = new URL('/ws', httpBase);
wsUrl.protocol = wsUrl.protocol === 'https:' ? 'wss:' : 'ws:';
const report = { healthz: null, guestToken: false, wsHandshake: false, twoPlayerMatch: false, quickMatch: false, inviteLink: null };

try {
  const health = await fetchHealthWithRetry(httpBase, origin);
  assert.equal(health.status, 200, '/healthz');
  report.healthz = await health.json();
  assert.equal(report.healthz.originsConfigured, true, 'healthz.originsConfigured');
  assert.ok(report.healthz.allowedOriginsCount > 0, 'healthz.allowedOriginsCount');

  const token = async name => {
    const response = await fetch(new URL('/guest-token', httpBase), {
      method: 'POST', headers: { Origin: origin, 'Content-Type': 'application/json' }, body: JSON.stringify({ name })
    });
    assert.equal(response.status, 200, '/guest-token');
    assert.equal(response.headers.get('access-control-allow-origin'), origin, 'guest-token CORS');
    const body = await response.json();
    assert.ok(typeof body.token === 'string' && body.token.length > 20, 'signed guest token');
    return body.token;
  };
  const stamp = Date.now().toString(36);
  const firstToken = await token(`SmokeA${stamp}`);
  const secondToken = await token(`SmokeB${stamp}`);
  report.guestToken = true;

  const first = await connect(wsUrl.href, origin);
  report.wsHandshake = true;
  const second = await connect(wsUrl.href, origin);
  first.send({ type: 'join', token: firstToken, clientVersion: '0.1.0-beta.1' });
  const room = await first.take('room_state');
  report.inviteLink = `https://hitmegame-iota.vercel.app/next/?room=${encodeURIComponent(room.roomCode)}`;
  second.send({ type: 'join', token: secondToken, roomCode: room.roomCode, clientVersion: '0.1.0-beta.1' });
  await second.take('room_state');
  first.send({ type: 'start' });
  let round = 0;
  while (round < 8) {
    const start = await first.take(['round_start', 'match_end']);
    await second.take(start.type);
    if (start.type === 'match_end') break;
    round = start.roundId;
    first.send({ type: 'commit', roundId: round, positionX: -2, positionY: -3, hasAim: true, aimX: -1, aimY: 0 });
    second.send({ type: 'commit', roundId: round, positionX: 2, positionY: 3, hasAim: true, aimX: 1, aimY: 0 });
    await first.take('room_state'); await second.take('room_state');
    first.send({ type: 'lock_ready', ready: true }); second.send({ type: 'lock_ready', ready: true });
    await first.take('reveal'); await second.take('reveal');
    await first.take('round_result'); await second.take('round_result');
  }
  if (round >= 8) await Promise.all([first.take('match_end'), second.take('match_end')]);
  report.twoPlayerMatch = true;
  first.close(); second.close();

  const quickA = await connect(wsUrl.href, origin), quickB = await connect(wsUrl.href, origin);
  quickA.send({ type: 'quick_match', token: await token(`QuickA${stamp}`), clientVersion: '0.1.0-beta.1' });
  assert.equal((await quickA.take('quick_match_waiting')).quickMatchHintSeconds, 30);
  quickB.send({ type: 'quick_match', token: await token(`QuickB${stamp}`), clientVersion: '0.1.0-beta.1' });
  await quickA.take('room_state'); await quickB.take('room_state');
  report.quickMatch = true;
  quickA.close(); quickB.close();
  console.log(JSON.stringify(report));
  console.log('Passed! 6/6 healthz, guest-token, ws-handshake, 2-client-match, quick-match, invite-link');
} catch (error) {
  console.error(JSON.stringify({ ...report, error: error instanceof Error ? error.message : String(error) }));
  process.exitCode = 1;
}

async function fetchHealthWithRetry(base, requestOrigin) {
  let lastError;
  for (let attempt = 0; attempt < 60; attempt++) {
    try {
      const response = await fetch(new URL('/healthz', base), { headers: { Origin: requestOrigin } });
      if (response.ok) return response;
      lastError = new Error(`/healthz returned HTTP ${response.status}`);
    } catch (error) { lastError = error; }
    await new Promise(resolve => setTimeout(resolve, 1000));
  }
  throw new Error(`healthz unavailable after 60 seconds: ${lastError instanceof Error ? lastError.message : String(lastError)}`);
}

async function connect(url, requestOrigin) {
  // Global WebSocket in Node ignores caller-supplied Origin headers. This tiny
  // RFC 6455 client deliberately sends Origin, so it exercises browser admission.
  const endpoint = new URL(url);
  const tls = endpoint.protocol === 'wss:';
  const port = Number(endpoint.port || (tls ? 443 : 80));
  const socket = tls ? connectTls({ host: endpoint.hostname, port, servername: endpoint.hostname })
    : connectTcp({ host: endpoint.hostname, port });
  const queue = [], waiters = [];
  let input = Buffer.alloc(0), upgraded = false, closed = false;
  const fail = error => {
    if (closed) return; closed = true;
    for (const waiter of waiters.splice(0)) { clearTimeout(waiter.timer); waiter.reject(error); }
  };
  socket.on('error', fail);
  socket.on('close', () => fail(new Error('WebSocket closed unexpectedly')));
  socket.on('data', chunk => {
    input = Buffer.concat([input, chunk]);
    if (!upgraded) {
      const end = input.indexOf('\r\n\r\n');
      if (end < 0) return;
      const head = input.subarray(0, end).toString('ascii');
      input = input.subarray(end + 4);
      if (!head.startsWith('HTTP/1.1 101')) return fail(new Error(`WebSocket handshake failed: ${head.split('\r\n')[0]}`));
      upgraded = true;
    }
    readFrames();
  });
  await new Promise((resolve, reject) => {
    const timer = setTimeout(() => reject(new Error('WebSocket open timeout')), 15000);
    socket.once('connect', () => {
      const path = `${endpoint.pathname || '/'}${endpoint.search}`;
      const key = randomBytes(16).toString('base64');
      socket.write(`GET ${path} HTTP/1.1\r\nHost: ${endpoint.host}\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Key: ${key}\r\nSec-WebSocket-Version: 13\r\nOrigin: ${requestOrigin}\r\n\r\n`);
    });
    socket.once('secureConnect', () => {
      const path = `${endpoint.pathname || '/'}${endpoint.search}`;
      const key = randomBytes(16).toString('base64');
      socket.write(`GET ${path} HTTP/1.1\r\nHost: ${endpoint.host}\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Key: ${key}\r\nSec-WebSocket-Version: 13\r\nOrigin: ${requestOrigin}\r\n\r\n`);
    });
    const waitUpgrade = setInterval(() => { if (upgraded) { clearInterval(waitUpgrade); clearTimeout(timer); resolve(); } }, 5);
    socket.once('error', error => { clearInterval(waitUpgrade); clearTimeout(timer); reject(error); });
  });
  const hello = await take('hello'); assert.equal(hello.type, 'hello');
  return { send: packet => writeFrame(0x1, Buffer.from(JSON.stringify({ v: 1, ...packet }))), take, close: () => { if (!closed) { writeFrame(0x8, Buffer.alloc(0)); socket.end(); } } };

  function readFrames() {
    while (input.length >= 2) {
      const first = input[0], second = input[1], opcode = first & 0x0f; let length = second & 0x7f, offset = 2;
      if (length === 126) { if (input.length < 4) return; length = input.readUInt16BE(2); offset = 4; }
      else if (length === 127) return fail(new Error('Unexpected large WebSocket frame'));
      if (input.length < offset + length) return;
      const payload = input.subarray(offset, offset + length); input = input.subarray(offset + length);
      if (opcode === 0x9) { writeFrame(0xA, payload); continue; }
      if (opcode === 0x8) { closed = true; socket.end(); return; }
      if (opcode !== 0x1) continue;
      const packet = JSON.parse(payload.toString('utf8'));
      const waiter = waiters.find(item => item.types.includes(packet.type) || packet.type === 'error');
      if (!waiter) { queue.push(packet); continue; }
      waiters.splice(waiters.indexOf(waiter), 1); clearTimeout(waiter.timer);
      packet.type === 'error' ? waiter.reject(new Error(packet.code)) : waiter.resolve(packet);
    }
  }
  function writeFrame(opcode, payload) {
    const mask = randomBytes(4); const length = payload.length;
    if (length > 0xffff) throw new Error('Smoke packet too large');
    const extended = length >= 126; const offset = extended ? 8 : 6;
    const frame = Buffer.alloc(offset + length); frame[0] = 0x80 | opcode; frame[1] = 0x80 | (extended ? 126 : length);
    if (extended) frame.writeUInt16BE(length, 2);
    mask.copy(frame, offset - 4);
    for (let index = 0; index < length; index++) frame[offset + index] = payload[index] ^ mask[index % 4];
    socket.write(frame);
  }
  function take(wanted) {
    const types = Array.isArray(wanted) ? wanted : [wanted];
    const index = queue.findIndex(packet => types.includes(packet.type) || packet.type === 'error');
    if (index >= 0) { const packet = queue.splice(index, 1)[0]; return packet.type === 'error' ? Promise.reject(new Error(packet.code)) : Promise.resolve(packet); }
    return new Promise((resolve, reject) => {
      const waiter = { types, resolve, reject, timer: null };
      waiter.timer = setTimeout(() => { const i = waiters.indexOf(waiter); if (i >= 0) waiters.splice(i, 1); reject(new Error(`Timeout waiting for ${types.join(' or ')}`)); }, 15000);
      waiters.push(waiter);
    });
  }
}
