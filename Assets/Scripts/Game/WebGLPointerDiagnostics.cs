using HitMe.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HitMe.Game
{
    /// <summary>On-screen WebGL pointer telemetry. It is intentionally gated behind ?debugPointer=1 or a development build.</summary>
    public sealed class WebGLPointerDiagnostics : MonoBehaviour
    {
        private float _smoothedDelta;

        public static bool IsRequested()
        {
#if DEVELOPMENT_BUILD
            return true;
#else
            return Application.absoluteURL.IndexOf("debugPointer=1", System.StringComparison.OrdinalIgnoreCase) >= 0;
#endif
        }

        private void Update() { _smoothedDelta = Mathf.Lerp(_smoothedDelta, Time.unscaledDeltaTime, .1f); }

        private void OnGUI()
        {
            if (!IsRequested()) return;
            var input = GetComponent<PlacementInput>(); var match = GetComponent<MatchController>();
            GUI.Box(new Rect(12f, Screen.height - 172f, 480f, 160f), "WEBGL POINTER DEBUG");
            float y = Screen.height - 148f;
            Draw(y, "scene=" + SceneManager.GetActiveScene().name + "  fps=" + (_smoothedDelta > 0 ? (1f / _smoothedDelta).ToString("0") : "-")); y += 22f;
            Draw(y, "touchCount=" + Input.touchCount + "  Screen=" + Screen.width + "x" + Screen.height); y += 22f;
            Draw(y, input == null ? "pointer adapter: missing" : "source=" + input.LastInputSource + " phase=" + input.LastPointerPhase); y += 22f;
            Draw(y, input == null ? "raw: -" : "raw=" + input.LastRawScreenPosition + " world=" + input.LastWorldPosition); y += 22f;
            Draw(y, match == null ? "gesture: missing" : "gesture=" + match.GestureMode + " hasAim=" + match.HasGestureAim); y += 22f;
            Draw(y, "url flag: debugPointer=1");
        }

        private static void Draw(float y, string value) { GUI.Label(new Rect(24f, y, 456f, 22f), value); }
    }
}
