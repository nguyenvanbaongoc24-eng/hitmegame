using System.Collections.Generic;
using UnityEngine;

namespace HitMe.AimKit
{
    /// <summary>
    /// Presentation ONLY. Consumes the already-computed authoritative aim vector;
    /// never calculates projectile direction, collision, auto-targeting or input.
    /// World-space XY (matches the current AimController's Vector2 model).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HitMeAimVisual2D : MonoBehaviour
    {
        public enum AimState { Aiming, Locked, Invalid }

        [Header("Generated visual children")]
        public SpriteRenderer originGlowRenderer;
        public SpriteRenderer originRingRenderer;
        public SpriteRenderer fanRenderer;
        public SpriteRenderer beamRenderer;
        public SpriteRenderer targetHaloRenderer;
        public SpriteRenderer targetReticleRenderer;
        public SpriteRenderer weaponHaloRenderer;
        public SpriteRenderer weaponRenderer;
        public SpriteRenderer arrowRenderer;
        public SpriteRenderer lockedRenderer;
        public SpriteRenderer invalidRenderer;

        [Header("Dot sprites")]
        public Sprite dotBright;
        public Sprite dotSoft;
        public Sprite dotTiny;

        [Header("Adjust per camera & gameplay units")]
        [Min(0.05f)] public float minLength = 0.55f;
        [Min(0.1f)] public float maxLength = 5.5f;
        [Min(0.08f)] public float dotSpacing = 0.28f;
        [Range(2, 48)] public int maxDots = 26;
        [Range(0.1f, 2f)] public float globalScale = 1f;
        public float visualZ = -0.1f;
        public int baseSortingOrder = 100;
        public bool pulseWhileAiming = true;
        [Range(0f, 0.5f)] public float pulseAmplitude = 0.065f;

        private readonly List<SpriteRenderer> _dots = new List<SpriteRenderer>();
        private Vector2 _origin;
        private Vector2 _direction = Vector2.right;
        private float _length;
        private bool _visible;
        private AimState _state;
        private float _targetBaseScale;

        private void Awake()
        {
            CreateDotPool();
            Hide();
        }

        /// <summary>Call ONLY while the pointer is held and a drag direction is valid.</summary>
        public void ShowAim(Vector2 originWorld, Vector2 authoritativeDirection, float visualLength, Sprite equippedWeaponIcon = null)
        {
            if (_dots.Count == 0) CreateDotPool();

            if (authoritativeDirection.sqrMagnitude < 0.0001f || float.IsNaN(visualLength))
            {
                Hide();
                return;
            }

            _origin = originWorld;
            _direction = authoritativeDirection.normalized;
            _length = Mathf.Clamp(visualLength, minLength, Mathf.Max(minLength, maxLength));
            _state = AimState.Aiming;
            _visible = true;
            AssignWeapon(equippedWeaponIcon);
            Redraw();
        }

        /// <summary>Optional short-lived state overlay. Call Hide() on phase change/release.</summary>
        public void SetState(AimState state)
        {
            if (!_visible) return;
            _state = state;
            Redraw();
        }

        /// <summary>Release, cancel, timeout, menu entry and reveal should call Hide().</summary>
        public void Hide()
        {
            _visible = false;
            SetEnabled(originGlowRenderer, false);
            SetEnabled(originRingRenderer, false);
            SetEnabled(fanRenderer, false);
            SetEnabled(beamRenderer, false);
            SetEnabled(targetHaloRenderer, false);
            SetEnabled(targetReticleRenderer, false);
            SetEnabled(weaponHaloRenderer, false);
            SetEnabled(weaponRenderer, false);
            SetEnabled(arrowRenderer, false);
            SetEnabled(lockedRenderer, false);
            SetEnabled(invalidRenderer, false);
            foreach (var dot in _dots) SetEnabled(dot, false);
        }

        private static void SetEnabled(SpriteRenderer r, bool enabled)
        {
            if (r != null) r.enabled = enabled;
        }

        private void AssignWeapon(Sprite weaponSprite)
        {
            if (weaponRenderer == null) return;
            weaponRenderer.sprite = weaponSprite;
            weaponRenderer.enabled = weaponSprite != null;
            if (weaponSprite == null) return;
            Vector2 bounds = weaponSprite.bounds.size;
            float dimension = Mathf.Max(bounds.x, bounds.y);
            float scale = dimension > 0.001f ? (0.62f * globalScale) / dimension : 1f;
            weaponRenderer.transform.localScale = new Vector3(scale, scale, 1);
        }

        private void CreateDotPool()
        {
            if (_dots.Count != 0) return;
            for (int i = 0; i < maxDots; i++)
            {
                var child = new GameObject("AimDot_" + i.ToString("00"));
                child.transform.SetParent(transform, false);
                var dot = child.AddComponent<SpriteRenderer>();
                dot.sprite = dotBright;
                dot.sortingOrder = baseSortingOrder + 4;
                dot.enabled = false;
                _dots.Add(dot);
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying && _dots.Count == 0) CreateDotPool();
        }

        private void Update()
        {
            if (!_visible || !pulseWhileAiming || _state != AimState.Aiming) return;
            float factor = 1f + Mathf.Sin(Time.unscaledTime * 4.3f) * pulseAmplitude;
            if (targetHaloRenderer != null) targetHaloRenderer.transform.localScale = Vector3.one * (0.56f * globalScale * factor);
            if (targetReticleRenderer != null) targetReticleRenderer.transform.localScale = Vector3.one * (_targetBaseScale * factor);
        }

        private void Position(SpriteRenderer sprite, Vector2 xy, float z, float scale, bool show)
        {
            if (sprite == null) return;
            sprite.transform.position = new Vector3(xy.x, xy.y, z);
            sprite.transform.rotation = Quaternion.identity;
            sprite.transform.localScale = Vector3.one * scale;
            sprite.enabled = show;
        }

        private void Redraw()
        {
            if (!_visible) return;
            Vector2 target = _origin + _direction * _length;
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            var rotation = Quaternion.Euler(0f, 0f, angle);
            float s = globalScale;
            Position(originGlowRenderer, _origin, visualZ, 0.42f*s, true);
            Position(originRingRenderer, _origin, visualZ - 0.001f, 0.40f*s, true);
            Position(targetHaloRenderer, target, visualZ - 0.010f, 0.56f*s, true);
            Position(targetReticleRenderer, target, visualZ - 0.012f, 0.42f*s, _state == AimState.Aiming);
            _targetBaseScale = 0.42f*s;
            Position(weaponHaloRenderer, target, visualZ - 0.013f, 0.35f*s, weaponRenderer != null && weaponRenderer.sprite != null && _state == AimState.Aiming);

            if (weaponRenderer != null && weaponRenderer.sprite != null)
            {
                weaponRenderer.transform.position = new Vector3(target.x, target.y, visualZ - 0.016f);
                weaponRenderer.transform.rotation = Quaternion.identity;
                weaponRenderer.enabled = _state == AimState.Aiming;
            }
            Position(arrowRenderer, target - _direction * (0.43f*s), visualZ - 0.014f, 0.29f*s, _state == AimState.Aiming);
            if (arrowRenderer != null) arrowRenderer.transform.rotation = rotation;
            Position(lockedRenderer, target, visualZ - 0.02f, 0.47f*s, _state == AimState.Locked);
            Position(invalidRenderer, target, visualZ - 0.02f, 0.47f*s, _state == AimState.Invalid);

            // Both beam/cone sprites have their pivot at LEFT CENTER (configured by editor importer).
            DrawStretch(fanRenderer, _origin, _length, rotation, 0.65f*s, _state == AimState.Aiming);
            DrawStretch(beamRenderer, _origin, _length, rotation, 0.60f*s, _state == AimState.Aiming);

            int count = Mathf.Clamp(Mathf.RoundToInt(_length / Mathf.Max(0.08f,dotSpacing)), 2, _dots.Count);
            for (int i = 0; i < _dots.Count; i++)
            {
                SpriteRenderer dot = _dots[i];
                bool show = i < count && _state == AimState.Aiming;
                dot.enabled = show;
                if (!show) continue;
                float t = (i + 0.7f) / (count + 0.5f);
                Vector2 xy = _origin + _direction * (t * _length);
                dot.transform.position = new Vector3(xy.x, xy.y, visualZ - 0.018f);
                dot.sprite = t < 0.28f ? dotTiny : (t < 0.58f ? dotSoft : dotBright);
                dot.transform.localScale = Vector3.one * (0.39f*s);
                dot.color = new Color(1,1,1,Mathf.Lerp(0.55f,0.98f,t));
            }
        }

        private void DrawStretch(SpriteRenderer r, Vector2 origin, float length, Quaternion rotation, float heightScale, bool show)
        {
            if (r == null) return;
            r.enabled = show;
            if (!show || r.sprite == null) return;
            r.transform.position = new Vector3(origin.x, origin.y, visualZ + 0.01f);
            r.transform.rotation = rotation;
            float width = Mathf.Max(0.001f, r.sprite.bounds.size.x);
            r.transform.localScale = new Vector3(length / width, heightScale, 1f);
        }
    }
}
