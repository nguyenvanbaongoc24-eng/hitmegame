-- DRAFT ONLY. Reward amounts and rename costs are placeholders pending product approval.
create or replace function public.rename_player(new_name text) returns void language plpgsql security definer set search_path=public as $$
declare cost integer := 100; me uuid := auth.uid(); n integer;
begin
 if me is null or char_length(new_name) not between 3 and 24 then raise exception 'invalid rename'; end if;
 select rename_count into n from profiles where id=me for update;
 if n is null then raise exception 'profile missing'; end if;
 if n > 0 then update wallets set gold=gold-cost,updated_at=now() where player_id=me and gold>=cost; if not found then raise exception 'insufficient gold'; end if; insert into wallet_transactions(player_id,delta,reason) values(me,-cost,'rename'); end if;
 update profiles set display_name=new_name,rename_count=rename_count+1 where id=me;
end $$;
create or replace function public.award_match_gold(p_match uuid) returns void language plpgsql security definer set search_path=public as $$
declare r record; amount integer;
begin
 for r in select * from match_players where match_id=p_match for update loop
   amount := case r.placement when 1 then 100 when 2 then 60 when 3 then 30 else 0 end;
   if amount>0 and not exists(select 1 from wallet_transactions where idempotency_key=p_match::text||':'||r.player_id::text||':reward') then
     update wallets set gold=gold+amount,updated_at=now() where player_id=r.player_id;
     update match_players set gold_reward=amount where match_id=p_match and player_id=r.player_id;
     insert into wallet_transactions(player_id,delta,reason,idempotency_key) values(r.player_id,amount,'match_reward',p_match::text||':'||r.player_id::text||':reward');
   end if;
 end loop;
end $$;
create or replace function public.buy_item(p_type text,p_key text) returns void language plpgsql security definer set search_path=public as $$
declare me uuid:=auth.uid(); price integer;
begin
 if me is null or p_type not in ('character','weapon') then raise exception 'invalid purchase'; end if;
 if p_type='character' then select price_gold into price from characters where key=p_key and active; else select price_gold into price from weapons where key=p_key and active; end if;
 if price is null then raise exception 'unknown item'; end if;
 if exists(select 1 from inventory where player_id=me and item_type=p_type and item_key=p_key) then raise exception 'already owned'; end if;
 update wallets set gold=gold-price,updated_at=now() where player_id=me and gold>=price; if not found then raise exception 'insufficient gold'; end if;
 insert into inventory(player_id,item_type,item_key) values(me,p_type,p_key); insert into wallet_transactions(player_id,delta,reason) values(me,-price,'purchase:'||p_type||':'||p_key);
end $$;
revoke all on function public.rename_player(text), public.award_match_gold(uuid), public.buy_item(text,text) from public;
grant execute on function public.rename_player(text), public.buy_item(text,text) to authenticated;
grant execute on function public.award_match_gold(uuid) to service_role;
