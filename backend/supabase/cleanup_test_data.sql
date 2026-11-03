-- PREVIEW ONLY. This file contains no DELETE/UPDATE/TRUNCATE statements.
-- Risk: guest IDs are opaque hashes; a name or timestamp is not proof that a
-- row is test data. Add only match IDs and guest player IDs verified from the
-- operator's own test log. Keep the allowlists empty until that review.
-- No live data is changed by running this query.

with candidate_matches(match_id) as (
  select null::uuid where false
  -- Replace the SELECT above with: values ('verified-match-uuid'::uuid)
), candidate_players(player_id) as (
  select null::text where false
  -- Replace the SELECT above with: values ('g_verified32hexcharacters'::text)
), candidate_reports(report_id) as (
  select null::bigint where false
  -- Replace the SELECT above with: values (verified_report_id::bigint)
)
select
  (select count(*) from candidate_matches) as selected_match_ids,
  (select count(*) from candidate_players) as selected_player_ids,
  (select count(*) from candidate_reports) as selected_report_ids,
  (select count(*) from public.matches m join candidate_matches c on c.match_id=m.id) as matching_matches,
  (select count(*) from public.match_players mp join candidate_matches c on c.match_id=mp.match_id) as matching_match_players,
  (select count(*) from public.guest_match_credits gc join candidate_matches c on c.match_id=gc.match_id) as matching_match_credits,
  (select count(*) from public.guest_profiles gp join candidate_players c on c.player_id=gp.player_id) as matching_guest_profiles,
  (select count(*) from public.guest_shop_requests sr join candidate_players c on c.player_id=sr.player_id) as matching_shop_requests,
  (select count(*) from public.guest_friend_requests fr join candidate_players c on c.player_id=fr.from_player_id or c.player_id=fr.to_player_id) as matching_friend_requests,
  (select count(*) from public.bug_reports br join candidate_reports c on c.report_id=br.id) as matching_bug_reports;
