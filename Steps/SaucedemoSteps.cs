using AventStack.ExtentReports.Gherkin.Model;
using Microsoft.Playwright;
using PlaywrightReqnRollCSharp.PageObjects;
using Reqnroll;

namespace PlaywrightReqnRollCSharp.Steps;

[Binding]
public sealed class SaucedemoSteps(ScenarioContext scenarioContext, ProductsPage productsPage, CartPage cartPage, CheckoutPage checkoutPage)
{

    [Then("Validate all products exist")]
    public async Task ThenValidateAllProductsExist(DataTable dataTable)
    {
        Assert.AreEqual("Products", await productsPage.GetTitle());

        var expectedProducts = dataTable.Rows.Select(r => r["ProductName"].ToString()).ToList();
        var actualProducts = await productsPage.GetAllProductNames();
        CollectionAssert.AreEquivalent(expectedProducts, actualProducts);
    }

    [When("Add products to cart")]
    public async Task WhenAddProductsToCart(DataTable dataTable)
    {
        foreach (var row in dataTable.Rows)
        {
            await productsPage.AddProductToCart(row["ProductName"].ToString());
        }
    }

    [When("Go to Cart")]
    public async Task WhenGoToCart()
    {
        await cartPage.GotoCart();
    }

    [Then("Validate products in cart")]
    public async Task ThenValidateProductsInCart(DataTable dataTable)
    {
        var expected = new List<string>();
        foreach (var row in dataTable.Rows)
        {
            expected.Add(row["ProductName"].ToString());
        }

        var actual = await cartPage.GetCartItems();

        CollectionAssert.AreEquivalent(expected, actual);
    }

    [When("Click on checkout")]
    public async Task WhenClickOnCheckout()
    {
        await cartPage.ClickCheckout();
    }

    [When("Checkout")]
    public async Task WhenCheckout(DataTable dataTable)
    {
        var record = dataTable.Rows[0];
        await checkoutPage.FillCheckout(record["FirstName"].ToString(), record["LastName"].ToString(), record["PostalCode"].ToString());
        await checkoutPage.ClickContinue();
    }

    [Then("Validate products in checkout")]
    public async Task ThenValidateProductsInCheckout(DataTable dataTable)
    {
        var expected = new List<string>();
        foreach (var row in dataTable.Rows)
        {
            expected.Add(row["ProductName"].ToString());
        }
    
        var actual = await checkoutPage.GetCheckoutItems();

        CollectionAssert.AreEquivalent(expected, actual);
    }

    [Then("Complete order")]
    public async Task ThenCompleteOrder()
    {
        await checkoutPage.ClickFinish();
    }

    [Then("Validate order is complete (.*)")]
    public async Task ThenValidateOrderIsComplete(string msg)
    {
        Assert.AreEqual("Thank you for your order!", await checkoutPage.GetCompleteOrder());
    }

    [When("Remove product from cart")]
    public async Task WhenRemoveProductFromCart(DataTable dataTable)
    {
        foreach (var row in dataTable.Rows)
        {
            await cartPage.RemoveProductFromCart(row["ProductName"].ToString());
        }
    }

    [Then("Validate product {string} is remove in checkout")]
    public async Task ThenValidateProductIsRemoveInCheckout(string product)
    {
        var actual = await checkoutPage.GetCheckoutItems();
        Assert.DoesNotContain(product, actual);
    }

    [Then("Remove product from checkout")]
    public async Task ThenRemoveProductFromCheckout(DataTable dataTable)
    {
        foreach (var row in dataTable.Rows)
        {
            await checkoutPage.RemoveProductFromCheckout(row["ProductName"].ToString());
        }
    }

}