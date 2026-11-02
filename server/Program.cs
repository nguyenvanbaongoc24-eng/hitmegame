using System.Net.WebSockets;
using System.Text;
using System.Threading.RateLimiting;
using HitMe.Server.Authority;
using HitMe.Server.Protocol;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRateLimiter(options => options.AddPolicy("guest-token", context =>
    RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ =>
        new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })));
var runtime = ServerRuntimeOptions.FromEnvironment(Environment.GetEnvironmentVariable);
var listenUrl = ServerHostOptions.UrlForPort(Environment.GetEnvironmentVariable("PORT"));
if (listenUrl is not null) builder.WebHost.UseUrls(listenUrl);
builder.Services.AddSingleton<IMatchResultStore, InMemoryMatchResultStore>();
builder.Services.AddSingleton<IAuthVerifier>(_ =>
{
    var encoded = Environment.GetEnvironmentVariable("GUEST_TOKEN_SIGNING_KEY");
    return string.IsNullOrWhiteSpace(encoded)
        ? new FakeAuthVerifier()
        : new SignedGuestAuthVerifier(Convert.FromBase64String(encoded));
});
builder.Services.AddSingleton<IBanStore>(_ =>
{
    var url = Environment.GetEnvironmentVariable("SUPABASE_URL");
    var key = Environment.GetEnvironmentVariable("SUPABASE_SECRET_KEY")
        ?? Environment.GetEnvironmentVariable("SUPABASE_SERVICE_KEY");
    if (!string.IsNullOrWhiteSpace(url) || !string.IsNullOrWhiteSpace(key))
    {
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("Both SUPABASE_URL and server-only secret key are required for shared bans.");
        return new SupabaseBanStore(new Uri(url), key);
    }
    var path = Environment.GetEnvironmentVariable("BAN_STORE_PATH");
    return string.IsNullOrWhiteSpace(path) ? new InMemoryBanStore() : new FileBanStore(path);
});
builder.Services.AddSingleton(provider => new RoomDirectory(maxRooms: runtime.MaxRooms,
    banStore: provider.GetRequiredService<IBanStore>(),
    resultStore: provider.GetRequiredService<IMatchResultStore>()));
builder.Services.AddSingleton<ProtocolGateway>();
builder.Services.AddSingleton<ServerMetrics>();
builder.Services.AddSingleton<WebSocketConnectionRegistry>();
builder.Services.AddSingleton(runtime);
builder.Services.AddSingleton(new ConnectionAdmission(runtime.MaxConnectionsPerIp,
    runtime.AllowedOrigins, runtime.MaxConnections));
builder.Services.AddHostedService<RoundDeadlineService>();
builder.Services.AddHostedService<ShutdownNotifier>();
var app = builder.Build();
app.UseRateLimiter();
app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = runtime.KeepAliveInterval });
app.MapGet("/health", () => Results.Ok(new { status = "ok", protocol = 1 }));
app.MapGet("/healthz", () => Results.Ok(new { status = "ok", protocol = 1 }));
app.MapPost("/guest-token", (GuestTokenRequest request, IAuthVerifier verifier) =>
{
    if (verifier is not SignedGuestAuthVerifier signed) return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    try { return Results.Ok(new { token = signed.Issue(request.Name) }); }
    catch (ArgumentException) { return Results.BadRequest(new { code = "invalid_guest_name" }); }
}).RequireRateLimiting("guest-token");
app.MapGet("/status", (RoomDirectory rooms, WebSocketConnectionRegistry connections) =>
    Results.Ok(new { status = "ok", protocol = ProtocolGateway.Version,
        rooms = rooms.Snapshot().Count, connections = connections.Count }));
app.MapGet("/metrics", (RoomDirectory rooms, WebSocketConnectionRegistry connections, ServerMetrics metrics) =>
{
    var snapshot = rooms.Snapshot();
    return Results.Ok(metrics.Snapshot(connections.Count, snapshot.Count,
        snapshot.Count(room => room.RoundSession is { } active && active.Match.State.Phase != HitMe.Core.Phase.MatchEnd)));
});
app.Map("/ws", async context => {
    if (!context.WebSockets.IsWebSocketRequest) { context.Response.StatusCode = 400; return; }
    var admission = context.RequestServices.GetRequiredService<ConnectionAdmission>();
    var origin = context.Request.Headers.Origin.ToString();
    if (!admission.IsAllowedOrigin(origin)) { context.Response.StatusCode = StatusCodes.Status403Forbidden; await context.Response.WriteAsJsonAsync(new { v = 1, type = "error", code = "origin_not_allowed" }); return; }
    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    if (!admission.TryAdmit(ip, out var admissionError)) { context.Response.StatusCode = StatusCodes.Status429TooManyRequests; await context.Response.WriteAsJsonAsync(new { v = 1, type = "error", code = admissionError }); return; }
    using var socket = await context.WebSockets.AcceptWebSocketAsync();
    var gateway = context.RequestServices.GetRequiredService<ProtocolGateway>();
    var connections = context.RequestServices.GetRequiredService<WebSocketConnectionRegistry>();
    var connectionId = Guid.NewGuid().ToString("N");
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("HitMe.WebSocket");
    var metrics = context.RequestServices.GetRequiredService<ServerMetrics>();
    connections.Register(connectionId, socket);
    try
    {
        while (socket.State == WebSocketState.Open)
        {
            string? raw;
            try { raw = await WebSocketMessageReader.ReceiveTextAsync(socket, context.RequestAborted); }
            catch (WebSocketException) { await socket.CloseOutputAsync(WebSocketCloseStatus.MessageTooBig, "message_too_large", CancellationToken.None); break; }
            if (raw is null) { await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", context.RequestAborted); break; }
            connections.Touch(connectionId);
            var type = SafeProtocolLog.TypeOf(raw);
            logger.LogInformation("{@ProtocolMessage}", SafeProtocolLog.Record(connectionId, type));
            try
            {
                var replies = gateway.HandleMessages(connectionId, raw);
                foreach (var reply in replies)
                    if (reply.Payload.Contains("\"type\":\"error\"", StringComparison.Ordinal))
                    {
                        using var parsed = System.Text.Json.JsonDocument.Parse(reply.Payload);
                        metrics.RecordError(parsed.RootElement.GetProperty("code").GetString() ?? "unknown");
                    }
                await Task.WhenAll(replies.Select(reply => connections.SendAsync(reply, context.RequestAborted)));
                foreach (var reply in replies.Where(reply => reply.CloseReason is not null))
                    await connections.CloseConnectionAsync(reply.ConnectionId, reply.CloseReason!);
            }
            catch (Exception exception)
            {
                // Keep raw packets and tokens out of diagnostics while making a
                // production transport failure observable to the operator.
                logger.LogError(exception, "Protocol processing failed for {Connection}", connectionId[..8]);
                break;
            }
        }
    }
    finally
    {
        gateway.Disconnect(connectionId);
        connections.Remove(connectionId);
        admission.Release(ip);
    }
});
app.Run();

public sealed record GuestTokenRequest(string Name);
