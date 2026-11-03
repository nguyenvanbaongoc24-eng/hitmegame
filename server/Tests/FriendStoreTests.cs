using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HitMe.Server.Authority;
using Xunit;

namespace HitMe.Server.Tests;

public sealed class FriendStoreTests
{
    [Fact]
    public void PendingAndAcceptedFriendsSurviveGatewayReplacementWhenStoreIsShared()
    {
        var store = new InMemoryFriendStore();
        Assert.Equal("ok", store.Request("a", "b"));
        Assert.Equal("friend_request_pending", store.Request("a", "b"));
        Assert.Equal("friend_request_not_found", store.Reply("b", "a", true));
        Assert.Equal("ok", store.Reply("a", "b", true));
        Assert.Equal("already_friends", store.Request("b", "a"));
    }

    [Fact]
    public void RejectedRequestDoesNotCreateFriendship()
    {
        var store = new InMemoryFriendStore();
        Assert.Equal("ok", store.Request("a", "b"));
        Assert.Equal("ok", store.Reply("a", "b", false));
        Assert.Equal("ok", store.Request("b", "a"));
    }

    [Fact]
    public void SupabaseAdapterSendsServerOnlyRpcAndReturnsCode()
    {
        var handler = new StubHandler();
        var store = new SupabaseFriendStore(new Uri("https://example.supabase.co"), "sb_secret_test",
            new HttpClient(handler));
        Assert.Equal("ok", store.Request("g_" + new string('a', 32), "g_" + new string('b', 32)));
        Assert.Equal("friend_request_pending", store.Reply("g_" + new string('a', 32), "g_" + new string('b', 32), true));
        Assert.Equal(2, handler.Calls);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        public int Calls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(Send(request, cancellationToken));
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal("/rest/v1/rpc/guest_friend_action", request.RequestUri!.AbsolutePath);
            Assert.Equal("sb_secret_test", request.Headers.GetValues("apikey").Single());
            var body = request.Content!.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
            Assert.Contains("p_from", body);
            Calls++;
            return new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(Calls == 1 ? "\"ok\"" : "\"friend_request_pending\"", Encoding.UTF8, "application/json") };
        }
    }
}
