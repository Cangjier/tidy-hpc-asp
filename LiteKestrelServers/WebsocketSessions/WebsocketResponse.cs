using System.Net.WebSockets;
using TidyHPC.LiteHttpServer.WebsocketServerSessions;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.ASP.LiteKestrelServers.WebsocketSessions;

public class WebsocketResponse : IResponse, IWebsocketResponse
{
    public WebsocketResponse(WebSocket webSocket,
        Stream body)
    {
        WebSocket = webSocket;
        Body = body;
    }

    public WebSocket WebSocket;

    public IResponseHeaders Headers { get; } = new WebsocketServerResponseHeaders();

    public async Task SendMessage(string message)
    {
        await WebSocket.SendAsync(new ArraySegment<byte>(Util.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public bool IsAlive() =>
        WebSocket.State != WebSocketState.Aborted &&
        WebSocket.State != WebSocketState.Closed &&
        WebSocket.State != WebSocketState.CloseSent &&
        WebSocket.State != WebSocketState.CloseReceived;

    public Stream Body { get; }

    public int StatusCode { get; set; }

    public void Dispose()
    {
        Body.Dispose();
    }

    public bool Equals(IWebsocketResponse? other)
    {
        return other is WebsocketResponse response && WebSocket == response.WebSocket;
    }

    public void Close()
    {
        WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None).Wait();
    }
}