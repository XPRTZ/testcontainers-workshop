namespace TestContainers.Shared.Services
{
    using System;
    using System.Collections.Generic;

    using Microsoft.EntityFrameworkCore;

    using TestContainers.Shared.Contexts;
    using TestContainers.Shared.Models;

    public class AccountService
    {
        private readonly StoreContext _storeContext;

        public AccountService(StoreContext storeContext)
        {
            _storeContext = storeContext ?? throw new ArgumentNullException(nameof(storeContext));
        }

        public IEnumerable<Account> GetAllAccounts()
        {
            return _storeContext.Accounts.AsEnumerable();
        }

        public int RemoveAllStaleAccounts()
        {
            return _storeContext.Database.ExecuteSqlInterpolated($"DELETE FROM accounts WHERE last_login is null AND created_on < '2020-01-01'");
        }
    }
}
