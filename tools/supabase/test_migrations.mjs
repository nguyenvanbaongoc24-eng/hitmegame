// Local SQL syntax and reward contract smoke test. PGlite is not Supabase.
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { resolve } from 'node:path';
import { PGlite } from '@electric-sql/pglite';

const root = resolve(import.meta.dirname, '../..');
const db = new PGlite();
const sql = statement => db.query(statement);
await db.exec('create schema auth; create table auth.users(id uuid primary key); create function auth.uid() returns uuid language sql as $$ select null::uuid $$;');
await db.exec('create role anon; create role authenticated; create role service_role;');
for (const filename of ['001_schema.sql', '002_rls.sql', '003_functions.sql',
  '004_match_result_store.sql', '005_reward_and_bans.sql']) {
  let source = await readFile(resolve(root, 'backend/supabase', filename), 'utf8');
  // PGlite does not ship Supabase's pgcrypto extension; modern PostgreSQL
  // provides gen_random_uuid() natively. The live migration retains the line.
  source = source.replace('create extension if not exists pgcrypto;', '');
  await db.exec(source);
  console.log(`parsed ${filename}`);
}

let cases = 0;
for (const [size, expected] of [
  [2, [100, 0]], [3, [150, 80, 0]], [4, [150, 80, 40, 0]],
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
console.log(`Passed! ${cases}/${cases} PGlite SQL and approved reward checks`);
await db.close();
