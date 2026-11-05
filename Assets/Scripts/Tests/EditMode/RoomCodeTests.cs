using HitMe.NetClientV1;
using NUnit.Framework;

namespace HitMe.Tests
{
    public sealed class RoomCodeTests
    {
        [TestCase(" hm4826 ", "HM4826")]
        [TestCase("aB2c3d", "AB2C3D")]
        public void AcceptsGeneratedAlphabetAndNormalizes(string raw, string expected)
        {
            Assert.IsTrue(RoomCode.TryNormalize(raw, out string actual));
            Assert.AreEqual(expected, actual);
        }

        [TestCase("")]
        [TestCase("HM482")]
        [TestCase("HM48267")]
        [TestCase("HM0001")]
        [TestCase("HM 826")]
        [TestCase("HMI826")]
        public void RejectsIncompleteOrAmbiguousCode(string raw)
        {
            Assert.IsFalse(RoomCode.TryNormalize(raw, out _));
        }
    }
}
