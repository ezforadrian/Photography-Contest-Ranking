namespace photoCon.Models
{
    public class vm_UserDetails
    {
        public string? UserRole { get; set; } = null;
        public string? UserName { get; set; } = string.Empty;

        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        public string? Department { get; set; } = string.Empty;
        public string? PayClass { get; set; } = null;
        public string? Email { get; set; } = string.Empty;
        public string? Password { get; set; } = string.Empty;

        public string? PasswordNew { get; set; } = string.Empty; //Used for Changing Password
        public string? PasswordExt { get; set; } = string.Empty; //
    }
}
