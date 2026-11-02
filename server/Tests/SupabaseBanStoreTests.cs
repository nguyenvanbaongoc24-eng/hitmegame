using System;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HitMe.Server.Authority;
using Xunit;

public sealed class SupabaseBanStoreTests
{
    [Fact]
    public void WritesAndReadsBanWithServerOnlyApiKey()
    {
        var calls = 0;
        var handler = new StubHandler(request =>
        {
            calls++;
            Assert.Equal("private-test-key", request.Headers.GetValues("apikey").Single());
            Assert.False(request.Headers.Contains("Authorization"));
            if (calls == 1)
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.Contains("on_conflict=player_id", request.RequestUri!.Query);
                return new HttpResponseMessage(HttpStatusCode.Created);
            }
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Contains("player_id=eq.g_test", request.RequestUri!.Query);
            return new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent("[{\"banned_until\":\"2026-11-02T00:02:00Z\"}]", Encoding.UTF8, "application/json") };
        });
        var store = new SupabaseBanStore(new Uri("https://example.supabase.co"), "private-test-key", new HttpClient(handler));
        var now = new DateTimeOffset(2026, 11, 2, 0, 0, 0, TimeSpan.Zero);
        store.Ban("g_test", now.AddSeconds(120));
        Assert.True(store.TryGetRemaining("g_test", now.AddSeconds(20), out var remaining));
        Assert.Equal(100, remaining);
        Assert.False(store.TryGetRemaining("g_test", now.AddSeconds(121), out _));
        Assert.Equal(3, calls);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> respond;
        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) => this.respond = respond;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) => respond(request);
    }
}
