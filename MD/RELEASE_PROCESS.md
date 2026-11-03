# HIT ME server beta release process

`prep/codex` is the development branch. `server-release` is a controlled deployment candidate branch, not proof that the client and Render work together. The owner keeps Render Auto-Deploy off during tests.

1. Before a release, run `dotnet test server/Tests/HitMe.Server.Tests.csproj --nologo`, the local SQL suite, and the WebSocket smoke test. Record the exact commit, test counts, protocol minimum and migration checksums. Verify that the Unity client understands `hello` and any new error codes.
2. Update `server-release` only by a reviewed fast-forward to that tested commit. Do not force push. Create an annotated tag `beta-0.1.N` on the same commit, with a short changelog covering protocol, schema, fixes and known limitations. Push the branch and tag.
3. In Render Settings → Build & Deploy, select `server-release` and leave Auto-Deploy off. Set `ALLOWED_ORIGINS`, Supabase variables, `SERVER_VERSION`, and `MIN_CLIENT_VERSION` in Render Environment. Start a manual deploy; verify `/healthz`, one room, reconnect, report submission and a full match. Publish the client only after protocol compatibility is confirmed.
4. Rollback: choose the prior known good `beta-0.1.N` commit for a manual deploy, then restore the matching WebGL client. Keep database migrations additive; do not roll back schema by deleting live player data. If compatibility is uncertain, set `MIN_CLIENT_VERSION=0.0.0` until the client is checked.
5. Freeze during the closed test. Ship only critical fixes, repeat checks and tag a new beta increment. Record tester visible changes and the affected build ID.

The current candidate has passed local tests and migration 011 on Supabase, but has **not** passed a Render deploy or the iPhone client handshake. Do not move Render to `server-release` yet.
