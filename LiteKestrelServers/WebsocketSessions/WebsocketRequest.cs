using TidyHPC.ASP.LiteKestrelServers.HttpSessions;
using TidyHPC.LiteHttpServer;
using TidyHPC.Routers.Urls;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.ASP.LiteKestrelServers.WebsocketSessions;

public class WebsocketRequest(Uri? url, Stream body, KestrelHttpHeaders headers) : IRequest
{
    /// <summary>
    /// 地址
    /// </summary>
    public Uri? Url { get; private set; } = url;

    /// <summary>
    /// 查询参数
    /// </summary>
    public IDictionary<string, string> Query { get; private set; } = new EmptyDictionary();

    /// <summary>
    /// Headers
    /// </summary>
    public IRequestHeaders Headers { get; private set; } = headers;

    /// <summary>
    /// Method
    /// </summary>
    public UrlMethods Method { get; private set; } = UrlMethods.WebSocket;

    /// <summary>
    /// 体
    /// </summary>
    public Stream Body { get; private set; } = body;

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Url = null!;
        Query = null!;
        Headers = null!;
        Body = null!;
    }
}