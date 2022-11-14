Feature: Orders

Scenario: There should be no orders for stale accounts
	Given all available orders in the store
	Then there should be no orders placed for a stale account
