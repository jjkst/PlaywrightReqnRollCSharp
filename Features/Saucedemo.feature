Feature: Saucedemo

As a user of the system
I would want to check all products exist, buy products and remove product from cart
So that I can use them later

Scenario: Check all products exist
	Given Login to the application	
	Then Validate all products exist
		| ProductName                       |
		| Sauce Labs Onesie                 |
		| Sauce Labs Bike Light             |
		| Sauce Labs Bolt T-Shirt           |
		| Test.allTheThings() T-Shirt (Red) |
		| Sauce Labs Fleece Jacket          |
		| Sauce Labs Backpack               |

Scenario: Buy products
	Given Login to the application
	When Add products to cart
		| ProductName           |
		| Sauce Labs Onesie     |
		| Sauce Labs Bike Light |
	And Go to Cart
	Then Validate products in cart
		| ProductName           |
		| Sauce Labs Onesie     |
		| Sauce Labs Bike Light |
	When Click on checkout
	And Checkout
		| FirstName | LastName | PostalCode |
		| John      | Doe      | 12345      |
	Then Validate products in checkout
		| ProductName           |
		| Sauce Labs Onesie     |
		| Sauce Labs Bike Light |
	And Complete order
	And Validate order is complete Thank you for your order!

Scenario: Remove products
	Given Login to the application
	When Add products to cart
		| ProductName           |
		| Sauce Labs Onesie     |
		| Sauce Labs Bike Light |
	And Go to Cart
	And Remove product from cart
		| ProductName       |
		| Sauce Labs Onesie |
	When Click on checkout
	And Checkout
		| FirstName | LastName | PostalCode |
		| John      | Doe      | 12345      |
	Then Validate product "Sauce Labs Onesie" is remove in checkout