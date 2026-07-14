namespace Jumia.Jumia.Domain.Models
{
    public class ShippingAddress
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
