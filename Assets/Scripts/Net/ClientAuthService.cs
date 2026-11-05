using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

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
#if UNITY_WEBGL && !UNITY_EDITOR
        private const string DefaultServerUrl = "wss://hitmegame.onrender.com/ws";
#else
        private const string DefaultServerUrl = "ws://127.0.0.1:8080/ws";
#endif

        private static string _cachedGuestToken = null;
        private static string _cachedServerUrl = null;

        [Serializable]
        private sealed class GuestTokenResponse { public string token; }

        [Serializable]
        private sealed class GuestTokenRequest { public string name; }

        // Online rooms require a server-issued token. Never submit a locally
        // constructed guest:<name> token to a production server.
        public static IEnumerator GetVerifiedGuestToken(Action<string> onSuccess, Action<string> onError)
        {
            string existing = GetOrCreateGuestToken();
            if (existing.StartsWith("guest1.", StringComparison.Ordinal))
            {
                onSuccess?.Invoke(existing);
                yield break;
            }

            string endpoint = GetServerUrl();
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out Uri wsUri) ||
                (wsUri.Scheme != "ws" && wsUri.Scheme != "wss"))
            {
                onError?.Invoke("Địa chỉ máy chủ không hợp lệ.");
                yield break;
            }
            var httpUri = new UriBuilder(wsUri)
            {
                Scheme = wsUri.Scheme == "wss" ? "https" : "http",
                Path = "/guest-token",
                Query = ""
            }.Uri;
            byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new GuestTokenRequest { name = GetGuestName() }));
            using (var request = new UnityWebRequest(httpUri.ToString(), "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                // Render's free instance can take roughly a minute to wake.
                // Keep the real request pending instead of reporting a false failure.
                request.timeout = 75;
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke("Không lấy được phiên khách từ máy chủ. Vui lòng thử lại.");
                    yield break;
                }
                var response = JsonUtility.FromJson<GuestTokenResponse>(request.downloadHandler.text);
                if (response == null || string.IsNullOrEmpty(response.token) ||
                    !response.token.StartsWith("guest1.", StringComparison.Ordinal))
                {
                    onError?.Invoke("Máy chủ trả phiên khách không hợp lệ.");
                    yield break;
                }
                SaveGuestToken(response.token);
                onSuccess?.Invoke(response.token);
            }
        }

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
