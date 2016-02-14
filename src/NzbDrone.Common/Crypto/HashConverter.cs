using System;
using System.Security.Cryptography;
using System.Text;

namespace NzbDrone.Common.Crypto
{
    public static class HashConverter
    {
        private static readonly SHA1 Sha1 = SHA1.Create();

        public static int GetHashInt31(string target)
        {
            var hash = GetHash(target);
            return BitConverter.ToInt32(hash, 0) & 0x7fffffff;
        }

        public static byte[] GetHash(string target)
        {
            lock (Sha1)
            {
                return Sha1.ComputeHash(Encoding.Default.GetBytes(target));
            }
        }
    }
}
