using HitMe.Core;

namespace HitMe.Server.Authority;

/// <summary>
/// Adapts private server commits into Core/Phase1 inputs. Combat resolution,
/// placement constraints, HP, elimination and tiebreaks remain in MatchState.
/// </summary>
public sealed class ServerMatch
{
    private readonly GameConfig config;

    public ServerMatch(GameConfig config, IEnumerable<string> playerIds)
    {
        this.config = (config ?? throw new ArgumentNullException(nameof(config))).Clone();
        State = new MatchState(this.config, CreatePlayers(this.config, playerIds));
    }

    public MatchState State { get; }
    public Round? ActiveRound { get; private set; }

    public Round BeginRound(TimeProvider? clock = null)
    {
        if (State.Phase == Phase.MatchEnd) throw new InvalidOperationException("Match has ended.");

        State.StartRound();
        ActiveRound = new Round(
            State.RoundNumber,
            config,
            State.Players.Where(player => player.IsAlive).Select(player => player.Id),
            clock);
        return ActiveRound;
    }

    public RoundResolution ResolveLockedRound()
    {
        var round = ActiveRound ?? throw new InvalidOperationException("No active round.");
        round.TryLockIfDue();
        if (!round.IsLocked) throw new InvalidOperationException("Round is not locked.");

        foreach (var player in State.Players)
        {
            if (!player.IsAlive) continue;

            var input = new RoundInput(player.Id) { IsReady = true };
            if (round.Commits.TryGetValue(player.Id, out var commit))
            {
                input.HasPlacement = true;
                input.Placement = commit.Position;
                if (HasUsableAim(commit.AimDirection))
                {
                    input.HasAim = true;
                    input.AimDirection = commit.AimDirection!.Value;
                }
            }

            // No commit keeps the previous position. On round 1 that position was
            // seeded below; NoAimRule=ThrowFacing is then applied by MatchState.
            State.SetInput(input);
        }

        State.LockRound();
        State.BeginReveal();
        State.BeginThrowing();
        return State.ResolveRound();
    }

    private static bool HasUsableAim(Vec2? aim)
    {
        if (!aim.HasValue) return false;
        var value = aim.Value;
        return !float.IsNaN(value.X) && !float.IsNaN(value.Y) &&
               !float.IsInfinity(value.X) && !float.IsInfinity(value.Y) &&
               value.LengthSquared > 0.00001f;
    }

    private static IEnumerable<PlayerState> CreatePlayers(GameConfig config, IEnumerable<string> playerIds)
    {
        if (playerIds is null) throw new ArgumentNullException(nameof(playerIds));

        var random = new SeededRandom(config.Seed);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var playerId in playerIds)
        {
            if (string.IsNullOrWhiteSpace(playerId) || !seen.Add(playerId))
                throw new ArgumentException("Player ids must be unique and non-empty.", nameof(playerIds));

            var position = new Vec2(
                random.Range(config.MinX, config.MaxX),
                random.Range(config.MinY, config.MaxY));
            yield return new PlayerState(playerId, playerId, false, config.StartHp, position, Vec2.Up);
        }
    }
}
