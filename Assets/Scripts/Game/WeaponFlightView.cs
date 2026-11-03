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

        public static float VisualWeaponScale = 0.28f;
        public static bool CustomScaleApplied;

        public static void SetVisualWeaponScale(float scale, bool fromUrl = false)
        {
            VisualWeaponScale = scale;
            if (fromUrl) CustomScaleApplied = true;
        }

        private float _elapsed;
        private SpriteRenderer _renderer;
        private SpriteRenderer _shadow;
        private static Sprite _sandalSprite;
        private static Sprite _shadowSprite;
        private static Sprite[] _hitFxFrames;

        public static void ConfigureSharedResources(GameConfigAsset configAsset)
        {
            if (configAsset == null) return;
            if (!CustomScaleApplied && configAsset.weaponScale > 0.01f) VisualWeaponScale = configAsset.weaponScale;
            if (configAsset.sandalProjectileSprite != null)
            {
                _sandalSprite = configAsset.sandalProjectileSprite;
            }
            if (configAsset.shadowSprite != null)
            {
                _shadowSprite = configAsset.shadowSprite;
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

            if (_shadow == null)
            {
                var sGo = new GameObject("Shadow");
                sGo.transform.SetParent(transform, false);
                _shadow = sGo.AddComponent<SpriteRenderer>();
                _shadow.sortingOrder = 3;
                _shadow.color = new Color(0f, 0f, 0f, 0.40f);
            }

            if (_shadowSprite == null)
            {
#if UNITY_EDITOR
                _shadowSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/shadow.png");
#endif
            }
            if (_shadow != null && _shadowSprite != null)
            {
                _shadow.sprite = _shadowSprite;
            }
            if (_shadow != null)
            {
                _shadow.gameObject.SetActive(true);
            }

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
                transform.localScale = new Vector3(VisualWeaponScale, VisualWeaponScale, 1f);
            }
            else
            {
                _renderer.color = Color.yellow;
                transform.localScale = new Vector3(0.50f, 0.16f, 1f);
            }

            _renderer.sortingOrder = 5;
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / Duration);
            float arcHeight = Mathf.Sin(progress * Mathf.PI) * 0.35f;

            Vector3 groundPos = Vector3.Lerp(StartPoint, EndPoint, progress);
            transform.position = new Vector3(groundPos.x, groundPos.y + arcHeight, groundPos.z);
            transform.Rotate(0, 0, SpinSpeed * Time.deltaTime);

            if (_shadow != null)
            {
                _shadow.transform.position = new Vector3(groundPos.x, groundPos.y - 0.18f, groundPos.z);
                float shadowScale = (0.50f - arcHeight * 0.25f) * VisualWeaponScale;
                // UiTheme is in Assembly-CSharp; HitMe.Game cannot reference it.
                // Default constants inlined: weaponShadowVerticalScaleFactor = 0.54f
                float shadowYFactor = 0.54f;
                _shadow.transform.localScale = new Vector3(shadowScale, shadowScale * shadowYFactor, 1f);
            }

            if (_elapsed >= Duration)
            {
                if (SpawnHitEffect && PooledHitFx != null)
                {
                    PooledHitFx.PlayAt(EndPoint, _hitFxFrames);
                }
                else
                {
                    Audio.AudioService.Instance?.PlayWallThud();
                }
                if (_shadow != null)
                {
                    _shadow.gameObject.SetActive(false);
                }
                gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (_shadow != null)
            {
                _shadow.gameObject.SetActive(false);
            }
        }
    }
}
