using Microsoft.Playwright;
using PlaywrightReqnRollCSharp.Support;

namespace PlaywrightReqnRollCSharp.PageObjects;

public class ProductsPage(PlaywrightContext context) : BasePage(context)
{
    private ILocator Title => _page.Locator("[data-test='title']");
    private ILocator InventoryList => _page.Locator("[data-test='inventory-item']");
    private ILocator InventoryItemName => _page.Locator("[data-test='inventory-item-name']");

    public async Task<string> GetTitle()
    {
        await Title.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        return await Title.TextContentAsync();
    }
    public async Task AddProductToCart(string productName)
    {
        var addToCartButton = InventoryList.Filter(new() { HasText = productName }).GetByRole(AriaRole.Button, new() { Name = "Add to cart" });
        await addToCartButton.ClickAsync();
    }

    public async Task<List<string>> GetAllProductNames()
    {
        var producs = await InventoryItemName.AllInnerTextsAsync();
        return [.. producs];
    }
}
