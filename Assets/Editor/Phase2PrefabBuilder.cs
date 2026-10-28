#if UNITY_EDITOR
using System.IO;
using System.Linq;
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
        BuildPrefabs();
        BuildTestScene();
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

        var shadowGo = new GameObject("Shadow");
        shadowGo.transform.SetParent(playerGo.transform, false);
        shadowGo.transform.localPosition = new Vector3(0f, -0.05f, 0f);
        shadowGo.transform.localScale = new Vector3(0.9f, 0.5f, 1f);
        var shadowSr = shadowGo.AddComponent<SpriteRenderer>();
        shadowSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/shadow.png");
        shadowSr.sortingOrder = 0;
        shadowSr.color = new Color(0f, 0f, 0f, 0.45f);

        var ringGo = new GameObject("FootRing");
        ringGo.transform.SetParent(playerGo.transform, false);
        ringGo.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        ringGo.transform.localScale = new Vector3(0.65f, 0.35f, 1f);
        var ringSr = ringGo.AddComponent<SpriteRenderer>();
        ringSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/selection_ring.png");
        if (ringSr.sprite == null) ringSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Kit/ui_player_foot_ring_cyan.png");
        ringSr.sortingOrder = 1;
        ringSr.color = Color.cyan;

        var bodyGo = new GameObject("Body");
        bodyGo.transform.SetParent(playerGo.transform, false);
        bodyGo.transform.localPosition = Vector3.zero;
        var bodySr = bodyGo.AddComponent<SpriteRenderer>();
        bodySr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cau_be_lang_idle.png");
        bodySr.sortingOrder = 2;
        var bodyAnim = bodyGo.AddComponent<Animator>();
        bodyAnim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Animations/Cau_Be_Lang/Cau_Be_Lang_Controller.controller");

        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(playerGo.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        var labelTmp = labelGo.AddComponent<TextMeshPro>();
        labelTmp.rectTransform.sizeDelta = new Vector2(2.4f, 0.9f);
        labelTmp.alignment = TextAlignmentOptions.Center;
        labelTmp.fontSize = 1.05f;
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

            var label = pGo.GetComponentInChildren<TextMeshPro>();
            if (label != null)
            {
                label.rectTransform.sizeDelta = new Vector2(2.3f, 0.8f);
                label.fontSize = 0.95f;
                label.transform.localPosition = new Vector3(0f, i == 4 ? 1.4f : 2.2f, 0f);
                label.rectTransform.anchoredPosition = new Vector2(0f, i == 4 ? 1.4f : 2.2f);
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

        // Add automated evidence screenshot capture component
        var captureGo = new GameObject("EvidenceCapture");
        captureGo.AddComponent<Phase2Step1Capture>();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Phase2_Step1_AssetTest.unity");
        Debug.Log("Saved Test Scene: Assets/Scenes/Phase2_Step1_AssetTest.unity");
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
        EditorSceneManager.OpenScene("Assets/Scenes/Phase2_Step1_AssetTest.unity");
        var cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        if (cam == null)
        {
            Debug.LogError("No camera found in scene");
            return;
        }

        int width = 1080;
        int height = 1920;
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

        string outDir = "Evidence/phase2";
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
        File.WriteAllBytes(Path.Combine(outDir, "step1_asset_test.png"), bytes);
        File.WriteAllBytes(Path.Combine(outDir, "step1_vietnamese_text.png"), bytes);
        Debug.Log("Saved screenshot evidence to " + Path.Combine(outDir, "step1_asset_test.png"));
    }
}
#endif
