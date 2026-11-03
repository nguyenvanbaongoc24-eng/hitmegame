"""Read anonymous beta snapshots by date. Requires SUPABASE_URL and SUPABASE_SECRET_KEY."""
import argparse
import json
import os
import urllib.error
import urllib.parse
import urllib.request


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--date", required=True, help="UTC date YYYY-MM-DD")
    args = parser.parse_args()
    root = os.environ.get("SUPABASE_URL", "").rstrip("/")
    key = os.environ.get("SUPABASE_SECRET_KEY", "")
    if not root.startswith("https://") or not key.startswith("sb_secret_"):
        raise SystemExit("Set SUPABASE_URL and server-only SUPABASE_SECRET_KEY.")
    query = urllib.parse.urlencode({"select": "recorded_at,server_version,counters",
        "recorded_at": "gte." + args.date + "T00:00:00Z", "order": "recorded_at.asc", "limit": "1000"})
    request = urllib.request.Request(root + "/rest/v1/beta_events?" + query, headers={"apikey": key})
    try:
        with urllib.request.urlopen(request, timeout=10) as response:
            rows = json.load(response)
    except urllib.error.HTTPError as error:
        raise SystemExit(f"Supabase metrics returned HTTP {error.code}.") from None
    rows = [row for row in rows if row["recorded_at"][:10] == args.date]
    if not rows:
        print(json.dumps({"date": args.date, "snapshots": 0}))
        return
    keys = ("matchesStarted", "matchesCompleted", "reconnects")
    first, last = rows[0]["counters"], rows[-1]["counters"]
    summary = {"date": args.date, "snapshots": len(rows), "serverVersions": sorted({r["server_version"] for r in rows}),
        "latestConnections": last.get("connections"), "latestRooms": last.get("rooms"),
        "latestActiveMatches": last.get("activeMatches"),
        "latestBroadcastP95Ms": last.get("broadcastP95Ms"),
        "latestRoundResolutionMedianMs": last.get("roundResolutionMedianMs")}
    summary.update({k: max(0, last.get(k, 0) - first.get(k, 0)) for k in keys})
    print(json.dumps(summary, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
