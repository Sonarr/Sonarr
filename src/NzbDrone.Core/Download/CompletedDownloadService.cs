using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using System.IO;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Download
{
    public interface ICompletedDownloadService
    {
        void CheckForCompletedItem(IDownloadClient downloadClient, TrackedDownload trackedDownload, List<History.History> grabbedHistory, List<History.History> importedHistory);
    }

    public class CompletedDownloadService : ICompletedDownloadService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly IDownloadedEpisodesImportService _downloadedEpisodesImportService;
        private readonly Logger _logger;

        public CompletedDownloadService(IEventAggregator eventAggregator,
                                     IConfigService configService,
                                     IDiskProvider diskProvider,
                                     IDownloadedEpisodesImportService downloadedEpisodesImportService,
                                     Logger logger)
        {
            _eventAggregator = eventAggregator;
            _configService = configService;
            _diskProvider = diskProvider;
            _downloadedEpisodesImportService = downloadedEpisodesImportService;
            _logger = logger;
        }

        private List<History.History> GetHistoryItems(List<History.History> grabbedHistory, string downloadClientId)
        {
            return grabbedHistory.Where(h => downloadClientId.Equals(h.Data.GetValueOrDefault(DownloadTrackingService.DOWNLOAD_CLIENT_ID)))
                                 .ToList();
        }

        public void CheckForCompletedItem(IDownloadClient downloadClient, TrackedDownload trackedDownload, List<History.History> grabbedHistory, List<History.History> importedHistory)
        {
            if (!_configService.EnableCompletedDownloadHandling)
            {
                return;
            }

            if (trackedDownload.DownloadItem.Status == DownloadItemStatus.Completed && trackedDownload.State == TrackedDownloadState.Downloading)
            {
                var grabbedItems = GetHistoryItems(grabbedHistory, trackedDownload.DownloadItem.DownloadClientId);

                if (!grabbedItems.Any() && trackedDownload.DownloadItem.Category.IsNullOrWhiteSpace())
                {
                    _logger.Trace("Ignoring download that wasn't grabbed by drone: " + trackedDownload.DownloadItem.Title);
                    return;
                }
                
                var importedItems = GetHistoryItems(importedHistory, trackedDownload.DownloadItem.DownloadClientId);

                if (importedItems.Any())
                {
                    trackedDownload.State = TrackedDownloadState.Imported;

                    _logger.Debug("Already added to history as imported: " + trackedDownload.DownloadItem.Title);
                }
                else
                {
                    string downloadedEpisodesFolder = _configService.DownloadedEpisodesFolder;
                    string downloadItemOutputPath = trackedDownload.DownloadItem.OutputPath;
                    if (downloadItemOutputPath.IsNullOrWhiteSpace())
                    {
                        _logger.Trace("Storage path not specified: " + trackedDownload.DownloadItem.Title);
                        return;
                    }

                    if (!downloadedEpisodesFolder.IsNullOrWhiteSpace() && (downloadedEpisodesFolder.PathEquals(downloadItemOutputPath) || downloadedEpisodesFolder.IsParentPath(downloadItemOutputPath)))
                    {
                        _logger.Trace("Storage path inside drone factory, ignoring download: " + trackedDownload.DownloadItem.Title);
                        return;
                    }

                    if (_diskProvider.FolderExists(trackedDownload.DownloadItem.OutputPath))
                    {
                        var decisions = _downloadedEpisodesImportService.ProcessFolder(new DirectoryInfo(trackedDownload.DownloadItem.OutputPath), trackedDownload.DownloadItem);

                        if (decisions.Any())
                        {
                            trackedDownload.State = TrackedDownloadState.Imported;
                        }
                    }
                    else if (_diskProvider.FileExists(trackedDownload.DownloadItem.OutputPath))
                    {
                        var decisions = _downloadedEpisodesImportService.ProcessFile(new FileInfo(trackedDownload.DownloadItem.OutputPath), trackedDownload.DownloadItem);

                        if (decisions.Any())
                        {
                            trackedDownload.State = TrackedDownloadState.Imported;
                        }
                    }
                    else
                    {
                        _logger.Debug("Storage path does not exist: " + trackedDownload.DownloadItem.Title);
                        return;
                    }
                }
            }

            if (_configService.RemoveCompletedDownloads && trackedDownload.State == TrackedDownloadState.Imported && !trackedDownload.DownloadItem.IsReadOnly)
            {
                try
                {
                    _logger.Info("Removing completed download from history: {0}", trackedDownload.DownloadItem.Title);
                    downloadClient.RemoveItem(trackedDownload.DownloadItem.DownloadClientId);

                    if (_diskProvider.FolderExists(trackedDownload.DownloadItem.OutputPath))
                    {
                        _logger.Info("Removing completed download directory: {0}", trackedDownload.DownloadItem.OutputPath);
                        _diskProvider.DeleteFolder(trackedDownload.DownloadItem.OutputPath, true);
                    }
                    else if (_diskProvider.FileExists(trackedDownload.DownloadItem.OutputPath))
                    {
                        _logger.Info("Removing completed download file: {0}", trackedDownload.DownloadItem.OutputPath);
                        _diskProvider.DeleteFile(trackedDownload.DownloadItem.OutputPath);
                    }

                    trackedDownload.State = TrackedDownloadState.Removed;
                }
                catch (NotSupportedException)
                {
                    _logger.Debug("Removing item not supported by your download client");
                }
            }
        }
    }
}
