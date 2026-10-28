#if UNITY_EDITOR
using System.IO;
using System.Linq;
using HitMe.Core;
using HitMe.Editor;
using HitMe.Game;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Phase2PrefabBuilder
{
    [MenuItem("Tools/HIT ME/Build Phase 2 Prefabs and Test Scene")]
    public static void BuildAll()
    {
        ProceduralFootElements.GenerateAll();
        BuildPrefabs();
        BuildTestScene();
        BuildHitboxComparisonScene();
        BuildTrueArenaScene();
    }

    public static void BuildPrefabs()
    {
        if (!Directory.Exists("Assets/Prefabs"))
        {
            Directory.CreateDirectory("Assets/Prefabs");
            AssetDatabase.Refresh();
        }

        // 1. Build HitFx.prefab
        var hitFxGo = new GameObject("HitFx");
        var hitFxSr = hitFxGo.AddComponent<SpriteRenderer>();
        hitFxSr.sortingOrder = 5;
        var hitFxComp = hitFxGo.AddComponent<HitFx>();
        hitFxComp.spriteRenderer = hitFxSr;

        string[] framePaths = new[]
        {
            "Assets/Art/VFX/Hit_Impact/01_Hit_Impact_F01.png",
            "Assets/Art/VFX/Hit_Impact/01_Hit_Impact_F02.png",
            "Assets/Art/VFX/Hit_Impact/01_Hit_Impact_F03.png",
            "Assets/Art/VFX/Hit_Impact/01_Hit_Impact_F04.png",
            "Assets/Art/VFX/Hit_Impact/01_Hit_Impact_F05.png",
            "Assets/Art/VFX/Hit_Impact/01_Hit_Impact_F06.png"
        };
        var frames = framePaths.Select(AssetDatabase.LoadAssetAtPath<Sprite>).Where(s => s != null).ToArray();
        hitFxComp.frames = frames;
        if (frames.Length > 0)
        {
            hitFxSr.sprite = frames.Length > 2 ? frames[2] : frames[0];
        }

        string hitFxPrefabPath = "Assets/Prefabs/HitFx.prefab";
        var hitFxPrefab = PrefabUtility.SaveAsPrefabAsset(hitFxGo, hitFxPrefabPath);
        Object.DestroyImmediate(hitFxGo);
        Debug.Log("Saved prefab: " + hitFxPrefabPath);

        // 2. Build WeaponView.prefab
        var weaponGo = new GameObject("WeaponView");
        var weaponSr = weaponGo.AddComponent<SpriteRenderer>();
        weaponSr.sortingOrder = 4;
        var sandalSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/dep_projectile.png");
        if (sandalSprite == null) sandalSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/dep_to_ong_projectile.png");
        weaponSr.sprite = sandalSprite;
        var weaponComp = weaponGo.AddComponent<WeaponView>();
        weaponComp.spriteRenderer = weaponSr;
        if (hitFxPrefab != null)
        {
            weaponComp.hitFxPrefab = hitFxPrefab.GetComponent<HitFx>();
        }

        string weaponPrefabPath = "Assets/Prefabs/WeaponView.prefab";
        PrefabUtility.SaveAsPrefabAsset(weaponGo, weaponPrefabPath);
        Object.DestroyImmediate(weaponGo);
        Debug.Log("Saved prefab: " + weaponPrefabPath);

        // 3. Build PlayerView.prefab
        var playerGo = new GameObject("PlayerView");
        var playerComp = playerGo.AddComponent<PlayerView>();

        // Procedural soft shadow (smooth alpha gradient, zero checkerboard)
        var shadowGo = new GameObject("Shadow");
        shadowGo.transform.SetParent(playerGo.transform, false);
        shadowGo.transform.localPosition = new Vector3(0f, -0.05f, 0f);
        shadowGo.transform.localScale = new Vector3(0.66f, 0.66f, 1f);
        var shadowSr = shadowGo.AddComponent<SpriteRenderer>();
        shadowSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/procedural_shadow.png");
        shadowSr.sortingOrder = 0;
        shadowSr.color = new Color(0f, 0f, 0f, 0.45f);

        // Procedural thin elliptical foot ring (perspective 0.6, width = 1.125u matching 12.5% arena width)
        var ringGo = new GameObject("FootRing");
        ringGo.transform.SetParent(playerGo.transform, false);
        ringGo.transform.localPosition = new Vector3(0f, 0.02f, 0f);
        ringGo.transform.localScale = new Vector3(0.66f, 0.66f, 1f);
        var ringSr = ringGo.AddComponent<SpriteRenderer>();
        ringSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/procedural_foot_ring.png");
        ringSr.sortingOrder = 1;
        ringSr.color = Color.cyan;

        // Body: Visual scale 0.7235 brings Nam Lang height down from 2.07u to 1.50u (~13% arena height)
        var bodyGo = new GameObject("Body");
        bodyGo.transform.SetParent(playerGo.transform, false);
        bodyGo.transform.localPosition = Vector3.zero;
        bodyGo.transform.localScale = new Vector3(0.7235f, 0.7235f, 1f);
        var bodySr = bodyGo.AddComponent<SpriteRenderer>();
        bodySr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_idle.png");
        bodySr.sortingOrder = 2;
        var bodyAnim = bodyGo.AddComponent<Animator>();
        bodyAnim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Animations/Cau_Be_Lang/Cau_Be_Lang_Controller.controller");

        // Debug Hitbox: Standardized R = 0.55f for all characters (DISABLED by default)
        var hbGo = new GameObject("DebugHitbox");
        hbGo.transform.SetParent(playerGo.transform, false);
        hbGo.transform.localPosition = Vector3.zero;
        var hbLr = hbGo.AddComponent<LineRenderer>();
        hbLr.useWorldSpace = false;
        hbLr.loop = true;
        hbLr.positionCount = 36;
        hbLr.startWidth = 0.035f;
        hbLr.endWidth = 0.035f;
        hbLr.startColor = new Color(0.1f, 1f, 0.4f, 0.95f);
        hbLr.endColor = new Color(0.1f, 1f, 0.4f, 0.95f);
        hbLr.sharedMaterial = bodySr.sharedMaterial;
        hbLr.sortingOrder = 4;
        float hbRadius = 0.55f;
        for (int i = 0; i < 36; i++)
        {
            float angle = Mathf.PI * 2f * i / 36;
            hbLr.SetPosition(i, new Vector3(Mathf.Cos(angle) * hbRadius, Mathf.Sin(angle) * hbRadius, 0f));
        }
        hbLr.enabled = false; // Tắt mặc định, chỉ bật bằng cờ debug

        // Separate physical touch collider (diameter >= 9mm, invisible)
        var touchCol = playerGo.AddComponent<CircleCollider2D>();
        touchCol.radius = 0.75f;
        touchCol.isTrigger = true;

        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(playerGo.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 1.75f, 0f);
        var labelTmp = labelGo.AddComponent<TextMeshPro>();
        labelTmp.rectTransform.sizeDelta = new Vector2(2.4f, 0.8f);
        labelTmp.alignment = TextAlignmentOptions.Center;
        labelTmp.fontSize = 0.95f;
        labelTmp.color = Color.white;
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Bold SDF.asset");
        if (fontAsset != null) labelTmp.font = fontAsset;
        labelTmp.text = "BẠN\n3/3 HP";

        var aimGo = new GameObject("Aim Ruler");
        aimGo.transform.SetParent(playerGo.transform, false);
        var aimLr = aimGo.AddComponent<LineRenderer>();
        aimLr.positionCount = 2;
        aimLr.startWidth = 0.06f;
        aimLr.endWidth = 0.025f;
        aimLr.startColor = Color.yellow;
        aimLr.endColor = Color.yellow;
        aimLr.sharedMaterial = bodySr.sharedMaterial;
        aimLr.sortingOrder = 3;
        aimLr.enabled = false;

        var touchGo = new GameObject("Aim Touch Ring");
        touchGo.transform.SetParent(playerGo.transform, false);
        var touchLr = touchGo.AddComponent<LineRenderer>();
        touchLr.loop = true;
        touchLr.positionCount = 32;
        touchLr.startWidth = 0.025f;
        touchLr.endWidth = 0.025f;
        touchLr.startColor = new Color(1f, 0.85f, 0f, 0.65f);
        touchLr.endColor = new Color(1f, 0.85f, 0f, 0.65f);
        touchLr.sharedMaterial = bodySr.sharedMaterial;
        touchLr.sortingOrder = 3;
        touchLr.enabled = false;

        string playerPrefabPath = "Assets/Prefabs/PlayerView.prefab";
        PrefabUtility.SaveAsPrefabAsset(playerGo, playerPrefabPath);
        Object.DestroyImmediate(playerGo);
        Debug.Log("Saved prefab: " + playerPrefabPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void BuildTestScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7.0f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.08f, 0.12f);
        camGo.tag = "MainCamera";
        camGo.transform.position = new Vector3(0, 0, -10);

        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Bold SDF.asset");

        // 1. Title
        CreateText("SceneTitle", "HIT ME – KIỂM TRA ASSET & TIẾNG VIỆT (PHASE 2 BƯỚC 1)", new Vector3(0f, 6.1f, 0f), new Vector2(7.6f, 0.8f), 1.45f, new Color(1f, 0.82f, 0.2f), fontAsset);

        // 2. Subtitle
        CreateText("SceneSubtitle", "TMP Static Atlas: BeVietnamPro-Bold SDF (134 ký tự tiếng Việt + ASCII)", new Vector3(0f, 5.55f, 0f), new Vector2(7.6f, 0.6f), 0.95f, new Color(0.45f, 0.90f, 0.75f), fontAsset);

        // 3. Required strings validation box
        CreateText("VietnameseStringsBox", "Trạng thái: [CHỌN HƯỚNG NÉM]  •  [Đối thủ đang ẩn]\nNút bấm: [SẴN SÀNG]   |   Kết quả: [Thắng] - [Thua]\nThông số: VÒNG 1/5  •  15 GIÂY  •  BẠN (3/3 HP)", new Vector3(0f, 4.65f, 0f), new Vector2(7.6f, 1.2f), 0.98f, new Color(0.95f, 0.96f, 0.98f), fontAsset);

        // 4. Spawn 6 Players with 5 distinct characters + 1 tinted
        string[] playerNames = new[]
        {
            "BẠN (Nam Làng)",
            "Bot 1 (Anh Nón Lá)",
            "Bot 2 (Anh Núi)",
            "Bot 3 (Mai Hồng)",
            "Bot 4 (Chú Chó Vàng)",
            "Bot 5 (Cậu Bé Tinted)"
        };

        Vector3[] positions = new[]
        {
            new Vector3(-2.3f, 1.4f, 0f),
            new Vector3(0f, 1.4f, 0f),
            new Vector3(2.3f, 1.4f, 0f),
            new Vector3(-2.3f, -1.6f, 0f),
            new Vector3(0f, -1.6f, 0f),
            new Vector3(2.3f, -1.6f, 0f)
        };

        Color[] ringColors = new[]
        {
            Color.cyan,
            new Color(1f, 0.35f, 0.35f),
            new Color(0.35f, 1f, 0.45f),
            new Color(1f, 0.85f, 0.25f),
            new Color(0.85f, 0.45f, 1f),
            new Color(1f, 0.55f, 0.85f)
        };

        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerView.prefab");

        for (int i = 0; i < 6; i++)
        {
            GameObject pGo;
            if (playerPrefab != null)
            {
                pGo = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            }
            else
            {
                pGo = new GameObject("Player_" + i);
                pGo.AddComponent<PlayerView>();
            }

            pGo.name = "Player_" + i;
            pGo.transform.position = positions[i];
            var pv = pGo.GetComponent<PlayerView>();
            pv.Initialise("p" + i, ringColors[i], i);
            pv.SetHitboxVisible(false); // Turned off by default in gameplay test

            var label = pGo.GetComponentInChildren<TextMeshPro>();
            if (label != null)
            {
                label.rectTransform.sizeDelta = new Vector2(2.3f, 0.8f);
                label.fontSize = 0.95f;
                float labelY = (i == 4 ? 1.40f : 1.75f);
                label.transform.localPosition = new Vector3(0f, labelY, 0f);
                label.rectTransform.anchoredPosition = new Vector2(0f, labelY);
                label.text = playerNames[i] + "\n3/3 HP";
            }
        }

        // 5. Spawn flying weapon (Dép)
        var weaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WeaponView.prefab");
        if (weaponPrefab != null)
        {
            var wGo = (GameObject)PrefabUtility.InstantiatePrefab(weaponPrefab);
            wGo.name = "WeaponView_Demo";
            wGo.transform.position = new Vector3(-1.4f, -4.2f, 0f);
            wGo.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            wGo.transform.rotation = Quaternion.Euler(0, 0, 35f);
        }

        // 6. Spawn HitFx demo
        var hitFxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HitFx.prefab");
        if (hitFxPrefab != null)
        {
            var fxGo = (GameObject)PrefabUtility.InstantiatePrefab(hitFxPrefab);
            fxGo.name = "HitFx_Demo";
            fxGo.transform.position = new Vector3(1.4f, -4.2f, 0f);
            fxGo.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            var hComp = fxGo.GetComponent<HitFx>();
            if (hComp != null && hComp.frames != null && hComp.frames.Length > 2)
            {
                var sr = fxGo.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = hComp.frames[2];
            }
        }

        // 7. Weapon & VFX label
        CreateText("WeaponDemoLabel", "WeaponView: Dép bay xoay        HitFx: Hiệu ứng trúng đòn", new Vector3(0f, -5.1f, 0f), new Vector2(7.6f, 0.6f), 0.95f, new Color(0.6f, 0.7f, 0.85f), fontAsset);

        // 8. Footer Info
        CreateText("FooterInfo", "Sprite Atlases: Characters, UI, VFX, Env  •  Tất cả Prefabs & Tiếng Việt hợp lệ", new Vector3(0f, -6.1f, 0f), new Vector2(7.6f, 0.6f), 0.85f, new Color(0.4f, 0.48f, 0.6f), fontAsset);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Phase2_Step1_AssetTest.unity");
        Debug.Log("Saved Test Scene: Assets/Scenes/Phase2_Step1_AssetTest.unity");
    }

    [MenuItem("Tools/HIT ME/Build Hitbox Comparison Scene")]
    public static void BuildHitboxComparisonScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 2.45f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.08f, 0.12f);
        camGo.tag = "MainCamera";
        camGo.transform.position = new Vector3(0f, 0.25f, -10f);

        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Bold SDF.asset");

        // 1. Sân (Arena 9.0 x 11.4)
        var arenaGo = new GameObject("Arena");
        var arenaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/arena_background.png");
        if (arenaSprite != null)
        {
            var arenaBgGo = new GameObject("ArenaBackground");
            arenaBgGo.transform.SetParent(arenaGo.transform);
            arenaBgGo.transform.position = Vector3.zero;
            arenaBgGo.transform.localScale = new Vector3(1.32f, 1.25f, 1f); // 9.0 x 12.8 units
            var bgSr = arenaBgGo.AddComponent<SpriteRenderer>();
            bgSr.sprite = arenaSprite;
            bgSr.sortingOrder = -5;
        }

        // 2. Titles inside Arena
        CreateText("ArenaTitle", "HIT ME – CHUẨN HÓA 6 NHÂN VẬT & HITBOX TRONG SÂN (R = 0.55, SÂN 9.0 x 11.4)", new Vector3(0f, 2.40f, 0f), new Vector2(8.5f, 0.45f), 0.95f, new Color(1f, 0.88f, 0.2f), fontAsset);
        CreateText("ArenaSubtitle", "Nam Làng cao 1.50u (13% sân)  •  Chó Vàng cao 1.08u (72.5% người)  •  Bóng & vòng chân dẹt 0.6 (rộng 1.125u = 12.5% sân)  •  Hitbox R = 0.55", new Vector3(0f, 2.15f, 0f), new Vector2(8.5f, 0.40f), 0.68f, new Color(0.2f, 0.95f, 0.65f), fontAsset);

        // 3. Six characters standing side by side
        string[] names = new[]
        {
            "BẠN (Nam Làng)",
            "Bot 1 (Anh Nón Lá)",
            "Bot 2 (Anh Núi)",
            "Bot 3 (Mai Hồng)",
            "Bot 4 (Chó Vàng)",
            "Bot 5 (Tinted)"
        };

        string[] heightLabels = new[]
        {
            "Cao: 1.50u (13% sân)\nHitbox: R=0.55",
            "Cao: 1.34u (12% sân)\nHitbox: R=0.55",
            "Cao: 1.24u (11% sân)\nHitbox: R=0.55",
            "Cao: 1.35u (12% sân)\nHitbox: R=0.55",
            "Cao: 1.08u (72.5% người)\nHitbox: R=0.55",
            "Cao: 1.50u (13% sân)\nHitbox: R=0.55"
        };

        float[] xPositions = new[] { -2.85f, -1.71f, -0.57f, 0.57f, 1.71f, 2.85f };
        float groundY = -0.55f;

        Color[] ringColors = new[]
        {
            Color.cyan,
            new Color(1f, 0.35f, 0.35f),
            new Color(0.35f, 1f, 0.45f),
            new Color(1f, 0.85f, 0.25f),
            new Color(0.85f, 0.45f, 1f),
            new Color(1f, 0.55f, 0.85f)
        };

        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerView.prefab");

        for (int i = 0; i < 6; i++)
        {
            GameObject pGo;
            if (playerPrefab != null) pGo = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            else { pGo = new GameObject("Player_" + i); pGo.AddComponent<PlayerView>(); }

            pGo.name = "Player_SideBySide_" + i;
            pGo.transform.position = new Vector3(xPositions[i], groundY, 0f);
            var pv = pGo.GetComponent<PlayerView>();
            pv.Initialise("p" + i, ringColors[i], i);
            pv.SetHitboxVisible(true, 0.55f); // Enabled specifically for this hitbox comparison scene

            var label = pGo.GetComponentInChildren<TextMeshPro>();
            if (label != null)
            {
                label.rectTransform.sizeDelta = new Vector2(1.25f, 0.85f);
                label.fontSize = 0.65f;
                float labelY = (i == 4 ? 1.45f : 1.80f);
                label.transform.localPosition = new Vector3(0f, labelY, 0f);
                label.rectTransform.anchoredPosition = new Vector2(0f, labelY);
                label.text = names[i] + "\n" + heightLabels[i];
            }
        }

        // 4. Legend below characters
        CreateText("Legend", "Vòng xanh neon: Hitbox Core thực tế (R = 0.55, D = 1.10)   |   Vòng màu: Vòng viền chân mỏng procedural (Rộng 1.125u = 12.5% sân, Dẹt 0.6)\nBóng đen: Bóng elip mềm sinh bằng Code (Không ô vuông)   |   Touch: Vùng chạm vô hình D >= 9mm tách biệt hiển thị", new Vector3(0f, -1.30f, 0f), new Vector2(8.5f, 0.65f), 0.65f, new Color(0.95f, 0.95f, 0.98f), fontAsset);

        // 5. Arena dimension markers
        CreateText("ArenaDim", "SÂN GAME: 9.0 x 11.4 (DIỆN TÍCH 102.6)  •  MỌI NHÂN VẬT DÙNG CHUNG 1 HITBOX R = 0.55", new Vector3(0f, -1.75f, 0f), new Vector2(8.5f, 0.40f), 0.65f, new Color(0.85f, 0.75f, 0.45f), fontAsset);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Phase2_Step1_HitboxTest.unity");
        Debug.Log("Saved Hitbox Test Scene: Assets/Scenes/Phase2_Step1_HitboxTest.unity");
    }

    [MenuItem("Tools/HIT ME/Build True Arena Scene")]
    public static void BuildTrueArenaScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6.20f; // Formatted for 1080x2340 (9:19.5 iPhone aspect)
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.04f, 0.05f, 0.08f);
        camGo.tag = "MainCamera";
        camGo.transform.position = new Vector3(0f, 0.15f, -10f);

        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Bold SDF.asset");

        // 1. True Arena Background (ARENA.png)
        var arenaGo = new GameObject("TrueArena");
        var arenaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ARENA.png");
        if (arenaSprite == null) arenaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/arena_background.png");
        if (arenaSprite != null)
        {
            var arenaBgGo = new GameObject("ArenaBackground");
            arenaBgGo.transform.SetParent(arenaGo.transform);
            arenaBgGo.transform.position = new Vector3(0f, 0.15f, 0f);
            // ARENA.png is 1024x1536 at 150 PPU -> 6.827 x 10.24 units.
            // Scale by 1.22 gives playable arena width matching 9.0 units.
            arenaBgGo.transform.localScale = new Vector3(1.22f, 1.22f, 1f);
            var bgSr = arenaBgGo.AddComponent<SpriteRenderer>();
            bgSr.sprite = arenaSprite;
            bgSr.sortingOrder = -10;
        }

        // 2. Place 6 Characters across arena in combat positions
        // (including top, center lotus, sides, bottom)
        Vector3[] combatPositions = new[]
        {
            new Vector3(0.0f, -2.4f, 0f),   // Player 0: BẠN (Nam Làng) - lower center
            new Vector3(2.2f, -0.6f, 0f),   // Player 1: Bot 1 (Anh Nón Lá) - mid right
            new Vector3(-2.1f, 0.5f, 0f),   // Player 2: Bot 2 (Anh Núi) - mid left
            new Vector3(-1.8f, 3.1f, 0f),   // Player 3: Bot 3 (Mai Hồng) - upper left near top edge
            new Vector3(1.8f, 3.1f, 0f),    // Player 4: Bot 4 (Chú Chó Vàng) - upper right near top edge
            new Vector3(0.0f, 1.2f, 0f)     // Player 5: Bot 5 (Tinted Nam Làng) - center lotus
        };

        // Perspective depth scale factor based on Y: objects higher up (farther) are scaled down (~0.90 at top, 1.0 at bottom)
        float[] depthScales = new[] { 1.00f, 0.96f, 0.94f, 0.89f, 0.89f, 0.93f };

        Color[] ringColors = new[]
        {
            Color.cyan,
            new Color(1f, 0.35f, 0.35f),
            new Color(0.35f, 1f, 0.45f),
            new Color(1f, 0.85f, 0.25f),
            new Color(0.85f, 0.45f, 1f),
            new Color(1f, 0.55f, 0.85f)
        };

        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerView.prefab");

        for (int i = 0; i < 6; i++)
        {
            GameObject pGo;
            if (playerPrefab != null) pGo = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            else { pGo = new GameObject("Player_" + i); pGo.AddComponent<PlayerView>(); }

            pGo.name = "Player_Arena_" + i;
            pGo.transform.position = combatPositions[i];
            var pv = pGo.GetComponent<PlayerView>();
            pv.Initialise("p" + i, ringColors[i], i);
            pv.SetHitboxVisible(false); // DEBUG hitbox circle is OFF by default in true arena

            // Apply depth perspective scale
            float ds = depthScales[i];
            var body = pGo.transform.Find("Body");
            if (body != null)
            {
                body.localScale = new Vector3(body.localScale.x * ds, body.localScale.y * ds, 1f);
                var sr = body.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sortingOrder = (int)(100 - combatPositions[i].y * 10);
            }
            var shadow = pGo.transform.Find("Shadow");
            if (shadow != null)
            {
                shadow.localScale = new Vector3(shadow.localScale.x * ds, shadow.localScale.y * ds, 1f);
            }
            var footRing = pGo.transform.Find("FootRing");
            if (footRing != null)
            {
                footRing.localScale = new Vector3(footRing.localScale.x * ds, footRing.localScale.y * ds, 1f);
            }

            var label = pGo.GetComponentInChildren<TextMeshPro>();
            if (label != null)
            {
                label.gameObject.SetActive(false); // Clean view without labels on gameplay arena
            }
        }

        // Aim indicator demo for Player 0 (BẠN) towards Mai Hồng
        var p0Aim = GameObject.Find("Player_Arena_0/Aim Ruler");
        if (p0Aim != null)
        {
            var lr = p0Aim.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.enabled = true;
                Vector3 p0Pos = combatPositions[0];
                Vector3 targetDir = (combatPositions[3] - p0Pos).normalized;
                lr.SetPosition(0, p0Pos + targetDir * 0.75f);
                lr.SetPosition(1, p0Pos + targetDir * 3.5f);
            }
        }

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Phase2_Step1_TrueArenaTest.unity");
        Debug.Log("Saved True Arena Test Scene: Assets/Scenes/Phase2_Step1_TrueArenaTest.unity");
    }

    private static TextMeshPro CreateText(string name, string text, Vector3 pos, Vector2 size, float fontSize, Color color, TMP_FontAsset font)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.rectTransform.position = pos;
        tmp.rectTransform.localPosition = pos;
        tmp.rectTransform.anchoredPosition = new Vector2(pos.x, pos.y);
        tmp.rectTransform.sizeDelta = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = fontSize;
        tmp.color = color;
        if (font != null) tmp.font = font;
        tmp.text = text;
        EditorUtility.SetDirty(go);
        EditorUtility.SetDirty(tmp.rectTransform);
        return tmp;
    }

    [MenuItem("Tools/HIT ME/Capture Phase 2 Step 1 Evidence")]
    public static void CaptureEvidence()
    {
        string outDir = "Evidence/phase2";
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

        // 1. Capture Hitbox Standardization Scene
        BuildHitboxComparisonScene();
        EditorSceneManager.OpenScene("Assets/Scenes/Phase2_Step1_HitboxTest.unity");
        var cam = Camera.main ?? Object.FindAnyObjectByType<Camera>();
        if (cam != null)
        {
            // Landscape (16:9): characters fill horizontal width nicely
            cam.orthographicSize = 2.45f;
            cam.transform.position = new Vector3(0f, 0.25f, -10f);
            RenderAndSave(cam, 1920, 1080, Path.Combine(outDir, "step1_hitbox_standardization.png"));

            // Portrait (9:16): full arena view
            cam.orthographicSize = 5.80f;
            cam.transform.position = new Vector3(0f, 0.15f, -10f);
            RenderAndSave(cam, 1080, 1920, Path.Combine(outDir, "step1_hitbox_standardization_portrait.png"));
        }

        // 2. Capture True Arena Scene (1080x2340 iPhone standard)
        BuildTrueArenaScene();
        EditorSceneManager.OpenScene("Assets/Scenes/Phase2_Step1_TrueArenaTest.unity");
        cam = Camera.main ?? Object.FindAnyObjectByType<Camera>();
        if (cam != null)
        {
            cam.orthographicSize = 6.20f;
            cam.transform.position = new Vector3(0f, 0.15f, -10f);
            RenderAndSave(cam, 1080, 2340, Path.Combine(outDir, "arena_scale_1080x2340.png"));
        }

        // 3. Capture Asset Test Scene
        BuildTestScene();
        EditorSceneManager.OpenScene("Assets/Scenes/Phase2_Step1_AssetTest.unity");
        cam = Camera.main ?? Object.FindAnyObjectByType<Camera>();
        if (cam != null)
        {
            RenderAndSave(cam, 1080, 1920, Path.Combine(outDir, "step1_asset_test.png"));
            RenderAndSave(cam, 1080, 1920, Path.Combine(outDir, "step1_vietnamese_text.png"));
        }
        Debug.Log("Captured all evidence images to " + outDir);
    }

    private static void RenderAndSave(Camera cam, int width, int height, string filePath)
    {
        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        Object.DestroyImmediate(screenShot);
        File.WriteAllBytes(filePath, bytes);
        Debug.Log("Saved screenshot: " + filePath);
    }
}
#endif
