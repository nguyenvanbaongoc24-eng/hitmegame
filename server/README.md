# HIT ME authoritative server

S1–S2 are implemented and verified: in-memory rooms, protocol envelope validation, room codes, player limits, host start, server-clock placement windows, private commit replacement, Core/Phase1 resolution, and post-lock reveal/result broadcasts. It links only `Assets/Scripts/Core/Phase1` as the rule source.

Run: `dotnet run --project server/HitMe.Server.csproj`.
Test: `dotnet test server/Tests/HitMe.Server.Tests.csproj --nologo`.

Đang dở: S3 reconnect/AFK isolation, S4 input hardening and bot simulation, golden vectors, and deployment review. Do not deploy this WIP server.
