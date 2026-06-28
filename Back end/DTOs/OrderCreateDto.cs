using System.ComponentModel.DataAnnotations;

namespace Jumia.DTOs
{
    public class OrderCreateDto
    {
        [Required(ErrorMessage = "DeliveryMethodId is required")]
        public int DeliveryMethodId { get; set; }

        [Required(ErrorMessage = "ShippingAddress is required")]
        public ShippingAddressDto ShippingAddress { get; set; } = null!;
    }
}
