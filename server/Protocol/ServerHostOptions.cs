namespace HitMe.Server.Protocol;

public static class ServerHostOptions
{
    public static string? UrlForPort(string? port)
    {
        return int.TryParse(port, out var value) && value is > 0 and <= 65535
            ? $"http://0.0.0.0:{value}"
            : null;
    }
}
