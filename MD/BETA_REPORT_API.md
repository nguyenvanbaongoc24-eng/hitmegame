# Beta report API for the Unity client

`POST https://<server>/report` with `Content-Type: application/json`. Allowed browser Origins come from server `ALLOWED_ORIGINS`. Request body is at most 4096 bytes and five requests per minute per IP. Do not send identity tokens, passwords or personal information.

```json
{"clientVersion":"0.1.0-beta.1","buildId":"b123","browser":"Safari iOS","screen":"1179x2556","roomCode":"HM4826","note":"Nút kẹt","recentLogs":["room join failed: timeout"]}
```

`clientVersion` and `buildId` are required. `recentLogs` is limited to 50 lines of at most 200 characters each; note to 500 characters. Server redacts token/password/secret patterns before persistence. Success: HTTP 202. Errors: HTTP 400 `invalid_report`, 403 for disallowed Origin, 413 `report_too_large`, 429 rate limit, 503 `report_unavailable` if Supabase is not configured or write fails. Client should offer copy-to-clipboard on 503.

Migration `011_beta_reports.sql` creates a server-only `bug_reports` table. The owner reads reports with `python tools/beta/list_reports.py --version 0.1.0-beta.1` after setting `SUPABASE_URL` and `SUPABASE_SECRET_KEY` in the process environment. This is not a public endpoint. Retention and purge policy await a project decision.
