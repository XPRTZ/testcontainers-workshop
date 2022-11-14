using System;
using System.Collections.Generic;

namespace TestContainers.Shared.Models
{
    public partial class Item
    {
        public Item()
        {
            Orders = new HashSet<Order>();
        }

        public int ItemId { get; set; }
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
