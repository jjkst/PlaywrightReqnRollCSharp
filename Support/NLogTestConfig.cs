using NLog;
using NLog.Config;
using NLog.Targets;
using PlaywrightMSTests.Steps;

namespace PlaywrightMSTests.Support;

public static class NLogTestConfig
{
    public static void SetupNLog()
    {
        var config = new LoggingConfiguration();

        // Create a file target
        var fileTarget = new FileTarget("logfile")
        {
            FileName = Hooks.ProjectDirectory + "/Logs/test-run-${shortdate}.log",
            Layout = "${longdate}|${uppercase:${level}}|${message} ${exception:format=tostring}"
        };
        config.AddTarget(fileTarget);

        // Define a rule to send all log messages to the file target
        var rule = new LoggingRule("*", LogLevel.Trace, fileTarget);
        config.LoggingRules.Add(rule);

        // Activate the configuration
        LogManager.Configuration = config;
    }
}