using System;
using System.IO;
using System.Linq;

namespace NzbDrone.Core.TransferProviders.Providers
{
    public class EmptyVirtualDiskProvider : IVirtualDiskProvider
    {
        public bool SupportStreaming => true;

        public string[] GetFiles()
        {
            return new string[0];
        }

        public TransferTask MoveFile(string vfsSourcePath, string destinationPath)
        {
            throw new FileNotFoundException("File not found in virtual filesystem", vfsSourcePath);
        }

        public TransferTask CopyFile(string vfsSourcePath, string destinationPath)
        {
            throw new FileNotFoundException("File not found in virtual filesystem", vfsSourcePath);
        }

        public Stream OpenFile(string vfsFilePath)
        {
            throw new FileNotFoundException("File not found in virtual filesystem", vfsFilePath);
        }
    }
}
