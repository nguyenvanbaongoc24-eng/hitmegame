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
            EnsureSortingLayers("ArenaFar", "ArenaMid", "CombatField", "ArenaDecor", "Actors", "Projectiles", "CombatVFX", "ArenaForeground", "HUD");
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
                importer.spritePixelsPerUnit = 100f;
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
                webGl.name = "WebGL"; webGl.overridden = true;
                webGl.maxTextureSize = (path.Contains("bg_") || path.Contains("arena_floor_master") || path.Contains("lotus_pond")) ? 2048 : 1024;
                importer.SetPlatformTextureSettings(webGl);
                importer.SaveAndReimport();
            }
            AssetDatabase.Refresh();
        }

        private static void BuildArena(GlobalWindSettings wind)
        {
            var prior = GameObject.Find("FinalArena_2_5D");
            if (prior != null) Object.DestroyImmediate(prior);

            // Permanently remove old prototype arena
            var oldArena = FindSceneObjectIncludingInactive("ArenaRoot_25D");
            if (oldArena != null) Object.DestroyImmediate(oldArena);

            var root = new GameObject("FinalArena_2_5D");

            // BASE SANDSTONE FLOOR: Fills the entire mobile portrait background behind the court
            CreateSprite(root.transform, "SandstoneBase", "Assets/HITME/Art/Environment/ground_tile_seamless.png", new Vector3(0f, -0.4f, 0f), new Vector3(1.2f, 1.2f, 1f), "ArenaFar", 0);

            // MASTER COMBAT ARENA FLOOR (CombatField)
            // 941 x 1672 PPU 50 at scale 0.3384 perfectly frames mobile portrait (390x844 / 1080x2340).
            // White line inner boundaries: X in [-2.70, +2.70], Y in [-5.59, +4.79], centered at (0, -0.4).
            // Exactly matches Image 2 target design with zero obstacles inside the court.
            CreateSprite(root.transform, "CombatField", "Assets/HITME/Art/Environment/arena_floor_master.png", new Vector3(0f, -0.4f, 0f), new Vector3(0.3384f, 0.3384f, 1f), "CombatField", 0);

            var camera = Camera.main;
            if (camera != null)
            {
                var controller = camera.GetComponent<CameraController25D>();
                if (controller != null) Object.DestroyImmediate(controller);
                camera.orthographic = true;
                camera.orthographicSize = 7.05f;
                camera.transform.position = new Vector3(0f, -0.4f, -10f);
                camera.transform.rotation = Quaternion.identity;
                camera.backgroundColor = new Color(0.92f, 0.70f, 0.44f);
            }
        }

        private static void BindNamLangAndDep()
        {
            var idle = SpriteAt(Normal + "sprite_001.png");
            var held = SpriteAt(DepHeld + "sprite_001.png");
            var projectile = SpriteAt(DepWeapon + "sprite_001.png");
            var shadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/shadow.png");
            var ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/selection_ring.png");

            foreach (var actor in Object.FindObjectsByType<ActorView>(FindObjectsSortMode.None))
            {
                actor.spriteIdle = idle;
                actor.spriteMove = null; actor.spriteThrowWindup = null; actor.spriteThrowRelease = null;
                actor.spriteHit = null; actor.spriteVictory = null; actor.spriteDefeat = null;
                actor.spriteAim = held != null ? held : idle;
                ReplaceWithFootAnchoredVisual(actor, idle);
                
                if (actor.characterRenderer != null)
                {
                    actor.characterRenderer.transform.localScale = Vector3.one * 0.70f;
                    actor.characterRenderer.sortingLayerName = "Actors";
                    actor.characterRenderer.sortingOrder = 10;
                }

                if (actor.shadowRenderer != null)
                {
                    actor.shadowRenderer.sprite = shadowSprite;
                    actor.shadowRenderer.sortingLayerName = "Actors";
                    actor.shadowRenderer.sortingOrder = 8;
                    actor.shadowRenderer.color = new Color(0f, 0f, 0f, 0.45f);
                    actor.shadowRenderer.transform.localScale = new Vector3(0.70f, 0.70f, 1f);
                }

                if (actor.ringRenderer != null)
                {
                    actor.ringRenderer.sprite = ringSprite;
                    actor.ringRenderer.sortingLayerName = "Actors";
                    actor.ringRenderer.sortingOrder = 9;
                    actor.ringRenderer.color = (actor.actorType == ActorType.Player) 
                        ? new Color(0.1f, 0.9f, 1f, 0.85f) 
                        : new Color(1f, 0.35f, 0.2f, 0.85f);
                    actor.ringRenderer.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
                }

                if (actor.aimBeam != null)
                {
                    actor.aimBeam.sortingLayerName = "Projectiles";
                    actor.aimBeam.sortingOrder = 25;
                    actor.aimBeam.startColor = new Color(1f, 0.85f, 0.2f, 0.35f);
                    actor.aimBeam.endColor = new Color(1f, 0.5f, 0.1f, 0.05f);
                }

                if (actor.aimLine != null)
                {
                    actor.aimLine.sortingLayerName = "Projectiles";
                    actor.aimLine.sortingOrder = 30;
                    actor.aimLine.startColor = new Color(1f, 0.95f, 0.3f, 0.95f);
                    actor.aimLine.endColor = new Color(1f, 0.6f, 0.1f, 0.85f);
                }

                if (actor.weaponTipRenderer != null)
                {
                    actor.weaponTipRenderer.sprite = projectile;
                    actor.weaponTipRenderer.sortingLayerName = "Projectiles";
                    actor.weaponTipRenderer.sortingOrder = 35;
                    actor.weaponTipRenderer.color = new Color(1f, 1f, 1f, 0.95f);
                }

                if (actor.weaponGlowRenderer != null)
                {
                    actor.weaponGlowRenderer.sortingLayerName = "Projectiles";
                    actor.weaponGlowRenderer.sortingOrder = 34;
                    actor.weaponGlowRenderer.color = new Color(1f, 0.85f, 0.2f, 0.6f);
                }
                
                // Hide actors in Main Menu; MatchStateMachine unhides player upon match start
                actor.SetVisibility(false);
                EditorUtility.SetDirty(actor);
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
            if (prefab != null)
            {
                var renderer = prefab.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sprite = projectile;
                    renderer.sortingLayerName = "Projectiles";
                    renderer.sortingOrder = 40;
                }
                PrefabUtility.SavePrefabAsset(prefab);
            }

            var data = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/ScriptableObjects/Characters/NamLang.asset");
            if (data != null) { data.spriteIdle = idle; data.spriteAim = held; data.spriteMove = data.spriteThrowWindup = data.spriteThrowRelease = data.spriteHit = data.spriteVictory = data.spriteDefeat = null; EditorUtility.SetDirty(data); }
            var weapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/ScriptableObjects/Weapons/DepToOng.asset");
            if (weapon != null) { weapon.projectileSprite = projectile; EditorUtility.SetDirty(weapon); }

            // Configure DestinationMarker with clean gold circular ring
            var destObj = GameObject.Find("DestinationMarker");
            if (destObj != null)
            {
                var destSr = destObj.GetComponent<SpriteRenderer>();
                if (destSr != null)
                {
                    destSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/destination_marker.png");
                    destSr.color = new Color(1f, 0.88f, 0.2f, 0.9f);
                    destSr.sortingLayerName = "CombatField";
                    destSr.sortingOrder = 15;
                }
                destObj.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
            }
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

                // Main Menu Full-Screen Vietnamese Village Gate Background
                var menuImage = ui.mainMenuPanel.GetComponent<Image>();
                if (menuImage != null)
                {
                    menuImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/HITME/Art/Environment/bg_village_gate_menu.png");
                    menuImage.color = Color.white;
                    menuImage.type = Image.Type.Simple;
                    menuImage.preserveAspect = false;
                    menuImage.raycastTarget = false;
                }

                // Clean old children
                for (int i = ui.mainMenuPanel.transform.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(ui.mainMenuPanel.transform.GetChild(i).gameObject);
                }

                // Soft vignette overlay to ensure pristine UI text legibility
                var vignette = CreateUIContainer("MenuVignette", ui.mainMenuPanel.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                var vigImg = vignette.AddComponent<Image>();
                vigImg.color = new Color(0.04f, 0.08f, 0.12f, 0.28f);
                vigImg.raycastTarget = false;

                // 1. TopProfileBar
                var topBar = CreateUIContainer("TopProfileBar", ui.mainMenuPanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -85f), new Vector2(980f, 110f));
                CreateSlicedImage("ProfileStrip", topBar.transform, "Assets/HITME/Art/UI/Skins/panel_profile_strip.png", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(980f, 110f));

                var avatarRing = CreateSlicedImage("AvatarRing", topBar.transform, "Assets/HITME/Art/UI/Skins/avatar_ring_cyan.png", new Vector2(0f, 0.5f), new Vector2(65f, 0f), new Vector2(96f, 96f));
                CreateSimpleImage("Portrait", avatarRing.transform, "Assets/HITME/Art/UI/Portraits/portrait_namlang.png", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(76f, 76f));

                var nameTxt = CreateUIText("PlayerName", topBar.transform, "Nam Làng", 32, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(130f, 14f), new Vector2(240f, 36f));
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
                CreateSimpleImage("LogoHitMe", ui.mainMenuPanel.transform, "Assets/HITME/Art/UI/Logo/logo_hit_me.png", new Vector2(0.5f, 1f), new Vector2(0f, -240f), new Vector2(620f, 240f));

                // 3. Character Showcase (Properly Centered with Contact Shadow & Lotus Podium)
                var showcase = CreateUIContainer("CharacterShowcase", ui.mainMenuPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), new Vector2(700f, 660f));
                var podium = CreateSimpleImage("LotusPodium", showcase.transform, "Assets/HITME/Art/Environment/lotus_emblem_clean.png", new Vector2(0.5f, 0f), new Vector2(0f, 15f), new Vector2(480f, 150f));
                podium.color = new Color(1f, 1f, 1f, 0.95f);
                CreateSimpleImage("CharacterSprite", showcase.transform, "Assets/HITME/Art/Characters/NamLang/namlang_dep_showcase.png", new Vector2(0.5f, 0f), new Vector2(0f, 65f), new Vector2(460f, 500f));

                var arrLeft = CreateSimpleImage("ArrowLeft", showcase.transform, "Assets/HITME/Art/UI/Skins/btn_circle_gold.png", new Vector2(0f, 0.45f), new Vector2(30f, 0f), new Vector2(74f, 74f));
                CreateSimpleImage("Icon", arrLeft.transform, "Assets/HITME/Art/UI/Icons/chevron_left.png", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(36f, 36f));
                var arrRight = CreateSimpleImage("ArrowRight", showcase.transform, "Assets/HITME/Art/UI/Skins/btn_circle_gold.png", new Vector2(1f, 0.45f), new Vector2(-30f, 0f), new Vector2(74f, 74f));
                CreateSimpleImage("Icon", arrRight.transform, "Assets/HITME/Art/UI/Icons/chevron_right.png", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(36f, 36f));

                // 4. Character & Weapon Selection Cards
                var selectors = CreateUIContainer("Selectors", ui.mainMenuPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -320f), new Vector2(920f, 135f));
                var charCard = CreateSlicedImage("CharCard", selectors.transform, "Assets/HITME/Art/UI/Skins/panel_card_dark_gold.png", new Vector2(0.5f, 0.5f), new Vector2(-235f, 0f), new Vector2(430f, 125f));
                CreateSimpleImage("CharIcon", charCard.transform, "Assets/HITME/Art/UI/Portraits/portrait_namlang.png", new Vector2(0f, 0.5f), new Vector2(55f, 0f), new Vector2(65f, 65f));
                var charCardTxt = CreateUIText("Label", charCard.transform, "NHÂN VẬT\nNam Làng", 26, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), new Vector2(50f, 0f), new Vector2(260f, 80f));
                charCardTxt.color = new Color(1f, 0.9f, 0.65f);

                var weaponCard = CreateSlicedImage("WeaponCard", selectors.transform, "Assets/HITME/Art/UI/Skins/panel_card_dark_gold.png", new Vector2(0.5f, 0.5f), new Vector2(235f, 0f), new Vector2(430f, 125f));
                CreateSimpleImage("WeaponIcon", weaponCard.transform, "Assets/HITME/Art/UI/Icons/weapon_dep.png", new Vector2(0f, 0.5f), new Vector2(55f, 0f), new Vector2(65f, 65f));
                var weaponCardTxt = CreateUIText("Label", weaponCard.transform, "VŨ KHÍ\nDép tổ ong", 26, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), new Vector2(50f, 0f), new Vector2(260f, 80f));
                weaponCardTxt.color = new Color(1f, 0.9f, 0.65f);

                // 5. Action Play Mode Buttons
                // CHƠI NHANH (Primary Yellow 3D CTA)
                var playBtn = CreateSlicedButton("BtnQuickPlay", ui.mainMenuPanel.transform, "Assets/HITME/Art/UI/Skins/btn_primary_yellow_normal.png", new Vector2(0.5f, 0.5f), new Vector2(0f, -500f), new Vector2(920f, 160f));
                CreateSimpleImage("PlayIcon", playBtn.transform, "Assets/HITME/Art/UI/Icons/play_outline.png", new Vector2(0f, 0.5f), new Vector2(80f, 0f), new Vector2(60f, 60f));
                var playTxt = CreateUIText("PlayText", playBtn.transform, "CHƠI NHANH", 44, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(30f, 18f), new Vector2(650f, 52f));
                playTxt.color = new Color(0.24f, 0.12f, 0f);
                var playSub = CreateUIText("PlaySub", playBtn.transform, "Online • Tối đa 6 người", 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(30f, -22f), new Vector2(650f, 32f));
                playSub.color = new Color(0.4f, 0.23f, 0.03f);

                // LUYỆN VỚI BOT (Navy 3D Bar)
                var botBtn = CreateSlicedButton("BtnTrainWithBot", ui.mainMenuPanel.transform, "Assets/HITME/Art/UI/Skins/btn_navy_normal.png", new Vector2(0.5f, 0.5f), new Vector2(0f, -675f), new Vector2(920f, 135f));
                CreateSimpleImage("BotIcon", botBtn.transform, "Assets/HITME/Art/UI/Icons/bot.png", new Vector2(0f, 0.5f), new Vector2(75f, 0f), new Vector2(52f, 52f));
                var botTxt = CreateUIText("BotText", botBtn.transform, "LUYỆN VỚI BOT", 36, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(25f, 0f), new Vector2(650f, 48f));
                botTxt.color = Color.white;

                // TẠO PHÒNG & VÀO PHÒNG
                var createBtn = CreateSlicedButton("BtnCreateRoom", ui.mainMenuPanel.transform, "Assets/HITME/Art/UI/Skins/btn_blue_normal.png", new Vector2(0.5f, 0.5f), new Vector2(-240f, -830f), new Vector2(430f, 115f));
                CreateSimpleImage("Icon", createBtn.transform, "Assets/HITME/Art/UI/Icons/person_add.png", new Vector2(0f, 0.5f), new Vector2(50f, 0f), new Vector2(40f, 40f));
                var createTxt = CreateUIText("Text", createBtn.transform, "TẠO PHÒNG", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(20f, 0f), new Vector2(280f, 40f));
                createTxt.color = Color.white;

                var joinBtn = CreateSlicedButton("BtnJoinRoom", ui.mainMenuPanel.transform, "Assets/HITME/Art/UI/Skins/btn_green_normal.png", new Vector2(0.5f, 0.5f), new Vector2(240f, -830f), new Vector2(430f, 115f));
                CreateSimpleImage("Icon", joinBtn.transform, "Assets/HITME/Art/UI/Icons/people.png", new Vector2(0f, 0.5f), new Vector2(50f, 0f), new Vector2(40f, 40f));
                var joinTxt = CreateUIText("Text", joinBtn.transform, "VÀO PHÒNG", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(20f, 0f), new Vector2(280f, 40f));
                joinTxt.color = Color.white;

                // 6. Bottom Navigation Dock
                var dock = CreateSlicedImage("BottomDock", ui.mainMenuPanel.transform, "Assets/HITME/Art/UI/Skins/dock_bg.png", new Vector2(0.5f, 0f), new Vector2(0f, 35f), new Vector2(980f, 140f));
                var tabHome = CreateUIContainer("TabHome", dock.transform, new Vector2(0.18f, 0.5f), new Vector2(0.18f, 0.5f), Vector2.zero, new Vector2(160f, 100f));
                CreateSimpleImage("Icon", tabHome.transform, "Assets/HITME/Art/UI/Icons/home.png", new Vector2(0.5f, 0.5f), new Vector2(0f, 14f), new Vector2(44f, 44f));
                var homeTxt = CreateUIText("Text", tabHome.transform, "Trang chủ", 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, -24f), new Vector2(140f, 26f));
                homeTxt.color = new Color(0.2f, 0.9f, 1f);

                var tabTrophy = CreateUIContainer("TabTrophy", dock.transform, new Vector2(0.50f, 0.5f), new Vector2(0.50f, 0.5f), Vector2.zero, new Vector2(160f, 100f));
                CreateSimpleImage("Icon", tabTrophy.transform, "Assets/HITME/Art/UI/Icons/trophy.png", new Vector2(0.5f, 0.5f), new Vector2(0f, 14f), new Vector2(44f, 44f));
                var trophyTxt = CreateUIText("Text", tabTrophy.transform, "Xếp hạng", 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, -24f), new Vector2(140f, 26f));
                trophyTxt.color = new Color(0.7f, 0.75f, 0.85f);

                var tabQuest = CreateUIContainer("TabQuest", dock.transform, new Vector2(0.82f, 0.5f), new Vector2(0.82f, 0.5f), Vector2.zero, new Vector2(160f, 100f));
                CreateSimpleImage("Icon", tabQuest.transform, "Assets/HITME/Art/UI/Icons/gift.png", new Vector2(0.5f, 0.5f), new Vector2(0f, 14f), new Vector2(44f, 44f));
                var questTxt = CreateUIText("Text", tabQuest.transform, "Nhiệm vụ", 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, -24f), new Vector2(140f, 26f));
                questTxt.color = new Color(0.7f, 0.75f, 0.85f);

                // Wire up buttons to authoritative UIManager
                ui.btnTrainWithBot = botBtn.GetComponent<Button>();
                ui.btnQuickMatch = playBtn.GetComponent<Button>();
                ui.btnCreateRoom = createBtn.GetComponent<Button>();
                EditorUtility.SetDirty(ui);
            }

            // REPAIR IN-MATCH HUD PANEL WITH FULL CLASH ROYALE SPEC
            if (ui.inGameHUDPanel != null)
            {
                var hudRt = ui.inGameHUDPanel.GetComponent<RectTransform>();
                hudRt.anchorMin = Vector2.zero; hudRt.anchorMax = Vector2.one;
                hudRt.offsetMin = hudRt.offsetMax = Vector2.zero;

                for (int i = ui.inGameHUDPanel.transform.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(ui.inGameHUDPanel.transform.GetChild(i).gameObject);
                }

                // 1. Top HUD Bar (Player Card, Match Info, Bot Card)
                var topBar = CreateUIContainer("TopBar", ui.inGameHUDPanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -95f), new Vector2(980f, 150f));
                var topBg = topBar.AddComponent<Image>();
                topBg.color = new Color(0.04f, 0.08f, 0.14f, 0.82f);

                // Player Info (Left)
                var pRing = CreateSlicedImage("PlayerRing", topBar.transform, "Assets/HITME/Art/UI/Skins/avatar_ring_cyan.png", new Vector2(0f, 0.5f), new Vector2(65f, 0f), new Vector2(88f, 88f));
                CreateSimpleImage("Portrait", pRing.transform, "Assets/HITME/Art/UI/Portraits/portrait_namlang.png", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(70f, 70f));
                var pName = CreateUIText("PlayerName", topBar.transform, "Nam Làng", 28, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(130f, 22f), new Vector2(240f, 38f));
                pName.color = Color.white;
                ui.txtPlayerHP = CreateUIText("PlayerHP", topBar.transform, "BẠN: HP 3/3", 26, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(130f, -20f), new Vector2(240f, 38f));
                ui.txtPlayerHP.color = new Color(0.2f, 0.95f, 0.45f);

                // Match Status (Center)
                var roundPill = CreateSlicedImage("RoundPill", topBar.transform, "Assets/HITME/Art/UI/Skins/pill_coin.png", new Vector2(0.5f, 0.5f), new Vector2(0f, 28f), new Vector2(210f, 44f));
                ui.txtRound = CreateUIText("RoundText", roundPill.transform, "VÒNG 1/20", 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200f, 38f));
                ui.txtRound.color = new Color(1f, 0.85f, 0.2f);
                ui.txtTimer = CreateUIText("TimerText", topBar.transform, "5s", 46, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, -22f), new Vector2(150f, 48f));
                ui.txtTimer.color = Color.white;

                // Bot Info (Right)
                var bRing = CreateSlicedImage("BotRing", topBar.transform, "Assets/HITME/Art/UI/Skins/btn_circle_navy.png", new Vector2(1f, 0.5f), new Vector2(-65f, 0f), new Vector2(88f, 88f));
                CreateSimpleImage("BotIcon", bRing.transform, "Assets/HITME/Art/UI/Icons/bot.png", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(54f, 54f));
                var bName = CreateUIText("BotName", topBar.transform, "Bot Luyện Tập", 28, TextAnchor.MiddleRight, new Vector2(1f, 0.5f), new Vector2(-130f, 22f), new Vector2(240f, 38f));
                bName.color = Color.white;
                ui.txtBotHP = CreateUIText("BotHP", topBar.transform, "BOT: HP 3/3", 26, TextAnchor.MiddleRight, new Vector2(1f, 0.5f), new Vector2(-130f, -20f), new Vector2(240f, 38f));
                ui.txtBotHP.color = new Color(1f, 0.35f, 0.35f);

                // 2. Phase Banner (Middle overlay)
                var bannerContainer = CreateUIContainer("PhaseBannerContainer", ui.inGameHUDPanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -195f), new Vector2(760f, 52f));
                var bannerBg = bannerContainer.AddComponent<Image>();
                bannerBg.color = new Color(0.04f, 0.08f, 0.14f, 0.72f);
                bannerBg.raycastTarget = false;
                ui.txtPhaseBanner = CreateUIText("PhaseBanner", bannerContainer.transform, "", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(740f, 46f));
                ui.txtPhaseBanner.color = new Color(1f, 0.9f, 0.2f);

                // 3. Compact Bottom HUD Bar
                var bottomBar = CreateUIContainer("BottomBar", ui.inGameHUDPanel.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 85f), new Vector2(980f, 150f));

                // Left: Equipped Weapon Card Slot
                var weaponSlot = CreateSlicedImage("WeaponSlot", bottomBar.transform, "Assets/HITME/Art/UI/Skins/panel_card_dark_gold.png", new Vector2(0f, 0.5f), new Vector2(95f, 0f), new Vector2(150f, 130f));
                CreateSimpleImage("Icon", weaponSlot.transform, "Assets/HITME/Art/UI/Icons/weapon_dep.png", new Vector2(0.5f, 0.5f), new Vector2(0f, 12f), new Vector2(75f, 75f));
                var wepName = CreateUIText("Name", weaponSlot.transform, "Dép tổ ong", 20, TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(140f, 26f));
                wepName.color = new Color(1f, 0.85f, 0.3f);

                // Right: Primary Yellow 3D Ready Button
                var readyBtn = CreateSlicedButton("BtnReady", bottomBar.transform, "Assets/HITME/Art/UI/Skins/btn_primary_yellow_normal.png", new Vector2(1f, 0.5f), new Vector2(-365f, 0f), new Vector2(700f, 130f));
                ui.txtReadyBtn = CreateUIText("ReadyText", readyBtn.transform, "SẴN SÀNG", 40, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600f, 52f));
                ui.txtReadyBtn.color = new Color(0.24f, 0.12f, 0f);

                ui.btnReady = readyBtn.GetComponent<Button>();
                EditorUtility.SetDirty(ui);
            }

            // REPAIR RESULT PANEL WITH IMMERSIVE WIN/LOSS PODIUM
            if (ui.resultPanel != null)
            {
                var resRt = ui.resultPanel.GetComponent<RectTransform>();
                resRt.anchorMin = Vector2.zero; resRt.anchorMax = Vector2.one;
                resRt.offsetMin = resRt.offsetMax = Vector2.zero;

                var resBg = ui.resultPanel.GetComponent<Image>();
                if (resBg != null) { resBg.color = new Color(0.02f, 0.05f, 0.08f, 0.88f); resBg.raycastTarget = true; }

                for (int i = ui.resultPanel.transform.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(ui.resultPanel.transform.GetChild(i).gameObject);
                }

                var modal = CreateSlicedImage("ResultModal", ui.resultPanel.transform, "Assets/HITME/Art/UI/Skins/panel_card_dark_gold.png", new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(900f, 1100f));
                ui.txtResultTitle = CreateUIText("Title", modal.transform, "CHIẾN THẮNG!", 54, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(800f, 75f));
                ui.txtResultSubtitle = CreateUIText("Subtitle", modal.transform, "Bạn đã hạ gục đối thủ xuất sắc!", 26, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -165f), new Vector2(800f, 40f));
                ui.txtResultSubtitle.color = new Color(0.7f, 0.75f, 0.85f);

                CreateSimpleImage("Podium", modal.transform, "Assets/HITME/Art/Environment/lotus_emblem_clean.png", new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), new Vector2(360f, 130f));
                CreateSimpleImage("Trophy", modal.transform, "Assets/HITME/Art/UI/Icons/trophy.png", new Vector2(0.5f, 0.5f), new Vector2(0f, 240f), new Vector2(160f, 160f));

                var statsBox = CreateSlicedImage("StatsBox", modal.transform, "Assets/HITME/Art/UI/Skins/panel_card_dark_selected.png", new Vector2(0.5f, 0.5f), new Vector2(0f, -90f), new Vector2(800f, 260f));
                ui.txtPlayerStats = CreateUIText("PlayerStats", statsBox.transform, "Bạn (Nam Làng)\nSố lượt ném: 5\nTrúng đích: 3\nTỉ lệ: 60.0%", 26, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(60f, 0f), new Vector2(340f, 220f));
                ui.txtPlayerStats.color = Color.white;
                ui.txtBotStats = CreateUIText("BotStats", statsBox.transform, "Bot Luyện Tập\nSố lượt ném: 5\nTrúng đích: 1\nTỉ lệ: 20.0%", 26, TextAnchor.MiddleLeft, new Vector2(1f, 0.5f), new Vector2(-60f, 0f), new Vector2(340f, 220f));
                ui.txtBotStats.color = new Color(0.8f, 0.85f, 0.95f);

                // Rematch CTA Button (Primary Yellow)
                var rematchBtn = CreateSlicedButton("BtnRematch", modal.transform, "Assets/HITME/Art/UI/Skins/btn_primary_yellow_normal.png", new Vector2(0.5f, 0f), new Vector2(0f, 215f), new Vector2(760f, 140f));
                var rematchTxt = CreateUIText("Text", rematchBtn.transform, "CHƠI LẠI", 40, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 50f));
                rematchTxt.color = new Color(0.24f, 0.12f, 0f);

                // Home Button (Navy)
                var homeBtn = CreateSlicedButton("BtnBackToHome", modal.transform, "Assets/HITME/Art/UI/Skins/btn_navy_normal.png", new Vector2(0.5f, 0f), new Vector2(0f, 75f), new Vector2(760f, 115f));
                var homeBtnTxt = CreateUIText("Text", homeBtn.transform, "VỀ TRANG CHỦ", 30, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 44f));
                homeBtnTxt.color = Color.white;

                ui.btnRematch = rematchBtn.GetComponent<Button>();
                ui.btnBackToHome = homeBtn.GetComponent<Button>();
                EditorUtility.SetDirty(ui);
            }
        }

        private static void BindPresentationAdapter()
        {
            var state = Object.FindFirstObjectByType<MatchStateMachine>();
            var aim = Object.FindFirstObjectByType<AimController>();
            var actors = Object.FindObjectsByType<ActorView>(FindObjectsSortMode.None);
            if (state == null || aim == null) return;
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

        private static void CreateLantern(Transform parent, string name, Vector3 pos, float phase)
        {
            var sprite = SpriteAt("Assets/HITME/Art/Environment/FestivalArena/HITME_B03_Red_Lantern.png"); if (sprite == null) return;
            var pivot = new GameObject(name); pivot.transform.SetParent(parent, false); pivot.transform.localPosition = pos;
            var swing = pivot.AddComponent<PivotSwing>();
            var so = new SerializedObject(swing);
            var phaseProp = so.FindProperty("phase");
            if (phaseProp != null) phaseProp.floatValue = phase;
            so.ApplyModifiedPropertiesWithoutUndo();
            var child = new GameObject("Sprite"); child.transform.SetParent(pivot.transform, false); child.transform.localPosition = new Vector3(0f, -sprite.bounds.extents.y * 0.9f, 0f); child.transform.localScale = Vector3.one * 0.9f;
            var renderer = child.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingLayerName = "ArenaMid"; renderer.sortingOrder = 4;
        }

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
            renderer.transform.localScale = Vector3.one * 0.70f;
            renderer.transform.localPosition = new Vector3(0f, idle != null ? idle.bounds.extents.y * 0.70f : 0f, 0f);
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
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;
            return btn;
        }

        private static Text CreateUIText(string name, Transform parent, string text, int fontSize, TextAnchor alignment, Vector2 anchor, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = CreateUIContainer(name, parent, anchor, anchor, anchoredPos, sizeDelta);
            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            var vietFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/HITME/Fonts/BeVietnamPro-Bold.ttf");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/HitMeFont.ttf");
            if (vietFont == null) vietFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.font = vietFont;
            txt.raycastTarget = false;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            return txt;
        }

        private static Sprite SpriteAt(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(eventSystemGo, "Create EventSystem");
            }
        }

        private static void EnsureSortingLayers(params string[] layerNames)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProp = tagManager.FindProperty("m_SortingLayers");
            var existing = new HashSet<string>();
            for (var i = 0; i < layersProp.arraySize; i++) existing.Add(layersProp.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue);
            foreach (var name in layerNames)
            {
                if (existing.Contains(name)) continue;
                layersProp.InsertArrayElementAtIndex(layersProp.arraySize);
                var elem = layersProp.GetArrayElementAtIndex(layersProp.arraySize - 1);
                elem.FindPropertyRelative("name").stringValue = name;
                elem.FindPropertyRelative("uniqueID").intValue = (int)layersProp.arraySize * 1000 + 1234;
            }
            tagManager.ApplyModifiedProperties();
        }
    }
}
