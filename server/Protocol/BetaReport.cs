using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace HitMe.Server.Protocol;

public sealed record BetaReport(string ClientVersion, string BuildId, string Browser, string Screen,
    string? RoomCode, string Note, string[] RecentLogs);

public static class BetaReportFilter
{
    private static readonly Regex Secrets = new(@"(?i)(postgres(?:ql)?://[^\s""'<>]+|sb_(?:secret|publishable)_[A-Za-z0-9_-]+|(?:bearer\s+)?eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+|(?:token|password|secret|apikey)\s*[:=]\s*[^\s,;]+)", RegexOptions.Compiled);

    public static BetaReport Clean(BetaReport source)
    {
        static string Strip(string? value, int max)
        {
            var redacted = Secrets.Replace(value ?? string.Empty, "[redacted]");
            return redacted[..Math.Min(redacted.Length, max)];
        }
        return new BetaReport(Strip(source.ClientVersion, 40), Strip(source.BuildId, 64),
            Strip(source.Browser, 160), Strip(source.Screen, 40), Strip(source.RoomCode, 12),
            Strip(source.Note, 500), (source.RecentLogs ?? Array.Empty<string>()).Take(50).Select(line => Strip(line, 200)).ToArray());
    }

    public static bool IsValid(BetaReport report) =>
        !string.IsNullOrWhiteSpace(report.ClientVersion) && !string.IsNullOrWhiteSpace(report.BuildId) &&
        report.ClientVersion.Length <= 40 && report.BuildId.Length <= 64 &&
        (report.Browser?.Length ?? 0) <= 160 && (report.Screen?.Length ?? 0) <= 40 &&
        (report.RoomCode?.Length ?? 0) <= 12 && (report.Note?.Length ?? 0) <= 500 &&
        (report.RecentLogs?.Length ?? 0) <= 50 &&
        (report.RecentLogs ?? Array.Empty<string>()).All(line => line.Length <= 200);
}

public sealed class SupabaseBetaReportStore
{
    private readonly Uri endpoint;
    private readonly string key;
    private readonly HttpClient http;

    public SupabaseBetaReportStore(Uri projectUrl, string secretKey, HttpClient? http = null)
    {
        if (projectUrl.Scheme != Uri.UriSchemeHttps) throw new ArgumentException("HTTPS required.");
        SupabaseServerKeyGuard.Validate(secretKey);
        endpoint = new Uri(projectUrl, "/rest/v1/bug_reports");
        key = secretKey;
        this.http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
    }

    public async Task<bool> SaveAsync(BetaReport report, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(new {
                client_version = report.ClientVersion, build_id = report.BuildId,
                browser = report.Browser, screen = report.Screen, room_code = report.RoomCode,
                note = report.Note, recent_logs = report.RecentLogs
            })
        };
        request.Headers.TryAddWithoutValidation("apikey", key);
        using var response = await http.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
