using System.Text.Json;
using HitMe.Server.Protocol;
using Xunit;

public sealed class SafeProtocolLogTests
{
    [Fact]
    public void LogRecordContainsOnlyMetadataNotTraceCommitCoordinatesOrToken()
    {
        const string raw = "{\"v\":1,\"type\":\"commit\",\"positionX\":3.14159,\"aimY\":-2.71828,\"reconnectToken\":\"secret-token\"}";
        var record = JsonSerializer.Serialize(SafeProtocolLog.Record("0123456789abcdef", SafeProtocolLog.TypeOf(raw)));
        Assert.Contains("commit", record);
        Assert.DoesNotContain("3.14159", record);
        Assert.DoesNotContain("-2.71828", record);
        Assert.DoesNotContain("secret-token", record);
        Assert.DoesNotContain("89abcdef", record);
    }
}
