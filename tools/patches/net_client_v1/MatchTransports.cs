// AG integration: copy beside NetClientDtos.cs. Use LoopbackTransport against a
// server adapter for editor tests. WebSocketTransport is Desktop-only; WebGL must
// supply a .jslib/browser WebSocket implementation in Phase 4. No reconnect loop
// is implemented here.
using System;
using System.Collections.Generic;
#if NET8_0_OR_GREATER
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace HitMe.NetClientV1;

public interface IMatchTransport : IDisposable
{
    event Action<ServerMessage> Received;
    bool IsConnected { get; }
    void Connect();
    void Send(ClientMessage message);
}

public sealed class LoopbackTransport : IMatchTransport
{
    private readonly IProtocolJson codec;
    private readonly Func<string, IReadOnlyList<string>> gateway;
    public event Action<ServerMessage>? Received;
    public bool IsConnected { get; private set; }

    // The gateway callback is the direct in-process adapter to ProtocolGateway;
    // it returns every payload targeted at this local client.
    public LoopbackTransport(IProtocolJson codec, Func<string, IReadOnlyList<string>> gateway)
    {
        this.codec = codec ?? throw new ArgumentNullException(nameof(codec));
        this.gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
    }

    public void Connect() => IsConnected = true;
    public void Send(ClientMessage message)
    {
        if (!IsConnected) throw new InvalidOperationException("transport_not_connected");
        foreach (var payload in gateway(codec.ToJson(message)))
            Received?.Invoke(ProtocolReader.ReadServerMessage(payload, codec));
    }
    public void Dispose() => IsConnected = false;
}

#if NET8_0_OR_GREATER
// Desktop skeleton backed by System.Net.WebSockets. It is intentionally absent
// from WebGL: AG must add a browser/.jslib WebSocket adapter there.
public sealed class WebSocketTransport : IMatchTransport
{
    private readonly IProtocolJson codec;
    private readonly Uri endpoint;
    private readonly CancellationTokenSource lifetime = new();
    private ClientWebSocket? socket;
    public event Action<ServerMessage>? Received;
    public bool IsConnected { get; private set; }

    public WebSocketTransport(IProtocolJson codec, Uri endpoint)
    {
        this.codec = codec ?? throw new ArgumentNullException(nameof(codec));
        this.endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
    }

    public void Connect() => ConnectAsync().GetAwaiter().GetResult();
    public async Task ConnectAsync()
    {
        if (IsConnected) return;
        socket = new ClientWebSocket();
        await socket.ConnectAsync(endpoint, lifetime.Token).ConfigureAwait(false);
        IsConnected = true;
        _ = ReceiveLoopAsync(socket, lifetime.Token);
    }

    public void Send(ClientMessage message) => SendAsync(message).GetAwaiter().GetResult();
    public async Task SendAsync(ClientMessage message)
    {
        if (socket is null || !IsConnected) throw new InvalidOperationException("transport_not_connected");
        var bytes = Encoding.UTF8.GetBytes(codec.ToJson(message));
        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, lifetime.Token).ConfigureAwait(false);
    }

    private async Task ReceiveLoopAsync(ClientWebSocket activeSocket, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        try
        {
            while (!cancellationToken.IsCancellationRequested && activeSocket.State == WebSocketState.Open)
            {
                var result = await activeSocket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
                if (result.MessageType == WebSocketMessageType.Close || !result.EndOfMessage) break;
                Received?.Invoke(ProtocolReader.ReadServerMessage(Encoding.UTF8.GetString(buffer, 0, result.Count), codec));
            }
        }
        catch (OperationCanceledException) { }
        finally { IsConnected = false; }
    }

    public void Dispose()
    {
        lifetime.Cancel();
        socket?.Dispose();
        IsConnected = false;
        Received = null;
    }
}
#else
public sealed class WebSocketTransport : IMatchTransport
{
    public event Action<ServerMessage>? Received;
    public bool IsConnected => false;
    public void Connect() => throw new PlatformNotSupportedException("WebGL needs a .jslib transport.");
    public void Send(ClientMessage message) => throw new PlatformNotSupportedException("WebGL needs a .jslib transport.");
    public void Dispose() { Received = null; }
}
#endif
