namespace HitMe.Server.Authority;

/// <summary>Draft persistence boundary. Only the authoritative server may apply a final result.</summary>
public interface IProfileStore
{
    ProfileSnapshot GetOrCreate(string playerId);
    bool TryApplyMatch(MatchResult result, IRewardPolicy rewardPolicy);
}

public sealed record ProfileSnapshot(string PlayerId, int Gold, int MatchesPlayed, int BestRank,
    string? DisplayName = null, int RenameCount = 0, IReadOnlyList<string>? PurchasedItemIds = null,
    string EquippedWeaponId = "dep_to_ong", string? EquippedCharacterId = null);

/// <summary>Test/local implementation; data is lost on process restart.</summary>
public sealed class InMemoryProfileStore : IProfileStore
{
    private readonly object gate = new();
    private readonly Dictionary<string, ProfileSnapshot> profiles = new(StringComparer.Ordinal);
    private readonly HashSet<Guid> appliedMatches = new();
    private readonly Dictionary<(string PlayerId, Guid RequestId), (ShopRequest Request, ShopResult Result)> shopRequests = new();
    private readonly Dictionary<string, HashSet<string>> ownedItems = new(StringComparer.Ordinal);
    public int RenameCostAfterFirst { get; init; } = 200;

    public ProfileSnapshot GetOrCreate(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId)) throw new ArgumentException("Player ID is required.", nameof(playerId));
        lock (gate)
        {
            if (!profiles.TryGetValue(playerId, out var profile))
                profiles[playerId] = profile = new ProfileSnapshot(playerId, 0, 0, 0);
            return profile;
        }
    }

    public bool TryApplyMatch(MatchResult result, IRewardPolicy rewardPolicy)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(rewardPolicy);
        lock (gate)
        {
            if (appliedMatches.Contains(result.MatchId)) return false;
            var updates = new Dictionary<string, ProfileSnapshot>(StringComparer.Ordinal);
            foreach (var player in result.Players)
            {
                if (string.IsNullOrWhiteSpace(player.PlayerId) || player.Rank < 1 || updates.ContainsKey(player.PlayerId))
                    throw new ArgumentException("Invalid match player result.", nameof(result));
                if (!profiles.TryGetValue(player.PlayerId, out var prior))
                    prior = new ProfileSnapshot(player.PlayerId, 0, 0, 0);
                var reward = player.RewardEligible && player.Rank <= 3 ? rewardPolicy.GoldFor(player) : 0;
                if (reward < 0) throw new InvalidOperationException("Reward policy returned negative gold.");
                updates[player.PlayerId] = prior with
                {
                    Gold = checked(prior.Gold + reward),
                    MatchesPlayed = checked(prior.MatchesPlayed + 1),
                    BestRank = prior.BestRank == 0 ? player.Rank : Math.Min(prior.BestRank, player.Rank)
                };
            }
            foreach (var update in updates) profiles[update.Key] = update.Value;
            appliedMatches.Add(result.MatchId);
            return true;
        }
    }

    public ShopResult ApplyShop(string playerId, ShopRequest request, ItemCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(request);
        lock (gate)
        {
            if (string.IsNullOrWhiteSpace(playerId) || request.RequestId == Guid.Empty
                || request.Action is not ("buy" or "equip" or "rename"))
                throw new ShopException("invalid_shop_request");
            if (shopRequests.TryGetValue((playerId, request.RequestId), out var prior))
            {
                if (prior.Request != request) throw new ShopException("request_id_reused");
                return prior.Result;
            }
            if (!profiles.TryGetValue(playerId, out var profile))
                profile = new ProfileSnapshot(playerId, 0, 0, 0);
            var owned = ownedItems.TryGetValue(playerId, out var existing)
                ? existing : new HashSet<string>(StringComparer.Ordinal);

            if (request.Action is "buy" or "equip")
            {
                if (!catalog.TryGet(request.ItemId, out var item)) throw new ShopException("item_not_found");
                if (request.Action == "buy")
                {
                    if (item.IsStarter || owned.Contains(item.Id)) throw new ShopException("already_owned");
                    if (profile.Gold < item.Price) throw new ShopException("insufficient_gold");
                    profile = profile with { Gold = profile.Gold - item.Price };
                    owned.Add(item.Id);
                }
                else
                {
                    if (!item.IsStarter && !owned.Contains(item.Id)) throw new ShopException("not_owned");
                    profile = item.Category == "weapon"
                        ? profile with { EquippedWeaponId = item.Id }
                        : profile with { EquippedCharacterId = item.Id };
                }
            }
            else
            {
                var name = request.NewName?.Trim();
                if (name is null || name.Length is < 3 or > 24
                    || name.Any(character => char.IsControl(character) || character is '<' or '>')
                    || new[] { "admin", "moderator", "fuck" }.Any(word => name.Contains(word, StringComparison.OrdinalIgnoreCase)))
                    throw new ShopException("invalid_name");
                if (profile.DisplayName == name) throw new ShopException("same_name");
                var cost = profile.RenameCount == 0 ? 0 : RenameCostAfterFirst;
                if (profile.Gold < cost) throw new ShopException("insufficient_gold");
                profile = profile with { Gold = profile.Gold - cost, DisplayName = name,
                    RenameCount = profile.RenameCount + 1 };
            }
            profile = profile with { PurchasedItemIds = owned.Order(StringComparer.Ordinal).ToArray() };
            profiles[playerId] = profile;
            ownedItems[playerId] = owned;
            var result = new ShopResult(profile.Gold, profile.DisplayName, profile.RenameCount,
                profile.EquippedWeaponId, profile.EquippedCharacterId);
            shopRequests[(playerId, request.RequestId)] = (request, result);
            return result;
        }
    }
}
