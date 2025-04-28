using LoadTest.Entities;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using LoadTest.Factory;

namespace LoadTest.UseCases;
public static class CheckEndPoint
{
    public static async Task<bool> SendRequest(this Configs Configs)
    {

        var httpClient = HttpClientFactory.CreateHttpClient(Configs);

        var jsonPayload = JsonSerializer.Serialize(Configs.RequestConfig.Payload);

        var request = new HttpRequestMessage(HttpMethod.Post, Configs.RequestConfig.Url);

        var encoding = Configs.RequestConfig.EncodingType ?? Encoding.UTF8;
        var contentType = Configs.RequestConfig.ContentType ?? "application/json";

        if (!string.IsNullOrWhiteSpace(jsonPayload) && jsonPayload != "{}")
        {
            request.Content = new StringContent(jsonPayload, encoding, contentType);
        }
        else
        {
            request.Content = new StringContent(string.Empty, encoding, contentType);
        }

        if (Configs.RequestConfig.Headers is not null)
        {
            foreach (var header in Configs.RequestConfig.Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        try
        {
            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Request successful");
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response: {responseContent}");

                return true;
            }
            else
            {
                Console.WriteLine($"Request failed with status code: {response.StatusCode}");

                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");

            return false;
        }
    }
}
