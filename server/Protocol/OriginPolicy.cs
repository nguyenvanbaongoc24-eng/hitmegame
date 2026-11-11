namespace HitMe.Server.Protocol;

/// <summary>Explicit, fail-closed browser Origin policy shared by HTTP and WebSocket admission.</summary>
public sealed class OriginPolicy
{
    private readonly HashSet<string> allowedOrigins;
    private readonly bool allowMissingOrigin;
    private readonly bool allowVercelPreviewOrigins;

    public OriginPolicy(IEnumerable<string>? allowedOrigins = null, bool allowMissingOrigin = false,
        bool allowVercelPreviewOrigins = false)
    {
        this.allowMissingOrigin = allowMissingOrigin;
        this.allowVercelPreviewOrigins = allowVercelPreviewOrigins;
        this.allowedOrigins = new HashSet<string>((allowedOrigins ?? Array.Empty<string>())
            .Select(NormalizeConfiguredOrigin).Where(origin => origin is not null).Cast<string>(), StringComparer.Ordinal);
    }

    public int AllowedOriginsCount => allowedOrigins.Count;
    public bool OriginsConfigured => allowedOrigins.Count > 0;

    public bool IsAllowed(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin)) return allowMissingOrigin;
        if (!TryNormalize(origin, out var normalized)) return false;
        return allowedOrigins.Contains(normalized)
            || (allowVercelPreviewOrigins && IsVercelPreviewOrigin(normalized));
    }

    /// <summary>Canonical browser-origin form: lower case, no path, no default port.</summary>
    public static string? NormalizeConfiguredOrigin(string? origin) => TryNormalize(origin, out var normalized) ? normalized : null;

    private static bool TryNormalize(string? origin, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(origin)
            || !Uri.TryCreate(origin.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || !string.IsNullOrEmpty(uri.UserInfo)
            || (uri.AbsolutePath != "/" && uri.AbsolutePath.Length != 0)
            || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment)) return false;
        normalized = $"{uri.Scheme.ToLowerInvariant()}://{uri.IdnHost.ToLowerInvariant()}";
        if (!uri.IsDefaultPort) normalized += $":{uri.Port}";
        return true;
    }

    private static bool IsVercelPreviewOrigin(string origin)
    {
        const string prefix = "https://hitmegame-";
        const string suffix = ".vercel.app";
        return origin.StartsWith(prefix, StringComparison.Ordinal)
            && origin.EndsWith(suffix, StringComparison.Ordinal)
            && origin.Length > prefix.Length + suffix.Length;
    }
}
