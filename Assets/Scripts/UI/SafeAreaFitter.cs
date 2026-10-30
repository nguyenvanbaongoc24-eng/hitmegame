using UnityEngine;
using UnityEngine.UI;

namespace HitMe.UI
{
    /// <summary>
    /// Constrains the UI root RectTransform to a portrait 1080x2340 aspect column
    /// (preventing HUD elements from stretching out to wide desktop browser edges)
    /// and applies mobile Safe Area insets (e.g. iPhone notch / Dynamic Island and Home Indicator).
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        public const float ReferenceWidth = 1080f;
        public const float ReferenceHeight = 2340f;
        public const float TargetPortraitAspect = ReferenceWidth / ReferenceHeight; // ~0.4615385f

        // Standard iPhone 14/15 Pro portrait safe area ratios (top ~102px/2340px, bottom ~80px/2340px)
        public const float SimulatedIphoneTopInsetNorm = 102f / ReferenceHeight;
        public const float SimulatedIphoneBottomInsetNorm = 80f / ReferenceHeight;

        [Tooltip("When enabled (or when running on mobile/iPhone where browser safeArea is not reported), applies iPhone safe area insets.")]
        public bool enforceMobileSafePadding = true;

        [Tooltip(" minimum top/bottom safe padding on all screens so HUD never touches window borders.")]
        public float minTopPaddingNorm = 0.010f;
        public float minBottomPaddingNorm = 0.012f;

        private RectTransform _rectTransform;
        private CanvasScaler _canvasScaler;
        private int _lastScreenWidth = -1;
        private int _lastScreenHeight = -1;
        private Rect _lastSafeArea;

        private void Awake()
        {
            CacheComponents();
            RefreshIfNeeded(true);
        }

        private void OnEnable()
        {
            CacheComponents();
            RefreshIfNeeded(true);
        }

        private void Update()
        {
            RefreshIfNeeded(false);
        }

        private void CacheComponents()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
            if (_canvasScaler == null)
            {
                _canvasScaler = GetComponentInParent<CanvasScaler>();
            }
        }

        public void RefreshIfNeeded(bool force)
        {
            int w = Screen.width;
            int h = Screen.height;
            Rect safe = Screen.safeArea;

            if (!force && w == _lastScreenWidth && h == _lastScreenHeight && safe == _lastSafeArea)
            {
                return;
            }

            _lastScreenWidth = w;
            _lastScreenHeight = h;
            _lastSafeArea = safe;

            bool isNarrowMobile = Application.isMobilePlatform;
            ApplyForScreen(w, h, safe, isNarrowMobile && enforceMobileSafePadding);
        }

        /// <summary>
        /// Deterministic layout calculation for any screen resolution and safeArea rect.
        /// Can be invoked directly by EditMode tests to verify Desktop Portrait, Desktop Wide, and iPhone Safe Area.
        /// </summary>
        public void ApplyForScreen(int screenWidth, int screenHeight, Rect safeAreaPixels, bool simulateIphoneNotchFallback = false)
        {
            CacheComponents();
            if (_rectTransform == null || screenWidth <= 0 || screenHeight <= 0)
            {
                return;
            }

            float currentAspect = (float)screenWidth / screenHeight;

            if (_canvasScaler != null)
            {
                _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                _canvasScaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
                // Match height when wider than 1080:2340 so the portrait column scales cleanly; match width if narrower
                _canvasScaler.matchWidthOrHeight = currentAspect >= TargetPortraitAspect ? 1.0f : 0.0f;
            }

            // 1. Portrait column horizontal bounds in normalized screen coordinates [0..1]
            float portMinX = 0f;
            float portMaxX = 1f;
            if (currentAspect > TargetPortraitAspect + 0.001f)
            {
                float portraitPixelWidth = screenHeight * TargetPortraitAspect;
                float normWidth = Mathf.Clamp01(portraitPixelWidth / screenWidth);
                portMinX = 0.50f - normWidth * 0.50f;
                portMaxX = 0.50f + normWidth * 0.50f;
            }

            // 2. Hardware/OS safeArea normalized bounds
            float safeMinX = Mathf.Clamp01(safeAreaPixels.xMin / screenWidth);
            float safeMaxX = Mathf.Clamp01(safeAreaPixels.xMax / screenWidth);
            float safeMinY = Mathf.Clamp01(safeAreaPixels.yMin / screenHeight);
            float safeMaxY = Mathf.Clamp01(safeAreaPixels.yMax / screenHeight);

            if (safeMaxX <= safeMinX) { safeMinX = 0f; safeMaxX = 1f; }
            if (safeMaxY <= safeMinY) { safeMinY = 0f; safeMaxY = 1f; }

            // 3. Apply iPhone safe area fallback if requested and OS reported full-screen safeArea
            if (simulateIphoneNotchFallback)
            {
                if (safeMinY < SimulatedIphoneBottomInsetNorm)
                {
                    safeMinY = SimulatedIphoneBottomInsetNorm;
                }
                if (safeMaxY > 1f - SimulatedIphoneTopInsetNorm)
                {
                    safeMaxY = 1f - SimulatedIphoneTopInsetNorm;
                }
            }
            else
            {
                if (safeMinY < minBottomPaddingNorm) safeMinY = minBottomPaddingNorm;
                if (safeMaxY > 1f - minTopPaddingNorm) safeMaxY = 1f - minTopPaddingNorm;
            }

            float finalMinX = Mathf.Max(portMinX, safeMinX);
            float finalMaxX = Mathf.Min(portMaxX, safeMaxX);
            float finalMinY = safeMinY;
            float finalMaxY = safeMaxY;

            _rectTransform.anchorMin = new Vector2(finalMinX, finalMinY);
            _rectTransform.anchorMax = new Vector2(finalMaxX, finalMaxY);
            _rectTransform.pivot = new Vector2(0.50f, 0.50f);
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }
    }
}
