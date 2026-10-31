// HIT ME deployment smoke test. Node.js 22+; no packages or secret required.
// Usage: node tools/deploy/smoke_test.mjs wss://YOUR_SERVER/ws
// For a local dry run: node tools/deploy/smoke_test.mjs --local
import assert from 'node:assert/strict';
import { createServer } from 'node:net';
import { spawn } from 'node:child_process';
import { resolve } from 'node:path';

const root = resolve(import.meta.dirname, '../..');
let server;
let address = process.argv[2];
if (address === '--local') {
  const probe = createServer();
  await new Promise(done => probe.listen(0, '127.0.0.1', done));
  const port = probe.address().port;
  await new Promise(done => probe.close(done));
  server = spawn('dotnet', [resolve(root, 'server/bin/Debug/net8.0/HitMe.Server.dll')], {
    cwd: root,
    env: { ...process.env, PORT: String(port) },
    windowsHide: true,
    stdio: 'ignore'
  });
  address = `ws://127.0.0.1:${port}/ws`;
  let healthy = false;
  for (let i = 0; i < 100; i++) {
    try { healthy = (await fetch(`http://127.0.0.1:${port}/healthz`)).ok; } catch { /* starting */ }
    if (healthy) break;
    await new Promise(done => setTimeout(done, 100));
  }
  assert.ok(healthy, 'Local server did not become healthy; build it first with dotnet build server/HitMe.Server.csproj');
}
if (!address || !/^wss?:\/\//.test(address)) {
  console.error('Usage: node tools/deploy/smoke_test.mjs wss://YOUR_SERVER/ws (or --local)');
  process.exitCode = 2;
} else {
  const bots = [];
  try {
    const unique = `${Date.now()}_${Math.random().toString(36).slice(2, 9)}`;
    for (let index = 0; index < 2; index++) bots.push(await connect(address));
    bots[0].send({ type: 'join', token: `guest:smoke_host_${unique}` });
    const room = await bots[0].take('room_state');
    assert.equal(room.protocolVersion, 1);
    assert.match(room.roomCode, /^HM\d{4}$/);
    bots[1].send({ type: 'join', token: `guest:smoke_guest_${unique}`, roomCode: room.roomCode });
    const joined = await bots[1].take('room_state');
    assert.equal(joined.players.length, 2);
    bots[0].send({ type: 'start' });
    let rounds = 0;
    let endings;
    while (!endings && rounds < 5) {
      const first = await bots[0].take(['round_start', 'match_end']);
      const second = await bots[1].take(first.type);
      if (first.type === 'match_end') { endings = [first, second]; break; }
      assert.equal(first.roundId, second.roundId);
      rounds++;
      bots[0].send({ type: 'commit', roundId: rounds, positionX: -2, positionY: -3, hasAim: true, aimX: -1, aimY: 0 });
      bots[1].send({ type: 'commit', roundId: rounds, positionX: 2, positionY: 3, hasAim: true, aimX: 1, aimY: 0 });
      await Promise.all(bots.map(bot => bot.take('room_state')));
      for (const bot of bots) bot.send({ type: 'lock_ready', ready: true });
      for (const bot of bots) {
        assert.equal((await bot.take('reveal')).roundId, rounds);
        assert.equal((await bot.take('round_result')).roundId, rounds);
      }
    }
    endings ??= await Promise.all(bots.map(bot => bot.take('match_end')));
    assert.deepEqual(endings[0].winners, endings[1].winners);
    assert.ok(rounds >= 1 && rounds <= 5);
    console.log(`Passed! 1/1 live WebSocket match, room=${room.roomCode}, rounds=${rounds}, winners=${endings[0].winners.length}`);
  } catch (error) {
    console.error('FAILED live WebSocket smoke test:', error);
    process.exitCode = 1;
  } finally {
    for (const bot of bots) bot.close();
    server?.kill();
  }
}

async function connect(url) {
  const socket = new WebSocket(url);
  const queue = [];
  const waiters = [];
  socket.addEventListener('message', event => {
    const packet = JSON.parse(event.data);
    const waiter = waiters.find(item => item.types.includes(packet.type) || packet.type === 'error');
    if (!waiter) { queue.push(packet); return; }
    waiters.splice(waiters.indexOf(waiter), 1);
    clearTimeout(waiter.timer);
    if (packet.type === 'error') waiter.reject(new Error(packet.code));
    else waiter.resolve(packet);
  });
  await new Promise((resolveOpen, rejectOpen) => {
    const timer = setTimeout(() => rejectOpen(new Error('WebSocket open timeout')), 10000);
    socket.addEventListener('open', () => { clearTimeout(timer); resolveOpen(); }, { once: true });
    socket.addEventListener('error', event => { clearTimeout(timer); rejectOpen(new Error(`WebSocket error: ${event.message ?? ''}`)); }, { once: true });
  });
  return {
    send: packet => socket.send(JSON.stringify({ v: 1, ...packet })),
    take: requested => {
      const types = Array.isArray(requested) ? requested : [requested];
      const index = queue.findIndex(packet => types.includes(packet.type) || packet.type === 'error');
      if (index >= 0) {
        const packet = queue.splice(index, 1)[0];
        return packet.type === 'error' ? Promise.reject(new Error(packet.code)) : Promise.resolve(packet);
      }
      return new Promise((resolvePacket, rejectPacket) => {
        const waiter = { types, resolve: resolvePacket, reject: rejectPacket };
        waiter.timer = setTimeout(() => { waiters.splice(waiters.indexOf(waiter), 1); rejectPacket(new Error(`Timeout waiting for ${types.join(' or ')}`)); }, 10000);
        waiters.push(waiter);
      });
    },
    close: () => socket.close()
  };
}
