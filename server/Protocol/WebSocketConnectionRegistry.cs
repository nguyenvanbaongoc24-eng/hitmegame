using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Diagnostics;

namespace HitMe.Server.Protocol;

/// <summary>Serializes writes per socket so a room broadcast cannot race a reply.</summary>
public sealed class WebSocketConnectionRegistry
{
    public const int MaxPendingWritesPerConnection = 4;
    private readonly ConcurrentDictionary<string, Connection> connections = new(StringComparer.Ordinal);
    public int Count => connections.Count;
    private readonly TimeSpan writeTimeout;
    private readonly ServerMetrics? metrics;

    public WebSocketConnectionRegistry(TimeSpan? writeTimeout = null, ServerMetrics? metrics = null)
    {
        this.writeTimeout = writeTimeout ?? TimeSpan.FromSeconds(1);
        this.metrics = metrics;
    }

    public void Register(string connectionId, WebSocket socket) => connections[connectionId] = new Connection(socket, DateTimeOffset.UtcNow);

    public void Remove(string connectionId) => connections.TryRemove(connectionId, out _);

    public void Touch(string connectionId, DateTimeOffset? now = null)
    {
        if (connections.TryGetValue(connectionId, out var connection)) connection.LastActivity = now ?? DateTimeOffset.UtcNow;
    }

    public async Task<IReadOnlyList<string>> CloseIdleAsync(TimeSpan timeout, DateTimeOffset? now = null)
    {
        var timestamp = now ?? DateTimeOffset.UtcNow;
        var closed = new List<string>();
        foreach (var item in connections)
        {
            if (timestamp - item.Value.LastActivity < timeout) continue;
            if (connections.TryRemove(item.Key, out var connection))
            {
                closed.Add(item.Key);
                await CloseAsync(connection, "idle_timeout");
            }
        }
        return closed;
    }

    public async Task<IReadOnlyList<string>> CloseUnjoinedAsync(TimeSpan timeout, Func<string, bool> isJoined, DateTimeOffset? now = null)
    {
        var timestamp = now ?? DateTimeOffset.UtcNow;
        var closed = new List<string>();
        foreach (var item in connections)
        {
            if (timestamp - item.Value.RegisteredAt < timeout || isJoined(item.Key)) continue;
            if (connections.TryRemove(item.Key, out var connection))
            {
                closed.Add(item.Key);
                await CloseAsync(connection, "join_timeout");
            }
        }
        return closed;
    }

    public async Task CloseAllAsync(string reason)
    {
        foreach (var item in connections.ToArray())
            if (connections.TryRemove(item.Key, out var connection)) await CloseAsync(connection, reason);
    }

    public async Task CloseConnectionAsync(string connectionId, string reason)
    {
        if (connections.TryRemove(connectionId, out var connection)) await CloseAsync(connection, reason);
    }

    public async Task SendAsync(GatewayMessage message, CancellationToken cancellationToken)
    {
        var watch = Stopwatch.StartNew();
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
                    metrics?.RecordBroadcast(watch.Elapsed.TotalMilliseconds);
                }
                catch (OperationCanceledException)
                {
                    await CloseSlowConnectionAsync(message.ConnectionId, connection);
                }
                catch (WebSocketException)
                {
                    // A peer may vanish between the state check and SendAsync.
                    // It must not abort the room handler or other players' fan-out.
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

    private static async Task CloseAsync(Connection connection, string reason)
    {
        try { await connection.Socket.CloseOutputAsync(WebSocketCloseStatus.EndpointUnavailable, reason, CancellationToken.None); }
        catch (WebSocketException) { connection.Socket.Abort(); }
        finally { connection.Socket.Abort(); }
    }

    private sealed class Connection
    {
        public Connection(WebSocket socket, DateTimeOffset lastActivity) { Socket = socket; LastActivity = lastActivity; RegisteredAt = lastActivity; }
        public WebSocket Socket { get; }
        public DateTimeOffset RegisteredAt { get; }
        public SemaphoreSlim WriteGate { get; } = new(1, 1);
        public int PendingWrites;
        public DateTimeOffset LastActivity;
    }
}
