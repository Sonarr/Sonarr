using System;
using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IRecycleBinProvider
    {
        void DeleteFolder(string path);
        void DeleteFile(string path);
        void Empty();
        void Cleanup();
    }

    public class RecycleBinProvider : IHandleAsync<SeriesDeletedEvent>, IExecute<CleanUpRecycleBinCommand>, IRecycleBinProvider
    {
        private readonly IDiskTransferService _diskTransferService;
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;


        public RecycleBinProvider(IDiskTransferService diskTransferService,
                                  IDiskProvider diskProvider,
                                  IConfigService configService,
                                  Logger logger)
        {
            _diskTransferService = diskTransferService;
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        public void DeleteFolder(string path)
        {
            _logger.Info("Attempting to send '{0}' to recycling bin", path);
            var recyclingBin = _configService.RecycleBin;

            if (string.IsNullOrWhiteSpace(recyclingBin))
            {
                _logger.Info("Recycling Bin has not been configured, deleting permanently. {0}", path);
                _diskProvider.DeleteFolder(path, true);
                _logger.Debug("Folder has been permanently deleted: {0}", path);
            }

            else
            {
                var destination = Path.Combine(recyclingBin, new DirectoryInfo(path).Name);

                _logger.Debug("Moving '{0}' to '{1}'", path, destination);
                _diskTransferService.TransferFolder(path, destination, TransferMode.Move);

                _logger.Debug("Setting last accessed: {0}", path);
                _diskProvider.FolderSetLastWriteTime(destination, DateTime.UtcNow);
                foreach (var file in _diskProvider.GetFiles(destination, SearchOption.AllDirectories))
                {
                    if (OsInfo.IsWindows)
                    {
                        //TODO: Better fix than this for non-Windows?
                        _diskProvider.FileSetLastWriteTime(file, DateTime.UtcNow);
                    }
                }

                _logger.Debug("Folder has been moved to the recycling bin: {0}", destination);
            }
        }

        public void DeleteFile(string path)
        {
            _logger.Debug("Attempting to send '{0}' to recycling bin", path);
            var recyclingBin = _configService.RecycleBin;

            if (string.IsNullOrWhiteSpace(recyclingBin))
            {
                _logger.Info("Recycling Bin has not been configured, deleting permanently. {0}", path);

                if (OsInfo.IsWindows)
                {
                    _logger.Debug(_diskProvider.GetFileAttributes(path));
                }

                _diskProvider.DeleteFile(path);
                _logger.Debug("File has been permanently deleted: {0}", path);
            }

            else
            {
                var fileInfo = new FileInfo(path);
                var destination = Path.Combine(recyclingBin, fileInfo.Name);

                var index = 1;
                while (_diskProvider.FileExists(destination))
                {
                    index++;
                    if (fileInfo.Extension.IsNullOrWhiteSpace())
                    {
                        destination = Path.Combine(recyclingBin, fileInfo.Name + "_" + index);
                    }
                    else
                    {
                        destination = Path.Combine(recyclingBin, Path.GetFileNameWithoutExtension(fileInfo.Name) + "_" + index + fileInfo.Extension);
                    }
                }

                try
                {
                    _logger.Debug("Moving '{0}' to '{1}'", path, destination);
                    _diskTransferService.TransferFile(path, destination, TransferMode.Move);
                }
                catch (IOException e)
                {
                    _logger.Error(e, "Unable to move '{0}' to the recycling bin: '{1}'", path, destination);
                    throw;
                }
                
                //TODO: Better fix than this for non-Windows?
                if (OsInfo.IsWindows)
                {
                    _diskProvider.FileSetLastWriteTime(destination, DateTime.UtcNow);
                }

                _logger.Debug("File has been moved to the recycling bin: {0}", destination);
            }
        }

        public void Empty()
        {
            if (string.IsNullOrWhiteSpace(_configService.RecycleBin))
            {
                _logger.Info("Recycle Bin has not been configured, cannot empty.");
                return;
            }

            _logger.Info("Removing all items from the recycling bin");

            foreach (var folder in _diskProvider.GetDirectories(_configService.RecycleBin))
            {
                _diskProvider.DeleteFolder(folder, true);
            }

            foreach (var file in _diskProvider.GetFiles(_configService.RecycleBin, SearchOption.TopDirectoryOnly))
            {
                _diskProvider.DeleteFile(file);
            }

            _logger.Debug("Recycling Bin has been emptied.");
        }

        public void Cleanup()
        {
            if (string.IsNullOrWhiteSpace(_configService.RecycleBin))
            {
                _logger.Info("Recycle Bin has not been configured, cannot cleanup.");
                return;
            }

            _logger.Info("Removing items older than 7 days from the recycling bin");

            foreach (var folder in _diskProvider.GetDirectories(_configService.RecycleBin))
            {
                if (_diskProvider.FolderGetLastWrite(folder).AddDays(7) > DateTime.UtcNow)
                {
                    _logger.Debug("Folder hasn't expired yet, skipping: {0}", folder);
                    continue;
                }

                _diskProvider.DeleteFolder(folder, true);
            }

            foreach (var file in _diskProvider.GetFiles(_configService.RecycleBin, SearchOption.TopDirectoryOnly))
            {
                if (_diskProvider.FileGetLastWrite(file).AddDays(7) > DateTime.UtcNow)
                {
                    _logger.Debug("File hasn't expired yet, skipping: {0}", file);
                    continue;
                }

                _diskProvider.DeleteFile(file);
            }

            _logger.Debug("Recycling Bin has been cleaned up.");
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            if (message.DeleteFiles)
            {
                if (_diskProvider.FolderExists(message.Series.Path))
                {
                    DeleteFolder(message.Series.Path);
                }
            }
        }

        public void Execute(CleanUpRecycleBinCommand message)
        {
            Cleanup();
        }
    }
}
