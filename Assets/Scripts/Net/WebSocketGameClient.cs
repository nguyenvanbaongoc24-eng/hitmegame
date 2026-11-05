using System;
using UnityEngine;

namespace HitMe.NetClientV1
{
    /// <summary>
    /// Phase 3 (A8): WebSocketGameClient implementing IGameClient over authoritative NET_PROTOCOL.md.
    /// Manages session, tokens, phase transitions, and user-friendly error translations.
    /// </summary>
    public sealed class WebSocketGameClient : IGameClient
    {
        public bool IsConnected => _transport != null && _transport.IsConnected;
        public string RoomCode => _session != null ? _session.RoomCode : string.Empty;
        public string ReconnectToken => _session != null ? _session.ReconnectToken : string.Empty;
        public ClientMatchPhase CurrentPhase => _session != null ? _session.Phase : ClientMatchPhase.Disconnected;

        public event Action<string> OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<RoomStateMessage> OnRoomStateUpdated;
        public event Action<int, long> OnRoundStarted;
        public event Action<RevealMessage> OnRevealReceived;
        public event Action<RoundResultMessage> OnRoundResultReceived;
        public event Action<MatchEndMessage> OnMatchEnded;
        public event Action<string, string> OnEmoteReceived;
        public event Action<string, string> OnErrorReceived;
        public event Action<string> OnQuickMatchWaiting;
        public event Action OnQuickMatchHint;

        private readonly IMatchTransport _transport;
        private readonly ClientMatchSession _session;
        private string _activeGuestToken;
        private bool _isDisposed = false;

        public WebSocketGameClient(IMatchTransport transport = null)
        {
            _transport = transport ?? new WebGLWebSocketTransport(ClientAuthService.GetServerUrl());
            _session = new ClientMatchSession(_transport);

            _session.MessageReceived += HandleServerMessage;
            _session.Closed += HandleSessionClosed;
            if (_transport is WebGLWebSocketTransport webTransport)
                webTransport.Opened += HandleTransportOpened;
        }

        public void Connect(string serverUrl = null)
        {
            if (!string.IsNullOrEmpty(serverUrl))
            {
                ClientAuthService.SetServerUrl(serverUrl);
            }
            _session.Connect();
            if (_transport.IsConnected) HandleTransportOpened();
        }

        private void HandleTransportOpened() => OnConnected?.Invoke(ClientAuthService.GetServerUrl());

        public void Disconnect()
        {
            _session.Send(new LeaveMessage { v = 1, type = "leave" });
            HandleSessionClosed("client_leave");
        }

        public void CreateRoom(string guestToken)
        {
            _activeGuestToken = string.IsNullOrEmpty(guestToken) ? ClientAuthService.GetOrCreateGuestToken() : guestToken;
            Connect();
            _session.Send(new JoinMessage
            {
                v = 1,
                type = "join",
                token = _activeGuestToken,
                roomCode = ""
            });
        }

        public void JoinRoom(string roomCode, string guestToken, string reconnectToken = "")
        {
            _activeGuestToken = string.IsNullOrEmpty(guestToken) ? ClientAuthService.GetOrCreateGuestToken() : guestToken;
            Connect();
            _session.Send(new JoinMessage
            {
                v = 1,
                type = "join",
                token = _activeGuestToken,
                roomCode = roomCode,
                reconnectToken = reconnectToken
            });
        }

        public void QuickMatch(string guestToken)
        {
            _activeGuestToken = string.IsNullOrEmpty(guestToken) ? ClientAuthService.GetOrCreateGuestToken() : guestToken;
            Connect();
            _session.Send(new QuickMatchMessage
            {
                v = 1,
                type = "quick_match",
                token = _activeGuestToken
            });
        }

        public void SetRoomReady(bool ready)
        {
            _session.Send(new RoomReadyMessage
            {
                v = 1,
                type = "room_ready",
                ready = ready
            });
        }

        public void HostStart()
        {
            _session.Send(new StartMessage
            {
                v = 1,
                type = "start"
            });
        }

