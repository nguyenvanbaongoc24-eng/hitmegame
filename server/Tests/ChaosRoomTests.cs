using System;
using System.Linq;
using System.Text.Json;
using HitMe.Core;
using HitMe.Server.Authority;
using HitMe.Server.Protocol;
using Xunit;
using Xunit.Abstractions;

public sealed class ChaosRoomTests
{
    private readonly ITestOutputHelper output;
    public ChaosRoomTests(ITestOutputHelper output) => this.output = output;

    [Theory]
    [InlineData(17)] [InlineData(31)] [InlineData(71)] [InlineData(103)] [InlineData(211)]
    public void TwoHundredMixedRoomsPerSeedCompleteWithinCoreRoundLimit(int seed)
    {
        var random = new Random(seed);
        var completed = 0;
        var solo = 0;
        var reconnects = 0;
        for (var game = 0; game < 200; game++)
        {
            var count = 1 + random.Next(6);
            var clock = new FakeClock();
            var store = new InMemoryMatchResultStore();
            var directory = new RoomDirectory(resultStore: store);
            var gateway = new ProtocolGateway(directory, clock);
            var tokens = Enumerable.Range(0, count).Select(i => $"guest:chaos_{seed}_{game}_{i}").ToArray();
            var connections = Enumerable.Range(0, count).Select(i => $"c{i}").ToArray();
            var first = Parse(gateway.Handle("c0", Request("join", tokens[0])));
            var code = first.GetProperty("roomCode").GetString()!;
            var reconnectTokens = new string[count];
            reconnectTokens[0] = first.GetProperty("reconnectToken").GetString()!;
            for (var i = 1; i < count; i++)
            {
                var joined = Parse(gateway.Handle(connections[i], Request("join", tokens[i], code)));
                reconnectTokens[i] = joined.GetProperty("reconnectToken").GetString()!;
            }
            Assert.Single(directory.Snapshot());
            if (count == 1)
            {
                Assert.Equal("not_enough_players", Parse(gateway.Handle("c0", Request("start"))).GetProperty("code").GetString());
                gateway.Handle("c0", Request("leave"));
                Assert.Empty(directory.Snapshot());
                solo++;
                continue;
            }
            if (count >= 3 && random.Next(4) == 0)
            {
                gateway.Handle(connections[^1], Request("leave"));
                var rejoined = Parse(gateway.Handle(connections[^1], Request("join", tokens[^1], code)));
                reconnectTokens[^1] = rejoined.GetProperty("reconnectToken").GetString()!;
            }
            var room = directory.Snapshot()[0];
            Assert.Equal(count, room.Players.Count);
            Assert.Equal("round_start", Parse(gateway.Handle("c0", Request("start"))).GetProperty("type").GetString());
            if (random.Next(2) == 0)
            {
                var index = random.Next(1, count);
                gateway.Disconnect(connections[index]);
                connections[index] = $"rejoined-{index}";
                var resumed = Parse(gateway.Handle(connections[index], JsonSerializer.Serialize(new
                { v = 1, type = "join", token = tokens[index], reconnectToken = reconnectTokens[index] })));
                Assert.Equal(code, resumed.GetProperty("roomCode").GetString());
                reconnects++;
            }
            for (var step = 0; step < 5 && room.RoundSession!.Match.State.Phase != Phase.MatchEnd; step++)
            {
                var round = room.RoundSession.ActiveRound.RoundId;
                var invalid = Parse(gateway.Handle(connections[0], "{\"v\":1,\"type\":\"commit\",\"junk\":7}"));
                Assert.Equal("bad_message", invalid.GetProperty("code").GetString());
                for (var i = 0; i < count; i++)
                {
                    var playerId = room.Players[i].Id;
                    if (room.RoundSession.Match.State.FindPlayer(playerId)?.IsAlive != true) continue;
                    if (random.Next(5) != 0)
                    {
                        var positionX = random.Next(-3, 4);
                        var positionY = random.Next(-4, 5);
                        gateway.Handle(connections[i], JsonSerializer.Serialize(new
                        { v = 1, type = "commit", roundId = round, positionX, positionY, hasAim = true, aimX = 1, aimY = 0 }));
                    }
                    gateway.Handle(connections[i], JsonSerializer.Serialize(new { v = 1, type = "lock_ready", ready = true }));
                }
                if (room.RoundSession.Match.State.Phase != Phase.MatchEnd && room.RoundSession.ActiveRound.RoundId == round)
                { clock.Advance(TimeSpan.FromSeconds(5)); gateway.Tick(); }
            }
            Assert.Equal(Phase.MatchEnd, room.RoundSession!.Match.State.Phase);
            Assert.InRange(room.RoundSession.Match.State.RoundNumber, 1, 5);
            Assert.True(store.TryGet(room.RoundSession.MatchId, out var result));
            Assert.Equal(count, result.Players.Length);
            Assert.All(result.Players, player => Assert.InRange(player.Rank, 1, count));
            Assert.Equal(1, store.Count);
            completed++;
        }
        output.WriteLine($"seed={seed} rooms=200 completed={completed} soloRejected={solo} reconnects={reconnects} exceptions=0 hung=0");
        Assert.Equal(200, completed + solo);
    }

    private static string Request(string type, string? token = null, string? roomCode = null) =>
        JsonSerializer.Serialize(new { v = 1, type, token, roomCode });
    private static JsonElement Parse(string value) => JsonDocument.Parse(value).RootElement.Clone();
    private sealed class FakeClock : TimeProvider
    {
        private DateTimeOffset now = new(2026, 11, 2, 0, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => now;
        public void Advance(TimeSpan time) => now += time;
    }
}
