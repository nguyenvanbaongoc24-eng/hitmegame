using System.Net.WebSockets;
using System.Text;

namespace HitMe.Server.Protocol;

/// <summary>Reads a text message into a fixed-size buffer; never allocates from a client length.</summary>
public static class WebSocketMessageReader
{
    public const int MaxFrameBytes = 1024;
    private const int ChunkBytes = 256;

    public static async Task<string?> ReceiveTextAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        var chunk = new byte[ChunkBytes];
        var frame = new byte[MaxFrameBytes];
        var total = 0;
        WebSocketReceiveResult result;
        do
        {
            result = await socket.ReceiveAsync(chunk, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close) return null;
            if (result.MessageType != WebSocketMessageType.Text) throw new WebSocketException(WebSocketError.InvalidMessageType);
            if (result.Count > MaxFrameBytes - total) throw new WebSocketException(WebSocketError.Faulted, "message_too_large");
            Buffer.BlockCopy(chunk, 0, frame, total, result.Count);
            total += result.Count;
        } while (!result.EndOfMessage);
        return Encoding.UTF8.GetString(frame, 0, total);
    }
}
