using UnityEngine;

namespace HitMe.Gameplay
{
    /// <summary>
    /// HIT ME - 2.5D Isometric / Top-Down Camera Controller.
    /// Đặc tả kỹ thuật (MD/04_ARENA_CAMERA.md):
    /// - Cố định góc nghiêng 45 độ (Pitch = 45°).
    /// - Bám theo Player (Smooth tracking) không gây giật khung hình / sub-pixel jitter.
    /// - Giới hạn biên camera trong sân đấu (Arena Bounds Clamping).
    /// - Hỗ trợ Camera Shake khi trúng đòn hoặc hiệu ứng ném.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public class CameraController25D : MonoBehaviour
    {
        [Header("Target Tracking")]
        [Tooltip("Transform mục tiêu cần bám theo (thường là Player)")]
        [SerializeField] private Transform target;
        [Tooltip("Độ mượt khi bám theo mục tiêu")]
        [Range(0.01f, 1.0f)]
        [SerializeField] private float smoothTime = 0.15f;

        [Header("2.5D Angle & Offset Configuration")]
        [Tooltip("Góc nghiêng từ trên xuống (chuẩn 45 độ theo đặc tả)")]
        [Range(30f, 60f)]
        [SerializeField] private float pitchAngle = 45f;
        [Tooltip("Góc xoay ngang Yaw (0 độ = nhìn thẳng về trục Z+)")]
        [SerializeField] private float yawAngle = 0f;
        [Tooltip("Khoảng cách từ Camera đến mặt đất/Target")]
        [SerializeField] private float distance = 14f;
        [Tooltip("Tâm nhìn dịch chuyển so với Target (VD: nâng lên ngang ngực nhân vật)")]
        [SerializeField] private Vector3 lookOffset = new Vector3(0f, 0.8f, 0f);

        [Header("Arena Bounds Clamping")]
        [Tooltip("Có kích hoạt giới hạn camera trong sân không")]
        [SerializeField] private bool enableBoundsClamping = true;
        [Tooltip("Tọa độ biên sân đấu (Min X, Max X, Min Z, Max Z)")]
        [SerializeField] private Vector2 arenaBoundsX = new Vector2(-6f, 6f);
        [SerializeField] private Vector2 arenaBoundsZ = new Vector2(-10f, 10f);

        [Header("Camera Projection Mode")]
        [Tooltip("Sử dụng Orthographic (chuẩn 2.5D Pixel-clean) hoặc Perspective")]
        [SerializeField] private bool useOrthographic = true;
        [SerializeField] private float orthographicSize = 7.5f;

        [Header("Camera Shake (Impact Feedback)")]
        private float shakeDuration = 0f;
        private float shakeMagnitude = 0f;
        private Vector3 currentVelocity;
        private Camera cam;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void Awake()
        {
            cam = GetComponent<Camera>();
            ApplyCameraSettings();
        }

        private void OnValidate()
        {
            if (cam == null) cam = GetComponent<Camera>();
            ApplyCameraSettings();
        }

        private void ApplyCameraSettings()
        {
            if (cam != null)
            {
                cam.orthographic = useOrthographic;
                if (useOrthographic)
                {
                    cam.orthographicSize = orthographicSize;
                }
            }
            // Khóa góc xoay cố định 45 độ chuẩn 2.5D
            transform.rotation = Quaternion.Euler(pitchAngle, yawAngle, 0f);
        }

        private void LateUpdate()
        {
            Vector3 focusPoint = Vector3.zero;

            if (target != null)
            {
                focusPoint = target.position + lookOffset;
            }

            // Kẹp vị trí trong giới hạn biên sân nếu bật bounds clamping
            if (enableBoundsClamping)
            {
                focusPoint.x = Mathf.Clamp(focusPoint.x, arenaBoundsX.x, arenaBoundsX.y);
                focusPoint.z = Mathf.Clamp(focusPoint.z, arenaBoundsZ.x, arenaBoundsZ.y);
            }

            // Tính toán vị trí camera dựa trên góc nghiêng 45 độ
            // Offset hướng lùi về phía sau và lên trên:
            Quaternion rotation = Quaternion.Euler(pitchAngle, yawAngle, 0f);
            Vector3 targetPosition = focusPoint - (rotation * Vector3.forward * distance);

            // Bám theo mượt mà bằng SmoothDamp (tránh snap giật viền sprite trên mobile)
            if (Application.isPlaying)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
            }
            else
            {
                transform.position = targetPosition;
            }

            // Cập nhật góc quay cố định
            transform.rotation = rotation;

            // Xử lý hiệu ứng rung màn hình (Screen Shake)
            if (shakeDuration > 0)
            {
                Vector3 shakeOffset = Random.insideUnitSphere * shakeMagnitude;
                shakeOffset.z = 0; // Giữ ổn định trục Z
                transform.position += shakeOffset;

                shakeDuration -= Time.deltaTime;
                shakeMagnitude = Mathf.Lerp(shakeMagnitude, 0f, Time.deltaTime * 5f);
            }
        }

        /// <summary>
        /// Kích hoạt rung màn hình khi nhân vật bị ném trúng (Hit) hoặc đòn ném uy lực (Throw).
        /// </summary>
        /// <param name="duration">Thời gian rung (giây, thường 0.15 - 0.3s)</param>
        /// <param name="magnitude">Độ mạnh của rung (thường 0.15 - 0.35m)</param>
        public void TriggerShake(float duration = 0.2f, float magnitude = 0.2f)
        {
            shakeDuration = duration;
            shakeMagnitude = magnitude;
        }

        /// <summary>
        /// Gizmos hiển thị đường bao quan sát và biên sân trong Unity Editor
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 center = new Vector3((arenaBoundsX.x + arenaBoundsX.y) * 0.5f, 0f, (arenaBoundsZ.x + arenaBoundsZ.y) * 0.5f);
            Vector3 size = new Vector3(arenaBoundsX.y - arenaBoundsX.x, 0.1f, arenaBoundsZ.y - arenaBoundsZ.x);
            Gizmos.DrawWireCube(center, size);

            if (target != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, target.position + lookOffset);
            }
        }
    }
}
