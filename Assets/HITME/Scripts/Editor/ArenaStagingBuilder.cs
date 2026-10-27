using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace HitMe.Editor
{
    public static class ArenaStagingBuilder
    {
        private const string StagingScenePath = "Assets/Scenes/ArenaVisual_Staging.unity";
        private const string EvidenceDir = "D:/HITME GAME/docs/evidence";

        [MenuItem("Tools/HIT ME/Visual Sprint 01/Build Staging and Render All")]
        public static void BuildStagingAndRenderAll()
        {
            Debug.Log("[ArenaStagingBuilder] Starting Sprint 01 Visual Staging build...");
            ConfigureImportSettings();
            EnsureSortingLayers();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // 1. Setup Camera
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 6.4f;
            camGo.transform.position = new Vector3(0f, -0.2f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(14, 22, 34, 255); // Rich dark navy

            // 2. Setup EventSystem
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // 3. Build Layers
            var combatFloorGo = BuildCombatFloor();
            var environmentGo = BuildEnvironment();
            var actorsGo = BuildActorsPlaceholder();
            var hudCanvasGo = BuildInMatchHUD(cam);
            var mainMenuCanvasGo = BuildMainMenu(cam);

            // 4. Save Prefabs
            SaveStagingPrefabs(combatFloorGo, environmentGo, hudCanvasGo, mainMenuCanvasGo);

            // 5. Save Scene
            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, StagingScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ArenaStagingBuilder] Saved scene to {StagingScenePath}");

            // 6. Capture 4 Unity-Rendered Screenshots
            Directory.CreateDirectory(EvidenceDir);
            CaptureVisualScreenshots(cam, combatFloorGo, environmentGo, actorsGo, hudCanvasGo, mainMenuCanvasGo);

            Debug.Log("[ArenaStagingBuilder] Sprint 01 Visual Staging build and render COMPLETE!");
        }

        private static void ConfigureImportSettings()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/HITME/Art" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                bool dirty = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    dirty = true;
                }
                if (importer.mipmapEnabled) { importer.mipmapEnabled = false; dirty = true; }
                if (!importer.alphaIsTransparency) { importer.alphaIsTransparency = true; dirty = true; }
                if (importer.wrapMode != TextureWrapMode.Clamp) { importer.wrapMode = TextureWrapMode.Clamp; dirty = true; }
                if (importer.filterMode != FilterMode.Bilinear) { importer.filterMode = FilterMode.Bilinear; dirty = true; }

                // 9-slice borders
                if (path.Contains("btn_primary_yellow")) { importer.spriteBorder = new Vector4(45, 45, 45, 45); dirty = true; }
                else if (path.Contains("btn_blue") || path.Contains("btn_green") || path.Contains("btn_navy")) { importer.spriteBorder = new Vector4(35, 35, 35, 35); dirty = true; }
                else if (path.Contains("panel_card_dark")) { importer.spriteBorder = new Vector4(35, 35, 35, 35); dirty = true; }
                else if (path.Contains("dock_bg")) { importer.spriteBorder = new Vector4(35, 35, 35, 35); dirty = true; }
                else if (path.Contains("pill_coin")) { importer.spriteBorder = new Vector4(20, 20, 20, 20); dirty = true; }
                else if (path.Contains("panel_profile_strip")) { importer.spriteBorder = new Vector4(25, 25, 25, 25); dirty = true; }

                if (dirty)
                {
                    importer.SaveAndReimport();
                }
            }
            AssetDatabase.Refresh();
        }

        private static void EnsureSortingLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var sortingLayers = tagManager.FindProperty("m_SortingLayers");
            string[] required = { "Default", "FarBackground", "EnvironmentBack", "CombatFloor", "ArenaBorder", "CenterDecal", "SideDecor", "Actors", "Foreground", "HUD" };
            
            var existing = new System.Collections.Generic.HashSet<string>();
            for (int i = 0; i < sortingLayers.arraySize; i++)
            {
                existing.Add(sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue);
            }

            foreach (var req in required)
            {
                if (!existing.Contains(req))
                {
                    sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
                    var entry = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
                    entry.FindPropertyRelative("name").stringValue = req;
                    entry.FindPropertyRelative("uniqueID").intValue = sortingLayers.arraySize * 100 + 77;
                }
            }
            tagManager.ApplyModifiedProperties();
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Font LoadVietnameseFont()
        {
            return AssetDatabase.LoadAssetAtPath<Font>("Assets/HITME/Fonts/BeVietnamPro-Bold.ttf");
        }

        // ==========================================
        // 1. COMBAT FLOOR (VÙNG CHƠI THẬT)
        // ==========================================
        private static GameObject BuildCombatFloor()
        {
            var root = new GameObject("[01_CombatFloor]");
            root.transform.position = Vector3.zero;

            // Combat Floor dimensions: 4.5 wide, 8.2 high, centered at (0, -0.4, 0)
            float floorW = 4.5f;
            float floorH = 8.2f;
            float centerY = -0.4f;

            // Floor Base: Clean stone tile texture tiled across combat area
            var tileSprite = LoadSprite("Assets/HITME/Art/Environment/HITME_A07_Stone_Tile_Floor.png");
            var floorGo = new GameObject("StoneTileFloor");
            floorGo.transform.SetParent(root.transform, false);
            floorGo.transform.position = new Vector3(0f, centerY, 0f);
            var srFloor = floorGo.AddComponent<SpriteRenderer>();
            srFloor.sprite = tileSprite;
            srFloor.sortingLayerName = "CombatFloor";
            srFloor.sortingOrder = 0;
            // Native size of sprite ~ 3.37 x 2.62 -> scale to fill 4.5 x 8.2
            float sx = floorW / (tileSprite != null ? tileSprite.bounds.size.x : 3.37f);
            float sy = floorH / (tileSprite != null ? tileSprite.bounds.size.y : 2.62f);
            floorGo.transform.localScale = new Vector3(sx, sy, 1f);
            srFloor.color = new Color(0.92f, 0.94f, 0.96f, 1f);

            // Weathering & subtle stone crack overlay
            var weathSprite = LoadSprite("Assets/HITME/Art/Environment/HITME_A10_Weathering_Detail.png");
            if (weathSprite != null)
            {
                var weathGo = new GameObject("WeatheringDetail");
                weathGo.transform.SetParent(root.transform, false);
                weathGo.transform.position = new Vector3(0f, centerY, -0.02f);
                var srW = weathGo.AddComponent<SpriteRenderer>();
                srW.sprite = weathSprite;
                srW.sortingLayerName = "CombatFloor";
                srW.sortingOrder = 1;
                srW.color = new Color(1f, 1f, 1f, 0.45f);
                srW.transform.localScale = new Vector3(sx * 0.9f, sy * 0.9f, 1f);
            }

            // Center Lotus Decal: Authentic Vietnamese red/gold lotus emblem (Proportional, NOT full screen!)
            var lotusSprite = LoadSprite("Assets/HITME/Art/Environment/HITME_A09_Lotus_Emblem.png");
            if (lotusSprite != null)
            {
                var lotusGo = new GameObject("CenterLotusDecal");
                lotusGo.transform.SetParent(root.transform, false);
                lotusGo.transform.position = new Vector3(0f, centerY, -0.05f);
                var srLotus = lotusGo.AddComponent<SpriteRenderer>();
                srLotus.sprite = lotusSprite;
                srLotus.sortingLayerName = "CenterDecal";
                srLotus.sortingOrder = 5;
                // Scale to ~ 1.8 units wide (leaving player and bot movement areas completely clear!)
                float lScale = 1.8f / lotusSprite.bounds.size.x;
                lotusGo.transform.localScale = new Vector3(lScale, lScale, 1f);
                srLotus.color = new Color(1f, 1f, 1f, 0.88f);
            }

            // Arena Perimeter Border (Distinct stone curb boundary)
            var borderSprite = LoadSprite("Assets/HITME/Art/Environment/HITME_A08_Arena_Border.png");
            if (borderSprite != null)
            {
                var borderGo = new GameObject("ArenaBorder");
                borderGo.transform.SetParent(root.transform, false);
                borderGo.transform.position = new Vector3(0f, centerY, -0.08f);
                var srBorder = borderGo.AddComponent<SpriteRenderer>();
                srBorder.sprite = borderSprite;
                srBorder.sortingLayerName = "ArenaBorder";
                srBorder.sortingOrder = 10;
                float bx = (floorW + 0.35f) / borderSprite.bounds.size.x;
                float by = (floorH + 0.35f) / borderSprite.bounds.size.y;
                borderGo.transform.localScale = new Vector3(bx, by, 1f);
                srBorder.color = new Color(1f, 1f, 1f, 0.95f);
            }

            // Dividing guideline dashes at centerY
            var divideGo = new GameObject("CenterDividingLine");
            divideGo.transform.SetParent(root.transform, false);
            divideGo.transform.position = new Vector3(0f, centerY, -0.04f);
            var srDiv = divideGo.AddComponent<SpriteRenderer>();
            srDiv.sprite = tileSprite;
            srDiv.sortingLayerName = "CombatFloor";
            srDiv.sortingOrder = 2;
            divideGo.transform.localScale = new Vector3(floorW * 0.95f / (tileSprite != null ? tileSprite.bounds.size.x : 3.37f), 0.04f, 1f);
            srDiv.color = new Color(1f, 0.85f, 0.3f, 0.35f);

            return root;
        }

        // ==========================================
        // 2. ENVIRONMENT (BỐI CẢNH NGOÀI SÂN ĐẤU)
        // ==========================================
        private static GameObject BuildEnvironment()
        {
            var root = new GameObject("[02_Environment]");
            root.transform.position = Vector3.zero;

            // --- A. FAR BACKGROUND (Y > 3.6f) ---
            var farMountains = LoadSprite("Assets/HITME/Art/Environment/HITME_A01_Far_Mountains.png");
            if (farMountains != null)
            {
                var go = new GameObject("FarMountains");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, 4.8f, 5f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = farMountains;
                sr.sortingLayerName = "FarBackground";
                sr.sortingOrder = 0;
                go.transform.localScale = new Vector3(1.35f, 1.35f, 1f);
            }

            var villageRooftops = LoadSprite("Assets/HITME/Art/Environment/HITME_A02_Village_Rooftops.png");
            if (villageRooftops != null)
            {
                var go = new GameObject("VillageRooftops");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, 4.2f, 4f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = villageRooftops;
                sr.sortingLayerName = "EnvironmentBack";
                sr.sortingOrder = 5;
                go.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
            }

            var templeRoof = LoadSprite("Assets/HITME/Art/Environment/HITME_A03_Temple_Roof.png");
            if (templeRoof != null)
            {
                var go = new GameObject("TempleRoof");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, 3.8f, 3f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = templeRoof;
                sr.sortingLayerName = "EnvironmentBack";
                sr.sortingOrder = 10;
                go.transform.localScale = new Vector3(1.15f, 1.15f, 1f);
            }

            var leftPillar = LoadSprite("Assets/HITME/Art/Environment/HITME_A04_Left_Stone_Pillar.png");
            if (leftPillar != null)
            {
                var go = new GameObject("LeftStonePillar");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(-2.2f, 3.1f, 2.5f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = leftPillar;
                sr.sortingLayerName = "EnvironmentBack";
                sr.sortingOrder = 15;
                go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            }

            var rightPillar = LoadSprite("Assets/HITME/Art/Environment/HITME_A05_Right_Stone_Pillar.png");
            if (rightPillar != null)
            {
                var go = new GameObject("RightStonePillar");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(2.2f, 3.1f, 2.5f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = rightPillar;
                sr.sortingLayerName = "EnvironmentBack";
                sr.sortingOrder = 15;
                go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            }

            var bunting = LoadSprite("Assets/HITME/Art/Environment/HITME_B01_Festival_Bunting.png");
            if (bunting != null)
            {
                var go = new GameObject("FestivalBunting");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, 3.4f, 2f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = bunting;
                sr.sortingLayerName = "EnvironmentBack";
                sr.sortingOrder = 20;
                go.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            }

            var flag = LoadSprite("Assets/HITME/Art/Environment/HITME_B02_Vietnamese_Flag.png");
            if (flag != null)
            {
                var go = new GameObject("VietnameseFlag");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(2.45f, 3.45f, 2f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = flag;
                sr.sortingLayerName = "EnvironmentBack";
                sr.sortingOrder = 22;
                go.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            }

            // Hanging lanterns
            var lantern = LoadSprite("Assets/HITME/Art/Environment/HITME_B03_Red_Lantern.png");
            if (lantern != null)
            {
                var l1 = new GameObject("LanternLeft");
                l1.transform.SetParent(root.transform, false);
                l1.transform.position = new Vector3(-1.6f, 3.2f, 1.8f);
                var sr1 = l1.AddComponent<SpriteRenderer>();
                sr1.sprite = lantern;
                sr1.sortingLayerName = "EnvironmentBack";
                sr1.sortingOrder = 25;
                l1.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

                var l2 = new GameObject("LanternRight");
                l2.transform.SetParent(root.transform, false);
                l2.transform.position = new Vector3(1.6f, 3.2f, 1.8f);
                var sr2 = l2.AddComponent<SpriteRenderer>();
                sr2.sprite = lantern;
                sr2.sortingLayerName = "EnvironmentBack";
                sr2.sortingOrder = 25;
                l2.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            }

            // --- B. SIDE MARGINS (X < -2.25f and X > +2.25f) ---
            var treeLeft = LoadSprite("Assets/HITME/Art/Environment/HITME_C01_Left_Tree_Canopy.png");
            if (treeLeft != null)
            {
                var go = new GameObject("LeftTreeCanopy");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(-2.65f, 1.7f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = treeLeft;
                sr.sortingLayerName = "SideDecor";
                sr.sortingOrder = 15;
                go.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            }

            var fenceLeft = LoadSprite("Assets/HITME/Art/Environment/HITME_B08_Bamboo_Fence.png");
            if (fenceLeft != null)
            {
                var go = new GameObject("LeftBambooFence");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(-2.55f, -0.6f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = fenceLeft;
                sr.sortingLayerName = "SideDecor";
                sr.sortingOrder = 14;
                go.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            }

            var drumLeft = LoadSprite("Assets/HITME/Art/Environment/HITME_B06_Festival_Drum.png");
            if (drumLeft != null)
            {
                var go = new GameObject("LeftFestivalDrum");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(-2.48f, -2.6f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = drumLeft;
                sr.sortingLayerName = "SideDecor";
                sr.sortingOrder = 16;
                go.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            }

            var treeRight = LoadSprite("Assets/HITME/Art/Environment/HITME_C02_Right_Tree_Canopy.png");
            if (treeRight != null)
            {
                var go = new GameObject("RightTreeCanopy");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(2.65f, 1.7f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = treeRight;
                sr.sortingLayerName = "SideDecor";
                sr.sortingOrder = 15;
                go.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            }

            var jarsRight = LoadSprite("Assets/HITME/Art/Environment/HITME_B10_Terracotta_Jars.png");
            if (jarsRight != null)
            {
                var go = new GameObject("RightTerracottaJars");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(2.48f, -0.6f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = jarsRight;
                sr.sortingLayerName = "SideDecor";
                sr.sortingOrder = 14;
                go.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            }

            var bananaRight = LoadSprite("Assets/HITME/Art/Environment/HITME_C04_Banana_Bush.png");
            if (bananaRight != null)
            {
                var go = new GameObject("RightBananaBush");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(2.55f, -2.6f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = bananaRight;
                sr.sortingLayerName = "SideDecor";
                sr.sortingOrder = 16;
                go.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            }

            // --- C. FOREGROUND MARGIN (Y < -4.8f) ---
            var lotusPond = LoadSprite("Assets/HITME/Art/Environment/lotus_pond_foreground.png");
            if (lotusPond != null)
            {
                var go = new GameObject("ForegroundLotusPond");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, -5.9f, -2f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = lotusPond;
                sr.sortingLayerName = "Foreground";
                sr.sortingOrder = 50;
                go.transform.localScale = new Vector3(0.32f, 0.32f, 1f);
            }

            return root;
        }

        // ==========================================
        // 3. ACTORS PLACEHOLDER
        // ==========================================
        private static GameObject BuildActorsPlaceholder()
        {
            var root = new GameObject("[03_ActorsPlaceholder]");
            root.transform.position = Vector3.zero;

            // Player: Nam Làng at (0, -3.0, -0.5)
            var namlangSprite = LoadSprite("Assets/HITME/Art/Characters/NamLang/namlang_dep_showcase.png");
            if (namlangSprite != null)
            {
                var go = new GameObject("Player_NamLang");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, -3.0f, -0.5f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = namlangSprite;
                sr.sortingLayerName = "Actors";
                sr.sortingOrder = 30;
                go.transform.localScale = new Vector3(0.46f, 0.46f, 1f);

                // Ground drop shadow
                var shadowGo = new GameObject("GroundShadow");
                shadowGo.transform.SetParent(go.transform, false);
                shadowGo.transform.localPosition = new Vector3(0f, -0.05f, 0.1f);
                var srShadow = shadowGo.AddComponent<SpriteRenderer>();
                srShadow.sprite = LoadSprite("Assets/HITME/Art/Environment/HITME_A10_Weathering_Detail.png");
                srShadow.color = new Color(0f, 0f, 0f, 0.38f);
                shadowGo.transform.localScale = new Vector3(1.2f, 0.35f, 1f);
            }

            // Opponent: Mai Hồng (Bot) at (0, 2.2, -0.5)
            var maihongSprite = LoadSprite("Assets/HITME/Art/Characters/MaiHong/maihong_idle.png");
            if (maihongSprite != null)
            {
                var go = new GameObject("Opponent_Bot");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, 2.2f, -0.5f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = maihongSprite;
                sr.sortingLayerName = "Actors";
                sr.sortingOrder = 30;
                go.transform.localScale = new Vector3(0.55f, 0.55f, 1f);

                // Ground drop shadow
                var shadowGo = new GameObject("GroundShadow");
                shadowGo.transform.SetParent(go.transform, false);
                shadowGo.transform.localPosition = new Vector3(0f, -0.05f, 0.1f);
                var srShadow = shadowGo.AddComponent<SpriteRenderer>();
                srShadow.sprite = LoadSprite("Assets/HITME/Art/Environment/HITME_A10_Weathering_Detail.png");
                srShadow.color = new Color(0f, 0f, 0f, 0.38f);
                shadowGo.transform.localScale = new Vector3(1.1f, 0.32f, 1f);
            }

            // Dotted Cyan Aim Line indicator
            var aimLineGo = new GameObject("AimLineIndicator");
            aimLineGo.transform.SetParent(root.transform, false);
            aimLineGo.transform.position = new Vector3(0f, -2.6f, -0.6f);
            for (int i = 0; i < 7; i++)
            {
                var dot = new GameObject($"Dot_{i}");
                dot.transform.SetParent(aimLineGo.transform, false);
                dot.transform.localPosition = new Vector3(i * 0.14f, i * 0.32f, 0f);
                var srDot = dot.AddComponent<SpriteRenderer>();
                srDot.sprite = LoadSprite("Assets/HITME/Art/UI/Icons/play_outline.png");
                srDot.color = new Color(0f, 0.95f, 1f, 0.75f - i * 0.08f);
                dot.transform.localScale = new Vector3(0.12f, 0.12f, 1f);
            }

            return root;
        }

        // ==========================================
        // 4. IN-MATCH COMPACT HUD
        // ==========================================
        private static GameObject BuildInMatchHUD(Camera cam)
        {
            var canvasGo = new GameObject("[04_InMatchHUD_Canvas]");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1.0f;
            canvas.sortingLayerName = "HUD";
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2340f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var font = LoadVietnameseFont();

            // TOP HUD CONTAINER
            var topHud = new GameObject("TopHUD_Panel");
            topHud.transform.SetParent(canvasGo.transform, false);
            var rtTop = topHud.AddComponent<RectTransform>();
            rtTop.anchorMin = new Vector2(0f, 1f);
            rtTop.anchorMax = new Vector2(1f, 1f);
            rtTop.pivot = new Vector2(0.5f, 1f);
            rtTop.sizeDelta = new Vector2(0f, 180f);
            rtTop.anchoredPosition = new Vector2(0f, -40f);

            // 1. Player Card (Left)
            var playerCard = CreateHudCard(topHud.transform, "PlayerCard", new Vector2(0f, 0.5f), new Vector2(250f, 0f), "BẠN", LoadSprite("Assets/HITME/Art/UI/Portraits/portrait_namlang.png"), font, true);

            // 2. Round & Timer Pill (Center)
            var centerPill = new GameObject("MatchPill");
            centerPill.transform.SetParent(topHud.transform, false);
            var rtPill = centerPill.AddComponent<RectTransform>();
            rtPill.anchorMin = new Vector2(0.5f, 0.5f);
            rtPill.anchorMax = new Vector2(0.5f, 0.5f);
            rtPill.sizeDelta = new Vector2(240f, 85f);
            rtPill.anchoredPosition = Vector2.zero;
            var imgPill = centerPill.AddComponent<Image>();
            imgPill.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/panel_card_dark_gold.png");
            imgPill.type = Image.Type.Sliced;
            imgPill.color = new Color32(18, 30, 48, 240);

            var txtRound = CreateText(centerPill.transform, "TxtRound", "VÒNG 1/5", font, 24, TextAnchor.UpperCenter, new Color32(255, 215, 0, 255));
            txtRound.rectTransform.anchoredPosition = new Vector2(0f, 12f);

            var txtTimer = CreateText(centerPill.transform, "TxtTimer", "⏱ 0:15", font, 20, TextAnchor.LowerCenter, new Color32(0, 240, 255, 255));
            txtTimer.rectTransform.anchoredPosition = new Vector2(0f, -14f);

            // 3. Bot Card (Right)
            var botCard = CreateHudCard(topHud.transform, "BotCard", new Vector2(1f, 0.5f), new Vector2(-250f, 0f), "BOT LUYỆN TẬP", LoadSprite("Assets/HITME/Art/UI/Icons/bot.png"), font, false);

            // BOTTOM HUD CONTAINER
            var bottomHud = new GameObject("BottomHUD_Panel");
            bottomHud.transform.SetParent(canvasGo.transform, false);
            var rtBottom = bottomHud.AddComponent<RectTransform>();
            rtBottom.anchorMin = new Vector2(0f, 0f);
            rtBottom.anchorMax = new Vector2(1f, 0f);
            rtBottom.pivot = new Vector2(0.5f, 0f);
            rtBottom.sizeDelta = new Vector2(0f, 220f);
            rtBottom.anchoredPosition = new Vector2(0f, 40f);

            // Quick Chat Button (Left)
            var btnChat = CreateIconButton(bottomHud.transform, "BtnChat", new Vector2(90f, 100f), LoadSprite("Assets/HITME/Art/UI/Icons/play_outline.png"), 72f);
            
            // Equipped Weapon Slot (Center-Left)
            var weaponSlot = new GameObject("EquippedWeaponSlot");
            weaponSlot.transform.SetParent(bottomHud.transform, false);
            var rtSlot = weaponSlot.AddComponent<RectTransform>();
            rtSlot.anchorMin = new Vector2(0.5f, 0.5f);
            rtSlot.anchorMax = new Vector2(0.5f, 0.5f);
            rtSlot.sizeDelta = new Vector2(150f, 150f);
            rtSlot.anchoredPosition = new Vector2(-120f, 0f);
            var imgSlot = weaponSlot.AddComponent<Image>();
            imgSlot.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/panel_card_dark_gold.png");
            imgSlot.type = Image.Type.Sliced;
            imgSlot.color = new Color32(18, 30, 48, 250);

            var weaponIcon = new GameObject("WeaponIcon");
            weaponIcon.transform.SetParent(weaponSlot.transform, false);
            var rtWIcon = weaponIcon.AddComponent<RectTransform>();
            rtWIcon.sizeDelta = new Vector2(110f, 110f);
            var imgWIcon = weaponIcon.AddComponent<Image>();
            imgWIcon.sprite = LoadSprite("Assets/HITME/Art/UI/Icons/weapon_dep.png");

            var countPill = CreateText(weaponSlot.transform, "CountPill", "∞", font, 24, TextAnchor.LowerRight, new Color32(255, 215, 0, 255));
            countPill.rectTransform.anchoredPosition = new Vector2(40f, -40f);

            // Action CTA Button: SẴN SÀNG (Right)
            var btnReady = new GameObject("BtnReadyAction");
            btnReady.transform.SetParent(bottomHud.transform, false);
            var rtReady = btnReady.AddComponent<RectTransform>();
            rtReady.anchorMin = new Vector2(0.5f, 0.5f);
            rtReady.anchorMax = new Vector2(0.5f, 0.5f);
            rtReady.sizeDelta = new Vector2(320f, 110f);
            rtReady.anchoredPosition = new Vector2(160f, 0f);
            var imgReady = btnReady.AddComponent<Image>();
            imgReady.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/btn_primary_yellow_normal.png");
            imgReady.type = Image.Type.Sliced;
            var btnR = btnReady.AddComponent<Button>();

            var txtReady = CreateText(btnReady.transform, "TxtReady", "SẴN SÀNG", font, 36, TextAnchor.MiddleCenter, new Color32(65, 35, 0, 255));
            txtReady.rectTransform.anchoredPosition = new Vector2(0f, 4f);

            return canvasGo;
        }

        private static GameObject CreateHudCard(Transform parent, string name, Vector2 anchor, Vector2 pos, string label, Sprite avatar, Font font, bool isPlayer)
        {
            var card = new GameObject(name);
            card.transform.SetParent(parent, false);
            var rt = card.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = new Vector2(360f, 110f);
            rt.anchoredPosition = pos;

            var bg = card.AddComponent<Image>();
            bg.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/panel_card_dark_gold.png");
            bg.type = Image.Type.Sliced;
            bg.color = new Color32(18, 28, 44, 240);

            // Avatar
            var avGo = new GameObject("Avatar");
            avGo.transform.SetParent(card.transform, false);
            var rtAv = avGo.AddComponent<RectTransform>();
            rtAv.anchorMin = isPlayer ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
            rtAv.anchorMax = isPlayer ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
            rtAv.sizeDelta = new Vector2(80f, 80f);
            rtAv.anchoredPosition = isPlayer ? new Vector2(50f, 0f) : new Vector2(-50f, 0f);
            var imgAv = avGo.AddComponent<Image>();
            imgAv.sprite = avatar;

            // Name
            var txtName = CreateText(card.transform, "Name", label, font, 22, isPlayer ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight, Color.white);
            txtName.rectTransform.anchoredPosition = isPlayer ? new Vector2(30f, 18f) : new Vector2(-30f, 18f);

            // Hearts
            var txtHearts = CreateText(card.transform, "Hearts", "♥ ♥ ♥", font, 26, isPlayer ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight, new Color32(255, 60, 60, 255));
            txtHearts.rectTransform.anchoredPosition = isPlayer ? new Vector2(30f, -16f) : new Vector2(-30f, -16f);

            return card;
        }

        private static GameObject CreateIconButton(Transform parent, string name, Vector2 pos, Sprite icon, float size)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = pos;
            var img = btnGo.AddComponent<Image>();
            img.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/btn_circle_navy.png");
            var btn = btnGo.AddComponent<Button>();

            if (icon != null)
            {
                var iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(btnGo.transform, false);
                var rtIcon = iconGo.AddComponent<RectTransform>();
                rtIcon.sizeDelta = new Vector2(size * 0.55f, size * 0.55f);
                var imgIcon = iconGo.AddComponent<Image>();
                imgIcon.sprite = icon;
                imgIcon.color = Color.white;
            }
            return btnGo;
        }

        // ==========================================
        // 5. MODULAR MAIN MENU STAGING
        // ==========================================
        private static GameObject BuildMainMenu(Camera cam)
        {
            var canvasGo = new GameObject("[05_MainMenu_Canvas]");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1.0f;
            canvas.sortingLayerName = "HUD";
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2340f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var font = LoadVietnameseFont();

            // Background Village Gate Backdrop
            var bgGo = new GameObject("MenuVillageBackdrop");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var rtBg = bgGo.AddComponent<RectTransform>();
            rtBg.anchorMin = Vector2.zero;
            rtBg.anchorMax = Vector2.one;
            rtBg.sizeDelta = Vector2.zero;
            var imgBg = bgGo.AddComponent<Image>();
            imgBg.sprite = LoadSprite("Assets/HITME/Art/Environment/bg_village_gate_menu.png");
            imgBg.color = new Color(0.85f, 0.9f, 0.95f, 1f);

            // Subtle dark overlay to make UI elements pop
            var dimGo = new GameObject("UiVignetteOverlay");
            dimGo.transform.SetParent(canvasGo.transform, false);
            var rtDim = dimGo.AddComponent<RectTransform>();
            rtDim.anchorMin = Vector2.zero;
            rtDim.anchorMax = Vector2.one;
            rtDim.sizeDelta = Vector2.zero;
            var imgDim = dimGo.AddComponent<Image>();
            imgDim.color = new Color32(10, 18, 30, 95);

            // 1. Top Profile Bar
            var topBar = new GameObject("TopProfileBar");
            topBar.transform.SetParent(canvasGo.transform, false);
            var rtBar = topBar.AddComponent<RectTransform>();
            rtBar.anchorMin = new Vector2(0f, 1f);
            rtBar.anchorMax = new Vector2(1f, 1f);
            rtBar.pivot = new Vector2(0.5f, 1f);
            rtBar.sizeDelta = new Vector2(0f, 130f);
            rtBar.anchoredPosition = new Vector2(0f, -40f);

            // Profile Strip
            var profileStrip = new GameObject("ProfileStrip");
            profileStrip.transform.SetParent(topBar.transform, false);
            var rtStrip = profileStrip.AddComponent<RectTransform>();
            rtStrip.anchorMin = new Vector2(0f, 0.5f);
            rtStrip.anchorMax = new Vector2(0f, 0.5f);
            rtStrip.sizeDelta = new Vector2(380f, 95f);
            rtStrip.anchoredPosition = new Vector2(230f, 0f);
            var imgStrip = profileStrip.AddComponent<Image>();
            imgStrip.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/panel_profile_strip.png");
            imgStrip.type = Image.Type.Sliced;

            var avRing = new GameObject("Avatar");
            avRing.transform.SetParent(profileStrip.transform, false);
            var rtRing = avRing.AddComponent<RectTransform>();
            rtRing.anchorMin = new Vector2(0f, 0.5f);
            rtRing.anchorMax = new Vector2(0f, 0.5f);
            rtRing.sizeDelta = new Vector2(80f, 80f);
            rtRing.anchoredPosition = new Vector2(50f, 0f);
            var imgAv = avRing.AddComponent<Image>();
            imgAv.sprite = LoadSprite("Assets/HITME/Art/UI/Portraits/portrait_namlang.png");

            var txtUser = CreateText(profileStrip.transform, "TxtUser", "Nam Làng", font, 24, TextAnchor.MiddleLeft, Color.white);
            txtUser.rectTransform.anchoredPosition = new Vector2(40f, 12f);

            var txtRank = CreateText(profileStrip.transform, "TxtRank", "⭐ Chiến Thần Làng", font, 18, TextAnchor.MiddleLeft, new Color32(255, 215, 0, 255));
            txtRank.rectTransform.anchoredPosition = new Vector2(40f, -14f);

            // Lotus Coin Pill
            var coinPill = new GameObject("LotusCoinPill");
            coinPill.transform.SetParent(topBar.transform, false);
            var rtCoin = coinPill.AddComponent<RectTransform>();
            rtCoin.anchorMin = new Vector2(1f, 0.5f);
            rtCoin.anchorMax = new Vector2(1f, 0.5f);
            rtCoin.sizeDelta = new Vector2(230f, 80f);
            rtCoin.anchoredPosition = new Vector2(-230f, 0f);
            var imgCoin = coinPill.AddComponent<Image>();
            imgCoin.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/pill_coin.png");
            imgCoin.type = Image.Type.Sliced;

            var coinIcon = new GameObject("CoinIcon");
            coinIcon.transform.SetParent(coinPill.transform, false);
            var rtCIcon = coinIcon.AddComponent<RectTransform>();
            rtCIcon.anchorMin = new Vector2(0f, 0.5f);
            rtCIcon.anchorMax = new Vector2(0f, 0.5f);
            rtCIcon.sizeDelta = new Vector2(50f, 50f);
            rtCIcon.anchoredPosition = new Vector2(38f, 0f);
            var imgCIcon = coinIcon.AddComponent<Image>();
            imgCIcon.sprite = LoadSprite("Assets/HITME/Art/UI/Icons/coin_lotus.png");

            var txtCoins = CreateText(coinPill.transform, "TxtCoins", "12,500", font, 24, TextAnchor.MiddleRight, new Color32(255, 225, 60, 255));
            txtCoins.rectTransform.anchoredPosition = new Vector2(-20f, 0f);

            // Settings Gear Button
            var btnSettings = new GameObject("BtnSettings");
            btnSettings.transform.SetParent(topBar.transform, false);
            var rtSet = btnSettings.AddComponent<RectTransform>();
            rtSet.anchorMin = new Vector2(1f, 0.5f);
            rtSet.anchorMax = new Vector2(1f, 0.5f);
            rtSet.sizeDelta = new Vector2(80f, 80f);
            rtSet.anchoredPosition = new Vector2(-60f, 0f);
            var imgSet = btnSettings.AddComponent<Image>();
            imgSet.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/btn_circle_navy.png");
            var btnS = btnSettings.AddComponent<Button>();

            var setIcon = new GameObject("Icon");
            setIcon.transform.SetParent(btnSettings.transform, false);
            var rtSIcon = setIcon.AddComponent<RectTransform>();
            rtSIcon.sizeDelta = new Vector2(44f, 44f);
            var imgSIcon = setIcon.AddComponent<Image>();
            imgSIcon.sprite = LoadSprite("Assets/HITME/Art/UI/Icons/gear.png");

            // 2. Official 3D HIT ME Logo
            var logoGo = new GameObject("LogoHITME");
            logoGo.transform.SetParent(canvasGo.transform, false);
            var rtLogo = logoGo.AddComponent<RectTransform>();
            rtLogo.anchorMin = new Vector2(0.5f, 1f);
            rtLogo.anchorMax = new Vector2(0.5f, 1f);
            rtLogo.sizeDelta = new Vector2(580f, 230f);
            rtLogo.anchoredPosition = new Vector2(0f, -220f);
            var imgLogo = logoGo.AddComponent<Image>();
            imgLogo.sprite = LoadSprite("Assets/HITME/Art/UI/Logo/logo_hit_me.png");

            // 3. Character Showcase (Nam Làng standing on sculpted pedestal)
            var pedestalGo = new GameObject("ShowcasePedestal");
            pedestalGo.transform.SetParent(canvasGo.transform, false);
            var rtPed = pedestalGo.AddComponent<RectTransform>();
            rtPed.anchorMin = new Vector2(0.5f, 0.5f);
            rtPed.anchorMax = new Vector2(0.5f, 0.5f);
            rtPed.sizeDelta = new Vector2(380f, 100f);
            rtPed.anchoredPosition = new Vector2(0f, 20f);
            var imgPed = pedestalGo.AddComponent<Image>();
            imgPed.sprite = LoadSprite("Assets/HITME/Art/UI/Decor/village_tile_pedestal.png");

            var charShowcase = new GameObject("ShowcaseNamLang");
            charShowcase.transform.SetParent(canvasGo.transform, false);
            var rtChar = charShowcase.AddComponent<RectTransform>();
            rtChar.anchorMin = new Vector2(0.5f, 0.5f);
            rtChar.anchorMax = new Vector2(0.5f, 0.5f);
            rtChar.sizeDelta = new Vector2(440f, 520f);
            rtChar.anchoredPosition = new Vector2(0f, 180f);
            var imgChar = charShowcase.AddComponent<Image>();
            imgChar.sprite = LoadSprite("Assets/HITME/Art/Characters/NamLang/namlang_dep_showcase.png");

            // 4. Character & Weapon Selectors (Symmetrical cards)
            var cardChar = CreateSelectorCard(canvasGo.transform, "CharSelectorCard", new Vector2(-240f, -140f), "NAM LÀNG", "Chiến Binh Làng", LoadSprite("Assets/HITME/Art/UI/Portraits/portrait_namlang.png"), font);
            var cardWeap = CreateSelectorCard(canvasGo.transform, "WeapSelectorCard", new Vector2(240f, -140f), "DÉP TỔ ONG", "Sát Thương: 100", LoadSprite("Assets/HITME/Art/UI/Icons/weapon_dep.png"), font);

            // 5. Play Buttons Group
            var btnPlayYellow = CreateMenuButton(canvasGo.transform, "BtnQuickPlay", new Vector2(0f, -320f), new Vector2(580f, 125f), "btn_primary_yellow_normal.png", "CHƠI NHANH", font, 38, new Color32(65, 35, 0, 255));
            var btnBotTraining = CreateMenuButton(canvasGo.transform, "BtnBotTraining", new Vector2(0f, -460f), new Vector2(580f, 105f), "btn_navy_normal.png", "LUYỆN VỚI BOT", font, 32, Color.white);

            var btnCreateRoom = CreateMenuButton(canvasGo.transform, "BtnCreateRoom", new Vector2(-155f, -580f), new Vector2(270f, 95f), "btn_green_normal.png", "TẠO PHÒNG", font, 24, Color.white);
            var btnJoinRoom = CreateMenuButton(canvasGo.transform, "BtnJoinRoom", new Vector2(155f, -580f), new Vector2(270f, 95f), "btn_blue_normal.png", "VÀO PHÒNG", font, 24, Color.white);

            // 6. Bottom Navigation Dock
            var dockGo = new GameObject("BottomNavigationDock");
            dockGo.transform.SetParent(canvasGo.transform, false);
            var rtDock = dockGo.AddComponent<RectTransform>();
            rtDock.anchorMin = new Vector2(0.5f, 0f);
            rtDock.anchorMax = new Vector2(0.5f, 0f);
            rtDock.sizeDelta = new Vector2(620f, 120f);
            rtDock.anchoredPosition = new Vector2(0f, 90f);
            var imgDock = dockGo.AddComponent<Image>();
            imgDock.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/dock_bg.png");
            imgDock.type = Image.Type.Sliced;
            imgDock.color = new Color32(14, 24, 40, 245);

            CreateDockTab(dockGo.transform, "TabHome", new Vector2(-180f, 0f), "Trang chủ", LoadSprite("Assets/HITME/Art/UI/Icons/home.png"), font, true);
            CreateDockTab(dockGo.transform, "TabRank", new Vector2(0f, 0f), "Xếp hạng", LoadSprite("Assets/HITME/Art/UI/Icons/trophy.png"), font, false);
            CreateDockTab(dockGo.transform, "TabQuests", new Vector2(180f, 0f), "Nhiệm vụ", LoadSprite("Assets/HITME/Art/UI/Icons/play_outline.png"), font, false);

            return canvasGo;
        }

        private static GameObject CreateSelectorCard(Transform parent, string name, Vector2 pos, string title, string subtitle, Sprite icon, Font font)
        {
            var card = new GameObject(name);
            card.transform.SetParent(parent, false);
            var rt = card.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(440f, 150f);
            rt.anchoredPosition = pos;

            var bg = card.AddComponent<Image>();
            bg.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/panel_card_dark_gold.png");
            bg.type = Image.Type.Sliced;
            bg.color = new Color32(18, 28, 44, 250);

            // Icon
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(card.transform, false);
            var rtIcon = iconGo.AddComponent<RectTransform>();
            rtIcon.anchorMin = new Vector2(0f, 0.5f);
            rtIcon.anchorMax = new Vector2(0f, 0.5f);
            rtIcon.sizeDelta = new Vector2(90f, 90f);
            rtIcon.anchoredPosition = new Vector2(65f, 0f);
            var imgIcon = iconGo.AddComponent<Image>();
            imgIcon.sprite = icon;

            // Title & Subtitle
            var txtTitle = CreateText(card.transform, "Title", title, font, 24, TextAnchor.MiddleLeft, new Color32(255, 215, 0, 255));
            txtTitle.rectTransform.anchoredPosition = new Vector2(35f, 16f);

            var txtSub = CreateText(card.transform, "Subtitle", subtitle, font, 18, TextAnchor.MiddleLeft, new Color32(180, 200, 220, 255));
            txtSub.rectTransform.anchoredPosition = new Vector2(35f, -16f);

            // Left/Right Chevrons
            CreateChevron(card.transform, "ChevLeft", new Vector2(25f, 0f), LoadSprite("Assets/HITME/Art/UI/Icons/chevron_left.png"));
            CreateChevron(card.transform, "ChevRight", new Vector2(-25f, 0f), LoadSprite("Assets/HITME/Art/UI/Icons/chevron_right.png"));

            return card;
        }

        private static void CreateChevron(Transform parent, string name, Vector2 pos, Sprite sprite)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = pos.x > 0 ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
            rt.anchorMax = pos.x > 0 ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
            rt.sizeDelta = new Vector2(28f, 28f);
            rt.anchoredPosition = pos;
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = new Color32(255, 215, 0, 200);
        }

        private static GameObject CreateMenuButton(Transform parent, string name, Vector2 pos, Vector2 size, string skinName, string label, Font font, int fontSize, Color textColor)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            var img = btnGo.AddComponent<Image>();
            img.sprite = LoadSprite($"Assets/HITME/Art/UI/Skins/{skinName}");
            img.type = Image.Type.Sliced;
            var btn = btnGo.AddComponent<Button>();

            var txt = CreateText(btnGo.transform, "Label", label, font, fontSize, TextAnchor.MiddleCenter, textColor);
            txt.rectTransform.anchoredPosition = new Vector2(0f, 4f);

            return btnGo;
        }

        private static void CreateDockTab(Transform parent, string name, Vector2 pos, string label, Sprite icon, Font font, bool isSelected)
        {
            var tab = new GameObject(name);
            tab.transform.SetParent(parent, false);
            var rt = tab.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(140f, 90f);
            rt.anchoredPosition = pos;

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(tab.transform, false);
            var rtIcon = iconGo.AddComponent<RectTransform>();
            rtIcon.sizeDelta = new Vector2(40f, 40f);
            rtIcon.anchoredPosition = new Vector2(0f, 14f);
            var imgIcon = iconGo.AddComponent<Image>();
            imgIcon.sprite = icon;
            imgIcon.color = isSelected ? new Color32(0, 230, 255, 255) : new Color32(140, 160, 185, 255);

            var txt = CreateText(tab.transform, "Label", label, font, 18, TextAnchor.MiddleCenter, isSelected ? new Color32(0, 230, 255, 255) : new Color32(140, 160, 185, 255));
            txt.rectTransform.anchoredPosition = new Vector2(0f, -22f);
        }

        private static Text CreateText(Transform parent, string name, string content, Font font, int size, TextAnchor alignment, Color color)
        {
            var textGo = new GameObject(name);
            textGo.transform.SetParent(parent, false);
            var rt = textGo.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400f, 60f);
            var txt = textGo.AddComponent<Text>();
            txt.text = content;
            txt.font = font;
            txt.fontSize = size;
            txt.alignment = alignment;
            txt.color = color;
            txt.raycastTarget = false;
            return txt;
        }

        // ==========================================
        // 6. SAVE REUSABLE PREFABS
        // ==========================================
        private static void SaveStagingPrefabs(GameObject combatFloor, GameObject env, GameObject hud, GameObject menu)
        {
            Directory.CreateDirectory("Assets/HITME/Prefabs/Arena");
            Directory.CreateDirectory("Assets/HITME/Prefabs/Presentation");

            PrefabUtility.SaveAsPrefabAsset(combatFloor, "Assets/HITME/Prefabs/Arena/Arena_CombatFloor.prefab");
            PrefabUtility.SaveAsPrefabAsset(env, "Assets/HITME/Prefabs/Arena/Arena_EnvironmentDecor.prefab");
            PrefabUtility.SaveAsPrefabAsset(hud, "Assets/HITME/Prefabs/Presentation/InMatchHUD_Compact.prefab");
            PrefabUtility.SaveAsPrefabAsset(menu, "Assets/HITME/Prefabs/Presentation/MainMenu_VisualStaging.prefab");

            Debug.Log("[ArenaStagingBuilder] Exported 4 visual staging prefabs into Assets/HITME/Prefabs/");
        }

        // ==========================================
        // 7. DIRECT UNITY RENDER & SCREENSHOT CAPTURE
        // ==========================================
        private static void CaptureVisualScreenshots(Camera cam, GameObject floor, GameObject env, GameObject actors, GameObject hud, GameObject menu)
        {
            int w = 1080;
            int h = 2340;
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;

            // Screenshot 1: Arena Floor Only
            floor.SetActive(true);
            env.SetActive(false);
            actors.SetActive(false);
            hud.SetActive(false);
            menu.SetActive(false);
            RenderAndSave(cam, rt, Path.Combine(EvidenceDir, "unity_01_arena_floor_only.png"));

            // Screenshot 2: Arena with Environmental Layers & Characters
            floor.SetActive(true);
            env.SetActive(true);
            actors.SetActive(true);
            hud.SetActive(false);
            menu.SetActive(false);
            RenderAndSave(cam, rt, Path.Combine(EvidenceDir, "unity_02_arena_with_environment.png"));

            // Screenshot 3: Main Menu Staging
            floor.SetActive(false);
            env.SetActive(false);
            actors.SetActive(false);
            hud.SetActive(false);
            menu.SetActive(true);
            RenderAndSave(cam, rt, Path.Combine(EvidenceDir, "unity_03_main_menu_staging.png"));

            // Screenshot 4: Compact HUD Preview over Arena
            floor.SetActive(true);
            env.SetActive(true);
            actors.SetActive(true);
            hud.SetActive(true);
            menu.SetActive(false);
            RenderAndSave(cam, rt, Path.Combine(EvidenceDir, "unity_04_compact_hud_preview.png"));

            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);

            // Restore all to active
            floor.SetActive(true);
            env.SetActive(true);
            actors.SetActive(true);
            hud.SetActive(false);
            menu.SetActive(true);
        }

        private static void RenderAndSave(Camera cam, RenderTexture rt, string outputPath)
        {
            Canvas.ForceUpdateCanvases();
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(outputPath, bytes);
            UnityEngine.Object.DestroyImmediate(tex);
            Debug.Log($"[ArenaStagingBuilder] Captured screenshot: {outputPath} ({bytes.Length / 1024} KB)");
        }
    }
}
