using System.Net.Http;
using System.Net.Http.Headers;

namespace SkEditor.Utilities.Docs;

public static class HttpClientExtensions
{
    extension(HttpClient client)
    {
        public HttpClient WithHeader(string key, string value)
        {
            client.DefaultRequestHeaders.Add(key, value);
            return client;
        }

        public HttpClient WithUserAgent(string userAgent)
        {
            return client.WithHeader("User-Agent", userAgent);
        }

        public HttpClient WithAuthorization(string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
            return client;
        }
    }
}