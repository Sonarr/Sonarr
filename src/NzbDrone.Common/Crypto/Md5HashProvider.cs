using System.Security.Cryptography;
using NzbDrone.Common.Disk;

namespace NzbDrone.Common.Crypto
{
    public class Md5HashProvider
    {
        private readonly IDiskProvider _diskProvider;

        public Md5HashProvider(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        public byte[] ComputeHash(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = _diskProvider.StreamFile(path))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }
    }
}