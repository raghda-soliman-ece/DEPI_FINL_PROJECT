using System;

namespace Jumia.Jumia.Domain.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? Revoked { get; set; }

        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;
    }
}
