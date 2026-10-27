using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using HitMe.Presentation;

namespace HitMe.EditorTools
{
    public static class VisualPreviewSceneBuilder
    {
        private const string ScenePath = "Assets/HITME/Scenes/VisualPreview.unity";
        private const string PrefabsDir = "Assets/HITME/Prefabs/Presentation";

        [MenuItem("HIT ME/Build Visual Preview Scene & Prefabs")]
        public static void BuildVisualPreviewScene()
        {
            EnsureDirectories();

            // 1. Create a pristine new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Camera Setup (iPhone 15 Portrait optimized 2.5D / Orthographic)
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.4f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.086f, 0.102f, 0.133f, 1f); // #161A22
            camera.transform.position = new Vector3(0f, 0f, -10f);

            // 3. Directional Light (Warm Village Sunlight matching Blender 5.2 setup)
            var lightGo = new GameObject("Warm Sunlight");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.957f, 0.839f); // #FFF4D6
            light.intensity = 1.35f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 4. EventSystem
            var eventGo = new GameObject("EventSystem");
            eventGo.AddComponent<EventSystem>();
            eventGo.AddComponent<StandaloneInputModule>();

            // 5. Main Canvas (iPhone 15 Portrait: 1179 x 2556)
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1179, 2556);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // 6. Build MainMenuPanel
            var mainMenuGo = BuildMainMenuPanel(canvasGo.transform);

            // 7. Build ArenaStagingPanel (Background and Environmental Layers)
            var arenaGo = BuildArenaStaging(cameraGo.transform.parent);

            // 8. Build InMatchHUDPanel
            var hudGo = BuildInMatchHUDPanel(canvasGo.transform);
            hudGo.SetActive(false); // Initially hidden, active in InMatchHUD mode

            // 9. Root VisualPreviewManager with VisualPreviewController
            var managerGo = new GameObject("VisualPreviewManager");
            var controller = managerGo.AddComponent<VisualPreviewController>();

            // Wire SerializedProperties on VisualPreviewController
            var serializedCtrl = new SerializedObject(controller);
            serializedCtrl.FindProperty("_mainMenuContainer").objectReferenceValue = mainMenuGo;
            serializedCtrl.FindProperty("_arenaStagingContainer").objectReferenceValue = arenaGo;
            serializedCtrl.FindProperty("_inMatchHUDContainer").objectReferenceValue = hudGo;
            serializedCtrl.FindProperty("_mainMenuPresenter").objectReferenceValue = mainMenuGo.GetComponent<MainMenuPresenter>();
            serializedCtrl.FindProperty("_hudPresenter").objectReferenceValue = hudGo.GetComponent<InMatchHUDPresenter>();

            // Wire Character showcase sprites
            var depSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/HITME/Art/Characters/NamLang/namlang_dep_showcase.png");
            var chaoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/HITME/Art/Characters/NamLang/namlang_chao_showcase.png");
            var votSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/HITME/Art/Characters/NamLang/namlang_vot_showcase.png");

            var charSpritesProp = serializedCtrl.FindProperty("_characterShowcaseSprites");
            charSpritesProp.arraySize = 3;
            charSpritesProp.GetArrayElementAtIndex(0).objectReferenceValue = depSprite;
            charSpritesProp.GetArrayElementAtIndex(1).objectReferenceValue = chaoSprite;
            charSpritesProp.GetArrayElementAtIndex(2).objectReferenceValue = votSprite;

            var charNamesProp = serializedCtrl.FindProperty("_characterNames");
            charNamesProp.arraySize = 3;
            charNamesProp.GetArrayElementAtIndex(0).stringValue = "Nam Làng (Dép)";
            charNamesProp.GetArrayElementAtIndex(1).stringValue = "Nam Làng (Chảo)";
            charNamesProp.GetArrayElementAtIndex(2).stringValue = "Nam Làng (Vợt)";

            // Wire Weapons
            var depIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/HITME/Art/Weapons/weapon_dep.png");
            var chaoIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/HITME/Art/Weapons/weapon_chao.png");
            var votIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/HITME/Art/Weapons/weapon_vot.png");

            var weaponIconsProp = serializedCtrl.FindProperty("_weaponIcons");
            weaponIconsProp.arraySize = 3;
            weaponIconsProp.GetArrayElementAtIndex(0).objectReferenceValue = depIcon;
            weaponIconsProp.GetArrayElementAtIndex(1).objectReferenceValue = chaoIcon;
            weaponIconsProp.GetArrayElementAtIndex(2).objectReferenceValue = votIcon;

            var weaponNamesProp = serializedCtrl.FindProperty("_weaponNames");
            weaponNamesProp.arraySize = 3;
            weaponNamesProp.GetArrayElementAtIndex(0).stringValue = "Dép Tổ Ong";
            weaponNamesProp.GetArrayElementAtIndex(1).stringValue = "Chảo Gang";
            weaponNamesProp.GetArrayElementAtIndex(2).stringValue = "Vợt Pickleball";

            serializedCtrl.ApplyModifiedProperties();

            // 10. Save Prefabs for Codex & Project Integration
            PrefabUtility.SaveAsPrefabAsset(mainMenuGo, $"{PrefabsDir}/MainMenuPanel.prefab");
            PrefabUtility.SaveAsPrefabAsset(hudGo, $"{PrefabsDir}/InMatchHUDPanel.prefab");

            // 11. Save Scene
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[VisualPreviewSceneBuilder] Successfully built and saved:\n- {ScenePath}\n- {PrefabsDir}/MainMenuPanel.prefab\n- {PrefabsDir}/InMatchHUDPanel.prefab");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists("Assets/HITME/Scenes"))
                Directory.CreateDirectory("Assets/HITME/Scenes");

            if (!Directory.Exists(PrefabsDir))
                Directory.CreateDirectory(PrefabsDir);
        }

        private static GameObject BuildMainMenuPanel(Transform parent)
        {
            var panel = new GameObject("MainMenuPanel", typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            var rt = (RectTransform)panel.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var presenter = panel.AddComponent<MainMenuPresenter>();
            var sPresenter = new SerializedObject(presenter);

            // Top Bar
            var topBar = CreateUIElement("TopProfileBar", panel.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -140f), new Vector2(0f, 80f));
            
            // Name Text
            var nameGo = CreateText("PlayerNameText", topBar.transform, "Nam Làng", 28, TextAnchor.MiddleLeft, new Vector2(160f, 20f), new Vector2(300f, 40f));
            sPresenter.FindProperty("_playerNameText").objectReferenceValue = nameGo.GetComponent<Text>();

            // Coin Text
            var coinGo = CreateText("CoinText", topBar.transform, "1.250", 26, TextAnchor.MiddleCenter, new Vector2(700f, 20f), new Vector2(160f, 40f));
            sPresenter.FindProperty("_coinBalanceText").objectReferenceValue = coinGo.GetComponent<Text>();

            // Character Showcase Area
            var showcaseGo = CreateUIElement("CharacterShowcase", panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(600f, 700f));
            var charImg = showcaseGo.AddComponent<Image>();
            charImg.preserveAspect = true;
            sPresenter.FindProperty("_characterShowcaseImage").objectReferenceValue = charImg;

            // Character Name Text
            var charNameGo = CreateText("SelectedCharName", panel.transform, "Nam Làng", 32, TextAnchor.MiddleCenter, new Vector2(0f, -220f), new Vector2(400f, 50f));
            sPresenter.FindProperty("_selectedCharacterNameText").objectReferenceValue = charNameGo.GetComponent<Text>();

            // Action Buttons
            var playBtnGo = CreateButton("QuickPlayButton", panel.transform, "CHƠI NHANH", new Vector2(0f, -520f), new Vector2(460f, 120f));
            sPresenter.FindProperty("_quickPlayButton").objectReferenceValue = playBtnGo.GetComponent<Button>();

            var botBtnGo = CreateButton("TrainWithBotButton", panel.transform, "LUYỆN VỚI BOT", new Vector2(0f, -660f), new Vector2(460f, 85f));
            sPresenter.FindProperty("_trainWithBotButton").objectReferenceValue = botBtnGo.GetComponent<Button>();

            var createRoomBtnGo = CreateButton("CreateRoomButton", panel.transform, "TẠO PHÒNG", new Vector2(-125f, -760f), new Vector2(210f, 80f));
            sPresenter.FindProperty("_createRoomButton").objectReferenceValue = createRoomBtnGo.GetComponent<Button>();

            var joinRoomBtnGo = CreateButton("JoinRoomButton", panel.transform, "VÀO PHÒNG", new Vector2(125f, -760f), new Vector2(210f, 80f));
            sPresenter.FindProperty("_joinRoomButton").objectReferenceValue = joinRoomBtnGo.GetComponent<Button>();

            // Toast Panel
            var toastGo = CreateUIElement("ToastPanel", panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 80f));
            var toastTxt = CreateText("ToastText", toastGo.transform, "Thông báo", 24, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(460f, 60f));
            sPresenter.FindProperty("_toastPanel").objectReferenceValue = toastGo;
            sPresenter.FindProperty("_toastText").objectReferenceValue = toastTxt.GetComponent<Text>();
            toastGo.SetActive(false);

            sPresenter.ApplyModifiedProperties();
            return panel;
        }

        private static GameObject BuildInMatchHUDPanel(Transform parent)
        {
            var panel = new GameObject("InMatchHUDPanel", typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            var rt = (RectTransform)panel.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var presenter = panel.AddComponent<InMatchHUDPresenter>();
            var sPresenter = new SerializedObject(presenter);

            // Top Bar
            var topBar = CreateUIElement("TopHUDBar", panel.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -140f), new Vector2(0f, 100f));
            var roundGo = CreateText("RoundText", topBar.transform, "LƯỢT 1/20", 28, TextAnchor.MiddleCenter, new Vector2(0f, 20f), new Vector2(260f, 40f));
            var timerGo = CreateText("TimerText", topBar.transform, "05s", 36, TextAnchor.MiddleCenter, new Vector2(0f, -25f), new Vector2(160f, 50f));
            sPresenter.FindProperty("_roundText").objectReferenceValue = roundGo.GetComponent<Text>();
            sPresenter.FindProperty("_timerText").objectReferenceValue = timerGo.GetComponent<Text>();

            // Player status
            var playerHearts = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                var h = CreateUIElement($"Heart_P_{i}", topBar.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(80f + i * 45f, 0f), new Vector2(40f, 40f));
                h.AddComponent<Image>();
                playerHearts[i] = h;
            }
            var playerHeartsProp = sPresenter.FindProperty("_playerHeartIcons");
            playerHeartsProp.arraySize = 3;
            for (int i = 0; i < 3; i++) playerHeartsProp.GetArrayElementAtIndex(i).objectReferenceValue = playerHearts[i].GetComponent<Image>();

            // Opponent status
            var opponentHearts = new GameObject[3];
            for (int i = 0; i < 3; i++)
            {
                var h = CreateUIElement($"Heart_O_{i}", topBar.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-160f + i * 45f, 0f), new Vector2(40f, 40f));
                h.AddComponent<Image>();
                opponentHearts[i] = h;
            }
            var opponentHeartsProp = sPresenter.FindProperty("_opponentHeartIcons");
            opponentHeartsProp.arraySize = 3;
            for (int i = 0; i < 3; i++) opponentHeartsProp.GetArrayElementAtIndex(i).objectReferenceValue = opponentHearts[i].GetComponent<Image>();

            // Bottom HUD
            var readyBtnGo = CreateButton("ReadyButton", panel.transform, "KHÓA LƯỢT", new Vector2(0f, -1000f), new Vector2(360f, 90f));
            sPresenter.FindProperty("_readyButton").objectReferenceValue = readyBtnGo.GetComponent<Button>();

            // Aim Indicator Ruler Transform
            var rulerGo = CreateUIElement("AimIndicatorRuler", panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 300f));
            sPresenter.FindProperty("_aimRulerTransform").objectReferenceValue = rulerGo.GetComponent<RectTransform>();

            sPresenter.ApplyModifiedProperties();
            return panel;
        }

        private static GameObject BuildArenaStaging(Transform parent)
        {
            var arena = new GameObject("ArenaStaging");
            if (parent != null) arena.transform.SetParent(parent, false);

            // Ground seamless tile
            var groundGo = new GameObject("ArenaGround");
            groundGo.transform.SetParent(arena.transform, false);
            var sr = groundGo.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/HITME/Art/Environment/ground_tile_seamless.png");
            sr.sortingOrder = 0;
            groundGo.transform.position = new Vector3(0f, -2.5f, 0f);

            // Flag with SpriteSequenceAnimator
            var flagGo = new GameObject("AnimatedFlag");
            flagGo.transform.SetParent(arena.transform, false);
            var flagSr = flagGo.AddComponent<SpriteRenderer>();
            flagSr.sortingOrder = 5;
            var flagAnim = flagGo.AddComponent<SpriteSequenceAnimator>();
            flagGo.transform.position = new Vector3(2.5f, 1.8f, 0f);

            var sAnim = new SerializedObject(flagAnim);
            sAnim.FindProperty("_framesPerSecond").floatValue = 14f;
            sAnim.FindProperty("_loop").boolValue = true;
            sAnim.FindProperty("_spriteRenderer").objectReferenceValue = flagSr;

            var flagFramesProp = sAnim.FindProperty("_frames");
            flagFramesProp.arraySize = 16;
            for (int i = 0; i < 16; i++)
            {
                var f = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/HITME/Art/Animation/flag_wave/flag_wave_f{i:00}.png");
                flagFramesProp.GetArrayElementAtIndex(i).objectReferenceValue = f;
            }
            sAnim.ApplyModifiedProperties();

            return arena;
        }

        private static GameObject CreateUIElement(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return go;
        }

        private static GameObject CreateText(string name, Transform parent, string content, int fontSize, TextAnchor alignment, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = CreateUIElement(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPos, sizeDelta);
            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.color = Color.white;
            return go;
        }

        private static GameObject CreateButton(string name, Transform parent, string label, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = CreateUIElement(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPos, sizeDelta);
            go.AddComponent<Image>();
            var btn = go.AddComponent<Button>();
            CreateText("Label", go.transform, label, 28, TextAnchor.MiddleCenter, Vector2.zero, sizeDelta);
            return go;
        }
    }
}
