using LoadTest.Entities;

namespace LoadTest.Extensions;
public static class HttpClientFactory
{
    public static HttpClient CreateHttpClient(Configs configs)
    {
        var handler = new HttpClientHandler
        {
        };

        var httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMinutes(configs.RequestConfig.HttpClientTimeoutFromMinutes)
        };

        return httpClient;
    }
}
