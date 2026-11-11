using HitMe.Server.Protocol;
using Xunit;

public sealed class OriginPolicyTests
{
    [Theory]
    [InlineData("https://hitmegame-iota.vercel.app", true)]
    [InlineData("HTTPS://HITMEGAME-IOTA.VERCEL.APP/", true)]
    [InlineData("https://hitmegame-iota.vercel.app:443", true)]
    [InlineData("http://hitmegame-iota.vercel.app", false)]
    [InlineData("https://hitmegame-iota.vercel.app:8443", false)]
    [InlineData("https://foreign.example", false)]
    [InlineData("", false)]
    public void ExactPolicyNormalizesOnlyEquivalentBrowserOrigins(string origin, bool expected)
    {
        var policy = new OriginPolicy(new[] { "https://hitmegame-iota.vercel.app" });
        Assert.Equal(expected, policy.IsAllowed(origin));
    }

    [Fact]
    public void EmptyConfigurationAndMissingOriginAreDeniedByDefault()
    {
        var policy = new OriginPolicy();
        Assert.False(policy.OriginsConfigured);
        Assert.False(policy.IsAllowed(null));
        Assert.False(policy.IsAllowed("https://hitmegame-iota.vercel.app"));
    }

    [Fact]
    public void OptionalPreviewAndNonBrowserFlagsAreOptIn()
    {
        var strict = new OriginPolicy(new[] { "https://hitmegame-iota.vercel.app" });
        Assert.False(strict.IsAllowed("https://hitmegame-preview-123.vercel.app"));
        var enabled = new OriginPolicy(null, allowMissingOrigin: true, allowVercelPreviewOrigins: true);
        Assert.True(enabled.IsAllowed("https://hitmegame-preview-123.vercel.app"));
        Assert.False(enabled.IsAllowed("https://other-preview-123.vercel.app"));
        Assert.True(enabled.IsAllowed(null));
    }

    [Fact]
    public void DrainStateStartsLiveAndBecomesDraining()
    {
        var state = new ServerDrainState();
        Assert.False(state.IsDraining);
        state.BeginDraining();
        Assert.True(state.IsDraining);
    }
}
