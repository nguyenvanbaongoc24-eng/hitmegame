using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HitMe.Server.Authority;
using Xunit;

public sealed class SupabaseGuestMatchStoreTests
{
    [Fact]
    public void SendsApprovedGuestResultOnlyThroughServerRpcAndReadsItBack()
    {
        var id = Guid.NewGuid();
        var firstId = "g_" + new string('a', 32);
        var secondId = "g_" + new string('b', 32);
        var ended = new DateTimeOffset(2026, 11, 2, 0, 0, 0, TimeSpan.Zero);
        var result = new MatchResult(id, ended,
            ImmutableArray.Create(new MatchPlayerResult(firstId, 1, 2, 1),
                new MatchPlayerResult(secondId, 2, 0, 0, false)), 1, "test-v1");
        var calls = 0;
        var handler = new StubHandler(request =>
        {
            calls++;
            Assert.Equal("private-test-key", request.Headers.GetValues("apikey").Single());
            Assert.False(request.Headers.Contains("Authorization"));
            if (calls <= 2)
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.EndsWith("/rpc/record_guest_match", request.RequestUri!.AbsolutePath);
                using var payload = JsonDocument.Parse(request.Content!.ReadAsStringAsync().Result);
                var players = payload.RootElement.GetProperty("p_players");
                Assert.Equal(2, players.GetArrayLength());
                Assert.False(players[1].GetProperty("reward_eligible").GetBoolean());
                return Reply(calls == 1 ? "true" : "false");
            }
            if (calls == 3)
            {
                Assert.Contains("matches?", request.RequestUri!.ToString());
                return Reply("[{\"ended_at\":\"2026-11-02T00:00:00Z\",\"protocol_version\":1,\"config_version\":\"test-v1\"}]");
            }
            Assert.Contains("match_players?", request.RequestUri!.ToString());
            return Reply($"[{{\"server_player_id\":\"{firstId}\",\"placement\":1,\"final_hp\":2,\"hits_landed\":1,\"reward_eligible\":true}},{{\"server_player_id\":\"{secondId}\",\"placement\":2,\"final_hp\":0,\"hits_landed\":0,\"reward_eligible\":false}}]");
        });
        var store = new SupabaseGuestMatchStore(new Uri("https://example.supabase.co"),
            "private-test-key", new HttpClient(handler));
        Assert.True(store.TrySave(result));
        Assert.False(store.TrySave(result));
        Assert.True(store.TryGet(id, out var fetched));
        Assert.Equal(result.MatchId, fetched.MatchId);
        Assert.Equal(result.EndedAt, fetched.EndedAt);
        Assert.Equal(result.ProtocolVersion, fetched.ProtocolVersion);
        Assert.Equal(result.ConfigVersion, fetched.ConfigVersion);
        Assert.Equal(result.Players.ToArray(), fetched.Players.ToArray());
        Assert.Equal(4, calls);
    }

    [Fact]
    public void RejectsUnsignedIdentityBeforeNetworkCall()
    {
        var store = new SupabaseGuestMatchStore(new Uri("https://example.supabase.co"), "private-test-key",
            new HttpClient(new StubHandler(_ => throw new Exception("network should not be used"))));
        var result = new MatchResult(Guid.NewGuid(), DateTimeOffset.UtcNow,
            ImmutableArray.Create(new MatchPlayerResult("guest:Alice", 1, 3, 0),
                new MatchPlayerResult("g_" + new string('b', 32), 2, 0, 0)), 1, "test");
        Assert.Throws<ArgumentException>(() => store.TrySave(result));
    }

    private static HttpResponseMessage Reply(string json) => new(HttpStatusCode.OK)
    { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) => respond(request);
    }
}
