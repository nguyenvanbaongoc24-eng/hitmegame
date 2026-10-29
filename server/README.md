# HIT ME authoritative server

S1 is implemented and verified: in-memory rooms, protocol envelope validation, room codes, player limits, host start, and per-connection size/rate limits. It links only `Assets/Scripts/Core/Phase1` as the rule source.

Run: `dotnet run --project server/HitMe.Server.csproj`.
Test: `dotnet test server/Tests/HitMe.Server.Tests.csproj --nologo`.

Đang dở: T1 is being extended so each replay checks every important event and final `Lock()` state. After that, S2 will add private round commits, server-clock locking, Core `RoundResolver`, reveal/result broadcasts, and the mandatory secrecy/equivalence tests. Do not deploy this WIP server.
