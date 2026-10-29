using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace HitMe.Server.Protocol;

/// <summary>Serializes writes per socket so a room broadcast cannot race a reply.</summary>
public sealed class WebSocketConnectionRegistry
{
    private readonly ConcurrentDictionary<string, Connection> connections = new(StringComparer.Ordinal);

    public void Register(string connectionId, WebSocket socket) => connections[connectionId] = new Connection(socket);

    public void Remove(string connectionId) => connections.TryRemove(connectionId, out _);

    public async Task SendAsync(GatewayMessage message, CancellationToken cancellationToken)
    {
        if (!connections.TryGetValue(message.ConnectionId, out var connection)) return;
        if (connection.Socket.State != WebSocketState.Open) return;

        await connection.WriteGate.WaitAsync(cancellationToken);
        try
        {
            if (connection.Socket.State == WebSocketState.Open)
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(message.Payload);
                await connection.Socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
            }
        }
        finally
        {
            connection.WriteGate.Release();
        }
    }

    private sealed class Connection
    {
        public Connection(WebSocket socket) => Socket = socket;
        public WebSocket Socket { get; }
        public SemaphoreSlim WriteGate { get; } = new(1, 1);
    }
}
