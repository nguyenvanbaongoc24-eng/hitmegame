namespace HitMe.Server.Protocol;

public sealed record ServerRuntimeOptions(
    TimeSpan IdleTimeout,
    TimeSpan KeepAliveInterval,
    IReadOnlyList<string> AllowedOrigins)
{
    public static ServerRuntimeOptions FromEnvironment(Func<string, string?> read)
    {
        var idle = int.TryParse(read("IDLE_TIMEOUT_SECONDS"), out var idleSeconds) && idleSeconds > 0
            ? TimeSpan.FromSeconds(idleSeconds) : TimeSpan.FromSeconds(90);
        var origins = (read("ALLOWED_ORIGINS") ?? "http://localhost,http://127.0.0.1")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return new ServerRuntimeOptions(idle, TimeSpan.FromSeconds(30), origins);
    }
}
