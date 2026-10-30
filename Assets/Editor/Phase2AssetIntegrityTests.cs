using System;
using System.Reflection;
using HitMe.UI;
using NUnit.Framework;
using TMPro;
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

        [Test]
        public void UiStrings_AllConstantsNonEmpty()
        {
            FieldInfo[] fields = typeof(UiStrings).GetFields(BindingFlags.Public | BindingFlags.Static);
            Assert.That(fields.Length, Is.GreaterThan(10), "UiStrings must define string constants");

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string))
                {
                    string val = (string)field.GetValue(null);
                    Assert.That(string.IsNullOrEmpty(val), Is.False, $"UiStrings.{field.Name} is null or empty");
                }
            }
        }

        [Test]
        public void UiStrings_AllGlyphsSupported_InVietnameseFont()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Bold SDF.asset");
            Assert.That(font, Is.Not.Null, "BeVietnamPro-Bold SDF font asset must exist");

            // Verify the specific test glyph check string: "Ế Ổ Ữ Ặ Đ Ằ Ẳ Ố Ậ Ứ Ớ Ệ Ồ Ẵ"
            string glyphCheck = UiStrings.GlyphCheckString;
            foreach (char ch in glyphCheck)
            {
                if (char.IsWhiteSpace(ch)) continue;
                Assert.That(font.HasCharacter(ch, true), Is.True, $"Font missing Vietnamese glyph '{ch}' (U+{(int)ch:X4}) from GlyphCheckString");
            }

            // Verify all characters across all UiStrings constants
            FieldInfo[] fields = typeof(UiStrings).GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string))
                {
                    string str = (string)field.GetValue(null);
                    foreach (char ch in str)
                    {
                        if (char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || char.IsSymbol(ch) || char.IsDigit(ch)) continue;
                        Assert.That(font.HasCharacter(ch, true), Is.True, $"Font missing glyph '{ch}' (U+{(int)ch:X4}) used in UiStrings.{field.Name}");
                    }
                }
            }
        }

        [Test]
        public void UiTheme_AssetIntegrity_AllColorsAndSpritesValid()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UiTheme>("Assets/Art/UI/UiTheme.asset");
            if (theme == null)
            {
                UiThemeBuilder.BuildTheme();
                theme = AssetDatabase.LoadAssetAtPath<UiTheme>("Assets/Art/UI/UiTheme.asset");
            }
            Assert.That(theme, Is.Not.Null, "UiTheme.asset must exist");
            Assert.That(theme.defaultFont, Is.Not.Null, "UiTheme default font must not be null");
            Assert.That(theme.bannerWood, Is.Not.Null, "bannerWood sprite missing");
            Assert.That(theme.btnCtaYellow, Is.Not.Null, "btnCtaYellow sprite missing");
            Assert.That(theme.btnCtaBlue, Is.Not.Null, "btnCtaBlue sprite missing");
            Assert.That(theme.cardCharNormal, Is.Not.Null, "cardCharNormal sprite missing");
            Assert.That(theme.heartFull, Is.Not.Null, "heartFull sprite missing");
            Assert.That(theme.heartBreak, Is.Not.Null, "heartBreak sprite missing");
            Assert.That(theme.weaponSandal, Is.Not.Null, "weaponSandal sprite missing");
        }
    }
}
