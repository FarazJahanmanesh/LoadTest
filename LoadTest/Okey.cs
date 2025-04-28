//using NBomber.Contracts.Stats;
//using NBomber.CSharp;
//using System.Text;
//using System.Text.Json;
//using System.Linq;

//namespace LoadTest
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            string url = "http://localhost:7127/v1/Device/AddDeviceByCcms";
//            string token = "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJuYW1laWQiOiJBUmxBbDF2TWVsWnJrbDZsY0VPdFZnPT0iLCJuYmYiOjE3NDU3NTM0NTcsImV4cCI6MTc0NTc1NzA1NywiaWF0IjoxNzQ1NzUzNDU3fQ."; // Replace with your actual token

//            // Create HTTP client
//            var httpClient = new HttpClient();

//            // Define the payload
//            var payload = new
//            {
//                playerID = "faraz",
//                ccmsCode = 1,
//                fcmToken = "1",
//                applicationName = "1",
//                platform = "1",
//                os = "1",
//                osVersion = "1",
//                clientModel = "1",
//                appVersion = "1",
//                appVersionCode = 1,
//                publishedStore = "1"
//            };

//            var jsonPayload = JsonSerializer.Serialize(payload);

//            // Create NBomber scenario
//            var scenario = Scenario.Create("load_test_scenario", async context =>
//            {
//                // Create HTTP request message
//                var request = new HttpRequestMessage(HttpMethod.Post, url)
//                {
//                    Headers =
//                    {
//                        { "accept", "*/*" },
//                        { "Authorization", $"Bearer {token}" }
//                    },
//                    Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
//                };

//                try
//                {
//                    // Send the request asynchronously
//                    var response = await httpClient.SendAsync(request);

//                    // Check if the response was successful
//                    if (response.IsSuccessStatusCode)
//                    {
//                        return Response.Ok();
//                    }
//                    else
//                    {
//                        return Response.Fail();
//                    }
//                }
//                catch (Exception)
//                {
//                    // Log the exception
//                    return Response.Fail();
//                }
//            })
//            .WithWarmUpDuration(TimeSpan.FromSeconds(5))
//            .WithLoadSimulations(
//                Simulation.Inject(rate: 5, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(3)),
//                Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(3)),
//                Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(3)),
//                Simulation.Inject(rate: 30, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(3)),
//                Simulation.Inject(rate: 40, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(3)),
//                Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(3))
//            );

//            // Run the NBomber scenarios and get the stats
//            var stats = NBomberRunner
//                .RegisterScenarios(scenario)
//                .WithReportFormats(ReportFormat.Txt, ReportFormat.Csv, ReportFormat.Html)
//                .Run();

//            // Retrieve the statistics for the first scenario
//            var scenarioStats = stats.ScenarioStats.First();

//            // Access statistics like successful requests, failed requests, and total requests
//            var successfulRequests = scenarioStats.Ok.Request.Count;
//            var failedRequests = scenarioStats.Fail.Request.Count;
//            var totalRequests = successfulRequests + failedRequests;

//            // Print the statistics to the console
//            Console.WriteLine($"Total requests: {totalRequests}");
//            Console.WriteLine($"Total successful requests: {successfulRequests}");
//            Console.WriteLine($"Total failed requests: {failedRequests}");
//        }
//    }
//}
