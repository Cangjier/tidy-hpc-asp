using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using TidyHPC.ASP.LiteKestrelServers.Extensions;
using TidyHPC.Routers.Urls;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.ASP.LiteKestrelServers.HttpSessions;

public class KestrelHttpFeatureRequest(IFeatureCollection features) : IRequest
{
    public IFeatureCollection Features { get; } = features;

    private IHttpRequestFeature? _request;
    private IHttpRequestFeature RequestFeature => _request ??= Features.Get<IHttpRequestFeature>() ?? throw new InvalidOperationException();

    private IHttpConnectionFeature? _connection;

    private IHttpConnectionFeature ConnectionFeature => _connection ??= Features.Get<IHttpConnectionFeature>() ?? throw new InvalidOperationException();

    private Uri? _url = null;

    public Uri? Url
    {
        get
        {
            if( _url == null)
            {
                _url = Features.GetUri();
            }
            return _url;
        }
    }

    private Dictionary<string, string>? _query = null;

    public IDictionary<string, string> Query
    {
        get
        {
            if (_query == null)
            {
                _query = new Dictionary<string, string>();
                var nameValues = QueryHelpers.ParseQuery(RequestFeature.QueryString);
                foreach (var item in nameValues)
                {
                    _query.Add(item.Key, item.Value.ToString());
                }
            }
            return _query;
        }
    }

    public IRequestHeaders Headers => new KestrelHttpHeaders(RequestFeature.Headers);

    public UrlMethods Method
    {
        get
        {
            return RequestFeature.Method switch
            {
                "GET" => UrlMethods.HTTP_GET,
                "POST" => UrlMethods.HTTP_POST,
                "PUT" => UrlMethods.HTTP_PUT,
                "DELETE" => UrlMethods.HTTP_DELETE,
                "HEAD" => UrlMethods.HTTP_HEAD,
                "OPTIONS" => UrlMethods.HTTP_OPTIONS,
                "TRACE" => UrlMethods.HTTP_TRACE,
                "CONNECT" => UrlMethods.HTTP_CONNECT,
                _ => throw new NotImplementedException()
            };
        }
    }

    public Stream Body
    {
        get => RequestFeature.Body;
    }

    public void Dispose()
    {
        _request = null;
        _connection = null;
        _query?.Clear();
        _query = null;
        _url = null;
    }
}
