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

        [Serializable]
        private class StartPracticeDto
        {
            public string characterId;
            public string weaponId;
            public int botCount = 3;
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
        private class SettingsDto
        {
            public float music = 1f;
            public float sfx = 1f;
            public string language = "vi";
            public bool reducedMotion = false;
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

        private void OnEnable()
        {
            MatchController.OnMatchE2EEvent += HandleMatchE2EEvent;
            if (matchController != null) matchController.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            MatchController.OnMatchE2EEvent -= HandleMatchE2EEvent;
            if (matchController != null) matchController.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void Start()
        {
            EmitReady();
            EmitScreen("home");
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
            int botCount = 3;
            int charIdx = 0;
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
                            charIdx = GetCharacterIndex(dto.characterId);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[WebBridge] StartPractice JSON parse: " + e.Message);
                }
            }

            if (uiController != null)
            {
                uiController.StartPracticeWithBots(botCount, charIdx);
            }
            else if (matchController != null)
            {
                matchController.localCharacterIndex = charIdx;
                matchController.StartMatch();
            }
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
            bool ready = true;
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var dto = JsonUtility.FromJson<ReadyDto>(json);
                    if (dto != null) ready = dto.ready;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[WebBridge] Ready parse: " + e.Message);
                }
            }
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
                    int idx = GetCharacterIndex(dto.id);
                    if (matchController != null) matchController.localCharacterIndex = idx;
                    if (uiController != null) uiController.SetSelectedPracticeCharacter(idx);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[WebBridge] SetCharacter parse: " + e.Message);
            }
        }

        public void SetWeapon(string json)
        {
            // Currently sandal (dep_to_ong) is default canonical weapon
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

        public void Resume(string json = null)
        {
            // Resume / dismiss settings modal
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
            // Display emote above local player
        }

        // ====================================================================
        // UNITY -> WEB (HitMeWebBridge.EmitUnityToWebJson)
        // ====================================================================

        public void EmitReady()
        {
            _sb.Clear();
            _sb.Append("{\"type\":\"ready\",\"build\":\"unity-prod\",\"version\":\"1.2\",");
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
        }

        public void EmitScreen(string screenName)
        {
            string json = "{\"type\":\"screen\",\"name\":\"" + screenName + "\"}";
            HitMeWebBridge.EmitUnityToWebJson(json);
        }

        public void EmitEvent(string kind, string sourceId, string targetId)
        {
            string json = "{\"type\":\"event\",\"kind\":\"" + kind + "\",\"sourceId\":\"" + (sourceId ?? "") + "\",\"targetId\":\"" + (targetId ?? "") + "\"}";
            HitMeWebBridge.EmitUnityToWebJson(json);
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
            _sb.Append("\"phase\":\"").Append(state.Phase.ToString().ToLowerInvariant()).Append("\",");
            _sb.Append("\"timeLeft\":").Append(matchController.SecondsRemaining.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)).Append(",");
            _sb.Append("\"weaponName\":\"Dép Tổ Ong\",");

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
                string charId = "nam_lang";
                if (i < CharacterRegistry.CanonicalCharacters.Length) charId = CharacterRegistry.CanonicalCharacters[i].id;

                float px = 0.5f, py = 0.5f;
                if (cam != null)
                {
                    Vector3 sp = cam.WorldToScreenPoint(new Vector3(p.Position.X, p.Position.Y, 0f));
                    px = Mathf.Clamp01(sp.x / Mathf.Max(1f, Screen.width));
                    py = Mathf.Clamp01(1f - (sp.y / Mathf.Max(1f, Screen.height)));
                }

                _sb.Append("{\"id\":\"").Append(p.Id)
                   .Append("\",\"name\":\"").Append(p.Name)
                   .Append("\",\"characterId\":\"").Append(charId)
                   .Append("\",\"slot\":").Append(i)
                   .Append(",\"hp\":").Append(p.Hp)
                   .Append(",\"maxHp\":").Append(state.Config.StartHp)
                   .Append(",\"alive\":").Append(p.IsAlive ? "true" : "false")
                   .Append(",\"isLocal\":").Append(isLocal ? "true" : "false")
                   .Append(",\"x\":").Append(px.ToString("F4", System.Globalization.CultureInfo.InvariantCulture))
                   .Append(",\"y\":").Append(py.ToString("F4", System.Globalization.CultureInfo.InvariantCulture))
                   .Append("}");
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
