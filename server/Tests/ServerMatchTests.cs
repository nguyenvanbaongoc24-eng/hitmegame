using System;
using System.Collections.Generic;
using HitMe.Core;
using HitMe.Server.Authority;
using Xunit;

public sealed class ServerMatchTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void MissingCommitKeepsSeededPositionAndUsesThrowFacing()
    {
        var config = Config();
        var server = new ServerMatch(config, new[] { "a", "b" });
        var expectedPosition = server.State.Players[0].Position;
        var round = server.BeginRound(new FakeTimeProvider(Start));

        Assert.True(round.TryMarkReady("a", out _));
        Assert.True(round.TryMarkReady("b", out _));
        server.ResolveLockedRound();

        Assert.Equal(expectedPosition, server.State.Players[0].Position);
        Assert.True(server.State.LockedInputs["a"].HasAim);
        Assert.Equal(Vec2.Up, server.State.LockedInputs["a"].AimDirection);
    }

    [Fact]
    public void LockedServerRoundMatchesDirectCoreResolutionForTheSameSeed()
    {
        var config = Config();
        var server = new ServerMatch(config, new[] { "a", "b" });
        var round = server.BeginRound(new FakeTimeProvider(Start));
        CommitAndReady(round, "a", new Vec2(-2, 0), new Vec2(1, 0));
        CommitAndReady(round, "b", new Vec2(2, 0), new Vec2(-1, 0));
        var serverResolution = server.ResolveLockedRound();

        var direct = new MatchState(config, SeededPlayers(config, new[] { "a", "b" }));
        direct.StartRound();
        direct.SetInput(Input("a", new Vec2(-2, 0), new Vec2(1, 0)));
        direct.SetInput(Input("b", new Vec2(2, 0), new Vec2(-1, 0)));
        direct.LockRound();
        direct.BeginReveal();
        direct.BeginThrowing();
        var directResolution = direct.ResolveRound();

        Assert.Equal(directResolution.Hits.Count, serverResolution.Hits.Count);
        Assert.Equal(directResolution.Damage.Count, serverResolution.Damage.Count);
        for (var i = 0; i < direct.Players.Count; i++)
        {
            Assert.Equal(direct.Players[i].Id, server.State.Players[i].Id);
            Assert.Equal(direct.Players[i].Hp, server.State.Players[i].Hp);
            Assert.Equal(direct.Players[i].Position, server.State.Players[i].Position);
            Assert.Equal(direct.Players[i].Facing, server.State.Players[i].Facing);
            Assert.Equal(direct.Players[i].HitsLanded, server.State.Players[i].HitsLanded);
        }
    }

    [Fact]
    public void NullAimIsAdaptedToCoreThrowFacing()
    {
        var server = new ServerMatch(Config(), new[] { "a", "b" });
        var round = server.BeginRound(new FakeTimeProvider(Start));
        CommitAndReady(round, "a", new Vec2(-2, 0), null);
        CommitAndReady(round, "b", new Vec2(2, 0), null);

        server.ResolveLockedRound();

        Assert.True(server.State.LockedInputs["a"].HasAim);
        Assert.Equal(Vec2.Up, server.State.LockedInputs["a"].AimDirection);
        Assert.True(server.State.LockedInputs["b"].HasAim);
        Assert.Equal(Vec2.Up, server.State.LockedInputs["b"].AimDirection);
    }

    private static GameConfig Config() => new()
    {
        Seed = 77,
        PlaceSeconds = 5,
        MaxPlayers = 2,
        NoAimRule = NoAimPolicy.ThrowFacing
    };

    private static void CommitAndReady(Round round, string playerId, Vec2 position, Vec2? aim)
    {
        Assert.True(round.TryCommit(playerId, new RoundCommit(round.RoundId, position, aim), out _));
        Assert.True(round.TryMarkReady(playerId, out _));
    }

    private static RoundInput Input(string playerId, Vec2 position, Vec2 aim) => new(playerId)
    {
        HasPlacement = true,
        Placement = position,
        HasAim = true,
        AimDirection = aim,
        IsReady = true
    };

    private static List<PlayerState> SeededPlayers(GameConfig config, IEnumerable<string> playerIds)
    {
        var random = new SeededRandom(config.Seed);
        var players = new List<PlayerState>();
        foreach (var playerId in playerIds)
        {
            players.Add(new PlayerState(
                playerId,
                playerId,
                false,
                config.StartHp,
                new Vec2(random.Range(config.MinX, config.MaxX), random.Range(config.MinY, config.MaxY)),
                Vec2.Up));
        }
        return players;
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset now;
        public FakeTimeProvider(DateTimeOffset now) => this.now = now;
        public override DateTimeOffset GetUtcNow() => now;
    }
}
