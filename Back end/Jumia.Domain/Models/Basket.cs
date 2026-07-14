using System.Collections.Generic;

namespace Jumia.Jumia.Domain.Models
{
    public class Basket
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        // Navigation properties
        public ICollection<BasketItem> Items { get; set; } = new List<BasketItem>();
    }
}
