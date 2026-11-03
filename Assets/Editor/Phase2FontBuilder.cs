#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class Phase2FontBuilder
{
    private const string VietnameseGlyphs =
        " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" +
        "ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚÝàáâãèéêìíòóôõùúýĂăĐđĨĩŨũƠơƯưẠạẢảẤấẦầẨẩẪẫẬậẮắẰằẲẳẴẵẶặẸẹẺẻẼẽẾếỀềỂểỄễỆệỈỉỊịỌọỎỏỐốỒồỔổỖỗỘộỚớỜờỞởỠỡỢợỤụỦủỨứỪừỬửỮữỰựỲỳỴỵỶỷỸỹ" +
        "–—‘’“”•…₫";

    [MenuItem("Tools/HIT ME/Build Vietnamese Font Asset")]
    public static void BuildFontAsset()
    {
        string fontPath = "Assets/Fonts/BeVietnamPro-Bold.ttf";
        Font font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        if (font == null)
        {
            fontPath = "Assets/Fonts/HitMeFont.ttf";
            font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        }
        if (font == null)
        {
            Debug.LogError("Cannot load font at Assets/Fonts/BeVietnamPro-Bold.ttf or HitMeFont.ttf");
            return;
        }

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            font,
            36,
            5,
            GlyphRenderMode.SDFAA,
            1024,
            1024,
            AtlasPopulationMode.Dynamic,
            true);

        if (fontAsset == null)
        {
            Debug.LogError("Failed to create TMP_FontAsset");
            return;
        }

        string glyphFilePath = "Assets/Editor/all_glyphs.txt";
        string glyphsToLoad = VietnameseGlyphs;
        if (File.Exists(glyphFilePath))
        {
            glyphsToLoad = File.ReadAllText(glyphFilePath, System.Text.Encoding.UTF8);
            foreach (char c in VietnameseGlyphs)
            {
                if (!glyphsToLoad.Contains(c)) glyphsToLoad += c;
            }
        }

        string missing;
        bool success = fontAsset.TryAddCharacters(glyphsToLoad, out missing);
        Debug.Log("Add Vietnamese Glyphs success: " + success + ", missing: " + (string.IsNullOrEmpty(missing) ? "None" : missing));

        // Save as Static mode so all added glyphs are serialized into the asset on disk
        fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;

        var fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts & Materials/LiberationSans SDF.asset");
        if (fallback != null)
        {
            if (fontAsset.fallbackFontAssetTable == null)
            {
                fontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>();
            }
            if (!fontAsset.fallbackFontAssetTable.Contains(fallback))
            {
                fontAsset.fallbackFontAssetTable.Add(fallback);
            }
        }

        string targetPath = "Assets/Fonts/BeVietnamPro-Bold SDF.asset";
        AssetDatabase.CreateAsset(fontAsset, targetPath);

        if (fontAsset.atlasTextures != null)
        {
            for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
            {
                var tex = fontAsset.atlasTextures[i];
                if (tex != null)
                {
                    tex.name = fontAsset.name + " Atlas " + i;
                    AssetDatabase.AddObjectToAsset(tex, fontAsset);
                }
            }
        }
        else if (fontAsset.atlasTexture != null)
        {
            fontAsset.atlasTexture.name = fontAsset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
        }

        if (fontAsset.material != null)
        {
            fontAsset.material.name = fontAsset.name + " Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Successfully created Static Atlas Vietnamese Font Asset at: " + targetPath);
    }
}
#endif
