using UnityEngine;

namespace HitMe.Core
{
    public class BotBrain
    {
        private float _thinkDuration = 3.0f;
        private float _elapsed = 0f;
        private bool _isLocked = false;

        public Vector2 StartPosition { get; private set; }
        public Vector2 TargetPosition { get; private set; }
        public Vector2 CurrentPosition { get; private set; }
        public Vector2 DraftDirection { get; private set; }

        public void PrepareNewRound(Vector2 initialBotPos)
        {
            _elapsed = 0f;
            _isLocked = false;
            StartPosition = initialBotPos;
            CurrentPosition = initialBotPos;
            _thinkDuration = Random.Range(2.2f, 4.2f); // Human-like reaction time < 5s

            // Bot picks a destination in the upper court (y in [1.0, 3.8])
            float posX = Random.Range(CombatRules.ArenaMinX * 0.82f, CombatRules.ArenaMaxX * 0.82f);
            float posY = Random.Range(1.0f, CombatRules.ArenaMaxY * 0.82f);
            TargetPosition = CombatRules.ClampToArena(new Vector2(posX, posY));

            // FAIR BOT (GP-009): Does NOT read player's secret coordinate!
            // Aims at general lower court (y in [-4.5, -1.0])
            float targetX = Random.Range(CombatRules.ArenaMinX * 0.8f, CombatRules.ArenaMaxX * 0.8f);
            float targetY = Random.Range(CombatRules.ArenaMinY * 0.85f, -1.0f);
            Vector2 generalTarget = new Vector2(targetX, targetY);

            DraftDirection = (generalTarget - TargetPosition).normalized;
        }

        public bool UpdateTick(float dt)
        {
            _elapsed += dt;

            // Move bot smoothly towards target position
            CurrentPosition = Vector2.MoveTowards(CurrentPosition, TargetPosition, CombatRules.BotMoveSpeed * dt);
            CurrentPosition = CombatRules.ClampToArena(CurrentPosition);

            if (!_isLocked && _elapsed >= _thinkDuration)
            {
                _isLocked = true;
                return true;
            }
            return _isLocked;
        }

        public LockedAction GetLockedAction(string botId)
        {
            // Atomically freeze at current physical position
            return new LockedAction(botId, CurrentPosition, DraftDirection, "dep_cao_su");
        }
    }
}
