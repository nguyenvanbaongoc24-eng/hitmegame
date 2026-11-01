using System;
using UnityEngine;
using HitMe.OnlineGlue;
using HitMe.NetClientV1;

namespace HitMe.UI
{
    /// <summary>
    /// Phase 3 (N2/N3): Unity implementation of IOnlineUi bridging OnlineFlowController
    /// to ProductionUIController on Unity's main thread.
    /// </summary>
    public sealed class UnityOnlineUi : MonoBehaviour, IOnlineUi
    {
        public static UnityOnlineUi Instance { get; private set; }

        private ProductionUIController _ui;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                gameObject.SetActive(false);
                return;
            }
            _ui = GetComponent<ProductionUIController>();
            if (_ui == null) _ui = FindAnyObjectByType<ProductionUIController>();
        }

        private ProductionUIController UI
        {
            get
            {
                if (_ui == null) _ui = FindAnyObjectByType<ProductionUIController>();
                return _ui;
            }
        }

        public void ShowLobby(RoomStateMessage room)
        {
            if (UI == null || room == null) return;
            UI.ShowScreen(ProductionScreen.Lobby);

            int playerCount = room.players != null ? room.players.Length : 0;
            string roomName = "Phòng Trực Tuyến";
            string roomCode = !string.IsNullOrEmpty(room.roomCode) ? room.roomCode : "HM0001";
            UI.UpdateLobbyRoster(playerCount, roomName, roomCode);
        }

        public void ShowWaitingForPlayers(int humans, int maxPlayers)
        {
            if (UI == null) return;
            UI.ShowToast($"Đang đợi người chơi... ({humans}/{maxPlayers})");
        }

        public void ShowNotEnoughPlayers(int minPlayers)
        {
            if (UI == null) return;
            UI.ShowErrorToast($"Cần ít nhất {minPlayers} người chơi để bắt đầu trận!");
        }

        public void ShowQuickMatchSearching()
        {
            if (UI == null) return;
            UI.ShowQuickMatchDialog(false);
        }

        public void ShowQuickMatchHint()
        {
            if (UI == null) return;
            UI.ShowQuickMatchDialog(true);
        }

        public void ShowCountdown(int seconds)
        {
            if (UI == null) return;
            UI.ShowToast($"Trận đấu bắt đầu sau {seconds}s...", 1.0f);
        }

        public void ShowPlacing(int round, int secondsLeft)
        {
            if (UI == null) return;
            UI.ShowScreen(ProductionScreen.Gameplay);
        }

        public void ShowLocked()
        {
            if (UI == null) return;
            UI.ShowToast("Đã khóa lượt đặt!");
        }

        public void ShowReveal(RevealMessage reveal)
        {
            if (UI == null || reveal == null) return;
            // Reveal opponent actions
        }

        public void ShowRoundResult(RoundResultMessage result)
        {
            if (UI == null || result == null) return;
            // Display round result
        }

        public void ShowMatchEnd(MatchEndMessage result)
        {
            if (UI == null || result == null) return;
            UI.ShowScreen(ProductionScreen.Result);
        }

        public void ShowError(string code, string textVi)
        {
            if (UI == null) return;
            string message = !string.IsNullOrEmpty(textVi) ? textVi : UiStrings.GetNetErrorMessage(code);
            UI.ShowErrorToast(message);
        }

        public void ShowReconnecting()
        {
            if (UI == null) return;
            UI.ShowReconnectOverlay(30f);
        }

        public void ShowServerShutdown()
        {
            if (UI == null) return;
            UI.HideReconnectOverlay();
            UI.ShowErrorToast("Máy chủ đã đóng hoặc đang bảo trì.");
        }

        public void ShowEmote(string playerId, string emoteId, bool visibleOnCharacter)
        {
            Audio.AudioService.Instance?.PlayChatPop();
            if (UI == null) return;
            UI.ShowToast($"[{playerId}] {emoteId}", 1.5f);
        }
    }
}
