using NUnit.Framework;
using HitMe.Core;

namespace HitMe.Tests
{
    [TestFixture]
    public class PlacementGestureV2Tests
    {
        private static PlacementGestureV2 CreateGesture()
        {
            var g = new PlacementGestureV2();
            g.BeginRound(new Vec2(2f, 2f), Vec2.Up);
            return g;
        }

        private static void Down(PlacementGestureV2 g, int pointerId = 1, float sx = 100f, float sy = 100f, float gx = 1f, float gy = 1f, bool inside = true)
        {
            g.PointerDown(pointerId, new Vec2(sx, sy), new Vec2(gx, gy), inside, 0);
        }

        private static void Move(PlacementGestureV2 g, int pointerId = 1, float sx = 100f, float sy = 100f, float gx = 1f, float gy = 1f, bool inside = true, float minAim = 10f)
        {
            g.PointerMove(pointerId, new Vec2(sx, sy), new Vec2(gx, gy), inside, 0, minAim);
        }

        [Test]
        public void Test01_TouchDownPlacesAndDefaultsFacing()
        {
            var g = CreateGesture();
            Down(g, 1, 100f, 100f, 1f, 1f, true);
            var s = g.Snapshot();
            Assert.IsTrue(s.HasPlaced);
            Assert.AreEqual(1f, s.Position.X, 0.001f);
            Assert.AreEqual(1f, s.Position.Y, 0.001f);
            Assert.AreEqual(Vec2.Up.X, s.Facing.X, 0.001f);
            Assert.AreEqual(Vec2.Up.Y, s.Facing.Y, 0.001f);
            Assert.IsFalse(s.AimActive);
        }

        [Test]
        public void Test02_DragAimsWithoutMovingPosition()
        {
            var g = CreateGesture();
            Down(g, 1, 100f, 100f, 1f, 1f, true);
            Move(g, 1, 150f, 100f, 4f, 1f, true, 10f); // 50px drag right
            var s = g.Snapshot();
            Assert.IsTrue(s.AimActive);
            Assert.AreEqual(1f, s.Position.X, 0.001f);
            Assert.AreEqual(1f, s.Position.Y, 0.001f);
            Assert.AreEqual(1f, s.Facing.X, 0.001f);
            Assert.AreEqual(0f, s.Facing.Y, 0.001f);
        }

        [Test]
        public void Test03_DragUnderThresholdKeepsFacing()
        {
            var g = CreateGesture();
            Down(g, 1, 100f, 100f, 1f, 1f, true);
            Move(g, 1, 105f, 100f, 2f, 1f, true, 10f); // 5px drag < 10px
            var s = g.Snapshot();
            Assert.IsFalse(s.AimActive);
            Assert.AreEqual(Vec2.Up.X, s.Facing.X, 0.001f);
            Assert.AreEqual(Vec2.Up.Y, s.Facing.Y, 0.001f);
        }

        [Test]
        public void Test04_PointerUpRetainsPositionAndFacing()
        {
            var g = CreateGesture();
            Down(g, 1, 100f, 100f, 1f, 1f, true);
            Move(g, 1, 150f, 100f, 4f, 1f, true, 10f);
            g.PointerUp(1, new Vec2(150f, 100f), new Vec2(4f, 1f), true, 0);
            var s = g.Snapshot();
            Assert.IsTrue(s.AimActive);
            Assert.AreEqual(1f, s.Facing.X, 0.001f);
            Assert.AreEqual(0f, s.Facing.Y, 0.001f);
        }

        [Test]
        public void Test05_NewTouchResetsFacingToDefaultAndClearsOldAim()
        {
            var g = CreateGesture();
            Down(g, 1, 100f, 100f, 1f, 1f, true);
            Move(g, 1, 150f, 100f, 4f, 1f, true, 10f);
            g.PointerUp(1, new Vec2(150f, 100f), new Vec2(4f, 1f), true, 0);

            // New touch at different point Q(5, 5)
            Down(g, 2, 200f, 200f, 5f, 5f, true);
            var s = g.Snapshot();
            Assert.AreEqual(5f, s.Position.X, 0.001f);
            Assert.AreEqual(5f, s.Position.Y, 0.001f);
            Assert.AreEqual(Vec2.Up.X, s.Facing.X, 0.001f);
            Assert.AreEqual(Vec2.Up.Y, s.Facing.Y, 0.001f);
            Assert.IsFalse(s.AimActive);
        }

        [Test]
        public void Test06_NewTouchThenDragAimsIndependently()
        {
            var g = CreateGesture();
            Down(g, 1, 100f, 100f, 1f, 1f, true);
            Move(g, 1, 150f, 100f, 4f, 1f, true, 10f);
            g.PointerUp(1, new Vec2(150f, 100f), new Vec2(4f, 1f), true, 0);

            Down(g, 2, 200f, 200f, 5f, 5f, true);
            Move(g, 2, 200f, 260f, 5f, 8f, true, 10f); // drag up
            var s = g.Snapshot();
            Assert.IsTrue(s.AimActive);
            Assert.AreEqual(0f, s.Facing.X, 0.001f);
            Assert.AreEqual(1f, s.Facing.Y, 0.001f);
        }

        [Test]
        public void Test07_ReverseDragBelowThresholdCancelsAim()
        {
            var g = CreateGesture();
            Down(g, 1, 100f, 100f, 1f, 1f, true);
            Move(g, 1, 150f, 100f, 4f, 1f, true, 10f);
            Assert.IsTrue(g.Snapshot().AimActive);

            // Drag back below threshold
            Move(g, 1, 102f, 100f, 1.1f, 1f, true, 10f);
            var s = g.Snapshot();
            Assert.IsFalse(s.AimActive);
            Assert.AreEqual(Vec2.Up.X, s.Facing.X, 0.001f);
            Assert.AreEqual(Vec2.Up.Y, s.Facing.Y, 0.001f);
        }

