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
    private readonly HashSet<string> allowedOrigins;

    public ConnectionAdmission(int maxConnectionsPerIp = 12, IEnumerable<string>? allowedOrigins = null, int maxTotalConnections = 500)
    {
        MaxConnectionsPerIp = maxConnectionsPerIp;
        MaxTotalConnections = maxTotalConnections;
        allowedOrigins = allowedOrigins ?? new[] { "http://localhost", "http://127.0.0.1" };
        this.allowedOrigins = new HashSet<string>(allowedOrigins, StringComparer.OrdinalIgnoreCase);
    }

    public int MaxConnectionsPerIp { get; }
    public int MaxTotalConnections { get; }
    public int TotalConnections { get { lock (gate) return totalConnections; } }

    public bool IsAllowedOrigin(string? origin) => string.IsNullOrWhiteSpace(origin) || allowedOrigins.Contains(origin);

    public bool TryAdmit(string ip, out string error)
    {
        lock (gate)
        {
            byIp.TryGetValue(ip, out var count);
            if (count >= MaxConnectionsPerIp || totalConnections >= MaxTotalConnections) { error = "connection_limit"; return false; }
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
