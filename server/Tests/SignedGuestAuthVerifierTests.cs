using System;
using System.Linq;
using HitMe.Server.Protocol;
using HitMe.Server.Authority;
using System.Text.Json;
using Xunit;

public sealed class SignedGuestAuthVerifierTests
{
    [Fact]
    public void IssuedGuestIdentitySurvivesVerifierRestartAndRejectsTampering()
    {
        var key = Enumerable.Range(0, 32).Select(i => (byte)i).ToArray();
        var first = new SignedGuestAuthVerifier(key);
        var token = first.Issue("Nam");
        Assert.True(new SignedGuestAuthVerifier(key).TryVerify(token, out var identity, out var error), error);
        Assert.StartsWith("g_", identity.PlayerId);
        Assert.Equal("Nam", identity.DisplayName);
        var changed = token.Replace("guest1.", "guest2.", StringComparison.Ordinal);
        Assert.False(first.TryVerify(changed, out _, out _));
        Assert.False(first.TryVerify("guest:Nam", out _, out _));
        Assert.False(new SignedGuestAuthVerifier(Enumerable.Repeat((byte)99, 32).ToArray()).TryVerify(token, out _, out _));
    }

    [Fact]
    public void ExpiredAndMalformedTokensAreRejected()
    {
        var key = Enumerable.Repeat((byte)7, 32).ToArray();
        var clock = new TestClock(new DateTimeOffset(2026, 11, 2, 0, 0, 0, TimeSpan.Zero));
        var issuer = new SignedGuestAuthVerifier(key, clock);
        var token = issuer.Issue("Nữ", TimeSpan.FromSeconds(1));
        Assert.False(issuer.TryVerify("guest1.bad.!", out _, out _));
        Assert.False(issuer.TryVerify(new string('x', 513), out _, out _));
        Assert.False(issuer.TryVerify(null, out _, out var missing));
        Assert.Equal("auth_required", missing);
        Assert.True(issuer.TryVerify(token, out _, out _));
        clock.Advance(TimeSpan.FromSeconds(2));
        Assert.False(issuer.TryVerify(token, out _, out _));
    }

    private sealed class TestClock : TimeProvider
    {
        private DateTimeOffset now;
        public TestClock(DateTimeOffset now) => this.now = now;
        public void Advance(TimeSpan amount) => now += amount;
        public override DateTimeOffset GetUtcNow() => now;
    }

    [Fact]
    public void GatewayAcceptsSignedTokenAndRejectsForgedGuest()
    {
        var verifier = new SignedGuestAuthVerifier(Enumerable.Repeat((byte)19, 32).ToArray());
        var gateway = new ProtocolGateway(new RoomDirectory(), auth: verifier);
        var token = verifier.Issue("Nam");
        var accepted = gateway.Handle("signed", JsonSerializer.Serialize(new { v = 1, type = "join", token }));
        Assert.Contains("\"type\":\"room_state\"", accepted);
        var rejected = gateway.Handle("forged", "{\"v\":1,\"type\":\"join\",\"token\":\"guest:Nam\"}");
        Assert.Equal("auth_invalid", JsonDocument.Parse(rejected).RootElement.GetProperty("code").GetString());
    }
}
