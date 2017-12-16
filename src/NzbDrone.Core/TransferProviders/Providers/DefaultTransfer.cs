using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Download;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.TransferProviders.Providers
{
    // Represents a local filesystem transfer.
    class DefaultTransfer : TransferProviderBase<NullConfig>
    {
        private readonly Logger _logger;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _transferService;

        public override string Name => "Default";

        public DefaultTransfer(IDiskTransferService transferService, IDiskProvider diskProvider, Logger logger)
        {
            _logger = logger;
            _diskProvider = diskProvider;
            _transferService = transferService;
        }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                yield return new TransferProviderDefinition
                {
                    Enable = true,
                    Name = "Default",
                    ImplementationName = nameof(DefaultTransfer),
                    Implementation = nameof(DefaultTransfer),
                    Settings = NullConfig.Instance
                };
            }
        }

        public override ValidationResult Test()
        {
            throw new NotImplementedException();
        }

        public override bool IsAvailable(DownloadClientPath item)
        {
            if (item == null) return false;

            var path = ResolvePath(item);

            return _diskProvider.FolderExists(path) || _diskProvider.FileExists(path);
        }

        // TODO: Give DirectVirtualDiskProvider the tempPath.
        public override IVirtualDiskProvider GetFileSystemWrapper(DownloadClientPath item, string tempPath = null)
        {
            var path = ResolvePath(item);

            if (_diskProvider.FolderExists(path) || _diskProvider.FileExists(path))
            {
                // Expose a virtual filesystem with only that directory/file in it.
                // This allows the caller to delete the directory if desired, but not it's siblings.
                return new DirectVirtualDiskProvider(_diskProvider, _transferService, Path.GetDirectoryName(path), path);
            }

            return new EmptyVirtualDiskProvider();
        }

        protected string ResolvePath(DownloadClientPath path)
        {
            return path.Path.FullPath;
        }
    }
}
