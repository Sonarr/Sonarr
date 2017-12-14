using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Timeline;
using NzbDrone.Common.TPL;

namespace NzbDrone.Core.TransferProviders.Providers
{
    public class DirectVirtualDiskProvider : IVirtualDiskProvider
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _transferService;
        private readonly string _rootFolder;
        private readonly List<string> _items;

        public bool SupportStreaming => true;

        public DirectVirtualDiskProvider(IDiskProvider diskProvider, IDiskTransferService transferService, string rootFolder, params string[] items)
        {
            _diskProvider = diskProvider;
            _transferService = transferService;
            _rootFolder = rootFolder;
            _items = items.ToList();
        }

        public string[] GetFiles()
        {
            return _items.SelectMany(GetFiles).Select(_rootFolder.GetRelativePath).ToArray();
        }

        private string[] GetFiles(string sourcePath)
        {
            if (_diskProvider.FileExists(sourcePath))
            {
                return new [] { sourcePath };
            }
            else
            {
                return _diskProvider.GetFiles(sourcePath, SearchOption.AllDirectories);
            }
        }

        public TransferTask MoveFile(string vfsSourcePath, string destinationPath)
        {
            return TransferFile(vfsSourcePath, destinationPath, TransferMode.Move);
        }

        public TransferTask CopyFile(string vfsSourcePath, string destinationPath)
        {
            return TransferFile(vfsSourcePath, destinationPath, TransferMode.Copy);
        }

        private TransferTask TransferFile(string vfsSourcePath, string destinationPath, TransferMode mode)
        {
            var sourcePath = ResolveVirtualPath(vfsSourcePath);

            var fileSize = _diskProvider.GetFileSize(sourcePath);
            var progress = new TimelineContext($"{mode} {Path.GetFileName(sourcePath)}", 0, fileSize);
            var task = Task.Factory.StartNew(() =>
            {
                progress.UpdateState(TimelineState.Started);
                _transferService.TransferFile(sourcePath, destinationPath, mode);
                if (mode == TransferMode.Move && _items.Contains(vfsSourcePath))
                {
                    // If it was moved, then remove it from the list.
                    _items.Remove(vfsSourcePath);
                }
                progress.FinishProgress();
            });

            return new TransferTask(progress, task);
        }

        public Stream OpenFile(string vfsFilePath)
        {
            var sourcePath = ResolveVirtualPath(vfsFilePath);

            return _diskProvider.OpenReadStream(sourcePath);
        }

        private string ResolveVirtualPath(string virtualPath)
        {
            if (Path.IsPathRooted(virtualPath))
            {
                throw new InvalidOperationException("Path not valid in the virtual filesystem");
            }

            var basePath = virtualPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];

            if (!_items.Contains(basePath))
            {
                throw new InvalidOperationException("Path not valid in the virtual filesystem");
            }

            return Path.Combine(_rootFolder, virtualPath);
        }
    }
}
