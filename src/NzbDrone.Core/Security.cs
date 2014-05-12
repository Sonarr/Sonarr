using System.Security.Cryptography;
using System.Text;

namespace NzbDrone.Core
{
    public static class Security
    {
        public static string SHA256Hash(this string input)
        {
            var stringBuilder = new StringBuilder();

            using (var hash = SHA256Managed.Create())
            {
                var enc = Encoding.UTF8;
                var result = hash.ComputeHash(enc.GetBytes(input));

                foreach (var b in result)
                {
                    stringBuilder.Append(b.ToString("x2"));
                }
            }

            return stringBuilder.ToString();
        }
    }
}
