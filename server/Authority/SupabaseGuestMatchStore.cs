using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HitMe.Server.Authority;

/// <summary>Server-only result writer; migration 006 credits signed guests atomically.</summary>
public sealed class SupabaseGuestMatchStore : IMatchResultStore
{
    private static readonly Regex GuestId = new("^g_[0-9a-f]{32}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private readonly HttpClient http;
    private readonly Uri root;
    private readonly string key;

    public SupabaseGuestMatchStore(Uri projectUrl, string secretKey, HttpClient? http = null)
    {
        if (projectUrl.Scheme != Uri.UriSchemeHttps) throw new ArgumentException("Supabase URL must use HTTPS.", nameof(projectUrl));
        if (string.IsNullOrWhiteSpace(secretKey)) throw new ArgumentException("Server secret is required.", nameof(secretKey));
        root = new Uri(projectUrl, "/rest/v1/");
        key = secretKey;
        this.http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
    }

    public bool TrySave(MatchResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Players.Length is < 2 or > 6 || result.Players.Any(p => !GuestId.IsMatch(p.PlayerId)))
            throw new ArgumentException("Durable rewards require signed guest IDs for every player.", nameof(result));
        using var request = Request(HttpMethod.Post, "rpc/record_guest_match");
        request.Content = JsonContent.Create(new
        {
            p_match_id = result.MatchId,
            p_ended_at = result.EndedAt,
            p_protocol_version = result.ProtocolVersion,
            p_config_version = result.ConfigVersion,
            p_players = result.Players.Select(p => new
            {
                player_id = p.PlayerId, rank = p.Rank, hp = p.Hp,
                hits_landed = p.HitsLanded, reward_eligible = p.RewardEligible
            }).ToArray()
        });
        using var response = http.Send(request);
        EnsureSuccess(response);
        return JsonSerializer.Deserialize<bool>(response.Content.ReadAsStream());
    }

    public bool TryGet(Guid matchId, out MatchResult result)
    {
        using var matchRequest = Request(HttpMethod.Get,
            $"matches?select=ended_at,protocol_version,config_version&id=eq.{matchId:D}");
        using var matchResponse = http.Send(matchRequest);
        EnsureSuccess(matchResponse);
        using var matchJson = JsonDocument.Parse(matchResponse.Content.ReadAsStream());
        if (matchJson.RootElement.GetArrayLength() == 0) { result = null!; return false; }
        var match = matchJson.RootElement[0];
        using var playersRequest = Request(HttpMethod.Get,
            $"match_players?select=server_player_id,placement,final_hp,hits_landed,reward_eligible&match_id=eq.{matchId:D}&order=placement.asc,server_player_id.asc");
        using var playersResponse = http.Send(playersRequest);
        EnsureSuccess(playersResponse);
        using var playersJson = JsonDocument.Parse(playersResponse.Content.ReadAsStream());
        var players = ImmutableArray.CreateBuilder<MatchPlayerResult>(playersJson.RootElement.GetArrayLength());
        foreach (var player in playersJson.RootElement.EnumerateArray())
            players.Add(new MatchPlayerResult(
                player.GetProperty("server_player_id").GetString()!,
                player.GetProperty("placement").GetInt32(),
                player.GetProperty("final_hp").GetInt32(),
                player.GetProperty("hits_landed").GetInt32(),
                player.GetProperty("reward_eligible").GetBoolean()));
        result = new MatchResult(matchId, match.GetProperty("ended_at").GetDateTimeOffset(),
            players.MoveToImmutable(), match.GetProperty("protocol_version").GetInt32(),
            match.GetProperty("config_version").GetString()!);
        return true;
    }

    private HttpRequestMessage Request(HttpMethod method, string relative)
    {
        var request = new HttpRequestMessage(method, new Uri(root, relative));
        request.Headers.TryAddWithoutValidation("apikey", key);
        return request;
    }

    private static void EnsureSuccess(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Supabase match store returned HTTP {(int)response.StatusCode}.");
    }
}
