using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HitMe.Server.Protocol;
using Xunit;

public sealed class BetaMetricsStoreTests
{
    [Fact]
    public async Task SendsOnlyAggregateCountersToServerOnlyTable()
    {
        string? payload = null;
        var client = new HttpClient(new CaptureHandler(async request =>
        {
            Assert.Equal("/rest/v1/beta_events", request.RequestUri!.AbsolutePath);
            Assert.True(request.Headers.Contains("apikey"));
            payload = await request.Content!.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.Created);
        }));
        var store = new SupabaseBetaMetricsStore(new Uri("https://test.supabase.co"),
            "sb_secret_for_test_only", client);
        var counters = new ServerMetrics();
        counters.RecordMatchStart();
        Assert.True(await store.SaveAsync(counters.Snapshot(2, 1, 1), "0.1.0-beta.1", CancellationToken.None));
        using var json = JsonDocument.Parse(payload!);
        Assert.Equal(1, json.RootElement.GetProperty("counters").GetProperty("matchesStarted").GetInt64());
        Assert.Equal("0.1.0-beta.1", json.RootElement.GetProperty("server_version").GetString());
        Assert.DoesNotContain("playerId", payload);
        Assert.DoesNotContain("token", payload);
    }

    private sealed class CaptureHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => respond(request);
    }
}
