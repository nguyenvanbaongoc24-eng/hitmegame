/* AG integration: copy with PlacementGestureV2.cs. Tests must call PlaySteps,
 * compare every event's expected state, and then compare the fixture final state.
 * This replaces the prior Test16 helper that compared only the final state.
 */
using System;
using System.Collections.Generic;
using System.Text.Json;
using HitMe.Core;

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
        public ReplayExpected Expected { get; set; } = null!;
    }

    public sealed class ReplayFixture
    {
        public List<ReplayEvent> Events { get; set; } = new();
        public ReplayExpected Expected { get; set; } = null!;
    }

    public readonly struct ReplayStep
    {
        public readonly ReplayEvent Event;
        public readonly GestureSnapshot Snapshot;
        public ReplayStep(ReplayEvent replayEvent, GestureSnapshot snapshot) { Event = replayEvent; Snapshot = snapshot; }
    }

    public static class GestureReplay
    {
        static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
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
