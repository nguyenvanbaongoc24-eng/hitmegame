using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HitMe.Server.Protocol;
using Xunit;

public sealed class BetaReportTests
{
    [Fact]
    public void RedactsSecretsFromAllUntrustedTextAndLimitsLogs()
    {
        var report = new BetaReport("0.1.0", "build", "Safari", "1179x2556", "HM4826",
            "token=abc123 sb_secret_testvalue", Enumerable.Repeat("Authorization: Bearer eyJabc.eyJdef.signature", 60).ToArray());
        var clean = BetaReportFilter.Clean(report);
        Assert.Equal(50, clean.RecentLogs.Length);
        Assert.DoesNotContain("abc123", clean.Note);
        Assert.DoesNotContain("sb_secret_", clean.Note);
        Assert.All(clean.RecentLogs, line => Assert.DoesNotContain("eyJabc", line));
    }

    [Fact]
    public void RejectsOversizedClientFields()
    {
        Assert.False(BetaReportFilter.IsValid(new BetaReport("0.1.0", "build", "Safari", "screen", null,
            new string('x', 501), Array.Empty<string>())));
        Assert.False(BetaReportFilter.IsValid(new BetaReport("0.1.0", "build", "Safari", "screen", null,
            "note", Enumerable.Repeat("line", 51).ToArray())));
    }

    [Fact]
    public async Task SupabaseAdapterUsesServerKeyOnlyAndFilteredReport()
    {
        var handler = new RecordingHandler();
        var store = new SupabaseBetaReportStore(new Uri("https://example.supabase.co"), "sb_secret_test", new HttpClient(handler));
        var report = BetaReportFilter.Clean(new BetaReport("0.1.0", "b1", "Safari", "1179x2556", "HM4826", "password=hidden", Array.Empty<string>()));
        Assert.True(await store.SaveAsync(report, CancellationToken.None));
        Assert.Equal("sb_secret_test", handler.ApiKey);
        Assert.Contains("[redacted]", handler.Body);
        Assert.DoesNotContain("hidden", handler.Body);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public string ApiKey { get; private set; } = string.Empty;
        public string Body { get; private set; } = string.Empty;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ApiKey = request.Headers.GetValues("apikey").Single();
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.Created);
        }
    }
}
