using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using HitMe.Core;
using HitMe.Server.Authority;

namespace HitMe.Server.Protocol;

public sealed class ClientEnvelope
{
    public int V { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? RoomCode { get; set; }
    public string? PlayerId { get; set; }
    public string? Name { get; set; }
    public string? Token { get; set; }
    public bool Ready { get; set; }
    public bool Start { get; set; }
    public string? ReconnectToken { get; set; }
    public int RoundId { get; set; }
    public float? PositionX { get; set; }
    public float? PositionY { get; set; }
    public float? AimX { get; set; }
    public float? AimY { get; set; }
}

public sealed record GatewayMessage(string ConnectionId, string Payload);

public sealed class ProtocolGateway
{
    public const int Version = 1;
    public const int MaxMessageBytes = 1024;
    public const int MaxMessagesPerSecond = 20;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly RoomDirectory rooms;
    private readonly TimeProvider clock;
    private readonly IAuthVerifier auth;
    private readonly Dictionary<string, Session> sessions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DisconnectedSession> disconnected = new(StringComparer.Ordinal);
    private readonly Dictionary<string, RateWindow> rateWindows = new(StringComparer.Ordinal);

    public ProtocolGateway(RoomDirectory rooms, TimeProvider? clock = null, IAuthVerifier? auth = null)
    {
        this.rooms = rooms;
        this.clock = clock ?? TimeProvider.System;
        this.auth = auth ?? new FakeAuthVerifier();
    }

    public string Handle(string connectionId, string raw)
    {
        var messages = HandleMessages(connectionId, raw);
        return messages.FirstOrDefault(message => message.ConnectionId == connectionId)?.Payload ?? Error("no_response");
    }

    public IReadOnlyList<GatewayMessage> HandleMessages(string connectionId, string raw)
    {
        if (Encoding.UTF8.GetByteCount(raw) > MaxMessageBytes) return Reply(connectionId, Error("message_too_large"));
        if (!Allow(connectionId)) return Reply(connectionId, Error("rate_limited"));

        ClientEnvelope? request;
        try { request = JsonSerializer.Deserialize<ClientEnvelope>(raw, Json); }
        catch (JsonException) { return Reply(connectionId, Error("invalid_json")); }
        if (request is null || request.V != Version || string.IsNullOrWhiteSpace(request.Type)) return Reply(connectionId, Error("invalid_envelope"));

        return request.Type switch
        {
            "join" => Reply(connectionId, Join(connectionId, request)),
            "leave" => Reply(connectionId, Leave(connectionId)),
            "room_ready" => RoomReadyMessages(connectionId, request),
            "start" => HostStartMessages(connectionId),
            "lock_ready" => LockReadyMessages(connectionId, request),
            "ready" => Reply(connectionId, Error("legacy_ready_use_room_ready_or_lock_ready")),
            "commit" => CommitMessages(connectionId, request),
            _ => Reply(connectionId, Error("unknown_type"))
        };
    }

    public IReadOnlyList<GatewayMessage> Tick()
    {
        PruneDisconnected();
        var output = new List<GatewayMessage>();
        foreach (var room in rooms.Snapshot())
        {
            if (!room.Started && room.TryStartCountdownIfDue(clock.GetUtcNow()))
            {
                output.AddRange(Target(room, room.RoundSession!.BeginRound(clock)));
                continue;
            }
            if (room.RoundSession is null) continue;
            output.AddRange(Target(room, room.RoundSession.Tick()));
        }
        return output;
    }

