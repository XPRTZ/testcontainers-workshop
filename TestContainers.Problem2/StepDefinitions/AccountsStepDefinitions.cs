namespace TestContainers.Problem2.StepDefinitions
{
    using TestContainers.Shared.Contexts;
    using TestContainers.Shared.Models;
    using TestContainers.Shared.Services;

    [Binding]
    public sealed class AccountsStepDefinitions
    {
        private readonly AccountService _accountService;
        private readonly StoreContext _storeContext;
        private IEnumerable<Account>? _accounts;
        private int _numberOfAccounts;
        private int _numberOfAccountsRemoved;

        public AccountsStepDefinitions(
            AccountService accountService,
            StoreContext storeContext)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _storeContext = storeContext ?? throw new ArgumentNullException(nameof(storeContext));
        }

        [Given(@"all available accounts in the store")]
        public void GivenAllAvailableAccountsInTheStore()
        {
            // 3) Retrieve all the acounts from the accountService or using a raw query from the StoreContext:
            // _storeContext.Database.ExecuteSqlInterpolated($"SELECT TOP {5} FROM accounts");
            _accounts = null;
        }

        [When(@"all the stale accounts are removed")]
        public void WhenAllTheStaleAccountsAreRemoved()
        {
            // 4) Remove all stale accounts. A stale account is an account where the field last_login is null
            // the result of either the service call or the SQL query is the number of rows affected
            _numberOfAccountsRemoved = 0;
        }

        [When(@"all the accounts are counted")]
        public void WhenAllTheAccountsAreCounted()
        {
            _numberOfAccounts = _accounts?.Count() ?? -1;
        }

        [Then(@"there should be (.*) accounts left")]
        public void ThenThereShouldBeAccountsLeft(int accountsLeft)
        {
            accountsLeft.Should().Be(_numberOfAccounts - _numberOfAccountsRemoved);
        }
    }
}