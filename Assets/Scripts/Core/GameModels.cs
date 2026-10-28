using System;
using UnityEngine;

namespace HitMe.Core
{
    // Legacy prototype state; Phase 1's server-shareable state lives in Core/Phase1/MatchState.cs.
    public enum LegacyMatchState
    {
        Initializing,
        PreparingRound,
        Aiming,
        Locking,
        Revealing,
        Resolving,
        RoundSummary,
        MatchEnded
    }

    public enum ActorType
    {
        Player,
        Bot
    }

    public enum CharacterPose
    {
        Idle,
        Move,
        Aim,
        ThrowWindup,
        ThrowRelease,
        Hit,
        Victory,
        Defeat
    }

    public enum MatchResultType
    {
        None,
        PlayerWin,
        BotWin,
        Draw
    }

    [Serializable]
    public class ActorStats
    {
        public int throwsCount = 0;
        public int hitsCount = 0;

        public float HitRate => throwsCount > 0 ? (float)hitsCount / throwsCount * 100f : 0f;
    }

    [Serializable]
    public class LockedAction
    {
        public string actorId;
        public Vector2 position;
        public Vector2 direction;
        public string weaponId;

        public LockedAction(string actorId, Vector2 position, Vector2 direction, string weaponId)
        {
            this.actorId = actorId;
            this.position = position;
            this.direction = direction;
            this.weaponId = weaponId;
        }
    }

    [Serializable]
    public class HitRecord
    {
        public string shooterId;
        public string targetId;
        public Vector2 hitPoint;
        public float timeOffset;
        public int damage;
    }

    public static class CombatRules
    {
        public const string RulesetStatus = "Prototype";
        public const string RulesetVersion = "1.2-ArenaMovement";

        // Core rules (ĐÃ CHỐT)
        public const float AimDuration = 5.0f; // 5s aiming countdown
        public const int InitialHP = 3;
        public const int DamagePerHit = 1;
        public const float ProjectileSpeed = 14.0f;
        public const float ProjectileRadius = 0.5f;
        public const float ActorHitRadius = 0.65f;

        // Player Movement Speed
        public const float PlayerMoveSpeed = 5.2f; // Smooth travel across arena within 5s
        public const float BotMoveSpeed = 4.0f;

        // Arena logical bounds (fitted to white line of ARENA.png)
        public const float ArenaMinX = -3.2f;
        public const float ArenaMaxX = 3.2f;
        public const float ArenaMinY = -5.0f;
        public const float ArenaMaxY = 4.2f;
        public const float CornerRadius = 0.9f;

        // Rounded rectangle boundary clamping
        public static Vector2 ClampToArena(Vector2 pos)
        {
            // First clamp inside rectangle box
            float x = Mathf.Clamp(pos.x, ArenaMinX, ArenaMaxX);
            float y = Mathf.Clamp(pos.y, ArenaMinY, ArenaMaxY);

            // Corner centers
            float innerMinX = ArenaMinX + CornerRadius;
            float innerMaxX = ArenaMaxX - CornerRadius;
            float innerMinY = ArenaMinY + CornerRadius;
            float innerMaxY = ArenaMaxY - CornerRadius;

            // Check if in one of the 4 rounded corners
            if (x < innerMinX && y < innerMinY)
            {
                // Bottom-left corner
                Vector2 cornerCenter = new Vector2(innerMinX, innerMinY);
                Vector2 dir = new Vector2(x, y) - cornerCenter;
                if (dir.magnitude > CornerRadius)
                {
                    Vector2 clamped = cornerCenter + dir.normalized * CornerRadius;
                    return clamped;
                }
            }
            else if (x > innerMaxX && y < innerMinY)
            {
                // Bottom-right corner
                Vector2 cornerCenter = new Vector2(innerMaxX, innerMinY);
                Vector2 dir = new Vector2(x, y) - cornerCenter;
                if (dir.magnitude > CornerRadius)
                {
                    Vector2 clamped = cornerCenter + dir.normalized * CornerRadius;
                    return clamped;
                }
            }
            else if (x < innerMinX && y > innerMaxY)
            {
                // Top-left corner
                Vector2 cornerCenter = new Vector2(innerMinX, innerMaxY);
                Vector2 dir = new Vector2(x, y) - cornerCenter;
                if (dir.magnitude > CornerRadius)
                {
                    Vector2 clamped = cornerCenter + dir.normalized * CornerRadius;
                    return clamped;
                }
            }
            else if (x > innerMaxX && y > innerMaxY)
            {
                // Top-right corner
                Vector2 cornerCenter = new Vector2(innerMaxX, innerMaxY);
                Vector2 dir = new Vector2(x, y) - cornerCenter;
                if (dir.magnitude > CornerRadius)
                {
                    Vector2 clamped = cornerCenter + dir.normalized * CornerRadius;
                    return clamped;
                }
            }

            return new Vector2(x, y);
        }
    }
}
