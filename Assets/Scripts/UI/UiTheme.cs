using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HitMe.UI
{
    public enum TimerVisualStyle
    {
        RingOnly = 0,
        GoldCoinAndRing = 1
    }

    [CreateAssetMenu(fileName = "UiTheme", menuName = "HIT ME/UI Theme", order = 1)]
    public class UiTheme : ScriptableObject
    {
        // =====================================================================
        // V7.0 Static Color Tokens (Dark Navy, Deep Navy, Golden Yellow, Cyan, Coral, Emerald, Cream)
        // =====================================================================
        public static readonly Color BgOverlay = new Color(0.03f, 0.06f, 0.10f, 0.82f);       // Deepest backdrop
        public static readonly Color PanelBg = new Color(0.067f, 0.106f, 0.173f, 0.97f);      // Dark Navy #111B2C
        public static readonly Color CardBg = new Color(0.043f, 0.078f, 0.129f, 0.96f);       // Deep Navy #0B1421
        public static readonly Color CardBgAlt = new Color(0.094f, 0.149f, 0.243f, 0.96f);    // Elevated Navy #18263E
        public static readonly Color PrimaryCta = new Color(1.000f, 0.804f, 0.220f, 1.0f);    // Golden Yellow #FFCD38
        public static readonly Color SecondaryCta = new Color(0.098f, 0.835f, 0.847f, 1.0f);  // Cyan #19D5D8
        public static readonly Color PositiveGreen = new Color(0.090f, 0.725f, 0.529f, 1.0f); // Emerald Green #17B987
        public static readonly Color DangerRed = new Color(0.965f, 0.416f, 0.365f, 1.0f);     // Coral Red #F66A5D
        public static readonly Color TextPrimary = new Color(1.000f, 0.973f, 0.910f, 1.0f);   // Cream White #FFF8E8
        public static readonly Color TextSecondary = new Color(0.722f, 0.780f, 0.878f, 1.0f); // Soft Slate #B8C7E0
        public static readonly Color TextDark = new Color(0.137f, 0.094f, 0.043f, 1.0f);      // Dark Ink on Gold CTA

        [Header("Bảng Màu Chủ Đạo (REFORM V7.0 Design System)")]
        public Color primaryYellow = new Color(1.000f, 0.804f, 0.220f, 1f);    // #FFCD38 Golden Yellow
        public Color alertRed = new Color(0.965f, 0.416f, 0.365f, 1f);         // #F66A5D Coral Red
        public Color actionGreen = new Color(0.090f, 0.725f, 0.529f, 1f);      // #17B987 Emerald Green
        public Color actionBlue = new Color(0.098f, 0.835f, 0.847f, 1f);       // #19D5D8 Cyan
        public Color woodBannerBrown = new Color(0.067f, 0.106f, 0.173f, 1f);  // #111B2C Dark Navy
        public Color goldBorder = new Color(1.000f, 0.804f, 0.220f, 1f);       // #FFCD38 Golden Border
        public Color darkSlateBg = new Color(0.043f, 0.078f, 0.129f, 0.96f);   // #0B1421 Deep Navy
        public Color creamText = new Color(1.000f, 0.973f, 0.910f, 1f);        // #FFF8E8 Cream White
        public Color subtitleGold = new Color(1.000f, 0.804f, 0.220f, 1f);     // #FFCD38 Golden Subtitle
        public Color lockedButtonMuted = new Color(0.35f, 0.40f, 0.48f, 0.85f);
        public Color watermarkYellow = new Color(1.0f, 0.90f, 0.0f, 0.95f);

        [Header("Phông Chữ Tiếng Việt (TMP_FontAsset)")]
        public TMP_FontAsset defaultFont;

        [Header("Cỡ Chữ Chuẩn")]
        public float fontHeader = 40f;
        public float fontTitle = 32f;
        public float fontSubtitle = 24f;
        public float fontButton = 28f;
        public float fontBody = 22f;
        public float fontSmall = 18f;

        [Header("Khoảng Cách Chuẩn (Padding & Spacing)")]
        public float spacingXs = 8f;
        public float spacingSm = 16f;
        public float spacingMd = 24f;
        public float spacingLg = 36f;

        [Header("Tài Nguyên Sprite UI (Kit)")]
        public Sprite bannerWood;
        public Sprite btnCtaYellow;
        public Sprite btnCtaBlue;
        public Sprite btnCtaGreen;
        public Sprite btnCtaRed;
        public Sprite btnCircleBack;
        public Sprite panelDarkGold;
        public Sprite cardCharNormal;
        public Sprite cardCharSelected;
        public Sprite inputBoxCyan;
        public Sprite rowLeaderboard;
        public Sprite rowLeaderboardMe;
        public Sprite timerCircleGold;
        public Sprite timerPulseRing;
        public Sprite bgHomeVillage;

        [Header("Tài Nguyên Gameplay & Aim Kit")]
        public Sprite heartFull;
        public Sprite heartBreak;
        public Sprite weaponSandal;
        public Sprite aimBeamSoft;
        public Sprite aimDirectionArrow;
        public Sprite aimOriginRing;
        public Sprite aimTargetReticle;
        public Sprite aimTargetLocked;

        [Header("Thời Lượng Hiệu Ứng (Juice Duration)")]
        public float bannerFadeDuration = 0.25f;
        public float buttonPressScale = 0.94f;
        public float pulseLoopDuration = 0.50f;
        public float heartShakeDuration = 0.35f;

        [Header("Hoạt Họa & Cảm Giác Chơi (Game Feel - Lô 9)")]
        public const float DefaultIdleBreathHorizontalFactor = 0.50f;
        public const float DefaultWeaponShadowVerticalScaleFactor = 0.54f;
        public float idleBreathSpeed = 3.2f;
        public float idleBreathScale = 0.025f;
        public float idleBreathHorizontalFactor = DefaultIdleBreathHorizontalFactor;
        public float hitReactionDuration = 0.22f;
        public float hitReactionRecoil = 0.12f;
        public float weaponSpinSpeed = -720f;
        public float weaponShadowOffset = 0.22f;
        public float weaponShadowVerticalScaleFactor = DefaultWeaponShadowVerticalScaleFactor;

        [Header("Round 38 G1–G8 In-Match HUD & Arena Framing Tokens")]
        public TimerVisualStyle timerStyle = TimerVisualStyle.RingOnly;
        public float playerCardHeightN2To4 = 68f;
        public float playerCardHeightN5To6 = 60f;
        public float playerCardAvatarSizeN2To4 = 38f;
        public float playerCardAvatarSizeN5To6 = 32f;
        public float playerCardGap = 6f;
        public float playerCardSafeSideMargin = 8f;
        public float playerCardNameLineHeight = 1.28f;
        public float timerCircleDiameter = 62f;
        public float timerPulseUrgentSeconds = 2.0f;
        public float phaseBannerDuration = 0.62f;
        public float phaseBannerOvershootScale = 1.14f;
        public float arenaViewZoomFactor = 0.88f; // Closer camera framing so characters occupy ~14-18% screen height
        public float placeBounceOvershoot = 1.15f;
        public float placeDustExpandScale = 1.35f;
        public float aimRulerDashSpeed = 3.2f;
        public float botSetupBottomBarPadding = 112f;
        public float minThumbTouchTargetPxAt1080 = 135f;

        private static UiTheme _instance;
        public static UiTheme Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<UiTheme>("UiTheme");
                }
                return _instance;
            }
            set => _instance = value;
        }
    }
}
