namespace PlaywrightMSTests.Support
{
    public class EnvironmentVariables
    {
        public string Url { get; set; }
        public string ApiBaseUrl { get; set; }

        public string DBServer { get; set; }

        public EnvironmentVariables(string env)
        {
            switch (env.ToLower())
            {
                case "test":
                    Url = "https://www.saucedemo.com/";
                    ApiBaseUrl = "https://api.saucedemo.com/";
                    DBServer = "localhost-test";
                    break;
                case "prod":
                    Url = "https://www.saucedemo.com/";
                    ApiBaseUrl = "https://api.saucedemo.com/";
                    DBServer = "localhost-prod";
                    break;
                default:
                    throw new ArgumentException($"Unknown environment: {env}");
            }
        }
    }
}
