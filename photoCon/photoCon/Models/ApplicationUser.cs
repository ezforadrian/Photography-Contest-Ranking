using Microsoft.AspNetCore.Identity;

namespace photoCon.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        public string? Department { get; set; } = string.Empty;
        public string? Position { get; set; } = string.Empty;
        public string? PayClass { get; set; } = string.Empty;
        public DateTime? LastLoginDateTime { get; set; }

    }
}
