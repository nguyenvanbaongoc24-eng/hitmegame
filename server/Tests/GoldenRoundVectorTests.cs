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
        Assert.Equal(7, file.Matches.Length);
        Assert.Equal(new[] { 2, 3, 4, 5, 6 }, file.Matches.Select(match => match.Players.Length).Distinct().OrderBy(count => count));
        Assert.Contains(file.Matches, match => match.Scenario == "max_rounds_hp_tie_then_hits");
        Assert.Contains(file.Matches, match => match.Scenario == "max_rounds_shared_tie");

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

    [Fact]
    public void GoldenMatchesCoverTwoToSixPlayersEliminationsAndFinalTiebreaks()
    {
        var file = GoldenRoundsCodec.Read(Path.Combine(AppContext.BaseDirectory, "golden", "rounds.json"));
        foreach (var vector in file.Matches)
        {
            var match = new MatchState(file.Config.ToCore(), vector.Players.Select(player => player.ToCore()));
            foreach (var turn in vector.Rounds)
            {
                if (match.Phase == Phase.MatchEnd) break;
                match.StartRound();
                foreach (var input in turn.Inputs)
                {
                    var player = match.FindPlayer(input.PlayerId);
                    if (player != null && player.IsAlive) Assert.True(match.SetInput(input.ToCore()));
                }
                match.LockRound();
                match.BeginReveal();
                match.BeginThrowing();
                match.ResolveRound();
            }

            Assert.Equal(vector.Expected.RoundNumber, match.RoundNumber);
            Assert.Equal(vector.Expected.Phase, match.Phase.ToString());
            Assert.Equal(vector.Expected.WinnerIds, match.WinnerIds);
            Assert.Equal(vector.Expected.Players.Length, match.Players.Count);
            for (var index = 0; index < match.Players.Count; index++)
            {
                Assert.Equal(vector.Expected.Players[index].Id, match.Players[index].Id);
                Assert.Equal(vector.Expected.Players[index].Hp, match.Players[index].Hp);
                Assert.Equal(vector.Expected.Players[index].HitsLanded, match.Players[index].HitsLanded);
            }
        }
    }
}
