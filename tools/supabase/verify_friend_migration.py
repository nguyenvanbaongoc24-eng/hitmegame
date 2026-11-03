"""Exercise live friend RPC inside a rollback-only transaction, without printing credentials.

Run: python tools/supabase/verify_friend_migration.py. Requires ignored .env
SUPABASE_DB_URL and psycopg. Never writes durable test users or friendships.
"""

from __future__ import annotations

import secrets

import psycopg

from apply_migrations import db_url


def main() -> int:
    connection = psycopg.connect(db_url(), sslmode="require", connect_timeout=10,
                                 application_name="hitme_friend_rollback_check")
    try:
        with connection.cursor() as cursor:
            cursor.execute("select to_regclass('public.guest_friend_requests') is not null")
            assert cursor.fetchone()[0], "migration 009 table missing"
            cursor.execute("select relrowsecurity from pg_class where oid='public.guest_friend_requests'::regclass")
            assert cursor.fetchone()[0], "RLS disabled"
            cursor.execute("select set_config('request.jwt.claim.role','',false)")
            cursor.execute("savepoint unauthenticated")
            try:
                cursor.execute("select public.guest_friend_action('request',%s,%s,null)",
                               ("g_" + secrets.token_hex(16), "g_" + secrets.token_hex(16)))
                raise AssertionError("empty role reached server-only RPC")
            except psycopg.Error:
                cursor.execute("rollback to savepoint unauthenticated")
            cursor.execute("select set_config('request.jwt.claim.role','service_role',false)")
            a, b = "g_" + secrets.token_hex(16), "g_" + secrets.token_hex(16)
            def call(action: str, from_id: str, to_id: str, accepted: bool | None = None) -> str:
                cursor.execute("select public.guest_friend_action(%s,%s,%s,%s)",
                               (action, from_id, to_id, accepted))
                return cursor.fetchone()[0]
            assert call("request", a, b) == "ok"
            assert call("request", a, b) == "friend_request_pending"
            assert call("reply", a, b, True) == "ok"
            assert call("request", b, a) == "already_friends"
            cursor.execute("select count(*) from public.guest_friend_requests where from_player_id=%s", (a,))
            assert cursor.fetchone()[0] == 1
        print("Passed! 7/7 live Supabase friend checks (transaction rolled back)")
        return 0
    finally:
        connection.rollback()
        connection.close()


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception:
        print("Live friend verification failed; no credentials or SQL values printed")
        raise SystemExit(1) from None
