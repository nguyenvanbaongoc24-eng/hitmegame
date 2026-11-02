-- Apply in the same transaction as 001-004 on a fresh project. The old
-- 003 award function is replaced here before any live caller can use it.
-- Server-only wallet credits still require verified auth.users identities.

alter table public.match_players
  add column reward_eligible boolean not null default true;

create table public.server_bans (
  player_id text primary key,
  banned_until timestamptz not null,
  updated_at timestamptz not null default now()
);
alter table public.server_bans enable row level security;
revoke all on public.server_bans from public, anon, authenticated;
grant select, insert, update, delete on public.server_bans to service_role;

create or replace function public.award_match_gold(p_match uuid) returns void
language plpgsql security definer set search_path = public
as $$
declare r record; amount integer; room_size integer;
begin
  select count(*) into room_size from public.match_players where match_id = p_match;
  if room_size not between 2 and 6 then raise exception 'invalid room size'; end if;
  if not exists (select 1 from public.matches where id = p_match and status = 'ended') then
    raise exception 'match not ended';
  end if;
  for r in select * from public.match_players
    where match_id = p_match and player_id is not null and reward_eligible
    order by server_player_id for update
  loop
    amount := case
      when room_size = 2 and r.placement = 1 then 100
      when room_size between 3 and 4 and r.placement = 1 then 150
      when room_size between 3 and 4 and r.placement = 2 then 80
      when room_size = 4 and r.placement = 3 then 40
      when room_size between 5 and 6 and r.placement = 1 then 200
      when room_size between 5 and 6 and r.placement = 2 then 120
      when room_size between 5 and 6 and r.placement = 3 then 60
      else 0 end;
    if amount = 0 then continue; end if;
    if exists (select 1 from public.wallet_transactions
      where idempotency_key = p_match::text || ':' || r.player_id::text || ':reward') then
      continue;
    end if;
    update public.wallets set gold = gold + amount, updated_at = now()
      where player_id = r.player_id;
    if not found then raise exception 'wallet missing for rewarded player'; end if;
    update public.match_players set gold_reward = amount
      where match_id = p_match and server_player_id = r.server_player_id;
    insert into public.wallet_transactions(player_id, delta, reason, idempotency_key)
      values(r.player_id, amount, 'match_reward',
        p_match::text || ':' || r.player_id::text || ':reward');
  end loop;
end $$;

revoke all on function public.award_match_gold(uuid) from public, anon, authenticated;
grant execute on function public.award_match_gold(uuid) to service_role;
