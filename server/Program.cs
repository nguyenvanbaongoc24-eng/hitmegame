using System.Net.WebSockets;
using System.Text;
using HitMe.Server.Authority;
using HitMe.Server.Protocol;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<RoomDirectory>();
builder.Services.AddSingleton<ProtocolGateway>();
builder.Services.AddSingleton<WebSocketConnectionRegistry>();
builder.Services.AddHostedService<RoundDeadlineService>();
var app = builder.Build();
app.UseWebSockets();
app.MapGet("/health", () => Results.Ok(new { status = "ok", protocol = 1 }));
app.Map("/ws", async context => {
    if (!context.WebSockets.IsWebSocketRequest) { context.Response.StatusCode = 400; return; }
    using var socket = await context.WebSockets.AcceptWebSocketAsync();
    var gateway = context.RequestServices.GetRequiredService<ProtocolGateway>();
    var connections = context.RequestServices.GetRequiredService<WebSocketConnectionRegistry>();
    var connectionId = Guid.NewGuid().ToString("N");
    var buffer = new byte[1024];
    connections.Register(connectionId, socket);
    try
    {
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(buffer, context.RequestAborted);
            if (result.MessageType == WebSocketMessageType.Close) { await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", context.RequestAborted); break; }
            var replies = gateway.HandleMessages(connectionId, Encoding.UTF8.GetString(buffer, 0, result.Count));
            foreach (var reply in replies) await connections.SendAsync(reply, context.RequestAborted);
        }
    }
    finally
    {
        gateway.Disconnect(connectionId);
        connections.Remove(connectionId);
    }
});
app.Run();
