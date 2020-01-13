using System;
using System.Collections.Generic;

namespace Haukcode.DatabaseUtils
{
    public class EnumStoredCache
    {
        public Dictionary<Type, IList<EnumValue>> CacheData { get; set; }

        public string Hash { get; set; }
    }
}
