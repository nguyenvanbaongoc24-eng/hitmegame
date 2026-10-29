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
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sortingOrder = 5;
            }
        }

        public void PlayAt(Vector3 position, Sprite[] customFrames = null)
        {
            if (customFrames != null && customFrames.Length > 0)
            {
                frames = customFrames;
            }
            transform.position = position;
            _timer = 0f;
            _index = 0;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (frames != null && frames.Length > 0 && spriteRenderer != null)
            {
                spriteRenderer.sprite = frames[0];
            }
            gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            _timer = 0f;
            _index = 0;
            if (frames != null && frames.Length > 0 && spriteRenderer != null)
            {
                spriteRenderer.sprite = frames[0];
            }
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0 || spriteRenderer == null)
            {
                gameObject.SetActive(false);
                return;
            }

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
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
