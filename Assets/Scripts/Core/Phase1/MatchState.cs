using System;
using System.Collections.Generic;

namespace HitMe.Core
{
    public sealed class MatchState
    {
        private readonly Dictionary<string, RoundInput> _inputs = new Dictionary<string, RoundInput>();
        private readonly List<string> _winnerIds = new List<string>();
        public GameConfig Config { get; private set; }
        public List<PlayerState> Players { get; private set; }
        public int RoundNumber { get; private set; }
        public Phase Phase { get; private set; }
        public IReadOnlyList<string> WinnerIds { get { return _winnerIds; } }
        public RoundResolution LastResolution { get; private set; }

        public MatchState(GameConfig config, IEnumerable<PlayerState> players)
        {
            Config = config.Clone();
            Players = new List<PlayerState>(players);
            RoundNumber = 0;
            Phase = Phase.RoundEnd;
        }

        public void StartRound()
        {
            if (Phase == Phase.MatchEnd) return;
            RoundNumber++;
            _inputs.Clear();
            LastResolution = null;
            Phase = Phase.Placing;
        }

        public bool SetInput(RoundInput input)
        {
            if (Phase != Phase.Placing || input == null) return false;
            var player = FindPlayer(input.PlayerId);
            if (player == null || !player.IsAlive) return false;
            _inputs[player.Id] = input;
            return true;
        }

        public bool AreAllAlivePlayersReady()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (!Players[i].IsAlive) continue;
                RoundInput input;
                if (!_inputs.TryGetValue(Players[i].Id, out input) || !input.IsReady) return false;
            }
            return true;
        }

        public Vec2 ClampToArena(Vec2 point)
        {
            var x = Math.Max(Config.MinX, Math.Min(Config.MaxX, point.X));
            var y = Math.Max(Config.MinY, Math.Min(Config.MaxY, point.Y));
            return new Vec2(x, y);
        }

        public Vec2 ConstrainPlacement(string playerId, Vec2 candidate)
        {
            var result = ClampToArena(candidate);
            for (int i = 0; i < Players.Count; i++)
            {
                var other = Players[i];
                if (!other.IsAlive || other.Id == playerId) continue;
                var delta = result - other.Position;
                var distance = delta.Length;
                if (distance < Config.MinSpawnDistance)
                {
                    var away = distance > 0.00001f ? delta / distance : new Vec2(1f, 0f);
                    result = ClampToArena(other.Position + away * Config.MinSpawnDistance);
                }
            }
            return result;
        }

        public void LockRound()
        {
            if (Phase != Phase.Placing) return;
            for (int i = 0; i < Players.Count; i++)
            {
                var player = Players[i];
                if (!player.IsAlive) continue;
                RoundInput input;
                if (!_inputs.TryGetValue(player.Id, out input)) input = new RoundInput(player.Id);
                if (input.HasPlacement) player.Position = ConstrainPlacement(player.Id, input.Placement);
                if (input.HasAim)
                {
                    input.AimDirection = input.AimDirection.Normalized;
                    if (input.AimDirection.LengthSquared <= 0.00001f) input.AimDirection = Vec2.Up;
                    player.Facing = input.AimDirection;
                    player.LastAim = input.AimDirection;
                }
                else
                {
                    switch (Config.NoAimRule)
                    {
                        case NoAimPolicy.NoThrow:
                            input.HasAim = false;
                            break;
                        case NoAimPolicy.RepeatLastAim:
                            input.AimDirection = player.LastAim.HasValue ? player.LastAim.Value : player.Facing;
                            if (input.AimDirection.LengthSquared <= 0.00001f) input.AimDirection = Vec2.Up;
                            player.Facing = input.AimDirection;
                            input.HasAim = true;
                            break;
                        case NoAimPolicy.ThrowFacing:
                        default:
                            input.AimDirection = player.Facing;
                            if (input.AimDirection.LengthSquared <= 0.00001f) input.AimDirection = Vec2.Up;
                            player.Facing = input.AimDirection;
                            input.HasAim = true;
                            break;
                    }
                }
                _inputs[player.Id] = input;
            }
            Phase = Phase.Locked;
        }

        public IReadOnlyDictionary<string, RoundInput> LockedInputs { get { return _inputs; } }
        public void BeginReveal() { if (Phase == Phase.Locked) Phase = Phase.Reveal; }
        public void BeginThrowing() { if (Phase == Phase.Reveal) Phase = Phase.Throwing; }

        public RoundResolution ResolveRound()
        {
            if (Phase != Phase.Throwing) return LastResolution;
            Phase = Phase.Resolve;
            var aliveBefore = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].IsAlive) aliveBefore.Add(Players[i].Id);
            }

            LastResolution = RoundResolver.Resolve(Players, _inputs, Config);

            for (int i = 0; i < Players.Count; i++)
            {
                var p = Players[i];
                if (!p.IsAlive && aliveBefore.Contains(p.Id) && p.EliminatedAtRound <= 0)
                {
                    p.EliminatedAtRound = RoundNumber;
                }
            }

            EvaluateEnd();
            return LastResolution;
        }

        public RankedPlayer[] GetRankings()
        {
            return MatchRanking.Rank(Players, Config.RoundLimitTiebreak);
        }

        private void EvaluateEnd()
        {
            int aliveCount = 0;
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].IsAlive) aliveCount++;
            }

            if (aliveCount <= 1)
            {
                MatchRanking.FillWinners(Players, Config.RoundLimitTiebreak, _winnerIds);
                Phase = Phase.MatchEnd;
                return;
            }

            if (Config.RoundLimitTiebreak != RoundLimitTiebreakPolicy.ContinueUntilElimination && RoundNumber >= Config.MaxRounds)
            {
                MatchRanking.FillWinners(Players, Config.RoundLimitTiebreak, _winnerIds);
                Phase = Phase.MatchEnd;
                return;
            }

            Phase = Phase.RoundEnd;
        }

        public PlayerState FindPlayer(string id)
        {
            for (int i = 0; i < Players.Count; i++) if (Players[i].Id == id) return Players[i];
            return null;
        }
    }
}
