using HitMe.Core;
using HitMe.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace HitMe.Tests.Editor
{
    public class GameplayRecoveryTests
    {
        [Test]
        public void ArenaClamp_LeavesInteriorPointUnchanged_AndClampsExteriorPoint()
        {
            Vector2 inside = new Vector2(0.5f, -1.25f);
            Assert.That(CombatRules.ClampToArena(inside), Is.EqualTo(inside));

            Vector2 outside = new Vector2(50f, 50f);
            Vector2 clamped = CombatRules.ClampToArena(outside);
            Assert.That(CombatRules.IsInsideArena(clamped), Is.True);
            Assert.That(clamped.x, Is.LessThanOrEqualTo(CombatRules.ArenaMaxX));
            Assert.That(clamped.y, Is.LessThanOrEqualTo(CombatRules.ArenaMaxY));
        }

        [TestCase(1f, 0f)]
        [TestCase(1f, 1f)]
        [TestCase(0f, 1f)]
        [TestCase(-1f, 1f)]
        [TestCase(-1f, 0f)]
        [TestCase(-1f, -1f)]
        [TestCase(0f, -1f)]
        [TestCase(1f, -1f)]
        public void AimDirection_NormalizesAllEightDirections(float x, float y)
        {
            Assert.That(CombatRules.TryNormalizeDirection(new Vector2(x, y), out Vector2 direction), Is.True);
            Assert.That(direction.magnitude, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(Vector2.Dot(direction, new Vector2(x, y)), Is.GreaterThan(0f));
        }

        [Test]
        public void CommitAction_IsIdempotent_WhenReleaseAndTimeoutRace()
        {
            GameObject matchObject = new GameObject("MatchStateMachine_Test");
            GameObject actorObject = new GameObject("PlayerActor_Test");
            try
            {
                MatchStateMachine match = matchObject.AddComponent<MatchStateMachine>();
                ActorView actor = actorObject.AddComponent<ActorView>();
                actor.actorId = "player";
                actor.currentHP = CombatRules.InitialHP;
                match.playerActor = actor;
                match.currentState = MatchState.Aiming;

                Assert.That(match.CommitPlayerAction(Vector2.zero, Vector2.right, true), Is.True);
                Assert.That(match.CommitPlayerAction(Vector2.one, Vector2.up, true), Is.False);
                Assert.That(match.IsPlayerActionLocked, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(actorObject);
                Object.DestroyImmediate(matchObject);
            }
        }

        [Test]
        public void ProjectileSweep_DetectsFastFirstHitPath()
        {
            Assert.That(ProjectileView.SegmentHitsCircle(
                new Vector2(-10f, 0f),
                new Vector2(10f, 0f),
                new Vector2(0f, 0.4f),
                0.5f), Is.True);

            Assert.That(ProjectileView.SegmentHitsCircle(
                new Vector2(-10f, 0f),
                new Vector2(10f, 0f),
                new Vector2(0f, 1f),
                0.5f), Is.False);
        }

        [Test]
        public void BotSeed_ReproducesDraftWithoutReadingPlayerState()
        {
            BotBrain first = new BotBrain();
            BotBrain second = new BotBrain();
            first.Reset(42);
            second.Reset(42);
            first.PrepareNewRound(new Vector2(0f, 3f));
            second.PrepareNewRound(new Vector2(0f, 3f));

            Assert.That(first.TargetPosition, Is.EqualTo(second.TargetPosition));
            Assert.That(first.DraftDirection, Is.EqualTo(second.DraftDirection));
        }

        [Test]
        public void MatchResult_SimultaneousEliminationIsDraw_AndRoundCapUsesHp()
        {
            Assert.That(CombatRules.DetermineMatchResult(0, 0, 3), Is.EqualTo(MatchResultType.Draw));
            Assert.That(CombatRules.DetermineMatchResult(3, 2, CombatRules.MaxRounds), Is.EqualTo(MatchResultType.PlayerWin));
            Assert.That(CombatRules.DetermineMatchResult(2, 2, CombatRules.MaxRounds), Is.EqualTo(MatchResultType.Draw));
            Assert.That(CombatRules.DetermineMatchResult(3, 3, 1), Is.EqualTo(MatchResultType.None));
        }

        [Test]
        public void WorldProjection_RoundTripsScreenPointOnArenaPlane()
        {
            GameObject cameraObject = new GameObject("ArenaCamera_Test");
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.aspect = 390f / 844f;
                ArenaProjection.ConfigureTopDownCamera(camera);
                Vector2 expected = new Vector2(1.25f, -2.1f);
                Vector3 screen = camera.WorldToScreenPoint(new Vector3(expected.x, expected.y, 0f));

                Assert.That(ArenaProjection.TryScreenPointToArena(camera, screen, out Vector2 actual), Is.True);
                Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.001f));
                Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.001f));
                Assert.That(camera.orthographicSize, Is.GreaterThanOrEqualTo(
                    (CombatRules.ArenaMaxX - CombatRules.ArenaMinX) * 0.5f / camera.aspect));
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
