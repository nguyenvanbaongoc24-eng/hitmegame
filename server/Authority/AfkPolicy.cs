namespace HitMe.Server.Authority;

/// <summary>Server-only policy; Phase 5 can replace the in-memory store with Supabase.</summary>
public sealed class AfkPolicy
{
    public int RoundsWithoutValidCommitBeforeElimination { get; init; } = 2;
    public int BanSeconds { get; init; } = 120;
}

public interface IBanStore
{
    void Ban(string playerId, DateTimeOffset until);
    bool TryGetRemaining(string playerId, DateTimeOffset now, out int remainingSeconds);
}

public sealed class InMemoryBanStore : IBanStore
{
    private readonly Dictionary<string, DateTimeOffset> bannedUntil = new(StringComparer.Ordinal);

    public void Ban(string playerId, DateTimeOffset until) => bannedUntil[playerId] = until;

    public bool TryGetRemaining(string playerId, DateTimeOffset now, out int remainingSeconds)
    {
        if (!bannedUntil.TryGetValue(playerId, out var until) || until <= now)
        {
            bannedUntil.Remove(playerId);
            remainingSeconds = 0;
            return false;
        }

        remainingSeconds = Math.Max(1, (int)Math.Ceiling((until - now).TotalSeconds));
        return true;
    }
}
