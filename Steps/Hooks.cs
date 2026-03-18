using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Reporter;
using Microsoft.Playwright;
using NLog;
using PlaywrightReqnRollCSharp.Support;
using Reqnroll;
using Reqnroll.BoDi;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace PlaywrightReqnRollCSharp.Steps;

[Binding]
public sealed class Hooks(IObjectContainer objectContainer, FeatureContext featureContext, ScenarioContext scenarioContext)
{
    public static string RuntimeDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
    public static string ProjectDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.FullName;
    public static string Url { get; set; }
    public static string ApiBaseUrl { get; set; }
    public static string DBServer { get; set; }

    public static Logger? Logger { get; set; }
    private List<string> _pageErrors = [];
    private List<string> _consoleErrors = [];
    public static TestContext? TestContext { get; set; }

    private static ExtentReports? _extentReports;
    private static readonly ConcurrentDictionary<string, ExtentTest> _extentfeature = new();
    private static readonly ConcurrentDictionary<string, ExtentTest> _extentscenario = new();
    private static GherkinKeyword GetGherkinKeyword(string type) => type switch
    {
        "Given" => new GherkinKeyword("Given"),
        "When" => new GherkinKeyword("When"),
        "Then" => new GherkinKeyword("Then"),
        "And" => new GherkinKeyword("And"),
        "But" => new GherkinKeyword("But"),
        _ => new GherkinKeyword("Then")
    };


    [BeforeTestRun]
    public static async Task BeforeTestRunAsync(TestContext context)
    {
        TestContext = context;

        // Setup User Accounts
        string usersString = TestContext.Properties["Users"].ToString();
        var users = usersString?.Split(',');
        UserAccountManager.InitializeAccounts(users);

        // Setup Environment Variables
        var environment = new EnvironmentVariables(TestContext.Properties["Environment"].ToString());
        Url = environment.Url;
        ApiBaseUrl = environment.ApiBaseUrl;
        DBServer = environment.DBServer;
        
        // Setup Logging  
        NLogTestConfig.SetupNLog();
        Logger = LogManager.GetCurrentClassLogger();
        Logger.Info(" ");
        Logger.Info("**************************************************************************");
        Logger.Info("Test run started");
        Logger.Info($"TargetUrl: {Url}");
        Logger.Info($"DbServer: {DBServer}");

        // Setup Reporting 
       Directory.CreateDirectory($"{ProjectDirectory}/TestResults");
        var dateWithoutSlashes = $"{DateTime.Now:d}".Replace("/", "");
        var projectname = ProjectDirectory.Split("\\").Last();
        var htmlReporter = new ExtentSparkReporter($"{ProjectDirectory}/TestResults/{projectname}.test-run-{dateWithoutSlashes}.html");
        htmlReporter.Config.DocumentTitle = "Test Execution Report";
        htmlReporter.Config.ReportName = "Automation Test Results";

        _extentReports = new ExtentReports();
        _extentReports.AttachReporter(htmlReporter);
    }

    [AfterTestRun]
    public static void AfterTestRunAsync()
    {
        Logger?.Info("******************* Test run completed ***************************");
        LogManager.Shutdown();
        _extentReports?.Flush();
    }


    [BeforeFeature]
    public static async Task BeforeFeature(FeatureContext featureContext)
    {
        string featureTitle = featureContext.FeatureInfo.Title;
        ExtentTest featureTest = _extentReports.CreateTest<Feature>(featureTitle);
        _extentfeature.TryAdd(featureTitle, featureTest);

        Logger?.Info($"******* Feature: {featureTitle} is starting");
    }

    [AfterFeature]
    public static void AfterFeature(FeatureContext featureContext)
    {
        Logger.Info($"******* Feature: {featureContext.FeatureInfo.Title} is complete");
    }


    [BeforeScenario]
    public async Task BeforeScenarioAsync()
    {            
        string user = UserAccountManager.GetUserAccount() ?? throw new Exception("No user accounts available.");

        scenarioContext["CurrentUser"] = user;
        scenarioContext["Password"] = TestContext.Properties["Password"].ToString();

        // Setup Browser
        var browserType = TestContext.Properties?["Browser"]?.ToString();
        bool headless = true;
        if (TestContext.Properties.ContainsKey("HeadlessBrowser"))
        {
            var headlessString = TestContext.Properties?["HeadlessBrowser"]?.ToString();
            headless = bool.Parse(headlessString);
     }
        var browser = await SetupBrowser(browserType, headless);
        var browserContext = await browser.NewContextAsync();

        // Start Tracing
        await browserContext.Tracing.StartAsync(new()
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });
        var page = await browserContext.NewPageAsync();
        page.PageError += (_, exception) =>
        {
            _pageErrors.Add($"[PAGE ERROR] {exception}");
        };
        page.Console += (_, message) =>
        {
            if (message.Type == "error")
            {
                _consoleErrors.Add($"[CONSOLE ERROR] {message.Text}");
            }
        };
        await page.GotoAsync(Url, new() { WaitUntil = WaitUntilState.Load, Timeout = TestConstants.ExtendedTimeout });

