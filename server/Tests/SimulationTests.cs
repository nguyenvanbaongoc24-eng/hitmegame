using HitMe.Core;
using HitMe.Server.Authority;
using Xunit;

public sealed class SimulationTests
{
    [Fact]
    public void TwentySixPlayerBotMatchesFinishWithinConfiguredRoundLimit()
    {
        var config = new GameConfig { MaxPlayers = 6, MaxRounds = 5, RoundLimitTiebreak = RoundLimitTiebreakPolicy.HighestHp };
        for (uint seed = 1; seed <= 20; seed++)
        {
            var result = MatchSimulator.RunBotMatchDetailed(config, seed);
            Assert.InRange(result.Rounds, 1, config.MaxRounds);
            Assert.NotEmpty(result.WinnerIds);
        }
    }

    [Fact]
    public void ServerAndGestureDefaultFacingAreIdentical()
    {
        Assert.Equal(PlacementGestureV2.DefaultFacing, ServerMatch.DefaultFacing);
    }
}
