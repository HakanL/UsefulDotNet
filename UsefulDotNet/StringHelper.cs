using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Haukcode.UsefulDotNet
{
    public static class StringHelper
    {
        public static string TrimLength(string value, int maxLength)
        {
            if (value?.Length > maxLength)
                return value[..maxLength];

            return value;
        }

        public static (string FirstName, string LastName) SplitFullName(string input)
        {
            if (input == null)
                return (null, null);

            int pos = input.IndexOf(' ');
            if (pos == -1)
                return (string.Empty, input);

            return (input[..pos], input[(pos + 1)..]);
        }

        public static string Sha1String(string input)
        {
            using var hashImplementation = SHA1.Create();
            byte[] hash = hashImplementation.ComputeHash(Encoding.UTF8.GetBytes(input));

            return string.Join("", hash.Select(x => x.ToString("x2")));
        }

        public static string Sha1Bytes(byte[] input)
        {
            using var hashImplementation = SHA1.Create();
            byte[] hash = hashImplementation.ComputeHash(input);

            return string.Join("", hash.Select(x => x.ToString("x2")));
        }

        public static string Sha256String(string input)
        {
            using var hashImplementation = SHA256.Create();
            byte[] hash = hashImplementation.ComputeHash(Encoding.UTF8.GetBytes(input));

            return string.Join("", hash.Select(x => x.ToString("x2")));
        }

        public static string GetParameterHash(object input)
        {
            using var hasher = SHA256.Create();
            byte[] hashBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(input)));

            return Convert.ToBase64String(hashBytes);
        }
    }
}
