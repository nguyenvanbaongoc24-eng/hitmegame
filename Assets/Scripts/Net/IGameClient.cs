using System;
using System.Collections.Generic;

namespace HitMe.NetClientV1
{
    /// <summary>
    /// Phase 3 (A8): Unified client network interface.
    /// Bridges UI/Gameplay to either local mock simulation (offline/bot) or authoritative WebSocket server.
    /// </summary>
    public interface IGameClient : IDisposable
    {
        bool IsConnected { get; }
        string RoomCode { get; }
        string ReconnectToken { get; }
        ClientMatchPhase CurrentPhase { get; }

        // Server Event Callbacks
        event Action<string> OnConnected;
        event Action<string> OnDisconnected;
        event Action<RoomStateMessage> OnRoomStateUpdated;
        event Action<int, long> OnRoundStarted; // roundId, deadlineUnixMs
        event Action<RevealMessage> OnRevealReceived;
        event Action<RoundResultMessage> OnRoundResultReceived;
        event Action<MatchEndMessage> OnMatchEnded;
        event Action<string, string> OnEmoteReceived; // playerId, emoteId
        event Action<string, string> OnErrorReceived; // errorCode, localizedText
        event Action<string> OnQuickMatchWaiting;
        event Action OnQuickMatchHint;

        // Client Outgoing Actions
        void Connect(string serverUrl = null);
        void Disconnect();
        void CreateRoom(string guestToken);
        void JoinRoom(string roomCode, string guestToken, string reconnectToken = "");
        void QuickMatch(string guestToken);
        void SetRoomReady(bool ready);
        void HostStart();
        void Commit(int roundId, float x, float y, bool hasAim, float aimX, float aimY);
        void LockReady(bool ready = true);
        void SendEmote(string emoteId);
        void LeaveRoom();
        void SendFriendRequest(string targetPlayerId);
    }
}
