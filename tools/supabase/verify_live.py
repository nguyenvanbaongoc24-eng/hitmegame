"""Read live migration state and exercise guest rewards/shop in a rolled-back transaction.

Run with SUPABASE_DB_URL in the process environment or ignored .env:
    python tools/supabase/verify_live.py
No test match, guest profile, or gold remains after this command.
"""

from __future__ import annotations

import hashlib
import uuid

import psycopg
from psycopg.types.json import Jsonb

from apply_migrations import FILES, SOURCES, db_url


def main() -> None:
    connection = psycopg.connect(db_url(), sslmode="require", connect_timeout=10,
                                 application_name="hitme_migration_verify")
    match_ids = [uuid.uuid4() for _ in range(3)]
    try:
        with connection.cursor() as cursor:
            cursor.execute("select filename, sha256 from public.hitme_schema_migrations")
            applied = dict(cursor.fetchall())
            assert len(applied) == len(FILES), "migration count mismatch"
            for filename in FILES:
                source = (SOURCES / filename).read_text(encoding="utf-8-sig")
                assert applied.get(filename) == hashlib.sha256(source.encode("utf-8")).hexdigest(), (
                    f"migration checksum mismatch: {filename}")
            print(f"Migration ledger: {len(FILES)}/{len(FILES)} checksums match")

            for table in ("guest_profiles", "guest_match_credits", "server_bans"):
                cursor.execute("select relrowsecurity from pg_class where oid=%s::regclass",
                               ("public." + table,))
                assert cursor.fetchone()[0], f"RLS missing on {table}"
                cursor.execute("select has_table_privilege('anon', %s, 'SELECT')",
                               ("public." + table,))
                assert not cursor.fetchone()[0], f"anon can read {table}"
            print("RLS: 3/3 server-only tables enabled; anon cannot SELECT")
            cursor.execute("""select has_table_privilege('anon','public.leaderboard','SELECT'),
                has_table_privilege('authenticated','public.leaderboard','SELECT'),
                has_function_privilege('anon',
                  'public.record_guest_match(uuid,timestamptz,integer,text,jsonb)','EXECUTE')""")
            anon_leaderboard, auth_leaderboard, anon_reward_rpc = cursor.fetchone()
            print(f"Access audit: anon_leaderboard={anon_leaderboard}, "
                  f"authenticated_leaderboard={auth_leaderboard}, anon_reward_rpc={anon_reward_rpc}")
            assert not anon_leaderboard, "anon can read leaderboard view"
            assert auth_leaderboard, "authenticated leaderboard access missing"
            assert not anon_reward_rpc, "anon can execute reward RPC"

            ids = ["g_" + uuid.uuid4().hex for _ in range(3)]
            players = [dict(player_id=player_id, rank=index + 1,
                            hp=3 - index, hits_landed=2 - index,
                            reward_eligible=True)
                       for index, player_id in enumerate(ids)]
            params = (match_ids[0], 1, "live-rollback-check", Jsonb(players))
            query = "select public.record_guest_match(%s, now(), %s, %s, %s)"
            cursor.execute(query, params)
            assert cursor.fetchone()[0] is True, "first match write failed"
            cursor.execute(query, params)
            assert cursor.fetchone()[0] is False, "duplicate match write succeeded"
            cursor.execute("""select mp.placement, mp.gold_reward, gp.gold, gp.matches_played
                from public.match_players mp join public.guest_profiles gp
                  on gp.player_id = mp.server_player_id
                where mp.match_id = %s order by mp.placement""", (match_ids[0],))
            rows = cursor.fetchall()
            assert rows == [(1, 150, 150, 1), (2, 80, 80, 1), (3, 40, 40, 1)], rows
            print("Guest reward RPC: 3-player 150/80/40; duplicate is a no-op")

            for table in ("guest_catalog", "guest_inventory", "guest_shop_requests", "guest_shop_settings"):
                cursor.execute("select relrowsecurity from pg_class where oid=%s::regclass",
                               ("public." + table,))
                assert cursor.fetchone()[0], f"RLS missing on {table}"
                cursor.execute("select has_table_privilege('anon', %s, 'SELECT')",
                               ("public." + table,))
                assert not cursor.fetchone()[0], f"anon can read {table}"
            cursor.execute("""select has_function_privilege('anon',
                'public.guest_shop_action(text,uuid,text,text,text)','EXECUTE')""")
            assert not cursor.fetchone()[0], "anon can execute shop RPC"
            print("Shop RLS: 4/4 server-only tables enabled; anon cannot SELECT or execute shop RPC")

            for extra_match in match_ids[1:]:
                cursor.execute(query, (extra_match, 1, "live-rollback-check", Jsonb(players)))
                assert cursor.fetchone()[0] is True
            shop_query = "select public.guest_shop_action(%s,%s,%s,%s,%s)"
            player = ids[0]
            first_rename = (player, uuid.uuid4(), "rename", None, "Test Runner A")
            cursor.execute(shop_query, first_rename)
            renamed = cursor.fetchone()[0]
            assert (renamed["gold"], renamed["renameCount"]) == (450, 1), renamed
            cursor.execute(shop_query, first_rename)
            assert cursor.fetchone()[0] == renamed, "rename replay changed balance"
            purchase = (player, uuid.uuid4(), "buy", "chao_ran", None)
            cursor.execute(shop_query, purchase)
            bought = cursor.fetchone()[0]
            assert bought["gold"] == 251, bought
            cursor.execute(shop_query, purchase)
            assert cursor.fetchone()[0] == bought, "purchase replay charged twice"
            cursor.execute(shop_query, (player, uuid.uuid4(), "equip", "chao_ran", None))
            equipped = cursor.fetchone()[0]
            assert equipped["equippedWeaponId"] == "chao_ran" and equipped["gold"] == 251, equipped
            cursor.execute(shop_query, (player, uuid.uuid4(), "rename", None, "Test Runner B"))
            second = cursor.fetchone()[0]
            assert (second["gold"], second["renameCount"]) == (51, 2), second
            print("Shop RPC: first rename free, purchase 199, equip, second rename 200; replays unchanged")
    finally:
        connection.rollback()
        with connection.cursor() as cursor:
            cursor.execute("select count(*) from public.matches where id = any(%s)", (match_ids,))
            assert cursor.fetchone()[0] == 0, "test matches persisted after rollback"
        connection.rollback()
        connection.close()
    print("Verification transaction rolled back; no test rows retained")


if __name__ == "__main__":
    try:
        main()
    except Exception as exc:
        print(f"Verification failed: {type(exc).__name__}: {exc if isinstance(exc, AssertionError) else 'database error'}")
        raise SystemExit(1) from None
