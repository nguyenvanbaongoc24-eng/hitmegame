using System;
using System.Collections.Generic;

namespace HitMe.Core
{
    [Serializable]
    public class BotDifficultyConfig
    {
        public int level;
        public string nameVi;
        public string nameEn;
        public float aimOffsetDegrees = 15f;
        public float centerWeight = 0.5f;
    }

    [Serializable]
    public class BotConfigGroup
    {
        public BotDifficultyConfig easy;
        public BotDifficultyConfig normal;
        public BotDifficultyConfig hard;
    }

    public sealed class Phase1BotBrain
    {
        private readonly BotLevel _level;
        private readonly SeededRandom _random;
        private readonly List<Vec2> _observedPositions = new List<Vec2>();
        private RoundInput _cachedInput;

        public static BotConfigGroup Config { get; set; }

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

            float maxOffsetAngle = 15f;
            if (_level == BotLevel.Random)
            {
                // Dễ: nhắm lệch góc lớn (35 độ)
                maxOffsetAngle = Config?.easy?.aimOffsetDegrees ?? 35f;
                target = new Vec2(_random.Range(config.MinX * 0.5f, config.MaxX * 0.5f), _random.Range(config.MinY * 0.5f, config.MaxY * 0.5f));
            }
            else if (_level == BotLevel.CenterHunter)
            {
                // Thường: đứng gần trung tâm hơn, độ lệch vừa phải (~15 độ)
                maxOffsetAngle = Config?.normal?.aimOffsetDegrees ?? 15f;
                placement = new Vec2(_random.Range(config.MinX * 0.7f, config.MaxX * 0.7f), _random.Range(config.MinY * 0.7f, config.MaxY * 0.7f));
                target = Vec2.Zero;
            }
            else if (_level == BotLevel.Predictor)
            {
                // Khó: đoán vị trí tốt hơn từ lịch sử các vòng trước (_observedPositions)
                // KHÔNG biết vị trí thật của người chơi trong vòng hiện tại
                maxOffsetAngle = Config?.hard?.aimOffsetDegrees ?? 5f;
                if (_observedPositions.Count > 0)
                {
                    target = _observedPositions[_observedPositions.Count - 1];
                    if (_observedPositions.Count > 1)
                    {
                        var velocity = _observedPositions[_observedPositions.Count - 1] - _observedPositions[_observedPositions.Count - 2];
                        target = target + velocity;
                    }
                }
                else
                {
                    target = Vec2.Zero;
                }
            }

            var baseDir = (target - placement).Normalized;
            // Áp dụng góc lệch ngẫu nhiên nhưng hoàn toàn tất định bằng seed
            float offsetAngle = _random.Range(-maxOffsetAngle, maxOffsetAngle);
            var direction = Vec2.Rotate(baseDir, offsetAngle).Normalized;

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
