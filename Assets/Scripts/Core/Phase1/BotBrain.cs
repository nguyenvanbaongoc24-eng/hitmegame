using System.Collections.Generic;

namespace HitMe.Core
{
    public sealed class Phase1BotBrain
    {
        private readonly BotLevel _level;
        private readonly SeededRandom _random;
        private readonly List<Vec2> _observedPositions = new List<Vec2>();
        private RoundInput _cachedInput;

        public Phase1BotBrain(BotLevel level, SeededRandom random)
        {
            _level = level;
            _random = random;
        }

        public void ObservePlayerPosition(Vec2 position)
        {
            _observedPositions.Add(position);
            if (_observedPositions.Count > 4) _observedPositions.RemoveAt(0);
        }

        public RoundInput MakeInput(PlayerState bot, GameConfig config)
        {
            var placement = new Vec2(_random.Range(config.MinX, config.MaxX), _random.Range(config.MinY, config.MaxY));
            var target = Vec2.Zero;
            if (_level == BotLevel.CenterHunter)
            {
                placement = new Vec2(_random.Range(config.MinX * 0.7f, config.MaxX * 0.7f), _random.Range(config.MinY * 0.7f, config.MaxY * 0.7f));
            }
            else if (_level == BotLevel.Predictor && _observedPositions.Count > 0)
            {
                target = _observedPositions[_observedPositions.Count - 1];
                if (_observedPositions.Count > 1)
                {
                    var velocity = _observedPositions[_observedPositions.Count - 1] - _observedPositions[_observedPositions.Count - 2];
                    target = target + velocity;
                }
            }
            var direction = (target - placement).Normalized;
            if (_cachedInput == null || _cachedInput.PlayerId != bot.Id)
            {
                _cachedInput = new RoundInput(bot.Id);
            }
            _cachedInput.HasPlacement = true;
            _cachedInput.Placement = placement;
            _cachedInput.HasAim = true;
            _cachedInput.AimDirection = direction;
            _cachedInput.IsReady = true;
            return _cachedInput;
        }
    }
}
