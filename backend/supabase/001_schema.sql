-- DRAFT ONLY: do not apply to a production Supabase project. Open decisions:
-- gold prices, match rewards, and rename limits require product approval.
create extension if not exists pgcrypto;

create table public.profiles (
  id uuid primary key references auth.users(id) on delete cascade,
  display_name text not null check (char_length(display_name) between 3 and 24),
  avatar_url text, equipped_character_key text not null default 'nam_lang',
  equipped_weapon_key text not null default 'dep', rename_count integer not null default 0,
  created_at timestamptz not null default now()
);
create table public.wallets (player_id uuid primary key references public.profiles(id) on delete cascade, gold integer not null default 0 check (gold >= 0), updated_at timestamptz not null default now());
create table public.characters (key text primary key, display_name text not null, price_gold integer not null check (price_gold >= 0), active boolean not null default true);
create table public.weapons (key text primary key, display_name text not null, price_gold integer not null check (price_gold >= 0), active boolean not null default true);
create table public.inventory (player_id uuid references public.profiles(id) on delete cascade, item_type text not null check (item_type in ('character','weapon')), item_key text not null, acquired_at timestamptz not null default now(), primary key (player_id,item_type,item_key));
create table public.matches (id uuid primary key default gen_random_uuid(), room_code text, seed bigint, started_at timestamptz not null default now(), ended_at timestamptz, status text not null check (status in ('running','ended','cancelled')));
create table public.match_players (match_id uuid references public.matches(id) on delete cascade, player_id uuid references public.profiles(id), placement integer, final_hp integer check (final_hp >= 0), gold_reward integer not null default 0, primary key(match_id,player_id));
create table public.quests (key text primary key, title text not null, target integer not null check(target > 0), reward_gold integer not null check(reward_gold >= 0), active boolean not null default true);
create table public.quest_progress (player_id uuid references public.profiles(id) on delete cascade, quest_key text references public.quests(key), progress integer not null default 0, claimed_at timestamptz, primary key(player_id,quest_key));
create table public.wallet_transactions (id uuid primary key default gen_random_uuid(), player_id uuid references public.profiles(id), delta integer not null, reason text not null, idempotency_key text unique, created_at timestamptz not null default now());
create view public.leaderboard as select p.id, p.display_name, coalesce(sum(mp.gold_reward),0)::bigint as lifetime_gold, count(mp.match_id)::bigint as matches_played from public.profiles p left join public.match_players mp on mp.player_id=p.id group by p.id,p.display_name;
