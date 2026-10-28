using UnityEngine;

namespace HitMe.Game
{
    public sealed class WeaponFlightView : MonoBehaviour
    {
        public Vector3 StartPoint;
        public Vector3 EndPoint;
        public float Duration;
        public float SpinSpeed = -720f;
        public HitFx HitFxPrefab;

        private float _elapsed;
        private SpriteRenderer _renderer;
        private static Sprite _sandalSprite;

        public void Initialise(Vector3 start, Vector3 end, float duration)
        {
            StartPoint = start;
            EndPoint = end;
            Duration = Mathf.Max(0.01f, duration);
            transform.position = start;

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
                _renderer.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1f);
                _renderer.color = Color.yellow;
                transform.localScale = new Vector3(.5f, .16f, 1f);
            }

            _renderer.sortingOrder = 4;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            transform.Rotate(0, 0, SpinSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(StartPoint, EndPoint, Mathf.Clamp01(_elapsed / Duration));

            if (_elapsed >= Duration)
            {
                if (HitFxPrefab != null)
                {
                    Instantiate(HitFxPrefab, EndPoint, Quaternion.identity);
                }
                Destroy(gameObject);
            }
        }
    }
}
