using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HitMe.Core;
using HitMe.Server.Authority;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Real Kestrel + ClientWebSocket regression.  This deliberately bypasses the
/// in-process gateway so a Phase 4 client can rely on the wire contract.
/// </summary>
public sealed class WebSocketEndToEndTests
{
    private readonly ITestOutputHelper output;
    public WebSocketEndToEndTests(ITestOutputHelper output) => this.output = output;

    [Fact]
    public async Task TwentyMixedRosterMatchesOverRealWebSocketsMatchCoreAndRecoverFromFaults()
    {
        var port = ReservePort();
        await using var host = await RunningServer.StartAsync(port);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(12));
        var samples = new List<double>();

        // A legacy client cannot silently enter a room without an identity token.
        await using (var legacy = await LiveBot.ConnectAsync(port, "guest:legacy", cancellation.Token))
        {
            var rejected = await legacy.SendAndReceiveAsync(new { v = 1, type = "join" }, "error", cancellation.Token);
            Assert.Equal("auth_required_use_join_token", rejected.GetProperty("code").GetString());
        }

        await using (var alone = await LiveBot.ConnectAsync(port, "guest:alone", cancellation.Token))
        {
            await alone.JoinAsync(null, null, cancellation.Token);
            var rejected = await alone.SendAndReceiveAsync(new { v = 1, type = "start" }, "error", cancellation.Token);
            Assert.Equal("not_enough_players", rejected.GetProperty("code").GetString());
        }

        for (var matchNumber = 1; matchNumber <= 20; matchNumber++)
        {
            var clients = new List<LiveBot>();
            try
            {
                var playerCount = 2 + ((matchNumber - 1) % 5);
                for (var index = 0; index < playerCount; index++)
                    clients.Add(await LiveBot.ConnectAsync(port, $"guest:z1_{matchNumber}_{index}", cancellation.Token));

                var hostState = await clients[0].JoinAsync(null, null, cancellation.Token);
                var roomCode = hostState.GetProperty("roomCode").GetString();
                Assert.False(string.IsNullOrWhiteSpace(roomCode));
                for (var index = 1; index < clients.Count; index++)
                    await clients[index].JoinAsync(roomCode, null, cancellation.Token);

                // The host starts immediately; every client must receive the same first round.
                await clients[0].SendAsync(new { v = 1, type = "start" }, cancellation.Token);
                for (var index = 0; index < clients.Count; index++)
                    Assert.Equal(1, (await clients[index].ReceiveTypeAsync("round_start", cancellation.Token)).GetProperty("roundId").GetInt32());

                var ids = clients.Select(client => client.PlayerId).ToArray();
                var direct = new ServerMatch(new GameConfig(), ids);
                for (var round = 1; round <= 5; round++)
                {
                    var directRound = direct.BeginRound();
                    for (var index = 0; index < clients.Count; index++)
                    {
                        // Match 1 injects malformed JSON, and every second round leaves
                        // one player uncommitted; both must remain safe and deterministic.
                        if (matchNumber == 4 && round == 1 && index == 2)
                        {
                            var invalid = await clients[index].SendAndReceiveAsync("{\"v\":1,\"type\":\"commit\",\"positionX\":\"bad\"}", "error", cancellation.Token);
                            Assert.Equal("invalid_json", invalid.GetProperty("code").GetString());
                        }
                        if (round % 2 == 0 && index == clients.Count - 1) continue;

                        var position = PositionFor(index);
                        var aim = AimOutward(index);
                        var commit = new RoundCommit(directRound.RoundId, position, aim);
                        Assert.True(directRound.TryCommit(ids[index], commit, out var directError), directError);
                        await clients[index].SendAsync(new { v = 1, type = "commit", roundId = round, positionX = position.X, positionY = position.Y, aimX = aim.X, aimY = aim.Y }, cancellation.Token);
                        Assert.Equal("room_state", (await clients[index].ReceiveTypeAsync("room_state", cancellation.Token)).GetProperty("type").GetString());
                    }

                    var lockStart = Stopwatch.GetTimestamp();
                    for (var index = 0; index < clients.Count - 1; index++)
                    {
                        Assert.True(directRound.TryMarkReady(ids[index], out var directReadyError), directReadyError);
                        await clients[index].SendAsync(new { v = 1, type = "lock_ready", ready = true }, cancellation.Token);
                    }
                    // Begin receiving before the final lock broadcasts.  This is also a
                    // real-client assertion that a normal six-client fan-out is never
                    // classified as a slow socket by the server's write queue.
                    var resultTasks = clients.Select(async client =>
                    {
                        var reveal = await client.ReceiveTypeAsync("reveal", cancellation.Token, host.Diagnostics);
                        Assert.Equal(round, reveal.GetProperty("roundId").GetInt32());
                        return await client.ReceiveTypeAsync("round_result", cancellation.Token, host.Diagnostics);
                    }).ToArray();
                    Assert.True(directRound.TryMarkReady(ids[^1], out var finalReadyError), finalReadyError);
                    await clients[^1].SendAsync(new { v = 1, type = "lock_ready", ready = true }, cancellation.Token);
                    var expected = direct.ResolveLockedRound();
                    foreach (var result in await Task.WhenAll(resultTasks)) AssertRoundMatchesCore(result, expected, direct);
                    samples.Add(Stopwatch.GetElapsedTime(lockStart).TotalMilliseconds);

                    if (direct.State.Phase == Phase.MatchEnd)
                    {
                        foreach (var client in clients)
                        {
                            var end = await client.ReceiveTypeAsync("match_end", cancellation.Token, host.Diagnostics);
                            Assert.Equal(direct.State.WinnerIds.OrderBy(id => id), end.GetProperty("winners").EnumerateArray().Select(item => item.GetString()).OrderBy(id => id));
                        }
                        break;
                    }

                    foreach (var client in clients)
                    {
                        var start = await client.ReceiveTypeAsync("round_start", cancellation.Token, host.Diagnostics);
                        Assert.Equal(round + 1, start.GetProperty("roundId").GetInt32());
                    }

                    // One genuine disconnect/reconnect occurs while a game is live.
                    if (matchNumber == 3 && round == 1)
                    {
                        var prior = clients[3];
                        var reconnectToken = prior.ReconnectToken;
                        await prior.DisposeAsync();
                        var replacement = await LiveBot.ConnectAsync(port, prior.Token, cancellation.Token);
                        JsonElement state = default;
                        for (var attempt = 0; attempt < 20; attempt++)
                        {
                            state = await replacement.JoinAsync(roomCode, reconnectToken, cancellation.Token);
                            if (state.GetProperty("type").GetString() == "room_state") break;
                            await Task.Delay(25, cancellation.Token);
                        }
                        Assert.Equal("room_state", state.GetProperty("type").GetString());
                        clients[3] = replacement;
                    }
                }
                Assert.Equal(Phase.MatchEnd, direct.State.Phase);
            }
            finally
            {
                foreach (var client in clients) await client.DisposeAsync();
            }
        }

