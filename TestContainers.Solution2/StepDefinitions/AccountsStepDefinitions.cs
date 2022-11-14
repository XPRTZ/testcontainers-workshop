namespace TestContainers.Solution2.StepDefinitions
{
    using TestContainers.Container.Database;
    using TestContainers.Shared;
    using TestContainers.Shared.Models;
    using TestContainers.Shared.Services;

    [Binding]
    public sealed class AccountsStepDefinitions
    {
        private readonly AccountService _accountService;

        private IEnumerable<Account>? _accounts;
        private int _numberOfAccounts;
        private int _numberOfAccountsRemoved;

        public AccountsStepDefinitions(
            AccountService accountService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        [Given(@"all available accounts in the store")]
        public void GivenAllAvailableAccountsInTheStore()
        {
            _accounts = _accountService.GetAllAccounts();
        }

        [When(@"all the stale accounts are removed")]
        public void WhenAllTheStaleAccountsAreRemoved()
        {
            _numberOfAccountsRemoved = _accountService.RemoveAllStaleAccounts();
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