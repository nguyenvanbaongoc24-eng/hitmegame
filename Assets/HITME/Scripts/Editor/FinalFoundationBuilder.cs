using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HitMe.Core;
using HitMe.Gameplay;
using HitMe.Presentation;
using HitMe.UI;

namespace HitMe.Editor
{
    public static class FinalFoundationBuilder
    {
        private const string ScenePath = "Assets/Scenes/MainMatch.unity";
        private const string Normal = "Assets/HITME/Art/Characters/NamLang/BÌNH THƯỜNG/sprites/";
        private const string DepHeld = "Assets/HITME/Art/Characters/NamLang/CẦM DÉP/sprites/";
        private const string DepWeapon = "Assets/HITME/Art/Weapons/DepToOng/sprites/";

        [MenuItem("Tools/HIT ME/FINAL/Build Visual Foundation")]
        public static void BuildVisualFoundation()
        {
            ConfigureFinalImports();
            EnsureSortingLayers("ArenaFar", "ArenaMid", "CombatField", "ArenaDecor", "Actors", "Projectiles", "CombatVFX", "HUD");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var wind = EnsureWindSettings();
            BuildArena(wind);
            BindNamLangAndDep();
            BuildModularUi();
            BindPresentationAdapter();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("HIT ME FINAL visual foundation built into MainMatch. Existing gameplay state machine was preserved.");
        }

        [MenuItem("Tools/HIT ME/FINAL/Configure Imports")]
        public static void ConfigureFinalImports()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/HITME/Art" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.spritePixelsPerUnit = (path.Contains("Characters/NamLang") || path.Contains("Art/UI/")) ? 100f : 50f;
                importer.spritePivot = path.Contains("Characters/NamLang") ? new Vector2(.5f, 0f) : new Vector2(.5f, .5f);

                // Configure 9-slice borders for UI skins (PPU 100 native)
                if (path.Contains("btn_primary_yellow")) importer.spriteBorder = new Vector4(45, 45, 45, 45);
                else if (path.Contains("btn_blue") || path.Contains("btn_green")) importer.spriteBorder = new Vector4(35, 35, 35, 35);
                else if (path.Contains("btn_navy")) importer.spriteBorder = new Vector4(35, 35, 35, 35);
                else if (path.Contains("btn_circle_gold") || path.Contains("btn_circle_navy")) importer.spriteBorder = new Vector4(45, 45, 45, 45);
                else if (path.Contains("panel_card_dark")) importer.spriteBorder = new Vector4(35, 35, 35, 35);
                else if (path.Contains("panel_profile_strip")) importer.spriteBorder = new Vector4(28, 28, 28, 28);
                else if (path.Contains("pill_coin")) importer.spriteBorder = new Vector4(26, 26, 26, 26);
                else if (path.Contains("dock_bg")) importer.spriteBorder = new Vector4(40, 40, 40, 40);
                else if (path.Contains("panel_toast")) importer.spriteBorder = new Vector4(24, 24, 24, 24);
                else if (path.Contains("avatar_ring_cyan")) importer.spriteBorder = new Vector4(48, 48, 48, 48);

