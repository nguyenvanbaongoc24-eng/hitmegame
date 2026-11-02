using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HitMe.Server.Authority;
using Xunit;

public sealed class PlayerProfileReaderTests
{
    [Fact]
    public void InMemoryResultStoreCreditsOnceAndExposesAuthoritativeGold()
    {
        var profiles = new InMemoryProfileStore();
        var store = new ProfiledMatchResultStore(new InMemoryMatchResultStore(), profiles);
        var id = Guid.NewGuid();
        var result = new MatchResult(id, DateTimeOffset.UtcNow,
            ImmutableArray.Create(new MatchPlayerResult("a", 1, 2, 1),
                new MatchPlayerResult("b", 2, 0, 0)), 1, "test");
        Assert.True(store.TrySave(result));
        Assert.False(store.TrySave(result));
        var reader = new InMemoryPlayerProfileReader(profiles);
        Assert.Equal(100, reader.Read("a").Gold);
        Assert.Equal(0, reader.Read("b").Gold);
        Assert.Equal(1, reader.Read("a").MatchesPlayed);
    }

    [Fact]
    public void SupabaseReaderUsesServerKeyAndReadsOnlyOneGuest()
    {
        var id = "g_" + new string('a', 32);
        var handler = new StubHandler(request =>
        {
            Assert.Equal("sb_secret_test_only", request.Headers.GetValues("apikey").Single());
            Assert.Contains($"player_id=eq.{id}", request.RequestUri!.Query);
            return new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent("[{\"gold\":150,\"matches_played\":1,\"best_rank\":1}]") };
        });
        var reader = new SupabaseGuestProfileReader(new Uri("https://project.example"),
            "sb_secret_test_only", new HttpClient(handler));
        var result = reader.Read(id);
        Assert.Equal(150, result.Gold);
        Assert.Equal(1, result.BestRank);
        Assert.Throws<ArgumentException>(() => reader.Read("bad"));
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) => respond(request);
    }
}
