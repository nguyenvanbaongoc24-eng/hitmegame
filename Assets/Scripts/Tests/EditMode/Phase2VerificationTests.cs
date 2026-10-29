using System.Collections.Generic;
using NUnit.Framework;
using HitMe.Core;

namespace HitMe.Tests
{
    public class Phase2VerificationTests
    {
        [Test]
        public void GameToWorld_And_WorldToGame_AreInvertible_WithinArenaBounds()
        {
            // Test center
            Vec2 center = new Vec2(0f, 0f);
            Vec2 worldCenter = ArenaCoordinateMapper.GameToWorld(center);
            Assert.That(worldCenter.X, Is.EqualTo(0f).Within(0.001f));
            Assert.That(worldCenter.Y, Is.EqualTo(0f).Within(0.001f));
            Vec2 backCenter = ArenaCoordinateMapper.WorldToGame(worldCenter);
            Assert.That(backCenter.X, Is.EqualTo(center.X).Within(0.001f));
            Assert.That(backCenter.Y, Is.EqualTo(center.Y).Within(0.001f));

            // Test arbitrary points inside arena
            Vec2[] testPoints = new Vec2[]
            {
                new Vec2(-2.5f, -3.0f),
                new Vec2(1.8f, 2.2f),
                new Vec2(-1.0f, 3.5f),
                new Vec2(2.8f, -4.0f)
            };

            foreach (var pt in testPoints)
            {
                Vec2 wPos = ArenaCoordinateMapper.GameToWorld(pt);
                Assert.That(ArenaCoordinateMapper.IsInsideArena(wPos), Is.True, $"Point {wPos} should be inside arena");
                Vec2 gPos = ArenaCoordinateMapper.WorldToGame(wPos);
                Assert.That(gPos.X, Is.EqualTo(pt.X).Within(0.001f));
                Assert.That(gPos.Y, Is.EqualTo(pt.Y).Within(0.001f));
            }
        }

        [Test]
        public void GameToWorld_ClampsOutOfBoundsCoordinates()
        {
            // Point far outside right/top
            Vec2 outside = new Vec2(100f, 100f);
            Vec2 clampedWorld = ArenaCoordinateMapper.GameToWorld(outside);
            Assert.That(clampedWorld.X, Is.EqualTo(ArenaCoordinateMapper.WorldMaxX).Within(0.001f));
            Assert.That(clampedWorld.Y, Is.EqualTo(ArenaCoordinateMapper.WorldMaxY).Within(0.001f));
            Assert.That(ArenaCoordinateMapper.IsInsideArena(clampedWorld), Is.True);

            // Point far outside left/bottom
            Vec2 outsideNeg = new Vec2(-100f, -100f);
            Vec2 clampedNeg = ArenaCoordinateMapper.GameToWorld(outsideNeg);
            Assert.That(clampedNeg.X, Is.EqualTo(ArenaCoordinateMapper.WorldMinX).Within(0.001f));
            Assert.That(clampedNeg.Y, Is.EqualTo(ArenaCoordinateMapper.WorldMinY).Within(0.001f));
            Assert.That(ArenaCoordinateMapper.IsInsideArena(clampedNeg), Is.True);
        }

