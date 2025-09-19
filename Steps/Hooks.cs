using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using Microsoft.Playwright;
using NLog;
using PlaywrightMSTests.Support;
using Reqnroll;
using System.Reflection;

namespace PlaywrightMSTests.Steps
{
    [Binding]
    public sealed class Hooks(FeatureContext featureContext, ScenarioContext scenarioContext)
    {
        public static string ProjectDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.FullName;
        public static string Url { get; set; }
        public static string ApiBaseUrl { get; set; }
        public static string DBServer { get; set; }

        public static Logger Logger;

        private static TestContext TestContext { get; set; }
        private IBrowserContext BrowserContext { get; set; }
        public IPage Page { get; set; }

        // Local reporting objects
        private static ExtentReports _extent;
        private static ExtentTest _extentfeature;
        private ExtentTest _extentscenario;


        [BeforeTestRun]
        public static async Task BeforeTestRunAsync(TestContext context)
        {
            TestContext = context;

            // Setup User Accounts
            string usersString = TestContext.Properties["Users"].ToString();
            var users = usersString.Split(',');
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
            _extent = new ExtentReports();
            _extent.AttachReporter(new ExtentSparkReporter($"{ProjectDirectory}/TestResults/test-run-${DateTime.Now:d}.html"));
        }

        [AfterTestRun]
        public static void AfterTestRunAsync()
        {
            Logger.Info("Test run completed");
            Logger.Info("**************************************************************************");
            Logger.Info(" ");
            LogManager.Shutdown();
            _extent.Flush();
        }


        [BeforeFeature]
        public static async Task BeforeFeature(FeatureContext featureContext)
        {
            Logger.Info($"------ Feature {featureContext.FeatureInfo.Title} is running");
            _extentfeature = _extent.CreateTest(featureContext.FeatureInfo.Title);          
        }

        [AfterFeature]
        public static void AfterFeature(FeatureContext featureContext)
        {
            Logger.Info($"------ Feature {featureContext.FeatureInfo.Title} is complete");
        }


        [BeforeScenario]
        public async Task BeforeScenarioAsync()
        {            
            string user = UserAccountManager.GetUserAccount() ?? throw new Exception("No user accounts available.");

            scenarioContext["CurrentUser"] = user;
            scenarioContext["Password"] = TestContext.Properties["Password"].ToString();

            // Setup Browser
            string browserType = TestContext.Properties["Browser"].ToString();
            bool headless = true;
            if (TestContext.Properties.Contains("HeadlessBrowser"))
            {
                string headlessString = TestContext.Properties["HeadlessBrowser"].ToString();
                headless = bool.Parse(headlessString);
            }
            var browser = await SetupBrowser(browserType, headless);
            BrowserContext = await browser.NewContextAsync();

            // Start Tracing
            await BrowserContext.Tracing.StartAsync(new()
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });
            Page = await BrowserContext.NewPageAsync();
            await Page.GotoAsync(Url);

            Logger.Info("----------------------------------------------------------------------------");
            Logger.Info($"------ Scenario {scenarioContext.ScenarioInfo.Title} is running");
            Logger.Info($"Browser: {browserType}");
            Logger.Info($"HeadlessMode: {headless}");
            _extentscenario = _extentfeature.CreateNode(scenarioContext.ScenarioInfo.Title);
            scenarioContext.Add("Page", Page);
        }

        [AfterScenario]
        public async Task AfterScenarioAsync()
        {
            Logger.Info($"Step: {scenarioContext.ScenarioInfo.Title} is complete.");
            Logger.Info("----------------------------------------------------------------------------");
            
            UserAccountManager.ReleaseUserAccount(scenarioContext["CurrentUser"].ToString());
            if (featureContext.FeatureInfo.Tags.Contains("Backend")) return;

            // Take screenshot and stop tracing            
            var filename = scenarioContext.ScenarioInfo.Title.Replace(" ", string.Empty);
            await Page.ScreenshotAsync(new()
            {
                Path = $"{ProjectDirectory}/TestResults/{filename}.png",
                FullPage = true,
            });

            var failed = new[] { UnitTestOutcome.Failed, UnitTestOutcome.Error, UnitTestOutcome.Timeout, UnitTestOutcome.Aborted }.Contains(TestContext.CurrentTestOutcome);
            if (failed)
            {
                await BrowserContext.Tracing.StopAsync(new()
                {
                    Path = $"{ProjectDirectory}/TestResults/{filename}.zip"
                });
            }

            await Page.CloseAsync();
        }

        [AfterStep]
        public void AfterStep()
        {
            var failed = new[] { UnitTestOutcome.Failed, UnitTestOutcome.Error, UnitTestOutcome.Timeout, UnitTestOutcome.Aborted }.Contains(TestContext.CurrentTestOutcome);

            if (failed)
            {
                Logger.Error($"Step: {scenarioContext.StepContext.StepInfo.Text} failed with error message - " + scenarioContext.TestError.Message);
                _extentscenario.CreateNode(scenarioContext.StepContext.StepInfo.Text).Fail(scenarioContext.TestError.Message);
            }
            else
            {
                Logger.Info($"Step: {scenarioContext.StepContext.StepInfo.Text} is passed");
                _extentscenario.CreateNode(scenarioContext.StepContext.StepInfo.Text);
            }
            
        }

        private static async Task<IBrowser> SetupBrowser(string browserType, bool headless)

        {
            var playwright = await Playwright.CreateAsync();

            IBrowser browser;
            switch (browserType.ToLower())
            {
                case "chrome":
                    browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Channel = "chrome",
                        Headless = headless
                    });
                    break;
                case "msedge":
                    browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Channel = "msedge",
                        Headless = headless
                    });
                    break;
                case "chromium":
                    browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = headless
                    });
                    break;
                case "webkit":
                    browser = await playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = headless
                    });
                    break;
                default:
                    throw new ArgumentException($"Unsupported browser type: {browserType}");
            }

            return browser;
        }
    }
}