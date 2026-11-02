using System;
using UnityEngine;

namespace HitMe.NetClientV1
{
    /// <summary>
    /// Phase 3 (A8): Guest Token and Server Endpoint Configuration service.
    /// Provides persistent guest credentials across browser/device sessions and server URL resolution.
    /// </summary>
    public static class ClientAuthService
    {
        private const string PrefKeyGuestToken = "HitMe_GuestToken";
        private const string PrefKeyGuestName = "HitMe_GuestName";
        private const string PrefKeyServerUrl = "HitMe_ServerUrl";
        private const string DefaultServerUrl = "ws://127.0.0.1:8080/ws";

        private static string _cachedGuestToken = null;
        private static string _cachedServerUrl = null;

        /// <summary>
        /// Retrieves or creates a persistent Guest Token.
        /// Format per NET_PROTOCOL.md: guest:<name> (e.g., "guest:Nam_4826")
        /// </summary>
        public static string GetOrCreateGuestToken()
        {
            if (!string.IsNullOrEmpty(_cachedGuestToken))
            {
                return _cachedGuestToken;
            }

            try
            {
                string stored = PlayerPrefs.GetString(PrefKeyGuestToken, "");
                if (!string.IsNullOrEmpty(stored))
                {
                    _cachedGuestToken = stored;
                    return _cachedGuestToken;
                }
            }
            catch
            {
                // WebGL sandbox fallback
            }

            string guestName = GetGuestName();
            string newToken = $"guest:{guestName}_{UnityEngine.Random.Range(1000, 9999)}";
            SaveGuestToken(newToken);
            return _cachedGuestToken;
        }

        public static void SaveGuestToken(string token)
        {
            _cachedGuestToken = token;
            try
            {
                PlayerPrefs.SetString(PrefKeyGuestToken, token);
                PlayerPrefs.Save();
            }
            catch
            {
            }
        }

        public static string GetGuestName()
        {
            try
            {
                string name = PlayerPrefs.GetString(PrefKeyGuestName, "");
                if (!string.IsNullOrEmpty(name)) return name;
            }
            catch
            {
            }
            return "Nam";
        }

        public static void SetGuestName(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            try
            {
                PlayerPrefs.SetString(PrefKeyGuestName, name);
                PlayerPrefs.Save();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Resolves the WebSocket server URL.
        /// Priority: (1) URL Query Parameter ?server=ws://..., (2) PlayerPrefs, (3) Default localhost.
        /// </summary>
        public static string GetServerUrl()
        {
            if (!string.IsNullOrEmpty(_cachedServerUrl))
            {
                return _cachedServerUrl;
            }

            // Check URL param if running in browser / WebGL
            if (Application.absoluteURL != null)
            {
                string url = Application.absoluteURL;
                int idx = url.IndexOf("server=", StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    int start = idx + 7;
                    int end = url.IndexOf('&', start);
                    string serverParam = end > start ? url.Substring(start, end - start) : url.Substring(start);
                    if (!string.IsNullOrEmpty(serverParam))
                    {
                        serverParam = Uri.UnescapeDataString(serverParam);
                        _cachedServerUrl = serverParam;
                        return _cachedServerUrl;
                    }
                }
            }

            try
            {
                string stored = PlayerPrefs.GetString(PrefKeyServerUrl, "");
                if (!string.IsNullOrEmpty(stored))
                {
                    _cachedServerUrl = stored;
                    return _cachedServerUrl;
                }
            }
            catch
            {
            }

            _cachedServerUrl = DefaultServerUrl;
            return _cachedServerUrl;
        }

        public static void SetServerUrl(string url)
        {
            _cachedServerUrl = url;
            try
            {
                PlayerPrefs.SetString(PrefKeyServerUrl, url);
                PlayerPrefs.Save();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Online flag check. "Luyện với bot" always uses Mock.
        /// Online rooms require NetConfig.Enabled == true and a valid URL.
        /// </summary>
        public static bool IsOnlineEnabled => NetConfig.Enabled;
    }
}
