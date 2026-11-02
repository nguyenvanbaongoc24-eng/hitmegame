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
            PlayAt(position, customFrames, null);
        }

        public void PlayAt(Vector3 position, Sprite[] customFrames, string rarity)
        {
            if (customFrames != null && customFrames.Length > 0)
            {
                frames = customFrames;
            }
            transform.position = position;
            _timer = 0f;
            _index = 0;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            ApplyRarityEffect(rarity);
            if (frames != null && frames.Length > 0 && spriteRenderer != null)
            {
                spriteRenderer.sprite = frames[0];
            }
            gameObject.SetActive(true);
        }

        public void ApplyRarityEffect(string rarity)
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;

            switch (rarity)
            {
                case "Rare":
                    spriteRenderer.color = new Color(0.35f, 0.85f, 1f, 1f); // Cyan splash
                    transform.localScale = Vector3.one * 1.15f;
                    break;
                case "Epic":
                    spriteRenderer.color = new Color(0.85f, 0.45f, 1f, 1f); // Royal purple aura
                    transform.localScale = Vector3.one * 1.30f;
                    break;
                case "Legendary":
                    spriteRenderer.color = new Color(1f, 0.80f, 0.15f, 1f); // Golden sunburst
                    transform.localScale = Vector3.one * 1.45f;
                    break;
                case "Mythic":
                    spriteRenderer.color = new Color(1f, 0.25f, 0.45f, 1f); // Mythic prismatic flare
                    transform.localScale = Vector3.one * 1.60f;
                    break;
                default: // Common
                    spriteRenderer.color = Color.white;
                    transform.localScale = Vector3.one;
                    break;
            }
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
