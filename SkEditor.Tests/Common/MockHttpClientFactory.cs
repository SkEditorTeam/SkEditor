using RichardSzalay.MockHttp;

namespace SkEditor.Tests.Common;

public class MockHttpClientFactory(MockHttpMessageHandler mockHttpMessageHandler) : IHttpClientFactory
{
    private readonly MockHttpMessageHandler _mockHttpMessageHandler = mockHttpMessageHandler ?? throw new ArgumentNullException(nameof(mockHttpMessageHandler));

    public HttpClient CreateClient(string name)
    {
        return _mockHttpMessageHandler.ToHttpClient();
    }
}

public static class MockHttpMessageHandlerExtensions
{
    public static IHttpClientFactory ToHttpClientFactory(this MockHttpMessageHandler mockHttpMessageHandler)
    {
        return new MockHttpClientFactory(mockHttpMessageHandler);
    }
}