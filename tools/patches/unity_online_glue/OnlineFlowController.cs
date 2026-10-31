// AG handoff (1) Copy this folder with net_client_v1 into a Unity Runtime assembly.
// (2) Implement UnityOnlineUi : IOnlineUi on Unity's main thread.
// (3) Construct ClientMatchSession with a WebGL/browser transport and pass it here.
// (4) Call Tick() from Update; server deadline remains authoritative.
// (5) Keep reconnectToken private and call Reconnect only after a new socket connects.
// (6) Room countdown is a local display estimate; server sends round_start at lock.
// (7) A room_state never contains positions or aims; only reveal may draw them.
// (8) Emotes during placing belong on player cards, never world positions.
// (9) server_shutdown is a WebSocket close reason, not a JSON message.
// (10) WebGL socket and UnityOnlineUi remain AG integration work.
using System;
using HitMe.NetClientV1;

namespace HitMe.OnlineGlue;

public interface IOnlineUi
{
    void ShowLobby(RoomStateMessage room);
    void ShowWaitingForPlayers(int humans, int maxPlayers);
    void ShowNotEnoughPlayers(int minPlayers);
    void ShowQuickMatchSearching();
    void ShowQuickMatchHint();
    void ShowCountdown(int seconds);
    void ShowPlacing(int round, int secondsLeft);
    void ShowLocked();
    void ShowReveal(RevealMessage reveal);
    void ShowRoundResult(RoundResultMessage result);
    void ShowMatchEnd(MatchEndMessage result);
    void ShowError(string code, string textVi);
    void ShowReconnecting();
    void ShowServerShutdown();
    void ShowEmote(string playerId, string emoteId, bool visibleOnCharacter);
}

public sealed class OnlineFlowController : IDisposable
{
    private readonly ClientMatchSession session;
    private readonly IOnlineUi ui;
    private readonly Func<DateTimeOffset> now;
    private DateTimeOffset? localCountdownEnd;
    private int displayedCountdown = -1;
    private int displayedPlacing = -1;
    private bool revealed;
    private string token = string.Empty;

    public OnlineFlowController(ClientMatchSession session, IOnlineUi ui, Func<DateTimeOffset>? now = null)
    {
        this.session = session ?? throw new ArgumentNullException(nameof(session));
        this.ui = ui ?? throw new ArgumentNullException(nameof(ui));
        this.now = now ?? (() => DateTimeOffset.UtcNow);
        session.MessageReceived += OnMessage;
        session.Closed += OnClosed;
    }

    public void Join(string verifiedToken, string roomCode = "")
    {
        token = verifiedToken;
        session.Send(new JoinMessage { type = "join", token = token, roomCode = roomCode });
    }

    public void QuickMatch(string verifiedToken)
    {
        token = verifiedToken;
        session.Send(new QuickMatchMessage { type = "quick_match", token = token });
    }

    public void Reconnect()
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(session.ReconnectToken))
        {
            ui.ShowError("reconnect_unavailable", "Không có phiên để vào lại.");
            return;
        }
        session.Connect();
        session.Send(new JoinMessage { type = "join", token = token, reconnectToken = session.ReconnectToken });
    }

    public void SetRoomReady(bool ready) => session.Send(new RoomReadyMessage { type = "room_ready", ready = ready });
    public void HostStart() => session.Send(new StartMessage { type = "start" });
    public void Commit(int roundId, float x, float y, bool hasAim, float aimX, float aimY) =>
        session.Send(new CommitMessage { type = "commit", roundId = roundId, positionX = x, positionY = y, hasAim = hasAim, aimX = aimX, aimY = aimY });
    public void LockReady()
    {
        if (session.Phase != ClientMatchPhase.Placing)
        {
            ui.ShowError("wrong_phase", "Chưa tới lúc khóa hướng ném.");
            return;
        }
        session.Send(new LockReadyMessage { type = "lock_ready" });
        ui.ShowLocked();
    }
    public void Leave() => session.Send(new LeaveMessage { type = "leave" });
    public void Emote(string emoteId) => session.Send(new EmoteMessage { type = "emote", emoteId = emoteId });

    public void Tick()
    {
        if (localCountdownEnd.HasValue)
        {
            var seconds = Math.Max(0, (int)Math.Ceiling((localCountdownEnd.Value - now()).TotalSeconds));
            if (seconds != displayedCountdown) { displayedCountdown = seconds; ui.ShowCountdown(seconds); }
        }
        if (session.Phase == ClientMatchPhase.Placing)
        {
            var seconds = Math.Max(0, (int)Math.Ceiling((session.DeadlineUnixMilliseconds - now().ToUnixTimeMilliseconds()) / 1000.0));
            if (seconds != displayedPlacing) { displayedPlacing = seconds; ui.ShowPlacing(session.RoundId, seconds); }
        }
    }

    private void OnMessage(ServerMessage message)
    {
        switch (message)
        {
            case RoomStateMessage room:
                if (!room.started)
                {
                    ui.ShowLobby(room);
                    var players = room.players ?? Array.Empty<RoomPlayerDto>();
                    if (players.Length < room.config.minPlayersToStart)
                    {
                        localCountdownEnd = null;
                        displayedCountdown = -1;
                        ui.ShowWaitingForPlayers(players.Length, room.config.maxPlayers);
                        ui.ShowNotEnoughPlayers(room.config.minPlayersToStart);
                    }
                    else if (AllReady(players))
                    {
                        if (!localCountdownEnd.HasValue) localCountdownEnd = now().AddSeconds(3);
                        Tick();
                    }
                    else
                    {
                        localCountdownEnd = null;
                        displayedCountdown = -1;
                        ui.ShowCountdown(0);
                    }
                }
                break;
            case QuickMatchWaitingMessage: ui.ShowQuickMatchSearching(); break;
            case QuickMatchHintMessage: ui.ShowQuickMatchHint(); break;
            case RoundStartMessage:
                localCountdownEnd = null;
                displayedPlacing = -1;
                revealed = false;
                Tick();
                break;
            case RevealMessage reveal:
                revealed = true;
                ui.ShowReveal(reveal);
                break;
            case RoundResultMessage result: ui.ShowRoundResult(result); break;
            case MatchEndMessage result: ui.ShowMatchEnd(result); break;
            case EmoteBroadcastMessage emote: ui.ShowEmote(emote.playerId, emote.emoteId, revealed); break;
            case ErrorMessage error: ui.ShowError(error.code, ErrorText(error)); break;
        }
    }

    private static bool AllReady(RoomPlayerDto[] players)
    {
        foreach (var player in players) if (!player.ready) return false;
        return true;
    }

    private void OnClosed(string reason)
    {
        localCountdownEnd = null;
        if (string.Equals(reason, "server_shutdown", StringComparison.Ordinal)) ui.ShowServerShutdown();
        else ui.ShowReconnecting();
    }

    private static string ErrorText(ErrorMessage error) => error.code switch
    {
        "not_enough_players" => "Cần thêm người chơi. Bạn có thể Luyện với bot.",
        "protocol_mismatch" => "Phiên bản trò chơi không khớp máy chủ.",
        "auth_invalid" or "auth_required_use_join_token" => "Phiên đăng nhập không hợp lệ.",
        "banned" => $"Bạn cần chờ {error.remainingSeconds} giây trước khi vào trận.",
        "reconnect_invalid" => "Phiên vào lại đã hết hạn.",
        _ => $"Máy chủ báo lỗi: {error.code}"
    };

    public void Dispose()
    {
        session.MessageReceived -= OnMessage;
        session.Closed -= OnClosed;
    }
}
