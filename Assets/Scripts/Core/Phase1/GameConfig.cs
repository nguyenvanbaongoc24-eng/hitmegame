using System;

namespace HitMe.Core
{
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
        public float MinAimDrag = 0.25f;
        public uint Seed = 12345;

        public float MinX { get { return -ArenaWidth * 0.5f + PlayerRadius; } }
        public float MaxX { get { return ArenaWidth * 0.5f - PlayerRadius; } }
        public float MinY { get { return -ArenaHeight * 0.5f + PlayerRadius; } }
        public float MaxY { get { return ArenaHeight * 0.5f - PlayerRadius; } }

        public GameConfig Clone()
        {
            return (GameConfig)MemberwiseClone();
        }
    }
}
