using Microsoft.Playwright;

namespace PlaywrightMSTests.PageObjects;

public class CartPage(IPage page)
{
    private readonly IPage _page = page;

    private ILocator ShopCartLink => _page.Locator("[data-test='shopping-cart-link']");    
    private ILocator InventoryListNames => _page.Locator("[data-test='inventory-item-name']");

    private ILocator InventoryList => _page.Locator("[data-test='inventory-item']");

    private ILocator Checkout => _page.Locator("[data-test='checkout']");

    public async Task GotoCart()
    {
        await ShopCartLink.ClickAsync();
    }

    public async Task<List<string>> GetCartItems()
    {
        var producs = await InventoryListNames.AllInnerTextsAsync();
        return [.. producs];
    }

    public async Task ClickCheckout()
    {
        await Checkout.ClickAsync();
    }
    public async Task RemoveProductFromCart(string productName)
    {
        var remove = InventoryList.Filter(new() { HasText = productName }).GetByRole(AriaRole.Button, new() { Name = "Remove" });
        await remove.ClickAsync();
    }
}
