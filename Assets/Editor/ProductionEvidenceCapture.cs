using System;
using System.Collections.Generic;
using System.IO;
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
        private const string EvidenceDir = "Evidence/phase3";
        private const int ShotWidth = 1080;
        private const int ShotHeight = 2340;

        [MenuItem("Tools/HIT ME/Capture 6 Evidence Screenshots (1080x2340)")]
        public static void CaptureAllEvidence()
        {
            Directory.CreateDirectory(EvidenceDir);

            // Rebuild UI layout to ensure fresh styling
            ProductionUISetup.BuildProductionUI();

            // Open Graybox scene
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
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

            var canvasObj = GameObject.Find("ProductionCanvas");
            var canvas = canvasObj.GetComponent<Canvas>();
            var ui = canvasObj.GetComponent<ProductionUIController>();
            var scaler = canvasObj.GetComponent<CanvasScaler>();

            // Setup ScreenSpaceCamera so batchmode Camera.Render() renders Canvas + World
            var originalRenderMode = canvas.renderMode;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1.0f;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ShotWidth, ShotHeight);
            scaler.matchWidthOrHeight = 1.0f;

            // Player views setup
            var players = CreateMockPlayers();
            var views = CreatePlayerViews(players);

            try
            {
                // ==================== 1. HOME SCREEN ====================
                SetPlayersVisible(views, false);
                ui.ShowScreen(ProductionScreen.Home);
                RenderAndSave(cam, Path.Combine(EvidenceDir, "01_home.png"));

                // ==================== 2. LOBBY SCREEN ====================
                ui.ShowScreen(ProductionScreen.Lobby);
                ui.txtLobbyHeader.text = "PHÒNG CHIẾN HỮU • SÂN LÀNG";
                ui.txtLobbyRoomCode.text = "MÃ PHÒNG: HM8888 • SÂN LÀNG";
                ui.txtLobbyCountdown.text = "Đang chờ tất cả người chơi sẵn sàng...";
                SetPlayersVisible(views, false);
                RenderAndSave(cam, Path.Combine(EvidenceDir, "02_lobby.png"));

                // ==================== 3. PLACING (LOCAL ONLY + AIM RULER) ====================
                // UX Rule: Trong pha Placing CHỈ hiện nhân vật người chơi và thước ngắm của họ.
                // Không hiện đối thủ, vị trí, hướng ngắm hoặc dép của đối thủ.
                ui.ShowScreen(ProductionScreen.Gameplay);
                ui.txtRound.text = "VÒNG 1/5";
                ui.txtTimer.text = "4.2s";
                ui.txtTimer.color = Color.white;
                ui.txtPhaseBanner.text = "CHỌN HƯỚNG NÉM • Chạm để đặt, kéo để ngắm";

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
                RenderAndSave(cam, Path.Combine(EvidenceDir, "03_placing_local_only.png"));

                // ==================== 4. REVEAL (ALL 6 PLAYERS) ====================
                // UX Rule: Reveal mới hiện toàn bộ 6 người.
                ui.txtPhaseBanner.text = "LỘ DIỆN ĐỐI THỦ!";
                ui.txtTimer.text = "0.0s";
                UpdateHudHp(ui, players, isPlacing: false);

                foreach (var kvp in views)
                {
                    kvp.Value.gameObject.SetActive(true);
                    var p = players.Find(x => x.Id == kvp.Key);
                    kvp.Value.Present(p, true, true, false, 1.2f, 1.5f, 0f);
                }
                RenderAndSave(cam, Path.Combine(EvidenceDir, "04_reveal_6players.png"));

                // ==================== 5. THROWING (FLYING SLIPPERS) ====================
                // UX Rule: Throwing mới hiện dép bay. Vũ khí dừng ở người đầu tiên trúng.
                ui.txtPhaseBanner.text = "NÉM DÉP!";
                var slippers = SpawnTestSlippers(players);
                foreach (var kvp in views)
                {
                    kvp.Value.gameObject.SetActive(true);
                    var p = players.Find(x => x.Id == kvp.Key);
                    kvp.Value.Present(p, true, false, false, 1.2f, 1.5f, 0f);
                }
                RenderAndSave(cam, Path.Combine(EvidenceDir, "05_throwing_slippers.png"));
                ClearTestSlippers(slippers);

                // ==================== 6. RESULT (FULL RANKINGS TABLE) ====================
                // UX Rule: Result hiển thị toàn bộ bảng xếp hạng.
                ui.ShowScreen(ProductionScreen.Result);
                SetPlayersVisible(views, false);

                var ranked = new List<PlayerState>(players);
                var winners = new List<string> { "player" };
                FillResultTable(ui, ranked, winners);
                RenderAndSave(cam, Path.Combine(EvidenceDir, "06_result_full_rankings.png"));

                Debug.Log("[EvidenceCapture] All 6 screenshots captured successfully to " + EvidenceDir);
            }
            finally
            {
                foreach (var kvp in views)
                {
                    if (kvp.Value != null) UnityEngine.Object.DestroyImmediate(kvp.Value.gameObject);
                }

                canvas.renderMode = originalRenderMode;
                EditorSceneManager.SaveScene(scene);
            }
        }

        private static void RenderAndSave(Camera cam, string outputPath)
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

                File.WriteAllBytes(outputPath, tex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex);
                Debug.Log($"[EvidenceCapture] Saved: {outputPath} ({ShotWidth}x{ShotHeight})");
            }
            finally
            {
                cam.targetTexture = prevCamRt;
                RenderTexture.active = prevRt;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        private static List<PlayerState> CreateMockPlayers()
        {
            return new List<PlayerState>
            {
                new PlayerState("player", "Nam Làng", false, 3, new Vec2(0f, -3.2f), new Vec2(0.6f, 0.8f)),
                new PlayerState("bot_1", "Nữ Năng Động", true, 3, new Vec2(-2.4f, -1.2f), new Vec2(0.8f, 0.4f)),
                new PlayerState("bot_2", "Quậy Boy", true, 2, new Vec2(2.4f, -1.0f), new Vec2(-0.7f, 0.5f)),
                new PlayerState("bot_3", "Cô Ba", true, 1, new Vec2(-2.2f, 2.4f), new Vec2(0.5f, -0.8f)),
                new PlayerState("bot_4", "Anh Teen", true, 2, new Vec2(2.2f, 2.5f), new Vec2(-0.6f, -0.7f)),
                new PlayerState("bot_5", "Chú Chó Vàng", true, 1, new Vec2(0f, 3.8f), new Vec2(0f, -1f))
            };
        }

        private static Dictionary<string, PlayerView> CreatePlayerViews(List<PlayerState> players)
        {
            var dict = new Dictionary<string, PlayerView>();
            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i];
                var go = new GameObject("View_" + p.Id);
                var view = go.AddComponent<PlayerView>();
                view.Initialise(p.Id, Color.white, i % 6);
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
