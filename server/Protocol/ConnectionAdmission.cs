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
    private readonly bool allowMissingOrigin;
    private readonly bool allowVercelPreviewOrigins;

    public ConnectionAdmission(int maxConnectionsPerIp = 12, IEnumerable<string>? allowedOrigins = null,
        int maxTotalConnections = 500, bool allowMissingOrigin = false, bool allowVercelPreviewOrigins = false)
    {
        MaxConnectionsPerIp = maxConnectionsPerIp;
        MaxTotalConnections = maxTotalConnections;
        this.allowMissingOrigin = allowMissingOrigin;
        this.allowVercelPreviewOrigins = allowVercelPreviewOrigins;
        this.allowedOrigins = new HashSet<string>(
            (allowedOrigins ?? Array.Empty<string>())
                .Select(NormalizeConfiguredOrigin)
                .Where(origin => origin is not null)
                .Cast<string>(), StringComparer.Ordinal);
    }

    public int MaxConnectionsPerIp { get; }
    public int MaxTotalConnections { get; }
    public int TotalConnections { get { lock (gate) return totalConnections; } }

    public int AllowedOriginsCount => allowedOrigins.Count;
    public bool OriginsConfigured => allowedOrigins.Count > 0;

    public bool IsAllowedOrigin(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin)) return allowMissingOrigin;
        if (!TryNormalizeOrigin(origin, out var normalized)) return false;
        return allowedOrigins.Contains(normalized)
            || (allowVercelPreviewOrigins && IsVercelPreviewOrigin(normalized));
    }

    /// <summary>Canonical browser-origin form: lower case, no path, no default port.</summary>
    public static string? NormalizeConfiguredOrigin(string? origin) =>
        TryNormalizeOrigin(origin, out var normalized) ? normalized : null;

    private static bool TryNormalizeOrigin(string? origin, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(origin)
            || !Uri.TryCreate(origin.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || !string.IsNullOrEmpty(uri.UserInfo)
            || (uri.AbsolutePath != "/" && uri.AbsolutePath.Length != 0)
            || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment)) return false;
        var host = uri.IdnHost.ToLowerInvariant();
        normalized = $"{uri.Scheme.ToLowerInvariant()}://{host}";
        if (!uri.IsDefaultPort) normalized += $":{uri.Port}";
        return true;
    }

    private static bool IsVercelPreviewOrigin(string origin)
    {
        if (!origin.StartsWith("https://hitmegame-", StringComparison.Ordinal)) return false;
        const string suffix = ".vercel.app";
        return origin.EndsWith(suffix, StringComparison.Ordinal)
            && origin.Length > "https://hitmegame-".Length + suffix.Length;
    }

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
