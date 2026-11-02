using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    public bool? HasAim { get; set; }
    public string? EmoteId { get; set; }
    public string? ChatId { get; set; }
    public string? TargetPlayerId { get; set; }
    public bool Accepted { get; set; }
    public string? RequestId { get; set; }
    public string? ItemId { get; set; }
    public string? NewName { get; set; }
}

public sealed record GatewayMessage(string ConnectionId, string Payload, string? CloseReason = null);

public sealed class ProtocolGateway
{
    public const int Version = 1;
    public const int LatestVersion = 2;
    public const int MaxMessageBytes = 1024;
    public const int MaxMessagesPerSecond = 20;
    public const int QuickMatchHintSeconds = 30;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly HashSet<string> WireFields = new(StringComparer.Ordinal)
    { "v", "type", "roomCode", "playerId", "name", "token", "ready", "start", "reconnectToken", "roundId", "positionX", "positionY", "aimX", "aimY", "hasAim", "emoteId", "chatId", "targetPlayerId", "accepted", "requestId", "itemId", "newName" };
    public static readonly IReadOnlySet<string> AllowedChatIds = new HashSet<string>(StringComparer.Ordinal)
    { "hello", "good_luck", "nice_throw", "well_played", "oops", "thanks" };
    private readonly RoomDirectory rooms;
    private readonly TimeProvider clock;
    private readonly IAuthVerifier auth;
    private readonly IPlayerProfileReader? profiles;
    private readonly IGuestShopService? shop;
    private readonly ServerMetrics? metrics;
    private readonly object gate = new();
    private readonly Dictionary<string, Session> sessions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, PendingQuickMatch> quickMatch = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DisconnectedSession> disconnected = new(StringComparer.Ordinal);
    private readonly Dictionary<string, RateWindow> rateWindows = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DateTimeOffset> lastChat = new(StringComparer.Ordinal);
    private readonly HashSet<(string From, string To)> friendRequests = new();
    private readonly HashSet<(string First, string Second)> friends = new();

    public ProtocolGateway(RoomDirectory rooms, TimeProvider? clock = null, IAuthVerifier? auth = null, IPlayerProfileReader? profiles = null, IGuestShopService? shop = null, ServerMetrics? metrics = null)
    {
        this.rooms = rooms;
        this.clock = clock ?? TimeProvider.System;
        this.auth = auth ?? new FakeAuthVerifier();
        this.profiles = profiles;
        this.shop = shop;
        this.metrics = metrics;
    }

    public string Handle(string connectionId, string raw)
    {
        var messages = HandleMessages(connectionId, raw);
        return messages.FirstOrDefault(message => message.ConnectionId == connectionId)?.Payload ?? Error("no_response");
    }

    public IReadOnlyList<GatewayMessage> HandleMessages(string connectionId, string raw)
    {
        lock (gate)
        {
            if (Encoding.UTF8.GetByteCount(raw) > MaxMessageBytes) return Reply(connectionId, Error("message_too_large"));
            if (!Allow(connectionId)) return Reply(connectionId, Error("rate_limited"));

            JsonDocument document;
            try { document = JsonDocument.Parse(raw); }
            catch (JsonException) { return Reply(connectionId, Error("invalid_json")); }
            using (document)
            {
                if (!ValidEnvelope(document.RootElement)) return Reply(connectionId, Error("bad_message"));
            }
            ClientEnvelope? request;
            try { request = JsonSerializer.Deserialize<ClientEnvelope>(raw, Json); }
            catch (Exception exception) when (exception is JsonException or FormatException or OverflowException)
            { return Reply(connectionId, Error("bad_message")); }
            if (request is null || string.IsNullOrWhiteSpace(request.Type)) return Reply(connectionId, Error("invalid_envelope"));
            if (request.V is not (Version or LatestVersion)) return Reply(connectionId, Error("protocol_mismatch"));

            var messages = request.Type switch
            {
                "join" => JoinMessages(connectionId, request),
                "quick_match" => QuickMatchMessages(connectionId, request),
                "leave" => LeaveMessages(connectionId),
                "room_ready" => RoomReadyMessages(connectionId, request),
                "start" => HostStartMessages(connectionId),
                "lock_ready" => LockReadyMessages(connectionId, request),
                "emote" => EmoteMessages(connectionId, request),
                "chat" when request.V == LatestVersion => ChatMessages(connectionId, request),
                "friend_request" when request.V == LatestVersion => FriendRequestMessages(connectionId, request),
                "friend_reply" when request.V == LatestVersion => FriendReplyMessages(connectionId, request),
                "buy_item" when request.V == LatestVersion => ShopMessages(connectionId, request, "buy"),
                "equip_item" when request.V == LatestVersion => ShopMessages(connectionId, request, "equip"),
                "rename_player" when request.V == LatestVersion => ShopMessages(connectionId, request, "rename"),
                "ready" => Reply(connectionId, Error("legacy_ready_use_room_ready_or_lock_ready")),
                "commit" => CommitMessages(connectionId, request),
                _ => Reply(connectionId, Error("unknown_type"))
            };
            return MapVersions(messages, connectionId, request.V);
        }
    }