        [Test]
        public void LocalizationTable_ContainsAllRequiredPhase2Keys_WithAccents()
        {
            string[] requiredKeys = new string[]
            {
                LocalizationTable.KeyPhaseAim,
                LocalizationTable.KeyOpponentHidden,
                LocalizationTable.KeyBtnReady,
                LocalizationTable.KeyResultWin,
                LocalizationTable.KeyResultLose,
                LocalizationTable.KeyResultDraw,
                LocalizationTable.KeyBtnRematch,
                LocalizationTable.KeyActionLocked,
                LocalizationTable.KeyRoundPrefix,
                LocalizationTable.KeyHpPlayer,
                LocalizationTable.KeyHpBot
            };

            foreach (var key in requiredKeys)
            {
                Assert.That(LocalizationTable.HasKey(key), Is.True, $"Missing localization key: {key}");
                string val = LocalizationTable.Get(key);
                Assert.That(string.IsNullOrEmpty(val), Is.False, $"Localization value empty for: {key}");
            }

            // Specific required text asserts per Phase 2 requirements
            Assert.That(LocalizationTable.Get(LocalizationTable.KeyPhaseAim), Is.EqualTo("CHỌN HƯỚNG NÉM"));
            Assert.That(LocalizationTable.Get(LocalizationTable.KeyOpponentHidden), Is.EqualTo("Đối thủ đang ẩn"));
            Assert.That(LocalizationTable.Get(LocalizationTable.KeyBtnReady), Is.EqualTo("SẴN SÀNG"));
            Assert.That(LocalizationTable.Get(LocalizationTable.KeyResultWin), Is.EqualTo("Thắng"));
            Assert.That(LocalizationTable.Get(LocalizationTable.KeyResultLose), Is.EqualTo("Thua"));
            Assert.That(LocalizationTable.Get(LocalizationTable.KeyResultTitle), Is.EqualTo("Kết quả"));
            Assert.That(LocalizationTable.Get(LocalizationTable.KeyHintTapPlace), Is.EqualTo("Chạm để đặt nhân vật"));
            Assert.That(LocalizationTable.Get(LocalizationTable.KeyHintHoldDrag), Is.EqualTo("Giữ và kéo để chọn hướng ném"));
            Assert.That(LocalizationTable.Get(LocalizationTable.KeyHintRetapReset), Is.EqualTo("Chạm chỗ khác để đặt lại · Giữ và kéo để đổi hướng"));
        }

        [Test]
        public void Homography_FourCorners_Invertible_WithinOneHundredth()
        {
            // 4 corners of the arena: Top-Left, Top-Right, Bottom-Left, Bottom-Right
            Vec2[] corners = new Vec2[]
            {
                new Vec2(ArenaCoordinateMapper.WorldMinX, ArenaCoordinateMapper.WorldMinY), // Bottom-Left (-3.2, -5.0)
                new Vec2(ArenaCoordinateMapper.WorldMaxX, ArenaCoordinateMapper.WorldMinY), // Bottom-Right (3.2, -5.0)
                new Vec2(ArenaCoordinateMapper.WorldMinX, ArenaCoordinateMapper.WorldMaxY), // Top-Left (-3.2, 4.2)
                new Vec2(ArenaCoordinateMapper.WorldMaxX, ArenaCoordinateMapper.WorldMaxY)  // Top-Right (3.2, 4.2)
            };

            foreach (var corner in corners)
            {
                Vec2 gamePos = ArenaCoordinateMapper.WorldToGame(corner);
                Vec2 backToWorld = ArenaCoordinateMapper.GameToWorld(gamePos);

                float errorX = System.Math.Abs(backToWorld.X - corner.X);
                float errorY = System.Math.Abs(backToWorld.Y - corner.Y);

                Assert.That(errorX, Is.LessThan(0.01f), $"Corner {corner} X homography error {errorX} >= 0.01");
                Assert.That(errorY, Is.LessThan(0.01f), $"Corner {corner} Y homography error {errorY} >= 0.01");
            }
        }

        [Test]
        public void GameConfig_PhaseTiming_RevealToResult_UnderSixSeconds()
        {
            var config = new GameConfig();
            Assert.That(config.RevealSeconds, Is.GreaterThan(0f));
            Assert.That(config.ThrowSeconds, Is.GreaterThan(0f));
            Assert.That(config.ResolveSeconds, Is.GreaterThan(0f));
            float total = config.TotalResolutionSeconds;
            Assert.That(total, Is.LessThanOrEqualTo(6.0f), $"Total resolution time {total}s exceeds 6.0s limit!");
        }
    }
}
