using System.Text;
using System.Text.Json;

namespace LoadTest.UseCases;
public class CheckEndPoint
{
    public async Task<bool> SendReq()
    {
        //should read from appsetting
        string url = "http://beta-kube-hap1.asax.local:31230/Contact/v1/Device/AddDeviceByCcms";
        string token = "Bearer eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJuYW1laWQiOiJBUmxBbDF2TWVsWnJrbDZsY0VPdFZnPT0iLCJuYmYiOjE3NDU3NTk5NDUsImV4cCI6MTc0NTc2MzU0NSwiaWF0IjoxNzQ1NzU5OTQ1fQ.";

        //should to move to config
        var httpClient = new HttpClient();

        //should read from appsetting
        var payload = new
        {
            playerID = "t",
            ccmsCode = 1,
            fcmToken = "1",
            applicationName = "1",
            platform = "1",
            os = "1",
            osVersion = "1",
            clientModel = "1",
            appVersion = "1",
            appVersionCode = 1,
            publishedStore = "1"
        };

        var jsonPayload = JsonSerializer.Serialize(payload);

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Headers =
            {
                { "accept", "*/*" },
                { "Authorization", $"{token}" }
            },
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };

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
