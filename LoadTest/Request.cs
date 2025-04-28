using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LoadTest
{
    public class Request
    {
        public async Task SendReq()
        {
            string url = "http://beta-kube-hap1.asax.local:31230/Contact/v1/Device/AddDeviceByCcms";
            string token = "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJuYW1laWQiOiJBUmxBbDF2TWVsWnJrbDZsY0VPdFZnPT0iLCJuYmYiOjE3NDU3NTk5NDUsImV4cCI6MTc0NTc2MzU0NSwiaWF0IjoxNzQ1NzU5OTQ1fQ.";

            // Create HTTP client
            var httpClient = new HttpClient();

            // Define the payload
            var payload = new
            {
                playerID = "faraz",
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

            // Serialize payload to JSON
            var jsonPayload = JsonSerializer.Serialize(payload);

            // Create HTTP request message
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Headers =
                {
                    { "accept", "*/*" },
                    { "Authorization", $"Bearer {token}" }
                },
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };

            try
            {
                // Send the request asynchronously
                var response = await httpClient.SendAsync(request);

                // Check if the response was successful
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Request successful");
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response: {responseContent}");
                }
                else
                {
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
        }
    }
}
