using UnityEditor;
using UnityEngine;

namespace HitMe.EditorTools
{
    public static class AudioOptimizer
    {
        [MenuItem("Tools/Optimize Audio for WebGL")]
        public static void Optimize()
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio" });
            int count = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null) continue;

                bool isBgmOrAmb = path.Contains("bgm_") || path.Contains("amb_");
                importer.forceToMono = true; // Monophonic audio cuts memory/transfer in half for mobile

                var defaultSettings = importer.defaultSampleSettings;
                if (isBgmOrAmb)
                {
                    defaultSettings.loadType = AudioClipLoadType.CompressedInMemory;
                    defaultSettings.compressionFormat = AudioCompressionFormat.Vorbis;
                    defaultSettings.quality = 0.35f; // ~96 kbps Vorbis
                }
                else
                {
                    defaultSettings.loadType = AudioClipLoadType.DecompressOnLoad;
                    defaultSettings.compressionFormat = AudioCompressionFormat.Vorbis;
                    defaultSettings.quality = 0.30f; // Lightweight SFX
                }

                importer.defaultSampleSettings = defaultSettings;

                var webglSettings = importer.GetOverrideSampleSettings("WebGL");
                webglSettings.loadType = defaultSettings.loadType;
                webglSettings.compressionFormat = defaultSettings.compressionFormat;
                webglSettings.quality = defaultSettings.quality;
                webglSettings.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
                importer.SetOverrideSampleSettings("WebGL", webglSettings);

                importer.SaveAndReimport();
                count++;
                Debug.Log($"[AudioOptimizer] Optimized {path} (mono=true, format=Vorbis, quality={defaultSettings.quality})");
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[AudioOptimizer] Successfully optimized {count} audio clips for WebGL.");
        }
    }
}
