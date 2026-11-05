using System.Collections.Generic;
using System.IO;
using HitMe.Audio;
using HitMe.Core;
using HitMe.Game;
using HitMe.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HitMe.Editor
{
    public static class ProductionUISetup
    {
        private const string GrayboxScenePath = "Assets/Scenes/Graybox.unity";
        private const string TMPFontPath = "Assets/Fonts/BeVietnamPro-Bold SDF.asset";
        private const string KitPath = "Assets/Art/UI/Kit/";
        private const string SpritesPath = "Assets/Sprites/";

        [MenuItem("Tools/HIT ME/Build Production UI for Graybox")]
        public static void BuildProductionUI()
        {
            var scene = EditorSceneManager.OpenScene(GrayboxScenePath, OpenSceneMode.Single);
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TMPFontPath);

            var spriteBannerWood = LoadSprite(KitPath + "ui_banner_wood.png");
            var spriteBtnYellow = LoadSprite(KitPath + "ui_btn_cta_yellow.png");
            var spriteBtnBlue = LoadSprite(KitPath + "ui_btn_cta_blue.png");
            var spriteBtnGreen = LoadSprite(KitPath + "ui_btn_cta_green.png");
            var spriteBtnRed = LoadSprite(KitPath + "ui_btn_cta_red.png");
            var spritePanelDarkGold = LoadSprite(KitPath + "ui_panel_dark_gold.png");
            var spriteInputCyan = LoadSprite(KitPath + "ui_input_box_cyan.png");
            var spriteHudPill = LoadSprite(KitPath + "ui_hud_pill_dark.png");
            var spriteCardChar = LoadSprite(KitPath + "ui_card_char_normal.png");
            var spriteCardSlot = LoadSprite(KitPath + "ui_card_slot_selected_cyan.png");
            var spriteRowLeaderboard = LoadSprite(KitPath + "ui_row_leaderboard.png");
            var spriteRowMe = LoadSprite(KitPath + "ui_row_leaderboard_me.png");
            var spriteCircleBack = LoadSprite(KitPath + "ui_btn_circle_back.png");
            var spriteArena = LoadSprite(SpritesPath + "ARENA.png");
            var spriteTimerGold = LoadSprite(KitPath + "ui_timer_circle_gold.png");
            var spriteTimerPulse = LoadSprite(KitPath + "ui_aim_pulse_gold.png");
            var spriteHeartFull = LoadSprite(KitPath + "ui_icon_heart_full.png");
            var spriteHeartBreak = LoadSprite(KitPath + "ui_icon_heart_break.png");
            var spriteCardSelected = LoadSprite(KitPath + "ui_card_char_selected.png");
            var spriteWeaponDep = LoadSprite("Assets/Art/Weapons/dep_to_ong_icon.png") ?? LoadSprite("Assets/Sprites/weapon_dep_full.png");
            var spriteBgHome = LoadSprite("Assets/Art/UI/ui_bg_home_village.png");
            var spriteSliderTrack = LoadSprite(KitPath + "ui_slider_track.png");
            var spriteSliderKnob = LoadSprite(KitPath + "ui_slider_knob.png");
            var spriteToggleOn = LoadSprite(KitPath + "ui_toggle_on.png");
            var spriteToggleOff = LoadSprite(KitPath + "ui_toggle_off.png");
            var spriteIconGear = LoadSprite(KitPath + "ui_icon_gear.png");
            var spriteIconCoin = LoadSprite(KitPath + "ui_icon_coin.png");
            var spriteIconGem = LoadSprite(KitPath + "ui_icon_gem.png");
            var spriteIconCrown = LoadSprite(KitPath + "ui_icon_crown.png");
            var spriteIconCheck = LoadSprite(KitPath + "ui_icon_check.png");

            // 3D Visual UI Kit & Badges
                        // 7 Chibi Character Heads (M2)
            var sprHeadNamLang = LoadSprite("Assets/Art/Characters/nam_lang/Menu/menu_nam_lang_head.png");
            var sprHeadNuNangDong = LoadSprite("Assets/Art/Characters/nu_nang_dong/Menu/menu_nu_nang_dong_head.png");
            var sprHeadCoBa = LoadSprite("Assets/Art/Characters/co_ba/Menu/menu_co_ba_head.png");
            var sprHeadAnhTeen = LoadSprite("Assets/Art/Characters/anh_teen/Menu/menu_anh_teen_head.png");
            var sprHeadQuayBoy = LoadSprite("Assets/Art/Characters/quay_boy/Menu/menu_quay_boy_head.png");
            var sprHeadChoVang = LoadSprite("Assets/Art/Characters/cho_vang/Menu/menu_cho_vang_head.png");
            var sprHeadMeoMun = LoadSprite("Assets/Art/Characters/meo_mun/Menu/menu_meo_mun_head.png");
            var charHeadSprites = new Sprite[] {
                sprHeadNamLang, sprHeadNuNangDong, sprHeadCoBa, sprHeadAnhTeen, sprHeadQuayBoy, sprHeadChoVang, sprHeadMeoMun
            };

            var spriteSpeechBubble = LoadSprite(KitPath + "ui_speech_bubble.png");
            var spriteLogoHitMe = LoadSprite(KitPath + "logo_hit_me.png");
            var spriteBtnPrimaryYellow = LoadSprite(KitPath + "btn_primary_yellow_normal.png");
            var spriteBtnBlue3D = LoadSprite(KitPath + "btn_blue_normal.png");
            var spriteBtnGreen3D = LoadSprite(KitPath + "btn_green_normal.png");
            var spriteBtnNavy3D = LoadSprite(KitPath + "btn_navy_normal.png") ?? spriteBannerWood;
            var spriteDockBg = LoadSprite(KitPath + "dock_bg.png");
            var spritePanelCardDarkGold = LoadSprite(KitPath + "panel_card_dark_gold.png") ?? spritePanelDarkGold;
            var spritePillCoin = LoadSprite(KitPath + "pill_coin.png") ?? spriteBannerWood;
            var spriteCircleNavy = LoadSprite(KitPath + "btn_circle_navy.png") ?? spriteCircleBack;
            var spriteCircleGold = LoadSprite(KitPath + "btn_circle_gold.png") ?? spriteCircleBack;
            var spriteAvatarRing = LoadSprite(KitPath + "avatar_ring_cyan.png");
            var spritePortraitNamLang = LoadSprite(KitPath + "portrait_namlang.png");
            var spriteIconPlay = LoadSprite(KitPath + "ui_icon_play.png");
            var spriteIconPersonAdd = LoadSprite(KitPath + "ui_icon_person_add.png");
            var spriteIconPeople = LoadSprite(KitPath + "ui_icon_people.png");
            var spriteIconBot = LoadSprite(KitPath + "ui_icon_bot.png");
            var spriteIconHome = LoadSprite(KitPath + "ui_icon_home.png");
            var spriteIconTrophy = LoadSprite(KitPath + "ui_icon_trophy.png");
            var spriteIconGift = LoadSprite(KitPath + "ui_icon_gift.png");
            var spriteIconChevronLeft = LoadSprite(KitPath + "ui_icon_chevron_left.png");
            var spriteIconChevronRight = LoadSprite(KitPath + "ui_icon_chevron_right.png");
            var spriteIconWeaponDep = LoadSprite(KitPath + "ui_icon_weapon_dep.png") ?? spriteWeaponDep;

            var colorCream = new Color(1.0f, 0.973f, 0.863f, 1f);
            var colorGoldSub = new Color(1.0f, 0.910f, 0.510f, 1f);
            var colorGreen = new Color(0.04f, 0.64f, 0.448f, 1f);

            // 1. Setup Camera for Portrait 1080x2340 (P2: keep arena aspect ratio, leave top clearance for SafeArea + 2-row HUD above village gate)
            var camGo = GameObject.Find("Main Camera");
            if (camGo == null) camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            if (cam == null) cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            // Safari 393x695 (aspect 0.5655): arena width 6.8267 fits 100% with orthographicSize 6.036f
            cam.orthographicSize = 6.036f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(36f / 255f, 24f / 255f, 16f / 255f, 1f); // Z6: #241810
            camGo.transform.position = new Vector3(0f, 0.40f, -10f);

            // 2. Setup ArenaView
            var arenaGo = GameObject.Find("Arena");
            if (arenaGo == null) arenaGo = new GameObject("Arena");
            var arenaView = arenaGo.GetComponent<ArenaView>();
            if (arenaView == null) arenaView = arenaGo.AddComponent<ArenaView>();
            if (spriteArena != null)
            {
                var bgObj = GameObject.Find("ArenaBackground");
                if (bgObj == null) bgObj = new GameObject("ArenaBackground");
                bgObj.transform.SetParent(arenaGo.transform);
                bgObj.transform.position = new Vector3(0f, 0.40f, 0f);
                bgObj.transform.localScale = Vector3.one;
                var sr = bgObj.GetComponent<SpriteRenderer>();
                if (sr == null) sr = bgObj.AddComponent<SpriteRenderer>();
                sr.sprite = spriteArena;
                sr.sortingOrder = -2;

                // B1: Remove ArenaBackdrop to prevent doubled temple gates and repeated seams
                var backdropObj = GameObject.Find("ArenaBackdrop");
                if (backdropObj != null)
                {
                    UnityEngine.Object.DestroyImmediate(backdropObj);
                }
            }
            var configAsset = AssetDatabase.LoadAssetAtPath<GameConfigAsset>("Assets/ScriptableObjects/GameConfig_Phase1.asset");
            if (configAsset != null)
            {
                PopulateConfigVisualAssets(configAsset, fontAsset);
            }
            arenaView.configAsset = configAsset;

            // 3. Setup AudioManager in Scene
            var audioGo = GameObject.Find("AudioManager");
            if (audioGo == null) audioGo = new GameObject("AudioManager");
            var audioMgr = audioGo.GetComponent<AudioManager>();
            if (audioMgr == null) audioMgr = audioGo.AddComponent<AudioManager>();
            AssignAudioClips(audioMgr);

            var audioSvc = audioGo.GetComponent<AudioService>();
            if (audioSvc == null) audioSvc = audioGo.AddComponent<AudioService>();
            AssignAudioServiceClips(audioSvc);

            // 4. Setup EventSystem
            var esGo = GameObject.Find("EventSystem") ?? new GameObject("EventSystem");
            if (esGo.GetComponent<EventSystem>() == null) esGo.AddComponent<EventSystem>();
            if (esGo.GetComponent<StandaloneInputModule>() == null) esGo.AddComponent<StandaloneInputModule>();

            // 5. Clean legacy Canvas if present
            var existingCanvas = GameObject.Find("ProductionCanvas");
            if (existingCanvas != null) Object.DestroyImmediate(existingCanvas);

            // 6. Build Canvas (1080x2340 Portrait, Scale With Screen Size)
            var canvasGo = new GameObject("ProductionCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2340f);
            scaler.matchWidthOrHeight = 1.0f; // Portrait height match

            var uiCtrl = canvasGo.AddComponent<ProductionUIController>();

            // SafeAreaRoot (P2: constrains HUD to portrait 1080x2340 column and applies iPhone safe area)
            var safeAreaGo = new GameObject("SafeAreaRoot", typeof(RectTransform), typeof(SafeAreaFitter));
            safeAreaGo.transform.SetParent(canvasGo.transform, false);
            var safeAreaRt = safeAreaGo.GetComponent<RectTransform>();
            safeAreaRt.anchorMin = Vector2.zero;
            safeAreaRt.anchorMax = Vector2.one;
            safeAreaRt.pivot = new Vector2(0.5f, 0.5f);
            safeAreaRt.offsetMin = Vector2.zero;
            safeAreaRt.offsetMax = Vector2.zero;

            var rootTr = safeAreaGo.transform;

            // ==================== A. TOAST OBJECT ====================
            var toastObj = CreatePanel(rootTr, "ToastContainer", new Vector2(0.5f, 0.885f), new Vector2(0.5f, 0.885f), new Vector2(0.5f, 0.5f), new Vector2(800f, 90f), spritePanelDarkGold, new Color(0.1f, 0.12f, 0.16f, 0.95f));
            var toastTxt = CreateTMP(toastObj.transform, "ToastText", fontAsset, "Tự ném theo hướng cuối", 36, new Color(1f, 0.9f, 0.3f), TextAlignmentOptions.Center);
            FillRect(toastTxt.rectTransform, new Vector2(20f, 10f), new Vector2(-20f, -10f));
            toastObj.SetActive(false);
            uiCtrl.toastObject = toastObj;
            uiCtrl.txtToast = toastTxt;

            // ==================== B. HOME PANEL ====================
            var homePanel = CreatePanel(rootTr, "HomePanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, Color.clear);
            uiCtrl.homePanel = homePanel;

            // 1. Village Background
            if (spriteBgHome != null)
            {
                var bgObj = CreateImage(homePanel.transform, "HomeBgVillage", spriteBgHome, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero);
                var bgRt = bgObj.GetComponent<RectTransform>();
                // Expand to cover outside safe area notch/home bar
                bgRt.offsetMin = new Vector2(-200f, -450f);
                bgRt.offsetMax = new Vector2(200f, 450f);
                var bgImg = bgObj.GetComponent<Image>();
                bgImg.preserveAspect = false;
                bgImg.color = Color.white;
            }

            // Top Bar: Player profile (Avatar + Ring), Currency Chips, and Settings (Safe Area row Y = 0.958f)
            var btnProfHeader = CreateButton(homePanel.transform, "BtnHomeProfileHeader", spriteCardSelected, new Vector2(0.19f, 0.958f), new Vector2(0.19f, 0.958f), new Vector2(0.50f, 0.50f), new Vector2(280f, 78f));
            if (spriteAvatarRing != null)
            {
                var ringGo = CreateImage(btnProfHeader.transform, "AvatarRing", spriteAvatarRing, new Vector2(0.16f, 0.50f), new Vector2(0.16f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(62f, 62f));
                ringGo.GetComponent<Image>().preserveAspect = true;
                if (spritePortraitNamLang != null)
                {
                    var headGo = CreateImage(ringGo.transform, "AvatarHead", spritePortraitNamLang, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(50f, 50f));
                    headGo.GetComponent<Image>().preserveAspect = true;
                }
            }
            var txtProfHeader = CreateTMP(btnProfHeader.transform, "Name", fontAsset, "Nam Làng\n<size=16><color=#19D5D8>Hồ sơ</color></size>", 22, colorCream, TextAlignmentOptions.Left, new Vector2(0.63f, 0.50f), new Vector2(165f, 54f), wrap: false);
            uiCtrl.btnHomeProfile = btnProfHeader.GetComponent<Button>();
            uiCtrl.txtHomePlayerName = txtProfHeader;

            var goldPill = CreateImage(homePanel.transform, "GoldPill", spritePillCoin, new Vector2(0.58f, 0.958f), new Vector2(0.58f, 0.958f), new Vector2(0.50f, 0.50f), new Vector2(210f, 64f));
            if (spriteIconCoin != null)
            {
                var iconGo = CreateImage(goldPill.transform, "CoinIcon", spriteIconCoin, new Vector2(0.18f, 0.5f), new Vector2(0.18f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40f, 40f));
                iconGo.GetComponent<Image>().preserveAspect = true;
            }
            var txtGold = CreateTMP(goldPill.transform, "Text", fontAsset, "500 G", 24, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.60f, 0.50f), new Vector2(125f, 44f), wrap: false);
            uiCtrl.txtHomeGold = txtGold;

            // Hidden or compact gem pill
            // HOME-02: No gem pill and no language button on Home screen
            var gemPill = CreateImage(homePanel.transform, "GemPill", spriteBannerWood, new Vector2(0.76f, 0.958f), new Vector2(0.76f, 0.958f), new Vector2(0.50f, 0.50f), new Vector2(110f, 56f));
            gemPill.SetActive(false);

            // Settings button on Home Screen (Top Right - >= 48px touch target)
            var btnHomeSettings = CreateButton(homePanel.transform, "BtnHomeSettings", spriteCircleNavy, new Vector2(0.91f, 0.958f), new Vector2(0.91f, 0.958f), new Vector2(0.50f, 0.50f), new Vector2(84f, 84f));
            if (spriteIconGear != null)
            {
                var iconGearGo = CreateImage(btnHomeSettings.transform, "GearIcon", spriteIconGear, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(44f, 44f));
                iconGearGo.GetComponent<Image>().preserveAspect = true;
            }
            uiCtrl.btnHomeSettings = btnHomeSettings.GetComponent<Button>();

            // Home Header & 3D Logo (Centered below top bar, Y = 0.825f, compact so it never crowds character)
            var headerSprite = spriteLogoHitMe != null ? spriteLogoHitMe : spriteBannerWood;
            var homeHeader = CreateImage(homePanel.transform, "HomeBanner", headerSprite, new Vector2(0.5f, 0.825f), new Vector2(0.5f, 0.825f), new Vector2(0.5f, 0.5f), new Vector2(680f, 300f));
            homeHeader.GetComponent<Image>().preserveAspect = true;
            var logoTxt = CreateTMP(homeHeader.transform, "LogoTxt", fontAsset, spriteLogoHitMe != null ? "" : UiStrings.GameTitle, 64, colorGoldSub, TextAlignmentOptions.Center);
            logoTxt.fontStyle = FontStyles.Bold;
            FillRect(logoTxt.rectTransform, new Vector2(20f, 10f), new Vector2(-20f, -10f));
            uiCtrl.txtHomeLogo = logoTxt;

            // Hero Soft Ground Shadow on the Village Courtyard's built-in Stone Lotus Pedestal (no duplicate pedestal sprite!)
            var spriteShadow = LoadSprite("Assets/Sprites/procedural_shadow.png");
            var pedestalObj = CreateImage(homePanel.transform, "HeroPedestal", spriteShadow, new Vector2(0.5f, 0.515f), new Vector2(0.5f, 0.515f), new Vector2(0.5f, 0.5f), new Vector2(270f, 64f));
            pedestalObj.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.42f);

            var heroObj = CreateImage(homePanel.transform, "HeroCharacter", configAsset?.characterIdleSprites?[0] ?? LoadSprite(SpritesPath + "cau_be_lang_idle.png"), new Vector2(0.5f, 0.625f), new Vector2(0.5f, 0.625f), new Vector2(0.5f, 0.5f), new Vector2(360f, 420f));
            var heroImg = heroObj.GetComponent<Image>();
            heroImg.preserveAspect = true;
            uiCtrl.imgHomeCharPortrait = heroImg;

            // Hero Navigation Arrows (Large Gold 3D Circles 92x92 + Chevron Icons)
            var btnPrevHero = CreateButton(homePanel.transform, "BtnHeroPrev", spriteCircleGold, new Vector2(0.14f, 0.615f), new Vector2(0.14f, 0.615f), new Vector2(0.5f, 0.5f), new Vector2(92f, 92f));
            if (spriteIconChevronLeft != null)
            {
                var iconPrev = CreateImage(btnPrevHero.transform, "Icon", spriteIconChevronLeft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(46f, 46f));
                iconPrev.GetComponent<Image>().preserveAspect = true;
            }
            btnPrevHero.GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                uiCtrl.ChangePracticeCharacter(-1);
            });

            var btnNextHero = CreateButton(homePanel.transform, "BtnHeroNext", spriteCircleGold, new Vector2(0.86f, 0.615f), new Vector2(0.86f, 0.615f), new Vector2(0.5f, 0.5f), new Vector2(92f, 92f));
            if (spriteIconChevronRight != null)
            {
                var iconNext = CreateImage(btnNextHero.transform, "Icon", spriteIconChevronRight, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(46f, 46f));
                iconNext.GetComponent<Image>().preserveAspect = true;
            }
            btnNextHero.GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                uiCtrl.ChangePracticeCharacter(1);
            });

            // 2 Symmetrical Loadout Selector Cards side by side (Y = 0.435f, 435x126)
            // Left Card: Character Selector
            var charCard = CreatePanel(homePanel.transform, "CharCard", new Vector2(0.28f, 0.435f), new Vector2(0.28f, 0.435f), new Vector2(0.5f, 0.5f), new Vector2(435f, 126f), spritePanelCardDarkGold, Color.white);
            var charCardBtn = charCard.AddComponent<Button>();
            charCardBtn.onClick.AddListener(() => { AudioManager.Instance?.PlayClick(); uiCtrl.ChangePracticeCharacter(1); });
            var charIcon = CreateImage(charCard.transform, "Icon", spritePortraitNamLang ?? configAsset?.characterIdleSprites?[0] ?? LoadSprite(SpritesPath + "cau_be_lang_idle.png"), new Vector2(0.17f, 0.5f), new Vector2(0.17f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(78f, 78f));
            charIcon.GetComponent<Image>().preserveAspect = true;
            CreateTMP(charCard.transform, "Title", fontAsset, "NHÂN VẬT", 19, new Color(0.098f, 0.835f, 0.847f), TextAlignmentOptions.MidlineLeft, new Vector2(0.32f, 0.68f), new Vector2(230f, 28f), wrap: false, pivot: new Vector2(0f, 0.5f));
            var charNameTxt = CreateTMP(charCard.transform, "CharName", fontAsset, "Nam Làng", 27, colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.32f, 0.34f), new Vector2(230f, 36f), wrap: false, pivot: new Vector2(0f, 0.5f));
            charNameTxt.fontStyle = FontStyles.Bold;
            uiCtrl.txtHomeCharName = charNameTxt;
            uiCtrl.imgHomeCharCardIcon = charIcon.GetComponent<Image>();
            if (spriteIconChevronRight != null)
            {
                var chevR = CreateImage(charCard.transform, "Chev", spriteIconChevronRight, new Vector2(0.91f, 0.5f), new Vector2(0.91f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(28f, 28f));
                chevR.GetComponent<Image>().preserveAspect = true;
            }

            // Right Card: Weapon Selector
            var weaponCard = CreatePanel(homePanel.transform, "WeaponCard", new Vector2(0.72f, 0.435f), new Vector2(0.72f, 0.435f), new Vector2(0.5f, 0.5f), new Vector2(435f, 126f), spritePanelCardDarkGold, Color.white);
            var weaponCardBtn = weaponCard.AddComponent<Button>();
            weaponCardBtn.onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                uiCtrl.ShowToast("Vũ khí: Dép Tổ Ong (Bán kính 0.52m • Sát thương 1 HP)");
            });
            var weaponIconHome = CreateImage(weaponCard.transform, "Icon", spriteIconWeaponDep, new Vector2(0.17f, 0.5f), new Vector2(0.17f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(78f, 78f));
            weaponIconHome.GetComponent<Image>().preserveAspect = true;
            CreateTMP(weaponCard.transform, "Title", fontAsset, "VŨ KHÍ", 19, colorGoldSub, TextAlignmentOptions.MidlineLeft, new Vector2(0.32f, 0.68f), new Vector2(230f, 28f), wrap: false, pivot: new Vector2(0f, 0.5f));
            var weaponTxtHome = CreateTMP(weaponCard.transform, "Text", fontAsset, "Dép Tổ Ong", 27, colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.32f, 0.34f), new Vector2(230f, 36f), wrap: false, pivot: new Vector2(0f, 0.5f));
            weaponTxtHome.fontStyle = FontStyles.Bold;
            if (spriteIconChevronRight != null)
            {
                var chevR2 = CreateImage(weaponCard.transform, "Chev", spriteIconChevronRight, new Vector2(0.91f, 0.5f), new Vector2(0.91f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(28f, 28f));
                chevR2.GetComponent<Image>().preserveAspect = true;
            }

            // Action Buttons (V7.0 Hierarchy: Primary Yellow CTA -> Symmetrical Create/Join -> Bot Training)
            // 1. CHƠI NHANH (Primary CTA Vàng 3D - Y = 0.335f, 890x155)
            var btnQuick = CreateButton(homePanel.transform, "BtnQuickMatch", spriteBtnPrimaryYellow, new Vector2(0.5f, 0.335f), new Vector2(0.5f, 0.335f), new Vector2(0.5f, 0.5f), new Vector2(890f, 155f));
            if (spriteIconPlay != null)
            {
                var iconPlay = CreateImage(btnQuick.transform, "PlayIcon", spriteIconPlay, new Vector2(0.24f, 0.50f), new Vector2(0.24f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(54f, 54f));
                iconPlay.GetComponent<Image>().preserveAspect = true;
            }
            var btnQuickTxt = CreateTMP(btnQuick.transform, "Label", fontAsset, "CHƠI NHANH", 40, new Color(0.14f, 0.09f, 0.03f), TextAlignmentOptions.Center, new Vector2(0.53f, 0.62f), new Vector2(680f, 48f));
            btnQuickTxt.fontStyle = FontStyles.Bold;
            var btnQuickSub = CreateTMP(btnQuick.transform, "Sub", fontAsset, "Ghép trận Online • 2–6 người", 22, new Color(0.32f, 0.18f, 0.04f), TextAlignmentOptions.Center, new Vector2(0.53f, 0.28f), new Vector2(680f, 30f));
            uiCtrl.btnQuickMatch = btnQuick.GetComponent<Button>();

            // 2. TẠO PHÒNG (3D Blue) + VÀO PHÒNG (3D Green) - Row Y = 0.238f, 435x132
            var btnCreate = CreateButton(homePanel.transform, "BtnCreateRoom", spriteBtnBlue3D, new Vector2(0.28f, 0.238f), new Vector2(0.28f, 0.238f), new Vector2(0.5f, 0.5f), new Vector2(435f, 132f));
            if (spriteIconPersonAdd != null)
            {
                var iconCreate = CreateImage(btnCreate.transform, "Icon", spriteIconPersonAdd, new Vector2(0.17f, 0.50f), new Vector2(0.17f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(44f, 44f));
                iconCreate.GetComponent<Image>().preserveAspect = true;
            }
            var btnCreateTxt = CreateTMP(btnCreate.transform, "Label", fontAsset, "TẠO PHÒNG", 30, Color.white, TextAlignmentOptions.Center, new Vector2(0.58f, 0.50f), new Vector2(270f, 46f), wrap: false);
            btnCreateTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnCreateRoom = btnCreate.GetComponent<Button>();

            var btnJoin = CreateButton(homePanel.transform, "BtnJoinRoom", spriteBtnGreen3D, new Vector2(0.72f, 0.238f), new Vector2(0.72f, 0.238f), new Vector2(0.5f, 0.5f), new Vector2(435f, 132f));
            if (spriteIconPeople != null)
            {
                var iconJoin = CreateImage(btnJoin.transform, "Icon", spriteIconPeople, new Vector2(0.17f, 0.50f), new Vector2(0.17f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(44f, 44f));
                iconJoin.GetComponent<Image>().preserveAspect = true;
            }
            var btnJoinTxt = CreateTMP(btnJoin.transform, "Label", fontAsset, "VÀO PHÒNG", 30, Color.white, TextAlignmentOptions.Center, new Vector2(0.58f, 0.50f), new Vector2(270f, 46f), wrap: false);
            btnJoinTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnJoinRoom = btnJoin.GetComponent<Button>();

            // 3. LUYỆN VỚI BOT (3D Navy Button - Y = 0.148f, 890x124)
            var btnBot = CreateButton(homePanel.transform, "BtnBotTraining", spriteBtnNavy3D, new Vector2(0.5f, 0.148f), new Vector2(0.5f, 0.148f), new Vector2(0.5f, 0.5f), new Vector2(890f, 124f));
            if (spriteIconBot != null)
            {
                var iconBot = CreateImage(btnBot.transform, "Icon", spriteIconBot, new Vector2(0.14f, 0.50f), new Vector2(0.14f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(48f, 48f));
                iconBot.GetComponent<Image>().preserveAspect = true;
            }
            var btnBotTxt = CreateTMP(btnBot.transform, "Label", fontAsset, "LUYỆN VỚI BOT", 32, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.52f, 0.50f), new Vector2(660f, 44f), wrap: false);
            btnBotTxt.fontStyle = FontStyles.Bold;
            if (spriteIconChevronRight != null)
            {
                var chevBot = CreateImage(btnBot.transform, "Chevron", spriteIconChevronRight, new Vector2(0.91f, 0.50f), new Vector2(0.91f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(32f, 32f));
                chevBot.GetComponent<Image>().preserveAspect = true;
            }
            uiCtrl.btnBotTraining = btnBot.GetComponent<Button>();

            // Bottom Dock Navigation (HOME-07: Unified dock_bg 960x114 with 3 tabs, Trang chủ active in Cyan #19D5D8)
            var navBarBg = CreatePanel(homePanel.transform, "BottomNavBar", new Vector2(0.5f, 0.052f), new Vector2(0.5f, 0.052f), new Vector2(0.5f, 0.5f), new Vector2(960f, 114f), spriteDockBg, Color.white);
            
            // Tab 1: Trang chủ (Active / Cyan)
            var btnHomeTab = CreateButton(navBarBg.transform, "BtnTabHome", null, new Vector2(0.20f, 0.50f), new Vector2(0.20f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(280f, 80f));
            if (spriteIconHome != null)
            {
                var iconTabHome = CreateImage(btnHomeTab.transform, "Icon", spriteIconHome, new Vector2(0.20f, 0.50f), new Vector2(0.20f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(36f, 36f));
                iconTabHome.GetComponent<Image>().preserveAspect = true;
            }
            var txtHomeTab = CreateTMP(btnHomeTab.transform, "Label", fontAsset, "Trang chủ", 24, new Color(0.098f, 0.835f, 0.847f), TextAlignmentOptions.Center, new Vector2(0.62f, 0.50f), new Vector2(170f, 40f), wrap: false);
            txtHomeTab.fontStyle = FontStyles.Bold;
            // Cyan underline for active tab
            var tabUnderline = CreatePanel(btnHomeTab.transform, "Underline", new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.5f), new Vector2(130f, 5f), null, new Color(0.098f, 0.835f, 0.847f));
            uiCtrl.btnHomeLogin = btnHomeTab.GetComponent<Button>();

            // Tab 2: Xếp hạng
            var btnRankTab = CreateButton(navBarBg.transform, "BtnTabRank", null, new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(280f, 80f));
            if (spriteIconTrophy != null)
            {
                var iconTabRank = CreateImage(btnRankTab.transform, "Icon", spriteIconTrophy, new Vector2(0.20f, 0.50f), new Vector2(0.20f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(34f, 34f));
                iconTabRank.GetComponent<Image>().preserveAspect = true;
            }
            var txtRankTab = CreateTMP(btnRankTab.transform, "Label", fontAsset, "Xếp hạng", 23, colorCream, TextAlignmentOptions.Center, new Vector2(0.62f, 0.50f), new Vector2(170f, 40f), wrap: false);
            btnRankTab.GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                uiCtrl.ShowToast("Tính năng Xếp hạng sắp ra mắt!");
            });

            // Tab 3: Nhiệm vụ
            var btnQuestTab = CreateButton(navBarBg.transform, "BtnTabQuest", null, new Vector2(0.80f, 0.50f), new Vector2(0.80f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(280f, 80f));
            if (spriteIconGift != null)
            {
                var iconTabQuest = CreateImage(btnQuestTab.transform, "Icon", spriteIconGift, new Vector2(0.20f, 0.50f), new Vector2(0.20f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(34f, 34f));
                iconTabQuest.GetComponent<Image>().preserveAspect = true;
            }
            var txtQuestTab = CreateTMP(btnQuestTab.transform, "Label", fontAsset, "Nhiệm vụ", 23, colorCream, TextAlignmentOptions.Center, new Vector2(0.62f, 0.50f), new Vector2(170f, 40f), wrap: false);
            btnQuestTab.GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                uiCtrl.ShowToast("Nhiệm vụ ngày sắp ra mắt!");
            });

            // Beta 0.1: Postponed tabs (Xếp hạng, Nhiệm vụ, Túi đồ) are cleanly hidden without invisible hit targets
            var btnLbBottom = CreateButton(navBarBg.transform, "BtnBottomLb", null, new Vector2(-20f, -20f), new Vector2(-20f, -20f), new Vector2(0.5f, 0.5f), Vector2.one);
            uiCtrl.btnHomeLeaderboard = btnLbBottom.GetComponent<Button>();
            btnLbBottom.gameObject.SetActive(false);

            var btnShopBottom = CreateButton(navBarBg.transform, "BtnBottomShop", null, new Vector2(-20f, -20f), new Vector2(-20f, -20f), new Vector2(0.5f, 0.5f), Vector2.one);
            uiCtrl.btnHomeShop = btnShopBottom.GetComponent<Button>();
            btnShopBottom.gameObject.SetActive(false);

            var btnInvBottom = CreateButton(homePanel.transform, "BtnBottomInv", null, new Vector2(-20f, -20f), new Vector2(-20f, -20f), new Vector2(0.5f, 0.5f), Vector2.one);
            uiCtrl.btnHomeInventory = btnInvBottom.GetComponent<Button>();
            btnInvBottom.gameObject.SetActive(false);

            // ==================== C. CREATE ROOM PANEL (PHASE G REDESIGN) ====================
            var createRoomPanel = CreatePanel(rootTr, "CreateRoomPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.043f, 0.078f, 0.129f, 0.97f));
            createRoomPanel.SetActive(false);
            uiCtrl.createRoomPanel = createRoomPanel;

            var btnBackCreate = CreateButton(createRoomPanel.transform, "BtnBack", spriteCircleBack, new Vector2(0.12f, 0.935f), new Vector2(0.12f, 0.935f), new Vector2(0.5f, 0.5f), new Vector2(96f, 96f));
            uiCtrl.btnBackFromCreate = btnBackCreate.GetComponent<Button>();

            var createHeader = CreateImage(createRoomPanel.transform, "CreateBanner", spriteBannerWood, new Vector2(0.54f, 0.935f), new Vector2(0.54f, 0.935f), new Vector2(0.5f, 0.5f), new Vector2(660f, 130f));
            CreateTMP(createHeader.transform, "Title", fontAsset, "TẠO PHÒNG ĐẤU", 42, colorGoldSub, TextAlignmentOptions.Center);

            // 1-Column Structured Form Card — Zero Left-Edge Overflow (pivot = (0, 0.5) for left labels, (1, 0.5) for right values)
            var createCard = CreatePanel(createRoomPanel.transform, "CreateCard", new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(940f, 820f), spritePanelDarkGold, Color.white);
            CreateTMP(createCard.transform, "L1", fontAsset, "TÊN PHÒNG ĐẤU", 26, colorGoldSub, TextAlignmentOptions.MidlineLeft, new Vector2(0.06f, 0.89f), new Vector2(820f, 40f), wrap: false, pivot: new Vector2(0f, 0.5f));

            var nameInputObj = CreatePanel(createCard.transform, "InputNameBox", new Vector2(0.5f, 0.77f), new Vector2(0.5f, 0.77f), new Vector2(0.5f, 0.5f), new Vector2(830f, 96f), spriteInputCyan, Color.white);
            var nameInputField = nameInputObj.AddComponent<TMP_InputField>();
            var nameInputText = CreateTMP(nameInputObj.transform, "Text", fontAsset, "Phòng Chiến Hữu", 34, colorCream, TextAlignmentOptions.MidlineLeft);
            FillRect(nameInputText.rectTransform, new Vector2(28f, 8f), new Vector2(-28f, -8f));
            nameInputField.textComponent = nameInputText;
            nameInputField.text = "Phòng Chiến Hữu";
            uiCtrl.inputRoomName = nameInputField;

            // Row 1: Số người tối đa
            var rowPlayers = CreatePanel(createCard.transform, "RowPlayers", new Vector2(0.5f, 0.59f), new Vector2(0.5f, 0.59f), new Vector2(0.5f, 0.5f), new Vector2(830f, 88f), spriteRowLeaderboard, Color.white);
            CreateTMP(rowPlayers.transform, "Label", fontAsset, "Số người tối đa", 26, new Color(0.72f, 0.78f, 0.88f), TextAlignmentOptions.MidlineLeft, new Vector2(0.05f, 0.5f), new Vector2(360f, 48f), wrap: false, pivot: new Vector2(0f, 0.5f));
            CreateTMP(rowPlayers.transform, "Value", fontAsset, "6 người (Chuẩn)", 28, colorGoldSub, TextAlignmentOptions.MidlineRight, new Vector2(0.95f, 0.5f), new Vector2(380f, 48f), wrap: false, pivot: new Vector2(1f, 0.5f));

            // Row 2: Chế độ phòng
            var rowMode = CreatePanel(createCard.transform, "RowMode", new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.5f), new Vector2(830f, 88f), spriteRowLeaderboard, Color.white);
            CreateTMP(rowMode.transform, "Label", fontAsset, "Chế độ phòng", 26, new Color(0.72f, 0.78f, 0.88f), TextAlignmentOptions.MidlineLeft, new Vector2(0.05f, 0.5f), new Vector2(360f, 48f), wrap: false, pivot: new Vector2(0f, 0.5f));
            CreateTMP(rowMode.transform, "Value", fontAsset, "Công khai • Mã mời", 28, new Color(0.098f, 0.835f, 0.847f), TextAlignmentOptions.MidlineRight, new Vector2(0.95f, 0.5f), new Vector2(380f, 48f), wrap: false, pivot: new Vector2(1f, 0.5f));

            // Row 3: Bản đồ thi đấu
            var rowMap = CreatePanel(createCard.transform, "RowMap", new Vector2(0.5f, 0.29f), new Vector2(0.5f, 0.29f), new Vector2(0.5f, 0.5f), new Vector2(830f, 88f), spriteRowLeaderboard, Color.white);
            CreateTMP(rowMap.transform, "Label", fontAsset, "Bản đồ thi đấu", 26, new Color(0.72f, 0.78f, 0.88f), TextAlignmentOptions.MidlineLeft, new Vector2(0.05f, 0.5f), new Vector2(360f, 48f), wrap: false, pivot: new Vector2(0f, 0.5f));
            CreateTMP(rowMap.transform, "Value", fontAsset, "Sân Làng Đình Cổ", 28, colorCream, TextAlignmentOptions.MidlineRight, new Vector2(0.95f, 0.5f), new Vector2(380f, 48f), wrap: false, pivot: new Vector2(1f, 0.5f));

            // Row 4: Thời gian ngắm
            var rowTimer = CreatePanel(createCard.transform, "RowTimer", new Vector2(0.5f, 0.14f), new Vector2(0.5f, 0.14f), new Vector2(0.5f, 0.5f), new Vector2(830f, 88f), spriteRowLeaderboard, Color.white);
            CreateTMP(rowTimer.transform, "Label", fontAsset, "Thời gian đặt & ngắm", 26, new Color(0.72f, 0.78f, 0.88f), TextAlignmentOptions.MidlineLeft, new Vector2(0.05f, 0.5f), new Vector2(380f, 48f), wrap: false, pivot: new Vector2(0f, 0.5f));
            CreateTMP(rowTimer.transform, "Value", fontAsset, "5.0 giây / vòng", 28, colorGoldSub, TextAlignmentOptions.MidlineRight, new Vector2(0.95f, 0.5f), new Vector2(360f, 48f), wrap: false, pivot: new Vector2(1f, 0.5f));

            var btnSubmitCreate = CreateButton(createRoomPanel.transform, "BtnSubmitCreate", spriteBtnPrimaryYellow, new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.5f), new Vector2(880f, 142f));
            CreateTMP(btnSubmitCreate.transform, "Label", fontAsset, "TẠO PHÒNG NGAY", 40, new Color(0.14f, 0.09f, 0.03f), TextAlignmentOptions.Center);
            uiCtrl.btnSubmitCreateRoom = btnSubmitCreate.GetComponent<Button>();

            // ==================== D. JOIN ROOM PANEL (PHASE I REDESIGN) ====================
            var joinRoomPanel = CreatePanel(rootTr, "JoinRoomPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.043f, 0.078f, 0.129f, 0.97f));
            joinRoomPanel.SetActive(false);
            uiCtrl.joinRoomPanel = joinRoomPanel;

            var btnBackJoin = CreateButton(joinRoomPanel.transform, "BtnBack", spriteCircleBack, new Vector2(0.12f, 0.935f), new Vector2(0.12f, 0.935f), new Vector2(0.5f, 0.5f), new Vector2(96f, 96f));
            uiCtrl.btnBackFromJoin = btnBackJoin.GetComponent<Button>();

            var joinHeader = CreateImage(joinRoomPanel.transform, "JoinBanner", spriteBannerWood, new Vector2(0.54f, 0.935f), new Vector2(0.54f, 0.935f), new Vector2(0.5f, 0.5f), new Vector2(660f, 130f));
            CreateTMP(joinHeader.transform, "Title", fontAsset, "VÀO PHÒNG", 42, colorGoldSub, TextAlignmentOptions.Center);

            // Positioned in upper half (Y = 0.67) so mobile OS keyboard never covers input or CTA
            var joinCard = CreatePanel(joinRoomPanel.transform, "JoinCard", new Vector2(0.5f, 0.67f), new Vector2(0.5f, 0.67f), new Vector2(0.5f, 0.5f), new Vector2(940f, 430f), spritePanelDarkGold, Color.white);
            CreateTMP(joinCard.transform, "L1", fontAsset, "NHẬP MÃ PHÒNG (6 KÝ TỰ)", 28, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.5f, 0.84f), new Vector2(820f, 44f));

            var codeInputObj = CreatePanel(joinCard.transform, "InputCodeBox", new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.5f), new Vector2(820f, 112f), spriteInputCyan, Color.white);
            var codeInputField = codeInputObj.AddComponent<TMP_InputField>();
            codeInputField.characterLimit = 8;
            var codePlaceholder = CreateTMP(codeInputObj.transform, "Placeholder", fontAsset, "Nhập mã phòng (VD: HM4826)...", 30, new Color(0.65f, 0.75f, 0.88f, 0.55f), TextAlignmentOptions.Center);
            FillRect(codePlaceholder.rectTransform, new Vector2(24f, 8f), new Vector2(-24f, -8f));
            var codeInputText = CreateTMP(codeInputObj.transform, "Text", fontAsset, "", 44, colorGoldSub, TextAlignmentOptions.Center);
            codeInputText.fontStyle = FontStyles.Bold;
            FillRect(codeInputText.rectTransform, new Vector2(24f, 8f), new Vector2(-24f, -8f));
            codeInputField.textComponent = codeInputText;
            codeInputField.placeholder = codePlaceholder;
            codeInputField.text = "";
            uiCtrl.inputRoomCode = codeInputField;

            var btnPaste = CreateButton(joinCard.transform, "BtnPaste", spriteBtnYellow, new Vector2(0.28f, 0.22f), new Vector2(0.28f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(380f, 88f));
            CreateTMP(btnPaste.transform, "Label", fontAsset, "DÁN MÃ", 30, new Color(0.14f, 0.09f, 0.03f), TextAlignmentOptions.Center);
            uiCtrl.btnPasteCode = btnPaste.GetComponent<Button>();

            var btnScan = CreateButton(joinCard.transform, "BtnScan", spriteBtnBlue, new Vector2(0.72f, 0.22f), new Vector2(0.72f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(380f, 88f));
            CreateTMP(btnScan.transform, "Label", fontAsset, "XÓA MÃ", 30, Color.white, TextAlignmentOptions.Center);
            uiCtrl.btnScanQr = btnScan.GetComponent<Button>();

            // Hidden by default until a valid room lookup succeeds
            var foundCard = CreatePanel(joinRoomPanel.transform, "FoundCard", new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.5f), new Vector2(940f, 150f), spriteCardSlot, Color.white);
            var foundTxt = CreateTMP(foundCard.transform, "FoundText", fontAsset, "", 28, new Color(0.09f, 0.725f, 0.529f), TextAlignmentOptions.Center);
            FillRect(foundTxt.rectTransform, new Vector2(24f, 10f), new Vector2(-24f, -10f));
            foundCard.SetActive(false);
            uiCtrl.roomFoundCard = foundCard;
            uiCtrl.txtFoundRoomInfo = foundTxt;

            var btnSubmitJoin = CreateButton(joinRoomPanel.transform, "BtnSubmitJoin", spriteBtnGreen3D, new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.5f), new Vector2(880f, 140f));
            CreateTMP(btnSubmitJoin.transform, "Label", fontAsset, "VÀO PHÒNG NGAY", 40, Color.white, TextAlignmentOptions.Center);
            uiCtrl.btnSubmitJoinRoom = btnSubmitJoin.GetComponent<Button>();

            // ==================== E. BOT TRAINING PANEL (PHASE L REDESIGN) ====================
            var botPanel = CreatePanel(rootTr, "BotTrainingPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, Color.clear);
            var botPanelRt = botPanel.GetComponent<RectTransform>();
            botPanelRt.offsetMin = Vector2.zero;
            botPanelRt.offsetMax = Vector2.zero;
            var botBg = CreatePanel(botPanel.transform, "BotBg", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.043f, 0.078f, 0.129f, 0.98f));
            var botBgRt = botBg.GetComponent<RectTransform>();
            botBgRt.offsetMin = new Vector2(-200f, -450f);
            botBgRt.offsetMax = new Vector2(200f, 450f);
            botPanel.SetActive(false);
            uiCtrl.botTrainingPanel = botPanel;

            // Back button
            var btnBackBot = CreateButton(botPanel.transform, "BtnBack", spriteCircleBack, new Vector2(0.12f, 0.935f), new Vector2(0.12f, 0.935f), new Vector2(0.5f, 0.5f), new Vector2(96f, 96f));
            uiCtrl.btnBackFromBotTraining = btnBackBot.GetComponent<Button>();

            // Header Banner
            var botHeader = CreateImage(botPanel.transform, "BotBanner", spriteBannerWood, new Vector2(0.54f, 0.935f), new Vector2(0.54f, 0.935f), new Vector2(0.5f, 0.5f), new Vector2(680f, 130f));
            var botTitleTxt = CreateTMP(botHeader.transform, "Title", fontAsset, UiStrings.BotTrainingTitle, 40, colorGoldSub, TextAlignmentOptions.Center);
            botTitleTxt.fontStyle = FontStyles.Bold;
            uiCtrl.txtBotTrainingHeader = botTitleTxt;

            // Subtitle
            CreateTMP(botPanel.transform, "Subtitle", fontAsset, UiStrings.BotTrainingSubtitle, 23, new Color(0.72f, 0.78f, 0.88f), TextAlignmentOptions.Center, new Vector2(0.5f, 0.865f), new Vector2(920f, 44f));

            // Section: Choose Character
            CreateTMP(botPanel.transform, "CharSelectHeader", fontAsset, "CHỌN NHÂN VẬT RA TRẬN", 26, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.5f, 0.818f), new Vector2(900f, 38f));

            // Character Cards Container (4 active characters prominent in Row 1, 3 upcoming in Row 2)
            var charGrid = CreatePanel(botPanel.transform, "CharGrid", new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.5f), new Vector2(960f, 265f), spritePanelDarkGold, Color.white);
            uiCtrl.botCharCardFrames.Clear();
            uiCtrl.botCharCardButtons.Clear();

            float[] r1X = { 0.14f, 0.38f, 0.62f, 0.86f };
            float[] r2X = { 0.24f, 0.50f, 0.76f };

            for (int c = 0; c < 7; c++)
            {
                float nx = c < 4 ? r1X[c] : r2X[c - 4];
                float ny = c < 4 ? 0.72f : 0.26f;

                var cardObj = CreatePanel(charGrid.transform, "CharCard_" + c, new Vector2(nx, ny), new Vector2(nx, ny), new Vector2(0.5f, 0.5f), new Vector2(218f, 116f), c == 0 ? spriteCardSelected : spriteCardChar, Color.white);
                var cardImg = cardObj.GetComponent<Image>();
                var cardBtn = cardObj.AddComponent<Button>();

                bool isEnabledChar = c < 4;
                Sprite spr = (charHeadSprites != null && c < charHeadSprites.Length) ? charHeadSprites[c] : null;

                var charSprObj = CreateImage(cardObj.transform, "CharSpr", spr, new Vector2(0.21f, 0.5f), new Vector2(0.21f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(68f, 68f));
                charSprObj.GetComponent<Image>().preserveAspect = true;

                string cShortName = c < PlayerView.CharacterNames.Length ? PlayerView.CharacterNames[c] : $"NV {c + 1}";
                var cLabel = CreateTMP(cardObj.transform, "Name", fontAsset, cShortName, 20, isEnabledChar ? colorCream : new Color(0.68f, 0.74f, 0.82f, 0.75f), TextAlignmentOptions.MidlineLeft, new Vector2(0.40f, isEnabledChar ? 0.50f : 0.60f), new Vector2(120f, 70f), wrap: true, pivot: new Vector2(0f, 0.5f));
                cLabel.fontStyle = FontStyles.Bold;

                if (!isEnabledChar)
                {
                    cardImg.color = new Color(0.50f, 0.54f, 0.62f, 0.72f);
                    var badge = CreateTMP(cardObj.transform, "Badge", fontAsset, "SẮP MỞ", 15, colorGoldSub, TextAlignmentOptions.MidlineLeft, new Vector2(0.40f, 0.22f), new Vector2(110f, 22f), wrap: false, pivot: new Vector2(0f, 0.5f));
                    badge.fontStyle = FontStyles.Bold;
                }

                uiCtrl.botCharCardFrames.Add(cardImg);
                uiCtrl.botCharCardButtons.Add(cardBtn);
            }

            // Character & Weapon Loadout Detail Box
            var charDetailCard = CreatePanel(botPanel.transform, "CharDetailCard", new Vector2(0.5f, 0.495f), new Vector2(0.5f, 0.495f), new Vector2(0.5f, 0.5f), new Vector2(940f, 144f), spritePanelDarkGold, Color.white);
            if (spriteIconWeaponDep != null)
            {
                var wpPreview = CreateImage(charDetailCard.transform, "WeaponPreview", spriteIconWeaponDep, new Vector2(0.09f, 0.50f), new Vector2(0.09f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(84f, 84f));
                wpPreview.GetComponent<Image>().preserveAspect = true;
            }
            var detailName = CreateTMP(charDetailCard.transform, "DetailName", fontAsset, UiStrings.Char1Name, 28, colorGoldSub, TextAlignmentOptions.MidlineLeft, new Vector2(0.17f, 0.68f), new Vector2(740f, 40f), wrap: false, pivot: new Vector2(0f, 0.5f));
            detailName.fontStyle = FontStyles.Bold;
            uiCtrl.txtSelectedCharacter = detailName;

            var detailDesc = CreateTMP(charDetailCard.transform, "DetailDesc", fontAsset, UiStrings.Char1Desc + " • Vũ khí: Dép Tổ Ong (0.52m)", 23, colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.17f, 0.30f), new Vector2(740f, 44f), wrap: true, pivot: new Vector2(0f, 0.5f));
            uiCtrl.txtSelectedCharDetail = detailDesc;

            // Bot Count Selector Card (Use clean 3D buttons without baked left-arrow sprite so - and + never overlap!)
            var botCountCard = CreatePanel(botPanel.transform, "BotCountCard", new Vector2(0.5f, 0.33f), new Vector2(0.5f, 0.33f), new Vector2(0.5f, 0.5f), new Vector2(940f, 170f), spritePanelDarkGold, Color.white);
            CreateTMP(botCountCard.transform, "Title", fontAsset, "SỐ ĐỐI THỦ MÁY (BOT) TRONG SÂN", 24, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.5f, 0.80f), new Vector2(800f, 35f));

            var btnDec = CreateButton(botCountCard.transform, "BtnDec", spriteBtnBlue, new Vector2(0.14f, 0.38f), new Vector2(0.14f, 0.38f), new Vector2(0.5f, 0.5f), new Vector2(104f, 84f));
            var decLabel = CreateTMP(btnDec.transform, "Label", fontAsset, "-", 46, Color.white, TextAlignmentOptions.Center);
            decLabel.fontStyle = FontStyles.Bold;
            uiCtrl.btnDecreaseBots = btnDec.GetComponent<Button>();

            var countDisplayTxt = CreateTMP(botCountCard.transform, "CountText", fontAsset, "4 người (1 Bạn + 3 Bot)", 34, colorCream, TextAlignmentOptions.Center, new Vector2(0.5f, 0.38f), new Vector2(520f, 60f));
            countDisplayTxt.fontStyle = FontStyles.Bold;
            uiCtrl.txtSelectedBotCount = countDisplayTxt;

            var btnInc = CreateButton(botCountCard.transform, "BtnInc", spriteBtnYellow, new Vector2(0.86f, 0.38f), new Vector2(0.86f, 0.38f), new Vector2(0.5f, 0.5f), new Vector2(104f, 84f));
            var incLabel = CreateTMP(btnInc.transform, "Label", fontAsset, "+", 46, new Color(0.14f, 0.09f, 0.03f), TextAlignmentOptions.Center);
            incLabel.fontStyle = FontStyles.Bold;
            uiCtrl.btnIncreaseBots = btnInc.GetComponent<Button>();

            // Start Bot Training CTA Yellow Button (Anchored to bottom at Y = 130px to fit 393x695)
            var btnStartBot = CreateButton(botPanel.transform, "BtnStartBot", spriteBtnPrimaryYellow, new Vector2(0.5f, 0.0f), new Vector2(0.5f, 0.0f), new Vector2(0.5f, 0.5f), new Vector2(880f, 150f));
            btnStartBot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 130f);
            var startBotTxt = CreateTMP(btnStartBot.transform, "Label", fontAsset, UiStrings.BtnStart + " (VÀO SÂN)", 42, new Color(0.14f, 0.09f, 0.03f), TextAlignmentOptions.Center);
            startBotTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnStartBotTraining = btnStartBot.GetComponent<Button>();

            // ==================== F. LOBBY PANEL (PHASE H REDESIGN) ====================
            var lobbyPanel = CreatePanel(rootTr, "LobbyPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.043f, 0.078f, 0.129f, 0.97f));
            lobbyPanel.SetActive(false);
            uiCtrl.lobbyPanel = lobbyPanel;

            // Back button
            var btnBackLobby = CreateButton(lobbyPanel.transform, "BtnBack", spriteCircleBack, new Vector2(0.12f, 0.935f), new Vector2(0.12f, 0.935f), new Vector2(0.5f, 0.5f), new Vector2(96f, 96f));
            btnBackLobby.GetComponent<Button>().onClick.AddListener(() => uiCtrl.ShowScreen(ProductionScreen.Home));

            // Header Banner (Constrained width + auto-sizing so long room titles never overflow onto Back button)
            var lobbyHeader = CreateImage(lobbyPanel.transform, "LobbyBanner", spriteBannerWood, new Vector2(0.55f, 0.935f), new Vector2(0.55f, 0.935f), new Vector2(0.5f, 0.5f), new Vector2(700f, 126f));
            var lobbyHeaderTxt = CreateTMP(lobbyHeader.transform, "Title", fontAsset, UiStrings.LobbyTitle, 32, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(610f, 64f), wrap: false);
            lobbyHeaderTxt.enableAutoSizing = true;
            lobbyHeaderTxt.fontSizeMin = 20f;
            lobbyHeaderTxt.fontSizeMax = 34f;
            uiCtrl.txtLobbyHeader = lobbyHeaderTxt;

            // Room Code Box & Copy button
            var roomCodeCard = CreatePanel(lobbyPanel.transform, "RoomCodeCard", new Vector2(0.5f, 0.855f), new Vector2(0.5f, 0.855f), new Vector2(0.5f, 0.5f), new Vector2(940f, 82f), spritePanelDarkGold, Color.white);
            var lobbyCodeTxt = CreateTMP(roomCodeCard.transform, "RoomCode", fontAsset, "MÃ PHÒNG: HM4826 • SÂN LÀNG", 27, colorGoldSub, TextAlignmentOptions.MidlineLeft, new Vector2(0.04f, 0.5f), new Vector2(580f, 50f), wrap: false, pivot: new Vector2(0f, 0.5f));
            uiCtrl.txtLobbyRoomCode = lobbyCodeTxt;

            var btnCopyCode = CreateButton(roomCodeCard.transform, "BtnCopyCode", spriteBtnYellow, new Vector2(0.84f, 0.5f), new Vector2(0.84f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(240f, 62f));
            var copyTxt = CreateTMP(btnCopyCode.transform, "Label", fontAsset, UiStrings.BtnCopyRoomCode, 24, new Color(0.14f, 0.09f, 0.03f), TextAlignmentOptions.Center);
            copyTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnCopyRoomCode = btnCopyCode.GetComponent<Button>();

            // 6 Slots Grid (with Character Avatar + Non-overlapping Name & Status Pill)
            var slotsRoot = CreatePanel(lobbyPanel.transform, "SlotsGrid", new Vector2(0.5f, 0.525f), new Vector2(0.5f, 0.525f), new Vector2(0.5f, 0.5f), new Vector2(940f, 550f), spritePanelDarkGold, Color.white);
            uiCtrl.lobbySlotsContainer = slotsRoot.transform;
            uiCtrl.lobbySlots.Clear();

            string[] defaultLobbyNames = new string[] { "Nam Làng (Bạn)", "Nữ Năng Động", "Quậy Boy", "Cô Ba", "Anh Teen", "Chú Chó Vàng" };
            for (int i = 0; i < 6; i++)
            {
                float yNorm = 0.88f - i * 0.155f;
                var slotObj = CreatePanel(slotsRoot.transform, "Slot_" + i, new Vector2(0.5f, yNorm), new Vector2(0.5f, yNorm), new Vector2(0.5f, 0.5f), new Vector2(880f, 80f), i == 0 ? spriteRowMe : spriteRowLeaderboard, Color.white);
                Sprite slotSpr = (charHeadSprites != null && charHeadSprites.Length > 0) ? charHeadSprites[i % Mathf.Min(4, charHeadSprites.Length)] : spritePortraitNamLang;
                if (slotSpr != null)
                {
                    var avGo = CreateImage(slotObj.transform, "SlotAvatar", slotSpr, new Vector2(0.06f, 0.5f), new Vector2(0.06f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(58f, 58f));
                    avGo.GetComponent<Image>().preserveAspect = true;
                }
                var sName = CreateTMP(slotObj.transform, "Name", fontAsset, defaultLobbyNames[i], 26, colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.12f, 0.5f), new Vector2(450f, 52f), wrap: false, pivot: new Vector2(0f, 0.5f));
                var sStatus = CreateTMP(slotObj.transform, "Status", fontAsset, i == 0 ? "CHƯA SẴN SÀNG" : "ĐÃ SẴN SÀNG", 24, i == 0 ? colorGoldSub : colorGreen, TextAlignmentOptions.MidlineRight, new Vector2(0.96f, 0.5f), new Vector2(280f, 52f), wrap: false, pivot: new Vector2(1f, 0.5f));

                var slotUI = new ProductionUIController.LobbySlotUI
                {
                    root = slotObj,
                    txtName = sName,
                    txtStatus = sStatus,
                    isReady = i != 0
                };
                uiCtrl.lobbySlots.Add(slotUI);
            }

            // Countdown / Notice Text (Positioned at Y = 0.255f, cleanly separated from SlotsGrid and buttons)
            var countdownTxt = CreateTMP(lobbyPanel.transform, "Countdown", fontAsset, UiStrings.LobbyWaitingAllReady, 24, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.5f, 0.255f), new Vector2(920f, 56f), wrap: true);
            uiCtrl.txtLobbyCountdown = countdownTxt;

            // 1-Player Button "SANG LUYỆN VỚI BOT"
            var btnGoBot = CreateButton(lobbyPanel.transform, "BtnGoToBotTraining", spriteBtnBlue, new Vector2(0.5f, 0.195f), new Vector2(0.5f, 0.195f), new Vector2(0.5f, 0.5f), new Vector2(880f, 88f));
            var goBotTxt = CreateTMP(btnGoBot.transform, "Label", fontAsset, UiStrings.BtnGoToBotTraining, 32, Color.white, TextAlignmentOptions.Center);
            goBotTxt.fontStyle = FontStyles.Bold;
            btnGoBot.SetActive(false);
            uiCtrl.btnLobbyToBotTraining = btnGoBot.GetComponent<Button>();

            // Ready / Start Button
            var btnLobbyReady = CreateButton(lobbyPanel.transform, "BtnLobbyReady", spriteBtnGreen3D, new Vector2(0.5f, 0.115f), new Vector2(0.5f, 0.115f), new Vector2(0.5f, 0.5f), new Vector2(880f, 108f));
            var lobbyReadyLabel = CreateTMP(btnLobbyReady.transform, "Label", fontAsset, UiStrings.BtnReady, 38, Color.white, TextAlignmentOptions.Center, wrap: false);
            lobbyReadyLabel.fontStyle = FontStyles.Bold;
            uiCtrl.btnLobbyReady = btnLobbyReady.GetComponent<Button>();
            uiCtrl.txtLobbyReadyLabel = lobbyReadyLabel;

            // Leave Room Button
            var btnLeave = CreateButton(lobbyPanel.transform, "BtnLeave", spriteBtnRed, new Vector2(0.5f, 0.040f), new Vector2(0.5f, 0.040f), new Vector2(0.5f, 0.5f), new Vector2(880f, 78f));
            var leaveTxt = CreateTMP(btnLeave.transform, "Label", fontAsset, UiStrings.BtnLeaveRoom, 28, Color.white, TextAlignmentOptions.Center, wrap: false);
            leaveTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnLeaveRoom = btnLeave.GetComponent<Button>();

            // ==================== G. QUICK MATCH OVERLAY (PHASE J REDESIGN) ====================
            var qmOverlay = CreatePanel(rootTr, "QuickMatchOverlay", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.03f, 0.06f, 0.10f, 0.92f));
            qmOverlay.SetActive(false);
            uiCtrl.quickMatchOverlay = qmOverlay;

            var qmCard = CreatePanel(qmOverlay.transform, "QmCard", new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(920f, 620f), spritePanelDarkGold, Color.white);
            var qmHeader = CreateImage(qmCard.transform, "QmBanner", spriteBannerWood, new Vector2(0.5f, 0.90f), new Vector2(0.5f, 0.90f), new Vector2(0.5f, 0.5f), new Vector2(680f, 114f));
            var qmTitle = CreateTMP(qmHeader.transform, "Title", fontAsset, "ĐANG TÌM TRẬN", 38, colorGoldSub, TextAlignmentOptions.Center);
            qmTitle.fontStyle = FontStyles.Bold;

            // Radar Pulse Ring + Central Player Avatar
            var qmPulseObj = CreateImage(qmCard.transform, "Pulse", spriteTimerPulse, new Vector2(0.5f, 0.64f), new Vector2(0.5f, 0.64f), new Vector2(0.5f, 0.5f), new Vector2(160f, 160f));
            qmPulseObj.GetComponent<Image>().color = new Color(0.098f, 0.835f, 0.847f, 0.90f);
            if (spritePortraitNamLang != null)
            {
                var qmAvatar = CreateImage(qmCard.transform, "PlayerAvatar", spritePortraitNamLang, new Vector2(0.5f, 0.64f), new Vector2(0.5f, 0.64f), new Vector2(0.5f, 0.5f), new Vector2(88f, 88f));
                qmAvatar.GetComponent<Image>().preserveAspect = true;
            }

            // 6 Player Slot Indicators (1 filled + 5 searching)
            for (int si = 0; si < 6; si++)
            {
                float sx = 0.25f + si * 0.10f;
                var slotDot = CreatePanel(qmCard.transform, "SlotDot_" + si, new Vector2(sx, 0.46f), new Vector2(sx, 0.46f), new Vector2(0.5f, 0.5f), new Vector2(44f, 16f), null, si == 0 ? new Color(0.09f, 0.725f, 0.529f, 1f) : new Color(0.22f, 0.30f, 0.42f, 0.85f));
            }

            var qmStatusTxt = CreateTMP(qmCard.transform, "StatusTxt", fontAsset, UiStrings.MatchmakingSearching, 28, colorCream, TextAlignmentOptions.Center, new Vector2(0.5f, 0.36f), new Vector2(840f, 52f), wrap: true);
            uiCtrl.txtQuickMatchStatus = qmStatusTxt;
            CreateTMP(qmCard.transform, "ElapsedTxt", fontAsset, "Thời gian chờ: 00:03 • Khu vực: VN-01", 21, new Color(0.68f, 0.76f, 0.88f), TextAlignmentOptions.Center, new Vector2(0.5f, 0.28f), new Vector2(760f, 34f), wrap: false);

            var btnQmToBot = CreateButton(qmCard.transform, "BtnQmToBot", spriteBtnYellow, new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(640f, 88f));
            var qmToBotTxt = CreateTMP(btnQmToBot.transform, "Label", fontAsset, "LUYỆN VỚI BOT NGAY", 28, new Color(0.14f, 0.09f, 0.03f), TextAlignmentOptions.Center);
            qmToBotTxt.fontStyle = FontStyles.Bold;
            btnQmToBot.SetActive(false);
            uiCtrl.btnQuickMatchToBots = btnQmToBot.GetComponent<Button>();

            var btnCancelQm = CreateButton(qmCard.transform, "BtnCancelQm", spriteBtnRed, new Vector2(0.5f, 0.11f), new Vector2(0.5f, 0.11f), new Vector2(0.5f, 0.5f), new Vector2(640f, 88f));
            var cancelQmTxt = CreateTMP(btnCancelQm.transform, "Label", fontAsset, "HỦY TÌM TRẬN", 28, Color.white, TextAlignmentOptions.Center);
            cancelQmTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnCancelQuickMatch = btnCancelQm.GetComponent<Button>();

            // ==================== F. GAMEPLAY HUD PANEL (PHASE M REDESIGN) ====================
            var hudPanel = CreatePanel(rootTr, "GameplayHudPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, Color.clear);
            hudPanel.SetActive(false);
            uiCtrl.gameplayHudPanel = hudPanel;

            // V7.0: Sleek translucent top & bottom HUD scrims (alpha = 0.24) instead of solid black/brown blocks covering the village courtyard!
            var topHudBoard = CreatePanel(hudPanel.transform, "TopHudBoard", new Vector2(0f, 0.860f), new Vector2(1f, 1.0f), new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.043f, 0.078f, 0.129f, 0.24f));
            topHudBoard.GetComponent<RectTransform>().offsetMin = new Vector2(-150f, 0f);
            topHudBoard.GetComponent<RectTransform>().offsetMax = new Vector2(150f, 150f);

            var bottomHudBoard = CreatePanel(hudPanel.transform, "BottomHudBoard", new Vector2(0f, 0.0f), new Vector2(1f, 0.120f), new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.043f, 0.078f, 0.129f, 0.24f));
            bottomHudBoard.GetComponent<RectTransform>().offsetMin = new Vector2(-150f, -150f);
            bottomHudBoard.GetComponent<RectTransform>().offsetMax = new Vector2(150f, 0f);

            // P3: TopPlacingBar (Round, Timer, and Sẵn sàng in the same horizontal safe-area row; only shown in Placing)
            var topPlacingBar = CreatePanel(hudPanel.transform, "TopPlacingBar", new Vector2(0.5f, 0.968f), new Vector2(0.5f, 0.968f), new Vector2(0.5f, 0.5f), new Vector2(1020f, 76f), null, Color.clear);
            uiCtrl.topPlacingBar = topPlacingBar;

            var roundPill = CreateImage(topPlacingBar.transform, "RoundPill", spriteBannerWood, new Vector2(0.18f, 0.5f), new Vector2(0.18f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(310f, 72f));
            var roundTxt = CreateTMP(roundPill.transform, "Text", fontAsset, "VÒNG 1/5", 28, colorCream, TextAlignmentOptions.Center);
            roundTxt.fontStyle = FontStyles.Bold;
            FillRect(roundTxt.rectTransform, Vector2.zero, Vector2.zero);
            uiCtrl.roundPill = roundPill;
            uiCtrl.txtRound = roundTxt;

            var timerPulse = CreateImage(topPlacingBar.transform, "TimerPulse", spriteTimerPulse, new Vector2(0.50f, 0.5f), new Vector2(0.50f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(96f, 96f));
            timerPulse.SetActive(false);
            uiCtrl.timerPulseRing = timerPulse.GetComponent<Image>();

            var timerPill = CreateImage(topPlacingBar.transform, "TimerPill", spriteTimerGold, new Vector2(0.50f, 0.5f), new Vector2(0.50f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(82f, 82f));
            var timerTxt = CreateTMP(timerPill.transform, "Text", fontAsset, "5.0s", 32, Color.white, TextAlignmentOptions.Center);
            timerTxt.outlineWidth = 0.22f;
            timerTxt.outlineColor = new Color32(11, 20, 33, 255);
            timerTxt.fontStyle = FontStyles.Bold;
            FillRect(timerTxt.rectTransform, Vector2.zero, Vector2.zero);
            uiCtrl.timerPill = timerPill;
            uiCtrl.txtTimer = timerTxt;

            var btnReadyInGame = CreateButton(topPlacingBar.transform, "BtnReady", spriteBtnYellow, new Vector2(0.80f, 0.5f), new Vector2(0.80f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(280f, 76f));
            var readyLabelInGame = CreateTMP(btnReadyInGame.transform, "Label", fontAsset, UiStrings.BtnReady, 28, new Color(0.14f, 0.09f, 0.03f), TextAlignmentOptions.Center);
            readyLabelInGame.fontStyle = FontStyles.Bold;
            uiCtrl.btnInGameReady = btnReadyInGame.GetComponent<Button>();
            uiCtrl.txtInGameReadyLabel = readyLabelInGame;

            // In-Game Settings Button (Top-right overlay access during gameplay, non-overlapping with BtnReady)
            var btnInGameSettings = CreateButton(hudPanel.transform, "BtnInGameSettings", spriteCircleBack, new Vector2(0.95f, 0.968f), new Vector2(0.95f, 0.968f), new Vector2(0.5f, 0.5f), new Vector2(76f, 76f));
            if (spriteIconGear != null)
            {
                var iconGearGo = CreateImage(btnInGameSettings.transform, "GearIcon", spriteIconGear, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(38f, 38f));
                iconGearGo.GetComponent<Image>().preserveAspect = true;
            }
            uiCtrl.btnInGameSettings = btnInGameSettings.GetComponent<Button>();

            // Phase Banner (below TopPlacingBar)
            var phaseBanner = CreateImage(hudPanel.transform, "PhaseBanner", spriteBannerWood, new Vector2(0.5f, 0.925f), new Vector2(0.5f, 0.925f), new Vector2(0.5f, 0.5f), new Vector2(980f, 52f));
            var phaseTxt = CreateTMP(phaseBanner.transform, "Text", fontAsset, UiStrings.PhasePlacing + " • Chạm để đặt, kéo để ngắm", 24, colorCream, TextAlignmentOptions.Center);
            phaseTxt.fontStyle = FontStyles.Bold;
            FillRect(phaseTxt.rectTransform, Vector2.zero, Vector2.zero);
            uiCtrl.phaseBannerObj = phaseBanner;
            uiCtrl.txtPhaseBanner = phaseTxt;

            // P3: 6 HP Cards in 2 rows x 3 columns (with Character Avatar + strictly non-overlapping Name, Hearts, Status)
            var hpContainer = CreatePanel(hudPanel.transform, "HpContainer", new Vector2(0.5f, 0.858f), new Vector2(0.5f, 0.858f), new Vector2(0.5f, 0.5f), new Vector2(1040f, 180f), null, Color.clear);
            uiCtrl.hpCardsContainer = hpContainer.transform;
            uiCtrl.hpCards.Clear();

            string[] playerIds = new string[] { "player", "bot-1", "bot-2", "bot-3", "bot-4", "bot-5" };
            string[] shortNames = new string[] { "Nam Làng", "Nữ Năng Động", "Cô Ba", "Anh Teen", "Quậy Boy", "Chú Chó Vàng" };
            float[] colAnchors = new float[] { 0.17f, 0.50f, 0.83f };
            float[] rowAnchors = new float[] { 0.75f, 0.25f };

            for (int i = 0; i < 6; i++)
            {
                int col = i % 3;
                int row = i / 3;
                Vector2 cardAnchor = new Vector2(colAnchors[col], rowAnchors[row]);
                var cardSprite = i == 0 ? spriteCardSelected : spriteCardChar;
                var cardObj = CreatePanel(hpContainer.transform, "HpCard_" + i, cardAnchor, cardAnchor, new Vector2(0.5f, 0.5f), new Vector2(330f, 76f), cardSprite, Color.white);
                var cardImg = cardObj.GetComponent<Image>();
                if (cardImg != null) cardImg.type = Image.Type.Sliced;

                Sprite headSpr = (charHeadSprites != null && charHeadSprites.Length > 0) ? charHeadSprites[i % Mathf.Min(4, charHeadSprites.Length)] : spritePortraitNamLang;
                if (headSpr != null)
                {
                    var headIcon = CreateImage(cardObj.transform, "CharHead", headSpr, new Vector2(0.08f, 0.66f), new Vector2(0.08f, 0.66f), new Vector2(0.5f, 0.5f), new Vector2(36f, 36f));
                    headIcon.GetComponent<Image>().preserveAspect = true;
                }

                var cName = CreateTMP(cardObj.transform, "Name", fontAsset, shortNames[i], 21, i == 0 ? colorGoldSub : colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.37f, 0.68f), new Vector2(148f, 30f), wrap: false, pivot: new Vector2(0.5f, 0.5f));
                cName.fontStyle = FontStyles.Bold;

                // Hearts Container (kept for test bindings, while crisp HP: 3/3 text is displayed for mobile legibility)
                var heartsContainer = CreatePanel(cardObj.transform, "HeartsContainer", new Vector2(0.80f, 0.68f), new Vector2(0.80f, 0.68f), new Vector2(0.5f, 0.5f), new Vector2(98f, 34f), null, Color.clear);
                Image[] heartImgs = new Image[3];
                for (int h = 0; h < 3; h++)
                {
                    float hx = (h - 1) * 31f;
                    var hObj = CreateImage(heartsContainer.transform, "Heart_" + h, spriteHeartFull, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(30f, 30f));
                    hObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(hx, 0f);
                    heartImgs[h] = hObj.GetComponent<Image>();
                }
                heartsContainer.SetActive(false);

                var cHearts = CreateTMP(cardObj.transform, "Hearts", fontAsset, "HP: 3/3", 19, colorGreen, TextAlignmentOptions.MidlineRight, new Vector2(0.80f, 0.68f), new Vector2(96f, 28f), wrap: false, pivot: new Vector2(0.5f, 0.5f));
                cHearts.fontStyle = FontStyles.Bold;
                cHearts.gameObject.SetActive(true);

                var cHpNum = CreateTMP(cardObj.transform, "HpNum", fontAsset, "CÒN SỐNG", 20, colorGreen, TextAlignmentOptions.Center, new Vector2(0.50f, 0.24f), new Vector2(280f, 26f), wrap: false, pivot: new Vector2(0.5f, 0.5f));
                cHpNum.fontStyle = FontStyles.Bold;

                uiCtrl.hpCards.Add(new ProductionUIController.PlayerHpCardUI
                {
                    root = cardObj,
                    imgBg = cardImg,
                    txtName = cName,
                    txtHearts = cHearts,
                    txtHpNum = cHpNum,
                    heartIcons = heartImgs,
                    playerId = playerIds[i]
                });
            }

            // P3: Weapon Indicator placed right below the bottom of the arena
            var weaponPill = CreateImage(hudPanel.transform, "WeaponPill", spriteBannerWood, new Vector2(0.5f, 0.062f), new Vector2(0.5f, 0.062f), new Vector2(0.5f, 0.5f), new Vector2(460f, 72f));
            var weaponIcon = CreateImage(weaponPill.transform, "WeaponIcon", spriteWeaponDep, new Vector2(0.14f, 0.5f), new Vector2(0.14f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(52f, 52f));
            var weaponTxt = CreateTMP(weaponPill.transform, "Text", fontAsset, UiStrings.WeaponSandalName, 26, colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.58f, 0.5f), new Vector2(300f, 50f));
            weaponTxt.fontStyle = FontStyles.Bold;
            uiCtrl.weaponPillObj = weaponPill;
            uiCtrl.txtWeaponIndicator = weaponTxt;

            // In-Game Emote Button (M3: Bottom right of BottomHudBoard with speech bubble icon)
            var btnEmote = CreateButton(hudPanel.transform, "BtnInGameEmote", spriteCircleBack, new Vector2(0.91f, 0.062f), new Vector2(0.91f, 0.062f), new Vector2(0.5f, 0.5f), new Vector2(84f, 84f));
            if (spriteSpeechBubble != null)
            {
                var emoteIcon = CreateImage(btnEmote.transform, "ChatIcon", spriteSpeechBubble, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(48f, 48f));
                emoteIcon.GetComponent<Image>().preserveAspect = true;
                emoteIcon.GetComponent<Image>().color = new Color(1.0f, 0.85f, 0.25f, 1.0f);
            }
            uiCtrl.btnInGameEmote = btnEmote.GetComponent<Button>();

            // Round Result Overlay (in Gameplay HUD during Resolve / RoundEnd)
            var roundResultObj = CreatePanel(hudPanel.transform, "RoundResultOverlay", new Vector2(0.5f, 0.60f), new Vector2(0.5f, 0.60f), new Vector2(0.5f, 0.5f), new Vector2(880f, 220f), spritePanelDarkGold, new Color(0.067f, 0.106f, 0.173f, 0.96f));
            roundResultObj.SetActive(false);
            uiCtrl.roundResultOverlay = roundResultObj;

            var rrBanner = CreateImage(roundResultObj.transform, "Header", spriteBannerWood, new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.5f), new Vector2(760f, 80f));
            var rrTitle = CreateTMP(rrBanner.transform, "Title", fontAsset, "KẾT QUẢ VÒNG", 32, colorGoldSub, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.txtRoundResultTitle = rrTitle;

            var rrDetail = CreateTMP(roundResultObj.transform, "Detail", fontAsset, "Tất cả người chơi đã ném dép!", 24, colorCream, TextAlignmentOptions.Center, new Vector2(0.5f, 0.38f), new Vector2(800f, 90f), wrap: true);
            uiCtrl.txtRoundResultDetail = rrDetail;

            // ==================== G. RESULT PANEL (PHASE N REDESIGN) ====================
            var resultPanel = CreatePanel(rootTr, "ResultPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.043f, 0.078f, 0.129f, 0.97f));
            resultPanel.SetActive(false);
            uiCtrl.resultPanel = resultPanel;

            var resultHeader = CreateImage(resultPanel.transform, "ResultBanner", spriteBannerWood, new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.5f), new Vector2(880f, 140f));
            var resTitle = CreateTMP(resultHeader.transform, "Title", fontAsset, "CHIẾN THẮNG!", 44, colorGoldSub, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.txtResultTitle = resTitle;

            var resSubtitle = CreateTMP(resultPanel.transform, "Subtitle", fontAsset, "Người thắng: Nam Làng", 26, colorCream, TextAlignmentOptions.Center, new Vector2(0.5f, 0.85f), new Vector2(960f, 54f), wrap: false);
            resSubtitle.enableAutoSizing = true;
            resSubtitle.fontSizeMin = 18f;
            resSubtitle.fontSizeMax = 28f;
            uiCtrl.txtResultSubtitle = resSubtitle;

            // Full Ranking Board (all 6 players — strictly non-overlapping columns!)
            var boardCard = CreatePanel(resultPanel.transform, "BoardCard", new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(940f, 700f), spritePanelDarkGold, Color.white);
            uiCtrl.resultRankingsContainer = boardCard.transform;
            uiCtrl.resultRows.Clear();

            for (int i = 0; i < 6; i++)
            {
                float yNorm = 0.88f - i * 0.155f;
                var rowObj = CreatePanel(boardCard.transform, "RankRow_" + i, new Vector2(0.5f, yNorm), new Vector2(0.5f, yNorm), new Vector2(0.5f, 0.5f), new Vector2(860f, 92f), i == 0 ? spriteRowMe : spriteRowLeaderboard, Color.white);
                var rRank = CreateTMP(rowObj.transform, "Rank", fontAsset, "#" + (i + 1), 28, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.06f, 0.5f), new Vector2(64f, 50f), wrap: false);
                var rBadge = CreateTMP(rowObj.transform, "RankBadge", fontAsset, "HẠNG " + (i + 1), 16, new Color(0.098f, 0.835f, 0.847f), TextAlignmentOptions.Center, new Vector2(0.14f, 0.5f), new Vector2(90f, 30f), wrap: false);
                rBadge.gameObject.SetActive(false);
                var rName = CreateTMP(rowObj.transform, "Name", fontAsset, defaultLobbyNames[i], 25, colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.12f, 0.5f), new Vector2(260f, 50f), wrap: false, pivot: new Vector2(0f, 0.5f));
                var rHp = CreateTMP(rowObj.transform, "Hp", fontAsset, "HP: " + Mathf.Max(0, 3 - i / 2) + "/3 • HIT: " + (3 - i / 2), 21, new Color(0.82f, 0.90f, 0.98f), TextAlignmentOptions.Center, new Vector2(0.56f, 0.5f), new Vector2(210f, 50f), wrap: false);
                var rStatus = CreateTMP(rowObj.transform, "Status", fontAsset, i == 0 ? "CHIẾN THẮNG" : "Đã loại", 22, i == 0 ? colorGoldSub : new Color(0.965f, 0.416f, 0.365f), TextAlignmentOptions.MidlineRight, new Vector2(0.96f, 0.5f), new Vector2(180f, 50f), wrap: false, pivot: new Vector2(1f, 0.5f));

                uiCtrl.resultRows.Add(new ProductionUIController.ResultRowUI
                {
                    root = rowObj,
                    txtRank = rRank,
                    txtRankBadge = rBadge,
                    txtName = rName,
                    txtHp = rHp,
                    txtStatus = rStatus,
                    imgBg = rowObj.GetComponent<Image>()
                });
            }

            var btnReplay = CreateButton(resultPanel.transform, "BtnReplay", spriteBtnPrimaryYellow, new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(880f, 130f));
            CreateTMP(btnReplay.transform, "Label", fontAsset, UiStrings.BtnReplay, 40, new Color(0.14f, 0.09f, 0.03f), TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnReplay = btnReplay.GetComponent<Button>();

            var btnHome = CreateButton(resultPanel.transform, "BtnHome", spriteBtnBlue3D, new Vector2(0.5f, 0.11f), new Vector2(0.5f, 0.11f), new Vector2(0.5f, 0.5f), new Vector2(880f, 110f));
            CreateTMP(btnHome.transform, "Label", fontAsset, "VỀ TRANG CHỦ", 34, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnBackToHome = btnHome.GetComponent<Button>();

            // ==================== H. LEAVE ROOM CONFIRMATION DIALOG ====================
            var leaveDialogObj = CreatePanel(rootTr, "DialogLeaveRoom", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.03f, 0.06f, 0.10f, 0.86f));
            leaveDialogObj.SetActive(false);
            uiCtrl.dialogLeaveRoom = leaveDialogObj;

            var leaveCard = CreatePanel(leaveDialogObj.transform, "LeaveCard", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(800f, 440f), spritePanelDarkGold, Color.white);

            var leaveBanner = CreateImage(leaveCard.transform, "LeaveBanner", spriteBannerWood, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.5f), new Vector2(640f, 100f));
            var leaveTitle = CreateTMP(leaveBanner.transform, "Title", fontAsset, "XÁC NHẬN RỜI PHÒNG", 32, colorGoldSub, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.txtLeaveDialogTitle = leaveTitle;

            var leaveMsg = CreateTMP(leaveCard.transform, "Message", fontAsset, "Bạn có chắc chắn muốn rời phòng không?\nMọi kết quả của trận đấu sẽ không được lưu.", 24, colorCream, TextAlignmentOptions.Center, new Vector2(0.5f, 0.55f), new Vector2(680f, 140f), wrap: true);
            uiCtrl.txtLeaveDialogMessage = leaveMsg;

            // Confirm Button (Red CTA)
            var btnConfirmLeave = CreateButton(leaveCard.transform, "BtnConfirmLeave", spriteBtnRed, new Vector2(0.28f, 0.22f), new Vector2(0.28f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(310f, 92f));
            CreateTMP(btnConfirmLeave.transform, "Label", fontAsset, "RỜI PHÒNG", 28, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnConfirmLeave = btnConfirmLeave.GetComponent<Button>();

            // Cancel Button
            var btnCancelLeave = CreateButton(leaveCard.transform, "BtnCancelLeave", spriteBtnYellow, new Vector2(0.72f, 0.22f), new Vector2(0.72f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(310f, 92f));
            CreateTMP(btnCancelLeave.transform, "Label", fontAsset, "Ở LẠI", 28, new Color(0.14f, 0.09f, 0.03f), TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnCancelLeave = btnCancelLeave.GetComponent<Button>();

            // Ensure Toast, Leave Dialog and Fingerprint are at the very top of canvas rendering hierarchy
            if (toastObj != null) toastObj.transform.SetAsLastSibling();
            if (leaveDialogObj != null) leaveDialogObj.transform.SetAsLastSibling();

            // ==================== J. JUICE & FEEDBACK CONTROLLER (Lô 4) ====================
            var themeAsset = AssetDatabase.LoadAssetAtPath<UiTheme>("Assets/Resources/UiTheme.asset");
            uiCtrl.theme = themeAsset;

            // Load Hit Impact VFX sprites
            var hitImpactFrames = new List<Sprite>();
            for (int fi = 1; fi <= 6; fi++)
            {
                var sp = LoadSprite("Assets/Art/VFX/Hit_Impact/01_Hit_Impact_F0" + fi + ".png");
                if (sp != null) hitImpactFrames.Add(sp);
            }

            var juiceCtrl = uiCtrl.gameObject.GetComponent<JuiceFeedbackController>();
            if (juiceCtrl == null) juiceCtrl = uiCtrl.gameObject.AddComponent<JuiceFeedbackController>();
            juiceCtrl.theme = themeAsset;
            juiceCtrl.timerPulseRing = uiCtrl.timerPulseRing;
            juiceCtrl.hitImpactFrames = hitImpactFrames.ToArray();
            juiceCtrl.poolWarmCount = 4;
            uiCtrl.juiceFeedback = juiceCtrl;
            EditorUtility.SetDirty(juiceCtrl);

            // ==================== K. SETTINGS DIALOG (PHASE F REDESIGN — COMPACT MODAL) ====================
            var dialogSettingsObj = CreatePanel(rootTr, "SettingsDialog", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.03f, 0.06f, 0.10f, 0.84f));
            dialogSettingsObj.SetActive(false);
            uiCtrl.dialogSettings = dialogSettingsObj;

            var settingsCard = CreatePanel(dialogSettingsObj.transform, "SettingsCard", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900f, 1020f), spritePanelDarkGold, Color.white);

            var settingsBanner = CreateImage(settingsCard.transform, "SettingsBanner", spriteBannerWood, new Vector2(0.5f, 0.93f), new Vector2(0.5f, 0.93f), new Vector2(0.5f, 0.5f), new Vector2(620f, 114f));
            var settingsTitleTxt = CreateTMP(settingsBanner.transform, "Title", fontAsset, UiStrings.SettingsTitle, 40, colorGoldSub, TextAlignmentOptions.Center, wrap: false);
            settingsTitleTxt.fontStyle = FontStyles.Bold;
            uiCtrl.txtSettingsTitle = settingsTitleTxt;

            // Top-right X Close Button (wired identically to HideSettingsDialog, using ASCII "X" so font glyph is 100% present)
            var btnCloseSettingsTopX = CreateButton(settingsCard.transform, "BtnCloseSettingsTopX", spriteBtnRed, new Vector2(0.91f, 0.93f), new Vector2(0.91f, 0.93f), new Vector2(0.5f, 0.5f), new Vector2(78f, 78f));
            var closeSettingsTopXLbl = CreateTMP(btnCloseSettingsTopX.transform, "Label", fontAsset, "X", 32, Color.white, TextAlignmentOptions.Center, wrap: false);
            closeSettingsTopXLbl.fontStyle = FontStyles.Bold;
            btnCloseSettingsTopX.GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                uiCtrl.HideSettingsDialog();
            });

            // Audio Section Header (No decorative "---" dashes!)
            CreateTMP(settingsCard.transform, "AudioHeader", fontAsset, UiStrings.SettingsAudioHeader, 26, new Color(0.098f, 0.835f, 0.847f), TextAlignmentOptions.MidlineLeft, new Vector2(0.07f, 0.82f), new Vector2(760f, 36f), wrap: false, pivot: new Vector2(0f, 0.5f));

            // Music Row (with clear spacing and % value indicator)
            CreateTMP(settingsCard.transform, "MusicLabel", fontAsset, UiStrings.SettingsMusic, 26, colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.07f, 0.74f), new Vector2(240f, 42f), wrap: false, pivot: new Vector2(0f, 0.5f));
            var musicSlider = CreateSlider(settingsCard.transform, "MusicSlider", spriteSliderTrack, spriteSliderKnob, new Vector2(0.55f, 0.74f), new Vector2(360f, 46f), 0.50f);
            uiCtrl.sliderMusicVolume = musicSlider;
            CreateTMP(settingsCard.transform, "MusicValue", fontAsset, "50%", 25, colorGoldSub, TextAlignmentOptions.MidlineRight, new Vector2(0.93f, 0.74f), new Vector2(96f, 40f), wrap: false, pivot: new Vector2(1f, 0.5f));

            // SFX Row
            CreateTMP(settingsCard.transform, "SfxLabel", fontAsset, UiStrings.SettingsSfx, 26, colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.07f, 0.64f), new Vector2(240f, 42f), wrap: false, pivot: new Vector2(0f, 0.5f));
            var sfxSlider = CreateSlider(settingsCard.transform, "SfxSlider", spriteSliderTrack, spriteSliderKnob, new Vector2(0.55f, 0.64f), new Vector2(360f, 46f), 0.80f);
            uiCtrl.sliderSfxVolume = sfxSlider;
            CreateTMP(settingsCard.transform, "SfxValue", fontAsset, "80%", 25, colorGoldSub, TextAlignmentOptions.MidlineRight, new Vector2(0.93f, 0.64f), new Vector2(96f, 40f), wrap: false, pivot: new Vector2(1f, 0.5f));

            // Controls & Display Section Header (No "---" dashes)
            CreateTMP(settingsCard.transform, "FeedbackHeader", fontAsset, "HIỂN THỊ & CHUYỂN ĐỘNG", 26, new Color(0.098f, 0.835f, 0.847f), TextAlignmentOptions.MidlineLeft, new Vector2(0.07f, 0.54f), new Vector2(760f, 36f), wrap: false, pivot: new Vector2(0f, 0.5f));

            // Reduced Motion Row
            CreateTMP(settingsCard.transform, "ReducedMotionLabel", fontAsset, UiStrings.SettingsReducedMotion, 26, colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.07f, 0.46f), new Vector2(520f, 42f), wrap: false, pivot: new Vector2(0f, 0.5f));
            var toggleMotion = CreateToggle(settingsCard.transform, "ToggleReducedMotion", spriteToggleOn, spriteToggleOff, new Vector2(0.84f, 0.46f), new Vector2(104f, 54f), false);
            uiCtrl.toggleReducedMotion = toggleMotion;

            // Language Section (No "---" dashes)
            CreateTMP(settingsCard.transform, "LangHeader", fontAsset, UiStrings.SettingsLanguage, 26, new Color(0.098f, 0.835f, 0.847f), TextAlignmentOptions.MidlineLeft, new Vector2(0.07f, 0.37f), new Vector2(760f, 36f), wrap: false, pivot: new Vector2(0f, 0.5f));

            var btnVi = CreateButton(settingsCard.transform, "BtnLangVi", spriteBtnYellow, new Vector2(0.28f, 0.28f), new Vector2(0.28f, 0.28f), new Vector2(0.5f, 0.5f), new Vector2(360f, 82f));
            CreateTMP(btnVi.transform, "Label", fontAsset, "TIẾNG VIỆT", 28, new Color(0.14f, 0.09f, 0.03f), TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnLanguageVi = btnVi.GetComponent<Button>();

            var btnEn = CreateButton(settingsCard.transform, "BtnLangEn", spriteBtnBlue, new Vector2(0.72f, 0.28f), new Vector2(0.72f, 0.28f), new Vector2(0.5f, 0.5f), new Vector2(360f, 82f));
            CreateTMP(btnEn.transform, "Label", fontAsset, "ENGLISH", 28, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnLanguageEn = btnEn.GetComponent<Button>();

            // Action Buttons (Report Bug + Close side by side, Leave Match below when in-game)
            var btnReportBug = CreateButton(settingsCard.transform, "BtnReportBug", spriteBtnYellow, new Vector2(0.28f, 0.15f), new Vector2(0.28f, 0.15f), new Vector2(0.5f, 0.5f), new Vector2(360f, 84f));
            CreateTMP(btnReportBug.transform, "Label", fontAsset, UiStrings.Get(UiStrings.SettingsBtnReportBug), 28, new Color(0.14f, 0.09f, 0.03f), TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnReportBug = btnReportBug.GetComponent<Button>();

            var btnCloseSettings = CreateButton(settingsCard.transform, "BtnCloseSettings", spriteBtnGreen3D, new Vector2(0.72f, 0.15f), new Vector2(0.72f, 0.15f), new Vector2(0.5f, 0.5f), new Vector2(360f, 84f));
            CreateTMP(btnCloseSettings.transform, "Label", fontAsset, UiStrings.SettingsBtnClose, 28, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnCloseSettings = btnCloseSettings.GetComponent<Button>();

            // Leave Match Button (only active during gameplay)
            var btnLeaveMatchSettings = CreateButton(settingsCard.transform, "BtnLeaveMatchSettings", spriteBtnRed, new Vector2(0.5f, 0.06f), new Vector2(0.5f, 0.06f), new Vector2(0.5f, 0.5f), new Vector2(460f, 70f));
            CreateTMP(btnLeaveMatchSettings.transform, "Label", fontAsset, UiStrings.SettingsBtnLeaveMatch, 26, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnLeaveMatchFromSettings = btnLeaveMatchSettings.GetComponent<Button>();

            // Build Version Text (subtle footer)
            var verObj = new GameObject("TxtSettingsBuildVersion", typeof(RectTransform));
            verObj.transform.SetParent(settingsCard.transform, false);
            var verRt = verObj.GetComponent<RectTransform>();
            verRt.anchorMin = new Vector2(0.05f, 0.008f);
            verRt.anchorMax = new Vector2(0.95f, 0.030f);
            verRt.sizeDelta = Vector2.zero;
            var verTxt = verObj.AddComponent<TextMeshProUGUI>();
            verTxt.font = fontAsset;
            verTxt.fontSize = 15f;
            verTxt.alignment = TextAlignmentOptions.Center;
            verTxt.color = new Color(0.68f, 0.75f, 0.85f, 0.72f);
            uiCtrl.txtSettingsBuildVersion = verTxt;

            if (dialogSettingsObj != null) dialogSettingsObj.transform.SetAsLastSibling();

            // ==================== L. TUTORIAL DIALOG (Lô 6) ====================
            var dialogTutObj = CreatePanel(rootTr, "TutorialDialog", Vector2.zero, Vector2.one, new Vector2(0.50f, 0.50f), Vector2.zero, null, new Color(0.02f, 0.03f, 0.05f, 0.88f));
            dialogTutObj.SetActive(false);
            uiCtrl.dialogTutorial = dialogTutObj;

            var tutCard = CreatePanel(dialogTutObj.transform, "TutorialCard", new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(940f, 720f), spritePanelDarkGold, Color.white);

            var tutBanner = CreateImage(tutCard.transform, "Banner", spriteBannerWood, new Vector2(0.50f, 0.90f), new Vector2(0.50f, 0.90f), new Vector2(0.50f, 0.50f), new Vector2(760f, 120f));
            var tutTitle = CreateTMP(tutBanner.transform, "Title", fontAsset, UiStrings.TutorialTitle, 36, colorGoldSub, TextAlignmentOptions.Center, wrap: false);
            tutTitle.fontStyle = FontStyles.Bold;
            uiCtrl.txtTutorialTitle = tutTitle;

            var tutStepHeader = CreateTMP(tutCard.transform, "StepHeader", fontAsset, UiStrings.TutorialStep1Title, 34, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.50f, 0.72f), new Vector2(860f, 50f));
            tutStepHeader.fontStyle = FontStyles.Bold;
            uiCtrl.txtTutorialStepHeader = tutStepHeader;

            var tutStepBody = CreateTMP(tutCard.transform, "StepBody", fontAsset, UiStrings.TutorialStep1Body, 28, colorCream, TextAlignmentOptions.Center, new Vector2(0.50f, 0.50f), new Vector2(820f, 180f), wrap: true);
            uiCtrl.txtTutorialStepBody = tutStepBody;

            // Step Indicator Dots
            var dot1 = CreatePanel(tutCard.transform, "Dot1", new Vector2(0.44f, 0.32f), new Vector2(0.44f, 0.32f), new Vector2(0.50f, 0.50f), new Vector2(26f, 26f), null, new Color(1f, 0.85f, 0.20f, 1f));
            var dot2 = CreatePanel(tutCard.transform, "Dot2", new Vector2(0.50f, 0.32f), new Vector2(0.50f, 0.32f), new Vector2(0.50f, 0.50f), new Vector2(26f, 26f), null, new Color(1f, 1f, 1f, 0.35f));
            var dot3 = CreatePanel(tutCard.transform, "Dot3", new Vector2(0.56f, 0.32f), new Vector2(0.56f, 0.32f), new Vector2(0.50f, 0.50f), new Vector2(26f, 26f), null, new Color(1f, 1f, 1f, 0.35f));
            uiCtrl.tutorialStepIndicators = new GameObject[] { dot1, dot2, dot3 };

            // Buttons: Skip & Next
            var btnTutSkip = CreateButton(tutCard.transform, "BtnSkip", spriteBtnRed, new Vector2(0.28f, 0.14f), new Vector2(0.28f, 0.14f), new Vector2(0.50f, 0.50f), new Vector2(340f, 85f));
            CreateTMP(btnTutSkip.transform, "Label", fontAsset, UiStrings.BtnTutorialSkip, 30, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnTutorialSkip = btnTutSkip.GetComponent<Button>();

            var btnTutNext = CreateButton(tutCard.transform, "BtnNext", spriteBtnYellow, new Vector2(0.72f, 0.14f), new Vector2(0.72f, 0.14f), new Vector2(0.50f, 0.50f), new Vector2(340f, 85f));
            var tutNextTxt = CreateTMP(btnTutNext.transform, "Label", fontAsset, UiStrings.BtnTutorialNext, 30, new Color(0.20f, 0.10f, 0f), TextAlignmentOptions.Center, wrap: false);
            tutNextTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnTutorialNext = btnTutNext.GetComponent<Button>();
            uiCtrl.txtTutorialNextLabel = tutNextTxt;

            if (dialogTutObj != null) dialogTutObj.transform.SetAsLastSibling();

            // ==================== M. PROFILE DIALOG (PHASE K REDESIGN — REAL AVATAR + STAT CARDS + RENAME INPUT) ====================
            var dialogProfileObj = CreatePanel(rootTr, "ProfileDialog", Vector2.zero, Vector2.one, new Vector2(0.50f, 0.50f), Vector2.zero, null, new Color(0.03f, 0.06f, 0.10f, 0.84f));
            dialogProfileObj.SetActive(false);
            uiCtrl.dialogProfile = dialogProfileObj;

            var profileCard = CreatePanel(dialogProfileObj.transform, "ProfileCard", new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(900f, 1020f), spritePanelDarkGold, Color.white);
            var profileBanner = CreateImage(profileCard.transform, "Banner", spriteBannerWood, new Vector2(0.50f, 0.93f), new Vector2(0.50f, 0.93f), new Vector2(0.50f, 0.50f), new Vector2(620f, 114f));
            var profTitle = CreateTMP(profileBanner.transform, "Title", fontAsset, UiStrings.ProfileTitle, 38, colorGoldSub, TextAlignmentOptions.Center, wrap: false);
            profTitle.fontStyle = FontStyles.Bold;
            uiCtrl.txtProfileTitle = profTitle;

            // Top-right X Close Button (ASCII "X")
            var btnCloseProfTopX = CreateButton(profileCard.transform, "BtnCloseProfileTopX", spriteBtnRed, new Vector2(0.91f, 0.93f), new Vector2(0.91f, 0.93f), new Vector2(0.5f, 0.5f), new Vector2(78f, 78f));
            var closeProfTopXLbl = CreateTMP(btnCloseProfTopX.transform, "Label", fontAsset, "X", 32, Color.white, TextAlignmentOptions.Center, wrap: false);
            closeProfTopXLbl.fontStyle = FontStyles.Bold;
            btnCloseProfTopX.GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.Instance?.PlayClick();
                uiCtrl.HideProfileDialog();
            });

            // Avatar Frame WITH Real Character Portrait inside (never an empty blue box!)
            var profAvatar = CreateImage(profileCard.transform, "Avatar", spriteCardSelected, new Vector2(0.22f, 0.74f), new Vector2(0.22f, 0.74f), new Vector2(0.50f, 0.50f), new Vector2(180f, 180f));
            if (spritePortraitNamLang != null)
            {
                var profPortrait = CreateImage(profAvatar.transform, "AvatarPortrait", spritePortraitNamLang, new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(152f, 152f));
                profPortrait.GetComponent<Image>().preserveAspect = true;
            }

            // Player Identity Block (Right of Avatar)
            var profName = CreateTMP(profileCard.transform, "PlayerName", fontAsset, "Nam Làng", 36, colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.36f, 0.80f), new Vector2(480f, 48f), wrap: false, pivot: new Vector2(0f, 0.5f));
            profName.fontStyle = FontStyles.Bold;
            uiCtrl.txtProfilePlayerName = profName;

            var profId = CreateTMP(profileCard.transform, "PlayerId", fontAsset, "ID: HM-9582 • Tân Binh Sân Đình", 23, new Color(0.72f, 0.78f, 0.88f), TextAlignmentOptions.MidlineLeft, new Vector2(0.36f, 0.73f), new Vector2(480f, 36f), wrap: false, pivot: new Vector2(0f, 0.5f));
            uiCtrl.txtProfilePlayerId = profId;

            // Currency Pill inside Identity Block
            var profGoldCard = CreatePanel(profileCard.transform, "GoldCard", new Vector2(0.62f, 0.65f), new Vector2(0.62f, 0.65f), new Vector2(0.50f, 0.50f), new Vector2(460f, 54f), spriteRowLeaderboard, Color.white);
            var profGold = CreateTMP(profGoldCard.transform, "Gold", fontAsset, "500 VÀNG • 20 NGỌC", 24, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.50f, 0.50f), new Vector2(420f, 40f));
            uiCtrl.txtProfileGold = profGold;

            // 2 Structured Stat Cards (Combat Record & Equipped Loadout)
            var statCardLeft = CreatePanel(profileCard.transform, "StatCardMatches", new Vector2(0.28f, 0.50f), new Vector2(0.28f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(380f, 124f), spriteRowLeaderboard, Color.white);
            CreateTMP(statCardLeft.transform, "Title", fontAsset, "THÀNH TÍCH ĐẤU", 20, new Color(0.098f, 0.835f, 0.847f), TextAlignmentOptions.Center, new Vector2(0.5f, 0.72f), new Vector2(340f, 30f));
            CreateTMP(statCardLeft.transform, "Value", fontAsset, "Sẵn sàng vào sân", 26, colorCream, TextAlignmentOptions.Center, new Vector2(0.5f, 0.34f), new Vector2(340f, 40f));

            var statCardRight = CreatePanel(profileCard.transform, "StatCardWeapon", new Vector2(0.72f, 0.50f), new Vector2(0.72f, 0.50f), new Vector2(0.5f, 0.5f), new Vector2(380f, 124f), spriteRowLeaderboard, Color.white);
            CreateTMP(statCardRight.transform, "Title", fontAsset, "VŨ KHÍ TRANG BỊ", 20, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.5f, 0.72f), new Vector2(340f, 30f));
            CreateTMP(statCardRight.transform, "Value", fontAsset, "Dép Tổ Ong", 26, colorCream, TextAlignmentOptions.Center, new Vector2(0.5f, 0.34f), new Vector2(340f, 40f));

            // Rename Cost & Rename Input Field (clearly separated at Y = 0.38f and Y = 0.28f)
            var profRenameCost = CreateTMP(profileCard.transform, "RenameCost", fontAsset, "ĐỔI TÊN NHÂN VẬT • " + UiStrings.FreeRenameLabel, 21, new Color(0.72f, 0.78f, 0.88f), TextAlignmentOptions.Center, new Vector2(0.50f, 0.37f), new Vector2(740f, 32f));
            uiCtrl.txtProfileRenameCost = profRenameCost;

            var profInputObj = CreatePanel(profileCard.transform, "InputNewName", new Vector2(0.50f, 0.28f), new Vector2(0.50f, 0.28f), new Vector2(0.50f, 0.50f), new Vector2(760f, 76f), spriteInputCyan, Color.white);
            var profInputField = profInputObj.AddComponent<TMP_InputField>();
            profInputField.characterLimit = 16;
            var profPlaceholder = CreateTMP(profInputObj.transform, "Placeholder", fontAsset, "Nhập tên hiển thị mới (3–16 ký tự)...", 24, new Color(0.65f, 0.75f, 0.88f, 0.65f), TextAlignmentOptions.MidlineLeft);
            FillRect(profPlaceholder.rectTransform, new Vector2(24f, 6f), new Vector2(-24f, -6f));
            var profInputText = CreateTMP(profInputObj.transform, "Text", fontAsset, "", 28, colorCream, TextAlignmentOptions.MidlineLeft);
            FillRect(profInputText.rectTransform, new Vector2(24f, 6f), new Vector2(-24f, -6f));
            profInputField.textComponent = profInputText;
            profInputField.placeholder = profPlaceholder;
            profInputField.text = "";
            uiCtrl.inputProfileNewName = profInputField;

            var btnRename = CreateButton(profileCard.transform, "BtnRename", spriteBtnYellow, new Vector2(0.28f, 0.13f), new Vector2(0.28f, 0.13f), new Vector2(0.50f, 0.50f), new Vector2(360f, 86f));
            CreateTMP(btnRename.transform, "Label", fontAsset, UiStrings.BtnRename, 28, new Color(0.14f, 0.09f, 0.03f), TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnProfileRename = btnRename.GetComponent<Button>();

            var btnCloseProf = CreateButton(profileCard.transform, "BtnClose", spriteBtnGreen3D, new Vector2(0.72f, 0.13f), new Vector2(0.72f, 0.13f), new Vector2(0.50f, 0.50f), new Vector2(360f, 86f));
            CreateTMP(btnCloseProf.transform, "Label", fontAsset, UiStrings.SettingsBtnClose, 28, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnCloseProfile = btnCloseProf.GetComponent<Button>();

            // ==================== N. SHOP DIALOG (Lô 7) ====================
            var dialogShopObj = CreatePanel(rootTr, "ShopDialog", Vector2.zero, Vector2.one, new Vector2(0.50f, 0.50f), Vector2.zero, null, new Color(0.02f, 0.03f, 0.05f, 0.88f));
            dialogShopObj.SetActive(false);
            uiCtrl.dialogShop = dialogShopObj;

            var shopCard = CreatePanel(dialogShopObj.transform, "ShopCard", new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(960f, 1320f), spritePanelDarkGold, Color.white);
            var shopBanner = CreateImage(shopCard.transform, "Banner", spriteBannerWood, new Vector2(0.50f, 0.93f), new Vector2(0.50f, 0.93f), new Vector2(0.50f, 0.50f), new Vector2(760f, 120f));
            var shopTitle = CreateTMP(shopBanner.transform, "Title", fontAsset, UiStrings.ShopTitle, 38, colorGoldSub, TextAlignmentOptions.Center, wrap: false);
            shopTitle.fontStyle = FontStyles.Bold;
            uiCtrl.txtShopTitle = shopTitle;

            var shopSub = CreateTMP(shopCard.transform, "Subtitle", fontAsset, UiStrings.ShopSubtitle, 22, colorCream, TextAlignmentOptions.Center, new Vector2(0.50f, 0.85f), new Vector2(860f, 40f));
            uiCtrl.txtShopSubtitle = shopSub;

            var shopGold = CreateTMP(shopCard.transform, "ShopGold", fontAsset, "500 VÀNG", 28, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.50f, 0.80f), new Vector2(400f, 35f));
            uiCtrl.txtShopGold = shopGold;

            string[] shopWeapons = { "Dép Tổ Ong (0G)", "Chảo Rán (100G)", "Cốc Bia (150G)", "Điếu Cày (225G)", "Búa Sắt (335G)", "Pickleball (500G)" };
            for (int i = 0; i < 6; i++)
            {
                float rowY = 0.72f - (i * 0.095f);
                var rowObj = CreatePanel(shopCard.transform, "ShopRow" + i, new Vector2(0.50f, rowY), new Vector2(0.50f, rowY), new Vector2(0.50f, 0.50f), new Vector2(860f, 80f), spriteRowLeaderboard, Color.white);
                CreateTMP(rowObj.transform, "ItemName", fontAsset, shopWeapons[i], 24, colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.35f, 0.50f), new Vector2(400f, 40f));
                var btnBuyRow = CreateButton(rowObj.transform, "BtnAction", i == 0 ? spriteBtnBlue : spriteBtnYellow, new Vector2(0.82f, 0.50f), new Vector2(0.82f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(200f, 60f));
                CreateTMP(btnBuyRow.transform, "Label", fontAsset, i == 0 ? UiStrings.BtnEquipped : UiStrings.BtnBuy, 24, i == 0 ? Color.white : new Color(0.20f, 0.10f, 0f), TextAlignmentOptions.Center, wrap: false);
            }

            var btnCloseShop = CreateButton(shopCard.transform, "BtnClose", spriteBtnGreen, new Vector2(0.50f, 0.08f), new Vector2(0.50f, 0.08f), new Vector2(0.50f, 0.50f), new Vector2(400f, 85f));
            CreateTMP(btnCloseShop.transform, "Label", fontAsset, UiStrings.SettingsBtnClose, 32, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnCloseShop = btnCloseShop.GetComponent<Button>();

            // ==================== O. INVENTORY DIALOG (Lô 7) ====================
            var dialogInvObj = CreatePanel(rootTr, "InventoryDialog", Vector2.zero, Vector2.one, new Vector2(0.50f, 0.50f), Vector2.zero, null, new Color(0.02f, 0.03f, 0.05f, 0.88f));
            dialogInvObj.SetActive(false);
            uiCtrl.dialogInventory = dialogInvObj;

            var invCard = CreatePanel(dialogInvObj.transform, "InvCard", new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(940f, 1000f), spritePanelDarkGold, Color.white);
            var invBanner = CreateImage(invCard.transform, "Banner", spriteBannerWood, new Vector2(0.50f, 0.92f), new Vector2(0.50f, 0.92f), new Vector2(0.50f, 0.50f), new Vector2(760f, 120f));
            var invTitle = CreateTMP(invBanner.transform, "Title", fontAsset, UiStrings.InventoryTitle, 38, colorGoldSub, TextAlignmentOptions.Center, wrap: false);
            invTitle.fontStyle = FontStyles.Bold;
            uiCtrl.txtInventoryTitle = invTitle;

            var invEq = CreateTMP(invCard.transform, "EquippedStatus", fontAsset, UiStrings.BtnEquipped + ": " + UiStrings.WeaponSandalName, 28, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.50f, 0.80f), new Vector2(700f, 40f));
            uiCtrl.txtInventoryEquippedWeapon = invEq;

            var defaultInvSlot = CreatePanel(invCard.transform, "Slot0", new Vector2(0.50f, 0.62f), new Vector2(0.50f, 0.62f), new Vector2(0.50f, 0.50f), new Vector2(800f, 120f), spriteCardSelected, Color.white);
            CreateTMP(defaultInvSlot.transform, "Name", fontAsset, "DÉP TỔ ONG (Mặc định)", 30, colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.38f, 0.50f), new Vector2(400f, 40f));
            var btnEqNow = CreateButton(defaultInvSlot.transform, "BtnEquipped", spriteBtnBlue, new Vector2(0.80f, 0.50f), new Vector2(0.80f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(220f, 70f));
            CreateTMP(btnEqNow.transform, "Label", fontAsset, UiStrings.BtnEquipped, 26, Color.white, TextAlignmentOptions.Center, wrap: false);

            var btnCloseInv = CreateButton(invCard.transform, "BtnClose", spriteBtnGreen, new Vector2(0.50f, 0.12f), new Vector2(0.50f, 0.12f), new Vector2(0.50f, 0.50f), new Vector2(400f, 85f));
            CreateTMP(btnCloseInv.transform, "Label", fontAsset, UiStrings.SettingsBtnClose, 32, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnCloseInventory = btnCloseInv.GetComponent<Button>();

            // ==================== P. LEADERBOARD DIALOG (Lô 7) ====================
            var dialogLbObj = CreatePanel(rootTr, "LeaderboardDialog", Vector2.zero, Vector2.one, new Vector2(0.50f, 0.50f), Vector2.zero, null, new Color(0.02f, 0.03f, 0.05f, 0.88f));
            dialogLbObj.SetActive(false);
            uiCtrl.dialogLeaderboard = dialogLbObj;

            var lbCard = CreatePanel(dialogLbObj.transform, "LeaderboardCard", new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(940f, 1100f), spritePanelDarkGold, Color.white);
            var lbBanner = CreateImage(lbCard.transform, "Banner", spriteBannerWood, new Vector2(0.50f, 0.92f), new Vector2(0.50f, 0.92f), new Vector2(0.50f, 0.50f), new Vector2(760f, 120f));
            var lbTitle = CreateTMP(lbBanner.transform, "Title", fontAsset, UiStrings.LeaderboardTitle, 36, colorGoldSub, TextAlignmentOptions.Center, wrap: false);
            lbTitle.fontStyle = FontStyles.Bold;
            uiCtrl.txtLeaderboardTitle = lbTitle;

            string[] topNames = { "1. Tuấn_HàNội • 1250đ", "2. Nam_Làng • 1120đ", "3. Hoa_Bé • 980đ", "4. Bạn (Khách_4826) • 850đ", "5. Bình_TổOng • 790đ" };
            for (int i = 0; i < 5; i++)
            {
                float rowY = 0.76f - (i * 0.11f);
                var row = CreatePanel(lbCard.transform, "Row" + i, new Vector2(0.50f, rowY), new Vector2(0.50f, rowY), new Vector2(0.50f, 0.50f), new Vector2(820f, 80f), i == 3 ? spriteRowMe : spriteRowLeaderboard, Color.white);
                CreateTMP(row.transform, "Name", fontAsset, topNames[i], 26, i == 3 ? colorGoldSub : colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.40f, 0.50f), new Vector2(500f, 40f));
            }

            var btnCloseLb = CreateButton(lbCard.transform, "BtnClose", spriteBtnGreen, new Vector2(0.50f, 0.12f), new Vector2(0.50f, 0.12f), new Vector2(0.50f, 0.50f), new Vector2(400f, 85f));
            CreateTMP(btnCloseLb.transform, "Label", fontAsset, UiStrings.SettingsBtnClose, 32, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnCloseLeaderboard = btnCloseLb.GetComponent<Button>();

            // ==================== Q. LOGIN DIALOG (Lô 8) ====================
            var dialogLoginObj = CreatePanel(rootTr, "LoginDialog", Vector2.zero, Vector2.one, new Vector2(0.50f, 0.50f), Vector2.zero, null, new Color(0.02f, 0.03f, 0.05f, 0.88f));
            dialogLoginObj.SetActive(false);
            uiCtrl.dialogLogin = dialogLoginObj;

            var loginCard = CreatePanel(dialogLoginObj.transform, "LoginCard", new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(0.50f, 0.50f), new Vector2(940f, 950f), spritePanelDarkGold, Color.white);
            var loginBanner = CreateImage(loginCard.transform, "Banner", spriteBannerWood, new Vector2(0.50f, 0.92f), new Vector2(0.50f, 0.92f), new Vector2(0.50f, 0.50f), new Vector2(760f, 120f));
            var loginTitle = CreateTMP(loginBanner.transform, "Title", fontAsset, UiStrings.LoginTitle, 38, colorGoldSub, TextAlignmentOptions.Center, wrap: false);
            loginTitle.fontStyle = FontStyles.Bold;
            uiCtrl.txtLoginTitle = loginTitle;

            var btnGuest = CreateButton(loginCard.transform, "BtnGuestLogin", spriteBtnYellow, new Vector2(0.50f, 0.72f), new Vector2(0.50f, 0.72f), new Vector2(0.50f, 0.50f), new Vector2(650f, 90f));
            CreateTMP(btnGuest.transform, "Label", fontAsset, UiStrings.LoginGuest, 30, new Color(0.20f, 0.10f, 0f), TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnSubmitGuestLogin = btnGuest.GetComponent<Button>();

            string[] socialLabels = { "Google", "Facebook", "Apple" };
            for (int i = 0; i < 3; i++)
            {
                float btnY = 0.56f - (i * 0.12f);
                var btnSoc = CreateButton(loginCard.transform, "Btn" + socialLabels[i], spriteBtnBlue, new Vector2(0.50f, btnY), new Vector2(0.50f, btnY), new Vector2(0.50f, 0.50f), new Vector2(650f, 85f));
                var socBtnComp = btnSoc.GetComponent<Button>();
                socBtnComp.interactable = false;
                CreateTMP(btnSoc.transform, "Label", fontAsset, socialLabels[i] + " (" + UiStrings.LoginComingSoon + ")", 26, new Color(0.85f, 0.85f, 0.90f, 0.65f), TextAlignmentOptions.Center, wrap: false);
            }

            var btnCloseLogin = CreateButton(loginCard.transform, "BtnClose", spriteBtnGreen, new Vector2(0.50f, 0.12f), new Vector2(0.50f, 0.12f), new Vector2(0.50f, 0.50f), new Vector2(400f, 85f));
            CreateTMP(btnCloseLogin.transform, "Label", fontAsset, UiStrings.SettingsBtnClose, 32, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnCloseLogin = btnCloseLogin.GetComponent<Button>();

            // Ensure Dialogs are on top
            if (dialogProfileObj != null) dialogProfileObj.transform.SetAsLastSibling();
            if (dialogShopObj != null) dialogShopObj.transform.SetAsLastSibling();
            if (dialogInvObj != null) dialogInvObj.transform.SetAsLastSibling();
            if (dialogLbObj != null) dialogLbObj.transform.SetAsLastSibling();
            if (dialogLoginObj != null) dialogLoginObj.transform.SetAsLastSibling();
            if (dialogTutObj != null) dialogTutObj.transform.SetAsLastSibling();

            // ==================== I. BUILD FINGERPRINT WATERMARK ====================
            var fpTxt = CreateTMP(rootTr, "BuildFingerprintWatermark", fontAsset,
                ProductionUIController.ResolveBuildFingerprint(),
                18, new Color(0.85f, 0.90f, 0.95f, 0.65f),
                TextAlignmentOptions.BottomRight,
                new Vector2(0.985f, 0.004f), new Vector2(960f, 32f), wrap: false, pivot: new Vector2(1f, 0f));
            fpTxt.transform.SetAsLastSibling();
            uiCtrl.txtBuildFingerprint = fpTxt;
            fpTxt.transform.SetAsLastSibling();
            uiCtrl.txtBuildFingerprint = fpTxt;

            // 7. Update MatchController references
            var matchCtrl = GameObject.Find("Match Controller")?.GetComponent<MatchController>();
            if (matchCtrl != null)
            {
                matchCtrl.configAsset = configAsset;
                matchCtrl.arena = arenaView;
                uiCtrl.matchController = matchCtrl;
                EditorUtility.SetDirty(matchCtrl);
            }

            // 7b. Setup WebBridge in Scene (B0)
            var webBridgeGo = GameObject.Find("WebBridge");
            if (webBridgeGo == null) webBridgeGo = new GameObject("WebBridge");
            var webBridge = webBridgeGo.GetComponent<HitMe.UI.WebBridge>();
            if (webBridge == null) webBridge = webBridgeGo.AddComponent<HitMe.UI.WebBridge>();
            webBridge.matchController = matchCtrl;
            webBridge.uiController = uiCtrl;
            EditorUtility.SetDirty(webBridgeGo);

            // 8. Configure Build Scenes (Graybox is sole production scene, MainMatch kept intact)
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(GrayboxScenePath, true)
            };
            EditorBuildSettings.scenes = scenes.ToArray();

            EditorSceneManager.MarkSceneDirty(scene);
            var bridgeGo = GameObject.Find("HitMeE2EBridge") ?? new GameObject("HitMeE2EBridge");
            if (bridgeGo.GetComponent<HitMeE2EBridge>() == null) bridgeGo.AddComponent<HitMeE2EBridge>();

            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("Successfully built Production UI for Graybox.unity!");
        }

        private static Sprite LoadSprite(string path)
        {
            var spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (spr != null) return spr;
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            if (allAssets != null)
            {
                for (int i = 0; i < allAssets.Length; i++)
                {
                    if (allAssets[i] is Sprite s) return s;
                }
            }
            return null;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Sprite sprite, Color color, bool raycastTarget = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            if (anchorMin == anchorMax)
            {
                rt.sizeDelta = size;
                rt.anchoredPosition = Vector2.zero;
            }
            else
            {
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color = color;
            img.raycastTarget = raycastTarget;
            return go;
        }

        private static GameObject CreateImage(Transform parent, string name, Sprite sprite, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, bool raycastTarget = false)
        {
            return CreatePanel(parent, name, anchorMin, anchorMax, pivot, size, sprite, Color.white, raycastTarget);
        }

        private static GameObject CreateButton(Transform parent, string name, Sprite sprite, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Color? explicitColor = null)
        {
            Color color = explicitColor ?? (sprite != null ? Color.white : Color.clear);
            var go = CreatePanel(parent, name, anchorMin, anchorMax, pivot, size, sprite, color, raycastTarget: true);
            var img = go.GetComponent<Image>();
            if (img != null)
            {
                img.color = color;
                if (sprite != null)
                {
                    img.sprite = sprite;
                    img.type = Image.Type.Sliced;
                }
            }
            go.AddComponent<Button>();
            return go;
        }

        private static Slider CreateSlider(Transform parent, string name, Sprite trackSprite, Sprite knobSprite, Vector2 anchor, Vector2 size, float defaultValue)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;

            var slider = go.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = defaultValue;

            // Background track
            var bgGo = CreatePanel(go.transform, "Background", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, trackSprite, new Color(0.2f, 0.25f, 0.35f, 0.9f), false);

            // Fill Area
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRt = fillArea.GetComponent<RectTransform>();
            FillRect(fillAreaRt, new Vector2(10f, 0f), new Vector2(-10f, 0f));

            // Fill bar uses solid color with null sprite so it has NO extra knob or end caps
            var fill = CreatePanel(fillArea.transform, "Fill", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.97f, 0.73f, 0.13f, 1f), false);
            slider.fillRect = fill.GetComponent<RectTransform>();

            // Handle Slide Area
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(go.transform, false);
            var handleAreaRt = handleArea.GetComponent<RectTransform>();
            FillRect(handleAreaRt, new Vector2(15f, 0f), new Vector2(-15f, 0f));

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var handleRt = handle.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(48f, 48f);
            var handleImg = handle.GetComponent<Image>();
            handleImg.sprite = knobSprite;
            handleImg.color = Color.white;
            handleImg.raycastTarget = true;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;

            return slider;
        }

        private static Toggle CreateToggle(Transform parent, string name, Sprite onSprite, Sprite offSprite, Vector2 anchor, Vector2 size, bool defaultValue)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;

            var toggle = go.GetComponent<Toggle>();
            toggle.isOn = defaultValue;

            var bg = CreatePanel(go.transform, "Background", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, offSprite ?? onSprite, Color.white);
            var checkmark = CreatePanel(bg.transform, "Checkmark", new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.85f), new Vector2(0.5f, 0.5f), Vector2.zero, onSprite, new Color(0.1f, 0.9f, 0.3f, 1f));
            toggle.graphic = checkmark.GetComponent<Image>();
            toggle.targetGraphic = bg.GetComponent<Image>();

            return toggle;
        }

        private static TextMeshProUGUI CreateTMP(Transform parent, string name, TMP_FontAsset font, string text, int fontSize, Color color, TextAlignmentOptions align, Vector2? anchor = null, Vector2? size = null, bool wrap = false, Vector2? pivot = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();

            if (anchor.HasValue)
            {
                rt.anchorMin = anchor.Value;
                rt.anchorMax = anchor.Value;
                rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
                rt.sizeDelta = size ?? new Vector2(400f, 80f);
                rt.anchoredPosition = Vector2.zero;
            }
            else
            {
                FillRect(rt, Vector2.zero, Vector2.zero);
            }

            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = color;
            tmp.alignment = align;
            tmp.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void FillRect(RectTransform rt, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        private static void AssignAudioClips(AudioManager audio)
        {
            audio.sfxClick = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_ui_click.wav");
            audio.sfxPlace = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_tap_place.wav");
            audio.sfxReady = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_ready.wav");
            audio.sfxReveal = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_reveal.wav");
            audio.sfxThrow = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_throw.wav");
            audio.sfxHit = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_hit_bop.wav");
            audio.sfxDeath = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_eliminate.wav");
            audio.sfxCountdownTick = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_countdown_tick.wav");
            audio.sfxCountdownLast = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_countdown_last.wav");
            audio.sfxWin = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_win.wav");
            audio.sfxLose = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_lose.wav");
            audio.bgmMenu = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/bgm_menu_loop.wav");
            audio.bgmMatch = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/bgm_match_loop.wav");
        }

        private static void AssignAudioServiceClips(AudioService audio)
        {
            audio.sfxClick = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_ui_click.wav");
            audio.sfxPlace = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_tap_place.wav");
            audio.sfxReady = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_ready.wav");
            audio.sfxReveal = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_reveal.wav");
            audio.sfxThrow = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_throw.wav");
            audio.sfxHit = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_hit_bop.wav");
            audio.sfxDamage = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_heart_break.wav");
            audio.sfxCountdownTick = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_countdown_tick.wav");
            audio.sfxCountdownLast = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_countdown_last.wav");
            audio.sfxWin = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_win.wav");
            audio.sfxLose = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_lose.wav");
            audio.bgmMenu = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/bgm_menu_loop.wav");
            audio.bgmMatch = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/bgm_match_loop.wav");
            audio.sfxFlightMissing = null;
        }

        private static void PopulateConfigVisualAssets(GameConfigAsset configAsset, TMP_FontAsset fontAsset)
        {
            configAsset.shadowSprite = LoadSprite("Assets/Sprites/procedural_shadow.png");
            configAsset.footRingSprite = LoadSprite("Assets/Sprites/procedural_foot_ring.png");
            configAsset.vietnameseFont = fontAsset;
            configAsset.characterIdleSprites = new Sprite[]
            {
                LoadSprite("Assets/Sprites/cau_be_lang_idle.png"),
                LoadSprite("Assets/Sprites/mai_hong_idle.png"),
                LoadSprite("Assets/Sprites/anh_non_la_idle.png"),
                LoadSprite("Assets/Sprites/anh_nui_idle.png"),
                LoadSprite("Assets/Sprites/co_nang_the_thao_idle.png"),
                LoadSprite("Assets/Sprites/chu_cho_vang_idle.png"),
                LoadSprite("Assets/Sprites/meo_mun_idle.png")
            };
            configAsset.weaponScale = 0.28f;
            configAsset.namLangPoseSprites = new Sprite[]
            {
                LoadSprite("Assets/Sprites/cau_be_lang_idle.png"),
                LoadSprite("Assets/Sprites/cau_be_lang_throw_windup.png") ?? LoadSprite("Assets/Sprites/cau_be_lang_idle.png"),
                LoadSprite("Assets/Sprites/cau_be_lang_throw_release.png") ?? LoadSprite("Assets/Sprites/cau_be_lang_idle.png"),
                LoadSprite("Assets/Sprites/cau_be_lang_hit.png"),
                LoadSprite("Assets/Sprites/cau_be_lang_defeat.png")
            };
            configAsset.anhNonLaPoseSprites = new Sprite[]
            {
                LoadSprite("Assets/Sprites/anh_non_la_idle.png"),
                LoadSprite("Assets/Sprites/anh_non_la_throw_windup.png") ?? LoadSprite("Assets/Sprites/anh_non_la_idle.png"),
                LoadSprite("Assets/Sprites/anh_non_la_throw_release.png") ?? LoadSprite("Assets/Sprites/anh_non_la_idle.png"),
                LoadSprite("Assets/Sprites/anh_non_la_hit.png"),
                LoadSprite("Assets/Sprites/anh_non_la_defeat.png")
            };
            configAsset.sandalProjectileSprite = LoadSprite("Assets/Sprites/dep_projectile.png") ?? LoadSprite("Assets/Sprites/dep_icon.png");
            configAsset.anhNuiThrowReleaseSprite = LoadSprite("Assets/Sprites/anh_nui_throw_release.png");
            configAsset.nuNangDongThrowReleaseSprite = LoadSprite("Assets/Sprites/co_nang_the_thao_throw_release.png");
            configAsset.heartFullSprite = LoadSprite("Assets/Art/UI/Kit/ui_icon_heart_full.png") ?? LoadSprite(KitPath + "ui_icon_heart_full.png");
            configAsset.hitFxFrames = new Sprite[]
            {
                LoadSprite("Assets/Art/VFX/Hit_Impact/01_Hit_Impact_F01.png"),
                LoadSprite("Assets/Art/VFX/Hit_Impact/01_Hit_Impact_F02.png"),
                LoadSprite("Assets/Art/VFX/Hit_Impact/01_Hit_Impact_F03.png"),
                LoadSprite("Assets/Art/VFX/Hit_Impact/01_Hit_Impact_F04.png"),
                LoadSprite("Assets/Art/VFX/Hit_Impact/01_Hit_Impact_F05.png"),
                LoadSprite("Assets/Art/VFX/Hit_Impact/01_Hit_Impact_F06.png")
            };
            EditorUtility.SetDirty(configAsset);
        }
    }
}
