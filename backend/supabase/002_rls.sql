-- DRAFT ONLY. Review security-definer search_path and service-role deployment policy before applying.
alter table public.profiles enable row level security; alter table public.wallets enable row level security; alter table public.inventory enable row level security; alter table public.match_players enable row level security; alter table public.quest_progress enable row level security;
alter table public.matches enable row level security; alter table public.wallet_transactions enable row level security;
alter table public.characters enable row level security; alter table public.weapons enable row level security; alter table public.quests enable row level security;
create policy "own profile read" on public.profiles for select using (auth.uid()=id);
-- No direct profile update: rename/equipment changes must be reviewed RPCs.
create policy "own wallet read" on public.wallets for select using (auth.uid()=player_id);
create policy "own inventory read" on public.inventory for select using (auth.uid()=player_id);
create policy "own match rows read" on public.match_players for select using (auth.uid()=player_id);
create policy "own quest progress read" on public.quest_progress for select using (auth.uid()=player_id);
create policy "own transaction read" on public.wallet_transactions for select using (auth.uid()=player_id);
create policy "own match read" on public.matches for select using (exists (select 1 from public.match_players mp where mp.match_id=id and mp.player_id=auth.uid()));
create policy "catalog characters read" on public.characters for select using (true);
create policy "catalog weapons read" on public.weapons for select using (true);
create policy "catalog quests read" on public.quests for select using (true);
-- No client insert/update/delete policies on wallets, inventory, or match_players.
-- PostgreSQL service_role bypasses RLS; only the authoritative match service uses it.
revoke all on public.profiles, public.wallets, public.inventory, public.matches, public.match_players, public.quest_progress, public.wallet_transactions, public.characters, public.weapons, public.quests from anon, authenticated;
grant select on public.profiles, public.wallets, public.inventory, public.matches, public.match_players, public.quest_progress, public.wallet_transactions, public.characters, public.weapons, public.quests to authenticated;
