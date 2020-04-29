using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Download
{
    public interface ICompletedDownloadService
    {
        void Check(TrackedDownload trackedDownload);
        void Import(TrackedDownload trackedDownload);
        bool VerifyImport(TrackedDownload trackedDownload, List<ImportResult> importResults);
    }

    public class CompletedDownloadService : ICompletedDownloadService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IHistoryService _historyService;
        private readonly IDownloadedEpisodesImportService _downloadedEpisodesImportService;
        private readonly IParsingService _parsingService;
        private readonly ISeriesService _seriesService;
        private readonly ITrackedDownloadAlreadyImported _trackedDownloadAlreadyImported;

        public CompletedDownloadService(IEventAggregator eventAggregator,
                                        IHistoryService historyService,
                                        IDownloadedEpisodesImportService downloadedEpisodesImportService,
                                        IParsingService parsingService,
                                        ISeriesService seriesService,
                                        ITrackedDownloadAlreadyImported trackedDownloadAlreadyImported)
        {
            _eventAggregator = eventAggregator;
            _historyService = historyService;
            _downloadedEpisodesImportService = downloadedEpisodesImportService;
            _parsingService = parsingService;
            _seriesService = seriesService;
            _trackedDownloadAlreadyImported = trackedDownloadAlreadyImported;
        }

        public void Check(TrackedDownload trackedDownload)
        {
            if (trackedDownload.DownloadItem.Status != DownloadItemStatus.Completed)
            {
                return;
            }

            // Only process tracked downloads that are still downloading
            if (trackedDownload.State != TrackedDownloadState.Downloading)
            {
                return;
            }

            var historyItem = _historyService.MostRecentForDownloadId(trackedDownload.DownloadItem.DownloadId);

            if (historyItem == null && trackedDownload.DownloadItem.Category.IsNullOrWhiteSpace())
            {
                trackedDownload.Warn("Download wasn't grabbed by Sonarr and not in a category, Skipping.");
                return;
            }

            var downloadItemOutputPath = trackedDownload.DownloadItem.OutputPath;

            if (downloadItemOutputPath.IsEmpty)
            {
                trackedDownload.Warn("Download doesn't contain intermediate path, Skipping.");
                return;
            }

            if ((OsInfo.IsWindows && !downloadItemOutputPath.IsWindowsPath) ||
                (OsInfo.IsNotWindows && !downloadItemOutputPath.IsUnixPath))
            {
                trackedDownload.Warn("[{0}] is not a valid local path. You may need a Remote Path Mapping.", downloadItemOutputPath);
                return;
            }

            var series = _parsingService.GetSeries(trackedDownload.DownloadItem.Title);

            if (series == null)
            {
                if (historyItem != null)
                {
                    series = _seriesService.GetSeries(historyItem.SeriesId);
                }

                if (series == null)
                {
                    trackedDownload.Warn("Series title mismatch, automatic import is not possible.");
                    return;
                }
            }

            trackedDownload.State = TrackedDownloadState.ImportPending;
        }

        public void Import(TrackedDownload trackedDownload)
        {
            trackedDownload.State = TrackedDownloadState.Importing;

            var outputPath = trackedDownload.DownloadItem.OutputPath.FullPath;
            var importResults = _downloadedEpisodesImportService.ProcessPath(outputPath, ImportMode.Auto,
                trackedDownload.RemoteEpisode.Series, trackedDownload.DownloadItem);

            if (VerifyImport(trackedDownload, importResults))
            {
                return;
            }

            trackedDownload.State = TrackedDownloadState.ImportPending;

            if (importResults.Empty())
            {
                trackedDownload.Warn("No files found are eligible for import in {0}", outputPath);
            }

            if (importResults.Any(c => c.Result != ImportResultType.Imported))
            {
                var statusMessages = importResults
                    .Where(v => v.Result != ImportResultType.Imported)
                    .Select(v => new TrackedDownloadStatusMessage(Path.GetFileName(v.ImportDecision.LocalEpisode.Path), v.Errors))
                    .ToArray();

                trackedDownload.Warn(statusMessages);
            }
        }

        public bool VerifyImport(TrackedDownload trackedDownload, List<ImportResult> importResults)
        {
            var allEpisodesImported = importResults.Where(c => c.Result == ImportResultType.Imported)
                                                   .SelectMany(c => c.ImportDecision.LocalEpisode.Episodes)
                                                   .Count() >= Math.Max(1,
                                          trackedDownload.RemoteEpisode.Episodes.Count);

            if (allEpisodesImported)
            {
                trackedDownload.State = TrackedDownloadState.Imported;
                _eventAggregator.PublishEvent(new DownloadCompletedEvent(trackedDownload));
                return true;
            }

            // Double check if all episodes were imported by checking the history if at least one
            // file was imported. This will allow the decision engine to reject already imported
            // episode files and still mark the download complete when all files are imported.

            // EDGE CASE: This process relies on EpisodeIds being consistent between executions, if a series is updated 
            // and an episode is removed, but later comes back with a different ID then Sonarr will treat it as incomplete.
            // Since imports should be relatively fast and these types of data changes are infrequent this should be quite
            // safe, but commenting for future benefit.

            if (importResults.Any(c => c.Result == ImportResultType.Imported))
            {
                var historyItems = _historyService.FindByDownloadId(trackedDownload.DownloadItem.DownloadId)
                                                  .OrderByDescending(h => h.Date)
                                                  .ToList();

                var allEpisodesImportedInHistory = _trackedDownloadAlreadyImported.IsImported(trackedDownload, historyItems);

                if (allEpisodesImportedInHistory)
                {
                    trackedDownload.State = TrackedDownloadState.Imported;
                    _eventAggregator.PublishEvent(new DownloadCompletedEvent(trackedDownload));
                    return true;
                }
            }

            return false;
        }
    }
}
