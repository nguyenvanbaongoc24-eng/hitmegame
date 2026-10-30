using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using HitMe.Audio;
using HitMe.Core;
using HitMe.Game;
using HitMe.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace HitMe.Editor
{
    public static class ProductionEvidenceCapture
    {
        private const string ScenePath = "Assets/Scenes/Graybox.unity";
        private const string SceneName = "Graybox";
        private const string EvidenceDir = "Evidence/phase3";
        private const int ShotWidth = 1080;
        private const int ShotHeight = 2340;

        [MenuItem("Tools/HIT ME/Capture 6 Evidence Screenshots (1080x2340)")]
        public static void CaptureAllEvidence()
        {
            Directory.CreateDirectory(EvidenceDir);

            string commitShort = WebBuildPipeline.WriteBuildFingerprintResource(SceneName);
            string dateTag = DateTime.Now.ToString("yyyyMMdd");
            string buildId = $"WebGL_{dateTag}_{commitShort}";

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvasObj = GameObject.Find("ProductionCanvas");
            if (canvasObj == null)
            {
                ProductionUISetup.BuildProductionUI();
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                canvasObj = GameObject.Find("ProductionCanvas");
            }

            var configAsset = AssetDatabase.LoadAssetAtPath<GameConfigAsset>("Assets/ScriptableObjects/GameConfig_Phase1.asset");
            if (configAsset != null)
            {
                PlayerView.ConfigureSharedResources(configAsset);
                WeaponFlightView.ConfigureSharedResources(configAsset);
            }

            var cam = Camera.main;
            if (cam == null) cam = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (cam == null)
            {
                Debug.LogError("[EvidenceCapture] No camera found in scene!");
                return;
            }

            cam.orthographic = true;
            cam.orthographicSize = 7.8f;
            cam.transform.position = new Vector3(0f, 0.80f, -10f);

            var canvas = canvasObj.GetComponent<Canvas>();
            var ui = canvasObj.GetComponent<ProductionUIController>();
            var scaler = canvasObj.GetComponent<CanvasScaler>();
            var fitter = canvasObj.GetComponentInChildren<SafeAreaFitter>(true);

            ui.ResolveHudElementContainers();
            ui.EnsureAndRefreshBuildFingerprint();

            var originalRenderMode = canvas.renderMode;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1.0f;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ShotWidth, ShotHeight);
            scaler.matchWidthOrHeight = 1.0f;

            if (fitter != null)
            {
                fitter.ApplyForScreen(ShotWidth, ShotHeight, new Rect(0f, 0f, ShotWidth, ShotHeight), false);
            }

            // 6 distinct characters in a 6-player match (indices: 0, 4, 3, 5, 1, 6 -> Nam Làng, Anh Teen, Cô Ba, Chó Vàng, Nữ Năng Động, Mèo Mun)
            int[] matchRosterIndices = { 0, 4, 3, 5, 1, 6 };
            var players = CreateMockPlayers(matchRosterIndices);
            var views = CreatePlayerViews(players, matchRosterIndices);

            try
            {
                // ==================== 1. HOME SCREEN ====================
                SetPlayersVisible(views, false);
                ui.ShowScreen(ProductionScreen.Home);
                ui.EnsureAndRefreshBuildFingerprint();
                RenderAndSave(cam, Path.Combine(EvidenceDir, "01_home.png"), commitShort, buildId, ShotWidth, ShotHeight);

                // ==================== 2. LOBBY SCREEN ====================
                ui.ShowScreen(ProductionScreen.Lobby);
                ui.txtLobbyHeader.text = "PHÒNG CHIẾN HỮU • SÂN LÀNG";
                ui.txtLobbyRoomCode.text = "MÃ PHÒNG: HM8888 • SÂN LÀNG";
                ui.txtLobbyCountdown.text = "Đang chờ tất cả người chơi sẵn sàng...";
                ui.EnsureAndRefreshBuildFingerprint();
                SetPlayersVisible(views, false);
                RenderAndSave(cam, Path.Combine(EvidenceDir, "02_lobby.png"), commitShort, buildId, ShotWidth, ShotHeight);

                // ==================== 3. PLACING (1080x2340) ====================
                ui.ShowScreen(ProductionScreen.Gameplay);
                ui.txtRound.text = "VÒNG 2/5";
                ui.txtTimer.text = "4.2s";
                ui.txtTimer.color = Color.white;
                ui.ApplyPhaseState(Phase.Placing);
                ui.EnsureAndRefreshBuildFingerprint();

                UpdateHudHp(ui, players, isPlacing: true);

                foreach (var kvp in views)
                {
                    bool isLocal = kvp.Key == "player";
                    kvp.Value.gameObject.SetActive(isLocal);
                    if (isLocal)
                    {
                        var p = players.Find(x => x.Id == kvp.Key);
                        kvp.Value.Present(p, true, true, true, 1.2f, 1.5f, 2.0f);
                    }
                }
                RenderAndSave(cam, Path.Combine(EvidenceDir, "03_placing_local_only.png"), commitShort, buildId, ShotWidth, ShotHeight);
                File.Copy(Path.Combine(EvidenceDir, "03_placing_local_only.png"), Path.Combine(EvidenceDir, "p4_01_placing_1080x2340.png"), true);
                WriteProvenanceJson(Path.Combine(EvidenceDir, "p4_01_placing_1080x2340.png"), commitShort, buildId, ShotWidth, ShotHeight);

                // Capture iPhone Safe Area Portrait & Wide Desktop 1920x1080 in Placing (P2)
                if (fitter != null)
                {
                    fitter.ApplyForScreen(ShotWidth, ShotHeight, new Rect(0f, 0f, ShotWidth, ShotHeight), true);
                    RenderAndSave(cam, Path.Combine(EvidenceDir, "p2_iphone_safearea_1080x2340.png"), commitShort, buildId, ShotWidth, ShotHeight);

                    fitter.ApplyForScreen(1920, 1080, new Rect(0f, 0f, 1920f, 1080f), false);
                    RenderAndSave(cam, Path.Combine(EvidenceDir, "p2_desktop_wide_1920x1080.png"), commitShort, buildId, 1920, 1080);

                    fitter.ApplyForScreen(ShotWidth, ShotHeight, new Rect(0f, 0f, ShotWidth, ShotHeight), false);
                }

                // ==================== 4. REVEAL (1080x2340) ====================
                ui.ApplyPhaseState(Phase.Reveal);
                ui.EnsureAndRefreshBuildFingerprint();
                UpdateHudHp(ui, players, isPlacing: false);

                foreach (var kvp in views)
                {
                    var p = players.Find(x => x.Id == kvp.Key);
                    kvp.Value.gameObject.SetActive(p.IsAlive);
                    if (p.IsAlive)
                    {
                        kvp.Value.Present(p, true, true, false, 1.2f, 1.5f, 0f);
                    }
                }
                RenderAndSave(cam, Path.Combine(EvidenceDir, "04_reveal_6players.png"), commitShort, buildId, ShotWidth, ShotHeight);
                File.Copy(Path.Combine(EvidenceDir, "04_reveal_6players.png"), Path.Combine(EvidenceDir, "p4_02_reveal_1080x2340.png"), true);
                WriteProvenanceJson(Path.Combine(EvidenceDir, "p4_02_reveal_1080x2340.png"), commitShort, buildId, ShotWidth, ShotHeight);
                File.Copy(Path.Combine(EvidenceDir, "04_reveal_6players.png"), Path.Combine("Evidence/phase2", "step2_characters.png"), true);
                WriteProvenanceJson(Path.Combine("Evidence/phase2", "step2_characters.png"), commitShort, buildId, ShotWidth, ShotHeight);

                // ==================== 5. THROWING (1080x2340) ====================
                ui.ApplyPhaseState(Phase.Throwing);
                ui.EnsureAndRefreshBuildFingerprint();
                var slippers = SpawnTestSlippers(players);
                foreach (var kvp in views)
                {
                    var p = players.Find(x => x.Id == kvp.Key);
                    kvp.Value.gameObject.SetActive(p.IsAlive);
                    if (p.IsAlive)
                    {
                        kvp.Value.Present(p, true, false, false, 1.2f, 1.5f, 0f);
                    }
                }
                RenderAndSave(cam, Path.Combine(EvidenceDir, "05_throwing_slippers.png"), commitShort, buildId, ShotWidth, ShotHeight);
                File.Copy(Path.Combine(EvidenceDir, "05_throwing_slippers.png"), Path.Combine(EvidenceDir, "p4_03_throwing_1080x2340.png"), true);
                WriteProvenanceJson(Path.Combine(EvidenceDir, "p4_03_throwing_1080x2340.png"), commitShort, buildId, ShotWidth, ShotHeight);
                File.Copy(Path.Combine(EvidenceDir, "05_throwing_slippers.png"), Path.Combine("Evidence/phase2", "step3_sandal_throw.png"), true);
                WriteProvenanceJson(Path.Combine("Evidence/phase2", "step3_sandal_throw.png"), commitShort, buildId, ShotWidth, ShotHeight);
                ClearTestSlippers(slippers);

                // ==================== 6. ROUND END (1080x2340) ====================
                ui.ApplyPhaseState(Phase.RoundEnd);
                ui.EnsureAndRefreshBuildFingerprint();
                UpdateHudHp(ui, players, isPlacing: false);
                foreach (var kvp in views)
                {
                    var p = players.Find(x => x.Id == kvp.Key);
                    kvp.Value.gameObject.SetActive(p.IsAlive);
                    if (p.IsAlive)
                    {
                        kvp.Value.Present(p, true, false, false, 1.2f, 1.5f, 0f);
                    }
                }
                RenderAndSave(cam, Path.Combine(EvidenceDir, "p4_04_roundend_1080x2340.png"), commitShort, buildId, ShotWidth, ShotHeight);

                // ==================== 7. MATCH END / RESULT (1080x2340) ====================
                SetPlayersVisible(views, false);
                var ranked = new List<PlayerState>(players);
                ranked.Sort(MatchRanking.DisplayOrderComparison);
                var winners = new List<string>(MatchRanking.Winners(players));
                ui.ApplyPhaseState(Phase.MatchEnd);
                ui.PopulateResultBoard(ranked, winners);
                ui.EnsureAndRefreshBuildFingerprint();

                RenderAndSave(cam, Path.Combine(EvidenceDir, "06_result_full_rankings.png"), commitShort, buildId, ShotWidth, ShotHeight);
                File.Copy(Path.Combine(EvidenceDir, "06_result_full_rankings.png"), Path.Combine(EvidenceDir, "p4_05_matchend_1080x2340.png"), true);
                WriteProvenanceJson(Path.Combine(EvidenceDir, "p4_05_matchend_1080x2340.png"), commitShort, buildId, ShotWidth, ShotHeight);
                File.Copy(Path.Combine(EvidenceDir, "06_result_full_rankings.png"), Path.Combine(EvidenceDir, "p1_matchend_after.png"), true);
                WriteProvenanceJson(Path.Combine(EvidenceDir, "p1_matchend_after.png"), commitShort, buildId, ShotWidth, ShotHeight);

                // ==================== 8. FULL 7-CHARACTER ROSTER SHOWCASE (F1) ====================
                ui.ShowScreen(ProductionScreen.Gameplay);
                ui.ApplyPhaseState(Phase.Reveal);
                ui.txtPhaseBanner.text = "ĐỦ 7 NHÂN VẬT THẬT • KHÔNG TINT MÀU";
                ui.EnsureAndRefreshBuildFingerprint();
                SetPlayersVisible(views, false);

                var all7Players = CreateAll7RosterPlayers();
                var all7Views = CreatePlayerViews(all7Players, new[] { 0, 1, 2, 3, 4, 5, 6 });
                try
                {
                    foreach (var kvp in all7Views)
                    {
                        kvp.Value.gameObject.SetActive(true);
                        var p = all7Players.Find(x => x.Id == kvp.Key);
                        kvp.Value.Present(p, true, false, false, 1.2f, 1.5f, 0f);
                    }
                    RenderAndSave(cam, Path.Combine(EvidenceDir, "07_roster_7characters.png"), commitShort, buildId, ShotWidth, ShotHeight);
                }
                finally
                {
                    foreach (var kvp in all7Views)
                    {
                        if (kvp.Value != null) UnityEngine.Object.DestroyImmediate(kvp.Value.gameObject);
                    }
                }

                // ==================== 9. FIVE AUDIT RESOLUTIONS (PHASE 2 AUDIT CHECK #9) ====================
                ui.ShowScreen(ProductionScreen.Gameplay);
                ui.ApplyPhaseState(Phase.Placing);
                ui.EnsureAndRefreshBuildFingerprint();
                UpdateHudHp(ui, players, isPlacing: true);

                foreach (var kvp in views)
                {
                    bool isLocal = kvp.Key == "player";
                    kvp.Value.gameObject.SetActive(isLocal);
                    if (isLocal)
                    {
                        var p = players.Find(x => x.Id == kvp.Key);
                        kvp.Value.Present(p, true, true, true, 1.2f, 1.5f, 2.0f);
                    }
                }

                (int w, int h)[] auditResolutions = new[]
                {
                    (1080, 1920),
                    (1179, 2556),
                    (1080, 2400),
                    (1080, 2520),
                    (1620, 2160)
                };

                foreach (var res in auditResolutions)
                {
                    if (fitter != null)
                    {
                        fitter.ApplyForScreen(res.w, res.h, new Rect(0f, 0f, res.w, res.h), false);
                    }
                    RenderAndSave(cam, Path.Combine(EvidenceDir, $"res_{res.w}x{res.h}_placing.png"), commitShort, buildId, res.w, res.h);
                }

                if (fitter != null)
                {
                    fitter.ApplyForScreen(ShotWidth, ShotHeight, new Rect(0f, 0f, ShotWidth, ShotHeight), false);
                }

                // ==================== 10. RULE V1.3 PLAYER COUNTS N = 2..6 (1080x2340) ====================
                for (int n = 2; n <= 6; n++)
                {
                    // HUD in Placing with N cards
                    ui.ShowScreen(ProductionScreen.Gameplay);
                    ui.ApplyPhaseState(Phase.Placing);
                    ui.EnsureAndRefreshBuildFingerprint();
                    ui.ConfigureHpCardsLayout(n);
                    var subPlayers = players.GetRange(0, n);
                    UpdateHudHp(ui, subPlayers, isPlacing: true);

                    foreach (var kvp in views)
                    {
                        bool inMatch = subPlayers.Exists(x => x.Id == kvp.Key);
                        bool isLocal = kvp.Key == "player";
                        kvp.Value.gameObject.SetActive(inMatch && isLocal);
                        if (inMatch && isLocal)
                        {
                            var p = subPlayers.Find(x => x.Id == kvp.Key);
                            kvp.Value.Present(p, true, true, true, 1.2f, 1.5f, 2.0f);
                        }
                    }

                    RenderAndSave(cam, Path.Combine(EvidenceDir, $"res_1080x2340_hud_N{n}.png"), commitShort, buildId, ShotWidth, ShotHeight);

                    // Result Panel with N rows
                    SetPlayersVisible(views, false);
                    var subRanked = new List<PlayerState>(subPlayers);
                    subRanked.Sort(MatchRanking.DisplayOrderComparison);
                    var subWinners = new List<string>(MatchRanking.Winners(subPlayers));
                    ui.ApplyPhaseState(Phase.MatchEnd);
                    ui.PopulateResultBoard(subRanked, subWinners);
                    ui.EnsureAndRefreshBuildFingerprint();

                    RenderAndSave(cam, Path.Combine(EvidenceDir, $"res_1080x2340_result_N{n}.png"), commitShort, buildId, ShotWidth, ShotHeight);
                }

                // ==================== 11. S9 W1 SANDAL SPRITE DIAGNOSIS ====================
                CaptureW1SandalDiagnosis(cam, commitShort, buildId);

                Debug.Log("[EvidenceCapture] All evidence screenshots captured with provenance to " + EvidenceDir);
            }
            finally
            {
                foreach (var kvp in views)
                {
                    if (kvp.Value != null) UnityEngine.Object.DestroyImmediate(kvp.Value.gameObject);
                }

                canvas.renderMode = originalRenderMode;
                // Re-open clean scene without saving temporary render mode changes so working tree stays clean
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        private static void CaptureW1SandalDiagnosis(Camera cam, string commitShort, string buildId)
        {
            string outPath = Path.Combine(EvidenceDir, "w1_sandal_sprite_diagnosis.png");
            int width = 1024;
            int height = 512;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            Color darkBg = new Color(0.08f, 0.10f, 0.14f);
            Color panelBg = new Color(0.13f, 0.16f, 0.22f);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = darkBg;
            tex.SetPixels(pixels);

            // Draw two background panel rectangles
            for (int y = 40; y < height - 60; y++)
            {
                for (int x = 40; x < (width / 2) - 20; x++)
                {
                    tex.SetPixel(x, y, panelBg);
                }
                for (int x = (width / 2) + 20; x < width - 40; x++)
                {
                    tex.SetPixel(x, y, panelBg);
                }
            }

            // Blit sandal projectile sprite on the left panel
            var sandalTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/dep_projectile.png");
            if (sandalTex == null) sandalTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Weapons/dep_to_ong_projectile.png");

            if (sandalTex != null)
            {
                RenderTexture tmpRt = RenderTexture.GetTemporary(sandalTex.width, sandalTex.height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(sandalTex, tmpRt);
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = tmpRt;
                Texture2D readableSandal = new Texture2D(sandalTex.width, sandalTex.height, TextureFormat.RGBA32, false);
                readableSandal.ReadPixels(new Rect(0, 0, sandalTex.width, sandalTex.height), 0, 0);
                readableSandal.Apply();
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(tmpRt);

                int targetSandalW = 280;
                int targetSandalH = (int)(280f * readableSandal.height / readableSandal.width);
                int startX = ((width / 2) - 60 - targetSandalW) / 2 + 40;
                int startY = (height - targetSandalH) / 2;

                for (int py = 0; py < targetSandalH; py++)
                {
                    for (int px = 0; px < targetSandalW; px++)
                    {
                        float u = (float)px / targetSandalW;
                        float v = (float)py / targetSandalH;
                        Color c = readableSandal.GetPixelBilinear(u, v);
                        if (c.a > 0.05f)
                        {
                            Color bg = tex.GetPixel(startX + px, startY + py);
                            tex.SetPixel(startX + px, startY + py, Color.Lerp(bg, c, c.a));
                        }
                    }
                }
                UnityEngine.Object.DestroyImmediate(readableSandal);
            }

            // Blit character Nam Lang on the right panel
            var charTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/nam_lang_idle.png");
            if (charTex != null)
            {
                RenderTexture tmpRt = RenderTexture.GetTemporary(charTex.width, charTex.height, 0, RenderTextureFormat.ARGB32);
                Graphics.Blit(charTex, tmpRt);
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = tmpRt;
                Texture2D readableChar = new Texture2D(charTex.width, charTex.height, TextureFormat.RGBA32, false);
                readableChar.ReadPixels(new Rect(0, 0, charTex.width, charTex.height), 0, 0);
                readableChar.Apply();
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(tmpRt);

                int targetCharH = 340;
                int targetCharW = (int)(340f * readableChar.width / readableChar.height);
                int startX = (width / 2) + 20 + ((width / 2) - 60 - targetCharW) / 2;
                int startY = (height - targetCharH) / 2;

                for (int py = 0; py < targetCharH; py++)
                {
                    for (int px = 0; px < targetCharW; px++)
                    {
                        float u = (float)px / targetCharW;
                        float v = (float)py / targetCharH;
                        Color c = readableChar.GetPixelBilinear(u, v);
                        if (c.a > 0.05f)
                        {
                            Color bg = tex.GetPixel(startX + px, startY + py);
                            tex.SetPixel(startX + px, startY + py, Color.Lerp(bg, c, c.a));
                        }
                    }
                }
                UnityEngine.Object.DestroyImmediate(readableChar);
            }

            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(outPath, bytes);
            UnityEngine.Object.DestroyImmediate(tex);

            WriteProvenanceJson(outPath, commitShort, buildId, width, height);
            Debug.Log($"[EvidenceCapture] Saved W1 sandal diagnosis to {outPath}");
        }

        private static void RenderAndSave(Camera cam, string outputPath, string commitShort, string buildId, int width, int height)
        {
            Canvas.ForceUpdateCanvases();
            var tmps = UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < tmps.Length; i++)
            {
                tmps[i].ForceMeshUpdate(true, true);
            }
            Canvas.ForceUpdateCanvases();

            var rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            var prevCamRt = cam.targetTexture;
            var prevRt = RenderTexture.active;

            try
            {
                cam.targetTexture = rt;
                cam.Render();

                RenderTexture.active = rt;
                var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();

                byte[] pngBytes = tex.EncodeToPNG();
                File.WriteAllBytes(outputPath, pngBytes);
                UnityEngine.Object.DestroyImmediate(tex);

                WriteProvenanceJson(outputPath, commitShort, buildId, width, height);
                Debug.Log($"[EvidenceCapture] Saved: {outputPath} ({width}x{height}) [commit={commitShort}]");
            }
            finally
            {
                cam.targetTexture = prevCamRt;
                RenderTexture.active = prevRt;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        private static void WriteProvenanceJson(string filePath, string commitShort, string buildId, int width, int height)
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            string sha256;
            using (var hasher = SHA256.Create())
            {
                byte[] hash = hasher.ComputeHash(bytes);
                var sb = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("x2"));
                sha256 = sb.ToString();
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK");
            string normalizedPath = filePath.Replace('\\', '/');
            string json = "{\n" +
                          $"  \"file\": \"{normalizedPath}\",\n" +
                          $"  \"sha256\": \"{sha256}\",\n" +
                          $"  \"commit\": \"{commitShort}\",\n" +
                          $"  \"scene\": \"{SceneName}\",\n" +
                          $"  \"build_id\": \"{buildId}\",\n" +
                          $"  \"resolution\": \"{width}x{height}\",\n" +
                          $"  \"timestamp\": \"{timestamp}\",\n" +
                          "  \"command\": \"HitMe.Editor.ProductionEvidenceCapture.CaptureAllEvidence\",\n" +
                          "  \"tool\": \"AG / Unity Batchmode Camera.Render\"\n" +
                          "}\n";
            File.WriteAllText(filePath + ".json", json, Encoding.UTF8);
        }

        private static List<PlayerState> CreateMockPlayers(int[] charIndices)
        {
            Vec2[] positions =
            {
                new Vec2(1.8f, -3.1f),
                new Vec2(-2.1f, -2.8f),
                new Vec2(1.1f, -1.2f),
                new Vec2(-1.8f, 1.5f),
                new Vec2(-2.1f, -1.7f),
                new Vec2(2.0f, 1.8f)
            };
            Vec2[] aims =
            {
                new Vec2(-0.5f, 0.86f),
                new Vec2(0.7f, 0.7f),
                new Vec2(-0.7f, -0.7f),
                new Vec2(0.5f, -0.8f),
                new Vec2(0.8f, 0.6f),
                new Vec2(-0.6f, -0.8f)
            };
            // Matches user screenshot scenario: 4 alive (2/3, 2/3, 1/3, 2/3 HP) and 2 eliminated (0/3 HP)
            int[] hps = { 2, 2, 1, 0, 2, 0 };
            int[] hits = { 3, 3, 2, 1, 1, 0 };

            var list = new List<PlayerState>(6);
            for (int i = 0; i < 6; i++)
            {
                string id = i == 0 ? "player" : $"bot-{i}";
                string name = PlayerView.CharacterNames[charIndices[i]];
                var p = new PlayerState(id, name, i > 0, hps[i], positions[i], aims[i]);
                p.HitsLanded = hits[i];
                list.Add(p);
            }
            return list;
        }

        private static List<PlayerState> CreateAll7RosterPlayers()
        {
            Vec2[] positions =
            {
                new Vec2(-2.5f, 2.6f),
                new Vec2(0.0f, 2.6f),
                new Vec2(2.5f, 2.6f),
                new Vec2(-2.5f, -0.2f),
                new Vec2(0.0f, -0.2f),
                new Vec2(2.5f, -0.2f),
                new Vec2(0.0f, -3.1f)
            };
            var list = new List<PlayerState>(7);
            for (int i = 0; i < 7; i++)
            {
                string id = i == 0 ? "player" : $"roster_{i}";
                string name = PlayerView.CharacterNames[i];
                list.Add(new PlayerState(id, name, i > 0, 3, positions[i], new Vec2(0f, 1f)));
            }
            return list;
        }

        private static Dictionary<string, PlayerView> CreatePlayerViews(List<PlayerState> players, int[] charIndices)
        {
            var dict = new Dictionary<string, PlayerView>();
            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
                var go = new GameObject("View_" + p.Id);
                var view = go.AddComponent<PlayerView>();
                int charIdx = (charIndices != null && i < charIndices.Length) ? charIndices[i] : i;
                view.Initialise(p.Id, Color.white, charIdx);
                go.transform.position = new Vector3(p.Position.X, p.Position.Y, 0f);
                dict[p.Id] = view;
            }
            return dict;
        }

        private static void SetPlayersVisible(Dictionary<string, PlayerView> views, bool visible)
        {
            foreach (var kvp in views)
            {
                if (kvp.Value != null) kvp.Value.gameObject.SetActive(visible);
            }
        }

        private static void UpdateHudHp(ProductionUIController ui, List<PlayerState> players, bool isPlacing)
        {
            for (int i = 0; i < ui.hpCards.Count && i < players.Count; i++)
            {
                var card = ui.hpCards[i];
                var p = players[i];
                if (card.root != null) card.root.SetActive(true);
                if (card.txtName != null) card.txtName.text = p.Name;

                // S3: HP is ALWAYS public in all phases (only position & aim direction are hidden during Placing).
                if (card.txtHearts != null)
                {
                    card.txtHearts.text = $"HP: {p.Hp}/3";
                    card.txtHearts.color = p.IsAlive
                        ? (p.Hp == 3 ? new Color(0.25f, 1f, 0.45f) : (p.Hp == 2 ? new Color(1f, 0.86f, 0.25f) : new Color(1f, 0.42f, 0.42f)))
                        : new Color(1f, 0.45f, 0.45f);
                }

                bool isLocal = p.Id == "player";
                if (!p.IsAlive)
                {
                    if (card.txtHpNum != null)
                    {
                        card.txtHpNum.text = "ĐÃ LOẠI";
                        card.txtHpNum.color = new Color(1f, 0.42f, 0.42f);
                    }
                    if (card.imgBg != null)
                    {
                        card.imgBg.color = new Color(0.55f, 0.42f, 0.45f, 0.92f);
                    }
                }
                else if (isPlacing && !isLocal)
                {
                    if (card.txtHpNum != null)
                    {
                        card.txtHpNum.text = "ĐANG ẨN";
                        card.txtHpNum.color = new Color(0.65f, 0.72f, 0.80f);
                    }
                    if (card.imgBg != null)
                    {
                        card.imgBg.color = new Color(0.92f, 0.95f, 1f, 1f);
                    }
                }
                else
                {
                    if (card.txtHpNum != null)
                    {
                        card.txtHpNum.text = "CÒN SỐNG";
                        card.txtHpNum.color = new Color(0.45f, 0.95f, 0.65f);
                    }
                    if (card.imgBg != null)
                    {
                        card.imgBg.color = Color.white;
                    }
                }
            }
        }

        private static List<GameObject> SpawnTestSlippers(List<PlayerState> players)
        {
            var list = new List<GameObject>();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/dep_projectile.png") 
                      ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/dep_icon.png");

            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
                if (!p.IsAlive) continue;
                var target = players[(i + 1) % players.Count];
                var mid = Vector3.Lerp(new Vector3(p.Position.X, p.Position.Y, 0f), new Vector3(target.Position.X, target.Position.Y, 0f), 0.45f);

                var go = new GameObject("TestSlipper_" + p.Id);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = 5;
                go.transform.position = mid;
                go.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
                Vector3 dir = (new Vector3(target.Position.X, target.Position.Y, 0f) - new Vector3(p.Position.X, p.Position.Y, 0f)).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                go.transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
                list.Add(go);
            }
            return list;
        }

        private static void ClearTestSlippers(List<GameObject> slippers)
        {
            foreach (var go in slippers)
            {
                if (go != null) UnityEngine.Object.DestroyImmediate(go);
            }
            slippers.Clear();
        }
    }
}
