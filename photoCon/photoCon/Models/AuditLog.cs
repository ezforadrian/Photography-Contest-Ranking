using Microsoft.AspNetCore.Identity;

namespace photoCon.Models
{
    public partial class AuditLog
    {
        public int LogID { get; set; }
        public string? LogType { get; set; } = string.Empty;
        public string? LogProcedure { get; set; } = string.Empty;
        public int? LogErrNumber { get; set; } = 0;
        public string? LogDescription { get; set; } = string.Empty;
        public string? DateTimeCreated { get; set; } = "1/1/2024";
        public string? CreatedBy { get; set; } = string.Empty;
    }
}
