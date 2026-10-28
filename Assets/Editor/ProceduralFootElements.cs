#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HitMe.Editor
{
    public static class ProceduralFootElements
    {
        [MenuItem("Tools/HIT ME/Generate Procedural Foot Elements")]
        public static void GenerateAll()
        {
            GenerateShadow();
            GenerateFootRing();
            AssetDatabase.Refresh();
        }

        public static void GenerateShadow()
        {
            int w = 256;
            int h = 154; // Perspective aspect ratio ~ 0.60
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            float cx = (w - 1) * 0.5f;
            float cy = (h - 1) * 0.5f;
            float rx = cx - 4f;
            float ry = cy - 4f;

            Color[] pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - cx) / rx;
                    float dy = (y - cy) / ry;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = 0f;
                    if (r <= 0.35f)
                    {
                        a = 1.0f;
                    }
                    else if (r < 1.0f)
                    {
                        float t = (r - 0.35f) / 0.65f;
                        a = 0.5f * (1.0f + Mathf.Cos(Mathf.PI * t));
                    }
                    pixels[y * w + x] = new Color(0f, 0f, 0f, a);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            string path = "Assets/Sprites/procedural_shadow.png";
            File.WriteAllBytes(path, bytes);
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            ConfigureSpriteImporter(path);
            Debug.Log("Procedural shadow generated at: " + path);
        }

        public static void GenerateFootRing()
        {
            int w = 256;
            int h = 154; // Perspective aspect ratio ~ 0.60
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            float cx = (w - 1) * 0.5f;
            float cy = (h - 1) * 0.5f;
            float rx = cx - 4f;
            float ry = cy - 4f;

            float ringRadius = 0.90f;
            float thickness = 0.075f; // Thin elegant ring stroke

            Color[] pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - cx) / rx;
                    float dy = (y - cy) / ry;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float dist = Mathf.Abs(r - ringRadius) / thickness;
                    float a = 0f;
                    if (dist < 1.0f)
                    {
                        float c = Mathf.Cos(0.5f * Mathf.PI * dist);
                        a = c * c;
                    }
                    pixels[y * w + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            string path = "Assets/Sprites/procedural_foot_ring.png";
            File.WriteAllBytes(path, bytes);
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            ConfigureSpriteImporter(path);
            Debug.Log("Procedural foot ring generated at: " + path);
        }

        private static void ConfigureSpriteImporter(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.spritePivot = new Vector2(0.5f, 0.5f);
                importer.spritePixelsPerUnit = 150f;
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
