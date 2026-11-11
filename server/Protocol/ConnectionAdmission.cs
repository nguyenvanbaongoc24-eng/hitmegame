namespace HitMe.Server.Protocol;

/// <summary>
/// Server-side admission limits.  This intentionally contains no player or commit data,
/// so it can be used before a WebSocket is accepted.
/// </summary>
public sealed class ConnectionAdmission
{
    private readonly Dictionary<string, int> byIp = new(StringComparer.Ordinal);
    private readonly object gate = new();
    private int totalConnections;
    private readonly OriginPolicy originPolicy;

    public ConnectionAdmission(int maxConnectionsPerIp = 12, IEnumerable<string>? allowedOrigins = null,
        int maxTotalConnections = 500, bool allowMissingOrigin = false, bool allowVercelPreviewOrigins = false)
    {
        MaxConnectionsPerIp = maxConnectionsPerIp;
        MaxTotalConnections = maxTotalConnections;
        originPolicy = new OriginPolicy(allowedOrigins, allowMissingOrigin, allowVercelPreviewOrigins);
    }

    public int MaxConnectionsPerIp { get; }
    public int MaxTotalConnections { get; }
    public int TotalConnections { get { lock (gate) return totalConnections; } }

    public int AllowedOriginsCount => originPolicy.AllowedOriginsCount;
    public bool OriginsConfigured => originPolicy.OriginsConfigured;
    public bool IsAllowedOrigin(string? origin) => originPolicy.IsAllowed(origin);

    public bool TryAdmit(string ip, out string error)
    {
        lock (gate)
        {
            byIp.TryGetValue(ip, out var count);
            if (count >= MaxConnectionsPerIp) { error = "connection_limit"; return false; }
            if (totalConnections >= MaxTotalConnections) { error = "server_busy"; return false; }
            byIp[ip] = count + 1;
            totalConnections++;
            error = string.Empty;
            return true;
        }
    }

    public void Release(string ip)
    {
        lock (gate)
        {
            if (!byIp.TryGetValue(ip, out var current)) return;
            if (current <= 1) byIp.Remove(ip);
            else byIp[ip] = current - 1;
            totalConnections--;
        }
    }
}
