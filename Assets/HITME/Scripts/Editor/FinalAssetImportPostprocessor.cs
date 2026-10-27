using UnityEditor;
using UnityEngine;

namespace HitMe.Editor
{
    public sealed class FinalAssetImportPostprocessor : AssetPostprocessor
    {
        private const string Root = "Assets/HITME/Art/";
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Root)) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.spritePixelsPerUnit = assetPath.Contains("Characters/NamLang") ? 100f : 50f;
            importer.spritePivot = assetPath.Contains("Characters/NamLang") ? new Vector2(.5f, 0f) : new Vector2(.5f, .5f);
        }
    }
}
