using System.Text.Json;
using HitMe.Server.Protocol;
using Xunit;

public sealed class ServerMetricsTests
{
    [Fact]
    public void SnapshotContainsOnlyAggregateCountersAndCorrectP95()
    {
        var metrics = new ServerMetrics();
        for (var i = 1; i <= 100; i++) metrics.RecordBroadcast(i);
        metrics.RecordRoundResolution(2);
        metrics.RecordRoundResolution(5);
        metrics.RecordError("bad_message");
        metrics.RecordError("bad_message");
        var snapshot = JsonSerializer.Serialize(metrics.Snapshot(6, 2, 1));
        using var json = JsonDocument.Parse(snapshot);
        Assert.Equal(6, json.RootElement.GetProperty("connections").GetInt32());
        Assert.Equal(2, json.RootElement.GetProperty("rooms").GetInt32());
        Assert.Equal(1, json.RootElement.GetProperty("activeMatches").GetInt32());
        Assert.Equal(95, json.RootElement.GetProperty("broadcastP95Ms").GetDouble());
        Assert.Equal(3.5, json.RootElement.GetProperty("roundResolutionMedianMs").GetDouble());
        Assert.Equal(2, json.RootElement.GetProperty("roundResolutionSamples").GetInt32());
        Assert.Equal(2, json.RootElement.GetProperty("errorsByCode").GetProperty("bad_message").GetInt64());
        Assert.DoesNotContain("token", snapshot);
        Assert.DoesNotContain("position", snapshot);
    }
}
