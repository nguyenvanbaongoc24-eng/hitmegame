using System;
using System.Collections.Immutable;
using HitMe.Server.Authority;
using Xunit;

public sealed class ProfileStoreTests
{
    [Fact]
    public void OneFinalResultCreditsOnlyTopThreeOnceAndUpdatesBestRank()
    {
        var store = new InMemoryProfileStore();
        var policy = new FixedGoldByRank(new[] { 100, 60, 30 }); // Test values, not approved economy.
        var first = Result(Guid.NewGuid(),
            new("winner", 1, 2, 3), new("second", 2, 1, 2),
            new("third", 3, 1, 1), new("last", 4, 0, 0));

        Assert.True(store.TryApplyMatch(first, policy));
        Assert.False(store.TryApplyMatch(first, policy));
        Assert.Equal(new ProfileSnapshot("winner", 100, 1, 1), store.GetOrCreate("winner"));
        Assert.Equal(new ProfileSnapshot("second", 60, 1, 2), store.GetOrCreate("second"));
        Assert.Equal(new ProfileSnapshot("third", 30, 1, 3), store.GetOrCreate("third"));
        Assert.Equal(new ProfileSnapshot("last", 0, 1, 4), store.GetOrCreate("last"));

        var second = Result(Guid.NewGuid(), new("winner", 2, 1, 3), new("last", 1, 3, 2));
        Assert.True(store.TryApplyMatch(second, policy));
        Assert.Equal(new ProfileSnapshot("winner", 160, 2, 1), store.GetOrCreate("winner"));
        Assert.Equal(new ProfileSnapshot("last", 100, 2, 1), store.GetOrCreate("last"));
    }

    [Fact]
    public void DefaultNoRewardKeepsWalletAtZeroAndRejectsPartialInvalidResult()
    {
        var store = new InMemoryProfileStore();
        var id = Guid.NewGuid();
        var invalid = Result(id, new("a", 1, 3, 0), new("a", 2, 1, 0));
        Assert.Throws<ArgumentException>(() => store.TryApplyMatch(invalid, new NoReward()));
        Assert.Equal(0, store.GetOrCreate("a").MatchesPlayed);

        Assert.True(store.TryApplyMatch(Result(id, new MatchPlayerResult("a", 1, 3, 0)), new NoReward()));
        Assert.Equal(new ProfileSnapshot("a", 0, 1, 1), store.GetOrCreate("a"));
    }

    private static MatchResult Result(Guid id, params MatchPlayerResult[] players) =>
        new(id, new DateTimeOffset(2026, 11, 2, 0, 0, 0, TimeSpan.Zero),
            ImmutableArray.Create(players), 1, MatchResultFactory.ConfigVersion);
}
