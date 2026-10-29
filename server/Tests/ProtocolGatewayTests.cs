using System;
using System.Collections.Generic;
using System.Text.Json;
using HitMe.Server.Authority;
using HitMe.Server.Protocol;
using Xunit;

public sealed class ProtocolGatewayTests
{
    private static ProtocolGateway Gateway() => new(new RoomDirectory(n => n == 1 ? "HM0001" : "HM0002"));
    private static string Request(string type, string playerId = "p1", string name = "One", string? roomCode = null, bool ready = false, bool start = false)
        => JsonSerializer.Serialize(new { v = 1, type, playerId, name, roomCode, ready, start });
    private static JsonElement Json(string text) => JsonDocument.Parse(text).RootElement.Clone();

    [Fact] public void CreateJoinAndHostStartRoom()
    {
        var gateway = Gateway();
        var created = Json(gateway.Handle("c1", Request("join")));
        Assert.Equal("room_state", created.GetProperty("type").GetString());
        Assert.Equal("HM0001", created.GetProperty("roomCode").GetString());
        Assert.True(created.GetProperty("config").TryGetProperty("placeSeconds", out _));
        var joined = Json(gateway.Handle("c2", Request("join", "p2", "Two", "HM0001")));
        Assert.Equal(2, joined.GetProperty("players").GetArrayLength());
        var started = Json(gateway.Handle("c1", Request("ready", start: true, ready: true)));
        Assert.Equal("round_start", started.GetProperty("type").GetString());
        Assert.Equal(1, started.GetProperty("roundId").GetInt32());
    }

    [Fact] public void CodeCollisionGeneratesAnotherCode()
    {
        var gateway = Gateway();
        gateway.Handle("c1", Request("join"));
        var second = Json(gateway.Handle("c2", Request("join", "p2", "Two")));
        Assert.Equal("HM0002", second.GetProperty("roomCode").GetString());
    }

    [Fact] public void RoomRejectsSeventhPlayer()
    {
        var gateway = Gateway();
        gateway.Handle("c1", Request("join"));
        for (var i = 2; i <= 6; i++) gateway.Handle($"c{i}", Request("join", $"p{i}", $"P{i}", "HM0001"));
        var reply = Json(gateway.Handle("c7", Request("join", "p7", "P7", "HM0001")));
        Assert.Equal("room_full", reply.GetProperty("code").GetString());
    }

    [Theory]
    [InlineData("{not json")]
    [InlineData("{\"v\":1,\"type\":\"wat\"}")]
    public void BadMessagesReturnErrors(string raw)
    {
        var reply = Json(Gateway().Handle("c1", raw));
        Assert.Equal("error", reply.GetProperty("type").GetString());
    }

    [Fact] public void OversizedMessageReturnsError()
    {
        var reply = Json(Gateway().Handle("c1", new string('x', ProtocolGateway.MaxMessageBytes + 1)));
        Assert.Equal("message_too_large", reply.GetProperty("code").GetString());
    }

    [Fact] public void TwentyMessagesPerSecondAreAllowedThenLimited()
    {
        var gateway = Gateway();
        for (var i = 0; i < ProtocolGateway.MaxMessagesPerSecond; i++) gateway.Handle("c1", Request("wat"));
        var reply = Json(gateway.Handle("c1", Request("wat")));
        Assert.Equal("rate_limited", reply.GetProperty("code").GetString());
    }

