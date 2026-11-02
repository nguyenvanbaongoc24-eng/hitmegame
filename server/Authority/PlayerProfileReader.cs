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
    private readonly bool shopSchemaEnabled;

    public SupabaseGuestProfileReader(Uri projectUrl, string secretKey, HttpClient? http = null, bool shopSchemaEnabled = false)
    {
        if (projectUrl.Scheme != Uri.UriSchemeHttps) throw new ArgumentException("Supabase URL must use HTTPS.", nameof(projectUrl));
        SupabaseServerKeyGuard.Validate(secretKey);
        root = new Uri(projectUrl, "/rest/v1/");
        key = secretKey;
        this.http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
        this.shopSchemaEnabled = shopSchemaEnabled;
    }

    public ProfileSnapshot Read(string playerId)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(playerId, "^g_[0-9a-f]{32}$"))
            throw new ArgumentException("Invalid guest ID.", nameof(playerId));
        var fields = shopSchemaEnabled
            ? "gold,matches_played,best_rank,display_name,rename_count,equipped_weapon_id,equipped_character_id"
            : "gold,matches_played,best_rank";
        using var request = new HttpRequestMessage(HttpMethod.Get,
            new Uri(root, $"guest_profiles?select={fields}&player_id=eq.{playerId}"));
        request.Headers.TryAddWithoutValidation("apikey", key);
        using var response = http.Send(request);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Supabase profile read returned HTTP {(int)response.StatusCode}.");
        using var json = JsonDocument.Parse(response.Content.ReadAsStream());
        if (json.RootElement.GetArrayLength() == 0) return new ProfileSnapshot(playerId, 0, 0, 0);
        var profile = json.RootElement[0];
        if (!shopSchemaEnabled)
            return new ProfileSnapshot(playerId, profile.GetProperty("gold").GetInt32(),
                profile.GetProperty("matches_played").GetInt32(), profile.GetProperty("best_rank").GetInt32());
        using var inventoryRequest = new HttpRequestMessage(HttpMethod.Get,
            new Uri(root, $"guest_inventory?select=item_id&player_id=eq.{playerId}"));
        inventoryRequest.Headers.TryAddWithoutValidation("apikey", key);
        using var inventoryResponse = http.Send(inventoryRequest);
        if (!inventoryResponse.IsSuccessStatusCode)
            throw new HttpRequestException($"Supabase inventory read returned HTTP {(int)inventoryResponse.StatusCode}.");
        using var inventory = JsonDocument.Parse(inventoryResponse.Content.ReadAsStream());
        var purchased = inventory.RootElement.EnumerateArray()
            .Select(row => row.GetProperty("item_id").GetString()!).ToArray();
        return new ProfileSnapshot(playerId, profile.GetProperty("gold").GetInt32(),
            profile.GetProperty("matches_played").GetInt32(), profile.GetProperty("best_rank").GetInt32(),
            profile.GetProperty("display_name").GetString(), profile.GetProperty("rename_count").GetInt32(),
            purchased, profile.GetProperty("equipped_weapon_id").GetString()!,
            profile.GetProperty("equipped_character_id").GetString());
    }
}
