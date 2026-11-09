using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HitMe.Core;
using HitMe.Data;
using HitMe.Game;

namespace HitMe.UI
{
    /// <summary>
    /// B0: WebBridge component attached to GameObject "WebBridge".
    /// Receives messages from Web DOM overlay via SendMessage("WebBridge", method, json)
    /// Emits events from Unity to Web via window.hitmeUi.onUnity(json)
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class WebBridge : MonoBehaviour
    {
        public static WebBridge Instance { get; private set; }

        public MatchController matchController;
        public ProductionUIController uiController;

        private float _lastMatchEmitTime;
        private const float MatchEmitInterval = 0.10f; // Max 10 Hz (100ms)
        private Camera _mainCamera;
        private readonly StringBuilder _sb = new StringBuilder(2048);

        private int _lastBotCount = 3;
        private bool _startingScenario;
        private int _lastCharIdx = 0;
        private string _currentToast = "";
        private float _toastExpiryTime = 0f;

        [Serializable]
        private class StartPracticeDto
        {
            public string characterId;
            public string weaponId;
            public int botCount = 3;
        }

        [Serializable]
        private class StartScenarioDto
        {
            public string name;
        }

        [Serializable]
        private class QuickPlayDto
        {
            public string characterId;
            public string weaponId;
        }

        [Serializable]
        private class JoinRoomDto
        {
            public string code;
            public string characterId;
            public string weaponId;
        }

        [Serializable]
        private class ReadyDto
        {
            public bool ready;
        }

        [Serializable]
        private class IdDto
        {
            public string id;
        }

        [Serializable]
        private class EmoteDto
        {
            public string kind = "phrase";
            public string id = "phrase_01";
        }

        [Serializable]
        private class SettingsDto
        {
            public float music = 1f;
            public float sfx = 1f;
            public float musicVolume = 1f;
            public float sfxVolume = 1f;
            public string language = "vi";
            public bool reducedMotion = false;
        }

        [Serializable]
        private class LayoutDto
        {
            public float top;
            public float bottom;
            public float safeTop;
            public float safeBottom;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (matchController == null) matchController = FindAnyObjectByType<MatchController>();
            if (uiController == null) uiController = FindAnyObjectByType<ProductionUIController>();
            _mainCamera = Camera.main;
        }

        private void EnsureMatchSubscribed()
        {
            if (matchController == null) matchController = FindAnyObjectByType<MatchController>();
            if (uiController == null) uiController = FindAnyObjectByType<ProductionUIController>();
            if (matchController != null)
            {
                matchController.OnPhaseChanged -= HandlePhaseChanged;
                matchController.OnPhaseChanged += HandlePhaseChanged;
                matchController.OnMatchEnded -= HandleMatchEnded;
                matchController.OnMatchEnded += HandleMatchEnded;
                matchController.OnToastRequested -= HandleToastRequested;
                matchController.OnToastRequested += HandleToastRequested;
            }
        }

        private void OnEnable()
        {
            MatchController.OnMatchE2EEvent += HandleMatchE2EEvent;
            EnsureMatchSubscribed();
            HitMe.Audio.AudioManager.OnSfxRequested += HandleSfxRequested;
            HitMe.Audio.AudioManager.OnMusicRequested += HandleMusicRequested;
            MatchController.OnEmoteTriggered += EmitEmote;
        }

        private void OnDisable()
        {
            MatchController.OnMatchE2EEvent -= HandleMatchE2EEvent;
            if (matchController != null)
            {
                matchController.OnPhaseChanged -= HandlePhaseChanged;
                matchController.OnMatchEnded -= HandleMatchEnded;
                matchController.OnToastRequested -= HandleToastRequested;
            }
            HitMe.Audio.AudioManager.OnSfxRequested -= HandleSfxRequested;
            HitMe.Audio.AudioManager.OnMusicRequested -= HandleMusicRequested;
            MatchController.OnEmoteTriggered -= EmitEmote;
        }

        private void Start()
        {
            EnsureMatchSubscribed();
            if (HitMeWebBridge.IsWebUiMode() == 1)
            {
                if (uiController != null) uiController.HideAllUGUIForWebMode();
            }

            string currentUrl = Application.absoluteURL;
            if (!string.IsNullOrEmpty(currentUrl) && currentUrl.IndexOf("audio=unity", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                PlayerPrefs.SetInt("HitMe_AudioUnity", 1);
            }
            else
            {
                PlayerPrefs.SetInt("HitMe_AudioUnity", 0);
            }

            EmitReady();
            EmitScreen("home");

            CheckAutostart();
        }

        private void CheckAutostart()
        {
            string url = Application.absoluteURL;
            if (string.IsNullOrEmpty(url)) return;

            if (url.IndexOf("autostart=practice", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string scenario = GetUrlParam(url, "scenario");
                if (IsScenarioEnabled() && DevScenario.IsKnown(scenario))
                {
                    StartScenario(JsonUtility.ToJson(new StartScenarioDto { name = scenario }));
                    return;
                }
                string charId = GetUrlParam(url, "char");
                string weaponId = GetUrlParam(url, "weapon");
                int bots = 3;
                string botsStr = GetUrlParam(url, "bots");
                if (!string.IsNullOrEmpty(botsStr) && int.TryParse(botsStr, out int bVal)) bots = bVal;

                var startDto = new StartPracticeDto
                {
                    characterId = string.IsNullOrEmpty(charId) ? "nam_lang" : charId,
                    weaponId = string.IsNullOrEmpty(weaponId) ? "dep_to_ong" : weaponId,
                    botCount = bots
                };
                StartPractice(JsonUtility.ToJson(startDto));
            }
        }

        private static string GetUrlParam(string url, string paramName)
        {
            if (string.IsNullOrEmpty(url)) return "";
            int qIdx = url.IndexOf('?');
            if (qIdx < 0) return "";
            string query = url.Substring(qIdx + 1);
            string[] pairs = query.Split('&');
            foreach (var p in pairs)
            {
                string[] kv = p.Split('=');
                if (kv.Length >= 2 && string.Equals(kv[0], paramName, StringComparison.OrdinalIgnoreCase))
                {
                    return Uri.UnescapeDataString(kv[1]);
                }
            }
            return "";
        }

        private static bool IsScenarioEnabled()
        {
            string url = Application.absoluteURL;
            return GetUrlParam(url, "dev") == "1" || GetUrlParam(url, "e2e") == "1";
        }

        public void StartScenario(string json)
        {
            if (!IsScenarioEnabled()) return;
            StartScenarioDto dto;
            try { dto = JsonUtility.FromJson<StartScenarioDto>(json); }
            catch { EmitError("scenario_invalid", "Kịch bản thử không hợp lệ"); return; }
            if (dto == null || !DevScenario.IsKnown(dto.name))
            { EmitError("scenario_invalid", "Kịch bản thử không hợp lệ"); return; }
            if (matchController == null)
            { EmitError("scenario_unavailable", "Trận đấu chưa sẵn sàng"); return; }
            matchController.ConfigureScenario(dto.name);
            int bots = dto.name == "ranking_ties" ? 5
                : dto.name == "tiebreak3" || dto.name == "tiebreak_draw" || dto.name == "hardcap" ? 2 : 1;
            _startingScenario = true;
            try
            {
                StartPractice(JsonUtility.ToJson(new StartPracticeDto
                { characterId = "nam_lang", weaponId = "dep_to_ong", botCount = bots }));
            }
            finally { _startingScenario = false; }
        }

        private void HandleMatchEnded(List<PlayerState> ranked, List<string> winners)
        {
            var state = matchController != null ? matchController.State : null;
            int currentRound = state != null ? state.RoundNumber : 1;
            var finalRanks = state != null ? state.GetRankings() : Array.Empty<RankedPlayer>();
            _sb.Clear();
            _sb.Append("{\"type\":\"result\",\"round\":").Append(currentRound)
               .Append(",\"roundLabel\":\"").Append(EscapeJson(state != null ? state.RoundLabel : "VÒNG 1"))
               .Append("\",\"tiebreakCount\":").Append(state != null ? state.TiebreakCount : 0)
               .Append(",\"winnerIds\":[");
            if (winners != null)
            {
                for (int i = 0; i < winners.Count; i++)
                {
                    if (i > 0) _sb.Append(",");
                    _sb.Append('"').Append(EscapeJson(winners[i])).Append('"');
                }
            }
            _sb.Append("],\"rankings\":[");
            for (int i = 0; i < finalRanks.Length; i++)
            {
                    if (i > 0) _sb.Append(",");
                    var row = finalRanks[i];
                    var p = row.Player;
                    bool survived = p.IsAlive;
                    string charId = matchController != null ? matchController.GetPlayerCharacterId(p.Id) : "nam_lang";
                    int roundSurvived = survived ? currentRound : (p.EliminatedAtRound > 0 ? p.EliminatedAtRound : currentRound);
                    _sb.Append("{\"id\":\"").Append(EscapeJson(p.Id))
                       .Append("\",\"name\":\"").Append(EscapeJson(p.Name))
                       .Append("\",\"characterId\":\"").Append(charId)
                       .Append("\",\"rank\":").Append(row.Rank)
                       .Append(",\"hits\":").Append(p.HitsLanded)
                       .Append(",\"throws\":").Append(p.Throws)
                       .Append(",\"accuracy\":").Append(row.Accuracy.ToString("F4", System.Globalization.CultureInfo.InvariantCulture))
                       .Append(",\"eliminatedRound\":").Append(p.EliminatedAtRound)
                       .Append(",\"roundSurvived\":").Append(roundSurvived)
                       .Append(",\"survived\":").Append(survived ? "true" : "false")
                       .Append("}");
            }
            int localHits = 0;
            if (ranked != null)
            {
                for (int h = 0; h < ranked.Count; h++)
                {
                    if (ranked[h].Id == "player") { localHits = ranked[h].HitsLanded; break; }
                }
            }
            _sb.Append("],\"hits\":").Append(localHits).Append("}");
            HitMeWebBridge.EmitUnityToWebJson(_sb.ToString());
            EmitScreen("result");
        }

        public void EmitError(string code, string message)
        {
            string json = "{\"type\":\"error\",\"code\":\"" + code + "\",\"message\":\"" + EscapeJson(message) + "\"}";
            HitMeWebBridge.EmitUnityToWebJson(json);
        }

        private void Update()
        {
            if (Time.unscaledTime - _lastMatchEmitTime >= MatchEmitInterval)
            {
                _lastMatchEmitTime = Time.unscaledTime;
                if (matchController != null && matchController.State != null)
                {
                    EmitMatchState();
                }
            }
        }

        // ====================================================================
        // WEB -> UNITY (SendMessage targets)
        // ====================================================================

        public void StartPractice(string json)
        {
            EnsureMatchSubscribed();
            if (!_startingScenario && matchController != null) matchController.ConfigureScenario(null);
            int botCount = 3;
            int charIdx = _lastCharIdx >= 0 ? _lastCharIdx : 0;
            string weaponId = "dep_to_ong";
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var dto = JsonUtility.FromJson<StartPracticeDto>(json);
                    if (dto != null)
                    {
                        if (dto.botCount > 0) botCount = dto.botCount;
                        if (!string.IsNullOrEmpty(dto.characterId))
                        {
                            var charDef = CharacterRegistry.GetById(dto.characterId);
                            if (!charDef.enabled)
                            {
                                EmitError("character_locked", "Nhân vật sắp ra mắt");
                                return;
                            }
                            charIdx = GetCharacterIndex(dto.characterId);
                        }
                        if (!string.IsNullOrEmpty(dto.weaponId))
                        {
                            weaponId = dto.weaponId;
                            if (weaponId != "dep" && weaponId != "dep_to_ong")
                            {
                                EmitError("weapon_locked", "Vũ khí chưa sẵn sàng trong trận này.");
                                return;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[WebBridge] StartPractice JSON parse: " + e.Message);
                }
            }

            _lastBotCount = botCount;
            _lastCharIdx = charIdx;
            if (matchController != null)
            {
                matchController.localCharacterIndex = charIdx;
            }

            if (uiController != null)
            {
                uiController.StartPracticeWithBots(botCount, charIdx);
            }
            else if (matchController != null)
            {
                matchController.StartMatch();
            }
            string chosenCharId = CharacterRegistry.GetByIndex(charIdx).id;
            HitMeWebBridge.EmitUnityToWebJson("{\"type\":\"selection\",\"characterId\":\"" + chosenCharId + "\"}");
            EmitMatchState();
            EmitScreen("match");
        }

        public void QuickPlay(string json)
        {
            int charIdx = 0;
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var dto = JsonUtility.FromJson<QuickPlayDto>(json);
                    if (dto != null && !string.IsNullOrEmpty(dto.characterId))
                    {
                        charIdx = GetCharacterIndex(dto.characterId);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[WebBridge] QuickPlay parse: " + e.Message);
                }
            }

            if (matchController != null) matchController.localCharacterIndex = charIdx;
            if (uiController != null)
            {
                uiController.ShowScreen(ProductionScreen.Lobby);
            }
            EmitScreen("lobby");
        }

        public void CreateRoom(string json)
        {
            if (uiController != null) uiController.ShowScreen(ProductionScreen.Lobby);
            EmitScreen("lobby");
        }

        public void JoinRoom(string json)
        {
            if (uiController != null) uiController.ShowScreen(ProductionScreen.Lobby);
            EmitScreen("lobby");
        }

        public void LeaveRoom(string json = null)
        {
            if (uiController != null) uiController.ShowScreen(ProductionScreen.Home);
            EmitScreen("home");
        }

        public void Ready(string json)
        {
            if (matchController != null)
            {
                matchController.RequestReady();
            }
        }

        public void SetCharacter(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var dto = JsonUtility.FromJson<IdDto>(json);
                if (dto != null && !string.IsNullOrEmpty(dto.id))
                {
                    var charDef = CharacterRegistry.GetById(dto.id);
                    if (!charDef.enabled)
                    {
                        EmitError("character_locked", "Nhân vật sắp ra mắt");
                        return;
                    }
                    int idx = GetCharacterIndex(dto.id);
                    _lastCharIdx = idx;
                    if (matchController != null) matchController.localCharacterIndex = idx;
                    if (uiController != null) uiController.SetSelectedPracticeCharacter(idx);

                    string selectionJson = "{\"type\":\"selection\",\"characterId\":\"" + charDef.id + "\"}";
                    HitMeWebBridge.EmitUnityToWebJson(selectionJson);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[WebBridge] SetCharacter parse: " + e.Message);
            }
        }

        public void SetWeapon(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var dto = JsonUtility.FromJson<IdDto>(json);
                if (dto != null && !string.IsNullOrEmpty(dto.id))
                {
                    if (dto.id != "dep" && dto.id != "dep_to_ong")
                    {
                        EmitError("weapon_locked", "Vũ khí chưa sẵn sàng trong trận này.");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[WebBridge] SetWeapon parse: " + e.Message);
            }
        }

        public void SetLayout(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var dto = JsonUtility.FromJson<LayoutDto>(json);
                if (dto != null)
                {
                    ApplyLayout(Mathf.Max(dto.top, dto.safeTop), Mathf.Max(dto.bottom, dto.safeBottom));
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[WebBridge] SetLayout parse error: " + e.Message);
            }
        }

        public void ApplyLayout(float topCss, float bottomCss)
        {
            var cam = _mainCamera != null ? _mainCamera : Camera.main;
            if (cam == null) return;

            float aspect = cam.aspect > 0.01f ? cam.aspect : (393f / 695f);
            float zoomFactor = UiTheme.Instance != null ? UiTheme.Instance.arenaViewZoomFactor : 0.88f;
            float targetOrtho = (3.4133f * zoomFactor) / Mathf.Max(0.01f, aspect);
            cam.orthographicSize = targetOrtho;

            float worldOffsetY = ((bottomCss - topCss) * 0.5f / 695f) * (2f * targetOrtho);
            var cam25D = cam.GetComponent<HitMe.Gameplay.CameraController25D>();
            if (cam25D != null)
            {
                cam25D.arenaCenter = new Vector3(0f, worldOffsetY, 0f);
            }
            else
            {
                cam.transform.position = new Vector3(0f, worldOffsetY, -10f);
            }

            EmitLayoutEvent(cam);
        }

        public void EmitLayoutEvent(Camera cam)
        {
            if (cam == null) return;
            Vector3 minScreen = cam.WorldToScreenPoint(new Vector3(-2.5f, -3.8f, 0f));
            Vector3 maxScreen = cam.WorldToScreenPoint(new Vector3(2.5f, 3.8f, 0f));

            float cssScaleX = 393f / Mathf.Max(1f, Screen.width);
            float cssScaleY = 695f / Mathf.Max(1f, Screen.height);
            float x_css = Mathf.Min(minScreen.x, maxScreen.x) * cssScaleX;
            float y_css = (Screen.height - Mathf.Max(minScreen.y, maxScreen.y)) * cssScaleY;
            float w_css = Mathf.Abs(maxScreen.x - minScreen.x) * cssScaleX;
            float h_css = Mathf.Abs(maxScreen.y - minScreen.y) * cssScaleY;

            string layoutJson = "{\"type\":\"layout\",\"playableRect\":{\"x\":" 
                + x_css.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + ",\"y\":"
                + y_css.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + ",\"w\":"
                + w_css.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + ",\"h\":"
                + h_css.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + "}}";
            HitMeWebBridge.EmitUnityToWebJson(layoutJson);
        }

        public void SetSettings(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var dto = JsonUtility.FromJson<SettingsDto>(json);
                if (dto != null)
                {
                    PlayerPrefs.SetString("HitMe_ReducedMotion", dto.reducedMotion ? "1" : "0");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[WebBridge] SetSettings parse: " + e.Message);
            }
        }

        public void Pause(string json = null)
        {
            if (matchController != null)
            {
                matchController.SetPaused(true);
            }
        }

        public void Resume(string json = null)
        {
            if (matchController != null)
            {
                matchController.SetPaused(false);
            }
        }

        public void Rematch(string json = null)
        {
            EnsureMatchSubscribed();
            int botCount = _lastBotCount > 0 ? _lastBotCount : 3;
            int charIdx = _lastCharIdx >= 0 ? _lastCharIdx : 0;
            if (matchController != null)
            {
                matchController.StopAndCleanupMatch();
                matchController.localCharacterIndex = charIdx;
            }
            if (uiController != null)
            {
                uiController.StartPracticeWithBots(botCount, charIdx);
            }
            else if (matchController != null)
            {
                matchController.StartMatch();
            }
            EmitMatchState();
            EmitScreen("match");
        }

        public void ExitMatch(string json = null)
        {
            if (matchController != null)
            {
                matchController.StopAndCleanupMatch();
            }
            if (uiController != null)
            {
                uiController.ShowScreen(ProductionScreen.Home);
            }
            EmitScreen("home");
        }

        public void SendEmote(string json)
        {
            string kind = "phrase";
            string id = "phrase_01";
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var dto = JsonUtility.FromJson<EmoteDto>(json);
                    if (dto != null)
                    {
                        if (!string.IsNullOrEmpty(dto.kind)) kind = dto.kind;
                        if (!string.IsNullOrEmpty(dto.id)) id = dto.id;
                    }
                }
                catch
                {
                    if (json.StartsWith("phrase_") || json.StartsWith("Chào") || json.StartsWith("Né")) { kind = "phrase"; id = json; }
                    else if (json.StartsWith("sticker_")) { kind = "sticker"; id = json; }
                }
            }

            if (matchController != null)
            {
                matchController.HandlePlayerEmote(kind, id, "player");
            }
            EmitEmote(kind, id, "player");
        }

        public void EmitEmote(string kind, string id, string playerId)
        {
            float screenX = 0.5f, screenY = 0.5f;
            bool includeScreen = (playerId == "player") || (matchController != null && matchController.State != null && matchController.State.Phase != Phase.Placing);
            var cam = _mainCamera != null ? _mainCamera : Camera.main;
            if (includeScreen && cam != null && matchController != null && matchController.State != null)
            {
                var p = matchController.State.FindPlayer(playerId);
                if (p != null)
                {
                    Vector3 sp = cam.WorldToScreenPoint(new Vector3(p.Position.X, p.Position.Y + 1.45f, 0f));
                    screenX = Mathf.Clamp01(sp.x / Mathf.Max(1f, Screen.width));
                    screenY = Mathf.Clamp01(1f - (sp.y / Mathf.Max(1f, Screen.height)));
                }
            }

            _sb.Clear();
            _sb.Append("{\"type\":\"emote\",\"kind\":\"").Append(kind).Append("\",\"id\":\"").Append(id).Append("\",\"playerId\":\"").Append(playerId).Append("\"");
            if (includeScreen)
            {
                _sb.Append(",\"screen\":{\"x\":").Append(screenX.ToString("F4", System.Globalization.CultureInfo.InvariantCulture))
                   .Append(",\"y\":").Append(screenY.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)).Append("}");
            }
            _sb.Append("}");
            HitMeWebBridge.EmitUnityToWebJson(_sb.ToString());
        }

        private void HandleSfxRequested(string sfxId, float vol, float pan)
        {
            string json = "{\"type\":\"sfx\",\"id\":\"" + sfxId + "\",\"vol\":" + vol.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "}";
            HitMeWebBridge.EmitUnityToWebJson(json);
        }

        private void HandleMusicRequested(string musicId, string action)
        {
            string json = "{\"type\":\"music\",\"id\":\"" + musicId + "\",\"action\":\"" + action + "\"}";
            HitMeWebBridge.EmitUnityToWebJson(json);
        }

        // ====================================================================
        // UNITY -> WEB (HitMeWebBridge.EmitUnityToWebJson)
        // ====================================================================

        private void HandleToastRequested(string msg)
        {
            _currentToast = msg ?? "";
            _toastExpiryTime = Time.unscaledTime + 2.5f;
        }

        private static string EscapeJson(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        public void EmitReady()
        {
            _sb.Clear();
            _sb.Append("{\"type\":\"ready\",\"build\":\"unity-prod\",\"version\":\"1.2\",\"catalogVersion\":1,");
            _sb.Append("\"roster\":[");
            var roster = CharacterRegistry.CanonicalCharacters;
            for (int i = 0; i < roster.Length; i++)
            {
                if (i > 0) _sb.Append(",");
                _sb.Append("{\"id\":\"").Append(roster[i].id)
                   .Append("\",\"name\":\"").Append(roster[i].displayNameVi)
                   .Append("\",\"enabled\":").Append(roster[i].enabled ? "true" : "false")
                   .Append("}");
            }
            _sb.Append("],\"weapons\":[");
            _sb.Append("{\"id\":\"dep_to_ong\",\"name\":\"Dép Tổ Ong\",\"enabled\":true},");
            _sb.Append("{\"id\":\"chao_chong_dinh\",\"name\":\"Chảo Chống Dính\",\"enabled\":false},");
            _sb.Append("{\"id\":\"vot_pickleball\",\"name\":\"Vợt Pickleball\",\"enabled\":false}");
            _sb.Append("]}");

            HitMeWebBridge.EmitUnityToWebJson(_sb.ToString());

            // Emit initial profile balance for Home gold chip
            HitMeWebBridge.EmitUnityToWebJson("{\"type\":\"profile\",\"balance\":500}");
        }

        public void EmitScreen(string screenName)
        {
            string json = "{\"type\":\"screen\",\"name\":\"" + screenName + "\"}";
            HitMeWebBridge.EmitUnityToWebJson(json);
        }

        public void EmitEvent(string kind, string sourceId, string targetId)
        {
            var cam = _mainCamera != null ? _mainCamera : Camera.main;
            float screenX = 0.5f, screenY = 0.5f;
            bool hasScreen = false;
            string target = !string.IsNullOrEmpty(targetId) ? targetId : sourceId;
            if (matchController != null && matchController.State != null && matchController.State.Phase != Phase.Placing && cam != null && !string.IsNullOrEmpty(target))
            {
                var p = matchController.State.FindPlayer(target);
                if (p != null)
                {
                    Vector3 sp = cam.WorldToScreenPoint(new Vector3(p.Position.X, p.Position.Y + 0.8f, 0f));
                    screenX = Mathf.Clamp01(sp.x / Mathf.Max(1f, Screen.width));
                    screenY = Mathf.Clamp01(1f - (sp.y / Mathf.Max(1f, Screen.height)));
                    hasScreen = true;
                }
            }

            _sb.Clear();
            _sb.Append("{\"type\":\"event\",\"kind\":\"").Append(kind ?? "")
               .Append("\",\"sourceId\":\"").Append(sourceId ?? "")
               .Append("\",\"targetId\":\"").Append(targetId ?? "")
               .Append("\"");
            if (hasScreen)
            {
                _sb.Append(",\"screen\":{\"x\":").Append(screenX.ToString("F4", System.Globalization.CultureInfo.InvariantCulture))
                   .Append(",\"y\":").Append(screenY.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)).Append("}");
            }
            _sb.Append("}");
            HitMeWebBridge.EmitUnityToWebJson(_sb.ToString());
        }

        private void HandleMatchE2EEvent(string kind, string sourceId, string targetId)
        {
            EmitEvent(kind, sourceId, targetId);
        }

        private void HandlePhaseChanged(Phase phase)
        {
            EmitMatchState();
        }

        private void EmitMatchState()
        {
            if (matchController == null || matchController.State == null) return;
            var state = matchController.State;
            var cam = _mainCamera != null ? _mainCamera : Camera.main;

            _sb.Clear();
            _sb.Append("{\"type\":\"match\",");
            _sb.Append("\"round\":").Append(state.RoundNumber).Append(",");
            _sb.Append("\"maxRounds\":").Append(state.Config.MaxRounds).Append(",");
            _sb.Append("\"roundLabel\":\"").Append(EscapeJson(state.RoundLabel)).Append("\",");
            _sb.Append("\"tiebreak\":").Append(state.IsTiebreak ? "true" : "false").Append(",");
            _sb.Append("\"tiebreakCount\":").Append(state.TiebreakCount).Append(",");
            int aliveCount = 0;
            for (int i = 0; i < state.Players.Count; i++) if (state.Players[i].IsAlive) aliveCount++;
            _sb.Append("\"aliveCount\":").Append(aliveCount).Append(",");
            _sb.Append("\"phase\":\"").Append(state.Phase.ToString().ToLowerInvariant()).Append("\",");
            _sb.Append("\"timeLeft\":").Append(matchController.SecondsRemaining.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)).Append(",");
            _sb.Append("\"weaponName\":\"Dép Tổ Ong\",");
            string currentToast = (Time.unscaledTime < _toastExpiryTime) ? _currentToast : "";
            _sb.Append("\"toast\":\"").Append(EscapeJson(currentToast)).Append("\",");

            // PlayableRect in screen / CSS normalized coordinates
            float leftNorm = 0.08f, rightNorm = 0.92f, topNorm = 0.22f, bottomNorm = 0.88f;
            if (cam != null)
            {
                Vector3 minScreen = cam.WorldToScreenPoint(new Vector3(ArenaCoordinateMapper.PlayMinX, ArenaCoordinateMapper.PlayMinY, 0f));
                Vector3 maxScreen = cam.WorldToScreenPoint(new Vector3(ArenaCoordinateMapper.PlayMaxX, ArenaCoordinateMapper.PlayMaxY, 0f));
                float sw = Mathf.Max(1f, Screen.width);
                float sh = Mathf.Max(1f, Screen.height);
                leftNorm = Mathf.Clamp01(Mathf.Min(minScreen.x, maxScreen.x) / sw);
                rightNorm = Mathf.Clamp01(Mathf.Max(minScreen.x, maxScreen.x) / sw);
                bottomNorm = Mathf.Clamp01(1f - (Mathf.Min(minScreen.y, maxScreen.y) / sh));
                topNorm = Mathf.Clamp01(1f - (Mathf.Max(minScreen.y, maxScreen.y) / sh));
            }
            _sb.Append("\"playableRect\":{\"left\":").Append(leftNorm.ToString("F4", System.Globalization.CultureInfo.InvariantCulture))
               .Append(",\"top\":").Append(topNorm.ToString("F4", System.Globalization.CultureInfo.InvariantCulture))
               .Append(",\"right\":").Append(rightNorm.ToString("F4", System.Globalization.CultureInfo.InvariantCulture))
               .Append(",\"bottom\":").Append(bottomNorm.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)).Append("},");

