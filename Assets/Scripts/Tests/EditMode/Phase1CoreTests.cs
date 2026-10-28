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

        [TestCase(RoundLimitTiebreakPolicy.ContinueUntilElimination)]
        [TestCase(RoundLimitTiebreakPolicy.HighestHp)]
        [TestCase(RoundLimitTiebreakPolicy.SharedWin)]
        public void RoundLimitTiebreakIsPreparedButInactiveUntilApproved(RoundLimitTiebreakPolicy policy)
        {
            var config = Config();
            config.MaxRounds = 1;
            config.RoundLimitTiebreak = policy;
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

        [Test]
        public void GestureCanPlaceThenPlaceAgainWithoutCreatingAnAim()
        {
            var gesture = new PlacementGestureState(Vec2.Zero, Vec2.Up);
            gesture.PointerDown(new Vec2(2f, 1f), .45f);
            Assert.That(gesture.Mode, Is.EqualTo(PlacementGestureMode.Moving));
            Assert.That(gesture.Placement, Is.EqualTo(new Vec2(2f, 1f)));
            gesture.PointerMove(new Vec2(-3f, 2f), .25f);
            gesture.PointerUp();
            gesture.PointerDown(new Vec2(1f, -4f), .45f);
            Assert.That(gesture.Placement, Is.EqualTo(new Vec2(1f, -4f)));
            Assert.That(gesture.HasAim, Is.False);
        }

        [Test]
        public void GestureHoldOnPlayerAndDragSetsDirectionWithoutMovingPlacement()
        {
            var gesture = new PlacementGestureState(new Vec2(2f, 3f), Vec2.Up);
            gesture.PointerDown(new Vec2(2.1f, 3f), .45f);
            gesture.PointerMove(new Vec2(5f, 3f), .25f);
            Assert.That(gesture.Mode, Is.EqualTo(PlacementGestureMode.Aiming));
            Assert.That(gesture.Placement, Is.EqualTo(new Vec2(2f, 3f)));
            Assert.That(gesture.HasAim, Is.True);
            Assert.That(gesture.AimDirection, Is.EqualTo(new Vec2(1f, 0f)));
        }

        [Test]
        public void GestureDragBelowMinAimDragDoesNotCommitAnAim()
        {
            var gesture = new PlacementGestureState(Vec2.Zero, Vec2.Up);
            gesture.PointerDown(Vec2.Zero, .45f);
            gesture.PointerMove(new Vec2(.24f, 0f), .25f);
            Assert.That(gesture.HasAim, Is.False);
            Assert.That(gesture.BuildRoundInput("player", false).HasAim, Is.False);
        }

        [Test]
        public void GestureUsesTheConfiguredAimThreshold()
        {
            var config = Config();
            config.MinAimDrag = .55f;
            var gesture = new PlacementGestureState(Vec2.Zero, Vec2.Up);
            gesture.PointerDown(Vec2.Zero, config.AimTouchRadius);
            gesture.PointerMove(new Vec2(.54f, 0f), config.MinAimDrag);
            Assert.That(gesture.HasAim, Is.False);
            gesture.PointerMove(new Vec2(.55f, 0f), config.MinAimDrag);
            Assert.That(gesture.HasAim, Is.True);
        }

        [Test]
        public void ActivePointerGateIgnoresSecondFingerUntilTheFirstEnds()
        {
            var gate = new ActivePointerGate();
            Assert.That(gate.TryAccept(10, PointerPhase.Began), Is.True);
            Assert.That(gate.TryAccept(11, PointerPhase.Began), Is.False);
            Assert.That(gate.TryAccept(11, PointerPhase.Moved), Is.False);
            Assert.That(gate.TryAccept(10, PointerPhase.Moved), Is.True);
            Assert.That(gate.TryAccept(10, PointerPhase.Ended), Is.True);
            Assert.That(gate.TryAccept(11, PointerPhase.Began), Is.True);
        }

        [Test]
        public void GestureDraggingBackNearPlayerCancelsAim()
        {
            var gesture = new PlacementGestureState(Vec2.Zero, Vec2.Up);
            gesture.PointerDown(Vec2.Zero, .45f);
            gesture.PointerMove(new Vec2(0f, 2f), .25f);
            Assert.That(gesture.HasAim, Is.True);
            gesture.PointerMove(new Vec2(.1f, 0f), .25f);
            Assert.That(gesture.HasAim, Is.False);
            Assert.That(gesture.EffectiveDirection, Is.EqualTo(Vec2.Up));
        }

        [Test]
        public void TimeoutLockUsesTheLastCommittedAim()
        {
            var config = Config();
            var player = Player("player", Vec2.Zero);
            var match = new MatchState(config, new[] { player, Player("other", new Vec2(3f, 0f)) });
            var gesture = new PlacementGestureState(Vec2.Zero, player.Facing);
            gesture.PointerDown(Vec2.Zero, config.PlayerRadius);
            gesture.PointerMove(new Vec2(2f, 0f), config.MinAimDrag);
            match.StartRound();
            match.SetInput(gesture.BuildRoundInput("player", true));
            match.LockRound();
            Assert.That(match.LockedInputs["player"].AimDirection, Is.EqualTo(new Vec2(1f, 0f)));
        }

        [Test]
        public void NoAimUsesCurrentFacingFallbackWhenTimerLocks()
        {
            var config = Config();
            config.NoAimRule = NoAimPolicy.ThrowFacing;
            var player = new PlayerState("player", "player", false, 3, Vec2.Zero, new Vec2(-1f, 0f));
            var match = new MatchState(config, new[] { player, Player("other", new Vec2(3f, 0f)) });
            var gesture = new PlacementGestureState(Vec2.Zero, player.Facing);
            match.StartRound();
            match.SetInput(gesture.BuildRoundInput("player", true));
            match.LockRound();
            Assert.That(gesture.HasAim, Is.False);
            Assert.That(match.LockedInputs["player"].AimDirection, Is.EqualTo(new Vec2(-1f, 0f)));
        }

        [Test]
        public void NoAimPolicy_NoThrow_DoesNotThrowWeapon()
        {
            var config = Config();
            config.NoAimRule = NoAimPolicy.NoThrow;
            var player = new PlayerState("player", "player", false, 3, Vec2.Zero, new Vec2(1f, 0f));
            var target = Player("target", new Vec2(3f, 0f));
            var match = new MatchState(config, new[] { player, target });
            match.StartRound();
            match.SetInput(new RoundInput("player") { HasPlacement = true, Placement = Vec2.Zero, HasAim = false });
            match.LockRound(); match.BeginReveal(); match.BeginThrowing();
            var resolution = match.ResolveRound();
            Assert.That(match.LockedInputs["player"].HasAim, Is.False);
            Assert.That(resolution.Hits.Count, Is.EqualTo(0));
            Assert.That(target.Hp, Is.EqualTo(3));
        }

        [Test]
        public void NoAimPolicy_RepeatLastAim_ReusesThePreviousExplicitAim()
        {
            var config = Config();
            config.NoAimRule = NoAimPolicy.RepeatLastAim;
            var player = new PlayerState("player", "player", false, 3, Vec2.Zero, Vec2.Up);
            var match = new MatchState(config, new[] { player, Player("target", new Vec2(5f, 0f)) });
            match.StartRound();
            match.SetInput(new RoundInput("player") { HasPlacement = true, Placement = Vec2.Zero, HasAim = true, AimDirection = new Vec2(1f, 0f) });
            match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
            match.StartRound();
            match.SetInput(new RoundInput("player") { HasPlacement = true, Placement = Vec2.Zero, HasAim = false });
            match.LockRound();
            Assert.That(match.LockedInputs["player"].HasAim, Is.True);
            Assert.That(match.LockedInputs["player"].AimDirection, Is.EqualTo(new Vec2(1f, 0f)));
        }

    }
}
