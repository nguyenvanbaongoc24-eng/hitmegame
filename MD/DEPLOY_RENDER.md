# Render deployment draft for HIT ME

This is a deployment guide, not evidence of a live service. Signed guest identities and durable Supabase match rewards are supported when the server-only environment variables are configured. Keep those keys out of the WebGL client.

## R1 diagnosis (2026-11-02)

`https://hitmegame.onrender.com/healthz` timed out from this workstation after 35 seconds; the web fetch also could not reach it. This does not establish whether the Render service is deployed, asleep, or failing to start. The same source started locally with `PORT=18765`, logged `Now listening on: http://0.0.0.0:18765`, and `GET /healthz` returned HTTP 200 in 329 ms with `{"status":"ok","protocol":1}`. The health handler has no database dependency. `server/Dockerfile` publishes the .NET 8 app and starts `HitMe.Server.dll`; `server/render.yaml` uses that Dockerfile and `/healthz`. Docker is unavailable on this workstation, so the image itself remains unverified.

For the live diagnosis, open Render Dashboard → `hit-me-server` → Events and Logs. Confirm the Blueprint's deployed commit, successful image build, `PORT` binding, and health check result. From a terminal run `curl -i --max-time 90 https://hitmegame.onrender.com/healthz`; expect HTTP 200 and `status: ok`. If it still times out, compare the dashboard's actual service URL with this URL and inspect startup errors before changing server code.

1. Push and merge reviewed server changes to the repository branch intended for deployment.
2. In Render, create a Blueprint from that repository and select the custom Blueprint path `server/render.yaml`. Its Docker build context is the repository root and Dockerfile is `server/Dockerfile`.
3. Set `ALLOWED_ORIGINS` to the exact HTTPS origin of the WebGL site, such as `https://game.example.com` (no trailing slash). Set `GUEST_TOKEN_SIGNING_KEY` to a private, persistent base64 string of at least 32 random bytes. The Blueprint prompts for these values. Rotating this key invalidates existing guest tokens.
4. Render provides `PORT`; the server listens on `0.0.0.0:$PORT`. Health path is `/healthz`. Do not put service keys in the Blueprint or WebGL client.
5. Once the service is healthy, open `https://<render-host>/healthz` and verify `status: ok`.
6. Run `node tools/deploy/smoke_test.mjs wss://<render-host>/ws` from a network allowed by `ALLOWED_ORIGINS`; the smoke tool and browser origin admission may need separate configuration. Check the script's origin before running.
7. Set the Unity/WebGL online endpoint to `wss://<render-host>/ws` through a deployment configuration, then test 2–6 real clients, reconnect and an entire match. Keep the online feature flag off until these checks pass.

The optional `BAN_STORE_PATH` file persists AFK bans only on one host with durable storage. Render's ephemeral filesystem or multiple replicas require a shared store; this deployment draft therefore does not claim durable bans. `SUPABASE_URL` and a server-only secret key will be needed once the reviewed Supabase adapter and migrations exist. Never put a secret in a client build or Git.

Not verified here: Docker build, Render deployment, public WSS/TLS, and iPhone play against Render. The Supabase migration and local signed-guest wallet credit have separate evidence; those checks do not verify this deployment.
