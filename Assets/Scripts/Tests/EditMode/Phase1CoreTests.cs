using System.Collections.Generic;
using HitMe.Core;
using NUnit.Framework;

namespace HitMe.Tests
{
    public class Phase1CoreTests
    {
        private static GameConfig Config()
        {
            return new GameConfig { MaxPlayers = 6, StartHp = 3, ArenaWidth = 10f, ArenaHeight = 16f, PlayerRadius = 0.45f, MinSpawnDistance = 0.9f, DamagePerHit = 1, MaxHitsPerThrow = 1, Seed = 42 };
        }

        private static PlayerState Player(string id, Vec2 position, int hp = 3)
        {
            return new PlayerState(id, id, false, hp, position, Vec2.Up);
        }

        private static RoundInput Throw(string id, Vec2 direction)
        {
            return new RoundInput(id) { HasPlacement = false, Placement = Vec2.Zero, HasAim = true, AimDirection = direction, IsReady = true };
        }

        [Test]
        public void StraightThrowHitsOnce_AndFarMissDoesNotHit()
        {
            var config = Config();
            var shooter = Player("a", new Vec2(0f, 0f));
            var target = Player("b", new Vec2(0f, 3f));
            var far = Player("c", new Vec2(3f, 3f));
            var players = new List<PlayerState> { shooter, target, far };
            var result = RoundResolver.Resolve(players, new Dictionary<string, RoundInput> { { "a", Throw("a", Vec2.Up) } }, config);
            Assert.That(result.Hits.Count, Is.EqualTo(1));
            Assert.That(target.Hp, Is.EqualTo(2));
            Assert.That(far.Hp, Is.EqualTo(3));
        }

        [TestCase(1, 2, 3)]
        [TestCase(2, 2, 2)]
        public void HitsAreDistanceSortedAndRespectMaxHits(int maxHits, int firstHp, int secondHp)
        {
            var config = Config(); config.MaxHitsPerThrow = maxHits;
            var first = Player("first", new Vec2(0f, 2f));
            var second = Player("second", new Vec2(0f, 4f));
            var players = new List<PlayerState> { Player("shooter", Vec2.Zero), second, first };
            var result = RoundResolver.Resolve(players, new Dictionary<string, RoundInput> { { "shooter", Throw("shooter", Vec2.Up) } }, config);
            Assert.That(result.Hits.Count, Is.EqualTo(maxHits));
            Assert.That(first.Hp, Is.EqualTo(firstHp));
            Assert.That(second.Hp, Is.EqualTo(secondHp));
        }

        [Test]
        public void MutualThrowsResolveSimultaneously()
        {
            var config = Config();
            var a = Player("a", new Vec2(0f, -2f));
            var b = Player("b", new Vec2(0f, 2f));
            var players = new List<PlayerState> { a, b };
            RoundResolver.Resolve(players, new Dictionary<string, RoundInput> { { "a", Throw("a", Vec2.Up) }, { "b", Throw("b", new Vec2(0f, -1f)) } }, config);
            Assert.That(a.Hp, Is.EqualTo(2));
            Assert.That(b.Hp, Is.EqualTo(2));
        }

        [Test]
        public void MultipleHitsAccumulateAndHpDoesNotGoBelowZero()
        {
            var config = Config();
            var target = Player("target", Vec2.Zero, 1);
            var left = Player("left", new Vec2(-3f, 0f));
            var right = Player("right", new Vec2(3f, 0f));
            var players = new List<PlayerState> { target, left, right };
            var result = RoundResolver.Resolve(players, new Dictionary<string, RoundInput> { { "left", Throw("left", new Vec2(1f, 0f)) }, { "right", Throw("right", new Vec2(-1f, 0f)) } }, config);
            Assert.That(result.Damage["target"], Is.EqualTo(2));
            Assert.That(target.Hp, Is.EqualTo(0));
        }

        [Test]
        public void EliminatedPlayerNeitherThrowsNorCanBeHit()
        {
            var config = Config();
            var eliminated = Player("dead", new Vec2(0f, 2f), 0);
            var living = Player("live", Vec2.Zero);
            var players = new List<PlayerState> { eliminated, living };
            var result = RoundResolver.Resolve(players, new Dictionary<string, RoundInput> { { "dead", Throw("dead", new Vec2(0f, -1f)) }, { "live", Throw("live", Vec2.Up) } }, config);
            Assert.That(result.Hits.Count, Is.EqualTo(0));
            Assert.That(living.Hp, Is.EqualTo(3));
        }

        [Test]
        public void PlacementIsClampedAndSeparated()
        {
            var config = Config();
            var match = new MatchState(config, new[] { Player("a", Vec2.Zero), Player("b", new Vec2(3f, 3f)) });
            var outside = match.ClampToArena(new Vec2(999f, -999f));
            Assert.That(outside.X, Is.EqualTo(config.MaxX));
            Assert.That(outside.Y, Is.EqualTo(config.MinY));
            var safe = match.ConstrainPlacement("b", Vec2.Zero);
            Assert.That(Vec2.Distance(safe, Vec2.Zero), Is.GreaterThanOrEqualTo(config.MinSpawnDistance - 0.0001f));
        }

        [Test]
        public void LastSurvivorEndsMatch()
        {
            var config = Config();
            var a = Player("a", new Vec2(0f, -2f));
            var b = Player("b", new Vec2(0f, 2f), 1);
            var match = new MatchState(config, new[] { a, b });
            match.StartRound(); match.SetInput(Throw("a", Vec2.Up)); match.SetInput(Throw("b", new Vec2(1f, 0f)));
            match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
            Assert.That(match.Phase, Is.EqualTo(Phase.MatchEnd));
            Assert.That(match.WinnerIds, Is.EquivalentTo(new[] { "a" }));
        }

        [Test]
        public void FinalTwoWhoEliminateEachOtherBothWin()
        {
            var config = Config();
            var a = Player("a", new Vec2(0f, -2f), 1);
            var b = Player("b", new Vec2(0f, 2f), 1);
            var match = new MatchState(config, new[] { a, b });
            match.StartRound(); match.SetInput(Throw("a", Vec2.Up)); match.SetInput(Throw("b", new Vec2(0f, -1f)));
            match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
            Assert.That(match.Phase, Is.EqualTo(Phase.MatchEnd));
            Assert.That(match.WinnerIds, Is.EquivalentTo(new[] { "a", "b" }));
        }

        [Test]
        public void MaxRoundsDoesNotOverrideLastSurvivorRule()
        {
            var config = Config(); config.MaxRounds = 1;
            var match = new MatchState(config, new[] { Player("a", new Vec2(-2f, 0f)), Player("b", new Vec2(2f, 0f)) });
            match.StartRound(); match.SetInput(Throw("a", Vec2.Up)); match.SetInput(Throw("b", Vec2.Up));
            match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
            Assert.That(match.Phase, Is.EqualTo(Phase.RoundEnd));
        }

        [Test]
        public void SameSeedAndInputsAreDeterministicAndThousandBotMatchesFinish()
        {
            var config = Config();
            int first = MatchSimulator.RunBotMatch(config, config.Seed);
            int second = MatchSimulator.RunBotMatch(config, config.Seed);
            Assert.That(second, Is.EqualTo(first));
            int totalRounds = 0;
            for (uint i = 1; i <= 1000; i++)
            {
                int rounds = MatchSimulator.RunBotMatch(config, i);
                Assert.That(rounds, Is.LessThan(1000));
                totalRounds += rounds;
            }
            Assert.That(totalRounds, Is.GreaterThan(0));
        }
    }
}
