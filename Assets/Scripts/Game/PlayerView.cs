using System;
using HitMe.Core;
using TMPro;
using UnityEngine;

namespace HitMe.Game
{
    public sealed class PlayerView : MonoBehaviour
    {
        public const int TotalCharacterCount = 7;

        public static readonly string[] CharacterNames =
        {
            "Nam Làng",
            "Nữ Năng Động",
            "Quậy Boy",
            "Cô Ba",
            "Anh Teen",
            "Chó Vàng",
            "Mèo Mun"
        };

        private static readonly float[] CharacterBodyScales =
        {
            0.7235f, // 0: Nam Làng (311px @ 150 PPU = 2.073u -> 1.50u)
            1.2016f, // 1: Nữ Năng Động (181px @ 150 PPU = 1.207u -> 1.45u)
            0.8638f, // 2: Quậy Boy (257px @ 150 PPU = 1.713u -> 1.48u)
            0.7740f, // 3: Cô Ba (281px @ 150 PPU = 1.873u -> 1.45u)
            0.7985f, // 4: Anh Teen (278px @ 150 PPU = 1.853u -> 1.48u)
            1.1820f, // 5: Chó Vàng (138px @ 150 PPU = 0.920u -> 1.0875u, 72.5% human)
            1.0660f  // 6: Mèo Mun (153px @ 150 PPU = 1.020u -> 1.0875u, 72.5% human)
        };

        public string playerId;
        public int characterIndex;

        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer _body;
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _shadow;
        [SerializeField] private SpriteRenderer _footRing;
        [SerializeField] private LineRenderer _hitboxCircle;
        [SerializeField] private TextMeshPro _label;
        [SerializeField] private LineRenderer _aim;
        [SerializeField] private LineRenderer _touchRing;

        private static Sprite _defaultShadow;
        private static Sprite _defaultRing;
        private static TMP_FontAsset _defaultFont;
        private static Sprite[] _defaultSprites;
        private static Sprite[] _namLangPoses;
        private static Sprite[] _anhNonLaPoses;

        private static readonly string[][] PrebuiltCharacterHpLabels = BuildCharacterHpLabels();

        private static string[][] BuildCharacterHpLabels()
        {
            var table = new string[TotalCharacterCount][];
            for (int c = 0; c < TotalCharacterCount; c++)
            {
                string name = CharacterNames[c];
                table[c] = new[]
                {
                    name + "\n0/3 HP",
                    name + "\n1/3 HP",
                    name + "\n2/3 HP",
                    name + "\n3/3 HP"
                };
            }
            return table;
        }

        public static string GetPrebuiltHpLabel(int characterIndex, int hp)
        {
            int c = Mathf.Clamp(characterIndex, 0, TotalCharacterCount - 1);
            int h = Mathf.Clamp(hp, 0, 3);
            return PrebuiltCharacterHpLabels[c][h];
        }

        private string _lastLabelName;
        private int _lastLabelCharIdx = -1;
        private int _lastLabelHp = -999;
        private readonly string[] _cachedHpLabels = new string[4];
        private float _lastTouchRadius = -1f;

        public static void ConfigureSharedResources(GameConfigAsset configAsset)
        {
            if (configAsset == null) return;
            if (configAsset.shadowSprite != null) _defaultShadow = configAsset.shadowSprite;
            if (configAsset.footRingSprite != null) _defaultRing = configAsset.footRingSprite;
            if (configAsset.vietnameseFont != null) _defaultFont = configAsset.vietnameseFont;
            if (configAsset.characterIdleSprites != null && configAsset.characterIdleSprites.Length >= TotalCharacterCount)
            {
                _defaultSprites = configAsset.characterIdleSprites;
            }
            if (configAsset.namLangPoseSprites != null && configAsset.namLangPoseSprites.Length >= 5)
            {
                _namLangPoses = configAsset.namLangPoseSprites;
            }
            if (configAsset.anhNonLaPoseSprites != null && configAsset.anhNonLaPoseSprites.Length >= 5)
            {
                _anhNonLaPoses = configAsset.anhNonLaPoseSprites;
            }
        }

        private void Awake()
        {
            BindComponents();
        }

