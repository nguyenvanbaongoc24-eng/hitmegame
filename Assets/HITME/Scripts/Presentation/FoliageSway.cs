using UnityEngine;

namespace HitMe.Presentation
{
    // Transform fallback for single-quad art. Root stays fixed because the renderer is a child of this pivot.
    public sealed class FoliageSway : MonoBehaviour
    {
        [SerializeField] private GlobalWindSettings wind;
        [SerializeField] private float phase;
        [SerializeField] private bool staticFallback;
        private Quaternion _baseRotation;
        private void Awake() { _baseRotation = transform.localRotation; }
        private void LateUpdate()
        {
            if (staticFallback || wind == null) return;
            var angle = Mathf.Sin(Time.time * wind.frequency + phase) * wind.maxAngle * wind.strength;
            transform.localRotation = _baseRotation * Quaternion.Euler(0f, 0f, angle);
        }
        public void SetStaticFallback(bool enabled) { staticFallback = enabled; transform.localRotation = _baseRotation; }
    }
}
