using System.Collections.Generic;
using HitMe.Core;
using NUnit.Framework;

namespace HitMe.Tests
{
    public sealed class V13CoreTests
    {
        [Test]
        public void MutualDeathStartsTiebreakForFinalistsOnly()
        {
            var match = Match(3, new GameConfig { StartHp = 1 });
            match.FindPlayer("c").Hp = 0;
            match.FindPlayer("c").EliminatedAtRound = 1;
            MutualHit(match);
            Assert.That(match.Phase, Is.EqualTo(Phase.RoundEnd));
            Assert.That(match.TiebreakPending, Is.True);
            match.StartRound();
            Assert.That(match.RoundLabel, Is.EqualTo("HIỆP PHỤ 1"));
            Assert.That(match.FindPlayer("a").Hp, Is.EqualTo(1));
            Assert.That(match.FindPlayer("b").Hp, Is.EqualTo(1));
            Assert.That(match.FindPlayer("c").Hp, Is.EqualTo(0));
        }

        [Test]
        public void ThirdMutualTiebreakEndsInCoWinners()
        {
            var match = Match(2, new GameConfig { StartHp = 1 });
            for (int i = 0; i < 4; i++)
            {
                MutualHit(match);
                if (i < 3) Assert.That(match.TiebreakPending, Is.True);
            }
            Assert.That(match.Phase, Is.EqualTo(Phase.MatchEnd));
            Assert.That(match.TiebreakCount, Is.EqualTo(3));
            Assert.That(match.WinnerIds, Is.EquivalentTo(new[] { "a", "b" }));
            foreach (var rank in match.GetRankings()) Assert.That(rank.Rank, Is.EqualTo(1));
        }

        [Test]
        public void SuddenDeathAtTenAndHardCapAtTwentyFive()
        {
            var match = Match(2, new GameConfig { StartHp = 3, NoAimRule = NoAimPolicy.NoThrow });
            for (int round = 1; round <= 25; round++)
            {
                NoHitRound(match);
                if (round == 9) Assert.That(match.FindPlayer("a").Hp, Is.EqualTo(3));
                if (round == 10) Assert.That(match.FindPlayer("a").Hp, Is.EqualTo(1));
            }
            Assert.That(match.NormalRoundNumber, Is.EqualTo(25));
            Assert.That(match.TiebreakPending, Is.True);
        }

        [Test]
        public void SoleSurvivorHasThrowAndHitStatistics()
        {
            var match = Match(2, new GameConfig { StartHp = 1 });
            match.StartRound();
            var a = match.FindPlayer("a");
            var b = match.FindPlayer("b");
            match.SetInput(new RoundInput("a") { HasAim = true, AimDirection = b.Position - a.Position });
            match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
            Assert.That(match.Phase, Is.EqualTo(Phase.MatchEnd));
            Assert.That(match.WinnerIds, Is.EquivalentTo(new[] { "a" }));
            var winner = match.GetRankings()[0];
            Assert.That(winner.Throws, Is.EqualTo(1));
            Assert.That(winner.HitsLanded, Is.EqualTo(1));
            Assert.That(winner.Accuracy, Is.EqualTo(1f));
        }

        private static MatchState Match(int count, GameConfig config)
        {
            var players = new List<PlayerState>();
            for (int i = 0; i < count; i++)
                players.Add(new PlayerState(((char)('a' + i)).ToString(), "P" + i, false,
                    config.StartHp, new Vec2(i * 2 - 2, i == 2 ? 2 : 0), Vec2.Up));
            return new MatchState(config, players);
        }

        private static void NoHitRound(MatchState match)
        {
            match.StartRound(); match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
        }

        private static void MutualHit(MatchState match)
        {
            match.StartRound();
            var a = match.FindPlayer("a"); var b = match.FindPlayer("b");
            match.SetInput(new RoundInput("a") { HasAim = true, AimDirection = b.Position - a.Position });
            match.SetInput(new RoundInput("b") { HasAim = true, AimDirection = a.Position - b.Position });
            match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
        }
    }
}
