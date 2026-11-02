using System.Net.Http.Json;
using System.Text.Json;

namespace HitMe.Server.Authority;

/// <summary>Server-only shared AFK ban store. Requires migration 005 and a secret API key.</summary>
public sealed class SupabaseBanStore : IBanStore
{
    private readonly HttpClient http;
    private readonly Uri baseUri;
    private readonly string key;

    public SupabaseBanStore(Uri projectUrl, string secretKey, HttpClient? http = null)
    {
        if (projectUrl.Scheme != Uri.UriSchemeHttps) throw new ArgumentException("Supabase URL must use HTTPS.", nameof(projectUrl));
        if (string.IsNullOrWhiteSpace(secretKey)) throw new ArgumentException("Server secret is required.", nameof(secretKey));
        baseUri = new Uri(projectUrl, "/rest/v1/server_bans");
        key = secretKey;
        this.http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
    }

    public void Ban(string playerId, DateTimeOffset until)
    {
        if (string.IsNullOrWhiteSpace(playerId)) throw new ArgumentException("Player ID is required.", nameof(playerId));
        using var request = NewRequest(HttpMethod.Post, "?on_conflict=player_id");
        request.Headers.TryAddWithoutValidation("Prefer", "resolution=merge-duplicates,return=minimal");
        request.Content = JsonContent.Create(new[] { new { player_id = playerId, banned_until = until } });
        using var response = http.Send(request);
        EnsureSuccess(response);
    }

    public bool TryGetRemaining(string playerId, DateTimeOffset now, out int remainingSeconds)
    {
        if (string.IsNullOrWhiteSpace(playerId)) throw new ArgumentException("Player ID is required.", nameof(playerId));
        using var request = NewRequest(HttpMethod.Get,
            "?select=banned_until&player_id=eq." + Uri.EscapeDataString(playerId));
        using var response = http.Send(request);
        EnsureSuccess(response);
        using var document = JsonDocument.Parse(response.Content.ReadAsStream());
        if (document.RootElement.GetArrayLength() == 0) { remainingSeconds = 0; return false; }
        var until = document.RootElement[0].GetProperty("banned_until").GetDateTimeOffset();
        remainingSeconds = until <= now ? 0 : Math.Max(1, (int)Math.Ceiling((until - now).TotalSeconds));
        return remainingSeconds > 0;
    }

    private HttpRequestMessage NewRequest(HttpMethod method, string query)
    {
        var request = new HttpRequestMessage(method, new Uri(baseUri.ToString() + query));
        request.Headers.TryAddWithoutValidation("apikey", key);
        return request;
    }

    private static void EnsureSuccess(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Supabase ban store returned HTTP {(int)response.StatusCode}.");
    }
}
