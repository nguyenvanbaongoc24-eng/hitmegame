using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace HitMe.Server.Protocol;

/// <summary>Serializes writes per socket so a room broadcast cannot race a reply.</summary>
public sealed class WebSocketConnectionRegistry
{
    public const int MaxPendingWritesPerConnection = 4;
    private readonly ConcurrentDictionary<string, Connection> connections = new(StringComparer.Ordinal);
    private readonly TimeSpan writeTimeout;

    public WebSocketConnectionRegistry(TimeSpan? writeTimeout = null)
    {
        this.writeTimeout = writeTimeout ?? TimeSpan.FromSeconds(1);
    }

    public void Register(string connectionId, WebSocket socket) => connections[connectionId] = new Connection(socket);

    public void Remove(string connectionId) => connections.TryRemove(connectionId, out _);

    public async Task SendAsync(GatewayMessage message, CancellationToken cancellationToken)
    {
        if (!connections.TryGetValue(message.ConnectionId, out var connection)) return;
        if (connection.Socket.State != WebSocketState.Open) return;

        if (Interlocked.Increment(ref connection.PendingWrites) > MaxPendingWritesPerConnection)
        {
            Interlocked.Decrement(ref connection.PendingWrites);
            await CloseSlowConnectionAsync(message.ConnectionId, connection);
            return;
        }

        var enteredWriteGate = false;
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(writeTimeout);
            try
            {
                await connection.WriteGate.WaitAsync(timeout.Token);
                enteredWriteGate = true;
            }
            catch (OperationCanceledException)
            {
                await CloseSlowConnectionAsync(message.ConnectionId, connection);
                return;
            }

            if (connection.Socket.State == WebSocketState.Open)
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(message.Payload);
                try
                {
                    await connection.Socket.SendAsync(bytes, WebSocketMessageType.Text, true, timeout.Token);
                }
                catch (OperationCanceledException)
                {
                    await CloseSlowConnectionAsync(message.ConnectionId, connection);
                }
            }
        }
        finally
        {
            if (enteredWriteGate) connection.WriteGate.Release();
            Interlocked.Decrement(ref connection.PendingWrites);
        }
    }

    private async Task CloseSlowConnectionAsync(string connectionId, Connection connection)
    {
        connections.TryRemove(connectionId, out _);
        try { await connection.Socket.CloseOutputAsync(WebSocketCloseStatus.EndpointUnavailable, "slow client", CancellationToken.None); }
        catch (WebSocketException) { connection.Socket.Abort(); }
    }

    private sealed class Connection
    {
        public Connection(WebSocket socket) => Socket = socket;
        public WebSocket Socket { get; }
        public SemaphoreSlim WriteGate { get; } = new(1, 1);
        public int PendingWrites;
    }
}
