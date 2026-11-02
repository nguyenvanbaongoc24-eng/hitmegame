using System.Text.Json;
using HitMe.Server.Protocol;

namespace HitMe.Server.Authority;

public interface IPlayerProfileReader
{
    ProfileSnapshot Read(string playerId);
}

public sealed class InMemoryPlayerProfileReader(InMemoryProfileStore profiles) : IPlayerProfileReader
{
    public ProfileSnapshot Read(string playerId) => profiles.GetOrCreate(playerId);
}

/// <summary>Local result persistence and wallet projection share one in-memory profile store.</summary>
public sealed class ProfiledMatchResultStore(IMatchResultStore results, InMemoryProfileStore profiles) : IMatchResultStore
{
    public bool TrySave(MatchResult result)
    {
        if (!results.TrySave(result)) return false;
        profiles.TryApplyMatch(result, new ApprovedGoldByRoomSize(result.Players.Length));
        return true;
    }

    public bool TryGet(Guid matchId, out MatchResult result) => results.TryGet(matchId, out result!);
}

/// <summary>Server-only Supabase REST projection; never expose its API key to a client.</summary>
public sealed class SupabaseGuestProfileReader : IPlayerProfileReader
{
    private readonly HttpClient http;
    private readonly Uri root;
    private readonly string key;

    public SupabaseGuestProfileReader(Uri projectUrl, string secretKey, HttpClient? http = null)
    {
        if (projectUrl.Scheme != Uri.UriSchemeHttps) throw new ArgumentException("Supabase URL must use HTTPS.", nameof(projectUrl));
        SupabaseServerKeyGuard.Validate(secretKey);
        root = new Uri(projectUrl, "/rest/v1/");
        key = secretKey;
        this.http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
    }

    public ProfileSnapshot Read(string playerId)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(playerId, "^g_[0-9a-f]{32}$"))
            throw new ArgumentException("Invalid guest ID.", nameof(playerId));
        using var request = new HttpRequestMessage(HttpMethod.Get,
            new Uri(root, $"guest_profiles?select=gold,matches_played,best_rank&player_id=eq.{playerId}"));
        request.Headers.TryAddWithoutValidation("apikey", key);
        using var response = http.Send(request);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Supabase profile read returned HTTP {(int)response.StatusCode}.");
        using var json = JsonDocument.Parse(response.Content.ReadAsStream());
        if (json.RootElement.GetArrayLength() == 0) return new ProfileSnapshot(playerId, 0, 0, 0);
        var profile = json.RootElement[0];
        return new ProfileSnapshot(playerId, profile.GetProperty("gold").GetInt32(),
            profile.GetProperty("matches_played").GetInt32(), profile.GetProperty("best_rank").GetInt32());
    }
}
