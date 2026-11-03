using System.Text.Json;
using HitMe.Server.Protocol;
using HitMe.Server.Authority;
using Xunit;

public sealed class ClientVersionPolicyTests
{
    [Fact]
    public void HelloContainsConfiguredVersions()
    {
        var policy = new ClientVersionPolicy("0.1.0-beta.3", "0.1.0");
        using var hello = JsonDocument.Parse(policy.Hello());
        Assert.Equal("hello", hello.RootElement.GetProperty("type").GetString());
        Assert.Equal("0.1.0-beta.3", hello.RootElement.GetProperty("serverVersion").GetString());
        Assert.Equal("0.1.0", hello.RootElement.GetProperty("minClientVersion").GetString());
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("0.0.9", false)]
    [InlineData("0.1.0", true)]
    [InlineData("0.1.0-beta.1", false)]
    [InlineData("0.2.0", true)]
    [InlineData("invalid", false)]
    public void MinimumVersionComparison(string? client, bool supported)
    {
        Assert.Equal(supported, new ClientVersionPolicy("0.1.1", "0.1.0").IsSupported(client));
    }

    [Fact]
    public void OldClientGetsExplicitErrorAndCloseInstruction()
    {
        var gateway = new ProtocolGateway(new RoomDirectory(), versions: new ClientVersionPolicy("0.1.1", "0.1.0"));
        var messages = gateway.HandleMessages("old", "{\"v\":2,\"type\":\"join\",\"token\":\"guest:test\",\"clientVersion\":\"0.0.9\"}");
        Assert.Single(messages);
        Assert.Equal("client_outdated", messages[0].CloseReason);
        using var response = JsonDocument.Parse(messages[0].Payload);
        Assert.Equal("client_outdated", response.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public void BetaNumericSuffixUsesNumericOrdering()
    {
        var policy = new ClientVersionPolicy("0.1.0-beta.10", "0.1.0-beta.2");
        Assert.True(policy.IsSupported("0.1.0-beta.10"));
        Assert.False(policy.IsSupported("0.1.0-beta.1"));
    }
}
