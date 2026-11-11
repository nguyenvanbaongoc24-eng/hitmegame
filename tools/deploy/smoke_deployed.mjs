// Deployment verification without secrets. Node 22+.
// node tools/deploy/smoke_deployed.mjs https://hitmegame.onrender.com --origin https://hitmegame-iota.vercel.app
import assert from 'node:assert/strict';

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
  const health = await fetch(new URL('/healthz', httpBase), { headers: { Origin: origin } });
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
  const firstToken = await token(`smoke_a_${stamp}`);
  const secondToken = await token(`smoke_b_${stamp}`);
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
  quickA.send({ type: 'quick_match', token: await token(`quick_a_${stamp}`), clientVersion: '0.1.0-beta.1' });
  assert.equal((await quickA.take('quick_match_waiting')).quickMatchHintSeconds, 30);
  quickB.send({ type: 'quick_match', token: await token(`quick_b_${stamp}`), clientVersion: '0.1.0-beta.1' });
  await quickA.take('room_state'); await quickB.take('room_state');
  report.quickMatch = true;
  quickA.close(); quickB.close();
  console.log(JSON.stringify(report));
  console.log('Passed! 6/6 healthz, guest-token, ws-handshake, 2-client-match, quick-match, invite-link');
} catch (error) {
  console.error(JSON.stringify({ ...report, error: error instanceof Error ? error.message : String(error) }));
  process.exitCode = 1;
}

async function connect(url, requestOrigin) {
  // Node 22's WebSocket (undici) sends this browser-equivalent Origin header.
  const socket = new WebSocket(url, [], { headers: { Origin: requestOrigin } });
  const queue = [], waiters = [];
  socket.addEventListener('message', event => {
    const packet = JSON.parse(event.data);
    const waiter = waiters.find(item => item.types.includes(packet.type) || packet.type === 'error');
    if (!waiter) return void queue.push(packet);
    waiters.splice(waiters.indexOf(waiter), 1); clearTimeout(waiter.timer);
    packet.type === 'error' ? waiter.reject(new Error(packet.code)) : waiter.resolve(packet);
  });
  await new Promise((resolve, reject) => {
    const timer = setTimeout(() => reject(new Error('WebSocket open timeout')), 15000);
    socket.addEventListener('open', () => { clearTimeout(timer); resolve(); }, { once: true });
    socket.addEventListener('error', () => { clearTimeout(timer); reject(new Error('WebSocket handshake failed')); }, { once: true });
  });
  const hello = await take('hello');
  assert.equal(hello.type, 'hello');
  return { send: packet => socket.send(JSON.stringify({ v: 1, ...packet })), take, close: () => socket.close() };
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
