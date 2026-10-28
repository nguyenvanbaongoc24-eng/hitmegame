using HitMe.Core;
using UnityEngine;

namespace HitMe.Game
{
    public sealed class ArenaView : MonoBehaviour
    {
        public GameConfigAsset configAsset;
        private SpriteRenderer _floor;
        private readonly SpriteRenderer[] _walls = new SpriteRenderer[4];
        private static Sprite _white;
        public GameConfig Config { get { return configAsset.ToCore(); } }

        private void Awake()
        {
            // WhiteTexture is one pixel. Use one pixel per world unit so the
            // configured 10 x 16 scale is an arena, not a 0.1 x 0.16 dot.
            _white = _white ?? Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1f);
            var config = Config;
            _floor = Make("Floor", new Color(.23f, .25f, .27f), Vector2.zero, new Vector2(config.ArenaWidth, config.ArenaHeight), -1);
            float t = .12f;
            _walls[0] = Make("Wall Top", Color.white, new Vector2(0, config.ArenaHeight / 2), new Vector2(config.ArenaWidth + t, t), 0);
            _walls[1] = Make("Wall Bottom", Color.white, new Vector2(0, -config.ArenaHeight / 2), new Vector2(config.ArenaWidth + t, t), 0);
            _walls[2] = Make("Wall Left", Color.white, new Vector2(-config.ArenaWidth / 2, 0), new Vector2(t, config.ArenaHeight), 0);
            _walls[3] = Make("Wall Right", Color.white, new Vector2(config.ArenaWidth / 2, 0), new Vector2(t, config.ArenaHeight), 0);
        }

        private SpriteRenderer Make(string label, Color color, Vector2 position, Vector2 scale, int order)
        {
            var go = new GameObject(label); go.transform.SetParent(transform); go.transform.position = position;
            go.transform.localScale = scale; var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = _white; renderer.color = color; renderer.sortingOrder = order; return renderer;
        }
    }
}
