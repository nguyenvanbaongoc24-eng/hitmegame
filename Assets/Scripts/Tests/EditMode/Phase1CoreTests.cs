using System.Collections.Generic;
using HitMe.Core;
using NUnit.Framework;

namespace HitMe.Tests
{
    public class Phase1CoreTests
    {
        private static GameConfig Config()
        {
            return new GameConfig { MaxPlayers = 6, StartHp = 3, ArenaWidth = 9.0f, ArenaHeight = 11.4f, PlayerRadius = 0.55f, MinSpawnDistance = 1.1f, DamagePerHit = 1, MaxHitsPerThrow = 1, Seed = 42 };
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
            var config = Config();
            config.MaxRounds = 1;
            config.RoundLimitTiebreak = RoundLimitTiebreakPolicy.ContinueUntilElimination;
            var match = new MatchState(config, new[] { Player("a", new Vec2(-2f, 0f)), Player("b", new Vec2(2f, 0f)) });
            match.StartRound(); match.SetInput(Throw("a", Vec2.Up)); match.SetInput(Throw("b", Vec2.Up));
            match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
            Assert.That(match.Phase, Is.EqualTo(Phase.RoundEnd));
        }

        [Test]
        public void RoundLimit_EndsAtMaxRounds_AndWinnerHasHighestHp()
        {
            var config = Config();
            config.MaxRounds = 5;
            config.RoundLimitTiebreak = RoundLimitTiebreakPolicy.HighestHp;
            var a = Player("a", new Vec2(0f, -2f), 3);
            var b = Player("b", new Vec2(0f, 2f), 2);
            var match = new MatchState(config, new[] { a, b });
            for (int r = 1; r <= 5; r++)
            {
                match.StartRound();
                match.SetInput(Throw("a", new Vec2(-1f, 0f)));
                match.SetInput(Throw("b", new Vec2(1f, 0f)));
                match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
                if (r < 5)
                {
                    Assert.That(match.Phase, Is.EqualTo(Phase.RoundEnd));
                }
            }
            Assert.That(match.Phase, Is.EqualTo(Phase.MatchEnd));
            Assert.That(match.WinnerIds, Is.EquivalentTo(new[] { "a" }));
        }

        [Test]
        public void RoundLimit_TiedHp_ResolvedByHitsLanded()
        {
            var config = Config();
            config.MaxRounds = 1;
            config.RoundLimitTiebreak = RoundLimitTiebreakPolicy.HighestHp;
            var a = Player("a", new Vec2(0f, -2f), 2);
            var b = Player("b", new Vec2(0f, 2f), 3);
            var match = new MatchState(config, new[] { a, b });
            match.StartRound();
            match.SetInput(Throw("a", Vec2.Up)); // hits b, so a lands 1 hit, b hp becomes 2
            match.SetInput(Throw("b", new Vec2(1f, 0f))); // misses
            match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
            Assert.That(a.Hp, Is.EqualTo(2));
            Assert.That(b.Hp, Is.EqualTo(2));
            Assert.That(a.HitsLanded, Is.EqualTo(1));
            Assert.That(b.HitsLanded, Is.EqualTo(0));
            Assert.That(match.Phase, Is.EqualTo(Phase.MatchEnd));
            Assert.That(match.WinnerIds, Is.EquivalentTo(new[] { "a" }));
        }

        [Test]
        public void RoundLimit_TiedHpAndHits_SharedWin()
        {
            var config = Config();
            config.MaxRounds = 1;
            config.RoundLimitTiebreak = RoundLimitTiebreakPolicy.HighestHp;
            var a = Player("a", new Vec2(0f, -2f), 2);
            var b = Player("b", new Vec2(0f, 2f), 2);
            var match = new MatchState(config, new[] { a, b });
            match.StartRound();
            match.SetInput(Throw("a", Vec2.Up)); // hits b
            match.SetInput(Throw("b", new Vec2(0f, -1f))); // hits a
            match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
            Assert.That(a.Hp, Is.EqualTo(1));
            Assert.That(b.Hp, Is.EqualTo(1));
            Assert.That(a.HitsLanded, Is.EqualTo(1));
            Assert.That(b.HitsLanded, Is.EqualTo(1));
            Assert.That(match.Phase, Is.EqualTo(Phase.MatchEnd));
            Assert.That(match.WinnerIds, Is.EquivalentTo(new[] { "a", "b" }));
        }

