using TidyHPC.ASP.LiteKestrelServers.Extensions;
using TidyHPC.Routers.Urls;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.ASP.LiteKestrelServers.HttpSessions;

public class KestrelHttpContextRequest(HttpContext httpContext) : IRequest
{
    public HttpContext HttpContext { get; } = httpContext;

    private Uri? _url = null;

    public Uri? Url
    {
        get
        {
            if (_url == null)
            {
                _url = HttpContext.Request.GetUri();
            }
            return _url;
        }
    }

    private Dictionary<string, string>? _qeury = null;

    public IDictionary<string, string> Query
    {
        get
        {
            if (_qeury == null)
            {
                _qeury = new Dictionary<string, string>();
                foreach (var item in HttpContext.Request.Query)
                {
                    _qeury.Add(item.Key, item.Value.ToString());
                }
            }
            return _qeury;
        }
    }

    public IRequestHeaders Headers => new KestrelHttpHeaders(HttpContext.Request.Headers);

    public UrlMethods Method => HttpContext.Request.Method switch
    {
        "GET" => UrlMethods.HTTP_GET,
        "POST" => UrlMethods.HTTP_POST,
        "PUT" => UrlMethods.HTTP_PUT,
        "DELETE" => UrlMethods.HTTP_DELETE,
        "TRACE" => UrlMethods.HTTP_TRACE,
        "OPTIONS" => UrlMethods.HTTP_OPTIONS,
        "CONNECT" => UrlMethods.HTTP_CONNECT,
        "PATCH" => UrlMethods.HTTP_PATCH,
        _ => UrlMethods.HTTP_GET
    };

    private Stream? _body = null;

    public Stream Body
    {
        get
        {
            if (_body == null)
            {
                _body = HttpContext.Request.Body;
            }
            return _body;
        }
    }

    public void Dispose()
    {
        _body = null;
        _url = null;
        _qeury = null;
        // 由于HttpContext是由Kestrel管理的，所以不需要手动释放
    }
}
