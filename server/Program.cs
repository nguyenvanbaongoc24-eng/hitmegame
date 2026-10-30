using System.Net.WebSockets;
using System.Text;
using HitMe.Server.Authority;
using HitMe.Server.Protocol;

var builder = WebApplication.CreateBuilder(args);
var listenUrl = ServerHostOptions.UrlForPort(Environment.GetEnvironmentVariable("PORT"));
if (listenUrl is not null) builder.WebHost.UseUrls(listenUrl);
builder.Services.AddSingleton<RoomDirectory>();
builder.Services.AddSingleton<ProtocolGateway>();
builder.Services.AddSingleton<WebSocketConnectionRegistry>();
builder.Services.AddSingleton<ConnectionAdmission>();
builder.Services.AddHostedService<RoundDeadlineService>();
var app = builder.Build();
app.UseWebSockets();
app.MapGet("/health", () => Results.Ok(new { status = "ok", protocol = 1 }));
app.MapGet("/healthz", () => Results.Ok(new { status = "ok", protocol = 1 }));
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
    connections.Register(connectionId, socket);
    try
    {
        while (socket.State == WebSocketState.Open)
        {
            string? raw;
            try { raw = await WebSocketMessageReader.ReceiveTextAsync(socket, context.RequestAborted); }
            catch (WebSocketException) { await socket.CloseOutputAsync(WebSocketCloseStatus.MessageTooBig, "message_too_large", CancellationToken.None); break; }
            if (raw is null) { await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", context.RequestAborted); break; }
            var replies = gateway.HandleMessages(connectionId, raw);
            await Task.WhenAll(replies.Select(reply => connections.SendAsync(reply, context.RequestAborted)));
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
