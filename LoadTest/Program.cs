using LoadTest.Entities;
using LoadTest.Factory;
using LoadTest.UseCases;
using Microsoft.Extensions.Configuration;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using Serilog;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace LoadTest;

public class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        Configs configs = config.GetSection("RequestConfig").Get<Configs>();

        if (configs is null)
            throw new Exception("problem to bind the config");

        bool isSuccess = await configs.SendRequest();

        if (!isSuccess)
            throw new Exception("problem to send request");

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
        .WithMaxFailCount(maxFailCount) 
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(
                rate: requestRate,
                interval: TimeSpan.FromSeconds(intervalFromSeconds),
                during: TimeSpan.FromMinutes(duringFromMinutes
            ))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFormats(ReportFormat.Txt, ReportFormat.Csv, ReportFormat.Html)
            .WithScenarioCompletionTimeout(TimeSpan.FromMinutes(scenarioCompletionTimeoutFromMinutes)) 
            .WithLoggerConfig(() => new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("nbomber_logs.txt", rollingInterval: RollingInterval.Day))
            .Run();

        var scenarioStats = stats.ScenarioStats.First();

        var successfulRequests = scenarioStats.Ok.Request.Count;
        var failedRequests = scenarioStats.Fail.Request.Count;
        var totalRequests = successfulRequests + failedRequests;
    }
}
