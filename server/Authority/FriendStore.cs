using System.Net.Http.Json;
using System.Text.Json;
using HitMe.Server.Protocol;

namespace HitMe.Server.Authority;

public interface IFriendStore
{
    string Request(string fromPlayerId, string toPlayerId);
    string Reply(string fromPlayerId, string toPlayerId, bool accepted);
}

public sealed class InMemoryFriendStore : IFriendStore
{
    private readonly object gate = new();
    private readonly HashSet<(string From, string To)> pending = new();
    private readonly HashSet<(string First, string Second)> friends = new();

    public string Request(string fromPlayerId, string toPlayerId)
    {
        lock (gate)
        {
            var pair = Pair(fromPlayerId, toPlayerId);
            if (friends.Contains(pair)) return "already_friends";
            return pending.Add((fromPlayerId, toPlayerId)) ? "ok" : "friend_request_pending";
        }
    }

    public string Reply(string fromPlayerId, string toPlayerId, bool accepted)
    {
        lock (gate)
        {
            if (!pending.Remove((fromPlayerId, toPlayerId))) return "friend_request_not_found";
            if (accepted) friends.Add(Pair(fromPlayerId, toPlayerId));
            return "ok";
        }
    }

    private static (string First, string Second) Pair(string a, string b) =>
        string.CompareOrdinal(a, b) < 0 ? (a, b) : (b, a);
}

/// <summary>Server-only RPC; the client never receives the Supabase key.</summary>
public sealed class SupabaseFriendStore : IFriendStore
{
    private readonly Uri root;
    private readonly string key;
    private readonly HttpClient http;

    public SupabaseFriendStore(Uri projectUrl, string secretKey, HttpClient? http = null)
    {
        if (projectUrl.Scheme != Uri.UriSchemeHttps) throw new ArgumentException("Supabase URL must use HTTPS.", nameof(projectUrl));
        SupabaseServerKeyGuard.Validate(secretKey);
        root = new Uri(projectUrl, "/rest/v1/rpc/guest_friend_action");
        key = secretKey;
        this.http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
    }

    public string Request(string fromPlayerId, string toPlayerId) => Action("request", fromPlayerId, toPlayerId, null);
    public string Reply(string fromPlayerId, string toPlayerId, bool accepted) => Action("reply", fromPlayerId, toPlayerId, accepted);

    private string Action(string action, string from, string to, bool? accepted)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, root)
        {
            Content = JsonContent.Create(new { p_action = action, p_from = from, p_to = to, p_accepted = accepted })
        };
        request.Headers.TryAddWithoutValidation("apikey", key);
        using var response = http.Send(request);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Friend store returned HTTP {(int)response.StatusCode}.");
        using var payload = JsonDocument.Parse(response.Content.ReadAsStream());
        return payload.RootElement.GetString() ?? "friend_unavailable";
    }
}
