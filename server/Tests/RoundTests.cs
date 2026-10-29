using System;
using HitMe.Core;
using HitMe.Server.Authority;
using Xunit;

public sealed class RoundTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CommitOverwritesUntilTheServerLocks()
    {
        var clock = new FakeTimeProvider(Start);
        var round = Create(clock);

        Assert.True(round.TryCommit("a", Commit(1, 1, 1), out _));
        Assert.True(round.TryCommit("a", Commit(1, 4, 5), out _));
        Assert.Equal(new Vec2(4, 5), round.Commits["a"].Position);

        clock.Advance(TimeSpan.FromSeconds(5));
        Assert.True(round.TryLockIfDue());
        Assert.Equal(RoundLockReason.Deadline, round.LockReason);
        Assert.False(round.TryCommit("a", Commit(1, 7, 8), out var error));
        Assert.Equal("round_locked", error);
        Assert.Equal(new Vec2(4, 5), round.Commits["a"].Position);
    }

    [Fact]
    public void AllReadyLocksBeforeDeadline()
    {
        var clock = new FakeTimeProvider(Start);
        var round = Create(clock);

        Assert.True(round.TryMarkReady("a", out _));
        Assert.False(round.IsLocked);
        Assert.True(round.TryMarkReady("b", out _));
        Assert.True(round.IsLocked);
        Assert.Equal(RoundLockReason.AllReady, round.LockReason);
    }

    [Fact]
    public void DeadlineUsesInjectedClock()
    {
        var clock = new FakeTimeProvider(Start);
        var round = Create(clock);

        clock.Advance(TimeSpan.FromSeconds(4.999));
        Assert.False(round.TryLockIfDue());
        clock.Advance(TimeSpan.FromMilliseconds(1));
        Assert.True(round.TryLockIfDue());
        Assert.Equal(RoundLockReason.Deadline, round.LockReason);
    }

    [Fact]
    public void LateAndWrongRoundCommitsAreIgnored()
    {
        var clock = new FakeTimeProvider(Start);
        var round = Create(clock);

        Assert.False(round.TryCommit("a", Commit(2, 1, 1), out var wrongRound));
        Assert.Equal("wrong_round", wrongRound);

        clock.Advance(TimeSpan.FromSeconds(5));
        Assert.False(round.TryCommit("a", Commit(1, 1, 1), out var late));
        Assert.Equal("round_locked", late);
        Assert.Empty(round.Commits);
    }

    private static Round Create(FakeTimeProvider clock)
    {
        return new Round(1, new GameConfig { PlaceSeconds = 5f }, new[] { "a", "b" }, clock);
    }

    private static RoundCommit Commit(int roundId, float x, float y) => new(roundId, new Vec2(x, y), null);

    private sealed class FakeTimeProvider : TimeProvider
    {
        private DateTimeOffset now;
        public FakeTimeProvider(DateTimeOffset now) => this.now = now;
        public override DateTimeOffset GetUtcNow() => now;
        public void Advance(TimeSpan elapsed) => now += elapsed;
    }
}
