-- Reviewed guest economy extension. Rename cost 200 gold approved 2026-11-02.
-- Risk: the server-only secret bypasses RLS; never expose it to WebGL.
-- Open decisions: durable account linking, character shop prices and rank points.
alter table public.guest_profiles
  add column display_name text,
  add column rename_count integer not null default 0 check (rename_count >= 0),
  add column equipped_weapon_id text not null default 'dep_to_ong',
  add column equipped_character_id text,
  add column is_test boolean not null default false;

create table public.guest_catalog (
  item_id text primary key,
  category text not null check (category in ('weapon','character')),
  price integer not null check (price >= 0),
  is_starter boolean not null default false
);
-- Snapshot from Unity Assets/Data/Items.json, commit d6770c9, version 1.
insert into public.guest_catalog(item_id,category,price,is_starter) values
 ('dep_to_ong','weapon',0,true),
 ('chao_ran','weapon',199,false),
 ('vot_pickleball','weapon',260,false),
 ('coc_bia_hoi','weapon',340,false),
 ('xe_dap_phuong_hoang','weapon',450,false),
 ('xe_may_honda_cub','weapon',1350,false),
 ('chu_cho_vang','weapon',1500,false);

create table public.guest_inventory (
  player_id text not null references public.guest_profiles(player_id) on delete cascade,
  item_id text not null references public.guest_catalog(item_id),
  acquired_at timestamptz not null default now(),
  primary key (player_id,item_id)
);
create table public.guest_shop_requests (
  player_id text not null references public.guest_profiles(player_id) on delete cascade,
  request_id uuid not null,
  action text not null,
  item_id text,
  new_name text,
  result jsonb not null,
  created_at timestamptz not null default now(),
  primary key (player_id,request_id)
);
create table public.guest_shop_settings (
  key text primary key,
  int_value integer not null check (int_value >= 0)
);
insert into public.guest_shop_settings(key,int_value) values ('rename_cost_after_first',200);

alter table public.guest_catalog enable row level security;
alter table public.guest_inventory enable row level security;
alter table public.guest_shop_requests enable row level security;
alter table public.guest_shop_settings enable row level security;
revoke all on public.guest_catalog, public.guest_inventory,
  public.guest_shop_requests, public.guest_shop_settings from public, anon, authenticated;
grant select on public.guest_catalog, public.guest_shop_settings to service_role;
grant select, insert on public.guest_inventory, public.guest_shop_requests to service_role;

create function public.guest_shop_action(
  p_player_id text, p_request_id uuid, p_action text,
  p_item_id text default null, p_new_name text default null
) returns jsonb
language plpgsql security definer set search_path = public, pg_temp
as $$
declare
  old_request public.guest_shop_requests%rowtype;
  item public.guest_catalog%rowtype;
  profile public.guest_profiles%rowtype;
  cost integer;
  result_value jsonb;
  cleaned_name text;
begin
  if p_player_id is null or p_player_id !~ '^g_[0-9a-f]{32}$'
     or p_request_id is null or p_action is null
     or p_action not in ('buy','equip','rename') then
    raise exception 'invalid_shop_request';
  end if;
  insert into public.guest_profiles(player_id) values(p_player_id)
    on conflict (player_id) do nothing;
  select * into profile from public.guest_profiles where player_id=p_player_id for update;
  select * into old_request from public.guest_shop_requests
    where player_id=p_player_id and request_id=p_request_id;
  if found then
    if old_request.action <> p_action or old_request.item_id is distinct from p_item_id
       or old_request.new_name is distinct from p_new_name then
      raise exception 'request_id_reused';
    end if;
    return old_request.result;
  end if;

  if p_action in ('buy','equip') then
    select * into item from public.guest_catalog where item_id=p_item_id;
    if not found then raise exception 'item_not_found'; end if;
    if p_action='buy' then
      if item.is_starter or exists(select 1 from public.guest_inventory
          where player_id=p_player_id and item_id=p_item_id) then
        raise exception 'already_owned';
      end if;
      update public.guest_profiles set gold=gold-item.price, updated_at=now()
        where player_id=p_player_id and gold>=item.price;
      if not found then raise exception 'insufficient_gold'; end if;
      insert into public.guest_inventory(player_id,item_id) values(p_player_id,p_item_id);
    else
      if not item.is_starter and not exists(select 1 from public.guest_inventory
          where player_id=p_player_id and item_id=p_item_id) then
        raise exception 'not_owned';
      end if;
      if item.category='weapon' then
        update public.guest_profiles set equipped_weapon_id=p_item_id,updated_at=now()
          where player_id=p_player_id;
      else
        update public.guest_profiles set equipped_character_id=p_item_id,updated_at=now()
          where player_id=p_player_id;
      end if;
    end if;
  else
    cleaned_name := btrim(p_new_name);
    if cleaned_name is null or char_length(cleaned_name) not between 3 and 24
       or cleaned_name ~ '[[:cntrl:]<>]' or cleaned_name ~* '(admin|moderator|fuck)' then
      raise exception 'invalid_name';
    end if;
    if profile.display_name = cleaned_name then raise exception 'same_name'; end if;
    select int_value into cost from public.guest_shop_settings
      where key='rename_cost_after_first';
    if cost is null then raise exception 'rename_cost_missing'; end if;
    if profile.rename_count=0 then cost := 0; end if;
    update public.guest_profiles set gold=gold-cost, display_name=cleaned_name,
      rename_count=rename_count+1, updated_at=now()
      where player_id=p_player_id and gold>=cost;
    if not found then raise exception 'insufficient_gold'; end if;
  end if;

  select jsonb_build_object('gold',gold,'displayName',display_name,
    'renameCount',rename_count,'equippedWeaponId',equipped_weapon_id,
    'equippedCharacterId',equipped_character_id) into result_value
    from public.guest_profiles where player_id=p_player_id;
  insert into public.guest_shop_requests(player_id,request_id,action,item_id,new_name,result)
    values(p_player_id,p_request_id,p_action,p_item_id,p_new_name,result_value);
  return result_value;
end $$;
revoke all on function public.guest_shop_action(text,uuid,text,text,text)
  from public, anon, authenticated;
grant execute on function public.guest_shop_action(text,uuid,text,text,text)
  to service_role;
