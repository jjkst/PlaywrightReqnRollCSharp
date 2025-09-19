using Microsoft.Playwright;
using NLog.Layouts;

namespace PlaywrightMSTests.PageObjects;

public class LoginPage(IPage page)
{
    private readonly IPage _page = page;
    private ILocator UsernameInput => _page.GetByRole(AriaRole.Textbox, new() { Name = "username" });
    private ILocator PasswordInput => _page.GetByRole(AriaRole.Textbox, new() { Name = "password" });
    private ILocator LoginButton => _page.GetByRole(AriaRole.Button, new() { Name = "Login" });

    private ILocator Menu => _page.GetByRole(AriaRole.Button, new() { Name = "Open Menu" });


    public async Task Login(string username, string password)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
    }

    public async Task OpenMenu()
    {
        await Menu.ClickAsync();
    }

    public async Task ClickLink(string link)
    {
        var menuLink = _page.GetByRole(AriaRole.Link, new() { Name = link });
        await menuLink.ClickAsync();
    }
}