    public IReadOnlyList<GatewayMessage> Tick()
    {
        lock (gate)
        {
            PruneDisconnected();
            var output = new List<GatewayMessage>();
            foreach (var entry in quickMatch.Where(pair => !pair.Value.Hinted && clock.GetUtcNow() - pair.Value.QueuedAt >= TimeSpan.FromSeconds(QuickMatchHintSeconds)).ToArray())
            {
                entry.Value.Hinted = true;
                output.Add(new GatewayMessage(entry.Key, JsonSerializer.Serialize(new { v = Version, type = "quick_match_hint", code = "no_opponent_yet", quickMatchHintSeconds = QuickMatchHintSeconds }, Json)));
            }
            foreach (var room in rooms.Snapshot())
            {
                if (!room.Started && room.TryStartCountdownIfDue(clock.GetUtcNow()))
                {
                    output.AddRange(Target(room, room.RoundSession!.BeginRound(clock)));
                    continue;
                }
                if (room.RoundSession is null) continue;
                var started = System.Diagnostics.Stopwatch.GetTimestamp();
                var roundMessages = room.RoundSession.Tick();
                RecordRoundResolutionIfAny(roundMessages, started);
                output.AddRange(Target(room, roundMessages));
            }
            return MapVersions(output);
        }
    }

    private string Join(string connectionId, ClientEnvelope request, out string? replacedConnection)
    {
        replacedConnection = null;
        if (sessions.ContainsKey(connectionId)) return Error("already_joined");
        if (string.IsNullOrWhiteSpace(request.Token)) return Error("auth_required_use_join_token");
        if (!auth.TryVerify(request.Token, out var identity, out var authError)) return Error(authError);
        if (rooms.BanStore.TryGetRemaining(identity.PlayerId, clock.GetUtcNow(), out var remainingSeconds)) return BannedError(remainingSeconds);
        if (!string.IsNullOrWhiteSpace(request.ReconnectToken)) return Reconnect(connectionId, request.ReconnectToken, identity.PlayerId);
        var active = sessions.FirstOrDefault(pair => pair.Value.PlayerId == identity.PlayerId);
        if (active.Key is not null)
        {
            sessions.Remove(active.Key);
            disconnected.Remove(active.Value.ReconnectToken);
            replacedConnection = active.Key;
            var resumed = new Session(active.Value.Room, active.Value.PlayerId, NewReconnectToken(), request.V);
            sessions[connectionId] = resumed;
            return RoomState(resumed.Room, resumed.ReconnectToken);
        }
        RoomAuthority? room;
        string error;
        if (string.IsNullOrWhiteSpace(request.RoomCode))
        {
            room = rooms.Create(identity.PlayerId, identity.DisplayName, new GameConfig(), out error);
            if (room is null) return Error(error == "room_limit" ? "server_busy" : error);
        }
        else if (!rooms.Join(request.RoomCode, identity.PlayerId, identity.DisplayName, out room, out error))
        {
            return Error(error);
        }
        var session = new Session(room!, identity.PlayerId, NewReconnectToken(), request.V);
        sessions[connectionId] = session;
        return RoomState(room!, session.ReconnectToken);
    }

