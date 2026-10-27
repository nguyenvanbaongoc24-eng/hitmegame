using UnityEngine;

namespace HitMe.Gameplay
{
    /// <summary>
    /// Optional scene camera rig for the world-space XY arena. The shipping
    /// gameplay view defaults to top-down because the arena art already contains
    /// perspective. Isometric tilt is available without changing the input plane.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public sealed class CameraController25D : MonoBehaviour
    {
        [SerializeField] private bool useIsometricTilt;
        [SerializeField, Range(-45f, 45f)] private float pitch = 25f;
        [SerializeField, Range(-45f, 45f)] private float yaw = 0f;
        [SerializeField] private float distance = ArenaProjection.DefaultCameraDistance;
        [SerializeField] private Transform legacyTarget;

        private Camera _camera;

        // Compatibility surface for legacy scene builders. The arena camera is
        // intentionally fixed; assigning a target must not reintroduce tracking.
        public Transform Target
        {
            get => legacyTarget;
            set => legacyTarget = value;
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            ApplyCameraSettings();
        }

        private void OnValidate()
        {
            if (_camera == null) _camera = GetComponent<Camera>();
            ApplyCameraSettings();
        }

        private void ApplyCameraSettings()
        {
            if (_camera == null) return;

            ArenaProjection.ConfigureTopDownCamera(_camera);
            if (!useIsometricTilt) return;

            Vector3 arenaCenter = new Vector3(0f, -0.4f, ArenaProjection.GameplayPlaneZ);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            _camera.transform.SetPositionAndRotation(
                arenaCenter - rotation * Vector3.forward * Mathf.Max(1f, distance),
                rotation);
        }

        public bool TryProjectToArena(Vector2 screenPoint, out Vector2 arenaPoint)
        {
            if (_camera == null) _camera = GetComponent<Camera>();
            return ArenaProjection.TryScreenPointToArena(_camera, screenPoint, out arenaPoint);
        }
    }
}
