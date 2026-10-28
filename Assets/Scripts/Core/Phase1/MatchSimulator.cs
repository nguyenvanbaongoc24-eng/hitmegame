using System.Collections.Generic;

namespace HitMe.Core
{
    public static class MatchSimulator
    {
        public sealed class SimulationResult
        {
            public int Rounds;
            public readonly List<string> WinnerIds = new List<string>();
        }

        public static int RunBotMatch(GameConfig config, uint seed)
        {
            return RunBotMatchDetailed(config, seed).Rounds;
        }

        public static SimulationResult RunBotMatchDetailed(GameConfig config, uint seed)
        {
            var random = new SeededRandom(seed);
            var players = new List<PlayerState>();
            var brains = new List<Phase1BotBrain>();
            for (int i = 0; i < config.MaxPlayers; i++)
            {
                var position = new Vec2(random.Range(config.MinX, config.MaxX), random.Range(config.MinY, config.MaxY));
                players.Add(new PlayerState("bot-" + i, "Bot " + (i + 1), true, config.StartHp, position, Vec2.Up));
                brains.Add(new Phase1BotBrain((BotLevel)(i % 3), random));
            }
            var match = new MatchState(config, players);
            int safety = 0;
            while (match.Phase != Phase.MatchEnd && safety++ < 1000)
            {
                match.StartRound();
                for (int i = 0; i < players.Count; i++)
                {
                    if (!players[i].IsAlive) continue;
                    match.SetInput(brains[i].MakeInput(players[i], config));
                }
                match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
            }
            var result = new SimulationResult { Rounds = match.RoundNumber };
            for (int i = 0; i < match.WinnerIds.Count; i++) result.WinnerIds.Add(match.WinnerIds[i]);
            return result;
        }
    }
}
