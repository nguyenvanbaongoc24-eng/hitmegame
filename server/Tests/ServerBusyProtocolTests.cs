using System.Text.Json;
using HitMe.Server.Authority;
using HitMe.Server.Protocol;
using Xunit;

public sealed class ServerBusyProtocolTests
{
    [Fact]
    public void FullRoomCapacityReportsServerBusyToClient()
    {
        var gateway = new ProtocolGateway(new RoomDirectory(maxRooms: 1));
        gateway.Handle("a", "{\"v\":2,\"type\":\"join\",\"token\":\"guest:alice\"}");
        using var reply = JsonDocument.Parse(gateway.Handle("b",
            "{\"v\":2,\"type\":\"join\",\"token\":\"guest:bob\"}"));
        Assert.Equal("server_busy", reply.RootElement.GetProperty("code").GetString());
    }
}
