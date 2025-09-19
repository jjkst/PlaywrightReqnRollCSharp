using Microsoft.Playwright;
using NLog;
using PlaywrightMSTests.PageObjects;
using Reqnroll;
using System.Threading.Tasks;

namespace PlaywrightMSTests.Steps
{
    [Binding]
    public sealed class LoginLogoutSteps(ScenarioContext scenarioContext)
    {
        IPage _page = (IPage) scenarioContext["Page"];

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        [Given("Login to the application")]
        public async Task GivenLoginToTheApplicationAsync()
        {

            var page = new LoginPage(_page);
            await page.Login(scenarioContext["CurrentUser"].ToString(), scenarioContext["Password"].ToString());
        }

        [When("Open menu and click on (.*)")]
        public async Task WhenOpenMenuAndClickOnLink(string link)
        {
            var page = new LoginPage(_page);
            await page.OpenMenu();
            await page.ClickLink(link);
        }

        [Then("Validate user is logged out")]
        public void ThenValidateUserIsLoggedOut()
        {
            var url = _page.Url;
            Assert.AreEqual("https://www.saucedemo.com/", url);
        }

    }
}