    private IReadOnlyList<GatewayMessage> JoinMessages(string connectionId, ClientEnvelope request)
    {
        var response = Join(connectionId, request, out var replacedConnection);
        if (!sessions.TryGetValue(connectionId, out var session)) return Reply(connectionId, response);
        // Every connected member must learn the new roster. RoomState contains
        // no private placement coordinates or aims, including during reconnect.
        var messages = RoomStateBroadcast(session.Room).ToList();
        if (session.Version == LatestVersion)
            messages.Add(new GatewayMessage(connectionId, ProfileMessage(session)));
        if (replacedConnection is not null)
            messages.Add(new GatewayMessage(replacedConnection, Error("session_replaced"), "session_replaced"));
        if (session.Room.RoundSession is { } roundSession && roundSession.Match.State.Phase == Phase.Placing)
        {
            var active = roundSession.ActiveRound;
            messages.Add(new GatewayMessage(connectionId, JsonSerializer.Serialize(new
            {
                v = Version, type = "round_start", roundId = active.RoundId,
                deadline = active.Deadline.ToUnixTimeMilliseconds()
            }, Json)));
        }
        return messages;
    }

    private IReadOnlyList<GatewayMessage> LeaveMessages(string connectionId)
    {
        var room = sessions.TryGetValue(connectionId, out var session) ? session.Room : null;
        var response = Leave(connectionId);
        var messages = Reply(connectionId, response).ToList();
        if (room is not null && rooms.Snapshot().Contains(room)) messages.AddRange(RoomStateBroadcast(room));
        return messages;
    }

    private IReadOnlyList<GatewayMessage> QuickMatchMessages(string connectionId, ClientEnvelope request)
    {
        if (sessions.ContainsKey(connectionId) || quickMatch.ContainsKey(connectionId)) return Reply(connectionId, Error("already_joined"));
        if (string.IsNullOrWhiteSpace(request.Token)) return Reply(connectionId, Error("auth_required_use_join_token"));
        if (!auth.TryVerify(request.Token, out var identity, out var authError)) return Reply(connectionId, Error(authError));
        if (rooms.BanStore.TryGetRemaining(identity.PlayerId, clock.GetUtcNow(), out var remainingSeconds)) return Reply(connectionId, BannedError(remainingSeconds));
        if (sessions.Values.Any(active => active.PlayerId == identity.PlayerId)) return Reply(connectionId, Error("already_joined"));
        if (quickMatch.Values.Any(waiting => waiting.Identity.PlayerId == identity.PlayerId)) return Reply(connectionId, Error("already_joined"));

        var opponent = quickMatch.FirstOrDefault();
        if (string.IsNullOrEmpty(opponent.Key))
        {
            quickMatch[connectionId] = new PendingQuickMatch(identity, clock.GetUtcNow(), request.V);
            return Reply(connectionId, JsonSerializer.Serialize(new { v = Version, type = "quick_match_waiting", quickMatchHintSeconds = QuickMatchHintSeconds }, Json));
        }

        quickMatch.Remove(opponent.Key);
        var room = rooms.Create(opponent.Value.Identity.PlayerId, opponent.Value.Identity.DisplayName, new GameConfig(), out var error);
        if (room is null) return Reply(connectionId, Error(error == "room_limit" ? "server_busy" : error));
        if (!rooms.Join(room.Code, identity.PlayerId, identity.DisplayName, out _, out error)) return Reply(connectionId, Error(error));
        var first = new Session(room, opponent.Value.Identity.PlayerId, NewReconnectToken(), opponent.Value.Version);
        var second = new Session(room, identity.PlayerId, NewReconnectToken(), request.V);
        sessions[opponent.Key] = first;
        sessions[connectionId] = second;
        room.TrySetRoomReady(first.PlayerId, true, clock.GetUtcNow(), out _);
        room.TrySetRoomReady(second.PlayerId, true, clock.GetUtcNow(), out _);
        var paired = new List<GatewayMessage>
        {
            new(opponent.Key, RoomState(room, first.ReconnectToken)),
            new(connectionId, RoomState(room, second.ReconnectToken))
        };
        if (first.Version == LatestVersion) paired.Add(new GatewayMessage(opponent.Key, ProfileMessage(first)));
        if (second.Version == LatestVersion) paired.Add(new GatewayMessage(connectionId, ProfileMessage(second)));
        return paired;
    }

