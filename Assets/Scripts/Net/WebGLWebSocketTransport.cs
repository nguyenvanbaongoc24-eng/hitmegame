using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HitMe.NetClientV1
{
    /// <summary>
    /// Phase 3 (A8): WebGL-compatible WebSocket transport bridging to HitMeWebSocket.jslib.
    /// In Editor / Standalone, operates with safe in-process fallback or loopback.
    /// </summary>
    public sealed class WebGLWebSocketTransport : IMatchTransport
    {
        public event Action<ServerMessage> Received;
        public event Action<string> Closed;
        public event Action Opened;
        public bool IsConnected { get; private set; }

        private readonly IProtocolJson _codec;
        private readonly string _endpoint;
        private int _handle = 0;
        private WebGLSocketReceiver _receiver;
        private readonly Queue<string> _pendingPayloads = new Queue<string>();

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int HitMeWsOpen(string url, string receiver);

        [DllImport("__Internal")]
        private static extern int HitMeWsIsOpen(int handle);

        [DllImport("__Internal")]
        private static extern int HitMeWsSend(int handle, string payload);

        [DllImport("__Internal")]
        private static extern void HitMeWsClose(int handle);
#endif

        public WebGLWebSocketTransport(string endpoint, IProtocolJson codec = null)
        {
            _endpoint = endpoint ?? ClientAuthService.GetServerUrl();
            _codec = codec ?? UnityJsonProtocol.Instance;
        }

        public void Connect()
        {
            if (IsConnected) return;
#if UNITY_WEBGL && !UNITY_EDITOR
            if (_handle > 0) return; // A connection is already opening.
#endif

            // Ensure receiver GameObject exists for jslib SendMessage callbacks
            var receiverObj = GameObject.Find("HitMeWsReceiver");
            if (receiverObj == null)
            {
                receiverObj = new GameObject("HitMeWsReceiver");
                UnityEngine.Object.DontDestroyOnLoad(receiverObj);
            }
            _receiver = receiverObj.GetComponent<WebGLSocketReceiver>();
            if (_receiver == null)
            {
                _receiver = receiverObj.AddComponent<WebGLSocketReceiver>();
            }

            _receiver.BindTransport(this);

#if UNITY_WEBGL && !UNITY_EDITOR
            _handle = HitMeWsOpen(_endpoint, "HitMeWsReceiver");
            if (_handle <= 0)
            {
                OnSocketError("connect_failed");
            }
#else
            // In Editor / Standalone, mark connected for loopback or test simulation
            IsConnected = true;
#endif
        }

        public void Send(ClientMessage message)
        {
            if (message == null) return;
            string payload = _codec.ToJson(message);
#if UNITY_WEBGL && !UNITY_EDITOR
            if (_handle <= 0) return;
            if (!IsConnected || HitMeWsIsOpen(_handle) == 0)
            {
                _pendingPayloads.Enqueue(payload);
                return;
            }
            if (HitMeWsSend(_handle, payload) == 0) Closed?.Invoke("send_failed");
#endif
        }

        public void DeliverPayload(string payload)
        {
            if (string.IsNullOrEmpty(payload)) return;
            try
            {
                var msg = ProtocolReader.ReadServerMessage(payload, _codec);
                if (msg != null)
                {
                    Received?.Invoke(msg);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[WebGLWebSocketTransport] Payload parse error: {ex.Message}");
            }
        }

        public void OnSocketOpen()
        {
            IsConnected = true;
#if UNITY_WEBGL && !UNITY_EDITOR
            while (_pendingPayloads.Count > 0)
            {
                if (HitMeWsSend(_handle, _pendingPayloads.Peek()) == 0)
                {
                    Closed?.Invoke("send_failed");
                    return;
                }
                _pendingPayloads.Dequeue();
            }
#endif
            Opened?.Invoke();
        }

        public void OnSocketMessage(string message)
        {
            DeliverPayload(message);
        }

        public void OnSocketError(string error)
        {
            _pendingPayloads.Clear();
            Closed?.Invoke(error);
        }

        public void OnSocketClose(string reason)
        {
            IsConnected = false;
            _handle = 0;
            _pendingPayloads.Clear();
            Closed?.Invoke(reason);
        }

        public void Dispose()
        {
            IsConnected = false;
            _pendingPayloads.Clear();
#if UNITY_WEBGL && !UNITY_EDITOR
            if (_handle > 0)
            {
                HitMeWsClose(_handle);
                _handle = 0;
            }
#endif
            Received = null;
            Closed = null;
            Opened = null;
        }
    }

    /// <summary>
    /// Hidden GameObject MonoBehaviour receiving SendMessage from HitMeWebSocket.jslib.
    /// </summary>
    public sealed class WebGLSocketReceiver : MonoBehaviour
    {
        private WebGLWebSocketTransport _transport;

        public void BindTransport(WebGLWebSocketTransport transport)
        {
            _transport = transport;
        }

        public void OnHitMeWsOpen(string handleStr)
        {
            _transport?.OnSocketOpen();
        }

        public void OnHitMeWsMessage(string data)
        {
            if (string.IsNullOrEmpty(data)) return;
            // format: "handle|payload"
            int sep = data.IndexOf('|');
            string payload = sep >= 0 ? data.Substring(sep + 1) : data;
            _transport?.OnSocketMessage(payload);
        }

        public void OnHitMeWsError(string data)
        {
            int sep = data.IndexOf('|');
            string error = sep >= 0 ? data.Substring(sep + 1) : data;
            _transport?.OnSocketError(error);
        }

        public void OnHitMeWsClose(string data)
        {
            int sep = data.IndexOf('|');
            string reason = sep >= 0 ? data.Substring(sep + 1) : data;
            _transport?.OnSocketClose(reason);
        }
    }
}
