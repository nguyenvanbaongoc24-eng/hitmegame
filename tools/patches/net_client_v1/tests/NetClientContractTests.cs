using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using HitMe.NetClientV1;
using Xunit;

public sealed class NetClientContractTests
{
    [Fact]
    public void DtosUseOnlyJsonUtilityCompatiblePublicFields()
    {
        var dtoTypes = typeof(Envelope).Assembly.GetTypes().Where(type =>
            type.IsClass && !type.IsAbstract &&
            type.IsDefined(typeof(SerializableAttribute), false));
        foreach (var type in dtoTypes)
        {
            Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.Instance));
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                Assert.False(IsForbidden(field.FieldType), $"{type.Name}.{field.Name} is not JsonUtility-compatible");
            }
        }
    }

    [Fact]
    public void EnvelopeIsReadBeforeTheTypedServerDto()
    {
        var codec = new SystemTextJsonCodec();
        var message = ProtocolReader.ReadServerMessage("{\"v\":1,\"type\":\"emote\",\"playerId\":\"p1\",\"emoteId\":\"happy\",\"phase\":\"placing\"}", codec);
        var emote = Assert.IsType<EmoteBroadcastMessage>(message);
        Assert.Equal("p1", emote.playerId);
    }

    [Fact]
    public void EveryDtoRoundTripsWithTheJsonUtilityEquivalent()
    {
        var options = SystemTextJsonCodec.Options;
        var dtoTypes = typeof(Envelope).Assembly.GetTypes().Where(type =>
            type.IsClass && !type.IsAbstract &&
            type.IsDefined(typeof(SerializableAttribute), false));
        foreach (var type in dtoTypes)
        {
            var value = Activator.CreateInstance(type)!;
            var json = JsonSerializer.Serialize(value, type, options);
            Assert.NotNull(JsonSerializer.Deserialize(json, type, options));
        }
    }

    [Fact]
    public void CommitUsesHasAimInsteadOfNullableAim()
    {
        var commit = new CommitMessage { v = 1, type = "commit", roundId = 7, positionX = 1, positionY = 2, hasAim = false };
        var json = new SystemTextJsonCodec().ToJson(commit);
        Assert.Contains("hasAim", json, StringComparison.Ordinal);
        Assert.DoesNotContain("aimDir", json, StringComparison.Ordinal);
    }

    [Fact]
    public void LoopbackTransportPassesClientMessageToTheDirectGatewayAdapter()
    {
        var codec = new SystemTextJsonCodec();
        var sent = string.Empty;
        using var transport = new LoopbackTransport(codec, payload =>
        {
            sent = payload;
            return new[] { "{\"v\":1,\"type\":\"error\",\"code\":\"test\"}" };
        });
        ServerMessage? received = null;
        transport.Received += message => received = message;
        transport.Connect();
        transport.Send(new JoinMessage { v = 1, type = "join", token = "guest:test" });
        Assert.Contains("join", sent, StringComparison.Ordinal);
        Assert.Contains("guest:test", sent, StringComparison.Ordinal);
        Assert.Equal("test", Assert.IsType<ErrorMessage>(received).code);
    }

    [Fact]
    public void ProtocolReaderHandlesEveryServerMessageType()
    {
        var codec = new SystemTextJsonCodec();
        foreach (var type in new[] { "room_state", "round_start", "reveal", "round_result", "match_end", "emote", "quick_match_waiting", "quick_match_hint", "quick_match_left", "error" })
            Assert.Equal(type, ProtocolReader.ReadServerMessage($"{{\"v\":1,\"type\":\"{type}\"}}", codec).type);
    }

    [Fact]
    public void ClientMatchSessionFollowsServerEventOrderWithoutRegressingOnCommitAck()
    {
        var codec = new SystemTextJsonCodec();
        var outgoing = new Queue<string>(new[]
        {
            "{\"v\":1,\"type\":\"room_state\",\"protocolVersion\":1,\"roomCode\":\"HM4826\",\"started\":false}",
            "{\"v\":1,\"type\":\"round_start\",\"roundId\":1,\"deadline\":1800000000000}",
            "{\"v\":1,\"type\":\"room_state\",\"protocolVersion\":1,\"roomCode\":\"HM4826\",\"started\":true}",
            "{\"v\":1,\"type\":\"reveal\",\"roundId\":1,\"players\":[]}",
            "{\"v\":1,\"type\":\"round_result\",\"roundId\":1,\"hits\":[],\"players\":[]}",
            "{\"v\":1,\"type\":\"match_end\",\"winners\":[\"p1\"]}"
        });
        using var transport = new LoopbackTransport(codec, _ => new[] { outgoing.Dequeue() });
        using var session = new ClientMatchSession(transport);
        var phases = new List<ClientMatchPhase>();
        session.PhaseChanged += phases.Add;
        session.Connect();
        for (var i = 0; i < 6; i++) session.Send(new StartMessage { type = "start" });
        Assert.Equal(new[] { ClientMatchPhase.Lobby, ClientMatchPhase.Placing, ClientMatchPhase.Revealed, ClientMatchPhase.RoundResult, ClientMatchPhase.MatchEnd }, phases);
        Assert.Equal("HM4826", session.RoomCode);
        Assert.Equal(1, session.RoundId);
        Assert.Equal(1800000000000, session.DeadlineUnixMilliseconds);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(3, false)]
    [InlineData(4, false)]
    [InlineData(5, false)]
    [InlineData(6, false)]
    public void ClientMatchSessionReadsRealHumanRosterSize(int count, bool waiting)
    {
        var codec = new SystemTextJsonCodec();
        var players = Enumerable.Range(1, count).Select(number => new { id = $"p{number}", name = $"P{number}", ready = false }).ToArray();
        var payload = JsonSerializer.Serialize(new { v = 1, type = "room_state", protocolVersion = 1, roomCode = "HM4826", players, config = new { maxPlayers = 6, minPlayersToStart = 2 } });
        using var transport = new LoopbackTransport(codec, _ => new[] { payload });
        using var session = new ClientMatchSession(transport);
        session.Connect();
        session.Send(new JoinMessage { type = "join", token = "guest:roster" });
        Assert.Equal(count, session.RoomPlayers.Length);
        Assert.Equal(6, session.MaxPlayers);
        Assert.Equal(2, session.MinPlayersToStart);
        Assert.Equal(waiting, session.WaitingForPlayers);
    }

    private static bool IsForbidden(Type type) =>
        type.IsGenericType || Nullable.GetUnderlyingType(type) != null ||
        type.GetInterfaces().Any(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IDictionary<,>));

    private sealed class SystemTextJsonCodec : IProtocolJson
    {
        internal static readonly JsonSerializerOptions Options = new() { IncludeFields = true, PropertyNamingPolicy = null };
        public string ToJson(object value) => JsonSerializer.Serialize(value, value.GetType(), Options);
        public T FromJson<T>(string json) => JsonSerializer.Deserialize<T>(json, Options)!;
    }
}
