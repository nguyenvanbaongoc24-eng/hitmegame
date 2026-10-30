using HitMe.Core;

namespace HitMe.Server.Authority;

public enum RoundLockReason
{
    None = 0,
    AllReady = 1,
    Deadline = 2
}

public sealed record RoundCommit(int RoundId, Vec2 Position, Vec2? AimDirection);

/// <summary>
/// Server-owned placement window. It owns deadline checks and never exposes other
/// players' commits; broadcasting is deliberately deferred to the reveal phase.
/// </summary>
public sealed class Round
{
    private readonly Dictionary<string, RoundCommit> commits = new(StringComparer.Ordinal);
    private readonly HashSet<string> playerIds;
    private readonly HashSet<string> readyPlayerIds = new(StringComparer.Ordinal);
    private readonly TimeProvider clock;

    public Round(int roundId, GameConfig config, IEnumerable<string> players, TimeProvider? clock = null)
    {
        if (roundId < 1) throw new ArgumentOutOfRangeException(nameof(roundId));
        if (config is null) throw new ArgumentNullException(nameof(config));

        playerIds = new HashSet<string>(players ?? throw new ArgumentNullException(nameof(players)), StringComparer.Ordinal);
        if (playerIds.Count == 0) throw new ArgumentException("A round needs at least one player.", nameof(players));

        RoundId = roundId;
        Config = config.Clone();
        this.clock = clock ?? TimeProvider.System;
        StartedAt = this.clock.GetUtcNow();
        Deadline = StartedAt + TimeSpan.FromSeconds(Config.PlaceSeconds);
    }

    public int RoundId { get; }
    public GameConfig Config { get; }
    public DateTimeOffset StartedAt { get; }
    public DateTimeOffset Deadline { get; }
    public bool IsLocked { get; private set; }
    public RoundLockReason LockReason { get; private set; }
    public DateTimeOffset? LockedAt { get; private set; }
    public IReadOnlyDictionary<string, RoundCommit> Commits => commits;

    public bool TryCommit(string playerId, RoundCommit commit, out string error)
    {
        TryLockIfDue();
        if (IsLocked) { error = "round_locked"; return false; }
        if (!playerIds.Contains(playerId)) { error = "not_in_round"; return false; }
        if (commit.RoundId != RoundId) { error = "wrong_round"; return false; }

        if (!TryNormalize(commit, out var normalized)) { error = "invalid_commit"; return false; }

        // Last commit is authoritative until the server locks this placement window.
        commits[playerId] = normalized;
        error = string.Empty;
        return true;
    }

    public bool TryMarkReady(string playerId, out string error)
    {
        TryLockIfDue();
        if (IsLocked) { error = "round_locked"; return false; }
        if (!playerIds.Contains(playerId)) { error = "not_in_round"; return false; }

        readyPlayerIds.Add(playerId);
        if (readyPlayerIds.Count == playerIds.Count) Lock(RoundLockReason.AllReady);
        error = string.Empty;
        return true;
    }

    public bool TryLockIfDue()
    {
        if (!IsLocked && clock.GetUtcNow() >= Deadline)
        {
            Lock(RoundLockReason.Deadline);
            return true;
        }

        return IsLocked;
    }

    private void Lock(RoundLockReason reason)
    {
        IsLocked = true;
        LockReason = reason;
        LockedAt = clock.GetUtcNow();
    }

    private bool TryNormalize(RoundCommit commit, out RoundCommit normalized)
    {
        if (!Finite(commit.Position)) { normalized = default!; return false; }
        var position = new Vec2(
            Math.Clamp(commit.Position.X, Config.MinX, Config.MaxX),
            Math.Clamp(commit.Position.Y, Config.MinY, Config.MaxY));
        var aim = commit.AimDirection;
        if (aim.HasValue && (!Finite(aim.Value) || aim.Value.LengthSquared <= 0.00001f)) aim = null;
        normalized = new RoundCommit(commit.RoundId, position, aim);
        return true;
    }

    private static bool Finite(Vec2 value) =>
        !float.IsNaN(value.X) && !float.IsNaN(value.Y) &&
        !float.IsInfinity(value.X) && !float.IsInfinity(value.Y);
}
