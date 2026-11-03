// Node 22+; live low-volume load probe. No credentials or account tokens are logged.
// node server/Tools/RenderLoad.mjs wss://HOST/ws --seconds 600 --interval 30
import assert from 'node:assert/strict';

const endpoint = process.argv[2];
const option = (flag, fallback) => {
  const index = process.argv.indexOf(flag);
  return index < 0 ? fallback : Number(process.argv[index + 1]);
};
const seconds = option('--seconds', 600);
const intervalSeconds = option('--interval', 30);
if (!/^wss?:\/\//.test(endpoint ?? '') || !Number.isInteger(seconds) || seconds < 1 ||
    !Number.isInteger(intervalSeconds) || intervalSeconds < 1)
  throw new Error('Usage: RenderLoad.mjs wss://HOST/ws --seconds 600 --interval 30');

const site = new URL(endpoint);
site.protocol = site.protocol === 'wss:' ? 'https:' : 'http:';
site.pathname = '/metrics';
const delay = ms => new Promise(done => setTimeout(done, ms));
const metrics = async () => {
  try {
    const response = await fetch(site, { signal: AbortSignal.timeout(10000) });
    return response.ok ? await response.json() : null;
  } catch { return null; }
};
const samples = [];
const failures = [];
const started = Date.now();
const ending = started + seconds * 1000;
let cycles = 0;
let completed = 0;
let disconnects = 0;
let peakConnections = 0;
const before = await metrics();

while (Date.now() < ending) {
  const cycleStart = Date.now();
  const outcomes = await Promise.allSettled([2, 4, 6].map(count => playRoom(count, cycles)));
  for (const outcome of outcomes) {
    if (outcome.status === 'fulfilled') {
      completed++;
      samples.push(...outcome.value);
    } else failures.push(String(outcome.reason?.message ?? outcome.reason));
  }
  const snapshot = await metrics();
  if (snapshot) peakConnections = Math.max(peakConnections, snapshot.connections ?? 0);
  cycles++;
  console.log(`cycle=${cycles} completed=${completed} errors=${failures.length} elapsedSeconds=${Math.round((Date.now() - started) / 1000)}`);
  const untilNext = Math.min(ending - Date.now(), intervalSeconds * 1000 - (Date.now() - cycleStart));
  if (untilNext > 0) await delay(untilNext);
}

samples.sort((a, b) => a - b);
const average = samples.length ? samples.reduce((a, b) => a + b, 0) / samples.length : 0;
const percentile = p => samples.length ? samples[Math.ceil(samples.length * p) - 1] : 0;
const after = await metrics();
const summary = {
  endpoint: site.origin, durationSeconds: Math.round((Date.now() - started) / 1000),
  cycles, expectedMatches: cycles * 3, completedMatches: completed,
  expectedRounds: completed * 5, measuredRounds: samples.length,
  averageRoundMs: Math.round(average), p95RoundMs: percentile(0.95), maxRoundMs: samples.at(-1) ?? 0,
  droppedConnections: disconnects, errors: failures.length, firstErrors: failures.slice(0, 5),
  peakObservedConnections: peakConnections, before, after,
  cpuAndMemory: 'unavailable from public Render metrics endpoint'
};
console.log(JSON.stringify(summary));
if (failures.length || completed !== cycles * 3 || samples.length !== completed * 5) process.exitCode = 1;

