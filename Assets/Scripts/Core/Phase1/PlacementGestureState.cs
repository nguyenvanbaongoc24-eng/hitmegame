namespace HitMe.Core
{
    public enum PlacementGestureMode { Idle, Moving, Aiming }

    /// <summary>
    /// Pure input state for one player's placement phase. It deliberately knows
    /// nothing about Unity input, camera conversion, arena collision, or timers.
    /// The game layer constrains Placement through MatchState before displaying it.
    /// </summary>
    public sealed class PlacementGestureState
    {
        public PlacementGestureMode Mode { get; private set; }
        public Vec2 Placement { get; private set; }
        public Vec2 FallbackDirection { get; private set; }
        public Vec2 AimDirection { get; private set; }
        public bool HasAim { get; private set; }
        public Vec2 EffectiveDirection { get { return HasAim ? AimDirection : FallbackDirection; } }

        public PlacementGestureState(Vec2 placement, Vec2 fallbackDirection)
        {
            Placement = placement;
            FallbackDirection = fallbackDirection.LengthSquared > 0.00001f ? fallbackDirection.Normalized : Vec2.Up;
            AimDirection = FallbackDirection;
            Mode = PlacementGestureMode.Idle;
        }

        public void SyncPlacement(Vec2 placement) { Placement = placement; }
        public void SetFallbackDirection(Vec2 direction)
        {
            FallbackDirection = direction.LengthSquared > 0.00001f ? direction.Normalized : Vec2.Up;
            if (!HasAim) AimDirection = FallbackDirection;
        }

        public void SetAimDirection(Vec2 direction)
        {
            if (direction.LengthSquared <= 0.00001f) return;
            AimDirection = direction.Normalized;
            HasAim = true;
        }

        public void PointerDown(Vec2 point, float playerHitRadius)
        {
            if (Vec2.Distance(point, Placement) <= playerHitRadius)
            {
                Mode = PlacementGestureMode.Aiming;
                return;
            }
            Mode = PlacementGestureMode.Moving;
            Placement = point;
        }

        public void PointerMove(Vec2 point, float minAimDrag)
        {
            if (Mode == PlacementGestureMode.Moving)
            {
                Placement = point;
                return;
            }
            if (Mode != PlacementGestureMode.Aiming) return;
            var delta = point - Placement;
            if (delta.Length < minAimDrag)
            {
                HasAim = false;
                AimDirection = FallbackDirection;
                return;
            }
            AimDirection = delta.Normalized;
            HasAim = true;
        }

        public void PointerUp() { Mode = PlacementGestureMode.Idle; }

        public RoundInput BuildRoundInput(string playerId, bool ready)
        {
            return new RoundInput(playerId)
            {
                HasPlacement = true,
                Placement = Placement,
                HasAim = HasAim,
                AimDirection = EffectiveDirection,
                IsReady = ready
            };
        }
    }
}
