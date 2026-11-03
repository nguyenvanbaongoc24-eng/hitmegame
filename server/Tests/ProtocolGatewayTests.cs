using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HitMe.Server.Authority;
using HitMe.Server.Protocol;
using Xunit;

public sealed class ProtocolGatewayTests
{
    private sealed class TestAuthVerifier : IAuthVerifier
    {
        public bool TryVerify(string? token, out PlayerIdentity identity, out string error)
        {
            if (token?.StartsWith("test:", StringComparison.Ordinal) == true) { identity = new PlayerIdentity(token[5..], token[5..]); error = string.Empty; return true; }
            identity = null!; error = "auth_invalid"; return false;
        }
    }
    private static ProtocolGateway Gateway() => new(new RoomDirectory(n => n == 1 ? "HM0001" : "HM0002"), auth: new TestAuthVerifier());
    private static string Request(string type, string playerId = "p1", string name = "One", string? roomCode = null, bool ready = false, bool start = false)
        => JsonSerializer.Serialize(new { v = 1, type, playerId, name, token = "test:" + playerId, roomCode, ready, start });
    private static JsonElement Json(string text) => JsonDocument.Parse(text).RootElement.Clone();

    [Fact]
    public void DefaultRoomCodesUseSixUnambiguousCryptographicCharacters()
    {
        var rooms = new RoomDirectory();
        var codes = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < 100; i++)
        {
            var room = rooms.Create($"p{i}", $"P{i}", null, out var error);
            Assert.Equal(string.Empty, error);
            Assert.Matches("^[A-HJ-KM-NP-Z2-9]{6}$", room.Code);
            Assert.True(codes.Add(room.Code));
        }
    }

    [Theory]
    [InlineData("{\"v\":1,\"type\":\"commit\",\"positionX\":\"bad\"}")]
    [InlineData("{\"v\":1,\"type\":\"commit\",\"positionX\":1e100,\"positionY\":0}")]
    [InlineData("{\"v\":1,\"type\":\"commit\",\"positionX\":0,\"positionY\":0,\"rogue\":42}")]
    [InlineData("{\"v\":1,\"type\":\"commit\",\"positionX\":0,\"positionY\":0,\"aimX\":1e100}")]
    public void InvalidWireFieldsReturnBadMessageWithoutChangingRound(string raw)
    {
        var rooms = new RoomDirectory(n => "HM0001");
        var gateway = new ProtocolGateway(rooms, auth: new TestAuthVerifier());
        gateway.Handle("c1", Request("join"));
        gateway.Handle("c2", Request("join", "p2", "Two", "HM0001"));
        gateway.Handle("c1", Request("start"));
        var reply = Json(gateway.Handle("c1", raw));
        Assert.Equal("bad_message", reply.GetProperty("code").GetString());
        Assert.Empty(rooms.Snapshot()[0].RoundSession!.ActiveRound.Commits);
    }

    [Fact]
    public void RepeatedMalformedCommitsEliminateOffenderWithoutChangingOtherPlayer()
    {
        var audits = new List<ProtocolViolationAudit>();
        var rooms = new RoomDirectory(n => "HM0001");
        var gateway = new ProtocolGateway(rooms, auth: new TestAuthVerifier(), auditViolation: audits.Add);
        gateway.Handle("c1", Request("join"));
        gateway.Handle("c2", Request("join", "p2", "Two", "HM0001"));
        gateway.Handle("c1", Request("start"));
        var match = rooms.Snapshot()[0].RoundSession!;
        var malformed = "{\"v\":1,\"type\":\"commit\",\"roundId\":1,\"positionX\":1e100,\"positionY\":0}";
        for (var i = 1; i <= ProtocolGateway.MaxInvalidCommitViolations; i++)
        {
            var response = Json(gateway.Handle("c1", malformed));
            Assert.Equal(i == ProtocolGateway.MaxInvalidCommitViolations ? "protocol_violation_eliminated" : "bad_message",
                response.GetProperty("code").GetString());
        }
        Assert.Equal(0, match.Match.State.FindPlayer("p1")!.Hp);
        Assert.Equal(3, match.Match.State.FindPlayer("p2")!.Hp);
        Assert.Empty(match.ActiveRound.Commits);
        Assert.Equal(ProtocolGateway.MaxInvalidCommitViolations, audits.Count);
        Assert.All(audits, entry =>
        {
            Assert.Equal("HM0001", entry.RoomId);
            Assert.Equal(match.MatchId, entry.MatchId);
            Assert.Equal("bad_message", entry.Code);
        });
        var valid = Json(gateway.Handle("c1", JsonSerializer.Serialize(new
        { v = 1, type = "commit", roundId = 1, positionX = 0, positionY = 0 })));
        Assert.Equal("protocol_violation_eliminated", valid.GetProperty("code").GetString());
    }

    [Fact]
    public void ValidPlacementRevisionsAreAllowedAndNeverCountAsCheating()
    {
        var audits = new List<ProtocolViolationAudit>();
        var rooms = new RoomDirectory(n => "HM0001");
        var gateway = new ProtocolGateway(rooms, auth: new TestAuthVerifier(), auditViolation: audits.Add);
        gateway.Handle("c1", Request("join"));
        gateway.Handle("c2", Request("join", "p2", "Two", "HM0001"));
        gateway.Handle("c1", Request("start"));
        for (var i = 0; i < 3; i++)
        {
            var response = Json(gateway.Handle("c1", JsonSerializer.Serialize(new
            { v = 1, type = "commit", roundId = 1, positionX = i, positionY = 0, hasAim = true, aimX = 1, aimY = 0 })));
            Assert.Equal("room_state", response.GetProperty("type").GetString());
        }
        Assert.Equal(2, rooms.Snapshot()[0].RoundSession!.ActiveRound.Commits["p1"].Position.X);
        Assert.Empty(audits);
    }

    [Fact]
    public void ReconnectCannotResetInvalidCommitCountWithinSameRound()
    {
        var rooms = new RoomDirectory(n => "HM0001");
        var gateway = new ProtocolGateway(rooms, auth: new TestAuthVerifier());
        var first = Json(gateway.Handle("c1", Request("join")));
        var reconnectToken = first.GetProperty("reconnectToken").GetString();
        gateway.Handle("c2", Request("join", "p2", "Two", "HM0001"));
        gateway.Handle("c1", Request("start"));
        var invalid = "{\"v\":1,\"type\":\"commit\",\"roundId\":1}";
        for (var i = 0; i < ProtocolGateway.MaxInvalidCommitViolations - 1; i++)
            Assert.Equal("invalid_commit", Json(gateway.Handle("c1", invalid)).GetProperty("code").GetString());
        gateway.Disconnect("c1");
        var resumed = Json(gateway.Handle("resumed", JsonSerializer.Serialize(new
        { v = 1, type = "join", token = "test:p1", reconnectToken })));
        Assert.Equal("room_state", resumed.GetProperty("type").GetString());
        Assert.Equal("protocol_violation_eliminated",
            Json(gateway.Handle("resumed", invalid)).GetProperty("code").GetString());
    }

    [Fact] public void CreateJoinAndHostStartRoom()
    {
        var gateway = Gateway();
        var created = Json(gateway.Handle("c1", Request("join")));
        Assert.Equal("room_state", created.GetProperty("type").GetString());
        Assert.Equal(ProtocolGateway.Version, created.GetProperty("protocolVersion").GetInt32());
        Assert.Equal("HM0001", created.GetProperty("roomCode").GetString());
        Assert.True(created.GetProperty("config").TryGetProperty("placeSeconds", out _));
        Assert.Equal(2, created.GetProperty("config").GetProperty("minPlayersToStart").GetInt32());
        var joined = Json(gateway.Handle("c2", Request("join", "p2", "Two", "HM0001")));
        Assert.Equal(2, joined.GetProperty("players").GetArrayLength());
        Assert.Equal(ProtocolGateway.Version, joined.GetProperty("protocolVersion").GetInt32());
        var started = Json(gateway.Handle("c1", Request("start")));
        Assert.Equal("round_start", started.GetProperty("type").GetString());
        Assert.Equal(1, started.GetProperty("roundId").GetInt32());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void JoinRejectsOlderOrNewerProtocolWithExplicitMismatch(int clientVersion)
    {
        var gateway = Gateway();
        var request = JsonSerializer.Serialize(new { v = clientVersion, type = "join", token = "test:p1" });
        var reply = Json(gateway.Handle("c1", request));
        Assert.Equal("error", reply.GetProperty("type").GetString());
        Assert.Equal("protocol_mismatch", reply.GetProperty("code").GetString());
        Assert.Equal(ProtocolGateway.Version, reply.GetProperty("v").GetInt32());
    }

    [Fact]
    public void V2JoinUsesV2EnvelopeWhileV1RoomMemberKeepsV1()
    {
        var gateway = Gateway();
        var first = Json(gateway.Handle("c1", Request("join")));
        var roomCode = first.GetProperty("roomCode").GetString();
        var request = JsonSerializer.Serialize(new { v = 2, type = "join", token = "test:p2", roomCode });
        var replies = gateway.HandleMessages("c2", request);
        using var second = JsonDocument.Parse(replies.First(message => message.ConnectionId == "c2"
            && message.Payload.Contains("\"type\":\"room_state\"", StringComparison.Ordinal)).Payload);
        using var firstUpdate = JsonDocument.Parse(replies.Single(message => message.ConnectionId == "c1").Payload);
        Assert.Equal(2, second.RootElement.GetProperty("v").GetInt32());
        Assert.Equal(2, second.RootElement.GetProperty("protocolVersion").GetInt32());
        Assert.Equal(1, firstUpdate.RootElement.GetProperty("v").GetInt32());
    }

    [Fact]
    public void ReadmeWireExamplesAreProducedByTheGateway()
    {
        var gateway = Gateway();
        var waiting = gateway.Handle("c1", Request("quick_match"));
        var onePlayer = Gateway();
        onePlayer.Handle("c1", Request("join"));
        var rejectedStart = onePlayer.Handle("c1", Request("start"));
        var readme = File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../README.md")));
        Assert.Contains($"`{waiting}`", readme);
        Assert.Contains($"`{rejectedStart}`", readme);
    }

    [Fact]
    public void JoinRequiresVerifiedTokenAndIgnoresClientClaimedIdentity()
    {
        var gateway = Gateway();

        var missing = Json(gateway.Handle("missing", JsonSerializer.Serialize(new { v = 1, type = "join", playerId = "forged", name = "Forged" })));
        Assert.Equal("auth_required_use_join_token", missing.GetProperty("code").GetString());

        var invalid = Json(gateway.Handle("invalid", JsonSerializer.Serialize(new { v = 1, type = "join", token = "not-a-token", playerId = "forged", name = "Forged" })));
        Assert.Equal("auth_invalid", invalid.GetProperty("code").GetString());

        var accepted = Json(gateway.Handle("trusted", JsonSerializer.Serialize(new { v = 1, type = "join", token = "test:trusted-player", playerId = "forged", name = "Forged" })));
        var player = accepted.GetProperty("players")[0];
        Assert.Equal("trusted-player", player.GetProperty("id").GetString());
        Assert.Equal("trusted-player", player.GetProperty("name").GetString());
    }

    [Fact]
    public void AllRoomReadyStartsAfterThreeSecondServerCountdown()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero));
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM0001"), clock, new TestAuthVerifier());
        gateway.Handle("c1", Request("join"));
        gateway.Handle("c2", Request("join", "p2", "Two", "HM0001"));
        Assert.All(gateway.HandleMessages("c1", Request("room_ready", ready: true)), message => Assert.Contains("room_state", message.Payload));
        var countdown = gateway.HandleMessages("c2", Request("room_ready", "p2", "Two", ready: true));
        Assert.All(countdown, message => Assert.Contains("room_state", message.Payload));
        clock.Advance(TimeSpan.FromSeconds(2.999));
        Assert.Empty(gateway.Tick());
        clock.Advance(TimeSpan.FromMilliseconds(1));
        var started = gateway.Tick();
        Assert.Equal(2, started.Count);
        Assert.All(started, message => Assert.Contains("round_start", message.Payload));
    }

    [Fact]
    public void UnreadyOrLeaveCancelsLobbyCountdownAndReadinessMessagesArePhaseScoped()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero));
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM0001"), clock, new TestAuthVerifier());
        gateway.Handle("c1", Request("join"));
        gateway.Handle("c2", Request("join", "p2", "Two", "HM0001"));
        gateway.HandleMessages("c1", Request("room_ready", ready: true));
        gateway.HandleMessages("c2", Request("room_ready", "p2", "Two", ready: true));
        gateway.HandleMessages("c2", Request("room_ready", "p2", "Two", ready: false));
        clock.Advance(TimeSpan.FromSeconds(3));
        Assert.Empty(gateway.Tick());
        Assert.Equal("wrong_phase", Json(gateway.Handle("c1", Request("lock_ready", ready: true))).GetProperty("code").GetString());
        gateway.Handle("c1", Request("start"));
        Assert.Equal("wrong_phase", Json(gateway.Handle("c1", Request("room_ready", ready: true))).GetProperty("code").GetString());
    }

    [Fact]
    public void LeavingRoomCancelsLobbyCountdown()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero));
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM0001"), clock, new TestAuthVerifier());
        gateway.Handle("c1", Request("join"));
        gateway.Handle("c2", Request("join", "p2", "Two", "HM0001"));
        gateway.HandleMessages("c1", Request("room_ready", ready: true));
        gateway.HandleMessages("c2", Request("room_ready", "p2", "Two", ready: true));
        gateway.Handle("c2", Request("leave", "p2", "Two"));
        clock.Advance(TimeSpan.FromSeconds(3));
        Assert.Empty(gateway.Tick());
    }

    [Fact]
    public void LegacyReadyReturnsExplicitMigrationError()
    {
        var gateway = Gateway();
        gateway.Handle("c1", Request("join"));
        var reply = Json(gateway.Handle("c1", Request("ready", ready: true)));
        Assert.Equal("legacy_ready_use_room_ready_or_lock_ready", reply.GetProperty("code").GetString());
    }

    [Fact]
    public void SixReadyPlayersStartTheCountdownWithoutWaitingForAnotherJoin()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero));
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM0001"), clock, new TestAuthVerifier());
        gateway.Handle("c1", Request("join"));
        for (var player = 2; player <= 6; player++) gateway.Handle($"c{player}", Request("join", $"p{player}", $"P{player}", "HM0001"));
        for (var player = 1; player <= 6; player++) gateway.HandleMessages($"c{player}", Request("room_ready", $"p{player}", $"P{player}", ready: true));
        clock.Advance(TimeSpan.FromSeconds(3));
        var started = gateway.Tick();
        Assert.Equal(6, started.Count);
        Assert.All(started, message => Assert.Contains("round_start", message.Payload));
    }

    [Fact]
    public void PlayerJoiningAfterHostStartIsRejected()
    {
        var gateway = Gateway();
        gateway.Handle("c1", Request("join"));
        gateway.Handle("c2", Request("join", "p2", "Two", "HM0001"));
        gateway.Handle("c1", Request("start"));
        var reply = Json(gateway.Handle("c3", Request("join", "p3", "Three", "HM0001")));
        Assert.Equal("room_started", reply.GetProperty("code").GetString());
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void EveryOnlineRosterSizeFromTwoToSixCanStart(int players)
    {
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM0001"), auth: new TestAuthVerifier());
        var created = Json(gateway.Handle("c1", Request("join")));
        var roomCode = created.GetProperty("roomCode").GetString();
        for (var player = 2; player <= players; player++)
            gateway.Handle($"c{player}", Request("join", $"p{player}", $"P{player}", roomCode));

        var started = gateway.HandleMessages("c1", Request("start"));
        Assert.Equal(players, started.Count);
        Assert.All(started, message => Assert.Contains("\"type\":\"round_start\"", message.Payload));
    }

    [Fact]
    public void OneOnlinePlayerCannotStartByHostOrByReadiness()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero));
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM0001"), clock, new TestAuthVerifier());
        gateway.Handle("c1", Request("join"));

        var hostStart = Json(gateway.Handle("c1", Request("start")));
        Assert.Equal("not_enough_players", hostStart.GetProperty("code").GetString());
        gateway.HandleMessages("c1", Request("room_ready", ready: true));
        clock.Advance(TimeSpan.FromSeconds(4));
        Assert.Empty(gateway.Tick());
    }

    [Fact]
    public void OnePlayerRoomRemainsAvailableWhileWaitingForAnotherHuman()
    {
        var rooms = new RoomDirectory(n => "HM0001");
        var gateway = new ProtocolGateway(rooms, auth: new TestAuthVerifier());
        gateway.Handle("c1", Request("join"));

        Assert.True(rooms.TryGet("HM0001", out var room));
        Assert.Single(room!.Players);
        Assert.False(room.Started);
    }

    [Fact]
    public void QuickMatchQueuesOnlyHumansThenPairsTwoReadyPlayersWithoutBots()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero));
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM0001"), clock, new TestAuthVerifier());
        var waiting = Json(gateway.Handle("c1", Request("quick_match")));
        Assert.Equal("quick_match_waiting", waiting.GetProperty("type").GetString());

        var paired = gateway.HandleMessages("c2", Request("quick_match", "p2", "Two"));
        Assert.Equal(2, paired.Count);
        Assert.All(paired, message =>
        {
            Assert.Contains("\"type\":\"room_state\"", message.Payload);
            Assert.DoesNotContain("\"bots\"", message.Payload);
            Assert.Equal(2, Json(message.Payload).GetProperty("players").GetArrayLength());
        });
        clock.Advance(TimeSpan.FromSeconds(3));
        var started = gateway.Tick();
        Assert.Equal(2, started.Count);
        Assert.All(started, message => Assert.Contains("\"type\":\"round_start\"", message.Payload));
    }

    [Fact]
    public void QuickMatchHintAppearsAfterThirtySecondsWithoutAddingABot()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero));
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM0001"), clock, new TestAuthVerifier());
        gateway.Handle("c1", Request("quick_match"));
        clock.Advance(TimeSpan.FromSeconds(30));
        var hint = Assert.Single(gateway.Tick());
        Assert.Equal("quick_match_hint", Json(hint.Payload).GetProperty("type").GetString());
        Assert.DoesNotContain("bot", hint.Payload, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(gateway.Tick());
    }

    [Fact]
    public void LeavingQuickMatchQueuePreventsGhostPairing()
    {
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM0001"), auth: new TestAuthVerifier());
        gateway.Handle("c1", Request("quick_match"));

        var left = Json(gateway.Handle("c1", Request("leave")));
        Assert.Equal("quick_match_left", left.GetProperty("type").GetString());

        var waiting = Json(gateway.Handle("c2", Request("quick_match", "p2", "Two")));
        Assert.Equal("quick_match_waiting", waiting.GetProperty("type").GetString());
    }

    [Fact]
    public void QuickMatchHintIsCancelledWhenAWaitingHumanIsPaired()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 30, 12, 0, 0, TimeSpan.Zero));
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM0001"), clock, new TestAuthVerifier());
        gateway.Handle("c1", Request("quick_match"));
        clock.Advance(TimeSpan.FromSeconds(ProtocolGateway.QuickMatchHintSeconds - 1));

        gateway.HandleMessages("c2", Request("quick_match", "p2", "Two"));
        clock.Advance(TimeSpan.FromSeconds(2));

        Assert.DoesNotContain(gateway.Tick(), message => Json(message.Payload).GetProperty("type").GetString() == "quick_match_hint");
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void PreLockCommitRemainsPrivateForEveryOnlineRosterSize(int players)
    {
        var rooms = new RoomDirectory(n => "HM0001");
        var gateway = new ProtocolGateway(rooms, auth: new TestAuthVerifier());
        var transcript = new Dictionary<string, List<string>>();
        void Send(string connectionId, string raw)
        {
            foreach (var message in gateway.HandleMessages(connectionId, raw))
            {
                if (!transcript.TryGetValue(message.ConnectionId, out var payloads))
                    transcript[message.ConnectionId] = payloads = new List<string>();
                payloads.Add(message.Payload);
            }
        }

        Send("c1", Request("join"));
        for (var player = 2; player <= players; player++)
            Send($"c{player}", Request("join", $"p{player}", $"P{player}", "HM0001"));
        Send("c1", Request("start"));

        const float traceX = 3.14159f;
        const float traceY = -2.71828f;
        Send("c2", JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 1, positionX = traceX, positionY = traceY, aimX = 0.12345f, aimY = -0.98765f }));
        Send("c2", JsonSerializer.Serialize(new { v = 1, type = "emote", emoteId = "happy" }));
        Send("c2", JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 99, positionX = traceX, positionY = traceY }));
        if (players >= 3)
        {
            Assert.True(rooms.TryGet("HM0001", out var room));
            room.RoundSession!.EliminateDisconnected("p3");
            Send("c3", JsonSerializer.Serialize(new { v = 1, type = "emote", emoteId = "happy" }));
        }
        Send("spectator", Request("join", "outsider", "Outsider", "HM0001"));
        Send("spectator", JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 1, positionX = traceX, positionY = traceY }));
        var reconnectToken = transcript["c1"].Select(Json)
            .Where(payload => payload.GetProperty("type").GetString() == "room_state")
            .Select(payload => payload.GetProperty("reconnectToken").GetString())
            .First(token => !string.IsNullOrWhiteSpace(token));
        gateway.Disconnect("c1");
        Send("reconnected", JsonSerializer.Serialize(new { v = 1, type = "join", token = "test:p1", reconnectToken }));

        foreach (var entry in transcript)
        {
            foreach (var payload in entry.Value)
            {
                Assert.DoesNotContain("3.14159", payload, StringComparison.Ordinal);
                Assert.DoesNotContain("-2.71828", payload, StringComparison.Ordinal);
                Assert.DoesNotContain("0.12345", payload, StringComparison.Ordinal);
                Assert.DoesNotContain("-0.98765", payload, StringComparison.Ordinal);
                Assert.DoesNotContain("\"position\"", payload, StringComparison.Ordinal);
                Assert.DoesNotContain("\"aim\"", payload, StringComparison.Ordinal);
                Assert.DoesNotContain("\"seed\"", payload, StringComparison.OrdinalIgnoreCase);
            }
        }
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
        var start = gateway.HandleMessages("c1", Request("start"));
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
        gateway.HandleMessages("c1", Request("room_ready", ready: true));
        gateway.HandleMessages("c1", Request("start"));

        var commitOne = JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 1, positionX = -2f, positionY = 0f, aimX = 1f, aimY = 0f });
        var commitTwo = JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 1, positionX = 2f, positionY = 0f, aimX = -1f, aimY = 0f });
        gateway.HandleMessages("c1", commitOne);
        gateway.HandleMessages("c2", commitTwo);

        Assert.Empty(gateway.HandleMessages("c1", Request("lock_ready", ready: true)));
        var afterLock = gateway.HandleMessages("c2", Request("lock_ready", "p2", "Two", ready: true));
        Assert.Equal(6, afterLock.Count);
        Assert.Contains(afterLock, message => message.ConnectionId == "c1" && message.Payload.Contains("\"type\":\"reveal\""));
        Assert.Contains(afterLock, message => message.ConnectionId == "c2" && message.Payload.Contains("\"type\":\"round_result\""));
        Assert.Contains(afterLock, message => message.ConnectionId == "c1" && message.Payload.Contains("\"type\":\"round_start\""));
    }

    [Fact]
    public void HasAimFalseOverridesStaleAimCoordinates()
    {
        var gateway = Gateway();
        gateway.Handle("c1", Request("join"));
        gateway.Handle("c2", Request("join", "p2", "Two", "HM0001"));
        gateway.Handle("c1", Request("start"));
        var commit = JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 1, positionX = 0f, positionY = 0f, hasAim = false, aimX = 1f, aimY = 0f });
        Assert.Equal("room_state", Json(gateway.Handle("c1", commit)).GetProperty("type").GetString());
        gateway.Handle("c2", JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 1, positionX = 2f, positionY = 0f }));
        gateway.HandleMessages("c1", Request("lock_ready", ready: true));
        var messages = gateway.HandleMessages("c2", Request("lock_ready", "p2", "Two", ready: true));
        var reveal = Json(messages.First(message => message.ConnectionId == "c1" && Json(message.Payload).GetProperty("type").GetString() == "reveal").Payload);
        var player = reveal.GetProperty("players").EnumerateArray().Single(item => item.GetProperty("id").GetString() == "p1");
        Assert.Equal(0f, player.GetProperty("aim").GetProperty("x").GetSingle());
        Assert.Equal(1f, player.GetProperty("aim").GetProperty("y").GetSingle());
    }

    [Fact]
    public void GatewayTickLocksAndBroadcastsAfterTheServerDeadline()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero));
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM0001"), clock, new TestAuthVerifier());
        gateway.Handle("c1", Request("join"));
        gateway.Handle("c2", Request("join", "p2", "Two", "HM0001"));
        gateway.HandleMessages("c1", Request("start"));
        gateway.HandleMessages("c1", JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 1, positionX = -2f, positionY = 0f }));

        clock.Advance(TimeSpan.FromSeconds(5));
        var messages = gateway.Tick();

        Assert.Equal(6, messages.Count);
        Assert.Contains(messages, message => message.ConnectionId == "c1" && message.Payload.Contains("\"type\":\"reveal\""));
        Assert.Contains(messages, message => message.ConnectionId == "c2" && message.Payload.Contains("\"type\":\"round_result\""));
        Assert.Contains(messages, message => message.ConnectionId == "c1" && message.Payload.Contains("\"type\":\"round_start\""));
        var lateCommit = Json(gateway.Handle("c2", JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 1, positionX = 2f, positionY = 0f })));
        // Locking publishes and immediately opens the next placement phase;
        // the old round id must still be rejected rather than altering round 2.
        Assert.Equal("wrong_round", lateCommit.GetProperty("code").GetString());
    }

    [Fact]
    public void SixConnectionPreLockTranscriptNeverLeaksTraceCommitToAnyOpponent()
    {
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM3141"), auth: new TestAuthVerifier());
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
        for (var player = 1; player <= 6; player++)
            Send($"c{player}", Request("room_ready", $"p{player}", $"P{player}", ready: true));
        Send("c1", Request("start", "p1", "One"));

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
        Send("c2", Request("lock_ready", "p2", "Two", ready: false));

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
    public void TwoClientPlacementTranscriptIsDataIndependentUntilIdenticalReveal()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 11, 3, 0, 0, 0, TimeSpan.Zero));
        var gateway = new ProtocolGateway(new RoomDirectory(n => "HM3141"), clock, new TestAuthVerifier());
        var transcript = new Dictionary<string, List<string>>();
        IReadOnlyList<GatewayMessage> Send(string client, object packet)
        {
            var sent = gateway.HandleMessages(client, JsonSerializer.Serialize(packet));
            foreach (var message in sent)
            {
                if (!transcript.TryGetValue(message.ConnectionId, out var payloads))
                    transcript[message.ConnectionId] = payloads = new List<string>();
                payloads.Add(message.Payload);
            }
            return sent;
        }

        Send("a", new { v = 1, type = "join", token = "test:p1" });
        var joinedB = Send("b", new { v = 1, type = "join", token = "test:p2", roomCode = "HM3141" });
        var reconnectToken = joinedB.Single(message => message.ConnectionId == "b")
            .Payload;
        reconnectToken = Json(reconnectToken).GetProperty("reconnectToken").GetString()!;
        Send("a", new { v = 1, type = "start" });
        var originalBCount = transcript["b"].Count;

        // Both placements are valid revisions in the same five-second window.
        var first = Send("a", new { v = 1, type = "commit", roundId = 1,
            positionX = 3.14159f, positionY = -2.71828f,
            hasAim = true, aimX = 0.12345f, aimY = -0.98765f });
        var second = Send("a", new { v = 1, type = "commit", roundId = 1,
            positionX = -3.12345f, positionY = 2.71828f,
            hasAim = true, aimX = -0.23456f, aimY = 0.87654f });
        Assert.Equal(originalBCount, transcript["b"].Count); // no packet or length side channel
        Assert.Single(first);
        Assert.Single(second);
        Assert.Equal(first[0].Payload, second[0].Payload); // submitter ack length/content invariant
        Assert.Equal("room_state", Json(first[0].Payload).GetProperty("type").GetString());

        gateway.Disconnect("b");
        clock.Advance(TimeSpan.FromSeconds(1));
        Send("b2", new { v = 1, type = "join", token = "test:p2", reconnectToken });
        Assert.Contains(transcript["b2"], payload => Json(payload).GetProperty("type").GetString() == "round_start");
        foreach (var payload in transcript["b"].Concat(transcript["b2"]))
        {
            Assert.DoesNotContain("3.14159", payload, StringComparison.Ordinal);
            Assert.DoesNotContain("-2.71828", payload, StringComparison.Ordinal);
            Assert.DoesNotContain("-3.12345", payload, StringComparison.Ordinal);
            Assert.DoesNotContain("2.71828", payload, StringComparison.Ordinal);
            Assert.DoesNotContain("0.12345", payload, StringComparison.Ordinal);
            Assert.DoesNotContain("-0.98765", payload, StringComparison.Ordinal);
            Assert.DoesNotContain("-0.23456", payload, StringComparison.Ordinal);
            Assert.DoesNotContain("0.87654", payload, StringComparison.Ordinal);
            Assert.DoesNotContain("\"position\"", payload, StringComparison.Ordinal);
            Assert.DoesNotContain("\"aim\"", payload, StringComparison.Ordinal);
            Assert.DoesNotContain("\"hasAim\"", payload, StringComparison.Ordinal);
            Assert.DoesNotContain("\"committed\"", payload, StringComparison.Ordinal);
            Assert.DoesNotContain("\"locked\"", payload, StringComparison.Ordinal);
        }

        Send("b2", new { v = 1, type = "commit", roundId = 1,
            positionX = 0f, positionY = 0f, hasAim = true, aimX = 0f, aimY = 1f });
        Assert.DoesNotContain(transcript["a"], payload => Json(payload).GetProperty("type").GetString() == "reveal");
        Send("a", new { v = 1, type = "lock_ready", ready = true });
        Assert.DoesNotContain(transcript["b2"], payload => Json(payload).GetProperty("type").GetString() == "reveal");
        var locked = Send("b2", new { v = 1, type = "lock_ready", ready = true });
        var reveals = locked.Where(message => Json(message.Payload).GetProperty("type").GetString() == "reveal").ToArray();
        Assert.Equal(2, reveals.Length);
        Assert.Equal(reveals[0].Payload, reveals[1].Payload);
        Assert.Equal(new[] { "a", "b2" }, reveals.Select(message => message.ConnectionId).OrderBy(id => id));
        var players = Json(reveals[0].Payload).GetProperty("players");
        Assert.Equal(2, players.GetArrayLength());
        Assert.All(players.EnumerateArray(), player =>
        {
            Assert.True(player.TryGetProperty("position", out _));
            Assert.True(player.TryGetProperty("aim", out _));
        });
    }

    [Fact]
    public void ReconnectWithinThirtySecondsKeepsRoomStateWithoutLeakingOpponentCommit()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero));
        var rooms = new RoomDirectory(n => "HM0001");
        var gateway = new ProtocolGateway(rooms, clock, new TestAuthVerifier());
        var firstJoin = Json(gateway.Handle("c1", Request("join")));
        var token = firstJoin.GetProperty("reconnectToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        gateway.Handle("c2", Request("join", "p2", "Two", "HM0001"));
        gateway.HandleMessages("c1", Request("room_ready", ready: true));
        gateway.HandleMessages("c1", Request("start"));
        gateway.HandleMessages("c2", JsonSerializer.Serialize(new { v = 1, type = "commit", roundId = 1, positionX = 3.14159f, positionY = -2.71828f, aimX = 0.12345f, aimY = -0.98765f }));

        gateway.Disconnect("c1");
        clock.Advance(TimeSpan.FromSeconds(20));
        var rejoined = Json(gateway.Handle("c1r", JsonSerializer.Serialize(new { v = 1, type = "join", playerId = "forged", token = "test:p1", reconnectToken = token })));
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
        var expired = Json(gateway.Handle("c1e", JsonSerializer.Serialize(new { v = 1, type = "join", playerId = "forged", token = "test:p1", reconnectToken = token })));
        Assert.Equal("reconnect_invalid", expired.GetProperty("code").GetString());
    }

    [Fact]
    public void ReconnectTokenIsRotatedAndBoundToTheVerifiedIdentity()
    {
        var gateway = Gateway();
        var joined = Json(gateway.Handle("c1", Request("join")));
        var firstToken = joined.GetProperty("reconnectToken").GetString();
        Assert.NotNull(firstToken);
        Assert.True(firstToken!.Length >= 43, "A 32-byte URL-safe Base64 token is at least 43 characters.");

        gateway.Disconnect("c1");
        var impersonation = Json(gateway.Handle("attacker", JsonSerializer.Serialize(new { v = 1, type = "join", token = "test:attacker", reconnectToken = firstToken })));
        Assert.Equal("reconnect_invalid", impersonation.GetProperty("code").GetString());

        var rejoined = Json(gateway.Handle("c1r", JsonSerializer.Serialize(new { v = 1, type = "join", token = "test:p1", reconnectToken = firstToken })));
        var secondToken = rejoined.GetProperty("reconnectToken").GetString();
        Assert.Equal("room_state", rejoined.GetProperty("type").GetString());
        Assert.NotEqual(firstToken, secondToken);

        gateway.Disconnect("c1r");
        var replayed = Json(gateway.Handle("replay", JsonSerializer.Serialize(new { v = 1, type = "join", token = "test:p1", reconnectToken = firstToken })));
        Assert.Equal("reconnect_invalid", replayed.GetProperty("code").GetString());
    }

    [Fact]
    public void ASecondConnectionWithTheSameIdentityReplacesTheOldOneWithoutDuplicatingPlayer()
    {
        var rooms = new RoomDirectory(n => "HM0001");
        var gateway = new ProtocolGateway(rooms, auth: new TestAuthVerifier());
        var first = Json(gateway.Handle("old", Request("join")));
        var oldToken = first.GetProperty("reconnectToken").GetString();
        var messages = gateway.HandleMessages("new", Request("join"));
        var replaced = messages.Single(message => message.ConnectionId == "old");
        Assert.Equal("session_replaced", Json(replaced.Payload).GetProperty("code").GetString());
        Assert.Equal("session_replaced", replaced.CloseReason);
        var current = Json(messages.Single(message => message.ConnectionId == "new").Payload);
        Assert.Equal("room_state", current.GetProperty("type").GetString());
        Assert.NotEqual(oldToken, current.GetProperty("reconnectToken").GetString());
        Assert.Single(rooms.Snapshot());
        Assert.Single(rooms.Snapshot()[0].Players);
        Assert.Equal("not_joined", Json(gateway.Handle("old", Request("room_ready", ready: true))).GetProperty("code").GetString());
        gateway.Disconnect("old");
        Assert.True(gateway.HasJoinedOrQueued("new"));
    }

    [Fact]
    public async Task RacingReconnectTokenAllowsExactlyOneWinner()
    {
        var rooms = new RoomDirectory(n => "HM0001");
        var gateway = new ProtocolGateway(rooms, auth: new TestAuthVerifier());
        var token = Json(gateway.Handle("original", Request("join"))).GetProperty("reconnectToken").GetString();
        gateway.Disconnect("original");
        var request = JsonSerializer.Serialize(new { v = 1, type = "join", token = "test:p1", reconnectToken = token });
        var attempts = await Task.WhenAll(
            Task.Run(() => Json(gateway.Handle("r1", request)).GetProperty("type").GetString()),
            Task.Run(() => Json(gateway.Handle("r2", request)).GetProperty("type").GetString()));
        Assert.Single(attempts, type => type == "room_state");
        Assert.Single(attempts, type => type == "error");
        Assert.Single(rooms.Snapshot()[0].Players);
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private DateTimeOffset now;
        public FakeTimeProvider(DateTimeOffset now) => this.now = now;
        public override DateTimeOffset GetUtcNow() => now;
        public void Advance(TimeSpan elapsed) => now += elapsed;
    }
}
