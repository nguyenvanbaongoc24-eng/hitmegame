using HitMe.Server.Protocol;
using Xunit;

public sealed class ServerHostOptionsTests
{
    [Theory]
    [InlineData("8080", "http://0.0.0.0:8080")]
    [InlineData("1", "http://0.0.0.0:1")]
    [InlineData("65535", "http://0.0.0.0:65535")]
    public void ValidPortProducesListenUrl(string port, string expected) => Assert.Equal(expected, ServerHostOptions.UrlForPort(port));

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("65536")]
    [InlineData("bad")]
    public void InvalidPortUsesHostDefault(string? port) => Assert.Null(ServerHostOptions.UrlForPort(port));
}
