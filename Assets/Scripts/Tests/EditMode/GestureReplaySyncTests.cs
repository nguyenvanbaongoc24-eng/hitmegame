using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using HitMe.Core;
using NUnit.Framework;
using UnityEngine;

namespace HitMe.GestureV2Sync
{
    public sealed class ReplayExpected
    {
        public bool HasPlaced { get; set; }
        public bool AimActive { get; set; }
        public bool Locked { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float FacingX { get; set; }
        public float FacingY { get; set; }
    }

    public sealed class ReplayEvent
    {
        public string Kind { get; set; } = "";
        public int PointerId { get; set; }
        public float Sx { get; set; }
        public float Sy { get; set; }
        public float Gx { get; set; }
        public float Gy { get; set; }
        public bool Inside { get; set; }
        public ReplayExpected Expected { get; set; }
    }

    public sealed class ReplayFixture
    {
        public List<ReplayEvent> Events { get; set; } = new List<ReplayEvent>();
        public ReplayExpected Expected { get; set; }
    }

    public readonly struct ReplayStep
    {
        public readonly ReplayEvent Event;
        public readonly GestureSnapshot Snapshot;
        public ReplayStep(ReplayEvent replayEvent, GestureSnapshot snapshot) { Event = replayEvent; Snapshot = snapshot; }
    }

    public static class GestureReplay
    {
        static readonly JsonSerializerOptions Json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        public static ReplayFixture Load(string json) => JsonSerializer.Deserialize<ReplayFixture>(json, Json) ?? throw new InvalidOperationException("Invalid replay fixture.");

        public static IReadOnlyList<ReplayStep> PlaySteps(ReplayFixture fixture, float threshold = 10)
        {
            var gesture = new PlacementGestureV2();
            gesture.BeginRound(Vec2.Zero, PlacementGestureV2.DefaultFacing);
            var steps = new List<ReplayStep>(fixture.Events.Count);
            foreach (var replayEvent in fixture.Events)
            {
                var screen = new Vec2(replayEvent.Sx, replayEvent.Sy);
                var game = new Vec2(replayEvent.Gx, replayEvent.Gy);
                switch (replayEvent.Kind)
                {
                    case "down": gesture.PointerDown(replayEvent.PointerId, screen, game, replayEvent.Inside, 0); break;
                    case "move": gesture.PointerMove(replayEvent.PointerId, screen, game, replayEvent.Inside, 0, threshold); break;
                    case "up": gesture.PointerUp(replayEvent.PointerId, screen, game, replayEvent.Inside, 0); break;
                    case "lock": gesture.Lock(); break;
                    default: throw new InvalidOperationException($"Unknown replay event '{replayEvent.Kind}'.");
                }
                steps.Add(new ReplayStep(replayEvent, gesture.Snapshot()));
            }
            return steps;
        }
    }
}

public sealed class GestureReplaySyncTests
{
    const float Epsilon = 0.0001f;

    public static string FixtureFolder => Path.Combine(Application.dataPath, "Tests", "Data", "gesture_v2_sync");

    [Test]
    public void AllReplayFixturesMatchEveryExpectedStepAndFinalState()
    {
        var folder = FixtureFolder;
        var files = Directory.GetFiles(folder, "*.json");
        Assert.That(files, Has.Length.EqualTo(12), "Expected the full v1.2 replay suite.");
        Array.Sort(files, StringComparer.Ordinal);

        foreach (var file in files)
        {
            AssertFixtureFile(file);
        }
    }

    public static void AssertFixtureByName(string fileName)
    {
        var path = Path.Combine(FixtureFolder, fileName);
        Assert.That(File.Exists(path), Is.True, $"Replay file exists: {path}");
        AssertFixtureFile(path);
    }

    public static void AssertFixtureFile(string file)
    {
        var fixture = HitMe.GestureV2Sync.GestureReplay.Load(File.ReadAllText(file));
        var steps = HitMe.GestureV2Sync.GestureReplay.PlaySteps(fixture);
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

    static void AssertSnapshot(HitMe.GestureV2Sync.ReplayExpected expected, GestureSnapshot actual, string fixture, object step)
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
