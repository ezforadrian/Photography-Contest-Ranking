using Microsoft.AspNetCore.Identity;

namespace photoCon.Models
{
    public partial class AspNetUserRoles
    {
        public string? UserId { get; set; } = string.Empty;
        public string? RoleId { get; set; } = string.Empty;
    }
}
