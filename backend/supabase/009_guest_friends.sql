-- Draft: server-only durable guest friend requests. Do not run until reviewed.
-- Guest identity is verified by the match server; SQL receives only server calls.
create table if not exists public.guest_friend_requests (
  from_player_id text not null references public.guest_profiles(player_id) on delete cascade,
  to_player_id text not null references public.guest_profiles(player_id) on delete cascade,
  status text not null check (status in ('pending','accepted','rejected')),
  updated_at timestamptz not null default now(),
  primary key (from_player_id,to_player_id),
  check (from_player_id <> to_player_id)
);
alter table public.guest_friend_requests enable row level security;
revoke all on public.guest_friend_requests from public, anon, authenticated;
grant select, insert, update on public.guest_friend_requests to service_role;

create or replace function public.guest_friend_action(
  p_action text, p_from text, p_to text, p_accepted boolean default null
) returns text language plpgsql security definer set search_path = public as $$
declare
  current_status text;
  inserted_count integer;
begin
  if auth.role() <> 'service_role' then raise exception 'server_only'; end if;
  if p_from !~ '^g_[0-9a-f]{32}$' or p_to !~ '^g_[0-9a-f]{32}$' or p_from = p_to then
    return 'invalid_friend_target';
  end if;
  if p_action = 'request' then
    if exists(select 1 from public.guest_friend_requests where
      ((from_player_id=p_from and to_player_id=p_to) or (from_player_id=p_to and to_player_id=p_from))
      and status='accepted') then return 'already_friends'; end if;
    insert into public.guest_profiles(player_id) values(p_from),(p_to) on conflict do nothing;
    insert into public.guest_friend_requests(from_player_id,to_player_id,status)
      values(p_from,p_to,'pending') on conflict (from_player_id,to_player_id) do nothing;
    get diagnostics inserted_count = row_count;
    if inserted_count = 1 then return 'ok'; end if;
    select status into current_status from public.guest_friend_requests
      where from_player_id=p_from and to_player_id=p_to;
    return case when current_status = 'pending' then 'friend_request_pending'
                else 'friend_request_closed' end;
  elsif p_action = 'reply' and p_accepted is not null then
    update public.guest_friend_requests
      set status=case when p_accepted then 'accepted' else 'rejected' end,
          updated_at=now()
      where from_player_id=p_from and to_player_id=p_to and status='pending';
    if found then return 'ok'; end if;
    return 'friend_request_not_found';
  end if;
  return 'invalid_friend_action';
end $$;
revoke all on function public.guest_friend_action(text,text,text,boolean) from public, anon, authenticated;
grant execute on function public.guest_friend_action(text,text,text,boolean) to service_role;