    [Fact]
    public void GatewayDoesNotSendAnOpponentsCommitBeforeLock()
    {
        var gateway = Gateway();
        gateway.Handle("c1", Request("join"));
        gateway.Handle("c2", Request("join", "p2", "Two", "HM0001"));
        var start = gateway.HandleMessages("c1", Request("ready", ready: true, start: true));
        Assert.Equal(2, start.Count);
        Assert.All(start, message => Assert.Contains("round_start", message.Payload));

        var commit = JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 1, positionX = 777.125f, positionY = -555.875f, aimX = 0.3f, aimY = 0.7f });
        var sent = gateway.HandleMessages("c2", commit);
        Assert.DoesNotContain(sent, message => message.ConnectionId == "c1");
        Assert.All(sent, message =>
        {
            Assert.DoesNotContain("777.125", message.Payload);
            Assert.DoesNotContain("-555.875", message.Payload);
            Assert.DoesNotContain("\"position\"", message.Payload);
            Assert.DoesNotContain("\"aim\"", message.Payload);
        });
    }

    [Fact]
    public void GatewayBroadcastsRevealOnlyWhenTheRoundLocks()
    {
        var gateway = Gateway();
        gateway.Handle("c1", Request("join"));
        gateway.Handle("c2", Request("join", "p2", "Two", "HM0001"));
        gateway.HandleMessages("c1", Request("ready", ready: true, start: true));

        var commitOne = JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 1, positionX = -2f, positionY = 0f, aimX = 1f, aimY = 0f });
        var commitTwo = JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 1, positionX = 2f, positionY = 0f, aimX = -1f, aimY = 0f });
        gateway.HandleMessages("c1", commitOne);
        gateway.HandleMessages("c2", commitTwo);

        Assert.Empty(gateway.HandleMessages("c1", Request("ready", ready: true)));
        var afterLock = gateway.HandleMessages("c2", Request("ready", "p2", "Two", ready: true));
        Assert.Equal(4, afterLock.Count);
        Assert.Contains(afterLock, message => message.ConnectionId == "c1" && message.Payload.Contains("\"type\":\"reveal\""));
        Assert.Contains(afterLock, message => message.ConnectionId == "c2" && message.Payload.Contains("\"type\":\"round_result\""));
    }

    [Fact]
    public void GatewayTickLocksAndBroadcastsAfterTheServerDeadline()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero));
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM0001"), clock);
        gateway.Handle("c1", Request("join"));
        gateway.Handle("c2", Request("join", "p2", "Two", "HM0001"));
        gateway.HandleMessages("c1", Request("ready", ready: true, start: true));
        gateway.HandleMessages("c1", JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 1, positionX = -2f, positionY = 0f }));

        clock.Advance(TimeSpan.FromSeconds(5));
        var messages = gateway.Tick();

        Assert.Equal(4, messages.Count);
        Assert.Contains(messages, message => message.ConnectionId == "c1" && message.Payload.Contains("\"type\":\"reveal\""));
        Assert.Contains(messages, message => message.ConnectionId == "c2" && message.Payload.Contains("\"type\":\"round_result\""));
        var lateCommit = Json(gateway.Handle("c2", JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 1, positionX = 2f, positionY = 0f })));
        Assert.Equal("round_locked", lateCommit.GetProperty("code").GetString());
    }

    [Fact]
    public void SixConnectionPreLockTranscriptNeverLeaksTraceCommitToAnyOpponent()
    {
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM3141"));
        var transcript = new Dictionary<string, List<string>>();
        void Send(string connectionId, string raw)
        {
            foreach (var message in gateway.HandleMessages(connectionId, raw))
            {
                if (!transcript.TryGetValue(message.ConnectionId, out var messages))
                    transcript[message.ConnectionId] = messages = new List<string>();
                messages.Add(message.Payload);
            }
        }

        Send("c1", Request("join", "p1", "One"));
        for (var player = 2; player <= 6; player++)
            Send($"c{player}", Request("join", $"p{player}", $"P{player}", "HM3141"));
        Send("c1", Request("ready", "p1", "One", ready: true, start: true));

        // Deliberately unusual values make accidental serialization detectable.
        Send("c2", JsonSerializer.Serialize(new
        {
            v = 1,
            type = "commit",
            roundId = 1,
            positionX = 3.14159f,
            positionY = -2.71828f,
            aimX = 0.12345f,
            aimY = -0.98765f
        }));
        Send("c2", JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 99, positionX = 3.14159f, positionY = -2.71828f }));
        Send("c2", Request("ready", "p2", "Two", ready: false));

        foreach (var connectionId in new[] { "c1", "c2", "c3", "c4", "c5", "c6" })
        {
            Assert.True(transcript.TryGetValue(connectionId, out var messages), $"No transcript for {connectionId}.");
            foreach (var payload in messages!)
            {
                Assert.DoesNotContain("3.14159", payload, StringComparison.Ordinal);
                Assert.DoesNotContain("3.142", payload, StringComparison.Ordinal);
                Assert.DoesNotContain("-2.71828", payload, StringComparison.Ordinal);
                Assert.DoesNotContain("-2.718", payload, StringComparison.Ordinal);
                Assert.DoesNotContain("0.12345", payload, StringComparison.Ordinal);
                Assert.DoesNotContain("-0.98765", payload, StringComparison.Ordinal);
                Assert.DoesNotContain("\"position\"", payload, StringComparison.Ordinal);
                Assert.DoesNotContain("\"aim\"", payload, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void ReconnectWithinThirtySecondsKeepsRoomStateWithoutLeakingOpponentCommit()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero));
        var rooms = new RoomDirectory(n => "HM0001");
        var gateway = new ProtocolGateway(rooms, clock);
        var firstJoin = Json(gateway.Handle("c1", Request("join")));
        var token = firstJoin.GetProperty("reconnectToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        gateway.Handle("c2", Request("join", "p2", "Two", "HM0001"));
        gateway.HandleMessages("c1", Request("ready", ready: true, start: true));
        gateway.HandleMessages("c2", JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 1, positionX = 3.14159f, positionY = -2.71828f, aimX = 0.12345f, aimY = -0.98765f }));

        gateway.Disconnect("c1");
        var rejoined = Json(gateway.Handle("c1r", JsonSerializer.Serialize(new { v = 1, type = "join", playerId = "p1", name = "One", reconnectToken = token })));
        Assert.Equal("room_state", rejoined.GetProperty("type").GetString());
        var payload = rejoined.GetRawText();
        Assert.DoesNotContain("3.14159", payload);
        Assert.DoesNotContain("-2.71828", payload);
        Assert.DoesNotContain("\"position\"", payload);
        Assert.DoesNotContain("\"aim\"", payload);
        Assert.True(rooms.TryGet("HM0001", out var reconnectedRoom));
        Assert.True(reconnectedRoom.Players.Single(player => player.Id == "p1").Ready);
        Assert.Equal(reconnectedRoom.Config.StartHp, reconnectedRoom.RoundSession!.Match.State.FindPlayer("p1")!.Hp);

        gateway.Disconnect("c1r");
        clock.Advance(TimeSpan.FromSeconds(31));
        gateway.Tick();
        Assert.True(rooms.TryGet("HM0001", out var room));
        Assert.DoesNotContain(room.Players, player => player.Id == "p1");
        var expired = Json(gateway.Handle("c1e", JsonSerializer.Serialize(new { v = 1, type = "join", playerId = "p1", name = "One", reconnectToken = token })));
        Assert.Equal("reconnect_invalid", expired.GetProperty("code").GetString());
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private DateTimeOffset now;
        public FakeTimeProvider(DateTimeOffset now) => this.now = now;
        public override DateTimeOffset GetUtcNow() => now;
        public void Advance(TimeSpan elapsed) => now += elapsed;
    }
}
