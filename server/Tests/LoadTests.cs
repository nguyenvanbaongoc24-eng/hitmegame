using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HitMe.Core;
using HitMe.Server.Authority;
using Xunit;
using Xunit.Abstractions;

public sealed class LoadTests
{
    private const int RoomCount = 50;
    private const int PlayersPerRoom = 6;
    private const int RoundsPerRoom = 5;
    private readonly ITestOutputHelper output;

    public LoadTests(ITestOutputHelper output) => this.output = output;

    [Fact]
    public async Task FiftySixPlayerRoomsCompleteFiveRoundsWithoutDropsOrExceptions()
    {
        using var process = Process.GetCurrentProcess();
        var cpuBefore = process.TotalProcessorTime;
        var peakBefore = process.PeakWorkingSet64;
        var rooms = new ConcurrentBag<RoomMeasurement>();
        var clock = Stopwatch.StartNew();

        await Task.WhenAll(Enumerable.Range(0, RoomCount).Select(roomId => Task.Run(() =>
        {
            var measurement = RunRoom(roomId);
            rooms.Add(measurement);
        }))).WaitAsync(TimeSpan.FromSeconds(30));

        clock.Stop();
        process.Refresh();
        var measurements = rooms.ToArray();
        Assert.Equal(RoomCount, measurements.Length);
        Assert.All(measurements, room =>
        {
            Assert.Equal(RoundsPerRoom, room.Rounds);
            Assert.Equal(0, room.DroppedConnections);
            Assert.True(room.BroadcastMessages > 0);
            Assert.True(room.RoundDurations.All(duration => duration >= TimeSpan.Zero));
        });

        var roundDurations = measurements.SelectMany(room => room.RoundDurations).OrderBy(value => value).ToArray();
        var average = TimeSpan.FromTicks((long)roundDurations.Average(value => value.Ticks));
        var p95 = roundDurations[(int)Math.Ceiling(roundDurations.Length * 0.95) - 1];
        var maximum = roundDurations[^1];
        var cpu = process.TotalProcessorTime - cpuBefore;
        var peak = Math.Max(peakBefore, process.PeakWorkingSet64);
        output.WriteLine(
            "Z3 local in-process: rooms={0}; players={1}; rounds={2}; elapsedMs={3:F1}; cpuMs={4:F1}; peakMiB={5:F2}; roundAvgMs={6:F3}; roundP95Ms={7:F3}; roundMaxMs={8:F3}; broadcastMessages={9}; droppedConnections=0",
            RoomCount, RoomCount * PlayersPerRoom, RoomCount * RoundsPerRoom,
            clock.Elapsed.TotalMilliseconds, cpu.TotalMilliseconds, peak / 1024d / 1024d,
            average.TotalMilliseconds, p95.TotalMilliseconds, maximum.TotalMilliseconds,
            measurements.Sum(room => room.BroadcastMessages));
    }

    private static RoomMeasurement RunRoom(int roomId)
    {
        var players = Enumerable.Range(0, PlayersPerRoom).Select(index => $"load_{roomId}_{index}").ToArray();
        var session = new ServerRoundSession(new GameConfig
        {
            MaxPlayers = PlayersPerRoom,
            MaxRounds = RoundsPerRoom,
            RoundLimitTiebreak = RoundLimitTiebreakPolicy.HighestHp,
            Seed = (uint)(1000 + roomId)
        }, players);
        session.BeginRound();
        var durations = new List<TimeSpan>();
        var broadcastMessages = PlayersPerRoom; // Initial round_start.

        for (var round = 0; round < RoundsPerRoom; round++)
        {
            var start = Stopwatch.GetTimestamp();
            for (var index = 0; index < players.Length; index++)
            {
                var position = Placement(index);
                var aim = position.Normalized;
                Assert.True(session.TryCommit(players[index], new RoundCommit(session.ActiveRound.RoundId, position, aim), out var error), error);
            }

            IReadOnlyList<OutboundMessage> messages = Array.Empty<OutboundMessage>();
            foreach (var player in players) messages = session.TryMarkReady(player, out _);
            durations.Add(Stopwatch.GetElapsedTime(start));
            broadcastMessages += messages.Count;
        }

        Assert.Equal(Phase.MatchEnd, session.Match.State.Phase);
        return new RoomMeasurement(RoundsPerRoom, 0, broadcastMessages, durations);
    }

    private static Vec2 Placement(int index) => index switch
    {
        0 => new Vec2(-3f, -4f), 1 => new Vec2(0f, -4f), 2 => new Vec2(3f, -4f),
        3 => new Vec2(-3f, 4f), 4 => new Vec2(0f, 4f), _ => new Vec2(3f, 4f)
    };

    private sealed record RoomMeasurement(int Rounds, int DroppedConnections, int BroadcastMessages, IReadOnlyList<TimeSpan> RoundDurations);
}
