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

        [Test]
        public void Test16_01_Replay_PlaceAndAim()
        {
            AssertReplayFixture("01_place_and_aim.json");
        }

        [Test]
        public void Test16_02_Replay_RetapResets()
        {
            AssertReplayFixture("02_retap_resets.json");
        }

        [Test]
        public void Test16_03_Replay_DragUnder4mmCancels()
        {
            AssertReplayFixture("03_drag_under_4mm_cancels.json");
        }

        [Test]
        public void Test16_04_Replay_NoTouch()
        {
            AssertReplayFixture("04_no_touch.json");
        }

        [Test]
        public void Test16_05_Replay_NoAim()
        {
            AssertReplayFixture("05_no_aim.json");
        }

        [Test]
        public void Test16_06_Replay_OutsideStart()
        {
            AssertReplayFixture("06_outside_start.json");
        }

        [Test]
        public void Test16_07_Replay_SecondFingerIgnored()
        {
            AssertReplayFixture("07_second_finger_ignored.json");
        }

        [Test]
        public void Test16_08_Replay_LockBlocksInput()
        {
            AssertReplayFixture("08_lock_blocks_input.json");
        }

        [Test]
        public void Test17_CancelAimByDraggingBackUnder4mmRestoresDefaultFacing()
        {
            var g = CreateGesture();
            float minAim = PlacementGestureV2.MmToPixels(4.0f, 160f); // ~25.2 px

            // 1. Touch down at screen (100, 100), game (2, 2)
            g.PointerDown(1, new Vec2(100f, 100f), new Vec2(2f, 2f), true, 0);
            var s1 = g.Snapshot();
            Assert.IsTrue(s1.HasPlaced);
            Assert.IsFalse(s1.AimActive);
            Assert.AreEqual(Vec2.Up.X, s1.Facing.X, 0.001f);
            Assert.AreEqual(Vec2.Up.Y, s1.Facing.Y, 0.001f);

            // 2. Drag right by 60 px (> 25.2 px) -> aim active, facing (1, 0)
            g.PointerMove(1, new Vec2(160f, 100f), new Vec2(6f, 2f), true, 0, minAim);
            var s2 = g.Snapshot();
            Assert.IsTrue(s2.AimActive);
            Assert.AreEqual(1f, s2.Facing.X, 0.001f);
            Assert.AreEqual(0f, s2.Facing.Y, 0.001f);

            // 3. Drag back to (103, 100) (3 px < 25.2 px threshold) -> aim canceled, facing restored to DefaultFacing
            g.PointerMove(1, new Vec2(103f, 100f), new Vec2(2.3f, 2f), true, 0, minAim);
            var s3 = g.Snapshot();
            Assert.IsFalse(s3.AimActive, "Aim must be inactive when dragged back under 4mm");
            Assert.AreEqual(Vec2.Up.X, s3.Facing.X, 0.001f, "Facing must restore to DefaultFacing (Up) under 4mm");
            Assert.AreEqual(Vec2.Up.Y, s3.Facing.Y, 0.001f, "Facing must restore to DefaultFacing (Up) under 4mm");

            // 4. Release pointer -> facing stays DefaultFacing (Up), aim inactive
            g.PointerUp(1, new Vec2(103f, 100f), new Vec2(2.3f, 2f), true, 0);
            var s4 = g.Snapshot();
            Assert.IsFalse(s4.AimActive);
            Assert.AreEqual(Vec2.Up.X, s4.Facing.X, 0.001f);
            Assert.AreEqual(Vec2.Up.Y, s4.Facing.Y, 0.001f);
        }

        [Test]
        public void Test18_RosterSelection_200Seeds_AllPlayerChoices_NeverDuplicatesCharacter()
        {
            Assert.AreEqual(7, HitMe.Game.PlayerView.TotalCharacterCount, "Roster must contain all 7 real characters.");
            for (int localChoice = 0; localChoice < HitMe.Game.PlayerView.TotalCharacterCount; localChoice++)
            {
                for (uint seed = 1; seed <= 200; seed++)
                {
                    var rng = new SeededRandom(seed);
                    int[] roster = HitMe.Game.MatchController.SelectMatchRoster(localChoice, 6, rng);
                    Assert.AreEqual(6, roster.Length);
                    Assert.AreEqual(localChoice, roster[0], "Local player character choice must be preserved at index 0.");

                    var seen = new System.Collections.Generic.HashSet<int>();
                    for (int i = 0; i < roster.Length; i++)
                    {
                        int c = roster[i];
                        Assert.IsTrue(c >= 0 && c < 7, $"Character index {c} out of valid range [0..6]");
                        Assert.IsTrue(seen.Add(c), $"Duplicate character {c} found in match roster for seed={seed}, localChoice={localChoice}");
                    }
                }
            }
        }

        private sealed class StepReplayEvent
        {
            public string Kind { get; set; } = "";
            public int PointerId { get; set; }
            public float Sx { get; set; }
            public float Sy { get; set; }
            public float Gx { get; set; }
            public float Gy { get; set; }
            public bool Inside { get; set; }
            public HitMe.GestureV2.ReplayExpected Expected { get; set; }
        }

        private sealed class StepReplayFixture
        {
            public System.Collections.Generic.List<StepReplayEvent> Events { get; set; } = new System.Collections.Generic.List<StepReplayEvent>();
            public HitMe.GestureV2.ReplayExpected Expected { get; set; } = new HitMe.GestureV2.ReplayExpected();
        }

        private static readonly System.Text.Json.JsonSerializerOptions StepJsonOptions =
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private static void AssertExpectedState(string context, HitMe.GestureV2.ReplayExpected e, GestureSnapshot s)
        {
            Assert.IsNotNull(e, $"{context}: Expected state is null");
            Assert.AreEqual(e.HasPlaced, s.HasPlaced, $"{context}: HasPlaced mismatch");
            Assert.AreEqual(e.AimActive, s.AimActive, $"{context}: AimActive mismatch");
            Assert.AreEqual(e.Locked, s.Locked, $"{context}: Locked mismatch");
            Assert.AreEqual(e.PositionX, s.Position.X, 0.001f, $"{context}: PositionX mismatch");
            Assert.AreEqual(e.PositionY, s.Position.Y, 0.001f, $"{context}: PositionY mismatch");
            Assert.AreEqual(e.FacingX, s.Facing.X, 0.001f, $"{context}: FacingX mismatch");
            Assert.AreEqual(e.FacingY, s.Facing.Y, 0.001f, $"{context}: FacingY mismatch");
        }

        private static void AssertReplayFixture(string filename)
        {
            string replaysDir = System.IO.Path.GetFullPath("tools/patches/gesture_v2/replays");
            string path = System.IO.Path.Combine(replaysDir, filename);
            Assert.IsTrue(System.IO.File.Exists(path), "Replay file exists: " + path);

            string json = System.IO.File.ReadAllText(path);
            var stepFixture = System.Text.Json.JsonSerializer.Deserialize<StepReplayFixture>(json, StepJsonOptions);
            Assert.IsNotNull(stepFixture, $"{filename}: stepFixture deserialized");

            var g = new PlacementGestureV2();
            g.BeginRound(Vec2.Zero, PlacementGestureV2.DefaultFacing);
            for (int i = 0; i < stepFixture.Events.Count; i++)
            {
                var ev = stepFixture.Events[i];
                var screen = new Vec2(ev.Sx, ev.Sy);
                var game = new Vec2(ev.Gx, ev.Gy);
                if (ev.Kind == "down") g.PointerDown(ev.PointerId, screen, game, ev.Inside, 0);
                else if (ev.Kind == "move") g.PointerMove(ev.PointerId, screen, game, ev.Inside, 0, 10f);
                else if (ev.Kind == "up") g.PointerUp(ev.PointerId, screen, game, ev.Inside, 0);
                else if (ev.Kind == "lock") g.Lock();

                AssertExpectedState($"{filename}[step {i}:{ev.Kind}]", ev.Expected, g.Snapshot());
            }

            var fixture = HitMe.GestureV2.GestureReplay.Load(json);
            var s = HitMe.GestureV2.GestureReplay.Play(fixture, 10f);
            AssertExpectedState($"{filename}[final]", fixture.Expected, s);
        }

        [Test]
        public void Test19_SteadyStateFrameAllocationIsZeroBytesAcross3600Frames()
        {
            var g = CreateGesture();
            float minAim = PlacementGestureV2.MmToPixels(4.0f, 160f);
            g.PointerDown(1, new Vec2(100f, 100f), new Vec2(1.5f, -1.0f), true, 0);

            // Warm up static label table and gesture methods
            _ = HitMe.Game.PlayerView.GetPrebuiltHpLabel(0, 3);
            g.PointerMove(1, new Vec2(160f, 120f), new Vec2(3.5f, 1.0f), true, 16, minAim);
            _ = g.Snapshot();

            long beforeBytes = System.GC.GetAllocatedBytesForCurrentThread();
            for (int frame = 0; frame < 3600; frame++)
            {
                float angle = (frame % 360) * 0.01745329f;
                float sx = 100f + (float)System.Math.Cos(angle) * 80f;
                float sy = 100f + (float)System.Math.Sin(angle) * 80f;
                g.PointerMove(1, new Vec2(sx, sy), new Vec2(1.5f, -1.0f), true, frame * 16L, minAim);
                var snap = g.Snapshot();
                for (int c = 0; c < 6; c++)
                {
                    string lbl = HitMe.Game.PlayerView.GetPrebuiltHpLabel(c, 3);
                    if (lbl.Length == 0) Assert.Fail();
                }
                if (!snap.HasPlaced) Assert.Fail();
            }
            long afterBytes = System.GC.GetAllocatedBytesForCurrentThread();
            long deltaBytes = afterBytes - beforeBytes;
            Assert.AreEqual(0L, deltaBytes, $"Expected 0 bytes allocated across 3,600 steady-state frames (60s @ 60fps), but got {deltaBytes} B.");
        }
    }
}
