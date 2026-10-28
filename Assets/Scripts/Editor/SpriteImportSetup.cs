using UnityEditor;
using UnityEngine;
using System.IO;

namespace HitMe.Editor
{
    public class SpriteImportSetup : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetPath.StartsWith("Assets/Sprites/") || assetPath.StartsWith("Assets/Art/"))
            {
                TextureImporter importer = (TextureImporter)assetImporter;
                ApplySpriteSettings(importer, assetPath);
            }
        }

        [MenuItem("Tools/HIT ME/Configure All Sprites")]
        public static void ConfigureAllSprites()
        {
            var searchDirs = new[] { "Assets/Sprites", "Assets/Art/VFX", "Assets/Art/UI", "Assets/Art/Characters" };
            foreach (var dir in searchDirs)
            {
                if (!Directory.Exists(dir)) continue;
                string[] files = Directory.GetFiles(dir, "*.png", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string path = file.Replace("\\", "/");
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null)
                    {
                        ApplySpriteSettings(importer, path);
                        importer.SaveAndReimport();
                    }
                }
            }
            AssetDatabase.Refresh();
            Debug.Log("Configured all sprites across Assets/Sprites and Assets/Art to Sprite (2D and UI).");
        }

        private static void ApplySpriteSettings(TextureImporter importer, string path)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 150f;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spritePixelsPerUnit = 150f;

            string fileName = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (fileName.Contains("idle") || fileName.Contains("move") || fileName.Contains("aim") ||
                fileName.Contains("throw") || fileName.Contains("hit") || fileName.Contains("victory") ||
                fileName.Contains("defeat") || fileName.Contains("cau_be") || fileName.Contains("anh_") ||
                fileName.Contains("mai_hong") || fileName.Contains("chu_cho"))
            {
                settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                settings.spritePivot = new Vector2(0.5f, 0.0f);
            }
            else
            {
                settings.spriteAlignment = (int)SpriteAlignment.Center;
                settings.spritePivot = new Vector2(0.5f, 0.5f);
            }

            importer.SetTextureSettings(settings);
        }
    }
}
