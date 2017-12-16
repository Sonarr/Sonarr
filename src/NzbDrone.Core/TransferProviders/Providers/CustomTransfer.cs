using System;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
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
        private readonly Logger _logger;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _transferService;

        public override string Name => "Mount";

        public CustomTransfer(IDiskTransferService transferService, IDiskProvider diskProvider, Logger logger)
        {
            _logger = logger;
            _diskProvider = diskProvider;
            _transferService = transferService;
        }

        public override ValidationResult Test()
        {
            throw new NotImplementedException();
        }

        public override bool IsAvailable(DownloadClientPath item)
        {
            if (item == null) return false;

            var path = ResolvePath(item);
            if (path == null)
            {
                return false;
            }

            return _diskProvider.FolderExists(path) || _diskProvider.FileExists(path);
        }

        // TODO: Give MountVirtualDiskProvider the tempPath.
        public override IVirtualDiskProvider GetFileSystemWrapper(DownloadClientPath item, string tempPath = null)
        {
            var path = ResolvePath(item);

            if (_diskProvider.FolderExists(path) || _diskProvider.FileExists(path))
            {
                // Expose a virtual filesystem with only that directory/file in it.
                // This allows the caller to delete the directory if desired, but not it's siblings.
                return new MountVirtualDiskProvider(_diskProvider, _transferService, Path.GetDirectoryName(path), path);
            }

            return new EmptyVirtualDiskProvider();
        }

        protected string ResolvePath(DownloadClientPath path)
        {
            return ResolvePath(path.Path, Settings.DownloadClientPath, Settings.LocalPath);
        }
    }
}
