using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using System.IO;

namespace NzbDrone.Core.Download
{
    public interface ICompletedDownloadService
    {
        void CheckForCompletedItem(IDownloadClient downloadClient, TrackedDownload trackedDownload, List<History.History> grabbedHistory, List<History.History> importedHistory);
        List<ImportResult> Import(TrackedDownload trackedDownload);
    }

    public class CompletedDownloadService : ICompletedDownloadService
    {
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly IDownloadedEpisodesImportService _downloadedEpisodesImportService;
        private readonly IHistoryService _historyService;
        private readonly Logger _logger;

        public CompletedDownloadService(IConfigService configService,
                                        IDiskProvider diskProvider,
                                        IDownloadedEpisodesImportService downloadedEpisodesImportService,
                                        IHistoryService historyService,
                                        Logger logger)
        {
            _configService = configService;
            _diskProvider = diskProvider;
            _downloadedEpisodesImportService = downloadedEpisodesImportService;
            _historyService = historyService;
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
                    UpdateStatusMessage(trackedDownload, LogLevel.Warn, "Download wasn't grabbed by drone or not in a category, ignoring download.");
                    return;
                }

                var importedItems = GetHistoryItems(importedHistory, trackedDownload.DownloadItem.DownloadClientId);

                if (importedItems.Any())
                {
                    trackedDownload.State = TrackedDownloadState.Imported;

                    UpdateStatusMessage(trackedDownload, LogLevel.Debug, "Already added to history as imported.");
                }

                else if (trackedDownload.Status != TrackedDownloadStatus.Ok)
                {
                    _logger.Debug("Tracked download status is: {0}, skipping import.", trackedDownload.Status);
                    return;
                }

                else
                {
                    var downloadedEpisodesFolder = _configService.DownloadedEpisodesFolder;
                    var downloadItemOutputPath = trackedDownload.DownloadItem.OutputPath;

                    if (downloadItemOutputPath.IsNullOrWhiteSpace())
                    {
                        UpdateStatusMessage(trackedDownload, LogLevel.Warn, "Download doesn't contain intermediate path, ignoring download.");
                        return;
                    }

                    if (!downloadedEpisodesFolder.IsNullOrWhiteSpace() && (downloadedEpisodesFolder.PathEquals(downloadItemOutputPath) || downloadedEpisodesFolder.IsParentPath(downloadItemOutputPath)))
                    {
                        UpdateStatusMessage(trackedDownload, LogLevel.Warn, "Intermediate Download path inside drone factory, ignoring download.");
                        return;
                    }

                    var importResults = Import(trackedDownload);

                    //Only attempt to associate it with a previous import if its still in the downloading state
                    if (trackedDownload.State == TrackedDownloadState.Downloading && importResults.Empty())
                    {
                        AssociateWithPreviouslyImported(trackedDownload, grabbedItems, importedHistory);
                    }
                }
            }

