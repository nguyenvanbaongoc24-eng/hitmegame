using System;
using UnityEngine;

namespace HitMe.Game
{
    public sealed class WeaponFlightView : MonoBehaviour
    {
        public Vector3 StartPoint;
        public Vector3 EndPoint;
        public float Duration;
        public float SpinSpeed = -720f;
        public HitFx PooledHitFx;
        public bool SpawnHitEffect;

        private float _elapsed;
        private SpriteRenderer _renderer;
        private static Sprite _sandalSprite;
        private static Sprite[] _hitFxFrames;

        public static void ConfigureSharedResources(GameConfigAsset configAsset)
        {
            if (configAsset == null) return;
            if (configAsset.sandalProjectileSprite != null)
            {
                _sandalSprite = configAsset.sandalProjectileSprite;
            }
            if (configAsset.hitFxFrames != null && configAsset.hitFxFrames.Length > 0)
            {
                _hitFxFrames = configAsset.hitFxFrames;
            }
        }

        public void Initialise(Vector3 start, Vector3 end, float duration, bool didHit = false, HitFx pooledHitFx = null)
        {
            StartPoint = start;
            EndPoint = end;
            Duration = Mathf.Max(0.01f, duration);
            SpawnHitEffect = didHit;
            PooledHitFx = pooledHitFx;
            _elapsed = 0f;
            transform.position = start;
            transform.rotation = Quaternion.identity;

            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            if (_renderer == null) _renderer = gameObject.AddComponent<SpriteRenderer>();

            if (_sandalSprite == null)
            {
#if UNITY_EDITOR
                _sandalSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/dep_projectile.png");
                if (_sandalSprite == null)
                {
                    _sandalSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/dep_to_ong_projectile.png");
                }
#endif
            }

            if (_sandalSprite != null)
            {
                _renderer.sprite = _sandalSprite;
                _renderer.color = Color.white;
                transform.localScale = Vector3.one;
            }
            else
            {
                _renderer.color = Color.yellow;
                transform.localScale = new Vector3(0.50f, 0.16f, 1f);
            }

            _renderer.sortingOrder = 4;
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            transform.Rotate(0, 0, SpinSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(StartPoint, EndPoint, Mathf.Clamp01(_elapsed / Duration));

            if (_elapsed >= Duration)
            {
                if (SpawnHitEffect && PooledHitFx != null)
                {
                    PooledHitFx.PlayAt(EndPoint, _hitFxFrames);
                }
                gameObject.SetActive(false);
            }
        }
    }
}
