using System;
using System.Collections.Generic;

namespace TestContainers.Shared.Models
{
    public partial class Account
    {
        public Account()
        {
            Orders = new HashSet<Order>();
        }

        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime CreatedOn { get; set; }
        public DateTime? LastLogin { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
