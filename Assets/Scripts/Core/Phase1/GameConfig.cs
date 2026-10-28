using System;

namespace HitMe.Core
{
    public enum NoAimPolicy
    {
        ThrowFacing = 0,
        NoThrow = 1,
        RepeatLastAim = 2
    }

    [Serializable]
    public sealed class GameConfig
    {
        public int MaxPlayers = 6;
        public int StartHp = 3;
        public float PlaceSeconds = 5f;
        public int MaxRounds = 5;
        public float ArenaWidth = 10f;
        public float ArenaHeight = 16f;
        public float PlayerRadius = 0.45f;
        public float WeaponSpeed = 14f;
        public int DamagePerHit = 1;
        public int MaxHitsPerThrow = 1;
        public float MinSpawnDistance = 0.9f;
        // 0.55 world units is about 59 pixels in the 1080px-wide portrait arena.
        // This avoids turning normal thumb jitter into an aim on a six-inch phone.
        public float MinAimDrag = 0.55f;
        public float AimTouchRadiusMultiplier = 1.75f;
        public float AimLineExtraLength = 0.35f;
        // Default preserves Phase 1's existing fallback behavior.
        public NoAimPolicy NoAimRule = NoAimPolicy.ThrowFacing;
        public uint Seed = 12345;

        public float MinX { get { return -ArenaWidth * 0.5f + PlayerRadius; } }
        public float MaxX { get { return ArenaWidth * 0.5f - PlayerRadius; } }
        public float MinY { get { return -ArenaHeight * 0.5f + PlayerRadius; } }
        public float MaxY { get { return ArenaHeight * 0.5f - PlayerRadius; } }
        public float AimTouchRadius { get { return PlayerRadius * AimTouchRadiusMultiplier; } }

        public GameConfig Clone()
        {
            return (GameConfig)MemberwiseClone();
        }
    }
}
