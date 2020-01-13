using System;

namespace Haukcode.UsefulDotNet
{
    public static class StringHelper
    {
        public static (string FirstName, string LastName) SplitFullName(string input)
        {
            if (input == null)
                return (null, null);

            int pos = input.IndexOf(' ');
            if (pos == -1)
                return (string.Empty, input);

            return (input.Substring(0, pos), input.Substring(pos + 1));
        }
    }
}
