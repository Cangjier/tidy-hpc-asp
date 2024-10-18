using Microsoft.AspNetCore.Http.Features;

namespace TidyHPC.ASP.LiteKestrelServers.Extensions;

public static class IFeatureCollectionExtensions
{
    public static Uri? GetUri(this IFeatureCollection features)
    {
        var requestFeature = features.Get<IHttpRequestFeature>();
        if (requestFeature == null)
        {
            return null;
        }

        var connectionFeature = features.Get<IHttpConnectionFeature>();
        if (connectionFeature == null)
        {
            return null;
        }
        var uriBuilder = new UriBuilder
        {
            Scheme = requestFeature.Scheme,
            Host = connectionFeature.LocalIpAddress?.MapToIPv4().ToString(),
            Path = new PathString(requestFeature.PathBase).Add(requestFeature.Path),
            Query = requestFeature.QueryString
        };
        return uriBuilder.Uri;
    }
}
