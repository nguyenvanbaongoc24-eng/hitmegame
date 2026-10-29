using System.Net.WebSockets;
var app = WebApplication.CreateBuilder(args).Build();
app.UseWebSockets();
app.MapGet("/health", () => Results.Ok(new { status = "ok", protocol = 1 }));
app.Map("/ws", async context => {
    if (!context.WebSockets.IsWebSocketRequest) { context.Response.StatusCode = 400; return; }
    using var socket = await context.WebSockets.AcceptWebSocketAsync();
    var buffer = new byte[1024];
    while (socket.State == WebSocketState.Open) { var result = await socket.ReceiveAsync(buffer, context.RequestAborted); if (result.MessageType == WebSocketMessageType.Close) await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", context.RequestAborted); }
});
app.Run();
