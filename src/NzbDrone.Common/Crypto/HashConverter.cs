using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NzbDrone.Common.Crypto
{
    public static class HashConverter
    {
        private static SHA1 HashAlgorithm = SHA1.Create();

        public static int GetHashInt31(string target)
        {
            var hash = HashAlgorithm.ComputeHash(Encoding.Default.GetBytes(target));

            return BitConverter.ToInt32(hash, 0) & 0x7fffffff;
        }
    }
}
