using System;

namespace HitMe.Core
{
    public readonly struct GestureSnapshot
    {
        public readonly bool HasPlaced;
        public readonly Vec2 Position;
        public readonly Vec2 Facing;
        public readonly bool AimActive;
        public readonly bool Locked;
        public readonly int GestureId;

        public GestureSnapshot(bool hasPlaced, Vec2 position, Vec2 facing, bool aimActive, bool locked, int gestureId)
        {
            HasPlaced = hasPlaced;
            Position = position;
            Facing = facing;
            AimActive = aimActive;
            Locked = locked;
            GestureId = gestureId;
        }
    }

    /// <summary>
    /// Pure C# gesture state machine for Placement Phase v1.2 (hit-me-gameplay-rules-v1.2.md).
    /// G1: Touch down anywhere inside arena -> places character immediately at touch point.
    /// G2: Hold and drag -> aim direction follows touch offset in game space without moving character.
    /// G3: Release finger -> retains position and aim until lock.
    /// G4: Retap elsewhere -> full reset of position, facing returns to DefaultFacing (Up), aim resets.
    /// G5: Drag under minAimPixels threshold (4mm) -> cancels aim, restores facing prior to drag.
    /// G6: Second touch ignored while first finger is held.
    /// G7: Touch started outside arena ignored; move exiting arena still tracks direction.
    /// </summary>
    public sealed class PlacementGestureV2
    {
        public static readonly Vec2 DefaultFacing = Vec2.Up;

        private int _activePointerId = -1;
        private int _gestureId;
        private bool _placed;
        private bool _aimActive;
        private bool _locked;

        private Vec2 _previousPosition;
        private Vec2 _position;
        private Vec2 _facing = DefaultFacing;
        private Vec2 _downScreen;
        private Vec2 _downGame;
        private Vec2 _preAimFacing = DefaultFacing;

        public static float MmToPixels(float mm, float dpi)
        {
            if (float.IsNaN(mm) || float.IsInfinity(mm) || mm < 0f) return 0f;
            if (float.IsNaN(dpi) || float.IsInfinity(dpi) || dpi <= 0f) dpi = 160f;
            return mm * dpi / 25.4f;
        }

        public void BeginRound(Vec2 previousPosition, Vec2 defaultFacing)
        {
            _previousPosition = previousPosition;
            _position = previousPosition;
            _facing = IsFinite(defaultFacing) && defaultFacing.Length > 0.0001f ? defaultFacing.Normalized : DefaultFacing;
            _activePointerId = -1;
            _placed = false;
            _aimActive = false;
            _locked = false;
            _gestureId++;
        }

        public GestureSnapshot Snapshot()
        {
            return new GestureSnapshot(_placed, _position, _facing, _aimActive, _locked, _gestureId);
        }

        public void PointerDown(int pointerId, Vec2 screen, Vec2 game, bool insideArena, long timeMs)
        {
            if (_locked || _activePointerId >= 0 || !insideArena || !IsFinite(screen) || !IsFinite(game))
            {
                return;
            }

            _activePointerId = pointerId;
            _placed = true;
            _aimActive = false;
            _downScreen = screen;
            _downGame = game;
            _position = game;
            _preAimFacing = DefaultFacing;
            _facing = DefaultFacing;
            _gestureId++;
        }

        public void PointerMove(int pointerId, Vec2 screen, Vec2 game, bool insideArena, long timeMs, float minAimPixels)
        {
            if (_locked || pointerId != _activePointerId || !IsFinite(screen) || !IsFinite(game))
            {
                return;
            }

            var screenDelta = screen - _downScreen;
            if (screenDelta.Length < minAimPixels)
            {
                _aimActive = false;
                _facing = _preAimFacing;
                return;
            }

            var gameDelta = game - _downGame;
            if (gameDelta.Length <= 0.00001f)
            {
                return;
            }

            _facing = gameDelta.Normalized;
            _aimActive = true;
        }

        public void PointerUp(int pointerId, Vec2 screen, Vec2 game, bool insideArena, long timeMs)
        {
            if (pointerId == _activePointerId)
            {
                _activePointerId = -1;
            }
        }

        public void Lock()
        {
            _locked = true;
            _activePointerId = -1;
        }

        private static bool IsFinite(Vec2 p)
        {
            return !float.IsNaN(p.X) && !float.IsNaN(p.Y) && !float.IsInfinity(p.X) && !float.IsInfinity(p.Y);
        }
    }
}