    private string Join(string connectionId, ClientEnvelope request)
    {
        if (sessions.ContainsKey(connectionId)) return Error("already_joined");
        if (string.IsNullOrWhiteSpace(request.Token)) return Error("auth_required_use_join_token");
        if (!auth.TryVerify(request.Token, out var identity, out var authError)) return Error(authError);
        if (!string.IsNullOrWhiteSpace(request.ReconnectToken)) return Reconnect(connectionId, request.ReconnectToken, identity.PlayerId);
        RoomAuthority? room;
        string error;
        if (string.IsNullOrWhiteSpace(request.RoomCode))
        {
            room = rooms.Create(identity.PlayerId, identity.DisplayName, new GameConfig(), out error);
            if (room is null) return Error(error);
        }
        else if (!rooms.Join(request.RoomCode, identity.PlayerId, identity.DisplayName, out room, out error))
        {
            return Error(error);
        }
        var session = new Session(room!, identity.PlayerId, NewReconnectToken());
        sessions[connectionId] = session;
        return RoomState(room!, session.ReconnectToken);
    }

    public void Disconnect(string connectionId)
    {
        if (!sessions.Remove(connectionId, out var session)) return;
        disconnected[session.ReconnectToken] = new DisconnectedSession(session, clock.GetUtcNow() + TimeSpan.FromSeconds(30));
    }

    private string Leave(string connectionId)
    {
        if (!sessions.Remove(connectionId, out var session)) return Error("not_joined");
        disconnected.Remove(session.ReconnectToken);
        return rooms.Leave(session.Room.Code, session.PlayerId, out var room, out var error) ? RoomState(room!) : Error(error);
    }

    private IReadOnlyList<GatewayMessage> RoomReadyMessages(string connectionId, ClientEnvelope request)
    {
        if (!sessions.TryGetValue(connectionId, out var session)) return Reply(connectionId, Error("not_joined"));
        if (session.Room.RoundSession is not null) return Reply(connectionId, Error("wrong_phase"));
        if (!session.Room.TrySetRoomReady(session.PlayerId, request.Ready, clock.GetUtcNow(), out var error)) return Reply(connectionId, Error(error));
        return RoomStateBroadcast(session.Room);
    }

    private IReadOnlyList<GatewayMessage> HostStartMessages(string connectionId)
    {
        if (!sessions.TryGetValue(connectionId, out var session)) return Reply(connectionId, Error("not_joined"));
        if (session.Room.RoundSession is not null) return Reply(connectionId, Error("wrong_phase"));
        if (!session.Room.TryHostStart(session.PlayerId, out var error)) return Reply(connectionId, Error(error));
        return Target(session.Room, session.Room.RoundSession!.BeginRound(clock));
    }

    private IReadOnlyList<GatewayMessage> LockReadyMessages(string connectionId, ClientEnvelope request)
    {
        if (!sessions.TryGetValue(connectionId, out var session)) return Reply(connectionId, Error("not_joined"));
        if (session.Room.RoundSession is null) return Reply(connectionId, Error("wrong_phase"));
        if (!request.Ready) return Reply(connectionId, Error("ready_required"));
        var roundMessages = session.Room.RoundSession.TryMarkReady(session.PlayerId, out var roundError);
        return string.IsNullOrEmpty(roundError) ? Target(session.Room, roundMessages) : Reply(connectionId, Error(roundError));
    }

    private IReadOnlyList<GatewayMessage> CommitMessages(string connectionId, ClientEnvelope request)
    {
        if (!sessions.TryGetValue(connectionId, out var session)) return Reply(connectionId, Error("not_joined"));
        var roundSession = session.Room.RoundSession;
        if (roundSession is null) return Reply(connectionId, Error("match_not_started"));
        if (!request.PositionX.HasValue || !request.PositionY.HasValue)
            return Reply(connectionId, Error("invalid_commit"));

        Vec2? aim = request.AimX.HasValue && request.AimY.HasValue
            ? new Vec2(request.AimX.Value, request.AimY.Value)
            : null;
        var commit = new RoundCommit(request.RoundId, new Vec2(request.PositionX.Value, request.PositionY.Value), aim);
        if (!roundSession.TryCommit(session.PlayerId, commit, out var error)) return Reply(connectionId, Error(error));

        // Deliberately only acknowledge the submitting client while placement is private.
        return Reply(connectionId, RoomState(session.Room));
    }

