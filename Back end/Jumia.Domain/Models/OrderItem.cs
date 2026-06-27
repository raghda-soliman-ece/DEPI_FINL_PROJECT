namespace Jumia.Jumia.Domain.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? PictureUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
