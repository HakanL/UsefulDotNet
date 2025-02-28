using System;
using Haukcode.UsefulDotNet;

namespace Haukcode.UsefulDotNet.Extensions
{
    public static partial class Extensions
    {
        public static string TrimLength(this string value, int maxLength)
        {
            return StringHelper.TrimLength(value, maxLength);
        }
    }
}
