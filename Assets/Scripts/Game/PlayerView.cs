using HitMe.Core;
using UnityEngine;

namespace HitMe.Game
{
    public sealed class PlayerView : MonoBehaviour
    {
        public string playerId;
        private SpriteRenderer _body;
        private TextMesh _label;
        private LineRenderer _aim;
        private static Sprite _circle;

        public void Initialise(string id, Color color)
        {
            playerId = id;
            _circle = _circle ?? MakeCircle();
            _body = gameObject.AddComponent<SpriteRenderer>(); _body.sprite = _circle; _body.color = color; _body.sortingOrder = 2;
            var text = new GameObject("Label"); text.transform.SetParent(transform); text.transform.localPosition = new Vector3(0, .65f, 0); _label = text.AddComponent<TextMesh>(); _label.anchor = TextAnchor.MiddleCenter; _label.alignment = TextAlignment.Center; _label.characterSize = .07f; _label.fontSize = 28; _label.color = Color.white;
            _aim = gameObject.AddComponent<LineRenderer>(); _aim.positionCount = 2; _aim.startWidth = .06f; _aim.endWidth = .025f; _aim.material = new Material(Shader.Find("Sprites/Default")); _aim.startColor = Color.yellow; _aim.endColor = Color.yellow; _aim.sortingOrder = 1;
        }

        public void Present(PlayerState player, bool visible, bool showAim)
        {
            if (_body == null) return;
            transform.position = new Vector3(player.Position.X, player.Position.Y, 0);
            _body.enabled = visible; _label.gameObject.SetActive(visible); _label.text = player.Name + "\nHP " + player.Hp;
            _body.color = player.IsAlive ? _body.color : new Color(.3f, .3f, .3f, .45f);
            _aim.enabled = visible && showAim && player.IsAlive;
            if (_aim.enabled) { _aim.SetPosition(0, transform.position); var end = transform.position + new Vector3(player.Facing.X, player.Facing.Y, 0) * 2f; _aim.SetPosition(1, end); }
        }

        private static Sprite MakeCircle()
        {
            const int size = 64; var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) { float dx = (x - 31.5f) / 31.5f; float dy = (y - 31.5f) / 31.5f; texture.SetPixel(x, y, dx * dx + dy * dy <= 1 ? Color.white : Color.clear); }
            texture.Apply(); return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
        }
    }
}