    public void Disconnect(string connectionId)
    {
        lock (gate)
        {
            rateWindows.Remove(connectionId);
            quickMatch.Remove(connectionId);
            if (!sessions.Remove(connectionId, out var session)) return;
            session.Room.RoundSession?.SetConnected(session.PlayerId, false);
            disconnected[session.ReconnectToken] = new DisconnectedSession(session, clock.GetUtcNow() + TimeSpan.FromSeconds(30));
        }
    }

    public bool HasJoinedOrQueued(string connectionId)
    {
        lock (gate) return sessions.ContainsKey(connectionId) || quickMatch.ContainsKey(connectionId);
    }

    private string Leave(string connectionId)
    {
        if (quickMatch.Remove(connectionId))
            return JsonSerializer.Serialize(new { v = Version, type = "quick_match_left" }, Json);

        if (!sessions.Remove(connectionId, out var session)) return Error("not_joined");
        disconnected.Remove(session.ReconnectToken);
        session.Room.RoundSession?.EliminateDisconnected(session.PlayerId);
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
        var started = System.Diagnostics.Stopwatch.GetTimestamp();
        var roundMessages = session.Room.RoundSession.TryMarkReady(session.PlayerId, out var roundError);
        RecordRoundResolutionIfAny(roundMessages, started);
        return string.IsNullOrEmpty(roundError) ? Target(session.Room, roundMessages) : Reply(connectionId, Error(roundError));
    }

    private void RecordRoundResolutionIfAny(IReadOnlyList<OutboundMessage> messages, long started)
    {
        if (metrics is not null && messages.Any(message => message.Payload.Contains("\"type\":\"round_result\"", StringComparison.Ordinal)))
            metrics.RecordRoundResolution(System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds);
    }

    private IReadOnlyList<GatewayMessage> CommitMessages(string connectionId, ClientEnvelope request)
    {
        if (!sessions.TryGetValue(connectionId, out var session)) return Reply(connectionId, Error("not_joined"));
        var roundSession = session.Room.RoundSession;
        if (roundSession is null) return Reply(connectionId, Error("match_not_started"));
        if (!request.PositionX.HasValue || !request.PositionY.HasValue)
            return Reply(connectionId, Error("invalid_commit"));

        Vec2? aim = request.HasAim != false && request.AimX.HasValue && request.AimY.HasValue
            ? new Vec2(request.AimX.Value, request.AimY.Value)
            : null;
        var commit = new RoundCommit(request.RoundId, new Vec2(request.PositionX.Value, request.PositionY.Value), aim);
        if (!roundSession.TryCommit(session.PlayerId, commit, out var error)) return Reply(connectionId, Error(error));

        // Deliberately only acknowledge the submitting client while placement is private.
        return Reply(connectionId, RoomState(session.Room));
    }

    private IReadOnlyList<GatewayMessage> EmoteMessages(string connectionId, ClientEnvelope request)
    {
        if (!sessions.TryGetValue(connectionId, out var session)) return Reply(connectionId, Error("not_joined"));
        var roundSession = session.Room.RoundSession;
        if (roundSession is null) return Reply(connectionId, Error("wrong_phase"));
        if (!roundSession.TrySendEmote(session.PlayerId, request.EmoteId, clock.GetUtcNow(), out var error)) return Reply(connectionId, Error(error));
        var payload = JsonSerializer.Serialize(new { v = Version, type = "emote", playerId = session.PlayerId, emoteId = request.EmoteId, phase = "placing" }, Json);
        return sessions.Where(pair => ReferenceEquals(pair.Value.Room, session.Room))
            .Select(pair => new GatewayMessage(pair.Key, payload)).ToArray();
    }

