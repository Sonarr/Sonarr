using System;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.TransferProviders.Providers
{
    // Indicates that the remote path is mounted locally, and thus should honor the DownloadItem isReadonly flag and may transfer slowly.

    public class MountSettings : IProviderConfig
    {
        public string DownloadClientPath { get; set; }
        public string MountPath { get; set; }

        public NzbDroneValidationResult Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class MountTransfer : TransferProviderBase<MountSettings>
    {
        public override string Link
        {
            get { throw new NotImplementedException(); }
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        public override ValidationResult Test()
        {
            throw new NotImplementedException();
        }
    }
}
