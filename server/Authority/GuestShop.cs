using System.Net.Http.Json;
using System.Text.Json;

namespace HitMe.Server.Authority;

public sealed record CatalogItem(string Id, string Category, int Price, bool IsStarter);
public sealed record ShopResult(int Gold, string? DisplayName, int RenameCount,
    string EquippedWeaponId, string? EquippedCharacterId);
public sealed record ShopRequest(string Action, Guid RequestId, string? ItemId, string? NewName);

public sealed class ShopException(string code) : Exception(code)
{
    public string Code { get; } = code;
}

public interface IGuestShopService
{
    ShopResult Execute(string playerId, ShopRequest request);
}

public sealed class UnavailableGuestShopService : IGuestShopService
{
    public ShopResult Execute(string playerId, ShopRequest request) => throw new ShopException("shop_unavailable");
}

/// <summary>Server catalog snapshot from Unity Items.json v1; prices are never taken from a client.</summary>
public sealed class ItemCatalog
{
    private readonly Dictionary<string, CatalogItem> items;

    public ItemCatalog(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        if (document.RootElement.GetProperty("version").GetInt32() != 1)
            throw new InvalidOperationException("Unsupported item catalog version.");
        items = document.RootElement.GetProperty("items").EnumerateArray()
            .Select(item => new CatalogItem(item.GetProperty("id").GetString()!,
                item.GetProperty("category").GetString()!, item.GetProperty("price").GetInt32(),
                item.GetProperty("isStarter").GetBoolean()))
            .ToDictionary(item => item.Id, StringComparer.Ordinal);
    }

    public IReadOnlyCollection<CatalogItem> Items => items.Values;
    public bool TryGet(string? id, out CatalogItem item) => items.TryGetValue(id ?? string.Empty, out item!);
}

public sealed class InMemoryGuestShopService(InMemoryProfileStore profiles, ItemCatalog catalog) : IGuestShopService
{
    public ShopResult Execute(string playerId, ShopRequest request) => profiles.ApplyShop(playerId, request, catalog);
}

/// <summary>Server-only atomic shop RPC; migration 008 enforces ownership and idempotency.</summary>
public sealed class SupabaseGuestShopService : IGuestShopService
{
    private readonly HttpClient http;
    private readonly Uri endpoint;
    private readonly string key;
    private static readonly HashSet<string> KnownErrors = new(StringComparer.Ordinal)
    { "invalid_shop_request", "request_id_reused", "item_not_found", "already_owned",
      "insufficient_gold", "not_owned", "invalid_name", "same_name" };

    public SupabaseGuestShopService(Uri projectUrl, string secretKey, HttpClient? http = null)
    {
        if (projectUrl.Scheme != Uri.UriSchemeHttps) throw new ArgumentException("Supabase URL must use HTTPS.", nameof(projectUrl));
        HitMe.Server.Protocol.SupabaseServerKeyGuard.Validate(secretKey);
        endpoint = new Uri(projectUrl, "/rest/v1/rpc/guest_shop_action");
        key = secretKey;
        this.http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
    }

    public ShopResult Execute(string playerId, ShopRequest request)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint);
        message.Headers.TryAddWithoutValidation("apikey", key);
        message.Content = JsonContent.Create(new
        {
            p_player_id = playerId, p_request_id = request.RequestId,
            p_action = request.Action, p_item_id = request.ItemId, p_new_name = request.NewName
        });
        using var response = http.Send(message);
        if (!response.IsSuccessStatusCode)
        {
            try
            {
                using var error = JsonDocument.Parse(response.Content.ReadAsStream());
                var code = error.RootElement.TryGetProperty("message", out var value) ? value.GetString() : null;
                throw new ShopException(code is not null && KnownErrors.Contains(code) ? code : "shop_unavailable");
            }
            catch (JsonException) { throw new ShopException("shop_unavailable"); }
        }
        using var json = JsonDocument.Parse(response.Content.ReadAsStream());
        var root = json.RootElement;
        return new ShopResult(root.GetProperty("gold").GetInt32(),
            root.GetProperty("displayName").GetString(), root.GetProperty("renameCount").GetInt32(),
            root.GetProperty("equippedWeaponId").GetString()!, root.GetProperty("equippedCharacterId").GetString());
    }
}
