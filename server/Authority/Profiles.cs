namespace HitMe.Server.Authority;

/// <summary>Draft persistence boundary. Only the authoritative server may apply a final result.</summary>
public interface IProfileStore
{
    ProfileSnapshot GetOrCreate(string playerId);
    bool TryApplyMatch(MatchResult result, IRewardPolicy rewardPolicy);
}

public sealed record ProfileSnapshot(string PlayerId, int Gold, int MatchesPlayed, int BestRank);

/// <summary>Test/local implementation; data is lost on process restart.</summary>
public sealed class InMemoryProfileStore : IProfileStore
{
    private readonly object gate = new();
    private readonly Dictionary<string, ProfileSnapshot> profiles = new(StringComparer.Ordinal);
    private readonly HashSet<Guid> appliedMatches = new();

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
}
