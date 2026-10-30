using System.Text.Json;

namespace HitMe.Server.Protocol;

/// <summary>Log metadata only. Raw packets, commits, coordinates and reconnect tokens never enter logs.</summary>
public static class SafeProtocolLog
{
    public static string TypeOf(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            return document.RootElement.TryGetProperty("type", out var type) && type.ValueKind == JsonValueKind.String
                ? type.GetString() ?? "invalid" : "invalid";
        }
        catch (JsonException) { return "invalid"; }
    }

    public static object Record(string connectionId, string type, string? error = null) => new
    {
        connection = connectionId.Length <= 8 ? connectionId : connectionId[..8],
        type,
        error
    };
}
