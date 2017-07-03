using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
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
        void Process(TrackedDownload trackedDownload, bool ignoreWarnings = false);
    }

    public class CompletedDownloadService : ICompletedDownloadService
    {
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IHistoryService _historyService;
        private readonly IDownloadedEpisodesImportService _downloadedEpisodesImportService;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;
        private readonly ISeriesService _seriesService;

        public CompletedDownloadService(IConfigService configService,
                                        IEventAggregator eventAggregator,
                                        IHistoryService historyService,
                                        IDownloadedEpisodesImportService downloadedEpisodesImportService,
                                        IParsingService parsingService,
                                        ISeriesService seriesService,
                                        Logger logger)
        {
            _configService = configService;
            _eventAggregator = eventAggregator;
            _historyService = historyService;
            _downloadedEpisodesImportService = downloadedEpisodesImportService;
            _parsingService = parsingService;
            _logger = logger;
            _seriesService = seriesService;
        }

        public void Process(TrackedDownload trackedDownload, bool ignoreWarnings = false)
        {
            if (trackedDownload.DownloadItem.Status != DownloadItemStatus.Completed)
            {
                return;
            }

            if (!ignoreWarnings)
            {
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
            }

            Import(trackedDownload);
        }

        private void Import(TrackedDownload trackedDownload)
        {
            var outputPath = trackedDownload.DownloadItem.OutputPath.FullPath;
            var importResults = _downloadedEpisodesImportService.ProcessPath(outputPath, ImportMode.Auto, trackedDownload.RemoteEpisode.Series, trackedDownload.DownloadItem);

            if (importResults.Empty())
            {
                trackedDownload.Warn("No files found are eligible for import in {0}", outputPath);
                return;
            }

            if (importResults.Count(c => c.Result == ImportResultType.Imported) >= Math.Max(1, trackedDownload.RemoteEpisode.Episodes.Count))
            {
                trackedDownload.State = TrackedDownloadStage.Imported;
                _eventAggregator.PublishEvent(new DownloadCompletedEvent(trackedDownload));
                return;
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
    }
}
