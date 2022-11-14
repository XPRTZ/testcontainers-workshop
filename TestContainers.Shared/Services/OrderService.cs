namespace TestContainers.Shared.Services
{
    using System;
    using System.Collections.Generic;

    using Microsoft.EntityFrameworkCore;

    using TestContainers.Shared.Contexts;
    using TestContainers.Shared.Models;

    public class OrderService
    {
        private readonly StoreContext _storeContext;

        public OrderService(StoreContext storeContext)
        {
            _storeContext = storeContext ?? throw new ArgumentNullException(nameof(storeContext));
        }

        public IEnumerable<Order> GetAllOrders()
        {
            return _storeContext
                .Orders
                .Include(order => order.User)
                .Include(order => order.Item)
                .AsEnumerable();
        }
    }
}
