using System;
using System.IO;
using System.Linq;
using HitMe.Core;
using HitMe.Server.Golden;
using Xunit;

public sealed class GoldenRoundVectorTests
{
    const float Epsilon = 0.0001f;

    [Fact]
    public void AllTwoHundredGoldenRoundsMatchCoreResolution()
    {
        var file = GoldenRoundsCodec.Read(Path.Combine(AppContext.BaseDirectory, "golden", "rounds.json"));
        Assert.Equal(1, file.SchemaVersion);
        Assert.Equal("phase1-v1.2", file.GameConfigVersion);
        Assert.Equal(200, file.Rounds.Length);
        Assert.Contains(file.Rounds, round => round.Scenario == "adjacent_players");
        Assert.Contains(file.Rounds, round => round.Scenario == "collinear_first_target_only");
        Assert.Contains(file.Rounds, round => round.Scenario == "eliminated_player_never_throws_or_hits");
        Assert.Contains(file.Rounds, round => round.Scenario == "simultaneous_mutual_hits");

        foreach (var round in file.Rounds)
        {
            var players = round.Players.Select(player => player.ToCore()).ToList();
            var inputs = round.Inputs.ToDictionary(input => input.PlayerId, input => input.ToCore());
            var actual = RoundResolver.Resolve(players, inputs, file.Config.ToCore());
            Assert.Equal(round.Expected.Hits.Length, actual.Hits.Count);
            for (var index = 0; index < actual.Hits.Count; index++)
            {
                Assert.Equal(round.Expected.Hits[index].ShooterId, actual.Hits[index].ShooterId);
                Assert.Equal(round.Expected.Hits[index].TargetId, actual.Hits[index].TargetId);
                Assert.InRange(Math.Abs(round.Expected.Hits[index].Distance - actual.Hits[index].Distance), 0f, Epsilon);
            }
            Assert.Equal(round.Expected.Players.Length, players.Count);
            for (var index = 0; index < players.Count; index++)
            {
                var expected = round.Expected.Players[index];
                var actualPlayer = players[index];
                Assert.Equal(expected.Id, actualPlayer.Id);
                Assert.Equal(expected.Hp, actualPlayer.Hp);
                Assert.Equal(expected.HitsLanded, actualPlayer.HitsLanded);
                Assert.Equal(new Vec2(expected.X, expected.Y), actualPlayer.Position);
                Assert.Equal(new Vec2(expected.FacingX, expected.FacingY), actualPlayer.Facing);
            }
        }
    }
}
