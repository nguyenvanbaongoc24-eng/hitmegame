# Render deployment draft for HIT ME

This is a deployment guide, not evidence of a live service. The server still accepts only `guest:` identities; do not enable real-money or persistent rewards on this build.

1. Push and merge reviewed server changes to the repository branch intended for deployment.
2. In Render, create a Blueprint from that repository and select the custom Blueprint path `server/render.yaml`. Its Docker build context is the repository root and Dockerfile is `server/Dockerfile`.
3. Set `ALLOWED_ORIGINS` to the exact HTTPS origin of the WebGL site, such as `https://game.example.com` (no trailing slash). The Blueprint prompts for this value.
4. Render provides `PORT`; the server listens on `0.0.0.0:$PORT`. Health path is `/healthz`. Do not put service keys in the Blueprint or WebGL client.
5. Once the service is healthy, open `https://<render-host>/healthz` and verify `status: ok`.
6. Run `node tools/deploy/smoke_test.mjs wss://<render-host>/ws` from a network allowed by `ALLOWED_ORIGINS`; the smoke tool and browser origin admission may need separate configuration. Check the script's origin before running.
7. Set the Unity/WebGL online endpoint to `wss://<render-host>/ws` through a deployment configuration, then test 2–6 real clients, reconnect and an entire match. Keep the online feature flag off until these checks pass.

The optional `BAN_STORE_PATH` file persists AFK bans only on one host with durable storage. Render's ephemeral filesystem or multiple replicas require a shared store; this deployment draft therefore does not claim durable bans. `SUPABASE_URL` and a server-only secret key will be needed once the reviewed Supabase adapter and migrations exist. Never put a secret in a client build or Git.

Not verified here: Docker build, Render deployment, public WSS/TLS, Supabase schema, and iPhone play. The proposed live gold policy is approved, but crediting remains disabled until authenticated account mapping and atomic persistence are tested.
