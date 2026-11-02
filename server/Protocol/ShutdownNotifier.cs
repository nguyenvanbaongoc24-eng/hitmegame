using HitMe.Core;
using HitMe.Server.Authority;

namespace HitMe.Server.Protocol;

/// <summary>Hosted-service stop is awaited by ASP.NET Core during SIGTERM graceful shutdown.</summary>
public sealed class ShutdownNotifier : IHostedService
{
    private readonly WebSocketConnectionRegistry connections;
    private readonly RoomDirectory rooms;
    private readonly ServerRuntimeOptions options;
    public ShutdownNotifier(WebSocketConnectionRegistry connections, RoomDirectory rooms, ServerRuntimeOptions options)
    {
        this.connections = connections;
        this.rooms = rooms;
        this.options = options;
    }
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Snapshot only rounds already active at shutdown. A newly opened round
        // must not extend the grace period. The deadline service is still alive
        // while this hosted service stops (registered earlier in Program).
        var active = rooms.Snapshot()
            .Where(room => room.RoundSession is { } session && session.Match.State.Phase == Phase.Placing)
            .Select(room => (Room: room, RoundId: room.RoundSession!.ActiveRound.RoundId)).ToArray();
        var deadline = DateTimeOffset.UtcNow + options.ShutdownRoundWait;
        while (!cancellationToken.IsCancellationRequested && DateTimeOffset.UtcNow < deadline &&
               active.Any(item => item.Room.RoundSession is { } session &&
                   session.Match.State.Phase == Phase.Placing && session.ActiveRound.RoundId == item.RoundId))
        {
            var remaining = deadline - DateTimeOffset.UtcNow;
            if (remaining <= TimeSpan.Zero) break;
            await Task.Delay(remaining < TimeSpan.FromMilliseconds(50) ? remaining : TimeSpan.FromMilliseconds(50), CancellationToken.None);
        }
        await connections.NotifyRestartAndCloseAsync();
    }
}
