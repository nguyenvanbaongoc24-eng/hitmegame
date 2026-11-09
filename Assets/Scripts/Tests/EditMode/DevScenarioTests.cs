using System.Collections.Generic;
using HitMe.Core;
using HitMe.Game;
using NUnit.Framework;

namespace HitMe.Tests
{
    public sealed class DevScenarioTests
    {
        [TestCase("tiebreak", 2, 1)]
        [TestCase("tiebreak3", 3, 1)]
        [TestCase("tiebreak_draw", 3, 3)]
        [TestCase("suddendeath", 2, 0)]
        [TestCase("hardcap", 3, 1)]
        [TestCase("ranking_ties", 6, 0)]
        public void ScriptedInputsResolveThroughProductionCore(string name, int playerCount, int tiebreaks)
        {
            var config = new GameConfig();
            DevScenario.Configure(name, config);
            Assert.AreEqual(playerCount, config.MaxPlayers);
            var players = new List<PlayerState>();
            for (int i = 0; i < playerCount; i++)
                players.Add(new PlayerState(i == 0 ? "player" : "bot-" + i,
                    "P" + i, i != 0, config.StartHp, new Vec2(i - 2, 0), Vec2.Up));
            var match = new MatchState(config, players);
            DevScenario.Prime(name, match);
            int safety = 0;
            bool sawSuddenDeath = false, sawHardCap = false, sawTiebreak = false;
            while (match.Phase != Phase.MatchEnd && safety++ < 30)
            {
                match.StartRound();
                DevScenario.PrepareRound(name, match);
                if (match.NormalRoundNumber == 10 && !match.IsTiebreak)
                {
                    sawSuddenDeath = true;
                    foreach (var p in players) if (p.IsAlive) Assert.AreEqual(1, p.Hp);
                }
                if (match.NormalRoundNumber == 25 && !match.IsTiebreak) sawHardCap = true;
                if (match.IsTiebreak)
                {
                    sawTiebreak = true;
                    foreach (var p in players) if (p.IsAlive) Assert.AreEqual(1, p.Hp);
                }
                foreach (var p in players)
                    if (p.IsAlive) Assert.IsTrue(match.SetInput(DevScenario.Input(name, match, p)));
                match.LockRound();
                match.BeginReveal();
                match.BeginThrowing();
                Assert.IsNotNull(match.ResolveRound());
            }
            Assert.Less(safety, 30, name + " did not end");
            Assert.AreEqual(tiebreaks, match.TiebreakCount, name);
            Assert.AreEqual(name == "tiebreak_draw" ? 3 : 1, match.WinnerIds.Count, name);
            Assert.IsTrue(name == "tiebreak_draw" || match.WinnerIds[0] == "player", name);
            if (name == "suddendeath") Assert.IsTrue(sawSuddenDeath);
            if (name == "hardcap") Assert.IsTrue(sawHardCap && sawTiebreak);
            if (name == "ranking_ties")
            {
                var ranks = match.GetRankings();
                int[] expected = { 1, 2, 3, 3, 5, 6 };
                for (int i = 0; i < expected.Length; i++) Assert.AreEqual(expected[i], ranks[i].Rank);
            }
        }
    }
}
