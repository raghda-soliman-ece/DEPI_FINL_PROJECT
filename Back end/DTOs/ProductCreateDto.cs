using System.ComponentModel.DataAnnotations;

namespace Jumia.DTOs
{
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Old price must be greater than 0")]
        public decimal? OldPrice { get; set; }

        public string? PictureUrl { get; set; }

        [Range(0, 10000, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; } = 0;

        [Required(ErrorMessage = "CategoryId is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "BrandId is required")]
        public int BrandId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
