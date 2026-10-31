// AG integration: copy beside NetClientDtos.cs and MatchTransports.cs.
// Callbacks may arrive off the Unity main thread; AG must marshal them before
// touching GameObjects. WebGL transport and reconnect orchestration are absent.
using System;

namespace HitMe.NetClientV1;

public enum ClientMatchPhase { Disconnected, Queueing, Lobby, Placing, Revealed, RoundResult, MatchEnd }

public sealed class ClientMatchSession : IDisposable
{
    private readonly IMatchTransport transport;

    public ClientMatchSession(IMatchTransport transport)
    {
        this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
        transport.Received += OnReceived;
    }

    public ClientMatchPhase Phase { get; private set; } = ClientMatchPhase.Disconnected;
    public int RoundId { get; private set; }
    public long DeadlineUnixMilliseconds { get; private set; }
    public string RoomCode { get; private set; } = string.Empty;
    public event Action<ServerMessage>? MessageReceived;
    public event Action<ClientMatchPhase>? PhaseChanged;

    public void Connect() => transport.Connect();
    public void Send(ClientMessage message) => transport.Send(message);

    private void OnReceived(ServerMessage message)
    {
        switch (message)
        {
            case QuickMatchWaitingMessage:
                SetPhase(ClientMatchPhase.Queueing);
                break;
            case QuickMatchLeftMessage:
                SetPhase(ClientMatchPhase.Disconnected);
                break;
            case RoomStateMessage room:
                if (room.protocolVersion != 1) throw new InvalidOperationException("protocol_mismatch");
                RoomCode = room.roomCode;
                if (!room.started || Phase is ClientMatchPhase.Disconnected or ClientMatchPhase.Queueing)
                    SetPhase(ClientMatchPhase.Lobby);
                break;
            case RoundStartMessage start:
                RoundId = start.roundId;
                DeadlineUnixMilliseconds = start.deadline;
                SetPhase(ClientMatchPhase.Placing);
                break;
            case RevealMessage:
                SetPhase(ClientMatchPhase.Revealed);
                break;
            case RoundResultMessage:
                SetPhase(ClientMatchPhase.RoundResult);
                break;
            case MatchEndMessage:
                SetPhase(ClientMatchPhase.MatchEnd);
                break;
        }
        MessageReceived?.Invoke(message);
    }

    private void SetPhase(ClientMatchPhase value)
    {
        if (Phase == value) return;
        Phase = value;
        PhaseChanged?.Invoke(value);
    }

    public void Dispose()
    {
        transport.Received -= OnReceived;
        transport.Dispose();
    }
}
