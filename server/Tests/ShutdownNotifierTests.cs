using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using HitMe.Core;
using HitMe.Server.Authority;
using HitMe.Server.Protocol;
using Xunit;

public sealed class ShutdownNotifierTests
{
    [Fact]
    public async Task ShutdownWaitsForCurrentRoundThenClosesSocket()
    {
        var (rooms, room) = StartedRoom();
        var connections = new WebSocketConnectionRegistry();
        var socket = new TestSocket();
        connections.Register("a", socket);
        var options = ServerRuntimeOptions.FromEnvironment(_ => null) with
            { ShutdownRoundWait = TimeSpan.FromMilliseconds(500) };
        var notifier = new ShutdownNotifier(connections, rooms, options);
        var watch = Stopwatch.StartNew();
        var stopping = notifier.StopAsync(CancellationToken.None);
        await Task.Delay(70);
        Assert.Equal(WebSocketState.Open, socket.State);
        room.RoundSession!.TryMarkReady("a", out _);
        room.RoundSession.TryMarkReady("b", out _);
        await stopping;
        Assert.InRange(watch.ElapsedMilliseconds, 60, 450);
        Assert.Equal("server_restarting", socket.Reason);
        Assert.Contains("\"code\":\"server_restarting\"", socket.LastPayload);
        Assert.Equal(0, connections.Count);
    }

    [Fact]
    public async Task ShutdownClosesAfterConfiguredBoundIfRoundStaysOpen()
    {
        var (rooms, _) = StartedRoom();
        var connections = new WebSocketConnectionRegistry();
        var socket = new TestSocket();
        connections.Register("a", socket);
        var options = ServerRuntimeOptions.FromEnvironment(_ => null) with
            { ShutdownRoundWait = TimeSpan.FromMilliseconds(80) };
        var watch = Stopwatch.StartNew();
        await new ShutdownNotifier(connections, rooms, options).StopAsync(CancellationToken.None);
        Assert.InRange(watch.ElapsedMilliseconds, 65, 400);
        Assert.Equal("server_restarting", socket.Reason);
        Assert.Contains("\"code\":\"server_restarting\"", socket.LastPayload);
    }

    private static (RoomDirectory Rooms, RoomAuthority Room) StartedRoom()
    {
        var rooms = new RoomDirectory();
        var room = rooms.Create("a", "A", new GameConfig(), out _);
        Assert.True(rooms.Join(room.Code, "b", "B", out _, out _));
        Assert.True(room.TryHostStart("a", out _));
        room.RoundSession!.BeginRound(TimeProvider.System);
        return (rooms, room);
    }

    private sealed class TestSocket : WebSocket
    {
        private WebSocketState state = WebSocketState.Open;
        public string? Reason { get; private set; }
        public string? LastPayload { get; private set; }
        public override WebSocketState State => state;
        public override WebSocketCloseStatus? CloseStatus => null;
        public override string? CloseStatusDescription => Reason;
        public override string? SubProtocol => null;
        public override void Abort() => state = WebSocketState.Aborted;
        public override Task CloseAsync(WebSocketCloseStatus status, string? description, CancellationToken token)
            => CloseOutputAsync(status, description, token);
        public override Task CloseOutputAsync(WebSocketCloseStatus status, string? description, CancellationToken token)
        { Reason = description; state = WebSocketState.Closed; return Task.CompletedTask; }
        public override void Dispose() => state = WebSocketState.Closed;
        public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken token)
            => throw new NotSupportedException();
        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType type, bool endOfMessage, CancellationToken token)
        { LastPayload = System.Text.Encoding.UTF8.GetString(buffer.Array!, buffer.Offset, buffer.Count); return Task.CompletedTask; }
    }
}
