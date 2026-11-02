using System;
using System.Collections.Generic;
using UnityEngine;

namespace HitMe.NetClientV1
{
    /// <summary>
    /// Phase 3 (A8): MockGameClient providing an in-process, zero-dependency client simulation.
    /// Used for "Luyện với bot" and offline testing without requiring a live server.
    /// </summary>
    public sealed class MockGameClient : IGameClient
    {
        public bool IsConnected { get; private set; } = true;
        public string RoomCode { get; private set; } = "HM0001";
        public string ReconnectToken { get; private set; } = "mock_reconnect_token";
        public ClientMatchPhase CurrentPhase { get; private set; } = ClientMatchPhase.Lobby;

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

        private int _currentRound = 0;
        private readonly List<RoomPlayerDto> _mockPlayers = new List<RoomPlayerDto>();
        private bool _isDisposed = false;

        public MockGameClient()
        {
            ResetMockLobby(2);
        }

        private void ResetMockLobby(int playerCount)
        {
            _mockPlayers.Clear();
            _mockPlayers.Add(new RoomPlayerDto { id = "player", name = "Nam Làng (Bạn)", ready = false });
            for (int i = 1; i < playerCount; i++)
            {
                _mockPlayers.Add(new RoomPlayerDto { id = $"bot_{i}", name = $"Bot #{i}", ready = true });
            }
        }

        public void Connect(string serverUrl = null)
        {
            IsConnected = true;
            CurrentPhase = ClientMatchPhase.Lobby;
            OnConnected?.Invoke(serverUrl ?? "mock://in-process");
        }

        public void Disconnect()
        {
            IsConnected = false;
            CurrentPhase = ClientMatchPhase.Disconnected;
            OnDisconnected?.Invoke("client_leave");
        }

        public void CreateRoom(string guestToken)
        {
            RoomCode = "HM" + UnityEngine.Random.Range(1000, 9999);
            ResetMockLobby(1);
            CurrentPhase = ClientMatchPhase.Lobby;
            BroadcastMockRoomState();
        }

        public void JoinRoom(string roomCode, string guestToken, string reconnectToken = "")
        {
            RoomCode = string.IsNullOrEmpty(roomCode) ? "HM4826" : roomCode;
            ResetMockLobby(3);
            CurrentPhase = ClientMatchPhase.Lobby;
            BroadcastMockRoomState();
        }

        public void QuickMatch(string guestToken)
        {
            CurrentPhase = ClientMatchPhase.Queueing;
            OnQuickMatchWaiting?.Invoke("Đang tìm đối thủ...");
            // Simulate quick match found
            RoomCode = "HM" + UnityEngine.Random.Range(1000, 9999);
            ResetMockLobby(2);
            CurrentPhase = ClientMatchPhase.Lobby;
            BroadcastMockRoomState();
        }

        public void SetRoomReady(bool ready)
        {
            if (_mockPlayers.Count > 0)
            {
                _mockPlayers[0].ready = ready;
            }
            BroadcastMockRoomState();

            // Auto-start if all ready and >= 2 players
            if (ready && _mockPlayers.Count >= 2)
            {
                HostStart();
            }
        }

        public void HostStart()
        {
            if (_mockPlayers.Count < 2)
            {
                OnErrorReceived?.Invoke("not_enough_players", "Cần ít nhất 2 người chơi để bắt đầu trận!");
                return;
            }

            _currentRound = 1;
            CurrentPhase = ClientMatchPhase.Placing;
            long deadline = DateTimeOffset.UtcNow.AddSeconds(5).ToUnixTimeMilliseconds();
            OnRoundStarted?.Invoke(_currentRound, deadline);
        }

        public void Commit(int roundId, float x, float y, bool hasAim, float aimX, float aimY)
        {
            // Simulated acknowledge
        }

        public void LockReady(bool ready = true)
        {
            CurrentPhase = ClientMatchPhase.Revealed;
            var reveal = new RevealMessage
            {
                v = 1,
                type = "reveal",
                roundId = _currentRound,
                players = new[]
                {
                    new RevealedPlayerDto { id = "player", position = new PointDto { x = 0f, y = -2f }, aim = new PointDto { x = 0f, y = 1f } },
                    new RevealedPlayerDto { id = "bot_1", position = new PointDto { x = 0f, y = 2f }, aim = new PointDto { x = 0f, y = -1f } }
                }
            };
            OnRevealReceived?.Invoke(reveal);

            // Simulate round result
            var roundRes = new RoundResultMessage
            {
                v = 1,
                type = "round_result",
                roundId = _currentRound,
                players = new[]
                {
                    new ResultPlayerDto { id = "player", hp = 3, eliminated = false },
                    new ResultPlayerDto { id = "bot_1", hp = 2, eliminated = false }
                }
            };
            OnRoundResultReceived?.Invoke(roundRes);

            // End or next
            if (_currentRound >= 5)
            {
                CurrentPhase = ClientMatchPhase.MatchEnd;
                OnMatchEnded?.Invoke(new MatchEndMessage { v = 1, type = "match_end", winners = new[] { "player" } });
            }
            else
            {
                _currentRound++;
                CurrentPhase = ClientMatchPhase.Placing;
                long deadline = DateTimeOffset.UtcNow.AddSeconds(5).ToUnixTimeMilliseconds();
                OnRoundStarted?.Invoke(_currentRound, deadline);
            }
        }

        public void SendEmote(string emoteId)
        {
            OnEmoteReceived?.Invoke("player", emoteId);
        }

        public void LeaveRoom()
        {
            CurrentPhase = ClientMatchPhase.Disconnected;
            OnDisconnected?.Invoke("client_leave");
        }

        public void SendFriendRequest(string targetPlayerId)
        {
            // Handled locally in mock
        }

        private void BroadcastMockRoomState()
        {
            var msg = new RoomStateMessage
            {
                v = 1,
                type = "room_state",
                protocolVersion = 1,
                roomCode = RoomCode,
                hostId = "player",
                started = CurrentPhase != ClientMatchPhase.Lobby,
                reconnectToken = ReconnectToken,
                players = _mockPlayers.ToArray(),
                config = new RoomConfigDto
                {
                    maxPlayers = 6,
                    minPlayersToStart = 2,
                    startHp = 3,
                    placeSeconds = 5,
                    maxRounds = 5,
                    arenaWidth = 8,
                    arenaHeight = 12,
                    playerRadius = 0.4f
                }
            };
            OnRoomStateUpdated?.Invoke(msg);
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            Disconnect();
        }
    }
}
