using System.Net.Http;

namespace SkEditor.Utilities.Docs;

public static class HttpClientExtensions
{
    public static HttpClient WithHeader(this HttpClient client, string key, string value)
    {
        client.DefaultRequestHeaders.Add(key, value);
        return client;
    }

    public static HttpClient WithUserAgent(this HttpClient client, string userAgent)
    {
        return client.WithHeader("User-Agent", userAgent);
    }

    public static HttpClient WithAuthorization(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", token);
        return client;
    }
}