        Assert.Equal(100, samples.Count);
        output.WriteLine($"Z1 20 matches x 5 rounds: average {samples.Average():F2} ms, max {samples.Max():F2} ms per lock/broadcast.");
    }

    private static void AssertRoundMatchesCore(JsonElement result, RoundResolution expected, ServerMatch direct)
    {
        var hits = result.GetProperty("hits").EnumerateArray().Select(hit => (hit.GetProperty("shooterId").GetString()!, hit.GetProperty("targetId").GetString()!)).OrderBy(hit => hit.Item1).ThenBy(hit => hit.Item2);
        Assert.Equal(expected.Hits.Select(hit => (hit.ShooterId, hit.TargetId)).OrderBy(hit => hit.Item1).ThenBy(hit => hit.Item2), hits);
        foreach (var player in result.GetProperty("players").EnumerateArray())
        {
            var expectedPlayer = direct.State.FindPlayer(player.GetProperty("id").GetString()!);
            Assert.NotNull(expectedPlayer);
            Assert.Equal(expectedPlayer!.Hp, player.GetProperty("hp").GetInt32());
            Assert.Equal(!expectedPlayer.IsAlive, player.GetProperty("eliminated").GetBoolean());
        }
    }

    private static Vec2 PositionFor(int index) => new(-4f + (index % 3) * 4f, -3f + (index / 3) * 6f);
    private static Vec2 AimOutward(int index) => index switch
    {
        0 => new Vec2(-1, -1), 1 => new Vec2(0, -1), 2 => new Vec2(1, -1),
        3 => new Vec2(-1, 1), 4 => new Vec2(0, 1), _ => new Vec2(1, 1)
    };

    private static int ReservePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private sealed class LiveBot : IAsyncDisposable
    {
        private readonly ClientWebSocket socket = new();
        private readonly Uri uri;
        private LiveBot(Uri uri, string token) { this.uri = uri; Token = token; }
        public string Token { get; }
        public string PlayerId { get; private set; } = string.Empty;
        public string ReconnectToken { get; private set; } = string.Empty;

        public static async Task<LiveBot> ConnectAsync(int port, string token, CancellationToken cancellationToken)
        {
            var bot = new LiveBot(new Uri($"ws://127.0.0.1:{port}/ws"), token);
            await bot.socket.ConnectAsync(bot.uri, cancellationToken);
            return bot;
        }

        public async Task<JsonElement> JoinAsync(string? roomCode, string? reconnectToken, CancellationToken cancellationToken)
        {
            await SendAsync(new { v = 1, type = "join", token = Token, roomCode, reconnectToken }, cancellationToken);
            var response = await ReceiveAsync(cancellationToken);
            if (response.GetProperty("type").GetString() == "room_state")
            {
                ReconnectToken = response.GetProperty("reconnectToken").GetString() ?? string.Empty;
                PlayerId = response.GetProperty("players").EnumerateArray().Single(player => player.GetProperty("name").GetString() == Token["guest:".Length..]).GetProperty("id").GetString()!;
            }
            return response;
        }

        public async Task SendAsync(object payload, CancellationToken cancellationToken) => await SendAsync(JsonSerializer.Serialize(payload), cancellationToken);
        public async Task SendAsync(string payload, CancellationToken cancellationToken) => await socket.SendAsync(Encoding.UTF8.GetBytes(payload), WebSocketMessageType.Text, true, cancellationToken);
        public async Task<JsonElement> SendAndReceiveAsync(object payload, string type, CancellationToken cancellationToken) { await SendAsync(payload, cancellationToken); return await ReceiveTypeAsync(type, cancellationToken); }
        public async Task<JsonElement> SendAndReceiveAsync(string payload, string type, CancellationToken cancellationToken) { await SendAsync(payload, cancellationToken); return await ReceiveTypeAsync(type, cancellationToken); }

        public async Task<JsonElement> ReceiveTypeAsync(string type, CancellationToken cancellationToken, string? diagnostics = null)
        {
            while (true)
            {
                var message = await ReceiveAsync(cancellationToken, diagnostics);
                if (message.GetProperty("type").GetString() == type) return message;
            }
        }

        private async Task<JsonElement> ReceiveAsync(CancellationToken cancellationToken, string? diagnostics = null)
        {
            var bytes = new byte[4096];
            WebSocketReceiveResult result;
            try { result = await socket.ReceiveAsync(bytes, cancellationToken); }
            catch (WebSocketException error) { throw new InvalidOperationException($"WebSocket receive failed. Server diagnostics: {diagnostics}", error); }
            if (result.MessageType == WebSocketMessageType.Close) throw new InvalidOperationException("Server closed WebSocket during Z1.");
            return JsonDocument.Parse(Encoding.UTF8.GetString(bytes, 0, result.Count)).RootElement.Clone();
        }

        public async ValueTask DisposeAsync()
        {
            try { if (socket.State == WebSocketState.Open) await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "test_done", CancellationToken.None); }
            catch (WebSocketException) { socket.Abort(); }
            socket.Dispose();
        }
    }

    private sealed class RunningServer : IAsyncDisposable
    {
        private readonly Process process;
        private readonly List<string> diagnostics = new();
        private RunningServer(Process process)
        {
            this.process = process;
            process.OutputDataReceived += (_, eventArgs) => { if (!string.IsNullOrEmpty(eventArgs.Data)) lock (diagnostics) diagnostics.Add(eventArgs.Data); };
            process.ErrorDataReceived += (_, eventArgs) => { if (!string.IsNullOrEmpty(eventArgs.Data)) lock (diagnostics) diagnostics.Add(eventArgs.Data); };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        public string Diagnostics { get { lock (diagnostics) return string.Join(" | ", diagnostics); } }

        public static async Task<RunningServer> StartAsync(int port)
        {
            var dll = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "bin", "Debug", "net8.0", "HitMe.Server.dll"));
            Assert.True(File.Exists(dll), $"Missing server executable: {dll}");
            var start = new ProcessStartInfo("dotnet") { UseShellExecute = false, RedirectStandardError = true, RedirectStandardOutput = true };
            start.ArgumentList.Add(dll);
            start.Environment["PORT"] = port.ToString();
            var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start Kestrel.");
            using var http = new HttpClient();
            for (var attempt = 0; attempt < 80; attempt++)
            {
                try { if ((await http.GetAsync($"http://127.0.0.1:{port}/healthz")).IsSuccessStatusCode) return new RunningServer(process); }
                catch (HttpRequestException) { }
                await Task.Delay(50);
            }
            process.Kill(entireProcessTree: true);
            throw new InvalidOperationException("Kestrel did not pass /healthz within four seconds.");
        }

        public ValueTask DisposeAsync()
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            process.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
