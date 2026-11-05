using HitMe.Audio;
using NUnit.Framework;

namespace HitMe.Tests.EditMode
{
    public class AudioRoutingTests
    {
        [Test]
        public void StandaloneUnityUsesItsAssignedAudioClips()
        {
            Assert.IsTrue(AudioManager.ShouldPlayUnityAudioForMode(false));
        }

        [Test]
        public void EmbeddedWebUiKeepsOwnershipOfAudio()
        {
            Assert.IsFalse(AudioManager.ShouldPlayUnityAudioForMode(true));
        }
    }
}
