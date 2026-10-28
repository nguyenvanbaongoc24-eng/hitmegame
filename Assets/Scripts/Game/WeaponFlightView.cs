using UnityEngine;

namespace HitMe.Game
{
    public sealed class WeaponFlightView : MonoBehaviour
    {
        public Vector3 StartPoint;
        public Vector3 EndPoint;
        public float Duration;
        private float _elapsed;
        public void Initialise(Vector3 start, Vector3 end, float duration)
        {
            StartPoint = start; EndPoint = end; Duration = duration; transform.position = start;
            var renderer = gameObject.AddComponent<SpriteRenderer>(); renderer.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f)); renderer.color = Color.yellow; transform.localScale = Vector3.one * .2f; renderer.sortingOrder = 4;
        }
        private void Update() { _elapsed += Time.deltaTime; transform.position = Vector3.Lerp(StartPoint, EndPoint, Mathf.Clamp01(_elapsed / Duration)); if (_elapsed >= Duration) Destroy(gameObject); }
    }
}
