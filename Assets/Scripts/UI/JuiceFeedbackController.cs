using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HitMe.UI
{
    /// <summary>
    /// Lô 4 – Juice & Feedback controller.
    /// • Button press scale (0.94x via UiTheme.buttonPressScale)
    /// • Hit impact VFX via zero-alloc GenericPool
    /// • Countdown pulse on timer ring
    /// • Damage number popup
    /// • Reduced-motion toggle (PlayerPrefs "ReducedMotion")
    /// </summary>
    public sealed class JuiceFeedbackController : MonoBehaviour
    {
        [Header("References")]
        public UiTheme theme;
        public Image timerPulseRing;
        public Transform vfxParent;

        [Header("Hit Impact VFX")]
        public Sprite[] hitImpactFrames;
        public int poolWarmCount = 4;

        private Game.GenericPool<Game.HitFx> _hitFxPool;
        private bool _reducedMotion;
        private Coroutine _pulseCoroutine;

        // Static key for PlayerPrefs
        public const string PrefKeyReducedMotion = "ReducedMotion";

        private readonly List<ButtonPressHandler> _buttonHandlers = new List<ButtonPressHandler>(16);

        private void Awake()
        {
            LoadReducedMotionPref();
        }

        private void Start()
        {
            if (theme == null) theme = UiTheme.Instance;

            // Initialize hit VFX pool
            if (hitImpactFrames != null && hitImpactFrames.Length > 0)
            {
                _hitFxPool = new Game.GenericPool<Game.HitFx>(CreateHitFx, vfxParent, poolWarmCount);
            }
        }

        private Game.HitFx CreateHitFx(Transform parent)
        {
            var go = new GameObject("HitFx_Pooled");
            if (parent != null) go.transform.SetParent(parent, false);
            var fx = go.AddComponent<Game.HitFx>();
            fx.frames = hitImpactFrames;
            return fx;
        }

        // ──────────────────────────────────────────────────
        // Reduced Motion
        // ──────────────────────────────────────────────────

        public bool ReducedMotion
        {
            get => _reducedMotion;
            set
            {
                _reducedMotion = value;
                try { PlayerPrefs.SetString(PrefKeyReducedMotion, value ? "1" : "0"); PlayerPrefs.Save(); }
                catch { /* WebGL sandbox may fail */ }
            }
        }

        private void LoadReducedMotionPref()
        {
            try { _reducedMotion = PlayerPrefs.GetString(PrefKeyReducedMotion, "0") == "1"; }
            catch { _reducedMotion = false; }
        }

        // ──────────────────────────────────────────────────
        // Button Press Scale (attach to all buttons in scene)
        // ──────────────────────────────────────────────────

        /// <summary>
        /// Wire press-scale animation to a button. Call during setup.
        /// </summary>
        public void RegisterButton(Button btn)
        {
            if (btn == null) return;
            float scale = (theme != null) ? theme.buttonPressScale : 0.94f;
            var handler = btn.gameObject.AddComponent<ButtonPressHandler>();
            handler.Init(scale, this);
            _buttonHandlers.Add(handler);
        }

        /// <summary>
        /// Wire press-scale to all buttons under a root transform.
        /// </summary>
        public void RegisterAllButtons(Transform root)
        {
            if (root == null) return;
            var buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                RegisterButton(buttons[i]);
            }
        }

        // ──────────────────────────────────────────────────
        // Hit Impact VFX (pooled, zero-alloc)
        // ──────────────────────────────────────────────────

        /// <summary>
        /// Play hit impact VFX at world position. Returns to pool automatically.
        /// </summary>
        public void PlayHitImpact(Vector3 worldPos)
        {
            if (_reducedMotion) return;
            if (_hitFxPool == null) return;

            var fx = _hitFxPool.Get();
            fx.PlayAt(worldPos);
            StartCoroutine(ReturnHitFxWhenDone(fx));
        }

        private IEnumerator ReturnHitFxWhenDone(Game.HitFx fx)
        {
            // Wait until HitFx deactivates itself (animation complete)
            while (fx != null && fx.gameObject.activeSelf)
            {
                yield return null;
            }
            if (fx != null) _hitFxPool.Return(fx);
        }

        // ──────────────────────────────────────────────────
        // Damage Number Popup & Game Feel (Lô 4 & L5)
        // ──────────────────────────────────────────────────

        /// <summary>
        /// Show floating damage text at a world position.
        /// </summary>
        public void ShowDamageNumber(Vector3 worldPos, int damage, Camera cam)
        {
            if (_reducedMotion) return;
            PlayHitImpact(worldPos);
            PlayCameraShake(0.08f, 0.14f);
            PlayHitStop(0.04f);
            StartCoroutine(SpawnFloatingDamageText(worldPos, damage));
        }

        private IEnumerator SpawnFloatingDamageText(Vector3 worldPos, int damage)
        {
            var go = new GameObject("DamageTextPopup");
            go.transform.position = worldPos + Vector3.up * 0.4f;
            var text = go.AddComponent<TMPro.TextMeshPro>();
            text.text = $"-{damage} HP 💔";
            text.fontSize = 4.5f;
            text.fontStyle = TMPro.FontStyles.Bold;
            text.color = new Color(1f, 0.25f, 0.25f, 1f);
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.sortingOrder = 10;

            float elapsed = 0f;
            float duration = 0.55f;
            Vector3 startPos = go.transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float p = elapsed / duration;
                if (go != null)
                {
                    go.transform.position = startPos + Vector3.up * (p * 0.6f);
                    text.color = new Color(1f, 0.25f, 0.25f, 1f - p);
                    float s = 1f + Mathf.Sin(p * Mathf.PI) * 0.25f;
                    go.transform.localScale = new Vector3(s, s, 1f);
                }
                yield return null;
            }

            if (go != null) Destroy(go);
        }

        /// <summary>
        /// Rung nhẹ camera khi trúng đòn (Game feel L5)
        /// </summary>
        public void PlayCameraShake(float intensity = 0.08f, float duration = 0.14f)
        {
            if (_reducedMotion) return;
            var cam = Camera.main;
            if (cam == null) return;
            StartCoroutine(CameraShakeRoutine(cam.transform, intensity, duration));
        }

        private IEnumerator CameraShakeRoutine(Transform camTr, float intensity, float duration)
        {
            Vector3 originalPos = camTr.position;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float damper = 1f - (elapsed / duration);
                float x = (Random.value * 2f - 1f) * intensity * damper;
                float y = (Random.value * 2f - 1f) * intensity * damper;
                if (camTr != null)
                {
                    camTr.position = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
                }
                yield return null;
            }
            if (camTr != null) camTr.position = originalPos;
        }

        /// <summary>
        /// Hit-stop ngắn tạo độ nảy / cảm giác va chạm có trọng lượng (Game feel L5)
        /// </summary>
        public void PlayHitStop(float duration = 0.04f)
        {
            if (_reducedMotion) return;
            StartCoroutine(HitStopRoutine(duration));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            float prevScale = Time.timeScale;
            Time.timeScale = 0.1f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = prevScale;
        }

        // ──────────────────────────────────────────────────
        // Countdown Pulse on Timer Ring
        // ──────────────────────────────────────────────────

        /// <summary>
        /// Start pulsing the timer ring (call when countdown enters last N seconds).
        /// </summary>
        public void StartCountdownPulse()
        {
            if (_reducedMotion) return;
            if (timerPulseRing == null) return;
            if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
            timerPulseRing.gameObject.SetActive(true);
            _pulseCoroutine = StartCoroutine(PulseRoutine());
        }

        /// <summary>
        /// Stop pulsing the timer ring.
        /// </summary>
        public void StopCountdownPulse()
        {
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
            }
            if (timerPulseRing != null)
            {
                timerPulseRing.transform.localScale = Vector3.one;
                var c = timerPulseRing.color;
                timerPulseRing.color = new Color(c.r, c.g, c.b, 1f);
                timerPulseRing.gameObject.SetActive(false);
            }
        }

        private IEnumerator PulseRoutine()
        {
            float duration = (theme != null) ? theme.pulseLoopDuration : 0.50f;
            float half = duration / 2f;
            float maxScale = 1.15f;
            float minAlpha = 0.4f;

            while (true)
            {
                // Scale up, fade out
                float t = 0f;
                while (t < half)
                {
                    t += Time.deltaTime;
                    float p = t / half;
                    float s = Mathf.Lerp(1f, maxScale, p);
                    timerPulseRing.transform.localScale = new Vector3(s, s, 1f);
                    var c = timerPulseRing.color;
                    timerPulseRing.color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, minAlpha, p));
                    yield return null;
                }

                // Scale down, fade in
                t = 0f;
                while (t < half)
                {
                    t += Time.deltaTime;
                    float p = t / half;
                    float s = Mathf.Lerp(maxScale, 1f, p);
                    timerPulseRing.transform.localScale = new Vector3(s, s, 1f);
                    var c = timerPulseRing.color;
                    timerPulseRing.color = new Color(c.r, c.g, c.b, Mathf.Lerp(minAlpha, 1f, p));
                    yield return null;
                }
            }
        }
    }

    /// <summary>
    /// Attached to buttons for press-scale animation (pointer down/up).
    /// Zero-alloc after setup — no lambdas, no delegates.
    /// </summary>
    public sealed class ButtonPressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private float _pressScale = 0.94f;
        private Vector3 _originalScale;
        private JuiceFeedbackController _controller;

        public void Init(float pressScale, JuiceFeedbackController controller)
        {
            _pressScale = pressScale;
            _controller = controller;
            _originalScale = transform.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_controller != null && _controller.ReducedMotion) return;
            transform.localScale = _originalScale * _pressScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.localScale = _originalScale;
        }

        private void OnDisable()
        {
            // Reset scale when disabled to avoid stuck state
            transform.localScale = _originalScale;
        }
    }
}
