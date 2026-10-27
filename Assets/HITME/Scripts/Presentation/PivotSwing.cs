using UnityEngine;

namespace HitMe.Presentation
{
    // Lanterns use a real hanging pivot instead of rubber-like shader distortion.
    public sealed class PivotSwing : MonoBehaviour
    {
        [SerializeField] private float amplitude = 5f;
        [SerializeField] private float frequency = 1.15f;
        [SerializeField] private float phase;
        private Quaternion _baseRotation;
        private void Awake() { _baseRotation = transform.localRotation; }
        private void LateUpdate() => transform.localRotation = _baseRotation * Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * frequency + phase) * amplitude);
    }
}
