using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Crypto;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.Download.Clients.Blackhole
{
    public interface IScanWatchFolder
    {
        IEnumerable<WatchFolderItem> GetItems(string watchFolder, TimeSpan waitPeriod);
    }

    public class ScanWatchFolder : IScanWatchFolder
    {
        private readonly Logger _logger;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskScanService _diskScanService;
        private readonly ICached<Dictionary<string, WatchFolderItem>>  _watchFolderItemCache;

        public ScanWatchFolder(ICacheManager cacheManager, IDiskScanService diskScanService, IDiskProvider diskProvider, Logger logger)
        {
            _logger = logger;
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
            _watchFolderItemCache = cacheManager.GetCache<Dictionary<string, WatchFolderItem>>(GetType());
        }

        public IEnumerable<WatchFolderItem> GetItems(string watchFolder, TimeSpan waitPeriod)
        {
            var newWatchItems = new Dictionary<string, WatchFolderItem>();
            var lastWatchItems = _watchFolderItemCache.Get(watchFolder, () => newWatchItems);

            foreach (var newWatchItem in GetDownloadItems(watchFolder, lastWatchItems, waitPeriod))
            {
                newWatchItems[newWatchItem.DownloadId] = newWatchItem;
            }

            _watchFolderItemCache.Set(watchFolder, newWatchItems, TimeSpan.FromMinutes(5));

            return newWatchItems.Values;
        }

        private IEnumerable<WatchFolderItem> GetDownloadItems(string watchFolder, Dictionary<string, WatchFolderItem> lastWatchItems, TimeSpan waitPeriod)
        {
            foreach (var folder in _diskScanService.FilterPaths(watchFolder, _diskProvider.GetDirectories(watchFolder)))
            {
                var title = FileNameBuilder.CleanFileName(Path.GetFileName(folder));

                var newWatchItem = new WatchFolderItem
                {
                    DownloadId = Path.GetFileName(folder) + "_" + _diskProvider.FolderGetCreationTime(folder).Ticks,
                    Title = title,

                    OutputPath = new OsPath(folder),

                    Status = DownloadItemStatus.Completed,
                    RemainingTime = TimeSpan.Zero
                };

                var oldWatchItem = lastWatchItems.GetValueOrDefault(newWatchItem.DownloadId);

                if (PreCheckWatchItemExpiry(newWatchItem, oldWatchItem))
                {
                    var files = _diskProvider.GetFiles(folder, SearchOption.AllDirectories);

                    newWatchItem.TotalSize = files.Select(_diskProvider.GetFileSize).Sum();
                    newWatchItem.Hash = GetHash(folder, files);

                    if (files.Any(_diskProvider.IsFileLocked))
                    {
                        newWatchItem.Status = DownloadItemStatus.Downloading;
                        newWatchItem.RemainingTime = null;
                    }

                    UpdateWatchItemExpiry(newWatchItem, oldWatchItem, waitPeriod);
                }

                yield return newWatchItem;
            }

            foreach (var videoFile in _diskScanService.FilterPaths(watchFolder, _diskScanService.GetVideoFiles(watchFolder, false)))
            {
                var title = FileNameBuilder.CleanFileName(Path.GetFileName(videoFile));

                var newWatchItem = new WatchFolderItem
                {
                    DownloadId = Path.GetFileName(videoFile) + "_" + _diskProvider.FileGetLastWrite(videoFile).Ticks,
                    Title = title,

                    OutputPath = new OsPath(videoFile),

                    Status = DownloadItemStatus.Completed,
                    RemainingTime = TimeSpan.Zero
                };

                var oldWatchItem = lastWatchItems.GetValueOrDefault(newWatchItem.DownloadId);

                if (PreCheckWatchItemExpiry(newWatchItem, oldWatchItem))
                {
                    newWatchItem.TotalSize = _diskProvider.GetFileSize(videoFile);
                    newWatchItem.Hash = GetHash(videoFile);

                    if (_diskProvider.IsFileLocked(videoFile))
                    {
                        newWatchItem.Status = DownloadItemStatus.Downloading;
                    }

                    UpdateWatchItemExpiry(newWatchItem, oldWatchItem, waitPeriod);
                }

                yield return newWatchItem;
            }
        }

        private static bool PreCheckWatchItemExpiry(WatchFolderItem newWatchItem, WatchFolderItem oldWatchItem)
        {
            if (oldWatchItem == null || oldWatchItem.LastChanged.AddHours(1) > DateTime.UtcNow)
            {
                return true;
            }

            newWatchItem.TotalSize = oldWatchItem.TotalSize;
            newWatchItem.Hash = oldWatchItem.Hash;

            return false;
        }

        private static void UpdateWatchItemExpiry(WatchFolderItem newWatchItem, WatchFolderItem oldWatchItem, TimeSpan waitPeriod)
        {
            if (oldWatchItem != null && newWatchItem.Hash == oldWatchItem.Hash)
            {
                newWatchItem.LastChanged = oldWatchItem.LastChanged;
            }
            else
            {
                newWatchItem.LastChanged = DateTime.UtcNow;
            }

            var remainingTime = waitPeriod - (DateTime.UtcNow - newWatchItem.LastChanged);

            if (remainingTime > TimeSpan.Zero)
            {
                newWatchItem.RemainingTime = remainingTime;
                newWatchItem.Status = DownloadItemStatus.Downloading;
            }
        }

        private string GetHash(string folder, string[] files)
        {
            var data = new StringBuilder();

            data.Append(folder);
            try
            {
                data.Append(_diskProvider.FolderGetLastWrite(folder).ToBinary());
            }
            catch (Exception ex)
            {
                _logger.Trace(ex, "Ignored hashing error during scan for {0}", folder);
            }

            foreach (var file in files.OrderBy(v => v))
            {
                data.Append(GetHash(file));
            }

            return HashConverter.GetHash(data.ToString()).ToHexString();
        }

        private string GetHash(string file)
        {
            var data = new StringBuilder();

            data.Append(file);
            try
            {
                data.Append(_diskProvider.FileGetLastWrite(file).ToBinary());
                data.Append(_diskProvider.GetFileSize(file));
            }
            catch (Exception ex)
            {
                _logger.Trace(ex, "Ignored hashing error during scan for {0}", file);
            }

            return HashConverter.GetHash(data.ToString()).ToHexString();
        }
    }
}
