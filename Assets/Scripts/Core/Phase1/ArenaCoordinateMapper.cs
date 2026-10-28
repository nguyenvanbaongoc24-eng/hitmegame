using System;

namespace HitMe.Core
{
    /// <summary>
    /// Bi-directional coordinate mapping between Phase 1 Core logical positions and Unity World space.
    /// Strictly guarantees arena boundary containment and invertibility.
    /// Pure C# implementation inside HitMe.Core assembly.
    /// </summary>
    public static class ArenaCoordinateMapper
    {
        public const float WorldMinX = -3.2f;
        public const float WorldMaxX = 3.2f;
        public const float WorldMinY = -5.0f;
        public const float WorldMaxY = 4.2f;

        public const float CoreMinX = -3.2f;
        public const float CoreMaxX = 3.2f;
        public const float CoreMinY = -5.0f;
        public const float CoreMaxY = 4.2f;

        private static float Clamp(float val, float min, float max)
        {
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        public static Vec2 GameToWorld(Vec2 gamePos)
        {
            float clampedX = Clamp(gamePos.X, CoreMinX, CoreMaxX);
            float clampedY = Clamp(gamePos.Y, CoreMinY, CoreMaxY);
            return new Vec2(clampedX, clampedY);
        }

        public static Vec2 WorldToGame(Vec2 worldPos)
        {
            float clampedX = Clamp(worldPos.X, WorldMinX, WorldMaxX);
            float clampedY = Clamp(worldPos.Y, WorldMinY, WorldMaxY);
            return new Vec2(clampedX, clampedY);
        }

        public static bool IsInsideArena(Vec2 pos)
        {
            return pos.X >= WorldMinX && pos.X <= WorldMaxX &&
                   pos.Y >= WorldMinY && pos.Y <= WorldMaxY;
        }

        public static Vec2 ClampToWorldArena(Vec2 pos)
        {
            return new Vec2(
                Clamp(pos.X, WorldMinX, WorldMaxX),
                Clamp(pos.Y, WorldMinY, WorldMaxY)
            );
        }
    }
}
