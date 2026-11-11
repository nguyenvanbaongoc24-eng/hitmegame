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
    public void TotalConnectionLimitCoversDifferentIpsAndReleasesCapacity()
    {
        var admission = new ConnectionAdmission(maxConnectionsPerIp: 4, maxTotalConnections: 2);
        Assert.True(admission.TryAdmit("10.0.0.1", out _));
        Assert.True(admission.TryAdmit("10.0.0.2", out _));
        Assert.False(admission.TryAdmit("10.0.0.3", out var error));
        Assert.Equal("server_busy", error);
        Assert.Equal(2, admission.TotalConnections);
        admission.Release("10.0.0.1");
        Assert.True(admission.TryAdmit("10.0.0.3", out _));
    }

    [Theory]
    [InlineData(0, 1024, false)]
    [InlineData(1023, 1, false)]
    [InlineData(1024, 1, true)]
    [InlineData(800, 225, true)]
    public void FixedFrameReaderRejectsOnlyPayloadsAboveOneKilobyte(int read, int incoming, bool expected)
    {
        Assert.Equal(expected, WebSocketMessageReader.ExceedsFixedLimit(read, incoming));
    }

    [Fact]
    public void OriginMustBeAllowListedExceptForNonBrowserClients()
    {
        var admission = new ConnectionAdmission(2, new[] { "HTTPS://HITME.EXAMPLE/", "http://localhost:8103" });
        Assert.True(admission.IsAllowedOrigin("https://hitme.example"));
        Assert.True(admission.IsAllowedOrigin("https://HITME.EXAMPLE/"));
        Assert.True(admission.IsAllowedOrigin("http://localhost:8103"));
        Assert.False(admission.IsAllowedOrigin("http://localhost:8104"));
        Assert.False(admission.IsAllowedOrigin(null));
        Assert.False(admission.IsAllowedOrigin("https://attacker.example"));
    }

    [Fact]
    public void PreviewOriginsAndMissingOriginRequireExplicitFlags()
    {
        var strict = new ConnectionAdmission(2, new[] { "https://hitmegame-iota.vercel.app" });
        Assert.False(strict.IsAllowedOrigin("https://hitmegame-preview-123.vercel.app"));
        Assert.False(strict.IsAllowedOrigin(null));

        var enabled = new ConnectionAdmission(2, new[] { "https://hitmegame-iota.vercel.app" },
            allowMissingOrigin: true, allowVercelPreviewOrigins: true);
        Assert.True(enabled.IsAllowedOrigin("https://hitmegame-preview-123.vercel.app"));
        Assert.False(enabled.IsAllowedOrigin("http://hitmegame-preview-123.vercel.app"));
        Assert.False(enabled.IsAllowedOrigin("https://hitmegame.vercel.app"));
        Assert.True(enabled.IsAllowedOrigin(null));
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
