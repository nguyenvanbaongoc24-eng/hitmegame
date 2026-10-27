using System;
using UnityEngine;

namespace HitMe.Core
{
    public class BotBrain
    {
        private System.Random _random = new System.Random(20261027);
        private float _thinkDuration = 3f;
        private float _elapsed;
        private bool _isLocked;

        public Vector2 StartPosition { get; private set; }
        public Vector2 TargetPosition { get; private set; }
        public Vector2 CurrentPosition { get; private set; }
        public Vector2 DraftDirection { get; private set; }

        public void Reset(int seed)
        {
            _random = new System.Random(seed);
            _elapsed = 0f;
            _isLocked = false;
        }

        public void PrepareNewRound(Vector2 initialBotPos)
        {
            _elapsed = 0f;
            _isLocked = false;
            StartPosition = initialBotPos;
            CurrentPosition = initialBotPos;
            _thinkDuration = NextFloat(2.2f, 4.2f);

            float posX = NextFloat(CombatRules.ArenaMinX * 0.82f, CombatRules.ArenaMaxX * 0.82f);
            float posY = NextFloat(1f, CombatRules.ArenaMaxY * 0.82f);
            TargetPosition = CombatRules.ClampToArena(new Vector2(posX, posY));

            // The bot deliberately aims at a general court region. It never reads
            // the player's secret position or aim command.
            float targetX = NextFloat(CombatRules.ArenaMinX * 0.8f, CombatRules.ArenaMaxX * 0.8f);
            float targetY = NextFloat(CombatRules.ArenaMinY * 0.85f, -1f);
            DraftDirection = CombatRules.NormalizeDirectionOrDefault(
                new Vector2(targetX, targetY) - TargetPosition,
                Vector2.down);
        }

        public bool UpdateTick(float deltaTime)
        {
            _elapsed += deltaTime;
            CurrentPosition = Vector2.MoveTowards(CurrentPosition, TargetPosition, CombatRules.BotMoveSpeed * deltaTime);
            CurrentPosition = CombatRules.ClampToArena(CurrentPosition);

            if (!_isLocked && _elapsed >= _thinkDuration) _isLocked = true;
            return _isLocked;
        }

        public LockedAction GetLockedAction(string botId)
        {
            return new LockedAction(botId, CurrentPosition, DraftDirection, "dep_to_ong", true);
        }

        private float NextFloat(float min, float max)
        {
            return min + (float)_random.NextDouble() * (max - min);
        }
    }
}
