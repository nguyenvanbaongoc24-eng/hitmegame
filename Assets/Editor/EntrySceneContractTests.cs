using System.Linq;
using HitMe.Core;
using HitMe.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HitMe.Editor
{
    public sealed class EntrySceneContractTests
    {
        [Test]
        public void BuildSettingsExactlyMatchProductionSceneList()
        {
            var enabled = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            CollectionAssert.AreEqual(BuildScenes.Production, enabled);
            Assert.That(enabled.Distinct().Count(), Is.EqualTo(enabled.Length));
        }

        [Test]
        public void EntrySceneUsesCorePhase1ControllerOnly()
        {
            EditorSceneManager.OpenScene(BuildScenes.Production[0], OpenSceneMode.Single);
            Assert.That(Object.FindFirstObjectByType<MatchController>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<MatchStateMachine>(), Is.Null);
        }

        [Test]
        public void NoBuildSceneReferencesLegacyMatchStateMachine()
        {
            foreach (var path in BuildScenes.Production)
            {
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                Assert.That(Object.FindFirstObjectByType<MatchStateMachine>(), Is.Null, path);
            }
        }
    }
}
