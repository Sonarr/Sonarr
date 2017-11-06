using System;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.TransferProviders.Providers
{
    // Indicates that the files use some custom external transfer method. It's not guaranteed that the files are already available.
    // The IsCopy flag indicates that the files are copied, not mounted. And thus can be safely moved during import, overriding the DownloadItem IsReadOnly flag.
    // This TransferProvider should also have a mechanism for detecting whether the external transfer is in progress. But it should be 'deferred'. (see IsAvailable())

    public class CustomTransferSettings : IProviderConfig
    {
        public string DownloadClientPath { get; set; }
        public string LocalPath { get; set; }

        public bool IsCopy { get; set; }

        public NzbDroneValidationResult Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class CustomTransfer : TransferProviderBase<CustomTransferSettings>
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
