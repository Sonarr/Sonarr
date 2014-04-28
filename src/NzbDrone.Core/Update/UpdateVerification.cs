using System;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.Update
{
    public interface IVerifyUpdates
    {
        Boolean Verify(UpdatePackage updatePackage, String packagePath);
    }

    public class UpdateVerification : IVerifyUpdates
    {
        private readonly IDiskProvider _diskProvider;

        public UpdateVerification(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        public Boolean Verify(UpdatePackage updatePackage, String packagePath)
        {
            using (var fileStream = _diskProvider.StreamFile(packagePath))
            {
                var hash = fileStream.SHA256Hash();

                return hash.Equals(updatePackage.Hash, StringComparison.CurrentCultureIgnoreCase);
            }
        }
    }
}
