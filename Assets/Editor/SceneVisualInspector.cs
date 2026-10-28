using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HitMe.Editor
{
    public static class SceneVisualInspector
    {
        [MenuItem("Tools/HIT ME/Capture Current MainMatch")]
        public static void CaptureMainMatch()
        {
            const string scenePath = "Assets/Scenes/MainMatch.unity";
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("No main camera found in " + scenePath);
                return;
            }

            int w = 1080;
            int h = 1920;

            RenderTexture rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32);
            RenderTexture prevRt = RenderTexture.active;
            RenderTexture prevCamRt = cam.targetTexture;

            try
            {
                cam.targetTexture = rt;
                RenderTexture.active = rt;
                cam.Render();

                Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tex.Apply();

                Directory.CreateDirectory("Evidence/phase2");
                byte[] bytes = tex.EncodeToPNG();
                string outPath = "Evidence/phase2/mainmatch_current.png";
                File.WriteAllBytes(outPath, bytes);
                Debug.Log("[SceneVisualInspector] Saved current MainMatch capture to " + outPath);
                Object.DestroyImmediate(tex);
            }
            finally
            {
                cam.targetTexture = prevCamRt;
                RenderTexture.active = prevRt;
                RenderTexture.ReleaseTemporary(rt);
            }
        }
    }
}
