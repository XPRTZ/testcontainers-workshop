Feature: Accounts

Scenario: Removing stale accounts
	Given all available accounts in the store
	When all the accounts are counted
	And all the stale accounts are removed
	Then there should be 852 accounts left