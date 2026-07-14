using System.Collections.Generic;

namespace Jumia.Jumia.Domain.Models
{
    public class DeliveryMethod
    {
        public int Id { get; set; }
        public string ShortName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string DeliveryTime { get; set; } = string.Empty;
        public decimal Price { get; set; }

        // Navigation properties
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
