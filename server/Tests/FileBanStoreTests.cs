using System;
using System.IO;
using HitMe.Server.Authority;
using HitMe.Server.Protocol;
using System.Text.Json;
using Xunit;

public sealed class FileBanStoreTests
{
    [Fact]
    public void BanSurvivesStoreRestartThenExpiresAtExactlyTwoMinutes()
    {
        var path = Path.Combine(Path.GetTempPath(), "hitme-ban-test-" + Guid.NewGuid().ToString("N") + ".json");
        var now = new DateTimeOffset(2026, 11, 2, 0, 0, 0, TimeSpan.Zero);
        try
        {
            var first = new FileBanStore(path);
            first.Ban("player", now + TimeSpan.FromSeconds(120));

            var restarted = new FileBanStore(path);
            Assert.True(restarted.TryGetRemaining("player", now + TimeSpan.FromSeconds(20), out var remaining));
            Assert.Equal(100, remaining);
            var gateway = new ProtocolGateway(new RoomDirectory(banStore: restarted),
                new FixedClock(now + TimeSpan.FromSeconds(20)), new PlayerAuth());
            using (var blocked = JsonDocument.Parse(gateway.Handle("c1", "{\"v\":1,\"type\":\"join\",\"token\":\"test:player\"}")))
                Assert.Equal("banned", blocked.RootElement.GetProperty("code").GetString());
            Assert.False(restarted.TryGetRemaining("player", now + TimeSpan.FromSeconds(120), out remaining));
            Assert.Equal(0, remaining);
            Assert.False(new FileBanStore(path).TryGetRemaining("player", now + TimeSpan.FromSeconds(121), out _));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(path + ".tmp")) File.Delete(path + ".tmp");
        }
    }

    private sealed class FixedClock : TimeProvider
    {
        private readonly DateTimeOffset now;
        public FixedClock(DateTimeOffset now) => this.now = now;
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class PlayerAuth : IAuthVerifier
    {
        public bool TryVerify(string? token, out PlayerIdentity identity, out string error)
        {
            if (token == "test:player") { identity = new PlayerIdentity("player", "Player"); error = ""; return true; }
            identity = null!; error = "auth_invalid"; return false;
        }
    }
}
