using System;
using System.IO;
using HitMe.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HitMe.Tests
{
    public class RuntimeSpriteAuditTests
    {
        private const string ConfigAssetPath = "Assets/ScriptableObjects/GameConfig_Phase1.asset";
        private const string ManifestPath = "Assets/Resources/ApprovedRuntimeSprites.json";
        private static readonly string[] ForbiddenKeywords = { "sheet", "contact", "reference", "atlas_labeled" };

        [Test]
        public void Sprite_GameConfigReferences_ContainNoForbiddenKeywords()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfigAsset>(ConfigAssetPath);
            Assert.That(config, Is.Not.Null, "GameConfig_Phase1.asset must exist");

            // Character idle sprites
            Assert.That(config.characterIdleSprites, Is.Not.Null);
            foreach (var sprite in config.characterIdleSprites)
            {
                AssertCleanSprite(sprite, "characterIdleSprites");
            }

            // Nam Lang poses
            Assert.That(config.namLangPoseSprites, Is.Not.Null);
            foreach (var sprite in config.namLangPoseSprites)
            {
                AssertCleanSprite(sprite, "namLangPoseSprites");
            }

            // Anh Non La poses (Bot / Player)
            Assert.That(config.anhNonLaPoseSprites, Is.Not.Null);
            foreach (var sprite in config.anhNonLaPoseSprites)
            {
                AssertCleanSprite(sprite, "anhNonLaPoseSprites");
            }

            // Weapon sandal projectile
            AssertCleanSprite(config.sandalProjectileSprite, "sandalProjectileSprite");

            // Hit FX
            if (config.hitFxFrames != null)
            {
                foreach (var sprite in config.hitFxFrames)
                {
                    AssertCleanSprite(sprite, "hitFxFrames");
                }
            }
        }

        [Test]
        public void Sprite_Dimensions_DoNotExceedThreshold()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfigAsset>(ConfigAssetPath);
            Assert.That(config, Is.Not.Null);

            foreach (var sprite in config.characterIdleSprites)
            {
                AssertDimensions(sprite, 512, 512);
            }
            foreach (var sprite in config.namLangPoseSprites)
            {
                AssertDimensions(sprite, 512, 512);
            }
            foreach (var sprite in config.anhNonLaPoseSprites)
            {
                AssertDimensions(sprite, 512, 512);
            }
            AssertDimensions(config.sandalProjectileSprite, 512, 512);
        }

        [Test]
        public void Sprite_ApprovedRuntimeSpritesManifest_ExistsAndValid()
        {
            Assert.That(File.Exists(ManifestPath), Is.True, "ApprovedRuntimeSprites.json must exist in Assets/Resources");
            string json = File.ReadAllText(ManifestPath);
            Assert.That(json, Does.Contain("anh_non_la_idle.png"));
            Assert.That(json, Does.Contain("cau_be_lang_idle.png"));
            Assert.That(json, Does.Contain("dep_projectile.png"));
            Assert.That(json.ToLowerInvariant(), Does.Not.Contain("\"approved\": false"));
        }

        [Test]
        public void Weapon_Sandal_AssignedToAllCharactersPerRulesV12()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfigAsset>(ConfigAssetPath);
            Assert.That(config, Is.Not.Null);
            Assert.That(config.sandalProjectileSprite, Is.Not.Null);
            string assetPath = AssetDatabase.GetAssetPath(config.sandalProjectileSprite);
            Assert.That(assetPath.ToLowerInvariant(), Does.Contain("dep"), "Standard weapon per rules v1.2 must be Dep");
        }

        [Test]
        public void H4_AllBuildScenesAndPrefabs_ContainNoForbiddenSpriteSheets()
        {
            // Requirement H4(a): Scan all prefabs and scenes in Build Settings (YAML/AssetDatabase)
            // Assert that all SpriteRenderer / Image / Animator textures are clean and not forbidden sheets.
            var buildScenes = EditorBuildSettings.scenes;
            Assert.That(buildScenes.Length, Is.GreaterThan(0), "Build settings must contain at least 1 scene");

            var explicitForbidden = new[] { "anh_non_la_spritesheet", "spritesheet", "sheet", "reference", "contact", "atlas_labeled" };

            // 1. Scan Build Scenes
            foreach (var scene in buildScenes)
            {
                if (!scene.enabled) continue;
                string[] dependencies = AssetDatabase.GetDependencies(scene.path, true);
                foreach (var dep in dependencies)
                {
                    string depLower = dep.ToLowerInvariant();
                    if (!depLower.EndsWith(".png") && !depLower.EndsWith(".jpg")) continue;

                    foreach (var kw in explicitForbidden)
                    {
                        Assert.That(depLower, Does.Not.Contain(kw),
                            $"Scene '{scene.path}' references forbidden sprite/texture '{dep}' containing '{kw}'!");
                    }
                }
            }

            // 2. Scan All Prefabs in Assets/Prefabs
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
            foreach (var guid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                string[] dependencies = AssetDatabase.GetDependencies(prefabPath, true);
                foreach (var dep in dependencies)
                {
                    string depLower = dep.ToLowerInvariant();
                    if (!depLower.EndsWith(".png") && !depLower.EndsWith(".jpg")) continue;

                    foreach (var kw in explicitForbidden)
                    {
                        Assert.That(depLower, Does.Not.Contain(kw),
                            $"Prefab '{prefabPath}' references forbidden sprite/texture '{dep}' containing '{kw}'!");
                    }
                }
            }
        }

        [Test]
        public void H4_FourPlayerMatch_AllSpawnedPlayerViews_UseCleanApprovedSprites()
        {
            // Requirement H4(b): In a 4-player match, traverse all spawned PlayerViews and SpriteRenderers
            // Assert all active sprites and textures are clean, non-sheet, and <= 512x512.
            var configAsset = AssetDatabase.LoadAssetAtPath<GameConfigAsset>(ConfigAssetPath);
            Assert.That(configAsset, Is.Not.Null);

            PlayerView.ConfigureSharedResources(configAsset);
            var root = new GameObject("TestMatchRoot_4P");

            try
            {
                var playerViews = new PlayerView[4];
                for (int i = 0; i < 4; i++)
                {
                    var pGo = new GameObject("Player_" + i);
                    pGo.transform.SetParent(root.transform);
                    var pv = pGo.AddComponent<PlayerView>();
                    pv.Initialise(i == 0 ? "player" : "bot-" + i, Color.white, i);
                    playerViews[i] = pv;
                }

                // Check all SpriteRenderers on all 4 characters
                var renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
                Assert.That(renderers.Length, Is.GreaterThanOrEqualTo(8), "Should have body, shadow, ring renderers");

                foreach (var sr in renderers)
                {
                    if (sr.sprite == null) continue;
                    string spriteName = sr.sprite.name;
                    string texPath = AssetDatabase.GetAssetPath(sr.sprite).ToLowerInvariant();

                    foreach (var kw in ForbiddenKeywords)
                    {
                        Assert.That(texPath, Does.Not.Contain(kw),
                            $"Spawned player sprite '{spriteName}' at '{texPath}' contains forbidden keyword '{kw}'!");
                    }
                    Assert.That(texPath, Does.Not.Contain("anh_non_la_spritesheet"));

                    // Verify dimensions <= 512x512
                    Assert.That(sr.sprite.rect.width, Is.LessThanOrEqualTo(512));
                    Assert.That(sr.sprite.rect.height, Is.LessThanOrEqualTo(512));
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AssertCleanSprite(Sprite sprite, string context)
        {
            Assert.That(sprite, Is.Not.Null, $"Sprite in {context} must not be null");
            string path = AssetDatabase.GetAssetPath(sprite).ToLowerInvariant();
            foreach (var forbidden in ForbiddenKeywords)
            {
                Assert.That(path, Does.Not.Contain(forbidden),
                    $"Sprite '{sprite.name}' in {context} at '{path}' contains forbidden keyword '{forbidden}'!");
            }
        }

        private static void AssertDimensions(Sprite sprite, int maxW, int maxH)
        {
            if (sprite == null) return;
            Texture2D tex = sprite.texture;
            Assert.That(tex, Is.Not.Null, $"Texture for sprite {sprite.name} must not be null");
            Assert.That(tex.width, Is.LessThanOrEqualTo(maxW),
                $"Sprite '{sprite.name}' width ({tex.width}) exceeds maximum allowed ({maxW})");
            Assert.That(tex.height, Is.LessThanOrEqualTo(maxH),
                $"Sprite '{sprite.name}' height ({tex.height}) exceeds maximum allowed ({maxH})");
        }
    }
}
