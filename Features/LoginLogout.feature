Feature: LoginLogout

As a user of the system
I would want to buy few items
So that I can use them later

Scenario: Login and Logout
	Given Login to the application
	When Open menu and click on Logout
	Then Validate user is logged out
