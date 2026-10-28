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
        private LineRenderer _touchRing;
        private static Sprite _circle;

        public void Initialise(string id, Color color)
        {
            playerId = id;
            _circle = _circle ?? MakeCircle();
            _body = gameObject.AddComponent<SpriteRenderer>(); _body.sprite = _circle; _body.color = color; _body.sortingOrder = 2;
            var text = new GameObject("Label"); text.transform.SetParent(transform); text.transform.localPosition = new Vector3(0, .65f, 0); _label = text.AddComponent<TextMesh>(); _label.anchor = TextAnchor.MiddleCenter; _label.alignment = TextAlignment.Center; _label.characterSize = .07f; _label.fontSize = 28; _label.color = Color.white;
            _aim = CreateLine("Aim Ruler"); _aim.positionCount = 2; _aim.startWidth = .06f; _aim.endWidth = .025f; _aim.startColor = Color.yellow; _aim.endColor = Color.yellow;
            _touchRing = CreateLine("Aim Touch Ring"); _touchRing.loop = true; _touchRing.positionCount = 32; _touchRing.startWidth = .025f; _touchRing.endWidth = .025f; _touchRing.startColor = new Color(1f, .85f, 0f, .65f); _touchRing.endColor = new Color(1f, .85f, 0f, .65f);
        }

        public void Present(PlayerState player, bool visible, bool showAim, bool showTouchRing, float touchRadius, float aimLineExtraLength, float aimPreviewLength)
        {
            if (_body == null) return;
            transform.position = new Vector3(player.Position.X, player.Position.Y, 0);
            _body.enabled = visible; _label.gameObject.SetActive(visible); _label.text = player.Name + "\nHP " + player.Hp;
            _body.color = player.IsAlive ? _body.color : new Color(.3f, .3f, .3f, .45f);
            _aim.enabled = visible && showAim && player.IsAlive;
            _touchRing.enabled = visible && showTouchRing && player.IsAlive;
            if (_touchRing.enabled) for (int i = 0; i < _touchRing.positionCount; i++) { float angle = Mathf.PI * 2f * i / _touchRing.positionCount; _touchRing.SetPosition(i, transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * touchRadius); }
            if (_aim.enabled) { var direction = new Vector3(player.Facing.X, player.Facing.Y, 0); float start = touchRadius; float length = Mathf.Max(2f, aimPreviewLength + aimLineExtraLength); _aim.SetPosition(0, transform.position + direction * start); _aim.SetPosition(1, transform.position + direction * (start + length)); }
        }

        private static Sprite MakeCircle()
        {
            const int size = 64; var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) { float dx = (x - 31.5f) / 31.5f; float dy = (y - 31.5f) / 31.5f; texture.SetPixel(x, y, dx * dx + dy * dy <= 1 ? Color.white : Color.clear); }
            texture.Apply(); return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
        }

        private LineRenderer CreateLine(string name)
        {
            var lineObject = new GameObject(name); lineObject.transform.SetParent(transform, false);
            var line = lineObject.AddComponent<LineRenderer>();
            // This is the material already referenced by a SpriteRenderer in this build,
            // so WebGL's shader stripping cannot remove it. Avoid Shader.Find at runtime.
            line.sharedMaterial = _body.sharedMaterial;
            line.sortingOrder = 1;
            return line;
        }
    }
}