async function playRoom(count, cycle) {
  const clients = [];
  let roomCode = 'not_created';
  let stage = 'connect';
  try {
    for (let i = 0; i < count; i++) clients.push(await connect());
    const roomTag = `load_${started}_${count}_${cycle}`;
    stage = 'create_room';
    clients[0].send({ type: 'join', token: `guest:${roomTag}_0` });
    const room = await clients[0].take('room_state');
    roomCode = room.roomCode;
    assert.equal(room.players.length, 1);
    for (let i = 1; i < count; i++) {
      stage = `join_${i}`;
      clients[i].send({ type: 'join', token: `guest:${roomTag}_${i}`, roomCode: room.roomCode });
      assert.equal((await clients[i].take('room_state')).players.length, i + 1);
    }
    stage = 'start';
    clients[0].send({ type: 'start' });
    const roundTimes = [];
    for (let round = 1; round <= 5; round++) {
      stage = `round_${round}_start`;
      const begins = await Promise.all(clients.map(client => client.take('round_start')));
      assert.ok(begins.every(begin => begin.roundId === round));
      stage = `round_${round}_commit`;
      for (let i = 0; i < count; i++) clients[i].send({
        type: 'commit', roundId: round, positionX: -3 + i * 1.2, positionY: 0,
        hasAim: true, aimX: 0, aimY: 1
      });
      // The server processes each socket's commit before its subsequent lock_ready.
      const lockAt = Date.now();
      clients.forEach(client => client.send({ type: 'lock_ready', ready: true }));
      stage = `round_${round}_reveal`;
      const reveals = await Promise.all(clients.map(client => client.take('reveal')));
      stage = `round_${round}_result`;
      const results = await Promise.all(clients.map(client => client.take('round_result')));
      assert.ok(reveals.every(reveal => reveal.roundId === round));
      assert.ok(results.every(result => result.roundId === round));
      roundTimes.push(Date.now() - lockAt);
    }
    stage = 'match_end';
    const endings = await Promise.all(clients.map(client => client.take('match_end')));
    assert.ok(endings.every(end => end.rankings?.length === count));
    return roundTimes;
  } catch (error) {
    throw new Error(`cycle=${cycle} size=${count} room=${roomCode} stage=${stage} cause=${error?.message ?? error}`);
  } finally {
    clients.forEach(client => client.close());
  }
}

async function connect() {
  const socket = new WebSocket(endpoint);
  let expectedClose = false;
  const queue = [];
  const waiters = [];
  let lastPacket = 'none';
  socket.addEventListener('message', event => {
    let packet;
    try { packet = JSON.parse(event.data); } catch { return; }
    lastPacket = `${packet.type ?? 'unknown'}:${packet.roundId ?? '-'}`;
    const waiter = waiters.find(item => item.type === packet.type || packet.type === 'error');
    if (!waiter) { queue.push(packet); return; }
    waiters.splice(waiters.indexOf(waiter), 1);
    clearTimeout(waiter.timer);
    packet.type === 'error' ? waiter.reject(new Error(packet.code)) : waiter.resolve(packet);
  });
  socket.addEventListener('close', () => {
    if (!expectedClose) disconnects++;
    for (const waiter of waiters.splice(0)) {
      clearTimeout(waiter.timer);
      waiter.reject(new Error(`socket_closed_waiting_${waiter.type}_after_${lastPacket}`));
    }
  });
  await new Promise((done, fail) => {
    const timer = setTimeout(() => fail(new Error('open_timeout')), 10000);
    socket.addEventListener('open', () => { clearTimeout(timer); done(); }, { once: true });
    socket.addEventListener('error', () => { clearTimeout(timer); fail(new Error('socket_error')); }, { once: true });
  });
  return {
    send: value => socket.send(JSON.stringify({ v: 1, ...value })),
    take: type => {
      const index = queue.findIndex(item => item.type === type || item.type === 'error');
      if (index >= 0) {
        const item = queue.splice(index, 1)[0];
        return item.type === 'error' ? Promise.reject(new Error(item.code)) : Promise.resolve(item);
      }
      return new Promise((resolve, reject) => {
        const waiter = { type, resolve, reject };
        waiter.timer = setTimeout(() => {
          waiters.splice(waiters.indexOf(waiter), 1);
          reject(new Error(`wait_timeout_${type}_after_${lastPacket}`));
        }, 15000);
        waiters.push(waiter);
      });
    },
    close: () => { expectedClose = true; socket.close(); }
  };
}
