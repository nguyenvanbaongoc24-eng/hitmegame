namespace HitMe.Server.Protocol;

public sealed record ServerRuntimeOptions(
    TimeSpan IdleTimeout,
    TimeSpan KeepAliveInterval,
    IReadOnlyList<string> AllowedOrigins,
    int MaxRooms,
    int MaxConnections,
    int MaxConnectionsPerIp,
    TimeSpan JoinTimeout,
    TimeSpan ShutdownRoundWait)
{
    public static ServerRuntimeOptions FromEnvironment(Func<string, string?> read)
    {
        var idle = int.TryParse(read("IDLE_TIMEOUT_SECONDS"), out var idleSeconds) && idleSeconds > 0
            ? TimeSpan.FromSeconds(idleSeconds) : TimeSpan.FromSeconds(90);
        var origins = (read("ALLOWED_ORIGINS") ?? "http://localhost,http://127.0.0.1")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        static int Positive(Func<string, string?> read, string key, int fallback) =>
            int.TryParse(read(key), out var value) && value > 0 ? value : fallback;
        return new ServerRuntimeOptions(idle, TimeSpan.FromSeconds(30), origins,
            Positive(read, "MAX_ROOMS", 50), Positive(read, "MAX_CONNECTIONS", 120),
            Positive(read, "MAX_CONNECTIONS_PER_IP", 12),
            TimeSpan.FromSeconds(Positive(read, "JOIN_TIMEOUT_SECONDS", 10)),
            TimeSpan.FromSeconds(Positive(read, "SHUTDOWN_ROUND_WAIT_SECONDS", 6)));
    }
}
