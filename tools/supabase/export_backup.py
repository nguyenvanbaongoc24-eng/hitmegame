"""Manual data backup: python tools/supabase/export_backup.py --output D:/secure/hitme.backup

Requires PostgreSQL pg_dump on PATH and SUPABASE_DB_URL in the environment or
ignored worktree .env. The connection string is never passed as a command argument.
The destination must be outside this Git worktree and must not already exist.
"""

import argparse
import os
import pathlib
import shutil
import subprocess
import tempfile
from urllib.parse import unquote, urlparse

ROOT = pathlib.Path(__file__).resolve().parents[2]
TABLES = (
    "profiles", "wallets", "inventory", "matches", "match_players",
    "quests", "quest_progress", "server_bans", "guest_profiles",
    "guest_match_credits", "guest_inventory", "guest_shop_requests",
)


def connection_url():
    value = os.environ.get("SUPABASE_DB_URL")
    if value:
        return value
    env_path = ROOT / ".env"
    if env_path.exists():
        for line in env_path.read_text(encoding="utf-8").splitlines():
            if line.startswith("SUPABASE_DB_URL="):
                return line.split("=", 1)[1].strip()
    raise RuntimeError("SUPABASE_DB_URL is not configured")


def main():
    parser = argparse.ArgumentParser(description="Export HIT ME Supabase data without echoing credentials")
    parser.add_argument("--output", type=pathlib.Path, required=True)
    args = parser.parse_args()
    destination = args.output.resolve()
    if destination.is_relative_to(ROOT):
        raise RuntimeError("Backup destination must be outside the Git worktree")
    if destination.exists():
        raise RuntimeError("Backup destination already exists; choose a new filename")
    binary = shutil.which("pg_dump")
    if binary is None:
        raise RuntimeError("pg_dump is not installed or not on PATH")
    parsed = urlparse(connection_url())
    if parsed.scheme not in ("postgres", "postgresql") or not parsed.hostname or not parsed.username:
        raise RuntimeError("SUPABASE_DB_URL is not a PostgreSQL connection URL")
    environment = os.environ.copy()
    environment.update(PGHOST=parsed.hostname, PGPORT=str(parsed.port or 5432),
                       PGDATABASE=parsed.path.lstrip("/") or "postgres",
                       PGUSER=unquote(parsed.username), PGPASSWORD=unquote(parsed.password or ""),
                       PGSSLMODE="require")
    destination.parent.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(dir=destination.parent) as temporary:
        staging = pathlib.Path(temporary) / "backup.dump"
        command = [binary, "--data-only", "--format=custom", "--no-owner", "--no-acl",
                   f"--file={staging}", *(f"--table=public.{table}" for table in TABLES)]
        result = subprocess.run(command, env=environment, stdout=subprocess.DEVNULL,
                                stderr=subprocess.DEVNULL, timeout=600, check=False)
        if result.returncode != 0 or not staging.is_file() or staging.stat().st_size == 0:
            raise RuntimeError(f"pg_dump failed (exit {result.returncode}); backup not published")
        staging.replace(destination)
    print(f"Backup written: {destination.name} ({destination.stat().st_size} bytes)")


if __name__ == "__main__":
    main()
