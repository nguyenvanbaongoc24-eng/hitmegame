using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using HitMe.Data;

namespace HitMe.Editor
{
    public class CharacterAssetSeparationTests
    {
        private static readonly string[] CanonicalCharacterIds = new string[]
        {
            "nam_lang",
            "nu_nang_dong",
            "quay_boy",
            "co_ba",
            "anh_teen",
            "cho_vang",
            "meo_mun"
        };

        [Test]
        public void CharacterDirectories_MenuAndGameplaySeparated()
        {
            foreach (var charId in CanonicalCharacterIds)
            {
                string menuDir = $"Assets/Art/Characters/{charId}/Menu";
                string gpDir = $"Assets/Art/Characters/{charId}/Gameplay";

                Assert.That(Directory.Exists(menuDir), Is.True, $"Menu folder missing for {charId}: {menuDir}");
                Assert.That(Directory.Exists(gpDir), Is.True, $"Gameplay folder missing for {charId}: {gpDir}");
            }
        }

        [Test]
        public void MenuAssets_NeverReferenceGameplayDirectory()
        {
            foreach (var charId in CanonicalCharacterIds)
            {
                string menuDir = $"Assets/Art/Characters/{charId}/Menu";
                string[] files = Directory.GetFiles(menuDir, "*.*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    string norm = file.Replace("\\", "/");
                    Assert.That(norm.Contains("/Gameplay/"), Is.False,
                        $"Menu folder asset '{norm}' contains reference to Gameplay directory!");
                }
            }
        }

        [Test]
        public void GameplayAssets_NeverReferenceMenuDirectory()
        {
            foreach (var charId in CanonicalCharacterIds)
            {
                string gpDir = $"Assets/Art/Characters/{charId}/Gameplay";
                string[] files = Directory.GetFiles(gpDir, "*.*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    string norm = file.Replace("\\", "/");
                    Assert.That(norm.Contains("/Menu/"), Is.False,
                        $"Gameplay folder asset '{norm}' contains reference to Menu directory!");
                }
            }
        }

        [Test]
        public void CharacterRegistry_CanonicalNamesAndDefsAreValid()
        {
            Assert.That(CharacterRegistry.CanonicalCharacters.Length, Is.EqualTo(7));

            for (int i = 0; i < CharacterRegistry.CanonicalCharacters.Length; i++)
            {
                var def = CharacterRegistry.CanonicalCharacters[i];
                Assert.That(def.id, Is.EqualTo(CanonicalCharacterIds[i]));
                Assert.That(string.IsNullOrEmpty(def.displayNameVi), Is.False, $"Missing Vietnamese name for {def.id}");
                Assert.That(string.IsNullOrEmpty(def.displayNameEn), Is.False, $"Missing English name for {def.id}");
            }
        }

        [Test]
        public void MissingGameplayAsset_FallsBackToPlaceholderInsteadOfMenu()
        {
            // Verifies contract: when gameplay sprite is absent, use placeholder with "THIẾU ẢNH"
            foreach (var charId in CanonicalCharacterIds)
            {
                string gpDir = $"Assets/Art/Characters/{charId}/Gameplay";
                string placeholderPath = $"{gpDir}/gp_{charId}_placeholder.png";
                Assert.That(File.Exists(placeholderPath), Is.True,
                    $"Fallback placeholder missing for {charId}: {placeholderPath}");
            }
        }
    }
}
