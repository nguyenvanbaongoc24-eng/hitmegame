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
        // Damage Number Popup
        // ──────────────────────────────────────────────────

        /// <summary>
        /// Show floating damage text at a screen position (over UI).
        /// Uses the existing toast system or a simple popup.
        /// </summary>
        public void ShowDamageNumber(Vector3 worldPos, int damage, Camera cam)
        {
            if (_reducedMotion) return;
            // Damage popups handled via HitFx sprite overlay for now
            PlayHitImpact(worldPos);
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
