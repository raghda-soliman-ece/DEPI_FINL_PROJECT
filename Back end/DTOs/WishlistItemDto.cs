using System;

namespace Jumia.DTOs
{
    public class WishlistItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductPictureUrl { get; set; }
        public decimal ProductPrice { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