        objectContainer.RegisterInstanceAs(browser);
        objectContainer.RegisterInstanceAs(browserContext);
        objectContainer.RegisterInstanceAs(page);
        var playwrightContext = new PlaywrightContext(objectContainer);
        objectContainer.RegisterInstanceAs(playwrightContext);

        var scenarioTitle = scenarioContext.ScenarioInfo.Title;
        if (_extentfeature.TryGetValue(featureContext.FeatureInfo.Title, out var featureTest))
        {
            scenarioTitle = scenarioContext.ScenarioInfo.Arguments.Count > 0 ?
                $"{scenarioTitle} - {scenarioContext.ScenarioInfo.Arguments[0]}" : scenarioTitle;

            ExtentTest featureScenario = featureTest.CreateNode<Scenario>(scenarioTitle);
            _extentscenario.TryAdd(scenarioTitle, featureScenario);
        }
        else
        {
            throw new Exception($"Failed to retrieve Extent Feature Test for: {featureContext.FeatureInfo.Title}");
        }

        Logger.Info($"******* Scenario: {scenarioTitle} is running");
    }

    [AfterScenario]
    public async Task AfterScenarioAsync()
    {
        StringBuilder log = new();
        log.AppendLine($@"{scenarioContext.ScenarioInfo.Title}_Context:");
        log.AppendLine(
            string.Join("\n", scenarioContext.Keys
                .Where(key => key != "CurrentUser" && key != "Password" && scenarioContext[key].GetType().Name == "String")
                .Select(key => $"{key}: {scenarioContext[key]}")
                .ToList()));
        log.AppendLine($@"PageErrors: {string.Join("\n", _pageErrors)}");
        log.Append($@"ConsoleErrors: {string.Join("\n", _consoleErrors)}");
        Logger.Info(log);
        Logger.Info("**********************************************************");

        UserAccountManager.ReleaseUserAccount(scenarioContext["CurrentUser"]?.ToString());

        if (featureContext.FeatureInfo.Tags.Contains("Backend")) return;

        var browser = objectContainer.Resolve<IBrowser>();
        await browser.CloseAsync();
    }

    [AfterStep]
    public async Task AfterStepAsync()
    {
        string stepType = scenarioContext.StepContext.StepInfo.StepDefinitionType.ToString();
        string stepInfo = scenarioContext.StepContext.StepInfo.Text;
        var gherkinKeyword = GetGherkinKeyword(stepType);

        var scenarioTitle = scenarioContext.ScenarioInfo.Title;
        scenarioTitle = scenarioContext.ScenarioInfo.Arguments.Count > 0 ? $"{scenarioTitle} - {scenarioContext.ScenarioInfo.Arguments[0]}" : scenarioTitle;
        var filename = scenarioTitle.Replace(" ", string.Empty).Replace("-", string.Empty);
        _extentscenario.TryGetValue(scenarioTitle, out ExtentTest currentExtentScenario);

        if (scenarioContext.TestError == null)
        {
            currentExtentScenario.CreateNode(gherkinKeyword, stepInfo);
        }
        else
        {
            var context = objectContainer.Resolve<IBrowserContext>();
            var screenshotPath = $"{ProjectDirectory}/TestResults/{filename}.png";

            Exception? innerException = scenarioContext.TestError?.InnerException;
            string? testError = scenarioContext.TestError?.Message;
            var failMsg = stepType == "Given" || stepType == "When" ? innerException?.ToString() : testError;

            var node = currentExtentScenario?.CreateNode(gherkinKeyword, stepInfo);
            var screenshot = await context.Pages[context.Pages.Count - 1].ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = true
            });
            node?.Fail(failMsg, MediaEntityBuilder.CreateScreenCaptureFromBase64String(Convert.ToBase64String(screenshot)).Build());

            Logger.Error($"{filename}_Step: {stepInfo} failed with error message \n {failMsg}");

            var browserContext = objectContainer.Resolve<IBrowserContext>();
            await browserContext.Tracing.StopAsync(new()
            {
                Path = $"{ProjectDirectory}/TestResults/{filename}.zip"
            });
        }
    }

    private static async Task<IBrowser> SetupBrowser(string browserType, bool headless)
    {
               var playwright = await Playwright.CreateAsync();
        IBrowser browser = browserType.ToLower() switch
        {
            "chrome" => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Channel = "chrome",
                Headless = headless
            }),
            "msedge" => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Channel = "msedge",
                Headless = headless
            }),
            "chromium" => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = headless
            }),
            "webkit" => await playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = headless
            }),
            _ => throw new ArgumentException($"Unsupported browser type: {browserType}"),
        };

        return browser;
    }
}