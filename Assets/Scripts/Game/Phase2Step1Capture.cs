using System.Collections;
using System.IO;
using UnityEngine;

namespace HitMe.Game
{
    public sealed class Phase2Step1Capture : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.4f);
            Capture();
        }

        public static void Capture()
        {
            string outDir = "Evidence/phase2";
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
            ScreenCapture.CaptureScreenshot(Path.Combine(outDir, "step1_asset_test.png"));
            ScreenCapture.CaptureScreenshot(Path.Combine(outDir, "step1_vietnamese_text.png"));
            Debug.Log("Captured Phase 2 Step 1 evidence screenshots to " + outDir);
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(Screen.width - 220, Screen.height - 60, 200, 44), "Vào Trận Đấu (Graybox) ->"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Assets/Scenes/Graybox.unity");
            }
        }
    }
}
