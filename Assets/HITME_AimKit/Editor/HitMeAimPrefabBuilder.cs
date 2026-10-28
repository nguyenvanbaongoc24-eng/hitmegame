using UnityEditor;
using UnityEngine;
using HitMe.AimKit;

namespace HitMe.AimKit.Editor
{
    public static class HitMeAimPrefabBuilder
    {
        private const string Root = "Assets/HITME_AimKit";
        private const string Art = Root + "/Art/";
        private const string Output = Root + "/Generated/HITME_AimVisual.prefab";

        [MenuItem("Tools/HIT ME/Generate Aim Visual Prefab")]
        public static void Generate()
        {
            string[] sprites = {
                "Aim_Origin_Ring", "Aim_Origin_Glow", "Aim_Dot_Bright", "Aim_Dot_Soft", "Aim_Dot_Tiny",
                "Aim_Fan_Soft", "Aim_Beam_Soft", "Aim_Target_Halo", "Aim_Target_Reticle",
                "Aim_Weapon_Preview_Halo", "Aim_Direction_Arrow", "Aim_Target_Locked", "Aim_Target_Invalid", "Aim_Trail_Spark"
            };
            foreach (string spriteName in sprites)
            {
                string path = Art + spriteName + ".png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    Debug.LogError("[HIT ME Aim] Missing PNG: " + path);
                    return;
                }
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.sRGBTexture = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.spritePixelsPerUnit = 256f;
                importer.maxTextureSize = 1024;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spritePixelsPerUnit = 256f;
                if (spriteName == "Aim_Fan_Soft" || spriteName == "Aim_Beam_Soft")
                {
                    settings.spriteAlignment = (int)SpriteAlignment.Custom;
                    settings.spritePivot = new Vector2(0f, 0.5f);
                }
                else
                {
                    settings.spriteAlignment = (int)SpriteAlignment.Center;
                    settings.spritePivot = new Vector2(0.5f, 0.5f);
                }
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
            if (!AssetDatabase.IsValidFolder(Root + "/Generated"))
                AssetDatabase.CreateFolder(Root, "Generated");

            GameObject root = new GameObject("HITME_AimVisual");
            try
            {
                var kit = root.AddComponent<HitMeAimVisual2D>();
                kit.originGlowRenderer = Add(root, "Origin_Glow", "Aim_Origin_Glow", 100);
                kit.originRingRenderer = Add(root, "Origin_Ring", "Aim_Origin_Ring", 101);
                kit.fanRenderer = Add(root, "Trail_Fan", "Aim_Fan_Soft", 102);
                kit.beamRenderer = Add(root, "Trail_Beam", "Aim_Beam_Soft", 103);
                kit.targetHaloRenderer = Add(root, "Target_Glow", "Aim_Target_Halo", 105);
                kit.targetReticleRenderer = Add(root, "Target_Reticle", "Aim_Target_Reticle", 110);
                kit.weaponHaloRenderer = Add(root, "Weapon_Preview_Halo", "Aim_Weapon_Preview_Halo", 111);
                kit.weaponRenderer = Add(root, "Weapon_Preview_RuntimeSprite", null, 112);
                kit.arrowRenderer = Add(root, "Direction_Arrow", "Aim_Direction_Arrow", 109);
                kit.lockedRenderer = Add(root, "Locked_State", "Aim_Target_Locked", 113);
                kit.invalidRenderer = Add(root, "Invalid_State", "Aim_Target_Invalid", 113);
                kit.dotBright = Load("Aim_Dot_Bright");
                kit.dotSoft = Load("Aim_Dot_Soft");
                kit.dotTiny = Load("Aim_Dot_Tiny");
                PrefabUtility.SaveAsPrefabAsset(root, Output);
                AssetDatabase.SaveAssets();
                Debug.Log("[HIT ME Aim] Generated: " + Output);
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("HIT ME Aim Kit", "Generated reusable prefab:\n" + Output + "\nDrag it into MainMatch and bind the authoritative aim events.", "OK");
                }
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static Sprite Load(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png");
        private static SpriteRenderer Add(GameObject parent, string name, string spriteName, int sortingOrder)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            var sprite = child.AddComponent<SpriteRenderer>();
            sprite.sprite = spriteName == null ? null : Load(spriteName);
            sprite.sortingOrder = sortingOrder;
            sprite.enabled = false;
            return sprite;
        }
    }
}
