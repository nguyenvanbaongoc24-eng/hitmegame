using System.Collections.Concurrent;
using System.Collections.Immutable;
using HitMe.Core;

namespace HitMe.Server.Authority;

public sealed record MatchPlayerResult(string PlayerId, int Rank, int Hp, int HitsLanded, bool RewardEligible = true);
public sealed record MatchResult(Guid MatchId, DateTimeOffset EndedAt,
    ImmutableArray<MatchPlayerResult> Players, int ProtocolVersion, string ConfigVersion);

/// <summary>One immutable final record per match ID; a future Supabase adapter can implement this boundary.</summary>
public interface IMatchResultStore
{
    bool TrySave(MatchResult result);
    bool TryGet(Guid matchId, out MatchResult result);
}

public sealed class InMemoryMatchResultStore : IMatchResultStore
{
    private readonly ConcurrentDictionary<Guid, MatchResult> results = new();
    public bool TrySave(MatchResult result) => results.TryAdd(result.MatchId, result);
    public bool TryGet(Guid matchId, out MatchResult result) => results.TryGetValue(matchId, out result!);
    public int Count => results.Count;
}

public static class MatchResultFactory
{
    public const string ConfigVersion = "core-phase1-v1.3";

    public static MatchResult Create(Guid matchId, DateTimeOffset endedAt, MatchState match,
        IReadOnlySet<string>? ineligiblePlayers = null)
    {
        var ordered = match.Players.OrderByDescending(player => player.Hp)
            .ThenByDescending(player => player.HitsLanded)
            .ThenBy(player => player.Id, StringComparer.Ordinal).ToArray();
        var players = ImmutableArray.CreateBuilder<MatchPlayerResult>(ordered.Length);
        var rank = 0;
        for (var i = 0; i < ordered.Length; i++)
        {
            if (i == 0 || ordered[i].Hp != ordered[i - 1].Hp || ordered[i].HitsLanded != ordered[i - 1].HitsLanded)
                rank = i + 1;
            players.Add(new MatchPlayerResult(ordered[i].Id, rank, ordered[i].Hp,
                ordered[i].HitsLanded, ineligiblePlayers?.Contains(ordered[i].Id) != true));
        }
        return new MatchResult(matchId, endedAt, players.MoveToImmutable(), 1, ConfigVersion);
    }
}

public interface IRewardPolicy
{
    int GoldFor(MatchPlayerResult player);
}

public sealed class NoReward : IRewardPolicy
{
    public int GoldFor(MatchPlayerResult player) => 0;
}

/// <summary>Calculation only; does not credit a wallet. Disabled unless explicitly selected.</summary>
public sealed class FixedGoldByRank : IRewardPolicy
{
    private readonly ImmutableArray<int> byRank;
    public FixedGoldByRank(IEnumerable<int> byRank)
    {
        this.byRank = byRank.ToImmutableArray();
        if (this.byRank.Any(amount => amount < 0)) throw new ArgumentOutOfRangeException(nameof(byRank));
    }
    public int GoldFor(MatchPlayerResult player) => player.Rank >= 1 && player.Rank <= byRank.Length ? byRank[player.Rank - 1] : 0;
}

/// <summary>Approved 2026-11-02; calculation only until authenticated accounts and durable crediting are connected.</summary>
public sealed class ApprovedGoldByRoomSize : IRewardPolicy
{
    private readonly int[] byRank;

    public ApprovedGoldByRoomSize(int playerCount)
    {
        byRank = playerCount switch
        {
            2 => new[] { 100, 0 },
            3 => new[] { 150, 80, 40 },
            4 => new[] { 150, 80, 40, 0 },
            5 or 6 => new[] { 200, 120, 60, 0, 0, 0 },
            _ => throw new ArgumentOutOfRangeException(nameof(playerCount))
        };
    }

    public int GoldFor(MatchPlayerResult player) =>
        player.Rank >= 1 && player.Rank <= byRank.Length ? byRank[player.Rank - 1] : 0;
}
