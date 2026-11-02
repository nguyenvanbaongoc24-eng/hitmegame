using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HitMe.Economy;
using HitMe.Game;
using HitMe.UI;
using HitMe.NetClientV1;

namespace HitMe.Tests
{
    [TestFixture]
    public class Phase4Luot3DataSanitizationTests
    {
        private GameObject _uiRoot;
        private ProductionUIController _controller;

        [SetUp]
        public void Setup()
        {
            NetConfig.Enabled = false;
            PlayerPrefs.DeleteKey(UiStrings.PrefKeyLanguage);
            UiStrings.SetLanguage(AppLanguage.Vietnamese);
            ItemCatalog.ResetForTesting();
            RankService.ResetForTesting();
            QuestService.ResetForTesting();
            FakeAccountService.Instance.ResetForTest("Khách_Sanitization", 500);

            _uiRoot = new GameObject("TestUIRoot");
            _controller = _uiRoot.AddComponent<ProductionUIController>();

            // Setup mock UI elements
            var homeGoldObj = new GameObject("TxtHomeGold");
            homeGoldObj.transform.SetParent(_uiRoot.transform);
            _controller.txtHomeGold = homeGoldObj.AddComponent<TextMeshProUGUI>();

            var homeGemsObj = new GameObject("TxtHomeGems");
            homeGemsObj.transform.SetParent(_uiRoot.transform);
            _controller.txtHomeGems = homeGemsObj.AddComponent<TextMeshProUGUI>();

            var profileGoldObj = new GameObject("TxtProfileGold");
            profileGoldObj.transform.SetParent(_uiRoot.transform);
            _controller.txtProfileGold = profileGoldObj.AddComponent<TextMeshProUGUI>();

            // Setup Leaderboard Dialog
            var lbDialog = new GameObject("DialogLeaderboard");
            lbDialog.transform.SetParent(_uiRoot.transform);
            var lbCard = new GameObject("Card");
            lbCard.transform.SetParent(lbDialog.transform);

            for (int i = 0; i < 5; i++)
            {
                var row = new GameObject("Row" + i);
                row.transform.SetParent(lbCard.transform);

                var rankText = new GameObject("Rank").AddComponent<TextMeshProUGUI>();
                rankText.transform.SetParent(row.transform);

                var nameText = new GameObject("Name").AddComponent<TextMeshProUGUI>();
                nameText.transform.SetParent(row.transform);

                var scoreText = new GameObject("Score").AddComponent<TextMeshProUGUI>();
                scoreText.transform.SetParent(row.transform);
            }
            _controller.dialogLeaderboard = lbDialog;
        }

        [TearDown]
        public void Teardown()
        {
            NetConfig.Enabled = false;
            PlayerPrefs.DeleteKey(UiStrings.PrefKeyLanguage);
            UiStrings.SetLanguage(AppLanguage.Vietnamese);
            if (_uiRoot != null) Object.DestroyImmediate(_uiRoot);
            FakeAccountService.Instance.ResetForTest("Khách_Sanitization", 500);
        }

        [Test]
        public void Test_L1_SocialLogin_NoPhaseLeaks()
        {
            // Verify coming soon strings don't mention 'Phase'
            Assert.That(UiStrings.LoginGoogleComingSoon.Contains("Phase"), Is.False, "LoginGoogleComingSoon must not leak Phase name");
            Assert.That(UiStrings.LoginFacebookComingSoon.Contains("Phase"), Is.False, "LoginFacebookComingSoon must not leak Phase name");
            Assert.That(UiStrings.LoginAppleComingSoon.Contains("Phase"), Is.False, "LoginAppleComingSoon must not leak Phase name");

            UiStrings.SetLanguage(AppLanguage.Vietnamese);
            Assert.That(UiStrings.Get(UiStrings.LoginGoogleComingSoon), Does.Contain("sắp ra mắt"));

            UiStrings.SetLanguage(AppLanguage.English);
            Assert.That(UiStrings.Get(UiStrings.LoginGoogleComingSoon), Does.Contain("coming soon"));
            UiStrings.SetLanguage(AppLanguage.Vietnamese);
        }

        [Test]
        public void Test_L1_OnlineMode_HidesMockLeaderboardAndFakeGold()
        {
            // 1. Offline / Practice mode: Shows real local gold and mock rankings
            NetConfig.Enabled = false;
            _controller.IsOnlineAccountLoaded = false;
            _controller.RefreshAccountDisplay();
            Assert.That(_controller.txtHomeGold.text, Is.EqualTo("500 G"));

            _controller.RefreshLeaderboardUI();
            var row0Name = _controller.dialogLeaderboard.transform.GetChild(0).Find("Row0/Name").GetComponent<TextMeshProUGUI>();
            Assert.That(row0Name.text, Does.Contain("Nguyễn_Vô_Địch"), "In offline mode, mock leaderboard is permitted");

            // 2. Online Mode with pending server data: MUST show Loading / No Data, never fake numbers
            NetConfig.Enabled = true;
            _controller.IsOnlineAccountLoaded = false;
            _controller.RefreshAccountDisplay();
            Assert.That(_controller.txtHomeGold.text, Is.EqualTo(UiStrings.Get(UiStrings.LoadingData)));

            _controller.RefreshProfileUI();
            Assert.That(_controller.txtProfileGold.text, Does.Contain(UiStrings.Get(UiStrings.LoadingData)));

            _controller.RefreshLeaderboardUI();
            Assert.That(row0Name.text, Is.EqualTo(UiStrings.Get(UiStrings.NoData)));
            var row0Score = _controller.dialogLeaderboard.transform.GetChild(0).Find("Row0/Score").GetComponent<TextMeshProUGUI>();
            Assert.That(row0Score.text, Is.EqualTo("-"));
        }

        [Test]
        public void Test_L1_L6_OnlineMode_BuyWeaponWaitsForServer()
        {
            NetConfig.Enabled = true;
            _controller.IsOnlineAccountLoaded = false;

            int initialGold = FakeAccountService.Instance.Gold;
            _controller.OnBuyWeapon("dep_to_ong_bac");

            // In online mode with pending server data, gold must NOT be deducted locally
            Assert.That(FakeAccountService.Instance.Gold, Is.EqualTo(initialGold));
            Assert.That(ItemCatalog.IsOwned("dep_to_ong_bac"), Is.False);
        }
    }
}
