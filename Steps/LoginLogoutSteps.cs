using Microsoft.Playwright;
using NLog;
using PlaywrightReqnRollCSharp.PageObjects;
using PlaywrightReqnRollCSharp.Support;
using Reqnroll;
using System.Threading.Tasks;

namespace PlaywrightReqnRollCSharp.Steps;

[Binding]
public sealed class LoginLogoutSteps(PlaywrightContext page, ScenarioContext scenarioContext, LoginPage loginPage)
{
    private static Logger Logger = LogManager.GetCurrentClassLogger();

    [Given("Login to the application")]
    public async Task GivenLoginToTheApplicationAsync()
    {
        await loginPage.Login(scenarioContext["CurrentUser"].ToString(), scenarioContext["Password"].ToString());
    }

    [When("Open menu and click on (.*)")]
    public async Task WhenOpenMenuAndClickOnLink(string link)
    {
        await loginPage.OpenMenu();
        await loginPage.ClickLink(link);
    }

    [Then("Validate user is logged out")]
    public void ThenValidateUserIsLoggedOut()
    {
        var url = page.CurrentPage.Url;
        Assert.AreEqual("https://www.saucedemo.com/", url);
    }

}