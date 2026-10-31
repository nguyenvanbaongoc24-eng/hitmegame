using System.Text.Json;
using HitMe.NetClientV1;
using HitMe.OnlineGlue;
using HitMe.Server.Authority;
using HitMe.Server.Protocol;
using Xunit;

public sealed class OnlineFlowTests
{
    [Theory]
    [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)] [InlineData(6)]
    public void RealGatewayRoundTripForEveryHumanRosterSize(int count)
    {
        var hub = new Hub();
        using var host = hub.Add("p1");
        host.Flow.Join("test:p1");
        var code = host.Session.RoomCode;
        Assert.NotEmpty(code);
        var others = new List<Client>();
        for (var i = 2; i <= count; i++)
        {
            var client = hub.Add($"p{i}");
            others.Add(client);
            client.Flow.Join($"test:p{i}", code);
        }
        Assert.Contains(host.Ui.Calls, call => call == $"lobby:{count}");
        host.Flow.HostStart();
        hub.Pump();
        Assert.Equal(ClientMatchPhase.Placing, host.Session.Phase);
        Assert.Contains(host.Ui.Calls, call => call.StartsWith("placing:1:"));
        // During placing, neither the server messages nor UI calls can contain opponent coordinates.
        Assert.DoesNotContain(host.Ui.Calls, call => call.StartsWith("reveal:"));
        for (var round = 1; round <= 5 && host.Session.Phase != ClientMatchPhase.MatchEnd; round++)
        {
            Assert.Equal(round, host.Session.RoundId);
            foreach (var client in others) client.Flow.Commit(round, 3.14159f, -2.71828f, true, .12345f, -.98765f);
            host.Flow.Commit(round, 0, 0, false, 0, 0);
            hub.AdvanceSeconds(6);
        }
        Assert.Contains(host.Ui.Calls, call => call.StartsWith("reveal:"));
        Assert.Contains(host.Ui.Calls, call => call.StartsWith("result:"));
        Assert.Equal(ClientMatchPhase.MatchEnd, host.Session.Phase);
        Assert.Contains(host.Ui.Calls, call => call.StartsWith("end:"));
        foreach (var client in others) client.Dispose();
    }

    [Fact]
    public void OneHumanGetsBotPracticeHintAndEarlyStartError()
    {
        var hub = new Hub();
        using var host = hub.Add("p1");
        host.Flow.Join("test:p1");
        Assert.Contains("waiting:1:6", host.Ui.Calls);
        Assert.Contains("need:2", host.Ui.Calls);
        host.Flow.HostStart();
        Assert.Contains(host.Ui.Calls, call => call.StartsWith("error:not_enough_players:"));
    }

    [Fact]
    public void LockReadyOutsidePlacementDoesNotShowLocked()
    {
        var hub = new Hub();
        using var host = hub.Add("p1");
        host.Flow.Join("test:p1");
        host.Flow.LockReady();
        Assert.DoesNotContain("locked", host.Ui.Calls);
        Assert.Contains(host.Ui.Calls, call => call.StartsWith("error:wrong_phase:"));
    }

    [Fact]
    public void QuickMatchHintAfterThirtySecondsDoesNotAddBot()
    {
        var hub = new Hub();
        using var host = hub.Add("p1");
        host.Flow.QuickMatch("test:p1");
        Assert.Contains("search", host.Ui.Calls);
        hub.AdvanceSeconds(30);
        Assert.Contains("hint", host.Ui.Calls);
        Assert.Empty(host.Session.RoomPlayers);
    }

    [Fact]
    public void CountdownCancelsWhenPlayerLeaves()
    {
        var hub = new Hub();
        using var host = hub.Add("p1");
        using var guest = hub.Add("p2");
        host.Flow.Join("test:p1");
        guest.Flow.Join("test:p2", host.Session.RoomCode);
        host.Flow.SetRoomReady(true);
        guest.Flow.SetRoomReady(true);
        Assert.Contains("countdown:3", host.Ui.Calls);
        guest.Flow.SetRoomReady(false);
        Assert.Equal("countdown:0", host.Ui.Calls.Last());
        hub.AdvanceSeconds(4);
        Assert.Equal(ClientMatchPhase.Lobby, host.Session.Phase);
    }

    [Fact]
    public void LeavingPlayerBroadcastsUpdatedRosterToRemainingPlayer()
    {
        var hub = new Hub();
        using var host = hub.Add("p1");
        using var guest = hub.Add("p2");
        host.Flow.Join("test:p1");
        guest.Flow.Join("test:p2", host.Session.RoomCode);
        guest.Flow.Leave();
        Assert.Single(host.Session.RoomPlayers);
        Assert.Equal("waiting:1:6", host.Ui.Calls.Last(call => call.StartsWith("waiting:")));
    }

    [Fact]
    public void CloseReasonChoosesShutdownOrReconnect()
    {
        var hub = new Hub();
        using var client = hub.Add("p1");
        client.Flow.Join("test:p1");
        client.Transport.SimulateClose("server_shutdown");
        Assert.Contains("shutdown", client.Ui.Calls);
        client.Transport.Connect();
        client.Transport.SimulateClose("network_lost");
        Assert.Contains("reconnecting", client.Ui.Calls);
    }

    [Fact]
    public void ReconnectUsesPrivateTokenAndPreservesRoom()
    {
        var hub = new Hub();
        using var old = hub.Add("p1");
        old.Flow.Join("test:p1");
        var code = old.Session.RoomCode;
        var reconnectToken = old.Session.ReconnectToken;
        Assert.NotEmpty(reconnectToken);
        hub.Gateway.Disconnect("p1");
        old.Transport.SimulateClose("network_lost");
        old.Flow.Reconnect();
        Assert.Equal(code, old.Session.RoomCode);
        Assert.NotEqual(reconnectToken, old.Session.ReconnectToken);
        Assert.Equal(ClientMatchPhase.Lobby, old.Session.Phase);
    }

    [Fact]
    public void ReconnectDuringPlacementRestoresServerRoundAndDeadline()
    {
        var hub = new Hub();
        using var host = hub.Add("p1");
        using var guest = hub.Add("p2");
        host.Flow.Join("test:p1");
        guest.Flow.Join("test:p2", host.Session.RoomCode);
        host.Flow.HostStart();
        var deadline = guest.Session.DeadlineUnixMilliseconds;
        hub.Gateway.Disconnect("p2");
        guest.Transport.SimulateClose("network_lost");
        guest.Flow.Reconnect();
        Assert.Equal(ClientMatchPhase.Placing, guest.Session.Phase);
        Assert.Equal(1, guest.Session.RoundId);
        Assert.Equal(deadline, guest.Session.DeadlineUnixMilliseconds);
    }

    [Theory]
    [InlineData("invalid", "auth_invalid")]
    [InlineData("", "auth_required_use_join_token")]
    public void InvalidJoinTokenIsShownAsAnError(string token, string code)
    {
        var hub = new Hub();
        using var client = hub.Add("p1");
        client.Flow.Join(token);
        Assert.Contains(client.Ui.Calls, call => call.StartsWith("error:" + code + ":"));
    }

    [Fact]
    public void VersionMismatchIsExplicit()
    {
        var hub = new Hub();
        using var client = hub.Add("p1");
        client.Transport.Send(new JoinMessage { v = 2, type = "join", token = "test:p1" });
        Assert.Contains(client.Ui.Calls, call => call.StartsWith("error:protocol_mismatch:"));
    }

    [Fact]
    public void InvalidReconnectTokenIsShown()
    {
        var hub = new Hub();
        using var client = hub.Add("p1");
        client.Transport.Send(new JoinMessage { type = "join", token = "test:p1", reconnectToken = "invalid" });
        Assert.Contains(client.Ui.Calls, call => call.StartsWith("error:reconnect_invalid:"));
    }

    [Fact]
    public void TwoConnectedRoundsWithoutCommitSurfaceAfkBan()
    {
        var hub = new Hub();
        using var host = hub.Add("p1");
        using var guest = hub.Add("p2");
        host.Flow.Join("test:p1");
        guest.Flow.Join("test:p2", host.Session.RoomCode);
        host.Flow.HostStart();
        for (var round = 1; round <= 2; round++)
        {
            guest.Flow.Commit(round, 1, 1, true, -1, 0);
            hub.AdvanceSeconds(6);
        }
        using var retry = hub.Add("p1-retry");
        retry.Flow.Join("test:p1");
        Assert.Contains(retry.Ui.Calls, call => call.StartsWith("error:banned:"));
    }

    [Fact]
    public void EmoteIsCardOnlyUntilReveal()
    {
        var hub = new Hub();
        using var a = hub.Add("p1");
        using var b = hub.Add("p2");
        a.Flow.Join("test:p1");
        b.Flow.Join("test:p2", a.Session.RoomCode);
        a.Flow.HostStart();
        b.Flow.Emote("happy");
        Assert.Contains("emote:p2:happy:False", a.Ui.Calls);
        a.Transport.Deliver("{\"v\":1,\"type\":\"reveal\",\"roundId\":1,\"players\":[]}");
        a.Transport.Deliver("{\"v\":1,\"type\":\"emote\",\"playerId\":\"p2\",\"emoteId\":\"happy\",\"phase\":\"reveal\"}");
        Assert.Contains("emote:p2:happy:True", a.Ui.Calls);
    }

    private sealed class Hub
    {
        private readonly FakeClock clock = new();
        private readonly Dictionary<string, LoopbackTransport> transports = new();
        public ProtocolGateway Gateway { get; }
        public Hub() => Gateway = new ProtocolGateway(new RoomDirectory(n => $"HM{n:0000}"), clock, new TestAuth());
        public Client Add(string id)
        {
            var codec = new Codec();
            var transport = new LoopbackTransport(codec, raw =>
            {
                Route(Gateway.HandleMessages(id, raw));
                return Array.Empty<string>();
            });
            transports.Add(id, transport);
            var session = new ClientMatchSession(transport);
            var ui = new FakeOnlineUi();
            var flow = new OnlineFlowController(session, ui, () => clock.GetUtcNow());
            session.Connect();
            return new Client(transport, session, flow, ui);
        }
        public void AdvanceSeconds(int seconds) { clock.Advance(TimeSpan.FromSeconds(seconds)); Pump(); }
        public void Pump() => Route(Gateway.Tick());
        private void Route(IReadOnlyList<GatewayMessage> messages)
        {
            foreach (var message in messages)
                if (transports.TryGetValue(message.ConnectionId, out var transport)) transport.Deliver(message.Payload);
        }
    }
    private sealed class Client : IDisposable
    {
        public LoopbackTransport Transport { get; }
        public ClientMatchSession Session { get; }
        public OnlineFlowController Flow { get; }
        public FakeOnlineUi Ui { get; }
        public Client(LoopbackTransport transport, ClientMatchSession session, OnlineFlowController flow, FakeOnlineUi ui)
        { Transport = transport; Session = session; Flow = flow; Ui = ui; }
        public void Dispose() { Flow.Dispose(); Session.Dispose(); }
    }
    private sealed class FakeClock : TimeProvider
    {
        private DateTimeOffset value = new(2026, 10, 31, 0, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => value;
        public void Advance(TimeSpan duration) => value += duration;
    }
    private sealed class TestAuth : IAuthVerifier
    {
        public bool TryVerify(string? token, out PlayerIdentity identity, out string error)
        {
            if (token?.StartsWith("test:", StringComparison.Ordinal) == true)
            { identity = new PlayerIdentity(token[5..], token[5..]); error = ""; return true; }
            identity = null!; error = "auth_invalid"; return false;
        }
    }
    private sealed class Codec : IProtocolJson
    {
        private static readonly JsonSerializerOptions Options = new() { IncludeFields = true };
        public string ToJson(object value) => JsonSerializer.Serialize(value, value.GetType(), Options);
        public T FromJson<T>(string json) => JsonSerializer.Deserialize<T>(json, Options)!;
    }
    private sealed class FakeOnlineUi : IOnlineUi
    {
        public List<string> Calls { get; } = new();
        public void ShowLobby(RoomStateMessage room) => Calls.Add($"lobby:{room.players.Length}");
        public void ShowWaitingForPlayers(int humans, int maxPlayers) => Calls.Add($"waiting:{humans}:{maxPlayers}");
        public void ShowNotEnoughPlayers(int minPlayers) => Calls.Add($"need:{minPlayers}");
        public void ShowQuickMatchSearching() => Calls.Add("search");
        public void ShowQuickMatchHint() => Calls.Add("hint");
        public void ShowCountdown(int seconds) => Calls.Add($"countdown:{seconds}");
        public void ShowPlacing(int round, int secondsLeft) => Calls.Add($"placing:{round}:{secondsLeft}");
        public void ShowLocked() => Calls.Add("locked");
        public void ShowReveal(RevealMessage reveal) => Calls.Add($"reveal:{reveal.players.Length}");
        public void ShowRoundResult(RoundResultMessage result) => Calls.Add($"result:{result.roundId}");
        public void ShowMatchEnd(MatchEndMessage result) => Calls.Add($"end:{result.winners.Length}");
        public void ShowError(string code, string textVi) => Calls.Add($"error:{code}:{textVi}");
        public void ShowReconnecting() => Calls.Add("reconnecting");
        public void ShowServerShutdown() => Calls.Add("shutdown");
        public void ShowEmote(string playerId, string emoteId, bool visibleOnCharacter) => Calls.Add($"emote:{playerId}:{emoteId}:{visibleOnCharacter}");
    }
}
