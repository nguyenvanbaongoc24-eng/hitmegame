/* AG integration: copy this test into an NUnit EditMode assembly; copy the JSON
 * fixtures to Assets/Tests/Data/gesture_v2_sync (or update FixtureFolder only).
 * Replace Test16's old final-state-only helper; do not delete unrelated tests.
 */
using System;
using System.IO;
using HitMe.Core;
using HitMe.GestureV2Sync;
using NUnit.Framework;

public sealed class GestureReplaySyncTests
{
    const float Epsilon = 0.0001f;

    [Test]
    public void AllReplayFixturesMatchEveryExpectedStepAndFinalState()
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "replays");
        var files = Directory.GetFiles(folder, "*.json");
        Assert.That(files, Has.Length.EqualTo(12), "Expected the full v1.2 replay suite.");
        Array.Sort(files, StringComparer.Ordinal);

        foreach (var file in files)
        {
            var fixture = GestureReplay.Load(File.ReadAllText(file));
            var steps = GestureReplay.PlaySteps(fixture);
            Assert.That(steps, Has.Count.EqualTo(fixture.Events.Count), $"{Path.GetFileName(file)}: step count");
            for (var index = 0; index < steps.Count; index++)
            {
                Assert.That(steps[index].Event.Expected, Is.Not.Null, $"{Path.GetFileName(file)} step {index + 1}: missing expected state");
                AssertSnapshot(steps[index].Event.Expected, steps[index].Snapshot, Path.GetFileName(file), index + 1);
            }
            var final = steps.Count == 0
                ? NewRoundSnapshot()
                : steps[steps.Count - 1].Snapshot;
            AssertSnapshot(fixture.Expected, final, Path.GetFileName(file), "final");
        }
    }

    [TestCase(160f, 25.19685f)]
    [TestCase(326f, 51.33858f)]
    [TestCase(460f, 72.44095f)]
    public void FourMillimetresUsesTheRequiredDpiConversion(float dpi, float expectedPixels)
    {
        Assert.That(PlacementGestureV2.MmToPixels(4f, dpi), Is.EqualTo(expectedPixels).Within(Epsilon));
    }

    [Test]
    public void NewRoundKeepsPreviousPositionAsHintAndResetsFacing()
    {
        var gesture = new PlacementGestureV2();
        gesture.BeginRound(new Vec2(3f, -2f), new Vec2(1f, 0f));
        var first = gesture.Snapshot();
        gesture.BeginRound(first.Position, PlacementGestureV2.DefaultFacing);
        var next = gesture.Snapshot();
        Assert.That(next.HasPlaced, Is.False, "new round: HasPlaced");
        Assert.That(next.Position, Is.EqualTo(new Vec2(3f, -2f)), "new round: Position");
        Assert.That(next.Facing, Is.EqualTo(Vec2.Up), "new round: Facing");
    }

    static GestureSnapshot NewRoundSnapshot()
    {
        var gesture = new PlacementGestureV2();
        gesture.BeginRound(Vec2.Zero, PlacementGestureV2.DefaultFacing);
        return gesture.Snapshot();
    }

    static void AssertSnapshot(ReplayExpected expected, GestureSnapshot actual, string fixture, object step)
    {
        var at = $"{fixture} step {step}";
        Assert.That(actual.HasPlaced, Is.EqualTo(expected.HasPlaced), $"{at}: HasPlaced");
        Assert.That(actual.AimActive, Is.EqualTo(expected.AimActive), $"{at}: AimActive");
        Assert.That(actual.Locked, Is.EqualTo(expected.Locked), $"{at}: Locked");
        Assert.That(actual.Position.X, Is.EqualTo(expected.PositionX).Within(Epsilon), $"{at}: PositionX");
        Assert.That(actual.Position.Y, Is.EqualTo(expected.PositionY).Within(Epsilon), $"{at}: PositionY");
        Assert.That(actual.Facing.X, Is.EqualTo(expected.FacingX).Within(Epsilon), $"{at}: FacingX");
        Assert.That(actual.Facing.Y, Is.EqualTo(expected.FacingY).Within(Epsilon), $"{at}: FacingY");
    }
}
