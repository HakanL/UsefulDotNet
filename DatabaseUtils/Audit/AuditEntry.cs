using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Haukcode.DatabaseUtils
{
    public class AuditEntry : IAuditEntry
    {
        internal AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }

        public string TableName { get; set; }

        public IDictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();

        public IDictionary<string, object> OldValues { get; } = new Dictionary<string, object>();

        public IDictionary<string, object> NewValues { get; } = new Dictionary<string, object>();

        public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();

        public bool HasTemporaryProperties => TemporaryProperties.Any();
    }
}
