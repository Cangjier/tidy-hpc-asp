using Microsoft.AspNetCore.Http.Features;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.ASP.LiteKestrelServers.HttpSessions;

public class KestrelHttpFeatureResponse(IFeatureCollection features) : IResponse
{
    public IFeatureCollection Features { get; } = features;

    private IHttpResponseFeature? _response = null;

    private IHttpResponseFeature ResponseFeature => _response ??= Features.Get<IHttpResponseFeature>() ?? throw new InvalidOperationException();

    private IHttpResponseBodyFeature? _responseBody = null;

    private IHttpResponseBodyFeature ResponseBody => _responseBody ??= Features.Get<IHttpResponseBodyFeature>() ?? throw new InvalidOperationException();

    public IResponseHeaders Headers => new KestrelHttpHeaders(ResponseFeature.Headers);

    private KestrelStream? _body = null;

    public Stream Body
    {
        get
        {
            if (_body == null)
            {
                _body = new KestrelStream(ResponseBody.Writer);
            }
            return _body;
        }
    }

    public TaskCompletionSource CompletionSource { get; } = new();

    public int StatusCode
    {
        get
        {
            return ResponseFeature.StatusCode;
        }
        set
        {
            ResponseFeature.StatusCode = value;
        }
    }

    public void Dispose()
    {
        CompletionSource.TrySetResult();
        _body = null;
        _response = null;
        _responseBody = null;
    }
}
