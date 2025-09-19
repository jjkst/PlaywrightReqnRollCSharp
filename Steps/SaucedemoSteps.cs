using AventStack.ExtentReports.Gherkin.Model;
using Microsoft.Playwright;
using PlaywrightMSTests.PageObjects;
using Reqnroll;

namespace PlaywrightMSTests.Steps
{
    [Binding]
    public sealed class SaucedemoSteps(ScenarioContext scenarioContext)
    {
        IPage _page = (IPage)scenarioContext["Page"];

        [Then("Validate all products exist")]
        public async Task ThenValidateAllProductsExist(DataTable dataTable)
        {
            var page = new ProductsPage(_page);
            Assert.AreEqual("Products", await page.GetTitle());

            var expectedProducts = dataTable.Rows.Select(r => r["ProductName"].ToString()).ToList();
            var actualProducts = await page.GetAllProductNames();
            CollectionAssert.AreEquivalent(expectedProducts, actualProducts);
        }

        [When("Add products to cart")]
        public async Task WhenAddProductsToCart(DataTable dataTable)
        {
            var page = new ProductsPage(_page);
            foreach (var row in dataTable.Rows)
            {
                await page.AddProductToCart(row["ProductName"].ToString());
            }
        }

        [When("Go to Cart")]
        public async Task WhenGoToCart()
        {
            var page = new CartPage(_page);
            await page.GotoCart();
        }

        [Then("Validate products in cart")]
        public async Task ThenValidateProductsInCart(DataTable dataTable)
        {
            var expected = new List<string>();
            foreach (var row in dataTable.Rows)
            {
                expected.Add(row["ProductName"].ToString());
            }

            var page = new CartPage(_page);
            var actual = await page.GetCartItems();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [When("Click on checkout")]
        public async Task WhenClickOnCheckout()
        {
            var page = new CartPage(_page);
            await page.ClickCheckout();
        }

        [When("Checkout")]
        public async Task WhenCheckout(DataTable dataTable)
        {
            var page = new CheckoutPage(_page);
            var record = dataTable.Rows[0];
            await page.FillCheckout(record["FirstName"].ToString(), record["LastName"].ToString(), record["PostalCode"].ToString());
            await page.ClickContinue();
        }

        [Then("Validate products in checkout")]
        public async Task ThenValidateProductsInCheckout(DataTable dataTable)
        {
            var expected = new List<string>();
            foreach (var row in dataTable.Rows)
            {
                expected.Add(row["ProductName"].ToString());
            }

            var page = new CheckoutPage(_page);
            var actual = await page.GetCheckoutItems();

            CollectionAssert.AreEquivalent(expected, actual);
        }

        [Then("Complete order")]
        public async Task ThenCompleteOrder()
        {
            var page = new CheckoutPage(_page);
            await page.ClickFinish();
        }

        [Then("Validate order is complete (.*)")]
        public async Task ThenValidateOrderIsComplete(string msg)
        {
            var page = new CheckoutPage(_page);
            Assert.AreEqual("Thank you for your order!", await page.GetCompleteOrder());
        }

        [When("Remove product from cart")]
        public async Task WhenRemoveProductFromCart(DataTable dataTable)
        {
            var page = new CartPage(_page);
            foreach (var row in dataTable.Rows)
            {
                await page.RemoveProductFromCart(row["ProductName"].ToString());
            }
        }

        [Then("Validate product {string} is remove in checkout")]
        public async Task ThenValidateProductIsRemoveInCheckout(string product)
        {
            var page = new CheckoutPage(_page);
            var actual = await page.GetCheckoutItems();
            Assert.DoesNotContain(product, actual);
        }

        [Then("Remove product from checkout")]
        public async Task ThenRemoveProductFromCheckout(DataTable dataTable)
        {
            var page = new CheckoutPage(_page);
            foreach (var row in dataTable.Rows)
            {
                await page.RemoveProductFromCheckout(row["ProductName"].ToString());
            }
        }


    }
}