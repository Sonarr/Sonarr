using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Imports;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
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
        private readonly IDownloadedMediaImportService _downloadedMediaImportService;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;
        private readonly ISeriesService _seriesService;
        private readonly IMovieService _movieService;

        public CompletedDownloadService(IConfigService configService,
                                        IEventAggregator eventAggregator,
                                        IHistoryService historyService,
                                        IDownloadedMediaImportService downloadedMediaImportService,
                                        IParsingService parsingService,
                                        ISeriesService seriesService,
                                        IMovieService movieService,
                                        Logger logger)
        {
            _configService = configService;
            _eventAggregator = eventAggregator;
            _historyService = historyService;
            _downloadedMediaImportService = downloadedMediaImportService;
            _parsingService = parsingService;
            _logger = logger;
            _seriesService = seriesService;
            _movieService = movieService;
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

                //TODO: Add a Download movie folder to configuration?
                var downloadedEpisodesFolder = new OsPath(_configService.DownloadedEpisodesFolder);

                if (downloadedEpisodesFolder.Contains(downloadItemOutputPath))
                {
                    trackedDownload.Warn("Intermediate Download path inside drone factory, Skipping.");
                    return;
                }

                if (!ShouldImport(trackedDownload, historyItem))
                    return;
            }

            Import(trackedDownload);
        }

        private bool ShouldImport(TrackedDownload trackedDownload, History.History historyItem)
        {
            var isSerie = CanParseSerie(trackedDownload, historyItem);
            var isMovie = !isSerie && CanParseMovie(trackedDownload, historyItem);

            if (!isSerie && !isMovie)
            {
                if (trackedDownload.DownloadItem.DownloadType == DownloadItemType.Series) trackedDownload.Warn("Series title mismatch, automatic import is not possible.");
                else if (trackedDownload.DownloadItem.DownloadType == DownloadItemType.Movie) trackedDownload.Warn("Movie title mismatch, automatic import is not possible.");
                else if (trackedDownload.DownloadItem.DownloadType == DownloadItemType.Unknown) trackedDownload.Warn("Unknown item, automatic import is not possible.");
                return false;
            }

            return isSerie || isMovie;
        }

        private bool CanParseSerie(TrackedDownload trackedDownload, History.History historyItem)
        {
            if (trackedDownload.DownloadItem.DownloadType == DownloadItemType.Series || trackedDownload.DownloadItem.DownloadType == DownloadItemType.Unknown)
            {
                var series = _parsingService.GetSeries(trackedDownload.DownloadItem.Title);

                if (series == null)
                {
                    if (historyItem != null)
                    {
                        series = _seriesService.GetSeries(historyItem.SeriesId);
                    }
                }
                return series != null;
            }
            return false;
        }

        private bool CanParseMovie(TrackedDownload trackedDownload, History.History historyItem)
        {
            if (trackedDownload.DownloadItem.DownloadType == DownloadItemType.Movie || trackedDownload.DownloadItem.DownloadType == DownloadItemType.Unknown)
            {
                var movie = _parsingService.GetMovie(trackedDownload.DownloadItem.Title);

                if (movie == null)
                {
                    if (historyItem != null)
                    {
                        movie = _movieService.GetMovie(historyItem.MovieId);
                    }
                }
                return movie != null;
            }
            return false;
        }

        private int GetItemsToCheck(RemoteItem remoteItem)
        {
            var remoteEpisodeInfo = remoteItem as RemoteEpisode;
            if (remoteEpisodeInfo != null)
            {
                return remoteEpisodeInfo.Episodes.Count;
            }
            return 1;
        }

        private void Import(TrackedDownload trackedDownload)
        {
            var outputPath = trackedDownload.DownloadItem.OutputPath.FullPath;

            List<ImportResult> importResults = _downloadedMediaImportService.ProcessPath(outputPath, trackedDownload.RemoteItem.Media, trackedDownload.DownloadItem);


            if (importResults.Empty())
            {
                trackedDownload.Warn("No files found are eligible for import in {0}", outputPath);
                return;
            }

            int itemsToCheck = GetItemsToCheck(trackedDownload.RemoteItem);

            if (importResults.Count(c => c.Result == ImportResultType.Imported) >= Math.Max(1, itemsToCheck))
            {
                trackedDownload.State = TrackedDownloadStage.Imported;
                _eventAggregator.PublishEvent(new DownloadCompletedEvent(trackedDownload));
                return;
            }

            if (importResults.Any(c => c.Result != ImportResultType.Imported))
            {
                var statusMessages = importResults
                    .Where(v => v.Result != ImportResultType.Imported)
                    .Select(v => new TrackedDownloadStatusMessage(Path.GetFileName(v.ImportDecision.LocalItem.Path), v.Errors))
                    .ToArray();

                trackedDownload.Warn(statusMessages);
            }
        }
    }
}
