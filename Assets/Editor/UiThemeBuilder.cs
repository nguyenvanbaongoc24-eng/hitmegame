using System.IO;
using HitMe.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace HitMe.Editor
{
    public static class UiThemeBuilder
    {
        private const string KitPath = "Assets/Art/UI/Kit/";
        private const string FontPath = "Assets/Fonts/BeVietnamPro-Bold SDF.asset";
        private const string ThemeAssetPath = "Assets/Art/UI/UiTheme.asset";
        private const string ResourcesThemePath = "Assets/Resources/UiTheme.asset";

        [MenuItem("Tools/HIT ME/Build UI Theme")]
        public static void BuildTheme()
        {
            ConfigureKitSprites();

            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (fontAsset == null)
            {
                Debug.LogError($"[UiThemeBuilder] Font asset not found at {FontPath}");
            }

            var theme = AssetDatabase.LoadAssetAtPath<UiTheme>(ThemeAssetPath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<UiTheme>();
                AssetDatabase.CreateAsset(theme, ThemeAssetPath);
            }

            // Colors extracted from ASSET HITME FINAL
            theme.primaryYellow = new Color(0.972f, 0.725f, 0.125f, 1f);     // #F8B920
            theme.alertRed = new Color(0.804f, 0.169f, 0.169f, 1f);          // #CD2B2B
            theme.actionGreen = new Color(0.04f, 0.64f, 0.447f, 1f);         // #09A372
            theme.actionBlue = new Color(0.157f, 0.380f, 0.890f, 1f);        // #2861E3
            theme.woodBannerBrown = new Color(0.424f, 0.200f, 0.075f, 1f);   // #6C3313
            theme.goldBorder = new Color(0.875f, 0.698f, 0.267f, 1f);        // #DFB244
            theme.darkSlateBg = new Color(0.059f, 0.090f, 0.165f, 0.95f);    // #0F172A
            theme.creamText = new Color(1.0f, 0.973f, 0.863f, 1f);           // #FFF8DC
            theme.subtitleGold = new Color(1.0f, 0.910f, 0.510f, 1f);        // #FFE882
            theme.lockedButtonMuted = new Color(0.35f, 0.40f, 0.48f, 0.85f);
            theme.watermarkYellow = new Color(1.0f, 0.902f, 0.0f, 0.95f);     // #FFE600

            theme.defaultFont = fontAsset;

            // Sprites from Kit
            theme.bannerWood = LoadSprite(KitPath + "ui_banner_wood.png");
            theme.btnCtaYellow = LoadSprite(KitPath + "ui_btn_cta_yellow.png");
            theme.btnCtaBlue = LoadSprite(KitPath + "ui_btn_cta_blue.png");
            theme.btnCtaGreen = LoadSprite(KitPath + "ui_btn_cta_green.png");
            theme.btnCtaRed = LoadSprite(KitPath + "ui_btn_cta_red.png");
            theme.btnCircleBack = LoadSprite(KitPath + "ui_btn_circle_back.png");
            theme.panelDarkGold = LoadSprite(KitPath + "ui_panel_dark_gold.png");
            theme.cardCharNormal = LoadSprite(KitPath + "ui_card_char_normal.png");
            theme.cardCharSelected = LoadSprite(KitPath + "ui_card_char_selected.png");
            theme.inputBoxCyan = LoadSprite(KitPath + "ui_input_box_cyan.png");
            theme.rowLeaderboard = LoadSprite(KitPath + "ui_row_leaderboard.png");
            theme.rowLeaderboardMe = LoadSprite(KitPath + "ui_row_leaderboard_me.png");
            theme.timerCircleGold = LoadSprite(KitPath + "ui_timer_circle_gold.png");
            theme.timerPulseRing = LoadSprite(KitPath + "ui_aim_pulse_gold.png");

            // Gameplay & Aim Kit
            theme.heartFull = LoadSprite(KitPath + "ui_icon_heart_full.png");
            theme.heartBreak = LoadSprite(KitPath + "ui_icon_heart_break.png");
            theme.weaponSandal = LoadSprite("Assets/Art/Weapons/dep_to_ong_icon.png") ?? LoadSprite("Assets/Sprites/weapon_dep_full.png");
            theme.aimBeamSoft = LoadSprite(KitPath + "ui_aim_beam_soft.png");
            theme.aimDirectionArrow = LoadSprite(KitPath + "ui_aim_direction_arrow.png");
            theme.aimOriginRing = LoadSprite(KitPath + "ui_aim_origin_ring.png");
            theme.aimTargetReticle = LoadSprite(KitPath + "ui_aim_target_reticle.png");
            theme.aimTargetLocked = LoadSprite(KitPath + "ui_aim_target_locked.png");

            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();

            // Also ensure copy exists in Resources for runtime loading
            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
            }
            AssetDatabase.CopyAsset(ThemeAssetPath, ResourcesThemePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[UiThemeBuilder] Built and saved UiTheme to {ThemeAssetPath} and {ResourcesThemePath}");
        }

        private static void ConfigureKitSprites()
        {
            var borders = new (string file, Vector4 border)[]
            {
                ("ui_banner_wood.png", new Vector4(40, 20, 40, 20)),
                ("ui_btn_cta_yellow.png", new Vector4(35, 25, 35, 25)),
                ("ui_btn_cta_blue.png", new Vector4(35, 25, 35, 25)),
                ("ui_btn_cta_green.png", new Vector4(35, 25, 35, 25)),
                ("ui_btn_cta_red.png", new Vector4(35, 25, 35, 25)),
                ("ui_card_char_normal.png", new Vector4(30, 30, 30, 30)),
                ("ui_card_char_selected.png", new Vector4(30, 30, 30, 30)),
                ("ui_row_leaderboard.png", new Vector4(30, 30, 30, 30)),
                ("ui_row_leaderboard_me.png", new Vector4(30, 30, 30, 30)),
                ("ui_panel_dark_gold.png", new Vector4(40, 40, 40, 40)),
                ("ui_hud_pill_dark.png", new Vector4(25, 25, 25, 25)),
                ("ui_input_box_cyan.png", new Vector4(25, 25, 25, 25))
            };

            foreach (var item in borders)
            {
                string path = KitPath + item.file;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    bool changed = false;
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        changed = true;
                    }
                    if (importer.spriteBorder != item.border)
                    {
                        importer.spriteBorder = item.border;
                        changed = true;
                    }
                    if (changed)
                    {
                        importer.SaveAndReimport();
                    }
                }
            }

            // Also configure non-9slice sprites in Kit
            string[] allKitFiles = Directory.GetFiles(KitPath, "*.png");
            foreach (var file in allKitFiles)
            {
                string path = file.Replace('\\', '/');
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }
            }
        }

        private static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
