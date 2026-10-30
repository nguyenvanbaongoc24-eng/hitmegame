using UnityEngine;
using TMPro;

namespace HitMe.UI
{
    [CreateAssetMenu(fileName = "UiTheme", menuName = "HIT ME/UI Theme", order = 1)]
    public class UiTheme : ScriptableObject
    {
        [Header("Bảng Màu Chủ Đạo (Trích xuất từ ASSET HITME FINAL)")]
        public Color primaryYellow = new Color(0.97f, 0.73f, 0.13f, 1f);     // #F8B920 Vàng Nắng Làng Quê (CTA)
        public Color alertRed = new Color(0.80f, 0.17f, 0.17f, 1f);          // #CD2B2B Đỏ Cờ Truyền Thống
        public Color actionGreen = new Color(0.04f, 0.64f, 0.447f, 1f);      // #09A372 Xanh Lá Tre Đồng Quê
        public Color actionBlue = new Color(0.16f, 0.38f, 0.89f, 1f);        // #2861E3 Xanh Dương Sông Nước
        public Color woodBannerBrown = new Color(0.42f, 0.20f, 0.07f, 1f);   // #6C3313 Nâu Gỗ Mái Đình
        public Color goldBorder = new Color(0.87f, 0.70f, 0.27f, 1f);        // #DFB244 Vàng Kim Cổ Điển
        public Color darkSlateBg = new Color(0.06f, 0.09f, 0.16f, 0.95f);    // #0F172A Nền Tối Xanh Chàm
        public Color creamText = new Color(1.0f, 0.97f, 0.86f, 1f);          // #FFF8DC Chữ Kem Dịu Mắt
        public Color subtitleGold = new Color(1.0f, 0.91f, 0.51f, 1f);       // #FFE882 Chữ Phụ Vàng Nắng
        public Color lockedButtonMuted = new Color(0.35f, 0.40f, 0.48f, 0.85f); // Trạng thái ĐÃ KHÓA
        public Color watermarkYellow = new Color(1.0f, 0.90f, 0.0f, 0.95f);  // #FFE600 Watermark góc dưới-trái

        [Header("Phông Chữ Tiếng Việt (TMP_FontAsset)")]
        public TMP_FontAsset defaultFont;

        [Header("Cỡ Chữ Chuẩn")]
        public float fontHeader = 40f;
        public float fontTitle = 32f;
        public float fontSubtitle = 24f;
        public float fontButton = 28f;
        public float fontBody = 20f;
        public float fontSmall = 16f;

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
