using System;
using HitMe.Core;

namespace HitMe.Game
{
    // Deterministic QA inputs. This drives the production MatchState and RoundResolver;
    // access is gated by WebBridge before a match can be configured.
    public static class DevScenario
    {
        public static readonly string[] Names =
        {
            "tiebreak", "tiebreak3", "tiebreak_draw", "suddendeath", "hardcap", "ranking_ties"
        };

        public static bool IsKnown(string name)
        {
            for (int i = 0; i < Names.Length; i++)
                if (string.Equals(Names[i], name, StringComparison.Ordinal)) return true;
            return false;
        }

        public static void Configure(string name, GameConfig config)
        {
            if (!IsKnown(name)) throw new ArgumentException("Unknown scenario", nameof(name));
            config.MaxPlayers = name == "tiebreak3" || name == "tiebreak_draw" || name == "hardcap" ? 3
                : name == "ranking_ties" ? 6 : 2;
            config.StartHp = name == "suddendeath" || name == "hardcap" ? 3 : 1;
            config.Seed = 130013u;
        }

        public static void Prime(string name, MatchState match)
        {
            int target = name == "suddendeath" ? 8 : name == "hardcap" ? 23 : 0;
            while (match.NormalRoundNumber < target)
            {
                match.StartRound();
                PrepareRound(name, match);
                foreach (var player in match.Players)
                    if (player.IsAlive) match.SetInput(new RoundInput(player.Id) { HasAim = true, AimDirection = Vec2.Up, IsReady = true });
                match.LockRound();
                match.BeginReveal();
                match.BeginThrowing();
                match.ResolveRound();
                if (match.Phase == Phase.MatchEnd) throw new InvalidOperationException("Scenario ended while priming");
            }
            // Historic hit totals in this test fixture distinguish the two hard-cap
            // leaders. The actual round-25 decision is still made by MatchState.
            if (name == "hardcap")
            {
                match.Players[0].HitsLanded = 1;
                match.Players[1].HitsLanded = 1;
            }
        }

        public static void PrepareRound(string name, MatchState match)
        {
            if (!IsKnown(name) || match.Phase != Phase.Placing) return;
            if (name == "ranking_ties")
            {
                // Each intended hit has its own horizontal lane. Other players
                // stand on the upper lane so no weapon can hit an earlier target.
                Set(match, 0, -2f, -3f);
                if (match.NormalRoundNumber == 1) Set(match, 5, -1f, -3f);
                if (match.NormalRoundNumber == 2) Set(match, 4, -1f, -3f);
                if (match.NormalRoundNumber == 3)
                {
                    Set(match, 2, -1f, -3f);
                    Set(match, 1, 2f, 2f);
                    Set(match, 3, 1f, 2f);
                }
                if (match.NormalRoundNumber == 4) Set(match, 1, -1f, -3f);
                if (match.NormalRoundNumber < 3)
                {
                    Set(match, 1, -3f, 2f); Set(match, 2, -1.5f, 2f);
                    Set(match, 3, 0f, 2f); if (match.NormalRoundNumber == 1) Set(match, 4, 1.5f, 2f);
                }
            }
            else if (name == "tiebreak3" || name == "tiebreak_draw")
            {
                Set(match, 0, -1f, -1f); Set(match, 1, 1f, -1f); Set(match, 2, 0f, 1f);
            }
            else if (name == "hardcap")
            {
                Set(match, 0, -2f, 0f); Set(match, 1, 0f, 0f); Set(match, 2, 2f, 0f);
            }
            else
            {
                Set(match, 0, -1f, 0f); Set(match, 1, 1f, 0f);
            }
        }

        public static RoundInput Input(string name, MatchState match, PlayerState player)
        {
            var input = new RoundInput(player.Id)
            {
                Placement = player.Position, HasPlacement = false,
                HasAim = true, AimDirection = Vec2.Up, IsReady = true
            };
            int i = match.Players.IndexOf(player);
            int round = match.NormalRoundNumber;
            if (name == "tiebreak")
            {
                if (i == 0) input.AimDirection = new Vec2(1f, 0f);
                else if (i == 1 && !match.IsTiebreak) input.AimDirection = new Vec2(-1f, 0f);
            }
            else if (name == "tiebreak3" || name == "tiebreak_draw")
            {
                if (!match.IsTiebreak || name == "tiebreak_draw")
                    input.AimDirection = Toward(match, i, (i + 1) % 3);
                else if (i == 0) input.AimDirection = Toward(match, 0, 1);
                else if (i == 1) input.AimDirection = Toward(match, 1, 2);
            }
            else if (name == "suddendeath")
            {
                if (round >= 11 && i == 0) input.AimDirection = new Vec2(1f, 0f);
            }
            else if (name == "hardcap")
            {
                if (match.IsTiebreak && i == 0) input.AimDirection = new Vec2(1f, 0f);
            }
            else if (name == "ranking_ties")
            {
                if (i == 0 && round <= 4) input.AimDirection = new Vec2(1f, 0f);
                if (i == 1 && round == 3) input.AimDirection = new Vec2(-1f, 0f);
                if (i == 2 && round == 3) input.AimDirection = new Vec2(0f, -1f);
                if (i == 5 && round == 1) input.AimDirection = new Vec2(0f, -1f);
                if (i == 4 && round == 2) input.AimDirection = new Vec2(0f, -1f);
                if (i == 1 && round == 4) input.AimDirection = new Vec2(0f, -1f);
            }
            return input;
        }

        private static void Set(MatchState match, int index, float x, float y)
        {
            if (index < match.Players.Count && match.Players[index].IsAlive)
                match.Players[index].Position = new Vec2(x, y);
        }

        private static Vec2 Toward(MatchState match, int from, int to)
        {
            return match.Players[to].Position - match.Players[from].Position;
        }
    }
}
