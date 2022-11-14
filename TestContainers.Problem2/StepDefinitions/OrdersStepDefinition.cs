namespace TestContainers.Problem2.StepDefinitions
{
    using System;
    using System.Collections.Generic;

    using TestContainers.Shared.Contexts;
    using TestContainers.Shared.Models;
    using TestContainers.Shared.Services;

    [Binding]
    internal class OrdersStepDefinition
    {
        private readonly OrderService _orderService;
        private readonly StoreContext _storeContext;
        private IEnumerable<Order>? _orders;

        public OrdersStepDefinition(
            OrderService orderService,
            StoreContext storeContext)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _storeContext = storeContext ?? throw new ArgumentNullException(nameof(storeContext));
        }

        [Given(@"all available orders in the store")]
        public void GivenAllAvailableOrdersInTheStore()
        {
            // 5) Retrieve all orders
            _orders = null;
        }

        [Then(@"there should be no orders placed for a stale account")]
        public void ThenThereShouldBeNoOrdersPlacedForAStaleAccount()
        {
            _orders?.Should().OnlyContain(order => order.User!.LastLogin != null);
        }
    }
}
