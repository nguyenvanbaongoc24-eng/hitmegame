using System;
using HitMe.Server.Authority;
using Xunit;
using Xunit.Abstractions;

public sealed class RoomLifecycleTests
{
    private readonly ITestOutputHelper output;
    public RoomLifecycleTests(ITestOutputHelper output) => this.output = output;

    [Fact]
    public void OneThousandCreateLeaveCyclesReleaseAllRoomsAndMemory()
    {
        var directory = new RoomDirectory(attempt => $"R{attempt:00000}");
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        var before = GC.GetTotalMemory(true);
        for (var i = 0; i < 1000; i++)
        {
            var room = directory.Create($"player-{i}", "Player", null, out var error);
            Assert.Equal(string.Empty, error);
            Assert.True(directory.Leave(room.Code, $"player-{i}", out _, out error));
            Assert.Equal(string.Empty, error);
        }
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        var after = GC.GetTotalMemory(true);
        output.WriteLine($"1000 rooms: before={before} bytes, after={after} bytes, delta={after - before} bytes");
        Assert.Empty(directory.Snapshot());
        Assert.True(after - before < 10_000_000, "The room directory retained excessive memory after 1000 leaves.");
    }
}
