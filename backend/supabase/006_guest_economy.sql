-- Server-only guest economy. Apply after 001-005 in the same transaction.
-- Risk: guest identities are only as durable as GUEST_TOKEN_SIGNING_KEY; rotate
-- it only with a migration plan. Account linking and retention remain undecided.
-- Approved 2026-11-02: 2 players 100/0; 3-4 150/80/40; 5-6 200/120/60.

create table public.guest_profiles (
  player_id text primary key check (player_id ~ '^g_[0-9a-f]{32}$'),
  gold integer not null default 0 check (gold >= 0),
  matches_played integer not null default 0 check (matches_played >= 0),
  best_rank integer not null default 0 check (best_rank between 0 and 6),
  updated_at timestamptz not null default now()
);
create table public.guest_match_credits (
  match_id uuid not null references public.matches(id) on delete cascade,
  player_id text not null references public.guest_profiles(player_id),
  gold integer not null check (gold >= 0),
  created_at timestamptz not null default now(),
  primary key (match_id, player_id)
);
alter table public.guest_profiles enable row level security;
alter table public.guest_match_credits enable row level security;
revoke all on public.guest_profiles, public.guest_match_credits from public, anon, authenticated;
grant select, insert, update on public.guest_profiles to service_role;
grant select, insert on public.guest_match_credits to service_role;

create or replace function public.record_guest_match(
  p_match_id uuid,
  p_ended_at timestamptz,
  p_protocol_version integer,
  p_config_version text,
  p_players jsonb
) returns boolean
language plpgsql security definer set search_path = public, pg_temp
as $$
declare
  room_size integer;
  row_player record;
  amount integer;
begin
  if jsonb_typeof(p_players) is distinct from 'array' then
    raise exception 'invalid guest match';
  end if;
  room_size := jsonb_array_length(p_players);
  if room_size not between 2 and 6 then raise exception 'invalid room size'; end if;
  if exists (
    select 1 from jsonb_to_recordset(p_players)
      as p(player_id text, rank integer, hp integer, hits_landed integer, reward_eligible boolean)
    where p.player_id is null or p.player_id !~ '^g_[0-9a-f]{32}$'
       or p.rank is null or p.rank not between 1 and room_size
       or p.reward_eligible is null
  ) then raise exception 'invalid guest player'; end if;
  if (select count(distinct p.player_id) from jsonb_to_recordset(p_players)
      as p(player_id text)) <> room_size then raise exception 'duplicate guest player'; end if;

  if not public.record_authoritative_match(p_match_id, p_ended_at,
      p_protocol_version, p_config_version, p_players) then return false; end if;

  for row_player in
    select * from jsonb_to_recordset(p_players)
      as p(player_id text, rank integer, hp integer, hits_landed integer,
           reward_eligible boolean)
    order by p.player_id
  loop
    amount := case
      when not row_player.reward_eligible then 0
      when room_size = 2 and row_player.rank = 1 then 100
      when room_size between 3 and 4 and row_player.rank = 1 then 150
      when room_size between 3 and 4 and row_player.rank = 2 then 80
      when room_size between 3 and 4 and row_player.rank = 3 then 40
      when room_size between 5 and 6 and row_player.rank = 1 then 200
      when room_size between 5 and 6 and row_player.rank = 2 then 120
      when room_size between 5 and 6 and row_player.rank = 3 then 60
      else 0 end;
    insert into public.guest_profiles(player_id) values (row_player.player_id)
      on conflict (player_id) do nothing;
    insert into public.guest_match_credits(match_id, player_id, gold)
      values (p_match_id, row_player.player_id, amount);
    update public.guest_profiles
      set gold = gold + amount,
          matches_played = matches_played + 1,
          best_rank = case when best_rank = 0 then row_player.rank
                           else least(best_rank, row_player.rank) end,
          updated_at = now()
      where player_id = row_player.player_id;
    update public.match_players set gold_reward = amount,
        reward_eligible = row_player.reward_eligible
      where match_id = p_match_id and server_player_id = row_player.player_id;
  end loop;
  return true;
end $$;

revoke all on function public.record_guest_match(uuid,timestamptz,integer,text,jsonb)
  from public, anon, authenticated;
grant execute on function public.record_guest_match(uuid,timestamptz,integer,text,jsonb)
  to service_role;
