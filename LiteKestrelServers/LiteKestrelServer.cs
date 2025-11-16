using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using TidyHPC.ASP.LiteKestrelServers.Extensions;
using TidyHPC.ASP.LiteKestrelServers.HttpSessions;
using TidyHPC.ASP.LiteKestrelServers.WebsocketSessions;
using TidyHPC.LiteHttpServer;
using TidyHPC.LiteHttpServer.WebsocketServerSessions;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Queues;
using TidyHPC.Routers.Urls;
using TidyHPC.Routers.Urls.Responses;

namespace TidyHPC.ASP.LiteKestrelServers;

/// <summary>
/// 轻量级Kestrel服务器
/// </summary>
public class LiteKestrelServer : Routers.Urls.Interfaces.IServer
{
    private class KestrelOptions : IOptions<KestrelServerOptions>
    {
        private KestrelOptions()
        {
            Value = new KestrelServerOptions();
        }

        public static KestrelOptions Defaults { get; } = new KestrelOptions();

        public KestrelServerOptions Value { get; init; }
    }

    private class SocketOptions : IOptions<SocketTransportOptions>
    {
        public static SocketOptions Defaults { get; } = new SocketOptions
        {
            Value = new SocketTransportOptions()
            {
                WaitForDataBeforeAllocatingBuffer = false,
                UnsafePreferInlineScheduling = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Environment.GetEnvironmentVariable("DOTNET_SYSTEM_NET_SOCKETS_INLINE_COMPLETIONS") == "1" : false,
            }
        };

        public SocketTransportOptions Value { get; init; } = new SocketTransportOptions();
    }

    private class DefaultLoggerFactories
    {
        public static ILoggerFactory Empty => new LoggerFactory();
    }

    private class App(WaitQueue<Session> sessionQueue) : IHttpApplication<Session>
    {
        private WaitQueue<Session> SessionQueue { get; } = sessionQueue;

        public Session CreateContext(IFeatureCollection features)
        {
            return new Session(new KestrelHttpFeatureRequest(features), new KestrelHttpFeatureResponse(features));
        }

        public void DisposeContext(Session context, Exception? exception)
        {
            // session在路由完成后会自动释放
        }

        public async Task ProcessRequestAsync(Session context)
        {
            SessionQueue.Enqueue(context);
            await ((KestrelHttpFeatureResponse)(context.Response)).CompletionSource.Task;
        }
    }

    private WaitQueue<Session> SessionQueue { get; } = new();

    public async Task<Session> GetNextSession()
    {
        return await SessionQueue.Dequeue();
    }

    public List<int> ListenPorts { get; } = [];

    public bool EnableAnyIP { get; set; } = true;

    public X509Certificate2? X509Certificate2 { get; set; }

    public async Task Start(CancellationToken cancellationToken)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            foreach (var port in ListenPorts)
            {
                if (EnableAnyIP)
                {
                    serverOptions.ListenAnyIP(port, listenOptions =>
                    {
                        if (X509Certificate2 != null)
                        {
                            listenOptions.UseHttps(X509Certificate2);
                        }
                    });
                }
                else
                {
                    serverOptions.Listen(System.Net.IPAddress.Loopback,port, listenOptions =>
                    {
                        if (X509Certificate2 != null)
                        {
                            listenOptions.UseHttps(X509Certificate2);
                        }
                    });
                }
                
            }
        });
        var app = builder.Build();
        app.UseWebSockets();
        app.Use(async (HttpContext context,Func<Task> next) =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket;
                try
                {
                    webSocket = await context.WebSockets.AcceptWebSocketAsync();
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    context.Response.StatusCode = 500;
                    return;
                }
                while (true)
                {
                    try
                    {
                        var message = await webSocket.ReceiveMessage(cancellationToken);
                        if (message.CloseStatus == WebSocketCloseStatus.NormalClosure)
                        {
                            break;
                        }
                        MemoryStream requestBody = new(Util.UTF8.GetBytes(message.Message));
                        WebsocketServerSendStream responseBody = new(webSocket);
                        responseBody.OnClose = () =>
                        {
                            requestBody.Dispose();
                        };
                        
                        Uri? url = null;
                        bool containsUrl = false;
                        if (Json.TryParse(message.Message, out var msg))
                        {
                            if (msg.ContainsKey("url"))
                            {
                                url = new Uri(context.Request.GetUri()!, msg.Read("url", string.Empty));
                                containsUrl = true;
                            }
                            msg.Dispose();
                        }
                        if (containsUrl == false)
                        {
                            continue;
                        }
                        Session session = new(new WebsocketRequest(url, requestBody, new KestrelHttpHeaders(context.Request.Headers)), new WebsocketResponse(webSocket, responseBody));
                        SessionQueue.Enqueue(session);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        break;
                    }
                }
                webSocket.Dispose();
            }
            else
            {
                var request = new KestrelHttpFeatureRequest(context.Features);
                var response = new KestrelHttpFeatureResponse(context.Features);
                response.StatusCode = 200;
                var session = new Session(request, response);
                SessionQueue.Enqueue(session);
                await response.CompletionSource.Task;
            }
        });
        await app.RunAsync();
    }

    public async Task Start()
    {
        await Start(CancellationToken.None);
    }

    public async Task<Session> GetNextSession(CancellationToken cancellationToken)
    {
        return await SessionQueue.Dequeue(cancellationToken);
    }
}
