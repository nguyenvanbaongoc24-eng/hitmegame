using HitMe.Server.Protocol;
using Xunit;

public sealed class FakeAuthVerifierTests
{
    [Theory]
    [InlineData(null, "auth_required")]
    [InlineData("", "auth_required")]
    [InlineData("bearer:forged", "auth_invalid")]
    [InlineData("guest:expired:Mai", "auth_invalid")]
    public void MissingOrInvalidTokenIsRejected(string? token, string error)
    {
        var verifier = new FakeAuthVerifier();
        Assert.False(verifier.TryVerify(token, out _, out var actual));
        Assert.Equal(error, actual);
    }

    [Fact]
    public void GuestTokenReceivesStableServerIssuedOpaqueIdAndSanitizedName()
    {
        var verifier = new FakeAuthVerifier();
        Assert.True(verifier.TryVerify("guest:  Mai\nHồng  ", out var first, out _));
        Assert.True(verifier.TryVerify("guest:  Mai\nHồng  ", out var second, out _));
        Assert.Equal(first.PlayerId, second.PlayerId);
        Assert.StartsWith("g_", first.PlayerId);
        Assert.Equal(34, first.PlayerId.Length); // prefix + 128 random bits in hex
        Assert.Equal("MaiHồng", first.DisplayName);
    }

    [Fact]
    public void DifferentGuestTokensCannotChooseTheSamePlayerId()
    {
        var verifier = new FakeAuthVerifier();
        verifier.TryVerify("guest:One", out var one, out _);
        verifier.TryVerify("guest:Two", out var two, out _);
        Assert.NotEqual(one.PlayerId, two.PlayerId);
    }
}
