using HitMe.Core;
using UnityEngine;

namespace HitMe.Game
{
    [CreateAssetMenu(menuName = "HIT ME/Phase 1 Game Config", fileName = "GameConfig_Phase1")]
    public sealed class GameConfigAsset : ScriptableObject
    {
        private static readonly GameConfig Defaults = new GameConfig();

        public int maxPlayers = Defaults.MaxPlayers;
        public int startHp = Defaults.StartHp;
        public float placeSeconds = Defaults.PlaceSeconds;
        public float revealSeconds = Defaults.RevealSeconds;
        public float throwSeconds = Defaults.ThrowSeconds;
        public float resolveSeconds = Defaults.ResolveSeconds;
        public int maxRounds = Defaults.MaxRounds;
        [Tooltip("Prepared for a future approved round cap. Phase 1 currently continues until elimination.")]
        public RoundLimitTiebreakPolicy roundLimitTiebreak = Defaults.RoundLimitTiebreak;
        public float arenaWidth = Defaults.ArenaWidth;
        public float arenaHeight = Defaults.ArenaHeight;
        public float playerRadius = Defaults.PlayerRadius;
        public float weaponSpeed = Defaults.WeaponSpeed;
        public int damagePerHit = Defaults.DamagePerHit;
        public int maxHitsPerThrow = Defaults.MaxHitsPerThrow;
        public float minSpawnDistance = Defaults.MinSpawnDistance;
        [Tooltip("Minimum world-space drag from the player before an aim direction is committed.")]
        public float minAimDrag = Defaults.MinAimDrag;
        [Tooltip("Multiplier for the visible and functional aim touch ring around the player.")]
        public float aimTouchRadiusMultiplier = Defaults.AimTouchRadiusMultiplier;
        [Tooltip("World-space distance that extends the aim ruler beyond the pointer.")]
        public float aimLineExtraLength = Defaults.AimLineExtraLength;
        [Tooltip("Fallback used when the placement timer ends without an aim.")]
        public NoAimPolicy noAimRule = Defaults.NoAimRule;
        public uint seed = Defaults.Seed;

        [Header("V4 Visual Scaling (Core Hitbox remains strictly 0.55f)")]
        public float characterScale = Defaults.CharacterScale;
        public float weaponScale = Defaults.WeaponScale;
        public float hitRingScale = Defaults.HitRingScale;

        [Header("Shared Runtime Visual Assets (WebGL + Editor)")]
        public Sprite shadowSprite;
        public Sprite footRingSprite;
        public TMPro.TMP_FontAsset vietnameseFont;
        public Sprite[] characterIdleSprites;
        public Sprite[] namLangPoseSprites;
        public Sprite[] anhNonLaPoseSprites;
        public Sprite sandalProjectileSprite;
        public Sprite[] hitFxFrames;

        public GameConfig ToCore()
        {
            return new GameConfig
            {
                MaxPlayers = maxPlayers,
                StartHp = startHp,
                PlaceSeconds = placeSeconds,
                RevealSeconds = revealSeconds,
                ThrowSeconds = throwSeconds,
                ResolveSeconds = resolveSeconds,
                MaxRounds = maxRounds,
                RoundLimitTiebreak = roundLimitTiebreak,
                ArenaWidth = arenaWidth,
                ArenaHeight = arenaHeight,
                PlayerRadius = playerRadius,
                WeaponSpeed = weaponSpeed,
                DamagePerHit = damagePerHit,
                MaxHitsPerThrow = maxHitsPerThrow,
                MinSpawnDistance = minSpawnDistance,
                MinAimDrag = minAimDrag,
                AimTouchRadiusMultiplier = aimTouchRadiusMultiplier,
                AimLineExtraLength = aimLineExtraLength,
                NoAimRule = noAimRule,
                Seed = seed,
                CharacterScale = characterScale,
                WeaponScale = weaponScale,
                HitRingScale = hitRingScale
            };
        }
    }
}
