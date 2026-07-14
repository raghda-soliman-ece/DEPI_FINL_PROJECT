using Microsoft.AspNetCore.Identity;

namespace Jumia.Jumia.Domain.Models
{
    public class AppUser : IdentityUser
    {
        // الخصائص الإضافية فقط
        public string DisplayName { get; set; } = string.Empty;
        public string? Address { get; set; }

        // Navigation properties
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public Basket? Basket { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}