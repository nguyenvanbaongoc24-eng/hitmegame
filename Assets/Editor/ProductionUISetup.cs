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
            cam.orthographicSize = 7.8f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.09f, 1f);
            camGo.transform.position = new Vector3(0f, 0.80f, -10f);

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
                bgObj.transform.position = Vector3.zero;
                bgObj.transform.localScale = new Vector3(1.05f, 1.05f, 1f);
                var sr = bgObj.GetComponent<SpriteRenderer>();
                if (sr == null) sr = bgObj.AddComponent<SpriteRenderer>();
                sr.sprite = spriteArena;
                sr.sortingOrder = -2;
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
            var toastObj = CreatePanel(rootTr, "ToastContainer", new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.5f), new Vector2(800f, 100f), spritePanelDarkGold, new Color(0.1f, 0.12f, 0.16f, 0.95f));
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
                var bgImg = bgObj.GetComponent<Image>();
                bgImg.preserveAspect = false;
                bgImg.color = Color.white;
            }

            // Dark vignette overlay on bottom area for crisp button legibility
            var vignetteBottom = CreatePanel(homePanel.transform, "BottomVignette", new Vector2(0f, 0f), new Vector2(1f, 0.48f), new Vector2(0.5f, 0f), Vector2.zero, null, new Color(0.04f, 0.05f, 0.08f, 0.72f));

            // Home Header & Logo
            var homeHeader = CreateImage(homePanel.transform, "HomeBanner", spriteBannerWood, new Vector2(0.5f, 0.93f), new Vector2(0.5f, 0.93f), new Vector2(0.5f, 0.5f), new Vector2(860f, 150f));
            var logoTxt = CreateTMP(homeHeader.transform, "LogoTxt", fontAsset, UiStrings.GameTitle, 76, colorGoldSub, TextAlignmentOptions.Center);
            logoTxt.fontStyle = FontStyles.Bold;
            FillRect(logoTxt.rectTransform, new Vector2(20f, 10f), new Vector2(-20f, -10f));
            uiCtrl.txtHomeLogo = logoTxt;

            var subtitleTxt = CreateTMP(homePanel.transform, "HomeSubtitle", fontAsset, UiStrings.HomeSubtitle, 28, colorCream, TextAlignmentOptions.Center, new Vector2(0.5f, 0.865f), new Vector2(800f, 40f));
            subtitleTxt.fontStyle = FontStyles.Bold;

            // Selected Character & Loadout Display Box (middle 0.50f - 0.78f)
            var loadoutCard = CreatePanel(homePanel.transform, "LoadoutCard", new Vector2(0.5f, 0.63f), new Vector2(0.5f, 0.63f), new Vector2(0.5f, 0.5f), new Vector2(940f, 320f), spritePanelDarkGold, Color.white);
            
            // Character Portrait inside Card Frame
            var portraitFrame = CreateImage(loadoutCard.transform, "PortraitFrame", spriteCardSelected, new Vector2(0.18f, 0.5f), new Vector2(0.18f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(200f, 260f));
            var portraitImgObj = CreateImage(portraitFrame.transform, "PortraitSprite", configAsset?.characterIdleSprites?[0] ?? LoadSprite(SpritesPath + "cau_be_lang_idle.png"), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(170f, 220f));
            var portraitImg = portraitImgObj.GetComponent<Image>();
            portraitImg.preserveAspect = true;
            uiCtrl.imgHomeCharPortrait = portraitImg;

            // Character Info (Right Side)
            var charHeaderTxt = CreateTMP(loadoutCard.transform, "CharHeader", fontAsset, "NHÂN VẬT ĐANG CHỌN", 24, colorGoldSub, TextAlignmentOptions.Left, new Vector2(0.64f, 0.80f), new Vector2(460f, 35f));
            var charNameTxt = CreateTMP(loadoutCard.transform, "CharName", fontAsset, UiStrings.Char1Name, 40, colorCream, TextAlignmentOptions.Left, new Vector2(0.64f, 0.60f), new Vector2(460f, 50f));
            charNameTxt.fontStyle = FontStyles.Bold;
            uiCtrl.txtHomeCharName = charNameTxt;

            var charDescTxt = CreateTMP(loadoutCard.transform, "CharDesc", fontAsset, UiStrings.Char1Desc, 22, colorCream, TextAlignmentOptions.Left, new Vector2(0.64f, 0.40f), new Vector2(460f, 50f), wrap: true);
            uiCtrl.txtHomeCharDesc = charDescTxt;

            var weaponPillHome = CreateImage(loadoutCard.transform, "WeaponPillHome", spriteBannerWood, new Vector2(0.64f, 0.18f), new Vector2(0.64f, 0.18f), new Vector2(0.5f, 0.5f), new Vector2(460f, 56f));
            var weaponIconHome = CreateImage(weaponPillHome.transform, "Icon", spriteWeaponDep, new Vector2(0.10f, 0.5f), new Vector2(0.10f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(42f, 42f));
            var weaponTxtHome = CreateTMP(weaponPillHome.transform, "Text", fontAsset, UiStrings.WeaponSandalName, 22, colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.58f, 0.5f), new Vector2(300f, 40f));
            weaponTxtHome.fontStyle = FontStyles.Bold;

            // 4 Action Buttons strictly in priority order (lower 1/3 of screen, touch area >= 135px)
            // 1. LUYỆN VỚI BOT (CTA Vàng chính)
            var btnBot = CreateButton(homePanel.transform, "BtnBotTraining", spriteBtnYellow, new Vector2(0.5f, 0.36f), new Vector2(0.5f, 0.36f), new Vector2(0.5f, 0.5f), new Vector2(880f, 130f));
            var btnBotTxt = CreateTMP(btnBot.transform, "Label", fontAsset, UiStrings.BtnPracticeBots, 44, new Color(0.22f, 0.11f, 0.02f), TextAlignmentOptions.Center, wrap: false);
            btnBotTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnBotTraining = btnBot.GetComponent<Button>();

            // 2. CHƠI NGAY / GHÉP NHANH (CTA Xanh dương)
            var btnQuick = CreateButton(homePanel.transform, "BtnQuickMatch", spriteBtnBlue, new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(880f, 120f));
            var btnQuickTxt = CreateTMP(btnQuick.transform, "Label", fontAsset, UiStrings.BtnPlayNow, 38, Color.white, TextAlignmentOptions.Center, wrap: false);
            btnQuickTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnQuickMatch = btnQuick.GetComponent<Button>();

            // 3. TẠO PHÒNG (CTA Xanh lá)
            var btnCreate = CreateButton(homePanel.transform, "BtnCreateRoom", spriteBtnGreen, new Vector2(0.5f, 0.155f), new Vector2(0.5f, 0.155f), new Vector2(0.5f, 0.5f), new Vector2(880f, 110f));
            var btnCreateTxt = CreateTMP(btnCreate.transform, "Label", fontAsset, UiStrings.BtnCreateRoom, 36, Color.white, TextAlignmentOptions.Center, wrap: false);
            btnCreateTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnCreateRoom = btnCreate.GetComponent<Button>();

            // 4. VÀO PHÒNG (CTA Xanh dương / kit)
            var btnJoin = CreateButton(homePanel.transform, "BtnJoinRoom", spriteBtnBlue, new Vector2(0.5f, 0.065f), new Vector2(0.5f, 0.065f), new Vector2(0.5f, 0.5f), new Vector2(880f, 105f));
            var btnJoinTxt = CreateTMP(btnJoin.transform, "Label", fontAsset, UiStrings.BtnJoinRoom, 36, Color.white, TextAlignmentOptions.Center, wrap: false);
            btnJoinTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnJoinRoom = btnJoin.GetComponent<Button>();

            // ==================== C. CREATE ROOM PANEL ====================
            var createRoomPanel = CreatePanel(rootTr, "CreateRoomPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.04f, 0.05f, 0.07f, 0.95f));
            createRoomPanel.SetActive(false);
            uiCtrl.createRoomPanel = createRoomPanel;

            var btnBackCreate = CreateButton(createRoomPanel.transform, "BtnBack", spriteCircleBack, new Vector2(0.12f, 0.94f), new Vector2(0.12f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(100f, 100f));
            uiCtrl.btnBackFromCreate = btnBackCreate.GetComponent<Button>();

            var createHeader = CreateImage(createRoomPanel.transform, "CreateBanner", spriteBannerWood, new Vector2(0.54f, 0.94f), new Vector2(0.54f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(650f, 140f));
            CreateTMP(createHeader.transform, "Title", fontAsset, "TẠO PHÒNG", 48, new Color(1f, 0.92f, 0.35f), TextAlignmentOptions.Center);

            var createCard = CreatePanel(createRoomPanel.transform, "CreateCard", new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.5f), new Vector2(940f, 750f), spritePanelDarkGold, Color.white);
            CreateTMP(createCard.transform, "L1", fontAsset, "TÊN PHÒNG:", 36, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Left, new Vector2(0.08f, 0.86f), new Vector2(300f, 50f));

            var nameInputObj = CreatePanel(createCard.transform, "InputNameBox", new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.5f), new Vector2(800f, 90f), spriteInputCyan, Color.white);
            var nameInputField = nameInputObj.AddComponent<TMP_InputField>();
            var nameInputText = CreateTMP(nameInputObj.transform, "Text", fontAsset, "Phòng Chiến Hữu", 36, Color.white, TextAlignmentOptions.Left);
            FillRect(nameInputText.rectTransform, new Vector2(20f, 5f), new Vector2(-20f, -5f));
            nameInputField.textComponent = nameInputText;
            nameInputField.text = "Phòng Chiến Hữu";
            uiCtrl.inputRoomName = nameInputField;

            CreateTMP(createCard.transform, "L2", fontAsset, "SỐ NGƯỜI CHƠI: 6 người (Tối đa)", 36, Color.white, TextAlignmentOptions.Left, new Vector2(0.08f, 0.58f), new Vector2(700f, 50f));
            CreateTMP(createCard.transform, "L3", fontAsset, "CHẾ ĐỘ: Công khai", 36, Color.white, TextAlignmentOptions.Left, new Vector2(0.08f, 0.44f), new Vector2(700f, 50f));
            CreateTMP(createCard.transform, "L4", fontAsset, "BẢN ĐỒ: Sân Làng Đình Cổ (9.0 x 11.4)", 36, Color.white, TextAlignmentOptions.Left, new Vector2(0.08f, 0.30f), new Vector2(700f, 50f));
            CreateTMP(createCard.transform, "L5", fontAsset, "THỜI GIAN NGẮM: 5.0 giây (Chuẩn)", 36, Color.white, TextAlignmentOptions.Left, new Vector2(0.08f, 0.16f), new Vector2(700f, 50f));

            var btnSubmitCreate = CreateButton(createRoomPanel.transform, "BtnSubmitCreate", spriteBtnBlue, new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(900f, 150f));
            CreateTMP(btnSubmitCreate.transform, "Label", fontAsset, "TẠO PHÒNG NGAY", 48, Color.white, TextAlignmentOptions.Center);
            uiCtrl.btnSubmitCreateRoom = btnSubmitCreate.GetComponent<Button>();

            // ==================== D. JOIN ROOM PANEL ====================
            var joinRoomPanel = CreatePanel(rootTr, "JoinRoomPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.04f, 0.05f, 0.07f, 0.95f));
            joinRoomPanel.SetActive(false);
            uiCtrl.joinRoomPanel = joinRoomPanel;

            var btnBackJoin = CreateButton(joinRoomPanel.transform, "BtnBack", spriteCircleBack, new Vector2(0.12f, 0.94f), new Vector2(0.12f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(100f, 100f));
            uiCtrl.btnBackFromJoin = btnBackJoin.GetComponent<Button>();

            var joinHeader = CreateImage(joinRoomPanel.transform, "JoinBanner", spriteBannerWood, new Vector2(0.54f, 0.94f), new Vector2(0.54f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(650f, 140f));
            CreateTMP(joinHeader.transform, "Title", fontAsset, "VÀO PHÒNG", 48, new Color(1f, 0.92f, 0.35f), TextAlignmentOptions.Center);

            var joinCard = CreatePanel(joinRoomPanel.transform, "JoinCard", new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), new Vector2(940f, 500f), spritePanelDarkGold, Color.white);
            CreateTMP(joinCard.transform, "L1", fontAsset, "NHẬP MÃ PHÒNG (6 KÝ TỰ):", 36, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Center, new Vector2(0.5f, 0.85f), new Vector2(700f, 50f));

            var codeInputObj = CreatePanel(joinCard.transform, "InputCodeBox", new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), new Vector2(800f, 100f), spriteInputCyan, Color.white);
            var codeInputField = codeInputObj.AddComponent<TMP_InputField>();
            var codeInputText = CreateTMP(codeInputObj.transform, "Text", fontAsset, "HM4826", 48, Color.yellow, TextAlignmentOptions.Center);
            codeInputText.fontStyle = FontStyles.Bold;
            FillRect(codeInputText.rectTransform, new Vector2(20f, 5f), new Vector2(-20f, -5f));
            codeInputField.textComponent = codeInputText;
            codeInputField.text = "HM4826";
            uiCtrl.inputRoomCode = codeInputField;

            var btnPaste = CreateButton(joinCard.transform, "BtnPaste", spriteBtnYellow, new Vector2(0.32f, 0.38f), new Vector2(0.32f, 0.38f), new Vector2(0.5f, 0.5f), new Vector2(340f, 90f));
            CreateTMP(btnPaste.transform, "Label", fontAsset, "DÁN MÃ", 34, new Color(0.2f, 0.1f, 0f), TextAlignmentOptions.Center);
            uiCtrl.btnPasteCode = btnPaste.GetComponent<Button>();

            var btnScan = CreateButton(joinCard.transform, "BtnScan", spriteBtnBlue, new Vector2(0.68f, 0.38f), new Vector2(0.68f, 0.38f), new Vector2(0.5f, 0.5f), new Vector2(340f, 90f));
            CreateTMP(btnScan.transform, "Label", fontAsset, "QUÉT MÃ QR", 34, Color.white, TextAlignmentOptions.Center);
            uiCtrl.btnScanQr = btnScan.GetComponent<Button>();

            var foundCard = CreatePanel(joinRoomPanel.transform, "FoundCard", new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.5f), new Vector2(940f, 200f), spriteCardSlot, Color.white);
            var foundTxt = CreateTMP(foundCard.transform, "FoundText", fontAsset, "✅ Đã tìm thấy: Phòng Chiến Hữu\nChủ phòng: Nam Làng • 4/6 người • Sân Làng", 34, new Color(0.2f, 1f, 0.4f), TextAlignmentOptions.Center);
            FillRect(foundTxt.rectTransform, new Vector2(20f, 10f), new Vector2(-20f, -10f));
            uiCtrl.roomFoundCard = foundCard;
            uiCtrl.txtFoundRoomInfo = foundTxt;

            var btnSubmitJoin = CreateButton(joinRoomPanel.transform, "BtnSubmitJoin", spriteBtnGreen, new Vector2(0.5f, 0.20f), new Vector2(0.5f, 0.20f), new Vector2(0.5f, 0.5f), new Vector2(900f, 150f));
            CreateTMP(btnSubmitJoin.transform, "Label", fontAsset, "VÀO PHÒNG", 48, Color.white, TextAlignmentOptions.Center);
            uiCtrl.btnSubmitJoinRoom = btnSubmitJoin.GetComponent<Button>();

            // ==================== E. BOT TRAINING PANEL ====================
            var botPanel = CreatePanel(rootTr, "BotTrainingPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.04f, 0.06f, 0.10f, 0.98f));
            botPanel.SetActive(false);
            uiCtrl.botTrainingPanel = botPanel;

            // Back button
            var btnBackBot = CreateButton(botPanel.transform, "BtnBack", spriteCircleBack, new Vector2(0.12f, 0.94f), new Vector2(0.12f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(96f, 96f));
            uiCtrl.btnBackFromBotTraining = btnBackBot.GetComponent<Button>();

            // Header Banner
            var botHeader = CreateImage(botPanel.transform, "BotBanner", spriteBannerWood, new Vector2(0.54f, 0.94f), new Vector2(0.54f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(680f, 140f));
            var botTitleTxt = CreateTMP(botHeader.transform, "Title", fontAsset, UiStrings.BotTrainingTitle, 40, colorGoldSub, TextAlignmentOptions.Center);
            botTitleTxt.fontStyle = FontStyles.Bold;
            uiCtrl.txtBotTrainingHeader = botTitleTxt;

            // Subtitle
            CreateTMP(botPanel.transform, "Subtitle", fontAsset, UiStrings.BotTrainingSubtitle, 22, colorCream, TextAlignmentOptions.Center, new Vector2(0.5f, 0.865f), new Vector2(940f, 50f));

            // Section: Choose Character (7 Characters)
            CreateTMP(botPanel.transform, "CharSelectHeader", fontAsset, "CHỌN NHÂN VẬT (7 NHÂN VẬT LÀNG)", 26, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.5f, 0.815f), new Vector2(900f, 40f));

            // 7 Character Cards Container
            var charGrid = CreatePanel(botPanel.transform, "CharGrid", new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.5f), new Vector2(960f, 260f), spritePanelDarkGold, Color.white);
            uiCtrl.botCharCardFrames.Clear();
            uiCtrl.botCharCardButtons.Clear();

            // Row 1: 4 cards, Row 2: 3 cards (centered)
            Sprite[] idleSprites = configAsset?.characterIdleSprites;
            float[] r1X = { 0.14f, 0.38f, 0.62f, 0.86f };
            float[] r2X = { 0.26f, 0.50f, 0.74f };

            for (int c = 0; c < 7; c++)
            {
                float nx = c < 4 ? r1X[c] : r2X[c - 4];
                float ny = c < 4 ? 0.73f : 0.27f;

                var cardObj = CreatePanel(charGrid.transform, "CharCard_" + c, new Vector2(nx, ny), new Vector2(nx, ny), new Vector2(0.5f, 0.5f), new Vector2(210f, 115f), c == 0 ? spriteCardSelected : spriteCardChar, Color.white);
                var cardImg = cardObj.GetComponent<Image>();
                var cardBtn = cardObj.AddComponent<Button>();

                // Character Sprite inside
                Sprite spr = (idleSprites != null && c < idleSprites.Length) ? idleSprites[c] : null;
                var charSprObj = CreateImage(cardObj.transform, "CharSpr", spr, new Vector2(0.24f, 0.5f), new Vector2(0.24f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(70f, 95f));
                charSprObj.GetComponent<Image>().preserveAspect = true;

                // Character Name inside card
                string cShortName = c < PlayerView.CharacterNames.Length ? PlayerView.CharacterNames[c] : $"NV {c + 1}";
                var cLabel = CreateTMP(cardObj.transform, "Name", fontAsset, cShortName, 20, colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.66f, 0.5f), new Vector2(110f, 80f), wrap: true);
                cLabel.fontStyle = FontStyles.Bold;

                uiCtrl.botCharCardFrames.Add(cardImg);
                uiCtrl.botCharCardButtons.Add(cardBtn);
            }

            // Character Detail Box
            var charDetailCard = CreatePanel(botPanel.transform, "CharDetailCard", new Vector2(0.5f, 0.495f), new Vector2(0.5f, 0.495f), new Vector2(0.5f, 0.5f), new Vector2(940f, 140f), spritePanelDarkGold, Color.white);
            var detailName = CreateTMP(charDetailCard.transform, "DetailName", fontAsset, UiStrings.Char1Name, 30, colorGoldSub, TextAlignmentOptions.Left, new Vector2(0.5f, 0.70f), new Vector2(880f, 40f));
            detailName.fontStyle = FontStyles.Bold;
            uiCtrl.txtSelectedCharacter = detailName;

            var detailDesc = CreateTMP(charDetailCard.transform, "DetailDesc", fontAsset, UiStrings.Char1Desc + " • Vũ khí: Dép Tổ Ong (Bán kính 0.52m)", 24, colorCream, TextAlignmentOptions.Left, new Vector2(0.5f, 0.32f), new Vector2(880f, 45f), wrap: true);
            uiCtrl.txtSelectedCharDetail = detailDesc;

            // Bot Count Selector Card
            var botCountCard = CreatePanel(botPanel.transform, "BotCountCard", new Vector2(0.5f, 0.33f), new Vector2(0.5f, 0.33f), new Vector2(0.5f, 0.5f), new Vector2(940f, 170f), spritePanelDarkGold, Color.white);
            CreateTMP(botCountCard.transform, "Title", fontAsset, "SỐ LƯỢNG BOT THAM GIA", 24, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.5f, 0.80f), new Vector2(800f, 35f));

            var btnDec = CreateButton(botCountCard.transform, "BtnDec", spriteCircleBack, new Vector2(0.18f, 0.38f), new Vector2(0.18f, 0.38f), new Vector2(0.5f, 0.5f), new Vector2(80f, 80f));
            CreateTMP(btnDec.transform, "Label", fontAsset, "−", 48, Color.white, TextAlignmentOptions.Center);
            uiCtrl.btnDecreaseBots = btnDec.GetComponent<Button>();

            var countDisplayTxt = CreateTMP(botCountCard.transform, "CountText", fontAsset, "4 người (1 Bạn + 3 Bot)", 36, colorCream, TextAlignmentOptions.Center, new Vector2(0.5f, 0.38f), new Vector2(480f, 60f));
            countDisplayTxt.fontStyle = FontStyles.Bold;
            uiCtrl.txtSelectedBotCount = countDisplayTxt;

            var btnInc = CreateButton(botCountCard.transform, "BtnInc", spriteCircleBack, new Vector2(0.82f, 0.38f), new Vector2(0.82f, 0.38f), new Vector2(0.5f, 0.5f), new Vector2(80f, 80f));
            CreateTMP(btnInc.transform, "Label", fontAsset, "+", 48, Color.white, TextAlignmentOptions.Center);
            uiCtrl.btnIncreaseBots = btnInc.GetComponent<Button>();

            // Start Bot Training CTA Yellow Button
            var btnStartBot = CreateButton(botPanel.transform, "BtnStartBot", spriteBtnYellow, new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.5f), new Vector2(880f, 130f));
            var startBotTxt = CreateTMP(btnStartBot.transform, "Label", fontAsset, UiStrings.BtnStart + " (VÀO SÂN)", 44, new Color(0.22f, 0.11f, 0.02f), TextAlignmentOptions.Center);
            startBotTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnStartBotTraining = btnStartBot.GetComponent<Button>();

            // ==================== F. LOBBY PANEL ====================
            var lobbyPanel = CreatePanel(rootTr, "LobbyPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.04f, 0.05f, 0.07f, 0.95f));
            lobbyPanel.SetActive(false);
            uiCtrl.lobbyPanel = lobbyPanel;

            // Back button
            var btnBackLobby = CreateButton(lobbyPanel.transform, "BtnBack", spriteCircleBack, new Vector2(0.12f, 0.94f), new Vector2(0.12f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(96f, 96f));
            btnBackLobby.GetComponent<Button>().onClick.AddListener(() => uiCtrl.ShowScreen(ProductionScreen.Home));

            // Header Banner
            var lobbyHeader = CreateImage(lobbyPanel.transform, "LobbyBanner", spriteBannerWood, new Vector2(0.54f, 0.94f), new Vector2(0.54f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(680f, 140f));
            var lobbyHeaderTxt = CreateTMP(lobbyHeader.transform, "Title", fontAsset, UiStrings.LobbyTitle, 36, colorGoldSub, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.txtLobbyHeader = lobbyHeaderTxt;

            // Room Code Box & Copy button
            var roomCodeCard = CreatePanel(lobbyPanel.transform, "RoomCodeCard", new Vector2(0.5f, 0.865f), new Vector2(0.5f, 0.865f), new Vector2(0.5f, 0.5f), new Vector2(940f, 75f), spritePanelDarkGold, Color.white);
            var lobbyCodeTxt = CreateTMP(roomCodeCard.transform, "RoomCode", fontAsset, "MÃ PHÒNG: HM4826 • SÂN LÀNG", 30, colorGoldSub, TextAlignmentOptions.MidlineLeft, new Vector2(0.38f, 0.5f), new Vector2(600f, 50f), wrap: false);
            uiCtrl.txtLobbyRoomCode = lobbyCodeTxt;

            var btnCopyCode = CreateButton(roomCodeCard.transform, "BtnCopyCode", spriteBtnYellow, new Vector2(0.85f, 0.5f), new Vector2(0.85f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(220f, 55f));
            var copyTxt = CreateTMP(btnCopyCode.transform, "Label", fontAsset, UiStrings.BtnCopyRoomCode, 24, new Color(0.22f, 0.11f, 0.02f), TextAlignmentOptions.Center);
            copyTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnCopyRoomCode = btnCopyCode.GetComponent<Button>();

            // 6 Slots Grid
            var slotsRoot = CreatePanel(lobbyPanel.transform, "SlotsGrid", new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(940f, 680f), spritePanelDarkGold, Color.white);
            uiCtrl.lobbySlotsContainer = slotsRoot.transform;
            uiCtrl.lobbySlots.Clear();

            string[] defaultLobbyNames = new string[] { "Nam Làng (Bạn)", "Nữ Năng Động", "Quậy Boy", "Cô Ba", "Anh Teen", "Chú Chó Vàng" };
            for (int i = 0; i < 6; i++)
            {
                float yNorm = 0.88f - i * 0.155f;
                var slotObj = CreatePanel(slotsRoot.transform, "Slot_" + i, new Vector2(0.5f, yNorm), new Vector2(0.5f, yNorm), new Vector2(0.5f, 0.5f), new Vector2(860f, 95f), i == 0 ? spriteRowMe : spriteRowLeaderboard, Color.white);
                var sName = CreateTMP(slotObj.transform, "Name", fontAsset, defaultLobbyNames[i], 30, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(0.06f, 0.5f), new Vector2(440f, 60f), wrap: false, pivot: new Vector2(0f, 0.5f));
                var sStatus = CreateTMP(slotObj.transform, "Status", fontAsset, i == 0 ? "CHƯA SẴN SÀNG" : "ĐÃ SẴN SÀNG", 26, i == 0 ? colorGoldSub : colorGreen, TextAlignmentOptions.MidlineRight, new Vector2(0.94f, 0.5f), new Vector2(340f, 60f), wrap: false, pivot: new Vector2(1f, 0.5f));

                var slotUI = new ProductionUIController.LobbySlotUI
                {
                    root = slotObj,
                    txtName = sName,
                    txtStatus = sStatus,
                    isReady = i != 0
                };
                uiCtrl.lobbySlots.Add(slotUI);
            }

            // Countdown / Notice Text
            var countdownTxt = CreateTMP(lobbyPanel.transform, "Countdown", fontAsset, UiStrings.LobbyWaitingAllReady, 28, colorGoldSub, TextAlignmentOptions.Center, new Vector2(0.5f, 0.32f), new Vector2(940f, 60f), wrap: true);
            uiCtrl.txtLobbyCountdown = countdownTxt;

            // 1-Player Button "SANG LUYỆN VỚI BOT"
            var btnGoBot = CreateButton(lobbyPanel.transform, "BtnGoToBotTraining", spriteBtnBlue, new Vector2(0.5f, 0.23f), new Vector2(0.5f, 0.23f), new Vector2(0.5f, 0.5f), new Vector2(880f, 105f));
            var goBotTxt = CreateTMP(btnGoBot.transform, "Label", fontAsset, UiStrings.BtnGoToBotTraining, 36, Color.white, TextAlignmentOptions.Center);
            goBotTxt.fontStyle = FontStyles.Bold;
            btnGoBot.SetActive(false);
            uiCtrl.btnLobbyToBotTraining = btnGoBot.GetComponent<Button>();

            // Ready / Start Button
            var btnLobbyReady = CreateButton(lobbyPanel.transform, "BtnLobbyReady", spriteBtnGreen, new Vector2(0.5f, 0.13f), new Vector2(0.5f, 0.13f), new Vector2(0.5f, 0.5f), new Vector2(880f, 115f));
            var lobbyReadyLabel = CreateTMP(btnLobbyReady.transform, "Label", fontAsset, UiStrings.BtnReady, 42, Color.white, TextAlignmentOptions.Center, wrap: false);
            lobbyReadyLabel.fontStyle = FontStyles.Bold;
            uiCtrl.btnLobbyReady = btnLobbyReady.GetComponent<Button>();
            uiCtrl.txtLobbyReadyLabel = lobbyReadyLabel;

            // Leave Room Button
            var btnLeave = CreateButton(lobbyPanel.transform, "BtnLeave", spriteBtnRed, new Vector2(0.5f, 0.045f), new Vector2(0.5f, 0.045f), new Vector2(0.5f, 0.5f), new Vector2(880f, 85f));
            var leaveTxt = CreateTMP(btnLeave.transform, "Label", fontAsset, UiStrings.BtnLeaveRoom, 30, Color.white, TextAlignmentOptions.Center, wrap: false);
            leaveTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnLeaveRoom = btnLeave.GetComponent<Button>();

            // ==================== G. QUICK MATCH OVERLAY ====================
            var qmOverlay = CreatePanel(rootTr, "QuickMatchOverlay", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.02f, 0.03f, 0.05f, 0.92f));
            qmOverlay.SetActive(false);
            uiCtrl.quickMatchOverlay = qmOverlay;

            var qmCard = CreatePanel(qmOverlay.transform, "QmCard", new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), new Vector2(920f, 520f), spritePanelDarkGold, Color.white);
            var qmHeader = CreateImage(qmCard.transform, "QmBanner", spriteBannerWood, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.5f), new Vector2(650f, 110f));
            var qmTitle = CreateTMP(qmHeader.transform, "Title", fontAsset, "GHÉP NHANH ONLINE", 36, colorGoldSub, TextAlignmentOptions.Center);
            qmTitle.fontStyle = FontStyles.Bold;

            CreateImage(qmCard.transform, "Pulse", spriteTimerPulse, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), new Vector2(100f, 100f));

            var qmStatusTxt = CreateTMP(qmCard.transform, "StatusTxt", fontAsset, UiStrings.MatchmakingSearching, 30, colorCream, TextAlignmentOptions.Center, new Vector2(0.5f, 0.44f), new Vector2(840f, 60f), wrap: true);
            uiCtrl.txtQuickMatchStatus = qmStatusTxt;

            var btnQmToBot = CreateButton(qmCard.transform, "BtnQmToBot", spriteBtnYellow, new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.5f), new Vector2(600f, 90f));
            var qmToBotTxt = CreateTMP(btnQmToBot.transform, "Label", fontAsset, "CHƠI VỚI BOT", 28, new Color(0.22f, 0.11f, 0.02f), TextAlignmentOptions.Center);
            qmToBotTxt.fontStyle = FontStyles.Bold;
            btnQmToBot.SetActive(false);
            uiCtrl.btnQuickMatchToBots = btnQmToBot.GetComponent<Button>();

            var btnCancelQm = CreateButton(qmCard.transform, "BtnCancelQm", spriteBtnRed, new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.5f), new Vector2(600f, 85f));
            var cancelQmTxt = CreateTMP(btnCancelQm.transform, "Label", fontAsset, "HỦY TÌM KIẾM", 28, Color.white, TextAlignmentOptions.Center);
            cancelQmTxt.fontStyle = FontStyles.Bold;
            uiCtrl.btnCancelQuickMatch = btnCancelQm.GetComponent<Button>();

            // ==================== F. GAMEPLAY HUD PANEL (P2 & P3) ====================
            var hudPanel = CreatePanel(rootTr, "GameplayHudPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, Color.clear);
            hudPanel.SetActive(false);
            uiCtrl.gameplayHudPanel = hudPanel;

            // P3: TopPlacingBar (Round, Timer, and Sẵn sàng in the same horizontal safe-area row; only shown in Placing)
            var topPlacingBar = CreatePanel(hudPanel.transform, "TopPlacingBar", new Vector2(0.5f, 0.968f), new Vector2(0.5f, 0.968f), new Vector2(0.5f, 0.5f), new Vector2(1020f, 76f), null, Color.clear);
            uiCtrl.topPlacingBar = topPlacingBar;

            var roundPill = CreateImage(topPlacingBar.transform, "RoundPill", spriteBannerWood, new Vector2(0.18f, 0.5f), new Vector2(0.18f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(320f, 72f));
            var roundTxt = CreateTMP(roundPill.transform, "Text", fontAsset, "VÒNG 1/5", 30, colorCream, TextAlignmentOptions.Center);
            roundTxt.fontStyle = FontStyles.Bold;
            FillRect(roundTxt.rectTransform, Vector2.zero, Vector2.zero);
            uiCtrl.roundPill = roundPill;
            uiCtrl.txtRound = roundTxt;

            var timerPulse = CreateImage(topPlacingBar.transform, "TimerPulse", spriteTimerPulse, new Vector2(0.50f, 0.5f), new Vector2(0.50f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(96f, 96f));
            timerPulse.SetActive(false);
            uiCtrl.timerPulseRing = timerPulse.GetComponent<Image>();

            var timerPill = CreateImage(topPlacingBar.transform, "TimerPill", spriteTimerGold, new Vector2(0.50f, 0.5f), new Vector2(0.50f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(80f, 80f));
            var timerTxt = CreateTMP(timerPill.transform, "Text", fontAsset, "5.0s", 28, colorCream, TextAlignmentOptions.Center);
            timerTxt.fontStyle = FontStyles.Bold;
            FillRect(timerTxt.rectTransform, Vector2.zero, Vector2.zero);
            uiCtrl.timerPill = timerPill;
            uiCtrl.txtTimer = timerTxt;

            var btnReadyInGame = CreateButton(topPlacingBar.transform, "BtnReady", spriteBtnYellow, new Vector2(0.82f, 0.5f), new Vector2(0.82f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(320f, 76f));
            var readyLabelInGame = CreateTMP(btnReadyInGame.transform, "Label", fontAsset, UiStrings.BtnReady, 30, Color.white, TextAlignmentOptions.Center);
            readyLabelInGame.fontStyle = FontStyles.Bold;
            uiCtrl.btnInGameReady = btnReadyInGame.GetComponent<Button>();
            uiCtrl.txtInGameReadyLabel = readyLabelInGame;

            // Phase Banner (below TopPlacingBar)
            var phaseBanner = CreateImage(hudPanel.transform, "PhaseBanner", spriteBannerWood, new Vector2(0.5f, 0.925f), new Vector2(0.5f, 0.925f), new Vector2(0.5f, 0.5f), new Vector2(1000f, 56f));
            var phaseTxt = CreateTMP(phaseBanner.transform, "Text", fontAsset, UiStrings.PhasePlacing + " • Chạm để đặt, kéo để ngắm", 26, colorCream, TextAlignmentOptions.Center);
            phaseTxt.fontStyle = FontStyles.Bold;
            FillRect(phaseTxt.rectTransform, Vector2.zero, Vector2.zero);
            uiCtrl.phaseBannerObj = phaseBanner;
            uiCtrl.txtPhaseBanner = phaseTxt;

            // P3: 6 HP Cards in 2 rows x 3 columns
            var hpContainer = CreatePanel(hudPanel.transform, "HpContainer", new Vector2(0.5f, 0.858f), new Vector2(0.5f, 0.858f), new Vector2(0.5f, 0.5f), new Vector2(1020f, 150f), null, Color.clear);
            uiCtrl.hpCardsContainer = hpContainer.transform;
            uiCtrl.hpCards.Clear();

            string[] playerIds = new string[] { "player", "bot-1", "bot-2", "bot-3", "bot-4", "bot-5" };
            string[] shortNames = new string[] { "Nam Làng", "Nữ Năng Động", "Quậy Boy", "Cô Ba", "Anh Teen", "Chú Chó Vàng" };
            float[] colAnchors = new float[] { 0.17f, 0.50f, 0.83f };
            float[] rowAnchors = new float[] { 0.76f, 0.24f };

            for (int i = 0; i < 6; i++)
            {
                int col = i % 3;
                int row = i / 3;
                Vector2 cardAnchor = new Vector2(colAnchors[col], rowAnchors[row]);
                var cardSprite = i == 0 ? spriteCardSelected : spriteCardChar;
                var cardObj = CreatePanel(hpContainer.transform, "HpCard_" + i, cardAnchor, cardAnchor, new Vector2(0.5f, 0.5f), new Vector2(330f, 72f), cardSprite, Color.white);
                var cardImg = cardObj.GetComponent<Image>();
                if (cardImg != null) cardImg.type = Image.Type.Sliced;

                var cName = CreateTMP(cardObj.transform, "Name", fontAsset, shortNames[i], 22, colorGoldSub, TextAlignmentOptions.MidlineLeft, new Vector2(0.33f, 0.70f), new Vector2(180f, 32f), wrap: false, pivot: new Vector2(0.5f, 0.5f));
                cName.fontStyle = FontStyles.Bold;

                // Hearts Container with 3 sprite hearts
                var heartsContainer = CreatePanel(cardObj.transform, "HeartsContainer", new Vector2(0.80f, 0.70f), new Vector2(0.80f, 0.70f), new Vector2(0.5f, 0.5f), new Vector2(96f, 30f), null, Color.clear);
                Image[] heartImgs = new Image[3];
                for (int h = 0; h < 3; h++)
                {
                    float hx = (h - 1) * 28f;
                    var hObj = CreateImage(heartsContainer.transform, "Heart_" + h, spriteHeartFull, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(24f, 24f));
                    hObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(hx, 0f);
                    heartImgs[h] = hObj.GetComponent<Image>();
                }

                var cHearts = CreateTMP(cardObj.transform, "Hearts", fontAsset, "HP: 3/3", 22, colorGreen, TextAlignmentOptions.MidlineRight, new Vector2(0.80f, 0.70f), new Vector2(110f, 32f), wrap: false, pivot: new Vector2(0.5f, 0.5f));
                cHearts.fontStyle = FontStyles.Bold;
                cHearts.gameObject.SetActive(false);

                var cHpNum = CreateTMP(cardObj.transform, "HpNum", fontAsset, "CÒN SỐNG", 20, colorGreen, TextAlignmentOptions.Center, new Vector2(0.50f, 0.24f), new Vector2(300f, 26f), wrap: false, pivot: new Vector2(0.5f, 0.5f));
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
            var weaponIcon = CreateImage(weaponPill.transform, "WeaponIcon", spriteWeaponDep, new Vector2(0.12f, 0.5f), new Vector2(0.12f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(52f, 52f));
            var weaponTxt = CreateTMP(weaponPill.transform, "Text", fontAsset, UiStrings.WeaponSandalName, 26, colorCream, TextAlignmentOptions.MidlineLeft, new Vector2(0.58f, 0.5f), new Vector2(300f, 50f));
            weaponTxt.fontStyle = FontStyles.Bold;
            uiCtrl.weaponPillObj = weaponPill;
            uiCtrl.txtWeaponIndicator = weaponTxt;

            // Round Result Overlay (in Gameplay HUD during Resolve / RoundEnd)
            var roundResultObj = CreatePanel(hudPanel.transform, "RoundResultOverlay", new Vector2(0.5f, 0.60f), new Vector2(0.5f, 0.60f), new Vector2(0.5f, 0.5f), new Vector2(880f, 220f), spritePanelDarkGold, new Color(0.08f, 0.10f, 0.14f, 0.94f));
            roundResultObj.SetActive(false);
            uiCtrl.roundResultOverlay = roundResultObj;

            var rrBanner = CreateImage(roundResultObj.transform, "Header", spriteBannerWood, new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.5f), new Vector2(760f, 80f));
            var rrTitle = CreateTMP(rrBanner.transform, "Title", fontAsset, "KẾT QUẢ VÒNG", 32, colorGoldSub, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.txtRoundResultTitle = rrTitle;

            var rrDetail = CreateTMP(roundResultObj.transform, "Detail", fontAsset, "Tất cả người chơi đã ném dép!", 24, Color.white, TextAlignmentOptions.Center, new Vector2(0.5f, 0.38f), new Vector2(800f, 90f), wrap: true);
            uiCtrl.txtRoundResultDetail = rrDetail;

            // ==================== G. RESULT PANEL (P1) ====================
            var resultPanel = CreatePanel(rootTr, "ResultPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.04f, 0.05f, 0.07f, 0.95f));
            resultPanel.SetActive(false);
            uiCtrl.resultPanel = resultPanel;

            var resultHeader = CreateImage(resultPanel.transform, "ResultBanner", spriteBannerWood, new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.5f), new Vector2(920f, 150f));
            var resTitle = CreateTMP(resultHeader.transform, "Title", fontAsset, "CHIẾN THẮNG!", 46, new Color(1f, 0.92f, 0.35f), TextAlignmentOptions.Center, wrap: false);
            uiCtrl.txtResultTitle = resTitle;

            var resSubtitle = CreateTMP(resultPanel.transform, "Subtitle", fontAsset, "Người thắng: Nam Làng", 25, Color.white, TextAlignmentOptions.Center, new Vector2(0.5f, 0.85f), new Vector2(1000f, 54f), wrap: false);
            resSubtitle.enableAutoSizing = true;
            resSubtitle.fontSizeMin = 18f;
            resSubtitle.fontSizeMax = 26f;
            uiCtrl.txtResultSubtitle = resSubtitle;

            // Full Ranking Board (all 6 players)
            var boardCard = CreatePanel(resultPanel.transform, "BoardCard", new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(940f, 720f), spritePanelDarkGold, Color.white);
            uiCtrl.resultRankingsContainer = boardCard.transform;
            uiCtrl.resultRows.Clear();

            for (int i = 0; i < 6; i++)
            {
                float yNorm = 0.88f - i * 0.155f;
                var rowObj = CreatePanel(boardCard.transform, "RankRow_" + i, new Vector2(0.5f, yNorm), new Vector2(0.5f, yNorm), new Vector2(0.5f, 0.5f), new Vector2(860f, 95f), i == 0 ? spriteRowMe : spriteRowLeaderboard, Color.white);
                var rRank = CreateTMP(rowObj.transform, "Rank", fontAsset, "#" + (i + 1), 28, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Center, new Vector2(0.05f, 0.5f), new Vector2(60f, 50f), wrap: false);
                var rBadge = CreateTMP(rowObj.transform, "RankBadge", fontAsset, "HẠNG " + (i + 1) + " [TẠM]", 16, new Color(1f, 0.9f, 0.4f), TextAlignmentOptions.Center, new Vector2(0.14f, 0.5f), new Vector2(110f, 32f), wrap: false);
                var rName = CreateTMP(rowObj.transform, "Name", fontAsset, defaultLobbyNames[i], 24, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(0.24f, 0.5f), new Vector2(240f, 50f), wrap: false, pivot: new Vector2(0f, 0.5f));
                var rHp = CreateTMP(rowObj.transform, "Hp", fontAsset, "HP: " + Mathf.Max(0, 3 - i / 2) + "/3 • HIT: " + (3 - i / 2), 22, new Color(0.85f, 0.95f, 1f), TextAlignmentOptions.Center, new Vector2(0.64f, 0.5f), new Vector2(230f, 50f), wrap: false);
                var rStatus = CreateTMP(rowObj.transform, "Status", fontAsset, i == 0 ? "★ THẮNG" : "✖ Đã loại", 22, i == 0 ? new Color(1f, 0.9f, 0.2f) : new Color(1f, 0.45f, 0.45f), TextAlignmentOptions.MidlineRight, new Vector2(0.96f, 0.5f), new Vector2(180f, 50f), wrap: false, pivot: new Vector2(1f, 0.5f));

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

            var btnReplay = CreateButton(resultPanel.transform, "BtnReplay", spriteBtnYellow, new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(880f, 130f));
            CreateTMP(btnReplay.transform, "Label", fontAsset, UiStrings.BtnReplay, 42, new Color(0.2f, 0.1f, 0f), TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnReplay = btnReplay.GetComponent<Button>();

            var btnHome = CreateButton(resultPanel.transform, "BtnHome", spriteBtnBlue, new Vector2(0.5f, 0.11f), new Vector2(0.5f, 0.11f), new Vector2(0.5f, 0.5f), new Vector2(880f, 110f));
            CreateTMP(btnHome.transform, "Label", fontAsset, "VỀ TRANG CHỦ", 38, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnBackToHome = btnHome.GetComponent<Button>();

            // ==================== H. LEAVE ROOM CONFIRMATION DIALOG ====================
            var leaveDialogObj = CreatePanel(rootTr, "DialogLeaveRoom", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.03f, 0.04f, 0.06f, 0.88f));
            leaveDialogObj.SetActive(false);
            uiCtrl.dialogLeaveRoom = leaveDialogObj;

            var leaveCard = CreatePanel(leaveDialogObj.transform, "LeaveCard", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(760f, 440f), spritePanelDarkGold, Color.white);

            var leaveBanner = CreateImage(leaveCard.transform, "LeaveBanner", spriteBannerWood, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.5f), new Vector2(640f, 100f));
            var leaveTitle = CreateTMP(leaveBanner.transform, "Title", fontAsset, "XÁC NHẬN RỜI PHÒNG", 32, colorGoldSub, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.txtLeaveDialogTitle = leaveTitle;

            var leaveMsg = CreateTMP(leaveCard.transform, "Message", fontAsset, "Bạn có chắc chắn muốn rời phòng không?\nMọi kết quả của trận đấu sẽ không được lưu.", 24, Color.white, TextAlignmentOptions.Center, new Vector2(0.5f, 0.55f), new Vector2(680f, 140f), wrap: true);
            uiCtrl.txtLeaveDialogMessage = leaveMsg;

            // Confirm Button (Red CTA)
            var btnConfirmLeave = CreateButton(leaveCard.transform, "BtnConfirmLeave", spriteBtnRed, new Vector2(0.28f, 0.22f), new Vector2(0.28f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(300f, 90f));
            CreateTMP(btnConfirmLeave.transform, "Label", fontAsset, "XÁC NHẬN", 30, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnConfirmLeave = btnConfirmLeave.GetComponent<Button>();

            // Cancel Button
            var btnCancelLeave = CreateButton(leaveCard.transform, "BtnCancelLeave", spriteBtnYellow, new Vector2(0.72f, 0.22f), new Vector2(0.72f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(300f, 90f));
            CreateTMP(btnCancelLeave.transform, "Label", fontAsset, "QUAY LẠI", 30, new Color(0.2f, 0.1f, 0f), TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnCancelLeave = btnCancelLeave.GetComponent<Button>();

            // Ensure Toast, Leave Dialog and Fingerprint are at the very top of canvas rendering hierarchy
            if (toastObj != null) toastObj.transform.SetAsLastSibling();
            if (leaveDialogObj != null) leaveDialogObj.transform.SetAsLastSibling();

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

            // 8. Configure Build Scenes (Graybox is sole production scene, MainMatch kept intact)
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(GrayboxScenePath, true)
            };
            EditorBuildSettings.scenes = scenes.ToArray();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("Successfully built Production UI for Graybox.unity!");
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Sprite sprite, Color color)
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
            return go;
        }

        private static GameObject CreateImage(Transform parent, string name, Sprite sprite, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size)
        {
            return CreatePanel(parent, name, anchorMin, anchorMax, pivot, size, sprite, Color.white);
        }

        private static GameObject CreateButton(Transform parent, string name, Sprite sprite, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size)
        {
            var go = CreatePanel(parent, name, anchorMin, anchorMax, pivot, size, sprite, Color.white);
            go.AddComponent<Button>();
            return go;
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

        private static void PopulateConfigVisualAssets(GameConfigAsset configAsset, TMP_FontAsset fontAsset)
        {
            configAsset.shadowSprite = LoadSprite("Assets/Sprites/procedural_shadow.png");
            configAsset.footRingSprite = LoadSprite("Assets/Sprites/procedural_foot_ring.png");
            configAsset.vietnameseFont = fontAsset;
            configAsset.characterIdleSprites = new Sprite[]
            {
                LoadSprite("Assets/Sprites/cau_be_lang_idle.png"),
                LoadSprite("Assets/Sprites/co_nang_the_thao_idle.png"),
                LoadSprite("Assets/Sprites/anh_nui_idle.png"),
                LoadSprite("Assets/Sprites/mai_hong_idle.png"),
                LoadSprite("Assets/Sprites/anh_non_la_idle.png"),
                LoadSprite("Assets/Sprites/chu_cho_vang_idle.png"),
                LoadSprite("Assets/Sprites/meo_mun_idle.png")
            };
            configAsset.namLangPoseSprites = new Sprite[]
            {
                LoadSprite("Assets/Sprites/cau_be_lang_idle.png"),
                LoadSprite("Assets/Sprites/cau_be_lang_aim.png"),
                LoadSprite("Assets/Sprites/cau_be_lang_throw_release.png"),
                LoadSprite("Assets/Sprites/cau_be_lang_hit.png"),
                LoadSprite("Assets/Sprites/cau_be_lang_defeat.png")
            };
            configAsset.anhNonLaPoseSprites = new Sprite[]
            {
                LoadSprite("Assets/Sprites/anh_non_la_idle.png"),
                LoadSprite("Assets/Sprites/anh_non_la_aim.png"),
                LoadSprite("Assets/Sprites/anh_non_la_throw_release.png"),
                LoadSprite("Assets/Sprites/anh_non_la_hit.png"),
                LoadSprite("Assets/Sprites/anh_non_la_defeat.png")
            };
            configAsset.sandalProjectileSprite = LoadSprite("Assets/Sprites/dep_projectile.png") ?? LoadSprite("Assets/Sprites/dep_icon.png");
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
