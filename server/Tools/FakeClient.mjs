// Node 22+: node server/Tools/FakeClient.mjs --local
// Or: node server/Tools/FakeClient.mjs wss://HOST/ws
// Plays three real WebSocket matches with 2, 4 and 6 guest connections.
import assert from 'node:assert/strict';
import { createServer } from 'node:net';
import { spawn } from 'node:child_process';
import { resolve } from 'node:path';

const root = resolve(import.meta.dirname, '../..');
let endpoint = process.argv[2];
let server;
if (endpoint === '--local') {
  const probe = createServer();
  await new Promise(done => probe.listen(0, '127.0.0.1', done));
  const port = probe.address().port;
  await new Promise(done => probe.close(done));
  server = spawn('dotnet', [resolve(root, 'server/bin/Debug/net8.0/HitMe.Server.dll')], {
    cwd: root, env: { ...process.env, PORT: String(port) }, windowsHide: true, stdio: 'ignore'
  });
  endpoint = `ws://127.0.0.1:${port}/ws`;
  let healthy = false;
  for (let attempt = 0; attempt < 100; attempt++) {
    try { healthy = (await fetch(`http://127.0.0.1:${port}/healthz`)).ok; } catch { /* waiting */ }
    if (healthy) break;
    await new Promise(done => setTimeout(done, 100));
  }
  assert.ok(healthy, 'Local server did not start; build server first');
}
if (!endpoint || !/^wss?:\/\//.test(endpoint)) throw new Error('Usage: FakeClient.mjs --local | wss://HOST/ws');

let passed = 0;
try {
  for (const count of [2, 4, 6]) {
    const clients = [];
    const issuedTokens = [];
    try {
      for (let i = 0; i < count; i++) clients.push(await connect(endpoint));
      const nonce = `${Date.now()}_${count}`;
      issuedTokens[0] = await tokenFor(`fake_${nonce}_0`);
      clients[0].send({ type: 'join', token: issuedTokens[0] });
      const room = await clients[0].take('room_state');
      assert.equal(room.protocolVersion, 1);
      assert.equal(room.players.length, 1);
      for (let i = 1; i < count; i++) {
        issuedTokens[i] = await tokenFor(`fake_${nonce}_${i}`);
        clients[i].send({ type: 'join', token: issuedTokens[i], roomCode: room.roomCode });
        const joined = await clients[i].take('room_state');
        assert.equal(joined.players.length, i + 1);
      }
      clients[0].send({ type: 'start' });
      let round = 0;
      let finalHp;
      let endings;
      while (round < 5 && !endings) {
        const starts = await Promise.all(clients.map(client => client.take(['round_start', 'match_end'])));
        if (starts[0].type === 'match_end') { endings = starts; break; }
        assert.ok(starts.every(start => start.type === 'round_start' && start.roundId === round + 1));
        round++;
        for (let i = 0; i < count; i++) {
          // No aim and separated positions make this a deterministic full-duration match.
          clients[i].send({ type: 'commit', roundId: round,
            positionX: -2 + i * 0.8, positionY: -3 + i * 0.9, hasAim: false });
        }
        await Promise.all(clients.map(client => client.take('room_state')));
        clients.forEach(client => client.send({ type: 'lock_ready', ready: true }));
        const reveals = await Promise.all(clients.map(client => client.take('reveal')));
        assert.ok(reveals.every(reveal => reveal.roundId === round && reveal.players.length === count));
        const results = await Promise.all(clients.map(client => client.take('round_result')));
        assert.ok(results.every(result => result.roundId === round && result.players.length === count));
        finalHp = results[0].players;
        assert.ok(results.every(result => JSON.stringify(result.players) === JSON.stringify(finalHp)));
        if (round === 5 || finalHp.filter(player => !player.eliminated).length <= 1)
          endings = await Promise.all(clients.map(client => client.take('match_end')));
      }
      assert.ok(endings && endings.every(end => JSON.stringify(end.winners) === JSON.stringify(endings[0].winners)));
      assert.ok(endings.every(end => end.rankings?.length === count));
      assert.ok(endings[0].rankings.every(row => row.rank >= 1 && row.rank <= count && row.hp >= 0));
      assert.ok(round >= 1 && round <= 5);
      assert.ok(endings[0].winners.every(id => finalHp.some(player => player.id === id)));
      const approvedFirstGold = count === 2 ? 100 : count <= 4 ? 150 : 200;
      const walletCredit = await verifyLiveCredits(issuedTokens, endings[0].rankings, count);
      console.log(`match N=${count} room=${room.roomCode} rounds=${round} winners=${endings[0].winners.length} approvedRank1Gold=${approvedFirstGold} walletCredit=${walletCredit}`);
      passed++;
    } finally { clients.forEach(client => client.close()); }
  }
  console.log(`Passed! ${passed}/3 real WebSocket matches`);
} finally { server?.kill(); }

async function tokenFor(name) {
  if (!process.env.GUEST_TOKEN_SIGNING_KEY) return `guest:${name}`;
  const url = new URL(endpoint);
  url.protocol = url.protocol === 'wss:' ? 'https:' : 'http:';
  url.pathname = '/guest-token';
  const response = await fetch(url, { method: 'POST', headers: { 'content-type': 'application/json' },
    body: JSON.stringify({ name }) });
  assert.equal(response.status, 200, `Guest token endpoint returned ${response.status}`);
  return (await response.json()).token;
}

async function verifyLiveCredits(tokens, rankings, size) {
  const key = process.env.SUPABASE_SECRET_KEY ?? process.env.SUPABASE_SERVICE_KEY;
  const project = process.env.SUPABASE_URL;
  if (!key || !project) return 'NOT_CONNECTED';
  assert.ok(key.startsWith('sb_secret_') || key.startsWith('eyJ'), 'Live credits require a server-only Supabase key');
  assert.ok(tokens.every(token => token.startsWith('guest1.')), 'Live credits require signed guest tokens');
  const ids = tokens.map(token => JSON.parse(Buffer.from(token.split('.')[1], 'base64url').toString('utf8')).id);
  for (const id of ids) {
    const ranking = rankings.find(row => row.playerId === id);
    assert.ok(ranking, `Missing result ranking for ${id}`);
    const url = new URL('/rest/v1/guest_profiles', project);
    url.searchParams.set('select', 'gold,matches_played,best_rank');
    url.searchParams.set('player_id', `eq.${id}`);
    const response = await fetch(url, { headers: { apikey: key } });
    assert.equal(response.status, 200, `Guest wallet query returned HTTP ${response.status}`);
    const rows = await response.json();
    assert.equal(rows.length, 1, `Missing guest wallet for ${id}`);
    const expected = size === 2 ? [100, 0] : size <= 4 ? [150, 80, 40, 0] : [200, 120, 60, 0, 0, 0];
    assert.equal(rows[0].gold, expected[ranking.rank - 1] ?? 0, `Wrong gold for ${id}`);
    assert.equal(rows[0].matches_played, 1, `Wrong match count for ${id}`);
    assert.equal(rows[0].best_rank, ranking.rank, `Wrong best rank for ${id}`);
  }
  return 'VERIFIED';
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
    if (packet.type === 'error') waiter.reject(new Error(packet.code)); else waiter.resolve(packet);
  });
  await new Promise((done, fail) => {
    const timer = setTimeout(() => fail(new Error('WebSocket open timeout')), 10000);
    socket.addEventListener('open', () => { clearTimeout(timer); done(); }, { once: true });
    socket.addEventListener('error', () => { clearTimeout(timer); fail(new Error('WebSocket open error')); }, { once: true });
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
      return new Promise((done, fail) => {
        const waiter = { types, resolve: done, reject: fail };
        waiter.timer = setTimeout(() => {
          waiters.splice(waiters.indexOf(waiter), 1);
          fail(new Error(`Timeout waiting for ${types.join(' or ')}`));
        }, 10000);
        waiters.push(waiter);
      });
    },
    close: () => socket.close()
  };
}
