#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public static class Phase2SpriteAtlasBuilder
{
    [MenuItem("Tools/HIT ME/Build Sprite Atlases")]
    public static void BuildAtlases()
    {
        string atlasDir = "Assets/Art/Atlases";
        if (!Directory.Exists(atlasDir))
        {
            Directory.CreateDirectory(atlasDir);
            AssetDatabase.Refresh();
        }

        CreateAtlas("Characters_Atlas", new[] { "Assets/Art/Characters", "Assets/Sprites" }, "Assets/Art/Atlases/Characters_Atlas.spriteatlas");
        CreateAtlas("UI_Atlas", new[] { "Assets/Art/UI" }, "Assets/Art/Atlases/UI_Atlas.spriteatlas");
        CreateAtlas("VFX_Atlas", new[] { "Assets/Art/VFX" }, "Assets/Art/Atlases/VFX_Atlas.spriteatlas");
        CreateAtlas("Env_Atlas", new[] { "Assets/Art/Arena" }, "Assets/Art/Atlases/Env_Atlas.spriteatlas");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Successfully created 4 Sprite Atlases in Assets/Art/Atlases/");
    }

    private static void CreateAtlas(string name, string[] targetFolderPaths, string savePath)
    {
        var atlas = new SpriteAtlas();
        atlas.name = name;

        var packSettings = new SpriteAtlasPackingSettings
        {
            blockOffset = 1,
            enableRotation = false,
            enableTightPacking = false,
            padding = 4
        };
        atlas.SetPackingSettings(packSettings);

        var textureSettings = new SpriteAtlasTextureSettings
        {
            readable = false,
            generateMipMaps = false,
            sRGB = true,
            filterMode = FilterMode.Bilinear
        };
        atlas.SetTextureSettings(textureSettings);

        var androidSettings = new TextureImporterPlatformSettings
        {
            name = "Android",
            overridden = true,
            maxTextureSize = 2048,
            format = TextureImporterFormat.ASTC_6x6,
            textureCompression = TextureImporterCompression.Compressed
        };
        atlas.SetPlatformSettings(androidSettings);

        var webglSettings = new TextureImporterPlatformSettings
        {
            name = "WebGL",
            overridden = true,
            maxTextureSize = 2048,
            format = TextureImporterFormat.Automatic,
            textureCompression = TextureImporterCompression.Compressed
        };
        atlas.SetPlatformSettings(webglSettings);

        var objectsToAdd = new System.Collections.Generic.List<Object>();
        foreach (var folder in targetFolderPaths)
        {
            var folderObj = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folder);
            if (folderObj != null)
            {
                objectsToAdd.Add(folderObj);
            }
        }

        if (objectsToAdd.Count > 0)
        {
            atlas.Add(objectsToAdd.ToArray());
        }

        AssetDatabase.CreateAsset(atlas, savePath);
        Debug.Log("Created Sprite Atlas: " + savePath);
    }
}
#endif
