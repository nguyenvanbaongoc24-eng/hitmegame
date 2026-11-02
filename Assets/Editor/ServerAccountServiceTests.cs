using NUnit.Framework;
using HitMe.Game;

namespace HitMe.Editor
{
    [TestFixture]
    public class ServerAccountServiceTests
    {
        private ServerAccountService _service;

        [SetUp]
        public void Setup()
        {
            _service = ServerAccountService.Instance;
            _service.ResetForTest();
        }

        [TearDown]
        public void Teardown()
        {
            _service.ResetForTest();
        }

        [Test]
        public void Test_ServerAccountService_InitialStatePending()
        {
            Assert.That(_service.IsDataLoaded, Is.False);
            Assert.That(_service.Gold, Is.EqualTo(0));
            Assert.That(_service.DisplayName, Is.Empty);
        }

        [Test]
        public void Test_ServerAccountService_ApplyProfileState_CodexV2Payload()
        {
            bool eventFired = false;
            _service.OnAccountUpdated += () => eventFired = true;

            string codexV2Json = @"{
                ""playerId"": ""HM-7788"",
                ""name"": ""Nam_Lang"",
                ""gold"": 1250,
                ""matchesPlayed"": 15,
                ""bestRank"": 1,
                ""rank"": ""unranked"",
                ""points"": 320,
                ""purchasedItemIds"": [""dep_to_ong_bac""],
                ""ownedItemIds"": [""dep_to_ong"", ""dep_to_ong_bac""],
                ""equippedWeaponId"": ""dep_to_ong_bac"",
                ""inventoryStatus"": ""starter_only""
            }";

            bool ok = _service.ApplyProfileState(codexV2Json);
            Assert.That(ok, Is.True);
            Assert.That(eventFired, Is.True);

            Assert.That(_service.IsDataLoaded, Is.True);
            Assert.That(_service.PlayerId, Is.EqualTo("HM-7788"));
            Assert.That(_service.DisplayName, Is.EqualTo("Nam_Lang"));
            Assert.That(_service.Gold, Is.EqualTo(1250));
            Assert.That(_service.MatchesPlayed, Is.EqualTo(15));
            Assert.That(_service.RankPoints, Is.EqualTo(320));
            Assert.That(_service.EquippedWeaponId, Is.EqualTo("dep_to_ong_bac"));
            Assert.That(_service.OwnedItemIds.Contains("dep_to_ong_bac"), Is.True);
        }

        [Test]
        public void Test_ServerAccountService_AuthoritativeMutationsDisallowed()
        {
            // Client cannot locally deduct or add gold on ServerAccountService
            Assert.That(_service.AddGold(100), Is.False);
            Assert.That(_service.DeductGold(50), Is.False);
            Assert.That(_service.Rename("Hacker"), Is.False);
        }

        [Test]
        public void Test_ServerAccountService_ApplyBuyResult()
        {
            _service.ApplyProfileState(@"{""playerId"":""HM-1"",""name"":""Test"",""gold"":500,""matchesPlayed"":0,""points"":0}");
            Assert.That(_service.Gold, Is.EqualTo(500));

            string buyResultJson = @"{
                ""success"": true,
                ""itemId"": ""chao_chong_dinh"",
                ""newGold"": 300,
                ""error"": null
            }";

            bool bought = _service.ApplyBuyResult(buyResultJson);
            Assert.That(bought, Is.True);
            Assert.That(_service.Gold, Is.EqualTo(300));
            Assert.That(_service.OwnedItemIds.Contains("chao_chong_dinh"), Is.True);
        }
    }
}
