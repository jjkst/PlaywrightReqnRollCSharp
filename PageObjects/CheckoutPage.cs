using Microsoft.Playwright;
using PlaywrightReqnRollCSharp.Support;

namespace PlaywrightReqnRollCSharp.PageObjects;

public class CheckoutPage(PlaywrightContext context) : BasePage(context)
{
    private ILocator FirstName => _page.Locator("[data-test='firstName']");
    private ILocator LastName => _page.Locator("[data-test='lastName']");
    private ILocator PostalCode => _page.Locator("[data-test='postalCode']");

    private ILocator ContinueButton => _page.Locator("[data-test='continue']");
    private ILocator CancelButton => _page.Locator("[data-test='cancel']");

    private ILocator InventoryList => _page.Locator("[data-test='inventory-item']");

    private ILocator InventoryListNames => _page.Locator("[data-test='inventory-item-name']");
    private ILocator FinishButton => _page.Locator("[data-test='finish']");

    private ILocator CompleteHeader => _page.Locator("[data-test='complete-header']");

    public async Task FillCheckout(string fname, string lname, string zipcode)
    {
        await FirstName.FillAsync(fname);
        await LastName.FillAsync(lname);
        await PostalCode.FillAsync(zipcode);
    }

    public async Task ClickContinue()
    {
        await ContinueButton.ClickAsync();
    }

    public async Task<List<string>> GetCheckoutItems()
    {
        var producs = await InventoryListNames.AllInnerTextsAsync();
        return [.. producs];
    }

    public async Task ClickFinish()
    {
        await FinishButton.ClickAsync();
    }

    public async Task<string> GetCompleteOrder()
    {
       return await CompleteHeader.TextContentAsync();
    }

    public async Task RemoveProductFromCheckout(string productName)
    {
        var remove = InventoryList.Filter(new() { HasText = productName }).GetByRole(AriaRole.Button, new() { Name = "Remove" });
        await remove.ClickAsync();
    }
}
