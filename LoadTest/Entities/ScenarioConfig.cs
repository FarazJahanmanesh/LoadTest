namespace LoadTest.Entities;

public class ScenarioConfig
{
    public string LoadTestScenarioName { get; set; }
    public int MaxFailCount { get; set; }
    public int RequestRate { get; set; }
    public int IntervalFromSeconds { get; set; }
    public int DuringFromMinutes { get; set; }
    public int ScenarioCompletionTimeoutFromMinutes { get; set; }
}
