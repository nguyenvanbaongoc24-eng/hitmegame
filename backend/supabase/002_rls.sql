-- DRAFT ONLY. Review security-definer search_path and service-role deployment policy before applying.
alter table public.profiles enable row level security; alter table public.wallets enable row level security; alter table public.inventory enable row level security; alter table public.match_players enable row level security; alter table public.quest_progress enable row level security;
create policy "own profile read" on public.profiles for select using (auth.uid()=id);
create policy "own profile update" on public.profiles for update using (auth.uid()=id) with check (auth.uid()=id);
create policy "own wallet read" on public.wallets for select using (auth.uid()=player_id);
create policy "own inventory read" on public.inventory for select using (auth.uid()=player_id);
create policy "own match rows read" on public.match_players for select using (auth.uid()=player_id);
create policy "own quest progress read" on public.quest_progress for select using (auth.uid()=player_id);
-- No client insert/update/delete policies on wallets, inventory, or match_players.
-- PostgreSQL service_role bypasses RLS; only the authoritative match service uses it.
revoke all on public.wallets, public.inventory, public.match_players, public.wallet_transactions from anon, authenticated;
grant select on public.profiles, public.wallets, public.inventory, public.match_players, public.quest_progress to authenticated;
