-- Beta reports are untrusted client text; only the match server may write/read.
-- Do not expose the service key in the WebGL client. Retention policy is pending.
create table if not exists public.bug_reports (
  id bigint generated always as identity primary key,
  created_at timestamptz not null default now(),
  client_version text not null,
  build_id text not null,
  browser text not null,
  screen text not null,
  room_code text,
  note text not null,
  recent_logs jsonb not null default '[]'::jsonb
);
alter table public.bug_reports enable row level security;
revoke all on public.bug_reports from public, anon, authenticated;
grant select, insert on public.bug_reports to service_role;
