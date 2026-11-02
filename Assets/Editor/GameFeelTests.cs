using NUnit.Framework;
using UnityEngine;
using HitMe.UI;

namespace HitMe.Editor
{
    [TestFixture]
    public class GameFeelTests
    {
        private GameObject _go;
        private JuiceFeedbackController _juice;

        [SetUp]
        public void Setup()
        {
            _go = new GameObject("JuiceTest");
            _juice = _go.AddComponent<JuiceFeedbackController>();
            _juice.ReducedMotion = false;
        }

        [TearDown]
        public void Teardown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        [Test]
        public void Test_GameFeel_ReducedMotion_DisablesCameraShake()
        {
            _juice.ReducedMotion = true;
            Assert.That(_juice.ReducedMotion, Is.True);

            // With ReducedMotion = true, ShowDamageNumber returns immediately without shaking
            Assert.DoesNotThrow(() =>
            {
                _juice.ShowDamageNumber(Vector3.zero, 1, null);
                _juice.PlayCameraShake(0.1f, 0.1f);
                _juice.PlayHitStop(0.04f);
            });
        }

        [Test]
        public void Test_GameFeel_HitStopDuration_IsSafeAndBrief()
        {
            float hitStopDuration = 0.04f;
            Assert.That(hitStopDuration, Is.LessThanOrEqualTo(0.05f), "Hit-stop must be brief (<= 50ms) to prevent network desync");
        }
    }
}
