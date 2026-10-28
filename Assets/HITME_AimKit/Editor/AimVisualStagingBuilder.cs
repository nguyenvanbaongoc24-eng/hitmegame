using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using HitMe.AimKit;
using HitMe.Gameplay;

namespace HitMe.AimKit.Editor
{
    public static class AimVisualStagingBuilder
    {
        private const string StagingScenePath = "Assets/Scenes/AimVisual_Staging.unity";
        private const string PrefabPath = "Assets/HITME_AimKit/Generated/HITME_AimVisual.prefab";
        private const string EvidenceDir = "Evidence/phase2";

        [MenuItem("Tools/HIT ME/Build Aim Visual Staging Scene")]
        public static void BuildStagingScene()
        {
            if (!Directory.Exists("Assets/Scenes"))
            {
                Directory.CreateDirectory("Assets/Scenes");
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera (Mobile Portrait 9:16)
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.08f, 0.06f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = 7.5f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 100f;
            camObj.transform.position = new Vector3(0f, 0f, -10f);
            camObj.transform.rotation = Quaternion.identity;

            // 2. Arena Floor
            GameObject arenaObj = new GameObject("ArenaFloor");
            var arenaSr = arenaObj.AddComponent<SpriteRenderer>();
            var arenaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ARENA.png") ??
                              AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/arena_background.png");
            arenaSr.sprite = arenaSprite;
            arenaSr.sortingOrder = -10;
            arenaObj.transform.position = new Vector3(0f, 0f, 0f);
            arenaObj.transform.localScale = new Vector3(1.02f, 1.02f, 1f);

            // 3. Nam Làng Character
            GameObject charObj = new GameObject("Nam_Lang");
            charObj.transform.position = new Vector3(0f, -1.2f, 0f);

            // Shadow
            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(charObj.transform, false);
            var shadowSr = shadowObj.AddComponent<SpriteRenderer>();
            shadowSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/procedural_shadow.png");
            shadowSr.sortingOrder = 1;
            shadowObj.transform.localPosition = new Vector3(0f, 0f, 0.02f);
            shadowObj.transform.localScale = new Vector3(0.66f, 0.66f, 1f);

            // Foot Ring
            GameObject ringObj = new GameObject("FootRing");
            ringObj.transform.SetParent(charObj.transform, false);
            var ringSr = ringObj.AddComponent<SpriteRenderer>();
            ringSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/procedural_foot_ring.png");
            ringSr.color = new Color(0.15f, 0.85f, 1f, 0.95f); // Cyan player color
            ringSr.sortingOrder = 2;
            ringObj.transform.localPosition = new Vector3(0f, 0f, 0.01f);
            ringObj.transform.localScale = new Vector3(0.66f, 0.66f, 1f);

            // Body
            GameObject bodyObj = new GameObject("Body");
            bodyObj.transform.SetParent(charObj.transform, false);
            var bodySr = bodyObj.AddComponent<SpriteRenderer>();
            bodySr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang.png");
            bodySr.sortingOrder = 5;
            bodyObj.transform.localPosition = Vector3.zero;
            bodyObj.transform.localScale = new Vector3(0.7235f, 0.7235f, 1f);

            // 4. Instantiate Aim Visual Prefab
            GameObject aimPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (aimPrefab == null)
            {
                HitMeAimPrefabBuilder.Generate();
                aimPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }

            GameObject aimInstance = (GameObject)PrefabUtility.InstantiatePrefab(aimPrefab);
            aimInstance.name = "HITME_AimVisual";
            aimInstance.transform.SetParent(charObj.transform, false);
            var aimVisual = aimInstance.GetComponent<HitMeAimVisual2D>();

            // 5. Staging Driver
            GameObject driverObj = new GameObject("AimVisual_StagingDriver");
            var driver = driverObj.AddComponent<AimVisualStagingDriver>();
            driver.aimVisual = aimVisual;
            driver.characterTransform = charObj.transform;
            driver.footOffset = Vector2.zero; // Pivot is already at bottom center
            driver.weaponIcons = new Sprite[]
            {
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/dep_to_ong_icon.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/chao_chong_dinh_icon.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/vot_pickleball_icon.png")
            };
            driver.currentWeaponIndex = 0;
            driver.testLength = 3.5f;
            driver.manualDirection = Vector2.up;
            driver.currentState = HitMeAimVisual2D.AimState.Aiming;
            driver.isAimActive = true;

            // Save Scene
            EditorSceneManager.SaveScene(scene, StagingScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[AimVisualStagingBuilder] Staging scene built at " + StagingScenePath);
        }

        [MenuItem("Tools/HIT ME/Run Visual QA Captures")]
        public static void RunVisualQACaptures()
        {
            if (!File.Exists(StagingScenePath))
            {
                BuildStagingScene();
            }

            Scene scene = EditorSceneManager.OpenScene(StagingScenePath, OpenSceneMode.Single);
            if (!Directory.Exists(EvidenceDir))
            {
                Directory.CreateDirectory(EvidenceDir);
            }

            var driver = Object.FindAnyObjectByType<AimVisualStagingDriver>();
            if (driver == null)
            {
                Debug.LogError("[AimVisualStagingBuilder] Missing AimVisualStagingDriver in scene.");
                return;
            }

            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("[AimVisualStagingBuilder] Missing Main Camera.");
                return;
            }

            int w = 1080;
            int h = 1920;

            // 1. Capture All 8 Directions
            for (int i = 0; i < 8; i++)
            {
                driver.SetDirection(i);
                driver.SetWeapon(0); // Dép
                driver.SetLength(3.2f);
                driver.SetState(HitMeAimVisual2D.AimState.Aiming);

                string filename = Path.Combine(EvidenceDir, $"aim_dir_{i}_{AimVisualStagingDriver.DirectionNames[i].ToLower().Replace('-', '_')}.png");
                CaptureCameraToFile(cam, w, h, filename);
                Debug.Log($"[QA Capture] Captured direction {i}: {AimVisualStagingDriver.DirectionNames[i]} -> {filename}");
            }

            // 2. Capture Short Drag (1.2u) and Long Drag (4.8u)
            driver.SetDirection(0); // Up
            driver.SetWeapon(0);
            driver.SetLength(1.2f);
            CaptureCameraToFile(cam, w, h, Path.Combine(EvidenceDir, "aim_drag_short.png"));

            driver.SetLength(4.8f);
            CaptureCameraToFile(cam, w, h, Path.Combine(EvidenceDir, "aim_drag_long.png"));

            // 3. Capture Weapons Preview (Dép, Chảo, Vợt)
            driver.SetDirection(1); // Up-Right
            driver.SetLength(3.5f);
            driver.SetWeapon(0); // Dép
            CaptureCameraToFile(cam, w, h, Path.Combine(EvidenceDir, "aim_weapon_dep.png"));

            driver.SetWeapon(1); // Chảo
            CaptureCameraToFile(cam, w, h, Path.Combine(EvidenceDir, "aim_weapon_chao.png"));

            driver.SetWeapon(2); // Vợt
            CaptureCameraToFile(cam, w, h, Path.Combine(EvidenceDir, "aim_weapon_vot.png"));

            // 4. Capture States (Aiming, Locked, Invalid)
            driver.SetDirection(0); // Up
            driver.SetWeapon(0);
            driver.SetLength(3.2f);
            driver.SetState(HitMeAimVisual2D.AimState.Aiming);
            CaptureCameraToFile(cam, w, h, Path.Combine(EvidenceDir, "aim_state_aiming.png"));

            driver.SetState(HitMeAimVisual2D.AimState.Locked);
            CaptureCameraToFile(cam, w, h, Path.Combine(EvidenceDir, "aim_state_locked.png"));

            driver.SetState(HitMeAimVisual2D.AimState.Invalid);
            CaptureCameraToFile(cam, w, h, Path.Combine(EvidenceDir, "aim_state_invalid.png"));

            // 5. Full Mobile Portrait Preview (iPhone 15 aspect 9:16)
            driver.SetDirection(1); // Up-Right
            driver.SetWeapon(0);
            driver.SetLength(3.5f);
            driver.SetState(HitMeAimVisual2D.AimState.Aiming);
            CaptureCameraToFile(cam, 1080, 2340, Path.Combine(EvidenceDir, "aim_staging_portrait_1080x2340.png"));

            Debug.Log("[AimVisualStagingBuilder] All QA captures completed successfully.");
        }

        [MenuItem("Tools/HIT ME/Integrate Aim Kit Into MainMatch Scene")]
        public static void IntegrateIntoMainMatch()
        {
            const string mainMatchPath = "Assets/Scenes/MainMatch.unity";
            if (!File.Exists(mainMatchPath))
            {
                Debug.LogError("[AimVisualStagingBuilder] MainMatch scene not found at " + mainMatchPath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(mainMatchPath, OpenSceneMode.Single);
            var playerObj = GameObject.Find("PlayerActor");
            if (playerObj == null)
            {
                Debug.LogError("[AimVisualStagingBuilder] PlayerActor GameObject not found in MainMatch scene.");
                return;
            }

            var actorView = playerObj.GetComponent<ActorView>();
            if (actorView == null)
            {
                Debug.LogError("[AimVisualStagingBuilder] ActorView component missing on PlayerActor.");
                return;
            }

            // Check if HITME_AimVisual child already exists
            var existingAim = playerObj.transform.Find("HITME_AimVisual");
            HitMeAimVisual2D aimVisualComponent;

            if (existingAim == null)
            {
                GameObject aimPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                if (aimPrefab == null)
                {
                    HitMeAimPrefabBuilder.Generate();
                    aimPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                }

                GameObject aimInstance = (GameObject)PrefabUtility.InstantiatePrefab(aimPrefab);
                aimInstance.name = "HITME_AimVisual";
                aimInstance.transform.SetParent(playerObj.transform, false);
                aimInstance.transform.localPosition = Vector3.zero;
                aimVisualComponent = aimInstance.GetComponent<HitMeAimVisual2D>();
            }
            else
            {
                aimVisualComponent = existingAim.GetComponent<HitMeAimVisual2D>();
            }

            actorView.aimVisual = aimVisualComponent;
            actorView.equippedWeaponIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/dep_to_ong_icon.png") ??
                                          AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/dep_icon.png");

            // Disable legacy line renderers
            if (actorView.aimLine != null) actorView.aimLine.enabled = false;
            if (actorView.aimBeam != null) actorView.aimBeam.enabled = false;
            if (actorView.weaponTip != null) actorView.weaponTip.gameObject.SetActive(false);

            EditorSceneManager.SaveScene(scene, mainMatchPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[AimVisualStagingBuilder] Successfully integrated HITME_AimVisual into MainMatch scene on PlayerActor.");
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
