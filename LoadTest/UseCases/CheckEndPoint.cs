using LoadTest.Entities;
using System.Text;
using System.Text.Json;

namespace LoadTest.UseCases;
public static class CheckEndPoint
{
    public static async Task<bool> SendRequest(this RequestConfig requestConfig)
    {
        var httpClient = new HttpClient();

         var jsonPayload = JsonSerializer.Serialize(requestConfig.Payload);

        var request = new HttpRequestMessage(HttpMethod.Post, requestConfig.Url)
        {
            Content = new StringContent(
                jsonPayload,
                requestConfig.EncodingType ?? Encoding.UTF8,  
                requestConfig.ContentType ?? "application/json" 
            )
        };

        if(requestConfig.Headers is not null)
        {
            foreach (var header in requestConfig.Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        try
        {
            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                //should have logger
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