    private IReadOnlyList<GatewayMessage> ChatMessages(string connectionId, ClientEnvelope request)
    {
        if (!sessions.TryGetValue(connectionId, out var session)) return Reply(connectionId, Error("not_joined"));
        if (request.ChatId is null || !AllowedChatIds.Contains(request.ChatId)) return Reply(connectionId, Error("invalid_chat_id"));
        var now = clock.GetUtcNow();
        if (lastChat.TryGetValue(session.PlayerId, out var previous) && now - previous < TimeSpan.FromSeconds(1))
            return Reply(connectionId, Error("chat_rate_limited"));
        lastChat[session.PlayerId] = now;
        var payload = JsonSerializer.Serialize(new { v = LatestVersion, type = "chat", playerId = session.PlayerId, chatId = request.ChatId }, Json);
        // Canned phrase IDs contain no placement coordinates or free-form text.
        return sessions.Where(pair => ReferenceEquals(pair.Value.Room, session.Room) && pair.Value.Version == LatestVersion)
            .Select(pair => new GatewayMessage(pair.Key, payload)).ToArray();
    }

    private IReadOnlyList<GatewayMessage> FriendRequestMessages(string connectionId, ClientEnvelope request)
    {
        if (!sessions.TryGetValue(connectionId, out var session)) return Reply(connectionId, Error("not_joined"));
        if (string.IsNullOrWhiteSpace(request.TargetPlayerId) || request.TargetPlayerId == session.PlayerId)
            return Reply(connectionId, Error("invalid_friend_target"));
        var target = sessions.FirstOrDefault(pair => pair.Value.PlayerId == request.TargetPlayerId
            && ReferenceEquals(pair.Value.Room, session.Room) && pair.Value.Version == LatestVersion);
        if (target.Key is null) return Reply(connectionId, Error("friend_target_unavailable"));
        var pairKey = FriendKey(session.PlayerId, request.TargetPlayerId);
        if (friends.Contains(pairKey)) return Reply(connectionId, Error("already_friends"));
        if (!friendRequests.Add((session.PlayerId, request.TargetPlayerId)))
            return Reply(connectionId, Error("friend_request_pending"));
        return new[] { new GatewayMessage(target.Key,
            JsonSerializer.Serialize(new { v = LatestVersion, type = "friend_request", playerId = session.PlayerId }, Json)) };
    }

    private IReadOnlyList<GatewayMessage> FriendReplyMessages(string connectionId, ClientEnvelope request)
    {
        if (!sessions.TryGetValue(connectionId, out var session)) return Reply(connectionId, Error("not_joined"));
        if (string.IsNullOrWhiteSpace(request.TargetPlayerId)
            || !friendRequests.Remove((request.TargetPlayerId, session.PlayerId)))
            return Reply(connectionId, Error("friend_request_not_found"));
        if (request.Accepted) friends.Add(FriendKey(session.PlayerId, request.TargetPlayerId));
        var payload = JsonSerializer.Serialize(new
        { v = LatestVersion, type = "friend_reply", playerId = session.PlayerId,
            targetPlayerId = request.TargetPlayerId, accepted = request.Accepted }, Json);
        var replies = new List<GatewayMessage> { new(connectionId, payload) };
        foreach (var source in sessions.Where(pair => pair.Value.PlayerId == request.TargetPlayerId && pair.Value.Version == LatestVersion))
            replies.Add(new GatewayMessage(source.Key, payload));
        return replies;
    }

    private static (string First, string Second) FriendKey(string a, string b) =>
        string.CompareOrdinal(a, b) < 0 ? (a, b) : (b, a);