    private string Reconnect(string connectionId, string reconnectToken, string authenticatedPlayerId)
    {
        PruneDisconnected();
        if (!disconnected.Remove(reconnectToken, out var detached)) return Error("reconnect_invalid");
        if (!string.Equals(detached.Session.PlayerId, authenticatedPlayerId, StringComparison.Ordinal))
        {
            disconnected[detached.Session.ReconnectToken] = detached;
            return Error("reconnect_invalid");
        }

        var session = new Session(detached.Session.Room, detached.Session.PlayerId, NewReconnectToken());
        sessions[connectionId] = session;
        return RoomState(session.Room, session.ReconnectToken);
    }

    private void PruneDisconnected()
    {
        var now = clock.GetUtcNow();
        foreach (var entry in disconnected.Where(pair => pair.Value.ExpiresAt <= now).ToArray())
        {
            disconnected.Remove(entry.Key);
            if (!sessions.Values.Any(session => ReferenceEquals(session.Room, entry.Value.Session.Room) && session.PlayerId == entry.Value.Session.PlayerId))
                rooms.Leave(entry.Value.Session.Room.Code, entry.Value.Session.PlayerId, out _, out _);
        }
    }

    private bool Allow(string connectionId)
    {
        var now = clock.GetUtcNow();
        if (!rateWindows.TryGetValue(connectionId, out var window)) { window = new RateWindow(now); rateWindows[connectionId] = window; }
        if (now - window.Start >= TimeSpan.FromSeconds(1)) { window.Start = now; window.Count = 0; }
        window.Count++;
        return window.Count <= MaxMessagesPerSecond;
    }

    private static string RoomState(RoomAuthority room, string? reconnectToken = null) => JsonSerializer.Serialize(new
    {
        v = Version,
        type = "room_state",
        roomCode = room.Code,
        hostId = room.HostId,
        started = room.Started,
        players = room.Players.Select(p => new { id = p.Id, name = p.Name, ready = p.Ready }),
        reconnectToken,
        config = new { maxPlayers = room.Config.MaxPlayers, startHp = room.Config.StartHp, placeSeconds = room.Config.PlaceSeconds, maxRounds = room.Config.MaxRounds, arenaWidth = room.Config.ArenaWidth, arenaHeight = room.Config.ArenaHeight, playerRadius = room.Config.PlayerRadius }
    }, Json);

    private IReadOnlyList<GatewayMessage> RoomStateBroadcast(RoomAuthority room)
    {
        var output = new List<GatewayMessage>();
        foreach (var session in sessions)
            if (ReferenceEquals(session.Value.Room, room)) output.Add(new GatewayMessage(session.Key, RoomState(room, session.Value.ReconnectToken)));
        return output;
    }

    private static string Error(string code) => JsonSerializer.Serialize(new { v = Version, type = "error", code }, Json);

    private static string NewReconnectToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static IReadOnlyList<GatewayMessage> Reply(string connectionId, string payload) => new[] { new GatewayMessage(connectionId, payload) };

    private IReadOnlyList<GatewayMessage> Target(RoomAuthority room, IReadOnlyList<OutboundMessage> messages)
    {
        var output = new List<GatewayMessage>();
        foreach (var message in messages)
        {
            foreach (var session in sessions)
            {
                if (ReferenceEquals(session.Value.Room, room) && session.Value.PlayerId == message.PlayerId)
                    output.Add(new GatewayMessage(session.Key, message.Payload));
            }
        }
        return output;
    }

    private sealed class Session
    {
        public Session(RoomAuthority room, string playerId, string reconnectToken) { Room = room; PlayerId = playerId; ReconnectToken = reconnectToken; }
        public RoomAuthority Room { get; }
        public string PlayerId { get; }
        public string ReconnectToken { get; }
    }

    private sealed record DisconnectedSession(Session Session, DateTimeOffset ExpiresAt);

    private sealed class RateWindow
    {
        public RateWindow(DateTimeOffset start) { Start = start; }
        public DateTimeOffset Start { get; set; }
        public int Count { get; set; }
    }
}
