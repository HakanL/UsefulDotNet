using System;
using System.Collections.Generic;

namespace Haukcode.DatabaseUtils
{
    public interface IAuditEntry
    {
        string TableName { get; }

        IDictionary<string, object> KeyValues { get; }

        IDictionary<string, object> OldValues { get; }

        IDictionary<string, object> NewValues { get; }
    }
}
