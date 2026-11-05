using HitMe.UI;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HitMe.Editor
{
    public sealed class ProductionHomeNavigationTests
    {
        [Test]
        public void HomeTabsHaveDistinctRuntimeBindingsAfterSceneBuild()
        {
            ProductionUISetup.BuildProductionUI();
            EditorSceneManager.OpenScene("Assets/Scenes/Graybox.unity");
            var ui = Object.FindFirstObjectByType<ProductionUIController>();

            Assert.That(ui, Is.Not.Null);
            Assert.That(ui.btnHomeTab, Is.Not.Null);
            Assert.That(ui.btnHomeTab.name, Is.EqualTo("BtnTabHome"));
            Assert.That(ui.btnHomeLeaderboard, Is.Not.Null);
            Assert.That(ui.btnHomeLeaderboard.name, Is.EqualTo("BtnTabRank"));
            Assert.That(ui.btnHomeQuests, Is.Not.Null);
            Assert.That(ui.btnHomeQuests.name, Is.EqualTo("BtnTabQuest"));
            Assert.That(ui.btnHomeLogin, Is.Not.Null);
            Assert.That(ui.btnHomeLogin.name, Is.EqualTo("BtnLogin"));
            Assert.That(ui.btnHomeLogin, Is.Not.SameAs(ui.btnHomeTab));
        }
    }
}
