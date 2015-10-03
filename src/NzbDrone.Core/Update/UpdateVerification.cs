using System;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Update
{
    public interface IVerifyUpdates
    {
        bool Verify(UpdatePackage updatePackage, string packagePath);
    }

    public class UpdateVerification : IVerifyUpdates
    {
        private readonly IDiskProvider _diskProvider;

        public UpdateVerification(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        public bool Verify(UpdatePackage updatePackage, string packagePath)
        {
            using (var fileStream = _diskProvider.OpenReadStream(packagePath))
            {
                var hash = fileStream.SHA256Hash();

                return hash.Equals(updatePackage.Hash, StringComparison.CurrentCultureIgnoreCase);
            }
        }
    }
}
