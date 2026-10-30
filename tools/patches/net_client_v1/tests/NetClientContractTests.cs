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
            (typeof(Envelope).IsAssignableFrom(type) || type == typeof(RoomPlayerDto)));
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
            (typeof(Envelope).IsAssignableFrom(type) || type == typeof(RoomPlayerDto)));
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
        Assert.Equal("test", Assert.IsType<ErrorMessage>(received).code);
    }

    private static bool IsForbidden(Type type) =>
        type.IsGenericType || Nullable.GetUnderlyingType(type) != null ||
        type.GetInterfaces().Any(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IDictionary<,>));

    private sealed class SystemTextJsonCodec : IProtocolJson
    {
        internal static readonly JsonSerializerOptions Options = new() { IncludeFields = true, PropertyNamingPolicy = null };
        public string ToJson<T>(T value) => JsonSerializer.Serialize(value, Options);
        public T FromJson<T>(string json) => JsonSerializer.Deserialize<T>(json, Options)!;
    }
}
