using System;
using System.Collections.Generic;

namespace Jumia.Jumia.Domain.Models
{
    public class Role
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string? NormalizedName { get; set; }
        public string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        // Navigation properties
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
