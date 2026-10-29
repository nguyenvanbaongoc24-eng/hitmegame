using System;
using HitMe.Core;
using HitMe.Server.Authority;
using Xunit;

public sealed class GameplayRuleContractTests
{
    [Fact]
    public void DefaultGameConfigMatchesApprovedCombatRules()
    {
        var config = new GameConfig();

        Assert.Equal(6, config.MaxPlayers);
        Assert.Equal(3, config.StartHp);
        Assert.Equal(5f, config.PlaceSeconds);
        Assert.Equal(5, config.MaxRounds);
        Assert.Equal(1, config.DamagePerHit);
        Assert.Equal(1, config.MaxHitsPerThrow);
        Assert.Equal(NoAimPolicy.ThrowFacing, config.NoAimRule);
        Assert.Equal(RoundLimitTiebreakPolicy.HighestHp, config.RoundLimitTiebreak);
    }

    [Fact]
    public void MissingCommitInNextRoundKeepsPriorLockedPositionAndUsesFacingFallback()
    {
        var config = new GameConfig
        {
            MaxPlayers = 2,
            MaxRounds = 5,
            StartHp = 99,
            Seed = 317,
            NoAimRule = NoAimPolicy.ThrowFacing
        };
        var clock = new FrozenTimeProvider();
        var match = new ServerMatch(config, new[] { "a", "b" });

        var first = match.BeginRound(clock);
        Assert.True(first.TryCommit("a", new RoundCommit(1, new Vec2(config.MinX, config.MinY), new Vec2(1, 0)), out _));
        Assert.True(first.TryMarkReady("a", out _));
        Assert.True(first.TryMarkReady("b", out _));
        match.ResolveLockedRound();
        var priorPosition = match.State.FindPlayer("a")!.Position;

        var second = match.BeginRound(clock);
        Assert.True(second.TryMarkReady("a", out _));
        Assert.True(second.TryMarkReady("b", out _));
        match.ResolveLockedRound();

        var player = match.State.FindPlayer("a")!;
        Assert.Equal(priorPosition, player.Position);
        Assert.True(match.State.LockedInputs["a"].HasAim);
        Assert.Equal(player.Facing, match.State.LockedInputs["a"].AimDirection);
    }

    private sealed class FrozenTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 10, 29, 0, 0, 0, TimeSpan.Zero);
    }
}
