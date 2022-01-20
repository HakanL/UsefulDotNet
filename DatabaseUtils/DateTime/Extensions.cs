using System;
using Haukcode.UsefulDotNet;

namespace Haukcode.DatabaseUtils.Extensions
{
    public static partial class Extensions
    {
        public static DateTime SpecifyKind(this DateTime input, DateTimeKind kind)
        {
            return DateTimeHelper.SpecifyKind(input, kind);
        }

        public static DateTime? SpecifyKind(this DateTime? input, DateTimeKind kind)
        {
            return DateTimeHelper.SpecifyKind(input, kind);
        }
    }
}
