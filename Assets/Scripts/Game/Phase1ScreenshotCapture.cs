using System;
using System.Collections;
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
        private Phase _observedPhase = Phase.RoundEnd;
        private float _phaseStartedAt;
        private Texture2D _reusableOutput;

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
            if (!enabled) { this.enabled = false; return; }
            Directory.CreateDirectory(_directory);
            Screen.SetResolution(1080, 2340, false);
            var camera = Camera.main;
            if (camera != null && camera.orthographic)
            {
                const float captureAspect = 1080f / 2340f;
                camera.aspect = captureAspect;
                camera.orthographicSize = Mathf.Max(10f, 5.1f / captureAspect);
            }
        }

        private void LateUpdate()
        {
            if (controller == null || controller.State == null) return;
            var phase = controller.State.Phase;
            if (phase != _observedPhase)
            {
                _observedPhase = phase;
                _phaseStartedAt = Time.unscaledTime;
            }
            var elapsed = Time.unscaledTime - _phaseStartedAt;
            if (phase == Phase.Placing && elapsed >= .25f) Capture(0, "shot_1_placing.png");
            else if (phase == Phase.Reveal && elapsed >= .2f) Capture(1, "shot_2_reveal.png");
            else if (phase == Phase.Throwing && elapsed >= .4f) Capture(2, "shot_3_throwing.png");
            else if (phase == Phase.MatchEnd && elapsed >= .2f) Capture(3, "shot_4_result.png");
        }

        private void Capture(int index, string name)
        {
            if (_captured[index]) return;
            _captured[index] = true;
            StartCoroutine(CaptureAtEndOfFrame(name));
        }

        private IEnumerator CaptureAtEndOfFrame(string name)
        {
            yield return new WaitForEndOfFrame();
            var cam = Camera.main;
            if (cam == null) yield break;
            var target = RenderTexture.GetTemporary(1080, 2340, 24, RenderTextureFormat.ARGB32);
            var prevCamRt = cam.targetTexture;
            var previous = RenderTexture.active;
            cam.targetTexture = target;
            cam.Render();
            RenderTexture.active = target;
            if (_reusableOutput == null)
            {
                _reusableOutput = new Texture2D(1080, 2340, TextureFormat.RGBA32, false);
            }
            _reusableOutput.ReadPixels(new Rect(0, 0, 1080, 2340), 0, 0);
            _reusableOutput.Apply(false, false);
            File.WriteAllBytes(Path.Combine(_directory, name), _reusableOutput.EncodeToPNG());
            cam.targetTexture = prevCamRt;
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
        }
    }
}
