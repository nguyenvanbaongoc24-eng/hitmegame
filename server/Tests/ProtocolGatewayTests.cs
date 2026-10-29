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
        Assert.True(started.GetProperty("started").GetBoolean());
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
}
