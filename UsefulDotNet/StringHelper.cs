using System;
using System.Security.Cryptography;
using System.Text;

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

        public static string Sha1String(string input)
        {
            using (var sha1 = new SHA1Managed())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));

                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
