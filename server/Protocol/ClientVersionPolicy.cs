using System.Text.Json;

namespace HitMe.Server.Protocol;

public sealed class ClientVersionPolicy
{
    public string ServerVersion { get; }
    public string MinClientVersion { get; }

    public ClientVersionPolicy(string serverVersion, string minClientVersion)
    {
        if (!TryParts(serverVersion, out _) || !TryParts(minClientVersion, out _))
            throw new ArgumentException("Invalid server/client version configuration.");
        ServerVersion = serverVersion;
        MinClientVersion = minClientVersion;
    }

    public static ClientVersionPolicy FromEnvironment(Func<string, string?> read) =>
        new(read("SERVER_VERSION") ?? "0.1.0-beta.1", read("MIN_CLIENT_VERSION") ?? "0.0.0");

    public string Hello() => JsonSerializer.Serialize(new
    {
        v = ProtocolGateway.LatestVersion, type = "hello", serverVersion = ServerVersion,
        minClientVersion = MinClientVersion
    });

    public bool IsSupported(string? clientVersion)
    {
        if (!TryParts(clientVersion ?? "0.0.0", out var client) || !TryParts(MinClientVersion, out var minimum)) return false;
        for (var i = 0; i < 3; i++)
        {
            if (client[i] > minimum[i]) return true;
            if (client[i] < minimum[i]) return false;
        }
        return true;
    }

    private static bool TryParts(string input, out int[] values)
    {
        values = new int[3];
        var core = input.Split('-', 2)[0].Split('.');
        if (core.Length != 3) return false;
        for (var i = 0; i < 3; i++)
            if (!int.TryParse(core[i], out values[i]) || values[i] < 0) return false;
        return true;
    }
}
