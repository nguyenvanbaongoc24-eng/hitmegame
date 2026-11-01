-- DRAFT ONLY; never apply to production without migration review and a local PostgreSQL test.
-- Risk: changes the match_players primary key so guest server IDs can be recorded.
-- Open decisions: account linking, retention, reward amounts, and rename costs.
-- The authoritative server currently uses InMemoryMatchResultStore and NoReward;
-- this migration does not enable persistence or award gold.

alter table public.matches
  add column protocol_version integer,
  add column config_version text;

alter table public.match_players
  add column server_player_id text,
  add column hits_landed integer not null default 0 check (hits_landed >= 0);

update public.match_players
  set server_player_id = player_id::text
  where server_player_id is null;

alter table public.match_players
  alter column server_player_id set not null,
  alter column player_id drop not null,
  drop constraint match_players_pkey,
  add constraint match_players_pkey primary key (match_id, server_player_id),
  add constraint match_players_rank_check check (placement >= 1),
  add constraint match_players_account_unique unique (match_id, player_id);

-- Call only with a server-held service role credential, never from a game client.
-- The first successful write wins. Repeating a matchId makes no changes.
create or replace function public.record_authoritative_match(
  p_match_id uuid,
  p_ended_at timestamptz,
  p_protocol_version integer,
  p_config_version text,
  p_players jsonb
) returns boolean
language plpgsql security definer set search_path = public
as $$
declare
  inserted_id uuid;
  expected_count integer;
begin
  if p_match_id is null or p_ended_at is null or p_protocol_version is null or p_protocol_version < 1
     or p_config_version is null or char_length(p_config_version) not between 1 and 100
     or jsonb_typeof(p_players) is distinct from 'array' then
    raise exception 'invalid match result';
  end if;

  expected_count := jsonb_array_length(p_players);
  if expected_count not between 1 and 6 then
    raise exception 'invalid player count';
  end if;
  if exists (
    select 1 from jsonb_to_recordset(p_players)
      as p(player_id text, rank integer, hp integer, hits_landed integer)
    where p.player_id is null or char_length(p.player_id) not between 1 and 128
       or p.rank is null or p.rank < 1 or p.rank > expected_count
       or p.hp is null or p.hp < 0 or p.hits_landed is null or p.hits_landed < 0
  ) then
    raise exception 'invalid player result';
  end if;

  insert into public.matches(id, ended_at, status, protocol_version, config_version)
    values (p_match_id, p_ended_at, 'ended', p_protocol_version, p_config_version)
    on conflict (id) do nothing returning id into inserted_id;
  if inserted_id is null then
    return false;
  end if;

  insert into public.match_players
    (match_id, server_player_id, placement, final_hp, hits_landed, gold_reward)
    select p_match_id, p.player_id, p.rank, p.hp, p.hits_landed, 0
    from jsonb_to_recordset(p_players)
      as p(player_id text, rank integer, hp integer, hits_landed integer);
  return true;
end $$;

revoke all on function public.record_authoritative_match(uuid,timestamptz,integer,text,jsonb)
  from public, anon, authenticated;
grant execute on function public.record_authoritative_match(uuid,timestamptz,integer,text,jsonb)
  to service_role;
