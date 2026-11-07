using HitMe.Core;
using Xunit;

public sealed class V13CoreTests
{
    [Fact]
    public void FinalTwoMutualHitsEnterTiebreakAndRestoreOnlyFinalists()
    {
        var match = NewMatch(3);
        match.FindPlayer("c").Hp = 0;
        match.FindPlayer("c").EliminatedAtRound = 1;
        MutualHit(match);
        Assert.Equal(Phase.RoundEnd, match.Phase);
        Assert.True(match.TiebreakPending);
        Assert.Empty(match.WinnerIds);
        match.StartRound();
        Assert.True(match.IsTiebreak);
        Assert.Equal("HIỆP PHỤ 1", match.RoundLabel);
        Assert.Equal(1, match.FindPlayer("a").Hp);
        Assert.Equal(1, match.FindPlayer("b").Hp);
        Assert.Equal(0, match.FindPlayer("c").Hp);
        Assert.True(Vec2.Distance(match.FindPlayer("a").Position, match.FindPlayer("b").Position) >= match.Config.MinSpawnDistance);
    }

    [Fact]
    public void ThreeFinalistsMutualDeathAndThreeTiebreaksEndAsCoWinners()
    {
        var match = NewMatch(3);
        for (int i = 0; i < 4; i++)
        {
            CircularHit(match);
            if (i < 3) { Assert.True(match.TiebreakPending); Assert.Empty(match.WinnerIds); }
        }
        Assert.Equal(Phase.MatchEnd, match.Phase);
        Assert.Equal(3, match.TiebreakCount);
        Assert.Equal(new[] { "a", "b", "c" }, match.WinnerIds);
        Assert.All(match.GetRankings(), player => Assert.Equal(1, player.Rank));
    }

    [Fact]
    public void SuddenDeathStartsAtNormalRoundTenAndNormalRoundTwentyFiveIsHardCap()
    {
        var config = new GameConfig { StartHp = 3, NoAimRule = NoAimPolicy.NoThrow };
        var match = NewMatch(2, config);
        for (int i = 1; i <= 25; i++)
        {
            NoHitRound(match);
            if (i == 9) Assert.Equal(3, match.FindPlayer("a").Hp);
            if (i == 10) Assert.Equal(1, match.FindPlayer("a").Hp);
            if (i < 25) Assert.Equal(Phase.RoundEnd, match.Phase);
        }
        Assert.Equal(25, match.NormalRoundNumber);
        Assert.True(match.TiebreakPending);
        match.StartRound();
        Assert.Equal("HIỆP PHỤ 1", match.RoundLabel);
    }

    [Fact]
    public void HardCapSelectsHpThenHitsAndOnlyTiedLeadersEnterTiebreak()
    {
        var config = new GameConfig { HardCapRound = 1, SuddenDeathRound = 10, NoAimRule = NoAimPolicy.NoThrow };
        var match = NewMatch(3, config);
        match.FindPlayer("a").Hp = 2;
        match.FindPlayer("b").Hp = 2;
        match.FindPlayer("c").Hp = 1;
        match.FindPlayer("a").HitsLanded = 2;
        match.FindPlayer("b").HitsLanded = 1;
        NoHitRound(match);
        Assert.Equal(Phase.MatchEnd, match.Phase);
        Assert.Equal(new[] { "a" }, match.WinnerIds);
        Assert.Equal(new[] { "a", "b", "c" }, System.Array.ConvertAll(match.GetRankings(), r => r.PlayerId));

        var tied = NewMatch(3, config);
        tied.FindPlayer("c").Hp = 1;
        NoHitRound(tied);
        Assert.True(tied.TiebreakPending);
        Assert.Equal(0, tied.FindPlayer("c").Hp);
        tied.StartRound();
        Assert.Equal(1, tied.FindPlayer("a").Hp);
        Assert.Equal(1, tied.FindPlayer("b").Hp);
        Assert.Equal(0, tied.FindPlayer("c").Hp);
    }

