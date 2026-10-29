using UnityEngine;

namespace HitMe.Game
{
    /// <summary>WebGL pointer telemetry flag helper without legacy IMGUI allocations.</summary>
    public sealed class WebGLPointerDiagnostics : MonoBehaviour
    {
        private float _smoothedDelta;
        public float CurrentFps => _smoothedDelta > 0.0001f ? (1f / _smoothedDelta) : 60f;

        public static bool IsRequested()
        {
            return Application.absoluteURL != null &&
                   Application.absoluteURL.IndexOf("debugPointer=1", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void Update()
        {
            _smoothedDelta = Mathf.Lerp(_smoothedDelta, Time.unscaledDeltaTime, .1f);
        }
    }
}