            _sb.Append("\"players\":[");
            for (int i = 0; i < state.Players.Count; i++)
            {
                if (i > 0) _sb.Append(",");
                var p = state.Players[i];
                bool isLocal = p.Id == "player";
                string charId = matchController.GetPlayerCharacterId(p.Id);
                bool canRevealPos = isLocal || state.Phase != Phase.Placing;

                _sb.Append("{\"id\":\"").Append(p.Id)
                   .Append("\",\"name\":\"").Append(EscapeJson(p.Name))
                   .Append("\",\"characterId\":\"").Append(charId)
                   .Append("\",\"slot\":").Append(i)
                   .Append(",\"hp\":").Append(p.Hp)
                   .Append(",\"maxHp\":").Append(state.Config.StartHp)
                   .Append(",\"alive\":").Append(p.IsAlive ? "true" : "false")
                   .Append(",\"isLocal\":").Append(isLocal ? "true" : "false");

                if (canRevealPos && cam != null)
                {
                    Vector3 sp = cam.WorldToScreenPoint(new Vector3(p.Position.X, p.Position.Y, 0f));
                    float px = Mathf.Clamp01(sp.x / Mathf.Max(1f, Screen.width));
                    float py = Mathf.Clamp01(1f - (sp.y / Mathf.Max(1f, Screen.height)));
                    _sb.Append(",\"x\":").Append(px.ToString("F4", System.Globalization.CultureInfo.InvariantCulture))
                       .Append(",\"y\":").Append(py.ToString("F4", System.Globalization.CultureInfo.InvariantCulture));
                }
                _sb.Append("}");
            }
            _sb.Append("]}");

            HitMeWebBridge.EmitUnityToWebJson(_sb.ToString());
        }

        private static int GetCharacterIndex(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return 0;
            if (int.TryParse(characterId, out int numericIdx)) return Mathf.Clamp(numericIdx, 0, CharacterRegistry.CanonicalCharacters.Length - 1);

            for (int i = 0; i < CharacterRegistry.CanonicalCharacters.Length; i++)
            {
                if (string.Equals(CharacterRegistry.CanonicalCharacters[i].id, characterId, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
