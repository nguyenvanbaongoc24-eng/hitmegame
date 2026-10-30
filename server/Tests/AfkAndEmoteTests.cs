using System;
using System.Text.Json;
using HitMe.Core;
using HitMe.Server.Authority;
using HitMe.Server.Protocol;
using Xunit;

public sealed class AfkAndEmoteTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void TwoConnectedRoundsWithoutACommitEliminateAndBanThePlayer()
    {
        var clock = new FakeTimeProvider(Start);
        var bans = new InMemoryBanStore();
        var session = new ServerRoundSession(Config(), new[] { "a", "b" }, new AfkPolicy(), bans);
        session.BeginRound(clock);

        CompleteRound(session, "a"); // b misses once.
        CompleteRound(session, "a"); // b misses twice and is eliminated before Core resolves.

        Assert.False(session.Match.State.FindPlayer("b")!.IsAlive);
        Assert.Equal(Phase.MatchEnd, session.Match.State.Phase);
        Assert.True(bans.TryGetRemaining("b", Start, out var remaining));
        Assert.Equal(120, remaining);
    }

    [Fact]
    public void AValidCommitResetsTheAfkCounter()
    {
        var bans = new InMemoryBanStore();
        var session = new ServerRoundSession(Config(), new[] { "a", "b" }, new AfkPolicy(), bans);
        session.BeginRound(new FakeTimeProvider(Start));

        CompleteRound(session, "a");
        CompleteRound(session, "a", "b");
        CompleteRound(session, "a");

        Assert.True(session.Match.State.FindPlayer("b")!.IsAlive);
        Assert.False(bans.TryGetRemaining("b", Start, out _));
    }

    [Fact]
    public void DisconnectDoesNotCountAsAfkAndExpiryDoesNotBan()
    {
        var bans = new InMemoryBanStore();
        var session = new ServerRoundSession(Config(), new[] { "a", "b" }, new AfkPolicy(), bans);
        session.BeginRound(new FakeTimeProvider(Start));
        session.SetConnected("b", false);

        CompleteRound(session, "a");
        CompleteRound(session, "a");
        Assert.True(session.Match.State.FindPlayer("b")!.IsAlive);
        Assert.False(bans.TryGetRemaining("b", Start, out _));

        session.EliminateDisconnected("b");
        Assert.False(session.Match.State.FindPlayer("b")!.IsAlive);
        Assert.False(bans.TryGetRemaining("b", Start, out _));
    }

    [Fact]
    public void BanBlocksJoinUntilItsServerClockExpires()
    {
        var clock = new FakeTimeProvider(Start);
        var bans = new InMemoryBanStore();
        bans.Ban("p1", Start + TimeSpan.FromSeconds(120));
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM0001", banStore: bans), clock, new FixedAuth("p1"));

        var blocked = Json(gateway.Handle("c1", Request("join")));
        Assert.Equal("banned", blocked.GetProperty("code").GetString());
        Assert.Equal(120, blocked.GetProperty("remainingSeconds").GetInt32());

        clock.Advance(TimeSpan.FromSeconds(120));
        Assert.Equal("room_state", Json(gateway.Handle("c1", Request("join"))).GetProperty("type").GetString());
    }

    [Fact]
    public void EmotesArePlacementOnlyRateLimitedAndNeverContainAimOrPosition()
    {
        var clock = new FakeTimeProvider(Start);
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM0001"), clock, new FixedAuth());
        gateway.Handle("c1", Request("join", "p1"));
        gateway.Handle("c2", Request("join", "p2", "HM0001"));
        gateway.HandleMessages("c1", Request("start", "p1"));

        var messages = gateway.HandleMessages("c1", JsonSerializer.Serialize(new { v = 1, type = "emote", emoteId = "happy" }));
        Assert.Equal(2, messages.Count);
        foreach (var message in messages)
        {
            var payload = Json(message.Payload);
            Assert.Equal("emote", payload.GetProperty("type").GetString());
            Assert.Equal("p1", payload.GetProperty("playerId").GetString());
            Assert.Equal("placing", payload.GetProperty("phase").GetString());
            Assert.DoesNotContain("position", message.Payload, StringComparison.Ordinal);
            Assert.DoesNotContain("aim", message.Payload, StringComparison.Ordinal);
        }
        Assert.Equal("emote_rate_limited", Json(gateway.Handle("c1", JsonSerializer.Serialize(new { v = 1, type = "emote", emoteId = "sad" }))).GetProperty("code").GetString());
        Assert.Equal("invalid_emote", Json(gateway.Handle("c2", JsonSerializer.Serialize(new { v = 1, type = "emote", emoteId = "dance" }))).GetProperty("code").GetString());

        clock.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(2, gateway.HandleMessages("c1", JsonSerializer.Serialize(new { v = 1, type = "emote", emoteId = "sad" })).Count);
        clock.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(2, gateway.HandleMessages("c1", JsonSerializer.Serialize(new { v = 1, type = "emote", emoteId = "angry" })).Count);
        clock.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal("emote_phase_limit", Json(gateway.Handle("c1", JsonSerializer.Serialize(new { v = 1, type = "emote", emoteId = "surprised" }))).GetProperty("code").GetString());
    }

    private static void CompleteRound(ServerRoundSession session, params string[] committed)
    {
        foreach (var playerId in committed)
            Assert.True(session.TryCommit(playerId, new RoundCommit(session.ActiveRound.RoundId, new Vec2(playerId == "a" ? -2 : 2, 0), new Vec2(playerId == "a" ? -1 : 1, 0)), out var error), error);
        foreach (var playerId in new[] { "a", "b" }) session.TryMarkReady(playerId, out _);
    }

    private static GameConfig Config() => new() { MaxPlayers = 2, PlaceSeconds = 5, NoAimRule = NoAimPolicy.ThrowFacing };
    private static string Request(string type, string playerId = "p1", string? roomCode = null) => JsonSerializer.Serialize(new { v = 1, type, token = "test:" + playerId, roomCode });
    private static JsonElement Json(string payload) => JsonDocument.Parse(payload).RootElement.Clone();

    private sealed class FixedAuth : IAuthVerifier
    {
        private readonly string fixedPlayer;
        public FixedAuth(string fixedPlayer = "") => this.fixedPlayer = fixedPlayer;
        public bool TryVerify(string? token, out PlayerIdentity identity, out string error)
        {
            var id = string.IsNullOrEmpty(fixedPlayer) ? token?["test:".Length..] : fixedPlayer;
            if (string.IsNullOrWhiteSpace(id) || token?.StartsWith("test:", StringComparison.Ordinal) != true) { identity = null!; error = "auth_invalid"; return false; }
            identity = new PlayerIdentity(id, id); error = string.Empty; return true;
        }
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private DateTimeOffset now;
        public FakeTimeProvider(DateTimeOffset now) => this.now = now;
        public override DateTimeOffset GetUtcNow() => now;
        public void Advance(TimeSpan value) => now += value;
    }
}
