using Microsoft.AspNetCore.Identity;

namespace Jumia.Jumia.Domain.Models
{
    public class UserRole : IdentityUserRole<string>
    {
        public virtual AppUser User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}