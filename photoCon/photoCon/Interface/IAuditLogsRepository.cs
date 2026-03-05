using photoCon.Models;

namespace photoCon.Interface
{
    public interface IAuditLogsRepository
    {
        bool SystemAuditLog(string LogType, string Procedure, int ErrorNumber, string Description, string UserID);
        bool Save();
    }
}