using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.ASP.LiteKestrelServers.HttpSessions;

public class KestrelHttpContextResponse(HttpContext httpContext) :IResponse
{
    public HttpContext HttpContext { get; private set; } = httpContext;

    public IResponseHeaders Headers => new KestrelHttpHeaders(HttpContext.Response.Headers);

    private Stream? _body = null;

    public Stream Body
    {
        get
        {
            if (_body == null)
            {
                _body = new KestrelStream(HttpContext.Response.BodyWriter);
            }
            return _body;
        }
    }

    public TaskCompletionSource CompletionSource { get; private set; } = new();

    public int StatusCode
    {
        get
        {
            return HttpContext.Response.StatusCode;
        }
        set
        {
            HttpContext.Response.StatusCode = value;
        }
    }

    public void Dispose()
    {
        CompletionSource.TrySetResult();
        _body = null;
        HttpContext = null!;
        CompletionSource = null!;
        // 由于HttpContext是由Kestrel管理的，所以不需要手动释放
    }
}
