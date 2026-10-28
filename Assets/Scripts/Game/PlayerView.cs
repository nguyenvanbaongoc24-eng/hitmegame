using System;
using HitMe.Core;
using TMPro;
using UnityEngine;

namespace HitMe.Game
{
    public sealed class PlayerView : MonoBehaviour
    {
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
        private static RuntimeAnimatorController[] _controllers;
        private static Sprite[] _defaultSprites;

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
            if (id.StartsWith("bot-"))
            {
                int botNum;
                if (int.TryParse(id.Replace("bot-", ""), out botNum))
                {
                    idx = (botNum % 5) + 1; // 1 to 5
                }
            }
            Initialise(id, color, idx);
        }

        public void Initialise(string id, Color color, int charIndex)
        {
            playerId = id;
            characterIndex = charIndex;
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

            // 3. Body & Animator
            if (_body == null)
            {
                var bGo = new GameObject("Body");
                bGo.transform.SetParent(transform, false);
                bGo.transform.localPosition = Vector3.zero;
                _body = bGo.AddComponent<SpriteRenderer>();
                _animator = bGo.AddComponent<Animator>();
            }

            _body.sortingOrder = 2;
            ApplyCharacterVisual(charIndex);

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
                _touchRing.loop = true;
                _touchRing.positionCount = 32;
                _touchRing.startWidth = 0.025f;
                _touchRing.endWidth = 0.025f;
                _touchRing.startColor = new Color(1f, 0.85f, 0f, 0.65f);
                _touchRing.endColor = new Color(1f, 0.85f, 0f, 0.65f);
            }
        }

        public void ApplyCharacterVisual(int index)
        {
            characterIndex = index;
            if (_body == null) return;

            int assetIdx = index % 5;
            if (_controllers != null && assetIdx < _controllers.Length && _controllers[assetIdx] != null && _animator != null)
            {
                _animator.runtimeAnimatorController = _controllers[assetIdx];
            }

            if (_defaultSprites != null && assetIdx < _defaultSprites.Length && _defaultSprites[assetIdx] != null)
            {
                _body.sprite = _defaultSprites[assetIdx];
            }

            // Target scale: Nam Lang height reduced from 2.073 to 1.50 world units (-28%, matching mockup ~13% arena height)
            // Scale factor for humans = 1.50 / 2.0733 = 0.7235f
            // Chú Chó Vàng height = 72.5% of human = 1.0875 units -> scale 1.182f
            if (assetIdx == 4)
            {
                _body.transform.localScale = new Vector3(1.182f, 1.182f, 1f);
            }
            else
            {
                _body.transform.localScale = new Vector3(0.7235f, 0.7235f, 1f);
            }

            // 6th player reuses character 0 with a color tint (Magenta / Purple)
            if (index == 5)
            {
                _body.color = new Color(1f, 0.55f, 0.85f, 1f); // Distinct tinted tint
            }
            else
            {
                _body.color = Color.white;
            }
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
                _hitboxCircle.startColor = new Color(0.1f, 1f, 0.4f, 0.95f); // Vibrant neon green
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
            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                _animator.Play(stateName);
            }
        }

        public void Present(PlayerState player, bool visible, bool showAim, bool showTouchRing, float touchRadius, float aimLineExtraLength, float aimPreviewLength)
        {
            if (_body == null) return;
            transform.position = new Vector3(player.Position.X, player.Position.Y, 0f);

            _body.enabled = visible;
            if (_shadow != null) _shadow.enabled = visible;
            if (_footRing != null) _footRing.enabled = visible;

            if (_label != null)
            {
                _label.gameObject.SetActive(visible);
                _label.text = player.Name + "\n" + player.Hp + "/3 HP";
            }

            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                if (!player.IsAlive)
                {
                    _animator.SetBool("IsEliminated", true);
                }
                else
                {
                    _animator.SetBool("IsEliminated", false);
                    _animator.SetBool("IsAiming", showAim);
                }
            }

            if (!player.IsAlive)
            {
                _body.color = new Color(0.35f, 0.35f, 0.35f, 0.5f);
            }

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
                if (_touchRing.enabled)
                {
                    for (int i = 0; i < _touchRing.positionCount; i++)
                    {
                        float angle = Mathf.PI * 2f * i / _touchRing.positionCount;
                        _touchRing.SetPosition(i, transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * touchRadius);
                    }
                }
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
            if (_controllers == null)
            {
                _controllers = new RuntimeAnimatorController[5];
                _controllers[0] = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Animations/Cau_Be_Lang/Cau_Be_Lang_Controller.controller");
                _controllers[1] = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Animations/Anh_Non_La/Anh_Non_La_Controller.controller");
                _controllers[2] = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Animations/ANH_NÚI/ANH_NÚI_Controller.controller");
                _controllers[3] = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Animations/CÔ_NÀNG_THỂ_THAO/CÔ_NÀNG_THỂ_THAO_Controller.controller");
                _controllers[4] = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Animations/CHÚ_CHÓ_VÀNG/CHÚ_CHÓ_VÀNG_Controller.controller");
            }
            if (_defaultSprites == null)
            {
                _defaultSprites = new Sprite[5];
                _defaultSprites[0] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_idle.png");
                _defaultSprites[1] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_idle.png");
                _defaultSprites[2] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_nui_idle.png");
                _defaultSprites[3] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/mai_hong_idle.png");
                _defaultSprites[4] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/chu_cho_vang_idle.png");
            }
#endif
        }

#if UNITY_EDITOR
        private static Sprite FindSubSprite(string path, string pattern)
        {
            var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is Sprite s && s.name.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return s;
                }
            }
            foreach (var a in assets)
            {
                if (a is Sprite s) return s;
            }
            return null;
        }
#endif

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