    [Fact]
    public void SharedEliminationRoundProducesCompetitionRanks()
    {
        var players = new[]
        {
            new PlayerState("a", "a", false, 1, Vec2.Zero, Vec2.Up),
            new PlayerState("b", "b", false, 0, Vec2.Zero, Vec2.Up) { EliminatedAtRound = 5 },
            new PlayerState("c", "c", false, 0, Vec2.Zero, Vec2.Up) { EliminatedAtRound = 4 },
            new PlayerState("d", "d", false, 0, Vec2.Zero, Vec2.Up) { EliminatedAtRound = 3, HitsLanded = 2 },
            new PlayerState("e", "e", false, 0, Vec2.Zero, Vec2.Up) { EliminatedAtRound = 3 },
            new PlayerState("f", "f", false, 0, Vec2.Zero, Vec2.Up) { EliminatedAtRound = 2 }
        };
        var ranks = MatchRanking.Rank(players, new[] { "a" });
        Assert.Equal(new[] { 1, 2, 3, 4, 4, 6 }, System.Array.ConvertAll(ranks, r => r.Rank));
    }

    [Fact]
    public void SoleSurvivorWinsAndThrowStatsAreRecorded()
    {
        var match = NewMatch(2);
        match.FindPlayer("b").Hp = 1;
        OneWayHit(match);
        Assert.Equal(Phase.MatchEnd, match.Phase);
        Assert.Equal(new[] { "a" }, match.WinnerIds);
        var winner = match.GetRankings()[0];
        Assert.Equal(1, winner.Throws);
        Assert.Equal(1, winner.Hits);
        Assert.Equal(1f, winner.Accuracy);
        Assert.Equal(1, match.GetRankings()[1].EliminatedRound);
    }

    [Fact]
    public void FixedSeedSimulationForTwoThroughSixPlayersTerminatesOneThousandTimes()
    {
        for (uint seed = 1; seed <= 1000; seed++)
        {
            var config = new GameConfig { MaxPlayers = 2 + (int)(seed % 5) };
            var result = MatchSimulator.RunBotMatchDetailed(config, seed);
            Assert.InRange(result.Rounds, 1, config.HardCapRound + config.TiebreakMax);
            Assert.NotEmpty(result.WinnerIds);
        }
    }

    private static MatchState NewMatch(int count, GameConfig? config = null)
    {
        var players = new System.Collections.Generic.List<PlayerState>();
        for (int i = 0; i < count; i++)
            players.Add(new PlayerState(((char)('a' + i)).ToString(), "P" + i, false,
                config?.StartHp ?? 1, new Vec2(i * 2 - 2, i == 2 ? 2 : 0), Vec2.Up));
        return new MatchState(config ?? new GameConfig { StartHp = 1 }, players);
    }

    private static void NoHitRound(MatchState match)
    {
        match.StartRound(); match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
    }

    private static void MutualHit(MatchState match)
    {
        match.StartRound();
        var a = match.FindPlayer("a"); var b = match.FindPlayer("b");
        match.SetInput(new RoundInput("a") { HasAim = true, AimDirection = b.Position - a.Position });
        match.SetInput(new RoundInput("b") { HasAim = true, AimDirection = a.Position - b.Position });
        match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
    }

    private static void CircularHit(MatchState match)
    {
        match.StartRound();
        var ids = new[] { "a", "b", "c" };
        for (int i = 0; i < 3; i++)
        {
            var from = match.FindPlayer(ids[i]); var to = match.FindPlayer(ids[(i + 1) % 3]);
            match.SetInput(new RoundInput(from.Id) { HasAim = true, AimDirection = to.Position - from.Position });
        }
        match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
    }

    private static void OneWayHit(MatchState match)
    {
        match.StartRound();
        var a = match.FindPlayer("a"); var b = match.FindPlayer("b");
        match.SetInput(new RoundInput("a") { HasAim = true, AimDirection = b.Position - a.Position });
        match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
    }
}
