using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class HitMeUIPackageImporter : AssetPostprocessor
{
    private static readonly Dictionary<string, Vector4> SliceBorders = new Dictionary<string, Vector4>
    {
        // Vector4(left, bottom, right, top)
        { "ui_btn_cta_yellow", new Vector4(68, 28, 68, 28) },
        { "ui_btn_cta_blue", new Vector4(60, 24, 60, 24) },
        { "ui_btn_cta_green", new Vector4(60, 24, 60, 24) },
        { "ui_btn_cta_red", new Vector4(60, 24, 60, 24) },
        { "ui_panel_dark_gold", new Vector4(44, 44, 44, 44) },
        { "ui_card_slot_normal", new Vector4(36, 36, 36, 36) },
        { "ui_card_slot_selected_cyan", new Vector4(36, 36, 36, 36) },
        { "ui_hud_pill_dark", new Vector4(42, 20, 42, 20) },
        { "ui_chat_row_normal", new Vector4(28, 20, 28, 20) },
        { "ui_chat_row_selected", new Vector4(28, 20, 28, 20) },
        { "ui_speech_bubble", new Vector4(32, 36, 32, 28) }
    };

    void OnPreprocessTexture()
    {
        if (!assetPath.Contains("HITME") && !assetPath.Contains("UI_Elements_PNG")) return;

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.spritePixelsPerUnit = 100f;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;

        foreach (var kv in SliceBorders)
        {
            if (assetPath.Contains(kv.Key))
            {
                settings.spriteBorder = kv.Value;
                break;
            }
        }
        importer.SetTextureSettings(settings);
    }
}
