using System;
using System.Linq;
using HitMe.Core;
using HitMe.Server.Authority;
using Xunit;

public sealed class ServerRoundSessionTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void NoPreLockByteSentToAContainsBsCommit()
    {
        var session = Create();
        var sent = session.BeginRound(new FakeTimeProvider(Start));
        Assert.True(session.TryCommit("b", new RoundCommit(1, new Vec2(777.125f, -555.875f), new Vec2(0.3f, 0.7f)), out _));

        var toA = sent.Where(message => message.PlayerId == "a").Select(message => message.Payload);
        foreach (var payload in toA)
        {
            Assert.DoesNotContain("777.125", payload, StringComparison.Ordinal);
            Assert.DoesNotContain("-555.875", payload, StringComparison.Ordinal);
            Assert.DoesNotContain("\"position\"", payload, StringComparison.Ordinal);
            Assert.DoesNotContain("\"aim\"", payload, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void RevealAndResultAreBroadcastOnlyAfterAllPlayersLock()
    {
        var session = Create();
        session.BeginRound(new FakeTimeProvider(Start));
        Assert.True(session.TryCommit("a", new RoundCommit(1, new Vec2(-2, 0), new Vec2(1, 0)), out _));
        Assert.True(session.TryCommit("b", new RoundCommit(1, new Vec2(2, 0), new Vec2(-1, 0)), out _));

        var beforeLock = session.TryMarkReady("a", out _);
        Assert.Empty(beforeLock);
        var afterLock = session.TryMarkReady("b", out _);

        Assert.Equal(6, afterLock.Count);
        Assert.Equal(2, afterLock.Count(message => message.Payload.Contains("\"type\":\"reveal\"", StringComparison.Ordinal)));
        Assert.Equal(2, afterLock.Count(message => message.Payload.Contains("\"type\":\"round_result\"", StringComparison.Ordinal)));
        Assert.Equal(2, afterLock.Count(message => message.Payload.Contains("\"type\":\"round_start\"", StringComparison.Ordinal)));
        Assert.All(afterLock.Where(message => message.Payload.Contains("\"type\":\"reveal\"", StringComparison.Ordinal)),
            message => Assert.Contains("\"position\"", message.Payload, StringComparison.Ordinal));
    }

    [Fact]
    public void MatchEndIsEmittedWhenCoreEndsTheMatch()
    {
        var config = Config();
        config.StartHp = 1;
        var session = new ServerRoundSession(config, new[] { "a", "b" });
        session.BeginRound(new FakeTimeProvider(Start));
        Assert.True(session.TryCommit("a", new RoundCommit(1, new Vec2(-2, 0), new Vec2(1, 0)), out _));
        Assert.True(session.TryCommit("b", new RoundCommit(1, new Vec2(2, 0), null), out _));
        session.TryMarkReady("a", out _);
        var messages = session.TryMarkReady("b", out _);

        Assert.Equal(2, messages.Count(message => message.Payload.Contains("\"type\":\"match_end\"", StringComparison.Ordinal)));
    }

    private static ServerRoundSession Create() => new(Config(), new[] { "a", "b" });

    private static GameConfig Config() => new()
    {
        Seed = 123,
        MaxPlayers = 2,
        PlaceSeconds = 5,
        NoAimRule = NoAimPolicy.ThrowFacing
    };

    private sealed class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset now;
        public FakeTimeProvider(DateTimeOffset now) => this.now = now;
        public override DateTimeOffset GetUtcNow() => now;
    }
}
