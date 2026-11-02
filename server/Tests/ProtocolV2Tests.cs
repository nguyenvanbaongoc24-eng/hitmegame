using System.Text.Json;
using HitMe.Server.Authority;
using HitMe.Server.Protocol;
using Xunit;

public sealed class ProtocolV2Tests
{
    private static string Wire(object payload) => JsonSerializer.Serialize(payload);
    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement.Clone();

    [Fact]
    public void CannedChatIsV2OnlyRateLimitedAndNeverIncludesPrivatePlacement()
    {
        var gateway = new ProtocolGateway(new RoomDirectory());
        var first = Parse(gateway.Handle("a", Wire(new { v = 2, type = "join", token = "guest:alice" })));
        var roomCode = first.GetProperty("roomCode").GetString();
        gateway.Handle("b", Wire(new { v = 2, type = "join", token = "guest:bob", roomCode }));
        gateway.Handle("a", Wire(new { v = 2, type = "start" }));
        gateway.Handle("b", Wire(new { v = 2, type = "commit", roundId = 1,
            positionX = 3.14159, positionY = -2.71828, hasAim = true, aimX = 0.12345, aimY = -0.98765 }));

        var messages = gateway.HandleMessages("a", Wire(new { v = 2, type = "chat", chatId = "hello" }));
        Assert.Equal(2, messages.Count);
        foreach (var message in messages)
        {
            var packet = Parse(message.Payload);
            Assert.Equal(2, packet.GetProperty("v").GetInt32());
            Assert.Equal("chat", packet.GetProperty("type").GetString());
            Assert.Equal("hello", packet.GetProperty("chatId").GetString());
            Assert.DoesNotContain("3.14159", message.Payload);
            Assert.DoesNotContain("-2.71828", message.Payload);
            Assert.DoesNotContain("0.12345", message.Payload);
            Assert.DoesNotContain("-0.98765", message.Payload);
        }
        Assert.Equal("chat_rate_limited", Parse(gateway.Handle("a", Wire(new { v = 2, type = "chat", chatId = "thanks" }))).GetProperty("code").GetString());
        Assert.Equal("invalid_chat_id", Parse(gateway.Handle("b", Wire(new { v = 2, type = "chat", chatId = "custom_text" }))).GetProperty("code").GetString());
    }

    [Fact]
    public void FriendRequestRequiresSameRoomAndAcceptanceByRecipient()
    {
        var gateway = new ProtocolGateway(new RoomDirectory());
        var first = Parse(gateway.Handle("a", Wire(new { v = 2, type = "join", token = "guest:alice" })));
        var roomCode = first.GetProperty("roomCode").GetString();
        var second = Parse(gateway.Handle("b", Wire(new { v = 2, type = "join", token = "guest:bob", roomCode })));
        var third = Parse(gateway.Handle("c", Wire(new { v = 2, type = "join", token = "guest:charlie" })));
        var alice = first.GetProperty("players")[0].GetProperty("id").GetString();
        var bob = second.GetProperty("players")[1].GetProperty("id").GetString();
        var charlie = third.GetProperty("players")[0].GetProperty("id").GetString();
        Assert.Equal("friend_target_unavailable", Parse(gateway.Handle("a", Wire(new { v = 2, type = "friend_request", targetPlayerId = charlie }))).GetProperty("code").GetString());
        var request = gateway.HandleMessages("a", Wire(new { v = 2, type = "friend_request", targetPlayerId = bob }));
        Assert.Single(request);
        Assert.Equal("b", request[0].ConnectionId);
        Assert.Equal(alice, Parse(request[0].Payload).GetProperty("playerId").GetString());
        Assert.Equal("friend_request_not_found", Parse(gateway.Handle("a", Wire(new { v = 2, type = "friend_reply", targetPlayerId = bob, accepted = true }))).GetProperty("code").GetString());
        var accepted = gateway.HandleMessages("b", Wire(new { v = 2, type = "friend_reply", targetPlayerId = alice, accepted = true }));
        Assert.Equal(2, accepted.Count);
        Assert.All(accepted, message => Assert.True(Parse(message.Payload).GetProperty("accepted").GetBoolean()));
        Assert.Equal("already_friends", Parse(gateway.Handle("a", Wire(new { v = 2, type = "friend_request", targetPlayerId = bob }))).GetProperty("code").GetString());
    }
}
