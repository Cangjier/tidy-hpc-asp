using System.Collections;
using TidyHPC.Routers.Urls.Interfaces;

namespace TidyHPC.ASP.LiteKestrelServers.HttpSessions;

public struct KestrelHttpHeaders(IHeaderDictionary target):IRequestHeaders,IResponseHeaders
{
    public IHeaderDictionary Target = target;

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        foreach (var item in Target)
        {
            yield return new KeyValuePair<string, string>(item.Key, item.Value.ToString());
        }
    }

    public string GetHeader(string key)
    {
        return Target[key].ToString();
    }

    public void SetHeader(string key, string value)
    {
        Target[key] = value;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
