using Microsoft.AspNetCore.Identity;

namespace Jumia.Jumia.Domain.Models
{
    public class Role : IdentityRole
    {
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}