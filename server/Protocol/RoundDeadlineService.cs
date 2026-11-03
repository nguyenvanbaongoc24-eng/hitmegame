namespace HitMe.Server.Protocol;

/// <summary>Checks server-owned placement deadlines without relying on client traffic.</summary>
public sealed class RoundDeadlineService : BackgroundService
{
    private readonly ProtocolGateway gateway;
    private readonly WebSocketConnectionRegistry connections;
    private readonly ServerRuntimeOptions options;

    public RoundDeadlineService(ProtocolGateway gateway, WebSocketConnectionRegistry connections, ServerRuntimeOptions options)
    {
        this.gateway = gateway;
        this.connections = connections;
        this.options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var idle = await connections.CloseIdleAsync(options.IdleTimeout);
            foreach (var connectionId in idle) gateway.Disconnect(connectionId);
            var unjoined = await connections.CloseUnjoinedAsync(options.JoinTimeout, gateway.HasJoinedOrQueued);
            foreach (var connectionId in unjoined) gateway.Disconnect(connectionId);
            var messages = gateway.Tick();
            await Task.WhenAll(messages.Select(message => connections.SendAsync(message, stoppingToken)));
            foreach (var message in messages.Where(message => message.CloseReason is not null))
                await connections.CloseConnectionAsync(message.ConnectionId, message.CloseReason!);
        }
    }
}
