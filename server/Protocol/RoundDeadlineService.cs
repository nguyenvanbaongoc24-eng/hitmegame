namespace HitMe.Server.Protocol;

/// <summary>Checks server-owned placement deadlines without relying on client traffic.</summary>
public sealed class RoundDeadlineService : BackgroundService
{
    private readonly ProtocolGateway gateway;
    private readonly WebSocketConnectionRegistry connections;

    public RoundDeadlineService(ProtocolGateway gateway, WebSocketConnectionRegistry connections)
    {
        this.gateway = gateway;
        this.connections = connections;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            foreach (var message in gateway.Tick())
                await connections.SendAsync(message, stoppingToken);
        }
    }
}