    private IReadOnlyList<GatewayMessage> ShopMessages(string connectionId, ClientEnvelope request, string action)
    {
        if (!sessions.TryGetValue(connectionId, out var session)) return Reply(connectionId, Error("not_joined"));
        if (shop is null) return Reply(connectionId, Error("shop_unavailable"));
        if (!Guid.TryParse(request.RequestId, out var requestId) || requestId == Guid.Empty)
            return Reply(connectionId, Error("invalid_shop_request"));
        try
        {
            var result = shop.Execute(session.PlayerId, new ShopRequest(action, requestId, request.ItemId, request.NewName));
            var output = new List<GatewayMessage>
            {
                new(connectionId, JsonSerializer.Serialize(new { v = LatestVersion, type = "shop_result",
                    requestId, action, gold = result.Gold, displayName = result.DisplayName,
                    renameCount = result.RenameCount, equippedWeaponId = result.EquippedWeaponId,
                    equippedCharacterId = result.EquippedCharacterId }, Json)),
                new(connectionId, ProfileMessage(session))
            };
            return output;
        }
        catch (ShopException error) { return Reply(connectionId, Error(error.Code)); }
        catch (Exception error) when (error is HttpRequestException or TaskCanceledException or JsonException or IOException)
        { return Reply(connectionId, Error("shop_unavailable")); }
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

        var session = new Session(detached.Session.Room, detached.Session.PlayerId, NewReconnectToken(), detached.Session.Version);
        sessions[connectionId] = session;
        session.Room.RoundSession?.SetConnected(session.PlayerId, true);
        return RoomState(session.Room, session.ReconnectToken);
    }

