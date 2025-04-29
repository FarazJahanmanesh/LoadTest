using LoadTest.Entities;
using LoadTest.Extensions;
using LoadTest.UseCases;
using Microsoft.Extensions.Configuration;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using Serilog;
using System.Text;
using System.Text.Json;

namespace LoadTest;

public class Program
{
    public static async Task Main(string[] args)
    {

        LoggerHelper.ConfigureLogger("logs/loadtest_log.txt",
            rollingInterval: RollingInterval.Day);

        try
        {
            Log.Information("Application Starting...");

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Configs configs = config.GetSection("Configs").Get<Configs>();

            if (configs is null)
            {
                Log.Error("config is empty");
                throw new Exception("Problem binding the config");
            }

            bool isSuccess = await configs.SendRequest();

            if (!isSuccess)
            {
                Log.Error("failed to send request to your end point");
                throw new Exception("Problem sending initial request");
            }

            var httpClient = HttpClientFactory.CreateHttpClient(configs);

            var jsonPayload = JsonSerializer.Serialize(configs.RequestConfig.Payload);

            Encoding encoding = configs.RequestConfig.EncodingType ?? Encoding.UTF8;
            string contentType = configs.RequestConfig.ContentType ?? "application/json";
            HttpMethod method = new HttpMethod(configs.RequestConfig.HttpMethodType);
            int maxFailCount = configs.ScenarioConfig.MaxFailCount;
            int requestRate = configs.ScenarioConfig.RequestRate;
            int intervalFromSeconds = configs.ScenarioConfig.IntervalFromSeconds;
            int duringFromMinutes = configs.ScenarioConfig.DuringFromMinutes;
            int scenarioCompletionTimeoutFromMinutes = configs.ScenarioConfig.ScenarioCompletionTimeoutFromMinutes;

            var scenario = Scenario.Create(configs.ScenarioConfig.LoadTestScenarioName, async context =>
            {
                var request = new HttpRequestMessage(method, configs.RequestConfig.Url);

                if (!string.IsNullOrWhiteSpace(jsonPayload) && jsonPayload != "{}")
                {
                    request.Content = new StringContent(jsonPayload, encoding, contentType);
                }
                else
                {
                    request.Content = new StringContent(string.Empty, encoding, contentType);
                }

                if (configs.RequestConfig.Headers is not null)
                {
                    foreach (var header in configs.RequestConfig.Headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                try
                {
                    var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                    if (response.IsSuccessStatusCode)
                    {
                        Log.Information($"Request succeeded {response}");
                        return Response.Ok();
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        Log.Error($"Request failed - Status Code: {response.IsSuccessStatusCode}, Content: {content}");
                        return Response.Fail();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Exception occurred during sending request Exception : {ex.Message}");
                    return Response.Fail();
                }
            })
            .WithMaxFailCount(maxFailCount)
            .WithoutWarmUp()
            .WithLoadSimulations(
                Simulation.Inject(
                    rate: requestRate,
                    interval: TimeSpan.FromSeconds(intervalFromSeconds),
                    during: TimeSpan.FromMinutes(duringFromMinutes)
                )
            );

            var stats = NBomberRunner
                .RegisterScenarios(scenario)
                .WithReportFormats(ReportFormat.Txt, ReportFormat.Csv, ReportFormat.Html)
                .WithScenarioCompletionTimeout(TimeSpan.FromMinutes(scenarioCompletionTimeoutFromMinutes))
                .Run();

            var scenarioStats = stats.ScenarioStats.First();
            var successfulRequests = scenarioStats.Ok.Request.Count;
            var failedRequests = scenarioStats.Fail.Request.Count;
            var totalRequests = successfulRequests + failedRequests;

            Log.Information("Test completed. Successful: {Successful}, Failed: {Failed}, Total: {Total}",
                successfulRequests, failedRequests, totalRequests);
        }
        catch (Exception ex)
        {
            Log.Fatal($"Application terminated unexpectedly Exception : {ex}");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