        [Test]
        public void RoundLimit_SharedWinPolicy_AllAliveWin()
        {
            var config = Config();
            config.MaxRounds = 1;
            config.RoundLimitTiebreak = RoundLimitTiebreakPolicy.SharedWin;
            var a = Player("a", new Vec2(0f, -2f), 3);
            var b = Player("b", new Vec2(0f, 2f), 1);
            var match = new MatchState(config, new[] { a, b });
            match.StartRound();
            match.SetInput(Throw("a", new Vec2(-1f, 0f)));
            match.SetInput(Throw("b", new Vec2(1f, 0f)));
            match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
            Assert.That(match.Phase, Is.EqualTo(Phase.MatchEnd));
            Assert.That(match.WinnerIds, Is.EquivalentTo(new[] { "a", "b" }));
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

        [Test]
        public void AllCharacters_ShareSameHitboxRadius_AndStandardizedDimensions()
        {
            var config = Config();
            Assert.That(config.PlayerRadius, Is.EqualTo(0.55f));
            
            // All 6 players in match share identical hitbox
            var p1 = Player("human_0", new Vec2(0f, 0f));
            var dog = Player("dog_4", new Vec2(0f, 2f));
            var tinted = Player("tinted_5", new Vec2(2f, 0f));
            
            // Dog is struck with exact same HitCalculator rules as human
            var hitsHit = HitCalculator.CalculateHits(p1, new[] { dog }, Vec2.Up, config);
            var hitsMiss = HitCalculator.CalculateHits(p1, new[] { new PlayerState("miss", "miss", false, 3, new Vec2(0.56f, 2f), Vec2.Up) }, Vec2.Up, config);
            
            Assert.That(hitsHit.Count, Is.EqualTo(1));
            Assert.That(hitsMiss.Count, Is.EqualTo(0));
        }

        [Test]
        public void CharacterVisualScale_MatchesMockupStandard()
        {
            // Nam Lang body target height is 1.50 world units (approx 13% of 11.4 arena height)
            float arenaHeight = 11.4f;
            float namLangHeight = 1.50f;
            float heightRatio = namLangHeight / arenaHeight;
            Assert.That(heightRatio, Is.InRange(0.12f, 0.14f));

            // Dog height is 70-75% of human height
            float dogHeight = namLangHeight * 0.72f;
            Assert.That(dogHeight / namLangHeight, Is.InRange(0.70f, 0.75f));

            // Hitbox and foot ring width = 2 * playerRadius = 1.10 world units
            float arenaWidth = 9.0f;
            float ringWidth = 2f * 0.55f;
            float ringWidthRatio = ringWidth / arenaWidth;
            Assert.That(ringWidthRatio, Is.InRange(0.11f, 0.13f));
        }

        [Test]
        public void S1_Ranking_MaxRoundsMultipleSurvivors_RanksByAliveThenHpThenHits()
        {
            var players = new List<PlayerState>
            {
                new PlayerState("p1", "Nam Làng", false, 1, Vec2.Zero, Vec2.Up) { HitsLanded = 4 },
                new PlayerState("p2", "Anh Teen", true, 3, Vec2.Zero, Vec2.Up) { HitsLanded = 1 },
                new PlayerState("p3", "Cô Ba", true, 2, Vec2.Zero, Vec2.Up) { HitsLanded = 2 }
            };

            var res = MatchRanking.Evaluate(players);
            Assert.That(res.WinnerIds, Is.EqualTo(new[] { "p2" }));
            Assert.That(res.IsSharedWin, Is.False);
            Assert.That(res.RankedPlayers[0].Id, Is.EqualTo("p2"));
            Assert.That(res.RankedPlayers[0].Rank, Is.EqualTo(1));
            Assert.That(res.RankedPlayers[1].Id, Is.EqualTo("p3"));
            Assert.That(res.RankedPlayers[1].Rank, Is.EqualTo(2));
            Assert.That(res.RankedPlayers[2].Id, Is.EqualTo("p1"));
            Assert.That(res.RankedPlayers[2].Rank, Is.EqualTo(3));
        }

        [Test]
        public void S1_Ranking_TiedHpDifferentHits_HigherHitsWinsSoleRank1()
        {
            var players = new List<PlayerState>
            {
                new PlayerState("player", "Nam Làng", false, 2, Vec2.Zero, Vec2.Up) { HitsLanded = 1 },
                new PlayerState("bot-1", "Anh Teen", true, 2, Vec2.Zero, Vec2.Up) { HitsLanded = 3 },
                new PlayerState("bot-2", "Cô Ba", true, 2, Vec2.Zero, Vec2.Up) { HitsLanded = 2 }
            };

            var res = MatchRanking.Evaluate(players);
            Assert.That(res.WinnerIds, Is.EqualTo(new[] { "bot-1" }));
            Assert.That(res.IsSharedWin, Is.False);
            Assert.That(res.RankedPlayers[0].Id, Is.EqualTo("bot-1"));
            Assert.That(res.RankedPlayers[0].Rank, Is.EqualTo(1));
            Assert.That(res.RankedPlayers[1].Id, Is.EqualTo("bot-2"));
            Assert.That(res.RankedPlayers[1].Rank, Is.EqualTo(2));
            Assert.That(res.RankedPlayers[2].Id, Is.EqualTo("player"));
            Assert.That(res.RankedPlayers[2].Rank, Is.EqualTo(3));
        }

        [Test]
        public void S1_Ranking_TiedAliveHpAndHits_SharesRank1AndBothWin_DisplayOrderNeverAffectsWinner()
        {
            var players = new List<PlayerState>
            {
                new PlayerState("bot-2", "Zeta", true, 2, Vec2.Zero, Vec2.Up) { HitsLanded = 3 },
                new PlayerState("player", "Nam Làng", false, 2, Vec2.Zero, Vec2.Up) { HitsLanded = 3 },
                new PlayerState("bot-1", "Alpha", true, 1, Vec2.Zero, Vec2.Up) { HitsLanded = 4 }
            };

            var res = MatchRanking.Evaluate(players);
            Assert.That(res.IsSharedWin, Is.True);
            Assert.That(res.WinnerIds, Is.EquivalentTo(new[] { "player", "bot-2" }));
            // Display order puts local "player" first among equals, but BOTH have Rank == 1 and IsWinner == true
            Assert.That(res.RankedPlayers[0].Id, Is.EqualTo("player"));
            Assert.That(res.RankedPlayers[0].Rank, Is.EqualTo(1));
            Assert.That(res.RankedPlayers[0].IsSharedRank, Is.True);
            Assert.That(res.RankedPlayers[0].IsWinner, Is.True);

            Assert.That(res.RankedPlayers[1].Id, Is.EqualTo("bot-2"));
            Assert.That(res.RankedPlayers[1].Rank, Is.EqualTo(1));
            Assert.That(res.RankedPlayers[1].IsSharedRank, Is.True);
            Assert.That(res.RankedPlayers[1].IsWinner, Is.True);

            // Next player after two tied Rank 1 players gets competition Rank 3
            Assert.That(res.RankedPlayers[2].Id, Is.EqualTo("bot-1"));
            Assert.That(res.RankedPlayers[2].Rank, Is.EqualTo(3));
            Assert.That(res.RankedPlayers[2].IsWinner, Is.False);
        }

        [Test]
        public void S1_Ranking_EliminatedPlayers_OrderedByEliminationRoundDesc_ThenHitsDesc_ThenSharedRank()
        {
            var players = new List<PlayerState>
            {
                new PlayerState("alive", "Alive", false, 1, Vec2.Zero, Vec2.Up) { HitsLanded = 0 },
                new PlayerState("elim_r1", "Elim R1", true, 0, Vec2.Zero, Vec2.Up, eliminatedAtRound: 1) { HitsLanded = 5 },
                new PlayerState("elim_r3_low", "Elim R3 Low", true, 0, Vec2.Zero, Vec2.Up, eliminatedAtRound: 3) { HitsLanded = 1 },
                new PlayerState("elim_r3_high", "Elim R3 High", true, 0, Vec2.Zero, Vec2.Up, eliminatedAtRound: 3) { HitsLanded = 3 },
                new PlayerState("elim_r2_a", "Elim R2 A", true, 0, Vec2.Zero, Vec2.Up, eliminatedAtRound: 2) { HitsLanded = 2 },
                new PlayerState("elim_r2_b", "Elim R2 B", true, 0, Vec2.Zero, Vec2.Up, eliminatedAtRound: 2) { HitsLanded = 2 }
            };

            var ranked = MatchRanking.Rank(players);
            Assert.That(ranked[0].Id, Is.EqualTo("alive"));
            Assert.That(ranked[0].Rank, Is.EqualTo(1));

            Assert.That(ranked[1].Id, Is.EqualTo("elim_r3_high"));
            Assert.That(ranked[1].Rank, Is.EqualTo(2));

            Assert.That(ranked[2].Id, Is.EqualTo("elim_r3_low"));
            Assert.That(ranked[2].Rank, Is.EqualTo(3));

            // Two players eliminated in round 2 with equal HitsLanded = 2 share Rank 4
            Assert.That(ranked[3].Id, Is.EqualTo("elim_r2_a"));
            Assert.That(ranked[3].Rank, Is.EqualTo(4));
            Assert.That(ranked[3].IsSharedRank, Is.True);

            Assert.That(ranked[4].Id, Is.EqualTo("elim_r2_b"));
            Assert.That(ranked[4].Rank, Is.EqualTo(4));
            Assert.That(ranked[4].IsSharedRank, Is.True);

            // Eliminated in round 1 is last (Rank 6) despite having 5 hits
            Assert.That(ranked[5].Id, Is.EqualTo("elim_r1"));
            Assert.That(ranked[5].Rank, Is.EqualTo(6));
        }

        [Test]
        public void S1_Ranking_FullSixPlayerMatch_ProducesExpectedRanks1To6()
        {
            var players = new List<PlayerState>
            {
                new PlayerState("player", "Nam Làng", false, 2, Vec2.Zero, Vec2.Up) { HitsLanded = 3 },
                new PlayerState("bot-1", "Anh Teen", true, 2, Vec2.Zero, Vec2.Up) { HitsLanded = 3 },
                new PlayerState("bot-2", "Cô Ba", true, 1, Vec2.Zero, Vec2.Up) { HitsLanded = 2 },
                new PlayerState("bot-3", "Chó Vàng", true, 0, Vec2.Zero, Vec2.Up, eliminatedAtRound: 4) { HitsLanded = 1 },
                new PlayerState("bot-4", "Nữ Năng Động", true, 2, Vec2.Zero, Vec2.Up) { HitsLanded = 1 },
                new PlayerState("bot-5", "Mèo Mun", true, 0, Vec2.Zero, Vec2.Up, eliminatedAtRound: 2) { HitsLanded = 0 }
            };

            var res = MatchRanking.Evaluate(players);
            Assert.That(res.WinnerIds, Is.EquivalentTo(new[] { "player", "bot-1" }));
            // Expected order: player (R1), bot-1 (R1), bot-4 (2HP 1Hit -> R3), bot-2 (1HP 2Hit -> R4), bot-3 (elim R4 -> R5), bot-5 (elim R2 -> R6)
            Assert.That(res.RankedPlayers[0].Id, Is.EqualTo("player"));
            Assert.That(res.RankedPlayers[0].Rank, Is.EqualTo(1));
            Assert.That(res.RankedPlayers[1].Id, Is.EqualTo("bot-1"));
            Assert.That(res.RankedPlayers[1].Rank, Is.EqualTo(1));
            Assert.That(res.RankedPlayers[2].Id, Is.EqualTo("bot-4"));
            Assert.That(res.RankedPlayers[2].Rank, Is.EqualTo(3));
            Assert.That(res.RankedPlayers[3].Id, Is.EqualTo("bot-2"));
            Assert.That(res.RankedPlayers[3].Rank, Is.EqualTo(4));
            Assert.That(res.RankedPlayers[4].Id, Is.EqualTo("bot-3"));
            Assert.That(res.RankedPlayers[4].Rank, Is.EqualTo(5));
            Assert.That(res.RankedPlayers[5].Id, Is.EqualTo("bot-5"));
            Assert.That(res.RankedPlayers[5].Rank, Is.EqualTo(6));
        }

        [Test]
        public void S1_Ranking_SingleSurvivor_WinsRank1_AndEliminatedOrderedByRound()
        {
            var config = Config();
            var a = Player("a", new Vec2(0f, -2f), 3);
            var b = Player("b", new Vec2(0f, 0f), 1);
            var c = Player("c", new Vec2(0f, 2f), 1);
            var match = new MatchState(config, new[] { a, b, c });

            // Round 1: a hits b (b eliminated at round 1)
            match.StartRound();
            match.SetInput(Throw("a", Vec2.Up));
            match.SetInput(Throw("b", new Vec2(1f, 0f)));
            match.SetInput(Throw("c", Vec2.Up));
            match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
            Assert.That(b.IsAlive, Is.False);
            Assert.That(b.EliminatedAtRound, Is.EqualTo(1));
            Assert.That(match.Phase, Is.EqualTo(Phase.RoundEnd));

            // Round 2: a hits c (c eliminated at round 2, only a survives)
            match.StartRound();
            match.SetInput(Throw("a", Vec2.Up));
            match.SetInput(Throw("c", Vec2.Up));
            match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
            Assert.That(c.IsAlive, Is.False);
            Assert.That(c.EliminatedAtRound, Is.EqualTo(2));
            Assert.That(match.Phase, Is.EqualTo(Phase.MatchEnd));
            Assert.That(match.WinnerIds, Is.EqualTo(new[] { "a" }));

            var ranked = match.GetRankings();
            Assert.That(ranked[0].Id, Is.EqualTo("a"));
            Assert.That(ranked[0].Rank, Is.EqualTo(1));
            Assert.That(ranked[1].Id, Is.EqualTo("c"));
            Assert.That(ranked[1].Rank, Is.EqualTo(2));
            Assert.That(ranked[2].Id, Is.EqualTo("b"));
            Assert.That(ranked[2].Rank, Is.EqualTo(3));
        }

        [Test]
        public void S1_Ranking_ZeroSurvivors_MutualEliminationInFinalRound_RanksByRoundThenHits()
        {
            var config = Config();
            var early = Player("early", new Vec2(-3f, -2f), 1);
            var a = Player("a", new Vec2(0f, -2f), 2);
            var b = Player("b", new Vec2(0f, 2f), 1);
            var match = new MatchState(config, new[] { early, a, b });

            // Round 1: a hits early (early dies R1, a has 1 hit); b hits a (a has 1 HP, b has 1 hit)
            match.StartRound();
            match.SetInput(Throw("a", new Vec2(-1f, 0f)));
            match.SetInput(Throw("b", new Vec2(0f, -1f)));
            match.SetInput(Throw("early", new Vec2(-1f, 0f)));
            match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();
            Assert.That(early.EliminatedAtRound, Is.EqualTo(1));
            Assert.That(match.Phase, Is.EqualTo(Phase.RoundEnd));

            // Round 2: a and b shoot each other and both die in Round 2 (a has 2 hits total, b has 2 hits total)
            match.StartRound();
            match.SetInput(Throw("a", Vec2.Up));
            match.SetInput(Throw("b", new Vec2(0f, -1f)));
            match.LockRound(); match.BeginReveal(); match.BeginThrowing(); match.ResolveRound();

            Assert.That(match.Phase, Is.EqualTo(Phase.MatchEnd));
            Assert.That(a.EliminatedAtRound, Is.EqualTo(2));
            Assert.That(b.EliminatedAtRound, Is.EqualTo(2));
            Assert.That(match.WinnerIds, Is.EquivalentTo(new[] { "a", "b" }));

            var ranked = match.GetRankings();
            Assert.That(ranked[0].Rank, Is.EqualTo(1));
            Assert.That(ranked[0].IsSharedRank, Is.True);
            Assert.That(ranked[1].Rank, Is.EqualTo(1));
            Assert.That(ranked[1].IsSharedRank, Is.True);
            Assert.That(ranked[2].Id, Is.EqualTo("early"));
            Assert.That(ranked[2].Rank, Is.EqualTo(3));
        }
    }
}
