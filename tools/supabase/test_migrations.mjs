// Local SQL syntax and reward contract smoke test. PGlite is not Supabase.
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { resolve } from 'node:path';
import { PGlite } from '@electric-sql/pglite';

const root = resolve(import.meta.dirname, '../..');
const db = new PGlite();
const sql = statement => db.query(statement);
await db.exec('create schema auth; create table auth.users(id uuid primary key); create function auth.uid() returns uuid language sql as $$ select null::uuid $$; create function auth.role() returns text language sql as $$ select current_setting(\'request.jwt.claim.role\', true) $$;');
await db.exec('create role anon; create role authenticated; create role service_role;');
for (const filename of ['001_schema.sql', '002_rls.sql', '003_functions.sql',
  '004_match_result_store.sql', '005_reward_and_bans.sql',
  '006_guest_economy.sql', '007_leaderboard_rls.sql', '008_guest_shop.sql', '009_guest_friends.sql', '010_friend_role_guard.sql']) {
  let source = await readFile(resolve(root, 'backend/supabase', filename), 'utf8');
  // PGlite does not ship Supabase's pgcrypto extension; modern PostgreSQL
  // provides gen_random_uuid() natively. The live migration retains the line.
  source = source.replace('create extension if not exists pgcrypto;', '');
  await db.exec(source);
  console.log(`parsed ${filename}`);
}

let cases = 0;
for (const [size, expected] of [
  [2, [100, 0]], [3, [150, 80, 40]], [4, [150, 80, 40, 0]],
  [5, [200, 120, 60, 0, 0]], [6, [200, 120, 60, 0, 0, 0]]
]) {
  const matchId = `00000000-0000-4000-8000-${String(size).padStart(12, '0')}`;
  await sql(`insert into public.matches(id,status) values ('${matchId}','ended')`);
  for (let rank = 1; rank <= size; rank++) {
    const account = `10000000-0000-4000-8000-${String(size * 10 + rank).padStart(12, '0')}`;
    await sql(`insert into auth.users(id) values ('${account}')`);
    await sql(`insert into public.profiles(id,display_name) values ('${account}','Player ${rank}')`);
    await sql(`insert into public.wallets(player_id) values ('${account}')`);
    await sql(`insert into public.match_players(match_id,server_player_id,player_id,placement,final_hp,hits_landed)
      values ('${matchId}','guest_${rank}','${account}',${rank},1,0)`);
  }
  await sql(`select public.award_match_gold('${matchId}')`);
  await sql(`select public.award_match_gold('${matchId}')`);
  const rows = (await sql(`select mp.placement, w.gold, mp.gold_reward
    from public.match_players mp join public.wallets w on mp.player_id=w.player_id
    where mp.match_id='${matchId}' order by mp.placement`)).rows;
  assert.deepEqual(rows.map(row => Number(row.gold)), expected);
  assert.deepEqual(rows.map(row => Number(row.gold_reward)), expected);
  cases++;
}
const rls = (await sql(`select relrowsecurity from pg_class where oid='public.server_bans'::regclass`)).rows[0];
assert.equal(rls.relrowsecurity, true);
cases++;
const sixMatch = '00000000-0000-4000-8000-000000000006';
await sql(`update public.match_players set reward_eligible=false, gold_reward=0
  where match_id='${sixMatch}' and placement=1`);
await sql(`delete from public.wallet_transactions where idempotency_key like '${sixMatch}:%'`);
await sql(`update public.wallets set gold=0 where player_id in
  (select player_id from public.match_players where match_id='${sixMatch}')`);
await sql(`select public.award_match_gold('${sixMatch}')`);
const excluded = (await sql(`select gold from public.wallets where player_id=
  (select player_id from public.match_players where match_id='${sixMatch}' and placement=1)`)).rows[0];
