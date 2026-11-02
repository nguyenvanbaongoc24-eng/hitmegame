-- The base leaderboard view was created without security_invoker. On Supabase,
-- anon inherited SELECT on the owner-rights view and could bypass profile RLS.
-- Keep authenticated access, but execute with each reader's RLS. A public
-- leaderboard needs a separately reviewed, privacy-safe projection later.
alter view public.leaderboard set (security_invoker = true);
revoke all on public.leaderboard from public, anon;
grant select on public.leaderboard to authenticated;
