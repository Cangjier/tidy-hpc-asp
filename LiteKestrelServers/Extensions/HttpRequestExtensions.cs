namespace TidyHPC.ASP.LiteKestrelServers.Extensions;

public static class HttpRequestExtensions
{
    public static Uri? GetUri(this HttpRequest request)
    {
        var uriBuilder = new UriBuilder
        {
            Scheme = request.Scheme,
            Host = request.Host.Host,
            Path = request.PathBase.Add(request.Path).Value,
            Query = request.QueryString.Value
        };
        return uriBuilder.Uri;
    }
}
