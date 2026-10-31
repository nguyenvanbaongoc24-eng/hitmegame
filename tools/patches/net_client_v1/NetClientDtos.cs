// AG integration: copy this folder into a Unity Runtime/Network assembly.  First
// provide IProtocolJson with UnityEngine.JsonUtility, then construct a transport.
// This patch intentionally does not implement WebGL's browser WebSocket bridge or
// automatic reconnect; those are Phase 4 integration work.
using System;

namespace HitMe.NetClientV1;

// JsonUtility serializes public fields only.  Keep every wire DTO flat and avoid
// Dictionary, nullable values, interfaces and polymorphic collections.
[Serializable]
public class Envelope
{
    public int v = 1;
    public string type = string.Empty;
}

[Serializable]
public abstract class ClientMessage : Envelope { }

[Serializable]
public sealed class JoinMessage : ClientMessage
{
    public string token = string.Empty;
    public string roomCode = string.Empty;
    public string reconnectToken = string.Empty;
}

[Serializable]
public sealed class QuickMatchMessage : ClientMessage { public string token = string.Empty; }

[Serializable]
public sealed class RoomReadyMessage : ClientMessage { public bool ready; }
[Serializable]
public sealed class StartMessage : ClientMessage { }
[Serializable]
public sealed class LockReadyMessage : ClientMessage { public bool ready = true; }
[Serializable]
public sealed class LeaveMessage : ClientMessage { }
[Serializable]
public sealed class EmoteMessage : ClientMessage { public string emoteId = string.Empty; }

[Serializable]
public sealed class CommitMessage : ClientMessage
{
    public int roundId;
    public float positionX;
    public float positionY;
    // `hasAim=false` replaces the legacy aimDir:null representation. The server
    // accepts both forms while existing clients migrate.
    public bool hasAim;
    public float aimX;
    public float aimY;
}

[Serializable]
public abstract class ServerMessage : Envelope { }

[Serializable]
public sealed class ErrorMessage : ServerMessage
{
    public string code = string.Empty;
    public int remainingSeconds;
}

[Serializable]
public sealed class RoomPlayerDto
{
    public string id = string.Empty;
    public string name = string.Empty;
    public bool ready;
}

[Serializable]
public sealed class RoomConfigDto
{
    public int maxPlayers;
    public int minPlayersToStart;
    public int startHp;
    public float placeSeconds;
    public int maxRounds;
    public float arenaWidth;
    public float arenaHeight;
    public float playerRadius;
}

[Serializable]
public sealed class PointDto { public float x; public float y; }
[Serializable]
public sealed class RevealedPlayerDto
{
    public string id = string.Empty;
    public PointDto position = new PointDto();
    public PointDto aim = new PointDto();
}
[Serializable]
public sealed class HitDto
{
    public string shooterId = string.Empty;
    public string targetId = string.Empty;
    public PointDto point = new PointDto();
    public float distance;
}
[Serializable]
public sealed class ResultPlayerDto
{
    public string id = string.Empty;
    public int hp;
    public bool eliminated;
}

[Serializable]
public sealed class RoomStateMessage : ServerMessage
{
    public int protocolVersion;
    public string roomCode = string.Empty;
    public string hostId = string.Empty;
    public bool started;
    public RoomPlayerDto[] players = Array.Empty<RoomPlayerDto>();
    public string reconnectToken = string.Empty;
    public RoomConfigDto config = new RoomConfigDto();
}

[Serializable]
public sealed class RoundStartMessage : ServerMessage { public int roundId; public long deadline; }
[Serializable]
public sealed class RevealMessage : ServerMessage { public int roundId; public RevealedPlayerDto[] players = Array.Empty<RevealedPlayerDto>(); }
[Serializable]
public sealed class RoundResultMessage : ServerMessage { public int roundId; public HitDto[] hits = Array.Empty<HitDto>(); public ResultPlayerDto[] players = Array.Empty<ResultPlayerDto>(); }
[Serializable]
public sealed class MatchEndMessage : ServerMessage { public string[] winners = Array.Empty<string>(); }
[Serializable]
public sealed class EmoteBroadcastMessage : ServerMessage { public string playerId = string.Empty; public string emoteId = string.Empty; public string phase = string.Empty; }
[Serializable]
public sealed class QuickMatchWaitingMessage : ServerMessage { public int quickMatchHintSeconds; }
[Serializable]
public sealed class QuickMatchHintMessage : ServerMessage { public string code = string.Empty; public int quickMatchHintSeconds; }
[Serializable]
public sealed class QuickMatchLeftMessage : ServerMessage { }

public interface IProtocolJson
{
    // Use the concrete message type; Send accepts ClientMessage and must not
    // accidentally serialize only Envelope fields.
    string ToJson(object value);
    T FromJson<T>(string json);
}

public static class ProtocolReader
{
    // Parse the envelope first, then parse only the DTO declared by `type`.
    public static ServerMessage ReadServerMessage(string json, IProtocolJson codec)
    {
        if (codec == null) throw new ArgumentNullException(nameof(codec));
        var envelope = codec.FromJson<Envelope>(json);
        if (envelope == null || envelope.v != 1) throw new InvalidOperationException("invalid_envelope");
        return envelope.type switch
        {
            "room_state" => codec.FromJson<RoomStateMessage>(json),
            "round_start" => codec.FromJson<RoundStartMessage>(json),
            "reveal" => codec.FromJson<RevealMessage>(json),
            "round_result" => codec.FromJson<RoundResultMessage>(json),
            "match_end" => codec.FromJson<MatchEndMessage>(json),
            "emote" => codec.FromJson<EmoteBroadcastMessage>(json),
            "quick_match_waiting" => codec.FromJson<QuickMatchWaitingMessage>(json),
            "quick_match_hint" => codec.FromJson<QuickMatchHintMessage>(json),
            "quick_match_left" => codec.FromJson<QuickMatchLeftMessage>(json),
            "error" => codec.FromJson<ErrorMessage>(json),
            _ => throw new InvalidOperationException("unknown_server_message")
        };
    }
}
