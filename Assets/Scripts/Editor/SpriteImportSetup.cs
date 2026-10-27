using UnityEditor;
using UnityEngine;
using System.IO;

namespace HitMe.Editor
{
    public class SpriteImportSetup : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetPath.StartsWith("Assets/Sprites/"))
            {
                TextureImporter importer = (TextureImporter)assetImporter;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
            }
        }

        [MenuItem("Tools/HIT ME/Configure All Sprites")]
        public static void ConfigureAllSprites()
        {
            string[] files = Directory.GetFiles("Assets/Sprites", "*.png");
            foreach (string file in files)
            {
                string path = file.Replace("\\", "/");
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.mipmapEnabled = false;
                    importer.alphaIsTransparency = true;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }
            }
            AssetDatabase.Refresh();
            Debug.Log("Configured all sprites in Assets/Sprites to Sprite (2D and UI).");
        }
    }
}
