using System.Text.Json;

namespace HitMe.Server.Protocol;

public sealed class ClientVersionPolicy
{
    public string ServerVersion { get; }
    public string MinClientVersion { get; }

    public ClientVersionPolicy(string serverVersion, string minClientVersion)
    {
        if (!TryParts(serverVersion, out _, out _) || !TryParts(minClientVersion, out _, out _))
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
        if (!TryParts(clientVersion ?? "0.0.0", out var client, out var clientSuffix) ||
            !TryParts(MinClientVersion, out var minimum, out var minimumSuffix)) return false;
        for (var i = 0; i < 3; i++)
        {
            if (client[i] > minimum[i]) return true;
            if (client[i] < minimum[i]) return false;
        }
        if (clientSuffix.Length == 0) return true;
        if (minimumSuffix.Length == 0) return false;
        var clientIds = clientSuffix.Split('.');
        var minimumIds = minimumSuffix.Split('.');
        for (var i = 0; i < Math.Min(clientIds.Length, minimumIds.Length); i++)
        {
            var leftNumber = int.TryParse(clientIds[i], out var left);
            var rightNumber = int.TryParse(minimumIds[i], out var right);
            var comparison = leftNumber && rightNumber ? left.CompareTo(right)
                : leftNumber ? -1 : rightNumber ? 1 : string.CompareOrdinal(clientIds[i], minimumIds[i]);
            if (comparison != 0) return comparison > 0;
        }
        return clientIds.Length >= minimumIds.Length;
    }

    private static bool TryParts(string input, out int[] values, out string suffix)
    {
        values = new int[3];
        var split = input.Split('-', 2);
        suffix = split.Length == 2 ? split[1] : string.Empty;
        if (split.Length == 2 && (suffix.Length == 0 || suffix.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not ('.' or '-')))) return false;
        var core = split[0].Split('.');
        if (core.Length != 3) return false;
        for (var i = 0; i < 3; i++)
            if (!int.TryParse(core[i], out values[i]) || values[i] < 0) return false;
        return true;
    }
}
