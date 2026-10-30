using HitMe.Server.Protocol;
using Xunit;

public sealed class ConnectionAdmissionTests
{
    [Fact]
    public void PerIpLimitRejectsTheNextConnectionAndReleaseAllowsItAgain()
    {
        var admission = new ConnectionAdmission(2, new[] { "https://hitme.example" });
        Assert.True(admission.TryAdmit("10.0.0.1", out _));
        Assert.True(admission.TryAdmit("10.0.0.1", out _));
        Assert.False(admission.TryAdmit("10.0.0.1", out var error));
        Assert.Equal("connection_limit", error);
        admission.Release("10.0.0.1");
        Assert.True(admission.TryAdmit("10.0.0.1", out _));
    }

    [Fact]
    public void OriginMustBeAllowListedExceptForNonBrowserClients()
    {
        var admission = new ConnectionAdmission(2, new[] { "https://hitme.example" });
        Assert.True(admission.IsAllowedOrigin("https://hitme.example"));
        Assert.True(admission.IsAllowedOrigin(null));
        Assert.False(admission.IsAllowedOrigin("https://attacker.example"));
    }

    [Fact]
    public void DirectoryRejectsCreationAtTheConfiguredRoomLimit()
    {
        var directory = new HitMe.Server.Authority.RoomDirectory(attempt => $"HM{attempt:0000}", maxRooms: 1);
        Assert.NotNull(directory.Create("a", "A", null, out _));
        Assert.Null(directory.Create("b", "B", null, out var error));
        Assert.Equal("room_limit", error);
    }
}
