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

            // 1. Setup Camera for Portrait 1080x2340
            var camGo = GameObject.Find("Main Camera");
            if (camGo == null) camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            if (cam == null) cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.07f, 0.09f, 1f);
            camGo.transform.position = new Vector3(0f, 0.2f, -10f);

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

            // 6. Build Canvas (1080x2340 Portrait)
            var canvasGo = new GameObject("ProductionCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2340f);
            scaler.matchWidthOrHeight = 1.0f; // Portrait height match

            var uiCtrl = canvasGo.AddComponent<ProductionUIController>();

            // Root container
            var rootTr = canvasGo.transform;

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

            // Home Header & Logo
            var homeHeader = CreateImage(homePanel.transform, "HomeBanner", spriteBannerWood, new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.5f), new Vector2(860f, 160f));
            var logoTxt = CreateTMP(homeHeader.transform, "LogoTxt", fontAsset, "HIT ME", 76, new Color(1f, 0.92f, 0.35f), TextAlignmentOptions.Center);
            logoTxt.fontStyle = FontStyles.Bold;
            FillRect(logoTxt.rectTransform, new Vector2(20f, 10f), new Vector2(-20f, -10f));
            uiCtrl.txtHomeLogo = logoTxt;

            // Selected Character & Weapon info box
            var loadoutCard = CreatePanel(homePanel.transform, "LoadoutCard", new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), new Vector2(920f, 180f), spritePanelDarkGold, Color.white);
            var loadoutTxt = CreateTMP(loadoutCard.transform, "LoadoutText", fontAsset, "NHÂN VẬT: Nam Làng (Lv.1)\nVŨ KHÍ: Dép Tổ Ong Huyền Thoại", 38, Color.white, TextAlignmentOptions.Center);
            FillRect(loadoutTxt.rectTransform, new Vector2(30f, 15f), new Vector2(-30f, -15f));
            uiCtrl.txtSelectedLoadout = loadoutTxt;

            // 4 Action Buttons
            // Button 1: Chơi Nhanh (Yellow CTA)
            var btnQuick = CreateButton(homePanel.transform, "BtnQuickMatch", spriteBtnYellow, new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.5f), new Vector2(880f, 140f));
            CreateTMP(btnQuick.transform, "Label", fontAsset, "CHƠI NHANH", 46, new Color(0.2f, 0.1f, 0f), TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnQuickMatch = btnQuick.GetComponent<Button>();

            // Button 2: Tạo Phòng (Blue CTA)
            var btnCreate = CreateButton(homePanel.transform, "BtnCreateRoom", spriteBtnBlue, new Vector2(0.5f, 0.33f), new Vector2(0.5f, 0.33f), new Vector2(0.5f, 0.5f), new Vector2(880f, 130f));
            CreateTMP(btnCreate.transform, "Label", fontAsset, "TẠO PHÒNG", 42, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnCreateRoom = btnCreate.GetComponent<Button>();

            // Button 3: Vào Phòng (Green CTA)
            var btnJoin = CreateButton(homePanel.transform, "BtnJoinRoom", spriteBtnGreen, new Vector2(0.5f, 0.23f), new Vector2(0.5f, 0.23f), new Vector2(0.5f, 0.5f), new Vector2(880f, 130f));
            CreateTMP(btnJoin.transform, "Label", fontAsset, "VÀO PHÒNG", 42, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnJoinRoom = btnJoin.GetComponent<Button>();

            // Button 4: Luyện với bot (Navy)
            var btnBot = CreateButton(homePanel.transform, "BtnBotTraining", spritePanelDarkGold, new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.5f), new Vector2(880f, 120f));
            CreateTMP(btnBot.transform, "Label", fontAsset, "LUYỆN VỚI BOT (OFFLINE)", 34, new Color(1f, 0.85f, 0.4f), TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnBotTraining = btnBot.GetComponent<Button>();

            // ==================== C. CREATE ROOM PANEL ====================
            var createRoomPanel = CreatePanel(rootTr, "CreateRoomPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.04f, 0.05f, 0.07f, 0.95f));
            createRoomPanel.SetActive(false);
            uiCtrl.createRoomPanel = createRoomPanel;

            // Back button
            var btnBackCreate = CreateButton(createRoomPanel.transform, "BtnBack", spriteCircleBack, new Vector2(0.12f, 0.94f), new Vector2(0.12f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(100f, 100f));
            uiCtrl.btnBackFromCreate = btnBackCreate.GetComponent<Button>();

            var createHeader = CreateImage(createRoomPanel.transform, "CreateBanner", spriteBannerWood, new Vector2(0.54f, 0.94f), new Vector2(0.54f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(650f, 140f));
            CreateTMP(createHeader.transform, "Title", fontAsset, "TẠO PHÒNG", 48, new Color(1f, 0.92f, 0.35f), TextAlignmentOptions.Center);

            // Container Panel
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

            // Found room card
            var foundCard = CreatePanel(joinRoomPanel.transform, "FoundCard", new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.5f), new Vector2(940f, 200f), spriteCardSlot, Color.white);
            var foundTxt = CreateTMP(foundCard.transform, "FoundText", fontAsset, "✅ Đã tìm thấy: Phòng Chiến Hữu\nChủ phòng: Nam Làng • 4/6 người • Sân Làng", 34, new Color(0.2f, 1f, 0.4f), TextAlignmentOptions.Center);
            FillRect(foundTxt.rectTransform, new Vector2(20f, 10f), new Vector2(-20f, -10f));
            uiCtrl.roomFoundCard = foundCard;
            uiCtrl.txtFoundRoomInfo = foundTxt;

            var btnSubmitJoin = CreateButton(joinRoomPanel.transform, "BtnSubmitJoin", spriteBtnGreen, new Vector2(0.5f, 0.20f), new Vector2(0.5f, 0.20f), new Vector2(0.5f, 0.5f), new Vector2(900f, 150f));
            CreateTMP(btnSubmitJoin.transform, "Label", fontAsset, "VÀO PHÒNG", 48, Color.white, TextAlignmentOptions.Center);
            uiCtrl.btnSubmitJoinRoom = btnSubmitJoin.GetComponent<Button>();

            // ==================== E. LOBBY PANEL ====================
            var lobbyPanel = CreatePanel(rootTr, "LobbyPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.04f, 0.05f, 0.07f, 0.95f));
            lobbyPanel.SetActive(false);
            uiCtrl.lobbyPanel = lobbyPanel;

            var lobbyHeader = CreateImage(lobbyPanel.transform, "LobbyBanner", spriteBannerWood, new Vector2(0.5f, 0.94f), new Vector2(0.5f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(920f, 150f));
            var lobbyHeaderTxt = CreateTMP(lobbyHeader.transform, "Title", fontAsset, "PHÒNG CHIẾN HỮU • SÂN LÀNG", 36, new Color(1f, 0.92f, 0.35f), TextAlignmentOptions.Center, wrap: false);
            uiCtrl.txtLobbyHeader = lobbyHeaderTxt;

            var lobbyCodeTxt = CreateTMP(lobbyPanel.transform, "RoomCode", fontAsset, "MÃ PHÒNG: HM4826 • SÂN LÀNG", 32, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Center, new Vector2(0.5f, 0.87f), new Vector2(900f, 50f), wrap: false);
            uiCtrl.txtLobbyRoomCode = lobbyCodeTxt;

            // Slots Grid
            var slotsRoot = CreatePanel(lobbyPanel.transform, "SlotsGrid", new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.5f), new Vector2(940f, 700f), spritePanelDarkGold, Color.white);
            uiCtrl.lobbySlotsContainer = slotsRoot.transform;
            uiCtrl.lobbySlots.Clear();

            string[] defaultLobbyNames = new string[] { "Nam Làng (Bạn)", "Nữ Năng Động", "Quậy Boy", "Cô Ba", "Anh Teen", "Chú Chó Vàng" };
            for (int i = 0; i < 6; i++)
            {
                float yNorm = 0.88f - i * 0.155f;
                var slotObj = CreatePanel(slotsRoot.transform, "Slot_" + i, new Vector2(0.5f, yNorm), new Vector2(0.5f, yNorm), new Vector2(0.5f, 0.5f), new Vector2(860f, 95f), i == 0 ? spriteRowMe : spriteRowLeaderboard, Color.white);
                var sName = CreateTMP(slotObj.transform, "Name", fontAsset, defaultLobbyNames[i], 32, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(0.06f, 0.5f), new Vector2(440f, 60f), wrap: false, pivot: new Vector2(0f, 0.5f));
                var sStatus = CreateTMP(slotObj.transform, "Status", fontAsset, i == 0 ? "CHƯA SẴN SÀNG" : "ĐÃ SẴN SÀNG", 28, i == 0 ? new Color(1f, 0.7f, 0.2f) : new Color(0.2f, 1f, 0.4f), TextAlignmentOptions.MidlineRight, new Vector2(0.94f, 0.5f), new Vector2(340f, 60f), wrap: false, pivot: new Vector2(1f, 0.5f));

                var slotUI = new ProductionUIController.LobbySlotUI
                {
                    root = slotObj,
                    txtName = sName,
                    txtStatus = sStatus,
                    isReady = i != 0
                };
                uiCtrl.lobbySlots.Add(slotUI);
            }

            var countdownTxt = CreateTMP(lobbyPanel.transform, "Countdown", fontAsset, "Đang chờ tất cả người chơi sẵn sàng...", 34, new Color(1f, 0.9f, 0.3f), TextAlignmentOptions.Center, new Vector2(0.5f, 0.33f), new Vector2(900f, 60f), wrap: false);
            uiCtrl.txtLobbyCountdown = countdownTxt;

            var btnLobbyReady = CreateButton(lobbyPanel.transform, "BtnLobbyReady", spriteBtnGreen, new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(880f, 130f));
            var lobbyReadyLabel = CreateTMP(btnLobbyReady.transform, "Label", fontAsset, "SẴN SÀNG", 44, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnLobbyReady = btnLobbyReady.GetComponent<Button>();
            uiCtrl.txtLobbyReadyLabel = lobbyReadyLabel;

            var btnLeave = CreateButton(lobbyPanel.transform, "BtnLeave", spriteBtnRed, new Vector2(0.5f, 0.11f), new Vector2(0.5f, 0.11f), new Vector2(0.5f, 0.5f), new Vector2(880f, 110f));
            CreateTMP(btnLeave.transform, "Label", fontAsset, "RỜI PHÒNG", 36, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnLeaveRoom = btnLeave.GetComponent<Button>();

            // ==================== F. GAMEPLAY HUD PANEL ====================
            var hudPanel = CreatePanel(rootTr, "GameplayHudPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, Color.clear);
            hudPanel.SetActive(false);
            uiCtrl.gameplayHudPanel = hudPanel;

            // Top Bar: Round & Timer
            var roundPill = CreateImage(hudPanel.transform, "RoundPill", spriteHudPill, new Vector2(0.18f, 0.96f), new Vector2(0.18f, 0.96f), new Vector2(0.5f, 0.5f), new Vector2(280f, 90f));
            var roundTxt = CreateTMP(roundPill.transform, "Text", fontAsset, "VÒNG 1/5", 34, new Color(1f, 0.92f, 0.35f), TextAlignmentOptions.Center);
            FillRect(roundTxt.rectTransform, Vector2.zero, Vector2.zero);
            uiCtrl.txtRound = roundTxt;

            var timerPill = CreateImage(hudPanel.transform, "TimerPill", spriteHudPill, new Vector2(0.50f, 0.96f), new Vector2(0.50f, 0.96f), new Vector2(0.5f, 0.5f), new Vector2(240f, 90f));
            var timerTxt = CreateTMP(timerPill.transform, "Text", fontAsset, "5.0s", 38, Color.white, TextAlignmentOptions.Center);
            FillRect(timerTxt.rectTransform, Vector2.zero, Vector2.zero);
            uiCtrl.txtTimer = timerTxt;

            var btnReadyInGame = CreateButton(hudPanel.transform, "BtnReady", spriteBtnGreen, new Vector2(0.82f, 0.96f), new Vector2(0.82f, 0.96f), new Vector2(0.5f, 0.5f), new Vector2(280f, 90f));
            var readyLabelInGame = CreateTMP(btnReadyInGame.transform, "Label", fontAsset, "SẴN SÀNG", 32, Color.white, TextAlignmentOptions.Center);
            uiCtrl.btnInGameReady = btnReadyInGame.GetComponent<Button>();
            uiCtrl.txtInGameReadyLabel = readyLabelInGame;

            // Phase Banner
            var phaseBanner = CreateImage(hudPanel.transform, "PhaseBanner", spriteInputCyan, new Vector2(0.5f, 0.90f), new Vector2(0.5f, 0.90f), new Vector2(0.5f, 0.5f), new Vector2(960f, 80f));
            var phaseTxt = CreateTMP(phaseBanner.transform, "Text", fontAsset, "CHỌN HƯỚNG NÉM • Chạm để đặt, kéo để ngắm", 32, Color.white, TextAlignmentOptions.Center);
            FillRect(phaseTxt.rectTransform, Vector2.zero, Vector2.zero);
            uiCtrl.txtPhaseBanner = phaseTxt;

            // 6 HP Cards strip
            var hpContainer = CreatePanel(hudPanel.transform, "HpContainer", new Vector2(0.5f, 0.83f), new Vector2(0.5f, 0.83f), new Vector2(0.5f, 0.5f), new Vector2(1040f, 130f), null, Color.clear);
            uiCtrl.hpCardsContainer = hpContainer.transform;
            uiCtrl.hpCards.Clear();

            string[] playerIds = new string[] { "player", "bot-1", "bot-2", "bot-3", "bot-4", "bot-5" };
            string[] shortNames = new string[] { "Nam Làng", "Nữ Động", "Quậy Boy", "Cô Ba", "Anh Teen", "Chó Vàng" };
            for (int i = 0; i < 6; i++)
            {
                float xNorm = 0.083f + i * 0.166f;
                var cardObj = CreatePanel(hpContainer.transform, "HpCard_" + i, new Vector2(xNorm, 0.5f), new Vector2(xNorm, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(158f, 115f), spriteHudPill, Color.white);
                var cName = CreateTMP(cardObj.transform, "Name", fontAsset, shortNames[i], 20, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Center, new Vector2(0.5f, 0.72f), new Vector2(148f, 28f), wrap: false);
                var cHearts = CreateTMP(cardObj.transform, "Hearts", fontAsset, "HP: 3/3", 22, new Color(0.2f, 1f, 0.4f), TextAlignmentOptions.Center, new Vector2(0.5f, 0.40f), new Vector2(148f, 28f), wrap: false);
                var cHpNum = CreateTMP(cardObj.transform, "HpNum", fontAsset, "CÒN SỐNG", 18, new Color(0.8f, 0.9f, 1f), TextAlignmentOptions.Center, new Vector2(0.5f, 0.15f), new Vector2(148f, 24f), wrap: false);

                uiCtrl.hpCards.Add(new ProductionUIController.PlayerHpCardUI
                {
                    root = cardObj,
                    txtName = cName,
                    txtHearts = cHearts,
                    txtHpNum = cHpNum,
                    playerId = playerIds[i]
                });
            }

            // Weapon Indicator at bottom
            var weaponPill = CreateImage(hudPanel.transform, "WeaponPill", spriteHudPill, new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0.5f), new Vector2(400f, 80f));
            var weaponTxt = CreateTMP(weaponPill.transform, "Text", fontAsset, "VŨ KHÍ: Dép Tổ Ong", 30, Color.white, TextAlignmentOptions.Center);
            FillRect(weaponTxt.rectTransform, Vector2.zero, Vector2.zero);
            uiCtrl.txtWeaponIndicator = weaponTxt;

            // ==================== G. RESULT PANEL ====================
            var resultPanel = CreatePanel(rootTr, "ResultPanel", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, null, new Color(0.04f, 0.05f, 0.07f, 0.95f));
            resultPanel.SetActive(false);
            uiCtrl.resultPanel = resultPanel;

            var resultHeader = CreateImage(resultPanel.transform, "ResultBanner", spriteBannerWood, new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.92f), new Vector2(0.5f, 0.5f), new Vector2(920f, 150f));
            var resTitle = CreateTMP(resultHeader.transform, "Title", fontAsset, "CHIẾN THẮNG! 🏆", 46, new Color(1f, 0.92f, 0.35f), TextAlignmentOptions.Center, wrap: false);
            uiCtrl.txtResultTitle = resTitle;

            var resSubtitle = CreateTMP(resultPanel.transform, "Subtitle", fontAsset, "Người thắng: Nam Làng", 32, Color.white, TextAlignmentOptions.Center, new Vector2(0.5f, 0.85f), new Vector2(900f, 50f), wrap: false);
            uiCtrl.txtResultSubtitle = resSubtitle;

            // Full Ranking Board (all 6 players)
            var boardCard = CreatePanel(resultPanel.transform, "BoardCard", new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.5f), new Vector2(940f, 720f), spritePanelDarkGold, Color.white);
            uiCtrl.resultRankingsContainer = boardCard.transform;
            uiCtrl.resultRows.Clear();

            for (int i = 0; i < 6; i++)
            {
                float yNorm = 0.88f - i * 0.155f;
                var rowObj = CreatePanel(boardCard.transform, "RankRow_" + i, new Vector2(0.5f, yNorm), new Vector2(0.5f, yNorm), new Vector2(0.5f, 0.5f), new Vector2(860f, 95f), i == 0 ? spriteRowMe : spriteRowLeaderboard, Color.white);
                var rRank = CreateTMP(rowObj.transform, "Rank", fontAsset, "#" + (i + 1), 32, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Center, new Vector2(0.07f, 0.5f), new Vector2(80f, 50f), wrap: false);
                var rName = CreateTMP(rowObj.transform, "Name", fontAsset, defaultLobbyNames[i], 30, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(0.18f, 0.5f), new Vector2(320f, 50f), wrap: false, pivot: new Vector2(0f, 0.5f));
                var rHp = CreateTMP(rowObj.transform, "Hp", fontAsset, (3 - i / 2) + " HP", 30, new Color(1f, 0.4f, 0.4f), TextAlignmentOptions.Center, new Vector2(0.66f, 0.5f), new Vector2(140f, 50f), wrap: false);
                var rStatus = CreateTMP(rowObj.transform, "Status", fontAsset, i == 0 ? "★ THẮNG" : "Đã loại", 28, i == 0 ? new Color(1f, 0.9f, 0.2f) : Color.gray, TextAlignmentOptions.MidlineRight, new Vector2(0.94f, 0.5f), new Vector2(200f, 50f), wrap: false, pivot: new Vector2(1f, 0.5f));

                uiCtrl.resultRows.Add(new ProductionUIController.ResultRowUI
                {
                    root = rowObj,
                    txtRank = rRank,
                    txtName = rName,
                    txtHp = rHp,
                    txtStatus = rStatus,
                    imgBg = rowObj.GetComponent<Image>()
                });
            }

            var btnReplay = CreateButton(resultPanel.transform, "BtnReplay", spriteBtnYellow, new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.5f), new Vector2(880f, 130f));
            CreateTMP(btnReplay.transform, "Label", fontAsset, "CHƠI LẠI", 42, new Color(0.2f, 0.1f, 0f), TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnReplay = btnReplay.GetComponent<Button>();

            var btnHome = CreateButton(resultPanel.transform, "BtnHome", spriteBtnBlue, new Vector2(0.5f, 0.11f), new Vector2(0.5f, 0.11f), new Vector2(0.5f, 0.5f), new Vector2(880f, 110f));
            CreateTMP(btnHome.transform, "Label", fontAsset, "VỀ TRANG CHỦ", 38, Color.white, TextAlignmentOptions.Center, wrap: false);
            uiCtrl.btnBackToHome = btnHome.GetComponent<Button>();

            // 7. Update MatchController references
            var matchCtrl = GameObject.Find("Match Controller")?.GetComponent<MatchController>();
            if (matchCtrl != null)
            {
                uiCtrl.matchController = matchCtrl;
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
    }
}
