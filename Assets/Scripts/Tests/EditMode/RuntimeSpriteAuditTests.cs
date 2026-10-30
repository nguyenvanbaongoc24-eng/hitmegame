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
