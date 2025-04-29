using Serilog;

namespace LoadTest.Extensions;

public static class LoggerHelper
{
    public static void ConfigureLogger(string LogFile, RollingInterval rollingInterval)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(LogFile, rollingInterval: rollingInterval)
            .CreateLogger();
    }
}
