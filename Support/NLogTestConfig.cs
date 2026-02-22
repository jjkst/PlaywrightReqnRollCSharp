using NLog;
using NLog.Config;
using NLog.Targets;
using PlaywrightReqnRollCSharp.Steps;

namespace PlaywrightReqnRollCSharp.Support;

public static class NLogTestConfig
{
    public static void SetupNLog()
    {
        var config = new LoggingConfiguration();

        var consoleTarget = new ConsoleTarget("logconsole")
        {
            Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}"
        };
        config.AddTarget(consoleTarget);

        // Create a file target
        var fileTarget = new FileTarget("logfile")
        {
            FileName = Hooks.ProjectDirectory + "/Logs/test-run-${shortdate}.log",
            Layout = "${longdate}|${uppercase:${level}}|${message} ${exception:format=tostring}"
        };
        config.AddTarget(fileTarget);

        config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);
        config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);


        // Activate the configuration
        LogManager.Configuration = config;
    }
}