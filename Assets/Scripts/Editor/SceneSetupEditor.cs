using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using HitMe.Core;
using HitMe.Gameplay;
using HitMe.UI;
using HitMe.Audio;

namespace HitMe.Editor
{
    public static class SceneSetupEditor
    {
        [MenuItem("Tools/HIT ME/Build Sample Scene")]
        public static void BuildSampleScene()
        {
            // 1. Ensure directories and configure sprites
            if (!Directory.Exists("Assets/Prefabs"))
            {
                Directory.CreateDirectory("Assets/Prefabs");
            }
            SpriteImportSetup.ConfigureAllSprites();
            AssetAutomationEditor.GenerateScriptableObjects();
            AssetDatabase.Refresh();

            // Create new empty scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Camera Setup
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6.8f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.backgroundColor = new Color(0.12f, 0.08f, 0.06f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();

            // 3. Load Sprites & Font
            Sprite arenaFloorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Arena/arena_floor_background.png");
            if (arenaFloorSprite == null) arenaFloorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/arena_background.png");
            Sprite arenaLotusSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Arena/arena_center_lotus_decal.png");
            Sprite arenaBoundarySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Arena/arena_boundary_line.png");

            Sprite idleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_idle.png");
            Sprite moveSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_move.png");
            Sprite aimSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_aim.png");
            Sprite throwWindupSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_throw_windup.png");
            Sprite throwReleaseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_throw_release.png");
            Sprite hitSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_hit.png");
            Sprite victorySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_victory.png");
            Sprite defeatSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_defeat.png");
            Sprite depProjSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/dep_to_ong_projectile.png");
            if (depProjSprite == null) depProjSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/dep_projectile.png");
            Sprite depIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/dep_to_ong_icon.png");
            if (depIconSprite == null) depIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/dep_icon.png");
            Sprite shadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/shadow.png");
            Sprite ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/selection_ring.png");
            Sprite destSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/destination_marker.png");

            Sprite aimGlowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/aim_glow.png");
            Sprite menuFullSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_menu_full.png");
            Sprite hudTopSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_hud_top.png");
            Sprite hudBottomSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ui_hud_bottom.png");

            Font vnFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/HitMeFont.ttf");
            if (vnFont == null) vnFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 4. Modular Layered Arena (AR-007, AR-009)
            GameObject arenaRoot = new GameObject("ArenaRoot_25D");

            // Layer 1: Floor / Yard
            GameObject floorObj = new GameObject("ArenaFloor");
            floorObj.transform.SetParent(arenaRoot.transform, false);
            SpriteRenderer floorSr = floorObj.AddComponent<SpriteRenderer>();
            floorSr.sprite = arenaFloorSprite;
            floorSr.sortingOrder = -20;
            floorObj.transform.localScale = new Vector3(1.02f, 1.02f, 1f);

            // Layer 2: Center Lotus Decal
            if (arenaLotusSprite != null)
            {
                GameObject lotusObj = new GameObject("ArenaCenterLotus");
                lotusObj.transform.SetParent(arenaRoot.transform, false);
                lotusObj.transform.localPosition = new Vector3(0f, 0.2f, 0f);
                lotusObj.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
                SpriteRenderer lotusSr = lotusObj.AddComponent<SpriteRenderer>();
                lotusSr.sprite = arenaLotusSprite;
                lotusSr.color = new Color(1f, 1f, 1f, 0.85f);
                lotusSr.sortingOrder = -10;
            }

            // Layer 3: Logical Boundary Line
            if (arenaBoundarySprite != null)
            {
                GameObject boundObj = new GameObject("ArenaBoundary");
                boundObj.transform.SetParent(arenaRoot.transform, false);
                boundObj.transform.localPosition = new Vector3(0f, 0f, 0f);
                boundObj.transform.localScale = new Vector3(1.0f, 1.0f, 1f);
                SpriteRenderer boundSr = boundObj.AddComponent<SpriteRenderer>();
                boundSr.sprite = arenaBoundarySprite;
                boundSr.color = new Color(1f, 1f, 1f, 0.75f);
                boundSr.sortingOrder = -5;
            }

            // 5. Create Projectile Prefab
            GameObject projObj = new GameObject("ProjectilePrefab");
            SpriteRenderer projSr = projObj.AddComponent<SpriteRenderer>();
            projSr.sprite = depProjSprite;
            projSr.sortingOrder = 15;
            projObj.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
            projObj.AddComponent<ProjectileView>();

            string prefabPath = "Assets/Prefabs/Projectile.prefab";
            GameObject projPrefab = PrefabUtility.SaveAsPrefabAsset(projObj, prefabPath);
            Object.DestroyImmediate(projObj);

            // 6. Create Destination Marker Object
            GameObject destMarkerObj = new GameObject("DestinationMarker");
            SpriteRenderer destSr = destMarkerObj.AddComponent<SpriteRenderer>();
            destSr.sprite = destSprite;
            destSr.sortingOrder = -5;
            destMarkerObj.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            destSr.enabled = false;

            // 7. Create Player Actor
            GameObject playerObj = CreateActorObject("PlayerActor", ActorType.Player, "Nam Làng (Bạn)",
                new Color(0f, 0.9f, 1f, 0.95f), idleSprite, moveSprite, aimSprite, throwWindupSprite,
                throwReleaseSprite, hitSprite, victorySprite, defeatSprite, shadowSprite, ringSprite, depIconSprite, aimGlowSprite);
            ActorView playerActor = playerObj.GetComponent<ActorView>();
            playerObj.transform.position = new Vector3(0f, -3.5f, 0f);

            // 8. Create Bot Actor
            GameObject botObj = CreateActorObject("BotActor", ActorType.Bot, "Bot Luyện Tập",
                new Color(1f, 0.2f, 0.25f, 0.95f), idleSprite, moveSprite, aimSprite, throwWindupSprite,
                throwReleaseSprite, hitSprite, victorySprite, defeatSprite, shadowSprite, ringSprite, depIconSprite, aimGlowSprite);
            ActorView botActor = botObj.GetComponent<ActorView>();
            botActor.characterRenderer.color = new Color(1f, 0.88f, 0.88f);
            botObj.transform.position = new Vector3(0f, 3.0f, 0f);

            // 9. Camera Controller 2.5D (Isometric tracking)
            CameraController25D camCtrl = camObj.AddComponent<CameraController25D>();
            camCtrl.Target = playerObj.transform;

            // 10. Aim Controller (with Movement & Destination Marker)
            GameObject aimCtrlObj = new GameObject("AimController");
            AimController aimController = aimCtrlObj.AddComponent<AimController>();
            aimController.mainCamera = cam;
            aimController.playerActor = playerActor;
            aimController.destinationMarker = destMarkerObj.transform;
            aimController.destinationMarkerSr = destSr;

            // 10. EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // 11. Canvas & UI
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // Setup UI Panels
            UIManager uiManager = canvasObj.AddComponent<UIManager>();
            SetupCanvasUI(canvasObj.transform, uiManager, menuFullSprite, hudTopSprite, hudBottomSprite, vnFont);

            // 12. Audio Manager
            GameObject audioObj = new GameObject("AudioManager");
            AudioManager audioMgr = audioObj.AddComponent<AudioManager>();
            audioMgr.bgmSource = audioObj.AddComponent<AudioSource>();
            audioMgr.sfxSource = audioObj.AddComponent<AudioSource>();

            audioMgr.sfxClick = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_ui_click.wav");
            audioMgr.sfxReady = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_ready.wav");
            audioMgr.sfxReveal = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_reveal.wav");
            audioMgr.sfxThrow = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_throw.wav");
            audioMgr.sfxHit = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_hit_bop.wav");
            audioMgr.sfxCountdownTick = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_countdown_tick.wav");
            audioMgr.sfxWin = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_win.wav");
            audioMgr.sfxLose = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/sfx_lose.wav");
            audioMgr.bgmMenu = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/bgm_menu_loop.wav");
            audioMgr.bgmMatch = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/bgm_match_loop.wav");

            // 13. Match State Machine
            GameObject matchObj = new GameObject("MatchStateMachine");
            MatchStateMachine sm = matchObj.AddComponent<MatchStateMachine>();
            sm.uiManager = uiManager;
            sm.aimController = aimController;
            sm.playerActor = playerActor;
            sm.botActor = botActor;
            sm.projectilePrefab = projPrefab;

            // 14. Save Scene
            string scenePath = "Assets/Scenes/MainMatch.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log("Sample Scene with full authentic UI and arena movement saved to " + scenePath);

            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = scenes;
            AssetDatabase.SaveAssets();
        }

        private static GameObject CreateActorObject(string name, ActorType type, string actorName,
            Color ringColor, Sprite idle, Sprite move, Sprite aim, Sprite throwW, Sprite throwR, Sprite hit,
            Sprite win, Sprite defeat, Sprite shadow, Sprite ring, Sprite weaponTip, Sprite aimGlow)
        {
            GameObject obj = new GameObject(name);
            ActorView view = obj.AddComponent<ActorView>();
            view.actorId = (type == ActorType.Player) ? "player" : "bot";
            view.actorName = actorName;
            view.actorType = type;
            view.ringColor = ringColor;

            view.spriteIdle = idle;
            view.spriteMove = move;
            view.spriteAim = aim;
            view.spriteThrowWindup = throwW;
            view.spriteThrowRelease = throwR;
            view.spriteHit = hit;
            view.spriteVictory = win;
            view.spriteDefeat = defeat;

            // Shadow
            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(obj.transform, false);
            shadowObj.transform.localPosition = new Vector3(0f, -0.62f, 0f);
            shadowObj.transform.localScale = new Vector3(0.9f, 0.45f, 1f);
            SpriteRenderer shadowSr = shadowObj.AddComponent<SpriteRenderer>();
            shadowSr.sprite = shadow;
            shadowSr.color = new Color(0f, 0f, 0f, 0.45f);
            shadowSr.sortingOrder = -3;
            view.shadowRenderer = shadowSr;

            // Selection Ring
            GameObject ringObj = new GameObject("Ring");
            ringObj.transform.SetParent(obj.transform, false);
            ringObj.transform.localPosition = new Vector3(0f, -0.62f, 0f);
            ringObj.transform.localScale = new Vector3(0.9f, 0.55f, 1f);
            SpriteRenderer ringSr = ringObj.AddComponent<SpriteRenderer>();
            ringSr.sprite = ring;
            ringSr.color = ringColor;
            ringSr.sortingOrder = -2;
            view.ringRenderer = ringSr;

            // Character Visual
            GameObject charVisual = new GameObject("CharacterVisual");
            charVisual.transform.SetParent(obj.transform, false);
            charVisual.transform.localPosition = new Vector3(0f, 0f, 0f);
            charVisual.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            SpriteRenderer charSr = charVisual.AddComponent<SpriteRenderer>();
            charSr.sprite = idle;
            charSr.sortingOrder = 0;
            view.characterRenderer = charSr;

            // 1. Aim Beam (Widening Cone)
            GameObject beamObj = new GameObject("AimBeam");
            beamObj.transform.SetParent(obj.transform, false);
            LineRenderer beam = beamObj.AddComponent<LineRenderer>();
            beam.useWorldSpace = true;
            beam.startWidth = 0.35f;
            beam.endWidth = 1.35f;
            beam.material = new Material(Shader.Find("Sprites/Default"));
            beam.startColor = new Color(0.2f, 0.85f, 1f, 0.20f);
            beam.endColor = new Color(0.2f, 0.85f, 1f, 0.48f);
            beam.sortingOrder = 4;
            beam.enabled = false;
            view.aimBeam = beam;

            // 2. Aim Dotted Line
            GameObject lineObj = new GameObject("AimLine");
            lineObj.transform.SetParent(obj.transform, false);
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.startWidth = 0.08f;
            line.endWidth = 0.08f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = Color.white;
            line.endColor = Color.white;
            line.sortingOrder = 5;
            line.enabled = false;
            view.aimLine = line;

            // 3. Weapon Tip Reticle (Slipper + Glow)
            GameObject tipObj = new GameObject("WeaponTip");
            tipObj.transform.SetParent(obj.transform, false);
            tipObj.SetActive(false);

            // Glow under slipper
            GameObject glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(tipObj.transform, false);
            glowObj.transform.localScale = new Vector3(0.85f, 0.55f, 1f);
            SpriteRenderer glowSr = glowObj.AddComponent<SpriteRenderer>();
            glowSr.sprite = aimGlow;
            glowSr.color = new Color(0.25f, 0.88f, 1f, 0.75f);
            glowSr.sortingOrder = 5;
            view.weaponGlowRenderer = glowSr;

            // Blue Slipper icon
            GameObject iconObj = new GameObject("SlipperIcon");
            iconObj.transform.SetParent(tipObj.transform, false);
            iconObj.transform.localScale = new Vector3(0.24f, 0.24f, 1f);
            SpriteRenderer tipSr = iconObj.AddComponent<SpriteRenderer>();
            tipSr.sprite = weaponTip;
            tipSr.sortingOrder = 6;
            view.weaponTip = tipObj.transform;
            view.weaponTipRenderer = tipSr;

            return obj;
        }

        private static void SetupCanvasUI(Transform canvasTr, UIManager ui,
            Sprite menuFullSprite, Sprite hudTopSprite, Sprite hudBottomSprite, Font font)
        {
            Color goldButtonBg = new Color(0.96f, 0.72f, 0.15f, 1f);
            Color darkButtonBg = new Color(0.22f, 0.18f, 0.16f, 1f);

            // ================= 1. MAIN MENU PANEL =================
            GameObject menuPanel = CreateUIPanel("MainMenuPanel", canvasTr, Color.white, true);
            ui.mainMenuPanel = menuPanel;
            Image menuImg = menuPanel.GetComponent<Image>();
            if (menuFullSprite != null) menuImg.sprite = menuFullSprite;
            menuImg.raycastTarget = true;

            // Make the entire main menu tap-to-start, while also keeping dedicated button areas
            Button menuBtn = menuPanel.AddComponent<Button>();
            ui.btnTrainWithBot = menuBtn;

            // Transparent overlay button for "CHƠI NHANH" (Exact center Y = -362f)
            GameObject btnQuickObj = CreateUIButton("BtnQuickMatch", menuPanel.transform, "", 24,
                new Color(1f, 1f, 1f, 0.01f), Color.clear,
                new Vector2(0f, -362f), new Vector2(800f, 160f), font);
            ui.btnQuickMatch = btnQuickObj.GetComponent<Button>();

            // Transparent overlay button for "TẠO PHÒNG" & "VÀO PHÒNG"
            GameObject btnCreateObj = CreateUIButton("BtnCreateRoom", menuPanel.transform, "", 24,
                new Color(1f, 1f, 1f, 0.01f), Color.clear,
                new Vector2(-220f, -490f), new Vector2(400f, 110f), font);
            ui.btnCreateRoom = btnCreateObj.GetComponent<Button>();

            // Notice text for unsupported modes
            Text txtNotice = CreateUIText("TxtNotice", menuPanel.transform, "", 24, FontStyle.Bold, new Color(1f, 0.85f, 0.2f),
                new Vector2(0f, -730f), new Vector2(800f, 60f), font);
            ui.txtNotice = txtNotice;


            // ================= 2. IN-GAME HUD PANEL =================
            GameObject hudPanel = CreateUIPanel("InGameHUDPanel", canvasTr, Color.clear, false);
            ui.inGameHUDPanel = hudPanel;
            RectTransform hudRt = hudPanel.GetComponent<RectTransform>();
            hudRt.anchorMin = Vector2.zero;
            hudRt.anchorMax = Vector2.one;
            hudRt.offsetMin = Vector2.zero;
            hudRt.offsetMax = Vector2.zero;
            hudPanel.GetComponent<Image>().raycastTarget = false;

            // Authentic Top Header Bar
            GameObject topBar = CreateUIPanel("TopBar", hudPanel.transform, Color.white, false);
            RectTransform topBarRt = topBar.GetComponent<RectTransform>();
            topBarRt.anchorMin = new Vector2(0.5f, 1f);
            topBarRt.anchorMax = new Vector2(0.5f, 1f);
            topBarRt.pivot = new Vector2(0.5f, 1f);
            topBarRt.anchoredPosition = new Vector2(0f, 0f);
            topBarRt.sizeDelta = new Vector2(1080f, 456f);
            Image topImg = topBar.GetComponent<Image>();
            if (hudTopSprite != null) topImg.sprite = hudTopSprite;
            topImg.raycastTarget = false;

            // Round Text ("VÒNG 1/5" already on artwork, only update on round > 1)
            Text txtRound = CreateUIText("TxtRound", topBar.transform, "", 28, FontStyle.Bold, new Color(1f, 0.88f, 0.2f),
                new Vector2(-320f, -118f), new Vector2(200f, 50f), font);
            ui.txtRound = txtRound;

            // Countdown Timer (Centered right inside golden clock circle)
            Text txtTimer = CreateUIText("TxtTimer", topBar.transform, "5", 34, FontStyle.Bold, new Color(0.12f, 0.10f, 0.08f),
                new Vector2(0f, -118f), new Vector2(90f, 60f), font);
            ui.txtTimer = txtTimer;

            // Player HP (under BẠN)
            Text txtPlayerHp = CreateUIText("TxtPlayerHP", topBar.transform, "", 22, FontStyle.Bold, new Color(1f, 0.3f, 0.3f),
                new Vector2(-365f, -250f), new Vector2(120f, 60f), font);
            ui.txtPlayerHP = txtPlayerHp;

            // Bot 1 HP (under Bot 1)
            Text txtBotHp = CreateUIText("TxtBotHP", topBar.transform, "", 22, FontStyle.Bold, new Color(1f, 0.3f, 0.3f),
                new Vector2(-220f, -250f), new Vector2(120f, 60f), font);
            ui.txtBotHP = txtBotHp;

            // Phase Banner ("CHỌN HƯỚNG NÉM | Đối thủ đang ẩn" already on artwork)
            Text txtPhase = CreateUIText("TxtPhaseBanner", topBar.transform, "", 24, FontStyle.Bold, new Color(0.2f, 0.95f, 1f),
                new Vector2(0f, -360f), new Vector2(650f, 50f), font);
            ui.txtPhaseBanner = txtPhase;


            // Authentic Bottom Toolbar
            GameObject bottomBar = CreateUIPanel("BottomBar", hudPanel.transform, Color.white, false);
            RectTransform bottomBarRt = bottomBar.GetComponent<RectTransform>();
            bottomBarRt.anchorMin = new Vector2(0.5f, 0f);
            bottomBarRt.anchorMax = new Vector2(0.5f, 0f);
            bottomBarRt.pivot = new Vector2(0.5f, 0f);
            bottomBarRt.anchoredPosition = new Vector2(0f, 0f);
            bottomBarRt.sizeDelta = new Vector2(1080f, 474f);
            Image bottomImg = bottomBar.GetComponent<Image>();
            if (hudBottomSprite != null) bottomImg.sprite = hudBottomSprite;
            bottomImg.raycastTarget = false;

            // SẴN SÀNG Button (positioned directly over the golden button, Y = 220f inside bottomBar)
            GameObject btnReadyObj = CreateUIButton("BtnReady", bottomBar.transform, "", 28,
                new Color(1f, 1f, 1f, 0.01f), Color.black,
                new Vector2(60f, 220f), new Vector2(440f, 150f), font);
            ui.btnReady = btnReadyObj.GetComponent<Button>();

            // Overlay indicator for "ĐÃ KHÓA" when ready
            Text txtReadyBtn = CreateUIText("TxtReadyBtn", btnReadyObj.transform, "ĐÃ KHÓA", 32, FontStyle.Bold, new Color(0.2f, 0.9f, 1f),
                Vector2.zero, new Vector2(400f, 100f), font);
            txtReadyBtn.gameObject.SetActive(false);
            ui.txtReadyBtn = txtReadyBtn;


            // ================= 3. RESULT PANEL =================
            GameObject resPanel = CreateUIPanel("ResultPanel", canvasTr, new Color(0.05f, 0.04f, 0.03f, 0.92f), true);
            ui.resultPanel = resPanel;

            Text txtResTitle = CreateUIText("TxtResultTitle", resPanel.transform, "CHIẾN THẮNG!", 54, FontStyle.Bold, new Color(1f, 0.85f, 0.2f),
                new Vector2(0f, 460f), new Vector2(800f, 90f), font);
            ui.txtResultTitle = txtResTitle;

            Text txtResSub = CreateUIText("TxtResultSubtitle", resPanel.transform, "Bạn đã hạ gục đối thủ xuất sắc!", 26, FontStyle.Normal, Color.white,
                new Vector2(0f, 380f), new Vector2(800f, 60f), font);
            ui.txtResultSubtitle = txtResSub;

            GameObject statsCard = CreateUIPanel("StatsCard", resPanel.transform, new Color(0.12f, 0.09f, 0.08f, 0.95f), false);
            SetRectTransform(statsCard.GetComponent<RectTransform>(), new Vector2(0f, 140f), new Vector2(750f, 340f));

            Text txtPStats = CreateUIText("TxtPlayerStats", statsCard.transform, "", 26, FontStyle.Normal, new Color(0.2f, 0.9f, 1f),
                new Vector2(0f, 70f), new Vector2(700f, 110f), font);
            ui.txtPlayerStats = txtPStats;

            Text txtBStats = CreateUIText("TxtBotStats", statsCard.transform, "", 26, FontStyle.Normal, new Color(1f, 0.5f, 0.5f),
                new Vector2(0f, -70f), new Vector2(700f, 110f), font);
            ui.txtBotStats = txtBStats;

            Sprite btnPrimaryGold = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/btn_primary_gold_9slice.png");
            Sprite btnSecondaryWood = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/btn_secondary_wood_9slice.png");

            GameObject btnRematchObj = CreateUIButton("BtnRematch", resPanel.transform, "CHƠI LẠI", 36, goldButtonBg, Color.black,
                new Vector2(0f, -170f), new Vector2(650f, 110f), font, btnPrimaryGold);
            ui.btnRematch = btnRematchObj.GetComponent<Button>();

            GameObject btnHomeObj = CreateUIButton("BtnBackToHome", resPanel.transform, "VỀ TRANG CHỦ", 30, darkButtonBg, Color.white,
                new Vector2(0f, -310f), new Vector2(650f, 100f), font, btnSecondaryWood);
            ui.btnBackToHome = btnHomeObj.GetComponent<Button>();
        }

        private static GameObject CreateUIPanel(string name, Transform parent, Color color, bool fullScreen)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Image img = obj.AddComponent<Image>();
            img.color = color;
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (fullScreen)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            return obj;
        }

