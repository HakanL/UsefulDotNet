using System;

namespace Haukcode.DatabaseUtils
{
    public static class DateTimeHelper
    {
        public static DateTime SpecifyKind(DateTime input, DateTimeKind kind)
        {
            return DateTime.SpecifyKind(input, kind);
        }

        public static DateTime? SpecifyKind(DateTime? input, DateTimeKind kind)
        {
            if (!input.HasValue)
                return null;

            return DateTime.SpecifyKind(input.Value, kind);
        }
    }
}
