using System.Collections.Concurrent;

namespace HitMe.Server.Protocol;

/// <summary>
/// Server-side admission limits.  This intentionally contains no player or commit data,
/// so it can be used before a WebSocket is accepted.
/// </summary>
public sealed class ConnectionAdmission
{
    private readonly ConcurrentDictionary<string, int> byIp = new(StringComparer.Ordinal);
    private readonly HashSet<string> allowedOrigins;

    public ConnectionAdmission(int maxConnectionsPerIp = 12, IEnumerable<string>? allowedOrigins = null)
    {
        MaxConnectionsPerIp = maxConnectionsPerIp;
        allowedOrigins = allowedOrigins ?? new[] { "http://localhost", "http://127.0.0.1" };
        this.allowedOrigins = new HashSet<string>(allowedOrigins, StringComparer.OrdinalIgnoreCase);
    }

    public int MaxConnectionsPerIp { get; }

    public bool IsAllowedOrigin(string? origin) => string.IsNullOrWhiteSpace(origin) || allowedOrigins.Contains(origin);

    public bool TryAdmit(string ip, out string error)
    {
        var total = byIp.AddOrUpdate(ip, 1, (_, current) => current + 1);
        if (total <= MaxConnectionsPerIp) { error = string.Empty; return true; }
        Release(ip);
        error = "connection_limit";
        return false;
    }

    public void Release(string ip)
    {
        while (byIp.TryGetValue(ip, out var current))
        {
            if (current <= 1) { byIp.TryRemove(new KeyValuePair<string, int>(ip, current)); return; }
            if (byIp.TryUpdate(ip, current - 1, current)) return;
        }
    }
}
