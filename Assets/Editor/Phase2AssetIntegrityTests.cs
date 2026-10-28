using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HitMe.Editor
{
    public class Phase2AssetIntegrityTests
    {
        [Test]
        public void PrefabIntegrity_NoMissingScriptsOrSpritesInPrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Prefabs", "Assets/HITME_AimKit/Generated" });
            Assert.That(guids.Length, Is.GreaterThan(0), "Prefabs should exist in project");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, $"Failed to load prefab at {path}");

                // Check for missing MonoBehaviour scripts
                var components = prefab.GetComponentsInChildren<Component>(true);
                foreach (var comp in components)
                {
                    Assert.That(comp, Is.Not.Null, $"Prefab at {path} contains a Missing (null) Component!");
                }

                // Check SpriteRenderers have valid sprites
                var spriteRenderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var sr in spriteRenderers)
                {
                    // Dynamic runtime slots (e.g. Weapon_Preview_RuntimeSprite) are populated dynamically by AimController
                    if (sr.gameObject.name != "Trail" && sr.gameObject.name != "AimBeam" && !sr.gameObject.name.Contains("Runtime"))
                    {
                        Assert.That(sr.sprite, Is.Not.Null, $"SpriteRenderer on {sr.gameObject.name} in {path} has missing sprite!");
                    }
                }
            }
        }
    }
}
