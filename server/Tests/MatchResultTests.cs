using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using HitMe.Core;
using HitMe.Server.Authority;
using Xunit;

public sealed class MatchResultTests
{
    [Fact]
    public void CompletedRoundPersistsOneImmutableResultWithAllRanks()
    {
        var store = new InMemoryMatchResultStore();
        var session = new ServerRoundSession(new GameConfig { MaxRounds = 1 }, new[] { "a", "b" }, resultStore: store);
        session.BeginRound(new FixedClock());
        Assert.True(session.TryCommit("a", new RoundCommit(1, new Vec2(-3, -3), new Vec2(-1, 0)), out _));
        Assert.True(session.TryCommit("b", new RoundCommit(1, new Vec2(3, 3), new Vec2(1, 0)), out _));
        Assert.Empty(session.TryMarkReady("a", out _));
        var messages = session.TryMarkReady("b", out _);
        Assert.Contains(messages, message => message.Payload.Contains("match_end"));
        var end = messages.First(message => message.Payload.Contains("\"type\":\"match_end\"", StringComparison.Ordinal));
        using (var json = JsonDocument.Parse(end.Payload))
        {
            var rankings = json.RootElement.GetProperty("rankings");
            Assert.Equal(2, rankings.GetArrayLength());
            Assert.All(rankings.EnumerateArray(), player => Assert.Equal(1, player.GetProperty("rank").GetInt32()));
        }
        Assert.True(store.TryGet(session.MatchId, out var saved));
        Assert.Equal(2, saved.Players.Length);
        Assert.All(saved.Players, player => Assert.Equal(1, player.Rank));
        Assert.Equal(1, store.Count);
        Assert.False(store.TrySave(saved with { ProtocolVersion = 99 }));
        Assert.True(store.TryGet(session.MatchId, out var unchanged));
        Assert.Equal(1, unchanged.ProtocolVersion);
        Assert.Equal(MatchResultFactory.ConfigVersion, saved.ConfigVersion);
    }

    [Fact]
    public void RankRewardsKeepTiesAndDoNotRewardAnEliminatedLastPlace()
    {
        var players = ImmutableArray.Create(
            new MatchPlayerResult("winner-a", 1, 2, 3),
            new MatchPlayerResult("winner-b", 1, 2, 3),
            new MatchPlayerResult("middle", 3, 1, 1),
            new MatchPlayerResult("afk", 4, 0, 0));
        IRewardPolicy disabled = new NoReward();
        IRewardPolicy configured = new FixedGoldByRank(new[] { 100, 50, 20 });
        Assert.All(players, player => Assert.Equal(0, disabled.GoldFor(player)));
        Assert.Equal(100, configured.GoldFor(players[0]));
        Assert.Equal(100, configured.GoldFor(players[1]));
        Assert.Equal(20, configured.GoldFor(players[2]));
        Assert.Equal(0, configured.GoldFor(players[3]));
    }

    [Fact]
    public void FakeStoreCanVerifyOneWriteAtTheBoundary()
    {
        var fake = new FakeStore();
        var session = new ServerRoundSession(new GameConfig { MaxRounds = 1 }, new[] { "a", "b" }, resultStore: fake);
        session.BeginRound(new FixedClock());
        session.TryMarkReady("a", out _);
        session.TryMarkReady("b", out _);
        Assert.Single(fake.Writes);
        Assert.Equal(session.MatchId, fake.Writes[0].MatchId);
    }

    private sealed class FakeStore : IMatchResultStore
    {
        public readonly List<MatchResult> Writes = new();
        public bool TrySave(MatchResult result) { Writes.Add(result); return true; }
        public bool TryGet(Guid matchId, out MatchResult result) { result = Writes.FirstOrDefault(item => item.MatchId == matchId)!; return result is not null; }
    }

    private sealed class FixedClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 11, 2, 0, 0, 0, TimeSpan.Zero);
    }
}
