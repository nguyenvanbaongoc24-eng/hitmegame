using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HitMe.Editor
{
    /// <summary>
    /// Automatic 9-Slice and Texture Importer for HIT ME VN UI Kit.
    /// Configures SpriteMode = Single, MeshType = FullRect, AlphaIsTransparency = true,
    /// and pre-configures exact 9-slice borders (Left, Bottom, Right, Top) for all UI elements.
    /// </summary>
    public class UIKit9SliceImporter : AssetPostprocessor
    {
        private const string UIKitPath = "Assets/HITME/Art/UI/Kit";

        // Dictionary defining 9-slice borders: Vector4(left, bottom, right, top) in pixels
        private static readonly Dictionary<string, Vector4> SpriteBorders = new Dictionary<string, Vector4>(StringComparer.OrdinalIgnoreCase)
        {
            // 1. Headers & Banners
            { "ui_banner_wood.png", new Vector4(60f, 30f, 60f, 30f) },

            // 2. Chunky 3D CTA Buttons (512x128 & 360x110)
            { "ui_btn_cta_yellow.png", new Vector4(64f, 32f, 64f, 32f) },
            { "ui_btn_cta_red.png", new Vector4(64f, 32f, 64f, 32f) },
            { "ui_btn_cta_blue.png", new Vector4(54f, 28f, 54f, 28f) },
            { "ui_btn_cta_green.png", new Vector4(54f, 28f, 54f, 28f) },

            // 3. 9-Slice Panel (512x512)
            { "ui_panel_dark_gold.png", new Vector4(44f, 44f, 44f, 44f) },

            // 4. Character / Weapon Cards (256x320)
            { "ui_card_char_normal.png", new Vector4(36f, 36f, 36f, 36f) },
            { "ui_card_char_selected.png", new Vector4(44f, 44f, 44f, 44f) },

            // 5. Input Field Box (480x100)
            { "ui_input_box_cyan.png", new Vector4(32f, 24f, 32f, 24f) },

            // 6. Leaderboard Rows (600x96)
            { "ui_row_leaderboard.png", new Vector4(32f, 24f, 32f, 24f) },
            { "ui_row_leaderboard_me.png", new Vector4(36f, 26f, 36f, 26f) },

            // 7. Toggle Switches (160x80)
            { "ui_toggle_on.png", new Vector4(36f, 36f, 36f, 36f) },
            { "ui_toggle_off.png", new Vector4(36f, 36f, 36f, 36f) },

            // 8. Sliders
            { "ui_slider_track.png", new Vector4(20f, 10f, 20f, 10f) },
            { "ui_slider_track_empty.png", new Vector4(16f, 8f, 16f, 8f) },
            { "ui_slider_track_fill.png", new Vector4(16f, 8f, 16f, 8f) },
            { "ui_slider_knob.png", Vector4.zero },

            // 9. Circle Back Button (128x128)
            { "ui_btn_circle_back.png", Vector4.zero }
        };

        [MenuItem("HIT ME/UI Kit/Configure 9-Slice Sprites")]
        public static void ConfigureAllUIKitSprites()
        {
            Debug.Log($"[UIKit9SliceImporter] Scanning directory: {UIKitPath}...");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { UIKitPath });
            int configuredCount = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var filename = Path.GetFileName(path);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer == null) continue;

                bool dirty = false;

                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    dirty = true;
                }

                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    dirty = true;
                }

                var tis = new TextureImporterSettings();
                importer.ReadTextureSettings(tis);
                if (tis.spriteMeshType != SpriteMeshType.FullRect)
                {
                    tis.spriteMeshType = SpriteMeshType.FullRect;
                    importer.SetTextureSettings(tis);
                    dirty = true;
                }

                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    dirty = true;
                }

                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    dirty = true;
                }

                if (importer.filterMode != FilterMode.Bilinear)
                {
                    importer.filterMode = FilterMode.Bilinear;
                    dirty = true;
                }

                // Apply predefined 9-slice border if registered
                if (SpriteBorders.TryGetValue(filename, out var border))
                {
                    if (importer.spriteBorder != border)
                    {
                        importer.spriteBorder = border;
                        dirty = true;
                        Debug.Log($"[UIKit9SliceImporter] Configured 9-Slice for {filename}: Border = {border}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[UIKit9SliceImporter] No custom border registered for {filename}, keeping default.");
                }

                if (dirty)
                {
                    importer.SaveAndReimport();
                    configuredCount++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[UIKit9SliceImporter] Successfully configured {configuredCount} / {guids.Length} UI Kit Sprites with 9-slicing!");
        }

        // Automatic post-processing when new sprites are dropped into Assets/HITME/Art/UI/Kit
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(UIKitPath, StringComparison.OrdinalIgnoreCase)) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;

            var tis = new TextureImporterSettings();
            importer.ReadTextureSettings(tis);
            tis.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(tis);

            var filename = Path.GetFileName(assetPath);
            if (SpriteBorders.TryGetValue(filename, out var border))
            {
                importer.spriteBorder = border;
            }
        }
    }
}
