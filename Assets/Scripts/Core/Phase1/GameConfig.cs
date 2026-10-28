using System;

namespace HitMe.Core
{
    public enum NoAimPolicy
    {
        ThrowFacing = 0,
        NoThrow = 1,
        RepeatLastAim = 2
    }

    // Prepared for a future approved round cap. Phase 1 keeps its current
    // elimination-only ending rule regardless of this configured value.
    public enum RoundLimitTiebreakPolicy
    {
        ContinueUntilElimination = 0,
        HighestHp = 1,
        SharedWin = 2
    }

    [Serializable]
    public sealed class GameConfig
    {
        public int MaxPlayers = 6;
        public int StartHp = 3;
        public float PlaceSeconds = 5f;
        public float RevealSeconds = 0.8f;
        public float ThrowSeconds = 1.35f;
        public float ResolveSeconds = 1.2f;
        public float TotalResolutionSeconds => RevealSeconds + ThrowSeconds + ResolveSeconds;
        public int MaxRounds = 5;
        public RoundLimitTiebreakPolicy RoundLimitTiebreak = RoundLimitTiebreakPolicy.HighestHp;
        public float ArenaWidth = 9.0f;
        public float ArenaHeight = 11.4f;
        public float PlayerRadius = 0.55f;
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