                var webGl = importer.GetPlatformTextureSettings("WebGL");
                webGl.name = "WebGL"; webGl.overridden = true; webGl.maxTextureSize = 1024;
                importer.SetPlatformTextureSettings(webGl);
                importer.SaveAndReimport();
            }
            AssetDatabase.Refresh();
        }

        private static void BuildArena(GlobalWindSettings wind)
        {
            var prior = GameObject.Find("FinalArena_2_5D");
            if (prior != null) Object.DestroyImmediate(prior);
            // This root owns the existing, full arena surface and its established gameplay
            // alignment. It is a valid runtime backdrop, not a UI mock-up. Keep it behind
            // the FINAL detail layers so transparent FINAL sprites never expose a blank clear.
            var oldArena = FindSceneObjectIncludingInactive("ArenaRoot_25D");
            if (oldArena != null) oldArena.SetActive(true);
            var root = new GameObject("FinalArena_2_5D");

            // FINAL layers are intentionally sized from their actual authored bounds. The
            // previous full-bleed tile was a small sprite scaled to hundreds of world units,
            // which hid the arena and produced the flat tan gameplay screen in WebGL.
            CreateSprite(root.transform, "FarMountains", "Assets/HITME/Art/Environment/FestivalArena/HITME_A01_Far_Mountains.png", new Vector3(0f, 3.0f, 0f), Vector3.one * 1.35f, "ArenaFar", 0);
            CreateSprite(root.transform, "VillageRooftops", "Assets/HITME/Art/Environment/FestivalArena/HITME_A02_Village_Rooftops.png", new Vector3(0f, 2.1f, 0f), Vector3.one * 1.4f, "ArenaFar", 1);
            CreateSprite(root.transform, "TempleRoof", "Assets/HITME/Art/Environment/FestivalArena/HITME_A03_Temple_Roof.png", new Vector3(0f, 1.6f, 0f), Vector3.one * 1.4f, "ArenaMid", 0);
            CreateSprite(root.transform, "CombatField", "Assets/HITME/Art/Environment/FestivalArena/HITME_A07_Stone_Tile_Floor.png", new Vector3(0f, -0.25f, 0f), Vector3.one * 1.85f, "CombatField", 0);
            CreateSprite(root.transform, "ArenaBorder", "Assets/HITME/Art/Environment/FestivalArena/HITME_A08_Arena_Border.png", new Vector3(0f, -0.22f, 0f), Vector3.one * 1.85f, "CombatField", 2);
            CreateSprite(root.transform, "LotusEmblem", "Assets/HITME/Art/Environment/FestivalArena/HITME_A09_Lotus_Emblem.png", new Vector3(0f, -0.1f, 0f), Vector3.one * 1.35f, "CombatField", 3);

            // Decorations, crowds and wind sways.
            CreateSprite(root.transform, "LeftCrowd", "Assets/HITME/Art/Environment/FestivalArena/HITME_C07_Left_Crowd.png", new Vector3(-3.05f, 1.4f, 0f), Vector3.one * .95f, "ArenaDecor", 0);
            CreateSprite(root.transform, "RightCrowd", "Assets/HITME/Art/Environment/FestivalArena/HITME_C08_Right_Crowd.png", new Vector3(3.05f, 1.4f, 0f), Vector3.one * .95f, "ArenaDecor", 1);
            CreatePivotSprite(root.transform, "LeftTreePivot", "Assets/HITME/Art/Environment/FestivalArena/HITME_C01_Left_Tree_Canopy.png", new Vector3(-4.2f, -4.7f, 0f), .9f, "ArenaDecor", 4, wind, .2f);
            CreatePivotSprite(root.transform, "RightTreePivot", "Assets/HITME/Art/Environment/FestivalArena/HITME_C02_Right_Tree_Canopy.png", new Vector3(4.2f, -4.7f, 0f), .9f, "ArenaDecor", 5, wind, 1.7f);
            CreateSprite(root.transform, "ForegroundLeafShadow", "Assets/HITME/Art/Environment/FestivalArena/HITME_C10_Leaf_Shadow.png", new Vector3(0f, -1.5f, 0f), Vector3.one * 1.55f, "ArenaDecor", 9);
            CreateLantern(root.transform, "LanternLeft", new Vector3(-3.8f, 3.7f, 0f), wind, .1f);
            CreateLantern(root.transform, "LanternRight", new Vector3(3.8f, 3.7f, 0f), wind, 2.5f);

            var camera = Camera.main;
            if (camera != null)
            {
                var controller = camera.GetComponent<CameraController25D>();
                if (controller != null) controller.enabled = false;
                camera.orthographic = true;
                camera.orthographicSize = 6.15f;
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.transform.rotation = Quaternion.identity;
                camera.backgroundColor = new Color(.08f, .15f, .18f);
            }
        }

        private static void BindNamLangAndDep()
        {
            var idle = SpriteAt(Normal + "sprite_001.png");
            var held = SpriteAt(DepHeld + "sprite_001.png");
            var projectile = SpriteAt(DepWeapon + "sprite_001.png");
            foreach (var actor in Object.FindObjectsByType<ActorView>(FindObjectsSortMode.None))
            {
                actor.spriteIdle = idle;
                actor.spriteMove = null; actor.spriteThrowWindup = null; actor.spriteThrowRelease = null;
                actor.spriteHit = null; actor.spriteVictory = null; actor.spriteDefeat = null;
                actor.spriteAim = held != null ? held : idle;
                ReplaceWithFootAnchoredVisual(actor, idle);
                if (actor.shadowRenderer != null) { actor.shadowRenderer.sortingLayerName = "Actors"; actor.shadowRenderer.sortingOrder = 9; }
                
                // Hide actors by default in Main Menu (they appear once match starts)
                actor.SetVisibility(false);
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
            if (prefab != null)
            {
                var renderer = prefab.GetComponent<SpriteRenderer>();
                if (renderer != null) { renderer.sprite = projectile; renderer.sortingLayerName = "Projectiles"; renderer.sortingOrder = 0; }
                EditorUtility.SetDirty(prefab);
            }
            var data = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/ScriptableObjects/Characters/NamLang.asset");
            if (data != null) { data.spriteIdle = idle; data.spriteAim = held; data.spriteMove = data.spriteThrowWindup = data.spriteThrowRelease = data.spriteHit = data.spriteVictory = data.spriteDefeat = null; EditorUtility.SetDirty(data); }
            var weapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/ScriptableObjects/Weapons/DepToOng.asset");
            if (weapon != null) { weapon.projectileSprite = projectile; EditorUtility.SetDirty(weapon); }
        }

        private static void BuildModularUi()
        {
            var canvas = Object.FindFirstObjectByType<Canvas>();
            var ui = Object.FindFirstObjectByType<UIManager>();
            if (canvas == null || ui == null) return;

            // Responsive Canvas Scaler (1080 x 2340 / iPhone 15 Mobile-First)
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 2340);
                scaler.matchWidthOrHeight = 0.5f;
                scaler.referencePixelsPerUnit = 100f;
            }

            var root = canvas.transform.Find("SafeAreaRoot") as RectTransform;
            if (root == null)
            {
                var safe = new GameObject("SafeAreaRoot", typeof(RectTransform), typeof(SafeAreaFitter));
                root = safe.GetComponent<RectTransform>(); root.SetParent(canvas.transform, false);
                root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one; root.offsetMin = root.offsetMax = Vector2.zero;
                var children = new List<Transform>();
                for (var i = 0; i < canvas.transform.childCount; i++) if (canvas.transform.GetChild(i) != root) children.Add(canvas.transform.GetChild(i));
                foreach (var child in children) child.SetParent(root, true);
            }

            EnsureEventSystem();

            // REBUILD MAIN MENU PANEL WITH PRISTINE CLASH ROYALE HIERARCHY
            if (ui.mainMenuPanel != null)
            {
                var menuRt = ui.mainMenuPanel.GetComponent<RectTransform>();
                menuRt.anchorMin = Vector2.zero; menuRt.anchorMax = Vector2.one;
                menuRt.offsetMin = menuRt.offsetMax = Vector2.zero;

                var menuImage = ui.mainMenuPanel.GetComponent<Image>();
                if (menuImage != null)
                {
                    menuImage.sprite = null;
                    menuImage.color = new Color(0f, 0f, 0f, 0.15f); // Soft dark vignette, allowing vibrant arena background to shine through
                    menuImage.raycastTarget = false;
                }

                // Clean old children
                for (int i = ui.mainMenuPanel.transform.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(ui.mainMenuPanel.transform.GetChild(i).gameObject);
                }

                // 1. TopProfileBar
                var topBar = CreateUIContainer("TopProfileBar", ui.mainMenuPanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -85f), new Vector2(980f, 110f));
                CreateSlicedImage("ProfileStrip", topBar.transform, "Assets/HITME/Art/UI/Skins/panel_profile_strip.png", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(980f, 110f));

                var avatarRing = CreateSlicedImage("AvatarRing", topBar.transform, "Assets/HITME/Art/UI/Skins/avatar_ring_cyan.png", new Vector2(0f, 0.5f), new Vector2(65f, 0f), new Vector2(96f, 96f));
                CreateSimpleImage("Portrait", avatarRing.transform, "Assets/HITME/Art/UI/Portraits/portrait_namlang.png", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(76f, 76f));

                var nameTxt = CreateUIText("PlayerName", topBar.transform, "Nam Làng", 30, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(130f, 14f), new Vector2(240f, 36f));
                nameTxt.color = Color.white;
                var subTxt = CreateUIText("PlayerSub", topBar.transform, "Hồ sơ", 20, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(130f, -18f), new Vector2(240f, 28f));
                subTxt.color = new Color(0.2f, 0.9f, 1f);

                var coinPill = CreateSlicedImage("CoinPill", topBar.transform, "Assets/HITME/Art/UI/Skins/pill_coin.png", new Vector2(1f, 0.5f), new Vector2(-160f, 0f), new Vector2(220f, 68f));
                CreateSimpleImage("CoinIcon", coinPill.transform, "Assets/HITME/Art/UI/Icons/coin_lotus.png", new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(48f, 48f));
                var coinTxt = CreateUIText("CoinBalance", coinPill.transform, "1.250", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(10f, 0f), new Vector2(110f, 36f));
                coinTxt.color = new Color(1f, 0.85f, 0.2f);
                CreateSimpleImage("PlusBtn", coinPill.transform, "Assets/HITME/Art/UI/Skins/btn_circle_plus.png", new Vector2(1f, 0.5f), new Vector2(-28f, 0f), new Vector2(44f, 44f));

                var settingsBtn = CreateSimpleImage("SettingsBtn", topBar.transform, "Assets/HITME/Art/UI/Skins/btn_circle_gold.png", new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(76f, 76f));
                CreateSimpleImage("GearIcon", settingsBtn.transform, "Assets/HITME/Art/UI/Icons/gear.png", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(40f, 40f));

                // 2. Official 3D HIT ME Logo
                CreateSimpleImage("LogoHitMe", ui.mainMenuPanel.transform, "Assets/HITME/Art/UI/Logo/logo_hit_me.png", new Vector2(0.5f, 1f), new Vector2(0f, -235f), new Vector2(580f, 230f));

                // 3. Character Showcase (Properly Centered with Contact Shadow & Lotus Podium)
                var showcase = CreateUIContainer("CharacterShowcase", ui.mainMenuPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 100f), new Vector2(700f, 640f));
                var podium = CreateSimpleImage("LotusPodium", showcase.transform, "Assets/HITME/Art/Environment/lotus_emblem_clean.png", new Vector2(0.5f, 0f), new Vector2(0f, 15f), new Vector2(460f, 150f));
                podium.color = new Color(1f, 1f, 1f, 0.9f);
                CreateSimpleImage("CharacterSprite", showcase.transform, "Assets/HITME/Art/Characters/NamLang/namlang_dep_showcase.png", new Vector2(0.5f, 0f), new Vector2(0f, 65f), new Vector2(460f, 500f));

                var arrLeft = CreateSimpleImage("ArrowLeft", showcase.transform, "Assets/HITME/Art/UI/Skins/btn_circle_gold.png", new Vector2(0f, 0.45f), new Vector2(30f, 0f), new Vector2(74f, 74f));
                CreateSimpleImage("Icon", arrLeft.transform, "Assets/HITME/Art/UI/Icons/chevron_left.png", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(36f, 36f));
                var arrRight = CreateSimpleImage("ArrowRight", showcase.transform, "Assets/HITME/Art/UI/Skins/btn_circle_gold.png", new Vector2(1f, 0.45f), new Vector2(-30f, 0f), new Vector2(74f, 74f));
                CreateSimpleImage("Icon", arrRight.transform, "Assets/HITME/Art/UI/Icons/chevron_right.png", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(36f, 36f));

                // 4. Character & Weapon Selection Cards
                var selectors = CreateUIContainer("Selectors", ui.mainMenuPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -260f), new Vector2(920f, 130f));
                var charCard = CreateSlicedImage("CharCard", selectors.transform, "Assets/HITME/Art/UI/Skins/panel_card_dark_gold.png", new Vector2(0.5f, 0.5f), new Vector2(-235f, 0f), new Vector2(430f, 125f));
                var charCardTxt = CreateUIText("Label", charCard.transform, "NHÂN VẬT\nNam Làng", 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 80f));
                charCardTxt.color = new Color(1f, 0.9f, 0.65f);

                var weaponCard = CreateSlicedImage("WeaponCard", selectors.transform, "Assets/HITME/Art/UI/Skins/panel_card_dark_gold.png", new Vector2(0.5f, 0.5f), new Vector2(235f, 0f), new Vector2(430f, 125f));
                CreateSimpleImage("WeaponIcon", weaponCard.transform, "Assets/HITME/Art/UI/Icons/weapon_dep.png", new Vector2(0f, 0.5f), new Vector2(55f, 0f), new Vector2(65f, 65f));
                var weaponCardTxt = CreateUIText("Label", weaponCard.transform, "VŨ KHÍ\nDép tổ ong", 24, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), new Vector2(50f, 0f), new Vector2(260f, 80f));
                weaponCardTxt.color = new Color(1f, 0.9f, 0.65f);

                // 5. Action Play Mode Buttons
                // CHƠI NHANH (Primary Yellow 3D)
                var playBtn = CreateSlicedButton("BtnQuickPlay", ui.mainMenuPanel.transform, "Assets/HITME/Art/UI/Skins/btn_primary_yellow_normal.png", new Vector2(0.5f, 0.5f), new Vector2(0f, -435f), new Vector2(900f, 160f));
                CreateSimpleImage("PlayIcon", playBtn.transform, "Assets/HITME/Art/UI/Icons/play_outline.png", new Vector2(0f, 0.5f), new Vector2(75f, 0f), new Vector2(56f, 56f));
                var playTxt = CreateUIText("PlayText", playBtn.transform, "CHƠI NHANH", 42, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(25f, 16f), new Vector2(650f, 52f));
                playTxt.color = new Color(0.25f, 0.12f, 0f);
                var playSub = CreateUIText("PlaySub", playBtn.transform, "Online • Tối đa 6 người", 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(25f, -22f), new Vector2(650f, 32f));
                playSub.color = new Color(0.4f, 0.25f, 0.05f);

                // LUYỆN VỚI BOT (Navy 3D Bar)
                var botBtn = CreateSlicedButton("BtnTrainWithBot", ui.mainMenuPanel.transform, "Assets/HITME/Art/UI/Skins/btn_navy_normal.png", new Vector2(0.5f, 0.5f), new Vector2(0f, -605f), new Vector2(900f, 125f));
                CreateSimpleImage("BotIcon", botBtn.transform, "Assets/HITME/Art/UI/Icons/bot.png", new Vector2(0f, 0.5f), new Vector2(70f, 0f), new Vector2(48f, 48f));
                var botTxt = CreateUIText("BotText", botBtn.transform, "LUYỆN VỚI BOT", 32, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(20f, 0f), new Vector2(650f, 44f));
                botTxt.color = Color.white;

                // TẠO PHÒNG & VÀO PHÒNG
                var createBtn = CreateSlicedButton("BtnCreateRoom", ui.mainMenuPanel.transform, "Assets/HITME/Art/UI/Skins/btn_blue_normal.png", new Vector2(0.5f, 0.5f), new Vector2(-235f, -740f), new Vector2(430f, 110f));
                CreateSimpleImage("Icon", createBtn.transform, "Assets/HITME/Art/UI/Icons/person_add.png", new Vector2(0f, 0.5f), new Vector2(50f, 0f), new Vector2(40f, 40f));
                var createTxt = CreateUIText("Text", createBtn.transform, "TẠO PHÒNG", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(20f, 0f), new Vector2(280f, 40f));
                createTxt.color = Color.white;

                var joinBtn = CreateSlicedButton("BtnJoinRoom", ui.mainMenuPanel.transform, "Assets/HITME/Art/UI/Skins/btn_green_normal.png", new Vector2(0.5f, 0.5f), new Vector2(235f, -740f), new Vector2(430f, 110f));
                CreateSimpleImage("Icon", joinBtn.transform, "Assets/HITME/Art/UI/Icons/people.png", new Vector2(0f, 0.5f), new Vector2(50f, 0f), new Vector2(40f, 40f));
                var joinTxt = CreateUIText("Text", joinBtn.transform, "VÀO PHÒNG", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(20f, 0f), new Vector2(280f, 40f));
                joinTxt.color = Color.white;

                // 6. Bottom Navigation Dock
                var dock = CreateSlicedImage("BottomDock", ui.mainMenuPanel.transform, "Assets/HITME/Art/UI/Skins/dock_bg.png", new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(980f, 135f));
                var tabHome = CreateUIContainer("TabHome", dock.transform, new Vector2(0.18f, 0.5f), new Vector2(0.18f, 0.5f), Vector2.zero, new Vector2(160f, 100f));
                CreateSimpleImage("Icon", tabHome.transform, "Assets/HITME/Art/UI/Icons/home.png", new Vector2(0.5f, 0.5f), new Vector2(0f, 14f), new Vector2(44f, 44f));
                var homeTxt = CreateUIText("Text", tabHome.transform, "Trang chủ", 20, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, -24f), new Vector2(140f, 26f));
                homeTxt.color = new Color(0.2f, 0.9f, 1f);

                var tabTrophy = CreateUIContainer("TabTrophy", dock.transform, new Vector2(0.50f, 0.5f), new Vector2(0.50f, 0.5f), Vector2.zero, new Vector2(160f, 100f));
                CreateSimpleImage("Icon", tabTrophy.transform, "Assets/HITME/Art/UI/Icons/trophy.png", new Vector2(0.5f, 0.5f), new Vector2(0f, 14f), new Vector2(44f, 44f));
                var trophyTxt = CreateUIText("Text", tabTrophy.transform, "Xếp hạng", 20, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, -24f), new Vector2(140f, 26f));
                trophyTxt.color = new Color(0.7f, 0.75f, 0.85f);

                var tabQuest = CreateUIContainer("TabQuest", dock.transform, new Vector2(0.82f, 0.5f), new Vector2(0.82f, 0.5f), Vector2.zero, new Vector2(160f, 100f));
                CreateSimpleImage("Icon", tabQuest.transform, "Assets/HITME/Art/UI/Icons/gift.png", new Vector2(0.5f, 0.5f), new Vector2(0f, 14f), new Vector2(44f, 44f));
                var questTxt = CreateUIText("Text", tabQuest.transform, "Nhiệm vụ", 20, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, -24f), new Vector2(140f, 26f));
                questTxt.color = new Color(0.7f, 0.75f, 0.85f);

                // Wire up buttons to authoritative UIManager
                ui.btnTrainWithBot = botBtn.GetComponent<Button>();
                ui.btnQuickMatch = playBtn.GetComponent<Button>();
                ui.btnCreateRoom = createBtn.GetComponent<Button>();
                EditorUtility.SetDirty(ui);
            }

            // REPAIR IN-MATCH HUD PANEL
            if (ui.inGameHUDPanel != null)
            {
                var top = ui.inGameHUDPanel.transform.Find("TopBar");
                if (top != null)
                {
                    var rect = top.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(.5f, 1f); rect.anchoredPosition = new Vector2(0f, -30f);
                    rect.sizeDelta = new Vector2(0f, 140f);
                    var image = top.GetComponent<Image>();
                    if (image != null) { image.sprite = null; image.color = new Color(.04f, .08f, .1f, .75f); }
                }

                var bottom = ui.inGameHUDPanel.transform.Find("BottomBar");
                if (bottom != null)
                {
                    var rect = bottom.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0f, 0f); rect.anchorMax = new Vector2(1f, 0f);
                    rect.pivot = new Vector2(.5f, 0f); rect.anchoredPosition = new Vector2(0f, 20f);
                    rect.sizeDelta = new Vector2(0f, 160f);
                    var image = bottom.GetComponent<Image>();
                    if (image != null) { image.sprite = null; image.color = new Color(.04f, .08f, .1f, .75f); }

                    // Style the ready button
                    var readyBtn = bottom.Find("BtnReady");
                    if (readyBtn != null)
                    {
                        var readyImg = readyBtn.GetComponent<Image>();
                        if (readyImg != null)
                        {
                            readyImg.sprite = SpriteAt("Assets/HITME/Art/UI/Skins/btn_primary_yellow_normal.png");
                            readyImg.type = Image.Type.Sliced;
                        }
                    }
                }
            }

            BuildResultPanel(ui);
        }

        private static void BuildResultPanel(UIManager ui)
        {
            if (ui.resultPanel == null) return;
            var panelImage = ui.resultPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.sprite = null;
                panelImage.color = new Color(0.015f, 0.025f, 0.04f, 0.78f);
                panelImage.raycastTarget = false;
            }

            for (var i = ui.resultPanel.transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(ui.resultPanel.transform.GetChild(i).gameObject);

            ui.txtResultTitle = CreateUIText("ResultTitle", ui.resultPanel.transform, "KẾT QUẢ", 54, TextAnchor.MiddleCenter, new Vector2(.5f, .5f), new Vector2(0f, 420f), new Vector2(920f, 90f));
            ui.txtResultSubtitle = CreateUIText("ResultSubtitle", ui.resultPanel.transform, "", 25, TextAnchor.MiddleCenter, new Vector2(.5f, .5f), new Vector2(0f, 335f), new Vector2(900f, 70f));
            ui.txtPlayerStats = CreateUIText("PlayerStats", ui.resultPanel.transform, "", 27, TextAnchor.MiddleCenter, new Vector2(.5f, .5f), new Vector2(0f, 120f), new Vector2(760f, 170f));
            ui.txtBotStats = CreateUIText("BotStats", ui.resultPanel.transform, "", 27, TextAnchor.MiddleCenter, new Vector2(.5f, .5f), new Vector2(0f, -110f), new Vector2(760f, 170f));
            ui.txtPlayerStats.color = new Color(.25f, .92f, 1f);
            ui.txtBotStats.color = new Color(1f, .48f, .48f);

            ui.btnRematch = CreateSlicedButton("BtnRematch", ui.resultPanel.transform, "Assets/HITME/Art/UI/Skins/btn_primary_yellow_normal.png", new Vector2(.5f, .5f), new Vector2(0f, -390f), new Vector2(820f, 125f));
            var rematchLabel = CreateUIText("Label", ui.btnRematch.transform, "CHƠI LẠI", 34, TextAnchor.MiddleCenter, new Vector2(.5f, .5f), Vector2.zero, new Vector2(620f, 60f));
            rematchLabel.color = new Color(.2f, .1f, 0f);
            ui.btnBackToHome = CreateSlicedButton("BtnBackToHome", ui.resultPanel.transform, "Assets/HITME/Art/UI/Skins/btn_navy_normal.png", new Vector2(.5f, .5f), new Vector2(0f, -540f), new Vector2(820f, 105f));
            CreateUIText("Label", ui.btnBackToHome.transform, "VỀ TRANG CHỦ", 30, TextAnchor.MiddleCenter, new Vector2(.5f, .5f), Vector2.zero, new Vector2(620f, 60f));
        }

        private static void BindPresentationAdapter()
        {
            var state = Object.FindFirstObjectByType<MatchStateMachine>();
            var aim = Object.FindFirstObjectByType<AimController>();
            var actors = Object.FindObjectsByType<ActorView>(FindObjectsSortMode.None);
            if (state == null || actors.Length < 2) return;
            ActorView player = null, bot = null;
            foreach (var a in actors) { if (a.actorType == ActorType.Player) player = a; else bot = a; }
            var adapter = state.GetComponent<MatchPresentationAdapter>();
            if (adapter == null) adapter = state.gameObject.AddComponent<MatchPresentationAdapter>();
            adapter.Bind(state, aim, player, bot, AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/ScriptableObjects/Characters/NamLang.asset"), AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/ScriptableObjects/Weapons/DepToOng.asset"));
            if (state.GetComponent<WebGLRuntimeDiagnostics>() == null) state.gameObject.AddComponent<WebGLRuntimeDiagnostics>();
        }

        private static GlobalWindSettings EnsureWindSettings()
        {
            const string path = "Assets/HITME/ScriptableObjects/GlobalWindSettings.asset";
            var settings = AssetDatabase.LoadAssetAtPath<GlobalWindSettings>(path);
            if (settings == null)
            {
                System.IO.Directory.CreateDirectory("Assets/HITME/ScriptableObjects");
                settings = ScriptableObject.CreateInstance<GlobalWindSettings>();
                AssetDatabase.CreateAsset(settings, path);
            }
            return settings;
        }

        private static void CreateSprite(Transform parent, string name, string path, Vector3 pos, Vector3 scale, string layer, int order)
        {
            var sprite = SpriteAt(path); if (sprite == null) return;
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localScale = scale;
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingLayerName = layer; renderer.sortingOrder = order;
        }

        private static void CreatePivotSprite(Transform parent, string name, string path, Vector3 pos, float scale, string layer, int order, GlobalWindSettings wind, float phase)
        {
            var sprite = SpriteAt(path); if (sprite == null) return;
            var pivot = new GameObject(name); pivot.transform.SetParent(parent, false); pivot.transform.localPosition = pos; pivot.AddComponent<FoliageSway>();
            var sway = pivot.GetComponent<FoliageSway>(); var so = new SerializedObject(sway); so.FindProperty("wind").objectReferenceValue = wind; so.FindProperty("phase").floatValue = phase; so.ApplyModifiedPropertiesWithoutUndo();
            var child = new GameObject("Sprite"); child.transform.SetParent(pivot.transform, false); child.transform.localPosition = new Vector3(0f, sprite.bounds.extents.y * scale, 0f); child.transform.localScale = Vector3.one * scale;
            var renderer = child.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingLayerName = layer; renderer.sortingOrder = order;
        }

        private static void CreateLantern(Transform parent, string name, Vector3 pos, GlobalWindSettings wind, float phase)
        {
            var sprite = SpriteAt("Assets/HITME/Art/Environment/FestivalArena/HITME_B03_Red_Lantern.png"); if (sprite == null) return;
            var pivot = new GameObject(name); pivot.transform.SetParent(parent, false); pivot.transform.localPosition = pos; pivot.AddComponent<PivotSwing>();
            var swing = pivot.GetComponent<PivotSwing>(); var so = new SerializedObject(swing); so.FindProperty("phase").floatValue = phase; so.ApplyModifiedPropertiesWithoutUndo();
            var child = new GameObject("Sprite"); child.transform.SetParent(pivot.transform, false); child.transform.localPosition = new Vector3(0f, -sprite.bounds.extents.y * .7f, 0f); child.transform.localScale = Vector3.one * .8f;
            var renderer = child.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingLayerName = "ArenaDecor"; renderer.sortingOrder = 8;
        }

        private static Sprite SpriteAt(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);

        private static GameObject FindSceneObjectIncludingInactive(string objectName)
        {
            foreach (var candidate in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (candidate.name == objectName && candidate.gameObject.scene.IsValid()) return candidate.gameObject;
            }
            return null;
        }

        private static void ReplaceWithFootAnchoredVisual(ActorView actor, Sprite idle)
        {
            var existing = actor.characterRenderer;
            if (existing == null) return;
            var visual = actor.transform.Find("FinalCharacterVisual");
            SpriteRenderer renderer;
            if (visual == null)
            {
                var go = new GameObject("FinalCharacterVisual"); go.transform.SetParent(actor.transform, false);
                renderer = go.AddComponent<SpriteRenderer>();
                renderer.color = existing.color; renderer.flipX = existing.flipX;
                Object.DestroyImmediate(existing);
            }
            else renderer = visual.GetComponent<SpriteRenderer>();
            renderer.sprite = idle; renderer.sortingLayerName = "Actors"; renderer.sortingOrder = 10;
            renderer.transform.localPosition = new Vector3(0f, idle != null ? idle.bounds.extents.y : 0f, 0f);
            actor.characterRenderer = renderer;
        }

        private static GameObject CreateUIContainer(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos; rt.sizeDelta = sizeDelta;
            return go;
        }

        private static Image CreateSimpleImage(string name, Transform parent, string spritePath, Vector2 anchor, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = CreateUIContainer(name, parent, anchor, anchor, anchoredPos, sizeDelta);
            var img = go.AddComponent<Image>();
            img.sprite = SpriteAt(spritePath);
            img.preserveAspect = true;
            img.raycastTarget = false;
            return img;
        }

        private static Image CreateSlicedImage(string name, Transform parent, string spritePath, Vector2 anchor, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = CreateUIContainer(name, parent, anchor, anchor, anchoredPos, sizeDelta);
            var img = go.AddComponent<Image>();
            img.sprite = SpriteAt(spritePath);
            img.type = Image.Type.Sliced;
            img.raycastTarget = false;
            return img;
        }

        private static Button CreateSlicedButton(string name, Transform parent, string spritePath, Vector2 anchor, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = CreateUIContainer(name, parent, anchor, anchor, anchoredPos, sizeDelta);
            var img = go.AddComponent<Image>();
            img.sprite = SpriteAt(spritePath);
            img.type = Image.Type.Sliced;
            img.raycastTarget = true;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            return btn;
        }

        private static Text CreateUIText(string name, Transform parent, string value, int size, TextAnchor alignment, Vector2 anchor, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = CreateUIContainer(name, parent, anchor, anchor, anchoredPos, sizeDelta);
            var txt = go.AddComponent<Text>();
            txt.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/HITME/Fonts/BeVietnamPro-Bold.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.text = value;
            txt.fontSize = size;
            txt.alignment = alignment;
            txt.color = Color.white;
            txt.raycastTarget = false;
            return txt;
        }

        private static void EnsureEventSystem()
        {
            var es = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                var go = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
                Debug.Log("Created missing EventSystem with StandaloneInputModule in MainMatch scene.");
            }
        }

        private static void EnsureSortingLayers(params string[] names)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset"); if (assets.Length == 0) return;
            var serialized = new SerializedObject(assets[0]); var layers = serialized.FindProperty("m_SortingLayers");
            foreach (var name in names)
            {
                var found = false; for (var i=0;i<layers.arraySize;i++) if (layers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == name) { found=true; break; }
                if (found) continue;
                var index = layers.arraySize; layers.InsertArrayElementAtIndex(index); var entry = layers.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("name").stringValue = name; entry.FindPropertyRelative("uniqueID").intValue = 1000 + index * 7919; entry.FindPropertyRelative("locked").boolValue = false;
            }
            serialized.ApplyModifiedProperties();
        }
    }
}
