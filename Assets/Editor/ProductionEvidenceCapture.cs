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
            cam.orthographicSize = 7.5f;
            cam.transform.position = new Vector3(0f, 0.2f, -10f);

            var canvas = canvasObj.GetComponent<Canvas>();
            var ui = canvasObj.GetComponent<ProductionUIController>();
            var scaler = canvasObj.GetComponent<CanvasScaler>();

            ui.EnsureAndRefreshBuildFingerprint();

            var originalRenderMode = canvas.renderMode;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1.0f;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ShotWidth, ShotHeight);
            scaler.matchWidthOrHeight = 1.0f;

            // 6 distinct characters in a 6-player match (indices: 0, 1, 2, 3, 5, 6 -> includes Nữ Năng Động, Chó Vàng, Mèo Mun)
            int[] matchRosterIndices = { 0, 1, 2, 3, 5, 6 };
            var players = CreateMockPlayers(matchRosterIndices);
            var views = CreatePlayerViews(players, matchRosterIndices);

            try
            {
                // ==================== 1. HOME SCREEN ====================
                SetPlayersVisible(views, false);
                ui.ShowScreen(ProductionScreen.Home);
                ui.EnsureAndRefreshBuildFingerprint();
                RenderAndSave(cam, Path.Combine(EvidenceDir, "01_home.png"), commitShort, buildId);

                // ==================== 2. LOBBY SCREEN ====================
                ui.ShowScreen(ProductionScreen.Lobby);
                ui.txtLobbyHeader.text = "PHÒNG CHIẾN HỮU • SÂN LÀNG";
                ui.txtLobbyRoomCode.text = "MÃ PHÒNG: HM8888 • SÂN LÀNG";
                ui.txtLobbyCountdown.text = "Đang chờ tất cả người chơi sẵn sàng...";
                ui.EnsureAndRefreshBuildFingerprint();
                SetPlayersVisible(views, false);
                RenderAndSave(cam, Path.Combine(EvidenceDir, "02_lobby.png"), commitShort, buildId);

                // ==================== 3. PLACING (LOCAL ONLY + AIM RULER) ====================
                ui.ShowScreen(ProductionScreen.Gameplay);
                ui.txtRound.text = "VÒNG 1/5";
                ui.txtTimer.text = "4.2s";
                ui.txtTimer.color = Color.white;
                ui.txtPhaseBanner.text = "CHỌN HƯỚNG NÉM • Chạm để đặt, kéo để ngắm";
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
                RenderAndSave(cam, Path.Combine(EvidenceDir, "03_placing_local_only.png"), commitShort, buildId);

                // ==================== 4. REVEAL (ALL 6 DISTINCT PLAYERS, NO TINT) ====================
                ui.txtPhaseBanner.text = "LỘ DIỆN ĐỐI THỦ!";
                ui.txtTimer.text = "0.0s";
                ui.EnsureAndRefreshBuildFingerprint();
                UpdateHudHp(ui, players, isPlacing: false);

                foreach (var kvp in views)
                {
                    kvp.Value.gameObject.SetActive(true);
                    var p = players.Find(x => x.Id == kvp.Key);
                    kvp.Value.Present(p, true, true, false, 1.2f, 1.5f, 0f);
                }
                RenderAndSave(cam, Path.Combine(EvidenceDir, "04_reveal_6players.png"), commitShort, buildId);
                File.Copy(Path.Combine(EvidenceDir, "04_reveal_6players.png"), Path.Combine("Evidence/phase2", "step2_characters.png"), true);
                WriteProvenanceJson(Path.Combine("Evidence/phase2", "step2_characters.png"), commitShort, buildId);

                // ==================== 5. THROWING (FLYING SLIPPERS) ====================
                ui.txtPhaseBanner.text = "NÉM DÉP!";
                ui.EnsureAndRefreshBuildFingerprint();
                var slippers = SpawnTestSlippers(players);
                foreach (var kvp in views)
                {
                    kvp.Value.gameObject.SetActive(true);
                    var p = players.Find(x => x.Id == kvp.Key);
                    kvp.Value.Present(p, true, false, false, 1.2f, 1.5f, 0f);
                }
                RenderAndSave(cam, Path.Combine(EvidenceDir, "05_throwing_slippers.png"), commitShort, buildId);
                File.Copy(Path.Combine(EvidenceDir, "05_throwing_slippers.png"), Path.Combine("Evidence/phase2", "step3_sandal_throw.png"), true);
                WriteProvenanceJson(Path.Combine("Evidence/phase2", "step3_sandal_throw.png"), commitShort, buildId);
                ClearTestSlippers(slippers);

                // ==================== 6. RESULT (FULL RANKINGS TABLE) ====================
                ui.ShowScreen(ProductionScreen.Result);
                ui.EnsureAndRefreshBuildFingerprint();
                SetPlayersVisible(views, false);

                var ranked = new List<PlayerState>(players);
                var winners = new List<string> { "player" };
                FillResultTable(ui, ranked, winners);
                RenderAndSave(cam, Path.Combine(EvidenceDir, "06_result_full_rankings.png"), commitShort, buildId);

                // ==================== 7. FULL 7-CHARACTER ROSTER SHOWCASE (F1) ====================
                ui.ShowScreen(ProductionScreen.Gameplay);
                ui.txtRound.text = "ROSTER 7/7";
                ui.txtTimer.text = "F1";
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
                    RenderAndSave(cam, Path.Combine(EvidenceDir, "07_roster_7characters.png"), commitShort, buildId);
                }
                finally
                {
                    foreach (var kvp in all7Views)
                    {
                        if (kvp.Value != null) UnityEngine.Object.DestroyImmediate(kvp.Value.gameObject);
                    }
                }

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

        private static void RenderAndSave(Camera cam, string outputPath, string commitShort, string buildId)
        {
            Canvas.ForceUpdateCanvases();

            var rt = RenderTexture.GetTemporary(ShotWidth, ShotHeight, 24, RenderTextureFormat.ARGB32);
            var prevCamRt = cam.targetTexture;
            var prevRt = RenderTexture.active;

            try
            {
                cam.targetTexture = rt;
                cam.Render();

                RenderTexture.active = rt;
                var tex = new Texture2D(ShotWidth, ShotHeight, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, ShotWidth, ShotHeight), 0, 0);
                tex.Apply();

                byte[] pngBytes = tex.EncodeToPNG();
                File.WriteAllBytes(outputPath, pngBytes);
                UnityEngine.Object.DestroyImmediate(tex);

                WriteProvenanceJson(outputPath, commitShort, buildId);
                Debug.Log($"[EvidenceCapture] Saved: {outputPath} ({ShotWidth}x{ShotHeight}) [commit={commitShort}]");
            }
            finally
            {
                cam.targetTexture = prevCamRt;
                RenderTexture.active = prevRt;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        private static void WriteProvenanceJson(string filePath, string commitShort, string buildId)
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
                          $"  \"resolution\": \"{ShotWidth}x{ShotHeight}\",\n" +
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
                new Vec2(0f, -3.2f),
                new Vec2(-2.4f, -1.2f),
                new Vec2(2.4f, -1.0f),
                new Vec2(-2.2f, 2.4f),
                new Vec2(2.2f, 2.5f),
                new Vec2(0f, 3.8f)
            };
            Vec2[] aims =
            {
                new Vec2(0.6f, 0.8f),
                new Vec2(0.8f, 0.4f),
                new Vec2(-0.7f, 0.5f),
                new Vec2(0.5f, -0.8f),
                new Vec2(-0.6f, -0.7f),
                new Vec2(0f, -1f)
            };
            int[] hps = { 3, 3, 2, 1, 2, 3 };

            var list = new List<PlayerState>(6);
            for (int i = 0; i < 6; i++)
            {
                string id = i == 0 ? "player" : $"bot_{i}";
                string name = PlayerView.CharacterNames[charIndices[i]];
                list.Add(new PlayerState(id, name, i > 0, hps[i], positions[i], aims[i]));
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

                bool isLocal = p.Id == "player";
                if (isPlacing && !isLocal)
                {
                    if (card.txtHearts != null)
                    {
                        card.txtHearts.text = "HP: ???";
                        card.txtHearts.color = new Color(0.6f, 0.65f, 0.7f);
                    }
                    if (card.txtHpNum != null)
                    {
                        card.txtHpNum.text = "ẨN";
                        card.txtHpNum.color = new Color(0.6f, 0.65f, 0.7f);
                    }
                }
                else
                {
                    if (card.txtHearts != null)
                    {
                        card.txtHearts.text = $"HP: {p.Hp}/3";
                        card.txtHearts.color = p.IsAlive
                            ? (p.Hp == 3 ? new Color(0.2f, 1f, 0.4f) : (p.Hp == 2 ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.3f, 0.3f)))
                            : Color.gray;
                    }
                    if (card.txtHpNum != null)
                    {
                        card.txtHpNum.text = p.IsAlive ? "CÒN SỐNG" : "ĐÃ LOẠI";
                        card.txtHpNum.color = p.IsAlive ? new Color(0.8f, 0.9f, 1f) : Color.gray;
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
                var target = players[(i + 1) % players.Count];
                var mid = Vector3.Lerp(new Vector3(p.Position.X, p.Position.Y, 0f), new Vector3(target.Position.X, target.Position.Y, 0f), 0.5f);

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

        private static void FillResultTable(ProductionUIController ui, List<PlayerState> ranked, List<string> winnerIds)
        {
            if (ui.txtResultTitle != null)
            {
                ui.txtResultTitle.text = "CHIẾN THẮNG! 🏆";
                ui.txtResultTitle.color = new Color(1f, 0.9f, 0.2f);
            }
            if (ui.txtResultSubtitle != null)
            {
                ui.txtResultSubtitle.text = "Người thắng: Nam Làng";
            }

            for (int i = 0; i < ui.resultRows.Count && i < ranked.Count; i++)
            {
                var row = ui.resultRows[i];
                var p = ranked[i];
                if (row.root != null) row.root.SetActive(true);
                if (row.txtRank != null) row.txtRank.text = "#" + (i + 1);
                if (row.txtName != null) row.txtName.text = p.Name;
                if (row.txtHp != null) row.txtHp.text = p.Hp + " HP";

                bool won = winnerIds.Contains(p.Id);
                if (row.txtStatus != null)
                {
                    row.txtStatus.text = won ? "★ THẮNG" : (p.IsAlive ? "Còn sống" : "Đã loại");
                    row.txtStatus.color = won ? new Color(1f, 0.9f, 0.2f) : (p.IsAlive ? Color.white : Color.gray);
                }

                if (row.imgBg != null)
                {
                    bool isMe = p.Id == "player";
                    row.imgBg.color = isMe ? new Color(0.2f, 0.7f, 1f, 0.35f) : new Color(0.1f, 0.15f, 0.2f, 0.6f);
                }
            }
        }
    }
}
