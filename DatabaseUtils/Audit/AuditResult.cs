using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Haukcode.DatabaseUtils
{
    public sealed class AuditResult<TAuditDataModel> where TAuditDataModel : class
    {
        public DbSet<TAuditDataModel> AuditTable { get; private set; }

        public IList<AuditEntry> AuditEntries { get; private set; }

        internal AuditResult(DbSet<TAuditDataModel> auditTable, IList<AuditEntry> auditEntries)
        {
            AuditTable = auditTable;
            AuditEntries = auditEntries;
        }
    }
}
