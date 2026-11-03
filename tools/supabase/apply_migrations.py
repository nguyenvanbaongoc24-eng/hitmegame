"""Apply HIT ME baseline SQL to an empty Supabase project, once, atomically.

Run: python Tools/supabase/apply_migrations.py --check
     python Tools/supabase/apply_migrations.py --apply
Reads SUPABASE_DB_URL from the process environment or ignored worktree .env.
Requires psycopg[binary]. Never prints the connection string or credentials.
The --check mode is read-only. --apply checks project identity, existing schema,
checksums, and takes a transaction-scoped lock before applying 001-010.
"""

from __future__ import annotations

import argparse
import hashlib
import os
from pathlib import Path
from urllib.parse import quote, unquote, urlsplit

PROJECT_REF = "lpylrbtpruyzdehkcaiu"
ROOT = Path(__file__).resolve().parents[2]
SOURCES = ROOT / "backend" / "supabase"
FILES = [
    "001_schema.sql", "002_rls.sql", "003_functions.sql",
    "004_match_result_store.sql", "005_reward_and_bans.sql",
    "006_guest_economy.sql", "007_leaderboard_rls.sql", "008_guest_shop.sql",
    "009_guest_friends.sql", "010_friend_role_guard.sql", "011_beta_reports.sql", "012_beta_events.sql",
]


def db_url() -> str:
    value = os.environ.get("SUPABASE_DB_URL")
    if not value:
        env_file = ROOT / ".env"
        if env_file.exists():
            for line in env_file.read_text(encoding="utf-8-sig").splitlines():
                if line.strip().startswith("SUPABASE_DB_URL="):
                    value = line.split("=", 1)[1].strip().strip('"').strip("'")
                    break
    if not value:
        raise RuntimeError("SUPABASE_DB_URL is missing from environment and ignored .env")
    # Passwords copied from the dashboard may contain raw @, [ or ] characters.
    # Escape only userinfo in memory; never rewrite or print the ignored .env.
    if "://" in value and "@" in value:
        scheme, authority = value.split("://", 1)
        userinfo, separator, host_and_path = authority.rpartition("@")
        if separator and ":" in userinfo:
            user, password = userinfo.split(":", 1)
            value = scheme + "://" + user + ":" + quote(unquote(password), safe="") + "@" + host_and_path
    parsed = urlsplit(value)
    username = unquote(parsed.username or "")
    if parsed.scheme not in ("postgres", "postgresql") or PROJECT_REF not in (parsed.hostname or "") + username:
        raise RuntimeError("Database URL does not identify the expected Supabase project")
    if parsed.port == 6543:
        raise RuntimeError("Use direct or session pooler port 5432 for migrations, not transaction pooler 6543")
    return value


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    mode = parser.add_mutually_exclusive_group(required=True)
    mode.add_argument("--check", action="store_true", help="inspect remote schema only")
    mode.add_argument("--apply", action="store_true", help="apply reviewed baseline migration")
    args = parser.parse_args()
    try:
        import psycopg
    except ImportError as exc:
        raise RuntimeError("Install psycopg[binary] before connecting") from exc

    connection = psycopg.connect(db_url(), sslmode="require", connect_timeout=10,
                                 application_name="hitme_migration")
    try:
        with connection.transaction():
            with connection.cursor() as cursor:
                cursor.execute("select current_database(), current_user, to_regclass('public.profiles'), to_regclass('public.hitme_schema_migrations')")
                database, user, profiles, ledger = cursor.fetchone()
                print(f"Connected: database={database}, role={user}, profiles_present={profiles is not None}, ledger_present={ledger is not None}")
                if args.check:
                    for table in ("profiles", "wallets", "matches", "match_players", "server_bans", "guest_profiles", "guest_match_credits", "guest_catalog", "guest_inventory", "guest_shop_requests", "guest_friend_requests"):
                        cursor.execute("select to_regclass(%s)", ("public." + table,))
                        print(f"{table}={cursor.fetchone()[0] is not None}")
                    return
                cursor.execute("select pg_advisory_xact_lock(hashtext('hitme_schema_migrations'))")
                if ledger is None:
                    if profiles is not None:
                        raise RuntimeError("Existing HIT ME tables without migration ledger: inspect manually; no write performed")
                    cursor.execute("""create table public.hitme_schema_migrations
                        (filename text primary key, sha256 text not null, applied_at timestamptz not null default now())""")
                    cursor.execute("revoke all on public.hitme_schema_migrations from public, anon, authenticated")
                    cursor.execute("grant select, insert on public.hitme_schema_migrations to service_role")
                for filename in FILES:
                    source = (SOURCES / filename).read_text(encoding="utf-8-sig")
                    digest = hashlib.sha256(source.encode("utf-8")).hexdigest()
                    cursor.execute("select sha256 from public.hitme_schema_migrations where filename=%s", (filename,))
                    row = cursor.fetchone()
                    if row:
                        if row[0] != digest:
                            raise RuntimeError(f"Migration checksum changed: {filename}; no write committed")
                        print(f"already applied: {filename}")
                        continue
                    cursor.execute(source)
                    cursor.execute("insert into public.hitme_schema_migrations(filename,sha256) values (%s,%s)", (filename, digest))
                    print(f"applied: {filename}")
        print(f"Migration transaction committed: {len(FILES)}/{len(FILES)} files checked")
    finally:
        connection.close()


if __name__ == "__main__":
    try:
        main()
    except Exception as exc:
        # Avoid driver tracebacks that can echo connection parameters.
        print(f"Migration stopped: {type(exc).__name__}: {exc if isinstance(exc, RuntimeError) else 'database error; inspect credentials/network privately'}")
        raise SystemExit(1) from None
