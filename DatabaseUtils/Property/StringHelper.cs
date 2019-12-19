using System;

namespace Haukcode.DatabaseUtils
{
    public static class StringHelper
    {
        public static string TrimLength(string value, int maxLength)
        {
            if (value?.Length > maxLength)
                return value.Substring(0, maxLength);

            return value;
        }
    }
}