        private static Text CreateUIText(string name, Transform parent, string text, int fontSize,
            FontStyle style, Color color, Vector2 pos, Vector2 size, Font font, TextAnchor align = TextAnchor.MiddleCenter)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Text txt = obj.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = align;
            txt.font = font;
            RectTransform rt = obj.GetComponent<RectTransform>();
            SetRectTransform(rt, pos, size);
            return txt;
        }

        private static GameObject CreateUIButton(string name, Transform parent, string label, int fontSize,
            Color bgColor, Color textColor, Vector2 pos, Vector2 size, Font font, Sprite slicedSprite = null)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Image img = obj.AddComponent<Image>();
            if (slicedSprite != null)
            {
                img.sprite = slicedSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = bgColor;
            }
            Button btn = obj.AddComponent<Button>();

            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(bgColor.r * 1.15f, bgColor.g * 1.15f, bgColor.b * 1.15f, 1f);
            cb.pressedColor = new Color(bgColor.r * 0.8f, bgColor.g * 0.8f, bgColor.b * 0.8f, 1f);
            btn.colors = cb;

            RectTransform rt = obj.GetComponent<RectTransform>();
            SetRectTransform(rt, pos, size);

            if (!string.IsNullOrEmpty(label))
            {
                CreateUIText("Label", obj.transform, label, fontSize, FontStyle.Bold, textColor, Vector2.zero, size, font);
            }
            return obj;
        }

        private static void SetRectTransform(RectTransform rt, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;
        }
    }
}
