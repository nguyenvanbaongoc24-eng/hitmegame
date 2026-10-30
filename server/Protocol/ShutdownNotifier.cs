namespace HitMe.Server.Protocol;

/// <summary>Hosted-service stop is awaited by ASP.NET Core during SIGTERM graceful shutdown.</summary>
public sealed class ShutdownNotifier : IHostedService
{
    private readonly WebSocketConnectionRegistry connections;
    public ShutdownNotifier(WebSocketConnectionRegistry connections) => this.connections = connections;
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => connections.CloseAllAsync("server_shutdown");
}
