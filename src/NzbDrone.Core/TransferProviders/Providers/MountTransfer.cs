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
        private readonly Logger _logger;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _transferService;

        public override string Name => "Mount";

        public MountTransfer(IDiskTransferService transferService, IDiskProvider diskProvider, Logger logger)
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
            var remotePath = path.Path;
            if (new OsPath(Settings.DownloadClientPath).Contains(remotePath))
            {
                var localPath = new OsPath(Settings.MountPath) + (remotePath - new OsPath(Settings.DownloadClientPath));

                return localPath.FullPath;
            }

            return null;
        }
    }
}