        public void Commit(int roundId, float x, float y, bool hasAim, float aimX, float aimY)
        {
            _session.Send(new CommitMessage
            {
                v = 1,
                type = "commit",
                roundId = roundId,
                positionX = x,
                positionY = y,
                hasAim = hasAim,
                aimX = aimX,
                aimY = aimY
            });
        }

        public void LockReady(bool ready = true)
        {
            _session.Send(new LockReadyMessage
            {
                v = 1,
                type = "lock_ready",
                ready = ready
            });
        }

        public void SendEmote(string emoteId)
        {
            _session.Send(new EmoteMessage
            {
                v = 1,
                type = "emote",
                emoteId = emoteId
            });
        }

        public void LeaveRoom()
        {
            _session.Send(new LeaveMessage
            {
                v = 1,
                type = "leave"
            });
        }

        public void SendFriendRequest(string targetPlayerId)
        {
            // Future Phase 3 friend endpoint
        }

        private void HandleServerMessage(ServerMessage message)
        {
            switch (message)
            {
                case RoomStateMessage room:
                    OnRoomStateUpdated?.Invoke(room);
                    break;
                case RoundStartMessage start:
                    OnRoundStarted?.Invoke(start.roundId, start.deadline);
                    break;
                case RevealMessage reveal:
                    OnRevealReceived?.Invoke(reveal);
                    break;
                case RoundResultMessage result:
                    OnRoundResultReceived?.Invoke(result);
                    break;
                case MatchEndMessage matchEnd:
                    OnMatchEnded?.Invoke(matchEnd);
                    break;
                case EmoteBroadcastMessage emote:
                    OnEmoteReceived?.Invoke(emote.playerId, emote.emoteId);
                    break;
                case QuickMatchWaitingMessage waiting:
                    OnQuickMatchWaiting?.Invoke($"Đang tìm đối thủ... ({waiting.quickMatchHintSeconds}s)");
                    break;
                case QuickMatchHintMessage:
                    OnQuickMatchHint?.Invoke();
                    break;
                case ErrorMessage error:
                    string viMsg = TranslateNetError(error);
                    OnErrorReceived?.Invoke(error.code, viMsg);
                    break;
            }
        }

        private void HandleSessionClosed(string reason)
        {
            OnDisconnected?.Invoke(reason);
        }

        /// <summary>
        /// Translates error codes from NET_PROTOCOL.md into user-friendly Vietnamese notifications.
        /// </summary>
        public static string TranslateNetError(ErrorMessage error)
        {
            if (error == null) return "Lỗi máy chủ không xác định.";

            return error.code switch
            {
                "not_enough_players" => "Cần ít nhất 2 người chơi để bắt đầu trận!",
                "session_replaced" => "Phiên của bạn đã được đăng nhập từ một thiết bị khác.",
                "room_full" => "Phòng đã đầy người chơi (tối đa 6 người)!",
                "room_not_found" => "Không tìm thấy phòng với mã đã nhập.",
                "room_started" => "Trận đấu trong phòng này đã bắt đầu.",
                "banned" => $"Tài khoản của bạn đang bị cấm đấu do AFK ({error.remainingSeconds}s còn lại).",
                "reconnect_invalid" => "Phiên vào lại đã hết hạn (quá 30 giây).",
                "protocol_mismatch" => "Phiên bản trò chơi không khớp máy chủ. Vui lòng tải lại trang!",
                "auth_required_use_join_token" => "Cần đăng nhập để tham gia phòng trực tuyến.",
                "auth_invalid" => "Mã đăng nhập không hợp lệ.",
                "rate_limited" => "Bạn gửi thao tác quá nhanh, vui lòng chờ trong giây lát.",
                "idle_timeout" => "Đã ngắt kết nối do không có thao tác trong thời gian dài.",
                "server_shutdown" => "Máy chủ đang bảo trì hoặc đã đóng.",
                _ => $"Lỗi từ máy chủ: {error.code}"
            };
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _session.MessageReceived -= HandleServerMessage;
            _session.Closed -= HandleSessionClosed;
            if (_transport is WebGLWebSocketTransport webTransport)
                webTransport.Opened -= HandleTransportOpened;
            _transport.Dispose();
        }
    }
}