        private void BindComponents()
        {
            if (_shadow == null)
            {
                var s = transform.Find("Shadow");
                if (s != null) _shadow = s.GetComponent<SpriteRenderer>();
            }
            if (_footRing == null)
            {
                var r = transform.Find("FootRing");
                if (r != null) _footRing = r.GetComponent<SpriteRenderer>();
            }
            if (_hitboxCircle == null)
            {
                var hb = transform.Find("DebugHitbox");
                if (hb != null) _hitboxCircle = hb.GetComponent<LineRenderer>();
            }
            if (_body == null)
            {
                var b = transform.Find("Body");
                if (b != null)
                {
                    _body = b.GetComponent<SpriteRenderer>();
                    _animator = b.GetComponent<Animator>();
                }
                else
                {
                    _body = GetComponent<SpriteRenderer>();
                    _animator = GetComponent<Animator>();
                }
            }
            if (_label == null)
            {
                _label = GetComponentInChildren<TextMeshPro>();
            }
            if (_aim == null)
            {
                var a = transform.Find("Aim Ruler");
                if (a != null) _aim = a.GetComponent<LineRenderer>();
            }
            if (_touchRing == null)
            {
                var t = transform.Find("Aim Touch Ring");
                if (t != null) _touchRing = t.GetComponent<LineRenderer>();
            }
        }

        public void Initialise(string id, Color color)
        {
            playerId = id;
            int idx = 0;
            if (id.StartsWith("bot-", StringComparison.Ordinal))
            {
                int botNum;
                if (int.TryParse(id.Substring(4), out botNum))
                {
                    idx = ((botNum - 1) % (TotalCharacterCount - 1)) + 1;
                }
            }
            Initialise(id, color, idx);
        }

        public void Initialise(string id, Color color, int charIndex)
        {
            playerId = id;
            characterIndex = ((charIndex % TotalCharacterCount) + TotalCharacterCount) % TotalCharacterCount;
            _lastLabelName = null;
            _lastLabelHp = -999;
            BindComponents();

            EnsureStaticResources();

            // 1. Shadow: Standardized procedural soft shadow for playerRadius = 0.55f (width = 1.125u, 12.5% arena width)
            if (_shadow == null)
            {
                var sGo = new GameObject("Shadow");
                sGo.transform.SetParent(transform, false);
                sGo.transform.localPosition = new Vector3(0f, -0.05f, 0f);
                _shadow = sGo.AddComponent<SpriteRenderer>();
                _shadow.sprite = _defaultShadow;
                _shadow.sortingOrder = 0;
                _shadow.color = new Color(0f, 0f, 0f, 0.45f);
                sGo.transform.localScale = new Vector3(0.66f, 0.66f, 1f);
            }
            else
            {
                _shadow.sprite = _defaultShadow;
                _shadow.color = new Color(0f, 0f, 0f, 0.45f);
                _shadow.transform.localScale = new Vector3(0.66f, 0.66f, 1f);
                _shadow.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            }

            // 2. Foot Ring: Standardized procedural ring for playerRadius = 0.55f (width = 1.125u, height = 0.675u, h/w = 0.6)
            if (_footRing == null)
            {
                var rGo = new GameObject("FootRing");
                rGo.transform.SetParent(transform, false);
                rGo.transform.localPosition = new Vector3(0f, 0.02f, 0f);
                _footRing = rGo.AddComponent<SpriteRenderer>();
                _footRing.sprite = _defaultRing;
                _footRing.sortingOrder = 1;
                _footRing.color = color;
                rGo.transform.localScale = new Vector3(0.66f, 0.66f, 1f);
            }
            else
            {
                _footRing.sprite = _defaultRing;
                _footRing.color = color;
                _footRing.transform.localScale = new Vector3(0.66f, 0.66f, 1f);
                _footRing.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            }

            // 3. Body
            if (_body == null)
            {
                var bGo = new GameObject("Body");
                bGo.transform.SetParent(transform, false);
                bGo.transform.localPosition = Vector3.zero;
                _body = bGo.AddComponent<SpriteRenderer>();
            }

            if (_animator != null)
            {
                _animator.runtimeAnimatorController = null;
                _animator.enabled = false;
            }

            _body.sortingOrder = 2;
            ApplyCharacterVisual(characterIndex);

            // 4. Label (TextMeshPro with Vietnamese Font)
            if (_label == null)
            {
                var lGo = new GameObject("Label");
                lGo.transform.SetParent(transform, false);
                lGo.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                _label = lGo.AddComponent<TextMeshPro>();
                _label.rectTransform.sizeDelta = new Vector2(2.4f, 0.9f);
                _label.alignment = TextAlignmentOptions.Center;
                _label.fontSize = 1.05f;
                _label.color = Color.white;
                if (_defaultFont != null) _label.font = _defaultFont;
            }

            // 5. Aim lines
            if (_aim == null)
            {
                _aim = CreateLine("Aim Ruler");
                _aim.positionCount = 2;
                _aim.startWidth = 0.06f;
                _aim.endWidth = 0.025f;
                _aim.startColor = Color.yellow;
                _aim.endColor = Color.yellow;
            }

            if (_touchRing == null)
            {
                _touchRing = CreateLine("Aim Touch Ring");
                _touchRing.useWorldSpace = false;
                _touchRing.loop = true;
                _touchRing.positionCount = 32;
                _touchRing.startWidth = 0.025f;
                _touchRing.endWidth = 0.025f;
                _touchRing.startColor = new Color(1f, 0.85f, 0f, 0.65f);
                _touchRing.endColor = new Color(1f, 0.85f, 0f, 0.65f);
            }
            else
            {
                _touchRing.useWorldSpace = false;
            }
        }

