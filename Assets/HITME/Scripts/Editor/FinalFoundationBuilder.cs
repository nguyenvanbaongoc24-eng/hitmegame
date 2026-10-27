using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
        private const string Environment = "Assets/HITME/Art/Environment/FestivalArena/";
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
                importer.spritePixelsPerUnit = path.Contains("Characters/NamLang") ? 100f : 50f;
                importer.spritePivot = path.Contains("Characters/NamLang") ? new Vector2(.5f, 0f) : new Vector2(.5f, .5f);
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
            var oldArena = GameObject.Find("ArenaRoot_25D");
            if (oldArena != null) oldArena.SetActive(false);
            var root = new GameObject("FinalArena_2_5D");
            CreateSprite(root.transform, "FarMountains", "HITME_A01_Far_Mountains.png", new Vector3(0f, 3.0f), 1.35f, "ArenaFar", 0);
            CreateSprite(root.transform, "VillageRooftops", "HITME_A02_Village_Rooftops.png", new Vector3(0f, 2.1f), 1.4f, "ArenaFar", 1);
            CreateSprite(root.transform, "TempleRoof", "HITME_A03_Temple_Roof.png", new Vector3(0f, 1.6f), 1.4f, "ArenaMid", 0);
            CreateSprite(root.transform, "CombatField", "HITME_A07_Stone_Tile_Floor.png", new Vector3(0f, -.25f), 1.85f, "CombatField", 0);
            CreateSprite(root.transform, "ArenaBorder", "HITME_A08_Arena_Border.png", new Vector3(0f, -.22f), 1.85f, "CombatField", 2);
            CreateSprite(root.transform, "LotusEmblem", "HITME_A09_Lotus_Emblem.png", new Vector3(0f, -.1f), 1.35f, "CombatField", 3);
            CreateSprite(root.transform, "LeftCrowd", "HITME_C07_Left_Crowd.png", new Vector3(-3.05f, 1.4f), .95f, "ArenaDecor", 0);
            CreateSprite(root.transform, "RightCrowd", "HITME_C08_Right_Crowd.png", new Vector3(3.05f, 1.4f), .95f, "ArenaDecor", 1);
            CreatePivotSprite(root.transform, "LeftTreePivot", "HITME_C01_Left_Tree_Canopy.png", new Vector3(-4.2f, -4.7f), .9f, "ArenaDecor", 4, wind, .2f);
            CreatePivotSprite(root.transform, "RightTreePivot", "HITME_C02_Right_Tree_Canopy.png", new Vector3(4.2f, -4.7f), .9f, "ArenaDecor", 5, wind, 1.7f);
            CreateSprite(root.transform, "ForegroundLeafShadow", "HITME_C10_Leaf_Shadow.png", new Vector3(0f, -1.5f), 1.55f, "ArenaDecor", 9);
            CreateLantern(root.transform, "LanternLeft", new Vector3(-3.8f, 3.7f), wind, .1f);
            CreateLantern(root.transform, "LanternRight", new Vector3(3.8f, 3.7f), wind, 2.5f);

            var camera = Camera.main;
            if (camera != null)
            {
                var controller = camera.GetComponent<CameraController25D>();
                if (controller != null) controller.enabled = false;
                camera.orthographic = true; camera.orthographicSize = 6.15f;
                camera.transform.position = new Vector3(0f, 0f, -10f); camera.transform.rotation = Quaternion.identity;
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
            var menuImage = ui.mainMenuPanel != null ? ui.mainMenuPanel.GetComponent<Image>() : null;
            if (menuImage != null) { menuImage.sprite = null; menuImage.color = new Color(.055f, .10f, .13f, .96f); menuImage.type = Image.Type.Simple; }
            if (ui.mainMenuPanel != null)
            {
                foreach (Transform child in ui.mainMenuPanel.transform) child.gameObject.SetActive(false);
                var title = TextElement("Title", ui.mainMenuPanel.transform, "HIT ME VN", 54, new Vector2(0f, 420f), new Vector2(900f, 100f));
                title.color = new Color(1f, .77f, .18f);
                TextElement("Subtitle", ui.mainMenuPanel.transform, "NAM LÀNG • DÉP TỔ ONG", 25, new Vector2(0f, 335f), new Vector2(900f, 60f));
                TextElement("Instruction", ui.mainMenuPanel.transform, "Chạm để đặt chân • Giữ và kéo để ngắm", 23, new Vector2(0f, -415f), new Vector2(900f, 60f));
                var play = ButtonElement("BtnPlayTraining", ui.mainMenuPanel.transform, "LUYỆN TẬP", new Vector2(0f, -280f));
                ui.btnTrainWithBot = play; ui.btnQuickMatch = null;
            }
            if (ui.inGameHUDPanel != null)
            {
                var top = ui.inGameHUDPanel.transform.Find("TopBar");
                if (top != null)
                {
                    var rect = top.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(1f, 1f); rect.pivot = new Vector2(.5f, 1f); rect.anchoredPosition = Vector2.zero; rect.sizeDelta = new Vector2(0f, 156f);
                    var image = top.GetComponent<Image>(); if (image != null) { image.sprite = null; image.color = new Color(.04f, .08f, .1f, .68f); }
                }
                var bottom = ui.inGameHUDPanel.transform.Find("BottomBar");
                if (bottom != null)
                {
                    var rect = bottom.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0f, 0f); rect.anchorMax = new Vector2(1f, 0f); rect.pivot = new Vector2(.5f, 0f); rect.anchoredPosition = Vector2.zero; rect.sizeDelta = new Vector2(0f, 172f);
                    var image = bottom.GetComponent<Image>(); if (image != null) { image.sprite = null; image.color = new Color(.04f, .08f, .1f, .70f); }
                }
            }
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

        private static void CreateSprite(Transform parent, string name, string filename, Vector3 pos, float scale, string layer, int order)
        {
            var sprite = SpriteAt(Environment + filename); if (sprite == null) return;
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localScale = Vector3.one * scale;
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingLayerName = layer; renderer.sortingOrder = order;
        }
        private static void CreatePivotSprite(Transform parent, string name, string filename, Vector3 pos, float scale, string layer, int order, GlobalWindSettings wind, float phase)
        {
            var sprite = SpriteAt(Environment + filename); if (sprite == null) return;
            var pivot = new GameObject(name); pivot.transform.SetParent(parent, false); pivot.transform.localPosition = pos; pivot.AddComponent<FoliageSway>();
            var sway = pivot.GetComponent<FoliageSway>(); var so = new SerializedObject(sway); so.FindProperty("wind").objectReferenceValue = wind; so.FindProperty("phase").floatValue = phase; so.ApplyModifiedPropertiesWithoutUndo();
            var child = new GameObject("Sprite"); child.transform.SetParent(pivot.transform, false); child.transform.localPosition = new Vector3(0f, sprite.bounds.extents.y * scale, 0f); child.transform.localScale = Vector3.one * scale;
            var renderer = child.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingLayerName = layer; renderer.sortingOrder = order;
        }
        private static void CreateLantern(Transform parent, string name, Vector3 pos, GlobalWindSettings wind, float phase)
        {
            var sprite = SpriteAt(Environment + "HITME_B03_Red_Lantern.png"); if (sprite == null) return;
            var pivot = new GameObject(name); pivot.transform.SetParent(parent, false); pivot.transform.localPosition = pos; pivot.AddComponent<PivotSwing>();
            var swing = pivot.GetComponent<PivotSwing>(); var so = new SerializedObject(swing); so.FindProperty("phase").floatValue = phase; so.ApplyModifiedPropertiesWithoutUndo();
            var child = new GameObject("Sprite"); child.transform.SetParent(pivot.transform, false); child.transform.localPosition = new Vector3(0f, -sprite.bounds.extents.y * .7f, 0f); child.transform.localScale = Vector3.one * .8f;
            var renderer = child.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingLayerName = "ArenaDecor"; renderer.sortingOrder = 8;
        }
        private static Sprite SpriteAt(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
        private static Text TextElement(string name, Transform parent, string value, int size, Vector2 position, Vector2 bounds)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>(); text.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/HitMeFont.ttf"); if (text.font == null) text.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); text.text = value; text.fontSize = size; text.fontStyle = FontStyle.Bold; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white;
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f,.5f); rect.anchoredPosition = position; rect.sizeDelta = bounds; return text;
        }
        private static Button ButtonElement(string name, Transform parent, string label, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f,.5f); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(520f, 125f);
            go.GetComponent<Image>().color = new Color(.95f, .57f, .14f, 1f); TextElement("Label", go.transform, label, 31, Vector2.zero, rect.sizeDelta); return go.GetComponent<Button>();
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