            if (_configService.RemoveCompletedDownloads)
            {
                RemoveCompleted(trackedDownload, downloadClient);
            }
        }

        public List<ImportResult> Import(TrackedDownload trackedDownload)
        {
            var importResults = new List<ImportResult>();

            if (_diskProvider.FolderExists(trackedDownload.DownloadItem.OutputPath))
            {
                importResults = _downloadedEpisodesImportService.ProcessFolder(new DirectoryInfo(trackedDownload.DownloadItem.OutputPath), trackedDownload.DownloadItem);

                ProcessImportResults(trackedDownload, importResults);
            }
            else if (_diskProvider.FileExists(trackedDownload.DownloadItem.OutputPath))
            {
                importResults = _downloadedEpisodesImportService.ProcessFile(new FileInfo(trackedDownload.DownloadItem.OutputPath), trackedDownload.DownloadItem);

                ProcessImportResults(trackedDownload, importResults);
            }

            return importResults;
        }

        private void UpdateStatusMessage(TrackedDownload trackedDownload, LogLevel logLevel, String message, params object[] args)
        {
            var statusMessage = String.Format(message, args);
            var logMessage = String.Format("[{0}] {1}", trackedDownload.DownloadItem.Title, statusMessage);

            if (trackedDownload.StatusMessage != statusMessage)
            {
                trackedDownload.SetStatusLevel(logLevel);
                trackedDownload.StatusMessage = statusMessage;
                _logger.Log(logLevel, logMessage);
            }
            else
            {
                _logger.Debug(logMessage);
            }
        }

        private void ProcessImportResults(TrackedDownload trackedDownload, List<ImportResult> importResults)
        {
            if (importResults.Empty())
            {
                UpdateStatusMessage(trackedDownload, LogLevel.Error, "No files found are eligible for import in {0}", trackedDownload.DownloadItem.OutputPath);
            }
            else if (importResults.Any(v => v.Result == ImportResultType.Imported) && importResults.All(v => v.Result == ImportResultType.Imported || v.Result == ImportResultType.Rejected))
            {
                UpdateStatusMessage(trackedDownload, LogLevel.Info, "Imported {0} files.", importResults.Count(v => v.Result == ImportResultType.Imported));

                trackedDownload.State = TrackedDownloadState.Imported;
            }
            else
            {
                var errors = importResults
                    .Where(v => v.Result == ImportResultType.Skipped || v.Result == ImportResultType.Rejected)
                    .Select(v => v.Errors.Aggregate(Path.GetFileName(v.ImportDecision.LocalEpisode.Path), (a, r) => a + "\r\n- " + r))
                    .Aggregate("Failed to import:", (a, r) => a + "\r\n" + r);

                trackedDownload.StatusMessages = importResults.Where(v => v.Result == ImportResultType.Skipped || v.Result == ImportResultType.Rejected)
                                                              .Select(v => new TrackedDownloadStatusMessage(Path.GetFileName(v.ImportDecision.LocalEpisode.Path), v.Errors)).ToList();

                UpdateStatusMessage(trackedDownload, LogLevel.Error, errors);
            }
        }

        private void AssociateWithPreviouslyImported(TrackedDownload trackedDownload, List<History.History> grabbedItems, List<History.History> importedHistory)
        {
            if (grabbedItems.Any())
            {
                var episodeIds = trackedDownload.RemoteEpisode.Episodes.Select(v => v.Id).ToList();

                // Check if we can associate it with a previous drone factory import.
                var importedItems = importedHistory.Where(v => v.Data.GetValueOrDefault(DownloadTrackingService.DOWNLOAD_CLIENT_ID) == null &&
                                                      episodeIds.Contains(v.EpisodeId) &&
                                                      v.Data.GetValueOrDefault("droppedPath") != null &&
                                                      new FileInfo(v.Data["droppedPath"]).Directory.Name == grabbedItems.First().SourceTitle
                                                      ).ToList();
                if (importedItems.Count == 1)
                {
                    var importedFile = new FileInfo(importedItems.First().Data["droppedPath"]);

                    if (importedFile.Directory.Name == grabbedItems.First().SourceTitle)
                    {
                        trackedDownload.State = TrackedDownloadState.Imported;

                        importedItems.First().Data[DownloadTrackingService.DOWNLOAD_CLIENT] = grabbedItems.First().Data[DownloadTrackingService.DOWNLOAD_CLIENT];
                        importedItems.First().Data[DownloadTrackingService.DOWNLOAD_CLIENT_ID] = grabbedItems.First().Data[DownloadTrackingService.DOWNLOAD_CLIENT_ID];
                        _historyService.UpdateHistoryData(importedItems.First().Id, importedItems.First().Data);

                        UpdateStatusMessage(trackedDownload, LogLevel.Debug, "Intermediate Download path does not exist, but found probable drone factory ImportEvent.");
                        return;
                    }
                }
            }

            UpdateStatusMessage(trackedDownload, LogLevel.Error, "Intermediate Download path does not exist: {0}", trackedDownload.DownloadItem.OutputPath);
        }

        private void RemoveCompleted(TrackedDownload trackedDownload, IDownloadClient downloadClient)
        {
            if (trackedDownload.State == TrackedDownloadState.Imported && !trackedDownload.DownloadItem.IsReadOnly)
            {
                try
                {
                    _logger.Debug("[{0}] Removing completed download from history.", trackedDownload.DownloadItem.Title);
                    downloadClient.RemoveItem(trackedDownload.DownloadItem.DownloadClientId);

                    if (_diskProvider.FolderExists(trackedDownload.DownloadItem.OutputPath))
                    {
                        _logger.Debug("Removing completed download directory: {0}",
                            trackedDownload.DownloadItem.OutputPath);
                        _diskProvider.DeleteFolder(trackedDownload.DownloadItem.OutputPath, true);
                    }
                    else if (_diskProvider.FileExists(trackedDownload.DownloadItem.OutputPath))
                    {
                        _logger.Debug("Removing completed download file: {0}", trackedDownload.DownloadItem.OutputPath);
                        _diskProvider.DeleteFile(trackedDownload.DownloadItem.OutputPath);
                    }

                    trackedDownload.State = TrackedDownloadState.Removed;
                }
                catch (NotSupportedException)
                {
                    UpdateStatusMessage(trackedDownload, LogLevel.Debug,
                        "Removing item not supported by your download client.");
                }
            }
        }

    }
}
