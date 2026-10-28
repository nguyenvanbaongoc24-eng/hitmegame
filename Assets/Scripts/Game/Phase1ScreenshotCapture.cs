using System;
using System.IO;
using HitMe.Core;
using UnityEngine;

namespace HitMe.Game
{
    public sealed class Phase1ScreenshotCapture : MonoBehaviour
    {
        public MatchController controller;
        private readonly bool[] _captured = new bool[4];
        private string _directory;

        private void Start()
        {
            var args = Environment.GetCommandLineArgs();
            bool enabled = false;
            _directory = Path.Combine(Application.dataPath, "..", "Evidence", "phase1");
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-capturePhase1") enabled = true;
                if (args[i] == "-captureDir" && i + 1 < args.Length) _directory = args[i + 1];
            }
            if (!enabled) { enabled = false; gameObject.SetActive(false); return; }
            Directory.CreateDirectory(_directory);
            Screen.SetResolution(1080, 2340, false);
        }

        private void LateUpdate()
        {
            if (controller == null || controller.State == null) return;
            var phase = controller.State.Phase;
            if (phase == Phase.Placing) Capture(0, "shot_1_placing.png");
            else if (phase == Phase.Reveal) Capture(1, "shot_2_reveal.png");
            else if (phase == Phase.Throwing) Capture(2, "shot_3_throwing.png");
            else if (phase == Phase.MatchEnd) Capture(3, "shot_4_result.png");
        }

        private void Capture(int index, string name)
        {
            if (_captured[index]) return;
            _captured[index] = true;
            ScreenCapture.CaptureScreenshot(Path.Combine(_directory, name), 1);
        }
    }
}
