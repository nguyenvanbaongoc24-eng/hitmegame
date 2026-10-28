using UnityEngine;

namespace HitMe.Game
{
    public sealed class HitFx : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        public Sprite[] frames;
        public float fps = 20f;
        private float _timer;
        private int _index;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (frames != null && frames.Length > 0 && spriteRenderer != null)
            {
                spriteRenderer.sprite = frames[0];
            }
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0 || spriteRenderer == null) return;
            _timer += Time.deltaTime;
            if (_timer >= 1f / fps)
            {
                _timer = 0f;
                _index++;
                if (_index < frames.Length)
                {
                    spriteRenderer.sprite = frames[_index];
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
