using System.Collections.Generic;

namespace Jumia.DTOs
{
    public class BasketDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ICollection<BasketItemDto> Items { get; set; } = new List<BasketItemDto>();
        public decimal TotalPrice { get; set; }
    }
}
