using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HitMe.Core;
using HitMe.Gameplay;
using HitMe.AimKit;

namespace HitMe.Editor
{
    public static class UIAndArenaMasterSetup
    {
        private const string MainMatchPath = "Assets/Scenes/MainMatch.unity";
        private const string KitPath = "Assets/Art/UI/Kit/";
        private const string EvidenceDir = "Evidence/phase2";

        private static Image SetupTextBackground(GameObject textObj, Sprite sprite, Image.Type type = Image.Type.Sliced, Color? color = null)
        {
            if (textObj == null) return null;
            Transform parent = textObj.transform.parent;
            if (parent == null) return null;

            // Remove any legacy child BgImage that might obscure text
            var legacyChild = textObj.transform.Find("BgImage");
            if (legacyChild != null) Object.DestroyImmediate(legacyChild.gameObject);

            string bgName = textObj.name + "_Bg";
            Transform bgTr = parent.Find(bgName);
            GameObject bgObj;
            if (bgTr == null)
            {
                bgObj = new GameObject(bgName, typeof(RectTransform), typeof(Image));
                bgObj.transform.SetParent(parent, false);
            }
            else
            {
                bgObj = bgTr.gameObject;
            }

            // Put bgObj immediately before textObj in hierarchy so text renders on top
            int textIndex = textObj.transform.GetSiblingIndex();
            int bgIndex = bgObj.transform.GetSiblingIndex();
            if (bgIndex > textIndex)
            {
                bgObj.transform.SetSiblingIndex(textIndex);
            }

            // Sync RectTransform with textObj
            var textRt = textObj.GetComponent<RectTransform>();
            var bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = textRt.anchorMin;
            bgRt.anchorMax = textRt.anchorMax;
            bgRt.pivot = textRt.pivot;
            bgRt.anchoredPosition = textRt.anchoredPosition;
            bgRt.sizeDelta = textRt.sizeDelta;

            var img = bgObj.GetComponent<Image>();
            img.sprite = sprite;
            img.type = type;
            img.color = color ?? Color.white;
            img.raycastTarget = false;

            return img;
        }

        private static Text EnsureButtonLabel(GameObject btnObj, Font font, string text, int fontSize, Color textColor)
        {
            if (btnObj == null) return null;
            Transform labelTr = btnObj.transform.Find("Text") ?? btnObj.transform.Find("Label");
            Text txt;
            if (labelTr == null)
            {
                var labelObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
                labelObj.transform.SetParent(btnObj.transform, false);
                var rt = labelObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                txt = labelObj.GetComponent<Text>();
            }
            else
            {
                txt = labelTr.GetComponent<Text>();
                if (txt == null) txt = labelTr.gameObject.AddComponent<Text>();
            }

            txt.font = font;
            txt.text = text;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.color = textColor;
            txt.alignment = TextAnchor.MiddleCenter;
            return txt;
        }

        [MenuItem("Tools/HIT ME/1. Configure UI Sprite Borders")]
        public static void ConfigureUISpriteBorders()
        {
            var borders = new (string name, Vector4 border)[]
            {
                ("ui_banner_wood", new Vector4(64, 36, 64, 36)),
                ("ui_btn_cta_yellow", new Vector4(56, 44, 56, 40)),
                ("ui_btn_cta_blue", new Vector4(48, 40, 48, 36)),
                ("ui_btn_cta_green", new Vector4(48, 40, 48, 36)),
                ("ui_btn_cta_red", new Vector4(56, 44, 56, 40)),
                ("ui_panel_dark_gold", new Vector4(48, 48, 48, 48)),
                ("ui_hud_pill_dark", new Vector4(48, 36, 48, 36)),
                ("ui_input_box_cyan", new Vector4(32, 32, 32, 32)),
                ("ui_card_slot_selected_cyan", new Vector4(32, 32, 32, 32)),
                ("ui_card_slot_normal", new Vector4(32, 32, 32, 32))
            };

            foreach (var item in borders)
            {
                string path = KitPath + item.name + ".png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spriteBorder = item.border;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                    Debug.Log($"[UI Setup] Configured 9-slice for {item.name}: border={item.border}");
                }
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/HIT ME/2. Rebuild UI, Arena & Characters in MainMatch")]
        public static void RebuildMainMatch()
        {
            ConfigureUISpriteBorders();

            Scene scene = EditorSceneManager.OpenScene(MainMatchPath, OpenSceneMode.Single);

            // ==========================================
            // 1. CAMERA & BACKGROUND
            // ==========================================
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0f, 0.15f, -10f);
                cam.transform.rotation = Quaternion.identity;
                cam.orthographic = true;
                cam.orthographicSize = 7.2f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.08f, 0.05f, 0.04f, 1f);

