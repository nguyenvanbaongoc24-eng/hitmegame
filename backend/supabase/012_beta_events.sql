-- Anonymous beta operations snapshots only. No player ID, token, packet or position.
-- Retention and dashboard access remain operator decisions.
create table if not exists public.beta_events (
  id bigint generated always as identity primary key,
  recorded_at timestamptz not null default now(),
  server_version text not null,
  counters jsonb not null
);
alter table public.beta_events enable row level security;
revoke all on public.beta_events from public, anon, authenticated;
grant select, insert on public.beta_events to service_role;
