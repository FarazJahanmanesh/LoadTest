using NBomber.Contracts.Stats;
using NBomber.CSharp;
using Serilog;
using Serilog.Events;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LoadTest
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string url = "http://beta-kube-hap1.asax.local:31230/Contact/v1/Device/AddDeviceByCcms";
            string token = "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJuYW1laWQiOiJBUmxBbDF2TWVsWnJrbDZsY0VPdFZnPT0iLCJuYmYiOjE3NDU3NjU0NTQsImV4cCI6MTc0NTc2OTA1NCwiaWF0IjoxNzQ1NzY1NDU0fQ.";

            var handler = new HttpClientHandler
            {
            };

            var httpClient = new HttpClient(handler)
            {

            };

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

            var jsonPayload = JsonSerializer.Serialize(payload);

            var scenario = Scenario.Create("load_test_scenario", async context =>
            {
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
                    var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                    if (response.IsSuccessStatusCode)
                    {
                        return Response.Ok();
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        context.Logger.Error($"Status Code: {response.StatusCode}, Content: {content}");
                        return Response.Fail();
                    }
                }
                catch (Exception ex)
                {
                    context.Logger.Error($"Exception: {ex.Message}");
                    return Response.Fail();
                }
            })
            .WithMaxFailCount(20000) 
            .WithoutWarmUp()
            .WithLoadSimulations(
                Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(10), during: TimeSpan.FromMinutes(2))
            );

            var stats = NBomberRunner
                .RegisterScenarios(scenario)
                .WithReportFormats(ReportFormat.Txt, ReportFormat.Csv, ReportFormat.Html)
                .WithScenarioCompletionTimeout(TimeSpan.FromMinutes(5)) 
                .WithLoggerConfig(() => new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.File("nbomber_logs.txt", rollingInterval: RollingInterval.Day))
                .Run();

            var scenarioStats = stats.ScenarioStats.First();

            var successfulRequests = scenarioStats.Ok.Request.Count;
            var failedRequests = scenarioStats.Fail.Request.Count;
            var totalRequests = successfulRequests + failedRequests;

            Console.WriteLine($"Total requests: {totalRequests}");
            Console.WriteLine($"Total successful requests: {successfulRequests}");
            Console.WriteLine($"Total failed requests: {failedRequests}");
        }
    }
}
