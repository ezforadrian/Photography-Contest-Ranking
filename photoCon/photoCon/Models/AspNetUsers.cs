using Microsoft.AspNetCore.Identity;

namespace photoCon.Models
{
    public partial class AspNetUsers
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? UserName { get; set; } = string.Empty;
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        public string? Department { get; set; } = string.Empty;
        public string? Position { get; set; } = null;
        public string? PayClass { get; set; } = null;
        public string? NormalizedUserName { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? NormalizedEmail { get; set; } = string.Empty;
        public bool? EmailConfirmed { get; set; } = true;
        public string? PasswordHash { get; set; } = string.Empty;
        public string? SecurityStamp { get; set; } = string.Empty;
        public string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
        public string? PhoneNumber { get; set; } = null;
        public bool? PhoneNumberConfirmed { get; set; } = false;
        public bool? TwoFactorEnabled { get; set; } = false;
        public DateTimeOffset? LockoutEnd { get; set; } = null;
        public bool? LockoutEnabled { get; set; } = true;
        public int? AccessFailedCount { get; set; } = 0;
    }
}
