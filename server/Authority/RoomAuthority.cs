using HitMe.Core;

namespace HitMe.Server.Authority;

public sealed class RoomPlayer
{
    public RoomPlayer(string id, string name) { Id = id; Name = name; }
    public string Id { get; }
    public string Name { get; }
    public bool Ready { get; set; }
}

public sealed class RoomAuthority
{
    private readonly List<RoomPlayer> players = new();
    private readonly int startCountdownSeconds;

    public RoomAuthority(string code, string hostId, GameConfig config, int startCountdownSeconds = 3)
    {
        Code = code;
        HostId = hostId;
        Config = config.Clone();
        this.startCountdownSeconds = startCountdownSeconds < 0 ? 0 : startCountdownSeconds;
    }

    public const int MinPlayers = 2;
    public const int MaxPlayers = 6;
    public string Code { get; }
    public string HostId { get; private set; }
    public GameConfig Config { get; }
    public bool Started { get; private set; }
    public DateTimeOffset? StartDeadline { get; private set; }
    public ServerRoundSession? RoundSession { get; private set; }
    public IReadOnlyList<RoomPlayer> Players => players;

    public bool TryJoin(string playerId, string name, out string error)
    {
        if (Started) { error = "room_started"; return false; }
        if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(name)) { error = "invalid_player"; return false; }
        if (players.Any(p => p.Id == playerId)) { error = "duplicate_player"; return false; }
        if (players.Count >= MaxPlayers) { error = "room_full"; return false; }
        players.Add(new RoomPlayer(playerId, name));
        error = string.Empty;
        return true;
    }

    public bool TryLeave(string playerId, out string error)
    {
        var player = players.FirstOrDefault(p => p.Id == playerId);
        if (player is null) { error = "not_in_room"; return false; }
        players.Remove(player);
        StartDeadline = null;
        if (player.Id == HostId && players.Count > 0) HostId = players[0].Id;
        error = string.Empty;
        return true;
    }

    public bool TrySetRoomReady(string playerId, bool ready, DateTimeOffset now, out string error)
    {
        if (Started) { error = "wrong_phase"; return false; }
        var player = players.FirstOrDefault(p => p.Id == playerId);
        if (player is null) { error = "not_in_room"; return false; }
        player.Ready = ready;
        if (!ready) StartDeadline = null;
        else if (players.Count >= MinPlayers && players.All(candidate => candidate.Ready))
            StartDeadline = now + TimeSpan.FromSeconds(startCountdownSeconds);
        error = string.Empty;
        return true;
    }

    public bool TryHostStart(string playerId, out string error)
    {
        if (Started) { error = "wrong_phase"; return false; }
        if (playerId != HostId) { error = "host_only"; return false; }
        if (players.Count < MinPlayers) { error = "not_enough_players"; return false; }
        StartNow();
        error = string.Empty;
        return true;
    }

    public bool TryStartCountdownIfDue(DateTimeOffset now)
    {
        if (Started || !StartDeadline.HasValue || now < StartDeadline.Value) return false;
        StartNow();
        return true;
    }

    private void StartNow()
    {
        Started = true;
        StartDeadline = null;
        RoundSession = new ServerRoundSession(Config, players.Select(p => p.Id));
    }
}

public sealed class RoomDirectory
{
    private readonly object gate = new();
    private readonly Dictionary<string, RoomAuthority> rooms = new(StringComparer.Ordinal);
    private readonly Func<int, string> codeForAttempt;
    private int attempts;
    private readonly int startCountdownSeconds;

    public RoomDirectory(Func<int, string>? codeForAttempt = null, int startCountdownSeconds = 3)
    {
        this.codeForAttempt = codeForAttempt ?? (n => $"HM{Random.Shared.Next(0, 10000):0000}");
        this.startCountdownSeconds = startCountdownSeconds;
    }

    public RoomAuthority Create(string hostId, string hostName, GameConfig? config, out string error)
    {
        lock (gate)
        {
            for (var i = 0; i < 100; i++)
            {
                var code = codeForAttempt(++attempts);
                if (rooms.ContainsKey(code)) continue;
                var room = new RoomAuthority(code, hostId, config ?? new GameConfig(), startCountdownSeconds);
                if (!room.TryJoin(hostId, hostName, out error)) return null!;
                rooms.Add(code, room);
                error = string.Empty;
                return room;
            }
            error = "room_code_exhausted";
            return null!;
        }
    }

    public bool TryGet(string code, out RoomAuthority room)
    {
        lock (gate) return rooms.TryGetValue(code, out room!);
    }

    public IReadOnlyList<RoomAuthority> Snapshot()
    {
        lock (gate) return rooms.Values.ToArray();
    }

    public bool Join(string code, string playerId, string playerName, out RoomAuthority? room, out string error)
    {
        lock (gate)
        {
            if (!rooms.TryGetValue(code, out var found)) { room = null; error = "room_not_found"; return false; }
            if (!found.TryJoin(playerId, playerName, out error)) { room = found; return false; }
            room = found; return true;
        }
    }

    public bool Leave(string code, string playerId, out RoomAuthority? room, out string error)
    {
        lock (gate)
        {
            if (!rooms.TryGetValue(code, out var found)) { room = null; error = "room_not_found"; return false; }
            if (!found.TryLeave(playerId, out error)) { room = found; return false; }
            if (found.Players.Count == 0) rooms.Remove(code);
            room = found;
            return true;
        }
    }
}
