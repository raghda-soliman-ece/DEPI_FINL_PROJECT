namespace Jumia.Jumia.Domain.Models
{
    public class UserRole
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public string RoleId { get; set; } = string.Empty;
        public Role Role { get; set; } = null!;
    }
}
