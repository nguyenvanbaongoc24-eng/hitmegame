using System;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using HitMe.Server.Protocol;
using Xunit;

public sealed class WebSocketConnectionRegistryTests
{
    [Fact]
    public async Task SlowClientTimesOutWithoutBlockingFiveFastClients()
    {
        var registry = new WebSocketConnectionRegistry(TimeSpan.FromMilliseconds(30));
        var slow = new BlockingWebSocket();
        registry.Register("slow", slow);
        var fast = Enumerable.Range(1, 5).Select(_ => new ImmediateWebSocket()).ToArray();
        for (var i = 0; i < fast.Length; i++) registry.Register($"fast-{i}", fast[i]);

        var watch = Stopwatch.StartNew();
        await Task.WhenAll(
            registry.SendAsync(new GatewayMessage("slow", "payload"), CancellationToken.None),
            Task.WhenAll(fast.Select((_, index) => registry.SendAsync(new GatewayMessage($"fast-{index}", "payload"), CancellationToken.None))));
        watch.Stop();

        Assert.All(fast, socket => Assert.Equal(1, socket.SendCount));
        Assert.NotEqual(WebSocketState.Open, slow.State);
        Assert.True(watch.Elapsed < TimeSpan.FromMilliseconds(250), "A slow socket delayed fast clients too long.");
    }

    private class ImmediateWebSocket : WebSocket
    {
        private WebSocketState state = WebSocketState.Open;
        public int SendCount { get; private set; }
        public override WebSocketState State => state;
        public override WebSocketCloseStatus? CloseStatus => null;
        public override string? CloseStatusDescription => null;
        public override string? SubProtocol => null;
        public override void Abort() => state = WebSocketState.Aborted;
        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken) { state = WebSocketState.Closed; return Task.CompletedTask; }
        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken) { state = WebSocketState.Closed; return Task.CompletedTask; }
        public override void Dispose() => state = WebSocketState.Closed;
        public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken) => throw new NotSupportedException();
        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken) { SendCount++; return Task.CompletedTask; }
    }

    private sealed class BlockingWebSocket : ImmediateWebSocket
    {
        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
            => Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }
}