        public void ApplyCharacterVisual(int index)
        {
            int assetIdx = ((index % TotalCharacterCount) + TotalCharacterCount) % TotalCharacterCount;
            characterIndex = assetIdx;
            if (_body == null) return;

            if (_defaultSprites != null && assetIdx < _defaultSprites.Length && _defaultSprites[assetIdx] != null)
            {
                _body.sprite = _defaultSprites[assetIdx];
            }

            float scale = CharacterBodyScales[assetIdx];
            _body.transform.localScale = new Vector3(scale, scale, 1f);
            _body.color = Color.white;
        }

        public void SetHitboxVisible(bool visible, float radius = 0.55f)
        {
            if (_hitboxCircle == null)
            {
                var hbGo = new GameObject("DebugHitbox");
                hbGo.transform.SetParent(transform, false);
                hbGo.transform.localPosition = Vector3.zero;
                _hitboxCircle = hbGo.AddComponent<LineRenderer>();
                _hitboxCircle.useWorldSpace = false;
                _hitboxCircle.loop = true;
                _hitboxCircle.positionCount = 36;
                _hitboxCircle.startWidth = 0.035f;
                _hitboxCircle.endWidth = 0.035f;
                _hitboxCircle.startColor = new Color(0.1f, 1f, 0.4f, 0.95f);
                _hitboxCircle.endColor = new Color(0.1f, 1f, 0.4f, 0.95f);
                if (_body != null && _body.sharedMaterial != null) _hitboxCircle.sharedMaterial = _body.sharedMaterial;
                _hitboxCircle.sortingOrder = 4;
                for (int i = 0; i < 36; i++)
                {
                    float angle = Mathf.PI * 2f * i / 36;
                    _hitboxCircle.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
                }
            }
            if (_hitboxCircle != null)
            {
                _hitboxCircle.useWorldSpace = false;
                _hitboxCircle.enabled = visible;
            }
        }

        public void PlayAnimation(string stateName)
        {
            if (_body == null) return;
            int idx = Mathf.Clamp(characterIndex, 0, TotalCharacterCount - 1);
            if (_defaultSprites != null && idx < _defaultSprites.Length && _defaultSprites[idx] != null)
            {
                _body.sprite = _defaultSprites[idx];
            }
        }

        public void Present(PlayerState player, bool visible, bool showAim, bool showTouchRing, float touchRadius, float aimLineExtraLength, float aimPreviewLength)
        {
            if (_body == null) return;
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            transform.position = new Vector3(player.Position.X, player.Position.Y, 0f);

            _body.enabled = visible;
            if (_shadow != null) _shadow.enabled = visible;
            if (_footRing != null) _footRing.enabled = visible;

            if (_label != null)
            {
                if (_label.gameObject.activeSelf != visible)
                {
                    _label.gameObject.SetActive(visible);
                }
                if (visible)
                {
                    _label.transform.localPosition = new Vector3(0f, 1.25f, 0f);
                    _label.fontSize = 0.85f;
                    UpdateLabelCached(player.Name, player.Hp);
                }
            }

            int idx = Mathf.Clamp(characterIndex, 0, TotalCharacterCount - 1);
            if (_defaultSprites != null && idx < _defaultSprites.Length && _defaultSprites[idx] != null)
            {
                _body.sprite = _defaultSprites[idx];
            }

            _body.color = player.IsAlive ? Color.white : new Color(0.35f, 0.35f, 0.35f, 0.5f);

            if (_aim != null)
            {
                _aim.enabled = visible && showAim && player.IsAlive;
                if (_aim.enabled)
                {
                    var direction = new Vector3(player.Facing.X, player.Facing.Y, 0f);
                    float start = touchRadius;
                    float length = Mathf.Max(2f, aimPreviewLength + aimLineExtraLength);
                    _aim.SetPosition(0, transform.position + direction * start);
                    _aim.SetPosition(1, transform.position + direction * (start + length));
                }
            }

            if (_touchRing != null)
            {
                _touchRing.enabled = visible && showTouchRing && player.IsAlive;
                if (_touchRing.enabled && Mathf.Abs(touchRadius - _lastTouchRadius) > 0.001f)
                {
                    _lastTouchRadius = touchRadius;
                    _touchRing.useWorldSpace = false;
                    int count = _touchRing.positionCount;
                    for (int i = 0; i < count; i++)
                    {
                        float angle = Mathf.PI * 2f * i / count;
                        _touchRing.SetPosition(i, new Vector3(Mathf.Cos(angle) * touchRadius, Mathf.Sin(angle) * touchRadius, 0f));
                    }
                }
            }
        }

