using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HitMe.UI;
using HitMe.Game;

namespace HitMe.Tests
{
    [TestFixture]
    public sealed class Phase4Luot4FeaturesTests
    {
        [SetUp]
        public void SetUp()
        {
            RingBufferLogger.Clear();
            PlayerPrefs.DeleteKey("HitMe_SelectedMapId");
        }

        [TearDown]
        public void TearDown()
        {
            RingBufferLogger.Clear();
            PlayerPrefs.DeleteKey("HitMe_SelectedMapId");
            UiStrings.SetLanguage(AppLanguage.Vietnamese);
        }

        [Test]
        public void RingBufferLogger_MaxFiftyEntries_FIFO()
        {
            for (int i = 1; i <= 60; i++)
            {
                RingBufferLogger.Log("INFO", $"Log message sequence {i}");
            }

            var logs = RingBufferLogger.GetRecentLogs();
            Assert.That(logs.Count, Is.EqualTo(50));
            // First entry should be sequence 11 (1-10 evicted)
            Assert.That(logs[0], Does.Contain("sequence 11"));
            // Last entry should be sequence 60
            Assert.That(logs[49], Does.Contain("sequence 60"));
        }

        [Test]
        public void RingBufferLogger_ZeroTokenLeak_SanitizesSensitiveData()
        {
            string rawMsg1 = "Connected to room TRGGWP with token: secret_token_xyz123456";
            string rawMsg2 = "Auth bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9";
            string rawMsg3 = "User guest:AfkGuest_9921 authenticated with password=SuperSecretPassword123";
            string rawMsg4 = "Private reconnectToken: MVfpQXsIVCIVCq7fGYeKOK3Wp7u2Bjve13xgf_wOQac";

            RingBufferLogger.Log("INFO", rawMsg1);
            RingBufferLogger.Log("INFO", rawMsg2);
            RingBufferLogger.Log("WARN", rawMsg3);
            RingBufferLogger.Log("INFO", rawMsg4);

            var logs = RingBufferLogger.GetRecentLogs();
            string combined = string.Join("\n", logs);

            // Assert secrets are NOT present
            Assert.That(combined, Does.Not.Contain("secret_token_xyz123456"));
            Assert.That(combined, Does.Not.Contain("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9"));
            Assert.That(combined, Does.Not.Contain("SuperSecretPassword123"));
            Assert.That(combined, Does.Not.Contain("MVfpQXsIVCIVCq7fGYeKOK3Wp7u2Bjve13xgf_wOQac"));
            Assert.That(combined, Does.Not.Contain("guest:AfkGuest_9921"));

            // Assert redacted markers exist
            Assert.That(combined, Does.Contain("[REDACTED]"));
        }

        [Test]
        public void RingBufferLogger_BuildBugReport_FormatVerification()
        {
            RingBufferLogger.Log("INFO", "Initialized UI");
            RingBufferLogger.Log("ERROR", "Connection dropped temporarily");

            string report = RingBufferLogger.BuildBugReport("db4e8f1", "Vietnamese", "Gameplay", "ROOM42", "Windows 11");

            Assert.That(report, Does.Contain("### HIT ME Bug Report"));
            Assert.That(report, Does.Contain("db4e8f1"));
            Assert.That(report, Does.Contain("Vietnamese"));
            Assert.That(report, Does.Contain("Gameplay"));
            Assert.That(report, Does.Contain("ROOM42"));
            Assert.That(report, Does.Contain("Windows 11"));
            Assert.That(report, Does.Contain("Initialized UI"));
            Assert.That(report, Does.Contain("Connection dropped temporarily"));
        }

        [Test]
        public void SettingsDialog_HasReportBugButton_AndBuildVersion()
        {
            var go = new GameObject("TestUI_BugReport");
            var ui = go.AddComponent<ProductionUIController>();

            var dialogSettingsObj = new GameObject("DialogSettings");
            dialogSettingsObj.transform.SetParent(go.transform, false);
            ui.dialogSettings = dialogSettingsObj;

            var toastObj = new GameObject("ToastObj");
            toastObj.transform.SetParent(go.transform, false);
            ui.toastObject = toastObj;
            ui.txtToast = toastObj.AddComponent<TextMeshProUGUI>();

            ui.ShowSettingsDialog();

            Assert.That(ui.btnReportBug, Is.Not.Null);
            Assert.That(ui.txtSettingsBuildVersion, Is.Not.Null);
            Assert.That(ui.txtSettingsBuildVersion.text, Does.Contain("build "));

            // Click report bug
            ui.OnReportBugClicked();

            // Toast displayed
            Assert.IsTrue(ui.txtToast.text.IndexOf("clipboard", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                          ui.txtToast.text.IndexOf("sao chép", System.StringComparison.OrdinalIgnoreCase) >= 0);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void MapManager_LoadsDataAndCyclesMaps()
        {
            var go = new GameObject("TestMapManager");
            var mapMgr = go.AddComponent<MapManager>();
            mapMgr.LoadMaps();

            Assert.That(mapMgr.Maps.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(mapMgr.Maps[0].id, Is.EqualTo("map_street_vietnam"));
            Assert.That(mapMgr.Maps[1].id, Is.EqualTo("map_village_lotus"));

            // Bounds must remain exactly 9x11.4
            Assert.That(mapMgr.Maps[0].arenaWidth, Is.EqualTo(9.0f));
            Assert.That(mapMgr.Maps[0].arenaHeight, Is.EqualTo(11.4f));
            Assert.That(mapMgr.Maps[1].arenaWidth, Is.EqualTo(9.0f));
            Assert.That(mapMgr.Maps[1].arenaHeight, Is.EqualTo(11.4f));

            // Cycle
            var next = mapMgr.CycleNextMap();
            Assert.That(next.id, Is.EqualTo("map_village_lotus"));
            Assert.That(PlayerPrefs.GetString("HitMe_SelectedMapId", ""), Is.EqualTo("map_village_lotus"));

            // Cycle back
            var back = mapMgr.CycleNextMap();
            Assert.That(back.id, Is.EqualTo("map_street_vietnam"));

            Object.DestroyImmediate(go);
        }
    }
}
