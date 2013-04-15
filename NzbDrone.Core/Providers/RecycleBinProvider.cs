using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Eventing;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Providers
{
    public class RecycleBinProvider : IHandleAsync<SeriesDeletedEvent>
    {
        private readonly DiskProvider _diskProvider;
        private readonly IConfigService _configService;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public RecycleBinProvider(DiskProvider diskProvider, IConfigService configService)
        {
            _diskProvider = diskProvider;
            _configService = configService;
        }

        public RecycleBinProvider()
        {
        }

        public virtual void DeleteDirectory(string path)
        {
            logger.Trace("Attempting to send '{0}' to recycling bin", path);
            var recyclingBin = _configService.RecycleBin;

            if (String.IsNullOrWhiteSpace(recyclingBin))
            {
                logger.Info("Recycling Bin has not been configured, deleting permanently.");
                _diskProvider.DeleteFolder(path, true);
                logger.Trace("Folder has been permanently deleted: {0}", path);
            }

            else
            {
                var destination = Path.Combine(recyclingBin, new DirectoryInfo(path).Name);

                logger.Trace("Moving '{0}' to '{1}'", path, destination);
                _diskProvider.MoveDirectory(path, destination);

                logger.Trace("Setting last accessed: {0}", path);
                _diskProvider.DirectorySetLastWriteTimeUtc(destination, DateTime.UtcNow);
                foreach (var file in _diskProvider.GetFiles(destination, SearchOption.AllDirectories))
                {
                    _diskProvider.FileSetLastWriteTimeUtc(file, DateTime.UtcNow);
                }

                logger.Trace("Folder has been moved to the recycling bin: {0}", destination);
            }
        }

        public virtual void DeleteFile(string path)
        {
            logger.Trace("Attempting to send '{0}' to recycling bin", path);
            var recyclingBin = _configService.RecycleBin;

            if (String.IsNullOrWhiteSpace(recyclingBin))
            {
                logger.Info("Recycling Bin has not been configured, deleting permanently.");
                _diskProvider.DeleteFile(path);
                logger.Trace("File has been permanently deleted: {0}", path);
            }

            else
            {
                var destination = Path.Combine(recyclingBin, new FileInfo(path).Name);

                logger.Trace("Moving '{0}' to '{1}'", path, destination);
                _diskProvider.MoveFile(path, destination);
                _diskProvider.FileSetLastWriteTimeUtc(destination, DateTime.UtcNow);
                logger.Trace("File has been moved to the recycling bin: {0}", destination);
            }
        }

        public virtual void Empty()
        {
            if (String.IsNullOrWhiteSpace(_configService.RecycleBin))
            {
                logger.Info("Recycle Bin has not been configured, cannot empty.");
                return;
            }

            logger.Info("Removing all items from the recycling bin");

            foreach (var folder in _diskProvider.GetDirectories(_configService.RecycleBin))
            {
                _diskProvider.DeleteFolder(folder, true);
            }

            foreach (var file in _diskProvider.GetFiles(_configService.RecycleBin, SearchOption.TopDirectoryOnly))
            {
                _diskProvider.DeleteFile(file);
            }

            logger.Trace("Recycling Bin has been emptied.");
        }

        public virtual void Cleanup()
        {
            if (String.IsNullOrWhiteSpace(_configService.RecycleBin))
            {
                logger.Info("Recycle Bin has not been configured, cannot cleanup.");
                return;
            }

            logger.Info("Removing items older than 7 days from the recycling bin");

            foreach (var folder in _diskProvider.GetDirectories(_configService.RecycleBin))
            {
                if (_diskProvider.GetLastFolderWrite(folder).AddDays(7) > DateTime.UtcNow)
                {
                    logger.Trace("Folder hasn't expired yet, skipping: {0}", folder);
                    continue;
                }

                _diskProvider.DeleteFolder(folder, true);
            }

            foreach (var file in _diskProvider.GetFiles(_configService.RecycleBin, SearchOption.TopDirectoryOnly))
            {
                if (_diskProvider.GetLastFileWrite(file).AddDays(7) > DateTime.UtcNow)
                {
                    logger.Trace("File hasn't expired yet, skipping: {0}", file);
                    continue;
                }

                _diskProvider.DeleteFile(file);
            }

            logger.Trace("Recycling Bin has been cleaned up.");
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            DeleteDirectory(message.Series.Path);
        }
    }
}