        private void UpdateLabelCached(string playerName, int hp)
        {
            int clampedHp = Mathf.Clamp(hp, 0, 3);
            int charIdx = Mathf.Clamp(characterIndex, 0, TotalCharacterCount - 1);
            if (charIdx >= 0 && charIdx < TotalCharacterCount &&
                string.Equals(CharacterNames[charIdx], playerName, StringComparison.Ordinal))
            {
                if (_lastLabelCharIdx != charIdx || _lastLabelHp != clampedHp)
                {
                    _lastLabelCharIdx = charIdx;
                    _lastLabelHp = clampedHp;
                    _label.text = PrebuiltCharacterHpLabels[charIdx][clampedHp];
                }
                return;
            }

            if (!string.Equals(_lastLabelName, playerName, StringComparison.Ordinal))
            {
                _lastLabelName = playerName;
                _cachedHpLabels[0] = playerName + "\n0/3 HP";
                _cachedHpLabels[1] = playerName + "\n1/3 HP";
                _cachedHpLabels[2] = playerName + "\n2/3 HP";
                _cachedHpLabels[3] = playerName + "\n3/3 HP";
                _lastLabelHp = -999;
            }

            if (_lastLabelHp != clampedHp)
            {
                _lastLabelHp = clampedHp;
                _label.text = _cachedHpLabels[clampedHp];
            }
        }

        private static void EnsureStaticResources()
        {
#if UNITY_EDITOR
            if (_defaultShadow == null)
            {
                _defaultShadow = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/procedural_shadow.png");
            }
            if (_defaultRing == null)
            {
                _defaultRing = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/procedural_foot_ring.png");
            }
            if (_defaultFont == null)
            {
                _defaultFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Bold SDF.asset");
            }
            if (_defaultSprites == null || _defaultSprites.Length < TotalCharacterCount)
            {
                _defaultSprites = new Sprite[TotalCharacterCount];
                _defaultSprites[0] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_idle.png");
                _defaultSprites[1] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/co_nang_the_thao_idle.png");
                _defaultSprites[2] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_nui_idle.png");
                _defaultSprites[3] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/mai_hong_idle.png");
                _defaultSprites[4] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_idle.png");
                _defaultSprites[5] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/chu_cho_vang_idle.png");
                _defaultSprites[6] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/meo_mun_idle.png");
            }
            if (_namLangPoses == null || _namLangPoses.Length < 5)
            {
                _namLangPoses = new Sprite[5];
                _namLangPoses[0] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_idle.png");
                _namLangPoses[1] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_aim.png");
                _namLangPoses[2] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_throw_release.png");
                _namLangPoses[3] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_hit.png");
                _namLangPoses[4] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_defeat.png");
            }
            if (_anhNonLaPoses == null || _anhNonLaPoses.Length < 5)
            {
                _anhNonLaPoses = new Sprite[5];
                _anhNonLaPoses[0] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_idle.png");
                _anhNonLaPoses[1] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_aim.png");
                _anhNonLaPoses[2] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_throw_release.png");
                _anhNonLaPoses[3] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_hit.png");
                _anhNonLaPoses[4] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_defeat.png");
            }
#endif
        }

        private LineRenderer CreateLine(string name)
        {
            var lineObject = new GameObject(name);
            lineObject.transform.SetParent(transform, false);
            var line = lineObject.AddComponent<LineRenderer>();
            if (_body != null && _body.sharedMaterial != null)
            {
                line.sharedMaterial = _body.sharedMaterial;
            }
            line.sortingOrder = 3;
            return line;
        }
    }
}
