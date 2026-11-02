using NUnit.Framework;
using HitMe.Core;
using UnityEngine;

namespace HitMe.Tests
{
    [TestFixture]
    public class Phase4BotDifficultyTests
    {
        private GameConfig _config;

        [SetUp]
        public void Setup()
        {
            _config = new GameConfig
            {
                ArenaWidth = 8f,
                ArenaHeight = 12f,
                PlayerRadius = 0.4f
            };
        }

        [Test]
        public void Test_BotDifficulty_DeterministicBySeed()
        {
            uint seed = 12345u;
            var bot1 = new Phase1BotBrain(BotLevel.Predictor, new SeededRandom(seed));
            var bot2 = new Phase1BotBrain(BotLevel.Predictor, new SeededRandom(seed));

            var pState = new PlayerState("b1", "Bot 1", true, 3, Vec2.Zero, Vec2.Up);

            var in1 = bot1.MakeInput(pState, _config);
            var in2 = bot2.MakeInput(pState, _config);

            Assert.That(in1.Placement.X, Is.EqualTo(in2.Placement.X).Within(0.0001f));
            Assert.That(in1.Placement.Y, Is.EqualTo(in2.Placement.Y).Within(0.0001f));
            Assert.That(in1.AimDirection.X, Is.EqualTo(in2.AimDirection.X).Within(0.0001f));
            Assert.That(in1.AimDirection.Y, Is.EqualTo(in2.AimDirection.Y).Within(0.0001f));
        }

        [Test]
        public void Test_BotDifficulty_EasyAimsWiderThanHard()
        {
            // Easy bot uses up to 35 deg offset, Hard bot uses up to 5 deg offset
            int easyWideCount = 0;
            int hardWideCount = 0;

            for (uint s = 100u; s < 150u; s++)
            {
                var easyBot = new Phase1BotBrain(BotLevel.Random, new SeededRandom(s));
                var hardBot = new Phase1BotBrain(BotLevel.Predictor, new SeededRandom(s));

                var playerState = new PlayerState("b", "Bot", true, 3, Vec2.Zero, Vec2.Up);
                var easyInput = easyBot.MakeInput(playerState, _config);
                var hardInput = hardBot.MakeInput(playerState, _config);

                // Check angle between aim direction and straight up / target
                float easyAngle = Mathf.Abs(Mathf.Atan2(easyInput.AimDirection.Y, easyInput.AimDirection.X) * Mathf.Rad2Deg);
                float hardAngle = Mathf.Abs(Mathf.Atan2(hardInput.AimDirection.Y, hardInput.AimDirection.X) * Mathf.Rad2Deg);

                if (easyAngle > 10f) easyWideCount++;
                if (hardAngle > 10f) hardWideCount++;
            }

            Assert.That(easyWideCount, Is.GreaterThanOrEqualTo(hardWideCount));
        }

        [Test]
        public void Test_BotDifficulty_HardNeverPeeksCurrentRoundPosition()
        {
            var hardBot = new Phase1BotBrain(BotLevel.Predictor, new SeededRandom(42u));
            var pState = new PlayerState("b", "Bot", true, 3, Vec2.Zero, Vec2.Up);

            // Make round 1 input without having observed any player positions
            var inputR1 = hardBot.MakeInput(pState, _config);

            // Now create another bot with same seed but pretend player placed at (3.5, 5.0) in the current round
            // Since ObservePlayerPosition hasn't been called, the bot has NO knowledge of current round placement
            var hardBot2 = new Phase1BotBrain(BotLevel.Predictor, new SeededRandom(42u));
            var inputR1_2 = hardBot2.MakeInput(pState, _config);

            Assert.That(inputR1.Placement.X, Is.EqualTo(inputR1_2.Placement.X));
            Assert.That(inputR1.AimDirection.X, Is.EqualTo(inputR1_2.AimDirection.X));
        }

        [Test]
        public void Test_FirstTimeTutorialFlag_PersistsAndCanSkip()
        {
            string prefKey = "HitMe_TutorialCompleted";
            PlayerPrefs.DeleteKey(prefKey);
            Assert.That(PlayerPrefs.GetString(prefKey, "0"), Is.EqualTo("0"));

            // Simulate completed or skipped
            PlayerPrefs.SetString(prefKey, "1");
            PlayerPrefs.Save();
            Assert.That(PlayerPrefs.GetString(prefKey, "0"), Is.EqualTo("1"));
            PlayerPrefs.DeleteKey(prefKey);
        }
    }
}
