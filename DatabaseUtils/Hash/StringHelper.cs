using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Haukcode.DatabaseUtils
{
    public static partial class StringHelper
    {
        public static string Sha1String(string input)
        {
            using (var sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));

                return string.Join("", hash.Select(x => x.ToString("x2")));
            }
        }

        public static string Sha1Bytes(byte[] input)
        {
            using (var sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(input);

                return string.Join("", hash.Select(x => x.ToString("x2")));
            }
        }
    }
}
