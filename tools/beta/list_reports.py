"""List recent beta reports. Set SUPABASE_URL and SUPABASE_SECRET_KEY in the process environment."""
import argparse
import json
import os
import urllib.parse
import urllib.request
import urllib.error


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--version", default="")
    parser.add_argument("--limit", type=int, default=20)
    args = parser.parse_args()
    root = os.environ.get("SUPABASE_URL", "").rstrip("/")
    key = os.environ.get("SUPABASE_SECRET_KEY", "")
    if not root.startswith("https://") or not key.startswith("sb_secret_"):
        raise SystemExit("Set SUPABASE_URL and server-only SUPABASE_SECRET_KEY in the environment.")
    query = {"select": "id,created_at,client_version,build_id,browser,screen,room_code,note,recent_logs",
             "order": "created_at.desc", "limit": str(max(1, min(args.limit, 100)))}
    if args.version:
        query["client_version"] = "eq." + args.version
    url = root + "/rest/v1/bug_reports?" + urllib.parse.urlencode(query)
    request = urllib.request.Request(url, headers={"apikey": key})
    try:
        with urllib.request.urlopen(request, timeout=10) as response:
            rows = json.load(response)
    except urllib.error.HTTPError as error:
        raise SystemExit(f"Supabase reports returned HTTP {error.code}.") from None
    print(json.dumps(rows, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
