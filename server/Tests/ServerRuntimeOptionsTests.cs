using System.Collections.Generic;
using HitMe.Server.Protocol;
using Xunit;

public sealed class ServerRuntimeOptionsTests
{
    [Fact]
    public void DefaultsBoundResourcesAndReadOnlyOriginList()
    {
        var options = ServerRuntimeOptions.FromEnvironment(_ => null);
        Assert.Equal(50, options.MaxRooms);
        Assert.Equal(120, options.MaxConnections);
        Assert.Equal(12, options.MaxConnectionsPerIp);
        Assert.Equal(10, options.JoinTimeout.TotalSeconds);
        Assert.Equal(90, options.IdleTimeout.TotalSeconds);
        Assert.Equal(6, options.ShutdownRoundWait.TotalSeconds);
    }

    [Fact]
    public void PositiveEnvironmentValuesOverrideResourceCaps()
    {
        var variables = new Dictionary<string, string> { ["MAX_ROOMS"]="30", ["MAX_CONNECTIONS"]="90",
            ["MAX_CONNECTIONS_PER_IP"]="4", ["JOIN_TIMEOUT_SECONDS"]="7", ["SHUTDOWN_ROUND_WAIT_SECONDS"]="9" };
        var options = ServerRuntimeOptions.FromEnvironment(key => variables.GetValueOrDefault(key));
        Assert.Equal(30, options.MaxRooms);
        Assert.Equal(90, options.MaxConnections);
        Assert.Equal(4, options.MaxConnectionsPerIp);
        Assert.Equal(7, options.JoinTimeout.TotalSeconds);
        Assert.Equal(9, options.ShutdownRoundWait.TotalSeconds);
    }
}
