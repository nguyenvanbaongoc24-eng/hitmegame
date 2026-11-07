using System;
using System.Linq;
using System.Text.Json;
using HitMe.Core;
using HitMe.Server.Authority;
using HitMe.Server.Protocol;
using Xunit;

public sealed class V13ProtocolTests
{
    [Fact]
    public void VersionThreeRejectsOldClientsWhenGateEnabled()
    {
        var gateway = new ProtocolGateway(new RoomDirectory(), minimumAcceptedVersion: ProtocolGateway.V13Version);
        foreach (var version in new[] { 1, 2 })
        {
            using var reply = JsonDocument.Parse(gateway.Handle("old" + version,
                JsonSerializer.Serialize(new { v = version, type = "join", token = "guest:old" + version })));
            Assert.Equal("client_outdated", reply.RootElement.GetProperty("code").GetString());
        }
        using var current = JsonDocument.Parse(gateway.Handle("current",
            JsonSerializer.Serialize(new { v = 3, type = "join", token = "guest:current" })));
        Assert.Equal("room_state", current.RootElement.GetProperty("type").GetString());
        Assert.Equal(3, current.RootElement.GetProperty("protocolVersion").GetInt32());
        Assert.Equal(25, current.RootElement.GetProperty("config").GetProperty("hardCapRound").GetInt32());
    }

    [Fact]
    public void MutualEliminationPublishesTiebreakOnlyAfterReveal()
    {
        var config = new GameConfig { StartHp = 1, TiebreakMax = 3 };
        var session = new ServerRoundSession(config, new[] { "a", "b" });
        var start = session.BeginRound();
        Assert.All(start, message =>
        {
            using var json = JsonDocument.Parse(message.Payload);
            Assert.Equal("VÒNG 1", json.RootElement.GetProperty("roundLabel").GetString());
            Assert.DoesNotContain("\"position\"", message.Payload);
        });
        Assert.True(session.TryCommit("a", new RoundCommit(1, new Vec2(-2, 0), new Vec2(1, 0)), out _));
        Assert.True(session.TryCommit("b", new RoundCommit(1, new Vec2(2, 0), new Vec2(-1, 0)), out _));
        Assert.Empty(session.TryMarkReady("a", out _));
        var published = session.TryMarkReady("b", out _);
        var result = published.First(message => message.PlayerId == "a" && message.Payload.Contains("\"type\":\"round_result\""));
        using (var json = JsonDocument.Parse(result.Payload))
        {
            var root = json.RootElement;
            Assert.True(root.GetProperty("tiebreak").GetProperty("pending").GetBoolean());
            Assert.Equal(new[] { "a", "b" }, root.GetProperty("eliminatedIds").EnumerateArray().Select(value => value.GetString()).OrderBy(value => value));
        }
        var next = published.First(message => message.PlayerId == "a" && message.Payload.Contains("\"type\":\"round_start\""));
        using var nextJson = JsonDocument.Parse(next.Payload);
        Assert.Equal("HIỆP PHỤ 1", nextJson.RootElement.GetProperty("roundLabel").GetString());
        Assert.True(nextJson.RootElement.GetProperty("tiebreak").GetProperty("active").GetBoolean());
        Assert.Equal(2, nextJson.RootElement.GetProperty("aliveCount").GetInt32());
    }

    [Fact]
    public void MatchEndIncludesV13RankAndThrowStatistics()
    {
        var session = new ServerRoundSession(new GameConfig { StartHp = 1 }, new[] { "a", "b" });
        session.BeginRound();
        Assert.True(session.TryCommit("a", new RoundCommit(1, new Vec2(-2, 0), new Vec2(1, 0)), out _));
        Assert.True(session.TryCommit("b", new RoundCommit(1, new Vec2(2, 0), new Vec2(1, 0)), out _));
        session.TryMarkReady("a", out _);
        var published = session.TryMarkReady("b", out _);
        var end = published.First(message => message.PlayerId == "a" && message.Payload.Contains("\"type\":\"match_end\""));
        using var json = JsonDocument.Parse(end.Payload);
        var root = json.RootElement;
        Assert.Equal("a", root.GetProperty("winnerIds")[0].GetString());
        Assert.Equal(0, root.GetProperty("tiebreakCount").GetInt32());
        var ranked = root.GetProperty("rankings")[0];
        Assert.Equal(1, ranked.GetProperty("rank").GetInt32());
        Assert.Equal(1, ranked.GetProperty("throws").GetInt32());
        Assert.Equal(1, ranked.GetProperty("hits").GetInt32());
        Assert.Equal(1f, ranked.GetProperty("accuracy").GetSingle());
        Assert.Equal(0, ranked.GetProperty("eliminatedRound").GetInt32());
    }
}
