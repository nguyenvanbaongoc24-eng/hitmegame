/* AG integration: copy this file and GestureReplay.cs into the same Core/Phase1
 * location as the existing PlacementGestureV2 implementation. Copy replays to
 * Assets/Tests/Data/gesture_v2_sync, and replace Test16's final-only helper with
 * GestureReplaySyncTests below. Do not retain the old final-only replay helper.
 */
using System;

namespace HitMe.Core
{
    public readonly struct GestureSnapshot
    {
        public readonly bool HasPlaced, AimActive, Locked;
        public readonly Vec2 Position, Facing;
        public readonly int GestureId;
        public GestureSnapshot(bool hasPlaced, Vec2 position, Vec2 facing, bool aimActive, bool locked, int gestureId)
        {
            HasPlaced = hasPlaced; Position = position; Facing = facing;
            AimActive = aimActive; Locked = locked; GestureId = gestureId;
        }
    }

    public sealed class PlacementGestureV2
    {
        public static readonly Vec2 DefaultFacing = Vec2.Up;
        int _activePointer = -1, _gestureId;
        bool _hasPlaced, _aimActive, _locked;
        Vec2 _position, _facing, _downScreen, _downGame, _preAim;

        public static float MmToPixels(float mm, float dpi)
        {
            if (float.IsNaN(mm) || float.IsInfinity(mm) || mm < 0) return 0;
            if (float.IsNaN(dpi) || float.IsInfinity(dpi) || dpi <= 0) dpi = 160;
            return mm * dpi / 25.4f;
        }

        public void BeginRound(Vec2 previousPosition, Vec2 defaultFacing)
        {
            _position = previousPosition;
            _facing = Finite(defaultFacing) && defaultFacing.Length > 0 ? defaultFacing.Normalized : DefaultFacing;
            _activePointer = -1;
            _hasPlaced = _aimActive = _locked = false;
            _gestureId++;
        }

        public GestureSnapshot Snapshot() => new(_hasPlaced, _position, _facing, _aimActive, _locked, _gestureId);

        public void PointerDown(int pointerId, Vec2 screen, Vec2 game, bool insideArena, long timeMs)
        {
            if (_locked || _activePointer >= 0 || !insideArena || !Finite(screen) || !Finite(game)) return;
            _activePointer = pointerId;
            _hasPlaced = true;
            _aimActive = false;
            _downScreen = screen;
            _downGame = game;
            _position = game;
            _preAim = _facing = DefaultFacing;
            _gestureId++;
        }

        public void PointerMove(int pointerId, Vec2 screen, Vec2 game, bool insideArena, long timeMs, float minAimPixels)
        {
            if (_locked || pointerId != _activePointer || !Finite(screen) || !Finite(game)) return;
            if ((screen - _downScreen).Length < minAimPixels)
            {
                _aimActive = false;
                _facing = _preAim;
                return;
            }
            var direction = game - _downGame;
            if (direction.Length <= 0.00001f) return;
            _facing = direction.Normalized;
            _aimActive = true;
        }

        public void PointerUp(int pointerId, Vec2 screen, Vec2 game, bool insideArena, long timeMs)
        {
            if (pointerId == _activePointer) _activePointer = -1;
        }

        public void Lock() { _locked = true; _activePointer = -1; }

        static bool Finite(Vec2 value) =>
            !float.IsNaN(value.X) && !float.IsInfinity(value.X) &&
            !float.IsNaN(value.Y) && !float.IsInfinity(value.Y);
    }
}
