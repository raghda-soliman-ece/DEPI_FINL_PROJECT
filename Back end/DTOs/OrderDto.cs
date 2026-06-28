using System;
using System.Collections.Generic;

namespace Jumia.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = "Pending";
        public decimal SubTotal { get; set; }
        public decimal DeliveryPrice { get; set; }
        public string DeliveryMethodName { get; set; } = string.Empty;
        public ShippingAddressDto? ShippingAddress { get; set; }
        public ICollection<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public decimal Total { get; set; }
    }
}
