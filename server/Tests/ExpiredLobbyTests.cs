using System;
using HitMe.Server.Authority;
using Xunit;

public sealed class ExpiredLobbyTests
{
    [Fact]
    public void InactiveLobbyExpiresButActiveMatchDoesNot()
    {
        var directory = new RoomDirectory();
        var waiting = directory.Create("host", "Host", null, out _);
        var active = directory.Create("a", "A", null, out _);
        Assert.True(directory.Join(active.Code, "b", "B", out _, out _));
        Assert.True(active.TryHostStart("a", out _));
        Assert.Empty(directory.PruneExpiredLobbies(DateTimeOffset.UtcNow.AddMinutes(9), TimeSpan.FromMinutes(10)));
        var pruned = directory.PruneExpiredLobbies(DateTimeOffset.UtcNow.AddMinutes(11), TimeSpan.FromMinutes(10));
        Assert.Single(pruned);
        Assert.Equal(waiting.Code, pruned[0].Code);
        Assert.False(directory.TryGet(waiting.Code, out _));
        Assert.True(directory.TryGet(active.Code, out _));
    }
}