assert.equal(Number(excluded.gold), 0);
cases++;
for (const [size, expected] of [
  [2, [100, 0]], [3, [150, 80, 40]], [4, [150, 80, 40, 0]],
  [5, [200, 120, 60, 0, 0]], [6, [200, 120, 60, 0, 0, 0]]
]) {
  const matchId = `20000000-0000-4000-8000-${String(size).padStart(12, '0')}`;
  const players = Array.from({ length: size }, (_, i) => ({
    player_id: `g_${String(size * 10 + i).padStart(32, '0')}`,
    rank: i + 1, hp: 3, hits_landed: 0, reward_eligible: i !== 0 || size !== 6
  }));
  const payload = JSON.stringify(players).replaceAll("'", "''");
  const call = `select public.record_guest_match('${matchId}',now(),1,'test','${payload}'::jsonb) as inserted`;
  assert.equal((await sql(call)).rows[0].inserted, true);
  assert.equal((await sql(call)).rows[0].inserted, false);
  const rows = (await sql(`select gp.gold,gp.matches_played,mp.gold_reward,mp.reward_eligible
    from public.match_players mp join public.guest_profiles gp on gp.player_id=mp.server_player_id
    where mp.match_id='${matchId}' order by mp.placement`)).rows;
  const rewards = size === 6 ? [0, ...expected.slice(1)] : expected;
  assert.deepEqual(rows.map(row => Number(row.gold)), rewards);
  assert.deepEqual(rows.map(row => Number(row.gold_reward)), rewards);
  assert.deepEqual(rows.map(row => Number(row.matches_played)), Array(size).fill(1));
  cases++;
}
assert.equal((await sql(`select relrowsecurity from pg_class where oid='public.guest_profiles'::regclass`)).rows[0].relrowsecurity, true);
cases++;
assert.equal((await sql(`select reloptions from pg_class where oid='public.leaderboard'::regclass`)).rows[0].reloptions.includes('security_invoker=true'), true);
cases++;
const shopper = 'g_' + 'f'.repeat(32);
const request = n => `30000000-0000-4000-8000-${String(n).padStart(12, '0')}`;
const action = (n, kind, item = null, name = null) => sql(`select public.guest_shop_action('${shopper}', '${request(n)}', '${kind}', ${item ? `'${item}'` : 'null'}, ${name ? `'${name}'` : 'null'}) as result`);
await sql(`insert into public.guest_profiles(player_id,gold) values ('${shopper}',500)`);
const bought = (await action(1, 'buy', 'chao_ran')).rows[0].result;
assert.equal(bought.gold, 301);
assert.equal((await action(1, 'buy', 'chao_ran')).rows[0].result.gold, 301);
assert.equal(Number((await sql(`select gold from public.guest_profiles where player_id='${shopper}'`)).rows[0].gold), 301);
cases++;
await action(2, 'equip', 'chao_ran');
assert.equal((await sql(`select equipped_weapon_id from public.guest_profiles where player_id='${shopper}'`)).rows[0].equipped_weapon_id, 'chao_ran');
cases++;
assert.equal((await action(3, 'rename', null, 'First Name')).rows[0].result.gold, 301);
assert.equal((await action(4, 'rename', null, 'Second Name')).rows[0].result.gold, 101);
cases++;
await assert.rejects(action(5, 'rename', null, 'Third Name'), /insufficient_gold/);
await assert.rejects(action(1, 'buy', 'vot_pickleball'), /request_id_reused/);
cases++;
await assert.rejects(action(6, 'buy', 'chao_ran'), /already_owned/);
await assert.rejects(action(7, 'equip', 'vot_pickleball'), /not_owned/);
cases++;
assert.equal((await sql(`select relrowsecurity from pg_class where oid='public.guest_inventory'::regclass`)).rows[0].relrowsecurity, true);
cases++;
const cleanupPreview = await readFile(resolve(root, 'backend/supabase/cleanup_test_data.sql'), 'utf8');
const executablePreview = cleanupPreview.replace(/^--.*$/gm, '');
assert.doesNotMatch(executablePreview, /\b(delete|update|truncate|drop|alter|insert|execute)\b/i);
const beforePreview = Number((await sql('select count(*)::int as n from public.guest_profiles')).rows[0].n);
const preview = (await db.query(cleanupPreview)).rows[0];
assert.deepEqual(Object.values(preview).map(Number), [0, 0, 0, 0, 0, 0]);
assert.equal(Number((await sql('select count(*)::int as n from public.guest_profiles')).rows[0].n), beforePreview);
cases++;
const friendA = 'g_' + 'a'.repeat(32);
const friendB = 'g_' + 'b'.repeat(32);
const friendAction = (action, from, to, accepted = 'null') =>
  sql(`select public.guest_friend_action('${action}','${from}','${to}',${accepted}) as code`);
await assert.rejects(friendAction('request', friendA, friendB), /server_only/);
cases++;
await sql(`select set_config('request.jwt.claim.role','service_role',false)`);
assert.equal((await friendAction('request', friendA, friendB)).rows[0].code, 'ok');
assert.equal((await friendAction('request', friendA, friendB)).rows[0].code, 'friend_request_pending');
assert.equal((await friendAction('reply', friendA, friendB, 'true')).rows[0].code, 'ok');
assert.equal((await friendAction('request', friendB, friendA)).rows[0].code, 'already_friends');
cases++;
assert.equal((await sql(`select relrowsecurity from pg_class where oid='public.guest_friend_requests'::regclass`)).rows[0].relrowsecurity, true);
cases++;
console.log(`Passed! ${cases}/${cases} PGlite SQL and approved reward checks`);
await db.close();