    private void PruneDisconnected()
    {
        var now = clock.GetUtcNow();
        foreach (var entry in disconnected.Where(pair => pair.Value.ExpiresAt <= now).ToArray())
        {
            disconnected.Remove(entry.Key);
            if (!sessions.Values.Any(session => ReferenceEquals(session.Room, entry.Value.Session.Room) && session.PlayerId == entry.Value.Session.PlayerId))
            {
                entry.Value.Session.Room.RoundSession?.EliminateDisconnected(entry.Value.Session.PlayerId);
                rooms.Leave(entry.Value.Session.Room.Code, entry.Value.Session.PlayerId, out _, out _);
            }
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

    private static bool ValidEnvelope(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object) return false;
        foreach (var field in root.EnumerateObject())
        {
            if (!WireFields.Contains(field.Name)) return false;
            var value = field.Value;
            if (value.ValueKind == JsonValueKind.Null && field.Name is not ("v" or "type")) continue;
            if (field.Name is "v" or "roundId")
            { if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out _)) return false; }
            else if (field.Name is "positionX" or "positionY" or "aimX" or "aimY")
            { if (value.ValueKind != JsonValueKind.Number || !value.TryGetSingle(out var number) || !float.IsFinite(number)) return false; }
            else if (field.Name is "ready" or "start" or "hasAim" or "accepted")
            { if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) return false; }
            else
            {
                var maxLength = field.Name switch { "type" => 32, "roomCode" => 6, "emoteId" => 32, "token" or "reconnectToken" => 256, _ => 64 };
                if (value.ValueKind != JsonValueKind.String || value.GetString()!.Length > maxLength) return false;
            }
        }
        return true;
    }

    private static string RoomState(RoomAuthority room, string? reconnectToken = null) => JsonSerializer.Serialize(new
    {
        v = Version,
        type = "room_state",
        protocolVersion = Version,
        roomCode = room.Code,
        hostId = room.HostId,
        started = room.Started,
        players = room.Players.Select(p => new { id = p.Id, name = p.Name, ready = p.Ready }),
        reconnectToken,
        config = new { maxPlayers = room.Config.MaxPlayers, minPlayersToStart = RoomAuthority.MinPlayers, startHp = room.Config.StartHp, placeSeconds = room.Config.PlaceSeconds, maxRounds = room.Config.MaxRounds, arenaWidth = room.Config.ArenaWidth, arenaHeight = room.Config.ArenaHeight, playerRadius = room.Config.PlayerRadius }
    }, Json);

    private IReadOnlyList<GatewayMessage> RoomStateBroadcast(RoomAuthority room)
    {
        var output = new List<GatewayMessage>();
        foreach (var session in sessions)
            if (ReferenceEquals(session.Value.Room, room)) output.Add(new GatewayMessage(session.Key, RoomState(room, session.Value.ReconnectToken)));
        return output;
    }

    private static string Error(string code) => JsonSerializer.Serialize(new { v = Version, type = "error", code }, Json);
    private static string BannedError(int remainingSeconds) => JsonSerializer.Serialize(new { v = Version, type = "error", code = "banned", remainingSeconds }, Json);

    private string ProfileMessage(Session session)
    {
        try
        {
            var snapshot = profiles?.Read(session.PlayerId) ?? new ProfileSnapshot(session.PlayerId, 0, 0, 0);
            return JsonSerializer.Serialize(new
            {
                v = LatestVersion, type = "profile_state", playerId = session.PlayerId,
                name = snapshot.DisplayName ?? session.Room.Players.FirstOrDefault(player => player.Id == session.PlayerId)?.Name ?? session.PlayerId,
                gold = snapshot.Gold, matchesPlayed = snapshot.MatchesPlayed, bestRank = snapshot.BestRank,
                rank = "unranked", points = 0,
                purchasedItemIds = snapshot.PurchasedItemIds ?? Array.Empty<string>(),
                ownedItemIds = new[] { "dep_to_ong" }.Concat(snapshot.PurchasedItemIds ?? Array.Empty<string>()).Distinct().ToArray(),
                equippedWeaponId = snapshot.EquippedWeaponId, equippedCharacterId = snapshot.EquippedCharacterId,
                renameCount = snapshot.RenameCount,
                inventoryStatus = snapshot.PurchasedItemIds is null ? "starter_only" : "authoritative"
            }, Json);
        }
        catch (HttpRequestException) { return Error("profile_unavailable"); }
    }

    private static string NewReconnectToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static IReadOnlyList<GatewayMessage> Reply(string connectionId, string payload) => new[] { new GatewayMessage(connectionId, payload) };

    private IReadOnlyList<GatewayMessage> MapVersions(IReadOnlyList<GatewayMessage> messages, string? requester = null, int requestedVersion = Version)
    {
        return messages.Select(message =>
        {
            var version = sessions.TryGetValue(message.ConnectionId, out var session) ? session.Version
                : quickMatch.TryGetValue(message.ConnectionId, out var waiting) ? waiting.Version
                : message.ConnectionId == requester ? requestedVersion : Version;
            if (version == Version) return message;
            var node = JsonNode.Parse(message.Payload)!.AsObject();
            node["v"] = version;
            if (node["type"]?.GetValue<string>() == "room_state") node["protocolVersion"] = version;
            return message with { Payload = node.ToJsonString(Json) };
        }).ToArray();
    }

    private IReadOnlyList<GatewayMessage> Target(RoomAuthority room, IReadOnlyList<OutboundMessage> messages)
    {
        var output = new List<GatewayMessage>();
        foreach (var message in messages)
        {
            foreach (var session in sessions)
            {
                if (ReferenceEquals(session.Value.Room, room) && session.Value.PlayerId == message.PlayerId)
                {
                    output.Add(new GatewayMessage(session.Key, message.Payload));
                    if (session.Value.Version == LatestVersion && message.Payload.Contains("\"type\":\"match_end\"", StringComparison.Ordinal))
                        output.Add(new GatewayMessage(session.Key, ProfileMessage(session.Value)));
                }
            }
        }
        return output;
    }

    private sealed class Session
    {
        public Session(RoomAuthority room, string playerId, string reconnectToken, int version) { Room = room; PlayerId = playerId; ReconnectToken = reconnectToken; Version = version; }
        public RoomAuthority Room { get; }
        public string PlayerId { get; }
        public string ReconnectToken { get; }
        public int Version { get; }
    }

    private sealed record DisconnectedSession(Session Session, DateTimeOffset ExpiresAt);
    private sealed class PendingQuickMatch
    {
        public PendingQuickMatch(PlayerIdentity identity, DateTimeOffset queuedAt, int version) { Identity = identity; QueuedAt = queuedAt; Version = version; }
        public PlayerIdentity Identity { get; }
        public DateTimeOffset QueuedAt { get; }
        public bool Hinted { get; set; }
        public int Version { get; }
    }

    private sealed class RateWindow
    {
        public RateWindow(DateTimeOffset start) { Start = start; }
        public DateTimeOffset Start { get; set; }
        public int Count { get; set; }
    }
}
