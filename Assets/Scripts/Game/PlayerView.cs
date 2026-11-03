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
            "Cô Ba",
            "Anh Teen",
            "Quậy Boy",
            "Chó Vàng",
            "Mèo Mun"
        };

        private static readonly float[] CharacterBodyScales =
        {
            0.7235f, // 0: Nam Làng (311px @ 150 PPU = 2.073u -> 1.50u)
            1.2016f, // 1: Nữ Năng Động (181px @ 150 PPU = 1.207u -> 1.45u)
            0.7740f, // 2: Cô Ba (281px @ 150 PPU = 1.873u -> 1.45u)
            0.7985f, // 3: Anh Teen (278px @ 150 PPU = 1.853u -> 1.48u)
            0.8638f, // 4: Quậy Boy (257px @ 150 PPU = 1.713u -> 1.48u)
            1.1820f, // 5: Chó Vàng (138px @ 150 PPU = 0.920u -> 1.0875u, 72.5% human)
            1.0660f  // 6: Mèo Mun (153px @ 150 PPU = 1.020u -> 1.0875u, 72.5% human)
        };

        public static readonly float[] CharacterSinkRatios =
        {
            0.18f, // 0: Nam Làng (18% chìm -> đầu nhìn thấy 56.7%)
            0.18f, // 1: Nữ Năng Động (18% chìm -> đầu nhìn thấy 56.1%)
            0.10f, // 2: Cô Ba (10% chìm -> đầu nhìn thấy 58.0%)
            0.10f, // 3: Anh Teen (10% chìm -> đầu nhìn thấy 58.3%)
            0.00f, // 4: Quậy Boy (tạm thời 0%, chờ ảnh Chibi mới)
            0.08f, // 5: Chó Vàng (8% chìm -> đầu nhìn thấy 57.2%)
            0.08f  // 6: Mèo Mun (8% chìm -> đầu nhìn thấy 58.0%)
        };

        public string playerId;
        public int characterIndex;

        // V4 Visual Scaling: Hitbox/physics PlayerRadius remains constant per GameConfig
        public static float VisualCharacterScale = 1.0f;
        public static float VisualHitRingScale = 1.0f;
        public static bool CustomScaleApplied;

        public static void SetVisualCharacterScale(float scale, bool fromUrl = false)
        {
            VisualCharacterScale = scale;
            if (fromUrl) CustomScaleApplied = true;
        }

        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer _body;
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _shadow;
        [SerializeField] private SpriteRenderer _footRing;
        [SerializeField] private SpriteRenderer _footRingFront;
        [SerializeField] private LineRenderer _hitboxCircle;
        [SerializeField] private TextMeshPro _label;
        [SerializeField] private LineRenderer _aim;
        [SerializeField] private LineRenderer _touchRing;

        [Header("E3 Aim Visual Components")]
        [SerializeField] private MeshFilter _aimConeFilter;
        [SerializeField] private MeshRenderer _aimConeRenderer;
        [SerializeField] private LineRenderer _aimConeOutline;
        [SerializeField] private SpriteRenderer[] _aimDots;
        [SerializeField] private SpriteRenderer _aimReticle;
        [SerializeField] private SpriteRenderer _aimWeaponIcon;
        private Mesh _aimConeMesh;

        public static readonly Color[] PlayerRingPalette = new Color[]
        {
            new Color(0.00f, 0.898f, 1.00f, 0.95f), // 0: Self (Cyan #00E5FF)
            new Color(1.00f, 0.843f, 0.00f, 0.95f), // 1: Opponent 1 (Gold #FFD700)
            new Color(1.00f, 0.200f, 0.400f, 0.95f), // 2: Opponent 2 (Pink/Red #FF3366)
            new Color(0.00f, 1.000f, 0.533f, 0.95f), // 3: Opponent 3 (Green #00FF88)
            new Color(0.702f, 0.400f, 1.000f, 0.95f), // 4: Opponent 4 (Purple #B366FF)
            new Color(1.00f, 0.600f, 0.000f, 0.95f)  // 5: Opponent 5 (Orange #FF9900)
        };

        private static Sprite _defaultShadow;
        private static Sprite _defaultRing;
        private static Sprite _defaultRingBack;
        private static Sprite _defaultRingFront;
        private static Sprite _defaultDot;
        private static Sprite _defaultWeaponIcon;
        private static TMP_FontAsset _defaultFont;
        private static Sprite[] _defaultSprites;
        private static Sprite[] _throwSprites;
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

        private string _currentState = "Idle";
        private Vector3 _baseBodyScale = Vector3.one;
        private float _hitReactionTimer;
        private float _spawnTimer;
        private float _throwTimer;
        private float _defeatStartTime;
        private Vector2 _currentAimDirection = Vector2.up;
        private Color _ringBaseColor = Color.white;
        private float _sinkOffsetY;

        private void Update()
        {
            if (_body == null) return;

            bool reducedMotion = string.Equals(PlayerPrefs.GetString("HitMe_ReducedMotion", "0"), "1", StringComparison.Ordinal);

            if (_currentState == "Spawn")
            {
                _spawnTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(_spawnTimer / 0.40f);
                if (!reducedMotion)
                {
                    // Scale pops from 60% to 100% with bounce overshoot
                    float bounceFactor = Mathf.Sin(progress * Mathf.PI) * 0.15f;
                    float s = Mathf.Lerp(0.60f, 1.0f, progress) + bounceFactor;
                    _body.transform.localScale = _baseBodyScale * s;
                    _body.transform.localPosition = new Vector3(0f, _sinkOffsetY + (1f - progress) * 0.10f, 0f);
                }
                else
                {
                    _body.transform.localScale = _baseBodyScale * Mathf.Lerp(0.60f, 1.0f, progress);
                    _body.transform.localPosition = new Vector3(0f, _sinkOffsetY, 0f);
                }

                if (progress >= 1.0f)
                {
                    _currentState = "Idle";
                    _body.transform.localScale = _baseBodyScale;
                    _body.transform.localPosition = new Vector3(0f, _sinkOffsetY, 0f);
                }
            }
            else if (_currentState == "Aim")
            {
                // Faster breathing pulse
                float breath = Mathf.Sin(Time.time * 7.0f) * 0.035f;
                if (!reducedMotion)
                {
                    // Tilt slightly in aim direction
                    float leanAngle = -_currentAimDirection.x * 7.0f;
                    _body.transform.localRotation = Quaternion.Euler(0f, 0f, leanAngle);
                    _body.transform.localPosition = new Vector3(_currentAimDirection.x * 0.04f, _sinkOffsetY + _currentAimDirection.y * 0.02f, 0f);
                    _body.transform.localScale = _baseBodyScale * (1f + breath);
                }
                else
                {
                    _body.transform.localRotation = Quaternion.identity;
                    _body.transform.localPosition = new Vector3(0f, _sinkOffsetY, 0f);
                    _body.transform.localScale = _baseBodyScale;
                }
            }
            else if (_currentState == "Throw")
            {
                if (_throwTimer > 0f)
                {
                    _throwTimer -= Time.deltaTime;
                    if (!reducedMotion)
                    {
                        float p = Mathf.Clamp01(1f - (_throwTimer / 0.35f));
                        float lunge = Mathf.Sin(p * Mathf.PI);
                        _body.transform.localPosition = new Vector3(_currentAimDirection.x * 0.08f * lunge, _sinkOffsetY + _currentAimDirection.y * 0.06f * lunge, 0f);
                    }
                    if (_throwTimer <= 0f)
                    {
                        _currentState = "Idle";
                        _body.transform.localRotation = Quaternion.identity;
                        _body.transform.localPosition = new Vector3(0f, _sinkOffsetY, 0f);
                        _body.transform.localScale = _baseBodyScale;
                        ApplyCharacterVisual(characterIndex);
                    }
                }
            }
            else if (_currentState == "Hit")
            {
                if (_hitReactionTimer > 0f)
                {
                    _hitReactionTimer -= Time.deltaTime;
                    // White flash
                    bool flash = Mathf.FloorToInt(_hitReactionTimer * 30f) % 2 == 0;
                    _body.color = flash ? new Color(1.8f, 1.8f, 1.8f, 1f) : Color.white;

                    if (!reducedMotion)
                    {
                        float vibration = Mathf.Sin(_hitReactionTimer * 60f) * 0.04f;
                        float recoil = (_hitReactionTimer / 0.25f) * -0.08f;
                        _body.transform.localPosition = new Vector3(vibration, _sinkOffsetY + recoil, 0f);
                    }
                    if (_hitReactionTimer <= 0f)
                    {
                        _currentState = "Idle";
                        _body.transform.localPosition = new Vector3(0f, _sinkOffsetY, 0f);
                        _body.color = Color.white;
                    }
                }
            }
            else if (_currentState == "Defeat" || _currentState == "KO")
            {
                _body.color = new Color(0.35f, 0.35f, 0.40f, 0.75f);
                float elapsed = Time.time - _defeatStartTime;
                if (!reducedMotion)
                {
                    float fallProgress = Mathf.Clamp01(elapsed / 0.30f);
                    float targetAngle = _body.flipX ? -75f : 75f;
                    _body.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, targetAngle, fallProgress));
                    _body.transform.localPosition = new Vector3(0f, _sinkOffsetY - 0.12f * fallProgress, 0f);
                }
                else
                {
                    _body.transform.localRotation = Quaternion.identity;
                    _body.transform.localPosition = new Vector3(0f, _sinkOffsetY - 0.08f, 0f);
                }

                // Fade out ground rings
                float ringAlpha = Mathf.Clamp01(1f - (elapsed / 0.40f));
                if (_footRing != null)
                {
                    _footRing.color = new Color(_ringBaseColor.r, _ringBaseColor.g, _ringBaseColor.b, _ringBaseColor.a * ringAlpha);
                }
                if (_footRingFront != null)
                {
                    _footRingFront.color = new Color(_ringBaseColor.r, _ringBaseColor.g, _ringBaseColor.b, _ringBaseColor.a * ringAlpha);
                }
            }
            else if (_currentState == "Win")
            {
                float bounce = Mathf.Abs(Mathf.Sin(Time.time * 7.0f));
                if (!reducedMotion)
                {
                    _body.transform.localPosition = new Vector3(0f, _sinkOffsetY + bounce * 0.18f, 0f);
                    _body.transform.localScale = _baseBodyScale * (1.0f + bounce * 0.08f);
                }
                else
                {
                    _body.transform.localPosition = new Vector3(0f, _sinkOffsetY, 0f);
                    _body.transform.localScale = _baseBodyScale * 1.05f;
                }

                // Ring flashing
                float ringFlash = Mathf.PingPong(Time.time * 5.0f, 1.0f);
                float ringA = Mathf.Lerp(0.35f, 1.0f, ringFlash);
                if (_footRing != null)
                {
                    _footRing.color = new Color(_ringBaseColor.r, _ringBaseColor.g, _ringBaseColor.b, ringA);
                }
                if (_footRingFront != null)
                {
                    _footRingFront.color = new Color(_ringBaseColor.r, _ringBaseColor.g, _ringBaseColor.b, ringA);
                }
            }
            else // Idle
            {
                _body.transform.localRotation = Quaternion.identity;
                _body.transform.localPosition = new Vector3(0f, _sinkOffsetY, 0f);
                if (!reducedMotion)
                {
                    float breath = Mathf.Sin(Time.time * 3.2f) * 0.025f;
                    float breathFactor = 0.50f;
                    _body.transform.localScale = _baseBodyScale * (1f + breath);
                }
                else
                {
                    _body.transform.localScale = _baseBodyScale;
                }
            }
        }

        public static void ConfigureSharedResources(GameConfigAsset configAsset)
        {
            if (configAsset == null) return;
            if (!CustomScaleApplied && configAsset.characterScale > 0.01f) VisualCharacterScale = configAsset.characterScale;
            if (configAsset.hitRingScale > 0.01f) VisualHitRingScale = configAsset.hitRingScale;
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
            if (configAsset.sandalProjectileSprite != null)
            {
                _defaultWeaponIcon = configAsset.sandalProjectileSprite;
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
            _ringBaseColor = color;
            _sinkOffsetY = -CharacterSinkRatios[characterIndex] * 1.48f;
            _lastLabelName = null;
            _lastLabelHp = -999;
            BindComponents();

            EnsureStaticResources();

            // 1. Shadow: Standardized procedural soft shadow for GameConfig.PlayerRadius (width = 1.125u, 12.5% arena width)
            if (_shadow == null)
            {
                var sGo = new GameObject("Shadow");
                sGo.transform.SetParent(transform, false);
                sGo.transform.localPosition = new Vector3(0f, -0.05f, 0f);
                _shadow = sGo.AddComponent<SpriteRenderer>();
                _shadow.sprite = _defaultShadow;
                _shadow.sortingOrder = -1;
                _shadow.color = new Color(0f, 0f, 0f, 0.44f);
                float shadowScale = 0.66f * VisualCharacterScale * 0.70f;
                sGo.transform.localScale = new Vector3(shadowScale, shadowScale, 1f);
            }
            else
            {
                _shadow.sprite = _defaultShadow;
                _shadow.color = new Color(0f, 0f, 0f, 0.44f);
                float shadowScale = 0.66f * VisualCharacterScale * 0.70f;
                _shadow.transform.localScale = new Vector3(shadowScale, shadowScale, 1f);
                _shadow.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            }

            // 2. Foot Ring: Standardized procedural two-half ring (G5)
            // Back half (sortingOrder = 1) renders behind the character body
            // Front half (sortingOrder = 3) renders in front of submerged feet
            float ringScale = 0.66f * VisualHitRingScale;
            if (_footRing == null)
            {
                var rGo = new GameObject("FootRing");
                rGo.transform.SetParent(transform, false);
                rGo.transform.localPosition = new Vector3(0f, 0.02f, 0f);
                _footRing = rGo.AddComponent<SpriteRenderer>();
                _footRing.sprite = _defaultRingBack != null ? _defaultRingBack : _defaultRing;
                _footRing.sortingOrder = 1;
                _footRing.color = color;
                rGo.transform.localScale = new Vector3(ringScale, ringScale, 1f);
            }
            else
            {
                _footRing.sprite = _defaultRingBack != null ? _defaultRingBack : _defaultRing;
                _footRing.color = color;
                _footRing.sortingOrder = 1;
                _footRing.transform.localScale = new Vector3(ringScale, ringScale, 1f);
                _footRing.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            }

            if (_footRingFront == null)
            {
                var rfGo = new GameObject("FootRingFront");
                rfGo.transform.SetParent(transform, false);
                rfGo.transform.localPosition = new Vector3(0f, 0.02f, 0f);
                _footRingFront = rfGo.AddComponent<SpriteRenderer>();
                _footRingFront.sprite = _defaultRingFront != null ? _defaultRingFront : _defaultRing;
                _footRingFront.sortingOrder = 3;
                _footRingFront.color = color;
                rfGo.transform.localScale = new Vector3(ringScale, ringScale, 1f);
            }
            else
            {
                _footRingFront.sprite = _defaultRingFront != null ? _defaultRingFront : _defaultRing;
                _footRingFront.color = color;
                _footRingFront.sortingOrder = 3;
                _footRingFront.transform.localScale = new Vector3(ringScale, ringScale, 1f);
                _footRingFront.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            }

            // 3. Body: Submerged per CharacterSinkRatios (G1d-3)
            float sinkRatio = CharacterSinkRatios[characterIndex];
            float sinkOffsetY = -sinkRatio * 1.48f;
            if (_body == null)
            {
                var bGo = new GameObject("Body");
                bGo.transform.SetParent(transform, false);
                bGo.transform.localPosition = new Vector3(0f, sinkOffsetY, 0f);
                _body = bGo.AddComponent<SpriteRenderer>();
            }
            else
            {
                _body.transform.localPosition = new Vector3(0f, sinkOffsetY, 0f);
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

            float baseScale = CharacterBodyScales[assetIdx];
            float scale = baseScale * VisualCharacterScale;
            _baseBodyScale = new Vector3(scale, scale, 1f);
            _body.transform.localScale = _baseBodyScale;
            _body.transform.localPosition = Vector3.zero;
            _body.color = Color.white;
        }

        public void SetHitboxVisible(bool visible, float radius = -1f)
        {
            if (radius <= 0f)
            {
                radius = new GameConfig().PlayerRadius;
            }
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
            _currentState = stateName;
            int idx = Mathf.Clamp(characterIndex, 0, TotalCharacterCount - 1);
            _sinkOffsetY = -CharacterSinkRatios[idx] * 1.48f;

            if (stateName == "Hit")
            {
                _hitReactionTimer = 0.25f;
            }
            else if (stateName == "Defeat" || stateName == "KO")
            {
                _defeatStartTime = Time.time;
            }
            else if (stateName == "Spawn")
            {
                _spawnTimer = 0f;
            }
            else if (stateName == "Throw")
            {
                _throwTimer = 0.35f;
                if (_defaultSprites != null && idx < _defaultSprites.Length && _defaultSprites[idx] != null)
                {
                    _body.sprite = _defaultSprites[idx];
                }
                return;
            }
            else if (stateName == "Aim")
            {
                // Z7: Keep high-res idle sprite instead of swapping to low-res aim sprite
                if (_defaultSprites != null && idx < _defaultSprites.Length && _defaultSprites[idx] != null)
                {
                    _body.sprite = _defaultSprites[idx];
                }
            }
            else if (stateName == "Idle")
            {
                _body.color = Color.white;
                _body.transform.localRotation = Quaternion.identity;
                _body.transform.localPosition = new Vector3(0f, _sinkOffsetY, 0f);
                _body.transform.localScale = _baseBodyScale;
                if (_defaultSprites != null && idx < _defaultSprites.Length && _defaultSprites[idx] != null)
                {
                    _body.sprite = _defaultSprites[idx];
                }
            }
        }

        public void Present(PlayerState player, bool visible, bool showAim, bool showTouchRing, float touchRadius, float aimLineExtraLength, float aimPreviewLength)
        {
            if (_body == null) return;
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            transform.position = new Vector3(player.Position.X, player.Position.Y, 0f);

            _sinkOffsetY = -CharacterSinkRatios[characterIndex] * 1.48f;
            _currentAimDirection = new Vector2(player.Facing.X, player.Facing.Y);
            if (_currentAimDirection.sqrMagnitude < 0.001f) _currentAimDirection = Vector2.up;

            _body.enabled = visible;
            if (_shadow != null) _shadow.enabled = visible;
            if (_footRing != null) _footRing.enabled = visible;
            if (_footRingFront != null) _footRingFront.enabled = visible;

            if (_label != null)
            {
                if (_label.gameObject.activeSelf != visible)
                {
                    _label.gameObject.SetActive(visible);
                }
                if (visible)
                {
                    _label.transform.localPosition = new Vector3(0f, 1.15f * VisualCharacterScale, 0f);
                    _label.fontSize = 0.85f * Mathf.Clamp(VisualCharacterScale * 0.70f, 0.9f, 1.35f);
                    UpdateLabelCached(player.Name, player.Hp);
                }
            }

            int idx = Mathf.Clamp(characterIndex, 0, TotalCharacterCount - 1);
            if (_currentState != "Throw" && _defaultSprites != null && idx < _defaultSprites.Length && _defaultSprites[idx] != null)
            {
                _body.sprite = _defaultSprites[idx];
            }

            float baseScale = CharacterBodyScales[idx];
            float scale = baseScale * VisualCharacterScale;
            _baseBodyScale = new Vector3(scale, scale, 1f);
            if (_currentState != "Spawn" && _currentState != "Win")
            {
                _body.transform.localScale = _baseBodyScale;
            }

            if (player.Facing.X < -0.05f) _body.flipX = true;
            else if (player.Facing.X > 0.05f) _body.flipX = false;

            if (_shadow != null)
            {
                float shadowScale = 0.66f * VisualCharacterScale * 0.70f;
                _shadow.transform.localScale = new Vector3(shadowScale, shadowScale, 1f);
            }

            if (_currentState != "Defeat" && _currentState != "KO")
            {
                _body.color = player.IsAlive ? Color.white : new Color(0.35f, 0.35f, 0.35f, 0.50f);
            }

            EnsureAimComponents();

            // E3 Aim Visual: Aim Cone + 10 Trajectory Dots + Reticle + Weapon Icon
            bool activeAim = visible && showAim && player.IsAlive;
            if (_aimConeRenderer != null) _aimConeRenderer.enabled = activeAim;
            if (_aimConeOutline != null) _aimConeOutline.enabled = activeAim;
            if (_aimReticle != null) _aimReticle.enabled = activeAim;
            if (_aimWeaponIcon != null) _aimWeaponIcon.enabled = activeAim;
            if (_aim != null) _aim.enabled = false;

            if (activeAim)
            {
                Vector3 dir = new Vector3(_currentAimDirection.x, _currentAimDirection.y, 0f).normalized;
                float startDist = 0.45f;
                float length = Mathf.Clamp(aimPreviewLength + aimLineExtraLength, 0.6f, 3.5f);
                Vector3 localStartPos = dir * startDist;
                Vector3 localTipPos = dir * (startDist + length);
                Vector3 perp = new Vector3(-dir.y, dir.x, 0f);

                float halfBaseWidth = 0.08f;
                float halfTipWidth = 0.08f + length * 0.1763f; // 20 deg opening (tan 10 deg ~ 0.1763)

                Vector3 localBaseLeft = localStartPos + perp * halfBaseWidth;
                Vector3 localBaseRight = localStartPos - perp * halfBaseWidth;
                Vector3 localTipLeft = localTipPos + perp * halfTipWidth;
                Vector3 localTipRight = localTipPos - perp * halfTipWidth;

                if (_aimConeMesh == null)
                {
                    _aimConeMesh = new Mesh();
                    _aimConeMesh.name = "AimConeMesh";
                    if (_aimConeFilter != null) _aimConeFilter.sharedMesh = _aimConeMesh;
                }
                _aimConeMesh.Clear();
                _aimConeMesh.vertices = new Vector3[] { localBaseLeft, localBaseRight, localTipLeft, localTipRight };
                _aimConeMesh.uv = new Vector2[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
                Color startCol = new Color(1f, 1f, 1f, 0.12f);
                Color endCol = new Color(0f, 0.898f, 1f, 0.38f);
                _aimConeMesh.colors = new Color[] { startCol, startCol, endCol, endCol };
                _aimConeMesh.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
                _aimConeMesh.RecalculateBounds();
                if (_aimConeFilter != null) _aimConeFilter.sharedMesh = _aimConeMesh;

                if (_aimConeOutline != null)
                {
                    _aimConeOutline.useWorldSpace = false;
                    if (_aimConeOutline.positionCount < 4) _aimConeOutline.positionCount = 4;
                    _aimConeOutline.SetPosition(0, localBaseLeft);
                    _aimConeOutline.SetPosition(1, localTipLeft);
                    _aimConeOutline.SetPosition(2, localTipRight);
                    _aimConeOutline.SetPosition(3, localBaseRight);
                }

                if (_aimDots != null)
                {
                    for (int i = 0; i < _aimDots.Length; i++)
                    {
                        if (_aimDots[i] == null) continue;
                        _aimDots[i].enabled = true;
                        float t = (i + 0.8f) / (_aimDots.Length + 0.5f);
                        Vector3 dotPos = Vector3.Lerp(localStartPos, localTipPos, t);
                        dotPos.z = -0.02f;
                        _aimDots[i].transform.localPosition = dotPos;
                        float dotScale = Mathf.Lerp(0.045f, 0.09f, t);
                        _aimDots[i].transform.localScale = new Vector3(dotScale, dotScale, 1f);
                        _aimDots[i].color = new Color(1f, 1f, 1f, Mathf.Lerp(0.55f, 0.95f, t));
                    }
                }

                if (_aimReticle != null)
                {
                    float pulse = 1.0f + Mathf.Sin(Time.time * 5.0f) * 0.05f;
                    float reticleScale = 0.50f * pulse;
                    _aimReticle.transform.localPosition = localTipPos + new Vector3(0f, 0f, -0.03f);
                    _aimReticle.transform.localScale = new Vector3(reticleScale, reticleScale, 1f);
                    _aimReticle.color = new Color(0f, 0.898f, 1f, 0.85f);
                }

                if (_aimWeaponIcon != null)
                {
                    _aimWeaponIcon.transform.localPosition = localTipPos + new Vector3(0f, 0f, -0.04f);
                    float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                    _aimWeaponIcon.transform.localRotation = Quaternion.Euler(0f, 0f, rotZ);
                    _aimWeaponIcon.transform.localScale = new Vector3(0.28f, 0.28f, 1f);
                    if (_defaultWeaponIcon != null) _aimWeaponIcon.sprite = _defaultWeaponIcon;
                }
            }
            else
            {
                if (_aimDots != null)
                {
                    for (int i = 0; i < _aimDots.Length; i++)
                    {
                        if (_aimDots[i] != null) _aimDots[i].enabled = false;
                    }
                }
            }

            if (_touchRing != null)
            {
                _touchRing.enabled = false;
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

        private void EnsureAimComponents()
        {
            if (_aimConeFilter == null)
            {
                var cGo = transform.Find("AimCone");
                if (cGo == null)
                {
                    var go = new GameObject("AimCone");
                    go.transform.SetParent(transform, false);
                    _aimConeFilter = go.AddComponent<MeshFilter>();
                    _aimConeRenderer = go.AddComponent<MeshRenderer>();
                }
                else
                {
                    _aimConeFilter = cGo.GetComponent<MeshFilter>();
                    _aimConeRenderer = cGo.GetComponent<MeshRenderer>();
                }
                if (_aimConeFilter != null && _aimConeMesh != null)
                {
                    _aimConeFilter.sharedMesh = _aimConeMesh;
                }
                if (_aimConeRenderer != null)
                {
                    _aimConeRenderer.material = new Material(Shader.Find("Sprites/Default"));
                    _aimConeRenderer.sortingOrder = 1;
                }
            }

            if (_aimConeOutline == null)
            {
                _aimConeOutline = CreateLine("AimConeOutline");
                _aimConeOutline.sortingOrder = 1;
                _aimConeOutline.startWidth = 0.025f;
                _aimConeOutline.endWidth = 0.025f;
                _aimConeOutline.loop = true;
                _aimConeOutline.startColor = new Color(1f, 1f, 1f, 0.85f);
                _aimConeOutline.endColor = new Color(1f, 1f, 1f, 0.85f);
                _aimConeOutline.positionCount = 4;
            }

            if (_aimDots == null || _aimDots.Length != 10)
            {
                _aimDots = new SpriteRenderer[10];
                for (int i = 0; i < 10; i++)
                {
                    string dotName = "AimDot_" + i;
                    var dTransform = transform.Find(dotName);
                    if (dTransform == null)
                    {
                        var dGo = new GameObject(dotName);
                        dGo.transform.SetParent(transform, false);
                        _aimDots[i] = dGo.AddComponent<SpriteRenderer>();
                    }
                    else
                    {
                        _aimDots[i] = dTransform.GetComponent<SpriteRenderer>();
                    }
                    _aimDots[i].sprite = _defaultDot != null ? _defaultDot : _defaultRing;
                    _aimDots[i].sortingOrder = 1;
                    _aimDots[i].enabled = false;
                }
            }

            if (_aimReticle == null)
            {
                var rTransform = transform.Find("AimReticle");
                if (rTransform == null)
                {
                    var rGo = new GameObject("AimReticle");
                    rGo.transform.SetParent(transform, false);
                    _aimReticle = rGo.AddComponent<SpriteRenderer>();
                }
                else
                {
                    _aimReticle = rTransform.GetComponent<SpriteRenderer>();
                }
                _aimReticle.sprite = _defaultRingFront != null ? _defaultRingFront : _defaultRing;
                _aimReticle.sortingOrder = 4;
                _aimReticle.enabled = false;
            }

            if (_aimWeaponIcon == null)
            {
                var wTransform = transform.Find("AimWeaponIcon");
                if (wTransform == null)
                {
                    var wGo = new GameObject("AimWeaponIcon");
                    wGo.transform.SetParent(transform, false);
                    _aimWeaponIcon = wGo.AddComponent<SpriteRenderer>();
                }
                else
                {
                    _aimWeaponIcon = wTransform.GetComponent<SpriteRenderer>();
                }
                _aimWeaponIcon.sprite = _defaultWeaponIcon;
                _aimWeaponIcon.sortingOrder = 5;
                _aimWeaponIcon.enabled = false;
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
            if (_defaultRingBack == null)
            {
                _defaultRingBack = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/procedural_foot_ring_back.png");
            }
            if (_defaultRingFront == null)
            {
                _defaultRingFront = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/procedural_foot_ring_front.png");
            }
            if (_defaultDot == null)
            {
                _defaultDot = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/aim_dot.png");
            }
            if (_defaultWeaponIcon == null)
            {
                _defaultWeaponIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/dep_icon.png");
            }
            if (_defaultFont == null)
            {
                _defaultFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Bold SDF.asset");
            }
            if (_defaultSprites == null || _defaultSprites.Length < TotalCharacterCount)
            {
                _defaultSprites = new Sprite[TotalCharacterCount];
                _defaultSprites[0] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_idle.png");
                _defaultSprites[1] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/mai_hong_idle.png");
                _defaultSprites[2] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_idle.png");
                _defaultSprites[3] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_nui_idle.png");
                _defaultSprites[4] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/co_nang_the_thao_idle.png");
                _defaultSprites[5] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/chu_cho_vang_idle.png");
                _defaultSprites[6] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/meo_mun_idle.png");
            }
            if (_throwSprites == null || _throwSprites.Length < TotalCharacterCount)
            {
                _throwSprites = new Sprite[TotalCharacterCount];
                _throwSprites[0] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_throw_release.png");
                _throwSprites[1] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/co_nang_the_thao_throw_release.png");
                _throwSprites[2] = null; // Quậy Boy (Sắp ra mắt)
                _throwSprites[3] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_throw_release.png");
                _throwSprites[4] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_nui_throw_release.png");
                _throwSprites[5] = null; // Chó Vàng (Sắp ra mắt)
                _throwSprites[6] = null; // Mèo Mun (Sắp ra mắt)
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
