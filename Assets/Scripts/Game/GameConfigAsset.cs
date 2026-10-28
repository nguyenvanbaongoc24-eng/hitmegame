using HitMe.Core;
using UnityEngine;

namespace HitMe.Game
{
    [CreateAssetMenu(menuName = "HIT ME/Phase 1 Game Config", fileName = "GameConfig_Phase1")]
    public sealed class GameConfigAsset : ScriptableObject
    {
        public int maxPlayers = 6;
        public int startHp = 3;
        public float placeSeconds = 5f;
        public int maxRounds = 5;
        public float arenaWidth = 10f;
        public float arenaHeight = 16f;
        public float playerRadius = 0.45f;
        public float weaponSpeed = 14f;
        public int damagePerHit = 1;
        public int maxHitsPerThrow = 1;
        public float minSpawnDistance = 0.9f;
        [Tooltip("Minimum world-space drag from the player before an aim direction is committed.")]
        public float minAimDrag = 0.55f;
        [Tooltip("Multiplier for the visible and functional aim touch ring around the player.")]
        public float aimTouchRadiusMultiplier = 1.75f;
        [Tooltip("World-space distance that extends the aim ruler beyond the pointer.")]
        public float aimLineExtraLength = 0.35f;
        [Tooltip("Fallback used when the placement timer ends without an aim.")]
        public NoAimPolicy noAimRule = NoAimPolicy.ThrowFacing;
        public uint seed = 12345;

        public GameConfig ToCore()
        {
            return new GameConfig { MaxPlayers = maxPlayers, StartHp = startHp, PlaceSeconds = placeSeconds, MaxRounds = maxRounds, ArenaWidth = arenaWidth, ArenaHeight = arenaHeight, PlayerRadius = playerRadius, WeaponSpeed = weaponSpeed, DamagePerHit = damagePerHit, MaxHitsPerThrow = maxHitsPerThrow, MinSpawnDistance = minSpawnDistance, MinAimDrag = minAimDrag, AimTouchRadiusMultiplier = aimTouchRadiusMultiplier, AimLineExtraLength = aimLineExtraLength, NoAimRule = noAimRule, Seed = seed };
        }
    }
}
