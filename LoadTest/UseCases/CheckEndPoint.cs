using LoadTest.Entities;
using LoadTest.Extensions;
using Serilog;
using System.Text;
using System.Text.Json;

namespace LoadTest.UseCases;
public static class CheckEndPoint
{
    public static async Task<bool> SendRequest(this Configs Configs)
    {

        LoggerHelper.ConfigureLogger("logs/loadtest_log.txt",
            rollingInterval: RollingInterval.Day);

        HttpClient httpClient = HttpClientFactory.CreateHttpClient(Configs);

        string jsonPayload = JsonSerializer.Serialize(Configs.RequestConfig.Payload);

        Log.Information($"request payload : {jsonPayload}");

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Configs.RequestConfig.Url);

        Encoding encoding = Configs.RequestConfig.EncodingType ?? Encoding.UTF8;
        string contentType = Configs.RequestConfig.ContentType ?? "application/json";

        Log.Information($"EncodingType from Configs : {Configs.RequestConfig.EncodingType}");
        Log.Information($"contentType from Configs : {Configs.RequestConfig.ContentType}");

        if (!string.IsNullOrWhiteSpace(jsonPayload) && jsonPayload != "{}")
        {
            request.Content = new StringContent(jsonPayload, encoding, contentType);
            Log.Information($"request content : {request.Content.ToString()}");
        }
        else
        {
            request.Content = new StringContent(string.Empty, encoding, contentType);
            Log.Information($"request content : {request.Content.ToString()}");
        }

        if (Configs.RequestConfig.Headers is not null)
        {
            foreach (var header in Configs.RequestConfig.Headers)
            {
                request.Headers.Add(header.Key, header.Value);
                Log.Information($"request header added by key : {header.Key} and value : {header.Value}");
            }
        }

        try
        {
            var response = await httpClient.SendAsync(request);

            Log.Information($"response : {response}");

            if (response.IsSuccessStatusCode)
            {
                Log.Information($"Request successful with StatusCode: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Log.Information($"Response Content: {responseContent}");
                return true;
            }
            else
            {
                Log.Error($"Request failed with status code: {response.StatusCode}");

                return false;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error occurred: {ex.Message}");

            return false;
        }
    }
}
