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
public sealed class RoomStateMessage : ServerMessage
{
    public string roomCode = string.Empty;
    public string hostId = string.Empty;
    public bool started;
    public RoomPlayerDto[] players = Array.Empty<RoomPlayerDto>();
    public string reconnectToken = string.Empty;
}

[Serializable]
public sealed class RoundStartMessage : ServerMessage { public int roundId; public long deadline; }
[Serializable]
public sealed class RevealMessage : ServerMessage { public int roundId; }
[Serializable]
public sealed class RoundResultMessage : ServerMessage { public int roundId; }
[Serializable]
public sealed class MatchEndMessage : ServerMessage { public string[] winners = Array.Empty<string>(); }
[Serializable]
public sealed class EmoteBroadcastMessage : ServerMessage { public string playerId = string.Empty; public string emoteId = string.Empty; public string phase = string.Empty; }

public interface IProtocolJson
{
    string ToJson<T>(T value);
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
            "error" => codec.FromJson<ErrorMessage>(json),
            _ => throw new InvalidOperationException("unknown_server_message")
        };
    }
}