                var camCtrl = cam.GetComponent<CameraController25D>();
                if (camCtrl != null)
                {
                    camCtrl.LockToArenaOverview = true;
                    camCtrl.OrthographicSize = 7.2f;
                    camCtrl.Target = null;
                }
            }

            // ==========================================
            // 2. ARENA & CONTEXT (ĐÌNH LÀNG VIỆT NAM)
            // ==========================================
            var arenaFloor = GameObject.Find("ArenaFloor");
            if (arenaFloor != null)
            {
                var sr = arenaFloor.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ARENA.png");
                    sr.sortingOrder = -20;
                }
                arenaFloor.transform.position = new Vector3(0f, 0.15f, 0f);
                arenaFloor.transform.localScale = new Vector3(1.22f, 1.22f, 1f);
            }

            var boundary = GameObject.Find("ArenaBoundary");
            if (boundary != null) boundary.SetActive(false); // ARENA.png already has courtyard boundaries

            var lotus = GameObject.Find("ArenaCenterLotus");
            if (lotus != null) lotus.SetActive(false); // ARENA.png already has center lotus medallion

            // ==========================================
            // 3. CHARACTERS (STANDARDIZED & CLEAN FEET)
            // ==========================================
            var shadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/procedural_shadow.png");
            var ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/procedural_foot_ring.png");

            // Player (Nam Làng)
            var playerObj = GameObject.Find("PlayerActor");
            if (playerObj != null)
            {
                playerObj.transform.position = new Vector3(0f, -3.2f, 0f);
                playerObj.transform.localScale = new Vector3(0.7235f, 0.7235f, 1f); // 1.50u height standard

                var actorView = playerObj.GetComponent<ActorView>();
                if (actorView != null)
                {
                    actorView.actorName = "Nam Làng (BẠN)";
                    actorView.equippedWeaponIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/dep_to_ong_icon.png");

                    // Connect HitMeAimVisual2D
                    var existingAim = playerObj.transform.Find("HITME_AimVisual");
                    if (existingAim == null)
                    {
                        var aimPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/HITME_AimKit/Generated/HITME_AimVisual.prefab");
                        if (aimPrefab != null)
                        {
                            var inst = (GameObject)PrefabUtility.InstantiatePrefab(aimPrefab);
                            inst.name = "HITME_AimVisual";
                            inst.transform.SetParent(playerObj.transform, false);
                            inst.transform.localPosition = Vector3.zero;
                            actorView.aimVisual = inst.GetComponent<HitMeAimVisual2D>();
                        }
                    }
                    else
                    {
                        actorView.aimVisual = existingAim.GetComponent<HitMeAimVisual2D>();
                    }

                    // Disable legacy lines
                    if (actorView.aimLine != null) actorView.aimLine.enabled = false;
                    if (actorView.aimBeam != null) actorView.aimBeam.enabled = false;
                    if (actorView.weaponTip != null) actorView.weaponTip.gameObject.SetActive(false);
                }

                var pShadow = playerObj.transform.Find("Shadow");
                if (pShadow != null)
                {
                    var sr = pShadow.GetComponent<SpriteRenderer>();
                    if (sr != null) { sr.sprite = shadowSprite; sr.sortingOrder = 1; }
                    pShadow.localScale = new Vector3(0.66f, 0.66f, 1f);
                }

                var pRing = playerObj.transform.Find("Ring");
                if (pRing != null)
                {
                    var sr = pRing.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = ringSprite;
                        sr.color = new Color(0.15f, 0.85f, 1f, 0.95f); // Cyan
                        sr.sortingOrder = 2;
                    }
                    pRing.localScale = new Vector3(0.66f, 0.66f, 1f);
                }

                var pVis = playerObj.transform.Find("CharacterVisual");
                if (pVis != null)
                {
                    var sr = pVis.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.sortingOrder = 5;
                }
            }

            // Bot (Anh Nón Lá / Bot Luyện Tập)
            var botObj = GameObject.Find("BotActor");
            if (botObj != null)
            {
                botObj.transform.position = new Vector3(0f, 2.5f, 0f);
                botObj.transform.localScale = new Vector3(0.7235f, 0.7235f, 1f);

                var actorView = botObj.GetComponent<ActorView>();
                if (actorView != null)
                {
                    actorView.actorName = "Anh Nón Lá (Bot)";
                    actorView.equippedWeaponIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/dep_to_ong_icon.png");
                }

                var bShadow = botObj.transform.Find("Shadow");
                if (bShadow != null)
                {
                    var sr = bShadow.GetComponent<SpriteRenderer>();
                    if (sr != null) { sr.sprite = shadowSprite; sr.sortingOrder = 1; }
                    bShadow.localScale = new Vector3(0.66f, 0.66f, 1f);
                }

                var bRing = botObj.transform.Find("Ring");
                if (bRing != null)
                {
                    var sr = bRing.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = ringSprite;
                        sr.color = new Color(1f, 0.35f, 0.25f, 0.95f); // Coral
                        sr.sortingOrder = 2;
                    }
                    bRing.localScale = new Vector3(0.66f, 0.66f, 1f);
                }

                var bVis = botObj.transform.Find("CharacterVisual");
                if (bVis != null)
                {
                    var sr = bVis.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        var botIdle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_idle.png");
                        if (botIdle != null)
                        {
                            sr.sprite = botIdle;
                            sr.color = Color.white;
                            if (actorView != null)
                            {
                                actorView.spriteIdle = botIdle;
                                actorView.spriteMove = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_move.png");
                                actorView.spriteAim = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_aim.png");
                                actorView.spriteThrowWindup = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_throw_windup.png");
                                actorView.spriteThrowRelease = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_throw_release.png");
                                actorView.spriteHit = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_hit.png");
                                actorView.spriteVictory = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_victory.png");
                                actorView.spriteDefeat = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/anh_non_la_defeat.png");
                            }
                        }
                        sr.sortingOrder = 5;
                    }
                }
            }

            // ==========================================
            // 4. UI CANVAS SCALER & PANELS STYLING
            // ==========================================
            var canvasObj = GameObject.Find("Canvas");
            if (canvasObj == null) return;

            var canvas = canvasObj.GetComponent<Canvas>();
            if (canvas != null && cam != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 10f;
                canvas.sortingOrder = 100;
            }

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/HitMeFont.ttf");
            var woodBanner = AssetDatabase.LoadAssetAtPath<Sprite>(KitPath + "ui_banner_wood.png");
            var btnYellow = AssetDatabase.LoadAssetAtPath<Sprite>(KitPath + "ui_btn_cta_yellow.png");
            var btnBlue = AssetDatabase.LoadAssetAtPath<Sprite>(KitPath + "ui_btn_cta_blue.png");
            var btnGreen = AssetDatabase.LoadAssetAtPath<Sprite>(KitPath + "ui_btn_cta_green.png");
            var panelGold = AssetDatabase.LoadAssetAtPath<Sprite>(KitPath + "ui_panel_dark_gold.png");
            var hudPill = AssetDatabase.LoadAssetAtPath<Sprite>(KitPath + "ui_hud_pill_dark.png");
            var timerGold = AssetDatabase.LoadAssetAtPath<Sprite>(KitPath + "ui_timer_circle_gold.png");
            var cardSlot = AssetDatabase.LoadAssetAtPath<Sprite>(KitPath + "ui_card_slot_selected_cyan.png");
            var inputBox = AssetDatabase.LoadAssetAtPath<Sprite>(KitPath + "ui_input_box_cyan.png");
            var depIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/dep_to_ong_icon.png");

            // --- A. MAIN MENU PANEL ---
            var menuTr = canvasObj.transform.Find("MainMenuPanel");
            if (menuTr != null)
            {
                var menuPanel = menuTr.gameObject;
                var mImg = menuPanel.GetComponent<Image>();
                if (mImg != null)
                {
                    mImg.sprite = null;
                    mImg.color = new Color(0.04f, 0.03f, 0.02f, 0.65f); // Rich dark backdrop over Arena
                }

                // Wood Title Banner
                var titleTr = menuTr.Find("TitleBanner");
                GameObject titleObj;
                if (titleTr == null)
                {
                    titleObj = new GameObject("TitleBanner", typeof(RectTransform), typeof(Image));
                    titleObj.transform.SetParent(menuTr, false);
                    titleObj.transform.SetSiblingIndex(0);
                }
                else
                {
                    titleObj = titleTr.gameObject;
                }
                var titleRt = titleObj.GetComponent<RectTransform>();
                titleRt.anchorMin = new Vector2(0.5f, 1f);
                titleRt.anchorMax = new Vector2(0.5f, 1f);
                titleRt.pivot = new Vector2(0.5f, 1f);
                titleRt.anchoredPosition = new Vector2(0f, -80f);
                titleRt.sizeDelta = new Vector2(720f, 180f);

                var titleImg = titleObj.GetComponent<Image>();
                titleImg.sprite = woodBanner;
                titleImg.type = Image.Type.Sliced;

                // Title Text
                var titleTextTr = titleObj.transform.Find("Text");
                GameObject titleTextObj;
                if (titleTextTr == null)
                {
                    titleTextObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
                    titleTextObj.transform.SetParent(titleObj.transform, false);
                }
                else
                {
                    titleTextObj = titleTextTr.gameObject;
                }
                var tt = titleTextObj.GetComponent<Text>();
                tt.font = font;
                tt.text = "HIT ME VN\n<size=24>CHIẾN ĐÌNH LÀNG</size>";
                tt.alignment = TextAnchor.MiddleCenter;
                tt.fontSize = 44;
                tt.fontStyle = FontStyle.Bold;
                tt.color = new Color(1f, 0.92f, 0.4f); // Golden yellow
                var ttRt = titleTextObj.GetComponent<RectTransform>();
                ttRt.anchorMin = Vector2.zero;
                ttRt.anchorMax = Vector2.one;
                ttRt.offsetMin = Vector2.zero;
                ttRt.offsetMax = Vector2.zero;

                // BtnQuickMatch
                var btnQuickTr = menuTr.Find("BtnQuickMatch");
                if (btnQuickTr != null)
                {
                    var btnQuick = btnQuickTr.gameObject;
                    var img = btnQuick.GetComponent<Image>();
                    if (img != null) { img.sprite = btnYellow; img.type = Image.Type.Sliced; img.color = Color.white; }
                    var rt = btnQuick.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0f);
                    rt.anchorMax = new Vector2(0.5f, 0f);
                    rt.pivot = new Vector2(0.5f, 0f);
                    rt.anchoredPosition = new Vector2(0f, 260f);
                    rt.sizeDelta = new Vector2(580f, 130f);

                    EnsureButtonLabel(btnQuick, font, "CHƠI VỚI BOT", 36, new Color(0.2f, 0.12f, 0.05f));
                }

                // BtnCreateRoom
                var btnRoomTr = menuTr.Find("BtnCreateRoom");
                if (btnRoomTr != null)
                {
                    var btnRoom = btnRoomTr.gameObject;
                    var img = btnRoom.GetComponent<Image>();
                    if (img != null) { img.sprite = btnBlue; img.type = Image.Type.Sliced; img.color = Color.white; }
                    var rt = btnRoom.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0f);
                    rt.anchorMax = new Vector2(0.5f, 0f);
                    rt.pivot = new Vector2(0.5f, 0f);
                    rt.anchoredPosition = new Vector2(0f, 110f);
                    rt.sizeDelta = new Vector2(580f, 120f);

                    EnsureButtonLabel(btnRoom, font, "TẠO PHÒNG (SẮP RA MẮT)", 28, Color.white);
                }
            }

            // --- B. IN-GAME HUD PANEL ---
            var hudTr = canvasObj.transform.Find("InGameHUDPanel");
            if (hudTr != null)
            {
                var hudPanel = hudTr.gameObject;
                var hImg = hudPanel.GetComponent<Image>();
                if (hImg != null) hImg.color = Color.clear;

                // TopBar
                var topBarTr = hudTr.Find("TopBar");
                if (topBarTr != null)
                {
                    var topBar = topBarTr.gameObject;
                    var rt = topBar.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(0.5f, 1f);
                    rt.anchoredPosition = new Vector2(0f, -40f);
                    rt.sizeDelta = new Vector2(0f, 260f);

                    var topImg = topBar.GetComponent<Image>();
                    if (topImg != null)
                    {
                        topImg.sprite = null;
                        topImg.color = Color.clear;
                    }

                    // Player HP Pill
                    var pHPTr = topBarTr.Find("TxtPlayerHP");
                    if (pHPTr != null)
                    {
                        var playerHp = pHPTr.gameObject;
                        var hprt = playerHp.GetComponent<RectTransform>();
                        hprt.anchorMin = new Vector2(0f, 1f);
                        hprt.anchorMax = new Vector2(0f, 1f);
                        hprt.pivot = new Vector2(0f, 1f);
                        hprt.anchoredPosition = new Vector2(30f, -20f);
                        hprt.sizeDelta = new Vector2(340f, 85f);

                        SetupTextBackground(playerHp, hudPill, Image.Type.Sliced);

                        var txt = playerHp.GetComponent<Text>();
                        if (txt != null)
                        {
                            txt.font = font;
                            txt.fontSize = 28;
                            txt.fontStyle = FontStyle.Bold;
                            txt.color = new Color(0.15f, 0.95f, 1f); // Bright Cyan
                            txt.alignment = TextAnchor.MiddleCenter;
                            txt.text = "BẠN: ♥ ♥ ♥";
                        }
                    }

                    // Bot HP Pill
                    var bHPTr = topBarTr.Find("TxtBotHP");
                    if (bHPTr != null)
                    {
                        var botHp = bHPTr.gameObject;
                        var hprt = botHp.GetComponent<RectTransform>();
                        hprt.anchorMin = new Vector2(1f, 1f);
                        hprt.anchorMax = new Vector2(1f, 1f);
                        hprt.pivot = new Vector2(1f, 1f);
                        hprt.anchoredPosition = new Vector2(-30f, -20f);
                        hprt.sizeDelta = new Vector2(340f, 85f);

                        SetupTextBackground(botHp, hudPill, Image.Type.Sliced);

                        var txt = botHp.GetComponent<Text>();
                        if (txt != null)
                        {
                            txt.font = font;
                            txt.fontSize = 28;
                            txt.fontStyle = FontStyle.Bold;
                            txt.color = new Color(1f, 0.45f, 0.4f); // Bright Coral
                            txt.alignment = TextAnchor.MiddleCenter;
                            txt.text = "BOT: ♥ ♥ ♥";
                        }
                    }

                    // Timer Badge
                    var timerTr = topBarTr.Find("TxtTimer");
                    if (timerTr != null)
                    {
                        var timer = timerTr.gameObject;
                        var trt = timer.GetComponent<RectTransform>();
                        trt.anchorMin = new Vector2(0.5f, 1f);
                        trt.anchorMax = new Vector2(0.5f, 1f);
                        trt.pivot = new Vector2(0.5f, 1f);
                        trt.anchoredPosition = new Vector2(0f, -10f);
                        trt.sizeDelta = new Vector2(140f, 140f);

                        SetupTextBackground(timer, timerGold, Image.Type.Simple);

                        var txt = timer.GetComponent<Text>();
                        if (txt != null)
                        {
                            txt.font = font;
                            txt.fontSize = 48;
                            txt.fontStyle = FontStyle.Bold;
                            txt.color = new Color(0.22f, 0.12f, 0.04f); // Dark Gold
                            txt.alignment = TextAnchor.MiddleCenter;
                            txt.text = "5";
                        }
                    }

                    // Phase Banner Pill
                    var bannerTr = topBarTr.Find("TxtPhaseBanner");
                    if (bannerTr != null)
                    {
                        var banner = bannerTr.gameObject;
                        var brt = banner.GetComponent<RectTransform>();
                        brt.anchorMin = new Vector2(0.5f, 1f);
                        brt.anchorMax = new Vector2(0.5f, 1f);
                        brt.pivot = new Vector2(0.5f, 1f);
                        brt.anchoredPosition = new Vector2(0f, -165f);
                        brt.sizeDelta = new Vector2(620f, 70f);

                        SetupTextBackground(banner, inputBox, Image.Type.Sliced);

                        var txt = banner.GetComponent<Text>();
                        if (txt != null)
                        {
                            txt.font = font;
                            txt.fontSize = 26;
                            txt.fontStyle = FontStyle.Bold;
                            txt.color = new Color(0.2f, 0.95f, 1f); // Cyan
                            txt.alignment = TextAnchor.MiddleCenter;
                            txt.text = "CHỌN VỊ TRÍ & NGẮM NÉM (5s)";
                        }
                    }
                }

                // BottomBar
                var bottomBarTr = hudTr.Find("BottomBar");
                if (bottomBarTr != null)
                {
                    var bottomBar = bottomBarTr.gameObject;
                    var bRt = bottomBar.GetComponent<RectTransform>();
                    bRt.anchorMin = new Vector2(0f, 0f);
                    bRt.anchorMax = new Vector2(1f, 0f);
                    bRt.pivot = new Vector2(0.5f, 0f);
                    bRt.anchoredPosition = new Vector2(0f, 0f);
                    bRt.sizeDelta = new Vector2(0f, 280f);

                    var bImg = bottomBar.GetComponent<Image>();
                    if (bImg != null)
                    {
                        bImg.sprite = null;
                        bImg.color = Color.clear;
                    }

                    // Weapon Slot Card
                    var slotTr = bottomBarTr.Find("WeaponCardSlot");
                    GameObject slotObj;
                    if (slotTr == null)
                    {
                        slotObj = new GameObject("WeaponCardSlot", typeof(RectTransform), typeof(Image));
                        slotObj.transform.SetParent(bottomBarTr, false);
                    }
                    else
                    {
                        slotObj = slotTr.gameObject;
                    }
                    var sRt = slotObj.GetComponent<RectTransform>();
                    sRt.anchorMin = new Vector2(0f, 0f);
                    sRt.anchorMax = new Vector2(0f, 0f);
                    sRt.pivot = new Vector2(0f, 0f);
                    sRt.anchoredPosition = new Vector2(40f, 40f);
                    sRt.sizeDelta = new Vector2(220f, 220f);

                    var sImg = slotObj.GetComponent<Image>();
                    sImg.sprite = cardSlot;
                    sImg.type = Image.Type.Sliced;
                    sImg.color = Color.white;

                    // Weapon Icon Inside Slot
                    var iconTr = slotObj.transform.Find("Icon");
                    GameObject iconObj;
                    if (iconTr == null)
                    {
                        iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                        iconObj.transform.SetParent(slotObj.transform, false);
                    }
                    else
                    {
                        iconObj = iconTr.gameObject;
                    }
                    var iRt = iconObj.GetComponent<RectTransform>();
                    iRt.anchorMin = new Vector2(0.5f, 0.5f);
                    iRt.anchorMax = new Vector2(0.5f, 0.5f);
                    iRt.pivot = new Vector2(0.5f, 0.5f);
                    iRt.anchoredPosition = new Vector2(0f, 15f);
                    iRt.sizeDelta = new Vector2(130f, 130f);

                    var iImg = iconObj.GetComponent<Image>();
                    iImg.sprite = depIcon;
                    iImg.preserveAspect = true;

                    // Weapon Title Inside Slot
                    var wNameTr = slotObj.transform.Find("Title");
                    GameObject wNameObj;
                    if (wNameTr == null)
                    {
                        wNameObj = new GameObject("Title", typeof(RectTransform), typeof(Text));
                        wNameObj.transform.SetParent(slotObj.transform, false);
                    }
                    else
                    {
                        wNameObj = wNameTr.gameObject;
                    }
                    var wRt = wNameObj.GetComponent<RectTransform>();
                    wRt.anchorMin = new Vector2(0f, 0f);
                    wRt.anchorMax = new Vector2(1f, 0f);
                    wRt.pivot = new Vector2(0.5f, 0f);
                    wRt.anchoredPosition = new Vector2(0f, 12f);
                    wRt.sizeDelta = new Vector2(0f, 36f);

                    var wTxt = wNameObj.GetComponent<Text>();
                    wTxt.font = font;
                    wTxt.text = "Dép Tổ Ong";
                    wTxt.fontSize = 20;
                    wTxt.fontStyle = FontStyle.Bold;
                    wTxt.color = new Color(0.2f, 0.95f, 1f);
                    wTxt.alignment = TextAnchor.MiddleCenter;

                    // BtnReady
                    var btnReadyTr = bottomBarTr.Find("BtnReady");
                    if (btnReadyTr != null)
                    {
                        var btnReady = btnReadyTr.gameObject;
                        var img = btnReady.GetComponent<Image>();
                        if (img != null) { img.sprite = btnGreen; img.type = Image.Type.Sliced; img.color = Color.white; }
                        var rt = btnReady.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(1f, 0f);
                        rt.anchorMax = new Vector2(1f, 0f);
                        rt.pivot = new Vector2(1f, 0f);
                        rt.anchoredPosition = new Vector2(-40f, 40f);
                        rt.sizeDelta = new Vector2(340f, 120f);

                        EnsureButtonLabel(btnReady, font, "SẴN SÀNG!", 34, Color.white);
                    }
                }
            }

            // --- C. RESULT PANEL ---
            var resTr = canvasObj.transform.Find("ResultPanel");
            if (resTr != null)
            {
                var resPanel = resTr.gameObject;
                var pImg = resPanel.GetComponent<Image>();
                if (pImg != null)
                {
                    pImg.color = new Color(0.04f, 0.03f, 0.02f, 0.85f); // Backdrop
                }

                var statsCardTr = resTr.Find("StatsCard");
                if (statsCardTr != null)
                {
                    var statsCard = statsCardTr.gameObject;
                    var cImg = statsCard.GetComponent<Image>() ?? statsCard.AddComponent<Image>();
                    cImg.sprite = panelGold;
                    cImg.type = Image.Type.Sliced;
                    cImg.color = Color.white;

                    var cRt = statsCard.GetComponent<RectTransform>();
                    cRt.anchorMin = new Vector2(0.5f, 0.5f);
                    cRt.anchorMax = new Vector2(0.5f, 0.5f);
                    cRt.pivot = new Vector2(0.5f, 0.5f);
                    cRt.anchoredPosition = new Vector2(0f, 60f);
                    cRt.sizeDelta = new Vector2(760f, 380f);

                    var pStatsTr = statsCardTr.Find("TxtPlayerStats");
                    if (pStatsTr != null)
                    {
                        var rt = pStatsTr.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(0.5f, 0.5f);
                        rt.anchorMax = new Vector2(0.5f, 0.5f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.anchoredPosition = new Vector2(0f, 60f);
                        rt.sizeDelta = new Vector2(680f, 120f);
                        var txt = pStatsTr.GetComponent<Text>();
                        if (txt != null)
                        {
                            txt.font = font;
                            txt.fontSize = 28;
                            txt.fontStyle = FontStyle.Bold;
                            txt.color = new Color(0.2f, 0.9f, 1f);
                            txt.alignment = TextAnchor.MiddleCenter;
                            txt.text = "Bạn (Nam Làng)\nSố lượt ném: 5 | Trúng: 3\nTỉ lệ: 60.0%";
                        }
                    }

                    var bStatsTr = statsCardTr.Find("TxtBotStats");
                    if (bStatsTr != null)
                    {
                        var rt = bStatsTr.GetComponent<RectTransform>();
                        rt.anchorMin = new Vector2(0.5f, 0.5f);
                        rt.anchorMax = new Vector2(0.5f, 0.5f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.anchoredPosition = new Vector2(0f, -60f);
                        rt.sizeDelta = new Vector2(680f, 120f);
                        var txt = bStatsTr.GetComponent<Text>();
                        if (txt != null)
                        {
                            txt.font = font;
                            txt.fontSize = 28;
                            txt.fontStyle = FontStyle.Bold;
                            txt.color = new Color(1f, 0.45f, 0.35f);
                            txt.alignment = TextAnchor.MiddleCenter;
                            txt.text = "Bot Luyện Tập\nSố lượt ném: 5 | Trúng: 1\nTỉ lệ: 20.0%";
                        }
                    }
                }

                // Result Title Banner
                var titleTxtTr = resTr.Find("TxtResultTitle");
                if (titleTxtTr != null)
                {
                    var titleTxt = titleTxtTr.gameObject;
                    var tRt = titleTxt.GetComponent<RectTransform>();
                    tRt.anchorMin = new Vector2(0.5f, 0.5f);
                    tRt.anchorMax = new Vector2(0.5f, 0.5f);
                    tRt.pivot = new Vector2(0.5f, 0.5f);
                    tRt.anchoredPosition = new Vector2(0f, 440f);
                    tRt.sizeDelta = new Vector2(680f, 150f);

                    SetupTextBackground(titleTxt, woodBanner, Image.Type.Sliced);

                    var txt = titleTxt.GetComponent<Text>();
                    if (txt != null)
                    {
                        txt.font = font;
                        txt.fontSize = 46;
                        txt.fontStyle = FontStyle.Bold;
                        txt.color = new Color(1f, 0.88f, 0.35f);
                        txt.alignment = TextAnchor.MiddleCenter;
                        txt.text = "CHIẾN THẮNG!";
                    }
                }

                // Subtitle
                var subTxtTr = resTr.Find("TxtResultSubtitle");
                if (subTxtTr != null)
                {
                    var subTxt = subTxtTr.gameObject;
                    var sRt = subTxt.GetComponent<RectTransform>();
                    sRt.anchorMin = new Vector2(0.5f, 0.5f);
                    sRt.anchorMax = new Vector2(0.5f, 0.5f);
                    sRt.pivot = new Vector2(0.5f, 0.5f);
                    sRt.anchoredPosition = new Vector2(0f, 330f);
                    sRt.sizeDelta = new Vector2(650f, 60f);

                    var txt = subTxt.GetComponent<Text>();
                    if (txt != null)
                    {
                        txt.font = font;
                        txt.fontSize = 26;
                        txt.fontStyle = FontStyle.Normal;
                        txt.color = Color.white;
                        txt.alignment = TextAnchor.MiddleCenter;
                        txt.text = "Bạn đã hạ gục đối thủ xuất sắc!";
                    }
                }

                // BtnRematch
                var btnRematchTr = resTr.Find("BtnRematch");
                if (btnRematchTr != null)
                {
                    var btnRematch = btnRematchTr.gameObject;
                    var img = btnRematch.GetComponent<Image>();
                    if (img != null) { img.sprite = btnYellow; img.type = Image.Type.Sliced; img.color = Color.white; }
                    var rt = btnRematch.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(0f, -200f);
                    rt.sizeDelta = new Vector2(560f, 110f);

                    EnsureButtonLabel(btnRematch, font, "ĐẤU LẠI", 34, new Color(0.2f, 0.12f, 0.05f));
                }

                // BtnBackToHome
                var btnHomeTr = resTr.Find("BtnBackToHome");
                if (btnHomeTr != null)
                {
                    var btnHome = btnHomeTr.gameObject;
                    var img = btnHome.GetComponent<Image>();
                    if (img != null) { img.sprite = btnBlue; img.type = Image.Type.Sliced; img.color = Color.white; }
                    var rt = btnHome.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(0f, -330f);
                    rt.sizeDelta = new Vector2(560f, 100f);

                    EnsureButtonLabel(btnHome, font, "VỀ MENU", 30, Color.white);
                }
            }

            // Set default runtime panel states
            if (menuTr != null) menuTr.gameObject.SetActive(true);
            if (hudTr != null) hudTr.gameObject.SetActive(false);
            if (resTr != null) resTr.gameObject.SetActive(false);

            EditorSceneManager.SaveScene(scene, MainMatchPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[MasterSetup] Successfully rebuilt MainMatch scene with true Arena, characters, and UI Kit!");
        }

        [MenuItem("Tools/HIT ME/3. Capture Rebuilt MainMatch States")]
        public static void CaptureRebuiltStates()
        {
            Scene scene = EditorSceneManager.OpenScene(MainMatchPath, OpenSceneMode.Single);
            Camera cam = Camera.main;
            if (cam == null) return;

            cam.transform.position = new Vector3(0f, 0.15f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.orthographic = true;
            cam.orthographicSize = 7.2f;

            var camCtrl = cam.GetComponent<CameraController25D>();
            if (camCtrl != null)
            {
                camCtrl.LockToArenaOverview = true;
                camCtrl.OrthographicSize = 7.2f;
                camCtrl.Target = null;
            }

            var canvasObj = GameObject.Find("Canvas");
            if (canvasObj != null)
            {
                var canvas = canvasObj.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = cam;
                    canvas.planeDistance = 10f;
                    canvas.sortingOrder = 100;
                }
            }

            Directory.CreateDirectory(EvidenceDir);

            var menuTr = canvasObj != null ? canvasObj.transform.Find("MainMenuPanel") : null;
            var hudTr = canvasObj != null ? canvasObj.transform.Find("InGameHUDPanel") : null;
            var resTr = canvasObj != null ? canvasObj.transform.Find("ResultPanel") : null;

            var playerObj = GameObject.Find("PlayerActor");
            var botObj = GameObject.Find("BotActor");

            ActorView playerView = playerObj != null ? playerObj.GetComponent<ActorView>() : null;

            int w = 1080;
            int h = 1920;

            // 1. Capture Main Menu State
            if (menuTr != null) menuTr.gameObject.SetActive(true);
            if (hudTr != null) hudTr.gameObject.SetActive(false);
            if (resTr != null) resTr.gameObject.SetActive(false);
            if (playerObj != null) playerObj.SetActive(true);
            if (botObj != null) botObj.SetActive(false); // only player on menu
            if (playerView != null && playerView.aimVisual != null) playerView.aimVisual.Hide();
            Canvas.ForceUpdateCanvases();
            CaptureCameraToFile(cam, w, h, Path.Combine(EvidenceDir, "mainmatch_menu_rebuilt.png"));

            // 2. Capture In-Game HUD + Aiming Gameplay State
            if (menuTr != null) menuTr.gameObject.SetActive(false);
            if (hudTr != null) hudTr.gameObject.SetActive(true);
            if (resTr != null) resTr.gameObject.SetActive(false);
            if (playerObj != null) playerObj.SetActive(true);
            if (botObj != null) botObj.SetActive(true);
            if (playerView != null && playerView.aimVisual != null)
            {
                Vector2 footPos = (Vector2)playerObj.transform.position + new Vector2(0f, -0.45f);
                playerView.aimVisual.ShowAim(footPos, (Vector2.up + Vector2.right * 0.35f).normalized, 3.8f, playerView.equippedWeaponIcon);
            }
            Canvas.ForceUpdateCanvases();
            CaptureCameraToFile(cam, w, h, Path.Combine(EvidenceDir, "mainmatch_gameplay_rebuilt.png"));

            // 3. Capture Result Screen State
            if (menuTr != null) menuTr.gameObject.SetActive(false);
            if (hudTr != null) hudTr.gameObject.SetActive(false);
            if (resTr != null) resTr.gameObject.SetActive(true);
            if (playerView != null && playerView.aimVisual != null) playerView.aimVisual.Hide();
            Canvas.ForceUpdateCanvases();
            CaptureCameraToFile(cam, w, h, Path.Combine(EvidenceDir, "mainmatch_result_rebuilt.png"));

            // Restore initial scene state
            if (menuTr != null) menuTr.gameObject.SetActive(true);
            if (hudTr != null) hudTr.gameObject.SetActive(false);
            if (resTr != null) resTr.gameObject.SetActive(false);
            if (playerObj != null) playerObj.SetActive(true);
            if (botObj != null) botObj.SetActive(true);

            EditorSceneManager.SaveScene(scene, MainMatchPath);
            Debug.Log("[MasterSetup] Successfully captured all rebuilt MainMatch states.");
        }

        [MenuItem("Tools/HIT ME/4. Rebuild And Capture All")]
        public static void RebuildAndCaptureAll()
        {
            RebuildMainMatch();
            CaptureRebuiltStates();
        }

        private static void CaptureCameraToFile(Camera cam, int width, int height, string filePath)
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            RenderTexture prevRt = RenderTexture.active;
            RenderTexture prevCamRt = cam.targetTexture;

            try
            {
                cam.targetTexture = rt;
                RenderTexture.active = rt;
                cam.Render();

                Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();

                byte[] pngBytes = tex.EncodeToPNG();
                File.WriteAllBytes(filePath, pngBytes);
                Object.DestroyImmediate(tex);
                Debug.Log($"[MasterSetup] Captured frame to {filePath}");
            }
            finally
            {
                cam.targetTexture = prevCamRt;
                RenderTexture.active = prevRt;
                RenderTexture.ReleaseTemporary(rt);
            }
        }
    }
}
