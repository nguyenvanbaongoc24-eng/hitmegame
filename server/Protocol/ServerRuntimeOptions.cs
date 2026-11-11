namespace HitMe.Server.Protocol;

public sealed record ServerRuntimeOptions(
    TimeSpan IdleTimeout,
    TimeSpan KeepAliveInterval,
    IReadOnlyList<string> AllowedOrigins,
    bool AllowMissingOrigin,
    bool AllowVercelPreviewOrigins,
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
        // An unset list must not accidentally expose a deployed server to every
        // browser. Local development explicitly supplies its origins.
        var origins = (read("ALLOWED_ORIGINS") ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ConnectionAdmission.NormalizeConfiguredOrigin)
            .Where(origin => origin is not null)
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        static int Positive(Func<string, string?> read, string key, int fallback) =>
            int.TryParse(read(key), out var value) && value > 0 ? value : fallback;
        return new ServerRuntimeOptions(idle, TimeSpan.FromSeconds(30), origins,
            IsEnabled(read("ALLOW_MISSING_ORIGIN")), IsEnabled(read("ALLOW_VERCEL_PREVIEW_ORIGINS")),
            Positive(read, "MAX_ROOMS", 50), Positive(read, "MAX_CONNECTIONS", 120),
            Positive(read, "MAX_CONNECTIONS_PER_IP", 12),
            TimeSpan.FromSeconds(Positive(read, "JOIN_TIMEOUT_SECONDS", 10)),
            TimeSpan.FromSeconds(Positive(read, "SHUTDOWN_ROUND_WAIT_SECONDS", 6)));
    }

    private static bool IsEnabled(string? value) =>
        string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
}
