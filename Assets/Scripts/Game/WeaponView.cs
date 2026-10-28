using UnityEngine;

namespace HitMe.Game
{
    public sealed class WeaponView : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        public float spinSpeed = -720f;
        public HitFx hitFxPrefab;

        private Vector3 _start;
        private Vector3 _end;
        private float _duration;
        private float _elapsed;
        private bool _isFlying;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void Launch(Vector3 start, Vector3 end, float duration, Sprite weaponSprite = null)
        {
            _start = start;
            _end = end;
            _duration = Mathf.Max(0.01f, duration);
            _elapsed = 0f;
            _isFlying = true;
            transform.position = start;
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null && weaponSprite != null)
            {
                spriteRenderer.sprite = weaponSprite;
            }
        }

        private void Update()
        {
            if (!_isFlying) return;
            _elapsed += Time.deltaTime;
            transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
            float t = Mathf.Clamp01(_elapsed / _duration);
            transform.position = Vector3.Lerp(_start, _end, t);
            if (_elapsed >= _duration)
            {
                _isFlying = false;
                if (hitFxPrefab != null)
                {
                    Instantiate(hitFxPrefab, _end, Quaternion.identity);
                }
                Destroy(gameObject);
            }
        }
    }
}