        [Test]
        public void Test08_SecondFingerIgnoredWhileFirstIsActive()
        {
            var g = CreateGesture();
            Down(g, 1, 100f, 100f, 1f, 1f, true);
            Down(g, 2, 900f, 900f, 9f, 9f, true); // second finger
            var s = g.Snapshot();
            Assert.AreEqual(1f, s.Position.X, 0.001f);
            Assert.AreEqual(1f, s.Position.Y, 0.001f);
        }

        [Test]
        public void Test09_OutsideStartIgnored_MoveOutsideStillAims()
        {
            var g = CreateGesture();
            Down(g, 1, 50f, 50f, 0f, 0f, false); // outside arena
            Assert.IsFalse(g.Snapshot().HasPlaced);

            // Start inside, move outside
            Down(g, 2, 100f, 100f, 1f, 1f, true);
            Move(g, 2, 180f, 100f, 5f, 1f, false, 10f); // moves outside
            var s = g.Snapshot();
            Assert.IsTrue(s.AimActive);
            Assert.AreEqual(1f, s.Facing.X, 0.001f);
            Assert.AreEqual(0f, s.Facing.Y, 0.001f);
        }

        [Test]
        public void Test10_LockBlocksSubsequentEvents()
        {
            var g = CreateGesture();
            Down(g, 1, 100f, 100f, 1f, 1f, true);
            g.Lock();
            Move(g, 1, 150f, 100f, 4f, 1f, true, 10f);
            var s = g.Snapshot();
            Assert.IsTrue(s.Locked);
            Assert.IsFalse(s.AimActive);
            Assert.AreEqual(Vec2.Up.X, s.Facing.X, 0.001f);
            Assert.AreEqual(Vec2.Up.Y, s.Facing.Y, 0.001f);
        }

        [Test]
        public void Test11_NoTouchRetainsPreviousRoundPosition()
        {
            var g = CreateGesture();
            var s = g.Snapshot();
            Assert.IsFalse(s.HasPlaced);
            Assert.AreEqual(2f, s.Position.X, 0.001f);
            Assert.AreEqual(2f, s.Position.Y, 0.001f);
            Assert.AreEqual(Vec2.Up.X, s.Facing.X, 0.001f);
        }

        [Test]
        public void Test12_NewRoundResetsFacingAndMarksUnplaced()
        {
            var g = CreateGesture();
            Down(g, 1, 100f, 100f, 1f, 1f, true);
            Move(g, 1, 150f, 100f, 4f, 1f, true, 10f);
            g.BeginRound(g.Snapshot().Position, Vec2.Up);
            var s = g.Snapshot();
            Assert.IsFalse(s.HasPlaced);
            Assert.AreEqual(1f, s.Position.X, 0.001f);
            Assert.AreEqual(Vec2.Up.X, s.Facing.X, 0.001f);
        }

        [Test]
        public void Test13_BoundaryClampingLogic()
        {
            var clamped = ArenaCoordinateMapper.ClampToWorldArena(new Vec2(100f, -100f));
            Assert.AreEqual(ArenaCoordinateMapper.WorldMaxX, clamped.X, 0.001f);
            Assert.AreEqual(ArenaCoordinateMapper.WorldMinY, clamped.Y, 0.001f);
        }

        [Test]
        public void Test14_DeterministicSequenceProducesIdenticalResults()
        {
            var g1 = CreateGesture();
            var g2 = CreateGesture();

            Down(g1, 1, 100f, 100f, 1f, 1f, true);
            Move(g1, 1, 130f, 140f, 2.5f, 3.5f, true, 10f);

            Down(g2, 1, 100f, 100f, 1f, 1f, true);
            Move(g2, 1, 130f, 140f, 2.5f, 3.5f, true, 10f);

            var s1 = g1.Snapshot();
            var s2 = g2.Snapshot();

            Assert.AreEqual(s1.Position.X, s2.Position.X, 0.0001f);
            Assert.AreEqual(s1.Position.Y, s2.Position.Y, 0.0001f);
            Assert.AreEqual(s1.Facing.X, s2.Facing.X, 0.0001f);
            Assert.AreEqual(s1.Facing.Y, s2.Facing.Y, 0.0001f);
            Assert.AreEqual(s1.AimActive, s2.AimActive);
        }

        [Test]
        public void Test15_MmToPixelsConversionsAcrossDifferentDpis()
        {
            // 4mm at 160 dpi = 4 * 160 / 25.4 = 25.197 px
            Assert.AreEqual(25.197f, PlacementGestureV2.MmToPixels(4f, 160f), 0.01f);

            // 4mm at 326 dpi (iPhone retina) = 4 * 326 / 25.4 = 51.338 px
            Assert.AreEqual(51.338f, PlacementGestureV2.MmToPixels(4f, 326f), 0.01f);

            // 4mm at 460 dpi (iPhone 15 Pro) = 4 * 460 / 25.4 = 72.441 px
            Assert.AreEqual(72.441f, PlacementGestureV2.MmToPixels(4f, 460f), 0.01f);

            // Invalid inputs fallback safely
            Assert.AreEqual(0f, PlacementGestureV2.MmToPixels(-5f, 160f));
            Assert.AreEqual(25.197f, PlacementGestureV2.MmToPixels(4f, -1f), 0.01f); // default to 160 dpi
        }
    }
}
