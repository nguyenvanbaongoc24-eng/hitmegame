using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HitMe.Editor
{
    public static class ArenaStagingBuilder
    {
        private const string StagingScenePath = "Assets/Scenes/ArenaVisual_Staging.unity";
        private const string EvidenceDir = "D:/HITME GAME/docs/evidence";

        [MenuItem("Tools/HIT ME/Visual Sprint 01/Build Staging and Render All")]
        public static void BuildStagingAndRenderAll()
        {
            Debug.Log("[ArenaStagingBuilder] Starting Sprint 01 Visual & UI Overhaul build...");
            ConfigureImportSettings();
            EnsureSortingLayers();

            var tmpFont = GetOrCreateTMPFont();
            Debug.Log($"[ArenaStagingBuilder] Using TMP font: {(tmpFont != null ? tmpFont.name : "null")}");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Setup Camera (Orthographic, framed for 9:16 mobile portrait)
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 5.2f;
            camGo.transform.position = new Vector3(0f, -0.2f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(18, 26, 38, 255); // Rich midnight navy

            // 2. Setup EventSystem
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // 3. Build Layers
            var combatFloorGo = BuildCombatFloor();
            var menuVillageGo = BuildVillageCourtyardEnvironment();
            var matchDecorGo = BuildArenaMatchDecorEnvironment();
            var actorsGo = BuildActorsStaging();
            var hudCanvasGo = BuildInMatchHUD(cam, tmpFont);
            var mainMenuCanvasGo = BuildMainMenu(cam, tmpFont);

            // 4. Save Prefabs
            SaveStagingPrefabs(combatFloorGo, matchDecorGo, hudCanvasGo, mainMenuCanvasGo);

            // 5. Save Scene
            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, StagingScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ArenaStagingBuilder] Saved scene to {StagingScenePath}");

            // 6. Capture 4 Unity-Rendered 9:16 Screenshots
            Directory.CreateDirectory(EvidenceDir);
            CaptureVisualScreenshots(cam, combatFloorGo, menuVillageGo, matchDecorGo, actorsGo, hudCanvasGo, mainMenuCanvasGo);

            Debug.Log("[ArenaStagingBuilder] Sprint 01 Visual & UI Overhaul COMPLETE!");
        }

        public static void ConfigureImportSettings()
        {
            AssetDatabase.Refresh();
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
                if (importer.filterMode != FilterMode.Bilinear) { importer.filterMode = FilterMode.Bilinear; dirty = true; }

                // Repeat wrap mode for seamless stone floor tile
                if (path.Contains("ground_tile_seamless") || path.Contains("HITME_A07_Stone_Tile_Floor"))
                {
                    if (importer.wrapMode != TextureWrapMode.Repeat)
                    {
                        importer.wrapMode = TextureWrapMode.Repeat;
                        dirty = true;
                    }
                    var tis = new TextureImporterSettings();
                    importer.ReadTextureSettings(tis);
                    if (tis.spriteMeshType != SpriteMeshType.FullRect)
                    {
                        tis.spriteMeshType = SpriteMeshType.FullRect;
                        importer.SetTextureSettings(tis);
                        dirty = true;
                    }
                }
                else
                {
                    if (importer.wrapMode != TextureWrapMode.Clamp)
                    {
                        importer.wrapMode = TextureWrapMode.Clamp;
                        dirty = true;
                    }
                }

                // 9-slice borders for UI buttons and panels
                if (path.Contains("btn_primary_yellow"))
                {
                    importer.spriteBorder = new Vector4(60, 60, 60, 60);
                    dirty = true;
                }
                else if (path.Contains("btn_blue") || path.Contains("btn_green") || path.Contains("btn_navy"))
                {
                    importer.spriteBorder = new Vector4(50, 50, 50, 50);
                    dirty = true;
                }
                else if (path.Contains("panel_card_dark"))
                {
                    importer.spriteBorder = new Vector4(50, 50, 50, 50);
                    dirty = true;
                }
                else if (path.Contains("dock_bg"))
                {
                    importer.spriteBorder = new Vector4(60, 60, 60, 60);
                    dirty = true;
                }
                else if (path.Contains("pill_coin"))
                {
                    importer.spriteBorder = new Vector4(45, 45, 45, 45);
                    dirty = true;
                }
                else if (path.Contains("panel_profile_strip"))
                {
                    importer.spriteBorder = new Vector4(45, 45, 45, 45);
                    dirty = true;
                }
                else if (path.Contains("avatar_ring_cyan"))
                {
                    importer.spriteBorder = new Vector4(40, 40, 40, 40);
                    dirty = true;
                }

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
            string[] required = { "Default", "FarBackground", "EnvironmentBack", "CombatFloor", "ArenaBorder", "CenterDecal", "SideDecor", "Actors", "AimLine", "Foreground", "HUD" };

            var existing = new HashSet<string>();
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

        private static TMP_FontAsset GetOrCreateTMPFont()
        {
            // 1. Try loading LiberationSans SDF from TextMesh Pro Resources
            const string tmpPath1 = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(tmpPath1);
            if (fontAsset != null) return fontAsset;

            fontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (fontAsset != null) return fontAsset;

            return null;
        }

        // ==========================================
        // 1. COMBAT FLOOR (VÙNG CHƠI THẬT - 4.0m x 6.4m)
        // Clean repeating seamless stone tile ground + crisp rounded border + centered lotus mandala
        // ==========================================
        private static GameObject BuildCombatFloor()
        {
            var root = new GameObject("[01_CombatFloor]");
            root.transform.position = Vector3.zero;

            float floorW = 3.4f;
            float floorH = 5.6f;
            float centerY = -0.1f;

            // 1. Tiled Stone Floor: Seamless square tiles repeating cleanly across 3.4 x 5.6
            var tileSprite = LoadSprite("Assets/HITME/Art/Environment/ground_tile_seamless.png");
            if (tileSprite == null) tileSprite = LoadSprite("Assets/HITME/Art/Environment/HITME_A07_Stone_Tile_Floor.png");

            var floorGo = new GameObject("StoneTileFloor_Tiled");
            floorGo.transform.SetParent(root.transform, false);
            floorGo.transform.position = new Vector3(0f, centerY, 0f);
            var srFloor = floorGo.AddComponent<SpriteRenderer>();
            srFloor.sprite = tileSprite;
            srFloor.drawMode = SpriteDrawMode.Tiled;
            srFloor.size = new Vector2(floorW, floorH);
            srFloor.tileMode = SpriteTileMode.Continuous;
            srFloor.sortingLayerName = "CombatFloor";
            srFloor.sortingOrder = 0;
            srFloor.color = new Color(0.98f, 0.95f, 0.90f, 1f); // Warm sandstone

            // 2. Center Lotus Mandala (Authentic Vietnamese lotus emblem, 1.6m width)
            var lotusSprite = LoadSprite("Assets/HITME/Art/Environment/arena_center_lotus_decal.png");
            if (lotusSprite == null) lotusSprite = LoadSprite("Assets/HITME/Art/Environment/HITME_A09_Lotus_Emblem.png");

            if (lotusSprite != null)
            {
                var lotusGo = new GameObject("CenterLotusDecal");
                lotusGo.transform.SetParent(root.transform, false);
                lotusGo.transform.position = new Vector3(0f, centerY, -0.03f);
                var srLotus = lotusGo.AddComponent<SpriteRenderer>();
                srLotus.sprite = lotusSprite;
                srLotus.sortingLayerName = "CenterDecal";
                srLotus.sortingOrder = 5;
                float lScale = 1.6f / lotusSprite.bounds.size.x;
                lotusGo.transform.localScale = new Vector3(lScale, lScale, 1f);
                srLotus.color = new Color(1f, 0.96f, 0.92f, 0.92f);
            }

            // 3. Perimeter White Rounded Boundary Curb
            var borderSprite = LoadSprite("Assets/HITME/Art/Environment/arena_boundary_line.png");
            if (borderSprite == null) borderSprite = LoadSprite("Assets/HITME/Art/Environment/HITME_A08_Arena_Border.png");

            if (borderSprite != null)
            {
                var borderGo = new GameObject("ArenaBorder");
                borderGo.transform.SetParent(root.transform, false);
                borderGo.transform.position = new Vector3(0f, centerY, -0.05f);
                var srBorder = borderGo.AddComponent<SpriteRenderer>();
                srBorder.sprite = borderSprite;
                srBorder.sortingLayerName = "ArenaBorder";
                srBorder.sortingOrder = 10;
                float bx = (floorW + 0.12f) / borderSprite.bounds.size.x;
                float by = (floorH + 0.12f) / borderSprite.bounds.size.y;
                borderGo.transform.localScale = new Vector3(bx, by, 1f);
                srBorder.color = Color.white;
            }

            return root;
        }

        // ==========================================
        // 2A. VILLAGE COURTYARD ENVIRONMENT (For Main Menu Staging)
        // Serene, sunny Vietnamese temple gate with blossoms, lanterns, flags, and tiled courtyard
        // ==========================================
        private static GameObject BuildVillageCourtyardEnvironment()
        {
            var root = new GameObject("[02_VillageCourtyard_Environment]");
            root.transform.position = Vector3.zero;

            // 1. Sky Gradient & Soft Cloud
            var sky = LoadSprite("Assets/HITME/Art/Environment/sky_gradient.png");
            if (sky != null)
            {
                var go = new GameObject("SkyGradient");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, 3.8f, 6f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sky;
                sr.sortingLayerName = "FarBackground";
                sr.sortingOrder = 0;
                go.transform.localScale = new Vector3(2.5f, 2.0f, 1f);
            }

            var cloud = LoadSprite("Assets/HITME/Art/Environment/cloud_soft.png");
            if (cloud != null)
            {
                var go = new GameObject("Cloud");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, 4.2f, 5.5f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = cloud;
                sr.sortingLayerName = "FarBackground";
                sr.sortingOrder = 1;
                go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            }

            // 2. Far Mountains & Village Rooftops
            var mountains = LoadSprite("Assets/HITME/Art/Environment/far_mountains.png");
            if (mountains != null)
            {
                var go = new GameObject("FarMountains");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, 4.4f, 5f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = mountains;
                sr.sortingLayerName = "FarBackground";
                sr.sortingOrder = 2;
                go.transform.localScale = new Vector3(1.35f, 1.35f, 1f);
            }

            var rooftops = LoadSprite("Assets/HITME/Art/Environment/village_rooftops.png");
            if (rooftops != null)
            {
                var go = new GameObject("VillageRooftops");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, 3.8f, 4f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = rooftops;
                sr.sortingLayerName = "EnvironmentBack";
                sr.sortingOrder = 5;
                go.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
            }

            // 3. Courtyard Ground (Tiled across lower 60% of viewport)
            var groundTile = LoadSprite("Assets/HITME/Art/Environment/ground_tile_seamless.png");
            if (groundTile != null)
            {
                var go = new GameObject("CourtyardGround");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, -1.8f, 3f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = groundTile;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(6.0f, 7.5f);
                sr.tileMode = SpriteTileMode.Continuous;
                sr.sortingLayerName = "CombatFloor";
                sr.sortingOrder = 0;
                sr.color = new Color(0.98f, 0.95f, 0.90f, 1f);
            }

            // 4. Temple Gate Structure: Stairs, Roof, Pillars, Lanterns, Bunting
            var stairs = LoadSprite("Assets/HITME/Art/Environment/temple_stairs.png");
            if (stairs != null)
            {
                var go = new GameObject("TempleStairs");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, 2.5f, 2f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = stairs;
                sr.sortingLayerName = "EnvironmentBack";
                sr.sortingOrder = 10;
                go.transform.localScale = new Vector3(1.15f, 1.15f, 1f);
            }

            var roof = LoadSprite("Assets/HITME/Art/Environment/temple_roof.png");
            if (roof != null)
            {
                var go = new GameObject("TempleRoof");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, 3.3f, 1.8f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = roof;
                sr.sortingLayerName = "EnvironmentBack";
                sr.sortingOrder = 12;
                go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            }

            var bunting = LoadSprite("Assets/HITME/Art/Environment/bunting.png");
            if (bunting != null)
            {
                var go = new GameObject("Bunting");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, 3.65f, 1.5f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = bunting;
                sr.sortingLayerName = "EnvironmentBack";
                sr.sortingOrder = 15;
                go.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            }

            var pilL = LoadSprite("Assets/HITME/Art/Environment/pillar_left.png");
            if (pilL != null)
            {
                var go = new GameObject("PillarLeft");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(-2.1f, 2.2f, 1.2f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = pilL;
                sr.sortingLayerName = "SideDecor";
                sr.sortingOrder = 10;
                go.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            }

            var pilR = LoadSprite("Assets/HITME/Art/Environment/pillar_right.png");
            if (pilR != null)
            {
                var go = new GameObject("PillarRight");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(2.1f, 2.2f, 1.2f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = pilR;
                sr.sortingLayerName = "SideDecor";
                sr.sortingOrder = 10;
                go.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            }

            var flag = LoadSprite("Assets/HITME/Art/Environment/flag.png");
            if (flag != null)
            {
                var goL = new GameObject("FlagLeft");
                goL.transform.SetParent(root.transform, false);
                goL.transform.position = new Vector3(-2.45f, 2.3f, 1.0f);
                var srL = goL.AddComponent<SpriteRenderer>();
                srL.sprite = flag;
                srL.sortingLayerName = "SideDecor";
                srL.sortingOrder = 12;
                goL.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

                var goR = new GameObject("FlagRight");
                goR.transform.SetParent(root.transform, false);
                goR.transform.position = new Vector3(2.45f, 2.3f, 1.0f);
                var srR = goR.AddComponent<SpriteRenderer>();
                srR.sprite = flag;
                srR.flipX = true;
                srR.sortingLayerName = "SideDecor";
                srR.sortingOrder = 12;
                goR.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
            }

            var lantern = LoadSprite("Assets/HITME/Art/Environment/lantern.png");
            if (lantern != null)
            {
                var goL = new GameObject("LanternLeft");
                goL.transform.SetParent(root.transform, false);
                goL.transform.position = new Vector3(-1.3f, 2.8f, 0.8f);
                var srL = goL.AddComponent<SpriteRenderer>();
                srL.sprite = lantern;
                srL.sortingLayerName = "SideDecor";
                srL.sortingOrder = 14;
                goL.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

                var goR = new GameObject("LanternRight");
                goR.transform.SetParent(root.transform, false);
                goR.transform.position = new Vector3(1.3f, 2.8f, 0.8f);
                var srR = goR.AddComponent<SpriteRenderer>();
                srR.sprite = lantern;
                srR.sortingLayerName = "SideDecor";
                srR.sortingOrder = 14;
                goR.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            }

            var flowerPots = LoadSprite("Assets/HITME/Art/Environment/flower_pots.png");
            if (flowerPots != null)
            {
                var goL = new GameObject("PotsLeft");
                goL.transform.SetParent(root.transform, false);
                goL.transform.position = new Vector3(-2.45f, 0.9f, 0.5f);
                var srL = goL.AddComponent<SpriteRenderer>();
                srL.sprite = flowerPots;
                srL.sortingLayerName = "SideDecor";
                srL.sortingOrder = 15;
                goL.transform.localScale = new Vector3(0.55f, 0.55f, 1f);

                var goR = new GameObject("PotsRight");
                goR.transform.SetParent(root.transform, false);
                goR.transform.position = new Vector3(2.45f, 0.9f, 0.5f);
                var srR = goR.AddComponent<SpriteRenderer>();
                srR.sprite = flowerPots;
                srR.flipX = true;
                srR.sortingLayerName = "SideDecor";
                srR.sortingOrder = 15;
                goR.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
            }

            // 5. Tree Canopies Overhead Framing
            var canopyLeft = LoadSprite("Assets/HITME/Art/Environment/canopy_left.png");
            if (canopyLeft != null)
            {
                var go = new GameObject("CanopyLeft");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(-1.8f, 4.2f, -0.2f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = canopyLeft;
                sr.sortingLayerName = "Foreground";
                sr.sortingOrder = 30;
                go.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            }

            var canopyRight = LoadSprite("Assets/HITME/Art/Environment/canopy_right.png");
            if (canopyRight != null)
            {
                var go = new GameObject("CanopyRight");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(1.8f, 4.2f, -0.2f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = canopyRight;
                sr.sortingLayerName = "Foreground";
                sr.sortingOrder = 30;
                go.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            }

            return root;
        }

        // ==========================================
        // 2B. ARENA MATCH DECOR ENVIRONMENT (For Match Combat View)
        // Spectator crowds behind wooden fences, stone parapets, and parasols
        // ==========================================
        private static GameObject BuildArenaMatchDecorEnvironment()
        {
            var root = new GameObject("[02_ArenaMatch_Decor]");
            root.transform.position = Vector3.zero;

            // 1. North Temple Entrance (Y > 2.6f)
            var roof = LoadSprite("Assets/HITME/Art/Environment/temple_roof.png");
            if (roof != null)
            {
                var go = new GameObject("MatchTempleRoof");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, 3.45f, 2.5f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = roof;
                sr.sortingLayerName = "EnvironmentBack";
                sr.sortingOrder = 10;
                go.transform.localScale = new Vector3(1.05f, 1.05f, 1f);
            }

            var stairs = LoadSprite("Assets/HITME/Art/Environment/temple_stairs.png");
            if (stairs != null)
            {
                var go = new GameObject("MatchTempleStairs");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(0f, 2.85f, 2f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = stairs;
                sr.sortingLayerName = "EnvironmentBack";
                sr.sortingOrder = 15;
                go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
            }

            // Red Lanterns flanking north gate
            var lantern = LoadSprite("Assets/HITME/Art/Environment/lantern.png");
            if (lantern == null) lantern = LoadSprite("Assets/HITME/Art/Environment/HITME_B03_Red_Lantern.png");
            if (lantern != null)
            {
                var goL = new GameObject("LanternNorthL");
                goL.transform.SetParent(root.transform, false);
                goL.transform.position = new Vector3(-1.35f, 3.2f, 1.8f);
                var srL = goL.AddComponent<SpriteRenderer>();
                srL.sprite = lantern;
                srL.sortingLayerName = "SideDecor";
                srL.sortingOrder = 18;
                goL.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

                var goR = new GameObject("LanternNorthR");
                goR.transform.SetParent(root.transform, false);
                goR.transform.position = new Vector3(1.35f, 3.2f, 1.8f);
                var srR = goR.AddComponent<SpriteRenderer>();
                srR.sprite = lantern;
                srR.sortingLayerName = "SideDecor";
                srR.sortingOrder = 18;
                goR.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
            }

            // Flower pots flanking north stairs outside bounds
            var pots = LoadSprite("Assets/HITME/Art/Environment/flower_pots.png");
            if (pots == null) pots = LoadSprite("Assets/HITME/Art/Environment/HITME_C03_Flower_Pots.png");
            if (pots != null)
            {
                var goL = new GameObject("PotsNorthL");
                goL.transform.SetParent(root.transform, false);
                goL.transform.position = new Vector3(-1.85f, 2.85f, 1.5f);
                var srL = goL.AddComponent<SpriteRenderer>();
                srL.sprite = pots;
                srL.sortingLayerName = "SideDecor";
                srL.sortingOrder = 16;
                goL.transform.localScale = new Vector3(0.48f, 0.48f, 1f);

                var goR = new GameObject("PotsNorthR");
                goR.transform.SetParent(root.transform, false);
                goR.transform.position = new Vector3(1.85f, 2.85f, 1.5f);
                var srR = goR.AddComponent<SpriteRenderer>();
                srR.sprite = pots;
                srR.flipX = true;
                srR.sortingLayerName = "SideDecor";
                srR.sortingOrder = 16;
                goR.transform.localScale = new Vector3(0.48f, 0.48f, 1f);
            }

            // 2. West Sideline (Left corridor: X ~ -2.60)
            var crowdL = LoadSprite("Assets/HITME/Art/Environment/HITME_C07_Left_Crowd.png");
            if (crowdL != null)
            {
                var goTop = new GameObject("CrowdLeftTop");
                goTop.transform.SetParent(root.transform, false);
                goTop.transform.position = new Vector3(-2.65f, 1.30f, 1f);
                var srTop = goTop.AddComponent<SpriteRenderer>();
                srTop.sprite = crowdL;
                srTop.sortingLayerName = "SideDecor";
                srTop.sortingOrder = 5;
                goTop.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

                var goBot = new GameObject("CrowdLeftBot");
                goBot.transform.SetParent(root.transform, false);
                goBot.transform.position = new Vector3(-2.65f, -1.45f, 1f);
                var srBot = goBot.AddComponent<SpriteRenderer>();
                srBot.sprite = crowdL;
                srBot.sortingLayerName = "SideDecor";
                srBot.sortingOrder = 5;
                goBot.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
            }

            var fence = LoadSprite("Assets/HITME/Art/Environment/HITME_B08_Bamboo_Fence.png");
            if (fence != null)
            {
                var go = new GameObject("FenceLeftMid");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(-2.45f, 0.05f, 0.8f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = fence;
                sr.sortingLayerName = "SideDecor";
                sr.sortingOrder = 8;
                go.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

                var goR = new GameObject("FenceRightMid");
                goR.transform.SetParent(root.transform, false);
                goR.transform.position = new Vector3(2.45f, 0.05f, 0.8f);
                var srR = goR.AddComponent<SpriteRenderer>();
                srR.sprite = fence;
                srR.flipX = true;
                srR.sortingLayerName = "SideDecor";
                srR.sortingOrder = 8;
                goR.transform.localScale = new Vector3(0.42f, 0.42f, 1f);
            }

            var flag = LoadSprite("Assets/HITME/Art/Environment/HITME_B02_Vietnamese_Flag.png");
            if (flag == null) flag = LoadSprite("Assets/HITME/Art/Environment/flag.png");
            if (flag != null)
            {
                var goL = new GameObject("FlagLeftMid");
                goL.transform.SetParent(root.transform, false);
                goL.transform.position = new Vector3(-2.55f, 0.65f, 0.6f);
                var srL = goL.AddComponent<SpriteRenderer>();
                srL.sprite = flag;
                srL.sortingLayerName = "SideDecor";
                srL.sortingOrder = 12;
                goL.transform.localScale = new Vector3(0.40f, 0.40f, 1f);

                var goR = new GameObject("FlagRightMid");
                goR.transform.SetParent(root.transform, false);
                goR.transform.position = new Vector3(2.55f, 0.65f, 0.6f);
                var srR = goR.AddComponent<SpriteRenderer>();
                srR.sprite = flag;
                srR.flipX = true;
                srR.sortingLayerName = "SideDecor";
                srR.sortingOrder = 12;
                goR.transform.localScale = new Vector3(0.40f, 0.40f, 1f);
            }

            // 3. East Sideline (Right corridor: X ~ +2.60)
            var crowdR = LoadSprite("Assets/HITME/Art/Environment/HITME_C08_Right_Crowd.png");
            if (crowdR != null)
            {
                var goTop = new GameObject("CrowdRightTop");
                goTop.transform.SetParent(root.transform, false);
                goTop.transform.position = new Vector3(2.65f, 1.30f, 1f);
                var srTop = goTop.AddComponent<SpriteRenderer>();
                srTop.sprite = crowdR;
                srTop.sortingLayerName = "SideDecor";
                srTop.sortingOrder = 5;
                goTop.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

                var goBot = new GameObject("CrowdRightBot");
                goBot.transform.SetParent(root.transform, false);
                goBot.transform.position = new Vector3(2.65f, -1.45f, 1f);
                var srBot = goBot.AddComponent<SpriteRenderer>();
                srBot.sprite = crowdR;
                srBot.sortingLayerName = "SideDecor";
                srBot.sortingOrder = 5;
                goBot.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
            }

            // 4. South Boundary: Balustrades, Flags, and Corner Parasols (Y < -2.8)
            var pilL = LoadSprite("Assets/HITME/Art/Environment/pillar_left.png");
            if (pilL != null)
            {
                var go = new GameObject("PillarSouthL");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(-1.95f, -3.10f, 0.5f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = pilL;
                sr.sortingLayerName = "SideDecor";
                sr.sortingOrder = 10;
                go.transform.localScale = new Vector3(0.70f, 0.70f, 1f);
            }

            var pilR = LoadSprite("Assets/HITME/Art/Environment/pillar_right.png");
            if (pilR != null)
            {
                var go = new GameObject("PillarSouthR");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(1.95f, -3.10f, 0.5f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = pilR;
                sr.sortingLayerName = "SideDecor";
                sr.sortingOrder = 10;
                go.transform.localScale = new Vector3(0.70f, 0.70f, 1f);
            }

            // Corner Parasols (Framing bottom outside arena boundary)
            var blueParasol = LoadSprite("Assets/HITME/Art/Environment/parasol_blue.png");
            if (blueParasol == null) blueParasol = LoadSprite("Assets/HITME/Art/Environment/HITME_B05_Blue_Parasol.png");
            if (blueParasol != null)
            {
                var go = new GameObject("BottomBlueParasol");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(-2.25f, -3.75f, -0.1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = blueParasol;
                sr.sortingLayerName = "SideDecor";
                sr.sortingOrder = 20;
                go.transform.localScale = new Vector3(0.80f, 0.80f, 1f);
            }

            var redParasol = LoadSprite("Assets/HITME/Art/Environment/parasol_red.png");
            if (redParasol == null) redParasol = LoadSprite("Assets/HITME/Art/Environment/HITME_B04_Red_Parasol.png");
            if (redParasol != null)
            {
                var go = new GameObject("BottomRedParasol");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(2.25f, -3.75f, -0.1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = redParasol;
                sr.sortingLayerName = "SideDecor";
                sr.sortingOrder = 20;
                go.transform.localScale = new Vector3(0.80f, 0.80f, 1f);
            }

            // Top canopy framing overhead
            var canopyLeft = LoadSprite("Assets/HITME/Art/Environment/canopy_left.png");
            if (canopyLeft != null)
            {
                var go = new GameObject("TopCanopyL");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(-1.8f, 4.3f, -0.2f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = canopyLeft;
                sr.sortingLayerName = "Foreground";
                sr.sortingOrder = 30;
                go.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            }

            var canopyRight = LoadSprite("Assets/HITME/Art/Environment/canopy_right.png");
            if (canopyRight != null)
            {
                var go = new GameObject("TopCanopyR");
                go.transform.SetParent(root.transform, false);
                go.transform.position = new Vector3(1.8f, 4.3f, -0.2f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = canopyRight;
                sr.sortingLayerName = "Foreground";
                sr.sortingOrder = 30;
                go.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            }

            return root;
        }

        // ==========================================
        // 3. GAMEPLAY ELEMENTS STAGING (Nam Làng, Aim Line, Target)
        // ==========================================
        private static GameObject BuildActorsStaging()
        {
            var root = new GameObject("[03_GameplayElements]");
            root.transform.position = Vector3.zero;

            // 1. Player Nam Làng (Lower area of combat field at 0, -1.8)
            var playerGo = new GameObject("Player_NamLang");
            playerGo.transform.SetParent(root.transform, false);
            playerGo.transform.position = new Vector3(0f, -1.8f, -0.5f);

            // Ground Indicator Ring (Cyan)
            var ringGo = new GameObject("GroundIndicatorRing");
            ringGo.transform.SetParent(playerGo.transform, false);
            ringGo.transform.localPosition = new Vector3(0f, -0.22f, 0.05f);
            var srRing = ringGo.AddComponent<SpriteRenderer>();
            srRing.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/avatar_ring_cyan.png");
            srRing.sortingLayerName = "Actors";
            srRing.sortingOrder = 5;
            srRing.color = new Color(0f, 0.95f, 1f, 0.9f);
            ringGo.transform.localScale = new Vector3(0.55f, 0.55f, 1f);

            // Player Sprite
            var charSprite = LoadSprite("Assets/HITME/Art/Characters/NamLang/namlang_dep_showcase.png");
            var srChar = playerGo.AddComponent<SpriteRenderer>();
            srChar.sprite = charSprite;
            srChar.sortingLayerName = "Actors";
            srChar.sortingOrder = 10;
            playerGo.transform.localScale = new Vector3(0.32f, 0.32f, 1f);

            // 2. Aim Trajectory & Target Flip-Flop
            var targetWeaponGo = new GameObject("Target_FlipFlop");
            targetWeaponGo.transform.SetParent(root.transform, false);
            targetWeaponGo.transform.position = new Vector3(0.95f, -0.3f, -0.5f);
            var srW = targetWeaponGo.AddComponent<SpriteRenderer>();
            srW.sprite = LoadSprite("Assets/HITME/Art/UI/Icons/weapon_dep.png");
            srW.sortingLayerName = "AimLine";
            srW.sortingOrder = 15;
            targetWeaponGo.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

            // Soft cyan highlight glow under target
            var glowGo = new GameObject("TargetGlow");
            glowGo.transform.SetParent(targetWeaponGo.transform, false);
            glowGo.transform.localPosition = Vector3.zero;
            var srGlow = glowGo.AddComponent<SpriteRenderer>();
            srGlow.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/avatar_ring_cyan.png");
            srGlow.sortingLayerName = "AimLine";
            srGlow.sortingOrder = 14;
            srGlow.color = new Color(0f, 0.9f, 1f, 0.6f);
            glowGo.transform.localScale = new Vector3(0.75f, 0.75f, 1f);

            // Dotted aim trajectory line from (0, -1.6) to (0.9, -0.35)
            var aimLineGo = new GameObject("AimTrajectoryLine");
            aimLineGo.transform.SetParent(root.transform, false);
            Vector3 startP = new Vector3(0f, -1.6f, -0.45f);
            Vector3 endP = new Vector3(0.9f, -0.35f, -0.45f);
            int dotCount = 8;
            for (int i = 1; i <= dotCount; i++)
            {
                float t = (float)i / (dotCount + 1);
                var dot = new GameObject($"Dot_{i}");
                dot.transform.SetParent(aimLineGo.transform, false);
                dot.transform.position = Vector3.Lerp(startP, endP, t);
                var srDot = dot.AddComponent<SpriteRenderer>();
                srDot.sprite = LoadSprite("Assets/HITME/Art/UI/Icons/play_outline.png");
                srDot.sortingLayerName = "AimLine";
                srDot.sortingOrder = 12;
                srDot.color = new Color(1f, 1f, 1f, 0.85f - (1f - t) * 0.2f);
                dot.transform.localScale = new Vector3(0.08f, 0.08f, 1f);
            }

            return root;
        }

        // ==========================================
        // 4. IN-MATCH COMPACT HUD (Reference: IPHONE MOCKUP.png)
        // Canvas Scaler: 390 x 844, match 0.5, SafeArea Protected
        // ==========================================
        private static GameObject BuildInMatchHUD(Camera cam, TMP_FontAsset tmpFont)
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
            scaler.referenceResolution = new Vector2(390f, 844f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // SafeArea Container (Protects Dynamic Island at top and Home Indicator at bottom)
            var safeArea = new GameObject("SafeAreaContainer");
            safeArea.transform.SetParent(canvasGo.transform, false);
            var rtSafe = safeArea.AddComponent<RectTransform>();
            rtSafe.anchorMin = Vector2.zero;
            rtSafe.anchorMax = Vector2.one;
            rtSafe.offsetMin = new Vector2(0f, 24f);  // Home indicator protection
            rtSafe.offsetMax = new Vector2(0f, -44f); // Dynamic Island protection

            // --- A. TOP BAR (Round Pill, 5s Countdown Timer, Settings Gear) ---
            var topBar = new GameObject("TopBar");
            topBar.transform.SetParent(safeArea.transform, false);
            var rtTop = topBar.AddComponent<RectTransform>();
            rtTop.anchorMin = new Vector2(0f, 1f);
            rtTop.anchorMax = new Vector2(1f, 1f);
            rtTop.pivot = new Vector2(0.5f, 1f);
            rtTop.sizeDelta = new Vector2(0f, 48f);
            rtTop.anchoredPosition = new Vector2(0f, -6f);

            // 1. Round Pill "VÒNG 1/5" (Left)
            var roundPill = new GameObject("RoundPill");
            roundPill.transform.SetParent(topBar.transform, false);
            var rtRP = roundPill.AddComponent<RectTransform>();
            rtRP.anchorMin = new Vector2(0f, 0.5f);
            rtRP.anchorMax = new Vector2(0f, 0.5f);
            rtRP.sizeDelta = new Vector2(92f, 36f);
            rtRP.anchoredPosition = new Vector2(60f, 0f);
            var imgRP = roundPill.AddComponent<Image>();
            imgRP.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/panel_card_dark_gold.png");
            imgRP.type = Image.Type.Sliced;
            imgRP.pixelsPerUnitMultiplier = 3.0f;
            imgRP.color = new Color32(14, 22, 34, 240);

            var txtRound = CreateTMPText(roundPill.transform, "TxtRound", "VÒNG <color=#FFD700>1/5</color>", tmpFont, 12, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            txtRound.rectTransform.anchoredPosition = Vector2.zero;

            // 2. Circular 5s Countdown Timer (Center)
            var timerCircle = new GameObject("TimerCircle");
            timerCircle.transform.SetParent(topBar.transform, false);
            var rtTC = timerCircle.AddComponent<RectTransform>();
            rtTC.anchorMin = new Vector2(0.5f, 0.5f);
            rtTC.anchorMax = new Vector2(0.5f, 0.5f);
            rtTC.sizeDelta = new Vector2(52f, 52f);
            rtTC.anchoredPosition = Vector2.zero;
            var imgTC = timerCircle.AddComponent<Image>();
            imgTC.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/btn_circle_gold.png");

            var txtTimeVal = CreateTMPText(timerCircle.transform, "TxtTimeVal", "5", tmpFont, 20, TextAlignmentOptions.Center, new Color32(255, 225, 60, 255), FontStyles.Bold);
            txtTimeVal.rectTransform.anchoredPosition = new Vector2(0f, 4f);

            var txtTimeSec = CreateTMPText(timerCircle.transform, "TxtTimeSec", "GIÂY", tmpFont, 8, TextAlignmentOptions.Center, new Color32(255, 200, 40, 255), FontStyles.Bold);
            txtTimeSec.rectTransform.anchoredPosition = new Vector2(0f, -12f);

            // 3. Settings Gear Button (Right, minimum 44 units touch target)
            CreateIconButton(topBar.transform, "BtnSettings", new Vector2(362f, 0f), LoadSprite("Assets/HITME/Art/UI/Icons/gear.png"), 44f);

            // --- B. PLAYER STATUS STRIP (6 Players: BẠN + 5 Bots, Y ~ -54) ---
            var playersStrip = new GameObject("PlayersStatusStrip");
            playersStrip.transform.SetParent(safeArea.transform, false);
            var rtPS = playersStrip.AddComponent<RectTransform>();
            rtPS.anchorMin = new Vector2(0.5f, 1f);
            rtPS.anchorMax = new Vector2(0.5f, 1f);
            rtPS.pivot = new Vector2(0.5f, 1f);
            rtPS.sizeDelta = new Vector2(366f, 68f);
            rtPS.anchoredPosition = new Vector2(0f, -54f);

            string[] names = { "BẠN", "Bot 1", "Bot 2", "Bot 3", "Bot 4", "Bot 5" };
            string[] portraitPaths = {
                "Assets/HITME/Art/UI/Portraits/head_nam_lang.png",
                "Assets/HITME/Art/UI/Portraits/head_anh_non_la.png",
                "Assets/HITME/Art/UI/Portraits/head_mai_hong.png",
                "Assets/HITME/Art/UI/Portraits/head_anh_nui.png",
                "Assets/HITME/Art/UI/Portraits/head_mai_hong.png",
                "Assets/HITME/Art/UI/Portraits/head_anh_non_la.png"
            };

            float cardW = 56f;
            float spacing = 6f;
            float startX = -((cardW * 6 + spacing * 5) / 2f) + cardW / 2f;

            for (int i = 0; i < 6; i++)
            {
                bool isUser = (i == 0);
                float posX = startX + i * (cardW + spacing);
                CreatePlayerMiniCard(playersStrip.transform, $"PlayerSlot_{i}", new Vector2(posX, 0f), names[i], LoadSprite(portraitPaths[i]), tmpFont, isUser);
            }

            // --- C. PHASE BANNER "CHỌN HƯỚNG NÉM | 🥷 Đối thủ đang ẩn" (Y ~ -136) ---
            var phaseBanner = new GameObject("PhaseBanner");
            phaseBanner.transform.SetParent(safeArea.transform, false);
            var rtPB = phaseBanner.AddComponent<RectTransform>();
            rtPB.anchorMin = new Vector2(0.5f, 1f);
            rtPB.anchorMax = new Vector2(0.5f, 1f);
            rtPB.pivot = new Vector2(0.5f, 1f);
            rtPB.sizeDelta = new Vector2(290f, 28f);
            rtPB.anchoredPosition = new Vector2(0f, -136f);
            var imgPB = phaseBanner.AddComponent<Image>();
            imgPB.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/panel_card_dark_gold.png");
            imgPB.type = Image.Type.Sliced;
            imgPB.pixelsPerUnitMultiplier = 3.0f;
            imgPB.color = new Color32(10, 16, 26, 250);

            var txtPhaseL = CreateTMPText(phaseBanner.transform, "TxtPhaseL", "<color=#FFD700>CHỌN HƯỚNG NÉM</color>  |", tmpFont, 10, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            txtPhaseL.rectTransform.sizeDelta = new Vector2(130f, 26f);
            txtPhaseL.rectTransform.anchoredPosition = new Vector2(-65f, -14f);

            var maskIcon = new GameObject("MaskIcon");
            maskIcon.transform.SetParent(phaseBanner.transform, false);
            var rtMask = maskIcon.AddComponent<RectTransform>();
            rtMask.sizeDelta = new Vector2(14f, 14f);
            rtMask.anchoredPosition = new Vector2(10f, -14f);
            var imgMask = maskIcon.AddComponent<Image>();
            imgMask.sprite = LoadSprite("Assets/HITME/Art/UI/Icons/mask_hidden.png");

            var txtPhaseR = CreateTMPText(phaseBanner.transform, "TxtPhaseR", "<color=#EEEEEE>Đối thủ đang ẩn</color>", tmpFont, 10, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            txtPhaseR.rectTransform.sizeDelta = new Vector2(110f, 26f);
            txtPhaseR.rectTransform.anchoredPosition = new Vector2(76f, -14f);

            // --- D. FLOATING GUIDE PILL "Kéo để chọn hướng" (Y ~ 110) ---
            var guidePill = new GameObject("GuidePill");
            guidePill.transform.SetParent(safeArea.transform, false);
            var rtGP = guidePill.AddComponent<RectTransform>();
            rtGP.anchorMin = new Vector2(0.5f, 0f);
            rtGP.anchorMax = new Vector2(0.5f, 0f);
            rtGP.sizeDelta = new Vector2(175f, 32f);
            rtGP.anchoredPosition = new Vector2(0f, 110f);
            var imgGP = guidePill.AddComponent<Image>();
            imgGP.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/panel_card_dark_gold.png");
            imgGP.type = Image.Type.Sliced;
            imgGP.pixelsPerUnitMultiplier = 3.0f;
            imgGP.color = new Color32(14, 22, 36, 235);

            var handIcon = new GameObject("HandPointerIcon");
            handIcon.transform.SetParent(guidePill.transform, false);
            var rtHand = handIcon.AddComponent<RectTransform>();
            rtHand.sizeDelta = new Vector2(18f, 18f);
            rtHand.anchoredPosition = new Vector2(-60f, 0f);
            var imgHand = handIcon.AddComponent<Image>();
            imgHand.sprite = LoadSprite("Assets/HITME/Art/UI/Icons/touch_pointer.png");

            var txtGuide = CreateTMPText(guidePill.transform, "TxtGuide", "Kéo để chọn hướng", tmpFont, 11, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            txtGuide.rectTransform.anchoredPosition = new Vector2(10f, 0f);

            // --- E. BOTTOM CONTROL BAR (Emote, Weapon Slot, SẴN SÀNG CTA, Smile) ---
            var bottomBar = new GameObject("BottomBar");
            bottomBar.transform.SetParent(safeArea.transform, false);
            var rtBB = bottomBar.AddComponent<RectTransform>();
            rtBB.anchorMin = new Vector2(0f, 0f);
            rtBB.anchorMax = new Vector2(1f, 0f);
            rtBB.pivot = new Vector2(0.5f, 0f);
            rtBB.sizeDelta = new Vector2(0f, 76f);
            rtBB.anchoredPosition = new Vector2(0f, 10f);

            // 1. Left Chat Button (48x48, touch target >= 44)
            CreateIconButton(bottomBar.transform, "BtnChat", new Vector2(36f, 0f), LoadSprite("Assets/HITME/Art/UI/Icons/chat_bubble.png"), 48f);

            // 2. Equipped Weapon Card
            var weaponCard = new GameObject("EquippedWeaponCard");
            weaponCard.transform.SetParent(bottomBar.transform, false);
            var rtWC = weaponCard.AddComponent<RectTransform>();
            rtWC.anchorMin = new Vector2(0f, 0.5f);
            rtWC.anchorMax = new Vector2(0f, 0.5f);
            rtWC.sizeDelta = new Vector2(62f, 64f);
            rtWC.anchoredPosition = new Vector2(100f, 0f);
            var imgWC = weaponCard.AddComponent<Image>();
            imgWC.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/panel_card_dark_gold.png");
            imgWC.type = Image.Type.Sliced;
            imgWC.pixelsPerUnitMultiplier = 3.0f;
            imgWC.color = new Color32(14, 24, 40, 250);

            var wIcon = new GameObject("WeaponIcon");
            wIcon.transform.SetParent(weaponCard.transform, false);
            var rtWIcon = wIcon.AddComponent<RectTransform>();
            rtWIcon.sizeDelta = new Vector2(40f, 40f);
            rtWIcon.anchoredPosition = new Vector2(0f, 6f);
            var imgWIcon = wIcon.AddComponent<Image>();
            imgWIcon.sprite = LoadSprite("Assets/HITME/Art/UI/Icons/weapon_dep.png");

            var txtWLabel = CreateTMPText(weaponCard.transform, "TxtWLabel", "Dép", tmpFont, 9, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            txtWLabel.rectTransform.anchoredPosition = new Vector2(0f, -18f);

            // 3. Giant Yellow CTA Button "SẴN SÀNG" (Width 170, Height 52)
            var btnReady = new GameObject("BtnReadyAction");
            btnReady.transform.SetParent(bottomBar.transform, false);
            var rtReady = btnReady.AddComponent<RectTransform>();
            rtReady.anchorMin = new Vector2(0.5f, 0.5f);
            rtReady.anchorMax = new Vector2(0.5f, 0.5f);
            rtReady.sizeDelta = new Vector2(170f, 52f);
            rtReady.anchoredPosition = new Vector2(35f, 0f);
            var imgReady = btnReady.AddComponent<Image>();
            imgReady.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/btn_primary_yellow_normal.png");
            imgReady.type = Image.Type.Sliced;
            imgReady.pixelsPerUnitMultiplier = 3.0f;
            btnReady.AddComponent<Button>();

            var txtReady = CreateTMPText(btnReady.transform, "TxtReady", "SẴN SÀNG", tmpFont, 18, TextAlignmentOptions.Center, new Color32(65, 35, 0, 255), FontStyles.Bold);
            txtReady.rectTransform.anchoredPosition = new Vector2(0f, 2f);

            // 4. Right Smile / Emote Button (48x48, touch target >= 44)
            CreateIconButton(bottomBar.transform, "BtnEmoteSmile", new Vector2(354f, 0f), LoadSprite("Assets/HITME/Art/UI/Icons/emote_smile.png"), 48f);

            return canvasGo;
        }

        private static void CreatePlayerMiniCard(Transform parent, string name, Vector2 pos, string label, Sprite avatar, TMP_FontAsset font, bool isUser)
        {
            var card = new GameObject(name);
            card.transform.SetParent(parent, false);
            var rt = card.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(56f, 66f);
            rt.anchoredPosition = pos;

            var bg = card.AddComponent<Image>();
            bg.sprite = LoadSprite(isUser ? "Assets/HITME/Art/UI/Skins/avatar_ring_cyan.png" : "Assets/HITME/Art/UI/Skins/panel_card_dark_gold.png");
            bg.type = Image.Type.Sliced;
            bg.pixelsPerUnitMultiplier = 3.0f;
            bg.color = isUser ? new Color32(0, 240, 255, 230) : new Color32(14, 22, 34, 240);

            // Name label / Tag
            var txtTag = CreateTMPText(card.transform, "Tag", label, font, 9, TextAlignmentOptions.Center, isUser ? new Color32(0, 240, 255, 255) : new Color32(200, 210, 225, 255), FontStyles.Bold);
            txtTag.rectTransform.anchoredPosition = new Vector2(0f, 21f);

            // Avatar Portrait
            if (avatar != null)
            {
                var avGo = new GameObject("Avatar");
                avGo.transform.SetParent(card.transform, false);
                var rtAv = avGo.AddComponent<RectTransform>();
                rtAv.sizeDelta = new Vector2(30f, 30f);
                rtAv.anchoredPosition = new Vector2(0f, 2f);
                var imgAv = avGo.AddComponent<Image>();
                imgAv.sprite = avatar;
            }

            // Hearts & HP "♥♥♥ 3/3"
            var txtHp = CreateTMPText(card.transform, "Hp", "<color=#FF3344>♥♥♥</color> 3/3", font, 8, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            txtHp.rectTransform.anchoredPosition = new Vector2(0f, -21f);
        }

        // ==========================================
        // 5. MODULAR MAIN MENU STAGING (Reference: MAIN MENU.png)
        // Canvas Scaler: 390 x 844, match 0.5, SafeArea Protected
        // Zero full-mockup background! Uses 9-sliced panels and modular components.
        // ==========================================
        private static GameObject BuildMainMenu(Camera cam, TMP_FontAsset tmpFont)
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
            scaler.referenceResolution = new Vector2(390f, 844f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // SafeArea Container
            var safeArea = new GameObject("SafeAreaContainer");
            safeArea.transform.SetParent(canvasGo.transform, false);
            var rtSafe = safeArea.AddComponent<RectTransform>();
            rtSafe.anchorMin = Vector2.zero;
            rtSafe.anchorMax = Vector2.one;
            rtSafe.offsetMin = new Vector2(0f, 24f);  // Protect Home Indicator
            rtSafe.offsetMax = new Vector2(0f, -44f); // Protect Dynamic Island

            // --- 1. TOP PROFILE BAR (Y = -8) ---
            var topBar = new GameObject("TopProfileBar");
            topBar.transform.SetParent(safeArea.transform, false);
            var rtBar = topBar.AddComponent<RectTransform>();
            rtBar.anchorMin = new Vector2(0f, 1f);
            rtBar.anchorMax = new Vector2(1f, 1f);
            rtBar.pivot = new Vector2(0.5f, 1f);
            rtBar.sizeDelta = new Vector2(0f, 48f);
            rtBar.anchoredPosition = new Vector2(0f, -8f);

            // A. Profile Capsule (Left)
            var profileStrip = new GameObject("ProfileStrip");
            profileStrip.transform.SetParent(topBar.transform, false);
            var rtStrip = profileStrip.AddComponent<RectTransform>();
            rtStrip.anchorMin = new Vector2(0f, 0.5f);
            rtStrip.anchorMax = new Vector2(0f, 0.5f);
            rtStrip.pivot = new Vector2(0f, 0.5f);
            rtStrip.sizeDelta = new Vector2(146f, 44f);
            rtStrip.anchoredPosition = new Vector2(12f, 0f);
            var imgStrip = profileStrip.AddComponent<Image>();
            imgStrip.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/panel_profile_strip.png");
            imgStrip.type = Image.Type.Sliced;
            imgStrip.pixelsPerUnitMultiplier = 3.0f;

            var avRing = new GameObject("AvatarRing");
            avRing.transform.SetParent(profileStrip.transform, false);
            var rtRing = avRing.AddComponent<RectTransform>();
            rtRing.anchorMin = new Vector2(0f, 0.5f);
            rtRing.anchorMax = new Vector2(0f, 0.5f);
            rtRing.pivot = new Vector2(0.5f, 0.5f);
            rtRing.sizeDelta = new Vector2(36f, 36f);
            rtRing.anchoredPosition = new Vector2(22f, 0f);
            var imgRing = avRing.AddComponent<Image>();
            imgRing.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/avatar_ring_cyan.png");

            var avFace = new GameObject("AvatarFace");
            avFace.transform.SetParent(avRing.transform, false);
            var rtFace = avFace.AddComponent<RectTransform>();
            rtFace.sizeDelta = new Vector2(28f, 28f);
            var imgFace = avFace.AddComponent<Image>();
            imgFace.sprite = LoadSprite("Assets/HITME/Art/UI/Portraits/portrait_namlang.png");

            var txtUser = CreateTMPText(profileStrip.transform, "TxtUser", "Nam Làng", tmpFont, 12, TextAlignmentOptions.Left, Color.white, FontStyles.Bold);
            txtUser.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            txtUser.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            txtUser.rectTransform.pivot = new Vector2(0f, 0.5f);
            txtUser.rectTransform.anchoredPosition = new Vector2(44f, 7f);
            txtUser.rectTransform.sizeDelta = new Vector2(92f, 18f);

            var txtRank = CreateTMPText(profileStrip.transform, "TxtRank", "Hồ sơ", tmpFont, 9, TextAlignmentOptions.Left, new Color32(0, 230, 255, 255));
            txtRank.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            txtRank.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            txtRank.rectTransform.pivot = new Vector2(0f, 0.5f);
            txtRank.rectTransform.anchoredPosition = new Vector2(44f, -9f);
            txtRank.rectTransform.sizeDelta = new Vector2(36f, 14f);

            var penIcon = new GameObject("PencilIcon");
            penIcon.transform.SetParent(profileStrip.transform, false);
            var rtPen = penIcon.AddComponent<RectTransform>();
            rtPen.anchorMin = new Vector2(0f, 0.5f);
            rtPen.anchorMax = new Vector2(0f, 0.5f);
            rtPen.pivot = new Vector2(0f, 0.5f);
            rtPen.sizeDelta = new Vector2(10f, 10f);
            rtPen.anchoredPosition = new Vector2(80f, -9f);
            var imgPen = penIcon.AddComponent<Image>();
            imgPen.sprite = LoadSprite("Assets/HITME/Art/UI/Icons/pencil.png");
            imgPen.color = new Color32(0, 230, 255, 255);

            // B. Gold Currency Pill (Center-Right)
            var coinPill = new GameObject("LotusCoinPill");
            coinPill.transform.SetParent(topBar.transform, false);
            var rtCoin = coinPill.AddComponent<RectTransform>();
            rtCoin.anchorMin = new Vector2(1f, 0.5f);
            rtCoin.anchorMax = new Vector2(1f, 0.5f);
            rtCoin.pivot = new Vector2(1f, 0.5f);
            rtCoin.sizeDelta = new Vector2(118f, 40f);
            rtCoin.anchoredPosition = new Vector2(-54f, 0f);
            var imgCoin = coinPill.AddComponent<Image>();
            imgCoin.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/pill_coin.png");
            imgCoin.type = Image.Type.Sliced;
            imgCoin.pixelsPerUnitMultiplier = 3.0f;

            var coinIcon = new GameObject("CoinIcon");
            coinIcon.transform.SetParent(coinPill.transform, false);
            var rtCIcon = coinIcon.AddComponent<RectTransform>();
            rtCIcon.anchorMin = new Vector2(0f, 0.5f);
            rtCIcon.anchorMax = new Vector2(0f, 0.5f);
            rtCIcon.sizeDelta = new Vector2(22f, 22f);
            rtCIcon.anchoredPosition = new Vector2(16f, 0f);
            var imgCIcon = coinIcon.AddComponent<Image>();
            imgCIcon.sprite = LoadSprite("Assets/HITME/Art/UI/Icons/coin_lotus.png");

            var txtCoins = CreateTMPText(coinPill.transform, "TxtCoins", "1.250", tmpFont, 13, TextAlignmentOptions.Center, new Color32(255, 225, 60, 255), FontStyles.Bold);
            txtCoins.rectTransform.anchoredPosition = new Vector2(4f, 0f);

            var plusBtn = new GameObject("PlusBtn");
            plusBtn.transform.SetParent(coinPill.transform, false);
            var rtPlus = plusBtn.AddComponent<RectTransform>();
            rtPlus.anchorMin = new Vector2(1f, 0.5f);
            rtPlus.anchorMax = new Vector2(1f, 0.5f);
            rtPlus.sizeDelta = new Vector2(20f, 20f);
            rtPlus.anchoredPosition = new Vector2(-14f, 0f);
            var imgPlus = plusBtn.AddComponent<Image>();
            imgPlus.sprite = LoadSprite("Assets/HITME/Art/UI/Icons/plus.png");

            // C. Settings Gear Button (Right, minimum 44 units touch target)
            CreateIconButton(topBar.transform, "BtnSettings", new Vector2(362f, 0f), LoadSprite("Assets/HITME/Art/UI/Icons/gear.png"), 44f);

            // --- 2. OFFICIAL 3D LOGO "HIT ME" (Y = -92) ---
            var logoGo = new GameObject("LogoHITME");
            logoGo.transform.SetParent(safeArea.transform, false);
            var rtLogo = logoGo.AddComponent<RectTransform>();
            rtLogo.anchorMin = new Vector2(0.5f, 1f);
            rtLogo.anchorMax = new Vector2(0.5f, 1f);
            rtLogo.sizeDelta = new Vector2(260f, 105f);
            rtLogo.anchoredPosition = new Vector2(0f, -92f);
            var imgLogo = logoGo.AddComponent<Image>();
            imgLogo.sprite = LoadSprite("Assets/HITME/Art/UI/Logo/logo_hit_me.png");

            // --- 3. CHARACTER SHOWCASE (Hero pedestal, Nam Làng, Cycle chevrons) ---
            var pedestalGo = new GameObject("ShowcasePedestal");
            pedestalGo.transform.SetParent(safeArea.transform, false);
            var rtPed = pedestalGo.AddComponent<RectTransform>();
            rtPed.anchorMin = new Vector2(0.5f, 0.5f);
            rtPed.anchorMax = new Vector2(0.5f, 0.5f);
            rtPed.sizeDelta = new Vector2(200f, 52f);
            rtPed.anchoredPosition = new Vector2(0f, 15f);
            var imgPed = pedestalGo.AddComponent<Image>();
            imgPed.sprite = LoadSprite("Assets/HITME/Art/UI/Decor/village_tile_pedestal.png");

            var charShowcase = new GameObject("ShowcaseNamLang");
            charShowcase.transform.SetParent(safeArea.transform, false);
            var rtChar = charShowcase.AddComponent<RectTransform>();
            rtChar.anchorMin = new Vector2(0.5f, 0.5f);
            rtChar.anchorMax = new Vector2(0.5f, 0.5f);
            rtChar.sizeDelta = new Vector2(190f, 230f);
            rtChar.anchoredPosition = new Vector2(0f, 95f);
            var imgChar = charShowcase.AddComponent<Image>();
            imgChar.sprite = LoadSprite("Assets/HITME/Art/Characters/NamLang/namlang_dep_showcase.png");

            // Cycle Chevrons (Left and Right of Hero at waist level, >= 44 units)
            CreateIconButton(safeArea.transform, "BtnCycleLeft", new Vector2(32f, 40f), LoadSprite("Assets/HITME/Art/UI/Icons/chevron_left.png"), 44f);
            CreateIconButton(safeArea.transform, "BtnCycleRight", new Vector2(358f, 40f), LoadSprite("Assets/HITME/Art/UI/Icons/chevron_right.png"), 44f);

            // --- 4. CHARACTER & WEAPON SELECTOR CARDS (Y ~ -95 from center) ---
            CreateSelectorCard(safeArea.transform, "CharSelectorCard", new Vector2(-88f, -95f), "NHÂN VẬT", "Nam Làng", LoadSprite("Assets/HITME/Art/UI/Portraits/portrait_namlang.png"), tmpFont, false);
            CreateSelectorCard(safeArea.transform, "WeapSelectorCard", new Vector2(88f, -95f), "VŨ KHÍ", "Dép", LoadSprite("Assets/HITME/Art/UI/Icons/weapon_dep.png"), tmpFont, true);

            // --- 5. ACTION BUTTONS (Giant CTA, Secondary Row, Bot Training) ---
            // A. Primary Giant CTA "CHƠI NHANH" (Width 350, Height 58, touch target >= 48)
            CreateMenuCTAButton(safeArea.transform, "BtnQuickPlay", new Vector2(0f, 192f), new Vector2(350f, 58f), "CHƠI NHANH", "Online • Tối đa 6 người", tmpFont);

            // B. Secondary Row: TẠO PHÒNG (Blue) & VÀO PHÒNG (Green) (Width 170, Height 48, touch target >= 44)
            CreateMenuActionButton(safeArea.transform, "BtnCreateRoom", new Vector2(-88f, 132f), new Vector2(170f, 48f), "btn_blue_normal.png", "TẠO PHÒNG", LoadSprite("Assets/HITME/Art/UI/Icons/person_add.png"), tmpFont);
            CreateMenuActionButton(safeArea.transform, "BtnJoinRoom", new Vector2(88f, 132f), new Vector2(170f, 48f), "btn_green_normal.png", "VÀO PHÒNG", LoadSprite("Assets/HITME/Art/UI/Icons/people.png"), tmpFont);

            // C. Tertiary Button "LUYỆN VỚI BOT" (Width 350, Height 46, touch target >= 44)
            CreateMenuBotButton(safeArea.transform, "BtnBotTraining", new Vector2(0f, 78f), new Vector2(350f, 46f), "LUYỆN VỚI BOT", tmpFont);

            // --- 6. BOTTOM NAVIGATION DOCK (Trang chủ, Xếp hạng, Nhiệm vụ) ---
            var dockGo = new GameObject("BottomNavigationDock");
            dockGo.transform.SetParent(safeArea.transform, false);
            var rtDock = dockGo.AddComponent<RectTransform>();
            rtDock.anchorMin = new Vector2(0.5f, 0f);
            rtDock.anchorMax = new Vector2(0.5f, 0f);
            rtDock.sizeDelta = new Vector2(366f, 58f);
            rtDock.anchoredPosition = new Vector2(0f, 2f);
            var imgDock = dockGo.AddComponent<Image>();
            imgDock.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/dock_bg.png");
            imgDock.type = Image.Type.Sliced;
            imgDock.pixelsPerUnitMultiplier = 3.0f;
            imgDock.color = new Color32(14, 24, 40, 245);

            CreateDockTab(dockGo.transform, "TabHome", new Vector2(-110f, 0f), "Trang chủ", LoadSprite("Assets/HITME/Art/UI/Icons/home.png"), tmpFont, true);
            CreateDockTab(dockGo.transform, "TabRank", new Vector2(0f, 0f), "Xếp hạng", LoadSprite("Assets/HITME/Art/UI/Icons/trophy.png"), tmpFont, false);
            CreateDockTab(dockGo.transform, "TabQuests", new Vector2(110f, 0f), "Nhiệm vụ", LoadSprite("Assets/HITME/Art/UI/Icons/gift.png"), tmpFont, false);

            return canvasGo;
        }

        private static GameObject CreateSelectorCard(Transform parent, string name, Vector2 pos, string title, string subtitle, Sprite icon, TMP_FontAsset font, bool isWeapon)
        {
            var card = new GameObject(name);
            card.transform.SetParent(parent, false);
            var rt = card.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(172f, 66f);
            rt.anchoredPosition = pos;

            var bg = card.AddComponent<Image>();
            bg.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/panel_card_dark_gold.png");
            bg.type = Image.Type.Sliced;
            bg.pixelsPerUnitMultiplier = 3.0f;
            bg.color = new Color32(16, 26, 42, 250);

            // Icon / Portrait
            if (icon != null)
            {
                var iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(card.transform, false);
                var rtIcon = iconGo.AddComponent<RectTransform>();
                rtIcon.anchorMin = new Vector2(0f, 0.5f);
                rtIcon.anchorMax = new Vector2(0f, 0.5f);
                rtIcon.sizeDelta = isWeapon ? new Vector2(36f, 36f) : new Vector2(40f, 40f);
                rtIcon.anchoredPosition = isWeapon ? new Vector2(36f, 0f) : new Vector2(26f, 0f);
                var imgIcon = iconGo.AddComponent<Image>();
                imgIcon.sprite = icon;
            }

            // Title & Subtitle
            float textStartX = isWeapon ? 62f : 54f;
            var txtTitle = CreateTMPText(card.transform, "Title", title, font, 9, TextAlignmentOptions.Left, new Color32(255, 215, 0, 255), FontStyles.Bold);
            txtTitle.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            txtTitle.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            txtTitle.rectTransform.sizeDelta = new Vector2(95f, 18f);
            txtTitle.rectTransform.anchoredPosition = new Vector2(textStartX + 47f, 10f);

            var txtSub = CreateTMPText(card.transform, "Subtitle", subtitle, font, 12, TextAlignmentOptions.Left, Color.white, FontStyles.Bold);
            txtSub.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            txtSub.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            txtSub.rectTransform.sizeDelta = new Vector2(95f, 20f);
            txtSub.rectTransform.anchoredPosition = new Vector2(textStartX + 47f, -8f);

            // Chevrons
            if (isWeapon)
            {
                CreateChevron(card.transform, "ChevLeft", new Vector2(10f, 0f), LoadSprite("Assets/HITME/Art/UI/Icons/chevron_left.png"));
                CreateChevron(card.transform, "ChevRight", new Vector2(-12f, 0f), LoadSprite("Assets/HITME/Art/UI/Icons/chevron_right.png"));
            }
            else
            {
                CreateChevron(card.transform, "ChevRight", new Vector2(-12f, 0f), LoadSprite("Assets/HITME/Art/UI/Icons/chevron_right.png"));
            }

            return card;
        }

        private static void CreateChevron(Transform parent, string name, Vector2 pos, Sprite sprite)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = pos.x > 0 ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
            rt.anchorMax = pos.x > 0 ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
            rt.sizeDelta = new Vector2(16f, 16f);
            rt.anchoredPosition = pos;
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = new Color32(255, 215, 0, 220);
        }

        private static GameObject CreateMenuCTAButton(Transform parent, string name, Vector2 pos, Vector2 size, string title, string sub, TMP_FontAsset font)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            var img = btnGo.AddComponent<Image>();
            img.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/btn_primary_yellow_normal.png");
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 3.0f;
            btnGo.AddComponent<Button>();

            // Play Triangle Icon
            var playIcon = new GameObject("PlayIcon");
            playIcon.transform.SetParent(btnGo.transform, false);
            var rtPI = playIcon.AddComponent<RectTransform>();
            rtPI.anchorMin = new Vector2(0f, 0.5f);
            rtPI.anchorMax = new Vector2(0f, 0.5f);
            rtPI.sizeDelta = new Vector2(26f, 26f);
            rtPI.anchoredPosition = new Vector2(40f, 0f);
            var imgPI = playIcon.AddComponent<Image>();
            imgPI.sprite = LoadSprite("Assets/HITME/Art/UI/Icons/play_outline.png");
            imgPI.color = new Color32(65, 35, 0, 255);

            var txtTitle = CreateTMPText(btnGo.transform, "Title", title, font, 20, TextAlignmentOptions.Center, new Color32(65, 35, 0, 255), FontStyles.Bold);
            txtTitle.rectTransform.anchoredPosition = new Vector2(15f, 8f);

            var txtSub = CreateTMPText(btnGo.transform, "Sub", sub, font, 11, TextAlignmentOptions.Center, new Color32(95, 55, 10, 230), FontStyles.Bold);
            txtSub.rectTransform.anchoredPosition = new Vector2(15f, -12f);

            return btnGo;
        }

        private static GameObject CreateMenuActionButton(Transform parent, string name, Vector2 pos, Vector2 size, string skinName, string label, Sprite icon, TMP_FontAsset font)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            var img = btnGo.AddComponent<Image>();
            img.sprite = LoadSprite($"Assets/HITME/Art/UI/Skins/{skinName}");
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 3.0f;
            btnGo.AddComponent<Button>();

            if (icon != null)
            {
                var iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(btnGo.transform, false);
                var rtIcon = iconGo.AddComponent<RectTransform>();
                rtIcon.anchorMin = new Vector2(0f, 0.5f);
                rtIcon.anchorMax = new Vector2(0f, 0.5f);
                rtIcon.sizeDelta = new Vector2(22f, 22f);
                rtIcon.anchoredPosition = new Vector2(24f, 0f);
                var imgIcon = iconGo.AddComponent<Image>();
                imgIcon.sprite = icon;
                imgIcon.color = Color.white;
            }

            var txt = CreateTMPText(btnGo.transform, "Label", label, font, 14, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            txt.rectTransform.anchoredPosition = new Vector2(12f, 0f);

            return btnGo;
        }

        private static GameObject CreateMenuBotButton(Transform parent, string name, Vector2 pos, Vector2 size, string label, TMP_FontAsset font)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            var img = btnGo.AddComponent<Image>();
            img.sprite = LoadSprite("Assets/HITME/Art/UI/Skins/btn_navy_normal.png");
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 3.0f;
            btnGo.AddComponent<Button>();

            var botIcon = new GameObject("BotIcon");
            botIcon.transform.SetParent(btnGo.transform, false);
            var rtBI = botIcon.AddComponent<RectTransform>();
            rtBI.anchorMin = new Vector2(0f, 0.5f);
            rtBI.anchorMax = new Vector2(0f, 0.5f);
            rtBI.sizeDelta = new Vector2(24f, 24f);
            rtBI.anchoredPosition = new Vector2(28f, 0f);
            var imgBI = botIcon.AddComponent<Image>();
            imgBI.sprite = LoadSprite("Assets/HITME/Art/UI/Icons/bot.png");

            var txt = CreateTMPText(btnGo.transform, "Label", label, font, 14, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
            txt.rectTransform.anchoredPosition = new Vector2(0f, 0f);

            CreateChevron(btnGo.transform, "ChevRight", new Vector2(-18f, 0f), LoadSprite("Assets/HITME/Art/UI/Icons/chevron_right.png"));

            return btnGo;
        }

        private static void CreateDockTab(Transform parent, string name, Vector2 pos, string label, Sprite icon, TMP_FontAsset font, bool isSelected)
        {
            var tab = new GameObject(name);
            tab.transform.SetParent(parent, false);
            var rt = tab.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(90f, 50f);
            rt.anchoredPosition = pos;

            tab.AddComponent<Button>();

            if (icon != null)
            {
                var iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(tab.transform, false);
                var rtIcon = iconGo.AddComponent<RectTransform>();
                rtIcon.sizeDelta = new Vector2(22f, 22f);
                rtIcon.anchoredPosition = new Vector2(0f, 8f);
                var imgIcon = iconGo.AddComponent<Image>();
                imgIcon.sprite = icon;
                imgIcon.color = isSelected ? new Color32(0, 240, 255, 255) : new Color32(160, 180, 200, 200);
            }

            var txt = CreateTMPText(tab.transform, "Label", label, font, 10, TextAlignmentOptions.Center, isSelected ? new Color32(0, 240, 255, 255) : new Color32(160, 180, 200, 200), FontStyles.Bold);
            txt.rectTransform.anchoredPosition = new Vector2(0f, -12f);

            if (isSelected)
            {
                var lineGo = new GameObject("SelectedLine");
                lineGo.transform.SetParent(tab.transform, false);
                var rtLine = lineGo.AddComponent<RectTransform>();
                rtLine.sizeDelta = new Vector2(40f, 3f);
                rtLine.anchoredPosition = new Vector2(0f, -22f);
                var imgLine = lineGo.AddComponent<Image>();
                imgLine.sprite = LoadSprite("Assets/HITME/Art/UI/Controls/nav_underline_cyan.png");
                imgLine.color = new Color32(0, 240, 255, 255);
            }
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
            btnGo.AddComponent<Button>();

            if (icon != null)
            {
                var iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(btnGo.transform, false);
                var rtIcon = iconGo.AddComponent<RectTransform>();
                rtIcon.sizeDelta = new Vector2(size * 0.55f, size * 0.55f);
                var imgIcon = iconGo.AddComponent<Image>();
                imgIcon.sprite = icon;
                imgIcon.color = name.Contains("Cycle") ? new Color32(255, 215, 0, 255) : Color.white;
            }
            return btnGo;
        }

        private static TextMeshProUGUI CreateTMPText(Transform parent, string name, string text, TMP_FontAsset font, float fontSize, TextAlignmentOptions alignment, Color color, FontStyles style = FontStyles.Normal)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            return tmp;
        }

        // ==========================================
        // 6. SAVE PREFABS
        // ==========================================
        private static void SaveStagingPrefabs(GameObject combatFloorGo, GameObject envGo, GameObject hudGo, GameObject menuGo)
        {
            Directory.CreateDirectory("Assets/HITME/Prefabs/Arena");
            Directory.CreateDirectory("Assets/HITME/Prefabs/Presentation");

            PrefabUtility.SaveAsPrefabAsset(combatFloorGo, "Assets/HITME/Prefabs/Arena/Arena_CombatFloor.prefab");
            PrefabUtility.SaveAsPrefabAsset(envGo, "Assets/HITME/Prefabs/Arena/Arena_EnvironmentDecor.prefab");
            PrefabUtility.SaveAsPrefabAsset(hudGo, "Assets/HITME/Prefabs/Presentation/InMatchHUD_Compact.prefab");
            PrefabUtility.SaveAsPrefabAsset(menuGo, "Assets/HITME/Prefabs/Presentation/MainMenu_VisualStaging.prefab");

            Debug.Log("[ArenaStagingBuilder] All 4 prefabs saved successfully to Assets/HITME/Prefabs/");
        }

        // ==========================================
        // 7. CAPTURE 4 9:16 SCREENSHOTS (1080 x 1920)
        // ==========================================
        private static void CaptureVisualScreenshots(Camera cam, GameObject floor, GameObject villageEnv, GameObject matchDecor, GameObject actors, GameObject hud, GameObject menu)
        {
            const int width = 1080;
            const int height = 1920;

            // 1. Arena Floor Only
            SetGroupActive(floor, true, villageEnv, false, matchDecor, false, actors, false, hud, false, menu, false);
            CaptureCameraToFile(cam, width, height, Path.Combine(EvidenceDir, "unity_01_arena_floor_only.png"));

            // 2. Arena With Environment (Match decor surroundings)
            SetGroupActive(floor, true, villageEnv, false, matchDecor, true, actors, false, hud, false, menu, false);
            CaptureCameraToFile(cam, width, height, Path.Combine(EvidenceDir, "unity_02_arena_with_environment.png"));

            // 3. Main Menu Staging (Village Courtyard + Modular Canvas UI)
            SetGroupActive(floor, false, villageEnv, true, matchDecor, false, actors, false, hud, false, menu, true);
            CaptureCameraToFile(cam, width, height, Path.Combine(EvidenceDir, "unity_03_main_menu_staging.png"));

            // 4. In-Match HUD Preview (Match in progress: Arena + Match Decor + Actors/Aim + HUD)
            SetGroupActive(floor, true, villageEnv, false, matchDecor, true, actors, true, hud, true, menu, false);
            CaptureCameraToFile(cam, width, height, Path.Combine(EvidenceDir, "unity_04_compact_hud_preview.png"));

            // Restore for editor inspection
            SetGroupActive(floor, true, villageEnv, false, matchDecor, true, actors, true, hud, true, menu, false);
        }

        private static void SetGroupActive(GameObject f, bool af, GameObject ve, bool ave, GameObject me, bool ame, GameObject a, bool aa, GameObject h, bool ah, GameObject m, bool am)
        {
            if (f != null) f.SetActive(af);
            if (ve != null) ve.SetActive(ave);
            if (me != null) me.SetActive(ame);
            if (a != null) a.SetActive(aa);
            if (h != null) h.SetActive(ah);
            if (m != null) m.SetActive(am);
        }

        private static void CaptureCameraToFile(Camera cam, int w, int h, string outputPath)
        {
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            var prevRT = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();

            cam.targetTexture = prevRT;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);

            byte[] bytes = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);

            File.WriteAllBytes(outputPath, bytes);
            Debug.Log($"[ArenaStagingBuilder] Rendered evidence screenshot: {outputPath} ({w}x{h})");
        }
    }
}
