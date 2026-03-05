using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Novell.Directory.Ldap;
using photoCon.Data;
using photoCon.Helper;
using photoCon.Interface;
using photoCon.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.DirectoryServices;
using System.Formats.Asn1;
using System.Linq;
using System.Security.Claims;

namespace photoCon.Repository
{
    public class AuditLogsRepository : IAuditLogsRepository
    {
        private readonly TallyProgramContext _tallyProgramContext;

        public AuditLogsRepository(TallyProgramContext tallyProgramContext)
        {
            _tallyProgramContext = tallyProgramContext;
        }

        public bool SystemAuditLog(string LogType, string Procedure, int ErrorNumber, string Description, string UserID) {
            AuditLog logDetails = new AuditLog {
                LogType = LogType,
                LogProcedure = Procedure,
                LogErrNumber = ErrorNumber,
                LogDescription = Description,
                DateTimeCreated = DateTime.Now.ToString(),
                CreatedBy = UserID
            };

            _tallyProgramContext.AuditLog.Add(logDetails);
            return Save();
        }

        public bool Save() {
            return _tallyProgramContext.SaveChanges() > 0;
        }
    }
}
